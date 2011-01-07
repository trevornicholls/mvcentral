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
    [DBTableAttribute("artist_info")]
    public class DBArtistInfo: DBBasicInfo, IComparable, IAttributeOwner {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        public DBArtistInfo()
            : base() {
        }

        #region Database Fields

        [DBField(AllowDynamicFiltering=false)]
        public string Artist {
            get { return _artist; }
            set { 
                _artist = value;
                Basic = value;
                PopulateSortBy();
                commitNeeded = true;
            }
        } private string _artist;

        #endregion

        #region General Management Methods
        

        #endregion

        #region Database Management Methods

        public static DBArtistInfo Get(int id) {
            return mvCentralCore.DatabaseManager.Get<DBArtistInfo>(id);
        }

        public static new List<DBArtistInfo> GetAll() {
            return mvCentralCore.DatabaseManager.Get<DBArtistInfo>(null);
        }

        public static DBArtistInfo Get(string Artist)
        {
            if (Artist.Trim().Length == 0) return null;
            foreach (DBArtistInfo db1 in GetAll())
            {
                if (String.Equals(Artist, db1.Artist)) return db1;
                if (String.Equals(Artist, db1.MdID)) return db1;

            }
            return null;
        }

        public static DBArtistInfo Get(DBTrackInfo mv)
        {
            if (mv.ArtistInfo.Count == 0) return null;
            foreach (DBArtistInfo db1 in GetAll())
            {
                if (db1.MdID.Trim().Length > 0)
                    if (String.Equals(db1.MdID, mv.ArtistInfo[0].MdID)) return db1;
                if (db1.Artist.Trim().Length > 0)
                    if (String.Equals(db1.Artist, mv.ArtistInfo[0].Artist)) return db1;

            }
            return null;
        }

        public static DBArtistInfo GetOrCreate(DBTrackInfo mv)
        {
            DBArtistInfo rtn = mv.ArtistInfo[0];
            if (rtn != null)
                return rtn;

            rtn = new DBArtistInfo();
            mv.ArtistInfo.Add(rtn);
            return rtn;
        }
        #endregion

        public override int CompareTo(object obj) {
            if (obj.GetType() == typeof(DBArtistInfo)) {
                return SortBy.CompareTo(((DBArtistInfo)obj).SortBy);
            }
            return 0;
        }

        public override string ToString() {
            return Artist;
        }

    }
}
