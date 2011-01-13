#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using NLog;

using DShowNET.Helper;
using DirectShowLib;
using DirectShowLib.Dvd;

using MediaPortal.GUI.Library;
using MediaPortal.GUI.Video;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Profile;
using MediaPortal.Configuration;
using mvCentral.Database;
using mvCentral.ROT;
using mvCentral.Utils;
using mvCentral.Localizations;
using mvCentral.LocalMediaManagement;

namespace mvCentral.Playlist
{
    public class PlayListPlayer
    {
        #region g_Player

        public interface IPlayer
        {
//            PlayList currentPlayList;
//            int lastvid;
            bool msgblocked { get; set; }
            bool Playing { get; }
            void Release();
            bool Play(string strFile);
            bool Play(PlayList p1, int index);
            void Stop();
            void SeekAsolutePercentage(int iPercentage);
            double Duration { get; }
            double CurrentPosition { get; }
            void SeekAbsolute(double dTime);
            bool HasVideo { get; }
            bool ShowFullScreenWindow();
        }

        private class FakePlayer : IPlayer
        {
            List<DSGrapheditROTEntry> l1;
            protected IGraphBuilder _graphBuilder = null;
            protected IBasicVideo2 _basicVideo = null;
            /// <summary> dvd control interface. </summary>
            protected IDvdControl2 _dvdCtrl = null;
            /// <summary> asynchronous command interface. </summary>
            protected IDvdCmd _cmdOption = null;
            protected IBaseFilter _dvdbasefilter = null;
            /// <summary> control interface. </summary>
            protected IMediaControl _mediaCtrl = null;

            public int lastvid;
            private DBTrackInfo CurrentTrack;
            public PlayList currentPlayList;
            private System.Windows.Forms.Timer playTimer = new System.Windows.Forms.Timer();
            private TimeSpan CurrentTime;
            public bool msgblocked1;
//            bool msgblocked1 {
//                get { return IPlayer.msgblocked; }
//                set { IPlayer.msgblocked = value; }
                
//                }
            bool IPlayer.msgblocked { get { return msgblocked1; } set { msgblocked1 = value; } }

            public bool Playing
            {
                get { return MediaPortal.Player.g_Player.Playing; }
            }

            public void Release()
            {
                MediaPortal.Player.g_Player.Release();
            }

            bool IPlayer.Play(string strFile)
            {
                return MediaPortal.Player.g_Player.Play(strFile);
            }
 
            public bool Play(PlayList p1 , int index)
            {
               bool result = false;
               string fn = p1[index].Track.LocalMedia[0].File.FullName;
               CurrentTrack = p1[index].Track;
               lastvid = index;
               // needed so everything goes in sequence otherwise stop gets handled after the play events
               GUIWindowManager.Process();
               if (CurrentTrack.LocalMedia[0].IsDVD)
               {
                   msgblocked1 = true;
                   PlayDVD(CurrentTrack);
                   result = true;
               }
               else
               {
                   result = MediaPortal.Player.g_Player.Play(fn);
               }
               SetInitTimerValues();
               playTimer.Start();
               return result;
            }


            public void Stop()
            {
                MediaPortal.Player.g_Player.Stop();
            }

            public void SeekAsolutePercentage(int iPercentage)
            {
                MediaPortal.Player.g_Player.SeekAsolutePercentage(iPercentage);
            }

            public double Duration
            {
                get { return MediaPortal.Player.g_Player.Duration; }
            }

            public double CurrentPosition
            {
                get { return MediaPortal.Player.g_Player.CurrentPosition; }
            }

            public void SeekAbsolute(double dTime)
            {
                MediaPortal.Player.g_Player.SeekAbsolute(dTime);
            }

            public bool HasVideo
            {
                get { return MediaPortal.Player.g_Player.HasVideo; }
            }

            public bool ShowFullScreenWindow()
            {
                return MediaPortal.Player.g_Player.ShowFullScreenWindow();
            }


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
                DBLocalMedia mediaToPlay = db1.LocalMedia[0];

