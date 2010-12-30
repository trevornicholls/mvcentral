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
    public class LocalProvider : IMusicVideoProvider
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private DBTrackInfo mv;

        // we should be using the MusicVideo object but we have to assign it before locking which 
        // is not good if the thread gets interupted after the asssignment, but before it gets 
        // locked. So we use this dumby var.
        private String lockObj = "";     

        public string Name {
            get {
                return "Local Data";
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
            get { return "Returns artwork already available on the local system."; }
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
            get { return true; }
        }

        public bool ProvidesArtistArt {
            get { return true; }
        }

        public bool ProvidesTrackArt
        {
            get { return true; }
        }

        public bool GetArtistArt(DBArtistInfo mv)
        {
            if (mv == null) 
                return false;

            // if we already have a artist move on for now
            if (mv.ArtFullPath.Trim().Length > 0)
                return false;

            bool found = false;
            
//            found &= getArtistArtFromArtistArtFolder(mv);
            found &= getOldArtistArt(mv);

            return found;
        }

        public bool GetAlbumArt(DBAlbumInfo mv)
        {
            if (mv == null)
                return false;

            // if we already have a artist move on for now
            if (mv.ArtFullPath.Trim().Length > 0)
                return false;
 
            bool found = false;

//            found &= getAlbumArtFromAlbumArtFolder(mv);
            found &= getOldAlbumArt(mv);

            return found;
        }

        public bool GetTrackArt(DBTrackInfo mv)
        {
            if (mv == null)
                return false;
            if (this.mv == null) this.mv = mv;

            // if we already have a trackimage move on for now
            if (mv.ArtFullPath.Trim().Length > 0)
                return false;

            bool found = false;

            found &= getTrackArtFromTrackArtFolder(mv);
            found &= getOldTrackArt(mv);

            return found;
        }

        private bool getArtistArtFromArtistArtFolder(DBArtistInfo mv)
        {
            if (mv == null)
                return false;

            // grab a list of possible filenames for the artist based on the user pattern
            string pattern = mvCentralCore.Settings.ArtistArtworkFilenamePattern;
            List<string> filenames = getPossibleNamesFromPattern(pattern, this.mv);

            // check the ArtistArt folder for the user patterned ArtistArt
            string artistArtFolderPath = mvCentralCore.Settings.ArtistArtFolder;
            FileInfo newArtistArt = getFirstFileFromFolder(artistArtFolderPath, filenames);
            if (newArtistArt != null && newArtistArt.Exists)
            {
                mv.ArtFullPath = newArtistArt.FullName;
                logger.Info("Loaded artistart from " + newArtistArt.FullName);
                return true;
            }

            return false;
        }


        // check for artistart in the artistart folder loaded from previous installs
        private bool getOldArtistArt(DBArtistInfo mv)
        {
            bool found = false;

            string artistartFolderPath = mvCentralCore.Settings.ArtistArtFolder;
            DirectoryInfo artistartFolder = new DirectoryInfo(artistartFolderPath);

            string safeName = mv.Artist.Replace(' ', '.').ToValidFilename();
            Regex oldArtistArtRegex = new Regex("^{?" + Regex.Escape(safeName) + "}? \\[-?\\d+\\]\\.(jpg|png)");

            foreach (FileInfo currFile in artistartFolder.GetFiles())
            {
                if (oldArtistArtRegex.IsMatch(currFile.Name))
                {
                    found &= mv.AddArtFromFile(currFile.FullName);
                }
            }

            return found;
        }

        private bool getAlbumArtFromAlbumArtFolder(DBAlbumInfo mv)
        {
            if (mv == null)
                return false;

            // grab a list of possible filenames for the albumart based on the user pattern
            string pattern = mvCentralCore.Settings.AlbumArtworkFilenamePattern;
            List<string> filenames = getPossibleNamesFromPattern(pattern, this.mv);

            // check the albumart folder for the user patterned albumart
            string albumArtFolderPath = mvCentralCore.Settings.AlbumArtFolder;
            FileInfo newAlbumArt = getFirstFileFromFolder(albumArtFolderPath, filenames);
            if (newAlbumArt != null && newAlbumArt.Exists)
            {
                mv.ArtFullPath = newAlbumArt.FullName;
                logger.Info("Loaded Albumimage from " + newAlbumArt.FullName);
                return true;
            }

            return false;
        }

        // check for albumimages in the album folder loaded from previous installs
        private bool getOldAlbumArt(DBAlbumInfo mv)
        {
            bool found = false;

            string AlbumArtFolderPath = mvCentralCore.Settings.AlbumArtFolder;
            DirectoryInfo albumartFolder = new DirectoryInfo(AlbumArtFolderPath);

            string safeName = mv.Album.Replace(' ', '.').ToValidFilename();
            Regex oldtrackRegex = new Regex("^{?" + Regex.Escape(safeName) + "}? \\[-?\\d+\\]\\.(jpg|png)");

            foreach (FileInfo currFile in albumartFolder.GetFiles())
            {
                if (oldtrackRegex.IsMatch(currFile.Name))
                {
                    found &= mv.AddArtFromFile(currFile.FullName);
                }
            }

            return found;
        }


        private bool getTrackArtFromTrackArtFolder(DBTrackInfo mv)
        {
            if (mv == null)
                return false;

            // grab a list of possible filenames for the artistart based on the user pattern
            string pattern = mvCentralCore.Settings.TrackArtworkFilenamePattern;
            List<string> filenames = getPossibleNamesFromPattern(pattern, mv);

            // check the artistart folder for the user patterned artistarts
            string trackartFolderPath = mvCentralCore.Settings.TrackArtFolder;
            FileInfo newTrackArt = getFirstFileFromFolder(trackartFolderPath, filenames);
            if (newTrackArt != null && newTrackArt.Exists)
            {
                mv.ArtFullPath = newTrackArt.FullName;
                logger.Info("Loaded trackimage from " + newTrackArt.FullName);
                return true;
            }

            return false;
        }

         // check for trackimages in the track folder loaded from previous installs
        private bool getOldTrackArt(DBTrackInfo mv)
        {
            bool found = false;

            string trackartFolderPath = mvCentralCore.Settings.TrackArtFolder;
            DirectoryInfo trackartFolder = new DirectoryInfo(trackartFolderPath);

            string safeName = mv.Track.Replace(' ', '.').ToValidFilename();
            Regex oldtrackRegex = new Regex("^{?" + Regex.Escape(safeName) + "}? \\[-?\\d+\\]\\.(jpg|png)");

            foreach (FileInfo currFile in trackartFolder.GetFiles())
            {
                if (oldtrackRegex.IsMatch(currFile.Name))
                {
                    found &= mv.AddArtFromFile(currFile.FullName);
                }
            }

            return found;
        }
        
        

        public bool GetArtwork(DBTrackInfo mv)
        {
            try {
                bool found = false;
                if (this.mv == null) this.mv = mv;
                found &= getArtistArtFromArtistArtFolder(mv.ArtistInfo[0]);
                found &= getOldArtistArt(mv.ArtistInfo[0]);

                found &= getAlbumArtFromAlbumArtFolder(mv.AlbumInfo[0]);
                found &= getOldAlbumArt(mv.AlbumInfo[0]);

                found &= getTrackArtFromTrackArtFolder(mv);
                found &= getOldTrackArt(mv);

                return found;
            }
            catch (Exception e) {
                if (e.GetType() == typeof(ThreadAbortException))
                    throw e;
                logger.Warn("Unexpected problem loading artwork via LocalProvider.");
            }

            return false;
        }

        // parses and replaces variables from a filename based on the pattern supplied
        // returning a list of possible file matches
        private List<string> getPossibleNamesFromPattern(string pattern, DBTrackInfo mv)
        {
            // try to create our filename(s)
            this.mv = mv;
            lock (lockObj){
                Regex parser = new Regex("%(.*?)%", RegexOptions.IgnoreCase);
                List<string> filenames = new List<string>();
                foreach (string currPattern in pattern.Split('|')) {
                    // replace db field patterns

                    string filename = parser.Replace(currPattern, new MatchEvaluator(dbNameParser)).Trim().ToLower();

                    // replace %filename% pattern
                    if (mv.LocalMedia.Count > 0)
                    {
                        string videoFileName = Path.GetFileNameWithoutExtension(mv.LocalMedia[0].File.Name);
                        filename = filename.Replace("%filename%", videoFileName);
                    }

                    filenames.Add(filename);
                }
                return filenames;
            }
        }

        private string dbNameParser(Match match) {
            // try to grab the field object
            string fieldName = match.Value.Substring(1, match.Length - 2);
            DBField  field = DBField.GetFieldByDBName(typeof(DBTrackInfo), fieldName);

            // if no dice, the user probably entered an invalid string.
            if (field == null && match.Value != "%filename") {
                logger.Error("Error parsing \"" + match.Value + "\" from local_art_pattern advanced setting. Not a database field name.");
                return match.Value;
            }

            return field.GetValue(mv).ToString();
        }

        // based on the filename list, returns the first file in the folder, otherwise null
        private FileInfo getFirstFileFromFolder(string folder, List<string> filenames) {
            foreach (string currFilename in filenames) {
                FileInfo newImage = new FileInfo(folder + "\\" + currFilename);
                if (newImage.Exists) 
                    return newImage;
            }

            return null;
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
