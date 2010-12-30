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
using DShowNET.Helper;
using DirectShowLib;
using DirectShowLib.Dvd;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;



namespace mvCentral.GUI
{
    public partial class GUIMain : GUIWindow
    {

        List<DSGrapheditROTEntry> l1;
        protected IGraphBuilder _graphBuilder = null;
        protected IBasicVideo2 _basicVideo = null;
        /// <summary> dvd control interface. </summary>
        protected IDvdControl2 _dvdCtrl = null;
        /// <summary> asynchronous command interface. </summary>
        protected IDvdCmd _cmdOption = null;
        protected IBaseFilter _dvdbasefilter = null;

        private void PurgeEntries()
        {
            if (l1 != null)
            foreach (DSGrapheditROTEntry rote in l1)
            {
                rote.Dispose();
            }
        }

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
                    DBTrackInfo db1 = (DBTrackInfo)facade.ListView.SelectedListItem.MusicTag;

                    g_Player.Play(db1.LocalMedia[0].File.FullName);
                    if (db1.LocalMedia[0].IsDVD)
                    {
                        string dvdNavigator;
                        using (MediaPortal.Profile.Settings xmlreader = new MPSettings())
                        {
                           dvdNavigator = xmlreader.GetValueAsString("dvdplayer", "navigator", "DVD Navigator");
                        }
                        PurgeEntries();
                        l1 = ROTClass.GetFilterGraphsFromROT();
                        foreach (DSGrapheditROTEntry e1 in l1)
                        {
                            logger.Info(e1.ToString());
                            _graphBuilder = e1.ConnectToROTEntry() as IGraphBuilder;
                            _dvdbasefilter = DirectShowUtil.GetFilterByName(_graphBuilder,dvdNavigator);
                            _dvdCtrl = _dvdbasefilter as IDvdControl2;
                            _basicVideo = _graphBuilder as IBasicVideo2;

                            int hr = _dvdCtrl.PlayChaptersAutoStop(1, db1.ChapterID, 1, 0, out _cmdOption);
                            DsError.ThrowExceptionForHR(hr);


                        }
                    }

                    GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);

                }
            }
        }
    }
}