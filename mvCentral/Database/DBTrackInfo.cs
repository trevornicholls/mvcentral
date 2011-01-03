using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using NLog;
using System.Web;
using System.Net;
using System.Threading;
using System.Collections;
using Cornerstone.Database;
using Cornerstone.Database.CustomTypes;
using Cornerstone.Database.Tables;
using mvCentral.LocalMediaManagement;
using System.Text.RegularExpressions;
using Cornerstone.Tools.Translate;
using System.Runtime.InteropServices;
using mvCentral.LocalMediaManagement.MusicVideoResources;
using Cornerstone.Extensions;

namespace mvCentral.Database {
    [DBTableAttribute("track_info")]
    public class DBTrackInfo: DBBasicInfo, IComparable, IAttributeOwner {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly object lockList = new object();
        public DBTrackInfo()
            : base() {
        }


        #region Database Fields

        [DBField(AllowDynamicFiltering=false)]
        public string Track {
            get { return _track; }
            set { 
                _track = value;
                Basic = value;
                PopulateSortBy();
                commitNeeded = true;
            }
        } private string _track;

        [DBField(AllowDynamicFiltering = false)]
        public string Chapter
        {
            get { return _chapter; }
            set
            {
                _chapter = value;
                commitNeeded = true;
            }
        } private string _chapter;

        [DBField(AllowDynamicFiltering = false)]
        public int ChapterID
        {
            get { return _chapterid; }
            set
            {
                _chapterid = value;
                commitNeeded = true;
            }
        } private int _chapterid;

        [DBField(AllowDynamicFiltering = false)]
        public String PlayTime
        {
            get
            {
                return _playtime;
            }                
            set
            {
                _playtime = value;
                commitNeeded = true;
            }
        } private string _playtime;

        [DBField(AllowDynamicFiltering = false)]
        public String OffsetTime
        {
            get
            {
                return _offsettime;
            }
            set
            {
                _offsettime = value;
                commitNeeded = true;
            }
        } private string _offsettime;

        [DBField(Filterable = false)]
        public string SortBy {
            get {
                if (_sortBy.Trim().Length == 0)
                    PopulateSortBy();

                return _sortBy; 
            }

            set {
                _sortBy = value;
                commitNeeded = true;
            }
        } private string _sortBy;
        
        [DBField(AllowAutoUpdate = false, FieldName = "date_added")]
        public DateTime DateAdded {
            get { return _dateAdded; }

            set {
                _dateAdded = value;
                commitNeeded = true;
            }
        } private DateTime _dateAdded;

        [DBRelation(AutoRetrieve=true)]
        public RelationList<DBTrackInfo, DBLocalMedia> LocalMedia {
            get {
                if (_localMedia == null) {
                    _localMedia = new RelationList<DBTrackInfo, DBLocalMedia>(this);
                }
                return _localMedia; 
            }
        } RelationList<DBTrackInfo, DBLocalMedia> _localMedia;


        [DBRelation(AutoRetrieve = true, Filterable = false)]
        public RelationList<DBTrackInfo, DBSourceMusicVideoInfo> SourceMusicVideoInfo
        {
            get
            {
                if (_sourceIDs == null)
                {
                    _sourceIDs = new RelationList<DBTrackInfo, DBSourceMusicVideoInfo>(this);
                }
                return _sourceIDs;
            }
        } RelationList<DBTrackInfo, DBSourceMusicVideoInfo> _sourceIDs;


        public DBSourceMusicVideoInfo GetSourceMusicVideoInfo(int scriptID)
        {
            return DBSourceMusicVideoInfo.GetOrCreate(this, scriptID);
        }

        public DBSourceMusicVideoInfo GetSourceMusicVideoInfo(DBSourceInfo source)
        {
            return DBSourceMusicVideoInfo.GetOrCreate(this, source);
        }





        [DBRelation(AutoRetrieve = true)]
        public RelationList<DBTrackInfo, DBUserMusicVideoSettings> UserSettings
        {
            get
            {
                if (_userSettings == null)
                {
                    _userSettings = new RelationList<DBTrackInfo, DBUserMusicVideoSettings>(this);
                }
                return _userSettings;
            }
        } RelationList<DBTrackInfo, DBUserMusicVideoSettings> _userSettings;

