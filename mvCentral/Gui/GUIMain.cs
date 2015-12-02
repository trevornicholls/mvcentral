using Common.GUIPlugins;

using Cornerstone.GUI.Dialogs;
using Cornerstone.Tools;

using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;

using mvCentral.Database;
using mvCentral.Localizations;
using mvCentral.LocalMediaManagement;
using mvCentral.Playlist;
using mvCentral.Utils;

using NLog;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;

using Action = MediaPortal.GUI.Library.Action;
using Layout = MediaPortal.GUI.Library.GUIFacadeControl.Layout;

namespace mvCentral.GUI
{
  public partial class MvGuiMain : WindowPluginBase
  {

    #region enums

    public enum MvView
    {
      None = 1,
      Artist = 2,
      Album = 3,
      Video = 4,
      AllAlbums = 5,
      AllVideos = 6,
      VideosOnAlbum = 7,
      Genres = 8,
      ArtistViaGenre = 9,
      ArtistTracks = 10,
      SearchedArtists = 11,
      DvdView = 12
    }

    public enum MvSort
    {
      Ascending,
      Desending
    }

    public enum View
    {
      Artists = 0,
      Albums = 1,
      Tracks = 2,
      Generes = 3,
      DvDs = 4
    }

    #endregion

    #region Declarations

    Dictionary<string, bool> _loggedProperties;

    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private mvCentralCore _core = mvCentralCore.Instance;
    readonly GUImvPlayList Player = new GUImvPlayList();

    readonly List<MvView> screenStack = new List<MvView>();
    List<DBArtistInfo> _foundArtists = new List<DBArtistInfo>();
    private DBArtistInfo _currArtist = null; 

    private bool _initComplete = false;
    private Thread initThread;
    private Thread _initThread2;
    private bool _preventDialogOnLoad = false;
    private readonly object propertySync = new object();
    private bool _persisting = false;
    private bool _layoutChanging = false;
    private string _selArtist = "";
    public static int CurrentArtistId = -1;
    public static DBArtistInfo CurrentArtistInfo = null;
    private string _selAlbum = "";
    private MvView _currentView = MvView.Artist;
    private MvView _runningView = MvView.None;
    private MvSort artistSort = MvSort.Ascending;
    private MvSort _videoSort = MvSort.Ascending;
    private int _genreTracks = 0;
    private string _lastViewedGenre = string.Empty;

    private List<string> artistTags = new List<string>();
    public System.Timers.Timer clearPropertyTimer;

    public int lastItemArt = 0, lastItemVid = 0, lastItemAlb = 0, lastGenreItem = 0,artistID = 0, albumID = 0;

    protected MvView CurrentView
    {
      get { return _currentView; }
      set { _currentView = value; }
    }

    protected bool AllowView(View view)
    {
      return true;
    }

    #endregion

    #region Skin Connection

    private enum GuiControls
    {
      PlayAllRandom = 6,
      PlaySmart = 7,
      PlayList = 8,
      StatsAndInfo = 9,
      GenreConfig = 10,
      Search = 11,
      MetaDataProgressBar = 12,
      Facade = 50
    }

    [SkinControlAttribute((int)GuiControls.PlayAllRandom)] protected GUIButtonControl BtnPlayAllRandom = null;
    [SkinControlAttribute((int)GuiControls.PlaySmart)] protected GUIButtonControl BtnSmartList = null;
    [SkinControlAttribute((int)GuiControls.PlayList)] protected GUIButtonControl BtnPlayList = null;
    [SkinControlAttribute((int)GuiControls.StatsAndInfo)] protected GUIButtonControl BtnStatsAndInfo = null;
    [SkinControlAttribute((int)GuiControls.GenreConfig)] protected GUIButtonControl BtnGenreConfig = null;
    [SkinControlAttribute((int)GuiControls.Search)] protected GUIButtonControl BtnSearch = null;

    [SkinControlAttribute((int)GuiControls.MetaDataProgressBar)] protected GUIProgressControl MetadataProgressBar = null;


    #endregion

    #region Overrides

    public override string GetModuleName()
    {
      return mvCentralCore.Settings.HomeScreenName;
    }

    public override bool Init()
    {
      base.Init();
      logger.Info("Initializing GUI...");

      // check if we can load the skin
      bool success = Load(GUIGraphicsContext.Skin + @"\mvCentral.xml");

      // get last active module settings 
      bool lastActiveModuleSetting = mvCentralCore.MediaPortalSettings.GetValueAsBool("general", "showlastactivemodule", false);
      int lastActiveModule = mvCentralCore.MediaPortalSettings.GetValueAsInt("general", "lastactivemodule", -1);
       _preventDialogOnLoad = (lastActiveModuleSetting && (lastActiveModule == GetID));

      // set some skin properties
      SetProperty("#mvCentral.Settings.HomeScreenName", mvCentralCore.Settings.HomeScreenName);
      artistTags.Clear();

      // start initialization of the music videos core services in a seperate thread
      initThread = new Thread(new ThreadStart(mvCentralCore.Initialize));
      initThread.Start();

      _initThread2 = new Thread(new ThreadStart(updateToneStylesAndComnpoers));
      _initThread2.Start();

      // ... and listen to the progress
      mvCentralCore.InitializeProgress += new ProgressDelegate(onCoreInitializationProgress);
      // Set last 3 added videos
      LastThreeVideos();
      // listen for additions to DB
      mvCentralCore.Importer.MusicVideoStatusChanged += new MusicVideoImporter.MusicVideoStatusChangedHandler(mvStatusChangedListener);
      // Listen for video screen changes
      GUIGraphicsContext.OnVideoWindowChanged += new VideoWindowChangedHandler(GUIGraphicsContext_OnVideoWindowChanged);
      // Listen to updates from artwork 
      mvCentralCore.ProcessManager.Progress += new ProcessProgressDelegate(ProcessManager_Progress);
      
      // If On start Video Info enabed set up timer to clear the Start.Playing property on fullscreen toggle
      if (mvCentralCore.Settings.EnableVideoStartInfo)
      {
        clearPropertyTimer = new System.Timers.Timer((double)mvCentralCore.Settings.VideoInfoStartTimer);
        clearPropertyTimer.Elapsed += new ElapsedEventHandler(clearPropertyTimer_Elapsed);
      }

      return success;
    }
    /// <summary>
    /// Catch GUI to full screen toggle, set property #mvCentral.isPlaying to enable pop-up
    /// </summary>
    void GUIGraphicsContext_OnVideoWindowChanged()
    {
      // make sure we are playing some video and the video source is us
      if (GUIGraphicsContext.IsPlayingVideo && (GUIPropertyManager.GetProperty("#mvCentral.isPlaying") == "true"))
      {
        if (GUIWindowManager.IsSwitchingToNewWindow)
        {
          if (GUIGraphicsContext.IsFullScreenVideo)
          {
            if (GUIPropertyManager.GetProperty("#mvCentral.Play.Started") == "false")
            {
              clearPropertyTimer.Enabled = true;
              logger.Debug("Set #mvCentral.Play.Started = true");
              GUIPropertyManager.SetProperty("#mvCentral.Play.Started", "true");
            }
          }
        }
      }
    }
    /// <summary>
    /// Reset the #mvCentral.Play.Started on timer elapsed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void clearPropertyTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
      logger.Debug("************* Clear Property Timer Fired ****************");
      logger.Debug("Set #mvCentral.Play.Started = false");
      GUIPropertyManager.SetProperty("#mvCentral.Play.Started", "false");
      clearPropertyTimer.Enabled = false;
    }
    /// <summary>
    /// Handle keyboard/remote action
    /// </summary>
    /// <param name="action"></param>
    public override void OnAction(MediaPortal.GUI.Library.Action action)
    {
      MediaPortal.GUI.Library.Action.ActionType wID = action.wID;
      // Queue video to playlist
      if (wID == MediaPortal.GUI.Library.Action.ActionType.ACTION_QUEUE_ITEM)
      {
        facadeLayout.SelectedListItemIndex = Player.playlistPlayer.CurrentItem;
        return;
      }
      // If on Track screen go back to artists screen
      if (wID == MediaPortal.GUI.Library.Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        artistID = CurrentArtistId;
        // Go Back to last view
        _currentView = getPreviousView();
        // Have we backed out to the last screen, if so exit otherwise load the previous screen
        if (_currentView == MvView.None)
          base.OnAction(action);
        else
          loadCurrent();
      }
      else if (wID == MediaPortal.GUI.Library.Action.ActionType.ACTION_CONTEXT_MENU)
      {
        string contextChoice = ShowContextMenu();
        if (contextChoice == Localization.AddToPlaylist)
        {
          //Add to playlist

          // If on a folder add all Videos for Artist
          if (facadeLayout.ListLayout.SelectedListItem.IsFolder)
          {
            _currArtist = DBArtistInfo.Get(facadeLayout.ListLayout.SelectedListItem.Label);
            var allTracksByArtist = DBTrackInfo.GetEntriesByArtist(_currArtist);
            AddToPlaylist(allTracksByArtist, false, mvCentralCore.Settings.ClearPlaylistOnAdd, mvCentralCore.Settings.GeneratedPlaylistAutoShuffle);
          }
          else
          {
            // Add video to playlist
            var playlist = Player.playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MVCENTRAL);
            var filename = facadeLayout.ListLayout.SelectedListItem.Label;
            var path = facadeLayout.ListLayout.SelectedListItem.Path;
            var video = (DBTrackInfo)facadeLayout.ListLayout.SelectedListItem.MusicTag;
            var p1 = new PlayListItem(video);
            p1.Track = video;
            playlist.Add(p1);
          }
        }
        else if (contextChoice == Localization.AddAllToPlaylist)
        {
          // Add all videos to the playlist

          var allTracks = DBTrackInfo.GetAll();
          AddToPlaylist(allTracks, false, mvCentralCore.Settings.ClearPlaylistOnAdd, mvCentralCore.Settings.GeneratedPlaylistAutoShuffle);
        }
        else if (contextChoice == Localization.AddToPlaylistNext)
        {
          // Add video as next playlist item

          addToPlaylistNext(facadeLayout.SelectedListItem);
        }
        else if (contextChoice == Localization.RefreshArtwork)
        {
          // Refresh the artwork

          if (facadeLayout.ListLayout.SelectedListItem.IsFolder)
          {
            _currArtist = DBArtistInfo.Get(facadeLayout.SelectedListItem.Label);
            GUIWaitCursor.Show();
            mvCentralCore.DataProviderManager.GetArt(_currArtist, false);
            GUIWaitCursor.Hide();
            facadeLayout.SelectedListItem.ThumbnailImage = _currArtist.ArtThumbFullPath;
            facadeLayout.SelectedListItem.RefreshCoverArt();
            facadeLayout.SelectedListItemIndex = facadeLayout.SelectedListItemIndex - 1;
            facadeLayout.SelectedListItemIndex = facadeLayout.SelectedListItemIndex + 1;
          }
          else
          {
            var video = (DBTrackInfo)facadeLayout.ListLayout.SelectedListItem.MusicTag;
            GUIWaitCursor.Show();
            mvCentralCore.DataProviderManager.GetArt(video, false);
            GUIWaitCursor.Hide();
            facadeLayout.SelectedListItem.ThumbnailImage = video.ArtFullPath;
            facadeLayout.SelectedListItem.RefreshCoverArt();
            facadeLayout.SelectedListItemIndex = facadeLayout.SelectedListItemIndex - 1;
            facadeLayout.SelectedListItemIndex = facadeLayout.SelectedListItemIndex + 1;
          }
        }
        else if (contextChoice == Localization.RateVid)
        {
          // Allow user rating of video

          OnSetRating(facadeLayout.SelectedListItemIndex);
        }
      }

