using System.Windows.Forms;
using System.IO;
using System;
using NLog;
using System.Collections.Generic;
using System.Collections;
//using MediaPortal.Playlists;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using mvCentral.Database;
using mvCentral.Playlist;

namespace mvCentral.GUI
{
    public partial class mvGUIMain : GUIWindow
    {

        private enum SmartMode
        {
            Favourites = 0,
            FreshTracks = 1,
            HighestRated = 2,
            Random = 3,
            LeastPlayed = 4,
            Cancel = 5,
        }
        #region playlist

        /// <summary>
        /// Adds a list of Music Videos to a playlist, or a list of artists Music Videos
        /// </summary>
        /// <param name="items"></param>
        /// <param name="playNow"></param>
        /// <param name="clear"></param>
        private void addToPlaylist(List<DBTrackInfo> items, bool playNow, bool clear, bool shuffle)
        {
            PlayList playlist = Player.playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MVCENTRAL);
            if (clear)
            {
                playlist.Clear();
            }
            foreach (DBTrackInfo video in items)
            {
                PlayListItem p1 = new PlayListItem(video);
                p1.Track = video;
                p1.FileName = video.LocalMedia[0].File.FullName;
                playlist.Add(p1);
            }
            Player.playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MVCENTRAL;
            if (shuffle)
            {
                playlist.Shuffle();
            }
            if (playNow)
            {
                Player.playlistPlayer.Play(0);               
                GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
            }
        }

        private void addToPlaylistNext(GUIListItem listItem)
        {
            PlayList playlist = Player.playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MVCENTRAL);
            PlayListItem item = new PlayListItem(listItem.TVTag as DBTrackInfo);


            playlist.Insert(item, Player.playlistPlayer.CurrentItem);
        }

        private void playRandomAll()
        {
            PlayList playlist = Player.playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MVCENTRAL);
            playlist.Clear();
            List<DBTrackInfo> videos = DBTrackInfo.GetAll();
            foreach (DBTrackInfo video in videos)
            {
                playlist.Add(new PlayListItem(video));
            }
            Player.playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MVCENTRAL;
            playlist.Shuffle();
            Player.playlistPlayer.Play(0);
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
        }

        private SmartMode ChooseSmartPlay()
        {
            GUIDialogMenu dlgMenu = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            if (dlgMenu != null)
            {
                dlgMenu.Reset();
//                dlgMenu.SetHeading(dm.getPluginName() + " - Smart Playlist Options");
                if (this.facade.Count > 0)
                {
                    dlgMenu.Add("Favourite Videos");
                    dlgMenu.Add("Newest Videos");
                    dlgMenu.Add("Highest Rated");
                    dlgMenu.Add("Random");
                    dlgMenu.Add("Least Played");
                }
                dlgMenu.DoModal(GetID);

                if (dlgMenu.SelectedLabel == -1) // Nothing was selected
                    return SmartMode.Cancel;

                return (SmartMode)Enum.Parse(typeof(SmartMode), dlgMenu.SelectedLabel.ToString());
            }        
            return SmartMode.Cancel; 
        }

        private void playSmart(SmartMode mode)
        {
            switch (mode)
            {
                case SmartMode.Favourites:
                    playFavourites();
                    break;
                case SmartMode.FreshTracks:
                    playFreshTracks();
                    break;
                case SmartMode.HighestRated:
                    playHighestRated();
                    break;
                case SmartMode.LeastPlayed:
                    playLeastPlayed();
                    break;
                case SmartMode.Random:
                    playRandomAll();
                    break;
                case SmartMode.Cancel:
                    break;
            }
        }

        private void playFavourites() 
        {
//            string avgPlayCount = dm.Execute("SELECT AVG(playCount) FROM Videos", true).Rows[0].fields[0];
//            int i = int.Parse(avgPlayCount.Split('.')[0]);
//            ArrayList leastPlayed = dm.getAllVideos(i, true);
//            addToPlaylist(leastPlayed, true, true, true);
        }

        private void playFreshTracks() 
        {
//            ArrayList newVideos = dm.getNewestVideos();
//            addToPlaylist(newVideos, true, true, false);
        }

        private void playHighestRated() 
        { DebugMsg("NOT IMPLEMENTED"); }

        private void playLeastPlayed() 
        {
//            string avgPlayCount = dm.Execute("SELECT AVG(playCount) FROM Videos", true).Rows[0].fields[0];           
//            int i = int.Parse(avgPlayCount.Split('.')[0]);
//            ArrayList leastPlayed = dm.getAllVideos(i, false);
//            addToPlaylist(leastPlayed, true, true, true);
        }
        #endregion
    }
}
