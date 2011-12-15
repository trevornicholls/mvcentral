using System;
using System.Collections.Generic;
using System.IO;
using Cornerstone.Tools;
using mvCentral.Database;
using mvCentral.Extensions;
using mvCentral.LocalMediaManagement;
using System.Text.RegularExpressions;
using NLog;
using System.Collections.ObjectModel;



namespace mvCentral.SignatureBuilders
{

  /// <summary>
  /// a mv signature object that is used as input to a dataprovider.
  /// </summary>
  public class MusicVideoSignature
  {

    #region Private Variables

    private static Logger logger = LogManager.GetCurrentClassLogger();
    private string baseTitle = null;

    #endregion

    #region Public properties

    /// <summary>
    /// The full musicvideo title
    /// </summary>
    public string Title
    { // ex. "Pirates of Silicon Valley"
      get { return title; }
      set
      {
        if (value != null)
          title = value.Trim();

        if (!String.IsNullOrEmpty(title))
        {
          keywords = title.ToKeywords();
          baseTitle = title.Equalize();
          logger.Debug("Normalize BaseTitle: " + baseTitle);

        }
        else
        {
          title = null;
          keywords = null;
          baseTitle = null;
        }

      }
    }

    private string title = null;

    /// <summary>
    /// Keywords derived from the full mv title, can be used
    /// by a data provider for better results.
    /// </summary>
    public string Keywords
    {
      get { return keywords; }
    } private string keywords = null;

    /// <summary>
    /// Year / Release Date 
    /// </summary>
    /// <example>
    /// 1999
    /// </example>
    public int? Year = null;

    public string MdId
    {
      get { return md_id; }
      set
      {
        if (value != null)
          md_id = value;
        if (md_id == string.Empty)
          md_id = null;
      }
    } private string md_id = null;

    public string ArtistMdId
    {
      get { return artistmd_id; }
      set
      {
        if (value != null)
          artistmd_id = value;
        if (artistmd_id == string.Empty)
          artistmd_id = null;
      }
    } private string artistmd_id = null;

    public string AlbumMdId
    {
      get { return albummd_id; }
      set
      {
        if (value != null)
          albummd_id = value;
        if (albummd_id == string.Empty)
          albummd_id = null;
      }
    } private string albummd_id = null;

    /// <summary>
    /// String version of the Disc ID (16 character hash of a DVD)
    /// </summary>
    public string DiscId
    {
      get
      {
        if (LocalMedia != null)
          discid = LocalMedia[0].DiscId;

        return discid;
      }
      set { discid = value; }
    } private string discid = null;

    /// <summary>
    /// String version of the filehash of the first mv file (16 characters)
    /// </summary>
    public string MusicVideoHash
    {
      get
      {
        if (LocalMedia != null)
          filehash = LocalMedia[0].FileHash;

        return filehash;
      }
      set { filehash = value; }
    } private string filehash = null;

    public List<DBLocalMedia> LocalMedia = null; // LocalMedia collection 

    #region Read-only

    /// <summary>
    /// The base foldername of the mv
    /// </summary>
    public string Folder
    {
      get
      {
        if (folder == null)
          updatePropertiesFromLocalMedia();

        return folder;
      }
    } private string folder = null;

    /// <summary>
    /// The filename of the mv
    /// </summary>
    public string File
    {
      get
      {
        if (file == null)
          updatePropertiesFromLocalMedia();

        return file;
      }
    } private string file = null;

    /// <summary>
    /// Complete path to the base folder
    /// </summary>
    public string Path
    {
      get
      {
        if (path == null)
          updatePropertiesFromLocalMedia();

        return path;
      }
    } private string path = null;


    /// <summary>
    /// Artist of the mv
    /// </summary>
    public string Artist
    {
      get
      {
        if (artist == null)
          updatePropertiesFromLocalMedia();

        return artist;
      }
      set
      {
        artist = value;
      }
    } private string artist = null;

    /// <summary>
    /// Album of the mv
    /// </summary>
    public string Album
    {
      get
      {
        if (album == null)
          updatePropertiesFromLocalMedia();

        return album;
      }
      set
      {
        album = value;
      }
    } private string album = null;

    /// <summary>
    /// Track of the mv
    /// </summary>
    public string Track
    {
      get
      {
        if (track == null)
          updatePropertiesFromLocalMedia();

        return track;
      }
      set
      {
        track = value;
      }
    } private string track = null;

    #endregion

    #endregion

    #region Constructors

    public MusicVideoSignature()
    {

    }

    public MusicVideoSignature(List<DBLocalMedia> localMedia)
    {
      LocalMedia = localMedia;
    }

