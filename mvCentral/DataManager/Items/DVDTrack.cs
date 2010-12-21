using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace MusicVideos
{
    public class DVDTrack
    {       
        private string trackName;
        private string reserved;
        private DVDItem parentDVD;
        private int playCount;
        private int trackID;

        public DVDTrack(int trackID, DVDItem parentDVD, string trackName, int playCount)
        {
            this.trackID = trackID;
            this.parentDVD  = parentDVD;
            this.trackName = trackName;
            this.playCount = playCount;
            //this.reserved;
        }

        public DVDTrack()
        {
            //Empty constructor
        }

        public void Save()
        {
            //To Implement
        }

        override public string ToString()
        {
            return trackName;
        }

        public int PlayCount
        {
            get { return playCount; }
            set { playCount = value; }
        }   
   
        public int TrackID
        {
            get { return trackID; }
            set { trackID = value; }
        }  

        public string Title
        {
            get { return trackName; }
            set { trackName = value; }
        }

        public DVDItem ParentDVD
        {
            get {return parentDVD;}
            set { parentDVD = value; }
        }
    }
}