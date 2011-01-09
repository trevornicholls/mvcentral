using System.Windows.Forms;
using System.IO;
using NLog;
using System.Collections.Generic;
using System.Collections;
using MediaPortal.Playlists;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Player;

namespace mvCentral.GUI
{
    public partial class mvGUIMain : GUIWindow
    {
        private void ArtistActions(Action.ActionType actionType)
        {
            if (actionType == Action.ActionType.ACTION_MUSIC_PLAY)
            {
//                addToPlaylist(dm.getAllVideos(facade.ListView.SelectedListItem.ItemId), true, true, false);
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
                currentView = View.Video;
                artistID = facade.ListLayout.SelectedListItem.ItemId;
                loadCurrent();
            }
        }
    }
}