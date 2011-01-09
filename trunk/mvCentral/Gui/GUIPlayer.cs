using System;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Threading;
using System.Collections.Generic;
using System.Collections;
using DShowNET.Helper;
using DirectShowLib;
using DirectShowLib.Dvd;
using Cornerstone.Database.CustomTypes;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Playlists;
using mvCentral.ROT;
using mvCentral.Localizations;
using mvCentral.Database;
using mvCentral.LocalMediaManagement;
using mvCentral.GUI;
using mvCentral.Utils;

using MediaPortal.Util;
using MediaPortal.InputDevices;
using NLog;

namespace mvCentral.GUI
{

    public delegate void mvPlayerEvent(DBTrackInfo mv);

    public class mvPlayer
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        public enum mvPlayerState { Idle, Processing, Playing }

        #region Private variables

        private mvGUIMain _gui;
        private bool customIntroPlayed = false;
        private bool mountedPlayback = false;
        private bool listenToExternalPlayerEvents = false;
        private DBLocalMedia queuedMedia;
        private int _activePart;
        private bool _resumeActive = false;

        private DBTrackInfo CurrentTrack;
        public PlayListPlayer listPlayer = PlayListPlayer.SingletonPlayer;
        public PlayList currentPlayList;



        List<DSGrapheditROTEntry> l1;
        protected IGraphBuilder _graphBuilder = null;
        protected IBasicVideo2 _basicVideo = null;
        /// <summary> dvd control interface. </summary>
        protected IDvdControl2 _dvdCtrl = null;
        /// <summary> asynchronous command interface. </summary>
        protected IDvdCmd _cmdOption = null;
        protected IBaseFilter _dvdbasefilter = null;



        #endregion

        #region Events

        public event mvPlayerEvent mvStarted;
        public event mvPlayerEvent mvStopped;
        public event mvPlayerEvent mvEnded;

        #endregion

        #region Ctor

