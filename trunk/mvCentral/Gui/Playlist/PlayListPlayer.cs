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
using System.Timers;
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
using mvCentral.GUI;

namespace mvCentral.Playlist
{



  public class PlayListPlayer
  {
    #region g_Player

    public interface IPlayer
    {
      bool msgblocked { get; set; }
      bool Playing { get; }
      void Release();
      bool Play(string strFile);
      void PlayDVD(DBTrackInfo db1);
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

      private System.Windows.Forms.Timer playTimer = new System.Windows.Forms.Timer();
      public bool msgblocked1;
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
        return MediaPortal.Player.g_Player.Play(strFile, g_Player.MediaType.Video);
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

      /// <summary>
      /// DVD Code - Play DVD
      /// </summary>
      /// <param name="db1"></param>
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
          //hr = _mediaCtrl.Run();
          //hr = _mediaCtrl.Pause();
          //_offsetseek = (ulong)seekbar.Value;
          TimeSpan t1 = TimeSpan.FromMilliseconds(0);
          TimeSpan t2 = TimeSpan.Parse(db1.OffsetTime);
          t1 = t1.Add(t2);
          //                            t1 = TimeSpan.Parse(db1.PlayTime);
          t2 = t2.Add(TimeSpan.Parse(db1.PlayTime));
          DvdHMSFTimeCode t3 = mvCentralUtils.ConvertToDvdHMSFTimeCode(t1);
          DvdHMSFTimeCode t4 = mvCentralUtils.ConvertToDvdHMSFTimeCode(t2);
          //if (state == FilterState.Stopped)
          int hr = _mediaCtrl.Run();
          hr = _dvdCtrl.PlayAtTimeInTitle(db1.TitleID, t3, DvdCmdFlags.Flush | DvdCmdFlags.Block, out _cmdOption);
          //                    hr = _dvdCtrl.PlayPeriodInTitleAutoStop(6, t3, t4, DvdCmdFlags.Flush | DvdCmdFlags.Block, out _cmdOption);
          DsError.ThrowExceptionForHR(hr);
          //int hr = _dvdCtrl.PlayChaptersAutoStop(1, db1.ChapterID, 1, 0, out _cmdOption);
          //DsError.ThrowExceptionForHR(hr);
        }
      }

