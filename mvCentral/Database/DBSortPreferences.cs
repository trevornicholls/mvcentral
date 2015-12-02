using Cornerstone.Database;
using Cornerstone.Database.Tables;

namespace mvCentral.Database
{
  [DBTableAttribute("sort_preferences")]
  public class DBSortPreferences : mvCentralDBTable
  {

    #region Database Fields

    [DBFieldAttribute(Default = "False")]
    public bool SortArtistAscending
    {
      get { return _sortArtistAscending; }
      set
      {
        if (_sortArtistAscending != value)
        {
          _sortArtistAscending = value;
          commitNeeded = true;
        }
      }
    } private bool _sortArtistAscending;


    [DBFieldAttribute(Default = "True")]
    public bool SortAlbumAscending
    {
      get { return _sortAlbumAscending; }
      set
      {
        if (_sortAlbumAscending != value)
        {
          _sortAlbumAscending = value;
          commitNeeded = true;
        }
      }
    } private bool _sortAlbumAscending;


    [DBFieldAttribute(Default = "True")]
    public bool SortVideoTitleAscending
    {
      get { return _sortVideoTitleAscending; }
      set
      {
        if (_sortVideoTitleAscending != value)
        {
          _sortVideoTitleAscending = value;
          commitNeeded = true;
        }
      }
    } private bool _sortVideoTitleAscending;



    [DBFieldAttribute(Default = "False")]
    public bool SortDateAddedAscending
    {
      get { return _sortDateAddedAscending; }
      set
      {
        if (_sortDateAddedAscending != value)
        {
          _sortDateAddedAscending = value;
          commitNeeded = true;
        }
      }
    } private bool _sortDateAddedAscending;

    [DBFieldAttribute(Default = "True")]
    public bool SortRuntimeAscending
    {
      get { return _sortRuntimeAscending; }
      set
      {
        if (_sortRuntimeAscending != value)
        {
          _sortRuntimeAscending = value;
          commitNeeded = true;
        }
      }
    } private bool _sortRuntimeAscending;


    [DBFieldAttribute(Default = "True")]
    public bool SortMostPlayedArtistsAscending
    {
      get { return _sortMostPlayedArtistsAscending; }
      set
      {
        if (_sortMostPlayedArtistsAscending != value)
        {
          _sortMostPlayedArtistsAscending = value;
          commitNeeded = true;
        }
      }
    } private bool _sortMostPlayedArtistsAscending;

    [DBFieldAttribute(Default = "False")]
    public bool SortMostPlayedVideosAscending
    {
      get { return _sortMostPlayedVideosAscending; }
      set
      {
        if (_sortMostPlayedVideosAscending != value)
        {
          _sortMostPlayedVideosAscending = value;
          commitNeeded = true;
        }
      }
    } private bool _sortMostPlayedVideosAscending;

    [DBFieldAttribute(Default = "False")]
    public bool SortLeastPlayedArtistsAscending
    {
      get { return _sortLeastPlayedArtistsAscending; }
      set
      {
        if (_sortLeastPlayedArtistsAscending != value)
        {
          _sortLeastPlayedArtistsAscending = value;
          commitNeeded = true;
        }
      }
    } private bool _sortLeastPlayedArtistsAscending;


    [DBFieldAttribute(Default = "True")]
    public bool SortLeastPlayedVideosAscending
    {
      get { return _sortLeastPlayedVideosAscending; }
      set
      {
        if (_sortLeastPlayedVideosAscending != value)
        {
          _sortLeastPlayedVideosAscending = value;
          commitNeeded = true;
        }
      }
    } private bool _sortLeastPlayedVideosAscending;


    [DBFieldAttribute(Default = "True")]
    public bool SortAlbumReleaseDateAscending
    {
      get { return _sortAlbumReleaseDateAscending; }
      set
      {
        if (_sortAlbumReleaseDateAscending != value)
        {
          _sortAlbumReleaseDateAscending = value;
          commitNeeded = true;
        }
      }
    } private bool _sortAlbumReleaseDateAscending;

    [DBFieldAttribute(Default = "True")]
    public bool SortComposerAscending
    {
      get { return _sortComposerAscending; }
      set
      {
        if (_sortComposerAscending != value)
        {
          _sortComposerAscending = value;
          commitNeeded = true;
        }
      }
    } private bool _sortComposerAscending;


    #endregion

    static DBSortPreferences instance;


    #region Database Management Methods


    public static DBSortPreferences Instance
    {
      get
      {
        if (instance == null)
        {
          var all = mvCentralCore.DatabaseManager.Get<DBSortPreferences>(null);

          if (all.Count > 0)
            instance = all[0];
          else
          {
            instance = new DBSortPreferences();
            instance.Commit();
          }
        }
        return instance;
      }
    }

    #endregion
  }
}
