using System;
using System.Collections.Generic;
using System.Text;
using mvCentral.Database;
using Cornerstone.Database;
using Cornerstone.Database.Tables;
using mvCentral.Properties;
using System.Reflection;
using mvCentral.LocalMediaManagement;
using mvCentral.SignatureBuilders;
using System.Collections.ObjectModel;
using NLog;
using System.IO;
using Cornerstone.Tools.Translate;
using System.Globalization;
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

    public ReadOnlyCollection<DBSourceInfo> DetailSources
    {
      get
      {
        return detailSources.AsReadOnly();
      }
    }
    private List<DBSourceInfo> detailSources;


    public ReadOnlyCollection<DBSourceInfo> AlbumSources
    {
      get
      {
        return albumSources.AsReadOnly();
      }
    }
    private List<DBSourceInfo> albumSources;

    public ReadOnlyCollection<DBSourceInfo> ArtistSources
    {
      get
      {
        return artistSources.AsReadOnly();
      }
    }
    private List<DBSourceInfo> artistSources;

    public ReadOnlyCollection<DBSourceInfo> TrackSources
    {
      get
      {
        return trackSources.AsReadOnly();
      }
    }
    private List<DBSourceInfo> trackSources;

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

      detailSources = new List<DBSourceInfo>();
      trackSources = new List<DBSourceInfo>();
      albumSources = new List<DBSourceInfo>();
      artistSources = new List<DBSourceInfo>();
      allSources = new List<DBSourceInfo>();

      sorters = new Dictionary<DataType, DBSourceInfoComparer>();
      sorters[DataType.DETAIL] = new DBSourceInfoComparer(DataType.DETAIL);
      sorters[DataType.ALBUM] = new DBSourceInfoComparer(DataType.ALBUM);
      sorters[DataType.ARTIST] = new DBSourceInfoComparer(DataType.ARTIST);
      sorters[DataType.TRACK] = new DBSourceInfoComparer(DataType.TRACK);

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

      foreach (DBSourceInfo currSource in detailSources)
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
          if (currSource.IsScriptable() && currType == DataType.TRACK)
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
          else if (currType == DataType.ALBUM && currSource.IsScriptable())
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
            if (currSource.Provider is LocalProvider)
            {
              currSource.SetPriority(currType, 0);
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
        case DataType.DETAIL:
          sourceList = detailSources;
          break;
        case DataType.ALBUM:
          sourceList = albumSources;
          break;
        case DataType.ARTIST:
          sourceList = artistSources;
          break;
        case DataType.TRACK:
          sourceList = trackSources;
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

      detailSources.Sort(sorters[DataType.DETAIL]);
      albumSources.Sort(sorters[DataType.ALBUM]);
      artistSources.Sort(sorters[DataType.ARTIST]);
      trackSources.Sort(sorters[DataType.TRACK]);
    }

    public void LoadInternalProviders()
    {
      logger.Info("Checking internal scripts for updates...");

      AddSource(typeof(LocalProvider));
      AddSource(typeof(LastFMProvider));
      //AddSource(typeof(EchoNestProvider));
      AddSource(typeof(DGProvider));
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
      if (updateOnly) return AddSourceResult.FAILED;

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

      lock (artistSources)
      {
        if (newSource.Provider.ProvidesArtistArt && !artistSources.Contains(newSource))
          artistSources.Add(newSource);
        else if (!newSource.Provider.ProvidesArtistArt && artistSources.Contains(newSource))
          artistSources.Remove(newSource);
      }

      lock (albumSources)
      {
        if (newSource.Provider.ProvidesAlbumArt && !albumSources.Contains(newSource))
          albumSources.Add(newSource);
        else if (!newSource.Provider.ProvidesAlbumArt && albumSources.Contains(newSource))
          albumSources.Remove(newSource);
      }

      lock (trackSources)
      {
        if (newSource.Provider.ProvidesTrackArt && !trackSources.Contains(newSource))
          trackSources.Add(newSource);
        else if (!newSource.Provider.ProvidesTrackArt && trackSources.Contains(newSource))
          trackSources.Remove(newSource);
      }

      lock (detailSources)
      {
        if (newSource.Provider.ProvidesDetails && !detailSources.Contains(newSource))
          detailSources.Add(newSource);
        else if (!newSource.Provider.ProvidesDetails && detailSources.Contains(newSource))
          detailSources.Remove(newSource);
      }
    }

    // reinitializes all the priorities based on existing list order.
    // should be called after new items have been added to the list or an
    // tiem has been ignored
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

    public List<DBTrackInfo> Get(MusicVideoSignature mvSignature)
    {
      List<DBSourceInfo> sources;
      lock (detailSources) sources = new List<DBSourceInfo>(detailSources);

      // Try each datasource (ordered by their priority) to get results
      List<DBTrackInfo> results = new List<DBTrackInfo>();
      foreach (DBSourceInfo currSource in sources)
      {
        if (currSource.IsDisabled(DataType.DETAIL))
          continue;

        // if we have reached the minimum number of possible matches required, we are done
        if (results.Count >= mvCentralCore.Settings.MinimumMatches &&
            mvCentralCore.Settings.MinimumMatches != 0)
          break;

        // search with the current provider
        List<DBTrackInfo> newResults = currSource.Provider.Get(mvSignature);

        // tag the results with the current source

        foreach (DBTrackInfo currMusicVideo in newResults)
          currMusicVideo.PrimarySource = currSource;

        // add results to our total result list and log what we found
        results.AddRange(newResults);
        //logger.Debug("SEARCH: Title='{0}', Provider='{1}', Version={2}, Number of Results={3}", mvSignature.Title, currSource.Provider.Name, currSource.Provider.Version, newResults.Count);
      }

      return results;
    }

    public void Update(DBTrackInfo mv)
    {
      List<DBSourceInfo> sources;
      lock (detailSources) sources = new List<DBSourceInfo>(detailSources);

      // unlock the mv fields for the first iteration
      mv.ProtectExistingValuesFromCopy(false);

      // first update from the primary source of this data
      int providerCount = 0;
      if (mv.PrimarySource != null && mv.PrimarySource.Provider != null)
      {
        UpdateResults success = mv.PrimarySource.Provider.Update(mv);
        //logger.Debug("UPDATE: Track='{0}', Provider='{1}', Version={2}, Result={3}", mv.Track, mv.PrimarySource.Provider.Name, mv.PrimarySource.Provider.Version, success.ToString());
        providerCount++;
      }

      foreach (DBSourceInfo currSource in sources)
      {
        if (currSource.IsDisabled(DataType.DETAIL))
          continue;

        if (currSource == mv.PrimarySource)
          continue;

        providerCount++;

        if (providerCount <= mvCentralCore.Settings.DataProviderRequestLimit || mvCentralCore.Settings.DataProviderRequestLimit == 0)
        {
          UpdateResults success = currSource.Provider.Update(mv);
          //logger.Debug("UPDATE: Track='{0}', Provider='{1}', Version={2}, Result={3}", mv.Track, currSource.Provider.Name, currSource.Provider.Version, success.ToString());
        }
        else
        {
          // stop update
          break;
        }

        if (mvCentralCore.Settings.UseTranslator)
        {
          mv.Translate();
        }
      }
    }

    public bool GetArt(DBBasicInfo mv)
    {

      logger.Debug("In Method GetArt - object is : " + mv.GetType().ToString());

      if (mv.GetType() == typeof(DBArtistInfo))
      {
        // if we have already hit our limit for the number of artist arts to load, quit
        if (mv.AlternateArts.Count >= mvCentralCore.Settings.MaxArtistArts)
          return true;

        List<DBSourceInfo> sources;
        lock (artistSources) sources = new List<DBSourceInfo>(artistSources);

        foreach (DBSourceInfo currSource in sources)
        {
          logger.Debug("Try to get art from provider : " + currSource.Provider.Name);
          if (currSource.IsDisabled(DataType.ARTIST))
            continue;

          bool success = currSource.Provider.GetArtistArt((DBArtistInfo)mv);
          if (success)
          {
            mv.Commit();
            //return true;
          }
        }
      }

      if (mv.GetType() == typeof(DBAlbumInfo))
      {
        // if we have already hit our limit for the number of album arts to load, quit
        if (mv.AlternateArts.Count >= mvCentralCore.Settings.MaxAlbumArts)
          return true;

        List<DBSourceInfo> sources;
        lock (albumSources) sources = new List<DBSourceInfo>(albumSources);

        foreach (DBSourceInfo currSource in sources)
        {
          logger.Debug("Try to get art from provider : " + currSource.Provider.Name);
          if (currSource.IsDisabled(DataType.ALBUM))
            continue;

          bool success = currSource.Provider.GetAlbumArt((DBAlbumInfo)mv);
          if (success)
          {
            mv.Commit();
            return true;
          }
        }
      }

      if (mv.GetType() == typeof(DBTrackInfo))
      {
        // if we have already hit our limit for the number of Track arts to load, quit
        if (mv.AlternateArts.Count >= mvCentralCore.Settings.MaxTrackArts)
          return true;

        List<DBSourceInfo> sources;
        lock (trackSources) sources = new List<DBSourceInfo>(trackSources);

        foreach (DBSourceInfo currSource in sources)
        {
          logger.Debug("Try to get art from provider : " + currSource.Provider.Name);
          if (currSource.IsDisabled(DataType.TRACK))
            continue;

          bool success = currSource.Provider.GetTrackArt((DBTrackInfo)mv);
          if (success)
          {
            mv.Commit();
            return true;
          }
        }
      }
      return false;
    }

    public bool GetDetails(DBBasicInfo mv)
    {

      if (mv.GetType() == typeof(DBArtistInfo))
      {
        List<DBSourceInfo> sources;
        lock (artistSources) sources = new List<DBSourceInfo>(artistSources);

        foreach (DBSourceInfo currSource in sources)
        {
          if (currSource.IsDisabled(DataType.ARTIST))
            continue;

          bool success = currSource.Provider.GetDetails((DBArtistInfo)mv);
          if (success) return true;
        }
      }

      if (mv.GetType() == typeof(DBAlbumInfo))
      {

        List<DBSourceInfo> sources;
        lock (albumSources) sources = new List<DBSourceInfo>(albumSources);

        foreach (DBSourceInfo currSource in sources)
        {
          if (currSource.IsDisabled(DataType.ALBUM))
            continue;

          bool success = currSource.Provider.GetDetails((DBAlbumInfo)mv);
          if (success) return true;
        }
      }

      if (mv.GetType() == typeof(DBTrackInfo))
      {
        // if we have already hit our limit for the number of Track arts to load, quit
        if (mv.AlternateArts.Count >= mvCentralCore.Settings.MaxTrackArts)
          return true;

        List<DBSourceInfo> sources;
        lock (trackSources) sources = new List<DBSourceInfo>(trackSources);

        foreach (DBSourceInfo currSource in sources)
        {
          if (currSource.IsDisabled(DataType.TRACK))
            continue;

          bool success = currSource.Provider.GetDetails((DBTrackInfo)mv);
          if (success) return true;
        }
      }
      return false;
    }

    #endregion
  }

}
