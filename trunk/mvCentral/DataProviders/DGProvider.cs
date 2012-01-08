using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Text;
using System.IO;
using System.Xml;
using System.Windows.Forms;
using Cornerstone.Tools;
using mvCentral.Database;
using mvCentral.SignatureBuilders;
using mvCentral.LocalMediaManagement;
using NLog;
using mvCentral.LocalMediaManagement.MusicVideoResources;
using mvCentral.Utils;
using mvCentral.ConfigScreen.Popups;

namespace mvCentral.DataProviders
{

  class DGProvider : InternalProvider, IMusicVideoProvider
  {
    private static Logger logger = LogManager.GetCurrentClassLogger();

    private static readonly object lockList = new object();

    // NOTE: To other developers creating other applications, using this code as a base
    //       or as a reference. PLEASE get your own API key. Do not reuse the one listed here
    //       it is intended for Music Videos use ONLY. API keys are free and easy to apply
    //       for. Visit this url: http://www.discogs.com/help/api

    #region API variables


    private const string apiMusicVideoUrl = "http://www.discogs.com/{0}/{1}?f=xml&api_key={2}";
    private const string apikey = "5f15ded8a8";
    private static string apiArtistGetInfo = string.Format(apiMusicVideoUrl, "artist", "{0}", apikey);
    private static string apiTrackGetInfo = string.Format(apiMusicVideoUrl, "release", "{0}", apikey);
    private static string apiSearch = string.Format("http://www.discogs.com/search?type=all&q={0}&f=xml&api_key={1}", "{0}", apikey);


    #endregion

    public string Name
    {
      get
      {
        return "www.discogs.com";
      }
    }

    public string Description
    {
      get { return "Returns details, art from discogs."; }
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
      get { return true; }
    }

    public bool ProvidesTrackArt
    {
      get { return false; }
    }

    public bool GetArtistArt(DBArtistInfo mv)
    {
      if (mv == null)
        return false;

      // if we already have a backdrop move on for now
      //if (mv.ArtFullPath.Trim().Length > 0) 
      //  return true;

      //if (mv.ArtFullPath.Trim().Length == 0)
      //{
        List<string> at = mv.ArtUrls;
        if (at != null)
        {
          // grab artistart loading settings
          int maxArtistArts = mvCentralCore.Settings.MaxArtistArts;

          int artistartAdded = 0;
          int count = 0;
          lock (mv)
          {
            foreach (string a2 in at)
            {
              if (mv.AlternateArts.Count >= maxArtistArts)
                break;

              if (mv.AddArtFromURL(a2) == ImageLoadResults.SUCCESS)
                artistartAdded++;

              count++;
            }
          }
          if (artistartAdded > 0)
          {
            // Update source info
            //                        mv.GetSourceMusicVideoInfo(SourceInfo).Identifier = mv.MdID;
            return true;
          }
        }
      //}

      // if we get here we didn't manage to find a proper backdrop
      // so return false
      return false;
    }