        public mvPlayer(mvGUIMain gui) {
            _gui = gui;

            // external player handlers
            MediaPortal.Util.Utils.OnStartExternal += new MediaPortal.Util.Utils.UtilEventHandler(onStartExternal);
            MediaPortal.Util.Utils.OnStopExternal += new MediaPortal.Util.Utils.UtilEventHandler(onStopExternal);

            // default player handlers
            g_Player.PlayBackStarted += new g_Player.StartedHandler(onPlaybackStarted);
            g_Player.PlayBackEnded += new g_Player.EndedHandler(onPlayBackEnded);
            g_Player.PlayBackStopped += new g_Player.StoppedHandler(onPlayBackStoppedOrChanged);
            GUIGraphicsContext.Receivers += new SendMessageHandler(this.OnMessage);

            try {
                // This is a handler added in RC4 - if we are using an older mediaportal version
                // this would throw an exception.
                g_Player.PlayBackChanged += new g_Player.ChangedHandler(onPlayBackStoppedOrChanged);
            }
            catch (Exception) {
                logger.Warn("Running MediaPortal 1.0 RC3 or earlier. Unexpected behavior may occur when starting playback of a new mv without stopping previous mv. Please upgrade for better performance.");
            }

            logger.Info("mv Player initialized.");
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Get a value indicating that a mv is being played
        /// </summary>
        
        public bool IsPlaying {
            get {
                return (_playerState != mvPlayerState.Idle);
            }
        } 

        public mvPlayerState State {
            get {
                return _playerState;
            }
        } private mvPlayerState _playerState = mvPlayerState.Idle;

        /// <summary>
        /// Gets the currently playing mv
        /// </summary>
        public DBTrackInfo Currentmv {
            get {
                if (_activemv != null && _playerState == mvPlayerState.Playing)
                    return _activemv;

                return null;
            }
        } private DBTrackInfo _activemv;

        /// <summary>
        /// Gets the currently playing local media object
        /// </summary>
        public DBLocalMedia CurrentMedia {
            get {
                if (_activeMedia != null && _playerState == mvPlayerState.Playing)
                    return _activeMedia;

                return null;
            }
        }

        private DBLocalMedia activeMedia {
            get {
                return _activeMedia;
            }
            set {
                _activeMedia = value;
                if (_activeMedia != null) {
                    _activemv = _activeMedia.AttachedmvCentral[0];
                    _activePart = _activeMedia.Part;
                }
                else {
                    _activemv = null;
                    _activePart = 0;
                }
            }
        } private DBLocalMedia _activeMedia;

        #endregion

        #region Public Methods

        public void Play(DBTrackInfo mv) {
            CurrentTrack = mv;
            Play(mv, 1);
        }

        public void Play(DBTrackInfo mv, int part) {

            // stop the internal player if it's running
            if (g_Player.Playing)
                g_Player.Stop();

            // set player state working
            _playerState = mvPlayerState.Processing;
            
            // queue the local media object in case we first need to play the custom intro
            // we can get back to it later.
            queuedMedia = mv.LocalMedia[part-1];
            
            // try playing our custom intro (if present). If successful quit, as we need to
            // wait for the intro to finish.
            bool success = playCustomIntro();
            if (success) return;

            // Start mv
            playmv(mv, part);
        }

        public void Stop() {
            if (g_Player.Playing)
                g_Player.Stop();
            
            resetPlayer();
        }

        #endregion

        #region Playback logic

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
                        using (MediaPortal.Profile.Settings xmlreader = mvCentralCore.MediaPortalSettings)
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



        private void playmv(DBTrackInfo mv, int requestedPart) {
            logger.Debug("playmv()");
            _playerState = mvPlayerState.Processing;

            if (mv == null || requestedPart > mv.LocalMedia.Count || requestedPart < 1) {
                resetPlayer();
                return;
            }

            logger.Debug("Request: mv='{0}', Part={1}", mv.Track, requestedPart);
            for (int i = 0; i < mv.LocalMedia.Count; i++) {
                logger.Debug("LocalMedia[{0}] = {1}  Duration = {2}", i, mv.LocalMedia[i].FullPath, mv.LocalMedia[i].Duration);
            }

            int part = requestedPart;

            // if this is a request to start the mv from the begining, check if we should resume
            // or prompt the user for disk selection
            if (requestedPart == 1) {
                // check if we should be resuming, and if not, clear resume data
                _resumeActive = PromptUserToResume(mv);
                if (_resumeActive)
                    part = mv.ActiveUserSettings.ResumePart;
                else
                    clearmvResumeState(mv);

                // if we have a multi-part mv composed of disk images and we are not resuming 
                // ask which part the user wants to play
                if (!_resumeActive && mv.LocalMedia.Count > 1 && (mv.LocalMedia[0].IsImageFile || mv.LocalMedia[0].IsVideoDisc)) {
                    GUIDialogFileStacking dlg = (GUIDialogFileStacking)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_FILESTACKING);
                    if (null != dlg) {
                        dlg.SetNumberOfFiles(mv.LocalMedia.Count);
                        dlg.DoModal(GUIWindowManager.ActiveWindow);
                        part = dlg.SelectedFile;
                        if (part < 1) {
                            resetPlayer();
                            return;
                        }
                    }
                }
            }

            DBLocalMedia mediaToPlay = mv.LocalMedia[part - 1];
            MediaState mediaState = mediaToPlay.State;
            while (mediaState != MediaState.Online) {
                switch (mediaState) {
                    case MediaState.Removed:
                        _gui.ShowMessage("Error", Localization.MediaIsMissing);
                        resetPlayer();
                        return; 
                    case MediaState.Offline:
                        string bodyString = String.Format(Localization.MediaNotAvailableBody, mediaToPlay.MediaLabel);
                        // Special debug line to troubleshoot availability issues
                        logger.Debug("Media not available: Path={0}, DriveType={1}, Serial={2}, ExpectedSerial={3}",
                            mediaToPlay.FullPath, mediaToPlay.ImportPath.GetDriveType().ToString(),
                            mediaToPlay.ImportPath.GetVolumeSerial(), mediaToPlay.VolumeSerial);

                        // Prompt user to enter media
                        if (!_gui.ShowCustomYesNo(Localization.MediaNotAvailableHeader, bodyString, Localization.Retry, Localization.Cancel, true)) {
                            // user cancelled so exit
                            resetPlayer();
                            return;
                        }
                        break;
                    case MediaState.NotMounted:
                        // Mount this media
                        MountResult result = mediaToPlay.Mount();
                        while (result == MountResult.Pending) {
                            if (_gui.ShowCustomYesNo(Localization.VirtualDriveHeader, Localization.VirtualDriveMessage, Localization.Retry, Localization.Cancel, true)) {
                                // User has chosen to retry
                                // We stay in the mount loop
                                result = mediaToPlay.Mount();
                            }
                            else {
                                // Exit the player
                                resetPlayer();
                                return;
                            }
                        }

                        // If the mounting failed (can not be solved within the loop) show error and return
                        if (result == MountResult.Failed) {
                            _gui.ShowMessage(Localization.Error, Localization.FailedMountingImage);
                            // Exit the player
                            resetPlayer();
                            return;
                        }
                        
                        // Mounting was succesfull, break the mount loop
                        break;
                }

                // Check mediaState again
                mediaState = mediaToPlay.State;
            }
            
            // Get the path to the playable video.
            string videoPath = mediaToPlay.GetVideoPath();

            // If the media is an image, it will be mounted by this point so
            // we flag the mounted playback variable
            mountedPlayback = mediaToPlay.IsImageFile;

            // if we do not have MediaInfo but have the AutoRetrieveMediaInfo setting toggled
            // get the media info
            if (!mediaToPlay.HasMediaInfo && mvCentralCore.Settings.AutoRetrieveMediaInfo) {
                mediaToPlay.UpdateMediaInfo();
                mediaToPlay.Commit();
            }

            // store the current media object so we can request it later
            queuedMedia = mediaToPlay;

            // start playback
            logger.Info("Playing: mv='{0}' FullPath='{1}', VideoPath='{2}', Mounted={3})", mv.Track, mediaToPlay.FullPath, videoPath, mountedPlayback.ToString());
            playFile(videoPath, mediaToPlay.VideoFormat);            
        }

