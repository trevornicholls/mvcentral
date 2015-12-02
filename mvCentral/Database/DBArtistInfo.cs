using Cornerstone.Database;
using Cornerstone.Database.Tables;

using mvCentral.Extensions;

using NLog;

using System;
using System.Collections.Generic;

namespace mvCentral.Database
{
  [DBTableAttribute("artist_info")]
  public class DBArtistInfo : DBBasicInfo, IComparable, IAttributeOwner
  {
    private static Logger logger = LogManager.GetCurrentClassLogger();

    public DBArtistInfo()
      : base()
    {
    }

    #region Database Fields

    [DBField(AllowDynamicFiltering = false)]
    public string Artist
    {
      get { return _artist; }
      set
      {
        _artist = value;
        Basic = value;
        PopulateSortBy();
        commitNeeded = true;
      }
    } private string _artist;


    [DBField(AllowDynamicFiltering = false)]
    public string ArtistAltname
    {
      get { return _artistAltName; }
      set
      {
        _artistAltName = value;
        Basic = value;
        commitNeeded = true;
      }
    } private string _artistAltName;

    [DBField]
    public string Born
    {
      get { return _born; }

      set
      {
        _born = value;
        commitNeeded = true;
      }
    } private string _born;

    [DBField]
    public string Death
    {
      get { return _death; }

      set
      {
        _death = value;
        commitNeeded = true;
      }
    } private string _death;


    [DBField]
    public string Formed
    {
      get { return _formed; }

      set
      {
        _formed = value;
        commitNeeded = true;
      }
    } private string _formed;

    [DBField]
    public string Disbanded
    {
      get { return _disbanded; }

      set
      {
        _disbanded = value;
        commitNeeded = true;
      }
    } private string _disbanded;


    [DBField]
    public string Genre
    {
      get { return _genre; }

      set
      {
        _genre = value;
        commitNeeded = true;
      }
    } private string _genre;

    [DBField]
    public string Styles
    {
      get { return _styles; }

      set
      {
        _styles = value;
        commitNeeded = true;
      }
    } private string _styles;

    [DBField]
    public string Tones
    {
      get { return _tones; }

      set
      {
        _tones = value;
        commitNeeded = true;
      }
    } private string _tones;

    [DBField]
    public string YearsActive
    {
      get { return _yearsactive; }

      set
      {
        _yearsactive = value;
        commitNeeded = true;
      }
    } private string _yearsactive;

    [DBField]
    public bool DisallowBackgroundUpdate
    {
      get { return _disallowbackgroundupdate; }

      set
      {
        _disallowbackgroundupdate = value;
        commitNeeded = true;
      }
    } private bool _disallowbackgroundupdate;

    #endregion

    #region General Management Methods


    #endregion

    #region Database Management Methods

    public static DBArtistInfo Get(int id)
    {
      return mvCentralCore.DatabaseManager.Get<DBArtistInfo>(id);
    }

    public static new List<DBArtistInfo> GetAll()
    {
      return mvCentralCore.DatabaseManager.Get<DBArtistInfo>(null);
    }

    public static DBArtistInfo Get(string Artist)
    {
      if (Artist.Trim().Length == 0) return null;
      foreach (DBArtistInfo db1 in GetAll())
      {
        if (String.Equals(Artist, db1.Artist, StringComparison.OrdinalIgnoreCase))
          return db1;

        if (String.Equals(Artist, db1.MdID))
          return db1;

      }
      return null;
    }

    public static List<DBArtistInfo> GetFuzzy(string Artist)
    {
      List<DBArtistInfo> artistList = new List<DBArtistInfo>();

      if (Artist.Trim().Length == 0)
        return null;
      foreach (DBArtistInfo artistRecord in GetAll())
      {
        if (artistRecord.Artist.Contains(Artist, StringComparison.OrdinalIgnoreCase))
          artistList.Add(artistRecord);
      }
      if (artistList.Count > 0)
        return artistList;
      else
        return null;
    }

    public static DBArtistInfo Get(DBTrackInfo mv)
    {
      if (mv.ArtistInfo.Count == 0) return null;
      foreach (DBArtistInfo db1 in GetAll())
      {
        if (db1.MdID.Trim().Length > 0)
          if (String.Equals(db1.MdID, mv.ArtistInfo[0].MdID)) return db1;
        if (db1.Artist.Trim().Length > 0)
          if (String.Equals(db1.Artist, mv.ArtistInfo[0].Artist)) return db1;

      }
      return null;
    }

    public static DBArtistInfo GetOrCreate(DBTrackInfo mv)
    {
      DBArtistInfo rtn = mv.ArtistInfo[0];
      if (rtn != null)
        return rtn;

      rtn = new DBArtistInfo();
      mv.ArtistInfo.Add(rtn);
      return rtn;
    }
    #endregion

    public override int CompareTo(object obj)
    {
      if (obj.GetType() == typeof(DBArtistInfo))
      {
        return SortBy.CompareTo(((DBArtistInfo)obj).SortBy);
      }
      return 0;
    }

    public override string ToString()
    {
      return Artist;
    }

  }
}
