using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MusicVideos.Database;
using Cornerstone.Extensions;

namespace MusicVideos.LocalMediaManagement.MusicVideoResources
{
    public class ArtistArt: ImageResource {
        public override string Filename {
            set {
                base.Filename = value;

                // build thumbnail filename
                string thumbsFolder = MusicVideosCore.Settings.ArtistArtThumbsFolder;
                FileInfo file = new FileInfo(Filename);
                ThumbFilename = thumbsFolder + "\\" + file.Name;
            }
        }

        // generate a filename for a artist art. should be unique based on the source hash
        private static string GenerateFilename(DBArtistInfo mv, string source) {
            string artFolder = MusicVideosCore.Settings.ArtistArtFolder;
            string safeName = mv.Artist.Replace(' ', '.').ToValidFilename();
            return artFolder + "\\{" + safeName + "} [" + source.GetHashCode() + "].jpg";
        }

        // genrate a filename for a artistart. should be unique based on the source hash
        private static string GenerateFilename(string mv, string source)
        {
            string artFolder = MusicVideosCore.Settings.ArtistArtFolder;
            string safeName = mv.Replace(' ', '.').ToValidFilename();
            return artFolder + "\\{" + safeName + "} [" + source.GetHashCode() + "].jpg";
        }

        public static ArtistArt FromUrl(DBArtistInfo mv, string url, out ImageLoadResults status) {
            return FromUrl(mv, url, false, out status);
        }

        public static ArtistArt FromUrl(string title, string url, out ImageLoadResults status)
        {
            DBArtistInfo mv = new DBArtistInfo();
            mv.Artist = title;
            return FromUrl(mv, url, false, out status);
        }
        public static ArtistArt FromUrl(string title, string url, bool ignoreRestrictions, out ImageLoadResults status)
        {
            DBArtistInfo mv = new DBArtistInfo();
            mv.Artist = title;
            return FromUrl(mv, url, ignoreRestrictions, out status);
        }

        public static ArtistArt FromUrl(DBArtistInfo mv, string url, bool ignoreRestrictions, out ImageLoadResults status) {
            ImageSize minSize = null;
            ImageSize maxSize = new ImageSize();

            if (!ignoreRestrictions) {
                minSize = new ImageSize();
                minSize.Width = MusicVideosCore.Settings.MinimumArtistWidth;
                minSize.Height = MusicVideosCore.Settings.MinimumArtistHeight;
            }

            maxSize.Width = MusicVideosCore.Settings.MaximumArtistWidth;
            maxSize.Height = MusicVideosCore.Settings.MaximumArtistHeight;

            bool redownload = MusicVideosCore.Settings.RedownloadArtistArtwork;

            ArtistArt newArtistart = new ArtistArt();
            newArtistart.Filename = GenerateFilename(mv, url);
            status = newArtistart.FromUrl(url, ignoreRestrictions, minSize, maxSize, redownload);

            switch (status) {
                case ImageLoadResults.SUCCESS:
                    logger.Info("Added artist art for \"{0}\" from: {1}", mv.Artist, url);
                    break;
                case ImageLoadResults.SUCCESS_REDUCED_SIZE:
                    logger.Debug("Added resized artist art for \"{0}\" from: {1}", mv.Artist, url);
                    break;
                case ImageLoadResults.FAILED_ALREADY_LOADED:
                    logger.Debug("Artist art for \"{0}\" from the following URL is already loaded: {1}", mv.Artist, url);
                    return null;
                case ImageLoadResults.FAILED_TOO_SMALL:
                    logger.Error("Downloaded artist art for \"{0}\" failed minimum resolution requirements: {1}", mv.Artist, url);
                    return null;
                case ImageLoadResults.FAILED:
                    logger.Error("Failed downloading artist art for \"{0}\": {1}", mv.Artist, url);
                    return null;
            }

            return newArtistart;
        }

        public static ArtistArt FromFile(DBArtistInfo mv, string path) {
            throw new NotImplementedException();
        }

    }
}