    public bool GetTrackArt(DBTrackInfo mv)
    {
      if (mv == null)
        return false;

      // if we already have a backdrop move on for now
      if (mv.ArtFullPath.Trim().Length > 0)
        return true;

      if ((bool)mvCentralCore.Settings["prefer_thumbnail"].Value)
      {
        logger.Debug("Call generateVideoThumbnail");
          return generateVideoThumbnail(mv);
      }

      List<string> at = mv.ArtUrls;
      int trackartAdded = 0;

      if (at != null)
      {
        // grab covers loading settings
        int maxTrackArt = mvCentralCore.Settings.MaxTrackArts;

        int count = 0;
        lock (at)
        {
          foreach (string a2 in at)
          {
            if (mv.AlternateArts.Count >= maxTrackArt) break;
            if (mv.AddArtFromURL(a2) == ImageLoadResults.SUCCESS)
              trackartAdded++;

            count++;
          }
        }
      }
      if (trackartAdded > 0)
      {
        mv.ArtFullPath = mv.AlternateArts[0];
        return true;
      }
      else
      {
        logger.Debug("No Track art found - Genterate Video Thumbnail");
          if (generateVideoThumbnail(mv))
            return true;
          else
            return false;
      }
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
    /// Get the Album Art
    /// </summary>
    /// <param name="mv"></param>
    /// <returns></returns>
    public bool GetAlbumArt(DBAlbumInfo mv)
    {
      if (mv == null)
        return false;

      if (mv.ArtFullPath.Trim().Length == 0)
      {
        List<string> at = mv.ArtUrls;
        if (at != null)
        {
          // grab album art loading settings
          int maxAlbumArt = mvCentralCore.Settings.MaxAlbumArts;

          int albumartAdded = 0;
          int count = 0;
          lock (at)
          {
            foreach (string a2 in at)
            {
              if (mv.AlternateArts.Count >= maxAlbumArt) break;
              if (mv.AddArtFromURL(a2) == ImageLoadResults.SUCCESS) albumartAdded++;

              count++;
            }
          }
          if (albumartAdded > 0)
          {
            // Update source info
            //                        mv.GetSourceMusicVideoInfo(SourceInfo).Identifier = mv.MdID;
            return true;
          }
        }
      }


      return true;
    }

    public bool GetDetails(DBBasicInfo mv)
    {
      if (mv.GetType() == typeof(DBAlbumInfo))
      {

        List<DBTrackInfo> a1 = DBTrackInfo.GetEntriesByAlbum((DBAlbumInfo)mv);
        if (a1.Count > 0)
        {
          string artist = a1[0].ArtistInfo[0].Artist;
          //first get artist info
          XmlNodeList xml = null;

          if (artist != null)
            xml = getXML(string.Format(apiArtistGetInfo, artist));
          else return false;

          if (xml == null)
            return false;
          XmlNode root = xml.Item(0).ParentNode;
          if (root.Attributes != null && root.Attributes["stat"].Value != "ok") return false;
          XmlNode n1 = root.SelectSingleNode(@"/resp/artist/releases");

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
            setMusicVideoAlbum(ref mv1, d1.label8.Text);
            GetAlbumArt((DBAlbumInfo)mv);
          };


        }
      }

      if (mv.GetType() == typeof(DBTrackInfo))
      {
        string artist = ((DBTrackInfo)mv).ArtistInfo[0].Artist;
        //first get artist info
        XmlNodeList xml = null;

        if (artist != null)
          xml = getXML(string.Format(apiArtistGetInfo, artist));
        else return false;

        if (xml == null)
          return false;
        XmlNode root = xml.Item(0).ParentNode;
        if (root.Attributes != null && root.Attributes["stat"].Value != "ok") return false;
        XmlNode n1 = root.SelectSingleNode(@"/resp/artist/releases");

        if (n1 == null)
          return false;

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
          DBTrackInfo mv1 = (DBTrackInfo)mv;
          setMusicVideoTrack(ref mv1, d1.label8.Text);
          GetTrackArt((DBTrackInfo)mv);
        };
      }
      return true;
    }

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
        setMusicVideoAlbum(ref mv1,AlbumMBID);
        GetAlbumArt((DBAlbumInfo)basicInfo);
      }
      return true;
    }

    private void setMusicVideoArtist(ref DBArtistInfo mv, string artist)
    {
      if (artist == null)
        return;
      XmlNodeList xml = getXML(string.Format(apiArtistGetInfo, artist));
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


    private DBTrackInfo getMusicVideoTrack(string track)
    {
      return getMusicVideoTrack(null, null, track);
    }

    private DBTrackInfo getMusicVideoTrack(string artist, string album, string track)
    {
      if (track == null)
        return null;
      XmlNodeList xml = null;

      //first get artist info

      if (artist != null)
        xml = getXML(string.Format(apiArtistGetInfo, artist));
      else return null;

      if (xml == null)
        return null;
      XmlNode root = xml.Item(0).ParentNode;
      if (root.Attributes != null && root.Attributes["stat"].Value != "ok") return null;
      XmlNodeList mvNodes = xml.Item(0).ChildNodes;
      DBTrackInfo mv = new DBTrackInfo();
      DBArtistInfo a1 = new DBArtistInfo();
      mv.ArtistInfo.Add(a1);
      foreach (XmlNode node in mvNodes)
      {
        string value = node.InnerText;
        switch (node.Name)
        {
          case "name":
            a1.Artist = value;
            break;
          case "profile":
            a1.bioContent = mvCentralUtils.StripHTML(value);
            break;
          case "images":
            {
              foreach (XmlNode x1 in node.ChildNodes)
              {

                a1.ArtUrls.Add(x1.Attributes["uri"].Value);
              }
            }
            break;


          case "releases":
            if (node.ChildNodes[0].InnerText.Trim().Length > 0)
            {
              if (album != null)
              {
                DBAlbumInfo d4 = new DBAlbumInfo();

                if (!mvCentralCore.Settings.UseMDAlbum)
                {
                  d4.Album = album;
                  mv.AlbumInfo.Add(d4);
                }
                else setMusicVideoAlbum(ref d4, node.FirstChild);
                mv.AlbumInfo.Add(d4);
              }
              else mv.AlbumInfo.Clear();
            }
            break;

        }
      }

      if (mv.ArtistInfo.Count == 0) return null;

      // get release info

      if (track != null)
        xml = getXML(string.Format(apiSearch, artist + " " + track));
      else return null;

      if (xml == null)
        return null;
      root = xml.Item(0).ParentNode;
      if (root.Attributes != null && root.Attributes["stat"].Value != "ok") return null;
      int numresults = Convert.ToInt16(root.FirstChild.Attributes["numResults"].Value);
      mvNodes = xml.Item(0).ChildNodes;
      if (numresults > 0)
      {
        Release r2 = new Release(mvNodes[0]);
        mv.Track = r2.title;
        mv.MdID = r2.id;
        mv.bioContent = mvCentralUtils.StripHTML(r2.summary);
      }
      else return null;
      return mv;
    }

    private void setMusicVideoTrack(ref DBTrackInfo mv, string id)
    {
      if (id == null || mv == null)
        return;
      XmlNodeList xml = null;

      // get release info

      xml = getXML(string.Format(apiTrackGetInfo, id));
      if (xml == null)
        return;
      XmlNode root = xml.Item(0).ParentNode;
      if (root.Attributes != null && root.Attributes["stat"].Value != "ok") return;
      mv.MdID = xml.Item(0).Attributes["id"].Value;
      ; XmlNodeList mvNodes = xml.Item(0).ChildNodes;
      foreach (XmlNode node in mvNodes)
      {
        string value = node.InnerText;
        switch (node.Name)
        {
          case "title":
            mv.Track = value;
            break;
          case "release":

            mv.MdID = node.FirstChild.Attributes["id"].Value;
            break;
          case "images":
            {
              mv.ArtUrls.Clear();
              foreach (XmlNode x1 in node.ChildNodes)
              {

                mv.ArtUrls.Add(x1.Attributes["uri"].Value);
              }
            }
            break;
        }
      }


    }

    private void setMusicVideoAlbum(ref DBAlbumInfo mv, string id)
    {
      if (id == null || mv == null)
        return;
      XmlNodeList xml = null;

      // get release info

      xml = getXML(string.Format(apiTrackGetInfo, id));
      if (xml == null)
        return;
      XmlNode root = xml.Item(0).ParentNode;
      if (root.Attributes != null && root.Attributes["stat"].Value != "ok") return;
      mv.MdID = xml.Item(0).Attributes["id"].Value;
      ; XmlNodeList mvNodes = xml.Item(0).ChildNodes;
      foreach (XmlNode node in mvNodes)
      {
        string value = node.InnerText;
        switch (node.Name)
        {
          case "title":
            mv.Album = value;
            break;
          case "release":

            mv.MdID = node.FirstChild.Attributes["id"].Value;
            break;
          case "images":
            {
              mv.ArtUrls.Clear();
              foreach (XmlNode x1 in node.ChildNodes)
              {
                mv.ArtUrls.Add(x1.Attributes["uri"].Value);
              }
            }
            break;
        }
      }


    }
    private void setMusicVideoAlbum(ref DBAlbumInfo mv, XmlNode node)
    {
      if (node == null || mv == null)
        return;


      if (node.Attributes["type"].Value != "Main") return;
      mv.MdID = node.Attributes["id"].Value;

      foreach (XmlNode node1 in node.ChildNodes)
      {
        switch (node1.Name)
        {
          case "release":

            //                        mv.MdID = value;
            break;
          case "title":
            mv.Album = node1.InnerText;
            break;
          case "image":

            break;
          case "year":
            //                       mv.y    
            break;
        }
      }
      return;
    }

    public UpdateResults UpdateTrack(DBTrackInfo mv)
    {
      if (mv == null)
        return UpdateResults.FAILED;
      lock (lockList)
      {
        DBArtistInfo db1 = DBArtistInfo.Get(mv);
        if (db1 != null)
        {
          mv.ArtistInfo[0] = db1;
        }
        if (mv.ArtistInfo.Count > 0)
        {
          mv.ArtistInfo[0].PrimarySource = mv.PrimarySource;
          mv.ArtistInfo[0].Commit();
        }
        DBAlbumInfo db2 = DBAlbumInfo.Get(mv);
        if (db2 != null)
        {
          mv.AlbumInfo[0] = db2;
        }
        if (mv.AlbumInfo.Count > 0)
        {
          foreach (DBAlbumInfo db3 in mv.AlbumInfo)
          {
            db3.PrimarySource = mv.PrimarySource;
            db3.Commit();
          }
        }
      }
      return UpdateResults.SUCCESS;
    }



    // given a url, retrieves the xml result set and returns the nodelist of Item objects
    private static XmlNodeList getXML(string url)
    {
      logger.Debug("Sending the request: " + url);

      mvWebGrabber grabber = Utility.GetWebGrabberInstance(url);
      grabber.Encoding = Encoding.UTF8;
      grabber.Timeout = 5000;
      grabber.TimeoutIncrement = 10;
      grabber.Request.AutomaticDecompression = System.Net.DecompressionMethods.GZip;
      if (grabber.GetResponse())
      {
        return grabber.GetXML();
      }
      else
        return null;
    }

  }



  public class Release : IComparable<Release>
  {
    public Release(XmlNode ArtistTopTrackPage)
    {

      if (ArtistTopTrackPage.Attributes["type"] != null) type = ArtistTopTrackPage.Attributes["type"].Value;
      if (ArtistTopTrackPage.Attributes["status"] != null) status = ArtistTopTrackPage.Attributes["status"].Value;
      if (ArtistTopTrackPage["title"] != null) title = ArtistTopTrackPage["title"].InnerText;
      if (ArtistTopTrackPage["format"] != null) format = ArtistTopTrackPage["format"].InnerText;
      if (ArtistTopTrackPage["label"] != null) label = ArtistTopTrackPage["label"].InnerText;
      if (ArtistTopTrackPage["uri"] != null) uri = ArtistTopTrackPage["uri"].InnerText;
      if (ArtistTopTrackPage["summary"] != null) summary = ArtistTopTrackPage["summary"].InnerText;
      if (uri != null) id = uri.Substring(uri.LastIndexOf("/") + 1);
      else if (ArtistTopTrackPage.Attributes["id"] != null) id = ArtistTopTrackPage.Attributes["id"].Value;

      if (id == null)
      {
        foreach (XmlNode topTrack in ArtistTopTrackPage)
          
          switch (topTrack.Name)
          {
            case "name":
              title = topTrack.InnerText;
              break;
            case "mbid":
              id = topTrack.InnerText;
              break;
          }
      }
    }

    public String type { get; set; }
    public String status { get; set; }
    public String title { get; set; }
    public String format { get; set; }
    public String label { get; set; }
    public String uri { get; set; }
    public String summary { get; set; }
    public String id { get; set; }

    public static Comparison<Release> TitleComparison = delegate(Release p1, Release p2)
    {
      return p1.title.CompareTo(p2.title);
    };
    public static Comparison<Release> IDComparison = delegate(Release p1, Release p2)
    {
      return p1.id.CompareTo(p2.id);
    };

    #region IComparable<Product> Members
    public int CompareTo(Release other)
    {
      return title.CompareTo(other.title);
    }
    #endregion


  }
}