    public MusicVideoSignature(DBTrackInfo mv)
    {
      Title = mv.Track;
      //            Year = mv.Year;
      MdId = mv.MdID;
      ArtistMdId = mv.ArtistInfo[0].MdID;
      if (mv.AlbumInfo.Count > 0) AlbumMdId = mv.AlbumInfo[0].MdID;
      LocalMedia = mv.LocalMedia;
    }

    public MusicVideoSignature(string title)
    {
      Title = title;
    }

    #endregion

    #region Public methods

    public MatchResult GetMatchResult(DBTrackInfo mv)
    {
      MdId = mv.MdID;
      ArtistMdId = mv.ArtistInfo[0].MdID;
      if (mv.AlbumInfo.Count > 0) AlbumMdId = mv.AlbumInfo[0].MdID;

      // Create a new score card
      MatchResult result = new MatchResult();

      // Get the default scores for this mv
      result.TitleScore = matchTitle(mv.ArtistInfo[0].Artist + " " + mv.Track);
      //            result.YearScore = matchYear(mv.Year);
      result.MdMatch = matchMd(mv.MdID);
      result.ArtistMdMatch = matchArtistMd(ArtistMdId);
      result.AlbumMdMatch = matchAlbumMd(AlbumMdId);

      // check if this match came from our #1 details provider
      ReadOnlyCollection<DBSourceInfo> detailSources = mvCentralCore.DataProviderManager.DetailSources;
      if (detailSources.Count > 0 && detailSources[0] == mv.PrimarySource)
        result.FromTopSource = true;
      else
        result.FromTopSource = false;

      // If we don't have a perfect score on the original title
      // iterate through the available alternate titles and check
      // them to lower the score if possible
      if (result.TitleScore > 0)
      {
        //                foreach (string alternateTitle in mv.AlternateTitles.ToArray()) {
        //                    int score = matchTitle(alternateTitle);
        // if this match is better than the previous one save the score
        //                    if (score < result.TitleScore) {
        //                        result.TitleScore = score;
        //                        result.AlternateTitle = alternateTitle;
        //                    }
        // if the best score is 0 (the best possible score) then stop score checking
        //                    if (result.TitleScore == 0) break;
        //                }
      }

      // return the result
      logger.Debug("MATCHING: '{0}' WITH: '{1}' RESULT: {2}", this.baseTitle, mv.Track, result.ToString());
      return result;
    }

    private int matchTitle(string title)
    {
      logger.Debug("Normalize : " + title);
      string otherTitle = title.Equalize();
      int score = AdvancedStringComparer.Levenshtein(baseTitle, otherTitle);
      return score;
    }

    private int matchYear(int year)
    {
      if (Year > 0 && year > 0)
      {
        int score = ((int)Year - year);
        score = (score < 0) ? score * -1 : score;
        return score;
      }
      else
      {
        return 0;
      }
    }

    private bool matchMd(string mdid)
    {
      if (md_id == null)
        return false;

      return (md_id == mdid);
    }

