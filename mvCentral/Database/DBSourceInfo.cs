using Cornerstone.Database;
using Cornerstone.Database.Tables;

using mvCentral.DataProviders;

using NLog;

using System;
using System.Collections.Generic;

namespace mvCentral.Database
{
  [DBTableAttribute("source_info")]
  public class DBSourceInfo : mvCentralDBTable
  {
    private static Logger logger = LogManager.GetCurrentClassLogger();

    #region Database Fields

    [DBFieldAttribute]
    public Type ProviderType
    {
      get { return providerType; }
      set
      {
        providerType = value;
        commitNeeded = true;
      }
    } protected Type providerType;

    /*
            [DBFieldAttribute(Default="null")]
            public DBScriptInfo SelectedScript {
                get { return selectedScript; }
                set {
                    selectedScript = value;
                    commitNeeded = true;
                }
            } protected DBScriptInfo selectedScript = null;
        
        
            [DBRelation(AutoRetrieve = true)]
            public RelationList<DBSourceInfo, DBScriptInfo> Scripts {
                get {
                    if (scripts == null) {
                        scripts = new RelationList<DBSourceInfo, DBScriptInfo>(this);
                    }
                    return scripts;
                }
            } RelationList<DBSourceInfo, DBScriptInfo> scripts;
            */
    [DBFieldAttribute]
    public int? DetailsPriority
    {
      get { return detailsPriority; }
      set
      {
        detailsPriority = value;
        commitNeeded = true;
      }
    } protected int? detailsPriority;

    [DBFieldAttribute]
    public int? ArtistDetailsPriority
    {
      get { return artistDetailsPriority; }
      set
      {
        artistDetailsPriority = value;
        commitNeeded = true;
      }
    } protected int? artistDetailsPriority;

    [DBFieldAttribute]
    public int? AlbumDetailsPriority
    {
      get { return albumDetailsPriority; }
      set
      {
        albumDetailsPriority = value;
        commitNeeded = true;
      }
    } protected int? albumDetailsPriority;


    [DBFieldAttribute]
    public int? AlbumPriority
    {
      get { return albumPriority; }
      set
      {
        albumPriority = value;
        commitNeeded = true;
      }
    } protected int? albumPriority;

    [DBFieldAttribute]
    public int? ArtistPriority
    {
      get { return artistPriority; }
      set
      {
        artistPriority = value;
        commitNeeded = true;
      }
    } protected int? artistPriority;

    [DBFieldAttribute]
    public int? TrackPriority
    {
      get { return trackPriority; }
      set
      {
        trackPriority = value;
        commitNeeded = true;
      }
    } protected int? trackPriority;

    public void SetPriority(DataType type, int value)
    {
      switch (type)
      {
        case DataType.TRACKDETAIL:
          DetailsPriority = value;
          break;
        case DataType.ARTISTDETAIL:
          ArtistDetailsPriority = value;
          break;
        case DataType.ALBUMDETAIL:
          AlbumDetailsPriority = value;
          break;
        case DataType.ALBUMART:
          AlbumPriority = value;
          break;
        case DataType.ARTISTART:
          ArtistPriority = value;
          break;
        case DataType.TRACKART:
          TrackPriority = value;
          break;
      }
    }

    public int? GetPriority(DataType type)
    {
      switch (type)
      {
        case DataType.TRACKDETAIL:
          return detailsPriority;
        case DataType.ARTISTDETAIL:
          return artistDetailsPriority;
        case DataType.ALBUMDETAIL:
          return albumDetailsPriority;
        case DataType.ALBUMART:
          return albumPriority;
        case DataType.ARTISTART:
          return artistPriority;
        case DataType.TRACKART:
          return trackPriority;
        default:
          return null;
      }
    }

    public bool IsDisabled(DataType type)
    {
      return GetPriority(type) == -1;
    }

    public bool IsScriptable()
    {
      return false;
      //            return !(SelectedScript == null || SelectedScript.Contents.Trim().Length == 0);
    }

    #endregion

    #region Properties

    public IMusicVideoProvider Provider
    {
      get
      {
        //                if (SelectedScript != null && !(SelectedScript.Contents.Trim().Length == 0))
        //                    return SelectedScript.Provider;       

        if (provider == null)
        {
          try
          {
            provider = (IMusicVideoProvider)Activator.CreateInstance(providerType);
          }
          catch (Exception e)
          {
            if (providerType != null)
              logger.Error("Failed creating instance for type '{0}': {1}", providerType, e);
            else
              logger.Error("Failed creating instance: no provider type specified.");
          }
        }

        return provider;

      }
    } protected IMusicVideoProvider provider = null;

    #endregion

    public override void Delete()
    {
      //            foreach (DBScriptInfo currScript in Scripts)
      //                currScript.Delete();

      base.Delete();
    }

    public override string ToString()
    {
      if (Provider != null)
        return Provider.Name;
      else
        return base.ToString();
    }

    #region Static Methods

    public static List<DBSourceInfo> GetAll()
    {
      return mvCentralCore.DatabaseManager.Get<DBSourceInfo>(null);
    }

    public static DBSourceInfo GetFromProviderObject(IMusicVideoProvider provider)
    {
      foreach (DBSourceInfo currSource in GetAll())
      {
        if (currSource.providerType == provider.GetType())
        {
          //                    if (currSource.IsScriptable())
          //                        return GetFromScriptID(((IScriptableMovieProvider)provider).ScriptID);
          //                    else
          return currSource;
        }
      }

      return null;
    }

    public static DBSourceInfo GetFromScriptID(int scriptID)
    {
      foreach (DBSourceInfo currSource in GetAll())
      {
        //                if (currSource.IsScriptable() && currSource.SelectedScript.Provider.ScriptID == scriptID)
        //                    return currSource;
      }

      return null;
    }

    #endregion

  }

  public class DBSourceInfoComparer : IComparer<DBSourceInfo>
  {
    private DataType sortType;

    public DBSourceInfoComparer(DataType sortType)
    {
      this.sortType = sortType;
    }

    public int Compare(DBSourceInfo x, DBSourceInfo y)
    {
      if (x.GetPriority(sortType) == -1 && y.GetPriority(sortType) == -1)
        return x.Provider.Name.CompareTo(y.Provider.Name);

      if (x.GetPriority(sortType) == -1)
        return 1;

      if (y.GetPriority(sortType) == -1)
        return -1;

      if (x.GetPriority(sortType) < y.GetPriority(sortType))
        return -1;

      if (x.GetPriority(sortType) > y.GetPriority(sortType))
        return 1;

      return 0;
    }
  }
}
