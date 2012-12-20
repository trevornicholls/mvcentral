using System.Windows.Forms;
using System.IO;
using NLog;
using System.Collections.Generic;
using System.Collections;
using MediaPortal.Playlists;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Player;

using mvCentral.Database;




namespace mvCentral.GUI
{
  public partial class mvGUIMain : WindowPluginBaseMVC
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
            clearPlaylist();

          foreach (DBArtistInfo currArtist in artistList)
          {
            List<DBTrackInfo> artistTracks = DBTrackInfo.GetEntriesByArtist(currArtist);
            addToPlaylist(artistTracks, false, false, mvCentralCore.Settings.GeneratedPlaylistAutoShuffle);
          }
          Player.playlistPlayer.Play(0);
          if (mvCentralCore.Settings.AutoFullscreen)
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
        }
        else
        {
          currentView = mvView.Genres;
          artistID = facadeLayout.SelectedListItem.ItemId;
          logger.Debug("Calling loadCurrent from GenreActions");
          loadCurrent();
        }
      }
    }
  }
}
