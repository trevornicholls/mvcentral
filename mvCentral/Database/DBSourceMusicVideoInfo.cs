using System;
using System.Collections.Generic;
using System.Text;
using Cornerstone.Database.Tables;
using Cornerstone.Database;
using NLog;

namespace mvCentral.Database {
    [DBTable("source_musicvideo_info")]
    public class DBSourceMusicVideoInfo: mvCentralDBTable{
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [DBField]
        public DBSourceInfo Source {
            get { return source; }
            set {
                source = value;
                commitNeeded = true;
            }
        } private DBSourceInfo source;

        public int? ScriptID {
            get {
 //               if (source.IsScriptable())
 //                   return source.SelectedScript.Provider.ScriptID;

                return null;
            }
        }

        [DBField]
        public DBBasicInfo musicvideo {
            get { return mv; }
            set {
                mv = value;
                commitNeeded = true;
            }
        } private DBBasicInfo mv;

        [DBField(Default = null)]
        public String Identifier {
            get { return identifier; }
            set {
                identifier = value;
                commitNeeded = true;
            }
        } private String identifier;

        #region Database Management Methods

        public static DBSourceMusicVideoInfo Get(int id)
        {
            return mvCentralCore.DatabaseManager.Get<DBSourceMusicVideoInfo>(id);
        }

        public static List<DBSourceMusicVideoInfo> GetAll()
        {
            return mvCentralCore.DatabaseManager.Get<DBSourceMusicVideoInfo>(null);
        }

        #endregion

        #region Static Methods

        public static DBSourceMusicVideoInfo Get(DBTrackInfo mv, DBSourceInfo source)
        {
            foreach (DBSourceMusicVideoInfo currInfo in mv.SourceMusicVideoInfo)
                if (currInfo.Source == source)
                    return currInfo;

            return null;
        }

        public static DBSourceMusicVideoInfo Get(DBBasicInfo movie, int scriptID)
        {
//            return Get(movie, DBSourceInfo.GetFromScriptID(scriptID));
            return null;
        }

        public static DBSourceMusicVideoInfo GetOrCreate(DBTrackInfo mv, DBSourceInfo source)
        {
            DBSourceMusicVideoInfo rtn = Get(mv, source);
            if (rtn != null)
                return rtn;

            rtn = new DBSourceMusicVideoInfo();
            rtn.musicvideo = mv;
            rtn.Source = source;

            // if this is the IMDb data source, populate the id with the imdb_id field
//            if (rtn.ScriptID == 874902 && mv.ImdbID.Trim().Length == 9)
//                rtn.Identifier = mv.ImdbID;

            mv.SourceMusicVideoInfo.Add(rtn);
            return rtn;
        }

        public static DBSourceMusicVideoInfo GetOrCreate(DBBasicInfo mv, int scriptID)
        {
 //           return GetOrCreate(mv, DBSourceInfo.GetFromScriptID(scriptID));
            return null;
        }

        #endregion

        public override string ToString() {
//            return "DBSourceMusicVideoInfo [title='" + musicvideo.Title + "', identifier='" + identifier + "', provider='" + Source.Provider.Name + "']";
            return null;
        }

    }
}
