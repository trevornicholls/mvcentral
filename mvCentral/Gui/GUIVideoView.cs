using Common.GUIPlugins;

using MediaPortal.GUI.Library;
using MediaPortal.Player;
using mvCentral.Database;

namespace mvCentral.GUI
{
  public partial class MvGuiMain : WindowPluginBase
  {

    private void VideoActions(MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      if ((actionType == MediaPortal.GUI.Library.Action.ActionType.ACTION_PLAY) || (actionType == MediaPortal.GUI.Library.Action.ActionType.ACTION_MUSIC_PLAY) || actionType == MediaPortal.GUI.Library.Action.ActionType.ACTION_SELECT_ITEM)
      {
        //play this song, or return to previous level
        if (facadeLayout.ListLayout.SelectedListItem.Label == "..")
        {
          _currentView = MvView.Artist;
          addToStack(_currentView, false);
          logger.Debug("Calling loadCurrent from VideoActions");
          loadCurrent();
        }
        else
        {
          //Play currently selected and activate video window
          string vidPath = facadeLayout.ListLayout.SelectedListItem.Path;
          DBTrackInfo db1 = (DBTrackInfo)facadeLayout.ListLayout.SelectedListItem.MusicTag;

          g_Player.Play(db1.LocalMedia[0].File.FullName);
          if (db1.LocalMedia[0].IsDVD)
          {
            //PlayDVD(db1);
          }
        }
        if (mvCentralCore.Settings.AutoFullscreen)
          GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
      }
    }
  }
}
