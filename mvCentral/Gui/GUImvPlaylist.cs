#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections;
using System.IO;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Util;
using MediaPortal.Video.Database;
using Action = MediaPortal.GUI.Library.Action;
using Layout = MediaPortal.GUI.Library.GUIFacadeControl.Layout;
using MediaPortal.Video;
using MediaPortal.GUI.Video;
using WindowPlugins;
using mvCentral.Database;

//namespace MediaPortal.GUI.Video
namespace mvCentral.GUI
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public class mvGUIVideoPlayList :  GUIVideoBaseWindow
  {
    #region Base variabeles

//    private DirectoryHistory m_history = new DirectoryHistory();
    private string currentFolder = string.Empty;
    private int currentSelectedItem = -1;
    private int previousControlId = 0;
    private int m_nTempPlayListWindow = 0;
//    private string m_strTempPlayListDirectory = string.Empty;
//    private VirtualDirectory m_directory = new VirtualDirectory();
    //PlayListPlayer playlistPlayer;
    private mvGUIMain _gui;

    #endregion

    [SkinControl(20)] protected GUIButtonControl btnShuffle = null;
    [SkinControl(21)] protected GUIButtonControl btnSave = null;
    [SkinControl(22)] protected GUIButtonControl btnClear = null;
    [SkinControl(23)] protected GUIButtonControl btnPlay = null;
    [SkinControl(24)] protected GUIButtonControl btnNext = null;
    [SkinControl(25)] protected GUIButtonControl btnPrevious = null;
    [SkinControl(30)] protected GUIToggleButtonControl btnRepeatPlaylist = null;


    public mvGUIVideoPlayList()
    {
        GetID =  (int)Window.WINDOW_VIDEO_PLAYLIST;
        GetID = 88888;
//        playlistPlayer = PlayListPlayer.SingletonPlayer;
//        PlaylistPlayer
//        playlistPlayer = _gui.play1.listPlayer;
//      m_directory.AddDrives();
//      m_directory.SetExtensions(MediaPortal.Util.Utils.VideoExtensions);
//      m_directory.AddExtension(".m3u");
    }

    public override bool Init()
    {
      currentFolder = Directory.GetCurrentDirectory();
      return Load(GUIGraphicsContext.Skin + @"\mvplaylist.xml");
    }


    /// <summary>
    /// MediaPortal will set #currentmodule with GetModuleName()
    /// </summary>
    /// <returns>Localized Window Name</returns>
    public override string GetModuleName()
    {
        return "mvplaylist";
    }

    protected override string SerializeName
    {
        get { return "videoplaylist"; }
    }

    #region BaseWindow Members

    protected override bool AllowLayout(Layout layout)
    {
      if (layout == Layout.List)
      {
        return false;
      }
      if (layout == Layout.Filmstrip)
      {
        return false;
      }
      if (layout == Layout.CoverFlow)
      {
        return false;
      }
      return true;
    }

    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_SHOW_PLAYLIST:
          GUIWindowManager.ShowPreviousWindow();
          return;
        case Action.ActionType.ACTION_MOVE_SELECTED_ITEM_UP:
          MovePlayListItemUp();
          break;
        case Action.ActionType.ACTION_MOVE_SELECTED_ITEM_DOWN:
          MovePlayListItemDown();
          break;
        case Action.ActionType.ACTION_DELETE_SELECTED_ITEM:
          DeletePlayListItem();
          break;
          // Handle case where playlist has been stopped and we receive a player action.
          // This allows us to restart the playback proccess...
        case Action.ActionType.ACTION_MUSIC_PLAY:
        case Action.ActionType.ACTION_NEXT_ITEM:
        case Action.ActionType.ACTION_PAUSE:
        case Action.ActionType.ACTION_PREV_ITEM:
          if (playlistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_VIDEO)
          {
            playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_VIDEO;
            if (g_Player.CurrentFile == "")
            {
              PlayList playList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO);
              if (playList != null && playList.Count > 0)
              {
                playlistPlayer.Play(0);
                UpdateButtonStates();
              }
            }
          }
          break;
      }

      base.OnAction(action);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();

      _gui = (mvGUIMain)GUIWindowManager.GetWindow(mvCentralCore.PluginID);
