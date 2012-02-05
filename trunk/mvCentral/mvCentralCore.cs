using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Windows.Forms;
using System.Text.RegularExpressions;

using Cornerstone.Database;
using Cornerstone.Database.Tables;
using Cornerstone.GUI.Dialogs;
using Cornerstone.GUI;
using Cornerstone.Tools;


using NLog;
using NLog.Config;
using NLog.Targets;

using mvCentral.BackgroundProcesses;
using mvCentral.Settings;
using mvCentral.Database;
using mvCentral.LocalMediaManagement;
using mvCentral.DataProviders;
using mvCentral.GUI;
using mvCentral.Localizations;
//using mvCentral.Utils;
using MediaPortal.Configuration;
using MediaPortal.Services;
using MediaPortal.GUI.Library;

namespace mvCentral
{
  class mvCentralCore
  {
    // Plugin ID
    public const int PluginID = 112011;

    public const string albumRegex = @"(?<artist>[^\\]+)\\(?<album>[^\\]+)\\(?:\d+\s+)?(?<track>[^\\]+)\.(?<ext>[^\r]+)$";
    
    private static Logger logger = LogManager.GetCurrentClassLogger();

    public static event ProgressDelegate InitializeProgress;

    public enum PowerEvent
    {
      Suspend,
      Resume
    }

    public enum ToneOrStyle
    {
      Tone,
      style
    }

    public delegate void PowerEventDelegate(PowerEvent powerEvent);
    public static event PowerEventDelegate OnPowerEvent;

    private const string dbFileName = "mvCentral.db3";
    private const string logFileName = "mvCentral.log";
    private const string oldLogFileName = "mvCentral.old.log";
    private static float loadingProgress;
    private static float loadingTotal;
    private static string loadingProgressDescription;

    private static object importerLock = new Object();
    private static object dbLock = new Object();
    private static object settingsLock = new Object();
    private static object processLock = new Object();

    #region Properties & Events

    // The MovieImporter object that should be used by all components of the plugin
    public static MusicVideoImporter Importer
    {
      get
      {
        lock (importerLock)
        {
          if (_importer == null)
            _importer = new MusicVideoImporter();
          return _importer;
        }
      }
    } private static MusicVideoImporter _importer;

    // The DatabaseManager that should be used by all components of the plugin.       
    public static DatabaseManager DatabaseManager
    {
      get
      {
        lock (dbLock)
        {
          if (_databaseManager == null)
            initDB();

          return _databaseManager;
        }
      }
    }  private static DatabaseManager _databaseManager;

    // The SettingsManager that should be used by all components of the plugin.
    public static mvCentralSettings Settings
    {
      get
      {
        lock (settingsLock)
        {
          if (_settings == null)
            _settings = new mvCentralSettings(DatabaseManager);

          return _settings;
        }
      }
    } private static mvCentralSettings _settings = null;

    public static DataProviderManager DataProviderManager
    {
      get
      {
        return DataProviderManager.GetInstance();
      }
    }

    public static BackgroundProcessManager ProcessManager
    {
      get
      {
        lock (processLock)
        {
          if (_processManager == null)
            _processManager = new BackgroundProcessManager();

          return _processManager;
        }
      }
    } private static BackgroundProcessManager _processManager = null;

    //       public static MovieBrowser Browser
    //       {
    //           get
    //           {
    //               return _browser;
    //           }

    //           internal set
    //            {
    //                _browser = value;
    //            }
    //        } private static MovieBrowser _browser = null;

    // Settings from Media Portal
    // Instead of calling this line whenever we need some MP setting we only define it once
    // There isn't really a central MePo settings manager (or is there?)
    public static MediaPortal.Profile.Settings MediaPortalSettings
    {
      get
      {
        MediaPortal.Profile.Settings mpSettings = new MediaPortal.Profile.Settings(MediaPortal.Configuration.Config.GetFile(MediaPortal.Configuration.Config.Dir.Config, "MediaPortal.xml"));
        return mpSettings;
      }
    }

    #endregion



    private static mvCentralCore _instance = null;

    // Constructor. Private because we are a singleton.
    private mvCentralCore() { }


    // Initializes the database connection to the Music video Plugin database
    private static void initDB()
    {
      if (_databaseManager != null)
        return;

      string fullDBFileName = MediaPortal.Configuration.Config.GetFile(MediaPortal.Configuration.Config.Dir.Database, dbFileName);
      _databaseManager = new DatabaseManager(fullDBFileName);

      // check that we at least have a default user
      List<DBUser> users = DBUser.GetAll();
      if (users.Count == 0)
      {
        DBUser defaultUser = new DBUser();
        defaultUser.Name = "Default User";
        defaultUser.Commit();
      }

      // add all filter helpers
      //           _databaseManager.AddFilterHelper<DBMovieInfo>(new FilterHelperDBMovieInfo());
    }

