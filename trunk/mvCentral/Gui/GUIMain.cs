using System;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
// Cornerstone
using Cornerstone.Database;
using Cornerstone.Database.Tables;
using Cornerstone.GUI.Dialogs;
using Cornerstone.Tools;
// Internal
using mvCentral.Database;
using mvCentral;
using mvCentral.LocalMediaManagement;
using mvCentral.Playlist;
using mvCentral.Utils;
using mvCentral.Localizations;
// Mediaportal
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using Action = MediaPortal.GUI.Library.Action;
using Layout = MediaPortal.GUI.Library.GUIFacadeControl.Layout;
using WindowPlugins;
using NLog;

namespace mvCentral.GUI
{
  public partial class mvGUIMain : WindowPluginBase
  {

    #region enums

    public enum mvView
    {
      None,
      Artist,
      Album,
      Video,
      AllAlbums,
      AllVideos,
      VideosOnAlbum
    }

    public enum mvSort
    {
      ascending,
      desending
    }

    public enum View
    {
      Artists,
      Albums,
      Tracks
    }

    #endregion

    #region Declarations

    Dictionary<string, bool> loggedProperties;

    private static Logger logger = LogManager.GetCurrentClassLogger();
    private mvCentralCore core = mvCentralCore.Instance;
    GUImvPlayList Player = new GUImvPlayList();
    //GUImvStatsAndInfo Stats = new GUImvStatsAndInfo();

    private bool initComplete = false;
    private Thread initThread;
    private bool preventDialogOnLoad = false;
    private readonly object propertySync = new object();
    private bool persisting = false;
    private bool layoutChanging = false;
    private string selArtist = "";
    private int currentArtistID = -1;
    private string selAlbum = "";
    private mvView currentView = mvView.Artist;
    private mvView previousView = mvView.None;
    private mvSort artistSort = mvSort.ascending;
    private mvSort videoSort = mvSort.ascending;

    private List<string> artistTags = new List<string>();

    public int lastItemArt = 0, lastItemVid = 0, lastItemAlb = 0, artistID = 0, albumID = 0;

    protected mvView CurrentView
    {
      get { return currentView; }
      set { currentView = value; }
    }

    protected bool AllowView(View view)
    {
      return true;
    }

    #endregion

    #region Skin Connection

    private enum GUIControls
    {
      PlayAllRandom = 6,
      PlaySmart = 7,
      PlayList = 8,
      StatsAndInfo = 9,
      Facade = 50
    }

    [SkinControlAttribute((int)GUIControls.PlayAllRandom)]
    protected GUIButtonControl btnPlayAllRandom = null;
    [SkinControlAttribute((int)GUIControls.PlaySmart)]
    protected GUIButtonControl btnSmartList = null;
    [SkinControlAttribute((int)GUIControls.PlayList)]
    protected GUIButtonControl btnPlayList = null;
    [SkinControlAttribute((int)GUIControls.StatsAndInfo)]
    protected GUIButtonControl btnStatsAndInfo = null;

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
      artistTags.Clear();

      // start initialization of the music videos core services in a seperate thread
      initThread = new Thread(new ThreadStart(mvCentralCore.Initialize));
      initThread.Start();

      // ... and listen to the progress
      mvCentralCore.InitializeProgress += new ProgressDelegate(onCoreInitializationProgress);

      return success;
    }