      private void PurgeEntries()
      {
        if (l1 != null)
          foreach (DSGrapheditROTEntry rote in l1)
          {
            rote.Dispose();
          }
      }
    }

    public IPlayer mvPlayer = new FakePlayer();
    

    #endregion


    private static Logger logger = LogManager.GetCurrentClassLogger();

    int _entriesNotFound = 0;
    int _currentItem = -1;
    PlayListType _currentPlayList = PlayListType.PLAYLIST_NONE;
    PlayList _mvCentralPlayList = new PlayList();
    PlayList _emptyPlayList = new PlayList();
    bool _repeatPlayList = mvCentralCore.Settings.repeatPlayList;
    bool _playlistAutoPlay = mvCentralCore.Settings.playlistAutoPlay;
    bool _playlistAutoShuffle = mvCentralCore.Settings.playlistAutoShuffle;
    string _currentPlaylistName = string.Empty;
    private bool listenToExternalPlayerEvents = false;
    private bool externalPlayerStopped = true;
    private bool m_bIsExternalPlayer = false;
    private bool m_bIsExternalDVDPlayer = false;
    private bool skipTrackActive = false;

    DBTrackInfo CurrentTrack = null;
    
    LastFMScrobble LastFMProfile = new LastFMScrobble();

    private System.Timers.Timer timerClearProperty;

    public PlayListPlayer()
    {
      Init();
      // Check if External Player is being used
      MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(MediaPortal.Configuration.Config.GetFile(MediaPortal.Configuration.Config.Dir.Config, "MediaPortal.xml"));
      m_bIsExternalPlayer = !xmlreader.GetValueAsBool("movieplayer", "internal", true);
      m_bIsExternalDVDPlayer = !xmlreader.GetValueAsBool("dvdplayer", "internal", true);

      if (!LastFMProfile.IsLoged)
        LastFMProfile.Login(mvCentralCore.Settings.LastFMUsername, mvCentralCore.Settings.LastFMPassword);

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
      timerClearProperty = new System.Timers.Timer(5000);

      GUIWindowManager.Receivers += new SendMessageHandler(this.OnMessage);
      GUIWindowManager.OnNewAction += new OnActionHandler(this.OnNewAction);

      // external player handlers
      MediaPortal.Util.Utils.OnStartExternal += new MediaPortal.Util.Utils.UtilEventHandler(onStartExternal);
      MediaPortal.Util.Utils.OnStopExternal += new MediaPortal.Util.Utils.UtilEventHandler(onStopExternal);
    }

    void OnNewAction(MediaPortal.GUI.Library.Action action)
    {
      PlayListItem item = GetCurrentItem();

      switch (action.wID)
      {
        case MediaPortal.GUI.Library.Action.ActionType.ACTION_NEXT_ITEM:
        case MediaPortal.GUI.Library.Action.ActionType.ACTION_NEXT_CHAPTER:
          skipTrackActive = true;
          PlayNext();
          if (item != null && mvCentralCore.Settings.SubmitOnLastFM)
            scrobbleSubmit(item);

          break;

        case MediaPortal.GUI.Library.Action.ActionType.ACTION_PREV_ITEM:
        case MediaPortal.GUI.Library.Action.ActionType.ACTION_PREV_CHAPTER:
          skipTrackActive = true;
          PlayPrevious();
          if (item != null && mvCentralCore.Settings.SubmitOnLastFM)
            scrobbleSubmit(item);

          break;
      }
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
              // when skipping video with Prev or Next the stopped message is received but as skipping
              // we do not want to treat this as a stop whcih resets and clears the playlist
              // the skipTrackActive bool is set before playNext of playPrevious methods are called and we check this
              // and only clear the playlist down if not true.
              if (skipTrackActive)
                skipTrackActive = false;
              else
              {
                SetProperties(item, true);
                Reset();
                _currentPlayList = PlayListType.PLAYLIST_NONE;
              }
              if (item != null && mvCentralCore.Settings.SubmitOnLastFM)
                scrobbleSubmit(item);
            }
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS, 0, 0, 0, -1, 0, null);
            GUIGraphicsContext.SendMessage(msg);
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_PLAYBACK_ENDED:
          {
            SetAsWatched();

            PlayListItem item = GetCurrentItem();
            if (item != null && mvCentralCore.Settings.SubmitOnLastFM)
              scrobbleSubmit(item);

            PlayNext();

            if (!mvPlayer.Playing)
            {
              logger.Debug("After GUI_MSG_PLAYBACK_ENDED g_Player.Playing is false");
              mvPlayer.Release();

              // Clear focus when playback ended
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS, 0, 0, 0, -1, 0, null);
              GUIGraphicsContext.SendMessage(msg);
            }
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_PLAY_FILE:
          {
            logger.Debug(string.Format("Playlistplayer: Start file ({0})", message.Label));
            // Play the file
            mvPlayer.Play(message.Label);
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_STOP_FILE:
          {
            logger.Debug(string.Format("Playlistplayer: Stop file"));
            mvPlayer.Stop();

            PlayListItem item = GetCurrentItem();
            if (item != null && mvCentralCore.Settings.SubmitOnLastFM)
              scrobbleSubmit(item);
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_SEEK_FILE_PERCENTAGE:
          {
            logger.Info(string.Format("Playlistplayer: SeekPercent ({0}%)", message.Param1));
            mvPlayer.SeekAsolutePercentage(message.Param1);
            logger.Debug(string.Format("Playlistplayer: SeekPercent ({0}%) done", message.Param1));
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_SEEK_FILE_END:
          {
            double duration = mvPlayer.Duration;
            double position = mvPlayer.CurrentPosition;
            if (position < duration - 1d)
            {
              logger.Info(string.Format("Playlistplayer: SeekEnd ({0})", duration));
              mvPlayer.SeekAbsolute(duration - 2d);
              logger.Debug(string.Format("Playlistplayer: SeekEnd ({0}) done", mvPlayer.CurrentPosition));
            }
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_SEEK_POSITION:
          {
            mvPlayer.SeekAbsolute(message.Param1);
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
    /// <summary>
    /// Play next video in playlist
    /// </summary>
    public void PlayNext()
    {
      if (_currentPlayList == PlayListType.PLAYLIST_NONE) 
        return;

      PlayList playlist = GetPlaylist(_currentPlayList);

      if (playlist.Count <= 0) 
        return;

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

      if (m_bIsExternalPlayer && externalPlayerStopped)
        return;

      if (!Play(iItem))
      {
        if (!mvPlayer.Playing)
        {
          PlayNext();
        }
      }
    }
    /// <summary>
    /// Play previous track in playlist
    /// </summary>
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
        if (!mvPlayer.Playing)
        {
          PlayPrevious();
        }
      }
    }
    /// <summary>
    /// Play the video
    /// </summary>
    /// <param name="filename"></param>
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
    /// <summary>
    /// Play the video
    /// </summary>
    /// <param name="iItem"></param>
    /// <returns></returns>
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

        GUIMessage GUIMessageToSend = null;
        _currentItem = iItem;
        PlayListItem item = playlist[_currentItem];
        GUIMessageToSend = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS, 0, 0, 0, _currentItem, 0, null);
        GUIMessageToSend.Label = item.Track.LocalMedia[0].File.FullName;
        logger.Debug("Sending GUI_MSG_ITEM_FOCUS message from PlayListPlayer");
        GUIGraphicsContext.SendMessage(GUIMessageToSend);

        GUIMessageToSend = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYLIST_CHANGED, 0, 0, 0, _currentItem, 0, null);
        GUIMessageToSend.Label = item.Track.LocalMedia[0].File.FullName;
        logger.Debug("Sending GUI_MSG_PLAYLIST_CHANGED message from PlayListPlayer");
        GUIGraphicsContext.SendMessage(GUIMessageToSend);


        if (playlist.AllPlayed())
        {
          //playlist.ResetStatus();
        }

        bool playResult = false;
        // If the file is an image file, it should be mounted before playing
        string filename = item.Track.LocalMedia[0].File.FullName;
        CurrentTrack = item.Track;

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
                  // Exit Player
                  return false;
                }
              }
              // If the mounting failed (can not be solved within the loop) show error and return
              if (result == MountResult.Failed)
              {
                GUIUtils.ShowOKDialog(Localization.Error, Localization.FailedMountingImage);
                // Exit the player
                return false;
              }
              // Mounting was succesfull, break the mount loop
              break;
          }

          // Check mediaState again
          mediaState = mediaToPlay.State;
        }

        // Start Listening to any External Player Events
        logger.Debug("Start Listening to any External Player Events");
        listenToExternalPlayerEvents = true;

        // Play File
        mvGUIMain.currentArtistID = (int)CurrentTrack.ArtistInfo[0].ID;
        mvGUIMain.currentArtistInfo = CurrentTrack.ArtistInfo[0];
        logger.Debug(string.Format("Start playing : Artist: {0} with and ID: {1} Filename :{2}", mvGUIMain.currentArtistInfo.Artist, mvGUIMain.currentArtistID, filename));
        
        if (mvPlayer.Playing)
          mvPlayer.Stop();

        //if (CurrentTrack.LocalMedia[0].IsDVD)
        //  mvPlayer.PlayDVD(CurrentTrack);
        //else
          playResult = mvPlayer.Play(filename);      

        // Stop Listening to any External Player Events
        logger.Debug("Stop Listening to any External Player Events");
        listenToExternalPlayerEvents = false;

        if (!playResult)
        {
          //	Count entries in current playlist that couldn't be played
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
          SetProperties(item, false);

          if (mvCentralCore.Settings.ShowOnLastFM)
            scrobbleSend(item);

          skipmissing = false;
          if (MediaPortal.Util.Utils.IsVideo(item.FileName))
          {
            if (mvPlayer.HasVideo)
            {
              // needed so everything goes in sequence otherwise stop gets handled after the play events
              if (mvCentralCore.Settings.AutoFullscreen)
              {
                logger.Debug("Setting Fullscreen");
                mvPlayer.ShowFullScreenWindow();
              }
            }
          }
        }
      }
      while (skipmissing);
     
      //SetInitTimerValues();
      //playTimer.Start();

      return mvPlayer.Playing;
    }

    void scrobbleSend(PlayListItem item)
    {    
      DBArtistInfo artistInfo = null;
      DBTrackInfo trackInfo = null;
      trackInfo = item.Track;
      artistInfo = DBArtistInfo.Get(trackInfo);
      TimeSpan tt = TimeSpan.Parse(trackInfo.PlayTime);
      logger.Debug("Sending Scrobble info {0} - {1} ({2})", artistInfo.Artist, trackInfo.Track, tt.TotalSeconds.ToString());
      LastFMProfile.NowPlaying(artistInfo.Artist, trackInfo.Track, (int)tt.TotalSeconds);
    }

    void scrobbleSubmit(PlayListItem item)
    {
      DBArtistInfo artistInfo = null;
      DBTrackInfo trackInfo = null;
      trackInfo = item.Track;
      artistInfo = DBArtistInfo.Get(trackInfo);
      TimeSpan tt = TimeSpan.Parse(trackInfo.PlayTime);
      logger.Debug("Submit Track to Last.FM {0} - {1} (2}", artistInfo.Artist, trackInfo.Track, tt.Seconds);
      LastFMProfile.Submit(artistInfo.Artist, trackInfo.Track, tt.Seconds);
      
    }

    /// <summary>        
    /// Updates the movie metadata on the playback screen (for when the user clicks info). 
    /// The delay is neccesary because Player tries to use metadata from the MyVideos database.
    /// We want to update this after that happens so the correct info is there.       
    /// </summary>
    /// <param name="item">Playlist item</param>
    /// <param name="clear">Clears the properties instead of filling them if True</param>
    private void SetProperties(PlayListItem item, bool clear)
    {
      // List of Play properities that can be overidden
      //#Play.Current.Director
      //#Play.Current.Genre
      //#Play.Current.Cast
      //#Play.Current.DVDLabel
      //#Play.Current.IMDBNumber
      //#Play.Current.File
      //#Play.Current.Plot
      //#Play.Current.PlotOutline
      //#Play.Current.UserReview
      //#Play.Current.Rating
      //#Play.Current.TagLine
      //#Play.Current.Votes
      //#Play.Current.Credits
      //#Play.Current.Thumb
      //#Play.Current.Title
      //#Play.Current.Year
      //#Play.Current.Runtime
      //#Play.Current.MPAARating

      logger.Debug("******************************************************");

      if (clear)
        logger.Debug("************* CLEAR Play Properities event *************");
      else
        logger.Debug("************** SET Play Properities event **************");

      logger.Debug("******************************************************");


      if (item == null)
        return;

      string title = string.Empty;
      string osdImage = string.Empty;
      string osdVideoImage = string.Empty;
      string osdArtistImage = string.Empty;
      string album = string.Empty;
      string genre = string.Empty;
      string isWatched = "no";

      DBArtistInfo artistInfo = null;
      DBTrackInfo trackInfo = null;

      if (!clear)
      {
        // Only sleep if setting the props
        Thread.Sleep(2000);
        timerClearProperty.Elapsed += new ElapsedEventHandler(timerClearProperty_Elapsed);
        timerClearProperty.Enabled = true;
        GUIPropertyManager.SetProperty("#mvCentral.Play.Started", "true");

        trackInfo = (DBTrackInfo)item.Track;
        artistInfo = DBArtistInfo.Get(trackInfo);
        // may not have an album
        if (trackInfo.AlbumInfo.Count > 0)
          album = trackInfo.AlbumInfo[0].Album;

        title = trackInfo.Track;

        if (System.IO.File.Exists(artistInfo.ArtFullPath))
          osdImage = artistInfo.ArtFullPath;

        if (System.IO.File.Exists(trackInfo.ArtFullPath))
          osdVideoImage = trackInfo.ArtFullPath;

        if (artistInfo.Genre.Trim().Length > 0)
          genre = artistInfo.Genre;

        // Has this video been watched
        DBUserMusicVideoSettings userSettings = trackInfo.ActiveUserSettings;
        if (userSettings.WatchedCount > 0)
          isWatched = "yes";


      }
      // Std Play Properities
      GUIPropertyManager.SetProperty("#Play.Current.Title", clear ? string.Empty : title);
      GUIPropertyManager.SetProperty("#Play.Current.Thumb", clear ? string.Empty : osdImage);
      GUIPropertyManager.SetProperty("#Play.Current.Genre", clear ? string.Empty : genre);
      GUIPropertyManager.SetProperty("#Play.Current.Runtime", clear ? string.Empty : trackDuration(trackInfo.PlayTime));
      GUIPropertyManager.SetProperty("#Play.Current.Rating", clear ? string.Empty : trackInfo.Rating.ToString());
      GUIPropertyManager.SetProperty("#Play.Current.Plot", clear ? string.Empty : trackInfo.bioContent);
      GUIPropertyManager.SetProperty("#Play.Current.IsWatched", isWatched);

      // mvCentral Play Properities
      GUIPropertyManager.SetProperty("#Play.Current.mvArtist", clear ? string.Empty : artistInfo.Artist);
      GUIPropertyManager.SetProperty("#Play.Current.mvAlbum", clear ? string.Empty : album);
      GUIPropertyManager.SetProperty("#Play.Current.mvVideo", clear ? string.Empty : title);
      GUIPropertyManager.SetProperty("#Play.Current.Video.Thumb", clear ? string.Empty : osdVideoImage);
      GUIPropertyManager.SetProperty("#mvCentral.isPlaying", clear ? "false" : "true");
      // Video Properities
      try
      {
        DBLocalMedia mediaInfo = (DBLocalMedia)trackInfo.LocalMedia[0];
        GUIPropertyManager.SetProperty("#Play.Current.AspectRatio", mediaInfo.VideoAspectRatio);
        GUIPropertyManager.SetProperty("#Play.Current.VideoCodec.Texture", mediaInfo.VideoCodec);
        GUIPropertyManager.SetProperty("#Play.Current.VideoResolution", mediaInfo.VideoResolution);
        GUIPropertyManager.SetProperty("#mvCentral.Current.videowidth", mediaInfo.VideoWidth.ToString());
        GUIPropertyManager.SetProperty("#mvCentral.Current.videoheight", mediaInfo.VideoHeight.ToString());
        GUIPropertyManager.SetProperty("#mvCentral.Current.videoframerate", mediaInfo.VideoFrameRate.ToString());
        GUIPropertyManager.SetProperty("#Play.Current.AudioCodec.Texture", mediaInfo.AudioCodec);
        GUIPropertyManager.SetProperty("#Play.Current.AudioChannels", mediaInfo.AudioChannels);

        if (!clear)
        {
          logger.Debug("**** Setting Play Properities for {0} ****", artistInfo.Artist);
          logger.Debug(" ");
          logger.Debug("#Play.Current.Title {0}", title);
          logger.Debug("#Play.Current.Thumb {0}", osdImage);
          logger.Debug("#Play.Current.Genre {0}", genre);
          logger.Debug("#Play.Current.Runtime {0}", trackDuration(trackInfo.PlayTime));
          logger.Debug("#Play.Current.Rating {0}", trackInfo.Rating.ToString());
          logger.Debug("#Play.Current.Plot {0}", trackInfo.bioContent);
          logger.Debug("#Play.Current.IsWatched {0}", isWatched);
          logger.Debug("#Play.Current.mvArtist {0}", artistInfo.Artist);
          logger.Debug("#Play.Current.mvAlbum {0}", album);
          logger.Debug("#Play.Current.mvVideo {0}", title);
          logger.Debug("#Play.Current.Video.Thumb {0}", osdVideoImage);
          logger.Debug("#mvCentral.isPlaying {0}", clear ? "false" : "true");
          logger.Debug("#Play.Current.AspectRatio {0}", mediaInfo.VideoAspectRatio);
          logger.Debug("#Play.Current.VideoCodec.Texture {0}", mediaInfo.VideoCodec);
          logger.Debug("#Play.Current.VideoResolution {0}", mediaInfo.VideoResolution);
          logger.Debug("#mvCentral.Current.videowidth {0}", mediaInfo.VideoWidth.ToString());
          logger.Debug("#mvCentral.Current.videoheight {0}", mediaInfo.VideoHeight.ToString());
          logger.Debug("#mvCentral.Current.videoframerate {0}", mediaInfo.VideoFrameRate.ToString());
          logger.Debug("#Play.Current.AudioCodec.Texture {0}", mediaInfo.AudioCodec);
          logger.Debug("#Play.Current.AudioChannels {0}", mediaInfo.AudioChannels);
          logger.Debug(" ");
        }
      }
      catch
      {
        GUIPropertyManager.SetProperty("#Play.Current.AspectRatio", string.Empty);
        GUIPropertyManager.SetProperty("#Play.Current.VideoCodec.Texture", string.Empty);
        GUIPropertyManager.SetProperty("#Play.Current.VideoResolution", string.Empty);
        GUIPropertyManager.SetProperty("#mvCentral.Current.videowidth", string.Empty);
        GUIPropertyManager.SetProperty("#mvCentral.Current.videoheight", string.Empty);
        GUIPropertyManager.SetProperty("#mvCentral.Current.videoframerate", string.Empty);
        GUIPropertyManager.SetProperty("#Play.Current.AudioCodec.Texture", string.Empty);
        GUIPropertyManager.SetProperty("#Play.Current.AudioChannels", string.Empty);
      }      

    }

    void timerClearProperty_Elapsed(object sender, ElapsedEventArgs e)
    {
      logger.Debug("************* Clear Property Timer Fired ****************");
      GUIPropertyManager.SetProperty("#mvCentral.Play.Started", "false");
      timerClearProperty.Elapsed -= new ElapsedEventHandler(timerClearProperty_Elapsed);
      timerClearProperty.Enabled = false;
    }

    /// <summary>
    /// Convert the track running time
    /// </summary>
    /// <param name="playTime"></param>
    /// <returns></returns>
    private string trackDuration(string playTime)
    {
      try
      {
        TimeSpan tt = TimeSpan.Parse(playTime);
        DateTime dt = new DateTime(tt.Ticks);
        string cTime = String.Format("{0:HH:mm:ss}", dt);
        if (cTime.StartsWith("00:"))
          return cTime.Substring(3);
        else
          return cTime;
      }
      catch
      {
        return "00:00:00";
      }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    private string replaceDynamicFields(string value, mvCentralDBTable item)
    {
      string result = value;

      Regex matchRegEx = new Regex(@"\<[a-zA-Z\.]+\>");
      foreach (Match m in matchRegEx.Matches(value))
      {
        string resolvedValue = "";//FieldGetter.resolveDynString(m.Value, item, false);
        result = result.Replace(m.Value, resolvedValue);
      }

      return result;
    }
    /// <summary>
    /// Set as Watched
    /// </summary>
    private void SetAsWatched()
    {
      PlayListItem item = GetCurrentItem();
      if (item == null)
        return;

      DBUserMusicVideoSettings userSettings = item.Track.ActiveUserSettings;
      userSettings.WatchedCount++;
      userSettings.Commit();
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
        case PlayListType.PLAYLIST_MVCENTRAL: return _mvCentralPlayList;
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

    public bool PlaylistAutoShuffle
    {
      get { return _playlistAutoShuffle; }
      set { _playlistAutoShuffle = value; }
    }

    #region External Player Event Handlers
    private void onStartExternal(Process proc, bool waitForExit)
    {
      // If we were listening for external player events
      if (listenToExternalPlayerEvents)
      {
        logger.Info("Playback Started in External Player");
      }
    }

    private void onStopExternal(Process proc, bool waitForExit)
    {
      if (!listenToExternalPlayerEvents)
        return;

      logger.Info("Playback Stopped in External Player");
      SetAsWatched();
      // Gram current video that just played
      PlayListItem item = GetCurrentItem();
      // Grab the video playtime
      TimeSpan tt = TimeSpan.Parse(item.Track.PlayTime);
      //Grab how long the extral player was playing
      TimeSpan rt = proc.ExitTime - proc.StartTime;

      DateTime trackPlayTime = new DateTime(tt.Ticks);
      DateTime playerRuntime = new DateTime(rt.Ticks);

      // if the track playtime and the runtime are within 3 seconds then assume playing ended
      // if outside this range assume stop was pressed 
      logger.Debug("External player stopped - differance between player runtime and track run time is {0}",Math.Abs((int)(playerRuntime - trackPlayTime).TotalSeconds));
      if (Math.Abs((int)(playerRuntime - trackPlayTime).TotalSeconds) < 3)
      {
        externalPlayerStopped = false;
        PlayNext();
      }


      if (!mvPlayer.Playing)
      {
        mvPlayer.Release();
        externalPlayerStopped = true;
        // Clear focus when playback ended
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS, 0, 0, 0, -1, 0, null);
        GUIGraphicsContext.SendMessage(msg);
      }
    }
    #endregion

  }
}