    private static void closeDB()
    {
      logger.Debug("In Method : closeDB()");

      if (_databaseManager == null)
        return;

      _databaseManager.Close();
    }

    // Initializes the logging system.
    private static void InitLogger(RichTextBox rtb)
    {
      // backup the current log file and clear for the new one
      try
      {
        FileInfo logFile = new FileInfo(MediaPortal.Configuration.Config.GetFile(MediaPortal.Configuration.Config.Dir.Log, logFileName));
        if (logFile.Exists)
        {
          if (File.Exists(MediaPortal.Configuration.Config.GetFile(MediaPortal.Configuration.Config.Dir.Log, oldLogFileName)))
            File.Delete(MediaPortal.Configuration.Config.GetFile(MediaPortal.Configuration.Config.Dir.Log, oldLogFileName));

          logFile.CopyTo(MediaPortal.Configuration.Config.GetFile(MediaPortal.Configuration.Config.Dir.Log, oldLogFileName));
          logFile.Delete();
        }
      }
      catch (Exception) { }

      // if no configuration exists go ahead and create one
      if (LogManager.Configuration == null) 
        LogManager.Configuration = new LoggingConfiguration();

      // build the logging target for music videos logging
      FileTarget mvLogTarget = new FileTarget();
      mvLogTarget.Name = "mvCentral";
      mvLogTarget.FileName = MediaPortal.Configuration.Config.GetFile(MediaPortal.Configuration.Config.Dir.Log, logFileName);
      mvLogTarget.Layout = "${date:format=dd-MMM-yyyy HH\\:mm\\:ss} " +
                          "${level:fixedLength=true:padding=5} " +
                          "[${logger:fixedLength=true:padding=20:shortName=true}]: ${message} " +
                          "${exception:format=tostring}";

      LogManager.Configuration.AddTarget("mvCentral", mvLogTarget);

      // Get current Log Level from MediaPortal 
      LogLevel logLevel;
      MediaPortal.Profile.Settings xmlreader = MediaPortalSettings;
      switch ((Level)xmlreader.GetValueAsInt("general", "loglevel", 0))
      {
        case Level.Error:
          logLevel = LogLevel.Error;
          break;
        case Level.Warning:
          logLevel = LogLevel.Warn;
          break;
        case Level.Information:
          logLevel = LogLevel.Info;
          break;
        case Level.Debug:
        default:
          logLevel = LogLevel.Debug;
          break;
      }

#if DEBUG
      logLevel = LogLevel.Debug;
#endif



      // set the logging rules for Music Videos logging
      LoggingRule mvRule = new LoggingRule("mvCentral.*", logLevel, mvLogTarget);
      LoggingRule CornerstoneRule = new LoggingRule("Cornerstone.*", logLevel, mvLogTarget);

      LogManager.Configuration.LoggingRules.Add(mvRule);
      LogManager.Configuration.LoggingRules.Add(CornerstoneRule);

      // force NLog to reload the configuration data
      LogManager.Configuration = LogManager.Configuration;
      
    }


