using System.Windows.Forms;
using System.IO;
using Common.GUIPlugins;
using NLog;
using System.Collections.Generic;
using System.Collections;
using MediaPortal.Playlists;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using WindowPlugins;

using mvCentral.Database;




namespace mvCentral.GUI
{
  public partial class MvGuiMain : WindowPluginBase
  {
    private void GenreActions(MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      if ((actionType == Action.ActionType.ACTION_MUSIC_PLAY) || (actionType == Action.ActionType.ACTION_PLAY) || (actionType == Action.ActionType.ACTION_PAUSE))
      {
        if ((actionType == Action.ActionType.ACTION_MUSIC_PLAY) || (actionType == Action.ActionType.ACTION_PLAY) || (actionType == Action.ActionType.ACTION_PAUSE && !g_Player.HasVideo))
        {

          List<DBArtistInfo> artistList = new List<DBArtistInfo>();
          List<DBArtistInfo> artistFullList = DBArtistInfo.GetAll();


          logger.Debug("Checking for matches for Genre : " + facadeLayout.SelectedListItem.Label);
          foreach (DBArtistInfo artistInfo in artistFullList)
          {
            if (tagMatched(facadeLayout.SelectedListItem.Label, artistInfo))
            {
              logger.Debug("Matched Artist {0} with Tag {1}", artistInfo.Artist, facadeLayout.SelectedListItem.Label);
              if (!artistList.Contains(artistInfo))
                artistList.Add(artistInfo);
            }
          }
          
          if (mvCentralCore.Settings.ClearPlaylistOnAdd)
            ClearPlaylist();

          foreach (DBArtistInfo currArtist in artistList)
          {
            List<DBTrackInfo> artistTracks = DBTrackInfo.GetEntriesByArtist(currArtist);
            AddToPlaylist(artistTracks, false, false, mvCentralCore.Settings.GeneratedPlaylistAutoShuffle);
          }
          Player.playlistPlayer.Play(0);
          if (mvCentralCore.Settings.AutoFullscreen)
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
        }
        else
        {
          _currentView = MvView.Genres;
          artistID = facadeLayout.SelectedListItem.ItemId;
          logger.Debug("Calling loadCurrent from GenreActions");
          loadCurrent();
        }
      }
    }
  }
}
