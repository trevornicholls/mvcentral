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
using MediaPortal.Playlists;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using mvCentral.Database;
using mvCentral;
using mvCentral.LocalMediaManagement;
  
namespace mvCentral.GUI
{
    public partial class GUIMain : GUIWindow
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

        [SkinControlAttribute(2)] protected GUIButtonControl buttonOne = null;
        [SkinControlAttribute(3)] protected GUIButtonControl buttonTwo = null;
        [SkinControlAttribute(4)] protected GUIButtonControl buttonThree = null;
        [SkinControlAttribute(10)] protected GUILabelControl hierachy = null;
        [SkinControlAttribute(11)] protected GUILabelControl artistName = null;
        [SkinControlAttribute(12)] protected GUITextScrollUpControl artistBio = null;
        [SkinControlAttribute(13)] protected GUIImage artistImage = null;
        [SkinControlAttribute(14)] protected GUIImage videoImage = null;
        [SkinControlAttribute(15)] protected GUILabelControl videoCount = null;
        [SkinControlAttribute(16)] protected GUILabelControl artistCount = null;
        [SkinControlAttribute(17)] protected GUIFadeLabel favVidLabel = null;
        [SkinControlAttribute(18)] protected GUIImage favVidImage = null;
        [SkinControlAttribute(19)] protected GUILabelControl favArtLabel = null;
        [SkinControlAttribute(20)] protected GUIImage favArtImage = null;
        [SkinControlAttribute(22)] protected GUILabelControl SortLabel = null;
        [SkinControlAttribute(99)] protected GUILabelControl dummyLabel = null;
        [SkinControlAttribute(50)] protected GUIFacadeControl facade;

        private PlayListPlayer listPlayer = PlayListPlayer.SingletonPlayer;
        private bool persisting = false;
        private View currentView = View.None;
        int lastItemArt = 0, lastItemVid = 0, artistID = 0;
        private string selArtist = "";

        public GUIMain() 
        {
            timeOut.Tick += new EventHandler(checkTime);
        }

        void TrackChanged(object sender, EventArgs e)
        {
            string currentTrack = "";
            try
            {
                currentTrack = listPlayer.GetCurrentItem().FileName;
            }
            catch { }
            if (lastTrack != currentTrack)
            {
                if (!(currentTrack == "" ))
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

            // start initialization of the moving pictures core services in a seperate thread
            RichTextBox rtb = null;
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
*/        }

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
*/                initComplete = true;

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
                 if (listPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO).Count > 0 && !(facade.ListLayout.SelectedListItem.IsFolder))
                 {
                     dlgMenu.Add("Add to Playlist as next item");
                 }
                 dlgMenu.Add("Cancel");
             }
             dlgMenu.DoModal(880);

             if (dlgMenu.SelectedLabel == -1) // Nothing was selected
                 return -1;

             return dlgMenu.SelectedLabel;
          }
          return -1;
       }

        protected override void OnClicked(int controlId, GUIControl control,
                MediaPortal.GUI.Library.Action.ActionType actionType)
        {
            if (controlId == 2)
                playRandomAll();
            if (control == buttonTwo)
                playSmart(ChooseSmartPlay());
            if (controlId == 4)
                GUIWindowManager.ActivateWindow(28);
            if (controlId == 50)
            {
                //Clicked on something in the facade
                if (facade.ListLayout.SelectedListItem.IsFolder)
                {
                    ArtistActions(actionType);
                }
                else 
                {
                    VideoActions(actionType);
                }
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
                            PlayList playlist = listPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO);
                            string filename = facade.ListLayout.SelectedListItem.Label;
                            string path = facade.ListLayout.SelectedListItem.Path;
                            playlist.Add(new PlayListItem(filename, path));
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
            foreach(DBArtistInfo db1 in list)
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
            foreach(DBTrackInfo db1 in list)
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
            if (facade.Count > 0 && ! persisting)
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
    }
}