    //for production, replace all references in this method from "SettingsManagerNew" to "SettingsManager"
    public static void initAdditionalSettings()
    {

      if (Settings.AlbumArtFolder.Trim() == "")
        Settings.AlbumArtFolder = MediaPortal.Configuration.Config.GetFolder(MediaPortal.Configuration.Config.Dir.Thumbs) + "\\mvCentral\\Albums\\FullSize";

      // create the albums folder if it doesn't already exist
      if (!Directory.Exists(Settings.AlbumArtFolder))
        Directory.CreateDirectory(Settings.AlbumArtFolder);

      if (Settings.AlbumArtThumbsFolder.Trim() == "")
        Settings.AlbumArtThumbsFolder = MediaPortal.Configuration.Config.GetFolder(MediaPortal.Configuration.Config.Dir.Thumbs) + "\\mvCentral\\Albums\\Thumbs";

      // create the thumbs folder if it doesn't already exist
      if (!Directory.Exists(Settings.AlbumArtThumbsFolder))
        Directory.CreateDirectory(Settings.AlbumArtThumbsFolder);

      if (Settings.TrackArtFolder.Trim() == "")
        Settings.TrackArtFolder = MediaPortal.Configuration.Config.GetFolder(MediaPortal.Configuration.Config.Dir.Thumbs) + "\\mvCentral\\Tracks\\FullSize";

      // create the tracks folder if it doesn't already exist
      if (!Directory.Exists(Settings.TrackArtFolder))
        Directory.CreateDirectory(Settings.TrackArtFolder);

      if (Settings.TrackArtThumbsFolder.Trim() == "")
        Settings.TrackArtThumbsFolder = MediaPortal.Configuration.Config.GetFolder(MediaPortal.Configuration.Config.Dir.Thumbs) + "\\mvCentral\\Tracks\\Thumbs";

      // create the thumbs folder if it doesn't already exist
      if (!Directory.Exists(Settings.TrackArtThumbsFolder))
        Directory.CreateDirectory(Settings.TrackArtThumbsFolder);

      if (Settings.ArtistArtFolder.Trim() == "")
        Settings.ArtistArtFolder = MediaPortal.Configuration.Config.GetFolder(MediaPortal.Configuration.Config.Dir.Thumbs) + "\\mvCentral\\Artists\\FullSize";

      // create the ArtistArt folder if it doesn't already exist
      if (!Directory.Exists(Settings.ArtistArtFolder))
        Directory.CreateDirectory(Settings.ArtistArtFolder);

      if (Settings.ArtistArtThumbsFolder.Trim() == "")
        Settings.ArtistArtThumbsFolder = MediaPortal.Configuration.Config.GetFolder(MediaPortal.Configuration.Config.Dir.Thumbs) + "\\mvCentral\\Artists\\Thumbs";

      // create the ArtistArt thumbs folder if it doesn't already exist
      if (!Directory.Exists(Settings.ArtistArtThumbsFolder))
        Directory.CreateDirectory(Settings.ArtistArtThumbsFolder);

    }

    private static void startBackgroundTasks()
    {
      logger.Info("Starting Background Processes...");
      ProcessManager.StartProcess(new MediaInfoUpdateProcess());
      ProcessManager.StartProcess(new UpdateArtworkProcess());
    }

    private static void stopBackgroundTasks()
    {
      logger.Info("Stopping Background Processes...");

      // Cancel background processes
      if (_processManager != null)
        _processManager.CancelAllProcesses();

      _processManager = null;
    }

    private static void checkVersionInfo()
    {
      // check if the version changed, and update the DB accordingly
      Version realVer = Assembly.GetExecutingAssembly().GetName().Version;

      if (realVer > GetDBVersionNumber())
      {
        Settings.Version = realVer.ToString();
        Settings.DataProvidersInitialized = false;
      }
    }

    public static Version GetDBVersionNumber()
    {
      return new Version(Settings.Version);
    }

    // Centralized handler for PowerMode events, will in turn fire our own event where the other components hook into
    private static void onSystemPowerModeChanged(object sender, Microsoft.Win32.PowerModeChangedEventArgs e)
    {
      if (e.Mode == Microsoft.Win32.PowerModes.Resume)
      {
        logger.Info("mvCentral is resuming from standby");

        // The database connection will be automatically reopened on first request
        // so we don't have to explicitly open it again

        // Start Device Manager
        DeviceManager.StartMonitor();

        // Start Background Tasks
        startBackgroundTasks();

        // Fire Event Resume
        if (OnPowerEvent != null)
          OnPowerEvent(PowerEvent.Resume);

      }
      else if (e.Mode == Microsoft.Win32.PowerModes.Suspend)
      {
        logger.Info("mvCentral is suspending");

        // Fire Event Suspend
        if (OnPowerEvent != null)
          OnPowerEvent(PowerEvent.Suspend);

        // Stop Background Tasks
        stopBackgroundTasks();

        // Stop Device Manager
        DeviceManager.StopMonitor();

        // Close DB Connection
        closeDB();
      }
    }


    private static void InitLocalization()
    {
      logger.Info("Initializing localization");
      Localization.Init();
      Localization.TranslateSkin();


    }



    #region Public Methods

    // Returns instance to this class as we only want to have one 
    // in existance at a time.
    public static mvCentralCore Instance
    {
      get
      {
        if (_instance == null)
          _instance = new mvCentralCore();

        return _instance;
      }
    }

    internal static void SetProperty(string property, string value)
    {
      if (property == null)
        return;

      //// If the value is empty always add a space
      //// otherwise the property will keep 
      //// displaying it's previous value
      if (String.IsNullOrEmpty(value))
        value = " ";

      GUIPropertyManager.SetProperty(property, value);
    }

    public static void Initialize()
    {
      Initialize(null);
    }

