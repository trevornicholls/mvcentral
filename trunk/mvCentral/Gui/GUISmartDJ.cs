using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
// MediaPortal
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Video;
using MediaPortal.Player;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.Video.Database;
using Action = MediaPortal.GUI.Library.Action;
using Layout = MediaPortal.GUI.Library.GUIFacadeControl.Layout;
using WindowPlugins;
// mvCentral
using mvCentral.Database;
using mvCentral;
using mvCentral.LocalMediaManagement;
using mvCentral.Playlist;
using mvCentral.Utils;
using mvCentral.Localizations;
using mvCentral.Extensions;
// Other
using NLog;


namespace mvCentral.GUI
{
  public class GUISmartDJ : WindowPluginBase
  {
    #region variables

    private static Logger logger = LogManager.GetCurrentClassLogger();

    const int windowID = 112015;

    List<DBArtistInfo> artistList = new List<DBArtistInfo>();
    List<DBArtistInfo> artistPlayList = new List<DBArtistInfo>();

    public string selectedGenre = "None";
    public string selectedLastFMTag = "None";
    public string selectedTone = "None";
    public string selectedStyle = "None";
    public string selectedComposer = "None";

    GUImvPlayList Player = new GUImvPlayList();

    #endregion

    #region Skin Connection

    private enum GUIControls
    {
      Genre = 30,
      LastFMTag = 31,
      Style = 32,
      Tone = 33,
      Composer = 34,
      Keyword = 35,
      MaxTime = 36,
      TotalArtists = 37,
      PlayPlaylist = 38,
      SavePlaylist = 39,
      Facade = 50
    }


    [SkinControl((int)GUIControls.Genre)] protected GUIButtonControl smartDJ_genre = null;
    [SkinControl((int)GUIControls.LastFMTag)] protected GUIButtonControl smartDJ_LastFMTag = null;
    [SkinControl((int)GUIControls.Style)] protected GUIButtonControl smartDJ_style = null;
    [SkinControl((int)GUIControls.Tone)] protected GUIButtonControl smartDJ_tone = null;
    [SkinControl((int)GUIControls.Composer)] protected GUIButtonControl smartDJ_composer = null;
    [SkinControl((int)GUIControls.Keyword)] protected GUIButtonControl smartDJ_keywork = null;
    [SkinControl((int)GUIControls.MaxTime)] protected GUISpinButton smartDJ_maxTime = null;

    [SkinControl((int)GUIControls.TotalArtists)] protected GUISpinButton smartDJ_TotalArtists = null;

    
    [SkinControl((int)GUIControls.PlayPlaylist)] protected GUIButtonControl SmartDJ_Play = null;
    [SkinControl((int)GUIControls.SavePlaylist)] protected GUIButtonControl SmartDJ_Save = null;



    #endregion

    #region Constructor

    public GUISmartDJ()
    {
    }

    #endregion

    #region Base overrides

    public override int GetID
    {
      get { return windowID; }
    }

    public override string GetModuleName()
    {
      return mvCentralCore.Settings.HomeScreenName;
    }
    /// <summary>
    /// Initilize
    /// </summary>
    /// <returns></returns>
    public override bool Init()
    {
      string xmlSkin = GUIGraphicsContext.Skin + @"\mvCentral.SmartDJ.xml";
      logger.Info("Loading main skin window: " + xmlSkin);
      return Load(xmlSkin);
    }
    /// <summary>
    /// Page loading actions
    /// </summary>
    protected override void OnPageLoad()
    {
      GUIControl.SetControlLabel(windowID, (int)GUIControls.Genre, "Genre: None");
      GUIControl.SetControlLabel(windowID, (int)GUIControls.LastFMTag, "LastFM Tag: None");
      GUIControl.SetControlLabel(windowID, (int)GUIControls.Style, "Style: None");
      GUIControl.SetControlLabel(windowID, (int)GUIControls.Tone, "Tone: None");
      GUIControl.SetControlLabel(windowID, (int)GUIControls.Composer, "Composer: None");

      artistList = DBArtistInfo.GetAll();
      facadeLayout.Visible = false;
    }
    /// <summary>
    /// Clean up
    /// </summary>
    /// <param name="new_windowId"></param>
    protected override void OnPageDestroy(int new_windowId)
    {

    }
    /// <summary>
    /// Deal with user input
    /// </summary>
    /// <param name="controlId"></param>
    /// <param name="control"></param>
    /// <param name="actionType"></param>
    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);

