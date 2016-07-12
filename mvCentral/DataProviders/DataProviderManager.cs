using Cornerstone.Database;

using mvCentral.Database;
using mvCentral.SignatureBuilders;

using NLog;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading;

namespace mvCentral.DataProviders
{
  public class DataProviderManager
  {
    public enum AddSourceResult
    {
      SUCCESS,          // successfully added the source
      FAILED,           // general failure, usually a parsing error or duplicate unscripted source
      SUCCESS_REPLACED, // success, but replaced existing version, this will fail when not in debug mode
      FAILED_VERSION,   // version conflict
      FAILED_DATE       // published date conflict
    }

    private static Logger logger = LogManager.GetCurrentClassLogger();
    private static DataProviderManager instance = null;
    private static String lockObj = "";

    Dictionary<DataType, DBSourceInfoComparer> sorters;
    private DatabaseManager dbManager = null;

    #region Properties

    public ReadOnlyCollection<DBSourceInfo> TrackDetailSources
    {
      get
      {
        return trackDetailSources.AsReadOnly();
      }
    }
    private List<DBSourceInfo> trackDetailSources;

    public ReadOnlyCollection<DBSourceInfo> ArtistDetailSources
    {
      get
      {
        return artistDetailSources.AsReadOnly();
      }
    }
    private List<DBSourceInfo> artistDetailSources;

    public ReadOnlyCollection<DBSourceInfo> AlbumDetailSources
    {
      get
      {
        return albumDetailSources.AsReadOnly();
      }
    }
    private List<DBSourceInfo> albumDetailSources;


    public ReadOnlyCollection<DBSourceInfo> AlbumArtSources
    {
      get
      {
        return albumArtSources.AsReadOnly();
      }
    }
    private List<DBSourceInfo> albumArtSources;

    public ReadOnlyCollection<DBSourceInfo> ArtistArtSources
    {
      get
      {
        return artistArtSources.AsReadOnly();
      }
    }
    private List<DBSourceInfo> artistArtSources;

    public ReadOnlyCollection<DBSourceInfo> TrackArtSources
    {
      get
      {
        return trackArtSources.AsReadOnly();
      }
    }
    private List<DBSourceInfo> trackArtSources;

    public ReadOnlyCollection<DBSourceInfo> AllSources
    {
      get { return allSources.AsReadOnly(); }
    }
    private List<DBSourceInfo> allSources;

    public bool DebugMode
    {
      get { return debugMode; }
      set
      {
        if (debugMode != value)
        {
          debugMode = value;
          foreach (DBSourceInfo currSource in allSources)
          {
            //                       if (currSource.Provider is IScriptableMusicVideoProvider)
            //                           ((IScriptableMusicVideoProvider)currSource.Provider).DebugMode = value;
          }
        }
      }
    }
    private bool debugMode;

    private bool updateOnly;

    #endregion

    #region Constructors

    public static void Initialize()
    {
      DataProviderManager.GetInstance();
    }

    public static DataProviderManager GetInstance()
    {
      lock (lockObj)
      {
        if (instance == null)
          instance = new DataProviderManager();
      }
      return instance;
    }

    private DataProviderManager()
    {
      dbManager = mvCentralCore.DatabaseManager;

      trackDetailSources = new List<DBSourceInfo>();
      artistDetailSources = new List<DBSourceInfo>();
      albumDetailSources = new List<DBSourceInfo>();
      trackArtSources = new List<DBSourceInfo>();
      albumArtSources = new List<DBSourceInfo>();
      artistArtSources = new List<DBSourceInfo>();
      allSources = new List<DBSourceInfo>();

      sorters = new Dictionary<DataType, DBSourceInfoComparer>();
      sorters[DataType.TRACKDETAIL] = new DBSourceInfoComparer(DataType.TRACKDETAIL);
      sorters[DataType.ARTISTDETAIL] = new DBSourceInfoComparer(DataType.ARTISTDETAIL);
      sorters[DataType.ALBUMDETAIL] = new DBSourceInfoComparer(DataType.ALBUMDETAIL);
      sorters[DataType.ALBUMART] = new DBSourceInfoComparer(DataType.ALBUMART);
      sorters[DataType.ARTISTART] = new DBSourceInfoComparer(DataType.ARTISTART);
      sorters[DataType.TRACKART] = new DBSourceInfoComparer(DataType.TRACKART);

      debugMode = mvCentralCore.Settings.DataSourceDebugActive;

      logger.Info("DataProviderManager Starting");
      loadProvidersFromDatabase();

      // if we have already done an initial load, set an internal flag to do updates only
      // when loading internal scripts. We dont want to load in previously deleted scripts
      // during the internal provider loading process.
      updateOnly = mvCentralCore.Settings.DataProvidersInitialized;
      LoadInternalProviders();

      updateOnly = false;
      mvCentralCore.Settings.DataProvidersInitialized = true;
    }

