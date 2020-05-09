using MediaPortal.GUI.Library;

using mvCentral.Database;
using mvCentral.Localizations;

using NLog;

using System.Collections.Generic;

namespace mvCentral.GUI
{

  /// <summary>
  /// enum of all possible sort fields
  /// </summary>
  public enum SortingFields
  {
    Artist = 1,
    Album = 2,
    VideoTitle = 3,
    DateAdded = 4,
    Runtime = 5,
    MostPlayedArtists = 6,
    MostPlayedVideos = 7,
    LeastPlayedArtists = 8,
    LeastPlayedVideos = 9,
    AlbumReleaseDate = 10,
    Composer = 11,
    Name = 12,
    Filename = 13
  }
  /// <summary>
  /// Enum for Direction
  /// </summary>
  public enum SortingDirections
  {
    Ascending,
    Descending
  }

  public class Sort
  {
    /// <summary>
    /// Return the translation for the sort type
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    public static string GetFriendlySortName(SortingFields field)
    {
      switch (field)
      {
        case SortingFields.Artist:
          return Localization.Artist;
        case SortingFields.Album:
          return Localization.Album;
        case SortingFields.VideoTitle:
          return Localization.VideoTitle;
        case SortingFields.DateAdded:
          return Localization.DateAdded;
        case SortingFields.MostPlayedArtists:
          return Localization.MostPlayedArtists;
        case SortingFields.MostPlayedVideos:
          return Localization.MostPlayedVideos;
        case SortingFields.LeastPlayedArtists:
          return Localization.LeastPlayedArtists;
        case SortingFields.LeastPlayedVideos:
          return Localization.LeastPlayedVideos;
        case SortingFields.AlbumReleaseDate:
          return Localization.AlbumReleaseDate;
        case SortingFields.Composer:
          return Localization.Composer;
        case SortingFields.Name:
          return Localization.Name;
        case SortingFields.Filename:
          return Localization.Filename;
        case SortingFields.Runtime:
          return Localization.Runtime;
        default:
          return "";
      }
    }
    /// <summary>
    /// Get the stored sort direction
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    public static SortingDirections GetLastSortDirection(SortingFields field)
    {
      bool ascending;
      switch (field)
      {
        case SortingFields.VideoTitle:
          ascending = DBSortPreferences.Instance.SortVideoTitleAscending;
          break;
        case SortingFields.Artist:
          ascending = DBSortPreferences.Instance.SortArtistAscending;
          break;
        case SortingFields.Album:
          ascending = DBSortPreferences.Instance.SortAlbumAscending;
          break;
        case SortingFields.DateAdded:
          ascending = DBSortPreferences.Instance.SortDateAddedAscending;
          break;
        case SortingFields.Runtime:
          ascending = DBSortPreferences.Instance.SortRuntimeAscending;
          break;
        case SortingFields.MostPlayedArtists:
          ascending = DBSortPreferences.Instance.SortMostPlayedArtistsAscending;
          break;
        case SortingFields.MostPlayedVideos:
          ascending = DBSortPreferences.Instance.SortMostPlayedVideosAscending;
          break;
        case SortingFields.LeastPlayedArtists:
          ascending = DBSortPreferences.Instance.SortLeastPlayedArtistsAscending;
          break;
        case SortingFields.LeastPlayedVideos:
          ascending = DBSortPreferences.Instance.SortLeastPlayedVideosAscending;
          break;
        case SortingFields.AlbumReleaseDate:
          ascending = DBSortPreferences.Instance.SortAlbumReleaseDateAscending;
          break;
        case SortingFields.Composer:
          ascending = DBSortPreferences.Instance.SortComposerAscending;
          break;
        case SortingFields.Name:
          ascending = DBSortPreferences.Instance.SortNameAscending;
          break;
        case SortingFields.Filename:
          ascending = DBSortPreferences.Instance.SortFilenameAscending;
          break;
        default:
          ascending = true;
          break;
      }
      if (ascending) return SortingDirections.Ascending;
      else return SortingDirections.Descending;
    }
    /// <summary>
    /// Store the currently selected sort direction
    /// </summary>
    /// <param name="field"></param>
    /// <param name="sortDirection"></param>
    public static void StoreLastSortDirection(SortingFields field, SortingDirections sortDirection)
    {

      bool isAscending = sortDirection == SortingDirections.Ascending;

      switch (field)
      {
        case SortingFields.VideoTitle:
          DBSortPreferences.Instance.SortVideoTitleAscending = isAscending;
          break;
        case SortingFields.Artist:
          DBSortPreferences.Instance.SortArtistAscending = isAscending;
          break;
        case SortingFields.Album:
          DBSortPreferences.Instance.SortAlbumAscending = isAscending;
          break;
        case SortingFields.DateAdded:
          DBSortPreferences.Instance.SortDateAddedAscending = isAscending;
          break;
        case SortingFields.Runtime:
          DBSortPreferences.Instance.SortRuntimeAscending = isAscending;
          break;
        case SortingFields.MostPlayedArtists:
          DBSortPreferences.Instance.SortMostPlayedArtistsAscending = isAscending;
          break;
        case SortingFields.MostPlayedVideos:
          DBSortPreferences.Instance.SortMostPlayedVideosAscending = isAscending;
          break;
        case SortingFields.LeastPlayedArtists:
          DBSortPreferences.Instance.SortLeastPlayedArtistsAscending = isAscending;
          break;
        case SortingFields.LeastPlayedVideos:
          DBSortPreferences.Instance.SortLeastPlayedVideosAscending = isAscending;
          break;
        case SortingFields.AlbumReleaseDate:
          DBSortPreferences.Instance.SortAlbumReleaseDateAscending = isAscending;
          break;
        case SortingFields.Composer:
          DBSortPreferences.Instance.SortComposerAscending = isAscending;
          break;
        case SortingFields.Name:
          DBSortPreferences.Instance.SortNameAscending = isAscending;
          break;
        case SortingFields.Filename:
          DBSortPreferences.Instance.SortFilenameAscending = isAscending;
          break;
        default:
          break;
      }
      if (DBSortPreferences.Instance.CommitNeeded)
        DBSortPreferences.Instance.Commit();
    }
  }
  /// <summary>
  /// Sort for the Artists
  /// </summary>
  public class GUIListItemArtistComparer : IComparer<GUIListItem>
  {
    private static Logger logger = LogManager.GetCurrentClassLogger();
    private SortingFields _sortField;
    private SortingDirections _sortDirection;

