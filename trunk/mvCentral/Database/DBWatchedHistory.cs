﻿using System;
using System.Collections.Generic;
using System.Text;
using mvCornerstone.Database.Tables;
using mvCornerstone.Database;

namespace mvCentral.Database
{
    [DBTableAttribute("watched_history")]
    public class DBWatchedHistory : mvCentralDBTable
    {

        #region Database Fields

        [DBFieldAttribute]
        public DBUser User {
            get { return _user; }
            set {
                _user = value;
                commitNeeded = true;
            }
        } private DBUser _user;


        [DBFieldAttribute]
        public DBTrackInfo Movie {
            get { return _movie; }
            set {
                _movie = value;
                commitNeeded = true;
            }
        } private DBTrackInfo _movie;


        [DBFieldAttribute(FieldName = "date_watched")]
        public DateTime DateWatched {
            get { return _dateWatched; }

            set {
                _dateWatched = value;
                commitNeeded = true;
            }
        } private DateTime _dateWatched;



        #endregion
        
        public static void AddWatchedHistory(DBTrackInfo movie, DBUser user) {
            DBWatchedHistory history = new DBWatchedHistory();
            history.DateWatched = DateTime.Now;
            history.Movie = movie;
            history.User = user;

//            movie.WatchedHistory.Add(history);
            history.Commit();
            movie.Commit();
        }
    }
}