                if (mediaToPlay.State != MediaState.Online) mediaToPlay.Mount();
                while (mediaToPlay.State != MediaState.Online) { Thread.Sleep(1000); };
                string dvdNavigator = "";
                string dslibdvdnavMonikerString = @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{1FFD2F97-0C57-4E21-9FC1-60DF6C6D20BF}";
                Log.Info("finding dslibdvdnav filter");
                IBaseFilter filter = Marshal.BindToMoniker(dslibdvdnavMonikerString) as IBaseFilter;
                if (filter != null)
                {
                    Log.Info("dslibdvdnav filter found!");
                    DirectShowUtil.ReleaseComObject(filter);
                    filter = null;
                    using (MediaPortal.Profile.Settings xmlreader = mvCentralCore.MediaPortalSettings)
                    {
                        xmlreader.SetValue("dvdplayer", "navigator", "dslibdvdnav");
                        dvdNavigator = xmlreader.GetValueAsString("dvdplayer", "navigator", "DVD Navigator");
                    }
                }
                else 
                    Log.Info("dslibdvdnav filter not found using mp default one!");


                MediaPortal.Player.g_Player.Play(mediaToPlay.GetVideoPath());

                // reset setting back to original
                if (dvdNavigator == "dslibdvdnav") 
                using (MediaPortal.Profile.Settings xmlreader = mvCentralCore.MediaPortalSettings)
                {
                    xmlreader.SetValue("dvdplayer", "navigator", "DVD Navigator");
                }
 
                PurgeEntries();
                l1 = ROTClass.GetFilterGraphsFromROT();
                foreach (DSGrapheditROTEntry e1 in l1)
                {
                    logger.Info(e1.ToString());
                    _graphBuilder = e1.ConnectToROTEntry() as IGraphBuilder;

                    _dvdbasefilter = DirectShowUtil.GetFilterByName(_graphBuilder, dvdNavigator);

                    _dvdCtrl = _dvdbasefilter as IDvdControl2;
                    _dvdCtrl.SetOption(DvdOptionFlag.HMSFTimeCodeEvents, true); // use new HMSF timecode format
                    _dvdCtrl.SetOption(DvdOptionFlag.ResetOnStop, false); 
                    _dvdCtrl.SetDVDDirectory(mediaToPlay.GetVideoPath());
                    DirectShowUtil.RenderOutputPins(_graphBuilder, _dvdbasefilter);
                    _basicVideo = _graphBuilder as IBasicVideo2;
                    _mediaCtrl = _graphBuilder as IMediaControl;
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
                    int hr = _mediaCtrl.Run();
                    hr = _dvdCtrl.PlayAtTimeInTitle(db1.TitleID, t3, DvdCmdFlags.Flush | DvdCmdFlags.Block, out _cmdOption);
//                    hr = _dvdCtrl.PlayPeriodInTitleAutoStop(6, t3, t4, DvdCmdFlags.Flush | DvdCmdFlags.Block, out _cmdOption);
                    DsError.ThrowExceptionForHR(hr);



                    //                            int hr = _dvdCtrl.PlayChaptersAutoStop(1, db1.ChapterID, 1, 0, out _cmdOption);
                    //                            DsError.ThrowExceptionForHR(hr);
                }
            }

            private void SetInitTimerValues()
            {
                CurrentTime = TimeSpan.FromSeconds(0);
                playTimer.Interval = 1000;
                playTimer.Tick += new EventHandler(playEvent);
            }

            private void playEvent(object sender, EventArgs e)
            {
                CurrentTime = CurrentTime.Add(TimeSpan.FromSeconds(1));
                TimeSpan t1 = TimeSpan.FromSeconds(MediaPortal.Player.g_Player.CurrentPosition);
                TimeSpan t2 = TimeSpan.Parse(CurrentTrack.PlayTime);
                if (CurrentTrack.OffsetTime.Trim().Length > 0) t2 = t2.Add(TimeSpan.Parse(CurrentTrack.OffsetTime));
                if (playTimer.Enabled)
                if (t1 > t2)
                //            if (CurrentTime > TimeSpan.Parse(CurrentTrack.PlayTime))
                {
                    playTimer.Stop();
//                    Stop();
                    GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYBACK_ENDED, 0, 0, 0, -1, 0, null);
                    GUIGraphicsContext.SendMessage(msg);
                }

