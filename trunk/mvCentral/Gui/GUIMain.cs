using System;
using System.Windows.Forms;
using System.IO;
using System.Threading;

using Cornerstone.Database;
using Cornerstone.Database.Tables;
using Cornerstone.GUI.Dialogs;
using Cornerstone.Tools;

using NLog;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

using mvCentral.Database;
using mvCentral;
using mvCentral.LocalMediaManagement;
using mvCentral.Playlist;
using mvCentral.Utils;

using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using Action = MediaPortal.GUI.Library.Action;
using Layout = MediaPortal.GUI.Library.GUIFacadeControl.Layout;
using WindowPlugins;

namespace mvCentral.GUI
{
  public partial class mvGUIMain : WindowPluginBase
  {

    #region enums

    public enum View
    {
      Artist,
      Video
    }

    //public enum Layout
    //{
    //  List,
    //  Icons,
    //  LargeIcons,
    //  FilmStrip,
    //  AlbumView,
    //  PlayList 
    //}

    #endregion

    #region Declarations

    private static Logger logger = LogManager.GetCurrentClassLogger();

    private mvCentralCore core = mvCentralCore.Instance;

    private bool initComplete = false;
    private Thread initThread;
    private bool preventDialogOnLoad = false;

    Dictionary<string, bool> loggedProperties;
    private readonly object propertySync = new object();

    //private string lastTrack = "";

    GUImvPlayList Player = new GUImvPlayList();
      
    

    private bool persisting = false;
    public int lastItemArt = 0, lastItemVid = 0, artistID = 0, albumID = 0;
    private string selArtist = "";
    private string selAlbum = "";

    private View currentView = View.Artist;

    protected View CurrentView
    {
      get { return currentView; }
      set { currentView = value; }
    }

    protected bool AllowView(View view)
    {
      //if (view == View.List)
      //  return false;

      return true;
    }

    #endregion

    #region Skin Connection

    private enum GUIControls
    {
      Layout = 2,
      SortAs = 3,
      ViewAs = 5,
      PlayAllRandom = 6,
      PlaySmart = 7,
      PlayList = 8,
      Hierachy = 10,
      ArtistName = 11,
      ArtistBio = 12,
      ArtistImage = 13,
      VideoImage = 14,
      VideoCount = 15,
      ArtistCount = 16,
      FavVidLabel = 17,
      FavVidImage = 18,
      FavArtLabel = 19,
      FavArtImage = 20,
      SortLabel = 22,
      DummyLabel = 30,
      Facade = 50
    }

    //[SkinControlAttribute((int)GUIControls.ViewAs)] protected GUIButtonControl btnViews = null;
    //[SkinControlAttribute((int)GUIControls.Layout)] protected GUIButtonControl btnLayouts = null;
    //[SkinControlAttribute((int)GUIControls.SortAs)] protected GUIButtonControl btnSortBy = null;
    //[SkinControlAttribute((int)GUIControls.Facade)] protected GUIFacadeControl facadeLayout;

    
    [SkinControlAttribute((int)GUIControls.PlayAllRandom)] protected GUIButtonControl btnPlayAllRandom = null;
    [SkinControlAttribute((int)GUIControls.PlaySmart)] protected GUIButtonControl btnSmartList = null;
    [SkinControlAttribute((int)GUIControls.PlayList)] protected GUIButtonControl btnPlayList = null;
    
    [SkinControlAttribute((int)GUIControls.Hierachy)] protected GUILabelControl hierachy = null;
    [SkinControlAttribute((int)GUIControls.ArtistName)] protected GUILabelControl artistName = null;
    [SkinControlAttribute((int)GUIControls.SortLabel)] protected GUILabelControl SortLabel = null;
    [SkinControlAttribute((int)GUIControls.DummyLabel)] protected GUILabelControl dummyLabel = null;
    [SkinControlAttribute((int)GUIControls.VideoCount)] protected GUILabelControl videoCount = null;
    [SkinControlAttribute((int)GUIControls.ArtistCount)] protected GUILabelControl artistCount = null;
    [SkinControlAttribute((int)GUIControls.FavArtLabel)] protected GUILabelControl favArtLabel = null;
    
