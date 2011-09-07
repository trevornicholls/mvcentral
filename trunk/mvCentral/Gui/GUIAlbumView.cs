﻿using System;
using System.Windows.Forms;
using System.IO;
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



namespace mvCentral.GUI
{
    public partial class mvGUIMain : GUIWindow
    {


 
        private void AlbumActions(MediaPortal.GUI.Library.Action.ActionType actionType)
        {
            if (actionType == MediaPortal.GUI.Library.Action.ActionType.ACTION_PLAY || actionType == MediaPortal.GUI.Library.Action.ActionType.ACTION_SELECT_ITEM)
            {
                //play this song, or return to previous level
                if (facade.ListLayout.SelectedListItem.Label == "..")
                {
                    currentView = View.Album;
                    loadCurrent();
                }
                else
                {
                    //Play currently selected and activate video window
                    string vidPath = facade.ListLayout.SelectedListItem.Path;
                    DBTrackInfo db1 = (DBTrackInfo)facade.ListLayout.SelectedListItem.MusicTag;
                    
                    g_Player.Play(db1.LocalMedia[0].File.FullName);
                    if (db1.LocalMedia[0].IsDVD)
                    {
//                        PlayDVD(db1);
                    }
                 }

                 GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);

                }
            }
        }
}