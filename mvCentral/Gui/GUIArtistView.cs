using Common.GUIPlugins;

using MediaPortal.GUI.Library;
using MediaPortal.Player;

using mvCentral.Database;

using System.Collections.Generic;

namespace mvCentral.GUI
{
  public partial class MvGuiMain : WindowPluginBase
  {
    private void ArtistActions(Action.ActionType actionType)
    {
      if ((actionType == Action.ActionType.ACTION_MUSIC_PLAY) || (actionType == Action.ActionType.ACTION_PLAY) || (actionType == Action.ActionType.ACTION_PAUSE))
      {
        if ((actionType == Action.ActionType.ACTION_MUSIC_PLAY) || (actionType == Action.ActionType.ACTION_PLAY) || (actionType == Action.ActionType.ACTION_PAUSE && !g_Player.HasVideo))
        {
          DBArtistInfo currArtist = DBArtistInfo.Get(facadeLayout.SelectedListItem.Label);
          List<DBTrackInfo> allTracksByArtist = DBTrackInfo.GetEntriesByArtist(currArtist);
          AddToPlaylist(allTracksByArtist, true, mvCentralCore.Settings.ClearPlaylistOnAdd, mvCentralCore.Settings.GeneratedPlaylistAutoShuffle);
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
        _currentView = MvView.Artist;
        artistID = facadeLayout.SelectedListItem.ItemId;
        logger.Debug("Calling loadCurrent from ArtistActions");
        loadCurrent();
      }
    }
  }
}