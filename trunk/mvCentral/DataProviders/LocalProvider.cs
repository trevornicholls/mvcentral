using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using mvCentral.Database;
using NLog;
using System.Net;
using Cornerstone.Database;
using System.Web;
using mvCentral.LocalMediaManagement;
using mvCentral.SignatureBuilders;
using System.Reflection;
using System.Threading;
using System.Globalization;
using Cornerstone.Extensions;

namespace mvCentral.DataProviders
{
  public class LocalProvider : IMusicVideoProvider
  {
    private static Logger logger = LogManager.GetCurrentClassLogger();

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
        return "Local Data";
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
      get { return "Music Videos Team"; }
    }

    public string Description
    {
      get { return "Returns artwork already available on the local system."; }
    }

    public string Language
    {
      get { return ""; }
    }

    public string LanguageCode
    {
      get { return ""; }
    }

    public List<string> LanguageCodeList
    {
      get
      {
        List<string> supportLanguages = new List<string>();
        return supportLanguages;
      }
    }


    public bool ProvidesDetails
    {
      get { return false; }
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

    /// <summary>
    /// Get Artist Artwork already on disk
    /// </summary>
    /// <param name="mv"></param>
    /// <returns></returns>
    public bool GetArtistArt(DBArtistInfo mv)
    {
      logger.Debug("In Method GetArtistArt(DBArtistInfo mv)");
      if (mv == null)
        return false;

      // if we already have a artist move on for now
      if (mv.ArtFullPath.Trim().Length > 0)
        return false;

      bool found = false;

      found &= getArtistArtFromArtistArtFolder(mv);
      found &= getOldArtistArt(mv);

      return found;
    }
    /// <summary>
    /// Get Album Artwork already on disk
    /// </summary>
    /// <param name="mv"></param>
    /// <returns></returns>
    public bool GetAlbumArt(DBAlbumInfo mv)
    {
      logger.Debug("In Method GetAlbumArt(DBAlbumInfo mv)");

      if (mv == null)
        return false;

      // if we already have a artist move on for now
      if (mv.ArtFullPath.Trim().Length > 0)
        return false;

      bool found = false;

      found &= getAlbumArtFromAlbumArtFolder(mv);
      found &= getOldAlbumArt(mv);

      return found;
    }
    /// <summary>
    /// Get Track/Video Artwork already on disk
    /// </summary>
    /// <param name="mv"></param>
    /// <returns></returns>
    public bool GetTrackArt(DBTrackInfo mv)
    {
      logger.Debug("In Method GetTrackArt(DBTrackInfo mv)");
      if (mv == null)
        return false;
      if (this.mvTrackObject == null) this.mvTrackObject = mv;

      // if we already have a trackimage move on for now
      if (mv.ArtFullPath.Trim().Length > 0)
        return false;

      bool found = false;

      found &= getTrackArtFromTrackArtFolder(mv);
      found &= getOldTrackArt(mv);

      return found;
    }
    /// <summary>
    /// Get Artist Artwork from the Artist Artwork folder.
    /// </summary>
    /// <param name="mvArtistObject"></param>
    /// <returns></returns>
    private bool getArtistArtFromArtistArtFolder(DBArtistInfo mvArtistObject)
    {
      logger.Debug("In Method getArtistArtFromArtistArtFolder(DBArtistInfo mv)");
      if (mvArtistObject == null)
        return false;

      // grab a list of possible filenames for the artist based on the user pattern
      string pattern = mvCentralCore.Settings.ArtistArtworkFilenamePattern;
      List<string> filenames = getPossibleNamesFromPattern(pattern, mvArtistObject);

      // check the ArtistArt folder for the user patterned ArtistArt
      string artistArtFolderPath = mvCentralCore.Settings.ArtistArtFolder;
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
      logger.Debug("In Method getPossibleNamesFromPattern(string pattern, object mv)");
      Regex parser = new Regex("%(.*?)%", RegexOptions.IgnoreCase);
      // Artist Artwork
      if (mv.GetType() == typeof(DBArtistInfo))
      {
        logger.Debug("In Method getPossibleNamesFromPattern(string pattern, object mv) Object:Artist");
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
        logger.Debug("In Method getPossibleNamesFromPattern(string pattern, object mv)  Object:Album");
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
        logger.Debug("In Method getPossibleNamesFromPattern(string pattern, object mv)  Object:Track");
        // try to create our filename(s)
        this.mvTrackObject = (DBTrackInfo)mv;
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
    /// Get Track field
    /// </summary>
    /// <param name="match"></param>
    /// <returns></returns>
    private string dbTrackNameParser(Match match)
    {
      // try to grab the field object
      string fieldName = match.Value.Substring(1, match.Length - 2);
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
    /// Get Album Field
    /// </summary>
    /// <param name="match"></param>
    /// <returns></returns>
    private string dbAlbumNameParser(Match match)
    {
      // try to grab the field object
      string fieldName = match.Value.Substring(1, match.Length - 2);
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
    /// based on the filename list, returns the first file in the folder, otherwise null
    /// </summary>
    /// <param name="folder"></param>
    /// <param name="filenames"></param>
    /// <returns></returns>
    private FileInfo getFirstFileFromFolder(string folder, List<string> filenames)
    {
      logger.Debug("In Method getFirstFileFromFolder(string folder, List<string> filenames)");
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
      throw new NotImplementedException();
    }

    public bool GetDetails(DBTrackInfo mv)
    {
      throw new NotImplementedException();
    }

    public List<DBTrackInfo> Get(MusicVideoSignature mvSignature)
    {
      throw new NotImplementedException();
    }

    public UpdateResults Update(DBTrackInfo mv)
    {
      throw new NotImplementedException();
    }

  }
}
