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

using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using mvCentral.Database;
using mvCentral;
using mvCentral.LocalMediaManagement;
using mvCentral.Playlist;

namespace mvCentral.GUI
{
  public partial class mvGUIMain : GUIWindow
  {
    private static Logger logger = LogManager.GetCurrentClassLogger();
    private mvCentralCore core = mvCentralCore.Instance;

    private bool initComplete = false;
    private Thread initThread;
    private bool preventDialogOnLoad = false;

    Dictionary<string, bool> loggedProperties;
    private readonly object propertySync = new object();


    private System.Windows.Forms.Timer checkTrack = new System.Windows.Forms.Timer();
    private string lastTrack = "";



    public mvPlayer play1 ;
    private bool persisting = false;
    private View currentView = View.None;
    public int lastItemArt = 0, lastItemVid = 0, artistID = 0;
    private string selArtist = "";


    #region Skin Connection

    private enum GUIControls
    {
      PlayAllRandom = 2,
      PlaySmart = 3,
      PlayList = 4,
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

    [SkinControlAttribute((int)GUIControls.PlayAllRandom)]
    protected GUIButtonControl playAllRandom = null;
    [SkinControlAttribute((int)GUIControls.PlaySmart)]
    protected GUIButtonControl playSmartList = null;
    [SkinControlAttribute((int)GUIControls.PlayList)]
    protected GUIButtonControl sowPlayList = null;
    [SkinControlAttribute((int)GUIControls.Hierachy)]
    protected GUILabelControl hierachy = null;
    [SkinControlAttribute((int)GUIControls.ArtistName)]
    protected GUILabelControl artistName = null;
    [SkinControlAttribute((int)GUIControls.ArtistBio)]
    protected GUITextScrollUpControl artistBio = null;
    [SkinControlAttribute((int)GUIControls.ArtistImage)]
    protected GUIImage artistImage = null;
    [SkinControlAttribute((int)GUIControls.VideoImage)]
    protected GUIImage videoImage = null;
    [SkinControlAttribute((int)GUIControls.VideoCount)]
    protected GUILabelControl videoCount = null;
    [SkinControlAttribute((int)GUIControls.ArtistCount)]
    protected GUILabelControl artistCount = null;
    [SkinControlAttribute((int)GUIControls.FavVidLabel)]
    protected GUIFadeLabel favVidLabel = null;
    [SkinControlAttribute((int)GUIControls.FavVidImage)]
    protected GUIImage favVidImage = null;
    [SkinControlAttribute((int)GUIControls.FavArtLabel)]
    protected GUILabelControl favArtLabel = null;
    [SkinControlAttribute((int)GUIControls.FavArtImage)]
    protected GUIImage favArtImage = null;
    [SkinControlAttribute((int)GUIControls.SortLabel)]
    protected GUILabelControl SortLabel = null;
    [SkinControlAttribute((int)GUIControls.DummyLabel)]
    protected GUILabelControl dummyLabel = null;
    [SkinControlAttribute((int)GUIControls.Facade)]
    protected GUIFacadeControl facade;

    #endregion


    public mvGUIMain()
    {
      timeOut.Tick += new EventHandler(checkTime);
      play1 = new mvPlayer(this);
//      g_Player.PlayBackChanged += new g_Player.ChangedHandler(OnChanged);
//      g_Player.PlayBackStarted += new g_Player.StartedHandler(OnStarted);
//      g_Player.PlayBackStopped += new g_Player.StoppedHandler(OnStopped);
//      g_Player.PlayBackEnded += new g_Player.EndedHandler(OnEnded);
      
 //     GUIWindowManager.Receivers += new SendMessageHandler(this.On1Message);
 //     GUIGraphicsContext.Receivers += new SendMessageHandler(this.On1Message);

    }


    public override bool OnMessage(GUIMessage message)
    {
        switch (message.Message)
        {
            case GUIMessage.MessageType.GUI_MSG_PLAYBACK_STOPPED:
//                PlayCD(message.Label);
                break;

                if (!g_Player.Playing)
                {
                    //                        listPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_VIDEO;
                    int count = play1.listPlayer.GetPlaylist(PlayListType.PLAYLIST_MVCENTRAL).Count;
                    lastItemVid++;
                    if (lastItemVid <= count)
                    {
                        PlayListItem p1 = play1.currentPlayList[lastItemVid - 1];
                        if (p1.Track != null)
                        {
                            DBTrackInfo mv = p1.Track;
                            if (mv != null)
                            {
                                //                                    if (mv.LocalMedia[0].IsDVD)
                                {
                                    play1.Play(mv);
                                    //                               CurrentTrack = mv;
                                    //                                listPlayer.Play(_gui.lastItemVid - 1);
                                    //                listPlayer.PlayNext();
                                    //                                    PlayDVD(mv);
                                }
                            }
                        }

                    }
                }

                break;

            case GUIMessage.MessageType.GUI_MSG_VOLUME_REMOVED:
//                MusicCD = null;
                break;
        }
        return base.OnMessage(message);

    }



