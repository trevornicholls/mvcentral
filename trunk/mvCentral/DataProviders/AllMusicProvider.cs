﻿using System;
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

    // NOTE: To other developers creating other applications, using this code as a base
    //       or as a reference. PLEASE get your own API key. Do not reuse the one listed here
    //       it is intended for Music Videos use ONLY. API keys are free and easy to apply
    //       for. Visit this url: http://http://www.allmusic.com/

    #region API variables


    private const string BaseURL = "http://www.allmusic.com/search/artist/";
    private const string AlbumRegExpPattern = @"<td\s*class=""cell""><a\s*href=""(?<albumURL>.*?)"">(?<albumName>.*)</a></td>";
    private static readonly Regex AlbumURLRegEx = new Regex(AlbumRegExpPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
    private const string ArtistRegExpPattern = @"<td><a href=""(?<artistURL>.*?)"">(?<artist>.*?)</a></td>\s*<td>(?<genres>.*?)</td>\s*<td>(?<years>.*?)</td>\s*</tr>";
    private static readonly Regex ArtistURLRegEx = new Regex(ArtistRegExpPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex BracketRegEx = new Regex(@"(.*)\(.+\)$", RegexOptions.Compiled);
    private static readonly Regex PunctuationRegex = new Regex("[?!.:;)()-_ ]", RegexOptions.Compiled);

    private static bool _strippedPrefixes;
    private static bool _logMissing;
    private static bool _useAlternative = true;
    private static bool _useProxy;
    private static string _proxyHost;
    private static int _proxyPort;

    #endregion

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

    public bool ProvidesDetails
    {
      get { return true; }
    }

    public bool ProvidesArtistArt
    {
      get { return true; }
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
    public bool GetAlbumArt(DBAlbumInfo mv)
    {
      return false;
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
    /// Get the Artist, Album Details
    /// </summary>
    /// <param name="mv"></param>
    /// <returns></returns>
    public bool GetDetails(DBBasicInfo mv)
    {
      string strArtistHTML;
      string strAlbumHTML;

      // Get details of the artist
      if (mv.GetType() == typeof(DBArtistInfo))
      {
        string artist = ((DBArtistInfo)mv).Artist;
        if (GetArtistHTML(artist, out strArtistHTML))
        {
          var artistInfo = new MusicArtistInfo();
          if (artistInfo.Parse(strArtistHTML))
          {
            artistInfo.Artist = artist;
            DBArtistInfo mv1 = (DBArtistInfo)mv;
            setMusicVideoArtist(ref mv1, artistInfo);
            //GetArtistArt((DBArtistInfo)mv);
          }
        }
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
            //GetArtistArt((DBArtistInfo)mv);
          }
        }
      }


      return true;
    }

    public List<DBTrackInfo> Get(MusicVideoSignature mvSignature)
    {
      List<DBTrackInfo> results = new List<DBTrackInfo>();
      if (mvSignature == null)
        return results;
      lock (lockList)
      {
        DBTrackInfo mv = getMusicVideoTrack(mvSignature.Artist, mvSignature.Album, mvSignature.Track);
        if (mv != null)
        {
          if (mv.ArtistInfo.Count == 0)
          {
            DBArtistInfo d4 = new DBArtistInfo();
            d4.Artist = mvSignature.Artist;
            mv.ArtistInfo.Add(d4);
          }
          results.Add(mv);
        }
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
    /// Set the Artist information
    /// </summary>
    /// <param name="mv"></param>
    /// <param name="artistInfo"></param>
    private void setMusicVideoArtist(ref DBArtistInfo mv, MusicArtistInfo artistInfo)
    {
      mv.bioSummary = getBioSummary(artistInfo.AMGBiography, 50);
      mv.bioContent = artistInfo.AMGBiography;
      mv.Genre = artistInfo.Genres;
      
    }
    /// <summary>
    /// Set the Album Information
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

    public string getBioSummary(string text, int maxWordCount)
    {
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

    private void setMusicVideoAlbum(ref DBAlbumInfo mv, XmlNode node)
    {
    }

    public UpdateResults Update(DBTrackInfo mv)
    {
      return UpdateResults.SUCCESS;
    }

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
    private bool GetArtistURL(String strArtist, out String strArtistURL)
    {
      strArtistURL = string.Empty;
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
            var altTry = GetArtistURLAlternative(strArtist, out strArtistURL);
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
    private bool GetArtistURLAlternative(String strArtist, out String strArtistURL)
    {
      strArtistURL = string.Empty;
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

            //            var artistURLRegExp = new Regex(@"<td><a href=""(?<artistURL>.*?)"">" + Regex.Escape(strCleanArtist) +
            //            var artistURLRegExp = new Regex(@"<td><a href=""(?<artistURL>.*?)"">(?<artist>.*?)</a></td>\s*<td>(?<genres>.*?)</td>\s*<td>(?<years>.*?)</td>\s*</tr>",
            //              RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

            var matches = ArtistURLRegEx.Matches(artistHTML);
            var numberOfMatchesWithYears = 0;
            //            var matchIndex = -1; // this is index in collection to use.   Will only be used if matched and incremented

            //            //some cases like Björk where there is only a single entry matching artist in list but stil returns list rather
            //            //then redirecting to artist page
            //           if (matches.Count == 1 &&  )
            //            {
            //              strArtistURL = matches[0].Groups["artistURL"].ToString();
            //#if DEBUG
            //              logger.Debug("GetArtistURLAlternative: Single match on artist screen: strArtistURL: {0}", strArtistURL);
            //#endif
            //              return true;
            //            }


            var strPotentialURL = string.Empty;
            var strCleanArtist = EncodeString(CleanArtist(strArtist));

            logger.Debug("GetArtistURLAlternative: Cleaned/Encoded Artist: |{0}|", strCleanArtist);

            // else there are either 0 or multiple matches so lets see how many have years populated
            foreach (Match m in matches)
            {
              var strCleanMatch = EncodeString(CleanArtist(m.Groups["artist"].ToString()));

              logger.Debug("GetArtistURLAlternative: Cleaned/Encoded matched Artist: |{0}|", strCleanMatch);


              if (strCleanArtist != strCleanMatch) continue;

              logger.Debug("GetArtistURLAlternative: Years: {0}", m.Groups["years"].ToString().Trim());

              if (string.IsNullOrEmpty(m.Groups["years"].ToString().Trim())) continue;

              //              matchIndex++;
              strPotentialURL = m.Groups["artistURL"].ToString();
              numberOfMatchesWithYears++;

              // give up if more than one match with years active
              if (numberOfMatchesWithYears > 1)
              {

                logger.Debug("GetArtistURLAlternative: Multiple matches with years active");

                return false;
              }
            }

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


          logger.Debug("MusicInfoHandler: GetAlbumURL: strAlbum: |{0}|", strAlbum);
          logger.Debug("MusicInfoHandler: GetAlbumURL: strStripStackEnding: |{0}|", strStripStackEnding);
          logger.Debug("MusicInfoHandler: GetAlbumURL: strAlbumRemoveBrackets: |{0}|", strAlbumRemoveBrackets);
          logger.Debug("MusicInfoHandler: GetAlbumURL: strRemovePunctuation: |{0}|", strRemovePunctuation);

          bool albumFound = false;
          for (var m = AlbumURLRegEx.Match(discHTML); m.Success; m = m.NextMatch())
          {
            //TODO duplciate albums?

            var strFoundValue = m.Groups["albumName"].ToString().ToLower();
            var strFoundPunctuation = PunctuationRegex.Replace(strFoundValue, "");
            var strFoundAnd = strFoundValue.Replace("&", "and").Replace("+", "and");

            logger.Debug("MusicInfoHandler: GetAlbumURL: strFoundValue: |{0}|", strFoundValue);
            logger.Debug("MusicInfoHandler: GetAlbumURL: strFoundPunctuation: |{0}|", strFoundPunctuation);


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
    /// Attempts to get the HTML from the artist page
    /// </summary>
    /// <param name="strArtist">Artist we are looking for</param>
    /// <param name="artistHTML">HTML of artist page</param>
    /// <returns>True if able to get HTML</returns>
    public bool GetArtistHTML(string strArtist, out string artistHTML)
    {
      artistHTML = string.Empty;
      try
      {
        String strRedirect;
        if (!GetArtistURL(strArtist, out strRedirect))
        {
          return false;
        }

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
        if (!GetArtistURL(strAlbumArtist, out strRedirect))
        {
          return false;
        }

        var strURL = strRedirect + "/discography/";
        if (!GetAlbumURL(strURL, strAlbum, out strAlbumURL))
        {
          strURL = strURL + "/compilations/";
          if (!GetAlbumURL(strURL, strAlbum, out strAlbumURL))
          {
            return false;
          }
        }


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
    /// Attempt to make string searching more helpful.   Removes all accents and puts in lower case
    /// Then escapes characters for use in URI
    /// </summary>
    /// <param name="strUnclean">String to be encoded</param>
    /// <returns>An encoded, cleansed string</returns>
    private string EncodeString(string strUnclean)
    {
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
      strCleanArtist = strCleanArtist.Replace("&", "and").Replace("+", "and").TrimStart("the ".ToCharArray());
      return strCleanArtist;
    }

    #endregion

  }
}