//      playlistPlayer = _gui.play1.listPlayer;

        currentLayout = Layout.Playlist;
      facadeLayout.CurrentLayout = currentLayout;

      LoadDirectory(string.Empty);
      if (g_Player.Playing && playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_VIDEO)
      {
        int iSong = playlistPlayer.CurrentSong;
        if (iSong >= 0 && iSong <= facadeLayout.Count)
        {
          GUIControl.SelectItemControl(GetID, facadeLayout.GetID, iSong);
        }
      }
      if (facadeLayout.Count <= 0)
      {
        GUIControl.FocusControl(GetID, btnLayouts.GetID);
      }


      using (MediaPortal.Profile.Settings settings = new MediaPortal.Profile.MPSettings())
      {
        playlistPlayer.RepeatPlaylist = settings.GetValueAsBool("movies", "repeat", true);
      }

      if (btnRepeatPlaylist != null)
      {
        btnRepeatPlaylist.Selected = playlistPlayer.RepeatPlaylist;
      }
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      currentSelectedItem = facadeLayout.SelectedListItemIndex;
      using (MediaPortal.Profile.Settings settings = new MediaPortal.Profile.MPSettings())
      {
        settings.SetValueAsBool("movies", "repeat", playlistPlayer.RepeatPlaylist);
      }
      base.OnPageDestroy(newWindowId);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      if (control == btnShuffle)
      {
        OnShufflePlayList();
      }
      else if (control == btnSave)
      {
        OnSavePlayList();
      }
      else if (control == btnClear)
      {
        OnClearPlayList();
      }
      else if (control == btnPlay)
      {
        GUIListItem item = facadeLayout.SelectedListItem;
        // If the file is an image file, it should be mounted before playing
//        if (VirtualDirectory.IsImageFile(System.IO.Path.GetExtension(item.Path)))
//        {
//          if (!GUIVideoFiles.MountImageFile(GUIWindowManager.ActiveWindow, item.Path))
//            return;
//        }
        playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_VIDEO;
        playlistPlayer.Reset();
//        _gui.play1.currentPlayList = playlistPlayer.GetPlaylist(PlayListType..PLAYLIST_VIDEO);
        int itemIndex = facadeLayout.SelectedListItemIndex;
        DBTrackInfo mv = _gui.play1.currentPlayList[itemIndex].Track;
        _gui.lastItemVid = itemIndex;
        _gui.play1.Play(mv);
//        playlistPlayer.Play(facadeLayout.SelectedListItemIndex);
        UpdateButtonStates();
      }
      else if (control == btnNext)
      {
        playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_VIDEO;
        GUIVideoFiles.PlayMovieFromPlayList(true);
      }
      else if (control == btnPrevious)
      {
        playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_VIDEO;
        playlistPlayer.PlayPrevious();
      }
      else if ((btnRepeatPlaylist != null) && (control == btnRepeatPlaylist))
      {
        playlistPlayer.RepeatPlaylist = btnRepeatPlaylist.Selected;
      }
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_PLAYBACK_STOPPED:
          {
            for (int i = 0; i < facadeLayout.Count; ++i)
            {
              GUIListItem item = facadeLayout[i];
              if (item != null && item.Selected)
              {
                item.Selected = false;
                break;
              }
            }

            UpdateButtonStates();
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_PLAYLIST_CHANGED:
          {
            //	global playlist changed outside playlist window
            LoadDirectory(string.Empty);

            if (previousControlId == facadeLayout.GetID && facadeLayout.Count <= 0)
            {
              previousControlId = btnLayouts.GetID;
              GUIControl.FocusControl(GetID, previousControlId);
            }
            SelectCurrentVideo();
          }
          break;
      }
      return base.OnMessage(message);
    }

    protected override void UpdateButtonStates()
    {
      base.UpdateButtonStates();
      if (facadeLayout.Count > 0)
      {
        btnClear.Disabled = false;
        btnPlay.Disabled = false;
        if (g_Player.Playing && playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_VIDEO)
        {
          btnNext.Disabled = false;
          btnPrevious.Disabled = false;
        }
        else
        {
          btnNext.Disabled = true;
          btnPrevious.Disabled = true;
        }
      }
      else
      {
        btnClear.Disabled = true;
        btnPlay.Disabled = true;
        btnNext.Disabled = true;
        btnPrevious.Disabled = true;
      }
    }

    protected override void LoadDirectory(string strNewDirectory)
    {
      if (facadeLayout != null)
      {
        GUIWaitCursor.Show();
        try
        {
          GUIListItem SelectedItem = facadeLayout.SelectedListItem;
          if (SelectedItem != null)
          {
            if (SelectedItem.IsFolder && SelectedItem.Label != "..")
            {
//              m_history.Set(SelectedItem.Label, currentFolder);
            }
          }
          currentFolder = strNewDirectory;
          facadeLayout.Clear();

          string strObjects = string.Empty;

          ArrayList itemlist = new ArrayList();

          PlayList playlist = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO);
          /* copy playlist from general playlist*/
          int iCurrentSong = -1;
          if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_VIDEO)
          {
            iCurrentSong = playlistPlayer.CurrentSong;
          }

          string strFileName;
          for (int i = 0; i < playlist.Count; ++i)
          {
            PlayListItem item = playlist[i];
            strFileName = item.FileName;

            GUIListItem pItem = new GUIListItem(item.Description);
            pItem.Path = strFileName;
            pItem.IsFolder = false;
            pItem.MusicTag = item.MusicTag;
            //pItem.m_bIsShareOrDrive = false;

            if (item.Duration > 0)
            {
              int nDuration = item.Duration;
              if (nDuration > 0)
              {
                  string str = MediaPortal.Util.Utils.SecondsToHMSString(nDuration);
                pItem.Label2 = str;
              }
              else
              {
                pItem.Label2 = string.Empty;
              }
            }
            itemlist.Add(pItem);
            MediaPortal.Util.Utils.SetDefaultIcons(pItem);
          }

          iCurrentSong = 0;
          strFileName = string.Empty;
          //	Search current playlist item
/*          if ((m_nTempPlayListWindow == GetID && m_strTempPlayListDirectory.IndexOf(currentFolder) >= 0 &&
               g_Player.Playing
               && playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_VIDEO_TEMP)
              ||
              (GetID == (int)Window.WINDOW_VIDEO_PLAYLIST &&
               playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_VIDEO
               && g_Player.Playing))
          {
            iCurrentSong = playlistPlayer.CurrentSong;
            if (iCurrentSong >= 0)
            {
              playlist = playlistPlayer.GetPlaylist(playlistPlayer.CurrentPlaylistType);
              if (iCurrentSong < playlist.Count)
              {
                PlayListItem item = playlist[iCurrentSong];
                strFileName = item.FileName;
              }
            }
          }
            */
//          SetIMDBThumbs(itemlist);

//          string strSelectedItem = m_history.Get(currentFolder);
          int iItem = 0;
          foreach (GUIListItem item in itemlist)
          {
            facadeLayout.Add(item);

            //	synchronize playlist with current directory
            if (strFileName.Length > 0 && item.Path == strFileName)
            {
              item.Selected = true;
            }
          }
          for (int i = 0; i < facadeLayout.Count; ++i)
          {
            GUIListItem item = facadeLayout[i];
 //           if (item.Label == strSelectedItem)
            {
              GUIControl.SelectItemControl(GetID, facadeLayout.GetID, iItem);
              break;
            }
//            iItem++;
          }
          int iTotalItems = itemlist.Count;
          if (itemlist.Count > 0)
          {
            GUIListItem rootItem = (GUIListItem)itemlist[0];
            if (rootItem.Label == "..")
            {
              iTotalItems--;
            }
          }

          //set object count label
          GUIPropertyManager.SetProperty("#itemcount", MediaPortal.Util.Utils.GetObjectCountLabel(iTotalItems));

          if (currentSelectedItem >= 0)
          {
            GUIControl.SelectItemControl(GetID, facadeLayout.GetID, currentSelectedItem);
          }
          UpdateButtonStates();
          GUIWaitCursor.Hide();
        }
        catch (Exception ex)
        {
          GUIWaitCursor.Hide();
          Log.Error("GUIVideoPlaylist: An error occured while loading the directory - {0}", ex.Message);
        }
      }
    }

    #endregion

    private void ClearFileItems()
    {
      GUIControl.ClearControl(GetID, facadeLayout.GetID);
    }

    private void OnClearPlayList()
    {
      currentSelectedItem = -1;
      ClearFileItems();
      playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO).Clear();
      if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_VIDEO)
      {
        playlistPlayer.Reset();
      }
      LoadDirectory(string.Empty);
      UpdateButtonStates();
      GUIControl.FocusControl(GetID, btnLayouts.GetID);
    }

    private void SetIMDBThumbs(ArrayList items)
    {
      GUIListItem listItem;
      ArrayList movies = new ArrayList();
      for (int x = 0; x < items.Count; ++x)
      {
        listItem = (GUIListItem)items[x];
        if (listItem.IsFolder)
        {
          if (File.Exists(listItem.Path + @"\VIDEO_TS\VIDEO_TS.IFO"))
          {
            movies.Clear();
            string pathName = listItem.Path + @"\VIDEO_TS";
            VideoDatabase.GetMoviesByPath(pathName, ref movies);
            for (int i = 0; i < movies.Count; ++i)
            {
              IMDBMovie movieDetails = (IMDBMovie)movies[i];
              string fileName = "VIDEO_TS.IFO";
              if (movieDetails.File[0] == '\\' || movieDetails.File[0] == '/')
              {
                movieDetails.File = movieDetails.File.Substring(1);
              }

              if (fileName.Length > 0)
              {
                if (movieDetails.File == fileName /*|| pItem->GetLabel() == info.Title*/)
                {
                    if (MediaPortal.Util.Utils.IsDVD(listItem.Path))
                  {
                    listItem.Label = String.Format("({0}:) {1}", listItem.Path.Substring(0, 1), movieDetails.Title);
                  }
                    string coverArtImage = MediaPortal.Util.Utils.GetCoverArt(Thumbs.MovieTitle, movieDetails.Title);
                    if (MediaPortal.Util.Utils.FileExistsInCache(coverArtImage))
                  {
                    listItem.ThumbnailImage = coverArtImage;
                    listItem.IconImageBig = coverArtImage;
                    listItem.IconImage = coverArtImage;
                  }
                  // look for better thumbs
                    coverArtImage = MediaPortal.Util.Utils.ConvertToLargeCoverArt(coverArtImage);
                    if (MediaPortal.Util.Utils.FileExistsInCache(coverArtImage))
                  {
                    listItem.ThumbnailImage = coverArtImage;
                  }
                  break;
                }
              }
            }
          }
        }
      }

      movies.Clear();
      VideoDatabase.GetMoviesByPath(currentFolder, ref movies);
      for (int x = 0; x < items.Count; ++x)
      {
        listItem = (GUIListItem)items[x];
        if (!listItem.IsFolder)
        {
          IMDBMovie movieDetails = new IMDBMovie();
          int idMovie = VideoDatabase.GetMovieInfo(listItem.Path, ref movieDetails);
          if (idMovie >= 0)
          {
              string coverArtImage = MediaPortal.Util.Utils.GetCoverArt(Thumbs.MovieTitle, movieDetails.Title);
              if (MediaPortal.Util.Utils.FileExistsInCache(coverArtImage))
            {
              listItem.ThumbnailImage = coverArtImage;
              listItem.IconImageBig = coverArtImage;
              listItem.IconImage = coverArtImage;
            }
            // look for better thumbs
              coverArtImage = MediaPortal.Util.Utils.ConvertToLargeCoverArt(coverArtImage);
              if (MediaPortal.Util.Utils.FileExistsInCache(coverArtImage))
            {
              listItem.ThumbnailImage = coverArtImage;
            }
          }
          else
          {
              MediaPortal.Util.Utils.SetThumbnails(ref listItem);
          }
        }
      }
    }

    protected override void OnClick(int itemIndex)
    {
      currentSelectedItem = facadeLayout.SelectedListItemIndex;
      GUIListItem item = facadeLayout.SelectedListItem;
      if (item == null)
      {
        return;
      }
      if (item.IsFolder)
      {
        return;
      }
      // If the file is an image file, it should be mounted before playing
//      if (VirtualDirectory.IsImageFile(System.IO.Path.GetExtension(item.Path)))
//      {
//        if (!GUIVideoFiles.MountImageFile(GUIWindowManager.ActiveWindow, item.Path))
//          return;
//      }
      playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_VIDEO;
      playlistPlayer.Reset();
//      _gui.play1.currentPlayList = playlistPlayer.GetPlaylist(PlayListType..PLAYLIST_VIDEO);
      DBTrackInfo mv = (DBTrackInfo)_gui.play1.currentPlayList[itemIndex].Track;
      _gui.lastItemVid = itemIndex;
      _gui.play1.Play(mv);
//      playlistPlayer.Play(itemIndex);
    }

    protected override void OnQueueItem(int itemIndex)
    {
      RemovePlayListItem(itemIndex);
    }

    private void RemovePlayListItem(int itemIndex)
    {
      GUIListItem listItem = facadeLayout[itemIndex];
      if (listItem == null)
      {
        return;
      }
      string songFileName = listItem.Path;

      playlistPlayer.Remove(PlayListType.PLAYLIST_VIDEO, songFileName);

      LoadDirectory(currentFolder);
      UpdateButtonStates();
      GUIControl.SelectItemControl(GetID, facadeLayout.GetID, itemIndex);
      SelectCurrentVideo();
    }

    private void OnShufflePlayList()
    {
      currentSelectedItem = facadeLayout.SelectedListItemIndex;
      ClearFileItems();
      PlayList playlist = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO);

      if (playlist.Count <= 0)
      {
        return;
      }
      string currentSongFileName = string.Empty;
      if (playlistPlayer.CurrentSong >= 0)
      {
        if (g_Player.Playing && playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_VIDEO)
        {
          PlayListItem item = playlist[playlistPlayer.CurrentSong];
          currentSongFileName = item.FileName;
        }
      }
      playlist.Shuffle();
      if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_VIDEO)
      {
        playlistPlayer.Reset();
      }

      if (currentSongFileName.Length > 0)
      {
        for (int i = 0; i < playlist.Count; i++)
        {
          PlayListItem playListItem = playlist[i];
          if (playListItem.FileName == currentSongFileName)
          {
            playlistPlayer.CurrentSong = i;
          }
        }
      }

      LoadDirectory(currentFolder);
    }

    private void OnSavePlayList()
    {
      currentSelectedItem = facadeLayout.SelectedListItemIndex;
      string playlistFileName = string.Empty;
      if (GetKeyboard(ref playlistFileName))
      {
        string playListPath = string.Empty;
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.MPSettings())
        {
          playListPath = xmlreader.GetValueAsString("movies", "playlists", string.Empty);
          playListPath = MediaPortal.Util.Utils.RemoveTrailingSlash(playListPath);
        }

        string fullPlayListPath = Path.GetFileNameWithoutExtension(playlistFileName);

        fullPlayListPath += ".m3u";
        if (playListPath.Length != 0)
        {
          fullPlayListPath = playListPath + @"\" + fullPlayListPath;
        }
        PlayList playlist = new PlayList();
        for (int i = 0; i < facadeLayout.Count; ++i)
        {
          GUIListItem listItem = facadeLayout[i];
          PlayListItem playListItem = new PlayListItem();
          playListItem.FileName = listItem.Path;
          playListItem.Description = listItem.Label;
          playListItem.Duration = listItem.Duration;
          playListItem.Type = PlayListItem.PlayListItemType.Video;
          playListItem.MusicTag = listItem.MusicTag;
          playlist.Add(playListItem);
        }
        PlayListM3uIO saver = new PlayListM3uIO();
        saver.Save(playlist, fullPlayListPath);
      }
    }

    private void SelectCurrentVideo()
    {
      if (g_Player.Playing && playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_VIDEO)
      {
        int currentSongIndex = playlistPlayer.CurrentSong;
        if (currentSongIndex >= 0 && currentSongIndex <= facadeLayout.Count)
        {
          GUIControl.SelectItemControl(GetID, facadeLayout.GetID, currentSongIndex);
        }
      }
    }

    private void MovePlayListItemUp()
    {
      if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_NONE)
      {
        playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_VIDEO;
      }

      if (playlistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_VIDEO
          || facadeLayout.CurrentLayout != Layout.Playlist
          || facadeLayout.PlayListLayout == null)
      {
        return;
      }

      int iItem = facadeLayout.SelectedListItemIndex;

      // Prevent moving backwards past the top song in the list

      PlayList playList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO);
      playList.MovePlayListItemUp(iItem);
      int selectedIndex = facadeLayout.MoveItemUp(iItem, true);

      if (iItem == playlistPlayer.CurrentSong)
      {
        playlistPlayer.CurrentSong = selectedIndex;
      }

      facadeLayout.SelectedListItemIndex = selectedIndex;
      UpdateButtonStates();
    }

    private void MovePlayListItemDown()
    {
      if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_NONE)
      {
        playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_VIDEO;
      }

      if (playlistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_VIDEO
          || facadeLayout.CurrentLayout != Layout.Playlist
          || facadeLayout.PlayListLayout == null)
      {
        return;
      }

      int iItem = facadeLayout.SelectedListItemIndex;
      PlayList playList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO);

      // Prevent moving fowards past the last song in the list
      // as this would cause the currently playing song to scroll
      // off of the list view...

      playList.MovePlayListItemDown(iItem);
      int selectedIndex = facadeLayout.MoveItemDown(iItem, true);

      if (iItem == playlistPlayer.CurrentSong)
      {
        playlistPlayer.CurrentSong = selectedIndex;
      }

      facadeLayout.SelectedListItemIndex = selectedIndex;

      UpdateButtonStates();
    }

    private void DeletePlayListItem()
    {
      if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_NONE)
      {
        playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_VIDEO;
      }

      if (playlistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_VIDEO
          || facadeLayout.CurrentLayout != Layout.Playlist
          || facadeLayout.PlayListLayout == null)
      {
        return;
      }

      int iItem = facadeLayout.SelectedListItemIndex;

      string currentFile = g_Player.CurrentFile;
      GUIListItem item = facadeLayout[iItem];

      PlayList loPlayList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC_VIDEO);
      string strFileName = string.Empty;

      RemovePlayListItem(iItem);

      if (facadeLayout.Count == 0)
      {
        g_Player.Stop();
      }
      else
      {
        facadeLayout.PlayListLayout.SelectedListItemIndex = iItem;
      }

      UpdateButtonStates();
    }
  }
}