        [DBRelation(AutoRetrieve = true, Filterable = false)]
        public RelationList<DBTrackInfo, DBArtistInfo> ArtistInfo {
            get {
                if (__artist == null) {
                    __artist = new RelationList<DBTrackInfo, DBArtistInfo>(this);
                }
                return __artist;
            }
        } RelationList<DBTrackInfo, DBArtistInfo> __artist;

        [DBRelation(AutoRetrieve = true, Filterable = false)]
        public RelationList<DBTrackInfo, DBAlbumInfo> AlbumInfo
        {
            get
            {
                if (_album == null)
                {
                    _album = new RelationList<DBTrackInfo, DBAlbumInfo>(this);
                }
                return _album;
            }
        } RelationList<DBTrackInfo, DBAlbumInfo> _album;

        public DBUserMusicVideoSettings ActiveUserSettings
        {
            get
            {
                return UserSettings[0];
            }
        }
        #endregion

        #region General Management Methods
        
        // deletes this artist from the database and sets all related DBLocalMedia to ignored
        public void DeleteAndIgnore() {
            foreach (DBLocalMedia currFile in LocalMedia) {
                currFile.Ignored = true;
                currFile.Commit();
            }

            Delete();
        }

        public override void Delete()
        {
            if (this.ID == null)
            {
                base.Delete();
                return;
            }

            DBAlbumInfo a1 = null;
            if (this.AlbumInfo.Count > 0) a1 = this.AlbumInfo[0];
            DBArtistInfo a3 = null;
            if (this.ArtistInfo.Count > 0) a3 = this.ArtistInfo[0];

            base.Delete();
            if (a1 != null)
            {

                List<DBTrackInfo> a7 = GetEntriesByAlbum(a1);
                if (a7 == null || a7.Count == 0)
                {
                    logger.Info("Removing Album '{0}' .", a1.Album);
                    a1.Delete();
                }

            }
            if (a3 != null)
            {
                List<DBTrackInfo> a7 = GetEntriesByArtist(a3);
                if (a7 == null || a7.Count == 0)
                {
                    logger.Info("Removing Artist '{0}' .", a3.Artist);
                    a3.Delete();
                }

            }
        }

        // this should be changed to reflectively commit all sub objects and relation lists
        // and moved down to the DatabaseTable class.
        public override void Commit()
        {
            if (this.ID == null)
            {
                base.Commit();
                commitNeeded = true;
            }

            foreach (DBSourceMusicVideoInfo currInfo in SourceMusicVideoInfo)
            {
                currInfo.Commit();
            }
            base.Commit();
        }


        #endregion
        
        
        #region Database Management Methods

        public static DBTrackInfo Get(int id) {
            return mvCentralCore.DatabaseManager.Get<DBTrackInfo>(id);
        }

        public static List<DBTrackInfo> GetAll() {
            return mvCentralCore.DatabaseManager.Get<DBTrackInfo>(null);
        }

        
        /// <summary>
        /// Returns a list of DBArtistInfo objects that match the Track
        /// </summary>
        /// <param name="mbid"></param>
        /// <returns></returns>
        public static List<DBArtistInfo> GetEntriesByArtist(string artistmbid)
        {
                lock (lockList)
                {
                    DBField artistmbidField = DBField.GetField(typeof(DBArtistInfo), "MdID");
                    ICriteria criteria = new BaseCriteria(artistmbidField, "=", artistmbid);
                    return mvCentralCore.DatabaseManager.Get<DBArtistInfo>(criteria);
                }
        }

        /// <summary>
        /// Returns a list of DBArtistInfo objects that match the Track
        /// </summary>
        /// <param name="mbid"></param>
        /// <returns></returns>
        public static List<DBArtistInfo> GetEntriesByArtistName(string artistname)
        {
            lock (lockList)
            {
                DBField artistnameField = DBField.GetField(typeof(DBArtistInfo), "Artist");
                ICriteria criteria = new BaseCriteria(artistnameField, "=", artistname);
                return mvCentralCore.DatabaseManager.Get<DBArtistInfo>(criteria);
            }
        }
        
        /// <summary>
        /// Returns a list of DBArtistInfo objects in intenal id that match the Track
        /// </summary>
        /// <param name="mbid"></param>
        /// <returns></returns>
        public static List<DBArtistInfo> GetEntriesByIntIdArtist(string trackintid)
        {
            lock (lockList)
            {
                DBField artistmbidField = DBField.GetField(typeof(DBArtistInfo), "trackint_id");
                ICriteria criteria = new BaseCriteria(artistmbidField, "=", trackintid);
                return mvCentralCore.DatabaseManager.Get<DBArtistInfo>(criteria);
            }
        }

