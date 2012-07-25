using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Text;
using System.IO;
using System.Xml;
using System.Windows.Forms;
using System.Text.RegularExpressions;

using Cornerstone.Tools;
using Cornerstone.Extensions;

using MediaPortal.Util;
using MediaPortal.Utils;

using mvCentral.Database;
using mvCentral.SignatureBuilders;
using mvCentral.LocalMediaManagement;
using NLog;
using mvCentral.LocalMediaManagement.MusicVideoResources;
using mvCentral.Utils;
using mvCentral.ConfigScreen.Popups;


namespace mvCentral.DataProviders
{

  class LastFMProvider : InternalProvider, IMusicVideoProvider
  {
    private static Logger logger = LogManager.GetCurrentClassLogger();

    private static readonly object lockList = new object();

    public string threadId
    {
      get
      {
        return Thread.CurrentThread.ManagedThreadId.ToString();
      }
    }

    // NOTE: To other developers creating other applications, using this code as a base
    //       or as a reference. PLEASE get your own API key. Do not reuse the one listed here
    //       it is intended for Music Videos use ONLY. API keys are free and easy to apply
    //       for. Visit this url: http://www.last.fm/api/intro

    #region API variables

    

    private const string apiMusicVideoUrl = "http://ws.audioscrobbler.com/2.0/?method={0}&api_key={1}";
    private const string apikey = "eadfb84ac56eddbf072efbfc18a90845";
    private const string apiSecret = "88b9694c60b240bd97ac1f02959f17c4";

    //        http://ws.audioscrobbler.com/2.0/?method=track.getinfo&api_key=b25b959554ed76058ac220b7b2e0a026&artist=cher&track=believe
    //        http://ws.audioscrobbler.com/2.0/?method=album.getinfo&api_key=b25b959554ed76058ac220b7b2e0a026&artist=Cher&album=Believe
    
    // Artist Info
    private static string apiArtistGetInfo = string.Format(apiMusicVideoUrl, "artist.getinfo&artist={0}&lang={1}", apikey);
    private static string apiArtistmbidGetInfo = string.Format(apiMusicVideoUrl, "artist.getinfo&mbid={0}&lang={1}", apikey);
    private static string apiArtistNameGetInfo = string.Format(apiMusicVideoUrl, "artist.getinfo&artist={0}&lang={1}", apikey);
    //Images
    private static string apiArtistmbidGetImagesInfo = string.Format(apiMusicVideoUrl, "artist.getimages&mbid={0}", apikey);
    private static string apiArtistNameGetImagesInfo = string.Format(apiMusicVideoUrl, "artist.getimages&artist={0}", apikey);
    private static string apiArtistGetImagesInfo = string.Format(apiMusicVideoUrl, "artist.getimages&artist={0}", apikey);
    // Albums
    private static string apiAlbumGetInfo = string.Format(apiMusicVideoUrl, "album.getinfo&album={0}&lang={1}", apikey);
    private static string apiAlbummbidGetInfo = string.Format(apiMusicVideoUrl, "album.getinfo&mbid={0}&lang={1}", apikey);
    private static string apiArtistAlbumGetInfo = string.Format(apiMusicVideoUrl, "album.getinfo&artist={0}&album={1}&lang={2}", apikey);
    private static string apiArtistTopAlbums = string.Format(apiMusicVideoUrl, "artist.gettopalbums&artist={0}", apikey);
    // Tracks
    private static string apiArtistTopTracks = string.Format(apiMusicVideoUrl, "artist.gettoptracks&artist={0}", apikey);
    private static string apiTrackGetInfo = string.Format(apiMusicVideoUrl, "set&track={0}&lang={1}", apikey);
    private static string apiTrackmbidGetInfo = string.Format(apiMusicVideoUrl, "track.getinfo&mbid={0}&lang={1}", apikey);
    private static string apiArtistTrackGetInfo = string.Format(apiMusicVideoUrl, "track.getinfo&artist={0}&track={1}&lang={2}", apikey);
    // Search
    private static string apiTrackSearch = string.Format(apiMusicVideoUrl, "track.search&track={0}", apikey);
    private static string apiArtistSearch = string.Format(apiMusicVideoUrl, "artist.search&artist={0}", apikey);
    private static string apiArtistTrackSearch = string.Format(apiMusicVideoUrl, "track.search&artist={0}&track={1}", apikey);

    #endregion

    public string Name
    {
      get
      {
        return "www.last.fm";
      }
    }

    public string Description
    {
      get { return "Returns details, art from lastfm."; }
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
        List<string> supportLanguages = new List<string>() { "en", "fr", "de", "pl", "ru", "es", "it","zh-CN" };
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
      get { return true; }
    }


    public DBTrackInfo GetArtistDetail(DBTrackInfo mv)
    {
      return mv;
    }

    public DBTrackInfo GetAlbumDetail(DBTrackInfo mv)
    {

      string albumTitle = mv.AlbumInfo[0].Album;
      string albumMBID = mv.AlbumInfo[0].MdID;
      string artist = mv.ArtistInfo[0].Artist;

      DBAlbumInfo mv1 = mv.AlbumInfo[0];
      setMusicVideoAlbum(ref mv1, artist, albumTitle, albumMBID);

      return mv;
    }


