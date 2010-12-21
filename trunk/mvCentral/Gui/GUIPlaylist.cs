using System.Windows.Forms;
using System.IO;
using System;
using NLog;
using System.Collections.Generic;
using System.Collections;
using MediaPortal.Playlists;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Player;


namespace mvCentral.GUI
{
    public partial class GUIMain : GUIWindow
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
        private void addToPlaylist(ArrayList items, bool playNow, bool clear, bool shuffle)
        {
            PlayList playlist = listPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO);
            if (clear)
            {
                playlist.Clear();
            }
//            foreach (mvDetails video in items)
            {
//                string artistName = dm.getArtist(int.Parse(video.ArtistID)).ToString();
//                playlist.Add(new PlayListItem(artistName + " - " + video.ToString(), video.File));
            }
            listPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_VIDEO;
            if (shuffle)
            {
                playlist.Shuffle();
            }
            if (playNow)
            {
                listPlayer.Play(0);               
                GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
            }
        }

        private void addToPlaylistNext(GUIListItem listItem)
        {
            PlayList playlist = this.listPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO);
            PlayListItem item = new PlayListItem(listItem.Label, listItem.Path);
            playlist.Insert(item, this.listPlayer.CurrentSong);
        }

        private void playRandomAll()
        {
            PlayList playlist = listPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO);
            playlist.Clear();
//            ArrayList videos = dm.getAllVideos();
//            foreach (mvDetails video in videos)
//            {
//                playlist.Add(new PlayListItem(video.Name, video.File));
//            }
            this.listPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_VIDEO;
            playlist.Shuffle();
            this.listPlayer.Play(0);
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
                dlgMenu.DoModal(880);

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
