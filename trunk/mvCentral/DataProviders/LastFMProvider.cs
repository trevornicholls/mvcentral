using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Text;
using System.IO;
using System.Xml;
using System.Windows.Forms;
using mvCornerstone.Tools;
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
    private static Logger logger = mvCentralCore.MyLogManager.Instance.GetCurrentClassLogger();

    private static readonly object lockList = new object();

    // NOTE: To other developers creating other applications, using this code as a base
    //       or as a reference. PLEASE get your own API key. Do not reuse the one listed here
    //       it is intended for Music Videos use ONLY. API keys are free and easy to apply
    //       for. Visit this url: http://www.last.fm/api/intro

    #region API variables



    private const string apiMusicVideoUrl = "http://ws.audioscrobbler.com/2.0/?method={0}&api_key={1}";
    private const string apikey = "3b40fddfaeaf4bf786fad7e4a42ac81c";

    //        http://ws.audioscrobbler.com/2.0/?method=track.getinfo&api_key=b25b959554ed76058ac220b7b2e0a026&artist=cher&track=believe
    //        http://ws.audioscrobbler.com/2.0/?method=album.getinfo&api_key=b25b959554ed76058ac220b7b2e0a026&artist=Cher&album=Believe

    private static string apiArtistGetInfo = string.Format(apiMusicVideoUrl, "artist.getinfo&artist={0}", apikey);
    private static string apiArtistmbidGetInfo = string.Format(apiMusicVideoUrl, "artist.getinfo&mbid={0}", apikey);
    private static string apiArtistNameGetInfo = string.Format(apiMusicVideoUrl, "artist.getinfo&artist={0}", apikey);

    private static string apiArtistmbidGetImagesInfo = string.Format(apiMusicVideoUrl, "artist.getimages&mbid={0}", apikey);
    private static string apiArtistNameGetImagesInfo = string.Format(apiMusicVideoUrl, "artist.getimages&artist={0}", apikey);

    private static string apiArtistGetImagesInfo = string.Format(apiMusicVideoUrl, "artist.getimages&artist={0}", apikey);

    private static string apiAlbumGetInfo = string.Format(apiMusicVideoUrl, "album.getinfo&album={0}", apikey);
    private static string apiAlbummbidGetInfo = string.Format(apiMusicVideoUrl, "album.getinfo&mbid={0}", apikey);
    private static string apiArtistAlbumGetInfo = string.Format(apiMusicVideoUrl, "album.getinfo&artist={0}&album={1}", apikey);
    private static string apiArtistTopAlbums = string.Format(apiMusicVideoUrl, "artist.gettopalbums&artist={0}", apikey);
    private static string apiArtistTopTracks = string.Format(apiMusicVideoUrl, "artist.gettoptracks&artist={0}", apikey);
    private static string apiTrackGetInfo = string.Format(apiMusicVideoUrl, "track.getinfo&track={0}", apikey);
    private static string apiTrackmbidGetInfo = string.Format(apiMusicVideoUrl, "track.getinfo&mbid={0}", apikey);
    private static string apiArtistTrackGetInfo = string.Format(apiMusicVideoUrl, "track.getinfo&artist={0}&track={1}", apikey);
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
      get { return "en"; }
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
      get { return true; }
    }

    public bool GetDetails(DBBasicInfo mv)
    {

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
          setMusicVideoArtist(ref mv1, "", mbid);
          GetArtistArt((DBArtistInfo)mv);
        };
      }


      if (mv.GetType() == typeof(DBAlbumInfo))
      {

        List<DBTrackInfo> a1 = DBTrackInfo.GetEntriesByAlbum((DBAlbumInfo)mv);
        if (a1.Count > 0)
        {
          string artist = a1[0].ArtistInfo[0].Artist;
          XmlNodeList xml = null;

          if (artist != null)
            xml = getXML(string.Format(apiArtistTopAlbums, artist));
          else return false;

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
          };


        }
      }

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




        List<Release> r1 = new List<Release>();
        while (page < totalPages - 1)
        {
          xml = getXML(string.Format(apiArtistTopTracks, artist) + "page=" + page);
          n1 = root.SelectSingleNode(@"/lfm/toptracks");

          foreach (XmlNode x1 in n1.ChildNodes)
          {
            Release r2 = new Release(x1);
            r1.Add(r2);
          }
          page++;
        }

        r1.Sort(Release.TitleComparison);

        DetailsPopup d1 = new DetailsPopup(r1);

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


    public bool GetArtistArt(DBArtistInfo mv)
    {
      logger.Info("In Method : GetArtistArt(DBArtistInfo mv)");
      if (mv == null)
        return false;

      // if we already have a backdrop move on for now
      if (mv.ArtFullPath.Trim().Length > 0)
        return true;

      if (mv.ArtFullPath.Trim().Length == 0)
      {
        GetArtistImages(mv);
        if (mv.ArtUrls != null)
        {
          // grab artistart loading settings
          int maxArtistArts = mvCentralCore.Settings.MaxArtistArts;

          int artistartAdded = 0;
          int count = 0;
          logger.Info("Lock mv.ArtUrls");
          lock (mv.ArtUrls)
          {
            foreach (string a2 in mv.ArtUrls)
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
      }

      // if we get here we didn't manage to find a proper backdrop
      // so return false
      return false;
    }

    public bool GetTrackArt(DBTrackInfo mv)
    {
      logger.Info("In Method : GetTrackArt(DBTrackInfo mv)");
      if (mv == null)
        return false;

      // if we already have a backdrop move on for now
      if (mv.ArtFullPath.Trim().Length > 0)
        return true;

      List<string> at = null;
      if (mvCentralUtils.IsGuid(mv.MdID))
        at = GetTrackImages(mv.MdID);
      else
        at = GetTrackImages(mv.ArtistInfo[0].Artist, mv.Track);

      if (at != null)
      {
        // grab covers loading settings
        int maxTrackArt = mvCentralCore.Settings.MaxTrackArts;

        int trackartAdded = 0;
        int count = 0;
        logger.Info("Lock at");
        lock (at)
        {
          foreach (string a2 in at)
          {
            if (mv.AlternateArts.Count >= maxTrackArt)
              break;
            if (mv.AddArtFromURL(a2) == ImageLoadResults.SUCCESS)
              trackartAdded++;

            count++;
          }
        }
        if (trackartAdded > 0)
        {
          mv.ArtFullPath = mv.AlternateArts[0];
          // Update source info
          //                        mv.GetSourceMusicVideoInfo(SourceInfo).Identifier = mv.MdID;
          return true;
        }
      }
      // if we get here we didn't manage to find a proper backdrop
      // so return false
      return false;
    }

    public bool GetAlbumArt(DBAlbumInfo mv)
    {
      logger.Info("In Method : GetAlbumArt(DBAlbumInfo mv)");
      if (mv == null)
        return false;
      List<string> at = mv.ArtUrls;
      if (at != null)
      {
        // grab album art loading settings
        int maxAlbumArt = mvCentralCore.Settings.MaxAlbumArts;

        int albumartAdded = 0;
        int count = 0;
        logger.Info("Lock at");
        lock (at)
        {
          foreach (string a2 in at)
          {
            if (mv.AlternateArts.Count >= maxAlbumArt)
              break;
            if (mv.AddArtFromURL(a2) == ImageLoadResults.SUCCESS)
              albumartAdded++;

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
      return true;
    }

    public bool GetDetails(DBTrackInfo mv)
    {
      throw new NotImplementedException();
    }

    public List<DBTrackInfo> Get(MusicVideoSignature mvSignature)
    {
      List<DBTrackInfo> results = new List<DBTrackInfo>();
      if (mvSignature == null)
        return results;
      lock (lockList)
      {
        DBTrackInfo mv = getMusicVideoTrack(mvSignature.Artist, mvSignature.Track);
        if (mv != null)
        {
          if (mv.ArtistInfo.Count == 0)
          {
            DBArtistInfo d4 = new DBArtistInfo();
            d4.Artist = mvSignature.Artist;
            mv.ArtistInfo.Add(d4);
          }

          if (mvSignature.Album != null)
          {
            if (!mvCentralCore.Settings.UseMDAlbum)
            {
              DBAlbumInfo d5 = new DBAlbumInfo();
              d5.Album = mvSignature.Album;
              setMusicVideoAlbum(ref d5, mvSignature.Album, null);
              mv.AlbumInfo.Clear();
              mv.AlbumInfo.Add(d5);
            }
          }
          else mv.AlbumInfo.Clear();

          results.Add(mv);
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
      XmlNodeList xml;
      if (!string.IsNullOrEmpty(artistmbid))
        xml = getXML(string.Format(apiArtistmbidGetInfo, artistmbid));
      else
        xml = getXML(string.Format(apiArtistNameGetInfo, artistName));

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
                mv.bioSummary = cdataSection.Value;
              }
              n1 = root.SelectSingleNode(@"/lfm/artist/bio/content");
              childNode1 = n1.ChildNodes[0];
              if (childNode1 is XmlCDataSection)
              {
                XmlCDataSection cdataSection = childNode1 as XmlCDataSection;
                mv.bioContent = cdataSection.Value;
              }
            }

            break;
        }
      }
      return;
    }

    private void setMusicVideoAlbum(ref DBAlbumInfo mv, string mbid)
    {
      setMusicVideoAlbum(ref mv, null, null, mbid);
    }

    private void setMusicVideoAlbum(ref DBAlbumInfo mv, string Album, string mbid)
    {
      setMusicVideoAlbum(ref mv, null, Album, mbid);
    }

    private void setMusicVideoAlbum(ref DBAlbumInfo mv, string artist, string album, string mbid)
    {
      if (album == null && mbid == null)
        return;

      XmlNodeList xml = null;

      if (artist == null && mbid == null) xml = getXML(string.Format(apiAlbumGetInfo, album));
      if (album == null && artist == null) xml = getXML(string.Format(apiAlbummbidGetInfo, mbid));
      if (mbid == null) xml = getXML(string.Format(apiArtistAlbumGetInfo, artist, album));
      if (mbid != null) xml = getXML(string.Format(apiAlbummbidGetInfo, mbid));

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
                mv.bioSummary = cdataSection.Value;
              }
              n1 = root.SelectSingleNode(@"/lfm/album/wiki/content");
              childNode1 = n1.ChildNodes[0];
              if (childNode1 is XmlCDataSection)
              {
                XmlCDataSection cdataSection = childNode1 as XmlCDataSection;
                mv.bioContent = cdataSection.Value;
              }
            }

            break;
        }
      }
      return;
    }

    private void setMusicVideoTrack(ref DBTrackInfo mv, string artist, string track, string mbid)
    {
      if (track == null && mbid == null)
        return;

      XmlNodeList xml = null;

      if (artist == null && mbid == null) xml = getXML(string.Format(apiTrackGetInfo, track));
      if (track == null && artist == null) xml = getXML(string.Format(apiTrackmbidGetInfo, mbid));
      if (mbid == null) xml = getXML(string.Format(apiArtistTrackGetInfo, artist, track));

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
          case "wiki":
            XmlNode n1 = root.SelectSingleNode(@"/lfm/track/wiki/summary");
            if (n1 != null && n1.ChildNodes != null)
            {
              XmlNode childNode1 = n1.ChildNodes[0];
              if (childNode1 is XmlCDataSection)
              {
                XmlCDataSection cdataSection = childNode1 as XmlCDataSection;
                mv.bioSummary = cdataSection.Value;
              }
              n1 = root.SelectSingleNode(@"/lfm/track/wiki/content");
              childNode1 = n1.ChildNodes[0];
              if (childNode1 is XmlCDataSection)
              {
                XmlCDataSection cdataSection = childNode1 as XmlCDataSection;
                mv.bioContent = cdataSection.Value;
              }
            }

            break;
        }
      }
      return;
    }

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
        xml = getXML(string.Format(apiTrackGetInfo, track));
      else
        xml = getXML(string.Format(apiArtistTrackGetInfo, artist, track));

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
              if (node.ChildNodes[2].InnerText.Trim().Length > 0)
                setMusicVideoAlbum(ref d4, node.ChildNodes[2].InnerText);
              else
                setMusicVideoAlbum(ref d4, node.ChildNodes[0].InnerText, node.ChildNodes[1].InnerText, null);
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
                mv.bioSummary = cdataSection.Value;
              }
              n1 = root.SelectSingleNode(@"/lfm/track/wiki/content");
              childNode1 = n1.ChildNodes[0];
              if (childNode1 is XmlCDataSection)
              {
                XmlCDataSection cdataSection = childNode1 as XmlCDataSection;
                mv.bioContent = cdataSection.Value;
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
          xml = getXML(string.Format(apiArtistTrackGetInfo, artist, s1));
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
      XmlNodeList xml = getXML(string.Format(apiArtistGetInfo, artist));
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


      xml = getXML(string.Format(apiAlbummbidGetInfo, mbid));
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

      xml = getXML(string.Format(apiTrackmbidGetInfo, mbid));
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

    private List<string> GetTrackImages(string artist, string track)
    {
      //int minWidth = mvCentralCore.Settings.MinimumTrackWidth;
      //int minHeight = mvCentralCore.Settings.MinimumAlbumHeight;

      XmlNodeList xml = null;
      xml = getXML(string.Format(apiArtistTrackGetInfo, artist, track));
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


    public UpdateResults Update(DBTrackInfo mv)
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
          mv.AlbumInfo[0].PrimarySource = mv.PrimarySource;
          mv.AlbumInfo[0].Commit();
        }
      }
      return UpdateResults.SUCCESS;
    }

    // given a url, retrieves the xml result set and returns the nodelist of Item objects
    private static XmlNodeList getXML(string url)
    {
      WebGrabber grabber = Utility.GetWebGrabberInstance(url);
      grabber.Encoding = Encoding.UTF8;
      grabber.Timeout = 5000;
      grabber.TimeoutIncrement = 10;
      if (grabber.GetResponse())
      {
        return grabber.GetXML();
      }
      else
        return null;
    }
  }
}