    // Should be the first thing that is run whenever the plugin launches, either
    // from the GUI or the Config Screen.
    public static void Initialize(RichTextBox rtb)
    {
      InitLogger(rtb);
      Version ver = Assembly.GetExecutingAssembly().GetName().Version;
      logger.Info(string.Format("mvCentral ({0}.{1}.{2}.{3})", ver.Major, ver.Minor, ver.Build, ver.Revision));
      logger.Info("Plugin launched");


      InitLocalization();
      //            InitSettings();
      //            InitPluginHandlers();
      //            InitTorrentHandlers();

      // Register Win32 PowerMode Event Handler
      Microsoft.Win32.SystemEvents.PowerModeChanged += new Microsoft.Win32.PowerModeChangedEventHandler(onSystemPowerModeChanged);
      DatabaseMaintenanceManager.MaintenanceProgress += new ProgressDelegate(DatabaseMaintenanceManager_MaintenanceProgress);


      // setup the data structures sotring our list of startup actions
      // we use this setup so we can easily add new tasks without having to 
      // tweak any magic numbers for the progress bar / loading screen
      List<WorkerDelegate> initActions = new List<WorkerDelegate>();
      Dictionary<WorkerDelegate, string> actionDescriptions = new Dictionary<WorkerDelegate, string>();
      WorkerDelegate newAction;

      newAction = new WorkerDelegate(initAdditionalSettings);
      actionDescriptions.Add(newAction, "Initializing Path Settings...");
      initActions.Add(newAction);

      newAction = new WorkerDelegate(DatabaseMaintenanceManager.UpdateImportPaths);
      actionDescriptions.Add(newAction, "Updating Import Paths...");
      initActions.Add(newAction);

      newAction = new WorkerDelegate(checkVersionInfo);
      actionDescriptions.Add(newAction, "Initializing Version Information...");
      initActions.Add(newAction);

      newAction = new WorkerDelegate(DataProviderManager.Initialize);
      actionDescriptions.Add(newAction, "Initializing Data Provider Manager...");
      initActions.Add(newAction);

      newAction = new WorkerDelegate(DatabaseMaintenanceManager.VerifyMusicVideoInformation);
      actionDescriptions.Add(newAction, "Updating Music Video Information...");
      initActions.Add(newAction);

      newAction = new WorkerDelegate(DeviceManager.StartMonitor);
      actionDescriptions.Add(newAction, "Starting Device Monitor...");
      initActions.Add(newAction);

      // load all the above actions and notify any listeners of our progress
      loadingProgress = 0;
      loadingTotal = initActions.Count;
      foreach (WorkerDelegate currAction in initActions)
      {
        try
        {
          if (InitializeProgress != null) InitializeProgress(actionDescriptions[currAction], (int)(loadingProgress * 100 / loadingTotal));
          loadingProgressDescription = actionDescriptions[currAction];
          currAction();
        }
        catch (Exception ex)
        {
          // don't log error if the init was aborted on purpose
          if (ex.GetType() == typeof(ThreadAbortException))
            throw ex;

          logger.ErrorException("Error: ", ex);
        }
        finally
        {
          loadingProgress++;
        }
      }

      if (InitializeProgress != null) InitializeProgress("Done!", 100);

      // stop listening
      DatabaseMaintenanceManager.MaintenanceProgress -= new ProgressDelegate(DatabaseMaintenanceManager_MaintenanceProgress);

      // Launch background tasks
      mvCentralCore.Settings.AutoRetrieveMediaInfo = true;
      startBackgroundTasks();

    }

    static void DatabaseMaintenanceManager_MaintenanceProgress(string actionName, int percentDone)
    {
      int baseProgress = (int)(loadingProgress * 100 / loadingTotal);
      if (InitializeProgress != null) InitializeProgress(loadingProgressDescription, baseProgress + (int)((float)percentDone / loadingTotal));
    }
    /// <summary>
    /// Plugin shutdown
    /// </summary>
    public static void Shutdown()
    {
      logger.Debug("In method : Shutdown()");

      // Unregister Win32 PowerMode Event Handler
      Microsoft.Win32.SystemEvents.PowerModeChanged -= new Microsoft.Win32.PowerModeChangedEventHandler(onSystemPowerModeChanged);

      DeviceManager.StopMonitor();

      // Stop Importer
      if (_importer != null)
        _importer.Stop();

      // Stop all background tasks
      stopBackgroundTasks();
      _importer = null;
      _settings = null;
      
      // Close the DB
      closeDB();
      logger.Info("Plugin Closed");
      
      // Kill the logger
      LogManager.Configuration = null;
    }

    #endregion
  }
}
