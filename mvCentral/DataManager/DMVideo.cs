using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SQLite.NET;
using NLog;
using MediaPortal.Configuration;
using System.Collections;
using MusicVideos.Details;

namespace MusicVideos.Data
{
    partial class DataManager
    {

        public bool addSong(string artistID, string path, string title, string year, string thumb)
        {
            logger.Info(String.Format("Adding {0} to database", title));
            path = path.Replace("\'", "\'\'");
            title = title.Replace("\'", "\'\'");
            thumb = thumb.Replace("\'", "\'\'");
            string today = DateTime.Now.ToOADate().ToString();
            //try
            //{
                dbConn.Execute("INSERT INTO Videos(artistID, path, title, year, thumb, playCount, dateAdded) VALUES(" + artistID + ", '" + path + "', '" + title + "', '" + year + "', '" + thumb + "', 0, '" + today + "')");
                return true;
            //}
            //catch (Exception)
            //{ return false; }
        }

        public bool addSong(mvDetails video)
        {
            logger.Info(String.Format("Adding {0} to database", video.Name));
            string path = video.File.Replace("\'", "\'\'");
            string title = video.Name.Replace("\'", "\'\'");
            string thumb = video.Thumbnail.Replace("\'", "\'\'");
            string year = "19XX";
            string today = DateTime.Now.ToOADate().ToString();
            if (video.ArtistID == null)
                return false;
            try
            {
                dbConn.Execute("INSERT INTO Videos(artistID, path, title, year, thumb, playCount, dateAdded) VALUES(" + video.ArtistID + ", '" + path + "', '" + title + "', '" + year + "', '" + thumb + "', 0, '" + today + "')");
                return true;
            }
            catch (Exception)
            { return false; }
        }

        public ArrayList getAllVideos(int ArtistID)
        {
            ArrayList output = new ArrayList();
            SQLiteResultSet rs = dbConn.Execute("SELECT * FROM Videos WHERE artistID = " + ArtistID);
            foreach (SQLiteResultSet.Row row in rs.Rows)
            {
                output.Add(new mvDetails(row.fields[1], row.fields[3],
                    row.fields[2], row.fields[7], row.fields[4], row.fields[5]));
            }
            return output;
        }

        public ArrayList getAllVideos()
        {
            ArrayList output = new ArrayList();
            SQLiteResultSet rs = dbConn.Execute("SELECT * FROM Videos");
            foreach (SQLiteResultSet.Row row in rs.Rows)
            {
                output.Add(new mvDetails(row.fields[1], row.fields[3],
                    row.fields[2], row.fields[7], row.fields[4], row.fields[5]));
            }
            return output;
        }

        public ArrayList getNewestVideos()
        {         
            ArrayList output = new ArrayList();
            SQLiteResultSet rs = dbConn.Execute("SELECT * FROM Videos ORDER BY dateAdded DESC");
            foreach (SQLiteResultSet.Row row in rs.Rows)
            {
                output.Add(new mvDetails(row.fields[1], row.fields[3],
                    row.fields[2], row.fields[7], row.fields[4], row.fields[5]));
            }
            return output;
        }

        public string getThumb(string SongID)
        {
            SQLiteResultSet rs = dbConn.Execute("SELECT thumb FROM Videos WHERE id = " + SongID);
            return rs.Rows[0].fields[0];
        }

        public string getSongName(string SongID)
        {
            SQLiteResultSet rs = dbConn.Execute("SELECT title FROM Videos WHERE id = " + SongID);
            return rs.Rows[0].fields[0];
        }

        public ArrayList getAllVideos(int Average, bool above)
        {
            ArrayList output = new ArrayList();
            string sqlCMD = "";
            if (above)
                sqlCMD = "SELECT * FROM Videos WHERE playCount >= " + Average;
            else
                sqlCMD = "SELECT * FROM Videos WHERE playCount <= " + Average;
            SQLiteResultSet rs = dbConn.Execute(sqlCMD);
            foreach (SQLiteResultSet.Row row in rs.Rows)
            {
                output.Add(new mvDetails(row.fields[1], row.fields[3],
                    row.fields[2], row.fields[7], row.fields[4], row.fields[5]));
            }
            return output;
        }

        public void UpdateSongName(string SongID, string newname)
        {
            newname = newname.Replace("\'", "\'\'");
            dbConn.Execute("UPDATE Videos SET title = '" + newname + "' WHERE id = " + SongID);
        }
    }
}
