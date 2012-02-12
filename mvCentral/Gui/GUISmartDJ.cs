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

    #region Enums

    private enum SearchField
    {
      None,
      Genre,
      LastFMTages,
      Tone,
      Style,
      Composer,
      Keyword
    }

    private enum FieldSetup
    {
      Success,
      Failed,
      Clear
    }

    #endregion

    #region variables

    private static Logger logger = LogManager.GetCurrentClassLogger();

    const int windowID = 112015;

    bool matchingMode = true;

    List<DBArtistInfo> fullArtistList = new List<DBArtistInfo>();
    List<DBTrackInfo> fullVideoList = new List<DBTrackInfo>();

    List<DBArtistInfo> artistPlayList = new List<DBArtistInfo>();

    public string selectedGenre = "None";
    public string selectedLastFMTag = "None";
    public string selectedTone = "None";
    public string selectedStyle = "None";
    public string selectedComposer = "None";
    public string selectedKeyword = "None";

    public string fieldSelected1 = string.Empty;
    public string fieldSelected2 = string.Empty;
    public string fieldSelected3 = string.Empty;
    public string fieldSelected4 = string.Empty;
    public string fieldSelected5 = string.Empty;
    public string fieldSelected6 = string.Empty;

    public string customSearchStr1 = string.Empty;
    public string customSearchStr2 = string.Empty;
    public string customSearchStr3 = string.Empty;
    public string customSearchStr4 = string.Empty;
    public string customSearchStr5 = string.Empty;
    public string customSearchStr6 = string.Empty;

    public bool genreAdded = false;
    public bool tagAdded = false;
    public bool toneAdded = false;
    public bool styleAdded = false;
    public bool composerAdded = false;
    public bool keywordAdded = false;

    GUImvPlayList Player = new GUImvPlayList();

    #endregion

    #region Skin Connection

    private enum GUIControls
    {
      smartMode = 20,
      PlayPlaylist = 21,
      SavePlaylist = 22,
      Genre = 30,
      LastFMTag = 31,
      Style = 32,
      Tone = 33,
      Composer = 34,
      Keyword = 35,
      MaxTime = 36,
      TotalArtists = 37,
      Facade = 50,
      Custom1 = 130,
      Custom2 = 131,
      Custom3 = 132,
      Custom4 = 133,
      Custom5 = 134,
      Custom6 = 135
    }



    [SkinControl((int)GUIControls.smartMode)] protected GUIButtonControl smartDJ_SmartMode = null;


    [SkinControl((int)GUIControls.Genre)] protected GUIButtonControl smartDJ_genre = null;
    [SkinControl((int)GUIControls.LastFMTag)] protected GUIButtonControl smartDJ_LastFMTag = null;
    [SkinControl((int)GUIControls.Style)] protected GUIButtonControl smartDJ_style = null;
    [SkinControl((int)GUIControls.Tone)] protected GUIButtonControl smartDJ_tone = null;
    [SkinControl((int)GUIControls.Composer)] protected GUIButtonControl smartDJ_composer = null;
    [SkinControl((int)GUIControls.Keyword)] protected GUIButtonControl smartDJ_keywork = null;
    [SkinControl((int)GUIControls.MaxTime)] protected GUISpinButton smartDJ_maxTime = null;

    [SkinControl((int)GUIControls.TotalArtists)] protected GUILabelControl smartDJ_TotalArtists = null;

    
    [SkinControl((int)GUIControls.PlayPlaylist)] protected GUIButtonControl SmartDJ_Play = null;
    [SkinControl((int)GUIControls.SavePlaylist)] protected GUIButtonControl SmartDJ_Save = null;

    [SkinControl((int)GUIControls.Custom1)] protected GUIButtonControl smartDJ_Custom1 = null;
    [SkinControl((int)GUIControls.Custom2)] protected GUIButtonControl smartDJ_Custom2 = null;
    [SkinControl((int)GUIControls.Custom3)] protected GUIButtonControl smartDJ_Custom3 = null;
    [SkinControl((int)GUIControls.Custom4)] protected GUIButtonControl smartDJ_Custom4 = null;
    [SkinControl((int)GUIControls.Custom5)] protected GUIButtonControl smartDJ_Custom5 = null;
    [SkinControl((int)GUIControls.Custom6)] protected GUIButtonControl smartDJ_Custom6 = null;


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
      initValues();
      fullArtistList = DBArtistInfo.GetAll();
      fullVideoList = DBTrackInfo.GetAll();
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
      FieldSetup result;

      switch (controlId)
      {
        case (int)GUIControls.smartMode:
          if (matchingMode)
          {
            matchingMode = false;
            GUIControl.SetControlLabel(windowID, (int)GUIControls.smartMode, "Mode: Filter");
            GUIPropertyManager.SetProperty("#mvCentral.MatchMode", "false");
            GUIControl.EnableControl(windowID, (int)GUIControls.Custom1);
            GUIControl.DisableControl(windowID, (int)GUIControls.Custom2);
            GUIControl.DisableControl(windowID, (int)GUIControls.Custom3);
            GUIControl.DisableControl(windowID, (int)GUIControls.Custom4);
            GUIControl.DisableControl(windowID, (int)GUIControls.Custom5);
            GUIControl.DisableControl(windowID, (int)GUIControls.Custom6);

          }
          else
          {
            matchingMode = true;
            GUIControl.SetControlLabel(windowID, (int)GUIControls.smartMode, "Mode: Match");
            GUIPropertyManager.SetProperty("#mvCentral.MatchMode", "true");
          }
          break;

        case (int)GUIControls.Genre:
          selectedGenre = selectGenre();
          GUIControl.SetControlLabel(windowID, (int)GUIControls.Genre, "Genre: " + selectedGenre);
          genPlaylist(fullArtistList);
          break;
        case (int)GUIControls.LastFMTag:
          selectedLastFMTag = selectLastFMTag();
          GUIControl.SetControlLabel(windowID, (int)GUIControls.LastFMTag, "Last FM Tag: " + selectedLastFMTag);
          genPlaylist(fullArtistList);
          break;
        case (int)GUIControls.Tone:
          selectedTone = selectTone();
          GUIControl.SetControlLabel(windowID, (int)GUIControls.Tone, "Tone: " + selectedTone);
          genPlaylist(fullArtistList);
          break;
        case (int)GUIControls.Style:
          selectedStyle = selectStyle();
          GUIControl.SetControlLabel(windowID, (int)GUIControls.Style, "Style: " + selectedStyle);
          genPlaylist(fullArtistList);
          break;
        case (int)GUIControls.Composer:
          selectedComposer = selectComposer();
          GUIControl.SetControlLabel(windowID, (int)GUIControls.Composer, "Composer: " + selectedComposer);
          genPlaylist(fullArtistList);
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
        case (int)GUIControls.Custom1:
          result = getFieldAndSearchValue(ref fieldSelected1, ref customSearchStr1, GUIControls.Custom1);
          if (result == FieldSetup.Success)
            GUIControl.EnableControl(windowID, (int)GUIControls.Custom2);

          if (result == FieldSetup.Clear)
            cleanAndReset(GUIControls.Custom1);

          break;
        case (int)GUIControls.Custom2:
          result = getFieldAndSearchValue(ref fieldSelected2, ref customSearchStr2, GUIControls.Custom2);
          if (result == FieldSetup.Success)
            GUIControl.EnableControl(windowID, (int)GUIControls.Custom3);        

          if (result == FieldSetup.Clear)
            cleanAndReset(GUIControls.Custom2);

          break;
        case (int)GUIControls.Custom3:
          result = getFieldAndSearchValue(ref fieldSelected3, ref customSearchStr3, GUIControls.Custom3);
          if (result == FieldSetup.Success)
            GUIControl.EnableControl(windowID, (int)GUIControls.Custom4);

          if (result == FieldSetup.Clear)
            cleanAndReset(GUIControls.Custom3);

          break;
        case (int)GUIControls.Custom4:
          result = getFieldAndSearchValue(ref fieldSelected4, ref customSearchStr4, GUIControls.Custom4);
          if (result == FieldSetup.Success)
            GUIControl.EnableControl(windowID, (int)GUIControls.Custom5);

          if (result == FieldSetup.Clear)
            cleanAndReset(GUIControls.Custom4);

          break;
        case (int)GUIControls.Custom5:
          result = getFieldAndSearchValue(ref fieldSelected5, ref customSearchStr5, GUIControls.Custom5);
          if (result == FieldSetup.Success)
            GUIControl.EnableControl(windowID, (int)GUIControls.Custom6);

          if (result == FieldSetup.Clear)
            cleanAndReset(GUIControls.Custom5);

          break;
        case (int)GUIControls.Custom6:
          result = getFieldAndSearchValue(ref fieldSelected6, ref customSearchStr6, GUIControls.Custom6);

          if (result == FieldSetup.Clear)
            cleanAndReset(GUIControls.Custom6);

          break;
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
    /// Remove the field and recalculate the playlist
    /// </summary>
    /// <param name="controlID"></param>
    private void cleanAndReset(GUIControls controlID)
    {
      // Shuffle up the custom fields
      if (controlID == GUIControls.Custom1)
      {
        fieldSelected1 = fieldSelected2;
        customSearchStr1 = customSearchStr2;

        fieldSelected2 = fieldSelected3;
        customSearchStr2 = customSearchStr3;

        fieldSelected3 = fieldSelected4;
        customSearchStr3 = customSearchStr4;

        fieldSelected4 = fieldSelected5;
        customSearchStr4 = customSearchStr5;

        fieldSelected5 = fieldSelected6;
        customSearchStr5 = customSearchStr6;

      }
      else if (controlID == GUIControls.Custom2)
      {
        fieldSelected2 = fieldSelected3;
        customSearchStr2 = customSearchStr3;
        fieldSelected3 = fieldSelected4;
        customSearchStr3 = customSearchStr4;
        fieldSelected4 = fieldSelected5;
        customSearchStr4 = customSearchStr5;
        fieldSelected5 = fieldSelected6;
        customSearchStr5 = customSearchStr6;
      }
      else if (controlID == GUIControls.Custom3)
      {
        fieldSelected3 = fieldSelected4;
        customSearchStr3 = customSearchStr4;
        fieldSelected4 = fieldSelected5;
        customSearchStr4 = customSearchStr5;
        fieldSelected5 = fieldSelected6;
        customSearchStr5 = customSearchStr6;
      }
      else if (controlID == GUIControls.Custom4)
      {
        fieldSelected4 = fieldSelected5;
        customSearchStr4 = customSearchStr5;
        fieldSelected5 = fieldSelected6;
        customSearchStr5 = customSearchStr6;
      }
      else if (controlID == GUIControls.Custom5)
      {
        fieldSelected5 = fieldSelected6;
        customSearchStr5 = customSearchStr6;
      }
      else if (controlID == GUIControls.Custom6)
      {
        fieldSelected5 = string.Empty;
        customSearchStr5 = string.Empty;
      }

      // Reset the enabled stauts of the controls
      if (fieldSelected2 == string.Empty)
      {
        GUIControl.DisableControl(windowID, (int)GUIControls.Custom2);
        GUIControl.SetControlLabel(windowID, (int)GUIControls.Custom2, "Select Fiter Field..");
      }
      if (fieldSelected3 == string.Empty)
      {
        GUIControl.DisableControl(windowID, (int)GUIControls.Custom3);
        GUIControl.SetControlLabel(windowID, (int)GUIControls.Custom3, "Select Fiter Field..");
      }
      if (fieldSelected4 == string.Empty)
      {
        GUIControl.DisableControl(windowID, (int)GUIControls.Custom4);
        GUIControl.SetControlLabel(windowID, (int)GUIControls.Custom4, "Select Fiter Field..");
      }
      if (fieldSelected5 == string.Empty)
      {
        GUIControl.DisableControl(windowID, (int)GUIControls.Custom5);
        GUIControl.SetControlLabel(windowID, (int)GUIControls.Custom5, "Select Fiter Field..");
      }
      if (fieldSelected6 == string.Empty)
      {
        GUIControl.DisableControl(windowID, (int)GUIControls.Custom6);
        GUIControl.SetControlLabel(windowID, (int)GUIControls.Custom6, "Select Fiter Field..");
      }


      //Reload the playlist
      artistPlayList.Clear();
      if (fieldSelected1 != string.Empty)
      {
        GUIControl.SetControlLabel(windowID, (int)GUIControls.Custom1, string.Format("Field: {0} ({1})", fieldSelected1, customSearchStr1));
        GUIControl.EnableControl(windowID, (int)GUIControls.Custom2);
        genFilterPlaylist(getFieldType(fieldSelected1), customSearchStr1);
      }

      if (fieldSelected2 != string.Empty)
      {
        GUIControl.SetControlLabel(windowID, (int)GUIControls.Custom2, string.Format("Field: {0} ({1})", fieldSelected2, customSearchStr2));
        GUIControl.EnableControl(windowID, (int)GUIControls.Custom3);
        genFilterPlaylist(getFieldType(fieldSelected2), customSearchStr2);
      }

      if (fieldSelected3 != string.Empty)
      {
        GUIControl.SetControlLabel(windowID, (int)GUIControls.Custom3, string.Format("Field: {0} ({1})", fieldSelected3, customSearchStr3));
        GUIControl.EnableControl(windowID, (int)GUIControls.Custom4);
        genFilterPlaylist(getFieldType(fieldSelected3), customSearchStr3);
      }

      if (fieldSelected4 != string.Empty)
      {
        GUIControl.SetControlLabel(windowID, (int)GUIControls.Custom4, string.Format("Field: {0} ({1})", fieldSelected4, customSearchStr4));
        GUIControl.EnableControl(windowID, (int)GUIControls.Custom5);
        genFilterPlaylist(getFieldType(fieldSelected4), customSearchStr4);
      }

      if (fieldSelected5 != string.Empty)
      {
        GUIControl.SetControlLabel(windowID, (int)GUIControls.Custom5, string.Format("Field: {0} ({1})", fieldSelected5, customSearchStr5));
        GUIControl.EnableControl(windowID, (int)GUIControls.Custom6);
        genFilterPlaylist(getFieldType(fieldSelected5), customSearchStr5);
      }

      if (fieldSelected6 != string.Empty)
      {
        GUIControl.SetControlLabel(windowID, (int)GUIControls.Custom6, string.Format("Field: {0} ({1})", fieldSelected6, customSearchStr6));
        genFilterPlaylist(getFieldType(fieldSelected6), customSearchStr6);
      }

    }

    /// <summary>
    /// Get the search field and search value
    /// </summary>
    /// <param name="fieldName"></param>
    /// <param name="searchValue"></param>
    /// <param name="controlID"></param>
    private FieldSetup getFieldAndSearchValue(ref string fieldName, ref string searchValue, GUIControls controlID)
    {
      SearchField thisField = SearchField.None;

      if (string.IsNullOrEmpty(fieldName))
      {
        fieldName = setFilterField();
        GUIControl.SetControlLabel(windowID, (int)controlID, "Field: " + fieldName);
      }
      searchValue = filterValue(fieldName, out thisField);
      if (searchValue.Equals("None", StringComparison.OrdinalIgnoreCase))
      {
        fieldName = string.Empty;
        searchValue = string.Empty;
        return FieldSetup.Clear;
      }

      GUIControl.SetControlLabel(windowID, (int)controlID, string.Format("Field: {0} ({1})", fieldName, searchValue));
      genFilterPlaylist(thisField, searchValue);
      return FieldSetup.Success;

    }

    /// <summary>
    /// Grab the filter value for the chosen field
    /// </summary>
    /// <param name="fieldName"></param>
    /// <param name="thisField"></param>
    /// <returns></returns>
    string filterValue(string fieldName, out SearchField thisField)
    {
      string strRetVal = string.Empty;
      thisField = SearchField.None;

      switch (fieldName.ToLower())
      {
        case "genre":
          thisField = SearchField.Genre;
          strRetVal = selectGenre();
          genreAdded = true;
          break;
        case "lastfmtag":
          thisField = SearchField.LastFMTages;
          strRetVal = selectLastFMTag();
          tagAdded = true;
          break;
        case "style":
          thisField = SearchField.Style;
          strRetVal = selectStyle();
          styleAdded = true;
          break;
        case "tone":
          thisField = SearchField.Tone;
          strRetVal = selectTone();
          toneAdded = true;
          break;
        case "composer":
          thisField = SearchField.Composer;
          strRetVal = selectComposer();
          break;
        case "keyword":
          thisField = SearchField.Keyword;
          strRetVal = selectGenre();
          keywordAdded = true;
          break;
      }
      return strRetVal;
    }
    /// <summary>
    /// Request the field from the user
    /// </summary>
    /// <returns></returns>
    private string setFilterField()
    {
      GUIDialogSelect dlg = (GUIDialogSelect)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_SELECT);
      if (dlg == null)
      {
        return string.Empty;
      }
      dlg.Reset();
      dlg.SetHeading("Select Filter Field");
      if (!fieldChosen("Genre"))
        dlg.Add("Genre");
      if (!fieldChosen("LastFMTag"))
        dlg.Add("LastFMTag");
      if (!fieldChosen("Style"))
        dlg.Add("Style");
      if (!fieldChosen("Tone"))
        dlg.Add("Tone");
      if (!fieldChosen("Composer"))
        dlg.Add("Composer");
      if (!fieldChosen("Keyword"))
        dlg.Add("Keyword");
      // show dialog and wait for result
      dlg.DoModal(GetID);

      return dlg.SelectedLabelText;
    }
    /// <summary>
    /// Check if we have the already selected the field
    /// </summary>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    private bool fieldChosen(string fieldName)
    {
      if (string.Equals(fieldSelected1, fieldName, StringComparison.OrdinalIgnoreCase))
        return true;
      if (string.Equals(fieldSelected2, fieldName, StringComparison.OrdinalIgnoreCase))
        return true;
      if (string.Equals(fieldSelected3, fieldName, StringComparison.OrdinalIgnoreCase))
        return true;
      if (string.Equals(fieldSelected4, fieldName, StringComparison.OrdinalIgnoreCase))
        return true;
      if (string.Equals(fieldSelected5, fieldName, StringComparison.OrdinalIgnoreCase))
        return true;
      if (string.Equals(fieldSelected6, fieldName, StringComparison.OrdinalIgnoreCase))
        return true;

      return false;

    }
    /// <summary>
    /// Play the generated playlist
    /// </summary>
    private void playSmartDJPlaylist()
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


    SearchField getFieldType(string fieldName)
    {
      if (fieldName.Equals("genre", StringComparison.OrdinalIgnoreCase))
        return SearchField.Genre;
      else if (fieldName.Equals("lastfmtags", StringComparison.OrdinalIgnoreCase))
        return SearchField.LastFMTages;
      else if (fieldName.Equals("style", StringComparison.OrdinalIgnoreCase))
        return SearchField.Style;
      else if(fieldName.Equals("tone", StringComparison.OrdinalIgnoreCase))
        return SearchField.Tone;
      else if (fieldName.Equals("composer", StringComparison.OrdinalIgnoreCase))
        return SearchField.Composer;
      else if (fieldName.Equals("keyword", StringComparison.OrdinalIgnoreCase))
        return SearchField.Keyword;

      return SearchField.None;
    }

    /// <summary>
    /// Generate a filtered playlist
    /// </summary>
    /// <param name="searchField"></param>
    /// <param name="strSearchValue"></param>
    void genFilterPlaylist(SearchField searchField, string strSearchValue)
    {
      
      switch (searchField)
      {
        case SearchField.Genre:
          if (artistPlayList.Count == 0)
          {
            // If have not selected list then run though and add matching artists
            foreach (DBArtistInfo artistData in fullArtistList)
            {
              if (string.Equals(artistData.Genre, strSearchValue, StringComparison.OrdinalIgnoreCase))
                artistPlayList.Add(artistData);
            }
          }
          else
          {
            // This is a filter on previous list, run though and remove non-matching artists 
            List<DBArtistInfo> workingArtistList = new List<DBArtistInfo>(artistPlayList);
            artistPlayList.Clear();
            foreach (DBArtistInfo artistData in workingArtistList)
            {
              if (string.Equals(artistData.Genre, strSearchValue, StringComparison.OrdinalIgnoreCase))
                artistPlayList.Add(artistData);
            }
          }
          break;
        case SearchField.LastFMTages:
          if (artistPlayList.Count == 0)
          {
            foreach (DBArtistInfo artistData in fullArtistList)
            {
              foreach (string tag in artistData.Tag)
              {
                if (String.Equals(tag, strSearchValue, StringComparison.OrdinalIgnoreCase))
                  artistPlayList.Add(artistData);
              }
            }
          }
          else
          {
            List<DBArtistInfo> workingArtistList = new List<DBArtistInfo>(artistPlayList);
            artistPlayList.Clear();
            foreach (DBArtistInfo artistData in workingArtistList)
            {
              foreach (string tag in artistData.Tag)
              {
                if (String.Equals(tag, strSearchValue, StringComparison.OrdinalIgnoreCase))
                  artistPlayList.Add(artistData);
              }
            }

          }
          break;
        case SearchField.Tone:
          if (artistPlayList.Count == 0)
          {
            foreach (DBArtistInfo artistData in fullArtistList)
            {
              if (artistData.Tones.Contains(strSearchValue, StringComparison.OrdinalIgnoreCase))
                artistPlayList.Add(artistData);
            }
          }
          else
          {
            List<DBArtistInfo> workingArtistList = new List<DBArtistInfo>(artistPlayList);
            artistPlayList.Clear();
            foreach (DBArtistInfo artistData in workingArtistList)
              {
                if (artistData.Tones.Contains(strSearchValue, StringComparison.OrdinalIgnoreCase))
                  artistPlayList.Add(artistData);
              }
          }
          break;
        case SearchField.Style:
          if (artistPlayList.Count == 0)
          {
            foreach (DBArtistInfo artistData in fullArtistList)
            {
              if (artistData.Styles.Contains(strSearchValue, StringComparison.OrdinalIgnoreCase))
                artistPlayList.Add(artistData);
            }
          }
          else
          {
            List<DBArtistInfo> workingArtistList = new List<DBArtistInfo>(artistPlayList);
            artistPlayList.Clear();
            foreach (DBArtistInfo artistData in workingArtistList)
            {
              if (artistData.Styles.Contains(strSearchValue, StringComparison.OrdinalIgnoreCase))
                artistPlayList.Add(artistData);
            }
          }

          break;
        case SearchField.Composer:
          break;
        case SearchField.Keyword:
          break;
        default:
          break;
      }
      buildFacade();

    }
    /// <summary>
    /// Generate match playlist based on selections
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

      buildFacade();
    }

    void buildFacade()
    {


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

      GUIControl.SetControlLabel(windowID, (int)GUIControls.TotalArtists, string.Format("Artists in Playlist: {0} / Videos in Playlist {1}", artistPlayList.Count.ToString(), facadeLayout.Count.ToString()));

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
    /// <summary>
    /// Set/Reset to defualt
    /// </summary>
    void initValues()
    {
      selectedGenre = "None";
      selectedLastFMTag = "None";
      selectedTone = "None";
      selectedStyle = "None";
      selectedComposer = "None";
      selectedKeyword = "None";

      fieldSelected1 = string.Empty;
      fieldSelected2 = string.Empty;
      fieldSelected3 = string.Empty;
      fieldSelected4 = string.Empty;
      fieldSelected5 = string.Empty;
      fieldSelected6 = string.Empty;

      customSearchStr1 = string.Empty;
      customSearchStr2 = string.Empty;
      customSearchStr3 = string.Empty;
      customSearchStr4 = string.Empty;
      customSearchStr5 = string.Empty;
      customSearchStr6 = string.Empty;

      genreAdded = false;
      tagAdded = false;
      toneAdded = false;
      styleAdded = false;
      composerAdded = false;
      keywordAdded = false;

      GUIControl.SetControlLabel(windowID, (int)GUIControls.smartMode, "Mode: Match");
      GUIControl.SetControlLabel(windowID, (int)GUIControls.smartMode, "Mode: Match");
      GUIPropertyManager.SetProperty("#mvCentral.MatchMode", "true");


      GUIControl.SetControlLabel(windowID, (int)GUIControls.Genre, "Genre: None");
      GUIControl.SetControlLabel(windowID, (int)GUIControls.LastFMTag, "LastFM Tag: None");
      GUIControl.SetControlLabel(windowID, (int)GUIControls.Style, "Style: None");
      GUIControl.SetControlLabel(windowID, (int)GUIControls.Tone, "Tone: None");
      GUIControl.SetControlLabel(windowID, (int)GUIControls.Composer, "Composer: None");

      GUIControl.SetControlLabel(windowID, (int)GUIControls.Custom1, "Select Fiter Field..");
      GUIControl.SetControlLabel(windowID, (int)GUIControls.Custom2, "Select Fiter Field..");
      GUIControl.SetControlLabel(windowID, (int)GUIControls.Custom3, "Select Fiter Field..");
      GUIControl.SetControlLabel(windowID, (int)GUIControls.Custom4, "Select Fiter Field..");
      GUIControl.SetControlLabel(windowID, (int)GUIControls.Custom5, "Select Fiter Field..");
      GUIControl.SetControlLabel(windowID, (int)GUIControls.Custom6, "Select Fiter Field..");
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
