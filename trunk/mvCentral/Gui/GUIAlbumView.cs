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

using mvCentral.Database;
using WindowPlugins;

namespace mvCentral.GUI
{
  public partial class MvGuiMain : WindowPluginBase
  {
    private void AlbumActions(MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      if ((actionType == Action.ActionType.ACTION_MUSIC_PLAY) || (actionType == Action.ActionType.ACTION_PLAY) || (actionType == Action.ActionType.ACTION_PAUSE))
      {
        if ((actionType == Action.ActionType.ACTION_MUSIC_PLAY) || (actionType == Action.ActionType.ACTION_PLAY) || (actionType == Action.ActionType.ACTION_PAUSE && !g_Player.HasVideo))
        {
          DBArtistInfo currArtist = DBArtistInfo.Get(facadeLayout.SelectedListItem.Label);
          List<DBTrackInfo> allTracksOnAlbum = DBTrackInfo.GetEntriesByAlbum((DBAlbumInfo)facadeLayout.SelectedListItem.MusicTag);
          AddToPlaylist(allTracksOnAlbum, true, mvCentralCore.Settings.ClearPlaylistOnAdd, mvCentralCore.Settings.GeneratedPlaylistAutoShuffle);
        }
        else
        {
          _currentView = MvView.Artist;
          artistID = facadeLayout.SelectedListItem.ItemId;
          logger.Debug("Calling loadCurrent from AlbumActions");
          loadCurrent();
        }
      }
    }
  }
}
