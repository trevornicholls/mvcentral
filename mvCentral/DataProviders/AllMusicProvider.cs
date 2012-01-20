using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
using System.Windows.Forms;
using System.Linq;
using System.Net;

using Cornerstone.Tools;
using mvCentral.Database;
using mvCentral.SignatureBuilders;
using mvCentral.LocalMediaManagement;
using mvCentral.LocalMediaManagement.MusicVideoResources;
using mvCentral.Utils;
using mvCentral.ConfigScreen.Popups;

using MediaPortal.Util;
using MediaPortal.Music.Database;
using NLog;


namespace mvCentral.DataProviders
{

  class AllMusicProvider : InternalProvider, IMusicVideoProvider
  {
    private static Logger logger = LogManager.GetCurrentClassLogger();

    private static readonly object lockList = new object();

    #region Provider variables

    private const string BaseURL = "http://www.allmusic.com/search/artist/";
    private const string AlbumRegExpPattern = @"<td\s*class=""cell""><a\s*href=""(?<albumURL>.*?)"">(?<albumName>.*)</a></td>";
    private static readonly Regex AlbumURLRegEx = new Regex(AlbumRegExpPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
    private const string ArtistRegExpPattern = @"<td><a href=""(?<artistURL>.*?)"">(?<artist>.*?)</a></td>\s*<td>(?<genres>.*?)</td>\s*<td>(?<years>.*?)</td>\s*</tr>";
    private static readonly Regex ArtistURLRegEx = new Regex(ArtistRegExpPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex BracketRegEx = new Regex(@"(.*)\(.+\)$", RegexOptions.Compiled);
    private static readonly Regex PunctuationRegex = new Regex("[?!.:;)()-_ ]", RegexOptions.Compiled);

    private Match _match = null;
    private string _strFormed = "";
    private static bool _strippedPrefixes;
    private static bool _logMissing;
    private static bool _useAlternative = true;
    private static bool _useProxy;
    private static string _proxyHost;
    private static int _proxyPort;

    List<string> albumURLList = new List<string>();

    #endregion

    #region Public Methods

    public string Name
    {
      get
      {
        return "www.allmusic.com";
      }
    }

    public string Description
    {
      get { return "Returns details, art from allmusic.com."; }
    }

    public string Language
    {
      get { return new CultureInfo("en").DisplayName; }
    }

    public string LanguageCode
    {
      get { return "en"; }
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



    public bool ProvidesArtistArt
    {
      get { return false; }
    }

    public bool ProvidesAlbumArt
    {
      get { return true; }
    }

    public bool ProvidesTrackArt
    {
      get { return false; }
    }


    /// <summary>
    /// get the artist details and update missing data from this source
    /// </summary>
    /// <param name="mv"></param>
    /// <returns></returns>
    public DBTrackInfo GetArtistDetail(DBTrackInfo mv)
    {
      string strArtistHTML;
      string strArtistURL;

      // Get details of the artist
      string artist = mv.ArtistInfo[0].Artist;
      if (GetArtistHTML(artist, out strArtistHTML, out strArtistURL))
      {
        var artistInfo = new MusicArtistInfo();
        if (artistInfo.Parse(strArtistHTML))
        {
          artistInfo.Artist = artist;
          DBArtistInfo mv1 = (DBArtistInfo)mv.ArtistInfo[0];
          updateMusicVideoArtist(ref mv1, artistInfo, strArtistHTML);
        }
      }
      return mv;
    }

    /// <summary>
    /// get the artist details and update missing data from this source
    /// </summary>
    /// <param name="mv"></param>
    /// <returns></returns>
    public DBTrackInfo GetAlbumDetail(DBTrackInfo mv)
    {
      string strAlbumHTML;
      string album = mv.AlbumInfo[0].Album;
      string artist = mv.ArtistInfo[0].Artist;

      if (GetAlbumHTML(artist, album, out strAlbumHTML))
      {
        var albumInfo = new MusicAlbumInfo();
        if (albumInfo.Parse(strAlbumHTML))
        {
          albumInfo.Artist = album;
          DBAlbumInfo mv1 = (DBAlbumInfo)mv.AlbumInfo[0];
          setMusicVideoAlbum(ref mv1, albumInfo);
        }
      }
      return mv;
    }


    /// <summary>
    /// Get Artist Artwork
    /// </summary>
    /// <param name="mv"></param>
    /// <returns></returns>
    public bool GetArtistArt(DBArtistInfo mv)
    {
      return false;
    }
    /// <summary>
    /// Get Track Artwork
    /// </summary>
    /// <param name="mv"></param>
    /// <returns></returns>
    public bool GetTrackArt(DBTrackInfo mv)
    {
      return false;
    }
    /// <summary>
    /// Get the Album Art
    /// </summary>
    /// <param name="mv"></param>
    /// <returns></returns>
    public bool GetAlbumArt(DBAlbumInfo mvAlbumObject)
    {
      logger.Info("In Method : GetAlbumArt(DBAlbumInfo mv)");

      if (mvAlbumObject == null)
        return false;

      List<string> albumImageList = mvAlbumObject.ArtUrls;
      // Reload existing Artwork - Why springs to mind??
      if (albumImageList.Count > 0)
      {
        // grab album art loading settings
        int maxAlbumArt = mvCentralCore.Settings.MaxAlbumArts;

        int albumartAdded = 0;
        int count = 0;
        foreach (string albumImage in albumImageList)
        {
          if (mvAlbumObject.AlternateArts.Count >= maxAlbumArt)
            break;

          if (mvAlbumObject.AddArtFromURL(albumImage) == ImageLoadResults.SUCCESS)
            albumartAdded++;

          count++;
        }
        // We added some artwork so commit
        if (count > 0)
          mvAlbumObject.Commit();
      }

      // Now add any new art from this provider
      string strAlbumHTML;
      DBArtistInfo artist = null;
      List<DBTrackInfo> tracksOnAlbum = DBTrackInfo.GetEntriesByAlbum(mvAlbumObject);

      if (tracksOnAlbum.Count > 0)
        artist = DBArtistInfo.Get(tracksOnAlbum[0]);

      if (GetAlbumHTML(artist.Artist, mvAlbumObject.Album, out strAlbumHTML))
      {
        var albumInfo = new MusicAlbumInfo();

        if (albumInfo.Parse(strAlbumHTML))
        {
          ImageLoadResults imageLoadResults = mvAlbumObject.AddArtFromURL(albumInfo.ImageURL);

          if (imageLoadResults == ImageLoadResults.SUCCESS || imageLoadResults == ImageLoadResults.SUCCESS_REDUCED_SIZE)
            mvAlbumObject.Commit();
        }
      }
      // We always return sucess...
      return true;
    }
    /// <summary>
    /// Get the Artist, Album Details
    /// </summary>
    /// <param name="mv"></param>
    /// <returns></returns>
    public bool GetDetails(DBBasicInfo mv)
    {
      string strArtistHTML;
      string strAlbumHTML;
      string strArtistURL;

      // Get details of the artist
      if (mv.GetType() == typeof(DBArtistInfo))
      {
        string artist = ((DBArtistInfo)mv).Artist;
        if (GetArtistHTML(artist, out strArtistHTML, out strArtistURL))
        {
          var artistInfo = new MusicArtistInfo();
          if (artistInfo.Parse(strArtistHTML))
          {
            artistInfo.Artist = artist;
            DBArtistInfo mv1 = (DBArtistInfo)mv;
            setMusicVideoArtist(ref mv1, artistInfo, strArtistHTML);
            GetArtistArt((DBArtistInfo)mv);
          }
        }
        return false;
      }
      // get details of the album
      if (mv.GetType() == typeof(DBAlbumInfo))
      {
        string album = ((DBAlbumInfo)mv).Album;
        List<DBTrackInfo> trackList = DBTrackInfo.GetEntriesByAlbum((DBAlbumInfo)mv);
        DBArtistInfo artist = DBArtistInfo.Get(trackList[0]);

        if (GetAlbumHTML(artist.Artist, album, out strAlbumHTML))
        {
          var albumInfo = new MusicAlbumInfo();
          if (albumInfo.Parse(strAlbumHTML))
          {
            albumInfo.Artist = album;
            DBAlbumInfo mv1 = (DBAlbumInfo)mv;
            setMusicVideoAlbum(ref mv1, albumInfo);
          }
        }
        return false;
      }
      return true;
    }

    /// <summary>
    /// This will try and get the track and album data. Will first find artist and then check all albums for a matching track
    /// </summary>
    /// <param name="mvSignature"></param>
    /// <returns></returns>
    public List<DBTrackInfo> GetTrackDetail(MusicVideoSignature mvSignature)
    {
      string strArtistHTML;
      string strAlbumHTML;
      string strArtistURL;
      bool albumFound = false;

      List<DBTrackInfo> results = new List<DBTrackInfo>();

      if (mvSignature == null)
        return results;

      lock (lockList)
      {

        string artist = mvSignature.Artist;
        if (GetArtistHTML(artist, out strArtistHTML, out strArtistURL))
        {
          var artistInfo = new MusicArtistInfo();
          if (artistInfo.Parse(strArtistHTML))
          {
            artistInfo.Artist = artist;
            if (GetAlbumURLList(strArtistURL))
            {
              // we have some albums - now check the tracks in each album
              foreach (string albumURL in albumURLList)
              {
                if (GetAlbumHTMLOnly(albumURL, out strAlbumHTML))
                {
                  var albumInfo = new MusicAlbumInfo();
                  if (albumInfo.Parse(strAlbumHTML))
                  {
                    string[] tracksOnAlbum = albumInfo.Tracks.Split('|');
                    foreach (string track in tracksOnAlbum)
                    {
                      if (!string.IsNullOrEmpty(track.Trim()))
                      {
                        string[] trackData = track.Split('@');
                        if (mvSignature.Track == trackData[1])
                        {
                          albumFound = true;
                          break;
                        }
                      }
                    }
                  }
                }
                if (albumFound)
                  break;
              }
            }

          }
        }

        //DBTrackInfo mv = getMusicVideoTrack(mvSignature.Artist, mvSignature.Album, mvSignature.Track);
        //if (mv != null)
        //{
        //  if (mv.ArtistInfo.Count == 0)
        //  {
        //    DBArtistInfo d4 = new DBArtistInfo();
        //    d4.Artist = mvSignature.Artist;
        //    mv.ArtistInfo.Add(d4);
        //  }
        //  results.Add(mv);
        //}

      }
      return results;
    }

    /// <summary>
    /// Get the album details 
    /// </summary>
    /// <param name="basicInfo"></param>
    /// <param name="albumTitle"></param>
    /// <param name="AlbumMBID"></param>
    /// <returns></returns>
    public bool GetAlbumDetails(DBBasicInfo basicInfo, string albumTitle, string AlbumMBID)
    {
      return true;
    }

    /// <summary>
    /// get the first x words from the Bio
    /// </summary>
    /// <param name="text"></param>
    /// <param name="maxWordCount"></param>
    /// <returns></returns>
    public string getBioSummary(string text, int maxWordCount)
    {
      // Bail out if no content to summerize
      if (string.IsNullOrEmpty(text.Trim()))
        return string.Empty;

      int wordCounter = 0;
      int stringIndex = 0;
      char[] delimiters = new[] { '\n', ' ', ',', '.' };

      while (wordCounter < maxWordCount)
      {
        stringIndex = text.IndexOfAny(delimiters, stringIndex + 1);
        if (stringIndex == -1)
          return text;

        ++wordCounter;
      }

      return text.Substring(0, stringIndex);
    }


    public UpdateResults UpdateTrack(DBTrackInfo mv)
    {
      return UpdateResults.SUCCESS;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Set the Album Information - override existing data
    /// </summary>
    /// <param name="mv"></param>
    /// <param name="albumInfo"></param>
    private void setMusicVideoAlbum(ref DBAlbumInfo mv, MusicAlbumInfo albumInfo)
    {
      mv.bioSummary = getBioSummary(albumInfo.Review, 50);
      mv.bioContent = albumInfo.Review;
      mv.YearReleased = albumInfo.DateOfRelease;
      mv.Rating = albumInfo.Rating;
    }
    /// <summary>
    /// Update missing album information
    /// </summary>
    /// <param name="mv"></param>
    /// <param name="albumInfo"></param>
    private void updateMusicVideoAlbum(ref DBAlbumInfo mv, MusicAlbumInfo albumInfo)
    {
      if (mv.bioSummary.Trim() == string.Empty)
        mv.bioSummary = getBioSummary(albumInfo.Review, 50);

      if (mv.bioContent.Trim() == string.Empty)
        mv.bioContent = albumInfo.Review;

      if (mv.YearReleased.Trim() == string.Empty)
        mv.YearReleased = albumInfo.DateOfRelease;

      if (mv.Rating == 0)
        mv.Rating = albumInfo.Rating;
    }
    /// <summary>
    /// Set the Artist information, override existing information
    /// </summary>
    /// <param name="mv"></param>
    /// <param name="artistInfo"></param>
    private void setMusicVideoArtist(ref DBArtistInfo mv, MusicArtistInfo artistInfo, string strArtistHTML)
    {
      _strFormed = string.Empty;
      HTMLUtil util = new HTMLUtil();
      // Formed - Get here directly as not a field provided by the Musicartistinfo class
      string pattern = @"<h3>.*Formed.*</h3>\s*?<p>(.*)</p>";
      if (FindPattern(pattern, strArtistHTML))
      {
        string strValue = _match.Groups[1].Value;
        util.RemoveTags(ref strValue);
        util.ConvertHTMLToAnsi(strValue, out _strFormed);
        _strFormed = _strFormed.Trim();
      }

      mv.Formed = _strFormed;
      mv.Born = artistInfo.Born;
      mv.bioSummary = getBioSummary(artistInfo.AMGBiography, 50);
      mv.bioContent = artistInfo.AMGBiography;
      mv.Genre = artistInfo.Genres;
      mv.Tones = artistInfo.Tones;
      mv.Styles = artistInfo.Styles;
      mv.YearsActive = artistInfo.YearsActive;
    }
    /// <summary>
    /// Update missing Artist information
    /// </summary>
    /// <param name="mv"></param>
    /// <param name="artistInfo"></param>
    private void updateMusicVideoArtist(ref DBArtistInfo mv, MusicArtistInfo artistInfo, string strArtistHTML)
    {
      _strFormed = string.Empty;
      HTMLUtil util = new HTMLUtil();
      // Formed - Get here directly as not a field provided by the Musicartistinfo class
      string pattern = @"<h3>.*Formed.*</h3>\s*?<p>(.*)</p>";
      if (FindPattern(pattern, strArtistHTML))
      {
        string strValue = _match.Groups[1].Value;
        util.RemoveTags(ref strValue);
        util.ConvertHTMLToAnsi(strValue, out _strFormed);
        _strFormed = _strFormed.Trim();
      }

      if (mv.Formed.Trim() == string.Empty)
        mv.Formed = _strFormed;

      if (mv.Born.Trim() == string.Empty)
        mv.Born = artistInfo.Born;

      if (mv.bioSummary.Trim() == string.Empty)
        mv.bioSummary = getBioSummary(artistInfo.AMGBiography, 50);

      if (mv.bioContent.Trim() == string.Empty)
        mv.bioContent = artistInfo.AMGBiography;

      if (mv.Genre.Trim() == string.Empty)
        mv.Genre = artistInfo.Genres;

      if (mv.Tones.Trim() == string.Empty)
        mv.Tones = artistInfo.Tones;

      if (mv.Styles.Trim() == string.Empty)
        mv.Styles = artistInfo.Styles;

      if (mv.YearsActive.Trim() == string.Empty)
        mv.YearsActive = artistInfo.YearsActive;

    }


    private DBTrackInfo getMusicVideoTrack(string track)
    {
      return getMusicVideoTrack(null, null, track);
    }

    private DBTrackInfo getMusicVideoTrack(string artist, string album, string track)
    {
      return null;
    }

    private void setMusicVideoTrack(ref DBTrackInfo mv, string id)
    {
    }
    /// <summary>
    /// Generate Thumbnail
    /// </summary>
    /// <param name="mv"></param>
    /// <returns></returns>
    bool generateVideoThumbnail(DBTrackInfo mv)
    {
      lock (this)
      {
        string outputFilename = Path.Combine(Path.GetTempPath(), mv.Track + DateTime.Now.ToFileTimeUtc().ToString() + ".jpg");

        if (mvCentral.Utils.VideoThumbCreator.CreateVideoThumb(mv.LocalMedia[0].File.FullName, outputFilename))
        {
          if (File.Exists(outputFilename))
          {
            mv.AddArtFromFile(outputFilename);
            File.Delete(outputFilename);
            return true;
          }
          else
            return false;
        }
        else
          return false;
      }
    }
    /// <summary>
    /// Do a Regex search with the given pattern and fill the Match object
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="searchString"></param>
    /// <returns></returns>
    private bool FindPattern(string pattern, string searchString)
    {
      Regex itemsFound = new Regex(
        pattern,
        RegexOptions.IgnoreCase
        | RegexOptions.Multiline
        | RegexOptions.IgnorePatternWhitespace
        | RegexOptions.Compiled
        );

      _match = itemsFound.Match(searchString);
      if (_match.Success)
      {
        return true;
      }

      return false;
    }


    #endregion

    #region URL and HTTP Handling

    /// <summary>
    /// Used to get the artist URL from allmusic.com based on artist name
    /// </summary>
    /// <param name="strArtist">artist name</param>
    /// <param name="strArtistURL">the URL of the artist</param>
    /// <returns>True if an artist page can be found else false</returns>
    /// <summary>
    /// Used to get the artist URL from allmusic.com based on artist name
    /// </summary>
    /// <param name="strArtist">artist name</param>
    /// <param name="strArtistURL">the URL of the artist</param>
    /// <returns>True if an artist page can be found else false</returns>
    private bool GetArtistURL(String strArtist, out String strArtistURL,out List<string> strArtistURLs)
    {
      strArtistURL = string.Empty;
      strArtistURLs = new List<string>();
      try
      {
        var strEncodedArtist = EncodeString(strArtist);
        var strURL = BaseURL + strEncodedArtist + "/filter:pop";

        logger.Debug("GetArtistURL: Request URL: {0}", strURL);

        var x = (HttpWebRequest)WebRequest.Create(strURL);
        if (_useProxy)
        {
          x.Proxy = new WebProxy(_proxyHost, _proxyPort);
        }

        x.ProtocolVersion = HttpVersion.Version10;
        x.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:6.0) Gecko/20100101 Firefox/6.0";
        x.ContentType = "text/html";
        x.Timeout = 30000;
        x.AllowAutoRedirect = false;

        using (var y = (HttpWebResponse)x.GetResponse())
        {
          x.Abort();
          if ((int)y.StatusCode != 302)
          {
            y.Close();

            if (!_useAlternative)
            {
              return false;
            }
            var altTry = GetArtistURLAlternative(strArtist, out strArtistURL, out strArtistURLs);
            if (altTry)
            {
              return true;
            }
            return altTry;
          }
          strArtistURL = y.GetResponseHeader("Location");

          logger.Debug("GetArtistURLAlternative: strArtistURL: {0}", strArtistURL);

          y.Close();

        }
      }
      catch (Exception ex)
      {
        logger.Error(ex);
        return false;
      }

      return true;
    }

    /// <summary>
    /// If unable to find an artist URL based on name straight away (ie. we are sent to search page)
    /// Then attempt to find artist within search results
    /// </summary>
    /// <param name="strArtist">Name of artist we are searching for</param>
    /// <param name="strArtistURL">URL of artist</param>
    /// <returns>True if artist page found</returns>
    private bool GetArtistURLAlternative(String strArtist, out String strArtistURL, out List<string> strArtistURLs)
    {
      strArtistURL = string.Empty;
      strArtistURLs = new List<string>();

      try
      {
        var strEncodedArtist = EncodeString(strArtist);
        var strURL = BaseURL + strEncodedArtist + "/filter:pop";

        logger.Debug("GetArtistURLAlternative: Request URL: {0}", strURL);

        var x = (HttpWebRequest)WebRequest.Create(strURL);

        if (_useProxy)
        {
          x.Proxy = new WebProxy(_proxyHost, _proxyPort);
        }

        x.ProtocolVersion = HttpVersion.Version10;
        x.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:6.0) Gecko/20100101 Firefox/6.0";
        x.ContentType = "text/html";
        x.Timeout = 30000;
        x.AllowAutoRedirect = false;

        using (var y = (HttpWebResponse)x.GetResponse())
        {
          using (var z = y.GetResponseStream())
          {
            if (z == null)
            {
              return false;
            }

            string artistHTML;

            using (var sr = new StreamReader(z, Encoding.UTF8))
            {
              artistHTML = sr.ReadToEnd();
            }

            z.Close();
            x.Abort();
            y.Close();

            var matches = ArtistURLRegEx.Matches(artistHTML);
            var numberOfMatchesWithYears = 0;
             //some cases like Björk where there is only a single entry matching artist in list but stil returns list rather then redirecting to artist page
            if (matches.Count == 1)
            {
              strArtistURL = matches[0].Groups["artistURL"].ToString();
              //logger.Debug("GetArtistURLAlternative: Single match on artist screen: strArtistURL: {0}", strArtistURL);
              return true;
            }

            var strPotentialURL = string.Empty;
            var strCleanArtist = EncodeString(CleanArtist(strArtist));

            logger.Debug("GetArtistURLAlternative: Cleaned/Encoded Artist: |{0}|", strCleanArtist);

            // else there are either 0 or multiple matches so lets see how many have years populated
            foreach (Match m in matches)
            {
              var strCleanMatch = EncodeString(CleanArtist(m.Groups["artist"].ToString()));
              //logger.Debug("GetArtistURLAlternative: Cleaned/Encoded matched Artist: |{0}| compare to match |{1}|", strCleanMatch, strCleanArtist);

              if (strCleanArtist != strCleanMatch) 
                continue;

              //logger.Debug("GetArtistURLAlternative: Years: {0}", m.Groups["years"].ToString().Trim());
              if (string.IsNullOrEmpty(m.Groups["years"].ToString().Trim())) 
                continue;

              strPotentialURL = m.Groups["artistURL"].ToString();
              numberOfMatchesWithYears++;

              // give up if more than one match with years active
              if (numberOfMatchesWithYears > 1)
              {
                logger.Debug("GetArtistURLAlternative: Multiple matches with years active - returning 1st entry");
                strPotentialURL = matches[0].Groups["artistURL"].ToString();
                //strArtistURLs.Clear();
                for (int i = 1; i < 5; i++)
                {
                  string artURL = matches[i].Groups["artistURL"].ToString();
                  strArtistURLs.Add(artURL);
                }
                break;
              }
            }

            // No valid match found (Not sure about this check...)
            if (numberOfMatchesWithYears == 0)
            {
              logger.Debug("GetArtistURLAlternative: No matches with years active");
              return false;
            }

            // only one match with years active so return URL for that artist.
            strArtistURL = strPotentialURL; // matches[matchIndex].Groups["artistURL"].ToString();

            logger.Debug("GetArtistURLAlternative: Single match on artist screen with years populated: strArtistURL: {0}", strArtistURL);

            return true;
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error(ex);
        return false;
      }
    }
    /// <summary>
    /// Attempts to get the album URL from allmusic.com
    /// First off just tries album artist and album but also
    /// attempt more fuzzy logic
    /// </summary>
    /// <param name="strArtistURL">This is artist URL page</param>
    /// <param name="strAlbum">Name of the album</param>
    /// <param name="strAlbumURL">URL of album</param>
    /// <returns>True if album page found</returns>
    private bool GetAlbumURL(String strArtistURL, String strAlbum, out String strAlbumURL)
    {
      strAlbumURL = string.Empty;
      string discHTML;
      try
      {
        var x = (HttpWebRequest)WebRequest.Create(strArtistURL);

        if (_useProxy)
        {
          x.Proxy = new WebProxy(_proxyHost, _proxyPort);
        }

        x.ProtocolVersion = HttpVersion.Version10;
        x.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:6.0) Gecko/20100101 Firefox/6.0";
        x.ContentType = "text/html";
        x.Timeout = 30000;
        x.AllowAutoRedirect = false;

        using (var y = (HttpWebResponse)x.GetResponse())
        {
          using (var z = y.GetResponseStream())
          {
            if (z == null)
            {
              return false;
            }

            using (var sr = new StreamReader(z, Encoding.UTF8))
            {
              discHTML = sr.ReadToEnd();
            }

            z.Close();
            x.Abort();
            y.Close();
          }

          // build up a list of possible alternatives

          // attempt to remove stack endings (eg. disc2, (CD2) etc)
          var strStripStackEnding = strAlbum;
          MediaPortal.Util.Utils.RemoveStackEndings(ref strStripStackEnding);
          // try and remove any thing else in brackets at end of album name
          // eg. (remastered), (special edition), (vinyl) etc
          var strAlbumRemoveBrackets = BracketRegEx.Replace(strAlbum, "$1").Trim();
          // try and repalce all punctuation to try and get a match
          // sometimes you have three dots in one format but two in another
          var strRemovePunctuation = PunctuationRegex.Replace(strAlbum, "").Trim();
          // replace & and + with "and"
          var strAndAlbum = strAlbum.Replace("&", "and").Replace("+", "and");


          //logger.Debug("MusicInfoHandler: GetAlbumURL: strAlbum: |{0}|", strAlbum);
          //logger.Debug("MusicInfoHandler: GetAlbumURL: strStripStackEnding: |{0}|", strStripStackEnding);
          //logger.Debug("MusicInfoHandler: GetAlbumURL: strAlbumRemoveBrackets: |{0}|", strAlbumRemoveBrackets);
          //logger.Debug("MusicInfoHandler: GetAlbumURL: strRemovePunctuation: |{0}|", strRemovePunctuation);

          bool albumFound = false;
          for (var m = AlbumURLRegEx.Match(discHTML); m.Success; m = m.NextMatch())
          {
            //TODO duplciate albums?

            var strFoundValue = m.Groups["albumName"].ToString().ToLower();
            var strFoundPunctuation = PunctuationRegex.Replace(strFoundValue, "");
            var strFoundAnd = strFoundValue.Replace("&", "and").Replace("+", "and");

            //logger.Debug("MusicInfoHandler: GetAlbumURL: strFoundValue: |{0}|", strFoundValue);
            //logger.Debug("MusicInfoHandler: GetAlbumURL: strFoundPunctuation: |{0}|", strFoundPunctuation);


            if (strFoundValue == strAlbum.ToLower())
            {
              albumFound = true;

              logger.Debug("MusicInfoHandler: GetAlbumURL: Matched album first time: {0}", strAlbum);

            }
            else if (strFoundValue == strStripStackEnding.ToLower())
            {
              albumFound = true;

              logger.Debug("MusicInfoHandler: GetAlbumURL: Matched album after stripping stack endings: {0}", strStripStackEnding);

            }
            else if (strFoundValue == strAlbumRemoveBrackets.ToLower())
            {
              albumFound = true;

              logger.Debug("MusicInfoHandler: GetAlbumURL: Matched album after stripping trailing brackets: {0}", strStripStackEnding);

            }
            else if (strFoundPunctuation == strRemovePunctuation.ToLower())
            {
              albumFound = true;

              logger.Debug("MusicInfoHandler: GetAlbumURL: Matched album after removing punctuation: {0}", strRemovePunctuation);

            }
            else if (strAndAlbum == strFoundAnd)
            {
              albumFound = true;

              logger.Debug("MusicInfoHandler: GetAlbumURL: Matched album after replacing and: {0}", strAndAlbum);

            }

            if (!albumFound) continue;
            strAlbumURL = m.Groups["albumURL"].ToString();
            break;
          }

          // return true if we have picked up a URL
          return !String.IsNullOrEmpty(strAlbumURL);
        }
      }
      catch (Exception ex)
      {
        logger.Error(ex);
        return false;
      }
    }

    /// <summary>
    /// Attempts to get a list of album URLs for the artist from allmusic.com
    /// </summary>
    /// <param name="strArtistURL">This is artist URL page</param>
    /// <returns>True if albums found</returns>
    private bool GetAlbumURLList(String strRedirect)
    {
      string discHTML;
      albumURLList.Clear();

      try
      {
        var strURL = strRedirect + "/discography/";

        var x = (HttpWebRequest)WebRequest.Create(strURL);

        if (_useProxy)
        {
          x.Proxy = new WebProxy(_proxyHost, _proxyPort);
        }

        x.ProtocolVersion = HttpVersion.Version10;
        x.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:6.0) Gecko/20100101 Firefox/6.0";
        x.ContentType = "text/html";
        x.Timeout = 30000;
        x.AllowAutoRedirect = false;

        using (var y = (HttpWebResponse)x.GetResponse())
        {
          using (var z = y.GetResponseStream())
          {
            if (z == null)
            {
              return false;
            }

            using (var sr = new StreamReader(z, Encoding.UTF8))
            {
              discHTML = sr.ReadToEnd();
            }

            z.Close();
            x.Abort();
            y.Close();
          }

          for (var m = AlbumURLRegEx.Match(discHTML); m.Success; m = m.NextMatch())
          {
            albumURLList.Add(m.Groups["albumURL"].ToString());
          }

          // return true if we have picked up a URL
          if (albumURLList.Count > 0)
            return true;
          else
            return false;
        }
      }
      catch (Exception ex)
      {
        logger.Error(ex);
        return false;
      }
    }


    /// <summary>
    /// Attempts to get the HTML from the artist page
    /// </summary>
    /// <param name="strArtist">Artist we are looking for</param>
    /// <param name="artistHTML">HTML of artist page</param>
    /// <returns>True if able to get HTML</returns>
    public bool GetArtistHTML(string strArtist, out String artistHTML, out String artistURL)
    {
      artistHTML = string.Empty;
      artistURL = string.Empty;

      try
      {
        String strRedirect;
        List<string> strArtistURLs = null;
        if (!GetArtistURL(strArtist, out strRedirect, out strArtistURLs))
        {
          return false;
        }
        artistURL = strRedirect;

        var x = (HttpWebRequest)WebRequest.Create(strRedirect);

        if (_useProxy)
        {
          x.Proxy = new WebProxy(_proxyHost, _proxyPort);
        }

        x.ProtocolVersion = HttpVersion.Version10;
        x.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:6.0) Gecko/20100101 Firefox/6.0";
        x.ContentType = "text/html";
        x.Timeout = 30000;
        x.AllowAutoRedirect = false;

        using (var y = (HttpWebResponse)x.GetResponse())
        {
          using (var z = y.GetResponseStream())
          {
            if (z == null)
            {
              x.Abort();
              y.Close();
              return false;
            }
            using (var sr = new StreamReader(z, Encoding.UTF8))
            {
              artistHTML = sr.ReadToEnd();
            }

            z.Close();
            x.Abort();
            y.Close();
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("Error retrieving artist data for: |{0}|", strArtist);
        logger.Error(ex);
        return false;
      }
      return true;
    }

    /// <summary>
    /// Attempts to get the HTML of album page
    /// </summary>
    /// <param name="strAlbumArtist">Album Artist we are looking for</param>
    /// <param name="strAlbum">Album we are looking for</param>
    /// <param name="albumHTML">HTML of album page</param>
    /// <returns>True if able to get HTML</returns>
    public bool GetAlbumHTML(string strAlbumArtist, string strAlbum, out string albumHTML)
    {
      albumHTML = string.Empty;
      try
      {
        String strRedirect;
        String strAlbumURL;
        List<string> strArtistURLs = null;
        if (!GetArtistURL(strAlbumArtist, out strRedirect, out strArtistURLs))
        {
          return false;
        }

        bool albumFound = false;

        var strURL = strRedirect + "/discography/";
        if (GetAlbumURL(strURL, strAlbum, out strAlbumURL))
          albumFound = true;
        else
        {
          strURL = strURL + "/compilations/";
          if (GetAlbumURL(strURL, strAlbum, out strAlbumURL))
            albumFound = true;
          else
            albumFound = false;
        }
        
        // If the album was not found and there are no additional artist URLs then exit
        if (!albumFound && strArtistURLs.Count == 0)
          return false;

        // We have not found the album but we do have additional artists we can check
        logger.Debug("Album details not found from Primary artist (1st in list), check artists 1 - 4 in the list");
        bool searchForAlbum = true;
        while (searchForAlbum && strArtistURLs.Count > 0)
        {
          logger.Debug("In While Loop");
          for (int i = 0; i < strArtistURLs.Count; i++)
          {
            logger.Debug("Checking {0} with URL {1}", i, strArtistURLs[i]);

            strURL = strArtistURLs[i] + "/discography/";
            if (GetAlbumURL(strURL, strAlbum, out strAlbumURL))
            {
              logger.Debug("Album found for artist URL : {0}", strURL);
              albumFound = true;
              searchForAlbum = false;
              break;
            }             
            else
            {
              strURL = strURL + "/compilations/";
              if (GetAlbumURL(strURL, strAlbum, out strAlbumURL))
              {
                logger.Debug("Album found for artist URL : {0}", strURL);
                albumFound = true;
                searchForAlbum = false;
                break;
              }
              else
              {
                albumFound = false;
                searchForAlbum = false;
              }
            }
          }
        }

        if (!albumFound)
          return false;


        logger.Debug("GetAlbumHTML: Album URL: {0}", strAlbumURL);


        var x = (HttpWebRequest)WebRequest.Create(strAlbumURL);

        if (_useProxy)
        {
          x.Proxy = new WebProxy(_proxyHost, _proxyPort);
        }

        using (var y = (HttpWebResponse)x.GetResponse())
        {
          using (var z = y.GetResponseStream())
          {
            if (z == null)
            {
              return false;
            }

            using (var sr = new StreamReader(z, Encoding.UTF8))
            {
              albumHTML = sr.ReadToEnd();
            }

            z.Close();
            x.Abort();
            y.Close();
          }
        }
        return true;
      }
      catch (Exception ex)
      {
        logger.Error("Error retrieving album data for: |{0}| - |{1}|", strAlbumArtist, strAlbum);
        logger.Error(ex);
      }
      return false;
    }

    /// <summary>
    /// Attempts to get the HTML of album page
    /// </summary>
    /// <param name="strAlbumArtist">Album Artist we are looking for</param>
    /// <param name="strAlbum">Album we are looking for</param>
    /// <param name="albumHTML">HTML of album page</param>
    /// <returns>True if able to get HTML</returns>
    public bool GetAlbumHTMLOnly(string strAlbumURL, out string albumHTML)
    {
      albumHTML = string.Empty;
      try
      {
        logger.Debug("GetAlbumHTML: Album URL: {0}", strAlbumURL);


        var x = (HttpWebRequest)WebRequest.Create(strAlbumURL);

        if (_useProxy)
        {
          x.Proxy = new WebProxy(_proxyHost, _proxyPort);
        }

        using (var y = (HttpWebResponse)x.GetResponse())
        {
          using (var z = y.GetResponseStream())
          {
            if (z == null)
            {
              return false;
            }

            using (var sr = new StreamReader(z, Encoding.UTF8))
            {
              albumHTML = sr.ReadToEnd();
            }

            z.Close();
            x.Abort();
            y.Close();
          }
        }
        return true;
      }
      catch (Exception ex)
      {
        logger.Error("Error retrieving album data for: |{0}|", strAlbumURL);
        logger.Error(ex);
      }
      return false;
    }


    /// <summary>
    /// Attempt to make string searching more helpful.   Removes all accents and puts in lower case
    /// Then escapes characters for use in URI
    /// </summary>
    /// <param name="strUnclean">String to be encoded</param>
    /// <returns>An encoded, cleansed string</returns>
    private string EncodeString(string strUnclean)
    {
      strUnclean = Regex.Replace(strUnclean, " {2,}", " ");
      var stFormD = strUnclean.Normalize(NormalizationForm.FormD);
      var sb = new StringBuilder();

      foreach (var t in from t in stFormD let uc = CharUnicodeInfo.GetUnicodeCategory(t) where uc != UnicodeCategory.NonSpacingMark select t)
      {
        sb.Append(t);
      }
      var strClean = Uri.EscapeDataString(sb.ToString().Normalize(NormalizationForm.FormC)).ToLower();

      return strClean;
    }
    /// <summary>
    /// Improve changes of matching artist by replacing & and + with "and"
    /// on both side of comparison
    /// Also remove "The"
    /// </summary>
    /// <param name="strArtist">artist we are searching for</param>
    /// <returns>Cleaned artist string</returns>
    private static string CleanArtist(string strArtist)
    {
      var strCleanArtist = strArtist.ToLower();
      strCleanArtist = strCleanArtist.Replace("&", "and");
      strCleanArtist = strCleanArtist.Replace("+", "and");
      //strCleanArtist = strCleanArtist.Replace(".", string.Empty);
      strCleanArtist = strCleanArtist.Replace("the ", string.Empty);
      return strCleanArtist;
    }

    #endregion

  }
}