    [SkinControlAttribute((int)GUIControls.ArtistImage)] protected GUIImage artistImage = null;
    [SkinControlAttribute((int)GUIControls.VideoImage)] protected GUIImage videoImage = null;
    [SkinControlAttribute((int)GUIControls.FavVidImage)] protected GUIImage favVidImage = null;
    [SkinControlAttribute((int)GUIControls.FavArtImage)] protected GUIImage favArtImage = null;

    [SkinControlAttribute((int)GUIControls.FavVidLabel)] protected GUIFadeLabel favVidLabel = null;
    [SkinControlAttribute((int)GUIControls.ArtistBio)] protected GUITextScrollUpControl artistBio = null;

    #endregion

    #region Overrides

    public override bool Init()
    {
      base.Init();
      logger.Info("Initializing GUI...");

      // check if we can load the skin
      bool success = Load(GUIGraphicsContext.Skin + @"\mvCentral.xml");

      // get last active module settings 
      bool lastActiveModuleSetting = mvCentralCore.MediaPortalSettings.GetValueAsBool("general", "showlastactivemodule", false);
      int lastActiveModule = mvCentralCore.MediaPortalSettings.GetValueAsInt("general", "lastactivemodule", -1);
      preventDialogOnLoad = (lastActiveModuleSetting && (lastActiveModule == GetID));

      // set some skin properties
      SetProperty("#mvCentral.Settings.HomeScreenName", mvCentralCore.Settings.HomeScreenName);

      // start initialization of the music videos core services in a seperate thread
      initThread = new Thread(new ThreadStart(mvCentralCore.Initialize));
      initThread.Start();

      // ... and listen to the progress
      mvCentralCore.InitializeProgress += new ProgressDelegate(onCoreInitializationProgress);

      return success;
    }

