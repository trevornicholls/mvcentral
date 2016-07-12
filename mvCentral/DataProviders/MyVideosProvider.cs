using Cornerstone.Database;
using Cornerstone.Extensions;
using MediaPortal.Configuration;
using MediaPortal.Util;
using mvCentral.Database;
using mvCentral.SignatureBuilders;
using SQLite.NET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;


namespace mvCentral.DataProviders
{
  public class MyVideosProvider : IMusicVideoProvider
  {
        #region IMusicVideoProvider Members

        public event EventHandler ProgressChanged;
    
        public string Name
        {
          get { return "local.mediaportal.videos"; }
        }

        public string Version {
          get { return "Internal"; }
        }

        public string Author {
          get { return "ajs"; }
        }

        public string Description {
          get { return "Retrieves mv cover artwork previously downloaded via MyVideos."; }
        }

        public string Language {
          get { return ""; }
        }
        
        public string LanguageCode {
          get { return ""; }
        }

        public List<string> LanguageCodeList
        {
          get
          {
            List<string> supportLanguages = new List<string>();
            return supportLanguages;
          }
        }

        public bool ProvidesTrackDetails
        {
          get { return false; }
        }

        public bool ProvidesArtistDetails
        {
          get { return false; }
        }

        public bool ProvidesAlbumDetails
        {
          get { return false; }
        }

        public bool ProvidesAlbumArt
        {
          get { return false; }
        }

        public bool ProvidesArtistArt
        {
          get { return false; }
        }

        public bool ProvidesTrackArt
        {
          get { return false; }
        }

        #endregion

        public DBTrackInfo GetArtistDetail(DBTrackInfo mv)
        {
          throw new NotImplementedException();
        }

        public DBTrackInfo GetAlbumDetail(DBTrackInfo mv)
        {
          throw new NotImplementedException();
        }

        /// <summary>
        /// Get the track details
        /// </summary>
        /// <param name="mvSignature"></param>
        /// <returns></returns>
        public List<DBTrackInfo> GetTrackDetail(MusicVideoSignature mvSignature)
        {
          return null;
        }

        /// <summary>
        /// Get Artist Artwork
        /// </summary>
        /// <param name="mvArtistObject"></param>
        /// <returns></returns>
        public bool GetArtistArt(DBArtistInfo mvArtistObject)
        {
          return false;
        }

        /// <summary>
        /// Get Track Artwork
        /// </summary>
        /// <param name="mv"></param>
        /// <returns></returns>
        public bool GetTrackArt(DBTrackInfo mv)
        {
          return false;
        }

        /// <summary>
        /// Get the Album Art
        /// </summary>
        /// <param name="mv"></param>
        /// <returns></returns>
        public bool GetAlbumArt(DBAlbumInfo mv)
        {
          return false;
        }

        /// <summary>
        /// Get the Artist, Album Details
        /// </summary>
        /// <param name="mv"></param>
        /// <returns></returns>
        public bool GetDetails(DBBasicInfo mv)
        {
          return false;
        }

        /// <summary>
        /// Get the album details 
        /// </summary>
        /// <param name="basicInfo"></param>
        /// <param name="albumTitle"></param>
        /// <param name="albumMbid"></param>
        /// <returns></returns>
        public bool GetAlbumDetails(DBBasicInfo basicInfo, string albumTitle, string albumMbid)
        {
          return true;
        }

        public UpdateResults UpdateTrack(DBTrackInfo mv)
        {
          return UpdateResults.SUCCESS;
        }

        /// <summary>
        /// Generate Thumbnail
        /// </summary>
        /// <param name="mv"></param>
        /// <returns></returns>
        bool generateVideoThumbnail(DBTrackInfo mv)
        {
          lock (this)
          {
            string outputFilename = Path.Combine(Path.GetTempPath(), mv.Track + DateTime.Now.ToFileTimeUtc().ToString() + ".jpg");

            if (mvCentral.Utils.VideoThumbCreator.CreateVideoThumb(mv.LocalMedia[0].File.FullName, outputFilename))
            {
              if (File.Exists(outputFilename))
              {
                mv.AddArtFromFile(outputFilename);
                File.Delete(outputFilename);
                return true;
              }
              else
                return false;
            }
            else
              return false;
          }
        }

        private void ReportProgress(string text)
        {
          if (ProgressChanged != null)
          {
            ProgressChanged(this, new ProgressEventArgs { Text = "Mediaportal Video DB: " + text });
          }
        }
  }
}
