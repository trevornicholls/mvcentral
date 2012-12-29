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

  class HTBackdropsProvider : InternalProvider, IMusicVideoProvider
  {
    private static Logger logger = LogManager.GetCurrentClassLogger();

    private static readonly object lockList = new object();

    // NOTE: To other developers creating other applications, using this code as a base
    //       or as a reference. PLEASE get your own API key. Do not reuse the one listed here
    //       it is intended for Music Videos use ONLY. API keys are free and easy to apply
    //       for. Visit this url: http://htbackdrops.com

    #region API variables

    // Example Calls
    // http://htbackdrops.com/api/0e9139ab07ccf97ff4cb6568428d46b2/searchXML?keywords=madonna&default_operator=and&aid=1,5,26&fields=title,keywords,caption,mb_name,mb_alias&inc=keywords,caption,mb_name,mb_aliases
    // http://htbackdrops.com/api/0e9139ab07ccf97ff4cb6568428d46b2/searchXML?mbid=79239441-bfd5-4981-a70c-55c3f15c1287&dratio=1:1
    // http://htbackdrops.com/api/0e9139ab07ccf97ff4cb6568428d46b2/download/<PICTURE_ID>/fullsize

    private const string APIKey = "0e9139ab07ccf97ff4cb6568428d46b2";

    private const string SearchArtistImage = "http://htbackdrops.com/api/{0}/searchXML?keywords={1}&default_operator=and&aid=1,5,26&fields=title,keywords,caption,mb_name,mb_alias&dratio=1:1";
    private const string SearchArtistImageMBID = "http://htbackdrops.com/api/{0}/searchXML?mbid={1}&dratio=1:1";
    private const string DownloadImage = "http://htbackdrops.com/api/{0}/download/{1}/fullsize";

    //private static bool _strippedPrefixes;
    //private static bool _logMissing;
    //private static bool _useAlternative = true;
    //private static bool _useProxy;
    //private static string _proxyHost;
    //private static int _proxyPort;

    #endregion

    public string Name
    {
      get
      {
        return "www.htbackdrops.com";
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
      get { return false; }
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
      XmlNodeList xml;

      // Get the images
      // If we have a MBID for this Artist use this to retrive the image otherwise fall back to keyword search
      if (string.IsNullOrEmpty(mvArtistObject.MdID.Trim()))
      {
          xml = getXML(SearchArtistImage, APIKey, mvArtistObject.Artist);
      }
      else
      {
          xml = getXML(SearchArtistImageMBID, APIKey, mvArtistObject.MdID);
      }

      // If we reveived nothing back, bail out
      if (xml == null)
        return false;

      XmlNode root = xml.Item(0).ParentNode;
      // Get image nodes
      XmlNodeList images = root.SelectNodes(@"/search/images/image");
      int artistartAdded = 0;
      // Loop though each nodce and add it
      foreach (XmlNode imageNode in images)
      {
        XmlNode imageIDNode = imageNode.SelectSingleNode("id");
        if (mvArtistObject.AddArtFromURL(string.Format(DownloadImage, APIKey, imageIDNode.InnerText)) == ImageLoadResults.SUCCESS)
          artistartAdded++;

      }
      if (artistartAdded > 0)
        return true;
      else
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
    private static XmlNodeList getXML(string baseString, params object[] parameters)
    {
        for (int i = 0; i < parameters.Length; i++)
        {
            parameters[i] = Uri.EscapeDataString((string)parameters[i]);
        }

        return getXMLFromURL(string.Format(baseString, parameters));
    }

    // given a url, retrieves the xml result set and returns the nodelist of Item objects
    private static XmlNodeList getXMLFromURL(string url)
    {
      logger.Debug("Sending the request: " + url.Replace("3b40fddfaeaf4bf786fad7e4a42ac81c","<apiKey>"));
      //logger.Debug("Sending the request: " + url);

      mvWebGrabber grabber = Utility.GetWebGrabberInstance(url);
      grabber.Encoding = Encoding.UTF8;
      grabber.Timeout = 5000;
      grabber.TimeoutIncrement = 10;
      if (grabber.GetResponse())
      {
        return grabber.GetXML();
      }
      else
      {
        logger.Debug("***** API ERROR *****: Code:{0} ({1})", grabber.errorCode, grabber.errorText);
        return null;
      }
    }

    #endregion

    public event EventHandler ProgressChanged;
  }
}