                if (!MediaPortal.Player.g_Player.Playing)
                {
//                    playNext();
                }

            }


            private void playNext()
            {
                if (!MediaPortal.Player.g_Player.Playing)
                {
                    playTimer.Stop();
                    int count =  currentPlayList.Count;
                    lastvid = lastvid++;
                    if (lastvid <= count)
                    {
                        PlayListItem p1 = currentPlayList[lastvid - 1];
                        if (p1.Track != null)
                        {
                            DBTrackInfo mv = (DBTrackInfo)p1.Track;
                            if (mv != null)
                                Play(currentPlayList, lastvid);
                        }
                    }
                }
            }

        }

        public IPlayer g_Player = new FakePlayer();
        
        #endregion


        private static Logger logger = LogManager.GetCurrentClassLogger();
        int _entriesNotFound = 0;
        int _currentItem = -1;
        PlayListType _currentPlayList = PlayListType.PLAYLIST_NONE;
        PlayList _tvseriesPlayList = new PlayList();        
        PlayList _emptyPlayList = new PlayList();    
        bool _repeatPlayList = mvCentralCore.Settings.repeatPlayList;
        bool _playlistAutoPlay = mvCentralCore.Settings.playlistAutoPlay;
        bool _playlistAutoShuffle = mvCentralCore.Settings.playlistAutoShuffle;
        string _currentPlaylistName = string.Empty;		
		private bool listenToExternalPlayerEvents = false;

        public PlayListPlayer()
        {
            Init();
        }

        private static PlayListPlayer singletonPlayer = new PlayListPlayer();

        public static PlayListPlayer SingletonPlayer
        {
            get
            {
                return singletonPlayer;
            }
        }

        public void Init()
        {
            GUIWindowManager.Receivers += new SendMessageHandler(this.OnMessage);

			// external player handlers
			MediaPortal.Util.Utils.OnStartExternal += new MediaPortal.Util.Utils.UtilEventHandler(onStartExternal);
			MediaPortal.Util.Utils.OnStopExternal += new MediaPortal.Util.Utils.UtilEventHandler(onStopExternal);
        }

        public void OnMessage(GUIMessage message)
        {
            switch (message.Message)
            {
                case GUIMessage.MessageType.GUI_MSG_PLAYBACK_STOPPED:
                {
                    PlayListItem item = GetCurrentItem();
                    if (item != null)
                    {                
                        Reset();
                         _currentPlayList = PlayListType.PLAYLIST_NONE;
 //                        SetProperties(item, true);
                    }
                    GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS, 0, 0, 0, -1, 0, null);
                    GUIGraphicsContext.SendMessage(msg);                    
                }
                break;

                case GUIMessage.MessageType.GUI_MSG_PLAYBACK_ENDED:
                {
                    SetAsWatched();
//                    if (!g_Player.msgblocked) 
                        PlayNext();
                    g_Player.msgblocked = false;
                    if (!g_Player.Playing)
                    {
                        g_Player.Release();

                        // Clear focus when playback ended
                        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS, 0, 0, 0, -1, 0, null);
                        GUIGraphicsContext.SendMessage(msg);
                    }
                }
                break;

                case GUIMessage.MessageType.GUI_MSG_PLAY_FILE:
                {
                    logger.Info(string.Format("Playlistplayer: Start file ({0})", message.Label));
                    g_Player.Play(message.Label);
                    if (!g_Player.Playing) g_Player.Stop();
                }
                break;
        
                case GUIMessage.MessageType.GUI_MSG_STOP_FILE:
                {
                    logger.Info(string.Format("Playlistplayer: Stop file"));
                    g_Player.Stop();
                }
                break;

                case GUIMessage.MessageType.GUI_MSG_SEEK_FILE_PERCENTAGE:
                {
                    logger.Info(string.Format("Playlistplayer: SeekPercent ({0}%)", message.Param1));
                    g_Player.SeekAsolutePercentage(message.Param1);
                    logger.Debug(string.Format("Playlistplayer: SeekPercent ({0}%) done", message.Param1));
                }
                break;
        
                case GUIMessage.MessageType.GUI_MSG_SEEK_FILE_END:
                {
                    double duration = g_Player.Duration;
                    double position = g_Player.CurrentPosition;
                    if (position < duration - 1d)
                    {
                        logger.Info(string.Format("Playlistplayer: SeekEnd ({0})", duration));
                        g_Player.SeekAbsolute(duration - 2d);
                        logger.Debug(string.Format("Playlistplayer: SeekEnd ({0}) done", g_Player.CurrentPosition));
                    }
                }
                break;
        
                case GUIMessage.MessageType.GUI_MSG_SEEK_POSITION:
                {
                    g_Player.SeekAbsolute(message.Param1);
                }
                break;
            }
        }

        public string Get(int iItem)
        {
            if (_currentPlayList == PlayListType.PLAYLIST_NONE) return string.Empty;

            PlayList playlist = GetPlaylist(_currentPlayList);
            if (playlist.Count <= 0) return string.Empty;

            if (iItem >= playlist.Count)
            {
                if (!_repeatPlayList)
                {
                    return string.Empty; ;
                }
                iItem = 0;
            }

            PlayListItem item = playlist[iItem];
            return item.FileName;
        }

        public PlayListItem GetCurrentItem()
        {
            if (_currentItem < 0) return null;

            PlayList playlist = GetPlaylist(_currentPlayList);
            if (playlist == null) return null;

            if (_currentItem < 0 || _currentItem >= playlist.Count)
                _currentItem = 0;

            if (_currentItem >= playlist.Count) return null;

            return playlist[_currentItem];
        }

        public PlayListItem GetNextItem()
        {
            if (_currentPlayList == PlayListType.PLAYLIST_NONE) return null;

            PlayList playlist = GetPlaylist(_currentPlayList);
            if (playlist.Count <= 0) return null;
            int iItem = _currentItem;
            iItem++;

            if (iItem >= playlist.Count)
            {
                if (!_repeatPlayList)
                    return null;

                iItem = 0;
            }

            PlayListItem item = playlist[iItem];
            return item;
        }

        public string GetNext()
        {
            PlayListItem resultingItem = GetNextItem();
            if (resultingItem != null)
                return resultingItem.FileName;
            else
                return string.Empty;
        }

        public void PlayNext()
        {
            if (_currentPlayList == PlayListType.PLAYLIST_NONE) return;

            PlayList playlist = GetPlaylist(_currentPlayList);
            if (playlist.Count <= 0) return;
            int iItem = _currentItem;
            iItem++;

            if (iItem >= playlist.Count)
            {
                if (!_repeatPlayList)
                {
                    _currentPlayList = PlayListType.PLAYLIST_NONE;
                     return;
                }
                iItem = 0;
            }

            if (!Play(iItem))
            {
                if (!g_Player.Playing)
                {
                    PlayNext();
                }
            }
        }

        public void PlayPrevious()
        {
            if (_currentPlayList == PlayListType.PLAYLIST_NONE)
                return;

            PlayList playlist = GetPlaylist(_currentPlayList);
            if (playlist.Count <= 0) return;
            int iItem = _currentItem;
            iItem--;
            if (iItem < 0)
                iItem = playlist.Count - 1;

            if (!Play(iItem))
            {
                if (!g_Player.Playing)
                {
                    PlayPrevious();
                }
            }
        }

        public void Play(string filename)
        {
            if (_currentPlayList == PlayListType.PLAYLIST_NONE)
                return;

            PlayList playlist = GetPlaylist(_currentPlayList);
            for (int i = 0; i < playlist.Count; ++i)
            {
                PlayListItem item = playlist[i];
                if (item.FileName.Equals(filename))
                {
                    Play(i);
                    return;
                }
            }
        }

        public bool Play(int iItem)
        {
            // if play returns false PlayNext is called but this does not help against selecting an invalid file
            bool skipmissing = false;
            do
            {
                if (_currentPlayList == PlayListType.PLAYLIST_NONE)
                {
                    logger.Debug("PlaylistPlayer.Play() no playlist selected");
                    return false;
                }
                PlayList playlist = GetPlaylist(_currentPlayList);
                if (playlist.Count <= 0)
                {
                    logger.Debug("PlaylistPlayer.Play() playlist is empty");
                    return false;
                }
                if (iItem < 0) iItem = 0;
                if (iItem >= playlist.Count)
                {
                    if (skipmissing)
                        return false;
                    else
                    {
                        if (_entriesNotFound < playlist.Count)
                            iItem = playlist.Count - 1;
                        else
                            return false;
                    }
                }
                
                _currentItem = iItem;
                PlayListItem item = playlist[_currentItem];

                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS, 0, 0, 0, _currentItem, 0, null);
                msg.Label = item.FileName;
                GUIGraphicsContext.SendMessage(msg);

                if (playlist.AllPlayed())
                {
//                    playlist.ResetStatus();
                }

                bool playResult = false;

                DBLocalMedia mediaToPlay = item.Track.LocalMedia[0];
                MediaState mediaState = mediaToPlay.State;
                while (mediaState != MediaState.Online)
                {
                    switch (mediaState)
                    {
                        case MediaState.Removed:
                            GUIUtils.ShowOKDialog("Error", Localization.MediaIsMissing);
 //                           resetPlayer();
                            return false;
                        case MediaState.Offline:
                            string bodyString = String.Format(Localization.MediaNotAvailableBody, mediaToPlay.MediaLabel);
                            // Special debug line to troubleshoot availability issues
                            logger.Debug("Media not available: Path={0}, DriveType={1}, Serial={2}, ExpectedSerial={3}",
                                mediaToPlay.FullPath, mediaToPlay.ImportPath.GetDriveType().ToString(),
                                mediaToPlay.ImportPath.GetVolumeSerial(), mediaToPlay.VolumeSerial);

                            // Prompt user to enter media
                            if (!GUIUtils.ShowCustomYesNoDialog(Localization.MediaNotAvailableHeader, bodyString, Localization.Retry, Localization.Cancel, true))
                            {
                                // user cancelled so exit
//                                resetPlayer();
                                return false;
                            }
                            break;
                        case MediaState.NotMounted:
                            // Mount this media
                            MountResult result = mediaToPlay.Mount();
                            while (result == MountResult.Pending)
                            {
                                if (GUIUtils.ShowCustomYesNoDialog(Localization.VirtualDriveHeader, Localization.VirtualDriveMessage, Localization.Retry, Localization.Cancel, true))
                                {
                                    // User has chosen to retry
                                    // We stay in the mount loop
                                    result = mediaToPlay.Mount();
                                }
                                else
                                {
                                    // Exit the player
 //                                   resetPlayer();
                                    return false;
                                }
                            }

                            // If the mounting failed (can not be solved within the loop) show error and return
                            if (result == MountResult.Failed)
                            {
                                GUIUtils.ShowOKDialog(Localization.Error, Localization.FailedMountingImage);
                                // Exit the player
  //                              resetPlayer();
                                return false;
                            }

                            // Mounting was succesfull, break the mount loop
                            break;
                    }

                    // Check mediaState again
                    mediaState = mediaToPlay.State;
                }


                // Start Listening to any External Player Events
				listenToExternalPlayerEvents = true;
/*
                #region Publish Play properties for InfoService plugin
                string seriesName = item.SeriesName;
                string seasonID = item.SeasonIndex;
                string episodeID = item.EpisodeIndex;
                string episodeName = item.EpisodeName;
                GUIPropertyManager.SetProperty("#TVSeries.Extended.Title", string.Format("{0}/{1}/{2}/{3}", seriesName, seasonID, episodeID, episodeName));
                MPTVSeriesLog.Write(string.Format("#TVSeries.Extended.Title: {0}/{1}/{2}/{3}", seriesName, seasonID, episodeID, episodeName));
                #endregion
            */
                // Play File
                playResult = g_Player.Play(playlist, _currentItem);

                // Stope Listening to any External Player Events
				listenToExternalPlayerEvents = false;

                if (!playResult)
                {
                    //	Count entries in current playlist
                    //	that couldn't be played
                    _entriesNotFound++;
                    logger.Info(string.Format("PlaylistPlayer: *** unable to play - {0} - skipping file!", item.FileName));

                    // do not try to play the next file list
                    if (MediaPortal.Util.Utils.IsVideo(item.FileName))
                        skipmissing = false;
                    else
                        skipmissing = true;

                    iItem++;
                }
                else
                {
//                    item.Played = true;
//                    item.IsWatched = true; // for facade watched icons
                    skipmissing = false;
                    if (MediaPortal.Util.Utils.IsVideo(item.FileName))
                    {
                        if (g_Player.HasVideo)
                        {                            
                            g_Player.ShowFullScreenWindow();
                            System.Threading.Thread.Sleep(2000);
 //                           SetProperties(item, false);
                        }
                    }
                }
            }
            while (skipmissing);
            return g_Player.Playing;
        }

        /// <summary>        
        /// Updates the movie metadata on the playback screen (for when the user clicks info). 
        /// The delay is neccesary because Player tries to use metadata from the MyVideos database.
        /// We want to update this after that happens so the correct info is there.       
        /// </summary>
        /// <param name="item">Playlist item</param>
        /// <param name="clear">Clears the properties instead of filling them if True</param>
