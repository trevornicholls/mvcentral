using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SQLite.NET;
using MediaPortal.Configuration;
using System.Collections;
using MusicVideos;
using System.Windows.Forms;
using NLog;

namespace MusicVideos.Data
{
    public partial class DataManager
    {
        private static string dbFilename;
        private static SQLiteClient dbConn;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        public DataManager(string dbName) 
        {
            logger.Info("Starting data manager");
            dbFilename = Config.GetFile(Config.Dir.Database, dbName);
            DBUpdater.Update(dbFilename);
            dbConn = new SQLiteClient(dbFilename);
        }

        #region settings
        public SQLiteResultSet getWatch()
        {
            logger.Info("Retrieving watch folders from Database");
            return dbConn.Execute("SELECT * FROM Folders");
        }

        public SQLiteResultSet getSettings()
        {
            logger.Info("Retrieving settings from Database");
            return dbConn.Execute("SELECT * FROM Settings");
        }

        public bool addWatch(string folder, string expression)
        {
            logger.Info("Adding watch folder to Database");
            try
            {
                dbConn.Execute("INSERT INTO Folders(folder, expression) VALUES('" + folder.Replace("\'", "\'\'") + "', '" + expression + "')");
                return true;
            }
            catch (Exception)
            { return false; }
        }

        public string getFFMPEG()
        {
            SQLiteResultSet rs = dbConn.Execute("SELECT ffmpeg FROM Settings");
            return rs.Rows[0].fields[0];
        }

        public string getExtensions()
        {
            SQLiteResultSet rs = dbConn.Execute("SELECT extensions FROM Settings");
            return rs.Rows[0].fields[0];
        }

        public string getSeconds()
        {
            SQLiteResultSet rs = dbConn.Execute("SELECT thumbTime FROM Settings");
            return rs.Rows[0].fields[0];
        }

        public string getPluginName()
        {
            SQLiteResultSet rs = dbConn.Execute("SELECT pluginName FROM Settings");
            return rs.Rows[0].fields[0];
        }
#endregion

        public string[] getStats()
        {
            string[] output = new string[6];
            output[0] = dbConn.Execute("SELECT COUNT(id) FROM Videos").Rows[0].fields[0]; //# of videos
            output[1] = dbConn.Execute("SELECT COUNT(id) FROM Artists").Rows[0].fields[0]; //# of artists
            SQLiteResultSet rs = dbConn.Execute("SELECT title, thumb FROM Videos ORDER BY playcount");
            output[2] = rs.Rows[0].fields[0];
            output[3] = rs.Rows[0].fields[1];
            rs = dbConn.Execute("SELECT SUM(playcount), artistID FROM Videos GROUP BY artistID ORDER BY SUM(playcount) DESC");
            string topID = rs.Rows[0].fields[1];
            rs = dbConn.Execute("SELECT artistName, artistImage FROM Artists WHERE id = " + topID);
            output[4] = rs.Rows[0].fields[0];
            output[5] = rs.Rows[0].fields[1];
            rs = null;
            return output;
        }

        public string cleanUp()
        {
            SQLiteResultSet rs = dbConn.Execute("SELECT * FROM Videos");
            int counter = 0;
            foreach (SQLiteResultSet.Row row in rs.Rows)
            {
                if (!(System.IO.File.Exists(row.fields[2])))
                {
                    dbConn.Execute("DELETE FROM Videos WHERE id = " + row.fields[0]);
                    System.IO.File.Delete(row.fields[7]);
                    counter++;
                }
            }
            return counter + " missing files removed.";
        }

        public bool Contains(string path)
        {
            path = path.Replace("\'", "\'\'");
            SQLiteResultSet rs = dbConn.Execute("SELECT id FROM Videos WHERE path = '" + path + "'");
            if (rs.Rows.Count > 0)
                return true;
            else
                return false;
        }

        public bool Remove(string table, int rowid)
        {
            logger.Info("Removing data from DB");
            try
            {
                dbConn.Execute("DELETE FROM " + table + " WHERE id = '" + rowid + "'");
                return true;
            }
            catch (Exception)
            { return false; }
        }

        public SQLiteResultSet getAll()
        {
            return dbConn.Execute("SELECT * FROM Artists a, Videos v WHERE a.id = v.artistID ORDER BY upper(a.artistName)");
        }
        
        public void Execute(string sql)
        {
            logger.Info("Executing SQL statement " + sql);
            dbConn.Execute(sql);
        }

        public SQLiteResultSet Execute(string sql, bool es)
        {
            logger.Info("Executing SQL statement " + sql + " and returning RS");
            return dbConn.Execute(sql);
        }

        public void play(string filename)
        {
            int i = 0;
            try
            {
                i = int.Parse(dbConn
                    .Execute("SELECT playCount FROM Videos WHERE path = '" + filename.Replace("\'", "\'\'") + "'").Rows[0].fields[0]);
            }
            catch{}
            try
            {
                dbConn.Execute("UPDATE Videos SET playCount = " +
                        (i + 1).ToString()
                        + " WHERE path = '" + filename.Replace("\'", "\'\'") + "'");
            }
            catch { }
        }
    }
}