        private bool playCustomIntro() {
            // Check if we have already played a custom intro
            if (!customIntroPlayed) {
                DBTrackInfo queuedmv = queuedMedia.AttachedmvCentral[0];
                // Only play custom intro for we are not resuming
                if (queuedmv.UserSettings == null || queuedmv.UserSettings.Count == 0 || queuedmv.ActiveUserSettings.ResumeTime < 30) {
                    string custom_intro = mvCentralCore.Settings.CustomIntroLocation;

                    // Check if the custom intro is specified by user and exists
                    if (custom_intro.Length > 0 && File.Exists(custom_intro)) {
                        logger.Debug("Playing Custom Intro: {0}", custom_intro);

                        // we set this variable before we start the actual playback
                        // because playFile to account for the blocking nature of the
                        // mediaportal external player logic
                        customIntroPlayed = true;

                        // start playback
                        playFile(custom_intro);
                        return true;
                    }
                }
            }

            return false;
        }

        // Start playback of a file (detects format first)
        private void playFile(string media) {
            VideoFormat videoFormat = VideoUtility.GetVideoFormat(media);
            if (videoFormat != VideoFormat.NotSupported) {
                playFile(media, videoFormat);
            }
            else {
                logger.Warn("'{0}' is not a playable video file.", media);
                resetPlayer();
            }
        }

        // start playback of a file (using known format)
        private void playFile(string media, VideoFormat videoFormat) {
            logger.Debug("Processing media for playback: File={0}, VideoFormat={1}", media, videoFormat);
                        
            // HD Playback
            if (videoFormat == VideoFormat.Bluray || videoFormat == VideoFormat.HDDVD) {

                // Take proper action according to playback setting
                bool hdExternal = mvCentralCore.Settings.UseExternalPlayer;

                // Launch external player if user has configured it for HD playback.
                if (hdExternal) {
                    LaunchHDPlayer(media);
                    return;
                }

                // Alternate playback HD content (without menu)
                string newMedia = videoFormat.GetMainFeatureFilePath(media);
                 if (newMedia != null) {
                    // Check if the stream extension is in the mediaportal extension list.
                    if (Utility.IsMediaPortalVideoFile(newMedia)) {
                        media = newMedia;
                    }
                    else {
                        // Show a dialog to the user that explains how to configure the alternate playback
                        string ext = (videoFormat == VideoFormat.Bluray) ? ".M2TS" : ".EVO";
                        logger.Info("HD Playback: extension '{0}' is missing from the mediaportal configuration.", ext);
                        _gui.ShowMessage(Localization.PlaybackFailedHeader, String.Format(Localization.PlaybackFailed, ext));
                        resetPlayer();
                        return;
                    }
                }

                logger.Info("HD Playback: Internal, Media={0}", media);
            }
            
            // We start listening to external player events
            listenToExternalPlayerEvents = true;
            
            // Play the file using the mediaportal player
            bool success = g_Player.Play(media.Trim());
//            bool success = listPlayer.Play(_gui.lastItemVid - 1);

            // We stop listening to external player events
            listenToExternalPlayerEvents = false;

            // if the playback started and we are still playing go full screen (internal player)
            if (success && g_Player.Playing)
                g_Player.ShowFullScreenWindow();
            else if (!success) {
                // if the playback did not happen, reset the player
                logger.Info("Playback failed: Media={0}", media);
                resetPlayer();
            }
        }

