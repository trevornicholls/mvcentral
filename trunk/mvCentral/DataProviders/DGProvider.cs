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

    class DGProvider: InternalProvider, IMusicVideoProvider {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static readonly object lockList = new object();

        // NOTE: To other developers creating other applications, using this code as a base
        //       or as a reference. PLEASE get your own API key. Do not reuse the one listed here
        //       it is intended for Music Videos use ONLY. API keys are free and easy to apply
        //       for. Visit this url: http://www.discogs.com/help/api

        #region API variables

        //          API Key: 5f15ded8a8

        private const string apiMusicVideoUrl = "http://www.discogs.com/{0}/{1}?f=xml&api_key={2}";
        private const string apikey = "5f15ded8a8";
        private static string apiArtistGetInfo = string.Format(apiMusicVideoUrl, "artist","{0}", apikey);
        private static string apiTrackGetInfo = string.Format(apiMusicVideoUrl, "release", "{0}", apikey);
        private static string apiSearch = string.Format("http://www.discogs.com/search?type=all&q={0}&f=xml&api_key={1}", "{0}", apikey);


        #endregion

        public string Name {
            get {
                return "www.discogs.com";
            }
        }

        public string Description {
            get { return "Returns details, covers and backdrops from discogs."; }
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
                List<string> at = mv.ArtUrls;
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

                List<string> at = mv.ArtUrls;
 //               if (mvCentralUtils.IsGuid(mv.MdID))
 //                   at = GetTrackImages(mv.MdID);
 //               else
 //                   at = GetTrackImages(mv.ArtistInfo[0].Artist, mv.Track);

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
                List<string> at = mv.ArtUrls;// getMusicVideoTrack(mv.MdID);
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

        public bool GetDetails(DBBasicInfo mv)
        {
//            throw new NotImplementedException();


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

                    List<Release> r1 = new List<Release>();
                    foreach (XmlNode x1 in n1.ChildNodes)
                    {
                        Release r2 = new Release(x1);
                        r1.Add(r2);
                    }
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
        
        public List<DBTrackInfo> Get(MusicVideoSignature mvSignature)
        {
           List<DBTrackInfo> results = new List<DBTrackInfo>();
           if (mvSignature == null)
               return results;
 //          try
 //          {
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
 //          }
 //          catch (Exception ex)
 //          {
 //              logger.ErrorException(String.Format("Error retrieving : {0} {1} from Last.FM", mvSignature.Artist, mvSignature.Track), ex); 
 //              return results;
 //          }
           
           return results;
        }



        private void setMusicVideoArtist(ref DBArtistInfo mv, string artist)
        {
            if (artist == null)
                return ;
            XmlNodeList xml = getXML(string.Format(apiArtistGetInfo,artist));
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
                        a1.bioContent = value;
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
//            if (mv.ArtistInfo[0].MdID.Trim().Length == 0) return null;


            // get release info

            if (track != null)
                xml = getXML(string.Format(apiSearch, artist + " " + track));
            else return null;

            if (xml == null)
                return null;
            root = xml.Item(0).ParentNode;
            if (root.Attributes != null && root.Attributes["stat"].Value != "ok") return null;
            int numresults = Convert.ToInt16(root.FirstChild.Attributes["numResults"].Value);
            int page = 1;
            mvNodes = xml.Item(0).ChildNodes;
            Release r2 = new Release(mvNodes[0]);
            mv.Track = r2.title;
            mv.MdID = r2.id;
            mv.bioContent = r2.summary;

/*            List <Release> r1 = new List<Release>(); 

            foreach (XmlNode x1 in mvNodes)
            {
                Release r2 = new Release(x1);
                r1.Add(r2);
            }
            while (page * 20 < numresults)
            {
                page++;
                xml = getXML(string.Format(apiSearch, artist + " " + track) + "&page=" + page.ToString());
                mvNodes = xml.Item(0).ChildNodes;
                foreach (XmlNode x1 in mvNodes)
                {
                    Release r2 = new Release(x1);
                    r1.Add(r2);
                }
            }
 */           return mv;
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
                return ;
            XmlNode root = xml.Item(0).ParentNode;
            if (root.Attributes != null && root.Attributes["stat"].Value != "ok") return ;
            mv.MdID = xml.Item(0).Attributes["id"].Value;
;            XmlNodeList mvNodes = xml.Item(0).ChildNodes;
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
                    foreach (DBAlbumInfo db3 in mv.AlbumInfo)
                    {
                        db3.PrimarySource = mv.PrimarySource;
                        db3.Commit();
                    }
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

        // given a url, retrieves the xml result set and returns the nodelist of Item objects
        private static XmlNodeList getXML(string url) {
            WebGrabber grabber = Utility.GetWebGrabberInstance(url);
            grabber.Encoding = Encoding.UTF8;
            grabber.Timeout = 5000;
            grabber.TimeoutIncrement = 10;
            grabber.Request.AutomaticDecompression = System.Net.DecompressionMethods.GZip;
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

    public class Release
    {
        public Release(XmlNode x1)
        {
            if (x1.Attributes["type"] != null) type = x1.Attributes["type"].Value;
            if (x1.Attributes["status"] != null) status = x1.Attributes["status"].Value;
            if (x1["title"] != null) title = x1["title"].InnerText;
            if (x1["format"] != null) format = x1["format"].InnerText;
            if (x1["label"] != null) label = x1["label"].InnerText;
            if (x1["uri"] != null) uri = x1["uri"].InnerText;
            if (x1["summary"] != null) summary = x1["summary"].InnerText;
            if (uri != null) id = uri.Substring(uri.LastIndexOf("/")+1);
            else if (x1.Attributes["id"] != null)  id = x1.Attributes["id"].Value;

            if (id == null)
            {
                foreach (XmlNode x2 in x1)
                    switch (x2.Name)
                    {
                        case "name":

                            title = x2.InnerText;
                            break;
                        case "mbid":
                            id = x2.InnerText;
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

    } 




}
