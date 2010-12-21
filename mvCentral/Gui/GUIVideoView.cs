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
    public partial class GUIMain : GUIWindow
    {
        private void VideoActions(Action.ActionType actionType)
        {
            if (actionType == Action.ActionType.ACTION_PLAY || actionType == Action.ActionType.ACTION_SELECT_ITEM)
            {
                //play this song, or return to previous level
                if (facade.ListView.SelectedListItem.Label == "..")
                {
                    currentView = View.Artist;
                    loadCurrent();
                }
                else
                {
                    //Play currently selected and activate video window
                    string vidPath = facade.ListView.SelectedListItem.Path;
                    g_Player.Play(vidPath, g_Player.MediaType.Video);
                    GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
                }
            }
        }
    }
}