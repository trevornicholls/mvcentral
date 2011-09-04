using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mvCentral.Database;
using System.IO;
using System.Threading;
using System.Drawing;
using mvCornerstone.Extensions;

namespace mvCentral.LocalMediaManagement.MusicVideoResources
{
    public class BasicArt: ImageResource {

        private static DBBasicInfo mvs = null;

        public BasicArt(DBBasicInfo mv)
        {
            mvs = mv;
        }

        public override string Filename {
            set {
                base.Filename = value;
                string thumbsFolder = null;
                if (mvs.GetType() == typeof(DBTrackInfo)) thumbsFolder = mvCentralCore.Settings.TrackArtThumbsFolder;
                if (mvs.GetType() == typeof(DBAlbumInfo)) thumbsFolder = mvCentralCore.Settings.AlbumArtThumbsFolder;
                if (mvs.GetType() == typeof(DBArtistInfo)) thumbsFolder = mvCentralCore.Settings.ArtistArtThumbsFolder;

                // build thumbnail filename
                FileInfo file = new FileInfo(Filename);
                ThumbFilename = thumbsFolder + "\\" + file.Name;
            }
        }

        // genrate a filename for a TrackArt. should be unique based on the source hash
        private static string GenerateFilename(string source) {
            string artFolder = null;
            if (mvs.GetType() == typeof(DBTrackInfo)) artFolder = mvCentralCore.Settings.TrackArtFolder;
            if (mvs.GetType() == typeof(DBAlbumInfo)) artFolder = mvCentralCore.Settings.AlbumArtFolder;
            if (mvs.GetType() == typeof(DBArtistInfo)) artFolder = mvCentralCore.Settings.ArtistArtFolder;
            
            
            string safeName = mvs.Basic.Replace(' ', '.').ToValidFilename();
            return artFolder + "\\{" + safeName + "} [" + source.GetHashCode() + "].jpg";
        }

        public static BasicArt FromUrl(DBBasicInfo mv, string url, out ImageLoadResults status)
        {
            return FromUrl(mv, url, false, out status);
        }

        public static BasicArt FromUrl(string title, string url, out ImageLoadResults status)
        {
            return FromUrl(mvs, url, false, out status);
        }

        public static BasicArt FromUrl(string title, string url, bool ignoreRestrictions, out ImageLoadResults status)
        {
            return FromUrl(mvs, url, ignoreRestrictions, out status);
        }

        public static BasicArt FromUrl(DBBasicInfo mv, string url, bool ignoreRestrictions, out ImageLoadResults status)
        {
            ImageSize minSize = null;
            ImageSize maxSize = new ImageSize();
            if (mvs == null) mvs = mv;
            if (!ignoreRestrictions)
            {
                minSize = new ImageSize();
                if (mvs.GetType() == typeof(DBTrackInfo))
                {
                    minSize.Width = mvCentralCore.Settings.MinimumTrackWidth;
                    minSize.Height = mvCentralCore.Settings.MinimumTrackHeight;
                }
                if (mvs.GetType() == typeof(DBAlbumInfo))
                {
                    minSize.Width = mvCentralCore.Settings.MinimumAlbumWidth;
                    minSize.Height = mvCentralCore.Settings.MinimumAlbumHeight;
                }
                if (mvs.GetType() == typeof(DBArtistInfo))
                {
                    minSize.Width = mvCentralCore.Settings.MinimumArtistWidth;
                    minSize.Height = mvCentralCore.Settings.MinimumArtistHeight;
                }
            }

            bool redownload = false;
            if (mvs.GetType() == typeof(DBTrackInfo))
            {
                maxSize.Width = mvCentralCore.Settings.MaximumTrackWidth;
                maxSize.Height = mvCentralCore.Settings.MaximumTrackHeight;
                redownload = mvCentralCore.Settings.RedownloadTrackArtwork;
            }
            if (mvs.GetType() == typeof(DBAlbumInfo))
            {
                maxSize.Width = mvCentralCore.Settings.MaximumAlbumWidth;
                maxSize.Height = mvCentralCore.Settings.MaximumAlbumHeight;
                redownload = mvCentralCore.Settings.RedownloadAlbumArtwork;
            }
            if (mvs.GetType() == typeof(DBArtistInfo))
            {
                maxSize.Width = mvCentralCore.Settings.MaximumArtistWidth;
                maxSize.Height = mvCentralCore.Settings.MaximumArtistHeight;
                redownload = mvCentralCore.Settings.RedownloadArtistArtwork;
            }

            BasicArt newTrack = new BasicArt(mv);
            newTrack.Filename = GenerateFilename(url);
            status = newTrack.FromUrl(url, ignoreRestrictions, minSize, maxSize, redownload);

            switch (status) {
                case ImageLoadResults.SUCCESS:
                    logger.Info("Added art for \"{0}\" from: {1}", mv.Basic, url);
                    break;
                case ImageLoadResults.SUCCESS_REDUCED_SIZE:
                    logger.Info("Added resized art for \"{0}\" from: {1}", mv.Basic, url);
                    break;
                case ImageLoadResults.FAILED_ALREADY_LOADED:
                    logger.Debug("art for \"{0}\" from the following URL is already loaded: {1}", mv.Basic, url);
                    return null;
                case ImageLoadResults.FAILED_TOO_SMALL:
                    logger.Debug("Downloaded art for \"{0}\" failed minimum resolution requirements: {1}", mv.Basic, url);
                    return null;
                case ImageLoadResults.FAILED:
                    logger.Error("Failed downloading art for \"{0}\": {1}", mv.Basic, url);
                    return null;
            }                       

            return newTrack;
        }