      else
        base.OnAction(action);
    }

    /// <summary>
    /// Set the user rating for the video, this will cycle though all video with the facade
    /// </summary>
    /// <param name="itemNumber"></param>
    void OnSetRating(int itemNumber)
    {
      GUIListItem item = facadeLayout[itemNumber];
      GUIDialogSetRating itemRating = (GUIDialogSetRating)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_RATING);


      DBTrackInfo videoToRate = (DBTrackInfo)item.MusicTag;
      DBArtistInfo artist = (DBArtistInfo)videoToRate.ArtistInfo[0];
      DBUserMusicVideoSettings userSettings = videoToRate.ActiveUserSettings;
      if (userSettings.UserRating == null)
        userSettings.UserRating = 0;

      try
      {
        itemRating.Rating = (int)userSettings.UserRating/2;
      }
      catch (DivideByZeroException)
      {
        itemRating.Rating = 0;
      }

      itemRating.SetTitle(String.Format("{0}-{1}", artist.Artist, videoToRate.Track));
      itemRating.FileName = videoToRate.LocalMedia[0].FullPath;
      itemRating.SetHeading("Rate Music Video");
      // Open the Dialog
      itemRating.DoModal(GetID);
      // Set the rating
      logger.Debug("Set rating of ({0}) for video {1}", (itemRating.Rating*2).ToString(CultureInfo.InvariantCulture), itemRating.FileName);
      userSettings.UserRating = itemRating.Rating*2;
      videoToRate.Rating = itemRating.Rating * 2;

      if (itemRating.Result == GUIDialogSetRating.ResultCode.Previous)
      {
        while (itemNumber > 0)
        {
          itemNumber--;
          item = facadeLayout[itemNumber];
          if (!item.IsFolder && !item.IsRemote)
          {
            OnSetRating(itemNumber);
            return;
          }
        }
      }

      if (itemRating.Result == GUIDialogSetRating.ResultCode.Next)
      {
        while (itemNumber + 1 < facadeLayout.Count)
        {
          itemNumber++;
          item = facadeLayout[itemNumber];
          if (!item.IsFolder && !item.IsRemote)
          {
            OnSetRating(itemNumber);
            return;
          }
        }
      }
    }
      
      
    /// <summary>
    /// Handle a playlist change
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public override bool OnMessage(GUIMessage message)
    {
      
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_PLAYLIST_CHANGED:
          {
            facadeLayout.SelectedListItemIndex = message.Param2;
            return true;
          }
        //break;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
        case GUIMessage.MessageType.GUI_MSG_ITEM_SELECT:

          // Respond to the correct control.  The value is retrived directly from the control by the called handler.
          if (message.TargetControlId == btnViews.GetID)
          {
            SetView(btnViews.SelectedItemValue);
            GUIControl.FocusControl(GetID, btnViews.GetID);
          }
          break;

      }
      return base.OnMessage(message);
    }
    /// <summary>
    /// De-init
    /// </summary>
    public override void DeInit()
    {
      base.DeInit();

      logger.Info("Deinitializing GUI...");

      // if the plugin was not fully initialized yet abort the initialization
      if (!_initComplete && initThread.IsAlive)
      {
        initThread.Abort();
        // wait for the thread to be aborted
        initThread.Join();
      }

      mvCentralCore.Shutdown();
      _initComplete = false;
      logger.Info("GUI Deinitialization Complete");
    }
    /// <summary>
    /// Return the plugin ID
    /// </summary>
    public override int GetID
    {
      get
      {
        return mvCentralCore.PluginID;
      }
      set
      {
      }
    }

    /// <summary>
    /// Load any settings - Windowsplugin Class override
    /// </summary>
    protected override void LoadSettings()
    {
      logger.Debug("LoadSettings");
      base.LoadSettings();
    }

    /// <summary>
    /// Override the initviews and add my own
    /// </summary>
    protected override void InitViewSelections()
    {
      btnViews.ClearMenu();

      // Add the view options to the menu.
      var index = 0;
      btnViews.AddItem(Localization.ViewAs + " " + Localization.Artists, index++);            // Artists
      if (DBAlbumInfo.GetAll().Count > 0 && !mvCentralCore.Settings.DisableAlbumSupport)
        btnViews.AddItem(Localization.ViewAs + " " + Localization.Albums, index++);           // Albums
      btnViews.AddItem(Localization.ViewAs + " " + Localization.Tracks, index++);             // Tracks
      if (DBGenres.GetSelected().Count > 0)
        btnViews.AddItem(Localization.ViewAs + " " + Localization.Genre, index++);            // Genres
      btnViews.AddItem(Localization.ViewAs + " " + "DVDs", index);                          // DVDs


      // Have the menu select the currently selected view.
      btnViews.SetSelectedItemByValue(int.Parse((mvCentralCore.Settings.ViewAs)));
    }

    /// <summary>
    /// List Available views - Windowsplugin Class override
    /// </summary>
    protected override void SetView(int selectedViewId)
    {
      // Display Artists, tracks or Albums
      logger.Debug("SetView Called: View {0}", selectedViewId);
      _persisting = false;

 
      // Bit messy this but only way
      if (selectedViewId == (int)View.Artists)
      {
        _currentView = MvView.Artist;
        loadArtists(artistSort);
        addToStack(_currentView, true);
      }
      else if (selectedViewId == (int)View.Albums)
      {
        _currentView = MvView.AllAlbums;
        addToStack(_currentView, true);
        loadAllAlbums();
      }
      else if (selectedViewId == (int)View.Tracks)
      {
        _currentView = MvView.AllVideos;
        addToStack(_currentView, true);
        loadAllVideos(_videoSort);
      }
      else if (selectedViewId == (int)View.Generes)
      {
        _currentView = MvView.Genres;
        addToStack(_currentView, true);
        loadGenres();
      }
      else if (selectedViewId == (int)View.DvDs)
      {
        _currentView = MvView.DvdView;
        addToStack(_currentView, true);
        loadDVDs();
      }

      logger.Debug("SwitchLayout: Storing MvView {0} and View {1}", _currentView, selectedViewId);
      mvCentralCore.Settings.DefaultViewAs = ((int)_currentView).ToString(CultureInfo.InvariantCulture);
      mvCentralCore.Settings.ViewAs = selectedViewId.ToString(CultureInfo.InvariantCulture);
      GUIControl.FocusControl(GetID, facadeLayout.GetID);
      setViewAsProperty(_currentView);
    }
    /// <summary>
    /// Show the sort options (None Currently)
    /// </summary>
    protected override void OnShowSort()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(495); // Sort options

      dlg.AddLocalizedString(103); // name
      dlg.AddLocalizedString(269); // artist
      dlg.AddLocalizedString(270); // album
      dlg.AddLocalizedString(266); // track
      dlg.AddLocalizedString(267); // duration
      dlg.AddLocalizedString(104); // date

      // show dialog and wait for result
      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1)
      {
        return;
      }

      UserMessage("Not Implemented", "", "Sorting is still in development", "");

      base.OnShowSort();
    }
    /// <summary>
    /// Sort direction chnage...so sort
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void SortChanged(object sender, SortEventArgs e)
    {
      SortFacade();
    }
    /// <summary>
    /// Sort the facade for chosen direction
    /// </summary>
    void SortFacade()
    {
      if (_currentView == MvView.Artist)
      {
        loadArtists(artistSort == MvSort.Ascending ? MvSort.Desending : MvSort.Ascending);
      }
      else if (_currentView == MvView.Video)
      {
        loadVideos(CurrentArtistId, _videoSort == MvSort.Ascending ? MvSort.Desending : MvSort.Ascending);
      }
      UpdateButtonStates();
    }
    /// <summary>
    /// Switch the layout
    /// </summary>
    protected override void SwitchLayout()
    {
      base.SwitchLayout();
      _layoutChanging = true;
      loadCurrent();
      _layoutChanging = false;
    }
    /// <summary>
    /// Search the DB for all tracks by an Artist and return that list
    /// </summary>
    List<DBArtistInfo> SearchOnArtist(string artistName)
    {
      List<DBArtistInfo> artistObject = DBArtistInfo.GetFuzzy(artistName);
      if (artistObject != null)
      {
        return artistObject;
      }
      return null;
    }
    /// <summary>
    /// Perform a search
    /// Only Artist search supported currently
    /// </summary>
    void DoSearch()
    {
      string searchString = "";

      //if (_setting.SearchHistory.Count > 0)
      //{
      //  GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      //  if (dlg == null) return;
      //  dlg.Reset();
      //  dlg.SetHeading(Translation.SearchHistory);
      //  dlg.Add(Translation.NewSearch);
      //  for (int i = _setting.SearchHistory.Count; i > 0; i--)
      //  {
      //    dlg.Add(_setting.SearchHistory[i - 1]);
      //  }
      //  dlg.DoModal(GetID);
      //  if (dlg.SelectedId == -1) return;
      //  searchString = dlg.SelectedLabelText;
      //  if (searchString == Translation.NewSearch)
      //    searchString = "";
      //}


      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)Window.WINDOW_VIRTUAL_KEYBOARD);

      if (null == keyboard) return;

      keyboard.Reset();
      keyboard.Text = searchString;
      keyboard.DoModal(GetID);
      if (keyboard.IsConfirmed)
      {
        // input confirmed -- execute the search
        searchString = keyboard.Text;
      }

      if ("" != searchString)
      {
        _foundArtists.Clear();
        _foundArtists = SearchOnArtist(searchString);
        if (_foundArtists != null)
        {
          // If only one artist returned display the tracks
          if (_foundArtists.Count == 1)
            LoadTracksForArtist(_foundArtists);
          else
            LoadSearchedArtists(_foundArtists, artistSort);
        }
      }

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

      if (control == btnLayouts)
      {
        mvCentralCore.Settings.DefaultView = ((int)CurrentLayout).ToString();
      }

      switch (controlId)
      {
        case (int)GuiControls.PlayAllRandom:
          playRandomAll();
          break;
        case (int)GuiControls.PlaySmart:
          playSmart(ChooseSmartPlay());
          break;
        case (int)GuiControls.PlayList:
          GUIWindowManager.ActivateWindow(Player.GetWindowId());
          break;
        case (int)GuiControls.StatsAndInfo:
          GUIWindowManager.ActivateWindow(GUImvStatsAndInfo.GetWindowId());
          break;
        case (int)GuiControls.GenreConfig:
          SetFavoriteTags();
          break;
        case (int)GuiControls.Search:
          DoSearch();
          break;
        case (int)GuiControls.Facade:
          //Clicked on something in the facade
          logger.Debug("Hit Key : " + actionType.ToString());
          switch (actionType)
          {
            case Action.ActionType.ACTION_PLAY:
            case Action.ActionType.ACTION_PAUSE:
            case Action.ActionType.ACTION_QUEUE_ITEM:
            case Action.ActionType.ACTION_MUSIC_PLAY:
            case Action.ActionType.ACTION_SELECT_ITEM:

              if (facadeLayout.SelectedListItem.IsFolder && ((actionType == Action.ActionType.ACTION_MUSIC_PLAY) || (actionType == Action.ActionType.ACTION_PLAY) || (actionType == Action.ActionType.ACTION_PAUSE)))
              {
                // Are we on an Artist or Album, if Artist add all tracks by artist to playlist else if album add all tracks on album to playlist
                if (facadeLayout.SelectedListItem.MusicTag != null && facadeLayout.SelectedListItem.MusicTag.GetType() == typeof(DBAlbumInfo))
                  AlbumActions(actionType);
                else if (facadeLayout.SelectedListItem.MusicTag != null && facadeLayout.SelectedListItem.MusicTag.GetType() == typeof(DBArtistInfo))
                  ArtistActions(actionType);
                else if (facadeLayout.SelectedListItem.MusicTag != null && facadeLayout.SelectedListItem.MusicTag.GetType() == typeof(DBGenres))
                  GenreActions(actionType);
              }
              else
              {

                GUIListItem selectedItem = facadeLayout.SelectedListItem;

                if (selectedItem.MusicTag.GetType() == typeof(DBGenres))
                {
                  DisplayByGenre(facadeLayout.SelectedListItem.Label);
                  break;
                }


                if (!selectedItem.IsFolder && selectedItem.MusicTag != null)
                {
                  // we have a track selected so add any other tracks which are on showing on the facade
                  List<DBTrackInfo> list1 = new List<DBTrackInfo>();
                  for (int i = 0; i < facadeLayout.ListLayout.Count; i++)
                  {
                    GUIListItem trackItem = facadeLayout.ListLayout[i];

                    if (!trackItem.IsFolder && trackItem.MusicTag != null)
                    {
                      list1.Add((DBTrackInfo)trackItem.MusicTag);
                    }
                  }
                  AddToPlaylist(list1, false, true, false);
                  Player.playlistPlayer.Play(list1.IndexOf((DBTrackInfo)selectedItem.MusicTag));
                  if (mvCentralCore.Settings.AutoFullscreen)
                    g_Player.ShowFullScreenWindow();
                  break;
                }
                //return to previous level
                if (facadeLayout.SelectedListItem.Label == "..")
                {
                  loadCurrent();
                }
                else
                {
                  artistID = facadeLayout.SelectedListItem.ItemId;
                  _currentView = MvView.Video;
                  addToStack(_currentView, false);
                  loadCurrent();
                }
              }
              break;
          }
          break;
      }
    }
    /// <summary>
    /// Initial load of GUI
    /// </summary>
    protected override void OnPageLoad()
    {

      InitViewSelections();
      UpdateButtonStates();
      // If we have a video running then chances are we are exiting fullt screen...save the view as we need to go though
      // the page setup and that would messup the view and window stack.
      if (_persisting && _currentView != MvView.None)
        _runningView = _currentView;

      // Get all Artists and Tracks
      List<DBArtistInfo> artList = DBArtistInfo.GetAll();
      List<DBTrackInfo> vidList = DBTrackInfo.GetAll();

      if (artList.Count == 0 && vidList.Count == 0)
      {
        GUIPropertyManager.SetProperty("#mvCentral.ViewAs", Localization.Artists);
        GUIPropertyManager.SetProperty("#mvCentral.Hierachy", "Empty DB");
        UserMessage("mvCentral - No Content", "There is no content to view", "", "Please setup plugin and scan in configuration");
        _currentView = MvView.None;
        addToStack(_currentView, true);
        return;
      }

      // Set Total Artists and Video Porperties
      GUIPropertyManager.SetProperty("#mvCentral.TotalArtists", artList.Count + " " + Localization.Artists);
      GUIPropertyManager.SetProperty("#mvCentral.TotalVideos", vidList.Count + " " + Localization.Videos);

      // set initial view to artists - default to artist is issues with reading value
      try
      {
        _currentView = (MvView)int.Parse(mvCentralCore.Settings.DefaultViewAs);
      }
      catch
      {
        _currentView = MvView.Artist;
        mvCentralCore.Settings.DefaultViewAs = ((int)_currentView).ToString();
      }
      // Check for invalid view - should actually store the corrected one against checking here...on the todo list :)
      //
      // Check for genres view select but none now defined
      if ((DBGenres.GetSelected().Count == 0 && _currentView == MvView.Genres))
      {
        _currentView = MvView.Artist;
        mvCentralCore.Settings.DefaultViewAs = ((int)_currentView).ToString();
      }
      // Also check for albums
      if ((DBAlbumInfo.GetAll().Count == 0 && _currentView == MvView.AllAlbums))
      {
        _currentView = MvView.Artist;
        mvCentralCore.Settings.DefaultViewAs = ((int)_currentView).ToString();
      }
      // Invalid start up view check - do not the album that was last viewed so set allAlbums
      if (_currentView == MvView.VideosOnAlbum)
        _currentView = MvView.AllAlbums;
      // and dont know what the arist was so default to artist view
      if (_currentView == MvView.ArtistViaGenre)
        _currentView = MvView.Artist;

      // Exit from fullscreen - restore save view and dont re-init window stack
      if (_persisting && _runningView != MvView.None)
        _currentView = _runningView;
      else
      {
        addToStack(_currentView, true);
      }
      setViewAsProperty(_currentView);
      logger.Info("GUI - Loaded ViewAs : {0}", _currentView.ToString());

      // Read last used layout from and set, default to list if not yet stored
      if (mvCentralCore.Settings.DefaultView == "lastused")
      {
        CurrentLayout = Layout.List;
        mvCentralCore.Settings.DefaultView = ((int)CurrentLayout).ToString();
      }
      else
        CurrentLayout = (Layout)int.Parse(mvCentralCore.Settings.DefaultView);

      logger.Info("GUI - Loaded Layout : {0}", CurrentLayout.ToString());

      SwitchLayout();
      UpdateButtonStates();

      GUIPropertyManager.Changed = true;

      if (btnSortBy != null)
      {
        btnSortBy.SortChanged += new SortEventHandler(SortChanged);
      }

      if (_loadParameter != null)
        processParameter();

      _persisting = true;


      base.OnPageLoad();
    }

    #endregion

    #region Public Methods

    public MvGuiMain()
    {
      timeOut.Tick += new EventHandler(checkTime);
    }

    public void SetFocusItem(int index)
    {
      facadeLayout.SelectedListItemIndex = index;
    }


    #endregion

    #region Private Methods

    void updateToneStylesAndComnpoers()
    {
      // Get all Artists and Tracks
      List<DBArtistInfo> artList = DBArtistInfo.GetAll();
      List<DBTrackInfo> vidList = DBTrackInfo.GetAll();

      //Update Tones, Styles & Genres
      logger.Debug("Update Tones, Styles and Genre");
      foreach (DBArtistInfo artistData in artList)
      {
        // Styles
        if (artistData.Styles.Trim().Length > 0)
        {
          string[] styles = artistData.Styles.Split(',');
          foreach (string style in styles)
          {
            if (DBTonesAndStyles.Get("S", style.Trim()) == null)
              DBTonesAndStyles.Add("S", style.Trim());
          }
        }
        // Tones
        if (artistData.Tones.Trim().Length > 0)
        {
          string[] tones = artistData.Tones.Split(',');
          foreach (string tone in tones)
          {
            if (DBTonesAndStyles.Get("T", tone.Trim()) == null)
              DBTonesAndStyles.Add("T", tone.Trim());
          }
        }
        //Genre - Inset Genre into genre table if not already there
        if (artistData.Genre.Trim().Length > 0)
        {
          if (DBGenres.Get(artistData.Genre.Trim()) == null)
            DBGenres.Add(true, artistData.Genre.Trim());
        }
      }
      logger.Debug("Update Tones, Styles and Genre Complete - Update Composers");
      // Update composer DB
      DBComposers.ClearAll();
      foreach (DBTrackInfo trackData in vidList)
      {
        // Skip if no composer data
        if (trackData.Composers.Trim().Length == 0)
          continue;
        // Split composers string and add if we do not already have them
        string[] composers = trackData.Composers.Split('|');
        foreach (string composer in composers)
        {
          if (DBComposers.Get(composer.Trim()) == null)
            DBComposers.Add(composer.Trim());
        }
      }
      logger.Debug("Composers Update complete");

    }


    /// <summary>
    /// Process the hyperlink parmeter(s)
    /// </summary>
    private void processParameter()
    {
      // Exit if we dont have a parameter
      if (_loadParameter == null)
        return;
      // Parameter must be in the form <command>:<parameter>, return if not
      if (!_loadParameter.Contains(":"))
        return;
      // Extract command and parameter
      string command = _loadParameter.Split(':')[0].Trim().ToUpper();
      string param = _loadParameter.Split(':')[1].Trim();
      // and process
      if (command == "ARTISTVIDEOS")
      {
        if (string.IsNullOrEmpty(param))
          return;
        _foundArtists = SearchOnArtist(param);
        if (_foundArtists != null)
        {
          // If only one artist returned display the tracks
          if (_foundArtists.Count == 1)
            LoadTracksForArtist(_foundArtists);
          else // Display a list of found artists
            LoadSearchedArtists(_foundArtists, artistSort);
        }
      }
    }
    /// <summary>
    /// Set the localised View As string
    /// </summary>
    /// <param name="viewAs"></param>
    private void setViewAsProperty(MvView viewAs)
    {
      switch (viewAs)
      {
        case MvView.Artist:
          GUIPropertyManager.SetProperty("#mvCentral.ViewAs", Localization.Artists);
          break;
        case MvView.AllAlbums:
          GUIPropertyManager.SetProperty("#mvCentral.ViewAs", Localization.Albums);
          break;
        case MvView.AllVideos:
          GUIPropertyManager.SetProperty("#mvCentral.ViewAs", Localization.Videos);
          break;
        case MvView.Genres:
          GUIPropertyManager.SetProperty("#mvCentral.ViewAs", Localization.Genre);
          break;
        case MvView.DvdView:
          GUIPropertyManager.SetProperty("#mvCentral.ViewAs", "Music DVD");
          break;
      }
      GUIPropertyManager.Changed = true;
    }
    /// <summary>
    /// Add current view to the screen navigation stack and optioanlly reset the stack if requested
    /// </summary>
    /// <param name="activeView"></param>
    /// <param name="resetStack"></param>
    private void addToStack(MvView activeView, bool resetStack)
    {
      if (resetStack)
        screenStack.Clear();

      if (TopWindow == MvView.AllAlbums && activeView == MvView.Video)
        activeView = MvView.VideosOnAlbum;

      if (TopWindow == MvView.Genres && activeView == MvView.Artist)
        activeView = MvView.ArtistViaGenre;


      if (screenStack.Count == 0)
      {
        screenStack.Add(activeView);
        return;
      }

      if (!screenStack.Contains(activeView))
        screenStack.Add(activeView);
    }
    /// <summary>
    /// Get the previous menu in the stack
    /// </summary>
    /// <returns></returns>
    private MvView getPreviousView()
    {
      if (screenStack.Count == 1)
      {
        return MvView.None;
      }

      int index = screenStack.Count - 1;
      MvView lastView = screenStack[index - 1];
      screenStack.RemoveAt(index);
      logger.Debug("(getPreviousScreen)");
      foreach (MvView mvv in screenStack)
        logger.Debug("Read Stack > {0}",mvv.ToString());
      return lastView;
    }
    /// <summary>
    /// Return top window in stack
    /// </summary>
    public MvView TopWindow
    {
      get
      {
        if (screenStack.Count > 0)
          return screenStack[0];
        else
          return MvView.None;
      }
    }

    private void DebugMsg(string Message)
    {
      GUIDialogOK dlg = (GUIDialogOK)GUIWindowManager.GetWindow(
        (int)GUIWindow.Window.WINDOW_DIALOG_OK);
      dlg.SetHeading("DEBUG MESSAGE");
      dlg.SetLine(1, Message);
      dlg.SetLine(2, "");
      dlg.SetLine(3, "");
      dlg.DoModal(GUIWindowManager.ActiveWindow);
    }
    /// <summary>
    /// Display user message
    /// </summary>
    /// <param name="Message"></param>
    private void UserMessage(string Heading,string Message1, string Message2, string Message3)
    {
      GUIDialogOK dlg = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
      dlg.SetHeading(Heading);
      dlg.SetLine(1, Message1);
      dlg.SetLine(2, Message2);
      dlg.SetLine(3, Message3);
      dlg.DoModal(GUIWindowManager.ActiveWindow);

    }
    /// <summary>
    /// Show all found tags and allow to marked as favorite
    /// </summary>
    /// <returns></returns>
    private int SetFavoriteTags()
    {
      artistTags.Clear();
      foreach (DBArtistInfo artistData in DBArtistInfo.GetAll())
      {
        foreach (string artistTag in artistData.Tag)
        {
          if (!artistTags.Contains(artistTag))
          {
            if (artistTag != artistData.Artist)
              artistTags.Add(artistTag);
          }
        }
      }

      artistTags.Sort(delegate(string p1, string p2) { return p1.CompareTo(p2); });
      GUIDialogMultiSelect dlgMenu = (GUIDialogMultiSelect)GUIWindowManager.GetWindow(112014);
      if (dlgMenu != null)
      {
        dlgMenu.Reset();
        dlgMenu.SetHeading(mvCentralCore.Settings.HomeScreenName + " - " + Localization.TagsToGenre);

        foreach (string artistTag in artistTags)
        {
          GUIListItem pItem = new GUIListItem(artistTag);
          pItem.TVTag = artistTag;
          DBGenres _genre = DBGenres.Get(artistTag);

          if (_genre != null && _genre.Enabled)
            pItem.Selected = true;
          else
            pItem.Selected = false;

          dlgMenu.Add(pItem);
        }
      }
      dlgMenu.DoModal(GetID);
      DBGenres.ClearAll();
      // Insert any AllMusic Genres
      foreach (DBArtistInfo artistData in DBArtistInfo.GetAll())
      {
        //Genre - Inset Genre into genre table if not already there
        if (artistData.Genre.Trim().Length > 0)
        {
          if (artistData.Genre.Contains(','))
          {
            string[] _genres = artistData.Genre.Split(',');
            foreach (string _genre in _genres)
            {
              if (DBGenres.Get(_genre.Trim()) == null)
                DBGenres.Add(true, _genre.Trim());
            }
          }
          else
          {
            if (DBGenres.Get(artistData.Genre.Trim()) == null)
              DBGenres.Add(true, artistData.Genre.Trim());
          }
        }
      }
      // Now add any selected Last.FM tags as genres
      for (int i = 0; i < dlgMenu.ListItems.Count; i++)
      {
        if (dlgMenu.ListItems[i].Selected)
          DBGenres.Add(true, dlgMenu.ListItems[i].TVTag.ToString().Trim());
        else
          DBGenres.Add(false, dlgMenu.ListItems[i].TVTag.ToString().Trim());
      }

      if (dlgMenu.SelectedLabel == -1) // Nothing was selected
        return -1;

      return dlgMenu.SelectedLabel;
    }
    /// <summary>
    /// Show the Conext Menu
    /// </summary>
    /// <returns></returns>
    private string ShowContextMenu()
    {
      GUIDialogMenu dlgMenu = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlgMenu != null)
      {
        dlgMenu.Reset();
        dlgMenu.SetHeading(mvCentralCore.Settings.HomeScreenName + " - " + Localization.ContextMenu);
        if (this.facadeLayout.Count > 0)
        {
          dlgMenu.Add(Localization.AddToPlaylist);
          dlgMenu.Add(Localization.AddAllToPlaylist);
          if (Player.playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MVCENTRAL).Count > 0 && !(facadeLayout.ListLayout.SelectedListItem.IsFolder))
          {
            dlgMenu.Add(Localization.AddToPlaylistNext);
          }
          dlgMenu.Add(Localization.RefreshArtwork);
          if (!facadeLayout.SelectedListItem.IsFolder)
          dlgMenu.Add(Localization.RateVid);
        }
        dlgMenu.DoModal(GetID);

        if (dlgMenu.SelectedLabel == -1) // Nothing was selected
          return null;

        return dlgMenu.SelectedLabelText;
      }
      return null;
    }
    /// <summary>
    /// Load the current view and set the idex to the last used.
    /// </summary>
    private void loadCurrent()
    {
      string thisArtist = "NULL";
      if (CurrentArtistInfo != null)
         thisArtist = CurrentArtistInfo.Artist;

      logger.Debug("In loadCurrent, View: {0},  Current Artist:{1}, and ID: {2}", _currentView.ToString(), thisArtist, CurrentArtistId.ToString());
      _persisting = true;     
      switch (_currentView)
      {
        case MvView.Artist:         
          loadArtists(artistSort);
          facadeLayout.SelectedListItemIndex = lastItemArt;
          break;
        case MvView.Album:
        case MvView.Video:
          if (TopWindow == MvView.AllAlbums && screenStack.Count == 1)
          {
            loadAllAlbums();
            facadeLayout.SelectedListItemIndex = lastItemAlb;
          }
          else
          {
            loadVideos(artistID, _videoSort);
            facadeLayout.SelectedListItemIndex = lastItemVid;
          }
          break;
        case MvView.AllAlbums:
          loadAllAlbums();
          facadeLayout.SelectedListItemIndex = lastItemAlb;
          break;
        case MvView.AllVideos:
          loadAllVideos(_videoSort);
          facadeLayout.SelectedListItemIndex = lastItemVid;
          break;
        case MvView.VideosOnAlbum:
          //artistID = currentArtistID;
          loadVideos(artistID, _videoSort);
          facadeLayout.SelectedListItemIndex = lastItemVid;
          break;
        case MvView.Genres:
          //artistID = currentArtistID;
          loadGenres();
          facadeLayout.SelectedListItemIndex = lastItemVid;
          break;
        case MvView.DvdView:
          loadDVDs();
          facadeLayout.SelectedListItemIndex = lastItemVid;
          break;
        case MvView.ArtistViaGenre:
          //artistID = currentArtistID;
          DisplayByGenre(_lastViewedGenre);
          facadeLayout.SelectedListItemIndex = lastGenreItem;
          break;
        case MvView.ArtistTracks:
          // If only one artist returned display the tracks
          if (_foundArtists.Count == 1)
            LoadTracksForArtist(_foundArtists);
          else
            LoadSearchedArtists(_foundArtists, artistSort); 
          break;
        case MvView.SearchedArtists:
          // If only one artist returned display the tracks
          if (_foundArtists.Count == 1)
            LoadTracksForArtist(_foundArtists);
          else
            LoadSearchedArtists(_foundArtists, artistSort);    
          break;

      }
      addToStack(_currentView, false);
    }
    /// <summary>
    /// Load artists
    /// </summary>
    private void loadArtists(MvSort sortDirection)
    {
      artistSort = sortDirection;

      // set the view
      List<DBArtistInfo> artistList = DBArtistInfo.GetAll();
      GUIPropertyManager.SetProperty("#mvCentral.Hierachy", Localization.Artists);
      GUIPropertyManager.SetProperty("#itemcount", artistList.Count.ToString());
      GUIPropertyManager.SetProperty("#itemtype", Localization.Artists);
      GUIPropertyManager.Changed = true;

      // Sort Artists
      if (sortDirection == MvSort.Ascending)
        artistList.Sort(delegate(DBArtistInfo p1, DBArtistInfo p2) { return p1.Artist.CompareTo(p2.Artist); });
      else
        artistList.Sort(delegate(DBArtistInfo p1, DBArtistInfo p2) { return p2.Artist.CompareTo(p1.Artist); });

      // Clear the facade and load the artists
      GUIControl.ClearControl(GetID, facadeLayout.GetID);
      foreach (DBArtistInfo artistData in artistList)
      {
        GUIListItem facadeItem = new GUIListItem();

        facadeItem.Label = artistData.Artist;

        if (string.IsNullOrEmpty(artistData.ArtFullPath.Trim()))
          facadeItem.ThumbnailImage = artistTrackArt(artistData);
        else
          facadeItem.ThumbnailImage = artistData.ArtFullPath;

        facadeItem.TVTag = mvCentralUtils.bioNoiseFilter(artistData.bioContent);
        facadeItem.AlbumInfoTag = mvCentralUtils.bioNoiseFilter(artistData.bioContent);
        facadeItem.ItemId = (int)artistData.ID;
        facadeItem.IsFolder = true;
        facadeItem.MusicTag = artistData;
        facadeItem.OnItemSelected += new GUIListItem.ItemSelectedHandler(onArtistSelected);
        facadeLayout.Add(facadeItem);

        foreach (string artistTag in artistData.Tag)
        {
          if (!artistTags.Contains(artistTag))
          {
            if (artistTag != artistData.Artist)
              artistTags.Add(artistTag);
          }
        }
      }
      artistTags.Sort(delegate(string p1, string p2) { return p1.CompareTo(p2); });

      // If first time though set properites to first item in facade
      if (facadeLayout.Count > 0 && !_persisting)
      {
        facadeLayout.SelectedListItemIndex = 0;
        onArtistSelected(facadeLayout.SelectedListItem, facadeLayout);
      }
      _persisting = true;
      GUIPropertyManager.SetProperty("#mvCentral.ArtistView", "true");
      GUIPropertyManager.SetProperty("#mvCentral.TrackView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.AlbumView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.GenreView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.DVDView", "false");

      clearVideoAudioProps();
      GUIPropertyManager.Changed = true;


    }
    /// <summary>
    /// Return artwork for the first track to have it, used in event of Artist having no art.
    /// </summary>
    /// <param name="artist"></param>
    /// <returns></returns>
    string artistTrackArt(DBArtistInfo artist)
    {
      List<DBTrackInfo> artistTracks = DBTrackInfo.GetEntriesByArtist(artist);
      foreach (DBTrackInfo artistTrack in artistTracks)
      {
        if (!string.IsNullOrEmpty(artistTrack.ArtFullPath.Trim()))
          return artistTrack.ArtFullPath;
      }
      return "defaultArtistBig.png";
    }
    /// <summary>
    /// Load and Display all Videos
    /// </summary>
    /// <param name="sortOrder"></param>
    private void loadAllVideos(MvSort sortOrder)
    {
      _videoSort = sortOrder;

      // Get list of all videos
      List<DBTrackInfo> artistTrackList = DBTrackInfo.GetAll();

      // Sort the tracks
      if (sortOrder == MvSort.Ascending)
        artistTrackList.Sort(delegate(DBTrackInfo p1, DBTrackInfo p2) { return p1.Track.CompareTo(p2.Track); });
      else
        artistTrackList.Sort(delegate(DBTrackInfo p1, DBTrackInfo p2) { return p2.Track.CompareTo(p1.Track); });

      // Clear the facade
      GUIControl.ClearControl(GetID, facadeLayout.GetID);

      // Load tracks into facade
      foreach (DBTrackInfo trackData in artistTrackList)
      {
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

        facadeItem.TVTag = mvCentralUtils.bioNoiseFilter(trackData.bioContent);
        facadeItem.Path = trackData.LocalMedia[0].File.FullName;
        facadeItem.IsFolder = false;
        facadeItem.OnItemSelected += new GUIListItem.ItemSelectedHandler(onVideoSelected);
        facadeItem.MusicTag = trackData;
        facadeItem.Rating = trackData.Rating;
        // If no thumbnail set a default
        if (!string.IsNullOrEmpty(trackData.ArtFullPath.Trim()))
          facadeItem.ThumbnailImage = trackData.ArtFullPath;
        else
          facadeItem.ThumbnailImage = "defaultVideoBig.png";

        facadeLayout.Add(facadeItem);
      }
      // Set properities to first item in list
      if (facadeLayout.Count > 0 && !_persisting)
      {
        facadeLayout.SelectedListItemIndex = 0;
        onVideoSelected(facadeLayout.SelectedListItem, facadeLayout);
      }
      _persisting = true;
      GUIPropertyManager.SetProperty("#itemcount", facadeLayout.Count.ToString());
      GUIPropertyManager.SetProperty("#itemtype", Localization.Videos);
      GUIPropertyManager.SetProperty("#mvCentral.Hierachy", Localization.AllVideos);
      GUIPropertyManager.SetProperty("#mvCentral.ArtistView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.TrackView", "true");
      GUIPropertyManager.SetProperty("#mvCentral.AlbumView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.GenreView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.DVDView", "false");
      clearVideoAudioProps();
      GUIPropertyManager.Changed = true;

      // Set properities to first item in list
      if (facadeLayout.Count > 0)
      {
        if (lastItemVid > 0 && lastItemVid < facadeLayout.Count)
        {
          facadeLayout.SelectedListItemIndex = lastItemVid;
          logger.Debug("(loadAllVideos) Facade Selected Index set to {0}", facadeLayout.SelectedListItemIndex);
          onVideoSelected(facadeLayout.SelectedListItem, facadeLayout);
        }
        else
        {
          facadeLayout.SelectedListItemIndex = 0;
          logger.Debug("(loadAllVideos) Facade Selected Index set to {0}", facadeLayout.SelectedListItemIndex);
          onVideoSelected(facadeLayout.SelectedListItem, facadeLayout);
        }
      }
    }

    /// <summary>
    /// Load Videos
    /// </summary>
    /// <param name="ArtistID"></param>
    private void loadVideos(int ArtistID, MvSort sortOrder)
    {
      DBAlbumInfo db1 = null;
      _videoSort = sortOrder;

      GUIPropertyManager.SetProperty("#mvCentral.Hierachy", Localization.Videos + " | " + DBArtistInfo.Get(ArtistID));
      GUIPropertyManager.Changed = true;
      // Load the albums (Not used Currently)
      if (facadeLayout.SelectedListItem == null)
      {
        if (albumID != 0 && !mvCentralCore.Settings.DisableAlbumSupport)
        {
          LoadTracksOnAlbum(albumID);
          GUIPropertyManager.SetProperty("#mvCentral.Hierachy", Localization.Albums + " | " + DBArtistInfo.Get(ArtistID));
          return;
        }
      }
      // If we are on an artist - load the album (Not Used Currently) - *** Possible Error ***
      if (facadeLayout.SelectedListItem != null && !mvCentralCore.Settings.DisableAlbumSupport)
      {
        if ((facadeLayout.SelectedListItem.MusicTag != null && facadeLayout.SelectedListItem.MusicTag.GetType() == typeof(DBAlbumInfo)))
        {

          if (_layoutChanging)
          {
            db1 = DBAlbumInfo.Get(albumID);
          }
          else
          {
            db1 = (DBAlbumInfo)facadeLayout.SelectedListItem.MusicTag;
            albumID = db1.ID.Value;
          }
          LoadTracksOnAlbum(db1.ID.Value);
          GUIPropertyManager.SetProperty("#mvCentral.Hierachy", Localization.Albums + " | " + db1.Album);
          GUIPropertyManager.SetProperty("#mvCentral.ArtistView", "false");
          GUIPropertyManager.SetProperty("#mvCentral.TrackView", "false");
          GUIPropertyManager.SetProperty("#mvCentral.AlbumView", "true");
          GUIPropertyManager.SetProperty("#mvCentral.GenreView", "false");
          GUIPropertyManager.SetProperty("#mvCentral.DVDView", "false");
          GUIPropertyManager.Changed = true;
          return;
        }
      }

      //// Grab the info for the currently selected artist
      //currArtist = DBArtistInfo.Get(ArtistID);

      ////  and store it
      //currentArtistID = ArtistID;
      // Load all videos for selected artist
      List<DBTrackInfo> artistTrackList = DBTrackInfo.GetEntriesByArtist(_currArtist);
      // And sort
      if (sortOrder == MvSort.Ascending)
        artistTrackList.Sort(delegate(DBTrackInfo p1, DBTrackInfo p2) { return p1.Track.CompareTo(p2.Track); });
      else
        artistTrackList.Sort(delegate(DBTrackInfo p1, DBTrackInfo p2) { return p2.Track.CompareTo(p1.Track); });

      this.artistID = ArtistID;
      // Clear facade 
      GUIControl.ClearControl(GetID, facadeLayout.GetID);
      // and load tracks if we dont already have them loaded
      foreach (DBTrackInfo trackData in artistTrackList)
      {
        // If no Album is associated or Album support is disabled then skip to next track
        if (trackData.AlbumInfo.Count == 0 || mvCentralCore.Settings.DisableAlbumSupport)
          continue;
        //We have an album
        DBAlbumInfo theAlbum = DBAlbumInfo.Get(trackData);

        // Check if we already have this album as a facade item
        bool isPresent = false;
        for (int i = 0; i <= facadeLayout.Count - 1; i++)
        {
          if (facadeLayout[i].Label == trackData.AlbumInfo[0].Album)
            isPresent = true;
        }
        // and skip adding if we do
        if (isPresent)
          continue;


        // Add Album to facade
        GUIListItem item = new GUIListItem();
        item.Label = theAlbum.Album;

        if (string.IsNullOrEmpty(theAlbum.ArtFullPath.Trim()))
          item.ThumbnailImage = "defaultVideoBig.png";
        else
          item.ThumbnailImage = theAlbum.ArtFullPath;

        item.TVTag = mvCentralUtils.bioNoiseFilter(theAlbum.bioContent);
        _selArtist = _currArtist.Artist;
        item.IsFolder = true;
        item.OnItemSelected += new GUIListItem.ItemSelectedHandler(onVideoSelected);
        item.MusicTag = theAlbum;
        item.Rating = theAlbum.Rating;
        facadeLayout.Add(item);

      }
      // Load tracks we don't have loaded
      foreach (DBTrackInfo trackData in artistTrackList)
      {
        // if this track is part of an album and we have not disabled Album support then skip the adding track to the facade
        if (trackData.AlbumInfo.Count > 0 && !mvCentralCore.Settings.DisableAlbumSupport)
          continue;

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

        facadeItem.TVTag = mvCentralUtils.bioNoiseFilter(trackData.bioContent);
        _selArtist = _currArtist.Artist;
        facadeItem.Path = trackData.LocalMedia[0].File.FullName;
        facadeItem.IsFolder = false;
        facadeItem.OnItemSelected += new GUIListItem.ItemSelectedHandler(onVideoSelected);
        facadeItem.MusicTag = trackData;
        facadeItem.Rating = trackData.Rating;
        // If no thumbnail set a default
        if (!string.IsNullOrEmpty(trackData.ArtFullPath.Trim()))
          facadeItem.ThumbnailImage = trackData.ArtFullPath;
        else
          facadeItem.ThumbnailImage = "defaultVideoBig.png";

        facadeLayout.Add(facadeItem);
      }

      //facadeLayout.Sort(new GUIListItemVideoComparer(SortingFields.Artist, SortingDirections.Ascending));

      
      
      // Set properities to first item in list
      if (facadeLayout.Count > 0)
      {
        if (lastItemVid > 0 && lastItemVid < facadeLayout.Count)
          facadeLayout.SelectedListItemIndex = lastItemVid;
        else
          facadeLayout.SelectedListItemIndex = 0;

        logger.Debug("(loadVideos) Facade Selected Index set to {0}", facadeLayout.SelectedListItemIndex);
        onVideoSelected(facadeLayout.SelectedListItem, facadeLayout);
      }
      _persisting = true;
      GUIPropertyManager.SetProperty("#mvCentral.Hierachy", Localization.Albums);
      GUIPropertyManager.SetProperty("#itemcount", facadeLayout.Count.ToString());
      GUIPropertyManager.SetProperty("#itemtype", Localization.Albums);
      // Set the view
      GUIPropertyManager.SetProperty("#mvCentral.ArtistView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.TrackView", "true");
      GUIPropertyManager.SetProperty("#mvCentral.AlbumView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.GenreView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.DVDView", "false");
      clearVideoAudioProps();
      GUIPropertyManager.Changed = true;
    }
    /// <summary>
    /// Load and Display All Albums
    /// </summary>
    private void loadAllAlbums()
    {
      GUIPropertyManager.SetProperty("#mvCentral.Hierachy", Localization.Albums);
      List<DBAlbumInfo> allAlbumsList = DBAlbumInfo.GetAll();

      // Clear facade and load tracks if we dont already have them loaded
      GUIControl.ClearControl(GetID, facadeLayout.GetID);

      // Load all the albums into the facade
      foreach (DBAlbumInfo theAlbum in allAlbumsList)
      {
        GUIListItem item = new GUIListItem();
        item.Label = theAlbum.Album;
        item.ItemId = (int)theAlbum.ID;

        if (string.IsNullOrEmpty(theAlbum.ArtFullPath))
          item.ThumbnailImage = "defaultAlbum.png";
        else
          item.ThumbnailImage = theAlbum.ArtFullPath;

        item.OnItemSelected += new GUIListItem.ItemSelectedHandler(onVideoSelected);
        item.TVTag = mvCentralUtils.bioNoiseFilter(theAlbum.bioContent);
        item.IsFolder = true;
        item.MusicTag = theAlbum;
        facadeLayout.Add(item);
      }
      GUIPropertyManager.SetProperty("#mvCentral.Hierachy", Localization.Videos);
      GUIPropertyManager.SetProperty("#itemcount", facadeLayout.Count.ToString());
      GUIPropertyManager.SetProperty("#itemtype", Localization.Albums);
      //
      GUIPropertyManager.SetProperty("#mvCentral.ArtistView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.TrackView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.AlbumView", "true");
      GUIPropertyManager.SetProperty("#mvCentral.GenreView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.DVDView", "false");
      clearVideoAudioProps();
      GUIPropertyManager.Changed = true;

      // Set properities to first item in list
      if (facadeLayout.Count > 0)
      {
        if (lastItemAlb > 0 && lastItemAlb < facadeLayout.Count)
          facadeLayout.SelectedListItemIndex = lastItemAlb;
        else
          facadeLayout.SelectedListItemIndex = 0;

        logger.Debug("(loadAllAlbums) Facade Selected Index set to {0}", facadeLayout.SelectedListItemIndex);
        onVideoSelected(facadeLayout.SelectedListItem, facadeLayout);
      }
      _persisting = true;
    }
    /// <summary>
    /// Create facade with the artists supplied in the list
    /// </summary>
    /// <param name="artistObjectList"></param>
    private void LoadSearchedArtists(List<DBArtistInfo> artistList, MvSort sortDirection)
    {
      // Set View
      _currentView = MvView.SearchedArtists;
      addToStack(_currentView, false);  

      GUIPropertyManager.SetProperty("#mvCentral.Hierachy", Localization.Artists);
      GUIPropertyManager.SetProperty("#itemcount", artistList.Count.ToString());
      GUIPropertyManager.SetProperty("#itemtype", Localization.Artists);
      GUIPropertyManager.Changed = true;

      // Sort Artists
      if (sortDirection == MvSort.Ascending)
        artistList.Sort(delegate(DBArtistInfo p1, DBArtistInfo p2) { return p1.Artist.CompareTo(p2.Artist); });
      else
        artistList.Sort(delegate(DBArtistInfo p1, DBArtistInfo p2) { return p2.Artist.CompareTo(p1.Artist); });

      // Clear facade and load tracks if we dont already have them loaded
      GUIControl.ClearControl(GetID, facadeLayout.GetID);

      foreach (DBArtistInfo artistData in artistList)
      {
        GUIListItem facadeItem = new GUIListItem();

        facadeItem.Label = artistData.Artist;
        if (string.IsNullOrEmpty(artistData.ArtFullPath.Trim()))
          facadeItem.ThumbnailImage = artistTrackArt(artistData);
        else
          facadeItem.ThumbnailImage = artistData.ArtFullPath;

        facadeItem.TVTag = mvCentralUtils.bioNoiseFilter(artistData.bioContent);
        facadeItem.AlbumInfoTag = mvCentralUtils.bioNoiseFilter(artistData.bioContent);
        facadeItem.ItemId = (int)artistData.ID;
        facadeItem.IsFolder = true;
        facadeItem.MusicTag = artistData;
        facadeItem.OnItemSelected += new GUIListItem.ItemSelectedHandler(onArtistSelected);
        facadeLayout.Add(facadeItem);

        foreach (string artistTag in artistData.Tag)
        {
          if (!artistTags.Contains(artistTag))
          {
            if (artistTag != artistData.Artist)
              artistTags.Add(artistTag);
          }
        }
      }
      artistTags.Sort(delegate(string p1, string p2) { return p1.CompareTo(p2); });

      // If first time though set properites to first item in facade
      if (facadeLayout.Count > 0 && !_persisting)
      {
        facadeLayout.SelectedListItemIndex = 0;
        onArtistSelected(facadeLayout.SelectedListItem, facadeLayout);
      }
      // Set focus to the facade
      GUIControl.FocusControl(GetID, facadeLayout.GetID);

      _persisting = true;
      GUIPropertyManager.SetProperty("#mvCentral.ArtistView", "true");
      GUIPropertyManager.SetProperty("#mvCentral.TrackView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.AlbumView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.GenreView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.DVDView", "false");
      clearVideoAudioProps();
      GUIPropertyManager.Changed = true;
    }
    /// <summary>
    /// Load and display tracks for this artist
    /// </summary>
    /// <param name="artistObject"></param>
    private void LoadTracksForArtist(List<DBArtistInfo> artistObjectList)
    {
      // Set View
      _currentView = MvView.ArtistTracks;
      addToStack(_currentView, false);

      // Clear facade and load tracks if we dont already have them loaded
      GUIControl.ClearControl(GetID, facadeLayout.GetID);

      // get the tracks by this artist

      foreach (DBArtistInfo artistObject in artistObjectList)
      {

        List<DBTrackInfo> tracksByArtist = DBTrackInfo.GetEntriesByArtist(artistObject);

        GUIPropertyManager.SetProperty("#mvCentral.Hierachy", Localization.Album + " | " + artistObject.Artist);


        foreach (DBTrackInfo track in tracksByArtist)
        {
          GUIListItem item = new GUIListItem();

          if (mvCentralCore.Settings.DisplayRawTrackText)
          {
            if (Path.GetFileName(track.LocalMedia[0].File.FullName).Contains("-"))
              item.Label = Regex.Match(Path.GetFileName(track.LocalMedia[0].File.FullName), @"(?:[\s-:;]{2,}|(?!.+?[\s-:;]{2,})\-)(?<track>[^\\$]*)\.").Groups["track"].Value;
            else
              item.Label = (Path.GetFileNameWithoutExtension(track.LocalMedia[0].File.FullName));

            item.Label = Regex.Replace(item.Label, @"\s*[{].*?[}]\s*", string.Empty, RegexOptions.IgnoreCase);

          }
          else
            item.Label = track.Track;

          // If no thumbnail set a default
          if (!string.IsNullOrEmpty(track.ArtFullPath.Trim()))
            item.ThumbnailImage = track.ArtFullPath;
          else
            item.ThumbnailImage = "defaultVideoBig.png";

          item.TVTag = mvCentralUtils.bioNoiseFilter(track.bioContent);
          item.Path = track.LocalMedia[0].File.FullName;
          item.IsFolder = false;
          item.OnItemSelected += new GUIListItem.ItemSelectedHandler(onVideoSelected);
          item.MusicTag = track;
          item.ItemId = (int)track.ID;
          item.Rating = track.Rating;
          facadeLayout.Add(item);
        }
      }
      // Always set index for first track
      if (facadeLayout.Count > 0)
      {
        if (lastItemVid > 0 && lastItemVid < facadeLayout.Count)
          facadeLayout.SelectedListItemIndex = lastItemVid;
        else
          facadeLayout.SelectedListItemIndex = 0;

        logger.Debug("(LoadTracksForArtist) Facade Selected Index set to {0}", facadeLayout.SelectedListItemIndex);

        onVideoSelected(facadeLayout.SelectedListItem, facadeLayout);
      }
      GUIControl.FocusControl(GetID, facadeLayout.GetID);

      GUIPropertyManager.SetProperty("#itemcount", facadeLayout.Count.ToString());
      GUIPropertyManager.SetProperty("#itemtype", Localization.Videos);
      // Set the view
      GUIPropertyManager.SetProperty("#mvCentral.ArtistView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.AlbumView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.TrackView", "true");
      GUIPropertyManager.SetProperty("#mvCentral.GenreView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.DVDView", "false");
      // Tell property manager we have changed something
      GUIPropertyManager.Changed = true;
    }
    /// <summary>
    /// Load Videos for this Album
    /// </summary>
    /// <param name="AlbumID"></param>
    private void LoadTracksOnAlbum(int AlbumID)
    {
      // Set View
      _currentView = MvView.VideosOnAlbum;
      addToStack(_currentView, false);

      GUIPropertyManager.SetProperty("#mvCentral.Hierachy", Localization.Album + " | " + DBAlbumInfo.Get(AlbumID));
      DBAlbumInfo currAlbum = DBAlbumInfo.Get(AlbumID);
      List<DBTrackInfo> trackList = DBTrackInfo.GetEntriesByAlbum(currAlbum);
      //
      this.albumID = AlbumID;
      // Clear facade and load tracks if we dont already have them loaded
      GUIControl.ClearControl(GetID, facadeLayout.GetID);
      foreach (DBTrackInfo track in trackList)
      {
        GUIListItem item = new GUIListItem();

        if (mvCentralCore.Settings.DisplayRawTrackText)
        {
          if (Path.GetFileName(track.LocalMedia[0].File.FullName).Contains("-"))
            item.Label = Regex.Match(Path.GetFileName(track.LocalMedia[0].File.FullName), @"(?:[\s-:;]{2,}|(?!.+?[\s-:;]{2,})\-)(?<track>[^\\$]*)\.").Groups["track"].Value;
          else
            item.Label = (Path.GetFileNameWithoutExtension(track.LocalMedia[0].File.FullName));

          item.Label = Regex.Replace(item.Label, @"\s*[{].*?[}]\s*", string.Empty, RegexOptions.IgnoreCase);
        }
        else
          item.Label = track.Track;

        if (string.IsNullOrEmpty(track.ArtFullPath.Trim()))
          item.ThumbnailImage = "defaultAlbum.png";
        else
          item.ThumbnailImage = track.ArtFullPath;

        item.TVTag = mvCentralUtils.bioNoiseFilter(track.bioContent);
        _selAlbum = currAlbum.Album;
        item.Path = track.LocalMedia[0].File.FullName;
        item.IsFolder = false;
        item.OnItemSelected += new GUIListItem.ItemSelectedHandler(onVideoSelected);
        item.MusicTag = track;
        item.ItemId = (int)currAlbum.ID;
        item.Rating = track.Rating;
        facadeLayout.Add(item);
      }
      // Always set index for first track
      if (facadeLayout.Count > 0 )
      {
        if (lastItemVid > 0 && lastItemVid < facadeLayout.Count)
          facadeLayout.SelectedListItemIndex = lastItemVid;
        else
          facadeLayout.SelectedListItemIndex = 0;

        logger.Debug("(LoadTracksOnAlbum) Facade Selected Index set to {0}", facadeLayout.SelectedListItemIndex);

        onVideoSelected(facadeLayout.SelectedListItem, facadeLayout);
      }
      GUIPropertyManager.SetProperty("#itemcount", facadeLayout.Count.ToString());
      GUIPropertyManager.SetProperty("#itemtype", Localization.Videos);
      // Set the view
      GUIPropertyManager.SetProperty("#mvCentral.ArtistView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.AlbumView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.TrackView", "true");
      GUIPropertyManager.SetProperty("#mvCentral.GenreView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.DVDView", "false");
      // Tell property manager we have changed something
      GUIPropertyManager.Changed = true;
    }
    /// <summary>
    /// Match genre with any last.fm tag in the Artist object
    /// </summary>
    /// <param name="genreTag"></param>
    /// <param name="artistData"></param>
    /// <returns></returns>
    private bool tagMatched(string genreTag, DBArtistInfo artistData)
    {
      foreach (string artistTag in artistData.Tag)
      {
        if (artistTag == genreTag)
        {
          return true;
        }
      }
      return false;
    }
    /// <summary>
    ///  Load the selected tags as Genres
    /// </summary>
    private void loadGenres()
    {
      List<DBGenres> genreList = DBGenres.GetSelected();
      genreList.Sort(delegate(DBGenres p1, DBGenres p2) { return p1.Genre.CompareTo(p2.Genre); });

      GUIPropertyManager.SetProperty("#mvCentral.Hierachy", Localization.Genre);
      GUIPropertyManager.SetProperty("#itemcount", genreList.Count.ToString());
      GUIPropertyManager.SetProperty("#itemtype", GUILocalizeStrings.Get(135));
      GUIPropertyManager.Changed = true;

      // Clear the facade and load the artists
      GUIControl.ClearControl(GetID, facadeLayout.GetID);
      foreach (DBGenres genreData in genreList)
      {
        GUIListItem facadeItem = new GUIListItem();

        facadeItem.Label = genreData.Genre;
        facadeItem.ThumbnailImage = "DefaultGenre.png";
        facadeItem.ItemId = (int)genreData.ID;
        facadeItem.IsFolder = true;
        facadeItem.MusicTag = genreData;
        facadeItem.OnItemSelected += new GUIListItem.ItemSelectedHandler(onGenreSelected);
        facadeLayout.Add(facadeItem);
      }
      //artistTags.Sort(delegate(string p1, string p2) { return p1.CompareTo(p2); });

      // If first time though set properites to first item in facade
      if (facadeLayout.Count > 0 && !_persisting)
      {
        facadeLayout.SelectedListItemIndex = 0;
        onGenreSelected(facadeLayout.SelectedListItem, facadeLayout);
      }
      _persisting = true;
      GUIPropertyManager.SetProperty("#mvCentral.GenreView", "true");
      GUIPropertyManager.SetProperty("#mvCentral.ArtistView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.TrackView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.AlbumView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.DVDView", "false");
      clearVideoAudioProps();
      GUIPropertyManager.Changed = true;
    }

    /// <summary>
    ///  Load the DVDs only
    /// </summary>
    private void loadDVDs()
    {

      List<DBTrackInfo> DVDList = DBTrackInfo.GetDVDEntries();

      // Sort the tracks
      DVDList.Sort(delegate(DBTrackInfo p1, DBTrackInfo p2) { return p1.Track.CompareTo(p2.Track); });


      // Clear the facade
      GUIControl.ClearControl(GetID, facadeLayout.GetID);

      // Load tracks into facade
      foreach (DBTrackInfo trackData in DVDList)
      {
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

        facadeItem.TVTag = mvCentralUtils.bioNoiseFilter(trackData.bioContent);
        facadeItem.Path = trackData.LocalMedia[0].File.FullName;
        facadeItem.IsFolder = false;
        facadeItem.OnItemSelected += new GUIListItem.ItemSelectedHandler(onDVDSelected);
        facadeItem.MusicTag = trackData;
        facadeItem.Rating = trackData.Rating;
        // If no thumbnail set a default
        if (!string.IsNullOrEmpty(trackData.ArtFullPath.Trim()))
          facadeItem.ThumbnailImage = trackData.ArtFullPath;
        else
          facadeItem.ThumbnailImage = "defaultVideoBig.png";

        facadeLayout.Add(facadeItem);
      }
      // Set properities to first item in list
      if (facadeLayout.Count > 0 && !_persisting)
      {
        facadeLayout.SelectedListItemIndex = 0;
        onVideoSelected(facadeLayout.SelectedListItem, facadeLayout);
      }
      _persisting = true;
      GUIPropertyManager.SetProperty("#itemcount", facadeLayout.Count.ToString());
      GUIPropertyManager.SetProperty("#itemtype", "DVDs");

      GUIPropertyManager.SetProperty("#mvCentral.ArtistView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.TrackView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.AlbumView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.GenreView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.DVDView", "true");
      clearVideoAudioProps();
      GUIPropertyManager.Changed = true;

      // Set properities to first item in list
      if (facadeLayout.Count > 0)
      {
        if (lastItemVid > 0 && lastItemVid < facadeLayout.Count)
        {
          facadeLayout.SelectedListItemIndex = lastItemVid;
          logger.Debug("(loadAllVideos) Facade Selected Index set to {0}", facadeLayout.SelectedListItemIndex);
          onVideoSelected(facadeLayout.SelectedListItem, facadeLayout);
        }
        else
        {
          facadeLayout.SelectedListItemIndex = 0;
          logger.Debug("(loadAllVideos) Facade Selected Index set to {0}", facadeLayout.SelectedListItemIndex);
          onVideoSelected(facadeLayout.SelectedListItem, facadeLayout);
        }
      }
    }

    /// <summary>
    /// Display Artists that match select genre
    /// </summary>
    private void DisplayByGenre(string genre)
    {
      _lastViewedGenre = genre;
      _currentView = MvView.Artist;
      addToStack(_currentView, false);

      List<DBArtistInfo> artistList = new List<DBArtistInfo>();
      List<DBArtistInfo> artistFullList = DBArtistInfo.GetAll();

      logger.Debug("Checking for matches for Genre : " + genre);
      foreach (DBArtistInfo artistInfo in artistFullList)
      {
        if (tagMatched(genre, artistInfo) || string.Equals(genre, artistInfo.Genre,StringComparison.OrdinalIgnoreCase))
        {
          if (!artistList.Contains(artistInfo))
            artistList.Add(artistInfo);
        }
      }
      
      GUIPropertyManager.SetProperty("#mvCentral.Hierachy", Localization.Genre + " | " + genre);
      GUIPropertyManager.SetProperty("#itemcount", artistList.Count.ToString());
      GUIPropertyManager.SetProperty("#itemtype", Localization.Artists);
      GUIPropertyManager.Changed = true;

      MvSort sortDirection = MvSort.Ascending;

      // Sort Artists
      if (sortDirection == MvSort.Ascending)
        artistList.Sort(delegate(DBArtistInfo p1, DBArtistInfo p2) { return p1.Artist.CompareTo(p2.Artist); });
      else
        artistList.Sort(delegate(DBArtistInfo p1, DBArtistInfo p2) { return p2.Artist.CompareTo(p1.Artist); });

      // Clear the facade and load the artists
      GUIControl.ClearControl(GetID, facadeLayout.GetID);
      foreach (DBArtistInfo artistData in artistList)
      {
        GUIListItem facadeItem = new GUIListItem();

        facadeItem.Label = artistData.Artist;
        if (string.IsNullOrEmpty(artistData.ArtFullPath.Trim()))
          facadeItem.ThumbnailImage = artistTrackArt(artistData);
        else
          facadeItem.ThumbnailImage = artistData.ArtFullPath;

        facadeItem.TVTag = mvCentralUtils.bioNoiseFilter(artistData.bioContent);
        facadeItem.AlbumInfoTag = mvCentralUtils.bioNoiseFilter(artistData.bioContent);
        facadeItem.ItemId = (int)artistData.ID;
        facadeItem.IsFolder = true;
        facadeItem.MusicTag = artistData;
        facadeItem.OnItemSelected += new GUIListItem.ItemSelectedHandler(onArtistSelected);
        facadeLayout.Add(facadeItem);

        foreach (string artistTag in artistData.Tag)
        {
          if (!artistTags.Contains(artistTag))
          {
            if (artistTag != artistData.Artist)
              artistTags.Add(artistTag);
          }
        }
      }
      artistTags.Sort(delegate(string p1, string p2) { return p1.CompareTo(p2); });

      // If first time though set properites to first item in facade
      if (facadeLayout.Count > 0)
      {
        facadeLayout.SelectedListItemIndex = 0;
        onArtistSelected(facadeLayout.SelectedListItem, facadeLayout);
      }
      GUIPropertyManager.SetProperty("#mvCentral.ArtistView", "true");
      GUIPropertyManager.SetProperty("#mvCentral.TrackView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.AlbumView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.GenreView", "false");
      GUIPropertyManager.Changed = true;
    }
    /// <summary>
    /// Perform action when Genre item selected
    /// </summary>
    /// <param name="item"></param>
    /// <param name="parent"></param>
    void onGenreSelected(GUIListItem item, GUIControl parent)
    {
      DBGenres selectGenre = (DBGenres)item.MusicTag;

      GUIPropertyManager.SetProperty("#mvCentral.ArtistBio", string.Empty);
      GUIPropertyManager.SetProperty("#mvCentral.TrackInfo", string.Empty);
      GUIPropertyManager.SetProperty("#mvCentral.Description", string.Empty);
      GUIPropertyManager.SetProperty("#mvCentral.ArtistTags", string.Empty);
      GUIPropertyManager.SetProperty("#mvCentral.BornOrFormed", string.Empty);

      GUIPropertyManager.SetProperty("#mvCentral.ArtistTracksRuntime", genreRunningTime(selectGenre));
      GUIPropertyManager.SetProperty("#mvCentral.VideosByArtist", _genreTracks.ToString());

      GUIPropertyManager.SetProperty("#mvCentral.Hierachy", GUILocalizeStrings.Get(135) + " | " + selectGenre.Genre);
      GUIPropertyManager.Changed = true;

      lastGenreItem = facadeLayout.SelectedListItemIndex;

    }
    /// <summary>
    /// Actions to perform when artist selected
    /// </summary>
    /// <param name="item"></param>
    /// <param name="parent"></param>
    void onArtistSelected(GUIListItem item, GUIControl parent)
    {
      if (item == null)
      {
        logger.Error("onArtistSelected - No Item!!!");
        return;
      }

      _currArtist = (DBArtistInfo)item.MusicTag;

      if (_currArtist == null)
      {
        logger.Error("Unable to get artist {0} !!", item.Label);
        return;
      }

      // Set the Bio Content
      if (string.IsNullOrEmpty(_currArtist.bioContent))
        GUIPropertyManager.SetProperty("#mvCentral.ArtistBio", string.Format(Localization.NoArtistBio, item.Label));
      else
        GUIPropertyManager.SetProperty("#mvCentral.ArtistBio", mvCentralUtils.bioNoiseFilter(_currArtist.bioContent));
      //mvCentralUtils.StripHTML(track.bioContent);
      
      // Set artist name and image
      GUIPropertyManager.SetProperty("#mvCentral.ArtistName", _currArtist.Artist);
      GUIPropertyManager.SetProperty("#mvCentral.ArtistImg", _currArtist.ArtFullPath);
      GUIPropertyManager.SetProperty("#mvCentral.VideosByArtist", DBTrackInfo.GetEntriesByArtist(_currArtist).Count.ToString());
      GUIPropertyManager.SetProperty("#mvCentral.ArtistTracksRuntime", runningTime(DBTrackInfo.GetEntriesByArtist(_currArtist)));
      // Set BornOrFormed property
      if (_currArtist.Formed == null)
        _currArtist.Formed = string.Empty;
      if (_currArtist.Born == null)
        _currArtist.Born = string.Empty;


      if (_currArtist.Formed.Trim().Length == 0 && _currArtist.Born.Trim().Length == 0)
        GUIPropertyManager.SetProperty("#mvCentral.BornOrFormed", "No Born/Formed Details");
      else if (_currArtist.Formed.Trim().Length == 0)
        GUIPropertyManager.SetProperty("#mvCentral.BornOrFormed", String.Format("{0}: {1}",Localization.Born, _currArtist.Born));
      else
        GUIPropertyManager.SetProperty("#mvCentral.BornOrFormed", String.Format("{0}: {1}",Localization.Formed, _currArtist.Formed));

      GUIPropertyManager.SetProperty("#mvCentral.Genre", _currArtist.Genre);

      // Artist Genres
      string artistTags = string.Empty;
      foreach (string tag in _currArtist.Tag)
        artistTags += tag + " | ";
      if (!string.IsNullOrEmpty(artistTags))
        GUIPropertyManager.SetProperty("#mvCentral.ArtistTags", artistTags.Remove(artistTags.Length - 2, 2));
      // Clear properites set for tracks
      clearVideoAudioProps();
      // Set the View
      GUIPropertyManager.SetProperty("#mvCentral.ArtistView", "true");
      GUIPropertyManager.SetProperty("#mvCentral.TrackView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.AlbumView", "false");
      // Let property manager knwo we have changes something
      GUIPropertyManager.Changed = true;
      // Store postions in facade
      lastItemArt = facadeLayout.SelectedListItemIndex;
      CurrentArtistInfo = _currArtist;
      CurrentArtistId = item.ItemId;

    }
    /// <summary>
    /// Video/Album item selected - set a load of properities
    /// </summary>
    /// <param name="item"></param>
    /// <param name="parent"></param>
    void onVideoSelected(GUIListItem item, GUIControl parent)
    {
      DBAlbumInfo albumInfo = null;
      DBTrackInfo trackInfo = null;

      // Is this a Track or Album object we are on
      if (item.MusicTag.GetType() == typeof(DBTrackInfo))
      {
        // This is an Video
        trackInfo = (DBTrackInfo)item.MusicTag;
        GUIPropertyManager.SetProperty("#mvCentral.VideoImg", item.ThumbnailImage);
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
          GUIPropertyManager.SetProperty("#mvCentral.Composers", trackInfo.Composers.Replace("|", ", "));
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

        // Get the artist 
        DBArtistInfo artistInfo = trackInfo.ArtistInfo[0];
        _currArtist = artistInfo;
        CurrentArtistId = item.ItemId;
        // And set some artist properites
        GUIPropertyManager.SetProperty("#mvCentral.ArtistName", artistInfo.Artist);
        GUIPropertyManager.SetProperty("#mvCentral.Genre", artistInfo.Genre);

        // Get the Album and set some skin props
        albumInfo = DBAlbumInfo.Get(trackInfo);
        if (albumInfo == null)
        {
          GUIPropertyManager.SetProperty("#mvCentral.Hierachy", artistInfo.Artist);
          GUIPropertyManager.SetProperty("#mvCentral.Album.Rating", string.Empty);
          GUIPropertyManager.SetProperty("#mvCentral.Album", string.Empty);
        }
        else
        {
          GUIPropertyManager.SetProperty("#mvCentral.Hierachy", artistInfo.Artist + " | " + albumInfo.Album);
          GUIPropertyManager.SetProperty("#mvCentral.Album", albumInfo.Album);
          GUIPropertyManager.SetProperty("#mvCentral.Album.Rating", albumInfo.Rating.ToString());
        }

        // Misc Proprities
        GUIPropertyManager.SetProperty("#mvCentral.Duration", trackDuration(trackInfo.PlayTime));

        if (trackInfo.LocalMedia[0].IsDVD)
        {
          GUIPropertyManager.SetProperty("#mvCentral.DVDView", "true");
          GUIPropertyManager.SetProperty("#mvCentral.TrackView", "false");
          GUIPropertyManager.SetProperty("#mvCentral.ArtistView", "false");
          GUIPropertyManager.SetProperty("#mvCentral.AlbumView", "false");
          clearVideoAudioProps();
        }
        else
        {
          // Set the view
          GUIPropertyManager.SetProperty("#mvCentral.TrackView", "true");
          GUIPropertyManager.SetProperty("#mvCentral.ArtistView", "false");
          GUIPropertyManager.SetProperty("#mvCentral.AlbumView", "false");
          GUIPropertyManager.SetProperty("#mvCentral.DVDView", "false");
          // Get the mediainfo details
          DBLocalMedia mediaInfo = (DBLocalMedia)trackInfo.LocalMedia[0];
          // Set Video skin props
          GUIPropertyManager.SetProperty("#mvCentral.LocalMedia.videoresolution", mediaInfo.VideoResolution);
          GUIPropertyManager.SetProperty("#mvCentral.LocalMedia.videoaspectratio", mediaInfo.VideoAspectRatio);
          GUIPropertyManager.SetProperty("#mvCentral.LocalMedia.videocodec", mediaInfo.VideoCodec);
          GUIPropertyManager.SetProperty("#mvCentral.LocalMedia.videowidth", mediaInfo.VideoWidth.ToString());
          GUIPropertyManager.SetProperty("#mvCentral.LocalMedia.videoheight", mediaInfo.VideoHeight.ToString());
          GUIPropertyManager.SetProperty("#mvCentral.LocalMedia.videoframerate", mediaInfo.VideoFrameRate.ToString());
          // Set Audio skin props
          GUIPropertyManager.SetProperty("#mvCentral.LocalMedia.audiocodec", mediaInfo.AudioCodec);
          GUIPropertyManager.SetProperty("#mvCentral.LocalMedia.audiochannels", mediaInfo.AudioChannels);
          GUIPropertyManager.SetProperty("#mvCentral.LocalMedia.audio", string.Format("{0} {1}", mediaInfo.AudioCodec, mediaInfo.AudioChannels));
        }

        if (item.Label != "..")
        {
          //logger.Debug("Setting lastItemVid (1) to {0}", facadeLayout.SelectedListItemIndex);
          lastItemVid = facadeLayout.SelectedListItemIndex;
        }

      }
      else
      {
        // Clear the audio/video props
        clearVideoAudioProps();
        // This is a Album
        albumInfo = (DBAlbumInfo)item.MusicTag;
        // Major issue as this should be an album - report and bomb out
        if (albumInfo == null)
        {
          logger.Error("Album data not found - exit method!");
          return;
        }

        GUIPropertyManager.SetProperty("#mvCentral.Album", albumInfo.Album);
        GUIPropertyManager.SetProperty("#mvCentral.Album.Rating", albumInfo.Rating.ToString());

        // get list of tracks in this album
        List<DBTrackInfo> tracksInAlbum = DBTrackInfo.GetEntriesByAlbum(albumInfo);
        DBArtistInfo thisArtist = DBArtistInfo.Get(tracksInAlbum[0]);
        GUIPropertyManager.SetProperty("#mvCentral.ArtistName", thisArtist.Artist);

        // Set the Hierachy
        GUIPropertyManager.SetProperty("#mvCentral.Hierachy", thisArtist.Artist);
        // Set image
        GUIPropertyManager.SetProperty("#mvCentral.VideoImg", item.ThumbnailImage);

        _currArtist = thisArtist;
        CurrentArtistId = item.ItemId;

        // Set the descrioption
        if (string.IsNullOrEmpty(albumInfo.bioContent.Trim()))
        {
          GUIPropertyManager.SetProperty("#mvCentral.TrackInfo", Localization.NoTrackInfo);
          GUIPropertyManager.SetProperty("#mvCentral.AlbumInfo", Localization.NoAlbumInfo);
        }
        else
        {
          GUIPropertyManager.SetProperty("#mvCentral.TrackInfo", mvCentralUtils.bioNoiseFilter(albumInfo.bioContent));
          GUIPropertyManager.SetProperty("#mvCentral.AlbumInfo", mvCentralUtils.bioNoiseFilter(albumInfo.bioContent));
        }
        // Set tracks and Runtime for Album contents
        GUIPropertyManager.SetProperty("#mvCentral.AlbumTracksRuntime", runningTime(tracksInAlbum));
        GUIPropertyManager.SetProperty("#mvCentral.TracksForAlbum", tracksInAlbum.Count.ToString());
        // Set the View
        GUIPropertyManager.SetProperty("#mvCentral.AlbumView", "true");
        GUIPropertyManager.SetProperty("#mvCentral.ArtistView", "false");
        GUIPropertyManager.SetProperty("#mvCentral.TrackView", "false");
        GUIPropertyManager.SetProperty("#mvCentral.DVDView", "false");
        if (item.Label != "..")
        {
          //logger.Debug("Setting lastItemAlb (1) to {0}", facadeLayout.SelectedListItemIndex);
          lastItemAlb = facadeLayout.SelectedListItemIndex;
        }

      }
      GUIPropertyManager.Changed = true;
    }


    /// <summary>
    /// DVD item selected - set a load of properities
    /// </summary>
    /// <param name="item"></param>
    /// <param name="parent"></param>
    void onDVDSelected(GUIListItem item, GUIControl parent)
    {
      DBTrackInfo trackInfo = null;

      // This is an Video
      trackInfo = (DBTrackInfo)item.MusicTag;
      GUIPropertyManager.SetProperty("#mvCentral.VideoImg", item.ThumbnailImage);
      // Track information
      if (string.IsNullOrEmpty(item.TVTag.ToString().Trim()))
        GUIPropertyManager.SetProperty("#mvCentral.TrackInfo", Localization.NoTrackInfo);
      else
        GUIPropertyManager.SetProperty("#mvCentral.TrackInfo", item.TVTag.ToString());
      // Track Rating
      GUIPropertyManager.SetProperty("#mvCentral.Track.Rating", trackInfo.Rating.ToString());

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

      // Get the artist 
      DBArtistInfo artistInfo = trackInfo.ArtistInfo[0];
      _currArtist = artistInfo;
      CurrentArtistId = item.ItemId;
      // And set some artist properites
      GUIPropertyManager.SetProperty("#mvCentral.ArtistName", artistInfo.Artist);
      GUIPropertyManager.SetProperty("#mvCentral.Genre", artistInfo.Genre);

      // Set BornOrFormed property
      if (_currArtist.Formed.Trim().Length == 0 && _currArtist.Born.Trim().Length == 0)
        GUIPropertyManager.SetProperty("#mvCentral.BornOrFormed", "No Born/Formed Details");
      else if (_currArtist.Formed.Trim().Length == 0)
        GUIPropertyManager.SetProperty("#mvCentral.BornOrFormed", String.Format("{0}: {1}", Localization.Born, _currArtist.Born));
      else
        GUIPropertyManager.SetProperty("#mvCentral.BornOrFormed", String.Format("{0}: {1}", Localization.Formed, _currArtist.Formed));

      // Misc Proprities
      GUIPropertyManager.SetProperty("#mvCentral.Duration", trackDuration(trackInfo.PlayTime));
      GUIPropertyManager.SetProperty("#mvCentral.Track.Rating", "0");
      GUIPropertyManager.SetProperty("#mvCentral.AlbumTracksRuntime", trackDuration(trackInfo.PlayTime));
      // Set the view
      GUIPropertyManager.SetProperty("#mvCentral.DVDView", "true");
      GUIPropertyManager.SetProperty("#mvCentral.TrackView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.ArtistView", "false");
      GUIPropertyManager.SetProperty("#mvCentral.AlbumView", "false");
      clearVideoAudioProps();

      if (item.Label != "..")
      {
        //logger.Debug("Setting lastItemVid (1) to {0}", facadeLayout.SelectedListItemIndex);
        lastItemVid = facadeLayout.SelectedListItemIndex;
      }

      GUIPropertyManager.SetProperty("#mvCentral.Hierachy", artistInfo.Artist);

      GUIPropertyManager.Changed = true;
    }


    /// <summary>
    /// Clear the Video and Audio Properities
    /// </summary>
    private void clearVideoAudioProps()
    {
      // Clear the video properites
      
      GUIPropertyManager.SetProperty("#mvCentral.LocalMedia.videoresolution", string.Empty);
      GUIPropertyManager.SetProperty("#mvCentral.LocalMedia.videoaspectratio", string.Empty);
      GUIPropertyManager.SetProperty("#mvCentral.LocalMedia.videocodec", string.Empty);
      // Audio
      GUIPropertyManager.SetProperty("#mvCentral.LocalMedia.audiocodec", string.Empty);
      GUIPropertyManager.SetProperty("#mvCentral.LocalMedia.audiochannels", string.Empty);
      GUIPropertyManager.SetProperty("#mvCentral.LocalMedia.audio", string.Empty);
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
    /// Give total running time for supplied tracklist
    /// </summary>
    /// <param name="property"></param>
    /// <param name="value"></param>
    private string runningTime(List<DBTrackInfo> trackList)
    {
      try
      {
        TimeSpan tt = TimeSpan.Parse("00:00:00");
        foreach (DBTrackInfo track in trackList)
        {
          tt += TimeSpan.Parse(track.PlayTime);
        }
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
    /// Give total running time for supplied tracklist
    /// </summary>
    /// <param name="property"></param>
    /// <param name="value"></param>
    private string genreRunningTime(DBGenres genreObject)
    {

      List<DBArtistInfo> artistList = new List<DBArtistInfo>();
      List<DBArtistInfo> artistFullList = DBArtistInfo.GetAll();

      try
      {
        TimeSpan tt = TimeSpan.Parse("00:00:00");

        foreach (DBArtistInfo artistInfo in artistFullList)
        {
          if (tagMatched(facadeLayout.SelectedListItem.Label, artistInfo) || artistInfo.Genre.Equals(facadeLayout.SelectedListItem.Label,StringComparison.OrdinalIgnoreCase))
          {
            if (!artistList.Contains(artistInfo))
              artistList.Add(artistInfo);
          }
        }
        _genreTracks = 0;
        foreach (DBArtistInfo artist in artistList)
        {
          List<DBTrackInfo> artistTracks = DBTrackInfo.GetEntriesByArtist(artist);
          foreach (DBTrackInfo track in artistTracks)
          {
            _genreTracks++;
            try
            {
              tt += TimeSpan.Parse(track.PlayTime);
            }
            catch { }
          }
        }
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
    /// Update last 3 videos
    /// </summary>
    private void LastThreeVideos()
    {
      try
      {
        List<DBTrackInfo> allTracks = DBTrackInfo.GetAll();
        if (allTracks.Count > 0)
        {
          allTracks.Sort(delegate(DBTrackInfo p1, DBTrackInfo p2) { return p2.DateAdded.CompareTo(p1.DateAdded); });
          // Latest 3 Artists added
          SetProperty("#mvCentral.Latest.Artist1", allTracks[0].ArtistInfo[0].Artist);
          SetProperty("#mvCentral.Latest.Artist2", allTracks[1].ArtistInfo[0].Artist);
          SetProperty("#mvCentral.Latest.Artist3", allTracks[2].ArtistInfo[0].Artist);
          // Images for lastest 3 artists
          SetProperty("#mvCentral.Latest.ArtistImage1", allTracks[0].ArtistInfo[0].ArtFullPath);
          SetProperty("#mvCentral.Latest.ArtistImage2", allTracks[1].ArtistInfo[0].ArtFullPath);
          SetProperty("#mvCentral.Latest.ArtistImage3", allTracks[2].ArtistInfo[0].ArtFullPath);
          // Latest 3 tracks Added
          SetProperty("#mvCentral.Latest.Track1", allTracks[0].Track);
          SetProperty("#mvCentral.Latest.Track2", allTracks[1].Track);
          SetProperty("#mvCentral.Latest.Track3", allTracks[2].Track);

          SetProperty("#mvCentral.latest.enabled", "true");
        }
        else
          logger.Debug("NO videos have been loaded into mvCentral - kinda pointless doing the lastet videos!");
      }
      catch
      {
        logger.Debug("Error during read of last 3 videos added - are there any videos?");
      }
    }


    #endregion

    #region Skin and Property Settings

    public void SetProperty(string property, string value)
    {
      SetProperty(property, value, false);
    }

    public void SetProperty(string property, string value, bool forceLogging)
    {
      if (property == null)
        return;

      if (mvCentralCore.Settings.LogAllSkinPropertyChanges)
        forceLogging = true;

      try
      {
        lock (propertySync)
        {

          if (_loggedProperties == null)
            _loggedProperties = new Dictionary<string, bool>();

          if (!_loggedProperties.ContainsKey(property) || forceLogging)
          {
            logger.Debug(property + " = \"" + value + "\"");
            _loggedProperties[property] = true;
          }
        }
      }
      catch (Exception e)
      {
        if (e is ThreadAbortException)
          throw e;

        logger.Warn("Internal .NET error from dictionary class!");
      }

      // If the value is empty add a space
      // otherwise the property will keep 
      // displaying it's previous value
      if (String.IsNullOrEmpty(value))
        GUIPropertyManager.SetProperty(property, " ");

      GUIPropertyManager.SetProperty(property, value);
    }

    /// <summary>
    /// Resets the property values for every key that starts with the given string
    /// </summary>
    /// <param name="startsWith">the prefix to reset</param>
    public void ResetProperties(string startsWith)
    {
      logger.Debug("Resetting properties: {0}", startsWith);
      foreach (string key in _loggedProperties.Keys)
      {
        if (key.StartsWith(startsWith))
          SetProperty(key, "");
      }
    }
    #endregion

    #region Loading and initialization

    private void showLoadingDialog()
    {
    }

    private void onCoreInitializationProgress(string actionName, int percentDone)
    {

      // Update the progress variables
      if (percentDone == 100)
      {
        actionName = "Loading GUI ...";
      }
      //            initProgressLastAction = actionName;
      //            initProgressLastPercent = percentDone;

      // If the progress dialog exists, update it.
      //            if (initDialog != null)
      //            {
      //                initDialog.SetLine(2, initProgressLastAction);
      //                initDialog.SetPercentage(initProgressLastPercent);
      //                initDialog.Progress();
      //            }

      // When we are finished initializing
      if (percentDone == 100)
      {

        // Start the background importer
        if (mvCentralCore.Settings.EnableImporterInGUI)
        {
          logger.Debug("Start Importer");
          mvCentralCore.Importer.Start();
          //mvCentralCore.Importer.Progress += new MusicVideoImporter.ImportProgressHandler(Importer_Progress);
        }

        // Load skin based settings from skin file
        // skinSettings = new mvCentralSkinSettings(_windowXmlFileName);

        // Get Moving Pictures specific autoplay setting
        try
        {
          //diskInsertedAction = (DiskInsertedAction)Enum.Parse(typeof(DiskInsertedAction), mvCentralCore.Settings.DiskInsertionBehavior);
        }
        catch
        {
          //diskInsertedAction = DiskInsertedAction.DETAILS;
        }

        // setup the image resources for cover and backdrop display
        int artworkDelay = mvCentralCore.Settings.ArtworkLoadingDelay;

        // setup the time for the random category backdrop refresh
        /*                activeMovieLookup.Timeout = new TimeSpan(0, 0, mvCentralCore.Settings.CategoryRandomArtworkRefreshInterval);

                        // create backdrop image swapper
                        backdrop = new ImageSwapper();
                        backdrop.ImageResource.Delay = artworkDelay;
                        backdrop.PropertyOne = "#mvCentral.Backdrop";

                        // create cover image swapper
                        cover = new AsyncImageResource();
                        cover.Property = "#mvCentral.Coverart";
                        cover.Delay = artworkDelay;



                        // instantiate player
                        moviePlayer = new MoviePlayer(this);
                        moviePlayer.MovieEnded += new MoviePlayerEvent(onMovieEnded);
                        moviePlayer.MovieStopped += new MoviePlayerEvent(onMovieStopped);

                        // Listen to the DeviceManager for external media activity (i.e. disks inserted)
                        logger.Debug("Listening for device changes.");
                        DeviceManager.OnVolumeInserted += new DeviceManager.DeviceManagerEvent(OnVolumeInserted);
                        DeviceManager.OnVolumeRemoved += new DeviceManager.DeviceManagerEvent(OnVolumeRemoved);

                        // Flag that the GUI is initialized
        */
        _initComplete = true;

        // If the initDialog is present close it
        //                if (initDialog != null)
        //                {
        //                    initDialog.Close();
        //                }

        // Report that we completed the init
        logger.Info("GUI Initialization Complete");
      }
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Display metadata background refresh % compete as progress bar and text percentage
    /// </summary>
    /// <param name="process"></param>
    /// <param name="progress"></param>
    void ProcessManager_Progress(AbstractBackgroundProcess process, double progress)
    {
      if (process.Name == "MediaInfo Updater")
      {
        if (MetadataProgressBar != null)
        {
          if (progress == 0.0)
          {
            MetadataProgressBar.Percentage = 0;
            GUIPropertyManager.SetProperty("#mvCentral.Metadata.Scan.Active", "false");
            GUIControl.HideControl(GetID, MetadataProgressBar.GetID);
          }
          else if (progress >= 100.0)
            GUIPropertyManager.SetProperty("#mvCentral.Metadata.Scan.Active", "false");
          else
          {
            SetProperty("#mvCentral.Metadata.Update.Progress", string.Format("{0:0.0%} {1}", (progress / 100), Localization.Compete));
            GUIPropertyManager.SetProperty("#mvCentral.Metadata.Scan.Active", "true");
            MetadataProgressBar.Percentage = (float)progress;
          }
        }
        else
        {
          GUIPropertyManager.SetProperty("#mvCentral.Metadata.Scan.Active", "false");
          SetProperty("#mvCentral.Metadata.Update.Progress", string.Format("{0:0.0%} {1}", 0, Localization.Compete));
        }
        GUIPropertyManager.Changed = true;
      }
    }
    /// <summary>
    /// Fired when new artist is commited to DB via background importer thread
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="action"></param>
    private void mvStatusChangedListener(MusicVideoMatch obj, MusicVideoImporterAction action)
    {
      if (action == MusicVideoImporterAction.COMMITED)
      {
        LastThreeVideos();
      }
      return;
    }

    #endregion

  }
}