        /// <summary>
        /// Returns a list of DBAlbumInfo objects based on int id that match the Track
        /// </summary>
        /// <param name="mbid"></param>
        /// <returns></returns>
        public static List<DBAlbumInfo> GetEntriesByIntIdAlbum(string trackintid)
        {
            lock (lockList)
            {
                DBField albummbidField = DBField.GetField(typeof(DBAlbumInfo), "trackint_id");
                ICriteria criteria = new BaseCriteria(albummbidField, "=", trackintid);
                return mvCentralCore.DatabaseManager.Get<DBAlbumInfo>(criteria);
            }
        }

        /// <summary>
        /// Returns a list of DBTrackInfo objects that match the album
        /// </summary>
        /// <param name="mbid"></param>
        /// <returns></returns>
        public static List<DBTrackInfo> GetEntriesByAlbum(DBAlbumInfo mv)
        {
            if (mv == null) return null;
            lock (lockList)
            {   
                List<DBTrackInfo> results = new List<DBTrackInfo>();
                List<DBTrackInfo> db1 = DBTrackInfo.GetAll();
                foreach (DBTrackInfo db2 in db1)
                {
                    if (db2.AlbumInfo.Count>0)
                      if (mv == db2.AlbumInfo[0]) results.Add(db2); 
                }
                return results;
            }
        }

        /// <summary>
        /// Returns a list of DBTrackInfo objects that match the artist
        /// </summary>
        /// <param name="mbid"></param>
        /// <returns></returns>
        public static List<DBTrackInfo> GetEntriesByArtist(DBArtistInfo mv)
        {
            if (mv == null) return null;
            lock (lockList)
            {
                List<DBTrackInfo> results = new List<DBTrackInfo>();
                List<DBTrackInfo> db1 = DBTrackInfo.GetAll();
                foreach (DBTrackInfo db2 in db1)
                {
                    if (db2.ArtistInfo.Count > 0)
                        if (mv == db2.ArtistInfo[0]) results.Add(db2);
                }
                return results;
            }
        }

        /// <summary>
        /// Returns a list of DBAlbumInfo objects that match the Track
        /// </summary>
        /// <param name="mbid"></param>
        /// <returns></returns>
        public static List<DBAlbumInfo> GetEntriesByAlbum(string albummbid)
        {
            lock (lockList)
            {
                DBField albummbidField = DBField.GetField(typeof(DBAlbumInfo), "MdID");
                ICriteria criteria = new BaseCriteria(albummbidField, "=", albummbid);
                return mvCentralCore.DatabaseManager.Get<DBAlbumInfo>(criteria);
            }
        }

        /// <summary>
        /// Returns a list of DBAlbumInfo objects that match the Track
        /// </summary>
        /// <param name="mbid"></param>
        /// <returns></returns>
        public static List<DBAlbumInfo> GetEntriesByAlbumName(string albumname)
        {
            lock (lockList)
            {
                DBField albumnameField = DBField.GetField(typeof(DBAlbumInfo), "Album");
                ICriteria criteria = new BaseCriteria(albumnameField, "=", albumname);
                return mvCentralCore.DatabaseManager.Get<DBAlbumInfo>(criteria);
            }
        }

        #endregion

        public override int CompareTo(object obj)
        {
            if (obj.GetType() == typeof(DBTrackInfo)) {
                return SortBy.CompareTo(((DBTrackInfo)obj).SortBy);
            }
            return 0;
        }

        public override string ToString() {
            return Track;
        }

        public bool PopulateDateAdded()
        {
            String dateOption = mvCentralCore.Settings.DateImportOption;

            if (dateOption == null)
                dateOption = "created";

            if (LocalMedia.Count == 0 || LocalMedia[0].ImportPath.GetDriveType() == DriveType.CDRom)
                dateOption = "current";

            switch (dateOption)
            {
                case "modified":
                    if (!LocalMedia[0].IsAvailable)
                        return false;

                    DateAdded = LocalMedia[0].File.LastWriteTime;
                    break;
                case "current":
                    DateAdded = DateTime.Now;
                    break;
                default:
                    if (!LocalMedia[0].IsAvailable)
                        return false;

                    DateAdded = LocalMedia[0].File.CreationTime;
                    break;
            }

            return true;
        }

    }
}