    /// <summary>
    /// Get the Artist, Album or Track details
    /// </summary>
    /// <param name="mv"></param>
    /// <returns></returns>
    public bool GetDetails(DBBasicInfo mv)
    {
      //logger.Debug("In Method : GetDetails(DBBasicInfo mv)");

      string inLang = mvCentralCore.Settings.DataProviderAutoLanguage;


      // Get Artist Info
      if (mv.GetType() == typeof(DBArtistInfo))
      {

        string artist = ((DBArtistInfo)mv).Artist;
        XmlNodeList xml = null;

        if (artist != null)
          xml = getXML(string.Format(apiArtistSearch, artist));
        else return false;

        if (xml == null)
          return false;
        XmlNode root = xml.Item(0).ParentNode;
        if (root.Attributes != null && root.Attributes["status"].Value != "ok") return false;
        XmlNode n1 = root.SelectSingleNode(@"/lfm/results/artistmatches");

        List<Release> r1 = new List<Release>();
        foreach (XmlNode x1 in n1.ChildNodes)
        {
          Release r2 = new Release(x1);
          if (r2.id != null || r2.id.Trim().Length > 0)
            r1.Add(r2);
        }
        r1.Sort(Release.TitleComparison);
        DetailsPopup d1 = new DetailsPopup(r1);

        if (d1.ShowDialog() == DialogResult.OK)
        {
          DBArtistInfo mv1 = (DBArtistInfo)mv;
          mv.ArtUrls.Clear();
          string title = d1.textBox1.Text;
          string mbid = d1.label8.Text;
          if (title.Trim().Length == 0) title = null;
          if (mbid.Trim().Length == 0) mbid = null;

          if (string.IsNullOrEmpty(mbid))
            setMusicVideoArtist(ref mv1, title, string.Empty);
          else
            setMusicVideoArtist(ref mv1, string.Empty, mbid);

            GetArtistArt((DBArtistInfo)mv);
        }
      }

      // Get Album Info
      if (mv.GetType() == typeof(DBAlbumInfo))
      {

        List<DBTrackInfo> a1 = DBTrackInfo.GetEntriesByAlbum((DBAlbumInfo)mv);
        if (a1.Count > 0)
        {
          string artist = a1[0].ArtistInfo[0].Artist;
          XmlNodeList xml = null;

          if (artist != null)
            xml = getXML(string.Format(apiArtistTopAlbums, artist));
          else 
            return false;

          if (xml == null)
            return false;

          XmlNode root = xml.Item(0).ParentNode;
          if (root.Attributes != null && root.Attributes["status"].Value != "ok") return false;
          XmlNode n1 = root.SelectSingleNode(@"/lfm/topalbums");

          List<Release> r1 = new List<Release>();
          foreach (XmlNode x1 in n1.ChildNodes)
          {
            Release r2 = new Release(x1);
            r1.Add(r2);
          }
          r1.Sort(Release.TitleComparison);
          DetailsPopup d1 = new DetailsPopup(r1);

          if (d1.ShowDialog() == DialogResult.OK)
          {
            DBAlbumInfo mv1 = (DBAlbumInfo)mv;
            mv.ArtUrls.Clear();
            string title = d1.textBox1.Text;
            string mbid = d1.label8.Text;
            if (title.Trim().Length == 0) title = null;
            if (mbid.Trim().Length == 0) mbid = null;
            setMusicVideoAlbum(ref mv1, artist, title, mbid);
            GetAlbumArt((DBAlbumInfo)mv);
          }
        }
      }

      // Get Track Info
      if (mv.GetType() == typeof(DBTrackInfo))
      {
        string artist = ((DBTrackInfo)mv).ArtistInfo[0].Artist;
        //first get artist info
        XmlNodeList xml = null;

        if (artist != null)
          xml = getXML(string.Format(apiArtistTopTracks, artist));
        else return false;

        if (xml == null)
          return false;

        XmlNode root = xml.Item(0).ParentNode;
        if (root.Attributes != null && root.Attributes["status"].Value != "ok") return false;
        XmlNode n1 = root.SelectSingleNode(@"/lfm/toptracks");

        int page = Convert.ToInt16(n1.Attributes["page"].Value);
        int perPage = Convert.ToInt16(n1.Attributes["perPage"].Value);
        int totalPages = Convert.ToInt16(n1.Attributes["totalPages"].Value);
        int total = Convert.ToInt16(n1.Attributes["total"].Value);
        // Process the page we already have
        List<Release> artistTopTracks = new List<Release>(); 

        foreach (XmlNode x1 in n1.ChildNodes)
        {
          Release r2 = new Release(x1);
          artistTopTracks.Add(r2);
        }

        for (int requestPage = 2; requestPage < totalPages; requestPage++)
        {
          // Now get the next Page
          xml = getXML(string.Format(apiArtistTopTracks, artist) + "&page=" + requestPage.ToString());
          root = xml.Item(0).ParentNode;
          XmlNode topTrackPage = root.SelectSingleNode(@"/lfm/toptracks");
          // Process the page we already have
          foreach (XmlNode track in topTrackPage.ChildNodes)
          {
            Release topTrack = new Release(track);
            artistTopTracks.Add(topTrack);
          }
        }

        artistTopTracks.Sort(Release.TitleComparison);

        DetailsPopup d1 = new DetailsPopup(artistTopTracks);

        if (d1.ShowDialog() == DialogResult.OK)
        {
          DBTrackInfo mv1 = (DBTrackInfo)mv;
          mv.ArtUrls.Clear();
          if (artist.Trim().Length == 0) artist = null;
          string title = d1.textBox1.Text;
          string mbid = d1.label8.Text;
          if (title.Trim().Length == 0) title = null;
          if (mbid.Trim().Length == 0) mbid = null;
          setMusicVideoTrack(ref mv1, artist, title, mbid);
          GetTrackArt((DBTrackInfo)mv);
        };
      }
      return true;
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
      List<DBTrackInfo> tracksOnAlbum = DBTrackInfo.GetEntriesByAlbum((DBAlbumInfo)basicInfo);
      if (tracksOnAlbum.Count > 0)
      {
        string artist = tracksOnAlbum[0].ArtistInfo[0].Artist;
        DBAlbumInfo mv1 = (DBAlbumInfo)basicInfo;
        basicInfo.ArtUrls.Clear();
        setMusicVideoAlbum(ref mv1, artist, albumTitle, AlbumMBID);
        GetAlbumArt((DBAlbumInfo)basicInfo);
      }
      return true;
    }

