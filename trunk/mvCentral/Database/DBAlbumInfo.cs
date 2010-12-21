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
    [DBTableAttribute("album_info")]
    public class DBAlbumInfo: DBBasicInfo, IComparable, IAttributeOwner {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        public DBAlbumInfo()
            : base() {
        }

        #region Database Fields

        [DBField(AllowDynamicFiltering=false)]
        public string Album {
            get { return _album; }
            set { 
                _album = value;
                Basic = value;
                PopulateSortBy();
                commitNeeded = true;
            }
        } private string _album;

        #endregion

        #region General Management Methods
        

        #endregion

        #region Database Management Methods

        public static DBAlbumInfo Get(int id) {
            return mvCentralCore.DatabaseManager.Get<DBAlbumInfo>(id);
        }

        public static List<DBAlbumInfo> GetAll() {
            return mvCentralCore.DatabaseManager.Get<DBAlbumInfo>(null);
        }

        public static DBAlbumInfo Get(string Album)
        {
            if (Album.Trim().Length == 0) return null;
            foreach (DBAlbumInfo db1 in GetAll())
            {
                if (String.Equals(Album, db1.Album)) return db1;
                if (String.Equals(Album, db1.MdID)) return db1;

            }
            return null;
        }
        public static DBAlbumInfo Get(DBTrackInfo mv)
        {
            if (mv.AlbumInfo.Count == 0) return null;
            foreach (DBAlbumInfo db1 in GetAll())
            {
                if (db1.MdID.Trim().Length > 0)
                  if (String.Equals(db1.MdID, mv.AlbumInfo[0].MdID)) return db1;
                if (db1.Album.Trim().Length > 0)
                    if (String.Equals(db1.Album, mv.AlbumInfo[0].Album)) return db1;

            }
            return null;
        }


        /// <summary>
        /// Returns a list of DBArtistInfo objects that match the Album
        /// </summary>
        /// <param name="mbid"></param>
        /// <returns></returns>
        public static List<DBArtistInfo> GetEntriesByArtist(string artistmbid)
        {
            DBField artistmbidField = DBField.GetField(typeof(DBArtistInfo), "MdID");
            ICriteria criteria = new BaseCriteria(artistmbidField, "=", artistmbid);
            return mvCentralCore.DatabaseManager.Get<DBArtistInfo>(criteria);
        }

        /// <summary>
        /// Returns a list of DBTrackInfo objects that match the Album
        /// </summary>
        /// <param name="mbid"></param>
        /// <returns></returns>
        public static List<DBTrackInfo> GetEntriesByTrack(string trackmbid)
        {
            DBField trackmbidField = DBField.GetField(typeof(DBTrackInfo), "MdID");
            ICriteria criteria = new BaseCriteria(trackmbidField, "=", trackmbid);
            return mvCentralCore.DatabaseManager.Get<DBTrackInfo>(criteria);
        }

        #endregion

        public override int CompareTo(object obj)
        {
            if (obj.GetType() == typeof(DBAlbumInfo)) {
                return SortBy.CompareTo(((DBAlbumInfo)obj).SortBy);
            }
            return 0;
        }

        public override string ToString() {
            return Album;
        }

    }
}