        public static BasicArt FromFile(DBBasicInfo mv, string path, out ImageLoadResults status)
        {
            return FromFile(mv, path, false, out status);
        }

        public static BasicArt FromFile(string title, string path, out ImageLoadResults status)
        {
            return FromFile(mvs, path, false, out status);
        }

        public static BasicArt FromFile(string title, string path, bool ignoreRestrictions, out ImageLoadResults status)
        {
            return FromFile(mvs, path, ignoreRestrictions, out status);
        }

        public static BasicArt FromFile(DBBasicInfo mv, string path, bool ignoreRestrictions, out ImageLoadResults status)
        {
            ImageSize minSize = null;
            ImageSize maxSize = new ImageSize();
            if (mvs == null) mvs = mv;
            if (!ignoreRestrictions)
            {
                minSize = new ImageSize();
                if (mvs.GetType() == typeof(DBTrackInfo))
                {
                    minSize.Width = mvCentralCore.Settings.MinimumTrackWidth;
                    minSize.Height = mvCentralCore.Settings.MinimumTrackHeight;
                }
                if (mvs.GetType() == typeof(DBAlbumInfo))
                {
                    minSize.Width = mvCentralCore.Settings.MinimumAlbumWidth;
                    minSize.Height = mvCentralCore.Settings.MinimumAlbumHeight;
                }
                if (mvs.GetType() == typeof(DBArtistInfo))
                {
                    minSize.Width = mvCentralCore.Settings.MinimumArtistWidth;
                    minSize.Height = mvCentralCore.Settings.MinimumArtistHeight;
                }
            }

            bool redownload = false;
            if (mvs.GetType() == typeof(DBTrackInfo))
            {
                maxSize.Width = mvCentralCore.Settings.MaximumTrackWidth;
                maxSize.Height = mvCentralCore.Settings.MaximumTrackHeight;
                redownload = mvCentralCore.Settings.RedownloadTrackArtwork;
            }
            if (mvs.GetType() == typeof(DBAlbumInfo))
            {
                maxSize.Width = mvCentralCore.Settings.MaximumAlbumWidth;
                maxSize.Height = mvCentralCore.Settings.MaximumAlbumHeight;
                redownload = mvCentralCore.Settings.RedownloadAlbumArtwork;
            }
            if (mvs.GetType() == typeof(DBArtistInfo))
            {
                maxSize.Width = mvCentralCore.Settings.MaximumArtistWidth;
                maxSize.Height = mvCentralCore.Settings.MaximumArtistHeight;
                redownload = mvCentralCore.Settings.RedownloadArtistArtwork;
            }


            BasicArt newTrack = new BasicArt(mv);
            newTrack.Filename = GenerateFilename(path);
            status = newTrack.FromFile(path, ignoreRestrictions, minSize, maxSize, redownload);

            switch (status)
            {
                case ImageLoadResults.SUCCESS:
                    logger.Info("Added art for \"{0}\" from: {1}", mv.Basic, path);
                    break;
                case ImageLoadResults.SUCCESS_REDUCED_SIZE:
                    logger.Info("Added resized art for \"{0}\" from: {1}", mv.Basic, path);
                    break;
                case ImageLoadResults.FAILED_ALREADY_LOADED:
                    logger.Debug("art for \"{0}\" from the following path is already loaded: {1}", mv.Basic, path);
                    return null;
                case ImageLoadResults.FAILED_TOO_SMALL:
                    logger.Debug("Downloaded art for \"{0}\" failed minimum resolution requirements: {1}", mv.Basic, path);
                    return null;
                case ImageLoadResults.FAILED:
                    logger.Error("Failed art for \"{0}\": {1}", mv.Basic, path);
                    return null;
            }

            return newTrack;
        }
    }
}