      switch (controlId)
      {
        case (int)GUIControls.Genre:
          selectedGenre = selectGenre();
          GUIControl.SetControlLabel(windowID, (int)GUIControls.Genre, "Genre: " + selectedGenre);
          genPlaylist(artistList);
          break;
        case (int)GUIControls.LastFMTag:
          selectedLastFMTag = selectLastFMTag();
          GUIControl.SetControlLabel(windowID, (int)GUIControls.LastFMTag, "Last FM Tag: " + selectedLastFMTag);
          genPlaylist(artistList);
          break;
        case (int)GUIControls.Tone:
          selectedTone = selectTone();
          GUIControl.SetControlLabel(windowID, (int)GUIControls.Tone, "Tone: " + selectedTone);
          genPlaylist(artistList);
          break;
        case (int)GUIControls.Style:
          selectedStyle = selectStyle();
          GUIControl.SetControlLabel(windowID, (int)GUIControls.Style, "Style: " + selectedStyle);
          genPlaylist(artistList);
          break;
        case (int)GUIControls.Composer:
          selectedComposer = selectComposer();
          GUIControl.SetControlLabel(windowID, (int)GUIControls.Composer, "Composer: " + selectedComposer);
          genPlaylist(artistList);
          break;
        case (int)GUIControls.Keyword:
          break;
        case (int)GUIControls.PlayPlaylist:
          if (facadeLayout.Count > 0)
            playSmartDJPlaylist();

          break;
        case (int)GUIControls.SavePlaylist:
            if (facadeLayout.Count > 0)
              saveSmartDJPlaylist();

          break;
        case (int)GUIControls.Facade:
          //Clicked on something in the facade
          logger.Debug("Hit Key : " + actionType.ToString());
          switch (actionType)
          {
            case Action.ActionType.ACTION_PLAY:
            case Action.ActionType.ACTION_PAUSE:
            case Action.ActionType.ACTION_QUEUE_ITEM:
            case Action.ActionType.ACTION_MUSIC_PLAY:
            case Action.ActionType.ACTION_SELECT_ITEM:

              GUIListItem selectedItem = facadeLayout.SelectedListItem;
              if (!selectedItem.IsFolder && selectedItem.MusicTag != null)
              {
                // we have a track selected so add any other tracks which are on showing on the facade
                List<DBTrackInfo> videoList = new List<DBTrackInfo>();
                for (int i = 0; i < facadeLayout.ListLayout.Count; i++)
                {
                  GUIListItem trackItem = facadeLayout.ListLayout[i];
                  if (!trackItem.IsFolder && trackItem.MusicTag != null)
                    videoList.Add((DBTrackInfo)trackItem.MusicTag);
                }
                addToPlaylist(videoList, false, true, false);
                Player.playlistPlayer.Play(videoList.IndexOf((DBTrackInfo)selectedItem.MusicTag));
                if (mvCentralCore.Settings.AutoFullscreen)
                  g_Player.ShowFullScreenWindow();
                break;
              }
              break;
          }
          break;
      }
    }
    /// <summary>
    /// Play the generated playlist
    /// </summary>
    void playSmartDJPlaylist()
    {
      GUIListItem selectedItem = facadeLayout.SelectedListItem;
      if (!selectedItem.IsFolder && selectedItem.MusicTag != null)
      {
        // we have a track selected so add any other tracks which are on showing on the facade
        List<DBTrackInfo> videoList = new List<DBTrackInfo>();
        for (int i = 0; i < facadeLayout.ListLayout.Count; i++)
        {
          GUIListItem trackItem = facadeLayout.ListLayout[i];
          if (!trackItem.IsFolder && trackItem.MusicTag != null)
            videoList.Add((DBTrackInfo)trackItem.MusicTag);
        }
        addToPlaylist(videoList, false, true, false);
        Player.playlistPlayer.Play(videoList.IndexOf((DBTrackInfo)selectedItem.MusicTag));
        if (mvCentralCore.Settings.AutoFullscreen)
          g_Player.ShowFullScreenWindow();
      }
    }
    /// <summary>
    /// Save the current playlist
    /// </summary>
    private void saveSmartDJPlaylist()
    {
      string playlistFileName = string.Empty;
      if (GetKeyboard(ref playlistFileName))
      {
        string playListPath = string.Empty;
        // Have we out own playlist folder configured
        if (!string.IsNullOrEmpty(mvCentralCore.Settings.PlayListFolder.Trim()))
          playListPath = mvCentralCore.Settings.PlayListFolder;
        else
        {
          // No, so use my videos location
          using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.MPSettings())
          {
            playListPath = xmlreader.GetValueAsString("movies", "playlists", string.Empty);
            playListPath = MediaPortal.Util.Utils.RemoveTrailingSlash(playListPath);
          }

          playListPath = MediaPortal.Util.Utils.RemoveTrailingSlash(playListPath);
        }

        // check if Playlist folder exists, create it if not
        if (!Directory.Exists(playListPath))
        {
          try
          {
            Directory.CreateDirectory(playListPath);
          }
          catch (Exception e)
          {
            logger.Info("Error: Unable to create Playlist path: " + e.Message);
            return;
          }
        }

        string fullPlayListPath = Path.GetFileNameWithoutExtension(playlistFileName);

        fullPlayListPath += ".mvplaylist";
        if (playListPath.Length != 0)
        {
          fullPlayListPath = playListPath + @"\" + fullPlayListPath;
        }
        PlayList playlist = new PlayList();
        for (int i = 0; i < facadeLayout.Count; ++i)
        {
          GUIListItem listItem = facadeLayout[i];
          PlayListItem playListItem = new PlayListItem();
          DBTrackInfo mv = (DBTrackInfo)listItem.TVTag;
          playListItem.Track = mv;
          playlist.Add(playListItem);
        }
        PlayListIO saver = new PlayListIO();
        saver.Save(playlist, fullPlayListPath);
      }
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
    /// Handle keyboard/remote action
    /// </summary>
    /// <param name="action"></param>
    public override void OnAction(MediaPortal.GUI.Library.Action action)
    {
      MediaPortal.GUI.Library.Action.ActionType wID = action.wID;

      base.OnAction(action);
    }


    #endregion

    #region Private Methods

    /// <summary>
    /// Generate a playlist based on selections
    /// </summary>
    /// <param name="workingArtistList"></param>
    /// <param name="controlID"></param>
    /// <param name="matchWith"></param>
    void genPlaylist(List<DBArtistInfo> workingArtistList)
    {
      artistPlayList.Clear();
      // Genre
      foreach (DBArtistInfo artistData in workingArtistList)
      {
        if (selectedGenre != "None")
        {
          if (artistData.Genre == selectedGenre)
            artistPlayList.Add(artistData);
        }
        // LastFM Tags
        if (selectedLastFMTag != "None")
        {
          foreach (string tag in artistData.Tag)
          {
            if (String.Equals(tag, selectedLastFMTag, StringComparison.OrdinalIgnoreCase))
              artistPlayList.Add(artistData);
          }
        }
        // Tones
        if (selectedTone != "None")
        {
          if (artistData.Tones.Contains(selectedTone, StringComparison.OrdinalIgnoreCase))
            artistPlayList.Add(artistData);
        }
        // Styles
        if (selectedStyle != "None")
        {
          if (artistData.Styles.Contains(selectedStyle, StringComparison.OrdinalIgnoreCase))
            artistPlayList.Add(artistData);
        }
      }

      // If we have an empty list but we have a composer chosen then grab all artists and filter out just tracks with that composer during the facade build
      if (artistPlayList.Count == 0 && selectedComposer != "None")
        artistPlayList = DBArtistInfo.GetAll();

      GUIControl.SetControlLabel(windowID, (int)GUIControls.TotalArtists, string.Format("Artists in Playlist: {0}", artistPlayList.Count.ToString()));
      // Clear the facade
      GUIControl.ClearControl(GetID, facadeLayout.GetID);
      // Load tracks into facade
      foreach (DBArtistInfo artist in artistPlayList)
      {
        foreach (DBTrackInfo trackData in DBTrackInfo.GetEntriesByArtist(artist))
        {
          // If filtering on composer the check the track
          if (selectedComposer != "None")
          {
            if (!trackData.Composers.Contains(selectedComposer, StringComparison.OrdinalIgnoreCase))
              continue;
          }
          GUIListItem facadeItem = new GUIListItem();
          if (mvCentralCore.Settings.DisplayRawTrackText)
          {
            if (Path.GetFileName(trackData.LocalMedia[0].File.FullName).Contains("-"))
              facadeItem.Label = Regex.Match(Path.GetFileName(trackData.LocalMedia[0].File.FullName), @"(?:[\s-:;]{2,}|(?!.+?[\s-:;]{2,})\-)(?<track>[^\\$]*)\.").Groups["track"].Value;
            else
              facadeItem.Label = (Path.GetFileNameWithoutExtension(trackData.LocalMedia[0].File.FullName));

            facadeItem.Label = Regex.Replace(facadeItem.Label, @"\s*[{].*?[}]\s*", string.Empty, RegexOptions.IgnoreCase);
          }
          else
            facadeItem.Label = trackData.Track;

          facadeItem.Label2 = trackData.ArtistInfo[0].Artist;
          facadeItem.TVTag = trackData.bioContent;
          facadeItem.Path = trackData.LocalMedia[0].File.FullName;
          facadeItem.IsFolder = false;
          //facadeItem.OnItemSelected += new GUIListItem.ItemSelectedHandler(onVideoSelected);
          facadeItem.MusicTag = trackData;
          facadeItem.Rating = trackData.Rating;
          // If no thumbnail set a default
          if (!string.IsNullOrEmpty(trackData.ArtFullPath.Trim()))
            facadeItem.ThumbnailImage = trackData.ArtFullPath;
          else
            facadeItem.ThumbnailImage = "defaultVideoBig.png";

          facadeLayout.Add(facadeItem);
        }
      }

      if (facadeLayout.Count > 0)
      {
        facadeLayout.Visible = true;
        GUIPropertyManager.SetProperty("#itemcount", facadeLayout.Count.ToString());
        GUIPropertyManager.SetProperty("#itemtype", Localization.Videos);
      }
      else
        facadeLayout.Visible = false;


    }

    /// <summary>
    /// Select Style
    /// </summary>
    /// <returns></returns>
    string selectGenre()
    {
      List<string> genreList = new List<string>();
      GUIDialogSelect dlg = (GUIDialogSelect)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_SELECT);
      if (dlg == null)
      {
        return string.Empty;
      }
      dlg.Reset();
      dlg.SetHeading("Select Genre");
      dlg.Add("None");
      foreach (DBArtistInfo artistObject in DBArtistInfo.GetAll())
      {
        if (artistObject.Genre.Trim().Length > 0)
        {
          if (!genreList.Contains(artistObject.Genre.Trim()))
          {
            genreList.Add(artistObject.Genre.Trim());
            dlg.Add(artistObject.Genre.Trim());
          }
        }
      }
      // show dialog and wait for result
      dlg.DoModal(GetID);

      return dlg.SelectedLabelText;
    }

    /// <summary>
    /// Select Style
    /// </summary>
    /// <returns></returns>
    string selectLastFMTag()
    {
      GUIDialogSelect dlg = (GUIDialogSelect)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_SELECT);
      if (dlg == null)
      {
        return string.Empty;
      }
      dlg.Reset();
      dlg.SetHeading("Select LastFM Tag");
      dlg.Add("None");     
      foreach (DBGenres lastFMTags in DBGenres.GetAll())
        dlg.Add(lastFMTags.Genre);

      // show dialog and wait for result
      dlg.DoModal(GetID);

      return dlg.SelectedLabelText;
    }

    /// <summary>
    /// Select Style
    /// </summary>
    /// <returns></returns>
    string selectStyle()
    {
      GUIDialogSelect dlg = (GUIDialogSelect)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_SELECT);
      if (dlg == null)
      {
        return string.Empty;
      }
      dlg.Reset();
      dlg.SetHeading("Select Style");
      List<string> styles = DBTonesAndStyles.GetAllStyles();
      styles.Sort(delegate(string p1, string p2) { return p1.CompareTo(p2); });
      dlg.Add("None");
      foreach (string style in styles)
        dlg.Add(style);

      // show dialog and wait for result
      dlg.DoModal(GetID);

      return dlg.SelectedLabelText;
    }

    /// <summary>
    /// Select Tone
    /// </summary>
    /// <returns></returns>
    string selectTone()
    {
      GUIDialogSelect dlg = (GUIDialogSelect)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_SELECT);
      if (dlg == null)
      {
        return string.Empty;
      }
      dlg.Reset();
      dlg.SetHeading("Select Tone");
      List<string> tones = DBTonesAndStyles.GetAllTones();
      tones.Sort(delegate(string p1, string p2) { return p1.CompareTo(p2); });
      dlg.Add("None");
      foreach (string tone in tones)
        dlg.Add(tone);

      // show dialog and wait for result
      dlg.DoModal(GetID);

      return dlg.SelectedLabelText;
    }
    /// <summary>
    /// Select Composer
    /// </summary>
    /// <returns></returns>
    string selectComposer()
    {
      GUIDialogSelect dlg = (GUIDialogSelect)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_SELECT);
      if (dlg == null)
      {
        return string.Empty;
      }
      dlg.Reset();
      dlg.SetHeading("Select Composer");
      dlg.Add("None");
      // Load and sort composers
      List<string> strComposers = new List<string>();
      foreach (DBComposers loadComposer in DBComposers.GetAll())
        strComposers.Add(loadComposer.composer);
      strComposers.Sort(delegate(string p1, string p2) { return p1.CompareTo(p2); });
      // add to dialog
      foreach (string composer in strComposers)
        dlg.Add(composer);

      // show dialog and wait for result
      dlg.DoModal(GetID);

      return dlg.SelectedLabelText;
    }


    #endregion

    #region Public Methods

    public static int GetWindowId()
    {
      return windowID;
    }

    #endregion

  }
}