    private bool matchArtistMd(string mdid)
    {
      if (artistmd_id == null)
        return false;

      return (artistmd_id == mdid);
    }
    private bool matchAlbumMd(string mdid)
    {
      if (albummd_id == null)
        return false;

      return (albummd_id == mdid);
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Updates the File, Folder and Path property using the LocalMedia data
    /// </summary>
    private void updatePropertiesFromLocalMedia()
    {
      if (LocalMedia != null)
      {
        string filenameToParse;

        DirectoryInfo baseFolder = Utility.GetMusicVideoBaseDirectory(LocalMedia[0].File.Directory);
        folder = baseFolder.Name;
        file = LocalMedia[0].File.Name;
        path = baseFolder.FullName;

        //if (file == "VIDEO_TS.IFO")
        //  parserFilename = folder + ".dvd";
        //else
        //  parserFilename = LocalMedia[0].File.FullName;

        filenameToParse = LocalMedia[0].File.FullName;
        FilenameParser parser = new FilenameParser(filenameToParse, baseFolder);

        parseResult CurrentParseResult = new parseResult();
        parser.Matches.Add(MusicVideoImporter.cFilename, file);
        parser.Matches.Add(MusicVideoImporter.cExt, LocalMedia[0].File.Extension);
        parser.Matches.Add(MusicVideoImporter.cPath, LocalMedia[0].TrimmedFullPath);
        parser.Matches.Add(MusicVideoImporter.cVolumeLabel, LocalMedia[0].MediaLabel);

        if (!parser.Matches.ContainsKey(MusicVideoImporter.cArtist))
        {
          CurrentParseResult.failedArtist = true;
          CurrentParseResult.success = false;
          CurrentParseResult.exception = "Artist is not valid";
        }

        if (!parser.Matches.ContainsKey(MusicVideoImporter.cTrack))
        {
          CurrentParseResult.failedTrack = true;
          CurrentParseResult.success = false;
          CurrentParseResult.exception = "Track is not valid";
        }

        if (!parser.Matches.ContainsKey(MusicVideoImporter.cAlbum))
        {
          CurrentParseResult.failedAlbum = true;
          CurrentParseResult.success = true;
          CurrentParseResult.exception = "Album is not valid";
        }

        CurrentParseResult.match_filename = LocalMedia[0].TrimmedFullPath;
        CurrentParseResult.full_filename = LocalMedia[0].File.Name;
        CurrentParseResult.parser = parser;

        if (!CurrentParseResult.failedArtist)
        {
          artist = CurrentParseResult.Artist;
          artist = Regex.Replace(artist, @"\s{2,}", " ").Trim();
        }

        if (!CurrentParseResult.failedAlbum)
        {
          album = CurrentParseResult.Album;
          album = Regex.Replace(album, @"\s{2,}", " ").Trim();
        }

        if (!CurrentParseResult.failedTrack)
        {
          track = CurrentParseResult.Track;
          track = Regex.Replace(track, @"\s{2,}", " ").Trim();
        }


        logger.Debug(string.Format("Result of Parsing : Artist: {0}    Album: {1}    Track: {2}", artist, album, track));
      }
    }


    #endregion

    #region Overrides

    public override string ToString()
    {
      return String.Format("Path= \"{0}\", Folder= \"{1}\", File= \"{2}\", Keywords= \"{3}\", Title= \"{4}\", Year= {5}, DiscId= \"{6}\", MusicVideoHash= \"{7}\", MdId= \"{8}\"",
      this.Path, this.Folder, this.File, this.Keywords, this.Title, this.Year.ToString(), this.DiscId, this.MusicVideoHash, this.MdId);
    }

    #endregion

  }

  /// <summary>
  /// This struct represents a score card that is the result of
  /// comparing a signature with mv information. The value can 
  /// be used to rank a list of possible matches and to determine 
  /// if they can be auto-approved.
  /// </summary>
  public struct MatchResult
  {

    #region Public Properties

    public int TitleScore;
    public int YearScore;
    public bool MdMatch;
    public bool ArtistMdMatch;
    public bool AlbumMdMatch;
    public string AlternateTitle;
    public bool FromTopSource;

    #endregion

    #region Public Methods

    /// <summary>
    /// Get a value indicating if an alternate title was used for the title score
    /// </summary>
    /// <returns>True if an alternate title was used for the title score</returns>
    public bool AlternateTitleUsed()
    {
      return (!String.IsNullOrEmpty(AlternateTitle));
    }

    /// <summary>
    /// Get a value indicating wether this result can be auto-approved because
    /// it meets the minimal requirements
    /// </summary>
    /// <returns>True, if the result can be auto-approved</returns>
    public bool AutoApprove()
    {

      if (!mvCentralCore.Settings.AutoApprove) return false;


      if (mvCentralCore.Settings.AutoApproveOnlyPrimarySource && !FromTopSource)
        return false;

      // DB Auto-Approval
      if (MdMatch && mvCentralCore.Settings.AutoApproveOnMDMatch)
        return true;

      // DB Auto-Approval
      if (ArtistMdMatch && mvCentralCore.Settings.AutoApproveOnArtistMDMatch)
        return true;

      // DB Auto-Approval
      if (AlbumMdMatch && mvCentralCore.Settings.AutoApproveOnAlbumMDMatch)
        return true;

      // Alternate Title Auto-Approve Limitation
      if (!mvCentralCore.Settings.AutoApproveOnAlternateTitle && AlternateTitleUsed())
        return false;

      // Title + Year Auto-Approval
      if (TitleScore <= mvCentralCore.Settings.AutoApproveThreshold)
        if (YearScore <= mvCentralCore.Settings.AutoApproveYearDifference)
          return true;

      return false;
    }

    #endregion

    #region Overrides

    public override string ToString()
    {
      return String.Format("TitleScore={0}, YearScore={1}, MdMatch={2},  ArtistMdMatch={3},  AlbumMdMatch={4}, AlternateTitleUsed={5}, AlternateTitle='{6}', AutoApprove={7}",
      TitleScore, YearScore, MdMatch, ArtistMdMatch, AlbumMdMatch, AlternateTitleUsed().ToString(), AlternateTitle, AutoApprove().ToString());
    }

    #endregion
  }
}
