using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Text;
using System.IO;
using System.Xml;
using Cornerstone.Tools;
using mvCentral.Database;
using mvCentral.SignatureBuilders;
using mvCentral.LocalMediaManagement;
using NLog;
using mvCentral.LocalMediaManagement.MusicVideoResources;
using mvCentral.Utils;

namespace mvCentral.DataProviders
{

    class LastFMProvider: InternalProvider, IMusicVideoProvider {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static readonly object lockList = new object();

        // NOTE: To other developers creating other applications, using this code as a base
        //       or as a reference. PLEASE get your own API key. Do not reuse the one listed here
        //       it is intended for Music Videos use ONLY. API keys are free and easy to apply
        //       for. Visit this url: http://www.last.fm/api/intro

        #region API variables

//         3b40fddfaeaf4bf786fad7e4a42ac81c


        private const string apiMusicVideoUrl = "http://ws.audioscrobbler.com/2.0/?method={0}&api_key={1}";
        private const string apikey = "3b40fddfaeaf4bf786fad7e4a42ac81c";
        private static string apiMusicVideoImdbLookup = string.Format(apiMusicVideoUrl, "MusicVideo.imdbLookup", "en");
        private static string apiMusicVideoSearch = string.Format(apiMusicVideoUrl, "MusicVideo.search", "en");
        private static string apiMusicVideoGetInfo = string.Format(apiMusicVideoUrl, "MusicVideo.getInfo", "en");
        private static string apiHashGetInfo = string.Format(apiMusicVideoUrl, "Hash.getInfo", "en");

//        http://ws.audioscrobbler.com/2.0/?method=track.getinfo&api_key=b25b959554ed76058ac220b7b2e0a026&artist=cher&track=believe
//        http://ws.audioscrobbler.com/2.0/?method=album.getinfo&api_key=b25b959554ed76058ac220b7b2e0a026&artist=Cher&album=Believe

        private static string apiArtistGetInfo = string.Format(apiMusicVideoUrl, "artist.getinfo&artist={0}", apikey);
        private static string apiArtistmbidGetInfo = string.Format(apiMusicVideoUrl, "artist.getinfo&mbid={0}", apikey);
        private static string apiArtistNameGetInfo = string.Format(apiMusicVideoUrl, "artist.getinfo&artist={0}", apikey);
        private static string apiArtistmbidGetImagesInfo = string.Format(apiMusicVideoUrl, "artist.getimages&mbid={0}", apikey);
        private static string apiArtistGetImagesInfo = string.Format(apiMusicVideoUrl, "artist.getimages&artist={0}", apikey);
        private static string apiAlbumGetInfo = string.Format(apiMusicVideoUrl, "album.getinfo&album={0}", apikey);
        private static string apiAlbummbidGetInfo = string.Format(apiMusicVideoUrl, "album.getinfo&mbid={0}", apikey);
        private static string apiArtistAlbumGetInfo = string.Format(apiMusicVideoUrl, "album.getinfo&artist={0}&album={1}", apikey);
        private static string apiTrackGetInfo = string.Format(apiMusicVideoUrl, "track.getinfo&track={0}", apikey);
        private static string apiTrackmbidGetInfo = string.Format(apiMusicVideoUrl, "track.getinfo&mbid={0}", apikey);
        private static string apiArtistTrackGetInfo = string.Format(apiMusicVideoUrl, "track.getinfo&artist={0}&track={1}", apikey);
        private static string apiTrackSearch = string.Format(apiMusicVideoUrl, "track.search&track={0}", apikey);
        private static string apiArtistTrackSearch = string.Format(apiMusicVideoUrl, "track.search&artist={0}&track={1}", apikey);

        #endregion

        public string Name {
            get {
                return "www.last.fm";
            }
        }

        public string Description {
            get { return "Returns details, covers and backdrops from lastfm."; }
        }

        public string Language {
            get { return new CultureInfo("en").DisplayName; }
        }

        public string LanguageCode {
            get { return "en"; }
        }

        public bool ProvidesDetails {
            get { return true; }
        }

        public bool ProvidesArtistArt {
            get { return true; }
        }

        public bool ProvidesAlbumArt {
            get { return true; }
        }

        public bool ProvidesTrackArt
        {
            get { return true; }
        }