    private void OnStopped(g_Player.MediaType type, int stoptime, string filename)
    {
        //       listPlayer.PlayNext();
    }

    private void OnChanged(g_Player.MediaType type, int stoptime, string filename)
    {
 //       listPlayer.PlayNext();
    }

    void OnStarted(g_Player.MediaType type, string filename)
    {
        if (play1.listPlayer.CurrentItem == -1) return;
        object o = play1.listPlayer.GetCurrentItem().Track;
        if (o != null && o.GetType() == typeof(DBTrackInfo))
        {
            DBTrackInfo mv = (DBTrackInfo)o;
            if (mv.LocalMedia[0].IsDVD)
            {
//                PlayDVD(mv);
            }
        }
    }

    void OnEnded(g_Player.MediaType type, string filename)
    {
    }

      void TrackChanged(object sender, EventArgs e)
    {
      string currentTrack = "";
      try
      {
          currentTrack = play1.listPlayer.GetCurrentItem().FileName;
      }
      catch { }
      if (lastTrack != currentTrack)
      {
        if (!(currentTrack == ""))
        {
          lastTrack = currentTrack;
        }
      }
    }

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

    private enum View
    {
      None,
      Artist,
      Video
    }

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

    private int ShowContextMenu()
    {
      GUIDialogMenu dlgMenu = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlgMenu != null)
      {
        logger.Info("HERE");
        dlgMenu.Reset();
        dlgMenu.SetHeading(mvCentralCore.Settings.HomeScreenName + " - Context Menu");
        if (this.facade.Count > 0)
        {
          dlgMenu.Add("Add to playlist");
          dlgMenu.Add("Add all to playlist");
          if (play1.listPlayer.GetPlaylist(PlayListType.PLAYLIST_MVCENTRAL).Count > 0 && !(facade.ListLayout.SelectedListItem.IsFolder))
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

    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      switch (controlId)
      {
        case (int)GUIControls.PlayAllRandom:
          playRandomAll();
          break;
        case (int)GUIControls.PlaySmart:
          playSmart(ChooseSmartPlay());
          break;
        case (int)GUIControls.PlayList:
          GUIWindowManager.ActivateWindow(88889);
          break;
        case (int)GUIControls.Facade:
          //Clicked on something in the facade
          if (facade.ListLayout.SelectedListItem.IsFolder)
          {
            ArtistActions(actionType);
          }
          else
          {
            GUIListItem selectedItem = facade.ListLayout.SelectedListItem;
            if (!selectedItem.IsFolder && selectedItem.MusicTag != null)
          { // we have a track selected so add any other tracks which
            // are on showing on the facade
            List<DBTrackInfo> list1 = new List<DBTrackInfo>();
            for(int i = 0; i < facade.ListLayout.Count; i++)
            {
              GUIListItem trackItem = facade.ListLayout[i];

              if (!trackItem.IsFolder && trackItem.MusicTag != null)
              {
                  list1.Add((DBTrackInfo)trackItem.MusicTag);
              }
            }
                
            addToPlaylist(list1, false, true, false);

            GUImvPlayList pgui = (GUImvPlayList)GUIWindowManager.GetWindow(88889);
            pgui.playlistPlayer.Play(lastItemVid - 1);
            play1.listPlayer.CurrentPlaylistName = "Test";
            play1.currentPlayList = play1.listPlayer.GetPlaylist(PlayListType.PLAYLIST_MVCENTRAL);
//            facade.ListLayout.SelectedItem = facade.ListLayout.SelectedListItemIndex - 1;
//            play1.Play((DBTrackInfo)play1.currentPlayList[lastItemVid - 1].Track);
//            listPlayer.Play(lastItemVid-1);
//            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
            break;
          }
            //return to previous level
            if (facade.ListLayout.SelectedListItem.Label == "..")
            {
                currentView = View.Artist;
                loadCurrent();
            }

//            VideoActions(actionType);
          }
          break;
      }
      //DebugMsg("Pressed: " + actionType.ToString());
      base.OnClicked(controlId, control, actionType);
    }

    public override void OnAction(MediaPortal.GUI.Library.Action action)
    {
      MediaPortal.GUI.Library.Action.ActionType wID = action.wID;
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
            if (facade.ListLayout.SelectedListItem.IsFolder)
            {
              DBArtistInfo currArtist = DBArtistInfo.Get(artistID);
              List<DBTrackInfo> list = DBTrackInfo.GetEntriesByArtist(currArtist);

              addToPlaylist(list, false, false, false);
            }
            else
            {
                PlayList playlist = play1.listPlayer.GetPlaylist(PlayListType.PLAYLIST_MVCENTRAL);
              string filename = facade.ListLayout.SelectedListItem.Label;
              string path = facade.ListLayout.SelectedListItem.Path;
              DBTrackInfo video = (DBTrackInfo)facade.ListLayout.SelectedListItem.MusicTag;
              PlayListItem p1 = new PlayListItem(video);
              p1.Track = video;
              playlist.Add(p1);

//              playlist.Add(new PlayListItem(filename, path));
            }
            break;
          case 1:
            if (facade.ListLayout.SelectedListItem.IsFolder)
            {
              // addToPlaylist(dm.getAllVideos(), true, true, false);
            }
            else
            {
              if (artistID != 0)
              {
                DBArtistInfo currArtist = DBArtistInfo.Get(artistID);
                List<DBTrackInfo> list = DBTrackInfo.GetEntriesByArtist(currArtist);
                addToPlaylist(list, false, false, false);
              }
            }
            break;
          case 2:
            addToPlaylistNext(facade.ListLayout.SelectedListItem);
            break;
          case 3:
          case -1:
            //Exit
            break;
        }
      }
      else
        base.OnAction(action);
    }