    #endregion

    #region Automatic Management Functionailty

    public HashSet<CultureInfo> GetAvailableLanguages()
    {
      HashSet<CultureInfo> results = new HashSet<CultureInfo>();

      foreach (DBSourceInfo currSource in trackDetailSources)
      {
        try
        {
          if (currSource.Provider.LanguageCodeList.Count == 0)
          {
            if (currSource.Provider.LanguageCode != "")
              results.Add(new CultureInfo(currSource.Provider.LanguageCode));
          }
          else
          {
            foreach (string languageCode in currSource.Provider.LanguageCodeList)
              results.Add(new CultureInfo(languageCode));
          }
        }
        catch (Exception e)
        {
          if (e is ThreadAbortException)
            throw e;
        }
      }

      return results;
    }

    public void AutoArrangeDataProviders()
    {
      if (mvCentralCore.Settings.DataProviderManagementMethod != "auto")
        return;

      string languageCode;
      try { languageCode = mvCentralCore.Settings.DataProviderAutoLanguage; }
      catch (ArgumentException)
      {
        languageCode = "en";
      }

      ArrangeDataProviders(languageCode);
    }

    public void ArrangeDataProviders(string languageCode)
    {
      foreach (DataType currType in Enum.GetValues(typeof(DataType)))
      {
        int nextRank = 10;
        foreach (DBSourceInfo currSource in getEditableList(currType))
        {
          // special case for imdb provider. should always be used as a last resort details provider
          if (currSource.IsScriptable() && currType == DataType.TRACKART)
          {
            if (languageCode != "en")
            {
              currSource.SetPriority(currType, 98);
              currSource.Commit();
            }
            else
            {
              currSource.SetPriority(currType, 1);
              currSource.Commit();
            }
          }
          else if (currType == DataType.ALBUMART && currSource.IsScriptable())
          {
            if (languageCode != "en")
            {
              currSource.SetPriority(currType, 98);
              currSource.Commit();
            }
            else
            {
              currSource.SetPriority(currType, 1);
              currSource.Commit();
            }
          }
          // not a generic language script and not for the selected language, disable
          else if (currSource.Provider.LanguageCode != "" && currSource.Provider.LanguageCode != "various" && currSource.Provider.LanguageCode != languageCode)
          {
            currSource.SetPriority(currType, -1);
            currSource.Commit();
          }
          // valid script, enable
          else
          {
            // Local Provider is Primary for artwork
            if (currSource.Provider is LocalProvider)
            {
              currSource.SetPriority(currType, 0);
              currSource.Commit();
            }
            // FanartTV Provider is Primary for artwork
            if (currSource.Provider is FanartTVProvider)
            {
              currSource.SetPriority(currType, 1);
              currSource.Commit();
            }
            // Last.Fm is primary for matching
            else if (currSource.Provider is LastFmProvider)
            {
              currSource.SetPriority(currType, 5);
              currSource.Commit();
            }
            else if (currSource.Provider.LanguageCode == "" || currSource.Provider.LanguageCode == "various")
            {
              currSource.SetPriority(currType, 50 + nextRank++);
              currSource.Commit();
            }
            else
            {
              currSource.SetPriority(currType, nextRank++);
              currSource.Commit();
            }
          }
        }
        // sort and normalize
        getEditableList(currType).Sort(sorters[currType]);
        normalizePriorities();
      }
    }

    #endregion

    #region DataProvider Management Functionality

