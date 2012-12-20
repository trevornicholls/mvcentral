using System.Windows.Forms;
using System.IO;
using System;
using System.Linq;
using NLog;
using System.Collections.Generic;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using mvCentral.Database;
using mvCentral.Playlist;
using mvCentral.Utils;
using mvCentral.Localizations;


namespace mvCentral.GUI
{
  public partial class mvGUIMain : WindowPluginBaseMVC
  {

    #region Enums

    private enum SmartMode
    {
      Favourites = 0,
      FreshTracks = 1,
      HighestRated = 2,
      RandomHD = 3,
      LeastPlayed = 4,
      ByTag = 5,
      ByGenre = 6,
      SmartDJ = 7,
      Cancel = 8
    }

    #endregion

    #region playlist
    /// <summary>
    /// clear the current playlist
    /// </summary>
    private void clearPlaylist()
    {
      PlayList playlist = Player.playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MVCENTRAL);
      playlist.Clear();
    }
    /// <summary>
    /// Adds a list of Music Videos to a playlist, or a list of artists Music Videos
    /// </summary>
    /// <param name="items"></param>
    /// <param name="playNow"></param>
    /// <param name="clear"></param>
    private void addToPlaylist(List<DBTrackInfo> items, bool playNow, bool clear, bool shuffle)
    {
      PlayList playlist = Player.playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MVCENTRAL);
      if (clear)
      {
        playlist.Clear();
      }
      foreach (DBTrackInfo video in items)
      {
        PlayListItem p1 = new PlayListItem(video);
        p1.Track = video;
        p1.FileName = video.LocalMedia[0].File.FullName;
        playlist.Add(p1);
      }
      Player.playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MVCENTRAL;
      if (shuffle)
      {
        playlist.Shuffle();
      }
      if (playNow)
      {
        Player.playlistPlayer.Play(0);
        if (mvCentralCore.Settings.AutoFullscreen)
          GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
      }
    }
    /// <summary>
    /// Add selected item to end of Playlist
    /// </summary>
    /// <param name="listItem"></param>
    private void addToPlaylistNext(GUIListItem listItem)
    {
      PlayList playlist = Player.playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MVCENTRAL);
      PlayListItem item = new PlayListItem(listItem.TVTag as DBTrackInfo);
      playlist.Insert(item, Player.playlistPlayer.CurrentItem);
    }
    /// <summary>
    /// Create a Random Playlist of All Videos
    /// </summary>
    private void playRandomAll()
    {
      PlayList playlist = Player.playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MVCENTRAL);
      playlist.Clear();
      List<DBTrackInfo> videos = DBTrackInfo.GetAll();
      foreach (DBTrackInfo video in videos)
      {
        playlist.Add(new PlayListItem(video));
      }
      Player.playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MVCENTRAL;
      playlist.Shuffle();
      Player.playlistPlayer.Play(0);
      if (mvCentralCore.Settings.AutoFullscreen)
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
    }
    /// <summary>
    /// Create a Random Playlist of All HD Videos
    /// </summary>
    private void playRandomHDAll()
    {
      PlayList playlist = Player.playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MVCENTRAL);
      playlist.Clear();
      List<DBTrackInfo> videos = DBTrackInfo.GetAll();
      foreach (DBTrackInfo video in videos)
      {
        DBLocalMedia mediaInfo = (DBLocalMedia)video.LocalMedia[0];
        if (mediaInfo.VideoResolution.StartsWith("1080") || mediaInfo.VideoResolution.StartsWith("720") || mediaInfo.VideoResolution.Equals("HD", StringComparison.OrdinalIgnoreCase))
          playlist.Add(new PlayListItem(video));
      }
      Player.playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MVCENTRAL;
      playlist.Shuffle();
      Player.playlistPlayer.Play(0);
      if (mvCentralCore.Settings.AutoFullscreen)
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
    }
    /// <summary>
    /// Select Smart Playlist
    /// </summary>
    /// <returns></returns>
    private SmartMode ChooseSmartPlay()
    {
      GUIDialogMenu dlgMenu = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlgMenu != null)
      {
        dlgMenu.Reset();
        dlgMenu.SetHeading(mvCentralUtils.PluginName() + " - " + Localization.SmartPlaylistTag);
        if (this.facadeLayout.Count > 0)
        {
          dlgMenu.Add(Localization.FavouriteVideos);
          dlgMenu.Add(Localization.LatestVideos);
          dlgMenu.Add(Localization.HighestRated);
          dlgMenu.Add(Localization.RandomHD);
          dlgMenu.Add(Localization.LeastPlayed);
          dlgMenu.Add(Localization.PlayByTag);
          dlgMenu.Add(Localization.PlayByGenre);
          dlgMenu.Add("Smart DJ");
        }
        dlgMenu.DoModal(GetID);

        if (dlgMenu.SelectedLabel == -1) // Nothing was selected
          return SmartMode.Cancel;

        return (SmartMode)Enum.Parse(typeof(SmartMode), dlgMenu.SelectedLabel.ToString());
      }
      return SmartMode.Cancel;
    }
    /// <summary>
    /// Create the Smart Playlist
    /// </summary>
    /// <param name="mode"></param>
    private void playSmart(SmartMode mode)
    {
      switch (mode)
      {
        case SmartMode.Favourites:
          playFavourites();
          break;
        case SmartMode.FreshTracks:
          playFreshTracks();
          break;
        case SmartMode.HighestRated:
          playHighestRated();
          break;
        case SmartMode.LeastPlayed:
          playLeastPlayed();
          break;
        case SmartMode.RandomHD:
          playRandomHDAll();
          break;
        case SmartMode.ByTag:
          playByTag();
          break;
        case SmartMode.ByGenre:
          playByGenre();
          break;
        case SmartMode.SmartDJ:
          SmartDJPlaylist();
          break;
        case SmartMode.Cancel:
          break;
      }
    }
    /// <summary>
    /// Play tracks by selected Genre
    /// </summary>
    private void playByGenre()
    {
      GUIDialogMenu dlgMenu = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlgMenu != null)
      {
        dlgMenu.Reset();
        dlgMenu.SetHeading(mvCentralUtils.PluginName() + " - " + Localization.SmartPlaylistOptions);

        List<DBGenres> genreList = DBGenres.GetAll();
        genreList.Sort(delegate(DBGenres p1, DBGenres p2) { return p1.Genre.CompareTo(p2.Genre); });

        foreach (DBGenres genre in genreList)
        {
          if (genre.Enabled)
            dlgMenu.Add(genre.Genre);
        }
        dlgMenu.DoModal(GetID);

        if (dlgMenu.SelectedLabel == -1) // Nothing was selected
          return;

        //dlgMenu.SelectedLabelText
        PlayList playlist = Player.playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MVCENTRAL);
        playlist.Clear();
        List<DBArtistInfo> allArtists = DBArtistInfo.GetAll();

        foreach (DBArtistInfo artist in allArtists)
        {
          if (tagMatched(dlgMenu.SelectedLabelText, artist))
          {
            logger.Debug("Matched Artist {0} with Tag {1}", artist.Artist, dlgMenu.SelectedLabelText);
            List<DBTrackInfo> theTracks = DBTrackInfo.GetEntriesByArtist(artist);
            foreach (DBTrackInfo artistTrack in theTracks)
            {
              playlist.Add(new PlayListItem(artistTrack));
            }
          }
        }
        Player.playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MVCENTRAL;
        
        if (mvCentralCore.Settings.GeneratedPlaylistAutoShuffle)
          playlist.Shuffle();

        Player.playlistPlayer.Play(0);
        if (mvCentralCore.Settings.AutoFullscreen)
          GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
      }
    }
    /// <summary>
    /// Play by Selected Tag
    /// </summary>
    private void playByTag()
    {
      GUIDialogMenu dlgMenu = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlgMenu != null)
      {
        dlgMenu.Reset();
        dlgMenu.SetHeading(mvCentralUtils.PluginName() + " - " + Localization.SmartPlaylistOptions);
        foreach (string artistTag in artistTags)
        {
          dlgMenu.Add(artistTag);
        }
        dlgMenu.DoModal(GetID);

        if (dlgMenu.SelectedLabel == -1) // Nothing was selected
          return;

        //dlgMenu.SelectedLabelText
        PlayList playlist = Player.playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MVCENTRAL);
        playlist.Clear();
        List<DBArtistInfo> allArtists = DBArtistInfo.GetAll();
        foreach (DBArtistInfo artist in allArtists)
        {
          if (artist.Tag.Contains(dlgMenu.SelectedLabelText))
          {
            List<DBTrackInfo> theTracks = DBTrackInfo.GetEntriesByArtist(artist);
            foreach (DBTrackInfo artistTrack in theTracks)
            {
              playlist.Add(new PlayListItem(artistTrack));
            }
          }
        }
        Player.playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MVCENTRAL;
        playlist.Shuffle();
        Player.playlistPlayer.Play(0);
        if (mvCentralCore.Settings.AutoFullscreen)
          GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
      }
    }
    /// <summary>
    /// Play favourites (ie. Most Played)
    /// </summary>
    private void playFavourites()
    {
      PlayList playlist = Player.playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MVCENTRAL);
      playlist.Clear();
      List<DBTrackInfo> videos = DBTrackInfo.GetAll();
      // Sort Most played first
      videos.Sort(delegate(DBTrackInfo p1, DBTrackInfo p2) { return p2.UserSettings[0].WatchedCount.CompareTo(p1.UserSettings[0].WatchedCount); });
      // Now add to the list
      foreach (DBTrackInfo video in videos)
      {
        playlist.Add(new PlayListItem(video));
      }
      Player.playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MVCENTRAL;
      Player.playlistPlayer.Play(0);
      if (mvCentralCore.Settings.AutoFullscreen)
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
    }
    /// <summary>
    /// Play Tracks that have been added in the past X Days
    /// </summary>
    private void playFreshTracks()
    {
      PlayList playlist = Player.playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MVCENTRAL);
      playlist.Clear();
      List<DBTrackInfo> videos = DBTrackInfo.GetAll();

      videos.RemoveAll(video => video.DateAdded < DateTime.Now.Subtract(new TimeSpan(mvCentralCore.Settings.OldAFterDays, 0, 0, 0, 0)));

      foreach (DBTrackInfo video in videos)
      {
        playlist.Add(new PlayListItem(video));
      }
      Player.playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MVCENTRAL;
      playlist.Shuffle();
      Player.playlistPlayer.Play(0);
      if (mvCentralCore.Settings.AutoFullscreen)
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
    }
    /// <summary>
    /// Play tracks with the highest rating first
    /// </summary>
    private void playHighestRated()
    {
      PlayList playlist = Player.playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MVCENTRAL);
      playlist.Clear();
      List<DBTrackInfo> videos = DBTrackInfo.GetAll();
      // Sort Most played first
      videos.Sort(delegate(DBTrackInfo p1, DBTrackInfo p2) { return p2.Rating.CompareTo(p1.Rating); });
      // Now add to the list
      foreach (DBTrackInfo video in videos)
      {
        playlist.Add(new PlayListItem(video));
      }
      Player.playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MVCENTRAL;
      Player.playlistPlayer.Play(0);
      if (mvCentralCore.Settings.AutoFullscreen)
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
    }
    /// <summary>
    /// Playlist startingh with the least played videos
    /// </summary>
    private void playLeastPlayed()
    {
      PlayList playlist = Player.playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MVCENTRAL);
      playlist.Clear();
      List<DBTrackInfo> videos = DBTrackInfo.GetAll();
      // Sort Most played first
      videos.Sort(delegate(DBTrackInfo p1, DBTrackInfo p2) { return p1.UserSettings[0].WatchedCount.CompareTo(p2.UserSettings[0].WatchedCount); });
      // Now add to the list
      foreach (DBTrackInfo video in videos)
      {
        playlist.Add(new PlayListItem(video));
      }
      Player.playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MVCENTRAL;
      Player.playlistPlayer.Play(0);
      if (mvCentralCore.Settings.AutoFullscreen)
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
    }
    /// <summary>
    /// Create SmartDJ Playlist
    /// </summary>
    private void SmartDJPlaylist()
    {
      GUIWindowManager.ActivateWindow(GUISmartDJ.GetWindowId());
    }


    #endregion

  }
}
