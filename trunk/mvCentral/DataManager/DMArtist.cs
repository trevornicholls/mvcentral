using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SQLite.NET;
using NLog;
using MediaPortal.Configuration;
using System.Collections;
using MusicVideos;

namespace MusicVideos.Data
{
    partial class DataManager
    {
        public ArrayList getAllArtists()
        {
            ArrayList output = new ArrayList();
            SQLiteResultSet rs = dbConn.Execute("SELECT * FROM Artists ORDER BY upper(artistName)");
            foreach (SQLiteResultSet.Row row in rs.Rows)
            {
                output.Add(new Artist(row.fields[0], row.fields[1], row.fields[2], row.fields[3]));
            }
            return output;
        }

        public string getBio(int id)
        {
            return dbConn.Execute("SELECT artistBio FROM Artists WHERE id = " + id).Rows[0].fields[0];
        }

        public Artist getArtist(int artistID)
        {
            return Artist.FromDB(artistID, dbConn);
        }

        public int addArtist(string name, string biography, string image)
        {
            logger.Info(String.Format("Adding {0} to database", name));
            name = name.Replace("\'", "\'\'");
            biography = biography.Replace("\'", "\'\'").Replace("&quot;", "\'\'");
            image = image.Replace("\'", "\'\'");
            SQLiteResultSet rs = dbConn.Execute("SELECT id FROM Artists WHERE artistName = '" + name + "'");
            try
            {
                return int.Parse(rs.Rows[0].fields[0]);
            }
            catch (Exception) { }
            dbConn.Execute("INSERT INTO Artists(artistName, artistBio, artistImage) VALUES('" + name + "', '" + biography + "', '" + image + "');");
            return int.Parse(dbConn.Execute("SELECT id FROM Artists WHERE artistName = '" + name + "'").Rows[0].fields[0]);
        }

        public int addArtist(Artist artist)
        {
            logger.Info(String.Format("Updating {0} in database", artist.ToString()));
            string name = artist.ToString().Replace("\'", "\'\'");
            string biography = artist.Bio.Replace("\'", "\'\'").Replace("&quot;", "\'\'");
            string image = artist.Image.Replace("\'", "\'\'");
            string tmpID = artist.ID;
            try
            {
                SQLiteResultSet rs = dbConn.Execute("SELECT * FROM Artists WHERE artistName='" + name + "'");
                if (rs.Rows.Count > 0)
                {
                    //We already have an artist in the database with the same name, move all videos over, and update that artist with these details
                    artist.ID = rs.Rows[0].fields[0];
                    dbConn.Execute("UPDATE Videos SET artistID=" + artist.ID + " WHERE artistID=" + tmpID);
                }
            }
            catch (Exception ee)
            {
                logger.Info("Updating artist, ERROR: " + ee.ToString());
            }
            try
            {
                dbConn.Execute("UPDATE Artists SET artistName='" + name + "', artistBio='" + biography + "', artistImage='" + image + "' WHERE id=" + artist.ID);
            }
            catch (Exception)
            { return 0; }
            return 1;
        }
    }
}