    protected override void OnPageLoad()
    {
      //            string[] stats = dm.getStats();     
      //            GUIPropertyManager.SetProperty("#MusicVids.TotalVideos", stats[0] + " Videos");
      //            GUIPropertyManager.SetProperty("#MusicVids.TotalArtists", stats[1] + " Artists");
      //            GUIPropertyManager.SetProperty("#MusicVids.MostPlayed", stats[2]);
      //            GUIPropertyManager.SetProperty("#MusicVids.FavArtist", stats[4]);
      //            favVidImage.SetFileName(stats[3]);
      //            favArtImage.SetFileName(stats[5]);

      List<DBTrackInfo> vidList = DBTrackInfo.GetAll();
      GUIPropertyManager.SetProperty("#mvCentral.TotalVideos", vidList.Count.ToString() + " Videos");
      
      SortLabel.Label = "";
      GUIPropertyManager.Changed = true;
      if (persisting)
      {
        loadCurrent();
      }
      else
      {
        loadArtists();
      }
      checkTrack.Interval = 5000;
      checkTrack.Start();
      checkTrack.Tick += new EventHandler(TrackChanged);

      base.OnPageLoad();
    }

    private void loadCurrent()
    {
      persisting = true;
      switch (currentView)
      {
        case View.Artist:
          loadArtists();
          facade.SelectedListItemIndex = lastItemArt;
          break;
        case View.Video:
          LoadVideos(artistID);
          facade.SelectedListItemIndex = lastItemVid;
          break;
      }

    }

    private void loadArtists()
    {
      currentView = View.Artist;

      GUIPropertyManager.SetProperty("#mvCentral.Hierachy", "Artists");
      GUIPropertyManager.Changed = true;
      List<DBArtistInfo> list = DBArtistInfo.GetAll();
      ArrayList state = new ArrayList();
      facade.Clear();
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
        facade.Add(item);
      }

      if (facade.Count > 0 && !persisting)
      {
        onArtistSelected(facade.ListLayout.ListItems[0], facade);
      }
      persisting = true;
      checkTrack.Start();
      dummyLabel.Visibility = System.Windows.Visibility.Hidden;
    }

