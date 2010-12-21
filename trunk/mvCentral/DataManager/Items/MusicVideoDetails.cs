using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicVideos.Details
{
    public class mvDetails
    {
        private string Artistid;
        private string Songid;
        private string SongName;
        private string filePath;
        private string thumbPath;
        private string playCount;
        private string rating;
        //private string year;  Not yet implemented

        public mvDetails(string Artistid, string name, string filePath, string thumbPath, string playCount, string rating)
        {
            this.Artistid = Artistid;
            this.SongName = name;
            this.filePath = filePath;
            this.thumbPath = thumbPath;
            this.playCount = playCount;
            this.rating = rating;
        }

        public mvDetails()
        {
            //Empty constructor
        }


        public void Save()
        {
 //           MusicVidsMain.dm.addSong(this);
        }

        override public string ToString()
        {
            return SongName;
        }

        public int Playcount
        {
            get { return int.Parse(playCount); }
            set { int cnt = int.Parse(playCount); cnt++; }
        }

        public string File
        {
           get{ return filePath; }
            set { filePath = value; }
        }

        public string Thumbnail
        {
            get { return thumbPath; }
            set { thumbPath = value; }
        }

        public int Rating
        {
            get { return int.Parse(rating); }
            set { rating = value.ToString(); }
        }

        public string Name
        {
            get {return SongName;}
            set {SongName = value;}
        }

        public string ArtistID
        {
            get { return Artistid; }
            set { Artistid = value; }
        }

    }
}