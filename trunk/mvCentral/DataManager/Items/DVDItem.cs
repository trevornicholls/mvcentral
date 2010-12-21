using System;
using System.Collections;
using System.Linq;
using System.Text;
using SQLite.NET;

namespace MusicVideos
{
    public class DVDItem
    {
        private Artist DVDArtist;
        private string DVDName;
        private string filePath;
        private string coverArt;       
        private int dvdID;
        private ArrayList tracks;
        private string[] reserved;       

        public DVDItem(Artist DVDArtist, string DVDName, string filePath, string coverArt, int dvdID)
        {
            this.DVDArtist = DVDArtist;
            this.DVDName = DVDName;
            this.filePath = filePath;
            this.coverArt = coverArt;
            this.tracks = new ArrayList();
            //this.reserved;
        }

        public DVDItem()
        {
            //Empty constructor
        }

        public ArrayList Tracks
        {
            set { tracks = value; }
            get { return tracks; }
        }

        public static DVDItem FromDB(int DVDID, SQLite.NET.SQLiteClient dbConn)
        {
            SQLiteResultSet.Row row = dbConn.Execute("SELECT * FROM DVD WHERE id = " + DVDID).Rows[0];
            string tmpid = row.fields[0];
            Artist tmpArtist = Artist.FromDB(int.Parse(row.fields[1]), dbConn);
            string tmpTitle = row.fields[2];
            string tmpArt = row.fields[3];
            string tmpPath = row.fields[4];
            DVDItem tmpDVD = new DVDItem(tmpArtist, tmpTitle, tmpPath, tmpArt, int.Parse(tmpid));
            SQLiteResultSet rs = MusicVideosCore.dm.Execute("SELECT * FROM DVDTracks WHERE dvdID = " + DVDID, true);
            foreach (SQLiteResultSet.Row row1 in rs.Rows)
            {
                tmpDVD.Tracks.Add(new DVDTrack(int.Parse(row1.fields[0]), tmpDVD, row1.fields[2], int.Parse(row1.fields[3])));
            }           
            return tmpDVD;
        }

        public void Save()
        {
            MusicVideosCore.dm.addDVD(this);
        }

        override public string ToString()
        {
            return DVDArtist + " - " + DVDName;
        }

        public int TrackCount
        {
            get { return tracks.Count; }           
        }      

        public string File
        {
           get{ return filePath; }
            set { filePath = value; }
        }

        public string CoverArt
        {
            get { return coverArt; }
            set { coverArt = value; }
        }

        public string Title
        {
            get { return DVDName; }
            set { DVDName = value; }
        }

        public Artist Artist
        {
            get {return DVDArtist;}
            set { DVDArtist = value; }
        }

        public int DVDID
        {
            get { return dvdID; }
            set { dvdID = value; }
        }

    }
}