using Cornerstone.Database;
using Cornerstone.Extensions;

using MediaPortal.Database;
using MediaPortal.Music.Database;
using MediaPortal.Util;

using mvCentral.ConfigScreen.Popups;
using mvCentral.Database;
using mvCentral.SignatureBuilders;

using NLog;

using SQLite.NET;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace mvCentral.DataProviders
{
  public class MyMusicProvider : IMusicVideoProvider
  {
    private static Logger logger = LogManager.GetCurrentClassLogger();

    public event EventHandler ProgressChanged;

    private DBTrackInfo mvTrackObject;
    // private DBArtistInfo mvArtistObject;
    // private DBAlbumInfo mvAlbumObject;

    // we should be using the MusicVideo object but we have to assign it before locking which 
    // is not good if the thread gets interupted after the asssignment, but before it gets 
    // locked. So we use this dumby var.
    private String lockObj = "";

    public string Name
    {
      get
      {
        return "mediaportal.music";
      }
    }

    public string Version
    {
      get
      {
        return "Internal";
      }
    }

    public string Author
    {
      get { return "ajs"; }
    }

    public string Description
    {
      get { return "Returns info and artwork already available on the local Mediaportal Music folders/database."; }
    }

    public string Language
    {
      get { return new CultureInfo("en").DisplayName; }
    }

    public string LanguageCode
    {
      get { return "various"; }
    }

    public List<string> LanguageCodeList
    {
      get
      {
        List<string> supportLanguages = new List<string>();
        return supportLanguages;
      }
    }

    public bool ProvidesTrackDetails
    {
      get { return true; }
    }

    public bool ProvidesArtistDetails
    {
      get { return true; }
    }

    public bool ProvidesAlbumDetails
    {
      get { return true; }
    }

    public bool ProvidesAlbumArt
    {
      get { return true; }
    }

    public bool ProvidesArtistArt
    {
      get { return true; }
    }

    public bool ProvidesTrackArt
    {
      get { return false; }
    }

    public DBTrackInfo GetArtistDetail(DBTrackInfo mv)
    {
      return mv;
    }

    public DBTrackInfo GetAlbumDetail(DBTrackInfo mv)
    {
      var albumTitle = mv.AlbumInfo[0].Album;
      var albumMbid = mv.AlbumInfo[0].MdID;
      var artist = mv.ArtistInfo[0].Artist;
      var albumData = mv.AlbumInfo[0];

      setMusicVideoAlbum(ref albumData, artist, albumTitle, albumMbid);
      return mv;
    }

    /// <summary>
    /// Get Local Artist Artwork, check for custom folder or artwork in the same folder as the music video
    /// </summary>
    /// <param name="artistInfo"></param>
    /// <returns></returns>
    public bool GetArtistArt(DBArtistInfo artistInfo)
    {
      logger.Debug("In Method GetArtistArt(DBArtistInfo artistInfo)");

      if (artistInfo == null)
        return false;

      // if we already have a artist move on for now
      if (artistInfo.ArtFullPath.Trim().Length > 0 && File.Exists(artistInfo.ArtFullPath.Trim()))
        return false;

      // Look for Artwork in Mediaportal folder
      return getMPArtistArt(artistInfo);
    }

    /// <summary>
    /// Get Album Artwork already on disk
    /// </summary>
    /// <param name="albumInfo"></param>
    /// <returns></returns>
    public bool GetAlbumArt(DBAlbumInfo albumInfo)
    {
      logger.Debug("In Method GetAlbumArt(DBAlbumInfo mv)");

      if (albumInfo == null)
        return false;

      // if we already have a artist move on for now
      if (albumInfo.ArtFullPath.Trim().Length > 0 && File.Exists(albumInfo.ArtFullPath.Trim()))
        return false;

      return getMPAlbumArt(albumInfo);
    }

    /// <summary>
    /// Get Track/Video Artwork already on disk
    /// </summary>
    /// <param name="trackInfo"></param>
    /// <returns></returns>
    public bool GetTrackArt(DBTrackInfo trackInfo)
    {
      logger.Debug("In Method GetTrackArt(DBTrackInfo mv)");

      if (trackInfo == null)
        return false;

      if (this.mvTrackObject == null) this.mvTrackObject = trackInfo;

      // if we already have a trackimage move on for now
      if (trackInfo.ArtFullPath.Trim().Length > 0 && File.Exists(trackInfo.ArtFullPath.Trim()))
        return false;

      return false;
    }

    /// <summary>
    /// Get the Artist Artwork from Mediaportal folder
    /// </summary>
    /// <param name="mvArtistObject"></param>
    /// <returns></returns>
    private bool getMPArtistArt(DBArtistInfo mvArtistObject)
    {
      logger.Debug("In Method getMPArtistArt(DBArtistInfo mv)");
      bool found = false;

      string thumbFolder = Thumbs.MusicArtists;
      string cleanTitle = MediaPortal.Util.Utils.MakeFileName(mvArtistObject.Artist);
      string filename = thumbFolder + @"\" + cleanTitle + "L.jpg";

      logger.Debug("In Method getMPArtistArt(DBArtistInfo mv) filename: " + filename);
      if (File.Exists(filename)) 
      {
        found &= mvArtistObject.AddArtFromFile(filename);
      }
      return found;
    }

    /// <summary>
    /// Get the Album Artwork from Mediaportal folder
    /// </summary>
    /// <param name="mvArtistObject"></param>
    /// <returns></returns>
    private bool getMPAlbumArt(DBAlbumInfo mvAlbumObject)
    {
      logger.Debug("In Method getMPAlbumArt(DBAlbumInfo mv)");
      bool found = false;

      string artist = string.Empty;
      string album = mvAlbumObject.Album;

      List<DBTrackInfo> a1 = DBTrackInfo.GetEntriesByAlbum(mvAlbumObject);
      if (a1.Count > 0)
      {
        artist = a1[0].ArtistInfo[0].Artist;
      }

      string thumbFolder = Thumbs.MusicAlbum ;
      string cleanTitle = string.Format("{0}-{1}", MediaPortal.Util.Utils.MakeFileName(artist), MediaPortal.Util.Utils.MakeFileName(album));
      string filename = thumbFolder + @"\" + cleanTitle + "L.jpg";

      logger.Debug("In Method getMPAlbumArt(DBAlbumInfo mv) filename: " + filename);
      if (File.Exists(filename)) 
      {
        found &= mvAlbumObject.AddArtFromFile(filename);
      }
      return found;
    }

    public bool GetDetails(DBBasicInfo mv)
    {
      logger.Debug("In Method: GetDetails(DBBasicInfo mv)");
      MusicDatabase m_db = null;
      string inLang = mvCentralCore.Settings.DataProviderAutoLanguage;

      ReportProgress(string.Empty);
      try
      {
        m_db = MusicDatabase.Instance;
      }
      catch (Exception e)
      {
        logger.Error("GetDetails: Music database init failed " + e.ToString());
        return false;
      }

      // ---------------- Get Artist Info ---------------------
      if (mv.GetType() == typeof (DBArtistInfo))
      {
        var artist = ((DBArtistInfo) mv).Artist;
        var releases = new List<Release>();
        var artists = new ArrayList();

        // Grab the Artist Info
        if (artist != null)
        {
          ReportProgress("Getting Artists...");
          artists.Clear();

          string strArtist = NormalizeArtist(artist);
          string strSQL = String.Format("SELECT a.strArtist FROM artist a, artistinfo i WHERE LOWER(a.strArtist) = LOWER(i.strArtist) AND i.strAMGBio IS NOT NULL AND TRIM(i.strAMGBio) <> '' AND LOWER(i.strArtist) LIKE '%{0}%';", strArtist);

          List<Song> songInfo = new List<Song>();
          m_db.GetSongsByFilter(strSQL, out songInfo, "artist");
          foreach (Song mySong in songInfo)
          {
            if (!string.IsNullOrEmpty(mySong.Artist))
            {
              artists.Add(mySong.Artist);
            }
          }
        }
        else
          return false;

        if (artists == null || artists.Count <= 0) 
          return false;

        foreach (string _artist in artists)
        {
          Release r2 = new Release(_artist, string.Empty);
          releases.Add(r2);
        }
        ReportProgress("Done!");

        // Now sort and Display the retrived matches
        releases.Sort(Release.TitleComparison);
        var resultsDialog = new DetailsPopup(releases);
        // Get the full info for the selection
        if (resultsDialog.ShowDialog() == DialogResult.OK)
        {
          var mv1 = (DBArtistInfo)mv;
          mv.ArtUrls.Clear();

          string title = resultsDialog.selectedItem.Text;
          string mbid = resultsDialog.label8.Text;
          if (title.Trim().Length == 0) title = null;
          if (mbid.Trim().Length == 0) mbid = null;

          setMusicVideoArtist(ref mv1, title, mbid);
          GetArtistArt((DBArtistInfo) mv);
        }
      }

      // -------------- Get Album Info --------------
      if (mv.GetType() == typeof(DBAlbumInfo))
      {
        List<DBTrackInfo> a1 = DBTrackInfo.GetEntriesByAlbum((DBAlbumInfo)mv);
        if (a1.Count > 0)
        {
          string artist = a1[0].ArtistInfo[0].Artist;
          var albums = new ArrayList(); ;

          if (artist != null)
          {
            ReportProgress("Getting Albums...");
            logger.Debug("GetDetails: Getting Albums: " + artist);

            albums.Clear();

            string strArtist = NormalizeArtist(artist);
            string strSQL = String.Format("select strAlbum, strReview FROM albuminfo WHERE strReview IS NOT NULL AND TRIM(strReview) <> '' AND (strArtist LIKE '%{0}%' OR strAlbumArtist LIKE '%{1}%');", strArtist, strArtist);

            List<Song> songInfo = new List<Song>();
            m_db.GetSongsByFilter(strSQL, out songInfo, "album");
            logger.Debug("GetDetails: Getting Albums: " + artist + " - " + songInfo.Count);
            foreach (Song mySong in songInfo)
            {
              if (!string.IsNullOrEmpty(mySong.Album))
              {
                albums.Add(mySong.Album);
              }
            }
          }
          else
            return false;

          if (albums == null || albums.Count <= 0)
            return false;

          List<Release> artistTopAlbumns = new List<Release>();
          foreach (string _album in albums)
          {
            logger.Debug("GetDetails: Getting Albums: " + artist + " - " + _album);
            Release r2 = new Release(_album, string.Empty);
            artistTopAlbumns.Add(r2);
          }

          ReportProgress("Done!");
          artistTopAlbumns.Sort(Release.TitleComparison);
          DetailsPopup d1 = new DetailsPopup(artistTopAlbumns);

          if (d1.ShowDialog() == DialogResult.OK)
          {
            DBAlbumInfo mv1 = (DBAlbumInfo)mv;
            mv.ArtUrls.Clear();
            string title = d1.selectedItem.Text;
            string mbid = d1.label8.Text;
            if (title.Trim().Length == 0) title = null;
            if (mbid.Trim().Length == 0) mbid = null;

            setMusicVideoAlbum(ref mv1, artist, title, mbid);
            GetAlbumArt((DBAlbumInfo)mv);
          }
        }
      }

      // -------------- Get Track Info --------------
      if (mv.GetType() == typeof(DBTrackInfo))
      {
        string artist = ((DBTrackInfo)mv).ArtistInfo[0].Artist;
        var tracks = new ArrayList();;

        if (artist != null)
        {
          ReportProgress("Getting Tracks...");
          logger.Debug("GetDetails: Getting Tracks: " + artist);

          tracks.Clear();

          string strArtist = NormalizeArtist(artist);
          string strSQL = String.Format("select strTitle FROM tracks WHERE strArtist LIKE '%| {0} |%' OR strAlbumArtist LIKE '%| {1} |%';');", strArtist, strArtist);

          List<Song> songInfo = new List<Song>();
          m_db.GetSongsByFilter(strSQL, out songInfo, "tracks");
          logger.Debug("GetDetails: Getting Tracks: " + artist + " - " + songInfo.Count);
          foreach (Song mySong in songInfo)
          {
            if (!string.IsNullOrEmpty(mySong.Title))
            {
              tracks.Add(mySong.Title);
            }
          }
        }
        else
            return false;

        if (tracks == null || tracks.Count <= 0)
          return false;

        List<Release> artistTopTracks = new List<Release>();
        foreach (string _track in tracks)
        {
          logger.Debug("GetDetails: Getting Track: " + artist + " - " + _track);
          Release r2 = new Release(_track, string.Empty);
          artistTopTracks.Add(r2);
        }

        ReportProgress("Done!");
        artistTopTracks.Sort(Release.TitleComparison);
        DetailsPopup d1 = new DetailsPopup(artistTopTracks);

        if (d1.ShowDialog() == DialogResult.OK)
        {
          DBTrackInfo mv1 = (DBTrackInfo)mv;
          mv.ArtUrls.Clear();
          if (artist.Trim().Length == 0) artist = null;
          string title = d1.selectedItem.Text;
          string mbid = d1.label8.Text;
          if (title.Trim().Length == 0) title = null;
          if (mbid.Trim().Length == 0) mbid = null;

          setMusicVideoTrack(ref mv1, artist, title, mbid);
          GetTrackArt((DBTrackInfo)mv);
        }
      }
      return true;
    }

    private void setMusicVideoArtist(ref DBArtistInfo mv, string artistName, string artistmbid)
    {
      if (string.IsNullOrEmpty(artistName))
        return;

      logger.Debug("In Method: setMusicVideoArtist(ref DBArtistInfo mv, string artistName, string artistmbid)");
      logger.Debug("In Method: setMusicVideoArtist(Artist: "+artistName+" MBID: "+artistmbid+")");

      MusicDatabase m_db = null;
      try
      {
        m_db = MusicDatabase.Instance;
      }
      catch (Exception e)
      {
        logger.Error("GetDetails: Music database init failed " + e.ToString());
        return;
      }

      var artistInfo = new MediaPortal.Music.Database.ArtistInfo();
      if (!m_db.GetArtistInfo(artistName, ref artistInfo))
        return;

      // Name
      mv.Artist = artistName;
      // MBID
      // mv.MdID = 
      // Tags
      char[] delimiters = new char[] { ',' };
      string[] tags = artistInfo.Genres.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
      foreach (string tag in tags)
      {
        mv.Tag.Add(tag.Trim());
      }
      tags = artistInfo.Styles.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
      foreach (string tag in tags)
      {
        mv.Tag.Add(tag.Trim());
      }
      // Bio
      mv.bioSummary = artistInfo.AMGBio;
      mv.bioContent = artistInfo.AMGBio;
      // Additional
      mv.Born = artistInfo.Born;
      mv.Genre = artistInfo.Genres;
      mv.Styles = artistInfo.Styles;
      mv.YearsActive = artistInfo.YearsActive;
      // Image URL
      if (!string.IsNullOrEmpty(artistInfo.Image) && !mv.ArtUrls.Contains(artistInfo.Image))
        mv.ArtUrls.Add(artistInfo.Image);
    }
    
    /// <summary>
    /// Grab the album data and process
    /// </summary>
    /// <param name="mv"></param>
    /// <param name="artist"></param>
    /// <param name="album"></param>
    /// <param name="mbid"></param>
    private void setMusicVideoAlbum(ref DBAlbumInfo mv, string artistName, string albumName, string mbid)
    {
      if (string.IsNullOrEmpty(artistName) && string.IsNullOrEmpty(albumName))
        return;

      logger.Debug(string.Format("In Method: setMusicVideoAlbum : " + (!string.IsNullOrEmpty(artistName) ? "Atrist ({0}) | " : "") +
                                                                      (!string.IsNullOrEmpty(albumName) ? "Album ({1}) | " : "") +
                                                                      (!string.IsNullOrEmpty(mbid) ? "MBID ({2})" : ""), 
                                                                      artistName, albumName, mbid));
      MusicDatabase m_db = null;
      try
      {
        m_db = MusicDatabase.Instance;
      }
      catch (Exception e)
      {
        logger.Error("GetDetails: Music database init failed " + e.ToString());
        return;
      }

      var albumInfo = new MediaPortal.Music.Database.AlbumInfo();
      if (!m_db.GetAlbumInfo(albumName, artistName, ref albumInfo))
        return;

      // Album name
      mv.Album = albumInfo.Album;
      // MBID
      // mv.MdID
      // Image URL
      if (!string.IsNullOrEmpty(albumInfo.Image) && !mv.ArtUrls.Contains(albumInfo.Image))
        mv.ArtUrls.Add(albumInfo.Image);
      // Tags: Actors, Directors and Writers
      // mv.Tag.Add(tagstr);
      // WIKI
      mv.bioSummary = albumInfo.Review;
      mv.bioContent = albumInfo.Review;
      // Tag
      char[] delimiters = new char[] { ',' };
      string[] tags = albumInfo.Styles.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
      foreach (string tag in tags)
      {
        mv.Tag.Add(tag.Trim());
      }
      // Additional
      if (albumInfo.Year > 0)
      {
        mv.YearReleased = Convert.ToString(albumInfo.Year);
      }
    }

    /// <summary>
    /// Grab the track data and process
    /// </summary>
    /// <param name="mv"></param>
    /// <param name="artist"></param>
    /// <param name="track"></param>
    /// <param name="mbid"></param>
    private void setMusicVideoTrack(ref DBTrackInfo mv, string artist, string track, string mbid)
    {
      if (track == null && mbid == null)
        return;

      logger.Debug("In Method: setMusicVideoTrack(Artist: " + artist + " Track: " + track + " MBID: " + mbid + ")");

      MusicDatabase m_db = null;
      try
      {
        m_db = MusicDatabase.Instance;
      }
      catch (Exception e)
      {
        logger.Error("GetDetails: Music database init failed " + e.ToString());
        return;
      }

      var trackInfo = new MediaPortal.Music.Database.Song();
      if (!m_db.GetSongByMusicTagInfo(artist, string.Empty, track, false, ref trackInfo))
        return;

      mv.Track = trackInfo.Title;
      //mv.MdID = value;
      // Tag
      char[] delimiters = new char[] { ',' };
      string[] tags = trackInfo.Genre.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
      foreach (string tag in tags)
      {
        mv.Tag.Add(tag.Trim());
      }
      // mv.ArtUrls.Add(trackInfo.Image);
      // mv.bioSummary = mvCentralUtils.StripHTML(cdataSection.Value);
      // mv.bioContent = mvCentralUtils.StripHTML(cdataSection.Value);
      return;
    }

    /// <summary>
    /// Get the track info
    /// </summary>
    /// <param name="track"></param>
    /// <returns></returns>
    private DBTrackInfo getMusicVideoTrack(string track)
    {
      return getMusicVideoTrack(null, track);
    }

    private DBTrackInfo getMusicVideoTrack(string artist, string track)
    {
      if (track == null)
        return null;

      logger.Debug("In Method: getMusicVideoTrack(Artist: " + artist + " Track: " + track + ")");

      MusicDatabase m_db = null;
      try
      {
        m_db = MusicDatabase.Instance;
      }
      catch (Exception e)
      {
        logger.Error("GetDetails: Music database init failed " + e.ToString());
        return null;
      }

      var trackInfo = new MediaPortal.Music.Database.Song();
      if (!m_db.GetSongByMusicTagInfo(artist, string.Empty, track, false, ref trackInfo))
        return null;

      DBTrackInfo mv = new DBTrackInfo();
      mv.Track = trackInfo.Title;
      // mv.MdID = value;
      // Artist
      var _artist = string.Empty ;
      if (!string.IsNullOrEmpty(trackInfo.AlbumArtist))
      {
        _artist = trackInfo.AlbumArtist;
      } else if (!string.IsNullOrEmpty(trackInfo.Artist))
      {
        _artist = trackInfo.Artist;
      }
      if (!string.IsNullOrEmpty(_artist))
      {
        DBArtistInfo d4 = new DBArtistInfo();
        setMusicVideoArtist(ref d4, _artist, null);
        mv.ArtistInfo.Add(d4);
      }
      // Album
      if (!string.IsNullOrEmpty(trackInfo.Album))
      {
        DBAlbumInfo d4 = new DBAlbumInfo();
        setMusicVideoAlbum(ref d4, _artist, trackInfo.Album, null);
        // Have we actually got a valid album?
        if (d4.Album.Trim() != string.Empty)
          mv.AlbumInfo.Add(d4);
      }
      // Tag
      char[] delimiters = new char[] { ',' };
      string[] tags = trackInfo.Genre.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
      foreach (string tag in tags)
      {
        mv.Tag.Add(tag.Trim());
      }
      // mv.bioSummary = mvCentralUtils.StripHTML(cdataSection.Value);
      // mv.bioContent = mvCentralUtils.StripHTML(cdataSection.Value);

      if (mv.ArtistInfo.Count == 0)
        return null;

      return mv;
    }

    public bool GetAlbumDetails(DBBasicInfo basicInfo, string albumTitle, string albumMbid)
    {
      logger.Debug("In Method: GetAlbumDetails: Album: " + albumTitle + " MBID: " + albumMbid);
      List<DBTrackInfo> tracksOnAlbum = DBTrackInfo.GetEntriesByAlbum((DBAlbumInfo)basicInfo);
      if (tracksOnAlbum.Count > 0)
      {
        string artist = tracksOnAlbum[0].ArtistInfo[0].Artist;
        DBAlbumInfo mv1 = (DBAlbumInfo)basicInfo;
        basicInfo.ArtUrls.Clear();
        setMusicVideoAlbum(ref mv1, artist, albumTitle, albumMbid);
        GetAlbumArt((DBAlbumInfo)basicInfo);
      }
      return true;
    }

    public bool GetDetails(DBTrackInfo mv)
    {
      throw new NotImplementedException();
    }

    public List<DBTrackInfo> GetTrackDetail(MusicVideoSignature mvSignature)
    {
      logger.Debug("In Method: GetTrackDetail(MusicVideoSignature mv)");
      // Switch off album support
      if (mvCentralCore.Settings.DisableAlbumSupport)
        mvSignature.Album = null;

      List<DBTrackInfo> results = new List<DBTrackInfo>();
      if (mvSignature == null)
        return results;

      lock (lockObj)
      {
        DBTrackInfo mvTrackData = null;
        // Artist/Album handling, if the track and artist dont match and the track contains the artist name this would indicate the that track is in the format /<artist>/<album>/<atrist - track>.<ext>
        // This will throw out the parseing so remove the artist name from the track.
        // This is not the best fix, need to add code so I know whch expression produced the result or better still have a ignore folder structure when pasring option.
        if (mvSignature.Track != null && mvSignature.Artist != null)
        {
          if ((mvSignature.Track.ToLower().Trim() != mvSignature.Artist.ToLower().Trim()) && mvSignature.Track.ToLower().Contains(mvSignature.Artist.ToLower().Trim()))
            mvTrackData = getMusicVideoTrack(mvSignature.Artist, Regex.Replace(mvSignature.Track, mvSignature.Artist, string.Empty, RegexOptions.IgnoreCase));
          else
            mvTrackData = getMusicVideoTrack(mvSignature.Artist, mvSignature.Track);
        }
        else
          mvTrackData = getMusicVideoTrack(mvSignature.Artist, mvSignature.Track);

        if (mvTrackData != null)
        {
          if (mvTrackData.ArtistInfo.Count == 0)
          {
            DBArtistInfo artistInfo = new DBArtistInfo();
            artistInfo.Artist = mvSignature.Artist;
            mvTrackData.ArtistInfo.Add(artistInfo);
          }

          if (mvSignature.Album != null && mvSignature.Artist != null)
          {
            if (!mvCentralCore.Settings.UseMDAlbum)
            {
              DBAlbumInfo albumInfo = new DBAlbumInfo();
              albumInfo.Album = mvSignature.Album;
              setMusicVideoAlbum(ref albumInfo, mvSignature.Artist, mvSignature.Album, null);
              mvTrackData.AlbumInfo.Clear();
              mvTrackData.AlbumInfo.Add(albumInfo);
            }
          }
          else if (mvTrackData.AlbumInfo.Count > 0 && mvCentralCore.Settings.SetAlbumFromTrackData)
          {
            logger.Debug("There are {0} Albums found for Artist: {1} / {2}", mvTrackData.AlbumInfo.Count.ToString(), mvSignature.Artist, mvSignature.Title);
            DBAlbumInfo albumInfo = new DBAlbumInfo();
            albumInfo.Album = mvTrackData.AlbumInfo[0].Album;
            setMusicVideoAlbum(ref albumInfo, mvSignature.Artist, mvTrackData.AlbumInfo[0].Album, null);
            mvTrackData.AlbumInfo.Clear();
            mvTrackData.ArtistInfo[0].Artist = mvSignature.Artist;
            mvTrackData.AlbumInfo.Add(albumInfo);
          }
          else
            mvTrackData.AlbumInfo.Clear();

          results.Add(mvTrackData);
        }
      }
      return results;
    }

    public UpdateResults UpdateTrack(DBTrackInfo mv)
    {
      // Have we anything to update?
      if (mv == null)
        return UpdateResults.FAILED;

      logger.Debug("In Method: UpdateTrack(DBTrackInfo mv)");

      lock (lockObj)
      {
        // Update the Artist
        var artistData = DBArtistInfo.Get(mv);
        if (artistData != null)
          mv.ArtistInfo[0] = artistData;

        if (mv.ArtistInfo.Count > 0)
        {
          mv.ArtistInfo[0].PrimarySource = mv.PrimarySource;
          mv.ArtistInfo[0].Commit();
        }
        // Update the Album
        var albumData = DBAlbumInfo.Get(mv);
        if (albumData != null)
          mv.AlbumInfo[0] = albumData;

        if (mv.AlbumInfo.Count > 0)
        {
          mv.AlbumInfo[0].PrimarySource = mv.PrimarySource;
          mv.AlbumInfo[0].Commit();
        }
      }
      return UpdateResults.SUCCESS;
    }

    private string NormalizeArtist(string strArtist)
    {
      DatabaseUtility.RemoveInvalidChars(ref strArtist);
      strArtist = strArtist.Replace('ä', '%');
      strArtist = strArtist.Replace('ö', '%');
      strArtist = strArtist.Replace('ü', '%');
      strArtist = strArtist.Replace('/', '%');
      strArtist = strArtist.Replace('-', '%');
      strArtist = strArtist.Replace("%%", "%");

      return strArtist;
    }

    private void ReportProgress(string text)
    {
      if (ProgressChanged != null)
      {
        ProgressChanged(this, new ProgressEventArgs { Text = "Mediaportal Music DB: " + text });
      }
    }
  }

}