    /// <summary>
    /// Constructor for GUIListItemMovieComparer
    /// </summary>
    /// <param name="sortField">The database field to sort by</param>
    /// <param name="sortDirection">The direction to sort by</param>
    public GUIListItemArtistComparer(SortingFields sortField, SortingDirections sortDirection)
    {
      _sortField = sortField;
      _sortDirection = sortDirection;

      Sort.StoreLastSortDirection(sortField, sortDirection);

      logger.Info("Sort Field: {0} Sort Direction: {1}", sortField, sortDirection);
    }


    public int Compare(GUIListItem x, GUIListItem y)
    {
      try
      {
        DBArtistInfo artistX = (DBArtistInfo)x.MusicTag;
        DBArtistInfo artistY = (DBArtistInfo)y.MusicTag;

        int rtn;

        switch (_sortField)
        {
          case SortingFields.Name:
            rtn = artistX.Artist.CompareTo(artistY.Artist);
            break;

          case SortingFields.DateAdded:
            rtn = artistX.DateAdded.CompareTo(artistY.DateAdded);
            break;

          // default to the title field
          case SortingFields.Artist:
          default:
            rtn = artistX.SortBy.CompareTo(artistY.SortBy);
            break;
        }

        // if both items are identical, fallback to using the Title
        if (rtn == 0)
          rtn = artistX.Artist.CompareTo(artistY.Artist);

        // if both items are STILL identical, fallback to using the ID
        if (rtn == 0)
          rtn = artistX.ID.GetValueOrDefault(0).CompareTo(artistY.ID.GetValueOrDefault(0));

        if (_sortDirection == SortingDirections.Descending)
          rtn = -rtn;

        return rtn;
      }
      catch
      {
        return 0;
      }
    }
  }
  /// <summary>
  /// Sort for the Albums
  /// </summary>
  public class GUIListItemAlbumComparer : IComparer<GUIListItem>
  {
    private static Logger logger = LogManager.GetCurrentClassLogger();
    private SortingFields _sortField;
    private SortingDirections _sortDirection;

    /// <summary>
    /// Constructor for GUIListItemMovieComparer
    /// </summary>
    /// <param name="sortField">The database field to sort by</param>
    /// <param name="sortDirection">The direction to sort by</param>
    public GUIListItemAlbumComparer(SortingFields sortField, SortingDirections sortDirection)
    {
      _sortField = sortField;
      _sortDirection = sortDirection;

      Sort.StoreLastSortDirection(sortField, sortDirection);

      logger.Info("Sort Field: {0} Sort Direction: {1}", sortField, sortDirection);
    }