    public void ChangePriority(DBSourceInfo source, DataType type, bool raise)
    {
      if (source.IsDisabled(type))
      {
        if (raise)
          SetDisabled(source, type, false);
        else return;
      }

      // grab the correct list 
      List<DBSourceInfo> sourceList = getEditableList(type);

      // make sure the specified source is in our list
      if (!sourceList.Contains(source))
        return;

      if (source.GetPriority(type) == null)
      {
        logger.Error("No priority set for " + type.ToString());
        return;
      }

      // make sure our index is in sync
      int index = sourceList.IndexOf(source);
      int oldPriority = (int)source.GetPriority(type);
      if (index != oldPriority)
        logger.Warn("Priority and List.IndexOf out of sync...");

      // raise priority 
      if (raise)
      {
        if (source.GetPriority(type) > 0)
        {
          source.SetPriority(type, oldPriority - 1);
          sourceList[index - 1].SetPriority(type, oldPriority);

          source.Commit();
          sourceList[index - 1].Commit();
        }
      }

      // lower priority
      else
      {
        if (source.GetPriority(type) < sourceList.Count - 1 &&
            sourceList[index + 1].GetPriority(type) != -1)
        {

          source.SetPriority(type, oldPriority + 1);
          sourceList[index + 1].SetPriority(type, oldPriority);

          source.Commit();
          sourceList[index + 1].Commit();
        }
      }

      // resort the list
      lock (sourceList) sourceList.Sort(sorters[type]);
    }

    public void SetDisabled(DBSourceInfo source, DataType type, bool disable)
    {
      if (disable)
      {
        source.SetPriority(type, -1);
        source.Commit();
      }
      else
        source.SetPriority(type, int.MaxValue);

      getEditableList(type).Sort(sorters[type]);
      normalizePriorities();
    }

    private List<DBSourceInfo> getEditableList(DataType type)
    {
      List<DBSourceInfo> sourceList = null;
      switch (type)
      {
        case DataType.TRACKDETAIL:
          sourceList = trackDetailSources;
          break;
        case DataType.ARTISTDETAIL:
          sourceList = artistDetailSources;
          break;
        case DataType.ALBUMDETAIL:
          sourceList = albumDetailSources;
          break;
        case DataType.ALBUMART:
          sourceList = albumArtSources;
          break;
        case DataType.ARTISTART:
          sourceList = artistArtSources;
          break;
        case DataType.TRACKART:
          sourceList = trackArtSources;
          break;
      }

      return sourceList;
    }

    public ReadOnlyCollection<DBSourceInfo> GetList(DataType type)
    {
      return getEditableList(type).AsReadOnly();
    }

    #endregion

    #region DataProvider Loading Logic

    private void loadProvidersFromDatabase()
    {
      logger.Info("Loading existing data sources...");

      foreach (DBSourceInfo currSource in DBSourceInfo.GetAll())
        updateListsWith(currSource);

      trackDetailSources.Sort(sorters[DataType.TRACKDETAIL]);
      artistDetailSources.Sort(sorters[DataType.ARTISTDETAIL]);
      albumDetailSources.Sort(sorters[DataType.ALBUMDETAIL]);
      albumArtSources.Sort(sorters[DataType.ALBUMART]);
      artistArtSources.Sort(sorters[DataType.ARTISTART]);
      trackArtSources.Sort(sorters[DataType.TRACKART]);
    }

    public void LoadInternalProviders()
    {
      logger.Info("Checking internal scripts for updates...");

      AddSource(typeof(LocalProvider));
      AddSource(typeof(FanartTVProvider));
      AddSource(typeof(LastFmProvider));
      AddSource(typeof(DgProvider));
      AddSource(typeof(AllMusicProvider));
      AddSource(typeof(HTBackdropsProvider)); 
      //AddSource(typeof(EchoNestProvider));
      AddSource(typeof(MyMusicProvider));
      AddSource(typeof(MyVideosProvider));
      AddSource(typeof(ManualProvider));
      normalizePriorities();
    }

    public AddSourceResult AddSource(Type providerType, string scriptContents)
    {
      return AddSource(providerType, scriptContents, false);
    }

    public AddSourceResult AddSource(Type providerType, string scriptContents, bool active)
    {
      return AddSourceResult.SUCCESS;
    }

    public AddSourceResult AddSource(Type providerType)
    {
      // internal scripts dont need to be updated, so just quit
      // if we dont need to reload everything
      //if (updateOnly) return AddSourceResult.FAILED;

      foreach (DBSourceInfo currSource in allSources)
        if (currSource.ProviderType == providerType)
          return AddSourceResult.FAILED;

      DBSourceInfo newSource = new DBSourceInfo();
      newSource.ProviderType = providerType;
      newSource.Commit();
      updateListsWith(newSource);
      normalizePriorities();

      return AddSourceResult.SUCCESS;
    }