        public bool GetArtistArt(DBArtistInfo mv)
        {
            if (mv == null)
                return false;

            // if we already have a backdrop move on for now
            if (mv.ArtFullPath.Trim().Length > 0)
                return true;

            // do we have an id?
 //           string tmdbID = getTheMusicVideoDbId(mv, true);
 //           if (tmdbID == null) {
 //               return false;
//            }

            if (mv.ArtFullPath.Trim().Length == 0)
            {
                List<string> at = GetArtistImages(mv.MdID);
                if (at != null)
                {
                    // grab artistart loading settings
                    int maxArtistArts = mvCentralCore.Settings.MaxArtistArts;

                    int artistartAdded = 0;
                    int count = 0;
                    foreach (string a2 in at)
                    {
                        if (mv.AlternateArts.Count >= maxArtistArts) break;
                        if (mv.AddArtFromURL(a2) == ImageLoadResults.SUCCESS) artistartAdded++;

                        count++;
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
                    foreach (string a2 in at)
                    {
                        if (mv.AlternateArts.Count >= maxTrackArt) break;
                        if (mv.AddArtFromURL(a2) == ImageLoadResults.SUCCESS)
                            trackartAdded++;

                        count++;
                    }
                    if (trackartAdded > 0)
                    {
  //                      mv.TrackArtFullPath = mv.AlternateTrackArts[0];
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
            if (mv == null)
                return false;
            // do we have an id?
//            string tmdID = getTheMusicVideoDbId(mv, true);
//            if (tmdID == null)
//            {
//                return false;
//            }

            if (mv.ArtFullPath.Trim().Length == 0 )
            {
                List<string> at = GetAlbumImages(mv.MdID);
                if (at != null)
                {
                    // grab album art loading settings
                    int maxAlbumArt = mvCentralCore.Settings.MaxAlbumArts;

                    int albumartAdded = 0;
                    int count = 0;
                    foreach (string a2 in at)
                    {
                        if (mv.AlternateArts.Count >= maxAlbumArt) break;
                        if (mv.AddArtFromURL(a2) == ImageLoadResults.SUCCESS) albumartAdded++;

                        count++;
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

        public bool GetDetails(DBTrackInfo mv)
        {
            throw new NotImplementedException();
        }
        
        public List<DBTrackInfo> Get(MusicVideoSignature mvSignature)
        {
           List<DBTrackInfo> results = new List<DBTrackInfo>();
           if (mvSignature == null)
               return results;
 //          try
 //          {
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
 //          }
 //          catch (Exception ex)
 //          {
 //              logger.ErrorException(String.Format("Error retrieving : {0} {1} from Last.FM", mvSignature.Artist, mvSignature.Track), ex); 
 //              return results;
 //          }
           
           return results;
        }

        private List<DBTrackInfo> Search(string item) {
            return Search(item, null);
        }

        private List<DBTrackInfo> Search(string title, int? year) 
        {
            List<DBTrackInfo> results = new List<DBTrackInfo>();
            return results;
        }

        public List<DBTrackInfo> GetmvCentralByHash(string hash) {
            List<DBTrackInfo> results = new List<DBTrackInfo>();
            return results;
        }

        private DBTrackInfo getMusicVideoById(string id) {
                return null;
        }

        private DBTrackInfo getMusicVideoByImdb(string imdbid) {
            return null;
        }

        private void setMusicVideoArtist(ref DBArtistInfo mv, string artistName, string artistmbid)
        {
          if (string.IsNullOrEmpty(artistName) && string.IsNullOrEmpty(artistmbid))  
                return ;

          XmlNodeList xml = null;;

          if (string.IsNullOrEmpty(artistmbid))
            xml = getXML(string.Format(apiArtistNameGetInfo, artistName));
          else
            xml = getXML(string.Format(apiArtistmbidGetInfo, artistmbid));

            if (xml == null)
                return ;
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
                    case "tags": // Actors, Directors and Writers
                        foreach (XmlNode tag in node.ChildNodes)
                        {
                            string tagstr = tag.FirstChild.LastChild.Value;
                            mv.Tag.Add(tagstr);
                        }
                        break;

                    case "bio":
                        XmlNode  n1 = root.SelectSingleNode(@"/lfm/artist/bio/summary");
                        if (n1 != null && n1.ChildNodes != null )
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
                return ;

            XmlNodeList xml = null;

            if (artist == null && mbid == null) xml = getXML(string.Format(apiAlbumGetInfo, album));
            if (album == null && artist == null) xml = getXML(string.Format(apiAlbummbidGetInfo, mbid));
            if (mbid == null) xml = getXML(string.Format(apiArtistAlbumGetInfo, artist, album));

            if (xml == null)
                return ;
            XmlNode root = xml.Item(0).ParentNode;
            if (root.Attributes != null && root.Attributes["status"].Value != "ok") return ;

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

                    case "image" :

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
            return ;
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
                          setMusicVideoArtist(ref d4, node.ChildNodes[0].InnerText, node.ChildNodes[1].InnerText);
                          mv.ArtistInfo.Add(d4);
                        }
                        break;


                    case "album":
                        if (node.ChildNodes[0].InnerText.Trim().Length > 0)
                        {
                            DBAlbumInfo d4 = new DBAlbumInfo();
                            if (node.ChildNodes[1].InnerText.Trim().Length > 0)
                               setMusicVideoAlbum(ref d4, node.ChildNodes[2].InnerText);
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

            if (mv.ArtistInfo.Count == 0) return null;
            //if (mv.ArtistInfo[0].MdID.Trim().Length == 0) return null;

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

        private List<string> GetArtistImages(string mbid)
        {
            XmlNodeList xml = null;
            xml = getXML(string.Format(apiArtistmbidGetImagesInfo, mbid));
            if (xml == null) return null;
            List<string> result = new List<string>();
            XmlNode root = xml.Item(0).ParentNode;
            if (root.Attributes != null && root.Attributes["status"].Value != "ok") return null;
            XmlNode n1 = root.SelectSingleNode(@"/lfm/images");
            if (n1 != null)
            {
 
                foreach (XmlNode n2 in n1.ChildNodes)
                {
                    switch (n2.Name)
                    {
                        case "image":
                            foreach (XmlNode n3 in n2.ChildNodes)
                            {
                                switch (n3.Name)
                                {
                                    case "sizes" :
                                        XmlNode n4 = n3.FirstChild;

                                        result.Add(n4.InnerText);

                                        break;
                                }
                            }
                                //                            mv.Title = value;


                            break;
                    }
                    
                }
            }
            
            
            
            return result;
        }

        private List<string> GetAlbumImages(string mbid)
        {
            XmlNodeList xml = null;
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


        public UpdateResults Update(DBTrackInfo mv) {
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
/*                foreach (DBArtistInfo currInfo in mv.ArtistInfo)
                {
                    DBArtistInfo db1 = DBArtistInfo.Get(mv);
                    if (db1 != null)
                    {
                        db1.Copy(DBArtistInfo db2);
                        mv.ArtistInfo.Clear();
                        db1.Commit();
                        db1.CommitNeeded = false;
                        mv.ArtistInfo.Add(db1);
                        continue;
                    }
                    currInfo.Commit();
                    currInfo.CommitNeeded = false;
                }

                /*                foreach (DBAlbumInfo currInfo in AlbumInfo)
                    {
                        DBAlbumInfo db1 = DBAlbumInfo.Get(this);
                        if (db1 != null)
                        {
                            this.AlbumInfo.Clear();
                            this.AlbumInfo.Add(db1);
                        }
                        currInfo.Commit();
                    }
                    */
            }
/*            lock (mv)
            {
 //               mv.Commit();
                mv.AddTables(mv);
            
                // create/commit album/artist info
                List<DBAlbumInfo> d1 = DBAlbumInfo.GetAll();
                foreach (DBAlbumInfo d3 in d1)
                {
                    foreach (string tr3 in d3.IntTrackID)
                        if (String.Equals(tr3, mv.IntTrackID))
                        {
                            if (mv.AlbumMdID == null) d3.Album = mv.Album;
                            else
                            {
                                DBAlbumInfo d4 = d3;
                                setMusicVideoAlbum(ref d4, mv.AlbumMdID);
                            }
                            d3.Commit();
                        }
                }
            
                logger.Info(" after update artist/album : " + mv.Track + " " + Thread.CurrentThread.GetHashCode().ToString());
            }*/
   //         string tmdbId = getTheMusicVideoDbId(mv, false);
            // check if tmdbId is still null, if so request id.
   //         if (tmdbId == null)

            return UpdateResults.SUCCESS;
//                return UpdateResults.FAILED_NEED_ID;

            // Grab the mv using the TMDB ID
  //          DBTrackInfo newMusicVideo = getMusicVideoById(tmdbId);
  //          if (newMusicVideo != null) {
  //              mv.GetSourceMusicVideoInfo(SourceInfo).Identifier = tmdbId;
  //              mv.CopyUpdatableValues(newMusicVideo);
  //              return UpdateResults.SUCCESS;
  //          }
  //          else {
  //              return UpdateResults.FAILED;
  //          }
        }

 
        private string getTheMusicVideoDbId(DBTrackInfo mv, bool fuzzyMatch) {            
            // check for internally stored ID
/*            DBSourceMusicVideoInfo idObj = mv.GetSourceMusicVideoInfo(SourceInfo);
            if (idObj != null && idObj.Identifier != null) {
                return idObj.Identifier;
            }

            // if available, lookup based on mdb ID
            else if (mv.MdID != null && mv.MdID.Trim().Length > 0) {
                string imdbId = mv.MdID.Trim();
                XmlNodeList xml = getXML(apiMusicVideoImdbLookup + imdbId);
                if (xml != null && xml.Count > 0) {
                    // Get TMDB Id
                    XmlNodeList idNodes = xml.Item(0).SelectNodes("//id");
                    if (idNodes.Count != 0) {
                        return idNodes[0].InnerText;
                    }
                }
            }

            // if asked for, do a fuzzy match based on title
            else if (fuzzyMatch) {
                // grab possible matches by main title
                List<DBTrackInfo> results = Search(mv.Track);

                // grab possible matches by alt titles
//                foreach (string currAltTitle in mv.AlternateTitles) {
//                    List<DBTrackInfo> tempResults = Search(mv.Title, mv.Year);
//                    if (results.Count == 0) tempResults = Search(mv.Title);

//                    results.AddRange(tempResults);
//                }

                // pick a possible match if one meets our requirements
                foreach (DBTrackInfo currMatch in results) {
                    if (CloseEnough(currMatch, mv))
                        return currMatch.GetSourceMusicVideoInfo(SourceInfo).Identifier;
                }
            }
  */          
            return null;
        }

        private bool CloseEnough(DBTrackInfo mv1, DBTrackInfo mv2) {
            if (CloseEnough(mv1.Track, mv2)) return true;

//            foreach (string currAltTitle in mv1.AlternateTitles) 
//                if (CloseEnough(currAltTitle, mv2)) return true;

            return false;
        }

        private bool CloseEnough(string title, DBTrackInfo mv) {
            int distance;
            
            distance = AdvancedStringComparer.Levenshtein(title, mv.Track);
            if (distance <= mvCentralCore.Settings.AutoApproveThreshold)
                return true;

//            foreach (string currAltTitle in mv.AlternateTitles) {
//                distance = AdvancedStringComparer.Levenshtein(title, currAltTitle);
//                if (distance <= mvCentralCore.Settings.AutoApproveThreshold)
//                    return true;
//            }

            return false;
        }

        /// <summary>
        /// Returns a mv list queries by filehash
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static List<DBTrackInfo> GetmvCentralByHashLookup(string hash) {
            List<DBTrackInfo> results = new List<DBTrackInfo>();
            XmlNodeList xml = getXML(apiHashGetInfo + hash);
            if (xml == null)
                return results;

            XmlNodeList mvNodes = xml.Item(0).SelectNodes("//mv");
            foreach (XmlNode mvNode in mvNodes) {

                if (mvNode == null || mvNode.ChildNodes.Count < 2)
                    continue;

                DBTrackInfo mv = new DBTrackInfo();
                foreach (XmlNode node in mvNode.ChildNodes) {
                    string value = node.InnerText;
                    switch (node.Name) {
                        case "name":
//                            mv.Title = value;
                            break;
                        case "imdb_id":
//                            mv.ImdbID = value;
                            break;
                        case "released":
                            DateTime date;
//                            if (DateTime.TryParse(value, out date))
//                                mv.Year = date.Year;
                            break;
                    }
                }
                results.Add(mv);
            }
            return results;
        }

        // given a url, retrieves the xml result set and returns the nodelist of Item objects
        private static XmlNodeList getXML(string url) {
            WebGrabber grabber = Utility.GetWebGrabberInstance(url);
            grabber.Encoding = Encoding.UTF8;
            grabber.Timeout = 5000;
            grabber.TimeoutIncrement = 10;
//            try
//            {
                if (grabber.GetResponse())
                {
                    //                string str = grabber.GetString();
                    //                XmlDocument doc = new XmlDocument();

                    //                doc.Load(new StringReader(str));
                    //                return doc.DocumentElement.ChildNodes;
                    return grabber.GetXML();
                }
                else
                    return null;
//            }
//            catch (Exception ex)
//            {
//                logger.ErrorException("grabber ", ex);
//                return null;
//            }
        }

    }

}