/*        private void SetProperties(PlayListItem item, bool clear)
        {
            if (item == null) return;

            string title = string.Empty;
            DBSeries series = null;
            DBSeason season = null;

            if (!clear)
            {
                title = string.Format("{0} - {1}x{2} - {3}", item.SeriesName, item.SeasonIndex, item.EpisodeIndex, item.EpisodeName);
                series = Helper.getCorrespondingSeries(item.Episode[DBEpisode.cSeriesID]);
                season = Helper.getCorrespondingSeason(item.Episode[DBEpisode.cSeriesID], int.Parse(item.SeasonIndex));
            }

            // Show Plot in OSD or Hide Spoilers (note: FieldGetter takes care of that)         
            GUIPropertyManager.SetProperty("#Play.Current.Plot", clear ? " " : FieldGetter.resolveDynString(TVSeriesPlugin.m_sFormatEpisodeMain, item.Episode));

            // Show Episode Thumbnail or Series Poster if Hide Spoilers is enabled
            string osdImage = string.Empty;            
            if (!clear)
            {
                foreach (KeyValuePair<string, string> kvp in SkinSettings.VideoOSDImages)
                {
                    switch (kvp.Key)
                    {
                        case "episode":
                            if (!DBOption.GetOptions(DBOption.cView_Episode_HideUnwatchedThumbnail) || item.Episode[DBOnlineEpisode.cWatched])
                                osdImage = ImageAllocator.ExtractFullName(localLogos.getFirstEpLogo(item.Episode));
                            break;
                        case "season":
                            osdImage = season.Banner;
                            break;
                        case "series":
                            osdImage = series.Poster;
                            break;
                        case "custom":
                            string value = replaceDynamicFields(kvp.Value, item.Episode);
                            string file = Helper.getCleanAbsolutePath(value);
                            if (System.IO.File.Exists(file))
                                osdImage = file;
                            break;
                    }

                    osdImage = osdImage.Trim();
                    if (string.IsNullOrEmpty(osdImage)) continue;
                    else break;
                }
            }
            GUIPropertyManager.SetProperty("#Play.Current.Thumb", clear ? " " : osdImage);

            foreach (KeyValuePair<string, string> kvp in SkinSettings.VideoPlayImages)
            {
                if (!clear)
                {
                    string value = replaceDynamicFields(kvp.Value, item.Episode);
                    string file = Helper.getCleanAbsolutePath(value);
                    if (System.IO.File.Exists(file))
                    {
                        MPTVSeriesLog.Write(string.Format("Setting play image {0} for property {1}", file, kvp.Key), MPTVSeriesLog.LogLevel.Debug);
                        GUIPropertyManager.SetProperty(kvp.Key, clear ? " " : file);
                    }
                }
                else
                {
                    MPTVSeriesLog.Write(string.Format("Clearing play image for property {0}", kvp.Key), MPTVSeriesLog.LogLevel.Debug);
                    GUIPropertyManager.SetProperty(kvp.Key, " ");
                }
            }

            GUIPropertyManager.SetProperty("#Play.Current.Title", clear ? "" : title);
            GUIPropertyManager.SetProperty("#Play.Current.Year", clear ? "" : item.FirstAired);
            GUIPropertyManager.SetProperty("#Play.Current.Genre", clear ? "" : FieldGetter.resolveDynString(TVSeriesPlugin.m_sFormatEpisodeSubtitle, item.Episode));
        }
        */
        private string replaceDynamicFields(string value, mvCentralDBTable item)
        {
            string result = value;

            Regex matchRegEx = new Regex(@"\<[a-zA-Z\.]+\>");
            foreach (Match m in matchRegEx.Matches(value))
            {
                string resolvedValue = "" ;//FieldGetter.resolveDynString(m.Value, item, false);
                result = result.Replace(m.Value, resolvedValue);
            }

            return result;
        }

        private void SetAsWatched()
        {
            PlayListItem item = GetCurrentItem();
            if (item == null)
                return;

//            item.Watched = true;
        }

        public int CurrentItem
        {
            get { return _currentItem; }
            set
            {     
                if (value >= -1 && value < GetPlaylist(CurrentPlaylistType).Count)
                    _currentItem = value;
            }
        }

        public string CurrentPlaylistName
        {
            get { return _currentPlaylistName; }
            set { _currentPlaylistName = value; }
        }

        public void Remove(PlayListType type, string filename)
        {
            PlayList playlist = GetPlaylist(type);
            int itemRemoved = playlist.Remove(filename);
            if (type != CurrentPlaylistType)
            {
                return;
            }
            if (_currentItem >= itemRemoved) _currentItem--;
        }

        public PlayListType CurrentPlaylistType
        {
            get { return _currentPlayList; }
            set
            {
                if (_currentPlayList != value)
                {
                    _currentPlayList = value;
                    _entriesNotFound = 0;
                }
            }
        }

        public PlayList GetPlaylist(PlayListType nPlayList)
        {
            switch (nPlayList)
            {        
                case PlayListType.PLAYLIST_MVCENTRAL: return _tvseriesPlayList;                
                default:
                    _emptyPlayList.Clear();
                    return _emptyPlayList;
            }
        }    

        public void Reset()
        {
            _currentItem = -1;
            _entriesNotFound = 0;
        }

        public int EntriesNotFound
        {
            get
            {
                return _entriesNotFound;
            }
        }

        public bool RepeatPlaylist
        {
            get { return _repeatPlayList; }
            set { _repeatPlayList = value; }
        }

        public bool PlaylistAutoPlay
        {
            get { return _playlistAutoPlay; }
            set { _playlistAutoPlay = value; }
        }

		public bool PlaylistAutoShuffle {
			get { return _playlistAutoShuffle; }
			set { _playlistAutoShuffle = value; }
		}

		#region External Player Event Handlers
		private void onStartExternal(Process proc, bool waitForExit) {
			// If we were listening for external player events
			if (listenToExternalPlayerEvents) {
                logger.Info("Playback Started in External Player");
			}
		}

		private void onStopExternal(Process proc, bool waitForExit) {
			if (!listenToExternalPlayerEvents)
				return;

            logger.Info("Playback Stopped in External Player");
			SetAsWatched();
			PlayNext();
			if (!g_Player.Playing) {
				g_Player.Release();

				// Clear focus when playback ended
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS, 0, 0, 0, -1, 0, null);
				GUIGraphicsContext.SendMessage(msg);
			}			
		}
		#endregion

    }
}
