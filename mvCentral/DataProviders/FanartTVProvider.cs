using MediaPortal.Music.Database;
using mvCentral.Database;
using mvCentral.LocalMediaManagement;
using mvCentral.LocalMediaManagement.MusicVideoResources;
using mvCentral.SignatureBuilders;
using mvCentral.Utils;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;


namespace mvCentral.DataProviders
{

  class FanartTVProvider : IMusicVideoProvider
  {
    private static Logger logger = LogManager.GetCurrentClassLogger();
    private static readonly object lockList = new object();
    
    public event EventHandler ProgressChanged;

    // NOTE: To other developers creating other applications, using this code as a base
    //       or as a reference. PLEASE get your own API key. Do not reuse the one listed here
    //       it is intended for Music Videos use ONLY. API keys are free and easy to apply
    //       for. Visit this url: https://fanart.tv/get-an-api-key/

    #region API variables

    // Example Calls
    // http://webservice.fanart.tv/v3/music/f4a31f0a-51dd-4fa7-986d-3095c40c5ed9?api_key=...&client_key=...
    // http://webservice.fanart.tv/v3/music/albums/9ba659df-5814-32f6-b95f-02b738698e7c?api_key=...&client_key=...

    private const string APIKey = "9dc9aec601b49ca7a9899186161f57bd";

    private const string SearchArtistImage = "http://webservice.fanart.tv/v3/music/{1}?api_key={0}";
    private const string SearchAlbumImage = "http://webservice.fanart.tv/v3/music/albums/{1}?api_key={0}";

    #endregion