    private void LoadVideos(int ArtistID)
    {
      currentView = View.Video;
      GUIPropertyManager.SetProperty("#mvCentral.Hierachy", "Artists | " + DBArtistInfo.Get(ArtistID));
      GUIPropertyManager.Changed = true;
      DBArtistInfo currArtist = DBArtistInfo.Get(ArtistID);
      List<DBTrackInfo> list = DBTrackInfo.GetEntriesByArtist(currArtist);
      this.artistID = ArtistID;
      facade.Clear();
      facade.Add(new GUIListItem(".."));
      foreach (DBTrackInfo db1 in list)
      {
        GUIListItem item = new GUIListItem();
        item.Label = db1.Track;
        item.ThumbnailImage = db1.ArtThumbFullPath;
        item.TVTag = db1.bioContent;
        selArtist = currArtist.Artist;
        item.Path = db1.LocalMedia[0].File.FullName;
        item.IsFolder = false;
        item.OnItemSelected += new GUIListItem.ItemSelectedHandler(onVideoSelected);
        item.MusicTag = db1;
        facade.Add(item);
      }
      if (facade.Count > 0 && !persisting)
      {
        onVideoSelected(facade.ListLayout.ListItems[0], facade);
      }
      dummyLabel.Visibility = System.Windows.Visibility.Visible;
    }

    void onArtistSelected(GUIListItem item, GUIControl parent)
    {
      GUIPropertyManager.SetProperty("#mvCentral.ArtistBio", item.TVTag.ToString());
      GUIPropertyManager.SetProperty("#mvCentral.ArtistName", item.Label);
      GUIPropertyManager.SetProperty("#mvCentral.ArtistImg", item.ThumbnailImage);
      GUIPropertyManager.Changed = true;
      lastItemArt = facade.ListLayout.SelectedListItemIndex;
    }

    void onVideoSelected(GUIListItem item, GUIControl parent)
    {
      GUIPropertyManager.SetProperty("#mvCentral.VideoImg", item.ThumbnailImage);
      GUIPropertyManager.Changed = true;
      if (item.Label != "..")
      {
        lastItemVid = facade.ListLayout.SelectedListItemIndex;
      }
    }


    #region dialogboxes
              public void ShowMessage(string heading, string lines) {
            string line1 = null, line2 = null, line3 = null, line4 = null;
            string[] linesArray = lines.Split(new string[] { "\\n" }, StringSplitOptions.None);

            if (linesArray.Length >= 1) line1 = linesArray[0];
            if (linesArray.Length >= 2) line2 = linesArray[1];
            if (linesArray.Length >= 3) line3 = linesArray[2];
            if (linesArray.Length >= 4) line4 = linesArray[3];

            ShowMessage(heading, line1, line2, line3, line4);
        }

        public void ShowMessage(string heading, string line1, string line2, string line3, string line4) {
            GUIDialogOK dialog = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
            dialog.Reset();
            dialog.SetHeading(heading);
            if (line1 != null) dialog.SetLine(1, line1);
            if (line2 != null) dialog.SetLine(2, line2);
            if (line3 != null) dialog.SetLine(3, line3);
            if (line4 != null) dialog.SetLine(4, line4);
            dialog.DoModal(GetID);
        }

        /// <summary>
        /// Displays a yes/no dialog with custom labels for the buttons
        /// This method may become obsolete in the future if media portal adds more dialogs
        /// </summary>
        /// <returns>True if yes was clicked, False if no was clicked</returns>
        public bool ShowCustomYesNo(string heading, string lines, string yesLabel, string noLabel, bool defaultYes) {
            GUIDialogYesNo dialog = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
            try {
                dialog.Reset();
                dialog.SetHeading(heading);
                string[] linesArray = lines.Split(new string[] { "\\n" }, StringSplitOptions.None);
                if (linesArray.Length > 0) dialog.SetLine(1, linesArray[0]);
                if (linesArray.Length > 1) dialog.SetLine(2, linesArray[1]);
                if (linesArray.Length > 2) dialog.SetLine(3, linesArray[2]);
                if (linesArray.Length > 3) dialog.SetLine(4, linesArray[3]);
                dialog.SetDefaultToYes(defaultYes);

                foreach (GUIControl item in dialog.controlList) {
                    if (item is GUIButtonControl) {
                        GUIButtonControl btn = (GUIButtonControl)item;
                        if (btn.GetID == 11 && !String.IsNullOrEmpty(yesLabel)) // Yes button
                            btn.Label = yesLabel;
                        else if (btn.GetID == 10 && !String.IsNullOrEmpty(noLabel)) // No button
                            btn.Label = noLabel;
                    }
                }
                dialog.DoModal(GetID);
                return dialog.IsConfirmed;
            }
            finally {
                // set the standard yes/no dialog back to it's original state (yes/no buttons)
                if (dialog != null) {
                    dialog.ClearAll();
                }
            }
        }

    #endregion
  }
}