    /// <summary>
    /// Handle action
    /// </summary>
    /// <param name="action"></param>
    public override void OnAction(MediaPortal.GUI.Library.Action action)
    {
      MediaPortal.GUI.Library.Action.ActionType wID = action.wID;

      if (wID == MediaPortal.GUI.Library.Action.ActionType.ACTION_QUEUE_ITEM)
      {
        facadeLayout.SelectedListItemIndex = Player.playlistPlayer.CurrentItem;
        return;
      }

      if (wID == MediaPortal.GUI.Library.Action.ActionType.ACTION_PREVIOUS_MENU && currentView == View.Video)
      {
        currentView = View.Artist;
        loadCurrent();
      }
      else if (wID == MediaPortal.GUI.Library.Action.ActionType.ACTION_CONTEXT_MENU)
      {
        int contextChoice = ShowContextMenu();
        switch (contextChoice)
        {
          case 0:
            //Add to playlist
            // If on a folder add all Videos for Artist
            if (facadeLayout.ListLayout.SelectedListItem.IsFolder)
            {
              DBArtistInfo currArtist = DBArtistInfo.Get(facadeLayout.ListLayout.SelectedListItem.Label);
              List<DBTrackInfo> allTracksByArtist = DBTrackInfo.GetEntriesByArtist(currArtist);

              addToPlaylist(allTracksByArtist, false, false, false);
            }
            else
            {
              // Add video to playlist
              PlayList playlist = Player.playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MVCENTRAL);
              string filename = facadeLayout.ListLayout.SelectedListItem.Label;
              string path = facadeLayout.ListLayout.SelectedListItem.Path;
              DBTrackInfo video = (DBTrackInfo)facadeLayout.ListLayout.SelectedListItem.MusicTag;
              PlayListItem p1 = new PlayListItem(video);
              p1.Track = video;
              playlist.Add(p1);
            }
            break;
          case 1:
            // Add all videos to the playlist
            List<DBTrackInfo> allTracks = DBTrackInfo.GetAll();
            addToPlaylist(allTracks, false, false, false);
            break;
          case 2:
            addToPlaylistNext(facadeLayout.ListLayout.SelectedListItem);
            break;
          default:
            //Exit
            break;
        }
      }
      else
        base.OnAction(action);
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_PLAYLIST_CHANGED:
          {
            facadeLayout.SelectedListItemIndex = message.Param2;
            return true;
          }
          //break;
      }
      return base.OnMessage(message);
    }

    public override void DeInit()
    {
      base.DeInit();

      logger.Info("Deinitializing GUI...");

      // if the plugin was not fully initialized yet
      // abort the initialization
      if (!initComplete && initThread.IsAlive)
      {
        initThread.Abort();
        // wait for the thread to be aborted
        initThread.Join();
      }

      mvCentralCore.Shutdown();
      initComplete = false;
      logger.Info("GUI Deinitialization Complete");
    }

    public override int GetID
    {
      get
      {
        return mvCentralCore.PluginID;
      }

      set
      {
      }
    }


    protected override void LoadSettings()
    {
      base.LoadSettings();
    }

    protected override void SaveSettings()
    {
      base.SaveSettings();
    }

    protected override void OnShowViews()
    {
      //base.OnShowViews();
    }

    protected override void OnInfo(int iItem)
    {
      base.OnInfo(iItem);
    }

    protected override void OnQueueItem(int item)
    {
      base.OnQueueItem(item);
    }

    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {

      base.OnClicked(controlId, control, actionType);

      if (control == btnLayouts)
      {
        mvCentralCore.Settings.DefaultView = ((int)CurrentLayout).ToString();
      }

      switch (controlId)
      {
        case (int)GUIControls.PlayAllRandom:
          playRandomAll();
          break;
        case (int)GUIControls.PlaySmart:
          playSmart(ChooseSmartPlay());
          break;
        case (int)GUIControls.PlayList:
          GUIWindowManager.ActivateWindow(Player.GetWindowId());
          break;
        case (int)GUIControls.Facade:
          //Clicked on something in the facade
          if (facadeLayout.ListLayout.SelectedListItem.IsFolder & actionType == Action.ActionType.ACTION_MUSIC_PLAY)
          {
            ArtistActions(actionType);
          }
          else
          {
            GUIListItem selectedItem = facadeLayout.SelectedListItem;
            if (!selectedItem.IsFolder && selectedItem.MusicTag != null)
            { // we have a track selected so add any other tracks which
              // are on showing on the facade
              List<DBTrackInfo> list1 = new List<DBTrackInfo>();
              for (int i = 0; i < facadeLayout.ListLayout.Count; i++)
              {
                GUIListItem trackItem = facadeLayout.ListLayout[i];

                if (!trackItem.IsFolder && trackItem.MusicTag != null)
                {
                  list1.Add((DBTrackInfo)trackItem.MusicTag);
                }
              }

              addToPlaylist(list1, false, true, false);

              Player.playlistPlayer.Play(lastItemVid);
              break;
            }
            //return to previous level
            if (facadeLayout.SelectedListItem.Label == "..")
            {
              currentView = View.Artist;
              loadCurrent();
            }
            else
            {
              currentView = View.Video;
              artistID = facadeLayout.SelectedListItem.ItemId;
              loadCurrent();
            }

            //            VideoActions(actionType);
          }
          break;
      }
      
    }

    public int useLayout {
      get {return int.Parse(mvCentralCore.Settings.DefaultView);}
      set { ;}
    }


    protected override void OnPageLoad()
    {
      logger.Debug("Start Loading Page...");

      int watchedCount = 0;
      DBArtistInfo mostPlayedArtist = null;

      logger.Debug("Load Tracks and Artists");
      List<DBTrackInfo> vidList = DBTrackInfo.GetAll();
      List<DBArtistInfo> artList = DBArtistInfo.GetAll();

      // Set Total Artists and Video Porperties
      GUIPropertyManager.SetProperty("#mvCentral.TotalVideos", DBTrackInfo.GetAll().Count + " Videos");
      GUIPropertyManager.SetProperty("#mvCentral.TotalArtists", DBArtistInfo.GetAll().Count + " Artists");

      // get the most played Track and Image
      logger.Debug("Get Most Played Track");
      vidList.Sort(delegate(DBTrackInfo p1, DBTrackInfo p2) { return p2.ActiveUserSettings.WatchedCount.CompareTo(p1.ActiveUserSettings.WatchedCount); });
      GUIPropertyManager.SetProperty("#mvCentral.MostPlayed", vidList[0].Track);
      DBArtistInfo artInfo = DBArtistInfo.Get(vidList[0]);
      favVidImage.SetFileName(artInfo.ArtFullPath);
      //
      // Work out the most played artist
      //
      logger.Debug("Get Most Played Artist");
      watchedCount = 0;
      foreach (DBArtistInfo currArtist in artList)
      {
        List<DBTrackInfo> list = DBTrackInfo.GetEntriesByArtist(currArtist);
        int watchedForArtist = 0;
        foreach (DBTrackInfo track in list)
        {
          watchedForArtist += track.ActiveUserSettings.WatchedCount;
        }
        if (watchedForArtist > watchedCount)
        {
          watchedCount = watchedForArtist;
          mostPlayedArtist = currArtist;
        }
      }
      if (mostPlayedArtist != null)
      {
        GUIPropertyManager.SetProperty("#mvCentral.FavArtist", mostPlayedArtist.Artist);
        favArtImage.SetFileName(mostPlayedArtist.ArtFullPath);
      }
      else
      {
        GUIPropertyManager.SetProperty("#mvCentral.FavArtist", " ");
      }


      SortLabel.Label = "";
      GUIPropertyManager.Changed = true;
      if (persisting)
      {
        logger.Debug("Load Current");
        loadCurrent();
      }
      else
      {
        logger.Debug("Load Artists");
        loadArtists();
      }

      CurrentLayout = (Layout)useLayout;
      SwitchLayout();
      UpdateButtonStates();

      logger.Debug("Loading Page Completed");


      base.OnPageLoad();
    }

    #endregion

    #region Public Methods

    public mvGUIMain()
    {
      timeOut.Tick += new EventHandler(checkTime);
    }

    public void SetFocusItem(int index)
    {
      facadeLayout.SelectedListItemIndex = index;

    }

    #endregion

    #region Private Methods

    private void DebugMsg(string Message)
    {
      GUIDialogOK dlg = (GUIDialogOK)GUIWindowManager.GetWindow(
        (int)GUIWindow.Window.WINDOW_DIALOG_OK);
      dlg.SetHeading("DEBUG MESSAGE");
      dlg.SetLine(1, Message);
      dlg.SetLine(2, "");
      dlg.SetLine(3, "");
      dlg.DoModal(GUIWindowManager.ActiveWindow);
    }

    /// <summary>
    /// Show the Conext Menu
    /// </summary>
    /// <returns></returns>
    private int ShowContextMenu()
    {
      GUIDialogMenu dlgMenu = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlgMenu != null)
      {
        logger.Info("HERE");
        dlgMenu.Reset();
        dlgMenu.SetHeading(mvCentralCore.Settings.HomeScreenName + " - Context Menu");
        if (this.facadeLayout.Count > 0)
        {
          dlgMenu.Add("Add to playlist");
          dlgMenu.Add("Add all to playlist");
          if (Player.playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MVCENTRAL).Count > 0 && !(facadeLayout.ListLayout.SelectedListItem.IsFolder))
          {
            dlgMenu.Add("Add to Playlist as next item");
          }
          dlgMenu.Add("Cancel");
        }
        dlgMenu.DoModal(GetID);

        if (dlgMenu.SelectedLabel == -1) // Nothing was selected
          return -1;

        return dlgMenu.SelectedLabel;
      }
      return -1;
    }
    /// <summary>
    /// Load the current ???
    /// </summary>
    private void loadCurrent()
    {
      persisting = true;
      switch (currentView)
      {
        case View.Artist:
          loadArtists();
          facadeLayout.SelectedListItemIndex = lastItemArt;
          break;
        case View.Video:
          LoadVideos(artistID);
          facadeLayout.SelectedListItemIndex = lastItemVid;
          break;
      }

    }

    /// <summary>
    /// Load artists
    /// </summary>
    private void loadArtists()
    {
      currentView = View.Artist;

      GUIPropertyManager.SetProperty("#mvCentral.Hierachy", "Artists");
      GUIPropertyManager.Changed = true;
      List<DBArtistInfo> list = DBArtistInfo.GetAll();
      // Sort Artist decending
      list.Sort(delegate(DBArtistInfo p1, DBArtistInfo p2) { return p1.Artist.CompareTo(p2.Artist); });

      ArrayList state = new ArrayList();
      facadeLayout.Clear();
      foreach (DBArtistInfo db1 in list)
      {
        GUIListItem item = new GUIListItem();
        item.Label = db1.Artist;
        item.ThumbnailImage = db1.ArtThumbFullPath;
        item.TVTag = db1.bioContent;
        item.AlbumInfoTag = db1.bioContent;
        item.ItemId = (int)db1.ID;
        item.IsFolder = true;
        item.OnItemSelected += new GUIListItem.ItemSelectedHandler(onArtistSelected);
        facadeLayout.Add(item);
      }


      if (facadeLayout.Count > 0 && !persisting)
      {
        onArtistSelected(facadeLayout.ListLayout.ListItems[0], facadeLayout);
      }
      persisting = true;
      dummyLabel.Visibility = System.Windows.Visibility.Hidden;
    }
    /// <summary>
    /// Load Videos
    /// </summary>
    /// <param name="ArtistID"></param>
    private void LoadVideos(int ArtistID)
    {
      currentView = View.Video;
      GUIPropertyManager.SetProperty("#mvCentral.Hierachy", "Artists | " + DBArtistInfo.Get(ArtistID));
      GUIPropertyManager.Changed = true;

      if (facadeLayout.SelectedListItem == null)
      {
        if (albumID != 0)
        {
          LoadAlbums(albumID);
          return;
        }

      }

      if (facadeLayout.SelectedListItem != null)
        if (facadeLayout.SelectedListItem.MusicTag != null && facadeLayout.SelectedListItem.MusicTag.GetType() == typeof(DBAlbumInfo))
        {
          DBAlbumInfo db1 = (DBAlbumInfo)facadeLayout.SelectedListItem.MusicTag;
          albumID = db1.ID.Value;
          LoadAlbums(db1.ID.Value);
          return;

        }


      DBArtistInfo currArtist = DBArtistInfo.Get(ArtistID);
      List<DBTrackInfo> list = DBTrackInfo.GetEntriesByArtist(currArtist);
      list.Sort(delegate(DBTrackInfo p1, DBTrackInfo p2) { return p1.Track.CompareTo(p2.Track); });
      this.artistID = ArtistID;
      facadeLayout.Clear();
      //      facade.Add(new GUIListItem(".."));

      foreach (DBTrackInfo db1 in list)
      {
        if (db1.AlbumInfo.Count == 0) continue;

        bool IsPresent = false;
        for (int i = 0; i <= facadeLayout.Count - 1; i++)
        {
          if (facadeLayout[i].Label == db1.AlbumInfo[0].Album) IsPresent = true;
        }
        if (IsPresent) continue;

        {
          GUIListItem item = new GUIListItem();
          item.Label = db1.AlbumInfo[0].Album;
          item.ThumbnailImage = db1.AlbumInfo[0].ArtThumbFullPath;
          item.TVTag = db1.AlbumInfo[0].bioContent;
          selArtist = currArtist.Artist;
          item.IsFolder = true;
          item.OnItemSelected += new GUIListItem.ItemSelectedHandler(onVideoSelected);
          item.MusicTag = db1.AlbumInfo[0];
          facadeLayout.Add(item);

        }

      }

      foreach (DBTrackInfo db1 in list)
      {
        if (db1.AlbumInfo.Count > 0) continue;
        GUIListItem item = new GUIListItem();
        item.Label = db1.Track;
        if (db1.LocalMedia[0].IsDVD)
          item.Label2 = "DVD entry";
        else item.Label2 = "Track entry";
        TimeSpan tt = TimeSpan.Parse(db1.PlayTime);
        DateTime dt = new DateTime(tt.Ticks);
        item.Label3 = String.Format("{0:HH:mm:ss}", dt);
        item.ThumbnailImage = db1.ArtThumbFullPath;
        item.TVTag = mvCentralUtils.StripHTML(db1.bioContent);
        selArtist = currArtist.Artist;
        item.Path = db1.LocalMedia[0].File.FullName;
        item.IsFolder = false;
        item.OnItemSelected += new GUIListItem.ItemSelectedHandler(onVideoSelected);
        item.MusicTag = db1;
        facadeLayout.Add(item);
      }

      if (facadeLayout.Count > 0 && !persisting)
      {
        onVideoSelected(facadeLayout.ListLayout.ListItems[0], facadeLayout);
      }
      dummyLabel.Visibility = System.Windows.Visibility.Visible;
    }
    /// <summary>
    /// Load Albums
    /// </summary>
    /// <param name="AlbumID"></param>
    private void LoadAlbums(int AlbumID)
    {
      currentView = View.Video;
      GUIPropertyManager.SetProperty("#mvCentral.Hierachy", "Artists | " + DBAlbumInfo.Get(AlbumID));
      GUIPropertyManager.Changed = true;
      DBAlbumInfo currAlbum = DBAlbumInfo.Get(AlbumID);
      List<DBTrackInfo> list = DBTrackInfo.GetEntriesByAlbum(currAlbum);
      this.albumID = AlbumID;
      facadeLayout.Clear();
      //      facade.Add(new GUIListItem(".."));
      foreach (DBTrackInfo db1 in list)
      {
        GUIListItem item = new GUIListItem();
        item.Label = db1.Track;
        if (db1.LocalMedia[0].IsDVD)
          item.Label2 = "DVD entry";
        else item.Label2 = "Track entry";
        item.Label3 = db1.PlayTime;
        item.ThumbnailImage = db1.ArtThumbFullPath;
        item.TVTag = mvCentralUtils.StripHTML(db1.bioContent);
        selAlbum = currAlbum.Album;
        item.Path = db1.LocalMedia[0].File.FullName;
        item.IsFolder = false;
        item.OnItemSelected += new GUIListItem.ItemSelectedHandler(onVideoSelected);
        item.MusicTag = db1;
        facadeLayout.Add(item);
      }

      if (facadeLayout.Count > 0 && !persisting)
      {
        onVideoSelected(facadeLayout.ListLayout.ListItems[0], facadeLayout);
      }
      dummyLabel.Visibility = System.Windows.Visibility.Visible;
    }
    /// <summary>
    /// Actions to perform when artist selected
    /// </summary>
    /// <param name="item"></param>
    /// <param name="parent"></param>
    void onArtistSelected(GUIListItem item, GUIControl parent)
    {
      if (string.IsNullOrEmpty(item.TVTag.ToString().Trim()))
        GUIPropertyManager.SetProperty("#mvCentral.ArtistBio", "No Artist Bio Avaiable");
      else
        GUIPropertyManager.SetProperty("#mvCentral.ArtistBio", mvCentralUtils.StripHTML(item.TVTag.ToString()));

      GUIPropertyManager.SetProperty("#mvCentral.ArtistName", item.Label);
      GUIPropertyManager.SetProperty("#mvCentral.ArtistImg", item.ThumbnailImage);
      GUIPropertyManager.Changed = true;
      lastItemArt = facadeLayout.ListLayout.SelectedListItemIndex;
    }

    void onVideoSelected(GUIListItem item, GUIControl parent)
    {
      GUIPropertyManager.SetProperty("#mvCentral.VideoImg", item.ThumbnailImage);
      if (string.IsNullOrEmpty(item.TVTag.ToString().Trim()))
        GUIPropertyManager.SetProperty("#mvCentral.ArtistBio", "No Track Information Avaiable");
      else
        GUIPropertyManager.SetProperty("#mvCentral.ArtistBio", mvCentralUtils.StripHTML(item.TVTag.ToString()));


      GUIPropertyManager.Changed = true;
      if (item.Label != "..")
      {
        lastItemVid = facadeLayout.ListLayout.SelectedListItemIndex;
      }
    }



    #endregion

    #region Skin and Property Settings

    public void SetProperty(string property, string value)
    {
      SetProperty(property, value, false);
    }

    public void SetProperty(string property, string value, bool forceLogging)
    {
      if (property == null)
        return;

      if (mvCentralCore.Settings.LogAllSkinPropertyChanges)
        forceLogging = true;

      try
      {
        lock (propertySync)
        {

          if (loggedProperties == null)
            loggedProperties = new Dictionary<string, bool>();

          if (!loggedProperties.ContainsKey(property) || forceLogging)
          {
            logger.Debug(property + " = \"" + value + "\"");
            loggedProperties[property] = true;
          }
        }
      }
      catch (Exception e)
      {
        if (e is ThreadAbortException)
          throw e;

        logger.Warn("Internal .NET error from dictionary class!");
      }

      // If the value is empty add a space
      // otherwise the property will keep 
      // displaying it's previous value
      if (String.IsNullOrEmpty(value))
        GUIPropertyManager.SetProperty(property, " ");

      GUIPropertyManager.SetProperty(property, value);
    }

    /// <summary>
    /// Resets the property values for every key that starts with the given string
    /// </summary>
    /// <param name="startsWith">the prefix to reset</param>
    public void ResetProperties(string startsWith)
    {
      logger.Debug("Resetting properties: {0}", startsWith);
      foreach (string key in loggedProperties.Keys)
      {
        if (key.StartsWith(startsWith))
          SetProperty(key, "");
      }
    }
    #endregion

    #region Loading and initialization

    private void showLoadingDialog()
    {
      /*            initDialog = (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
                  initDialog.Reset();
                  initDialog.ShowProgressBar(true);
                  initDialog.SetHeading("Loading Moving Pictures");
                  initDialog.SetLine(1, string.Empty);
                  initDialog.SetLine(2, initProgressLastAction);
                  initDialog.SetPercentage(initProgressLastPercent);
                  initDialog.Progress();
                  initDialog.DoModal(GetID);
      */
    }

    private void onCoreInitializationProgress(string actionName, int percentDone)
    {

      // Update the progress variables
      if (percentDone == 100)
      {
        actionName = "Loading GUI ...";
      }
      //            initProgressLastAction = actionName;
      //            initProgressLastPercent = percentDone;

      // If the progress dialog exists, update it.
      //            if (initDialog != null)
      //            {
      //                initDialog.SetLine(2, initProgressLastAction);
      //                initDialog.SetPercentage(initProgressLastPercent);
      //                initDialog.Progress();
      //            }

      // When we are finished initializing
      if (percentDone == 100)
      {

        // Start the background importer
        if (mvCentralCore.Settings.EnableImporterInGUI)
        {
          //                    mvCentralCore.Importer.Start();
          //                    mvCentralCore.Importer.Progress += new MusicVideoImporter.ImportProgressHandler(Importer_Progress);
        }

        // Load skin based settings from skin file
        //                skinSettings = new mvCentralSkinSettings(_windowXmlFileName);

        // Get Moving Pictures specific autoplay setting
        try
        {
          //                    diskInsertedAction = (DiskInsertedAction)Enum.Parse(typeof(DiskInsertedAction), mvCentralCore.Settings.DiskInsertionBehavior);
        }
        catch
        {
          //                    diskInsertedAction = DiskInsertedAction.DETAILS;
        }

        // setup the image resources for cover and backdrop display
        int artworkDelay = mvCentralCore.Settings.ArtworkLoadingDelay;

        // setup the time for the random category backdrop refresh
        /*                activeMovieLookup.Timeout = new TimeSpan(0, 0, mvCentralCore.Settings.CategoryRandomArtworkRefreshInterval);

                        // create backdrop image swapper
                        backdrop = new ImageSwapper();
                        backdrop.ImageResource.Delay = artworkDelay;
                        backdrop.PropertyOne = "#mvCentral.Backdrop";

                        // create cover image swapper
                        cover = new AsyncImageResource();
                        cover.Property = "#mvCentral.Coverart";
                        cover.Delay = artworkDelay;



                        // instantiate player
                        moviePlayer = new MoviePlayer(this);
                        moviePlayer.MovieEnded += new MoviePlayerEvent(onMovieEnded);
                        moviePlayer.MovieStopped += new MoviePlayerEvent(onMovieStopped);

                        // Listen to the DeviceManager for external media activity (i.e. disks inserted)
                        logger.Debug("Listening for device changes.");
                        DeviceManager.OnVolumeInserted += new DeviceManager.DeviceManagerEvent(OnVolumeInserted);
                        DeviceManager.OnVolumeRemoved += new DeviceManager.DeviceManagerEvent(OnVolumeRemoved);

                        // Flag that the GUI is initialized
        */
        initComplete = true;

        // If the initDialog is present close it
        //                if (initDialog != null)
        //                {
        //                    initDialog.Close();
        //                }

        // Report that we completed the init
        logger.Info("GUI Initialization Complete");
      }
    }

    #endregion




  }
}