    /// <summary>
    /// Request artist artwork from last.fm
    /// </summary>
    /// <param name="mvArtistObject"></param>
    /// <returns></returns>
    public bool GetArtistArt(DBArtistInfo mvArtistObject)
    {
      if (mvArtistObject == null)
        return false;

      // Request artist images
      GetArtistImages(mvArtistObject);
      // if we have some - process
      if (mvArtistObject.ArtUrls != null)
      {
        // grab artistart loading settings
        int maxArtistArts = mvCentralCore.Settings.MaxArtistArts;

        int artistartAdded = 0;
        lock (mvArtistObject)
        {
          try
          {
            foreach (string artistImage in mvArtistObject.ArtUrls)
            {
              if (mvArtistObject.AlternateArts.Count >= maxArtistArts)
                break;
              if (mvArtistObject.AddArtFromURL(artistImage) == ImageLoadResults.SUCCESS)
                artistartAdded++;
            }
          }
          catch { }
        }
        if (artistartAdded > 0)
          return true;
      }
      return false;
    }
    /// <summary>
    /// Lets see if last.fm has some artwork for us
    /// </summary>
    /// <param name="mvTrackObject"></param>
    /// <returns></returns>
    public bool GetTrackArt(DBTrackInfo mvTrackObject)
    {
      List<string> trackImageList = null;
      int trackartAdded = 0;
      bool success = false;

      if (mvTrackObject == null)
        return false;

      // If video thumbnails prefered and this is a video file grab image(s) and return, this assumes success which is not good
      if (mvCentralCore.Settings.PreferThumbnail)
      {
        success = generateVideoThumbnail(mvTrackObject);
      }

      if (success)
        return success;
      else
      {
        // lets see what available from last.fm
        if (mvCentralUtils.IsGuid(mvTrackObject.MdID))
          trackImageList = GetTrackImages(mvTrackObject.MdID);
        else
          trackImageList = GetTrackImages(mvTrackObject.ArtistInfo[0].Artist, mvTrackObject.Track);
        // If we got nothing back from last.fm and we have not already tried to grab a fram from the video..try and grab a frame
        if (trackImageList == null && !mvCentralCore.Settings.PreferThumbnail)
        {
          if (generateVideoThumbnail(mvTrackObject))
            return true;
          else
            return false;
        }
        else if (trackImageList == null)
          return false;
      }

      // So we have some imeages from last.fm...let grab them
      if (trackImageList.Count > 0)
      {
        // grab covers loading settings
        int maxTrackArt = mvCentralCore.Settings.MaxTrackArts;
        int count = 0;

        foreach (string trackImage in trackImageList)
        {
          if (mvTrackObject.AlternateArts.Count >= maxTrackArt)
            break;

          if (mvTrackObject.AddArtFromURL(trackImage) == ImageLoadResults.SUCCESS)
            trackartAdded++;

          count++;
        }
      }
      // If we have some artwork then set the primay image to the first in the list
      if (trackartAdded > 0)
      {
        mvTrackObject.ArtFullPath = mvTrackObject.AlternateArts[0];
        return true;
      }
      else if (!mvCentralCore.Settings.PreferThumbnail)
      {
        // Right we have tried to get images from last.fm and they have been rejected.
        // provided we have not already tired, grab an image from the video.
        if (generateVideoThumbnail(mvTrackObject))
          return true;
        else
          return false;
      }
      else
        return false; // Well all that was a water of CPU cycles...bugger all found
    }
    /// <summary>
    /// Generate Thumbnail
    /// </summary>
    /// <param name="mv"></param>
    /// <returns></returns>
    bool generateVideoThumbnail(DBTrackInfo mv)
    {
      if (mvCentralCore.Settings.DisableMTN)
        return false;

      string outputFilename = Path.Combine(Path.GetTempPath(), mv.Track + ".jpg");

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
    /// <summary>
    /// Get album art work
    /// </summary>
    /// <param name="mvAlbumObject"></param>
    /// <returns></returns>
    public bool GetAlbumArt(DBAlbumInfo mvAlbumObject)
    {
      logger.Info("In Method : GetAlbumArt(DBAlbumInfo mv)");

      if (mvAlbumObject == null)
        return false;

      List<string> albumImageList = mvAlbumObject.ArtUrls;
      // First reload any artwork we have strored
      if (albumImageList.Count > 0)
      {
        // grab album art loading settings
        int maxAlbumArt = mvCentralCore.Settings.MaxAlbumArts;

        int albumartAdded = 0;
        foreach (string albumImage in albumImageList)
        {
          if (mvAlbumObject.AlternateArts.Count >= maxAlbumArt)
            break;

          ImageLoadResults imageLoadResults = mvAlbumObject.AddArtFromURL(albumImage);
          if (imageLoadResults == ImageLoadResults.SUCCESS || imageLoadResults == ImageLoadResults.SUCCESS_REDUCED_SIZE)
            albumartAdded++;
        }
      }
      else
      {
        // Now add any new artwork, this handles the sitution where an album has been created manually
        DBArtistInfo artist = null;
        List<DBTrackInfo> tracksOnAlbum = DBTrackInfo.GetEntriesByAlbum(mvAlbumObject);
        if (tracksOnAlbum.Count > 0)
          artist = DBArtistInfo.Get(tracksOnAlbum[0]);

        if (string.IsNullOrEmpty(mvAlbumObject.MdID.Trim()))
          setMusicVideoAlbum(ref mvAlbumObject, artist.Artist, mvAlbumObject.Album, null);
        else
          setMusicVideoAlbum(ref mvAlbumObject, mvAlbumObject.MdID);
      }

      mvAlbumObject.Commit();
      return true;
    }
    /// <summary>
    /// GetDetails - not used but will keep for now
    /// </summary>
    /// <param name="mv"></param>
    /// <returns></returns>
    public bool GetDetails(DBTrackInfo mv)
    {
      throw new NotImplementedException();
    }
    /// <summary>
    /// Retrive Track, Artist and if requested Album data
    /// </summary>
    /// <param name="mvSignature"></param>
    /// <returns></returns>
    public List<DBTrackInfo> GetTrackDetail(MusicVideoSignature mvSignature)
    {
      // Switch off album support
      if (mvCentralCore.Settings.DisableAlbumSupport)
        mvSignature.Album = null;

      List<DBTrackInfo> results = new List<DBTrackInfo>();
      if (mvSignature == null)
        return results;

      lock (lockList)
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
            mvTrackData.AlbumInfo.Add(albumInfo);

          }
          else
            mvTrackData.AlbumInfo.Clear();

          results.Add(mvTrackData);
        }
      }
      return results;
    }

    private List<DBTrackInfo> Search(string item)
    {
      return Search(item, null);
    }

    private List<DBTrackInfo> Search(string title, int? year)
    {
      List<DBTrackInfo> results = new List<DBTrackInfo>();
      return results;
    }

    public List<DBTrackInfo> GetmvCentralByHash(string hash)
    {
      List<DBTrackInfo> results = new List<DBTrackInfo>();
      return results;
    }

    private DBTrackInfo getMusicVideoById(string id)
    {
      return null;
    }

    private DBTrackInfo getMusicVideoByImdb(string imdbid)
    {
      return null;
    }

    private void setMusicVideoArtist(ref DBArtistInfo mv, string artistName, string artistmbid)
    {
      logger.Debug("In method : setMusicVideoArtist(ref DBArtistInfo mv, string artistName, string artistmbid)");

      XmlNodeList xml = null;

      // Have we an MBID for this artist
      if (string.IsNullOrEmpty(artistmbid))
        // No, use Artist Name for lookup
        xml = getXML(string.Format(apiArtistNameGetInfo, artistName, mvCentralCore.Settings.DataProviderAutoLanguage));
      else
        // Use MBID for Lookup 
        xml = getXML(string.Format(apiArtistmbidGetInfo, artistmbid, mvCentralCore.Settings.DataProviderAutoLanguage));

      // Did we get some data? Bail out if not
      if (xml == null)
        return;

      XmlNode root = xml.Item(0).ParentNode;
      if (root.Attributes != null && root.Attributes["status"].Value != "ok") return;

      XmlNodeList mvNodes = xml.Item(0).ChildNodes;

      foreach (XmlNode node in mvNodes)
      {
        string value = node.InnerText;
        switch (node.Name)
        {
          case "name":
            mv.Artist = value;
            break;
          case "mbid":
            mv.MdID = value;
            break;
          case "tags":
            foreach (XmlNode tag in node.ChildNodes)
            {
              string tagstr = tag.FirstChild.LastChild.Value;
              mv.Tag.Add(tagstr);
            }
            break;

          case "bio":
            XmlNode n1 = root.SelectSingleNode(@"/lfm/artist/bio/summary");
            if (n1 != null && n1.ChildNodes != null)
            {
              XmlNode childNode1 = n1.ChildNodes[0];
              if (childNode1 is XmlCDataSection)
              {
                XmlCDataSection cdataSection = childNode1 as XmlCDataSection;
                mv.bioSummary = mvCentralUtils.StripHTML(cdataSection.Value);
              }
              n1 = root.SelectSingleNode(@"/lfm/artist/bio/content");
              childNode1 = n1.ChildNodes[0];
              if (childNode1 is XmlCDataSection)
              {
                XmlCDataSection cdataSection = childNode1 as XmlCDataSection;
                mv.bioContent = mvCentralUtils.StripHTML(cdataSection.Value);
              }
            }
            break;
        }
      }
      return;
    }
    /// <summary>
    /// Grab the album data and process (overload)
    /// </summary>
    /// <param name="mv"></param>
    /// <param name="mbid"></param>
    private void setMusicVideoAlbum(ref DBAlbumInfo mv, string mbid)
    {
      setMusicVideoAlbum(ref mv, null, null, mbid);
    }
    /// <summary>
    /// Grab the album data and process (overload)
    /// </summary>
    /// <param name="mv"></param>
    /// <param name="Album"></param>
    /// <param name="mbid"></param>
    private void setMusicVideoAlbum(ref DBAlbumInfo mv, string Album, string mbid)
    {
      setMusicVideoAlbum(ref mv, null, Album, mbid);
    }
    /// <summary>
    /// Grab the album data and process
    /// </summary>
    /// <param name="mv"></param>
    /// <param name="artist"></param>
    /// <param name="album"></param>
    /// <param name="mbid"></param>
    private void setMusicVideoAlbum(ref DBAlbumInfo mv, string artist, string album, string mbid)
    {
      logger.Debug(string.Format("In method setMusicVideoAlbum : Atrist ({0})   |    Album ({1})    |    MBID ({2})", artist, album, mbid));

      XmlNodeList xml = null;
      //if (mbid != null)
      //  if (mbid.Trim().Length == 0)
      //    mbid = string.Empty;

      // Do we have a valid parameter - bail out if not
      if (album == null && mbid == null)
        return;

      // API Call takes MbId or Artist & Album
      if (!(string.IsNullOrEmpty(mbid) || mbid.Trim().Length == 0))
        xml = getXML(string.Format(apiAlbummbidGetInfo, mbid, mvCentralCore.Settings.DataProviderAutoLanguage));
      else if (!string.IsNullOrEmpty(artist) && !string.IsNullOrEmpty(album))
        xml = getXML(string.Format(apiArtistAlbumGetInfo, artist, album, mvCentralCore.Settings.DataProviderAutoLanguage));
      
      // Did we get some data back, if not bail out
      if (xml == null)
        return;

      // Get the root node and check the status - if failed abort futher processing
      XmlNode root = xml.Item(0).ParentNode;
      if (root.Attributes != null && root.Attributes["status"].Value != "ok") 
        return;
      // Grab the infomation nodes
      XmlNodeList mvAlbumNodes = xml.Item(0).ChildNodes;
      // and process
      foreach (XmlNode node in mvAlbumNodes)
      {
        string value = node.InnerText;
        switch (node.Name)
        {
          case "name":
            mv.Album = value;
            break;
          case "mbid":
            mv.MdID = value;
            break;
          case "tags": // Actors, Directors and Writers
            foreach (XmlNode tag in node.ChildNodes)
            {
              string tagstr = tag.FirstChild.LastChild.Value;
              mv.Tag.Add(tagstr);
            }
            break;
          case "image":
            if (!mv.ArtUrls.Contains(value))
              mv.ArtUrls.Add(value);

            break;
          case "wiki":
            XmlNode n1 = root.SelectSingleNode(@"/lfm/album/wiki/summary");
            if (n1 != null && n1.ChildNodes != null)
            {
              XmlNode childNode1 = n1.ChildNodes[0];
              if (childNode1 is XmlCDataSection)
              {
                XmlCDataSection cdataSection = childNode1 as XmlCDataSection;
                mv.bioSummary = mvCentralUtils.StripHTML(cdataSection.Value);
              }
              n1 = root.SelectSingleNode(@"/lfm/album/wiki/content");
              childNode1 = n1.ChildNodes[0];
              if (childNode1 is XmlCDataSection)
              {
                XmlCDataSection cdataSection = childNode1 as XmlCDataSection;
                mv.bioContent = mvCentralUtils.StripHTML(cdataSection.Value);
              }
            }
            break;
        }
      }
      return;
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

      XmlNodeList xml = null;

      // If we only have a valid track name
      if (artist == null && mbid == null && track != null)
        xml = getXML(string.Format(apiTrackGetInfo, track, mvCentralCore.Settings.DataProviderAutoLanguage));

      // if we only have a valid MBID
      if (track == null && artist == null && mbid != null)
        xml = getXML(string.Format(apiTrackmbidGetInfo, mbid, mvCentralCore.Settings.DataProviderAutoLanguage));

      // If now MBID but we do have Artist & Track
      if (mbid == null && artist != null && track != null)
        xml = getXML(string.Format(apiArtistTrackGetInfo, artist, track, mvCentralCore.Settings.DataProviderAutoLanguage));

      // We had nothing so lets get out of here
      if (xml == null)
        return;


      XmlNode root = xml.Item(0).ParentNode;
      if (root.Attributes != null && root.Attributes["status"].Value != "ok") return;

      XmlNodeList mvNodes = xml.Item(0).ChildNodes;

      foreach (XmlNode node in mvNodes)
      {
        string value = node.InnerText;
        switch (node.Name)
        {
          case "name":

            mv.Track = value;
            break;
          case "mbid":
            mv.MdID = value;
            break;
          case "tags": // Actors, Directors and Writers
            foreach (XmlNode tag in node.ChildNodes)
            {
              string tagstr = tag.FirstChild.LastChild.Value;
              mv.Tag.Add(tagstr);
            }
            break;

          case "image":
            mv.ArtUrls.Add(value);
            break;
          case "artist":
            break;
          case "album":
            break;
          case "toptags":
            foreach (XmlNode tag in node.ChildNodes)
            {
              XmlNode tagName = tag.SelectSingleNode("name");
              if (tagName != null)
              {
                mv.Tag.Add(tagName.InnerText);
              }
            }
            break;
          case "wiki":
            XmlNode n1 = root.SelectSingleNode(@"/lfm/track/wiki/summary");
            if (n1 != null && n1.ChildNodes != null)
            {
              XmlNode childNode1 = n1.ChildNodes[0];
              if (childNode1 is XmlCDataSection)
              {
                XmlCDataSection cdataSection = childNode1 as XmlCDataSection;
                mv.bioSummary = mvCentralUtils.StripHTML(cdataSection.Value);
              }
              n1 = root.SelectSingleNode(@"/lfm/track/wiki/content");
              childNode1 = n1.ChildNodes[0];
              if (childNode1 is XmlCDataSection)
              {
                XmlCDataSection cdataSection = childNode1 as XmlCDataSection;
                mv.bioContent = mvCentralUtils.StripHTML(cdataSection.Value);
              }
            }

            break;
        }
      }
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

      XmlNodeList xml = null;

      if (artist == null)
        xml = getXML(string.Format(apiTrackGetInfo, track, mvCentralCore.Settings.DataProviderAutoLanguage));
      else
        xml = getXML(string.Format(apiArtistTrackGetInfo, artist, track, mvCentralCore.Settings.DataProviderAutoLanguage));

      if (xml == null)
        return null;
      XmlNode root = xml.Item(0).ParentNode;
      if (root.Attributes != null && root.Attributes["status"].Value != "ok") return null;

      XmlNodeList mvNodes = xml.Item(0).ChildNodes;

      DBTrackInfo mv = new DBTrackInfo();
      foreach (XmlNode node in mvNodes)
      {
        string value = node.InnerText;
        switch (node.Name)
        {
          case "name":

            mv.Track = value;
            break;
          case "mbid":
            mv.MdID = value;
            break;
          case "artist":
            if (node.ChildNodes[0].InnerText.Trim().Length > 0)
            {
              DBArtistInfo d4 = new DBArtistInfo();
              //                            if (node.ChildNodes[1].InnerText.Trim().Length > 0)
              setMusicVideoArtist(ref d4, node.ChildNodes[0].InnerText, node.ChildNodes[1].InnerText);

              mv.ArtistInfo.Add(d4);
            }
            break;


          case "album":
            if (node.ChildNodes[0].InnerText.Trim().Length > 0)
            {
              DBAlbumInfo d4 = new DBAlbumInfo();

              // Is there an MBID for this album
              if (node.ChildNodes[2].InnerText.Trim().Length > 0)
                // Use it for the lookup
                setMusicVideoAlbum(ref d4, node.ChildNodes[2].InnerText);
              else
                // No MBID - Use Srtist and Album name instead
                setMusicVideoAlbum(ref d4, node.ChildNodes[0].InnerText.Trim(), node.ChildNodes[1].InnerText, null);

              // Have we actually got a valid album?
              if (d4.Album.Trim() != string.Empty)
                mv.AlbumInfo.Add(d4);
            }
            break;

          case "tags": // Actors, Directors and Writers
            foreach (XmlNode tag in node.ChildNodes)
            {
              string tagstr = tag.FirstChild.LastChild.Value;
              mv.Tag.Add(tagstr);
            }
            break;

          case "wiki":
            XmlNode n1 = root.SelectSingleNode(@"/lfm/track/wiki/summary");
            if (n1 != null && n1.ChildNodes != null)
            {
              XmlNode childNode1 = n1.ChildNodes[0];
              if (childNode1 is XmlCDataSection)
              {
                XmlCDataSection cdataSection = childNode1 as XmlCDataSection;
                mv.bioSummary = mvCentralUtils.StripHTML(cdataSection.Value);
              }
              n1 = root.SelectSingleNode(@"/lfm/track/wiki/content");
              childNode1 = n1.ChildNodes[0];
              if (childNode1 is XmlCDataSection)
              {
                XmlCDataSection cdataSection = childNode1 as XmlCDataSection;
                mv.bioContent = mvCentralUtils.StripHTML(cdataSection.Value);
              }
            }

            break;
        }
      }

      if (mv.ArtistInfo.Count == 0)
        return null;

      //if (mv.ArtistInfo[0].MdID.Trim().Length == 0)
      //    return null;

      return mv;
    }

    private List<string> GetAlbumMbid(string artist, string track)
    {
      List<string> str = getMusicVideoTrackSearch(artist, track);
      if (str != null)
      {
        List<string> result = new List<string>();
        foreach (string s1 in str)
        {
          XmlNodeList xml = null;
          xml = getXML(string.Format(apiArtistTrackGetInfo, artist, s1, mvCentralCore.Settings.DataProviderAutoLanguage));
          if (xml == null) continue;
          XmlNode root = xml.Item(0).ParentNode;
          if (root.Attributes != null && root.Attributes["status"].Value != "ok") continue;
          XmlNode n1 = root.SelectSingleNode(@"/lfm/track/album/mbid");
          if (n1 != null && n1.InnerText != "") result.Add(n1.InnerText);
        }
        return result;
      }
      return null;
    }


    private string GetArtistMbid(string artist)
    {
      logger.Debug("In method : GetArtistMbid(string artist)");

      XmlNodeList xml = null;

      xml = getXML(string.Format(apiArtistGetInfo, artist, mvCentralCore.Settings.DataProviderAutoLanguage));

      if (xml == null) return null;
      XmlNode root = xml.Item(0).ParentNode;
      if (root.Attributes != null && root.Attributes["status"].Value != "ok") return null;
      XmlNode n1 = root.SelectSingleNode(@"/lfm/artist/mbid");
      if (n1 != null && n1.InnerText != "") return n1.InnerText;
      return null;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="mv"></param>
    private void GetArtistImages(DBArtistInfo mv)
    {
      int minWidth = mvCentralCore.Settings.MinimumArtistWidth;
      int minHeight = mvCentralCore.Settings.MinimumArtistHeight;

      XmlNodeList xml = null;
      if (!string.IsNullOrEmpty(mv.MdID.Trim()))
        xml = getXML(string.Format(apiArtistmbidGetImagesInfo, mv.MdID));
      else
        xml = getXML(string.Format(apiArtistNameGetImagesInfo, mv.Artist));

      if (xml == null) return;
      List<string> result = new List<string>();
      XmlNode root = xml.Item(0).ParentNode;
      if (root.Attributes != null && root.Attributes["status"].Value != "ok") return;
      XmlNode n1 = root.SelectSingleNode(@"/lfm/images");


      if (n1 != null)
      {
        foreach (XmlNode n2 in n1.ChildNodes)
        {
          if (n2.Name == "image")
          {
            foreach (XmlNode n3 in n2.ChildNodes)
            {
              if (n3.Name == "sizes")
              {
                XmlNode n4 = n3.FirstChild;
                XmlNode imageWidth = n3.FirstChild.Attributes.GetNamedItem("width");
                XmlNode imageHeight = n3.FirstChild.Attributes.GetNamedItem("height");
                if (int.Parse(imageHeight.InnerText) >= minHeight && int.Parse(imageWidth.InnerText) >= minWidth)
                  mv.ArtUrls.Add(n4.InnerText);
              }
            }
          }
        }
      }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="mbid"></param>
    /// <returns></returns>
    private List<string> GetAlbumImages(string mbid)
    {
      XmlNodeList xml = null;
      int minWidth = mvCentralCore.Settings.MinimumAlbumWidth;
      int minHeight = mvCentralCore.Settings.MinimumAlbumHeight;


      xml = getXML(string.Format(apiAlbummbidGetInfo, mbid, mvCentralCore.Settings.DataProviderAutoLanguage));
      if (xml == null) return null;
      List<string> result = new List<string>();
      XmlNode root = xml.Item(0).ParentNode;
      if (root.Attributes != null && root.Attributes["status"].Value != "ok") return null;
      XmlNode n1 = root.SelectSingleNode(@"/lfm/album");
      if (n1 != null)
      {

        foreach (XmlNode n2 in n1.ChildNodes)
        {
          switch (n2.Name)
          {
            case "image":
              foreach (XmlNode n3 in n2.ChildNodes)
              {
                XmlNode imageWidth = n3.Attributes.GetNamedItem("width");
                XmlNode imageHeight = n3.Attributes.GetNamedItem("height");
                if (int.Parse(imageHeight.InnerText) >= minHeight && int.Parse(imageWidth.InnerText) >= minWidth)
                  result.Add(n3.InnerText);
              }
              break;
          }

        }
      }
      return result;
    }

    private List<string> GetTrackImages(string mbid)
    {
      XmlNodeList xml = null;
      int minWidth = mvCentralCore.Settings.MinimumTrackWidth;
      int minHeight = mvCentralCore.Settings.MinimumAlbumHeight;

      xml = getXML(string.Format(apiTrackmbidGetInfo, mbid, mvCentralCore.Settings.DataProviderAutoLanguage));
      if (xml == null) return null;
      List<string> result = new List<string>();
      XmlNode root = xml.Item(0).ParentNode;
      if (root.Attributes != null && root.Attributes["status"].Value != "ok") return null;
      XmlNode n1 = root.SelectSingleNode(@"/lfm/track/album");
      if (n1 != null)
      {

        foreach (XmlNode n2 in n1.ChildNodes)
        {
          switch (n2.Name)
          {
            case "image":
              foreach (XmlNode n3 in n2.ChildNodes)
              {
                try
                {
                  XmlNode imageWidth = n3.Attributes.GetNamedItem("width");
                  XmlNode imageHeight = n3.Attributes.GetNamedItem("height");
                  if (int.Parse(imageHeight.InnerText) >= minHeight && int.Parse(imageWidth.InnerText) >= minWidth)
                    result.Add(n3.InnerText);
                }
                catch { }
              }
              break;
          }

        }
      }
      if (result.Count == 0)
        return result;
      else
      return null;
    }

    private List<string> GetTrackImages(string artist, string track)
    {
      //int minWidth = mvCentralCore.Settings.MinimumTrackWidth;
      //int minHeight = mvCentralCore.Settings.MinimumAlbumHeight;

      XmlNodeList xml = null;
      xml = getXML(string.Format(apiArtistTrackGetInfo, artist, track, mvCentralCore.Settings.DataProviderAutoLanguage));
      if (xml == null) return null;
      List<string> result = new List<string>();
      XmlNode root = xml.Item(0).ParentNode;
      if (root.Attributes != null && root.Attributes["status"].Value != "ok") return null;
      XmlNode n1 = root.SelectSingleNode(@"/lfm/track/album");
      if (n1 != null)
      {

        foreach (XmlNode n2 in n1.ChildNodes)
        {
          switch (n2.Name)
          {
            case "image":
              foreach (XmlNode n3 in n2.ChildNodes)
              {
                //XmlNode imageWidth = n3.Attributes.GetNamedItem("width");
                //XmlNode imageHeight = n3.Attributes.GetNamedItem("height");
                //if (int.Parse(imageHeight.InnerText) >= minHeight && int.Parse(imageWidth.InnerText) >= minWidth)
                  result.Add(n3.InnerText);
              }
              break;
          }

        }
      }
      return result;
    }

    private List<string> getMusicVideoTrackSearch(string track)
    {
      return getMusicVideoTrackSearch(null, track);
    }

    private List<string> getMusicVideoTrackSearch(string artist, string track)
    {
      if (track == null)
        return null;

      XmlNodeList xml = null;

      if (artist == null)
        xml = getXML(string.Format(apiTrackSearch, track));
      else
        xml = getXML(string.Format(apiArtistTrackSearch, artist, track));

      if (xml == null)
        return null;
      XmlNode root = xml.Item(0).ParentNode;
      if (root.Attributes != null && root.Attributes["status"].Value != "ok") return null;

      XmlNodeList mvNodes = root.SelectNodes("/lfm/results/trackmatches");
      if (mvNodes != null && mvNodes[0].ChildNodes != null && mvNodes[0].ChildNodes.Count != 0)
      {
        XmlNodeList n1 = mvNodes[0].ChildNodes;
        List<string> result = new List<string>();
        foreach (XmlNode n2 in n1)
        {
          if (n2.ChildNodes != null && n2.ChildNodes.Count != 0)
          {
            XmlNodeList n3 = n2.ChildNodes;
            if (n3.Item(1).InnerText == artist)
              result.Add(n3.Item(0).InnerText);
          }
        }
        if (result.Count > 0) return result;
      }
      return null;
    }

    /// <summary>
    /// Add artist and if found album to the track data object
    /// </summary>
    /// <param name="trackData"></param>
    /// <returns></returns>
    public UpdateResults UpdateTrack(DBTrackInfo trackData)
    {
      if (trackData == null)
        return UpdateResults.FAILED;

      lock (lockList)
      {
        DBArtistInfo artistData = DBArtistInfo.Get(trackData);
        if (artistData != null)
        {
          trackData.ArtistInfo[0] = artistData;
        }
        if (trackData.ArtistInfo.Count > 0)
        {
          trackData.ArtistInfo[0].PrimarySource = trackData.PrimarySource;
          trackData.ArtistInfo[0].Commit();
        }
        DBAlbumInfo albumData = DBAlbumInfo.Get(trackData);
        if (albumData != null)
        {
          trackData.AlbumInfo[0] = albumData;
        }
        if (trackData.AlbumInfo.Count > 0)
        {
          trackData.AlbumInfo[0].PrimarySource = trackData.PrimarySource;
          trackData.AlbumInfo[0].Commit();
        }
      }
      return UpdateResults.SUCCESS;
    }

    // given a url, retrieves the xml result set and returns the nodelist of Item objects
    private static XmlNodeList getXML(string url)
    {
      XmlDocument xmldoc = new XmlDocument();

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
  }
}
