using System;
using System.Windows.Forms;
using System.IO;

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
  
namespace mvCentral.GUI
{
    public partial class GUIMain : GUIWindow
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private mvCentralCore core = mvCentralCore.Instance;
//        DatabaseManager dm = mvCentralCore.DatabaseManager;

        private Timer checkTrack = new Timer();
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
//                    dm.play(currentTrack);
                    lastTrack = currentTrack;
                }
            }
        }

        public override bool Init()
        {
            base.Init();   
            core.Initialize(null);
            logger.Info("Initializing GUI");
            return Load(GUIGraphicsContext.Skin + @"\mvCentral.xml");
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
                 if (listPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO).Count > 0 && !(facade.ListView.SelectedListItem.IsFolder))
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
                if (facade.ListView.SelectedListItem.IsFolder)
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
                        if (facade.ListView.SelectedListItem.IsFolder)
                        {
                            DBArtistInfo currArtist = DBArtistInfo.Get(artistID);
                            List<DBTrackInfo> list = DBTrackInfo.GetEntriesByArtist(currArtist);
 
                            addToPlaylist(list, false, false, false);
                        }
                        else
                        {
                            PlayList playlist = listPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO);
                            string filename = facade.ListView.SelectedListItem.Label;
                            string path = facade.ListView.SelectedListItem.Path;
                            playlist.Add(new PlayListItem(filename, path));
                        }
                        break;
                    case 1:
                        if (facade.ListView.SelectedListItem.IsFolder)
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
                        addToPlaylistNext(facade.ListView.SelectedListItem);
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
                onArtistSelected(facade.ListView.ListItems[0], facade);
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
                facade.Add(item);
            }
            if (facade.Count > 0 && ! persisting)
            {
                onVideoSelected(facade.ListView.ListItems[0], facade);
            }
            dummyLabel.Visibility = System.Windows.Visibility.Visible;
        }

        void onArtistSelected(GUIListItem item, GUIControl parent)
        {
            GUIPropertyManager.SetProperty("#mvCentral.ArtistBio", item.TVTag.ToString());
            GUIPropertyManager.SetProperty("#mvCentral.ArtistName", item.Label);
            GUIPropertyManager.SetProperty("#mvCentral.ArtistImg", item.ThumbnailImage);
            GUIPropertyManager.Changed = true;
            lastItemArt = facade.ListView.SelectedListItemIndex;
        }

        void onVideoSelected(GUIListItem item, GUIControl parent)
        {
            GUIPropertyManager.SetProperty("#mvCentral.VideoImg", item.ThumbnailImage);
            GUIPropertyManager.Changed = true;
            if (item.Label != "..")
            {
                lastItemVid = facade.ListView.SelectedListItemIndex;
            }
        }
    }
}