    /// <summary>
    /// Handle keyboard/remote action
    /// </summary>
    /// <param name="action"></param>
    public override void OnAction(MediaPortal.GUI.Library.Action action)
    {
      MediaPortal.GUI.Library.Action.ActionType wID = action.wID;
      // Queue video to playlist
      if (wID == MediaPortal.GUI.Library.Action.ActionType.ACTION_QUEUE_ITEM)
      {
        facadeLayout.SelectedListItemIndex = Player.playlistPlayer.CurrentItem;
        return;
      }
      // If on Track screen go back to artists screen
      if (wID == MediaPortal.GUI.Library.Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        artistID = currentArtistID;
        switch (currentView)
        {
          case mvView.Album:
            loadCurrent();
            break;
          case mvView.Video:
            currentView = mvView.Artist;
            loadCurrent();
            break;
          case mvView.VideosOnAlbum:
            if (previousView == mvView.AllAlbums)
              currentView = previousView;
            else
              currentView = mvView.Video;

            loadCurrent();
            break;
          default:
            // We are out of here
            base.OnAction(action);
            break;
        }
      }
      else if (wID == MediaPortal.GUI.Library.Action.ActionType.ACTION_CONTEXT_MENU)
      {
        int contextChoice = ShowContextMenu();
        DBArtistInfo currArtist = null;
        switch (contextChoice)
        {
          case 0:
            //Add to playlist
            // If on a folder add all Videos for Artist
            if (facadeLayout.ListLayout.SelectedListItem.IsFolder)
            {
              currArtist = DBArtistInfo.Get(facadeLayout.ListLayout.SelectedListItem.Label);
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
            addToPlaylistNext(facadeLayout.SelectedListItem);
            break;
          case 3:
            if (facadeLayout.ListLayout.SelectedListItem.IsFolder)
            {
              currArtist = DBArtistInfo.Get(facadeLayout.SelectedListItem.Label);
              GUIWaitCursor.Show();
              mvCentralCore.DataProviderManager.GetArt(currArtist);
              GUIWaitCursor.Hide();
              facadeLayout.SelectedListItem.ThumbnailImage = currArtist.ArtThumbFullPath;
              facadeLayout.SelectedListItem.RefreshCoverArt();
              facadeLayout.SelectedListItemIndex = facadeLayout.SelectedListItemIndex - 1;
              facadeLayout.SelectedListItemIndex = facadeLayout.SelectedListItemIndex + 1;
            }
            else
            {
              DBTrackInfo video = (DBTrackInfo)facadeLayout.ListLayout.SelectedListItem.MusicTag;
              GUIWaitCursor.Show();
              mvCentralCore.DataProviderManager.GetArt(video);
              GUIWaitCursor.Hide();
              facadeLayout.SelectedListItem.ThumbnailImage = video.ArtFullPath;
              facadeLayout.SelectedListItem.RefreshCoverArt();
              facadeLayout.SelectedListItemIndex = facadeLayout.SelectedListItemIndex - 1;
              facadeLayout.SelectedListItemIndex = facadeLayout.SelectedListItemIndex + 1;
            }
            break;
          default:
            //Exit
            break;
        }
      }
      else
        base.OnAction(action);
    }
    /// <summary>
    /// Handle a playlist change
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
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
    /// <summary>
    /// De-init
    /// </summary>
    public override void DeInit()
    {
      base.DeInit();

      logger.Info("Deinitializing GUI...");

      // if the plugin was not fully initialized yet abort the initialization
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
    /// <summary>
    /// Return the plugin ID
    /// </summary>
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
    /// <summary>
    /// Retrun false for unsupported layouts - Windowsplugin Class override
    /// </summary>
    /// <param name="layout"></param>
    /// <returns></returns>
    protected override bool AllowLayout(Layout layout)
    {
      if (layout == Layout.AlbumView)
        return false;

      return base.AllowLayout(layout);
    }
    /// <summary>
    /// Load any settings - Windowsplugin Class override
    /// </summary>
    protected override void LoadSettings()
    {
      base.LoadSettings();
    }
    /// <summary>
    /// Save any settings - Windowsplugin Class override
    /// </summary>
    protected override void SaveSettings()
    {
      base.SaveSettings();
    }
    /// <summary>
    /// List avaiable views - Windowsplugin Class override
    /// </summary>
    protected override void OnShowViews()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(499); // Views menu
      dlg.Add(Localization.Artists);
      dlg.Add(Localization.Albums);
      dlg.Add(Localization.Tracks);

      // show dialog and wait for result
      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1)
        return;
      // Display Artists, tracks or Albums - TBA
      persisting = false;

      switch (dlg.SelectedId)
      {
        case 1:
          loadArtists(artistSort);
          currentView = mvView.Artist;
          break;
        case 2:
          loadAllAlbums();
          currentView = mvView.AllAlbums;
          break;
        case 3:
          loadAllVideos(videoSort);
          currentView = mvView.AllVideos;
          break;
      }
      previousView = currentView;
      GUIControl.FocusControl(GetID, facadeLayout.GetID);
    }
    /// <summary>
    /// Show the layout selection menu
    /// </summary>
    protected override void OnShowLayouts()
    {
      base.OnShowLayouts();

      if (currentView == mvView.Artist)
        facadeLayout.SelectedListItemIndex = lastItemArt;
      else if (currentView == mvView.Video)
        facadeLayout.SelectedListItemIndex = lastItemVid;
      else if (currentView == mvView.Album)
        facadeLayout.SelectedListItemIndex = lastItemAlb;
    }
    /// <summary>
    /// Show the sort options (None Currently)
    /// </summary>
    protected override void OnShowSort()
    {
      base.OnShowSort();
    }
    /// <summary>
    /// Sort direction chnage...so sort
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void SortChanged(object sender, SortEventArgs e)
    {
      sortFacade();
    }
    /// <summary>
    /// Sort the facade for chosen direction
    /// </summary>
    void sortFacade()
    {
      if (currentView == mvView.Artist)
      {
        if (artistSort == mvSort.ascending)
        {
          loadArtists(mvSort.desending);
        }
        else
        {
          loadArtists(mvSort.ascending);
        }
      }
      else if (currentView == mvView.Video)
      {
        if (videoSort == mvSort.ascending)
        {
          loadVideos(currentArtistID, mvSort.desending);
        }
        else
        {
          loadVideos(currentArtistID, mvSort.ascending);
        }
      }
      UpdateButtonStates();
    }
    /// <summary>
    /// Action the Info Button
    /// </summary>
    /// <param name="iItem"></param>
    protected override void OnInfo(int iItem)
    {
      base.OnInfo(iItem);
    }
    /// <summary>
    /// Queue and item
    /// </summary>
    /// <param name="item"></param>
    protected override void OnQueueItem(int item)
    {
      base.OnQueueItem(item);
    }
    /// <summary>
    /// Switch the layout
    /// </summary>
    protected override void SwitchLayout()
    {
      base.SwitchLayout();
      layoutChanging = true;
      loadCurrent();
      layoutChanging = false;
    }
    /// <summary>
    /// Deal with user input
    /// </summary>
    /// <param name="controlId"></param>
    /// <param name="control"></param>
    /// <param name="actionType"></param>
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
        case (int)GUIControls.StatsAndInfo:
          GUIWindowManager.ActivateWindow(GUImvStatsAndInfo.GetWindowId());
          break;
        case (int)GUIControls.Facade:
          //Clicked on something in the facade
          logger.Debug("Hit Key : " + actionType.ToString());
          switch (actionType)
          {
            case Action.ActionType.ACTION_PLAY:
            case Action.ActionType.ACTION_PAUSE:
            case Action.ActionType.ACTION_QUEUE_ITEM:
            case Action.ActionType.ACTION_MUSIC_PLAY:
            case Action.ActionType.ACTION_SELECT_ITEM:

              if (facadeLayout.SelectedListItem.IsFolder && ((actionType == Action.ActionType.ACTION_MUSIC_PLAY) || (actionType == Action.ActionType.ACTION_PAUSE)))
              {
                // Are we on an Artist or Album, if Artist add all tracks by artist to playlist else if album add all tracks on album to playlist
                if ((facadeLayout.SelectedListItem.MusicTag.GetType() == typeof(DBAlbumInfo)))
                  AlbumActions(actionType);
                else if ((facadeLayout.SelectedListItem.MusicTag.GetType() == typeof(DBArtistInfo)))
                  ArtistActions(actionType);
              }
              else
              {

                GUIListItem selectedItem = facadeLayout.SelectedListItem;
                if (!selectedItem.IsFolder && selectedItem.MusicTag != null)
                {
                  // we have a track selected so add any other tracks which are on showing on the facade
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
                  g_Player.ShowFullScreenWindow();
                  break;
                }
                //return to previous level
                if (facadeLayout.SelectedListItem.Label == "..")
                {
                  currentView = mvView.Artist;
                  loadCurrent();
                }
                else
                {
                  artistID = facadeLayout.SelectedListItem.ItemId;
                  previousView = currentView;
                  currentView = mvView.Video;
                  loadCurrent();
                }
              }
              break;
          }
          break;
      }
    }
    /// <summary>
    /// Initial load of GUI
    /// </summary>
    protected override void OnPageLoad()
    {
      // Get all Artists and Tracks
      List<DBArtistInfo> artList = DBArtistInfo.GetAll();
      List<DBTrackInfo> vidList = DBTrackInfo.GetAll();
      // Set Total Artists and Video Porperties
      GUIPropertyManager.SetProperty("#mvCentral.TotalArtists", artList.Count + " " + Localization.Artists);
      GUIPropertyManager.SetProperty("#mvCentral.TotalVideos", vidList.Count + " " + Localization.Videos);
      // Read last used layout from and set, default to list if not yet stored
      if (mvCentralCore.Settings.DefaultView == "lastused")
      {
        CurrentLayout = Layout.List;
        mvCentralCore.Settings.DefaultView = ((int)CurrentLayout).ToString();
      }

      CurrentLayout = (Layout)int.Parse(mvCentralCore.Settings.DefaultView);

      SwitchLayout();
      UpdateButtonStates();
      if (persisting)
      {
        loadCurrent();
      }
      else
      {
        loadArtists(artistSort);
      }

      GUIPropertyManager.Changed = true;

      if (btnSortBy != null)
      {
        btnSortBy.SortChanged += new SortEventHandler(SortChanged);
      }

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
        dlgMenu.Reset();
        dlgMenu.SetHeading(mvCentralCore.Settings.HomeScreenName + " - " + Localization.ContextMenu);
        if (this.facadeLayout.Count > 0)
        {
          dlgMenu.Add(Localization.AddToPlaylist);
          dlgMenu.Add(Localization.AddAllToPlaylist);
          if (Player.playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MVCENTRAL).Count > 0 && !(facadeLayout.ListLayout.SelectedListItem.IsFolder))
          {
            dlgMenu.Add(Localization.AddToPlaylistNext);
          }
          dlgMenu.Add(Localization.Cancel);

          dlgMenu.Add(Localization.RefreshArtwork);
        }
        dlgMenu.DoModal(GetID);

        if (dlgMenu.SelectedLabel == -1) // Nothing was selected
          return -1;

        return dlgMenu.SelectedLabel;
      }
      return -1;
    }
    /// <summary>
    /// Load the current view and set the idex to the last used.
    /// </summary>
    private void loadCurrent()
    {
      persisting = true;     
      switch (currentView)
      {
        case mvView.Artist:
          loadArtists(artistSort);
          facadeLayout.SelectedListItemIndex = lastItemArt;
          break;
        case mvView.Video:
          loadVideos(artistID, videoSort);
          facadeLayout.SelectedListItemIndex = lastItemVid;
          break;
        case mvView.AllAlbums:
          loadAllAlbums();
          facadeLayout.SelectedListItemIndex = lastItemAlb;
          break;
        case mvView.AllVideos:
          loadAllVideos(videoSort);
          facadeLayout.SelectedListItemIndex = lastItemVid;
          break;
        case mvView.VideosOnAlbum:
          artistID = currentArtistID;
          loadVideos(artistID, videoSort);
          facadeLayout.SelectedListItemIndex = lastItemVid;
          break;
      }
    }
    /// <summary>
    /// Load artists
    /// </summary>
    private void loadArtists(mvSort sortDirection)
    {
      artistSort = sortDirection;
      // set the view
      currentView = mvView.Artist;
      List<DBArtistInfo> artistList = DBArtistInfo.GetAll();
      GUIPropertyManager.SetProperty("#mvCentral.Hierachy", Localization.Artists);
      GUIPropertyManager.SetProperty("#itemcount", artistList.Count.ToString());
      GUIPropertyManager.Changed = true;

      // Sort Artists
      if (sortDirection == mvSort.ascending)
        artistList.Sort(delegate(DBArtistInfo p1, DBArtistInfo p2) { return p1.Artist.CompareTo(p2.Artist); });
      else
        artistList.Sort(delegate(DBArtistInfo p1, DBArtistInfo p2) { return p2.Artist.CompareTo(p1.Artist); });

      // Clear the facade and load the artists
      GUIControl.ClearControl(GetID, facadeLayout.GetID);
      foreach (DBArtistInfo artistData in artistList)
      {
        GUIListItem facadeItem = new GUIListItem();

        facadeItem.Label = artistData.Artist;
        if (string.IsNullOrEmpty(artistData.ArtThumbFullPath.Trim()))
          facadeItem.ThumbnailImage = artistTrackArt(artistData);
        else
          facadeItem.ThumbnailImage = artistData.ArtThumbFullPath;

        facadeItem.TVTag = artistData.bioContent;
        facadeItem.AlbumInfoTag = artistData.bioContent;
        facadeItem.ItemId = (int)artistData.ID;
        facadeItem.IsFolder = true;
        facadeItem.OnItemSelected += new GUIListItem.ItemSelectedHandler(onArtistSelected);
        facadeLayout.Add(facadeItem);

        foreach (string artistTag in artistData.Tag)
        {
          if (!artistTags.Contains(artistTag))
          {
            if (artistTag != artistData.Artist)
              artistTags.Add(artistTag);
          }
        }
      }
      artistTags.Sort(delegate(string p1, string p2) { return p1.CompareTo(p2); });

      // If first time though set properites to first item in facade
      if (facadeLayout.Count > 0 && !persisting)
      {
        facadeLayout.SelectedListItemIndex = 0;
        onArtistSelected(facadeLayout.SelectedListItem, facadeLayout);
      }
      persisting = true;
      GUIPropertyManager.SetProperty("#mvCentral.ArtistView", "true");
      GUIPropertyManager.SetProperty("#mvCentral.TrackView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.AlbumView", "false");
      clearVideoAudioProps();
      GUIPropertyManager.Changed = true;


    }
    /// <summary>
    /// Return artwork for the first track to have it, used in event of Artist having no art.
    /// </summary>
    /// <param name="artist"></param>
    /// <returns></returns>
    string artistTrackArt(DBArtistInfo artist)
    {
      List<DBTrackInfo> artistTracks = DBTrackInfo.GetEntriesByArtist(artist);
      foreach (DBTrackInfo artistTrack in artistTracks)
      {
        if (!string.IsNullOrEmpty(artistTrack.ArtFullPath.Trim()))
          return artistTrack.ArtFullPath;
      }
      return "defaultArtistBig.png";
    }
    /// <summary>
    /// Load and Display all Videos
    /// </summary>
    /// <param name="sortOrder"></param>
    private void loadAllVideos(mvSort sortOrder)
    {
      currentView = mvView.Video;
      videoSort = sortOrder;

      // Get list of all videos
      List<DBTrackInfo> artistTrackList = DBTrackInfo.GetAll();

      // Sort the tracks
      if (sortOrder == mvSort.ascending)
        artistTrackList.Sort(delegate(DBTrackInfo p1, DBTrackInfo p2) { return p1.Track.CompareTo(p2.Track); });
      else
        artistTrackList.Sort(delegate(DBTrackInfo p1, DBTrackInfo p2) { return p2.Track.CompareTo(p1.Track); });

      // Clear the facade
      GUIControl.ClearControl(GetID, facadeLayout.GetID);

      // Load tracks into facade
      foreach (DBTrackInfo trackData in artistTrackList)
      {
        GUIListItem facadeItem = new GUIListItem();
        facadeItem.Label = trackData.Track;
        facadeItem.TVTag = trackData.bioContent;
        facadeItem.Path = trackData.LocalMedia[0].File.FullName;
        facadeItem.IsFolder = false;
        facadeItem.OnItemSelected += new GUIListItem.ItemSelectedHandler(onVideoSelected);
        facadeItem.MusicTag = trackData;
        // If no thumbnail set a default
        if (!string.IsNullOrEmpty(trackData.ArtThumbFullPath.Trim()))
          facadeItem.ThumbnailImage = trackData.ArtThumbFullPath;
        else
          facadeItem.ThumbnailImage = "defaultVideoBig.png";

        facadeLayout.Add(facadeItem);
      }
      // Set properities to first item in list
      if (facadeLayout.Count > 0 && !persisting)
      {
        facadeLayout.SelectedListItemIndex = 0;
        onVideoSelected(facadeLayout.SelectedListItem, facadeLayout);
      }
      persisting = true;
      GUIPropertyManager.SetProperty("#itemcount", facadeLayout.Count.ToString());
      GUIPropertyManager.SetProperty("#mvCentral.Hierachy", Localization.Videos);
      GUIPropertyManager.SetProperty("#mvCentral.ArtistView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.TrackView", "true");
      GUIPropertyManager.SetProperty("#mvCentral.AlbumView", "false");
      clearVideoAudioProps();
      GUIPropertyManager.Changed = true;

      // Set properities to first item in list
      if (facadeLayout.Count > 0)
      {
        facadeLayout.SelectedListItemIndex = 0;
        onVideoSelected(facadeLayout.SelectedListItem, facadeLayout);
      }
    }

    /// <summary>
    /// Load Videos
    /// </summary>
    /// <param name="ArtistID"></param>
    private void loadVideos(int ArtistID, mvSort sortOrder)
    {
      DBAlbumInfo db1 = null;
      //previousView = currentView;
      videoSort = sortOrder;

      GUIPropertyManager.SetProperty("#mvCentral.Hierachy", Localization.Videos + " | " + DBArtistInfo.Get(ArtistID));
      GUIPropertyManager.Changed = true;
      // Load the albums (Not used Currently)
      if (facadeLayout.SelectedListItem == null)
      {
        if (albumID != 0)
        {
          LoadTracksOnAlbum(albumID);
          GUIPropertyManager.SetProperty("#mvCentral.Hierachy", Localization.Albums + " | " + DBArtistInfo.Get(ArtistID));
          return;
        }

      }
      // If we are on an artist - load the album (Not Used Currently) - *** Possible Error ***
      if (facadeLayout.SelectedListItem != null)
        if ((facadeLayout.SelectedListItem.MusicTag != null && facadeLayout.SelectedListItem.MusicTag.GetType() == typeof(DBAlbumInfo)) || (layoutChanging && currentView == mvView.VideosOnAlbum))
        {
          if (!(layoutChanging && currentView != mvView.VideosOnAlbum))
          {

            if ((previousView != mvView.AllAlbums || previousView != mvView.VideosOnAlbum) && !layoutChanging)
              previousView = mvView.Album;

            if (layoutChanging)
            {
              db1 = DBAlbumInfo.Get(albumID);
            }
            else
            {
              db1 = (DBAlbumInfo)facadeLayout.SelectedListItem.MusicTag;
              albumID = db1.ID.Value;
            }

            LoadTracksOnAlbum(db1.ID.Value);
            GUIPropertyManager.SetProperty("#mvCentral.Hierachy", Localization.Albums + " | " + db1.Album);
            GUIPropertyManager.SetProperty("#mvCentral.ArtistView", "false");
            GUIPropertyManager.SetProperty("#mvCentral.TrackView", "false");
            GUIPropertyManager.SetProperty("#mvCentral.AlbumView", "true");
            GUIPropertyManager.Changed = true;
            return;
          }
        }
      previousView = currentView;
      // Ok, we are now on Videos against albums so set the view
      currentView = mvView.Video;
      // Grab the info for the currently selected artist
      DBArtistInfo currArtist = DBArtistInfo.Get(ArtistID);
      //  and store it
      currentArtistID = ArtistID;
      // Load all videos for selected artist
      List<DBTrackInfo> artistTrackList = DBTrackInfo.GetEntriesByArtist(currArtist);
      // And sort
      if (sortOrder == mvSort.ascending)
        artistTrackList.Sort(delegate(DBTrackInfo p1, DBTrackInfo p2) { return p1.Track.CompareTo(p2.Track); });
      else
        artistTrackList.Sort(delegate(DBTrackInfo p1, DBTrackInfo p2) { return p2.Track.CompareTo(p1.Track); });


      this.artistID = ArtistID;
      // Clear facade 
      GUIControl.ClearControl(GetID, facadeLayout.GetID);
      // and load tracks if we dont already have them loaded
      foreach (DBTrackInfo trackData in artistTrackList)
      {
        // If no Album is associated then skip to next track
        if (trackData.AlbumInfo.Count == 0)
          continue;

        // Check if we already have this album as a facade item
        bool IsPresent = false;
        for (int i = 0; i <= facadeLayout.Count - 1; i++)
        {
          if (facadeLayout[i].Label == trackData.AlbumInfo[0].Album)
            IsPresent = true;
        }
        // and skip adding if we do
        if (IsPresent)
          continue;

        // Add Album to facade
        {
          GUIListItem item = new GUIListItem();
          item.Label = trackData.AlbumInfo[0].Album;

          if (string.IsNullOrEmpty(trackData.AlbumInfo[0].ArtThumbFullPath.Trim()))
            item.ThumbnailImage = "defaultVideoBig.png";
          else
            item.ThumbnailImage = trackData.AlbumInfo[0].ArtThumbFullPath;

          item.TVTag = trackData.AlbumInfo[0].bioContent;
          selArtist = currArtist.Artist;
          item.IsFolder = true;
          item.OnItemSelected += new GUIListItem.ItemSelectedHandler(onVideoSelected);
          item.MusicTag = trackData.AlbumInfo[0];
          facadeLayout.Add(item);
        }
      }
      // Load tracks we don't have loaded
      foreach (DBTrackInfo trackData in artistTrackList)
      {
        // if this track is part of an album then skip the adding track to the facade
        if (trackData.AlbumInfo.Count > 0) 
          continue;
        
        GUIListItem facadeItem = new GUIListItem();
        facadeItem.Label = trackData.Track;
        facadeItem.TVTag = trackData.bioContent;
        selArtist = currArtist.Artist;
        facadeItem.Path = trackData.LocalMedia[0].File.FullName;
        facadeItem.IsFolder = false;
        facadeItem.OnItemSelected += new GUIListItem.ItemSelectedHandler(onVideoSelected);
        facadeItem.MusicTag = trackData;
        // If no thumbnail set a default
        if (!string.IsNullOrEmpty(trackData.ArtThumbFullPath.Trim()))
          facadeItem.ThumbnailImage = trackData.ArtThumbFullPath;
        else
          facadeItem.ThumbnailImage = "defaultVideoBig.png";

        facadeLayout.Add(facadeItem);
      }
      // Set properities to first item in list
      if (facadeLayout.Count > 0 && !persisting)
      {
        facadeLayout.SelectedListItemIndex = 0;
        onVideoSelected(facadeLayout.SelectedListItem, facadeLayout);
      }
      persisting = true;
      GUIPropertyManager.SetProperty("#mvCentral.Hierachy", Localization.Albums);
      GUIPropertyManager.SetProperty("#itemcount", facadeLayout.Count.ToString());
      // Set the view
      GUIPropertyManager.SetProperty("#mvCentral.ArtistView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.TrackView", "true");
      GUIPropertyManager.SetProperty("#mvCentral.AlbumView", "false");
      clearVideoAudioProps();
      GUIPropertyManager.Changed = true;
    }
    /// <summary>
    /// Load and Display All Albums
    /// </summary>
    private void loadAllAlbums()
    {
      previousView = currentView;
      currentView = mvView.AllAlbums;

      GUIPropertyManager.SetProperty("#mvCentral.Hierachy", Localization.Albums);
      List<DBAlbumInfo> allAlbumsList = DBAlbumInfo.GetAll();

      // Clear facade and load tracks if we dont already have them loaded
      GUIControl.ClearControl(GetID, facadeLayout.GetID);

      // Load all the albums into the facade
      foreach (DBAlbumInfo theAlbum in allAlbumsList)
      {
        GUIListItem item = new GUIListItem();
        item.Label = theAlbum.Album;

        if (string.IsNullOrEmpty(theAlbum.ArtThumbFullPath))
          item.ThumbnailImage = "defaultAlbum.png";
        else
          item.ThumbnailImage = theAlbum.ArtThumbFullPath;

        item.OnItemSelected += new GUIListItem.ItemSelectedHandler(onVideoSelected);
        item.TVTag = theAlbum.bioContent;
        item.IsFolder = true;
        item.MusicTag = theAlbum;
        facadeLayout.Add(item);
      }
      GUIPropertyManager.SetProperty("#mvCentral.Hierachy", Localization.Videos);
      GUIPropertyManager.SetProperty("#itemcount", facadeLayout.Count.ToString());
      GUIPropertyManager.SetProperty("#mvCentral.ArtistView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.TrackView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.AlbumView", "true");
      clearVideoAudioProps();
      GUIPropertyManager.Changed = true;

      // Set properities to first item in list
      if (facadeLayout.Count > 0 && !persisting)
      {
        facadeLayout.SelectedListItemIndex = 0;
        onVideoSelected(facadeLayout.SelectedListItem, facadeLayout);
      }
      persisting = true;
    }
    /// <summary>
    /// Load Videos for this Album
    /// </summary>
    /// <param name="AlbumID"></param>
    private void LoadTracksOnAlbum(int AlbumID)
    {
      previousView = currentView;
      currentView = mvView.VideosOnAlbum;
      GUIPropertyManager.SetProperty("#mvCentral.Hierachy", Localization.Album + " | " + DBAlbumInfo.Get(AlbumID));
      DBAlbumInfo currAlbum = DBAlbumInfo.Get(AlbumID);
      List<DBTrackInfo> list = DBTrackInfo.GetEntriesByAlbum(currAlbum);
      //
      this.albumID = AlbumID;
      // Clear facade and load tracks if we dont already have them loaded
      GUIControl.ClearControl(GetID, facadeLayout.GetID);
      foreach (DBTrackInfo db1 in list)
      {
        GUIListItem item = new GUIListItem();
        item.Label = db1.Track;
        if (string.IsNullOrEmpty(db1.ArtThumbFullPath.Trim()))
          item.ThumbnailImage = "defaultAlbum.png";
        else
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
        facadeLayout.SelectedListItemIndex = 0;
        onVideoSelected(facadeLayout.SelectedListItem, facadeLayout);
      }
      persisting = true;

      GUIPropertyManager.SetProperty("#itemcount", facadeLayout.Count.ToString());
      // Set the view
      GUIPropertyManager.SetProperty("#mvCentral.ArtistView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.TrackView", "true");
      GUIPropertyManager.SetProperty("#mvCentral.AlbumView", "false");
      // Tell property manager we have changed something
      GUIPropertyManager.Changed = true;
    }
    /// <summary>
    /// Actions to perform when artist selected
    /// </summary>
    /// <param name="item"></param>
    /// <param name="parent"></param>
    void onArtistSelected(GUIListItem item, GUIControl parent)
    {
      // Set the Bio Content
      if (string.IsNullOrEmpty(item.TVTag.ToString().Trim()))
        GUIPropertyManager.SetProperty("#mvCentral.ArtistBio", string.Format(Localization.NoArtistBio, item.Label));
      else
        GUIPropertyManager.SetProperty("#mvCentral.ArtistBio", item.TVTag.ToString());
      // Set artist name and image
      GUIPropertyManager.SetProperty("#mvCentral.ArtistName", item.Label);
      GUIPropertyManager.SetProperty("#mvCentral.ArtistImg", item.ThumbnailImage);
      // How many videos do we have for this artist
      DBArtistInfo currArtist = DBArtistInfo.Get(item.Label);
      GUIPropertyManager.SetProperty("#mvCentral.VideosByArtist", DBTrackInfo.GetEntriesByArtist(currArtist).Count.ToString());
      GUIPropertyManager.SetProperty("#mvCentral.ArtistTracksRuntime", runningTime(DBTrackInfo.GetEntriesByArtist(currArtist)));
      // Artist Genres
      string artistTags = string.Empty;
      foreach (string tag in currArtist.Tag)
        artistTags += tag + " | ";
      if (!string.IsNullOrEmpty(artistTags))
        GUIPropertyManager.SetProperty("#mvCentral.ArtistTags", artistTags.Remove(artistTags.Length - 2, 2));
      // Clear properites set for tracks
      clearVideoAudioProps();
      // Set the View
      GUIPropertyManager.SetProperty("#mvCentral.ArtistView", "true");
      GUIPropertyManager.SetProperty("#mvCentral.TrackView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.AlbumView", "false");
      // Let property manager knwo we have chnages something
      GUIPropertyManager.Changed = true;
      // Store postions in facade
      lastItemArt = facadeLayout.SelectedListItemIndex;
    }
    /// <summary>
    /// Video/Album item selected - set a load of properities
    /// </summary>
    /// <param name="item"></param>
    /// <param name="parent"></param>
    void onVideoSelected(GUIListItem item, GUIControl parent)
    {
      DBAlbumInfo albumInfo = null;
      DBTrackInfo trackInfo = null;

      // Is this a Track or Album object we are on
      if (item.MusicTag.GetType() == typeof(DBTrackInfo))
      {
        // This is an Video
        trackInfo = (DBTrackInfo)item.MusicTag;
        GUIPropertyManager.SetProperty("#mvCentral.VideoImg", item.ThumbnailImage);
        if (string.IsNullOrEmpty(item.TVTag.ToString().Trim()))
          GUIPropertyManager.SetProperty("#mvCentral.TrackInfo", "No Track Information Avaiable");
        else
          GUIPropertyManager.SetProperty("#mvCentral.TrackInfo", item.TVTag.ToString());

        DBLocalMedia mediaInfo = (DBLocalMedia)trackInfo.LocalMedia[0];
        DBArtistInfo artistInfo = trackInfo.ArtistInfo[0];
        // Video
        GUIPropertyManager.SetProperty("#mvCentral.LocalMedia.videoresolution", mediaInfo.VideoResolution);
        GUIPropertyManager.SetProperty("#mvCentral.LocalMedia.videoaspectratio", mediaInfo.VideoAspectRatio);
        GUIPropertyManager.SetProperty("#mvCentral.LocalMedia.videocodec", mediaInfo.VideoCodec);
        logger.Debug(string.Format("Video Props for {0} - (Res: {1}) (Aspect: {2}) (Codec: {3})", item.Label, mediaInfo.VideoResolution, mediaInfo.VideoAspectRatio, mediaInfo.VideoCodec));
        // Audio
        GUIPropertyManager.SetProperty("#mvCentral.LocalMedia.audiocodec", mediaInfo.AudioCodec);
        GUIPropertyManager.SetProperty("#mvCentral.LocalMedia.audiochannels", mediaInfo.AudioChannels);
        GUIPropertyManager.SetProperty("#mvCentral.LocalMedia.audio", string.Format("{0} {1}", mediaInfo.AudioCodec, mediaInfo.AudioChannels));
        logger.Debug(string.Format("Audio Props for {0} - (Codec: {1}) (Channels: {2})", item.Label, mediaInfo.AudioCodec, mediaInfo.AudioChannels));
        // Misc Proprities
        GUIPropertyManager.SetProperty("#mvCentral.Duration", trackDuration(trackInfo.PlayTime));
        GUIPropertyManager.SetProperty("#mvCentral.ArtistName", artistInfo.Artist);
        // Set the view
        GUIPropertyManager.SetProperty("#mvCentral.TrackView", "true");
        GUIPropertyManager.SetProperty("#mvCentral.ArtistView", "false");
        GUIPropertyManager.SetProperty("#mvCentral.AlbumView", "false");
      }
      else
      {
        // This is a Album
        albumInfo = (DBAlbumInfo)item.MusicTag;
        GUIPropertyManager.SetProperty("#mvCentral.Hierachy", Localization.Album + " | " + DBAlbumInfo.Get(albumInfo.Album));
        GUIPropertyManager.SetProperty("#mvCentral.Album", albumInfo.Album);
        // get list of tracks in this album
        List<DBTrackInfo> tracksInAlbum = DBTrackInfo.GetEntriesByAlbum(albumInfo);
        // Set image
        GUIPropertyManager.SetProperty("#mvCentral.VideoImg", item.ThumbnailImage);
        if (string.IsNullOrEmpty(item.TVTag.ToString().Trim()))
          GUIPropertyManager.SetProperty("#mvCentral.TrackInfo", "No Track Information Avaiable");
        else
          GUIPropertyManager.SetProperty("#mvCentral.TrackInfo", item.TVTag.ToString());
        // Set tracks and Runtime for Album contents
        GUIPropertyManager.SetProperty("#mvCentral.AlbumTracksRuntime", runningTime(tracksInAlbum));
        GUIPropertyManager.SetProperty("#mvCentral.TracksForAlbum", tracksInAlbum.Count.ToString());
        // Set the View
        GUIPropertyManager.SetProperty("#mvCentral.AlbumView", "true");
        GUIPropertyManager.SetProperty("#mvCentral.ArtistView", "false");
        GUIPropertyManager.SetProperty("#mvCentral.TrackView", "false");
      }
      GUIPropertyManager.Changed = true;
      if (item.Label != "..")
      {
        lastItemVid = facadeLayout.SelectedListItemIndex;
        lastItemAlb = facadeLayout.SelectedListItemIndex;
      }
    }
    /// <summary>
    /// Clear the Video and Audio Properities
    /// </summary>
    private void clearVideoAudioProps()
    {
      // Clear the video properites
      GUIPropertyManager.SetProperty("#mvCentral.LocalMedia.videoresolution", string.Empty);
      GUIPropertyManager.SetProperty("#mvCentral.LocalMedia.videoaspectratio", string.Empty);
      GUIPropertyManager.SetProperty("#mvCentral.LocalMedia.videocodec", string.Empty);
      // Audio
      GUIPropertyManager.SetProperty("#mvCentral.LocalMedia.audiocodec", string.Empty);
      GUIPropertyManager.SetProperty("#mvCentral.LocalMedia.audiochannels", string.Empty);
      GUIPropertyManager.SetProperty("#mvCentral.LocalMedia.audio", string.Empty);
    }
    /// <summary>
    /// Convert the track running time
    /// </summary>
    /// <param name="playTime"></param>
    /// <returns></returns>
    private string trackDuration(string playTime)
    {
      try
      {
        TimeSpan tt = TimeSpan.Parse(playTime);
        DateTime dt = new DateTime(tt.Ticks);
        string cTime = String.Format("{0:HH:mm:ss}", dt);
        if (cTime.StartsWith("00:"))
          return cTime.Substring(3);
        else
          return cTime;
      }
      catch
      {
        return "00:00:00";
      }
    }
    /// <summary>
    /// Give total running time for supplied tracklist
    /// </summary>
    /// <param name="property"></param>
    /// <param name="value"></param>
    private string runningTime(List<DBTrackInfo> trackList)
    {
      TimeSpan tt = TimeSpan.Parse("00:00:00");
      foreach (DBTrackInfo track in trackList)
      {
        tt += TimeSpan.Parse(track.PlayTime);
      }
      DateTime dt = new DateTime(tt.Ticks);
      string cTime = String.Format("{0:HH:mm:ss}", dt);
      if (cTime.StartsWith("00:"))
        return cTime.Substring(3);
      else
        return cTime;
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