    public string Name
    {
      get
      {
        return "www.fanart.tv";
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
      get { return "Returns art from fanart.tv"; }
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
      get { return false; }
    }

    public bool ProvidesAlbumDetails
    {
      get { return false; }
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


    public DBTrackInfo GetArtistDetail(DBTrackInfo mv)
    {
      throw new NotImplementedException();
    }

    public DBTrackInfo GetAlbumDetail(DBTrackInfo mv)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Get the track details
    /// </summary>
    /// <param name="mvSignature"></param>
    /// <returns></returns>
    public List<DBTrackInfo> GetTrackDetail(MusicVideoSignature mvSignature)
    {
      logger.Debug("In Method: GetTrackDetail(MusicVideoSignature mvSignature)");
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
    /// Get Artist Artwork
    /// </summary>
    /// <param name="mvArtistObject"></param>
    /// <returns></returns>
    public bool GetArtistArt(DBArtistInfo mvArtistObject)
    {
      if (string.IsNullOrEmpty(mvArtistObject.MdID.Trim()))
        return false;
      logger.Debug("In Method: GetArtistArt(DBArtistInfo mvArtistObject)");

      // Get the images
      // If we have a MBID for this Artist use this to retrive the image otherwise fall back to keyword search
      var html = getJSON(SearchArtistImage, APIKey, mvArtistObject.MdID.Trim());

      // If we reveived nothing back, bail out
      if (string.IsNullOrEmpty(html))
        return false;

      int artistartAdded = 0;
      var URLList = new List<string>();
      URLList = ExtractURL("artistthumb", html);
      if (URLList != null)
      {
        foreach (string _url in URLList)
        {
          var _fileURL = _url.Substring(checked(_url.IndexOf("|") + 1));
          if (mvArtistObject.AddArtFromURL(_fileURL) == ImageLoadResults.SUCCESS)
            artistartAdded++;
        }
      }

      return (artistartAdded > 0);
    }

    /// <summary>
    /// Get the Album Art
    /// </summary>
    /// <param name="mv"></param>
    /// <returns></returns>
    public bool GetAlbumArt(DBAlbumInfo mv)
    {
      if (string.IsNullOrEmpty(mv.MdID.Trim()))
        return false;
      logger.Debug("In Method: GetAlbumArt(DBAlbumInfo mv)");

      // Get the images
      // If we have a MBID for this Artist use this to retrive the image otherwise fall back to keyword search
      var html = getJSON(SearchAlbumImage, APIKey, mv.MdID.Trim());

      // If we reveived nothing back, bail out
      if (string.IsNullOrEmpty(html))
        return false;

      int albumartAdded = 0;
      var URLList = new List<string>();
      URLList = ExtractURL("albumcover", html);
      if (URLList != null)
      {
        foreach (string _url in URLList)
        {
          var _fileURL = _url.Substring(checked(_url.IndexOf("|") + 1));
          if (mv.AddArtFromURL(_fileURL) == ImageLoadResults.SUCCESS)
            albumartAdded++;
        }
      }

      return (albumartAdded > 0);
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
    /// Set the Artist information
    /// </summary>
    /// <param name="mv"></param>
    /// <param name="artistInfo"></param>
    private void setMusicVideoArtist(ref DBArtistInfo mv, MusicArtistInfo artistInfo)
    {
    }

    /// <summary>
    /// Set the Album Information
    /// </summary>
    /// <param name="mv"></param>
    /// <param name="albumInfo"></param>
    private void setMusicVideoAlbum(ref DBAlbumInfo mv, MusicAlbumInfo albumInfo)
    {
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

    public UpdateResults UpdateTrack(DBTrackInfo mv)
    {
      return UpdateResults.SUCCESS;
    }

    #region URL and HTTP Handling

    // calls the getXMLFromURL but the URL is formatted using
    // the baseString with the given parameters escaped them to be usable on URLs.
    private static string getJSON(string baseString, params object[] parameters)
    {
        for (int i = 0; i < parameters.Length; i++)
        {
            parameters[i] = Uri.EscapeDataString((string)parameters[i]);
        }

        return getJSONFromURL(string.Format(baseString, parameters));
    }

    // given a url, retrieves the xml result set and returns the nodelist of Item objects
    private static string getJSONFromURL(string url)
    {
      // Add Fanart.TV personal API Key
      if (!string.IsNullOrWhiteSpace(mvCentralCore.Settings.FanartTVPersonalAPIkey))
      {
        url = url + "&client_key=" + mvCentralCore.Settings.FanartTVPersonalAPIkey;
      }

      logger.Debug("Sending the request: " + url.Replace(APIKey, "<apiKey>").Replace(mvCentralCore.Settings.FanartTVPersonalAPIkey,"<personalAPIkey>"));

      mvWebGrabber grabber = Utility.GetWebGrabberInstance(url);
      grabber.Encoding = Encoding.UTF8;
      grabber.Timeout = 10000;
      grabber.TimeoutIncrement = 1000;
      if (grabber.GetResponse(APIKey))
      {
        return grabber.GetString();
      }
      else
      {
        logger.Debug("***** API ERROR *****: Code:{0} ({1})", grabber.errorCode, grabber.errorText);
        return null;
      }
    }

    #endregion

    private List<string> ExtractURLLang(string Sec, string AInputString, string Lang, bool LangIndep = true)
    {
      const string SECRE = @"\""%1.+?\[([^\]]+?)\]";
      const string URLRE = @"\""id.\:[^}]*?\""([^}]+?)\""[^}]+?url.\:[^}]*?\""([^}]+?)\""([^}]+?lang.\:[^}]*?\""(%1)\"")"; // Id URL

      var B = (string)null;
      var URLList = new List<string>();
      var L = (string)null;

      if (string.IsNullOrEmpty(AInputString) || (AInputString == "null"))
        return URLList;

      L = (string.IsNullOrEmpty(Lang) ? "Any" : ("en".Equals(Lang, StringComparison.CurrentCulture) ? Lang : Lang + "/" + "en"));
      L = (LangIndep ? string.Empty : L);

      Regex r = new Regex(SECRE.Replace("%1", Sec), RegexOptions.IgnoreCase);
      MatchCollection mc = r.Matches(AInputString);
      foreach (Match m in mc)
      {
        B = m.Value;
        break;
      }

      if (!string.IsNullOrWhiteSpace(B))
      {
        Regex ru = new Regex(URLRE.Replace("%1", (string.IsNullOrEmpty(Lang) ? "[^}]+?" : Lang)) + (LangIndep ? "?" : string.Empty), RegexOptions.IgnoreCase);
        MatchCollection mcu = ru.Matches(B);
        foreach (Match mu in mcu)
        {
          URLList.Add(mu.Groups[1] + "|" + mu.Groups[2]);
        }
        logger.Debug("Extract URL - " + (string.IsNullOrEmpty(L) ? string.Empty : "Lang: [" + L + "] ") + "[" + Sec + "] URLs Found: " + URLList.Count);
      }
      return URLList;
    }

    private List<string> ExtractURL(string Sec, string AInputString, bool LangIndep = true)
    {
      if (LangIndep || string.IsNullOrEmpty(mvCentralCore.Settings.DataProviderAutoLanguage))
        return ExtractURLLang(Sec, AInputString, string.Empty, true);                                                 // Any Language
      else
      {
        var URLList = new List<string>();

        URLList = ExtractURLLang(Sec, AInputString, mvCentralCore.Settings.DataProviderAutoLanguage, LangIndep);      // Language from Settings
        if ((URLList.Count <= 0) && !"en".Equals(mvCentralCore.Settings.DataProviderAutoLanguage, StringComparison.CurrentCulture))
          URLList = ExtractURLLang(Sec, AInputString, "en", LangIndep);                                               // Default Language
        if ((URLList.Count <= 0) && !"en".Equals(mvCentralCore.Settings.DataProviderAutoLanguage, StringComparison.CurrentCulture))
          URLList = ExtractURLLang(Sec, AInputString, string.Empty, true);                                            // Any Language
        return URLList;
      }
    }

    private void ReportProgress(string text)
    {
      if (ProgressChanged != null)
      {
        ProgressChanged(this, new ProgressEventArgs { Text = "Fanart.TV DB: " + text });
      }
    }
  }
}