        #endregion

        #region External HD Player

        // This method launches an external HD player controlled by Moving Pictures
        // Eventually when Mediaportal has a native solution for HD video disc formats
        // this will be not needed anymore.
        private void LaunchHDPlayer(string videoPath) {
            logger.Info("HD Playback: Launching external player.");

            // First check if the user supplied executable for the external player is valid
            string execPath = mvCentralCore.Settings.ExternalPlayerExecutable;
            if (!File.Exists(execPath)) {
                // if it's not show a dialog explaining the error
                _gui.ShowMessage("Error", Localization.MissingExternalPlayerExe);
                logger.Info("HD Playback: The external player executable '{0}' is missing.", execPath);
                // do nothing
                resetPlayer();
                return;
            }

            // process the argument string and replace the 'filename' variable
            string arguments = mvCentralCore.Settings.ExternalPlayerArguements;
            string videoRoot = Utility.GetMusicVideoBaseDirectory(new FileInfo(videoPath).Directory).FullName;
            string filename = Utility.IsDriveRoot(videoRoot) ? videoRoot : videoPath;
            string fps = ((int)(queuedMedia.VideoFrameRate + 0.5f)).ToString();
            arguments = arguments.Replace("%filename%", filename);
            arguments = arguments.Replace("%fps%", fps);

            logger.Debug("External Player: Video='{0}', FPS={1}, ExecCommandLine={2} {3}", filename, fps, execPath, arguments);

            // Set Refresh Rate Based On FPS if needed
            if (mvCentralCore.Settings.UseDynamicRefreshRateChangerWithExternalPlayer) {
                double framerate = double.Parse(queuedMedia.VideoFrameRate.ToString(NumberFormatInfo.InvariantInfo), NumberFormatInfo.InvariantInfo);
                logger.Info("Requesting new refresh rate: FPS={0}", framerate.ToString());
                RefreshRateChanger.SetRefreshRateBasedOnFPS(framerate, filename, RefreshRateChanger.MediaType.Video);
                if (RefreshRateChanger.RefreshRateChangePending) {
                    TimeSpan ts = DateTime.Now - RefreshRateChanger.RefreshRateChangeExecutionTime;
                    if (ts.TotalSeconds > RefreshRateChanger.WAIT_FOR_REFRESHRATE_RESET_MAX) {
                        logger.Info("Refresh rate change failed. Please check your mediaportal log and configuration", RefreshRateChanger.WAIT_FOR_REFRESHRATE_RESET_MAX);
                        RefreshRateChanger.ResetRefreshRateState();
                    } 
                }
            }

            // Setup the external player process
            ProcessStartInfo processinfo = new ProcessStartInfo();
            processinfo.FileName = execPath;
            processinfo.Arguments = arguments;

            Process hdPlayer = new Process();
            hdPlayer.StartInfo = processinfo;
            hdPlayer.Exited += OnHDPlayerExited;
            hdPlayer.EnableRaisingEvents = true;

            try {
                // start external player process
                hdPlayer.Start();
                
                // disable mediaportal input devices
                InputDevices.Stop();

                // hide mediaportal and suspend rendering to save resources for the external player
                GUIGraphicsContext.BlankScreen = true;
                GUIGraphicsContext.form.Hide();
                GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.SUSPENDING;

                logger.Info("HD Playback: External player started.");
                onMediaStarted(queuedMedia);
            }
            catch (Exception e) {
                logger.ErrorException("HD Playback: Could not start the external player process.", e);
                resetPlayer();
            }
        }

        private void OnHDPlayerExited(object obj, EventArgs e) {
            
            // Restore refresh rate if it was changed
            if (mvCentralCore.Settings.UseDynamicRefreshRateChangerWithExternalPlayer && RefreshRateChanger.RefreshRateChangePending)
                RefreshRateChanger.AdaptRefreshRate();

            // enable mediaportal input devices
            InputDevices.Init();

            // show mediaportal and start rendering
            GUIGraphicsContext.BlankScreen = false;
            GUIGraphicsContext.form.Show();
            GUIGraphicsContext.ResetLastActivity();
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GETFOCUS, 0, 0, 0, 0, 0, null);
            GUIWindowManager.SendThreadMessage(msg);
            GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.RUNNING;
            logger.Info("HD Playback: The external player has exited.");