    public int Compare(GUIListItem x, GUIListItem y)
    {
      if (x.IsFolder && x.Label == "..")
      {
        return -1;
      }
      if (y.IsFolder && y.Label == "..")
      {
        return 1;
      }

      try
      {
        DBAlbumInfo albumX = (DBAlbumInfo)x.MusicTag;
        DBAlbumInfo albumY = (DBAlbumInfo)y.MusicTag;

        int rtn = 0;

        switch (_sortField)
        {
          case SortingFields.Name:
          case SortingFields.Album:
            rtn = albumX.Album.CompareTo(albumY.Album);
            break;

          case SortingFields.DateAdded:
            rtn = albumX.DateAdded.CompareTo(albumY.DateAdded);
            break;

          case SortingFields.Artist:
            List<DBTrackInfo> tracksInAlbum = DBTrackInfo.GetEntriesByAlbum(albumX);
            DBArtistInfo artistX = DBArtistInfo.Get(tracksInAlbum[0]);
            tracksInAlbum = DBTrackInfo.GetEntriesByAlbum(albumY);
            DBArtistInfo artistY = DBArtistInfo.Get(tracksInAlbum[0]);

            if (artistX != null && artistY != null)
              rtn = artistX.Artist.CompareTo(artistY.Artist);
            break;

          // default to the title field
          default:
            rtn = albumX.SortBy.CompareTo(albumY.SortBy);
            break;
        }

        // if both items are identical, fallback to using the Title
        if (rtn == 0)
          rtn = albumX.Album.CompareTo(albumY.Album);

        // if both items are STILL identical, fallback to using the ID
        if (rtn == 0)
          rtn = albumX.ID.GetValueOrDefault(0).CompareTo(albumY.ID.GetValueOrDefault(0));

        if (_sortDirection == SortingDirections.Descending)
          rtn = -rtn;

        return rtn;
      }
      catch
      {
        return 0;
      }
    }
  }
  /// <summary>
  /// Sort for the Tracks
  /// </summary>
  public class GUIListItemVideoComparer : IComparer<GUIListItem>
  {
    private static Logger logger = LogManager.GetCurrentClassLogger();
    private SortingFields _sortField;
    private SortingDirections _sortDirection;

    /// <summary>
    /// Constructor for GUIListItemMovieComparer
    /// </summary>
    /// <param name="sortField">The database field to sort by</param>
    /// <param name="sortDirection">The direction to sort by</param>
    public GUIListItemVideoComparer(SortingFields sortField, SortingDirections sortDirection)
    {
      _sortField = sortField;
      _sortDirection = sortDirection;

      Sort.StoreLastSortDirection(sortField, sortDirection);

      logger.Info("Sort Field: {0} Sort Direction: {1}", sortField, sortDirection);
    }


    public int Compare(GUIListItem x, GUIListItem y)
    {
      if (x.IsFolder && x.Label == "..")
      {
        return -1;
      }
      if (y.IsFolder && y.Label == "..")
      {
        return 1;
      }

      if (x.IsFolder && y.IsFolder)
      {
        return x.Label.CompareTo(y.Label);
      }

      if (x.IsFolder && !y.IsFolder)
      {
        return -1;
      }
      if (!x.IsFolder && y.IsFolder)
      {
        return 1;
      }

      try
      {
        DBTrackInfo trackX = (DBTrackInfo)x.MusicTag;
        DBTrackInfo trackY = (DBTrackInfo)y.MusicTag;

        int rtn = 0;

        switch (_sortField)
        {
          case SortingFields.Name:
            rtn = trackX.Track.CompareTo(trackY.Track);
            break;

          case SortingFields.DateAdded:
            rtn = trackX.DateAdded.CompareTo(trackY.DateAdded);
            break;

          case SortingFields.Filename:
            rtn = trackX.LocalMedia[0].File.FullName.CompareTo(trackY.LocalMedia[0].File.FullName);
            break;

          case SortingFields.Runtime:
            rtn = trackX.LocalMedia[0].Duration.CompareTo(trackY.LocalMedia[0].Duration);
            break;

          case SortingFields.Artist:
            rtn = x.Label2.CompareTo(y.Label2);
            break;

          case SortingFields.Album:
            DBAlbumInfo albumX = null;
            DBAlbumInfo albumY = null;

            if (trackX.AlbumInfo.Count > 0)
              albumX = DBAlbumInfo.Get(trackX);
            if (trackY.AlbumInfo.Count > 0)
              albumY = DBAlbumInfo.Get(trackY);
            if (albumX != null && albumY != null)
              rtn = albumX.Album.CompareTo(albumY.Album);
            break;

          // default to the title field
          default:
            rtn = trackX.SortBy.CompareTo(trackY.SortBy);
            break;
        }

        // if both items are identical, fallback to using the Title
        if (rtn == 0)
          rtn = trackX.Track.CompareTo(trackY.Track);

        // if both items are STILL identical, fallback to using the ID
        if (rtn == 0)
          rtn = trackX.ID.GetValueOrDefault(0).CompareTo(trackY.ID.GetValueOrDefault(0));

        if (_sortDirection == SortingDirections.Descending)
          rtn = -rtn;

        return rtn;
      }
      catch
      {
        return 0;
      }
    }
  }
}

