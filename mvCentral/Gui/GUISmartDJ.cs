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
    private bool persisting = false;

    List<DBArtistInfo> fullArtistList = new List<DBArtistInfo>();
    List<DBTrackInfo> fullVideoList = new List<DBTrackInfo>();

    List<DBArtistInfo> artistPlayList = new List<DBArtistInfo>();

    List<DBTrackInfo> videoList = new List<DBTrackInfo>();

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
      SmartDJMode = 20,
      PlayPlaylist = 21,
      SavePlaylist = 22,
      ShufflePlaylist = 23,
      Playlist = 24,
      FieldButton1 = 30,
      FieldButton2 = 31,
      FieldButton3 = 32,
      FieldButton4 = 33,
      FieldButton5 = 34,
      FieldButton6 = 35,
      MaxTime = 36,
      TotalArtists = 37,
      Facade = 50
    }

    [SkinControl((int)GUIControls.SmartDJMode)] protected GUIButtonControl smartDJ_SmartMode = null;
    [SkinControl((int)GUIControls.PlayPlaylist)] protected GUIButtonControl SmartDJ_Play = null;
    [SkinControl((int)GUIControls.SavePlaylist)] protected GUIButtonControl SmartDJ_Save = null;
    [SkinControl((int)GUIControls.ShufflePlaylist)] protected GUICheckButton smartDJ_ShufflePlaylist = null;
    [SkinControl((int)GUIControls.Playlist)] protected GUIButtonControl smartDJ_Playlist = null;

    [SkinControl((int)GUIControls.FieldButton1)] protected GUIButtonControl smartDJ_Button1 = null;
    [SkinControl((int)GUIControls.FieldButton2)] protected GUIButtonControl smartDJ_Button2 = null;
    [SkinControl((int)GUIControls.FieldButton3)] protected GUIButtonControl smartDJ_Button3 = null;
    [SkinControl((int)GUIControls.FieldButton4)] protected GUIButtonControl smartDJ_Button4 = null;
    [SkinControl((int)GUIControls.FieldButton5)] protected GUIButtonControl smartDJ_Button5 = null;
    [SkinControl((int)GUIControls.FieldButton6)] protected GUIButtonControl smartDJ_Button6 = null;
    [SkinControl((int)GUIControls.MaxTime)] protected GUISpinButton smartDJ_maxTime = null;

    [SkinControl((int)GUIControls.TotalArtists)] protected GUILabelControl smartDJ_TotalArtists = null;


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
      base.OnPageLoad();
      // Clear the skin properties
      clearFacadeItemProps();
      // Clear the facade
      facadeLayout.CurrentLayout = GUIFacadeControl.Layout.List;
      smartDJ_ShufflePlaylist.Selected = mvCentralCore.Settings.SmartDJPlaylistShuffle;
      if (!persisting)
      {
        initValues();
        fullArtistList = DBArtistInfo.GetAll();
        fullVideoList = DBTrackInfo.GetAll();
        facadeLayout.Visible = false;
        GUIControl.DisableControl(windowID, (int)GUIControls.PlayPlaylist);
        GUIControl.DisableControl(windowID, (int)GUIControls.SavePlaylist);
      }
      else
        refreshValues();
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
        case (int)GUIControls.SmartDJMode:
          if (matchingMode)
          {
            matchingMode = false;
            setButtonControls();
            GUIControl.SetControlLabel(windowID, (int)GUIControls.SmartDJMode, Localization.ModeFilter);
            GUIPropertyManager.SetProperty("#mvCentral.SmartDJ.Mode", "Mode: Filter");
            GUIControl.EnableControl(windowID, (int)GUIControls.FieldButton1);
            GUIControl.DisableControl(windowID, (int)GUIControls.FieldButton2);
            GUIControl.DisableControl(windowID, (int)GUIControls.FieldButton3);
            GUIControl.DisableControl(windowID, (int)GUIControls.FieldButton4);
            GUIControl.DisableControl(windowID, (int)GUIControls.FieldButton5);
            GUIControl.DisableControl(windowID, (int)GUIControls.FieldButton6);

          }
          else
          {
            matchingMode = true;
            setButtonControls();
            GUIControl.SetControlLabel(windowID, (int)GUIControls.SmartDJMode, Localization.ModeMatch);
            GUIPropertyManager.SetProperty("#mvCentral.SmartDJ.Mode", "Mode: Filter");
          }
          break;

        case (int)GUIControls.FieldButton1:
          if (matchingMode)
          {
            selectedGenre = selectGenre();
            GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton1, "Genre: " + selectedGenre);
            genPlaylist(fullArtistList);
          }
          else
          {
            result = getFieldAndSearchValue(ref fieldSelected1, ref customSearchStr1, GUIControls.FieldButton1);
            if (result == FieldSetup.Success)
              GUIControl.EnableControl(windowID, (int)GUIControls.FieldButton2);

            if (result == FieldSetup.Clear)
              cleanAndReset(GUIControls.FieldButton1);
          }
          break;
        case (int)GUIControls.FieldButton2:
          if (matchingMode)
          {
            selectedLastFMTag = selectLastFMTag();
            GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton2, "Last FM Tag: " + selectedLastFMTag);
            genPlaylist(fullArtistList);
          }
          else
          {
            result = getFieldAndSearchValue(ref fieldSelected2, ref customSearchStr2, GUIControls.FieldButton2);
            if (result == FieldSetup.Success)
              GUIControl.EnableControl(windowID, (int)GUIControls.FieldButton3);

            if (result == FieldSetup.Clear)
              cleanAndReset(GUIControls.FieldButton2);
          }
          break;
        case (int)GUIControls.FieldButton3:
          if (matchingMode)
          {
            selectedTone = selectTone();
            GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton3, "Tone: " + selectedTone);
            genPlaylist(fullArtistList);
          }
          else
          {
            result = getFieldAndSearchValue(ref fieldSelected3, ref customSearchStr3, GUIControls.FieldButton3);
            if (result == FieldSetup.Success)
              GUIControl.EnableControl(windowID, (int)GUIControls.FieldButton4);

            if (result == FieldSetup.Clear)
              cleanAndReset(GUIControls.FieldButton3);
          }
          break;
        case (int)GUIControls.FieldButton4:
          if (matchingMode)
          {
            selectedStyle = selectStyle();
            GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton4, "Style: " + selectedStyle);
            genPlaylist(fullArtistList);
          }
          else
          {
            result = getFieldAndSearchValue(ref fieldSelected4, ref customSearchStr4, GUIControls.FieldButton4);
            if (result == FieldSetup.Success)
              GUIControl.EnableControl(windowID, (int)GUIControls.FieldButton5);

            if (result == FieldSetup.Clear)
              cleanAndReset(GUIControls.FieldButton4);
          }
          break;
        case (int)GUIControls.FieldButton5:
          if (matchingMode)
          {
            selectedComposer = selectComposer();
            GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton5, "Composer: " + selectedComposer);
            genPlaylist(fullArtistList);
          }
          else
          {
            result = getFieldAndSearchValue(ref fieldSelected5, ref customSearchStr5, GUIControls.FieldButton5);
            if (result == FieldSetup.Success)
              GUIControl.EnableControl(windowID, (int)GUIControls.FieldButton6);

            if (result == FieldSetup.Clear)
              cleanAndReset(GUIControls.FieldButton5);

          }
          break;
        case (int)GUIControls.FieldButton6:
          if (matchingMode)
          {
            selectedKeyword = getKeyword();
            GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton6, "Keyword: " + selectedKeyword);
            genPlaylist(fullArtistList);
          }
          else
          {
            result = getFieldAndSearchValue(ref fieldSelected6, ref customSearchStr6, GUIControls.FieldButton6);

            if (result == FieldSetup.Clear)
              cleanAndReset(GUIControls.FieldButton6);
          }
          break;
        case (int)GUIControls.PlayPlaylist:
          if (facadeLayout.Count > 0)
            playSmartDJPlaylist();

          break;
        case (int)GUIControls.SavePlaylist:
          if (facadeLayout.Count > 0)
            saveSmartDJPlaylist();

          break;
        case (int)GUIControls.Playlist:
          if (!Player.playlistPlayer.mvPlayer.Playing)
          {
            videoList.Clear();
            for (int i = 0; i < facadeLayout.ListLayout.Count; i++)
            {
              GUIListItem trackItem = facadeLayout.ListLayout[i];
              if (!trackItem.IsFolder && trackItem.MusicTag != null)
                videoList.Add((DBTrackInfo)trackItem.MusicTag);
            }
            addToPlaylist(videoList, false, true, smartDJ_ShufflePlaylist.Selected);
          }
          GUIWindowManager.ActivateWindow(Player.GetWindowId());
          break;

        case (int)GUIControls.ShufflePlaylist:
          mvCentralCore.Settings.SmartDJPlaylistShuffle = smartDJ_ShufflePlaylist.Selected;
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
              videoList.Clear();
              if (!selectedItem.IsFolder && selectedItem.MusicTag != null)
              {
                // we have a track selected so add any other tracks which are on showing on the facade
                for (int i = 0; i < facadeLayout.ListLayout.Count; i++)
                {
                  GUIListItem trackItem = facadeLayout.ListLayout[i];
                  if (!trackItem.IsFolder && trackItem.MusicTag != null)
                    videoList.Add((DBTrackInfo)trackItem.MusicTag);
                }
                // Now create a Playlist
                addToPlaylist(videoList, false, true, false);
                // And play it
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
    /// Handle keyboard/remote action
    /// </summary>
    /// <param name="action"></param>
    public override void OnAction(MediaPortal.GUI.Library.Action action)
    {
      MediaPortal.GUI.Library.Action.ActionType wID = action.wID;

      base.OnAction(action);
    }


    #endregion

    #region Public Methods

    public static int GetWindowId()
    {
      return windowID;
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
      if (controlID == GUIControls.FieldButton1)
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
      else if (controlID == GUIControls.FieldButton2)
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
      else if (controlID == GUIControls.FieldButton3)
      {
        fieldSelected3 = fieldSelected4;
        customSearchStr3 = customSearchStr4;
        fieldSelected4 = fieldSelected5;
        customSearchStr4 = customSearchStr5;
        fieldSelected5 = fieldSelected6;
        customSearchStr5 = customSearchStr6;
      }
      else if (controlID == GUIControls.FieldButton4)
      {
        fieldSelected4 = fieldSelected5;
        customSearchStr4 = customSearchStr5;
        fieldSelected5 = fieldSelected6;
        customSearchStr5 = customSearchStr6;
      }
      else if (controlID == GUIControls.FieldButton5)
      {
        fieldSelected5 = fieldSelected6;
        customSearchStr5 = customSearchStr6;
      }
      else if (controlID == GUIControls.FieldButton6)
      {
        fieldSelected5 = string.Empty;
        customSearchStr5 = string.Empty;
      }

      // Reset the enabled stauts of the controls
      if (fieldSelected2 == string.Empty)
      {
        GUIControl.DisableControl(windowID, (int)GUIControls.FieldButton2);
        GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton2, Localization.SelFilter);
      }
      if (fieldSelected3 == string.Empty)
      {
        GUIControl.DisableControl(windowID, (int)GUIControls.FieldButton3);
        GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton3,Localization.SelFilter);
      }
      if (fieldSelected4 == string.Empty)
      {
        GUIControl.DisableControl(windowID, (int)GUIControls.FieldButton4);
        GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton4,Localization.SelFilter);
      }
      if (fieldSelected5 == string.Empty)
      {
        GUIControl.DisableControl(windowID, (int)GUIControls.FieldButton5);
        GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton5,Localization.SelFilter);
      }
      if (fieldSelected6 == string.Empty)
      {
        GUIControl.DisableControl(windowID, (int)GUIControls.FieldButton6);
        GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton6,Localization.SelFilter);
      }


      //Reload the playlist
      artistPlayList.Clear();
      if (fieldSelected1 != string.Empty)
      {
        GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton1, string.Format("{0}: {1} ({2})",Localization.Field, fieldSelected1, customSearchStr1));
        GUIControl.EnableControl(windowID, (int)GUIControls.FieldButton2);
        genFilterPlaylist(getFieldType(fieldSelected1), customSearchStr1);
      } else if (fieldSelected2 != string.Empty)
      {
        GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton2, string.Format("{0}: {1} ({2})", Localization.Field, fieldSelected2, customSearchStr2));
        GUIControl.EnableControl(windowID, (int)GUIControls.FieldButton3);
        genFilterPlaylist(getFieldType(fieldSelected2), customSearchStr2);
      } else if (fieldSelected3 != string.Empty)
      {
        GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton3, string.Format("{0}: {1} ({2})", Localization.Field, fieldSelected3, customSearchStr3));
        GUIControl.EnableControl(windowID, (int)GUIControls.FieldButton4);
        genFilterPlaylist(getFieldType(fieldSelected3), customSearchStr3);
      } else if (fieldSelected4 != string.Empty)
      {
        GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton4, string.Format("{0}: {1} ({2})", Localization.Field, fieldSelected4, customSearchStr4));
        GUIControl.EnableControl(windowID, (int)GUIControls.FieldButton5);
        genFilterPlaylist(getFieldType(fieldSelected4), customSearchStr4);
      } else if (fieldSelected5 != string.Empty)
      {
        GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton5, string.Format("{0}: {1} ({2})", Localization.Field, fieldSelected5, customSearchStr5));
        GUIControl.EnableControl(windowID, (int)GUIControls.FieldButton6);
        genFilterPlaylist(getFieldType(fieldSelected5), customSearchStr5);
      }
      else if (fieldSelected6 != string.Empty)
      {
        GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton6, string.Format("{0}: {1} ({2})", Localization.Field, fieldSelected6, customSearchStr6));
        genFilterPlaylist(getFieldType(fieldSelected6), customSearchStr6);
      }
      else
      {
        GUIControl.ClearControl(windowID, (int)GUIControls.Facade);
        GUIControl.DisableControl(windowID, (int)GUIControls.FieldButton2);
        GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton1,Localization.SelFilter);
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
        GUIControl.SetControlLabel(windowID, (int)controlID, Localization.Field + ": " + fieldName);
      }
      searchValue = filterValue(fieldName, out thisField);
      if (searchValue.Equals("None", StringComparison.OrdinalIgnoreCase))
      {
        fieldName = string.Empty;
        searchValue = string.Empty;
        return FieldSetup.Clear;
      }

      GUIControl.SetControlLabel(windowID, (int)controlID, string.Format("{0}: {1} ({2})", Localization.Field, fieldName, searchValue));
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
          strRetVal = getKeyword();
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
      GUIDialogSelect2 dlg = (GUIDialogSelect2)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_SELECT2);
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
        addToPlaylist(videoList, false, true, smartDJ_ShufflePlaylist.Selected);
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
          DBTrackInfo mv = (DBTrackInfo)listItem.MusicTag;
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
    /// Get the selected field type
    /// </summary>
    /// <param name="fieldName"></param>
    /// <returns></returns>
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
              if (string.Equals(artistData.Genre, strSearchValue, StringComparison.OrdinalIgnoreCase) || artistData.Genre.Contains(strSearchValue,StringComparison.OrdinalIgnoreCase))
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
          // If we have an empty list but we have a composer chosen then grab all artists and filter out just tracks with that composer during the facade build
          if (artistPlayList.Count == 0 && selectedComposer != "None")
            artistPlayList = DBArtistInfo.GetAll();

          break;
        case SearchField.Keyword:
          if (artistPlayList.Count == 0)
          {
            foreach (DBArtistInfo artistData in fullArtistList)
            {
              if (artistData.bioContent.Contains(strSearchValue, StringComparison.OrdinalIgnoreCase))
                artistPlayList.Add(artistData);
            }
          }
          else
          {
            List<DBArtistInfo> workingArtistList = new List<DBArtistInfo>(artistPlayList);
            artistPlayList.Clear();
            foreach (DBArtistInfo artistData in workingArtistList)
            {
              if (artistData.bioContent.Contains(strSearchValue, StringComparison.OrdinalIgnoreCase))
                artistPlayList.Add(artistData);
            }
          }
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
          if (artistData.Genre == selectedGenre || artistData.Genre.Contains(selectedGenre))
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
        // Keyword
        if (selectedKeyword != "None")
        {
          if (artistData.bioContent.Contains(selectedStyle, StringComparison.OrdinalIgnoreCase))
            artistPlayList.Add(artistData);
          else if (keywordInTrackDescription(artistData,selectedKeyword))
            artistPlayList.Add(artistData);
        }
      }

      // If we have an empty list but we have a composer chosen then grab all artists and filter out just tracks with that composer during the facade build
      if (artistPlayList.Count == 0 && selectedComposer != "None")
        artistPlayList = DBArtistInfo.GetAll();

      buildFacade();
    }
    /// <summary>
    /// Does the keyword exist in the Track description
    /// </summary>
    /// <param name="artistInfo"></param>
    /// <returns></returns>
    bool keywordInTrackDescription(DBArtistInfo artistInfo, string keyWord)
    {
      List<DBTrackInfo> videosByArtist = DBTrackInfo.GetEntriesByArtist(artistInfo);
      foreach (DBTrackInfo track in videosByArtist)
      {
        if (track.bioContent.Contains(keyWord, StringComparison.OrdinalIgnoreCase))
          return true;
      }
      return false;
    }
    /// <summary>
    /// Populate the facade
    /// </summary>
    void buildFacade()
    {
      // Clear the facade
      GUIControl.ClearControl(GetID, facadeLayout.GetID);
      clearFacadeItemProps();
      // Load tracks into facade
      foreach (DBArtistInfo artist in artistPlayList)
      {
        foreach (DBTrackInfo trackData in DBTrackInfo.GetEntriesByArtist(artist))
        {
          // If filtering on composer the check the track.
          if (selectedComposer != "None")
          {
            if (!trackData.Composers.Contains(selectedComposer, StringComparison.OrdinalIgnoreCase))
              continue;
          }
          // 
          GUIListItem facadeItem = new GUIListItem();
          // Check if raw filename display is required
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
          facadeItem.TVTag = mvCentralUtils.bioNoiseFilter(trackData.bioContent);
          facadeItem.Path = trackData.LocalMedia[0].File.FullName;
          facadeItem.IsFolder = false;
          facadeItem.MusicTag = trackData;
          facadeItem.Rating = trackData.Rating;
          // If no thumbnail set a default
          if (!string.IsNullOrEmpty(trackData.ArtFullPath.Trim()))
            facadeItem.ThumbnailImage = trackData.ArtFullPath;
          else
            facadeItem.ThumbnailImage = "defaultVideoBig.png";
          // Set the played status
          if (trackData.ActiveUserSettings.WatchedCount > 0)
          {
            facadeItem.IsPlayed = true;
            facadeItem.Shaded = true; ;
            facadeItem.IconImage = GUIGraphicsContext.Skin + @"\Media\tvseries_Watched.png";
          }
          else
          {
            facadeItem.IsPlayed = false;
            facadeItem.Shaded = false;
            facadeItem.IconImage = GUIGraphicsContext.Skin + @"\Media\tvseries_UnWatched.png";
          }
          facadeItem.OnItemSelected += new GUIListItem.ItemSelectedHandler(onVideoSelected);
          facadeLayout.Add(facadeItem);
        }
      }

      if (facadeLayout.Count > 0)
      {
        facadeLayout.Visible = true;
        GUIPropertyManager.SetProperty("#itemcount", facadeLayout.Count.ToString());
        GUIPropertyManager.SetProperty("#itemtype", Localization.Videos);
        GUIControl.EnableControl(windowID, (int)GUIControls.PlayPlaylist);
        GUIControl.EnableControl(windowID, (int)GUIControls.SavePlaylist);
        // Select first item in the list, set the skin properities and set focus to the facade
        facadeLayout.SelectedListItemIndex = 0;
        onVideoSelected(facadeLayout.SelectedListItem, facadeLayout);
        GUIControl.FocusControl(windowID, (int)GUIControls.Facade);
      }
      else
      {
        facadeLayout.Visible = false;
        GUIControl.DisableControl(windowID, (int)GUIControls.PlayPlaylist);
        GUIControl.DisableControl(windowID, (int)GUIControls.SavePlaylist);
      }
      // Set the info label
      GUIControl.SetControlLabel(windowID, (int)GUIControls.TotalArtists, string.Format("{0}: {1} / {2}: {3}", Localization.SelArtists, artistPlayList.Count.ToString(), Localization.SelVideos, facadeLayout.Count.ToString()));
    }
    /// <summary>
    /// Video/Album item selected - set properities
    /// </summary>
    /// <param name="item"></param>
    /// <param name="parent"></param>
    void onVideoSelected(GUIListItem item, GUIControl parent)
    {
      DBTrackInfo trackInfo = null;
      // Is this a Track we are on - best to be safe
      if (item.MusicTag.GetType() == typeof(DBTrackInfo))
      {
        // This is an Video
        trackInfo = (DBTrackInfo)item.MusicTag;
        // Basic Props
        GUIPropertyManager.SetProperty("#mvCentral.ArtistName", trackInfo.ArtistInfo[0].Artist);
        // Track information
        if (string.IsNullOrEmpty(item.TVTag.ToString().Trim()))
          GUIPropertyManager.SetProperty("#mvCentral.TrackInfo", Localization.NoTrackInfo);
        else
          GUIPropertyManager.SetProperty("#mvCentral.TrackInfo", item.TVTag.ToString());
        // Track Rating
        GUIPropertyManager.SetProperty("#mvCentral.Track.Rating", trackInfo.Rating.ToString());
        // Track Composers
        if (trackInfo.Composers.Trim().Length == 0)
          GUIPropertyManager.SetProperty("#mvCentral.Composers", Localization.NoComposerInfo);
        else
          GUIPropertyManager.SetProperty("#mvCentral.Composers", trackInfo.Composers.Replace('|', ','));
        // Duration
        GUIPropertyManager.SetProperty("#mvCentral.Duration", trackDuration(trackInfo.PlayTime));
        // #iswatched
        DBUserMusicVideoSettings userSettings = trackInfo.ActiveUserSettings;
        if (userSettings.WatchedCount > 0)
        {
          GUIPropertyManager.SetProperty("#iswatched", "yes");
          GUIPropertyManager.SetProperty("#mvCentral.Watched.Count", userSettings.WatchedCount.ToString());
        }
        else
        {
          GUIPropertyManager.SetProperty("#iswatched", "no");
          GUIPropertyManager.SetProperty("#mvCentral.Watched.Count", "0");
        }
      }
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
    /// Select Style
    /// </summary>
    /// <returns></returns>
    string selectGenre()
    {
      List<string> genreList = new List<string>();
      GUIDialogSelect2 dlg = (GUIDialogSelect2)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_SELECT2);
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
          if (artistObject.Genre.Contains(","))
          {
            string[] _genres = artistObject.Genre.Split(',');
            foreach (string _genre in _genres)
            {
              if (!genreList.Contains(_genre.Trim()))
              {
                genreList.Add(_genre.Trim());
                dlg.Add(_genre.Trim());
              }
            }
          }
          else
          {
            if (!genreList.Contains(artistObject.Genre.Trim()))
            {
              genreList.Add(artistObject.Genre.Trim());
              dlg.Add(artistObject.Genre.Trim());
            }
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
      GUIDialogSelect2 dlg = (GUIDialogSelect2)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_SELECT2);
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
      GUIDialogSelect2 dlg = (GUIDialogSelect2)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_SELECT2);
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
      GUIDialogSelect2 dlg = (GUIDialogSelect2)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_SELECT2);
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
      GUIDialogSelect2 dlg = (GUIDialogSelect2)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_SELECT2);
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
    /// Capture the Keyword
    /// </summary>
    /// <returns></returns>
    string getKeyword()
    {
      string keyWord = string.Empty;
      if (GetKeyboard(ref keyWord))
      {
        return keyWord;
      }
      return "None";
    }


    /// <summary>
    /// Set/Reset to defualt
    /// </summary>
    void initValues()
    {
      persisting = true;
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

      matchingMode = true;
      GUIControl.SetControlLabel(windowID, (int)GUIControls.SmartDJMode, Localization.ModeMatch);
      GUIPropertyManager.SetProperty("#mvCentral.SmartDJ.Mode", "Mode: Matching");
      setButtonControls();
    }
    /// <summary>
    /// When re-entering SmartDJ refersh the current values
    /// </summary>
    void refreshValues()
    {
      // Set the buttons
      if (matchingMode)
      {
        GUIControl.SetControlLabel(windowID, (int)GUIControls.SmartDJMode, Localization.ModeMatch);
        GUIPropertyManager.SetProperty("#mvCentral.SmartDJ.Mode", "Mode: Matching");
        // Set default first
        GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton1, "Genre: None");
        GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton2, "LastFM Tag: None");
        GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton3, "Style: None");
        GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton4, "Tone: None");
        GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton5, "Composer: None");
        GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton6, "Keyword: None");
        // Now set any Values
        if (selectedGenre != string.Empty)
          GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton1, "Genre: " + selectedGenre);

        if (selectedLastFMTag != string.Empty)
          GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton2, "LastFM Tag: " + selectedLastFMTag);

        if (selectedStyle != string.Empty)
          GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton3, "Style: " + selectedStyle);

        if (selectedTone != string.Empty)
          GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton4, "Tone: " + selectedTone);

        if (selectedComposer != string.Empty)
          GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton5, "Composer: " + selectedComposer);

        if (selectedKeyword != string.Empty)
          GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton6, "Keyword: " + selectedKeyword);
        // Build and Display the facade
        buildFacade();
      }
      else
      {
        GUIControl.SetControlLabel(windowID, (int)GUIControls.SmartDJMode, Localization.ModeFilter);
        GUIPropertyManager.SetProperty("#mvCentral.SmartDJ.Mode", "Mode: Filter");
        // Set the defaults
        GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton1, Localization.SelFilter);
        GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton2, Localization.SelFilter);
        GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton3, Localization.SelFilter);
        GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton4, Localization.SelFilter);
        GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton5, Localization.SelFilter);
        GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton6, Localization.SelFilter);
        GUIControl.DisableControl(windowID, (int)GUIControls.FieldButton2);
        GUIControl.DisableControl(windowID, (int)GUIControls.FieldButton3);
        GUIControl.DisableControl(windowID, (int)GUIControls.FieldButton4);
        GUIControl.DisableControl(windowID, (int)GUIControls.FieldButton5);
        GUIControl.DisableControl(windowID, (int)GUIControls.FieldButton6);
        // Now set the existing valuses
        if (fieldSelected1 != string.Empty)
        {
          GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton1, string.Format("{0}: {1} ({2})", Localization.Field, fieldSelected1, customSearchStr1));
          GUIControl.DisableControl(windowID, (int)GUIControls.FieldButton2);
        }
        if (fieldSelected2 != string.Empty)
        {
          GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton2, string.Format("{0}: {1} ({2})", Localization.Field, fieldSelected2, customSearchStr2));
          GUIControl.DisableControl(windowID, (int)GUIControls.FieldButton3);
        }
        if (fieldSelected3 != string.Empty)
        {
          GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton3, string.Format("{0}: {1} ({2})", Localization.Field, fieldSelected3, customSearchStr3));
          GUIControl.DisableControl(windowID, (int)GUIControls.FieldButton4);
        }
        if (fieldSelected4 != string.Empty)
        {
          GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton4, string.Format("{0}: {1} ({2})", Localization.Field, fieldSelected4, customSearchStr4));
          GUIControl.DisableControl(windowID, (int)GUIControls.FieldButton5);
        }
        if (fieldSelected5 != string.Empty)
        {
          GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton5, string.Format("{0}: {1} ({2})", Localization.Field, fieldSelected5, customSearchStr5));
          GUIControl.DisableControl(windowID, (int)GUIControls.FieldButton6);
        }
        if (fieldSelected6 != string.Empty)
          GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton6, string.Format("{0}: {1} ({2})", Localization.Field, fieldSelected6, customSearchStr6));
        // Build and display the facade
        buildFacade();
      }
    }

    /// <summary>
    /// Set the button text for the selected mode
    /// </summary>
    void setButtonControls()
    {
      if (matchingMode)
      {
        GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton1, "Genre: None");
        GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton2, "LastFM Tag: None");
        GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton3, "Style: None");
        GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton4, "Tone: None");
        GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton5, "Composer: None");
        GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton6, "Keyword: None");
      }
      else
      {
        GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton1, Localization.SelFilter);
        GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton2, Localization.SelFilter);
        GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton3, Localization.SelFilter);
        GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton4, Localization.SelFilter);
        GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton5, Localization.SelFilter);
        GUIControl.SetControlLabel(windowID, (int)GUIControls.FieldButton6, Localization.SelFilter);
      }
    }

    /// <summary>
    /// Clear the facade item props
    /// </summary>
    void clearFacadeItemProps()
    {
      GUIPropertyManager.SetProperty("#mvCentral.ArtistName", string.Empty);
      GUIPropertyManager.SetProperty("#mvCentral.TrackInfo", string.Empty);
      GUIPropertyManager.SetProperty("#mvCentral.Track.Rating", string.Empty);
      GUIPropertyManager.SetProperty("#mvCentral.Composers", string.Empty);
      GUIPropertyManager.SetProperty("#mvCentral.Duration", string.Empty);
    }


    #endregion

  }
}