    public void RemoveSource(DBSourceInfo source)
    {
      if (source == null)
        return;

      foreach (DataType currType in Enum.GetValues(typeof(DataType)))
        lock (getEditableList(currType))
          getEditableList(currType).Remove(source);

      lock (allSources) allSources.Remove(source);
      source.Delete();
    }

    private void updateListsWith(DBSourceInfo newSource)
    {
      if (newSource.ProviderType == null)
      {
        logger.Info("Removing invalid provider.");
        newSource.Delete();
        return;
      }

      lock (allSources)
        if (!allSources.Contains(newSource))
          allSources.Add(newSource);

      lock (artistArtSources)
      {
        if (newSource.Provider.ProvidesArtistArt && !artistArtSources.Contains(newSource))
          artistArtSources.Add(newSource);
        else if (!newSource.Provider.ProvidesArtistArt && artistArtSources.Contains(newSource))
          artistArtSources.Remove(newSource);
      }

      lock (albumArtSources)
      {
        logger.Info("albumArtSources: " + newSource.Provider + " " + newSource.Provider.ProvidesAlbumArt + " - " + !albumArtSources.Contains(newSource));
        if (newSource.Provider.ProvidesAlbumArt && !albumArtSources.Contains(newSource))
          albumArtSources.Add(newSource);
        else if (!newSource.Provider.ProvidesAlbumArt && albumArtSources.Contains(newSource))
          albumArtSources.Remove(newSource);
      }

      lock (trackArtSources)
      {
        if (newSource.Provider.ProvidesTrackArt && !trackArtSources.Contains(newSource))
          trackArtSources.Add(newSource);
        else if (!newSource.Provider.ProvidesTrackArt && trackArtSources.Contains(newSource))
          trackArtSources.Remove(newSource);
      }
      
      lock (trackDetailSources)
      {
        if (newSource.Provider.ProvidesTrackDetails && !trackDetailSources.Contains(newSource))
          trackDetailSources.Add(newSource);
        else if (!newSource.Provider.ProvidesTrackDetails && trackDetailSources.Contains(newSource))
          trackDetailSources.Remove(newSource);
      }

      lock (artistDetailSources)
      {
        if (newSource.Provider.ProvidesArtistDetails && !artistDetailSources.Contains(newSource))
          artistDetailSources.Add(newSource);
        else if (!newSource.Provider.ProvidesArtistDetails && artistDetailSources.Contains(newSource))
          artistDetailSources.Remove(newSource);
      }

      lock (albumDetailSources)
      {
        if (newSource.Provider.ProvidesAlbumDetails && !albumDetailSources.Contains(newSource))
          albumDetailSources.Add(newSource);
        else if (!newSource.Provider.ProvidesAlbumDetails && albumDetailSources.Contains(newSource))
          albumDetailSources.Remove(newSource);
      }
    
    }

    // reinitializes all the priorities based on existing list order.
    // should be called after new items have been added to the list or an
    // item has been ignored
    private void normalizePriorities()
    {
      foreach (DataType currType in Enum.GetValues(typeof(DataType)))
      {
        int count = 0;
        foreach (DBSourceInfo currSource in getEditableList(currType))
        {
          if (currSource.GetPriority(currType) != count && currSource.GetPriority(currType) != -1)
          {
            currSource.SetPriority(currType, count);
            currSource.Commit();
          }
          count++;
        }
      }
    }

    #endregion

    #region Data Loading Methods

