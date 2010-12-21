using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MusicVideos.Data;

namespace MusicVideos
{
    public class Artist
    {
        private string id;
        private string name;
        private string bio;
        private string image;

        public Artist(string id, string name, string bio, string image)
        {
            this.id = id;
            this.name = name;
            this.bio = bio;
            this.image = image;
        }

        public void Save()
        {
            MusicVideosCore.dm.addArtist(this);
        }

        public Artist()
        {}

        public static Artist FromDB(int artistID, SQLite.NET.SQLiteClient dbConn)
        {
            SQLite.NET.SQLiteResultSet rs = dbConn.Execute("SELECT * FROM Artists WHERE id = " + artistID);
            return (new Artist(rs.Rows[0].fields[0], rs.Rows[0].fields[1], rs.Rows[0].fields[2], rs.Rows[0].fields[3]));
        }

        public Artist(string id, string name)
        {
            this.id = id;
            this.name = name;
        }

        override public string ToString()
        {
            return name;
        }

        public string Name
        {
            set { name = value; }
            get { return name; }
        }

        public string ID
        {
            get { return id; }
            set { id = value; }
        }

        public string Bio
        {
            get { return bio; }
            set { bio = value; }
        }

        public string Image
        {
            get { return image; }
            set { image = value; }
        }
    }
}
