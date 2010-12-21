using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using mvCentral.Database;
using NLog;
using System.Net;
using Cornerstone.Database;
using System.Web;
using mvCentral.LocalMediaManagement;
using mvCentral.SignatureBuilders;
using System.Reflection;
using System.Threading;
using System.Globalization;
using Cornerstone.Extensions;

namespace mvCentral.DataProviders
{
    public class ManualProvider : IMusicVideoProvider
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        // we should be using the MusicVideo object but we have to assign it before locking which 
        // is not good if the thread gets interupted after the asssignment, but before it gets 
        // locked. So we use this dumby var.
        private String lockObj = "";     

        public string Name {
            get {
                return "Manual Dummy Data";
            }
        }

        public string Version {
            get {
                return "Internal";
            }
        }

        public string Author {
            get { return "Music Videos Team"; }
        }

        public string Description {
            get { return "Dummo provider for manual addition."; }
        }

        public string Language {
            get { return ""; }
        }

        public string LanguageCode {
            get { return ""; }
        }

        public bool ProvidesDetails
        {
            get { return false; }
        }

        public bool ProvidesAlbumArt {
            get { return false; }
        }

        public bool ProvidesArtistArt {
            get { return false; }
        }

        public bool ProvidesTrackArt
        {
            get { return false; }
        }

        public bool GetArtistArt(DBArtistInfo mv)
        {
            return false;
        }

        public bool GetAlbumArt(DBAlbumInfo mv)
        {
                return false;
        }

        public bool GetTrackArt(DBTrackInfo mv)
        {
                return false;
        }

       

        public bool GetArtwork(DBTrackInfo mv)
        {
            return false;
        }


        public bool GetDetails(DBTrackInfo mv)
        {
            throw new NotImplementedException();
        }

        public List<DBTrackInfo> Get(MusicVideoSignature mvSignature) {
            throw new NotImplementedException();
        }

        public UpdateResults Update(DBTrackInfo mv) {
            throw new NotImplementedException();
        }

    }
}