    /// <summary>
    /// Based on the calculated signature retrive data from providers
    /// </summary>
    /// <param name="mvSignature"></param>
    /// <returns></returns>
    public List<DBTrackInfo> GetTrackDetail(MusicVideoSignature mvSignature)
    {
      List<DBSourceInfo> trackSources;
      lock (trackDetailSources) trackSources = new List<DBSourceInfo>(trackDetailSources);

      List<DBSourceInfo> artistSources;
      lock (artistDetailSources) artistSources = new List<DBSourceInfo>(artistDetailSources);


      // Try each datasource (ordered by their priority) to get results
      List<DBTrackInfo> results = new List<DBTrackInfo>();
      foreach (DBSourceInfo currSource in trackSources)
      {
        if (currSource.IsDisabled(DataType.TRACKDETAIL))
          continue;

        // if we have reached the minimum number of possible matches required, we are done
        if (results.Count >= mvCentralCore.Settings.MinimumMatches && mvCentralCore.Settings.MinimumMatches != 0)
        {
          logger.Debug("We have reached the minimum number of possible matches required, we are done.");
          break;
        }

        // search with the current provider
        List<DBTrackInfo> newResults = currSource.Provider.GetTrackDetail(mvSignature);

        // tag the results with the current source
        foreach (DBTrackInfo currMusicVideo in newResults)
        {
          currMusicVideo.PrimarySource = currSource;

          // ****************** Additional Artist Info Processing ******************
          // Check and update Artist details from additional providers
          DBArtistInfo artInfo = new DBArtistInfo();
          artInfo = currMusicVideo.ArtistInfo[0];

          foreach (DBSourceInfo artistExtraInfo in artistDetailSources)
          {
            if (artistExtraInfo != currSource)
            {
              logger.Debug("Searching for additional Artist infomation from Provider: " + artistExtraInfo.Provider.Name);
              artInfo = artistExtraInfo.Provider.GetArtistDetail(currMusicVideo).ArtistInfo[0];
            }
          }
          artInfo.DisallowBackgroundUpdate = true;
          currMusicVideo.ArtistInfo[0] = artInfo;

          // If album support disabled or no associated album then skip album processing
          if (mvCentralCore.Settings.DisableAlbumSupport || currMusicVideo.AlbumInfo.Count == 0)
            continue;

          // ****************** Additional Album Info Processing ******************
          // Check and update Album details from additional providers
          DBAlbumInfo albumInfo = new DBAlbumInfo();
          albumInfo = currMusicVideo.AlbumInfo[0];

          foreach (DBSourceInfo albumExtraInfo in albumDetailSources)
          {
            if (albumExtraInfo != currSource)
            {
              logger.Debug("Searching for additional Album infomation from Provider: " + albumExtraInfo.Provider.Name);
              albumInfo = albumExtraInfo.Provider.GetAlbumDetail(currMusicVideo).AlbumInfo[0];
            }
          }
          albumInfo.DisallowBackgroundUpdate = true;
          currMusicVideo.AlbumInfo[0] = albumInfo;
        }       

        // add results to our total result list and log what we found
        results.AddRange(newResults);
      }
      return results;
    }

    /// <summary>
    /// Update the track with the received data
    /// </summary>
    /// <param name="mvTrackInfo"></param>
    public void Update(DBTrackInfo mvTrackInfo)
    {
      List<DBSourceInfo> sources;
      lock (trackDetailSources) sources = new List<DBSourceInfo>(trackDetailSources);

      // unlock the mv fields for the first iteration
      mvTrackInfo.ProtectExistingValuesFromCopy(false);

      // first update from the primary source of this data
      int providerCount = 0;
      if (mvTrackInfo.PrimarySource != null && mvTrackInfo.PrimarySource.Provider != null)
      {
        UpdateResults success = mvTrackInfo.PrimarySource.Provider.UpdateTrack(mvTrackInfo);
        logger.Debug("*** UPDATE: Track='{0}', Provider='{1}', Version={2}, Result={3}", mvTrackInfo.Track, mvTrackInfo.PrimarySource.Provider.Name, mvTrackInfo.PrimarySource.Provider.Version, success.ToString());
        providerCount++;
      }

      foreach (DBSourceInfo currSource in sources)
      {
        if (currSource.IsDisabled(DataType.TRACKDETAIL))
          continue;

        if (currSource == mvTrackInfo.PrimarySource)
          continue;

        providerCount++;

        if (providerCount <= mvCentralCore.Settings.DataProviderRequestLimit || mvCentralCore.Settings.DataProviderRequestLimit == 0)
        {
          UpdateResults success = currSource.Provider.UpdateTrack(mvTrackInfo);
          logger.Debug("*** UPDATE: Track='{0}', Provider='{1}', Version={2}, Result={3}", mvTrackInfo.Track, currSource.Provider.Name, currSource.Provider.Version, success.ToString());
        }
        else
        {
          // stop update
          break;
        }

        if (mvCentralCore.Settings.UseTranslator)
        {
          mvTrackInfo.Translate();
        }
      }
    }

