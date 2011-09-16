using System.Windows.Forms;
using System.IO;
using System;
using System.Linq;
using NLog;
using System.Collections.Generic;
using System.Collections;
//using MediaPortal.Playlists;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using mvCentral.Database;
using mvCentral.Playlist;
using mvCentral.Utils;
using mvCentral.Localizations;
using WindowPlugins;

namespace mvCentral.GUI
{
  public partial class mvGUIMain : WindowPluginBase
  {

    private enum SmartMode
    {
      Favourites = 0,
      FreshTracks = 1,
      HighestRated = 2,
      Random = 3,
      LeastPlayed = 4,
      ByGenre = 5,
      Cancel = 6,
    }
    #region playlist

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
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
      }
    }

    private void addToPlaylistNext(GUIListItem listItem)
    {
      PlayList playlist = Player.playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MVCENTRAL);
      PlayListItem item = new PlayListItem(listItem.TVTag as DBTrackInfo);


      playlist.Insert(item, Player.playlistPlayer.CurrentItem);
    }

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
      GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
    }

    private SmartMode ChooseSmartPlay()
    {
      GUIDialogMenu dlgMenu = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlgMenu != null)
      {
        dlgMenu.Reset();
        dlgMenu.SetHeading(mvCentralUtils.PluginName() + " - " + Localization.SmartPlaylistGenre);
        if (this.facadeLayout.Count > 0)
        {
          dlgMenu.Add(Localization.FavouriteVideos);
          dlgMenu.Add(Localization.LatestVideos);
          dlgMenu.Add(Localization.HighestRated);
          dlgMenu.Add(Localization.Random);
          dlgMenu.Add(Localization.LeastPlayed);
          dlgMenu.Add(Localization.PlayByGenre);
        }
        dlgMenu.DoModal(GetID);

        if (dlgMenu.SelectedLabel == -1) // Nothing was selected
          return SmartMode.Cancel;

        return (SmartMode)Enum.Parse(typeof(SmartMode), dlgMenu.SelectedLabel.ToString());
      }
      return SmartMode.Cancel;
    }

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
        case SmartMode.Random:
          playRandomAll();
          break;
        case SmartMode.ByGenre:
          playByGenre();
          break;
        case SmartMode.Cancel:
          break;
      }
    }

    private void playByGenre()
    {
      GUIDialogMenu dlgMenu = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlgMenu != null)
      {
        dlgMenu.Reset();
        dlgMenu.SetHeading(mvCentralUtils.PluginName() + " - " + Localization.SmartPlaylistOptions);
        foreach (string aGenre in artistGenres)
        {
          dlgMenu.Add(aGenre);
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
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
      }
    }

    private void playFavourites()
    {
      { DebugMsg("NOT IMPLEMENTED"); }
      //            string avgPlayCount = dm.Execute("SELECT AVG(playCount) FROM Videos", true).Rows[0].fields[0];
      //            int i = int.Parse(avgPlayCount.Split('.')[0]);
      //            ArrayList leastPlayed = dm.getAllVideos(i, true);
      //            addToPlaylist(leastPlayed, true, true, true);
    }

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
      GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
    }

    private void playHighestRated()
    { DebugMsg("NOT IMPLEMENTED"); }

    private void playLeastPlayed()
    {
      { DebugMsg("NOT IMPLEMENTED"); }
      //            string avgPlayCount = dm.Execute("SELECT AVG(playCount) FROM Videos", true).Rows[0].fields[0];           
      //            int i = int.Parse(avgPlayCount.Split('.')[0]);
      //            ArrayList leastPlayed = dm.getAllVideos(i, false);
      //            addToPlaylist(leastPlayed, true, true, true);
    }
    #endregion
  }
}
