using MediaPortal.Music.Database;

using mvCentral.Database;
using mvCentral.LocalMediaManagement.MusicVideoResources;
using mvCentral.SignatureBuilders;
using mvCentral.Utils;

using NLog;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;


namespace mvCentral.DataProviders
{

  class AllMusicProvider : InternalProvider, IMusicVideoProvider
  {
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #region Provider variables

    private const string BaseURL = "http://www.allmusic.com";
    private const string SearchURL = BaseURL + "/search/artists/";

    // **** Artist Regex ****
    //private const string ArtistRegExpPattern = @"<tr class=""search-result artist"">.*?<div class=""name"">\s*<a href=""(?<artistURL>.*?)"".*?>(?<artist>.*?)</a>\s*</div>\s*<div class=""info"">\s*(?<genres>.*?)\s*<br/>\s*(?<years>.*?)\s*</div>";

    private const string ArtistRegExpPattern = @"<div class=""name"">\s*<a href=""(?<artistURL>.*?)"".*?>(?<artist>.*?)<\/a>\s*<\/div>\s*<div class=""genres"">\s*(?<genres>.*?)<\/div>\s*<div class=""decades"">\s*(?<years>.*?)<\/div>";
    
    private static readonly Regex ArtistUrlRegEx = new Regex(ArtistRegExpPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    // **** Album Regex ****
    // private const string AlbumRegExpPattern = @"<td class=""title primary_link"".*?<a href=""(?<albumURL>.*?)"" class=""title.*?"" data-tooltip="".*?"">(?<albumName>.*?)</a>";
    private const string AlbumRegExpPattern = @"<td.class=.cover.>[^<]*?<a.href=""(?<albumURL>.*?)""[^<]*?title=""(?<albumName>.*?)""";
    private static readonly Regex AlbumUrlRegEx = new Regex(AlbumRegExpPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    // Some clean up regex
    private static readonly Regex BracketRegEx = new Regex(@"\s*[\(\[\{].*?[\]\)\}]\s*", RegexOptions.Compiled);
    private static readonly Regex PunctuationRegex = new Regex(@"[^\w\s]", RegexOptions.Compiled);

    private Match _match = null;
    readonly List<string> _albumUrlList = new List<string>();

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
      get { return false; } // Disabled Currently due to Allmusic site changes
    }

    public bool ProvidesAlbumDetails
    {
      get { return false; }  // Disabled Currently due to Allmusic site changes
    }

    public bool ProvidesArtistArt
    {
      get { return false; }
    }

    public bool ProvidesAlbumArt
    {
      get { return false; } // Disabled Currently due to Allmusic site changes
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
      string strArtistHtml;
      string strArtistUrl;

      // Get details of the artist
      string artist = mv.ArtistInfo[0].Artist;
      if (GetArtistHTML(artist, out strArtistHtml, out strArtistUrl))
      {
        var artistInfo = AMGHTMLParser.ParseArtistHTML(strArtistHtml, artist);
        //var artistInfo = new MusicArtistInfo();
        if (artistInfo != null)
        {
          artistInfo.Artist = artist;
          DBArtistInfo mv1 = (DBArtistInfo)mv.ArtistInfo[0];
          UpdateMusicVideoArtist(ref mv1, artistInfo, strArtistHtml);
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
        //var albumInfo = new MusicAlbumInfo();
        //if (albumInfo.Parse(strAlbumHTML))
        var albumInfo = AMGHTMLParser.ParseAlbumHTML(strAlbumHTML, album, artist);
        if (albumInfo != null)
        {
          albumInfo.Artist = album;
          DBAlbumInfo mv1 = (DBAlbumInfo)mv.AlbumInfo[0];
          SetMusicVideoAlbum(ref mv1, albumInfo);
          getTrackComposers(mv, strAlbumHTML, string.Empty);
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
      Logger.Debug("In Method : GetAlbumArt(DBAlbumInfo mv)");

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
      {
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

      ReportProgress(string.Empty);

      // Get details of the artist
      if (mv.GetType() == typeof(DBArtistInfo))
      {
        string artist = ((DBArtistInfo)mv).Artist;
        ReportProgress("Getting Artist...");
        if (GetArtistHTML(artist, out strArtistHTML, out strArtistURL))
        {
          var artistInfo = AMGHTMLParser.ParseArtistHTML(strArtistHTML, artist);

          if (artistInfo != null)
          {
            artistInfo.Artist = artist;
            DBArtistInfo mv1 = (DBArtistInfo)mv;
            SetMusicVideoArtist(ref mv1, artistInfo, strArtistHTML);
            GetArtistArt((DBArtistInfo)mv);
            ReportProgress("Done...");
            return true;
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
          var albumInfo = AMGHTMLParser.ParseAlbumHTML(strAlbumHTML, album, artist.Artist);
          if (albumInfo != null)
          {
            albumInfo.Artist = album;
            DBAlbumInfo mv1 = (DBAlbumInfo)mv;
            SetMusicVideoAlbum(ref mv1, albumInfo);
          }
        }
        return false;
      }

      // get details of the track
      if (mv.GetType() == typeof(DBTrackInfo))
      {
        if (((DBTrackInfo)mv).LocalMedia[0].IsDVD)
        {
          string track = ((DBTrackInfo)mv).Track;
          GetDVDDetails((DBTrackInfo)mv);
          return true;
        }
        else
        {
          string track = ((DBTrackInfo)mv).Track;
          GetTrackDetails((DBTrackInfo)mv);
          return true;
        }
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
      List<DBTrackInfo> results = new List<DBTrackInfo>();
      return results;
    }
    /// <summary>
    /// Get the details of the track
    /// </summary>
    /// <param name="trackObject"></param>
    private void GetTrackDetails(DBTrackInfo trackObject)
    {
      string strArtistHTML;
      string strAlbumHTML;
      string strArtistURL;
      bool songFound = false;
      const int trackName = 1;
      const int trackURL = 3;

      List<DBTrackInfo> results = new List<DBTrackInfo>();

      string strAlbumArtist = trackObject.ArtistInfo[0].Artist;

      if (GetArtistHTML(strAlbumArtist, out strArtistHTML, out strArtistURL))
      {
        var artistInfo = new MusicArtistInfo();
        if (artistInfo.Parse(strArtistHTML))
        {
          artistInfo.Artist = strAlbumArtist;
          if (GetAlbumURLList(strArtistURL))
          {
            // we have some albums - now check the tracks in each album
            foreach (string albumURL in _albumUrlList)
            {
              // If we found the song then exit...
              if (songFound)
                break;

              if (GetPageHTMLOnly(albumURL, out strAlbumHTML))
              {
                string strAlbum = Regex.Match(albumURL, "[^/]+-", RegexOptions.Singleline | RegexOptions.IgnoreCase).Value;
                strAlbum = Regex.Replace(strAlbum, "[-?*]", " ", RegexOptions.Singleline | RegexOptions.IgnoreCase);

                var albumInfo = AMGHTMLParser.ParseAlbumHTML(strAlbumHTML, strAlbum.Trim(), strAlbumArtist);

                if (albumInfo != null)
                {
                  string[] tracksOnAlbum = albumInfo.Tracks.Split('|');
                  foreach (string track in tracksOnAlbum)
                  {
                    if (!string.IsNullOrEmpty(track.Trim()))
                    {
                      string[] trackData = track.Split('@');

                      if (string.Equals(trackObject.Track, trackData[trackName], StringComparison.CurrentCultureIgnoreCase))
                      {
                        Logger.Debug("Get Composers for Track {0} by {1}", trackObject.Track, strAlbumArtist);
                        songFound = getTrackComposers(trackObject, strAlbumHTML, trackData[trackURL]);
                        Logger.Debug("Composers for Track {0} by {1} are {2}", trackObject.Track, strAlbumArtist, trackObject.Composers);
                        break;
                      }
                    }
                  }
                }
              }
            }
          }
        }
      }
    }

    /// <summary>
    /// Get the details of the track
    /// </summary>
    /// <param name="trackObject"></param>
    private void GetDVDDetails(DBTrackInfo trackObject)
    {
      string strArtistHTML;
      string strAlbumHTML;
      string strArtistURL;
      bool songFound = false;

      List<DBTrackInfo> results = new List<DBTrackInfo>();

      string artist = trackObject.ArtistInfo[0].Artist;

      if (GetArtistHTML(artist, out strArtistHTML, out strArtistURL))
      {
        var artistInfo = new MusicArtistInfo();
        if (artistInfo.Parse(strArtistHTML))
        {
          artistInfo.Artist = artist;
          if (GetDVDURLList(strArtistURL))
          {
            // we have some albums - now check the tracks in each album
            foreach (string albumURL in _albumUrlList)
            {
              if (GetPageHTMLOnly(albumURL, out strAlbumHTML))
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
                      if (trackObject.Track == trackData[1])
                      {
                        songFound = getTrackComposers(trackObject, strAlbumHTML, trackData[3]);
                        break;
                      }
                    }
                  }
                }
              }
            }
          }
        }
      }
    }


    /// <summary>
    /// Get the Rating for the track
    /// </summary>
    /// <param name="trackObject"></param>
    /// <param name="albumHTML"></param>
    /// <returns></returns>
    bool getTrackComposers(DBTrackInfo trackObject, string albumHTML, string trackURL)
    {
      string strSongHTML = string.Empty;
      string songComposers = string.Empty; ;

      var strAlbumRemoveBrackets = BracketRegEx.Replace(trackObject.Track, "$1").Trim();
      var strRemovePunctuation = PunctuationRegex.Replace(trackObject.Track, "").Trim();
      var strAndAlbum = trackObject.Track.Replace("&", "and").Replace("+", "and");

      // Extract the composers first
      string trackHTML = string.Empty;
      GetPageHTMLOnly(trackURL, out trackHTML);
      MatchCollection allMatchResults = null;
      CaptureCollection captureCollection;
      GroupCollection groupCollection;
      try
      {
        Regex composerRegion = new Regex(@"Composed by(?<composers>.+?)<\/h3>", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);
        Match match = composerRegion.Match(trackHTML);
        if (match.Success)
      	{
   	      string _composers = match.Groups[1].Value;
          Regex trackRegex = new Regex(@"<a.href=[^>]+?>(?<composer>[^<]+)<\/a>", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);
          allMatchResults = trackRegex.Matches(_composers);
          foreach (Match cmatch in allMatchResults)
          {
            groupCollection = cmatch.Groups;
            captureCollection = groupCollection[1].Captures;
            for (int i = 0; i < captureCollection.Count; i++)
              songComposers += (i == captureCollection.Count - 1) ? captureCollection[i].Value : captureCollection[i].Value + "|";
          }
       	}
        trackObject.Composers = songComposers;
        return true;
      }
      catch (Exception)
      { } 

      return false;
    }

    /// <summary>
    /// Get the album details 
    /// </summary>
    /// <param name="basicInfo"></param>
    /// <param name="albumTitle"></param>
    /// <param name="albumMbid"></param>
    /// <returns></returns>
    public bool GetAlbumDetails(DBBasicInfo basicInfo, string albumTitle, string albumMbid)
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
    private void SetMusicVideoAlbum(ref DBAlbumInfo mv, AlbumInfo albumInfo)
    {
      mv.bioSummary = getBioSummary(albumInfo.Review, 50);
      mv.bioContent = albumInfo.Review;
      mv.YearReleased = albumInfo.Year.ToString();
      mv.Rating = albumInfo.Rating;
    }

    /// <summary>
    /// Set the Artist information, override existing information
    /// </summary>
    /// <param name="mv"></param>
    /// <param name="artistInfo"></param>
    private void SetMusicVideoArtist(ref DBArtistInfo mv, ArtistInfo artistInfo, string strArtistHTML)
    {
      // Now fill in the data
      mv.Formed = mvCentralUtils.StripHTML(artistInfo.Formed);
      mv.Disbanded = mvCentralUtils.StripHTML(artistInfo.Disbanded);
      mv.Born = mvCentralUtils.StripHTML(artistInfo.Born);
      mv.Death = mvCentralUtils.StripHTML(artistInfo.Death);
      mv.bioSummary = getBioSummary(artistInfo.AMGBio, 50);
      mv.bioContent = artistInfo.AMGBio;
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
    private void UpdateMusicVideoArtist(ref DBArtistInfo mv, ArtistInfo artistInfo, string strArtistHTML)
    {
      if (mv.Formed.Trim() == string.Empty)
        mv.Formed = mvCentralUtils.StripHTML(artistInfo.Formed);

      if (mv.Disbanded.Trim() == string.Empty)
        mv.Disbanded = mvCentralUtils.StripHTML(artistInfo.Disbanded);

      if (mv.Born.Trim() == string.Empty)
        mv.Born = mvCentralUtils.StripHTML(artistInfo.Born);

      if (mv.Death.Trim() == string.Empty)
        mv.Death = mvCentralUtils.StripHTML(artistInfo.Death);

      if (mv.bioSummary.Trim() == string.Empty)
        mv.bioSummary = getBioSummary(artistInfo.AMGBio, 50);

      if (mv.bioContent.Trim() == string.Empty)
        mv.bioContent = artistInfo.AMGBio;

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
      Regex itemsFound = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

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
      bool Classical = false;
      try
      {
        var strEncodedArtist = EncodeString(strArtist);
        var strURL = SearchURL + strEncodedArtist;

        Logger.Debug("GetArtistURL: Request URL: {0}", strURL);

        var x = (HttpWebRequest)WebRequest.Create(strURL);

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

            var matches = ArtistUrlRegEx.Matches(artistHTML);
            var numberOfMatchesWithYears = 0;

            var strPotentialURL = string.Empty;
            var strCleanArtist = EncodeString(CleanArtist(strArtist));
            Logger.Debug("GetArtistURL: Cleaned/Encoded Artist: |{0}|", strCleanArtist);
            // else there are either 0 or multiple matches so lets see how many have years populated
            foreach (Match m in matches)
            {
              var strCleanMatch = EncodeString(CleanArtist(m.Groups["artist"].ToString()));
              Logger.Debug("GetArtistURL: Cleaned/Encoded matched Artist: |{0}|", strCleanMatch);

              if (strCleanArtist != strCleanMatch) 
                continue;

              // Skip the years check if this is Classical
              if (m.Groups["genres"].ToString().ToLower().Contains("classical"))
                Classical = true;


              Logger.Debug("GetArtistURL: Years: {0}", m.Groups["years"].ToString().Trim());
              if (string.IsNullOrEmpty(m.Groups["years"].ToString().Trim()) && !Classical)
                continue;


              numberOfMatchesWithYears++;

              //give up if more than one match with years active
              if (numberOfMatchesWithYears > 1)
              {
                Logger.Debug("GetArtistURL: Multiple matches with years active");
                break;
              }
              strPotentialURL = m.Groups["artistURL"].ToString();
            }

 
            if (numberOfMatchesWithYears == 0 && !Classical)
            {
              Logger.Debug("GetArtistURL: No matches with years active");
              return false;
            }

            // only one match with years active so return URL for that artist.
            strArtistURL = strPotentialURL;
            Logger.Debug("GetArtistURL: Single match on artist screen with years populated: strArtistURL: {0}", strArtistURL);

            return true;
          }
        }



      }
      catch (Exception ex)
      {
        Logger.Error(ex);
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
      if (!strArtistURL.StartsWith("http"))
      {
        Logger.Debug("Invalid Artist URL {0}- exit routine",strArtistURL);
        strAlbumURL = string.Empty;
        return false;
      }
      strAlbumURL = string.Empty;
      string discHTML;
      try
      {
        var x = (HttpWebRequest)WebRequest.Create(strArtistURL);

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

          bool albumFound = false;
          for (var m = AlbumUrlRegEx.Match(discHTML); m.Success; m = m.NextMatch())
          {
            //TODO duplciate albums?

            var strFoundValue = m.Groups["albumName"].ToString().ToLower();
            var strFoundPunctuation = PunctuationRegex.Replace(strFoundValue, "");
            var strFoundAnd = strFoundValue.Replace("&", "and").Replace("+", "and");

            if (strFoundValue == strAlbum.ToLower())
            {
              albumFound = true;
              Logger.Debug("AllMusic: GetAlbumURL: Matched album first time: {0}", strAlbum);
            }
            else if (strFoundValue == strStripStackEnding.ToLower())
            {
              albumFound = true;
              Logger.Debug("AllMusic: GetAlbumURL: Matched album after stripping stack endings: {0}", strStripStackEnding);
            }
            else if (strFoundValue == strAlbumRemoveBrackets.ToLower())
            {
              albumFound = true;
              Logger.Debug("AllMusic: GetAlbumURL: Matched album after stripping trailing brackets: {0}", strStripStackEnding);
            }
            else if (strFoundPunctuation == strRemovePunctuation.ToLower())
            {
              albumFound = true;
              Logger.Debug("AllMusic: GetAlbumURL: Matched album after removing punctuation: {0}", strRemovePunctuation);
            }
            else if (strAndAlbum == strFoundAnd)
            {
              albumFound = true;
              Logger.Debug("AllMusic: GetAlbumURL: Matched album after replacing and: {0}", strAndAlbum);
            }

            if (!albumFound) continue;
            strAlbumURL = m.Groups["albumURL"].ToString();
            if (!String.IsNullOrEmpty(strAlbumURL) && !strAlbumURL.StartsWith("http"))
            {
              strAlbumURL = BaseURL + strAlbumURL;
            }
            break;
          }

          // return true if we have picked up a URL
          return !String.IsNullOrEmpty(strAlbumURL);
        }
      }
      catch (Exception ex)
      {
        Logger.Error(ex);
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
      _albumUrlList.Clear();

      try
      {
        var strURL = strRedirect + "/discography";

        var x = (HttpWebRequest)WebRequest.Create(strURL);

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

          for (var m = AlbumUrlRegEx.Match(discHTML); m.Success; m = m.NextMatch())
          {
            string strAlbumURL = m.Groups["albumURL"].ToString();
            if (!String.IsNullOrEmpty(strAlbumURL))
            {
              if (!strAlbumURL.StartsWith("http"))
              {
                strAlbumURL = BaseURL + strAlbumURL;
              }
              _albumUrlList.Add(strAlbumURL);
            }
          }

          // return true if we have picked up a URL
          if (_albumUrlList.Count > 0)
            return true;
          else
            return false;
        }
      }
      catch (Exception ex)
      {
        Logger.Error(ex);
        return false;
      }
    }

    /// <summary>
    /// Attempts to get a list of album URLs for the artist from allmusic.com
    /// </summary>
    /// <param name="strArtistURL">This is artist URL page</param>
    /// <returns>True if albums found</returns>
    private bool GetDVDURLList(String strRedirect)
    {
      string discHTML;
      _albumUrlList.Clear();

      try
      {
        var strURL = strRedirect + "/discography/video/";

        var x = (HttpWebRequest)WebRequest.Create(strURL);

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

          for (var m = AlbumUrlRegEx.Match(discHTML); m.Success; m = m.NextMatch())
          {
            string strAlbumURL = m.Groups["albumURL"].ToString();
            if (!String.IsNullOrEmpty(strAlbumURL))
            {
              if (!strAlbumURL.StartsWith("http"))
              {
                strAlbumURL = BaseURL + strAlbumURL;
              }
              _albumUrlList.Add(strAlbumURL);
            }
          }

          // return true if we have picked up a URL
          if (_albumUrlList.Count > 0)
            return true;
          else
            return false;
        }
      }
      catch (Exception ex)
      {
        Logger.Error(ex);
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
        Logger.Error("Error retrieving artist data for: |{0}|", strArtist);
        Logger.Error(ex);
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

        var strURL = strRedirect + "/discography/";
        if (!GetAlbumURL(strURL, strAlbum, out strAlbumURL))
        {
          strURL = strRedirect + "/discography/compilations/";
          if (!GetAlbumURL(strURL, strAlbum, out strAlbumURL))
          {
            return false;
          }
        }
        Logger.Debug("GetAlbumHTML: Album URL: {0}", strAlbumURL);

        var x = (HttpWebRequest)WebRequest.Create(strAlbumURL);

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
        Logger.Error("Error retrieving album data for: |{0}| - |{1}|", strAlbumArtist, strAlbum);
        Logger.Error(ex);
      }
      return false;
    }

    /// <summary>
    /// Attempts to get the HTML of page
    /// </summary>
    /// <param name="strURL">URL of the page to get</param>
    /// <param name="pageHTML">Returned HTML of page</param>
    /// <returns>True if able to get HTML</returns>
    public bool GetPageHTMLOnly(string strURL, out string pageHTML)
    {
      pageHTML = string.Empty;
      try
      {
        Logger.Debug("GetPageHTMLOnly: URL: {0}", strURL);

        var x = (HttpWebRequest)WebRequest.Create(strURL);
        using (var y = (HttpWebResponse)x.GetResponse())
        {
          using (var z = y.GetResponseStream())
          {
            if (z == null)
              return false;

            using (var sr = new StreamReader(z, Encoding.UTF8))
              pageHTML = sr.ReadToEnd();

            z.Close();
            x.Abort();
            y.Close();
          }
        }
        return true;
      }
      catch (Exception ex)
      {
        Logger.Error("Error retrieving HTML data for: |{0}|", strURL);
        Logger.Error(ex);
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
      strCleanArtist = Regex.Replace(strCleanArtist, "^the ", "", RegexOptions.IgnoreCase);

      return strCleanArtist;
    }

    public event EventHandler ProgressChanged;
    private void ReportProgress(string text)
    {
      if (ProgressChanged != null)
      {
        ProgressChanged(this, new ProgressEventArgs { Text = "AllMusic: " + text });
      }
    }
    #endregion

  }
}