    /// <summary>
    /// get the artist details
    /// </summary>
    /// <param name="mv"></param>
    /// <returns></returns>
    public DBTrackInfo GetArtistDetail(DBTrackInfo mv)
    {
      // ****************** Additional Artist Info Processing ******************
      // Check and update Artist details from additional providers
      DBArtistInfo artInfo = new DBArtistInfo();
      artInfo = mv.ArtistInfo[0];

      foreach (DBSourceInfo artistExtraInfo in artistDetailSources)
      {
        if (artInfo.PrimarySource != artistExtraInfo.Provider)
        {
          artInfo = artistExtraInfo.Provider.GetArtistDetail(mv).ArtistInfo[0];
          artInfo.PrimarySource = artistExtraInfo;
        }
      }
      mv.ArtistInfo[0] = artInfo;

      return mv;
    }

    /// <summary>
    /// get the album details
    /// </summary>
    /// <param name="mv"></param>
    /// <returns></returns>
    public DBTrackInfo GetAlbumDetail(DBTrackInfo mv)
    {
      // ****************** Additional Album Info Processing ******************
      // Check and update Album details from additional providers
      DBAlbumInfo albumInfo = new DBAlbumInfo();
      albumInfo = mv.AlbumInfo[0];

      foreach (DBSourceInfo albumExtraInfo in albumDetailSources)
      {
        if (albumInfo.PrimarySource != albumExtraInfo.Provider)
        {
          albumInfo = albumExtraInfo.Provider.GetAlbumDetail(mv).AlbumInfo[0];
          albumInfo.PrimarySource = albumExtraInfo;
        }
      }
      mv.AlbumInfo[0] = albumInfo;

      return mv;
    }

