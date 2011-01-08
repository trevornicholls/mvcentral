using System;
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

        public void PlayDVD(DBTrackInfo db1)
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

//                            hr = _mediaCtrl.Run();
//                            hr = _mediaCtrl.Pause();
//                            _offsetseek = (ulong)seekbar.Value;
                            TimeSpan t1 = TimeSpan.FromMilliseconds(0);
                            TimeSpan t2 = TimeSpan.Parse(db1.OffsetTime);
                            t1 = t1.Add(t2);
//                            t1 = TimeSpan.Parse(db1.PlayTime);
                            t2 = t2.Add(TimeSpan.Parse(db1.PlayTime));
                            DvdHMSFTimeCode t3 = mvCentralUtils.ConvertToDvdHMSFTimeCode(t1);
                            DvdHMSFTimeCode t4 = mvCentralUtils.ConvertToDvdHMSFTimeCode(t2);
                            //                if (state == FilterState.Stopped) 
                            int hr = _dvdCtrl.PlayPeriodInTitleAutoStop(1, t3, t4, DvdCmdFlags.Flush | DvdCmdFlags.Block, out _cmdOption);
                            DsError.ThrowExceptionForHR(hr);



//                            int hr = _dvdCtrl.PlayChaptersAutoStop(1, db1.ChapterID, 1, 0, out _cmdOption);
//                            DsError.ThrowExceptionForHR(hr);
                        }
        }

        private void VideoActions(MediaPortal.GUI.Library.Action.ActionType actionType)
        {
            if (actionType == MediaPortal.GUI.Library.Action.ActionType.ACTION_PLAY || actionType == MediaPortal.GUI.Library.Action.ActionType.ACTION_SELECT_ITEM)
            {
                //play this song, or return to previous level
                if (facade.ListLayout.SelectedListItem.Label == "..")
                {
                    currentView = View.Artist;
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