            // call the logic for when an external player exits 
            onExternalExit();
        }

        #endregion

        #region Internal Player Event Handlers

        public void OnMessage(GUIMessage message)
        {
            switch (message.Message)
            {
//                case GUIMessage.MessageType.GUI_MSG_PLAYBACK_STOPPED:
                case GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS:
                    //                if (listPlayer.CurrentSong == -1) break;
                    //                DBTrackInfo mv = (DBTrackInfo)facade.ListLayout.SelectedListItem.MusicTag;
//                    if (listPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_NONE && !g_Player.Playing)
                        if (!g_Player.Playing)
                        {
//                        listPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_VIDEO;
                        int count = listPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO).Count;
                        _gui.lastItemVid++;
                        if (_gui.lastItemVid <= count)
                        {
                            PlayListItem p1 = currentPlayList[_gui.lastItemVid - 1];
                            if (p1.MusicTag != null)
                            {
                                DBTrackInfo mv = (DBTrackInfo)p1.MusicTag;
                                if (mv != null)
                                {
//                                    if (mv.LocalMedia[0].IsDVD)
                                    {
                                        Play(mv);
           //                               CurrentTrack = mv;
          //                                listPlayer.Play(_gui.lastItemVid - 1);
                                        //                listPlayer.PlayNext();
                                        //                                    PlayDVD(mv);
                                    }
                                }
                            }
                        }
                    }
                    break;
            }


            //        base.OnMessage(message);