    /// <summary>
    /// Get artwork for suppiled object, Artist, Album or Track
    /// </summary>
    /// <param name="mvDBObject"></param>
    /// <returns></returns>
    public bool GetArt(DBBasicInfo mvDBObject, bool primarySourceOnly)
    {
      bool success = false;
      int artWorkAdded = 0;
      logger.Debug("In Method: GetArt(DBBasicInfo mvDBObject, bool primarySourceOnly = " + primarySourceOnly + ")");

      // Artist
      if (mvDBObject.GetType() == typeof(DBArtistInfo))
      {
        var _count = 0;
        foreach (string _art in mvDBObject.AlternateArts)
        {
          if (File.Exists(_art))
            _count++;
        }
        logger.Debug("GetArt: Artist: mvDBObject.AlternateArts.Count: " + _count + " mvCentralCore.Settings.MaxArtistArts: " + mvCentralCore.Settings.MaxArtistArts);
        // if we have already hit our limit for the number of artist arts to load, quit
        if (_count >= mvCentralCore.Settings.MaxArtistArts)
          return true;

        List<DBSourceInfo> sources;
        lock (artistArtSources) sources = new List<DBSourceInfo>(artistArtSources);

        foreach (DBSourceInfo currSource in sources)
        {
          if (currSource.IsDisabled(DataType.ARTISTART))
            continue;

          if (currSource.Provider != mvDBObject.PrimarySource.Provider && primarySourceOnly)
            continue;
          logger.Debug("Try to get art from provider : " + currSource.Provider.Name);

          success = currSource.Provider.GetArtistArt((DBArtistInfo)mvDBObject);
          if (success)
            artWorkAdded++;
        }
        if (artWorkAdded > 0)
        {
          mvDBObject.Commit();
          return true;
        }
      }

      // Album
      if (mvDBObject.GetType() == typeof(DBAlbumInfo))
      {
        var _count = 0;
        foreach (string _art in mvDBObject.AlternateArts)
        {
          if (File.Exists(_art))
            _count++;
        }
        logger.Debug("GetArt: Album: mvDBObject.AlternateArts.Count: " + _count + " mvCentralCore.Settings.MaxAlbumArts: " + mvCentralCore.Settings.MaxAlbumArts);
        // if we have already hit our limit for the number of album arts to load, quit
        if (_count >= mvCentralCore.Settings.MaxAlbumArts)
          return true;

        List<DBSourceInfo> sources;
        lock (albumArtSources) sources = new List<DBSourceInfo>(albumArtSources);
        artWorkAdded = 0;
        foreach (DBSourceInfo currSource in sources)
        {
          logger.Debug("*** : " + currSource.Provider.Name + " - " + currSource.IsDisabled(DataType.ALBUMART));
          if (currSource.IsDisabled(DataType.ALBUMART))
            continue;

          logger.Debug("*** : " + currSource.Provider.Name + " - " + mvDBObject.PrimarySource.Provider.Name + " - " + primarySourceOnly);
          if (currSource.Provider != mvDBObject.PrimarySource.Provider && primarySourceOnly)
            continue;

          logger.Debug("Try to get art from provider: " + currSource.Provider.Name);

          success = currSource.Provider.GetAlbumArt((DBAlbumInfo)mvDBObject);
          if (success)
            artWorkAdded++;
        }
        if (artWorkAdded > 0)
        {
          mvDBObject.Commit();
          return true;
        }
      }

      // Track
      if (mvDBObject.GetType() == typeof(DBTrackInfo))
      {
        var _count = 0;
        foreach (string _art in mvDBObject.AlternateArts)
        {
          if (File.Exists(_art))
            _count++;
        }
        logger.Debug("GetArt: Track: mvDBObject.AlternateArts.Count: " + _count + " mvCentralCore.Settings.MaxTrackArts: " + mvCentralCore.Settings.MaxTrackArts);
        // if we have already hit our limit for the number of Track arts to load, quit
        if (_count >= mvCentralCore.Settings.MaxTrackArts)
          return true;

        List<DBSourceInfo> sources;
        lock (trackArtSources) sources = new List<DBSourceInfo>(trackArtSources);
        artWorkAdded = 0;
        foreach (DBSourceInfo currSource in sources)
        {
          if (currSource.IsDisabled(DataType.TRACKART))
            continue;
          
          if (currSource.Provider != mvDBObject.PrimarySource.Provider && primarySourceOnly)
            continue;

          logger.Debug("Try to get art from provider : " + currSource.Provider.Name);

          success = currSource.Provider.GetTrackArt((DBTrackInfo)mvDBObject);
          if (success)
            artWorkAdded++;
        }
        if (artWorkAdded > 0)
        {
          mvDBObject.Commit();
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Get the details for the suppkied object type, Artist, Album or Track
    /// </summary>
    /// <param name="mvDbObject"></param>
    /// <returns></returns>
    public bool GetDetails(DBBasicInfo mvDbObject)
    {
      // Artist 
      if (mvDbObject.GetType() == typeof(DBArtistInfo))
      {
        // if we have already hit our limit for the number of Artist arts to load, quit
        if (mvDbObject.AlternateArts.Count >= mvCentralCore.Settings.MaxArtistArts)
          return true;

        List<DBSourceInfo> sources;
        lock (artistArtSources) sources = new List<DBSourceInfo>(artistArtSources);

        foreach (DBSourceInfo currSource in sources)
        {
          if (currSource.IsDisabled(DataType.ARTISTART))
            continue;

          bool success = currSource.Provider.GetDetails((DBArtistInfo)mvDbObject);
          if (success) return true;
        }
      }

      // Album
      if (mvDbObject.GetType() == typeof(DBAlbumInfo))
      {
        // if we have already hit our limit for the number of Album arts to load, quit
        if (mvDbObject.AlternateArts.Count >= mvCentralCore.Settings.MaxAlbumArts)
          return true;

        List<DBSourceInfo> sources;
        lock (albumArtSources) sources = new List<DBSourceInfo>(albumArtSources);

        foreach (DBSourceInfo currSource in sources)
        {
          if (currSource.IsDisabled(DataType.ALBUMART))
            continue;

          bool success = currSource.Provider.GetDetails((DBAlbumInfo)mvDbObject);
          if (success) return true;
        }
      }

      // Track
      if (mvDbObject.GetType() == typeof(DBTrackInfo))
      {
        // if we have already hit our limit for the number of Track arts to load, quit
        if (mvDbObject.AlternateArts.Count >= mvCentralCore.Settings.MaxTrackArts)
          return true;

        List<DBSourceInfo> sources;
        lock (trackArtSources) sources = new List<DBSourceInfo>(trackArtSources);

        foreach (DBSourceInfo currSource in sources)
        {
          if (currSource.IsDisabled(DataType.TRACKART))
            continue;

          bool success = currSource.Provider.GetDetails((DBTrackInfo)mvDbObject);
          if (success) return true;
        }
      }
      return false;
    }

    #endregion
  }

}
