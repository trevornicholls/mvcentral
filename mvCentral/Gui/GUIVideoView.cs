﻿using System;
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
using MediaPortal.Profile;
using mvCentral.ROT;
using mvCentral.Database;
using mvCentral.Utils;
using DShowNET.Helper;
using DirectShowLib;
using DirectShowLib.Dvd;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using WindowPlugins;




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
