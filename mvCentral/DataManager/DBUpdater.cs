using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using SQLite.NET;

namespace MusicVideos.Data
{
    public static class DBUpdater
    {
        static SQLiteClient dbConn;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static void Update(string DBFileName)
        {
            if (!System.IO.File.Exists(DBFileName))
                CreateDB(DBFileName);
            else
            {
                dbConn = new SQLiteClient(DBFileName);
                string dbVersion = "0.1", currentVersion = "0.5";
                try
                {
                    dbVersion = dbConn.Execute("SELECT version FROM Settings").Rows[0].fields[0];
                }
                catch { }

                if (dbVersion == currentVersion)
                    logger.Info("Database already up to date");
                else
                    doUpdates(dbVersion);
                dbConn.Close();
            }
        }

        private static void doUpdates(string dbVersion)
        {
            try
            {
            dbConn.Execute("BEGIN TRANSACTION");
                switch (dbVersion)
                {
                    case "0.1":
                        Update04();
                        goto case "0.4";
                    case "0.4":
                        Update041();
                        goto case "0.41";
                    case "0.41":
                        Update05();
                        goto case "0.5";
                    case "0.5":
                        //Current version
                        break;
                }
                dbConn.Execute("COMMIT TRANSACTION");
            }
            catch (Exception e)
            {
                logger.Info(e.ToString());
                dbConn.Execute("ROLLBACK TRANSACTION");
            }
        }

        private static void CreateDB(string dbFilename)
        {
            //This should always be the latest schema
            dbConn = new SQLiteClient(dbFilename);
            dbConn.Execute("CREATE TABLE Videos (id integer PRIMARY KEY AUTOINCREMENT, artistID integer, path char(300), title char(100), playCount integer, rating integer, year integer, thumb char(300), dateAdded char(50))");
            dbConn.Execute("CREATE TABLE Artists (id integer PRIMARY KEY AUTOINCREMENT, artistName char(100), artistBio char(700), artistImage char(300))");
            dbConn.Execute("CREATE TABLE Settings (ffmpeg char(300), thumbTime char(3), pluginName char(50), extensions char(150), version char(10), DT char (150))");
            dbConn.Execute("CREATE TABLE Folders (id integer PRIMARY KEY AUTOINCREMENT, folder char(300), expression char(100))");
            dbConn.Execute("CREATE TABLE DVD (id integer PRIMARY KEY AUTOINCREMENT, artistID integer, dvdTitle char(75), coverArt char(150))");
            dbConn.Execute("CREATE TABLE DVDTracks (id integer PRIMARY KEY AUTOINCREMENT, dvdID integer, trackName char(75), playcount integer, reserved char(150))");
            dbConn.Execute("INSERT INTO Settings VALUES('c:\\ffmpeg.exe', '5', 'Music Videos', '.avi.mpg.mkv.wmv.divx.ts.ogm.vob.mp4.m4v.mpeg.m2v', '0.4, '')");
            logger.Info("Database created");
            dbConn.Close();
        }

        private static void Update04()
        {
            string ffmpeg, time, name;
            ffmpeg = dbConn.Execute("SELECT ffmpeg FROM Settings").Rows[0].fields[0];
            time = dbConn.Execute("SELECT thumbTime FROM Settings").Rows[0].fields[0];
            name = dbConn.Execute("SELECT pluginName FROM Settings").Rows[0].fields[0];
            dbConn.Execute("ALTER TABLE Videos ADD dateAdded char(50)");
            dbConn.Execute("UPDATE Videos SET dateAdded = '" + DateTime.Now.ToOADate() + "'");
            dbConn.Execute("DROP TABLE Settings");
            dbConn.Execute("CREATE TABLE Settings (ffmpeg char(300), thumbTime char(3), pluginName char(50), extensions char(150), version char(10))");
            dbConn.Execute("INSERT INTO Settings VALUES('" + ffmpeg + "', '" + time + "', '" + name + "', '.avi.mpg.mkv.wmv.divx.ts.ogm.vob.mp4.m4v.mpeg.m2v', '0.4')");
            logger.Info("Database updated for V0.4");
        }

        private static void Update041()
        {           
            dbConn.Execute("ALTER TABLE Settings ADD DT char (150)");
            dbConn.Execute("UPDATE Settings SET version = '0.41'");
            logger.Info("Database updated for V0.41");
        }

        private static void Update05()
        {           
            dbConn.Execute("CREATE TABLE DVD (id integer PRIMARY KEY AUTOINCREMENT, artistID integer, dvdTitle char(75), coverArt char(150), path char(150))");
            dbConn.Execute("CREATE TABLE DVDTracks (id integer PRIMARY KEY AUTOINCREMENT, dvdID integer, trackName char(75), playcount integer, reserved char(150))");
            dbConn.Execute("UPDATE Settings SET version = '0.5'");
            logger.Info("Database updated for V0.5");
        }
    }
}
