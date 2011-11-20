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

using WindowPlugins;

namespace mvCentral.GUI
{
  public partial class mvGUIMain : WindowPluginBase
  {
    private void ArtistActions(Action.ActionType actionType)
    {
      if ((actionType == Action.ActionType.ACTION_MUSIC_PLAY) || (actionType == Action.ActionType.ACTION_PLAY) || (actionType == Action.ActionType.ACTION_PAUSE))
      {
        if ((actionType == Action.ActionType.ACTION_MUSIC_PLAY) || (actionType == Action.ActionType.ACTION_PLAY) || (actionType == Action.ActionType.ACTION_PAUSE && !g_Player.HasVideo))
        {
          DBArtistInfo currArtist = DBArtistInfo.Get(facadeLayout.SelectedListItem.Label);
          List<DBTrackInfo> allTracksByArtist = DBTrackInfo.GetEntriesByArtist(currArtist);
          addToPlaylist(allTracksByArtist, true, mvCentralCore.Settings.ClearPlaylistOnAdd, mvCentralCore.Settings.GeneratedPlaylistAutoShuffle);
        }
      }
      else if (actionType == Action.ActionType.REMOTE_0 ||
              actionType == Action.ActionType.REMOTE_1 ||
              actionType == Action.ActionType.REMOTE_2 ||
              actionType == Action.ActionType.REMOTE_3 ||
              actionType == Action.ActionType.REMOTE_4 ||
              actionType == Action.ActionType.REMOTE_5 ||
              actionType == Action.ActionType.REMOTE_6 ||
              actionType == Action.ActionType.REMOTE_7 ||
              actionType == Action.ActionType.REMOTE_8 ||
              actionType == Action.ActionType.REMOTE_9)
        DoSpell(actionType);
      else
      {
        currentView = mvView.Artist;
        artistID = facadeLayout.SelectedListItem.ItemId;
        logger.Debug("Calling loadCurrent from ArtistActions");
        loadCurrent();
      }
    }
  }
}