//            return true;
//            return base.OnMessage(message);
        }

        private void onPlaybackStarted(g_Player.MediaType type, string filename)
        {
            if (_playerState == mvPlayerState.Processing && g_Player.Player.Playing) {
                logger.Info("Playback Started: Internal, File={0}", filename);

                // get the duration of the media 
                updateMediaDuration(queuedMedia);

                // get the mv
                DBTrackInfo mv = queuedMedia.AttachedmvCentral[0];

                // and jump to our resume position if necessary
                if (_resumeActive) {
                    if (g_Player.IsDVD) {
                        logger.Debug("Resume: DVD state.");
                        g_Player.Player.SetResumeState(mv.ActiveUserSettings.ResumeData.Data);
                    }
                    else {
                        logger.Debug("Resume: Time={0}", mv.ActiveUserSettings.ResumeTime);
                        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SEEK_POSITION, 0, 0, 0, 0, 0, null);
                        msg.Param1 = mv.ActiveUserSettings.ResumeTime;
                        GUIGraphicsContext.SendMessage(msg);
                    }
                    // deactivate resume
                    _resumeActive = false;
                }


//                if (_gui.listPlayer.CurrentSong == -1) return;
//                object o = _gui.listPlayer.GetCurrentItem().MusicTag;
//                if (o != null && o.GetType() == typeof(DBTrackInfo))
//                {
//                    DBTrackInfo mv1 = (DBTrackInfo)o;
                    if (CurrentTrack.LocalMedia[0].IsDVD)
                    {
                        PlayDVD(CurrentTrack);
                    }
//                }





                // Trigger mv started
                onMediaStarted(queuedMedia);
            }
        }

        private void onPlayBackStoppedOrChanged(g_Player.MediaType type, int timemvStopped, string filename) {
            if (type != g_Player.MediaType.Video || _playerState != mvPlayerState.Playing)
                return;

            logger.Debug("OnPlayBackStoppedOrChanged: File={0}, mv={1}, Part={2}, TimeStopped={3}", filename, _activemv.Track, _activePart, timemvStopped);

            // Because we can't get duration for DVD's at start like with normal files
            // we are getting the duration when the DVD is stopped. If the duration of 
            // feature is an hour or more it's probably the main feature and we will update
            // the database. 
            if (g_Player.IsDVD && (g_Player.Player.Duration >= 3600)) {
                DBLocalMedia playingFile = _activemv.LocalMedia[_activePart - 1];
                updateMediaDuration(playingFile);
            }

            int requiredWatchedPercent = mvCentralCore.Settings.MinimumWatchPercentage;
//            int watchedPercentage = _activemv.GetPercentage(_activePart, timemvStopped);
            int watchedPercentage = 50;

            logger.Debug("Watched: Percentage=" + watchedPercentage + ", Required=" + requiredWatchedPercent);

            // if enough of the mv has been watched
            if (watchedPercentage >= requiredWatchedPercent) {
                // run mv ended logic
                onmvEnded(_activemv);
            }
            // otherwise, store resume data.
            else {
                byte[] resumeData = null;
                g_Player.Player.GetResumeState(out resumeData);
                updatemvResumeState(_activemv, _activePart, timemvStopped, resumeData);
                // run mv stopped logic
                onmvStopped(_activemv);
            }            
        }

        private void onPlayBackEnded(g_Player.MediaType type, string filename) {
            if (type != g_Player.MediaType.Video || _playerState != mvPlayerState.Playing)
                return;

            if (handleCustomIntroEnded())
                return;

            logger.Debug("OnPlayBackEnded filename={0} currentmv={1} currentPart={2}", filename, _activemv.Track, _activePart);
            if (_activemv.LocalMedia.Count >= (_activePart + 1)) {
                logger.Debug("Goto next part");
                _activePart++;
                playmv(_activemv, _activePart);
            }
            else {
                onmvEnded(_activemv);
            }
        }

        #endregion

        #region External Player Event Handlers

        private void onStartExternal(Process proc, bool waitForExit) {
            // If we were listening for external player events
            if (_playerState == mvPlayerState.Processing && listenToExternalPlayerEvents) {
                logger.Info("Playback Started: External");
                onMediaStarted(queuedMedia);
            }
        }

        private void onStopExternal(Process proc, bool waitForExit) {
            if (_playerState != mvPlayerState.Playing || !listenToExternalPlayerEvents)
                return;

            logger.Debug("Handling: OnStopExternal()");
            
            // call the logic for when an external player exits
            onExternalExit();
        }

        #endregion

        #region Player Events

        private bool handleCustomIntroEnded() {
            if (customIntroPlayed) {

                // Set custom intro played back to false
                customIntroPlayed = false;

                // If a custom intro was just played, we need to play the selected mv
                playmv(queuedMedia.AttachedmvCentral[0], queuedMedia.Part);
                return true;
            }

            return false;
        }

        private void onExternalExit() {
            if (handleCustomIntroEnded())
                return;

            if (Currentmv == null)
                return;

            if (_activePart < _activemv.LocalMedia.Count) {
                string sBody = String.Format(Localization.ContinueToNextPartBody, (_activePart + 1)) + "\n" + _activemv.Track;
                bool bContinue = _gui.ShowCustomYesNo(Localization.ContinueToNextPartHeader, sBody, null, null, true);

                if (bContinue) {
                    logger.Debug("Goto next part");
                    _activePart++;
                    playmv(_activemv, _activePart);
                }
                else {
                    // mv stopped
                    onmvStopped(_activemv);
                }
            }
            else {
                // mv ended
                onmvEnded(_activemv);
            }
        }

        private void onMediaStarted(DBLocalMedia localMedia) {
           // set playback active
           _playerState = mvPlayerState.Playing;
           
           DBTrackInfo previousmv = Currentmv;
           activeMedia = localMedia;

           // Update OSD (delayed)
           Thread newThread = new Thread(new ThreadStart(UpdatePlaybackInfo));
           newThread.Start();

           // only invoke mv started event if we were not playing this mv before
           if ((previousmv != Currentmv) && (mvStarted != null))
               mvStarted(Currentmv);
        }

        private void onmvStopped(DBTrackInfo mv) {
            // reset player
            resetPlayer();

            // invoke event
            if (mvStopped != null)
                mvStopped(mv);
        }

        private void onmvEnded(DBTrackInfo mv) {
            // update watched counter
            updatemvWatchedCounter(mv);

            // clear resume state
            clearmvResumeState(mv);

            // reset player
            resetPlayer();

            // invoke event
            if (mvEnded != null)
                mvEnded(mv);           
        }

        /// <summary>
        /// Resets player variables
        /// </summary>
        private void resetPlayer() {

            // If we have an image mounted, unmount it
            if (mountedPlayback) {
                queuedMedia.UnMount();
                mountedPlayback = false;
            }
            
            // reset player variables

            if (GUIGraphicsContext.IsFullScreenVideo)
                GUIGraphicsContext.IsFullScreenVideo = false;

            activeMedia = null;
            queuedMedia = null;
            _playerState = mvPlayerState.Idle;
            _resumeActive = false;
            listenToExternalPlayerEvents = false;
            customIntroPlayed = false;
           
            logger.Debug("Reset.");
        }

        #endregion

        #region mv Update Methods

        // store the duration of the file if it is not set
        private void updateMediaDuration(DBLocalMedia localMedia) {
            if (localMedia.Duration == 0) {
                logger.Debug("UpdateMediaDuration: LocalMedia={0}, Format={1}, Duration={2}", localMedia.FullPath, localMedia.VideoFormat, g_Player.Player.Duration.ToString(NumberFormatInfo.InvariantInfo));
                localMedia.Duration = ((int)g_Player.Player.Duration) * 1000;
                localMedia.Commit();
            }
        }

        private void updatemvWatchedCounter(DBTrackInfo mv) {
            if (mv == null)
                return;

            // get the user settings for the default profile (for now)
            DBUserMusicVideoSettings userSetting = mv.ActiveUserSettings;
            userSetting.WatchedCount++; // increment watch counter
            userSetting.Commit();
            DBWatchedHistory.AddWatchedHistory(mv, userSetting.User);
        }

        private void clearmvResumeState(DBTrackInfo mv) {
            updatemvResumeState(mv, 0, 0, null);
        }

        private void updatemvResumeState(DBTrackInfo mv, int part, int timePlayed, byte[] resumeData) {
            if (mv.UserSettings.Count == 0)
                return;

            // get the user settings for the default profile (for now)
            DBUserMusicVideoSettings userSetting = mv.ActiveUserSettings;

            if (timePlayed > 0) {
                // set part and time data 
                userSetting.ResumePart = part;
                userSetting.ResumeTime = timePlayed;
                userSetting.ResumeData = new ByteArray(resumeData);
                logger.Debug("Updating mv resume state.");
            }
            else {
                // clear the resume settings
                userSetting.ResumePart = 0;
                userSetting.ResumeTime = 0;
                userSetting.ResumeData = null;
                logger.Debug("Clearing mv resume state.");
            }
            // save the changes to the user setting for this mv
            userSetting.Commit();
        }

        #endregion

        #region GUI/OSD

        // Updates the mv metadata on the playback screen (for when the user clicks info). 
        // The delay is necessary because Player tries to use metadata from the MyVideos database.
        // We want to update this after that happens so the correct info is there.
        private void UpdatePlaybackInfo() {
            Thread.Sleep(2000);
            if (Currentmv != null) {
                _gui.SetProperty("#Play.Current.Track", Currentmv.Track);
                _gui.SetProperty("#Play.Current.Plot", Currentmv.bioContent);
                _gui.SetProperty("#Play.Current.Thumb", Currentmv.ArtThumbFullPath);
//                _gui.SetProperty("#Play.Current.Year", Currentmv.Year.ToString());

//                if (Currentmv.Genres.Count > 0)
//                    _gui.SetProperty("#Play.Current.Genre", Currentmv.Genres[0]);
//                else
//                    _gui.SetProperty("#Play.Current.Genre", "");

            }
        }

        private bool PromptUserToResume(DBTrackInfo mv) {
            if (mv.UserSettings == null || mv.UserSettings.Count == 0 || (mv.ActiveUserSettings.ResumePart < 2 && mv.ActiveUserSettings.ResumeTime <= 30))
                return false;

            logger.Debug("Resume Prompt: mv='{0}', ResumePart={1}, ResumeTime={2}", mv.Track, mv.ActiveUserSettings.ResumePart, mv.ActiveUserSettings.ResumeTime);

            // figure out the resume time to display to the user
            int displayTime = mv.ActiveUserSettings.ResumeTime;
            if (mv.LocalMedia.Count > 1) {
                for (int i = 0; i < mv.ActiveUserSettings.ResumePart - 1; i++) {
                    if (mv.LocalMedia[i].Duration > 0)
                        displayTime += (mv.LocalMedia[i].Duration / 1000); // convert milliseconds to seconds
                }
            }

            string sbody = mv.Track + "\n" + Localization.ResumeFrom + " " + MediaPortal.Util.Utils.SecondsToHMSString(displayTime);
            bool bResume = _gui.ShowCustomYesNo(Localization.ResumeFromLast, sbody, null, null, true);

            if (bResume)
                return true;

            return false;
        }

        #endregion

    }
}
