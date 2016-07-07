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
    private DBArtistInfo mvArtistObject;
    private DBAlbumInfo mvAlbumObject;

    // we should be using the MusicVideo object but we have to assign it before locking which 
    // is not good if the thread gets interupted after the asssignment, but before it gets 
    // locked. So we use this dumby var.
    private String lockObj = "";

    public string Name
    {
      get
      {
        return "mediaportal.mymusic";
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
      get { return false; }
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
      get { return true; }
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

      bool found = false;
      // If a custom artfolder is specified the search
      if (mvCentralCore.Settings.SearchCustomFolderForArtistArt)
      {
        found = getArtistArtFromCustomArtistArtFolder(artistInfo);
        logger.Debug("Sucessfully added fanart from custom folder: {0}", artistInfo.ArtFullPath);
      }
      // Look for Artwork in same folder as video
      if (!found)
        found = getArtistArtFromArtistArtFolder(artistInfo);
      // Look for Artwork in Mediaportal folder
      if (!found)
        found = getMPArtistArt(artistInfo);
      // Look for artwork in the ..\thumbs\mvCentral\Artists\FullSize folder
      if (!found)
        found = getOldArtistArt(artistInfo);

      return found;
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

      bool found = false;

      if (mvCentralCore.Settings.SearchCustomFolderForAlbumArt)
      {
        found = getAlbumArtFromCustomAlbumArtFolder(albumInfo);
        logger.Debug("Sucessfully added fanart from custom folder: {0}", albumInfo.ArtFullPath);
      }

      if (!found)
        found = getAlbumArtFromAlbumArtFolder(albumInfo);

      // Look for Artwork in Mediaportal folder
      if (!found)
        found = getMPAlbumArt(albumInfo);

      if (!found)
        found = getOldAlbumArt(albumInfo);

      return found;
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

      bool found = false;

      if (mvCentralCore.Settings.SearchCustomFolderForTrackArt)
      {
        found = getTrackArtFromCustomTrackArtFolder(trackInfo);
        logger.Debug("Sucessfully added fanart from custom folder: {0}", trackInfo.ArtFullPath);
      }

      if (!found)
        found = getTrackArtFromTrackArtFolder(trackInfo);

      if (!found)
        found = getOldTrackArt(trackInfo);

      return found;
    }

    /// <summary>
    /// Get Artist Artwork from the Custom Artist Artwork folder.
    /// </summary>
    /// <param name="mvArtistObject"></param>
    /// <returns></returns>
    private bool getArtistArtFromCustomArtistArtFolder(DBArtistInfo mvArtistObject)
    {
      string artistArtFolderPath = string.Empty;

      logger.Debug("In Method getArtistArtFromCustomArtistArtFolder(DBArtistInfo mv)");
      if (mvArtistObject == null)
        return false;

      // grab a list of possible filenames for the artist based on the user pattern
      string pattern = mvCentralCore.Settings.ArtistArtworkFilenamePattern;
      List<string> filenames = getPossibleNamesFromPattern(pattern, mvArtistObject);

      // check the ArtistArt folder for the user patterned ArtistArt
      artistArtFolderPath = mvCentralCore.Settings.CustomArtistArtFolder;
      FileInfo newArtistArt = getFirstFileFromFolder(artistArtFolderPath, filenames);
      if (newArtistArt != null && newArtistArt.Exists)
      {
        mvArtistObject.ArtFullPath = newArtistArt.FullName;
        mvArtistObject.AlternateArts.Add(newArtistArt.FullName);
        mvArtistObject.GenerateThumbnail();
        logger.Info("Loaded artistart from " + newArtistArt.FullName);
        return true;
      }
      return false;
    }

    /// <summary>
    /// Get Artist Artwork from the Artist Artwork folder.
    /// </summary>
    /// <param name="mvArtistObject"></param>
    /// <returns></returns>
    private bool getArtistArtFromArtistArtFolder(DBArtistInfo mvArtistObject)
    {
      string artistArtFolderPath = string.Empty;

      logger.Debug("In Method getArtistArtFromArtistArtFolder(DBArtistInfo mv)");
      if (mvArtistObject == null)
        return false;

      // grab a list of possible filenames for the artist based on the user pattern
      string pattern = mvCentralCore.Settings.ArtistArtworkFilenamePattern;
      List<string> filenames = getPossibleNamesFromPattern(pattern, mvArtistObject);

      // check the ArtistArt folder for the user patterned ArtistArt
      artistArtFolderPath = mvCentralCore.Settings.ArtistArtFolder;
      FileInfo newArtistArt = getFirstFileFromFolder(artistArtFolderPath, filenames);
      if (newArtistArt != null && newArtistArt.Exists)
      {
        mvArtistObject.ArtFullPath = newArtistArt.FullName;
        logger.Info("Loaded artistart from " + newArtistArt.FullName);
        return true;
      }
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
      if (System.IO.File.Exists(filename)) 
      {
        found &= mvArtistObject.AddArtFromFile(filename);
      }
      return found;
    }

    /// <summary>
    /// Get the Artist Artwork using the old Method
    /// </summary>
    /// <param name="mvArtistObject"></param>
    /// <returns></returns>
    private bool getOldArtistArt(DBArtistInfo mvArtistObject)
    {
      logger.Debug("In Method getOldArtistArt(DBArtistInfo mv)");
      bool found = false;

      string artistartFolderPath = mvCentralCore.Settings.ArtistArtFolder;
      DirectoryInfo artistartFolder = new DirectoryInfo(artistartFolderPath);

      string safeName = mvArtistObject.Artist.Replace(' ', '.').ToValidFilename();
      Regex oldArtistArtRegex = new Regex("^{?" + Regex.Escape(safeName) + "}? \\[-?\\d+\\]\\.(jpg|png)");

      foreach (FileInfo currFile in artistartFolder.GetFiles())
      {
        if (oldArtistArtRegex.IsMatch(currFile.Name))
        {
          found &= mvArtistObject.AddArtFromFile(currFile.FullName);
        }
      }
      return found;
    }

    /// <summary>
    /// get the Album Artwork from the custom album artwork folder 
    /// </summary>
    /// <param name="mvAlbumObject"></param>
    /// <returns></returns>
    private bool getAlbumArtFromCustomAlbumArtFolder(DBAlbumInfo mvAlbumObject)
    {
      logger.Debug("In Method getAlbumArtFromCustomAlbumArtFolder(DBAlbumInfo mv)");
      if (mvAlbumObject == null)
        return false;

      // grab a list of possible filenames for the albumart based on the user pattern
      string pattern = mvCentralCore.Settings.AlbumArtworkFilenamePattern;
      List<string> filenames = getPossibleNamesFromPattern(pattern, mvAlbumObject);

      // check the albumart folder for the user patterned albumart
      string albumArtFolderPath = mvCentralCore.Settings.CustomAlbumArtFolder;
      FileInfo newAlbumArt = getFirstFileFromFolder(albumArtFolderPath, filenames);
      if (newAlbumArt != null && newAlbumArt.Exists)
      {
        mvAlbumObject.ArtFullPath = newAlbumArt.FullName;
        mvAlbumObject.AlternateArts.Add(newAlbumArt.FullName);
        mvAlbumObject.GenerateThumbnail();
        logger.Info("Loaded Albumimage from " + newAlbumArt.FullName);
        return true;
      }
      return false;
    }

    /// <summary>
    /// get the Album Artwork from Album Artwork folder 
    /// </summary>
    /// <param name="mvAlbumObject"></param>
    /// <returns></returns>
    private bool getAlbumArtFromAlbumArtFolder(DBAlbumInfo mvAlbumObject)
    {
      logger.Debug("In Method getAlbumArtFromAlbumArtFolder(DBAlbumInfo mv)");
      if (mvAlbumObject == null)
        return false;

      // grab a list of possible filenames for the albumart based on the user pattern
      string pattern = mvCentralCore.Settings.AlbumArtworkFilenamePattern;
      List<string> filenames = getPossibleNamesFromPattern(pattern, mvAlbumObject);

      // check the albumart folder for the user patterned albumart
      string albumArtFolderPath = mvCentralCore.Settings.AlbumArtFolder;
      FileInfo newAlbumArt = getFirstFileFromFolder(albumArtFolderPath, filenames);
      if (newAlbumArt != null && newAlbumArt.Exists)
      {
        mvAlbumObject.ArtFullPath = newAlbumArt.FullName;
        logger.Info("Loaded Albumimage from " + newAlbumArt.FullName);
        return true;
      }
      return false;
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
      if (System.IO.File.Exists(filename)) 
      {
        found &= mvAlbumObject.AddArtFromFile(filename);
      }
      return found;
    }

    /// <summary>
    /// Get the Album Artwork using the old method
    /// </summary>
    /// <param name="mvAlbumObject"></param>
    /// <returns></returns>
    private bool getOldAlbumArt(DBAlbumInfo mvAlbumObject)
    {
      logger.Debug("In Method getOldAlbumArt(DBAlbumInfo mv)");
      bool found = false;

      string AlbumArtFolderPath = mvCentralCore.Settings.AlbumArtFolder;
      DirectoryInfo albumartFolder = new DirectoryInfo(AlbumArtFolderPath);

      string safeName = mvAlbumObject.Album.Replace(' ', '.').ToValidFilename();
      Regex oldtrackRegex = new Regex("^{?" + Regex.Escape(safeName) + "}? \\[-?\\d+\\]\\.(jpg|png)");

      foreach (FileInfo currFile in albumartFolder.GetFiles())
      {
        if (oldtrackRegex.IsMatch(currFile.Name))
        {
          found &= mvAlbumObject.AddArtFromFile(currFile.FullName);
        }
      }
      return found;
    }

    /// <summary>
    /// Get Track Artwork from Custom Track Artwork folder
    /// </summary>
    /// <param name="mvTrackObject"></param>
    /// <returns></returns>
    private bool getTrackArtFromCustomTrackArtFolder(DBTrackInfo mvTrackObject)
    {
      bool found = false;
      logger.Debug("In Method getTrackArtFromCustomTrackArtFolder(DBTrackInfo mv)");

      if (mvTrackObject == null)
        return false;

      // grab a list of possible filenames for the artistart based on the user pattern
      string pattern = mvCentralCore.Settings.TrackArtworkFilenamePattern;
      List<string> filenames = getPossibleNamesFromPattern(pattern, mvTrackObject);

      // check the artistart folder for the user patterned artistarts
      string trackartFolderPath = mvCentralCore.Settings.CustomTrackArtFolder;
      FileInfo newTrackArt = getFirstFileFromFolder(trackartFolderPath, filenames);
      if (newTrackArt != null && newTrackArt.Exists)
      {
        mvTrackObject.ArtFullPath = newTrackArt.FullName;
        mvTrackObject.AlternateArts.Add(newTrackArt.FullName);
        mvTrackObject.GenerateThumbnail();
        logger.Info("Loaded trackimage from " + newTrackArt.FullName);
        return true;
      }
      return found;
    }

    /// <summary>
    /// Get Track Artwork from Track Artwork folder
    /// </summary>
    /// <param name="mvTrackObject"></param>
    /// <returns></returns>
    private bool getTrackArtFromTrackArtFolder(DBTrackInfo mvTrackObject)
    {
      bool found = false;
      logger.Debug("In Method getTrackArtFromTrackArtFolder(DBTrackInfo mv)");

      if (mvTrackObject == null)
        return false;

      // grab a list of possible filenames for the artistart based on the user pattern
      string pattern = mvCentralCore.Settings.TrackArtworkFilenamePattern;
      List<string> filenames = getPossibleNamesFromPattern(pattern, mvTrackObject);

      // check the artistart folder for the user patterned artistarts
      string trackartFolderPath = mvCentralCore.Settings.TrackArtFolder;
      FileInfo newTrackArt = getFirstFileFromFolder(trackartFolderPath, filenames);
      if (newTrackArt != null && newTrackArt.Exists)
      {
        mvTrackObject.ArtFullPath = newTrackArt.FullName;
        logger.Info("Loaded trackimage from " + newTrackArt.FullName);
        return true;
      }
      return found;
    }

    /// <summary>
    /// Get the Track Artwork using the old Method
    /// </summary>
    /// <param name="mv"></param>
    /// <returns></returns>
    private bool getOldTrackArt(DBTrackInfo mv)
    {
      logger.Debug("In Method getOldTrackArt(DBTrackInfo mv)");
      bool found = false;

      string trackartFolderPath = mvCentralCore.Settings.TrackArtFolder;
      DirectoryInfo trackartFolder = new DirectoryInfo(trackartFolderPath);

      string safeName = mv.Track.Replace(' ', '.').ToValidFilename();
      Regex oldtrackRegex = new Regex("^{?" + Regex.Escape(safeName) + "}? \\[-?\\d+\\]\\.(jpg|png)");

      foreach (FileInfo currFile in trackartFolder.GetFiles())
      {
        if (oldtrackRegex.IsMatch(currFile.Name))
        {
          found &= mv.AddArtFromFile(currFile.FullName);
        }
      }

      return found;
    }

    /// <summary>
    /// parses and replaces variables from a filename based on the pattern supplied
    /// returning a list of possible file matches
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="mv"></param>
    /// <returns></returns>
    private List<string> getPossibleNamesFromPattern(string pattern, object mv)
    {
      Regex parser = new Regex("%(.*?)%", RegexOptions.IgnoreCase);
      // Artist Artwork
      if (mv.GetType() == typeof(DBArtistInfo))
      {
        mvArtistObject = (DBArtistInfo)mv;
        lock (lockObj)
        {
          List<string> filenames = new List<string>();
          foreach (string currPattern in pattern.Split('|'))
          {
            // replace db field patterns
            string filename = parser.Replace(currPattern, new MatchEvaluator(dbArtistNameParser)).Trim().ToLower();
            if (filename != null)
              filenames.Add(filename);
          }
          return filenames;
        }
      }
      // Album Artwork
      if (mv.GetType() == typeof(DBAlbumInfo))
      {
        mvAlbumObject = (DBAlbumInfo)mv;
        lock (lockObj)
        {
          List<string> filenames = new List<string>();
          foreach (string currPattern in pattern.Split('|'))
          {
            // replace db field patterns
            string filename = parser.Replace(currPattern, new MatchEvaluator(dbAlbumNameParser)).Trim().ToLower();
            if (filename != null)
              filenames.Add(filename);
          }
          return filenames;
        }
      }
      // This is track data
      if (mv.GetType() == typeof(DBTrackInfo))
      {
        // try to create our filename(s)
        this.mvTrackObject = (DBTrackInfo)mv;
        //this.mvTrackObject.LocalMedia[0].TrimmedFullPath;
        lock (lockObj)
        {
          List<string> filenames = new List<string>();
          foreach (string currPattern in pattern.Split('|'))
          {
            // replace db field patterns

            string filename = parser.Replace(currPattern, new MatchEvaluator(dbTrackNameParser)).Trim().ToLower();

            // replace %filename% pattern
            if (mvTrackObject.LocalMedia.Count > 0)
            {
              string videoFileName = Path.GetFileNameWithoutExtension(mvTrackObject.LocalMedia[0].File.Name);
              filename = filename.Replace("%filename%", videoFileName);
            }

            filenames.Add(filename);
          }
          return filenames;
        }
      }

      //should never get here
      return null;
    }

    /// <summary>
    /// Get Artist field
    /// </summary>
    /// <param name="match"></param>
    /// <returns></returns>
    private string dbArtistNameParser(Match match)
    {
      // try to grab the field object
      string fieldName = match.Value.Substring(1, match.Length - 2);

      // match the DB Field
      DBField field = DBField.GetFieldByDBName(typeof(DBArtistInfo), fieldName);

      // if no dice, the user probably entered an invalid string.
      if (field == null && match.Value != "%filename")
      {
        logger.Error("Error parsing \"" + match.Value + "\" from local_art_pattern advanced setting. Not a database field name.");
        return match.Value;
      }

      return field.GetValue(mvArtistObject).ToString();
    }

    /// <summary>
    /// Get Album Field
    /// </summary>
    /// <param name="match"></param>
    /// <returns></returns>
    private string dbAlbumNameParser(Match match)
    {
      // try to grab the field object
      string fieldName = match.Value.Substring(1, match.Length - 2);

      // match the DB Field
      DBField field = DBField.GetFieldByDBName(typeof(DBAlbumInfo), fieldName);

      // if no dice, the user probably entered an invalid string.
      if (field == null && match.Value != "%filename")
      {
        logger.Error("Error parsing \"" + match.Value + "\" from local_art_pattern advanced setting. Not a database field name.");
        return match.Value;
      }

      return field.GetValue(mvAlbumObject).ToString();
    }

    /// <summary>
    /// Get Track field
    /// </summary>
    /// <param name="match"></param>
    /// <returns></returns>
    private string dbTrackNameParser(Match match)
    {
      // try to grab the field object
      string fieldName = match.Value.Substring(1, match.Length - 2);

      //Bit of a bodge here to support %title%,really need to handle this differnetly
      if (fieldName.ToLower() == "title")
        return Path.GetFileNameWithoutExtension(mvTrackObject.LocalMedia[0].TrimmedFullPath);

      // match the DB Field
      DBField field = DBField.GetFieldByDBName(typeof(DBTrackInfo), fieldName);

      // if no dice, the user probably entered an invalid string.
      if (field == null && match.Value != "%filename")
      {
        logger.Error("Error parsing \"" + match.Value + "\" from local_art_pattern advanced setting. Not a database field name.");
        return match.Value;
      }

      return field.GetValue(mvTrackObject).ToString();
    }

    /// <summary>
    /// based on the filename list, returns the first file in the folder, otherwise null
    /// </summary>
    /// <param name="folder"></param>
    /// <param name="filenames"></param>
    /// <returns></returns>
    private FileInfo getFirstFileFromFolder(string folder, List<string> filenames)
    {
      foreach (string currFilename in filenames)
      {
        // make sure what we have is a valie filename
        char[] invalidChars = Path.GetInvalidFileNameChars();
        string invalidString = Regex.Escape(new string(invalidChars));
        string legalFilename = Regex.Replace(currFilename, "[" + invalidString + "]", "");

        FileInfo newImage = new FileInfo(folder + "\\" + legalFilename);
        if (newImage.Exists)
          return newImage;
      }

      return null;
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
          
          string strArtist = artist;
          DatabaseUtility.RemoveInvalidChars(ref strArtist);
          strArtist = strArtist.Replace('ä', '%');
          strArtist = strArtist.Replace('ö', '%');
          strArtist = strArtist.Replace('ü', '%');
          strArtist = strArtist.Replace('/', '%');
          strArtist = strArtist.Replace('-', '%');
          strArtist = strArtist.Replace("%%", "%");
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
          var albums = new ArrayList();;

          if (artist != null)
          {
            ReportProgress("Getting Albums...");
            logger.Debug("GetDetails: Getting Albums: " + artist);

            albums.Clear();

            string strArtist = artist;
            DatabaseUtility.RemoveInvalidChars(ref strArtist);
            strArtist = strArtist.Replace('ä', '%');
            strArtist = strArtist.Replace('ö', '%');
            strArtist = strArtist.Replace('ü', '%');
            strArtist = strArtist.Replace('/', '%');
            strArtist = strArtist.Replace('-', '%');
            strArtist = strArtist.Replace("%%", "%");
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
      throw new NotImplementedException();
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

    private void ReportProgress(string text)
    {
      if (ProgressChanged != null)
      {
        ProgressChanged(this, new ProgressEventArgs { Text = "Local music DB: " + text });
      }
    }
  }

}
