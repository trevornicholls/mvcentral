using Cornerstone.Database;
using Cornerstone.Database.CustomTypes;
using Cornerstone.Database.Tables;

using System.Collections.Generic;

namespace mvCentral.Database
{
  [DBTableAttribute("user_musicvideo_settings")]
  public class DBUserMusicVideoSettings : mvCentralDBTable
  {
    public bool RatingChanged = false;
    public bool WatchCountChanged = false;

    public override void AfterDelete()
    {
    }

    #region Database Fields

    [DBFieldAttribute(Filterable = false)]
    public DBUser User
    {
      get { return user; }
      set
      {
        user = value;
        commitNeeded = true;
      }
    } private DBUser user;

    // Value between 0 and 10
    [DBFieldAttribute(FieldName = "user_rating", Default = null, AllowDynamicFiltering = false)]
    public int? UserRating
    {
      get { return _userRating; }
      set
      {
        if (value > 5) value = 5;
        if (value < 1) value = 1;

        if (_userRating != value)
        {
          _userRating = value;
          commitNeeded = true;
          if (!RetrievalInProcess) RatingChanged = true;
        }
      }
    } private int? _userRating;

    [DBFieldAttribute(FieldName = "watched", AllowDynamicFiltering = false)]
    public int WatchedCount
    {
      get { return _watched; }
      set
      {
        if (_watched != value)
        {
          _watched = value;
          commitNeeded = true;
          if (!RetrievalInProcess) WatchCountChanged = true;
        }
      }
    } private int _watched;

    [DBFieldAttribute(FieldName = "resume_part", Filterable = false)]
    public int ResumePart
    {
      get { return _resumePart; }

      set
      {
        _resumePart = value;
        commitNeeded = true;
      }
    } private int _resumePart;


    [DBFieldAttribute(FieldName = "resume_time")]
    public int ResumeTime
    {
      get { return _resumeTime; }

      set
      {
        _resumeTime = value;
        commitNeeded = true;
      }
    } private int _resumeTime;

    [DBFieldAttribute(FieldName = "resume_data", Default = null, Filterable = false)]
    public ByteArray ResumeData
    {
      get { return _resumeData; }

      set
      {
        _resumeData = value;
        commitNeeded = true;
      }
    } private ByteArray _resumeData;

    [DBRelation(AutoRetrieve = true, Filterable = false)]
    public RelationList<DBUserMusicVideoSettings, DBTrackInfo> AttachedMovies
    {
      get
      {
        if (_attachedMovies == null)
        {
          _attachedMovies = new RelationList<DBUserMusicVideoSettings, DBTrackInfo>(this);
        }
        return _attachedMovies;
      }
    } RelationList<DBUserMusicVideoSettings, DBTrackInfo> _attachedMovies;

    #endregion

    #region Database Management Methods

    public static DBUserMusicVideoSettings Get(int id)
    {
      return mvCentralCore.DatabaseManager.Get<DBUserMusicVideoSettings>(id);
    }

    public static List<DBUserMusicVideoSettings> GetAll()
    {
      return mvCentralCore.DatabaseManager.Get<DBUserMusicVideoSettings>(null);
    }

    #endregion
  }
}
