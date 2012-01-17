﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornerstone.Tools;
using mvCentral.Database;
using NLog;
using System.Threading;

namespace mvCentral.BackgroundProcesses
{
    internal class MediaInfoUpdateProcess: AbstractBackgroundProcess {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public override string Name {
            get { return "MediaInfo Updater"; }
        }

        public override string Description {
            get {
                return "This process updates MediaInfo details for all media in the " +
                       "database that currently does not have MediaInfo retrieved.";
            }
        }

        public override void Work() {
            if (!mvCentralCore.Settings.AutoRetrieveMediaInfo)
                return;

            logger.Info("Begining background media info update process.");

            List<DBLocalMedia> allLocalMedia = DBLocalMedia.GetAll();
            foreach (DBLocalMedia lm in allLocalMedia) {
                if (lm.ID != null && !lm.HasMediaInfo) {
                    lm.UpdateMediaInfo();
                    lm.Commit();
                }
            }


            foreach (DBTrackInfo currTrack in DBTrackInfo.GetAll())
            {
              // Check for Artist missing data

              try
              {
                logger.Debug("Checking for Artist missing deails " + currTrack.GetType().ToString() + " CurrMusicVideo.ID : " + currTrack.Track);
                if (currTrack.ID == null)
                  continue;

                if (currTrack.ArtistInfo[0].Genre.Trim().Length == 0)
                {
                  mvCentralCore.DataProviderManager.GetArtistDetail(currTrack);

                  // because this operation can take some time we check again
                  // if the artist/album/track was not deleted while we were getting artwork
                  if (currTrack.ID == null)
                    continue;

                  currTrack.Commit();
                }
              }
              catch (Exception e)
              {
                if (e is ThreadAbortException)
                  throw e;

                logger.ErrorException("Error retrieving Video details for " + currTrack.Basic, e);
              }
              // Check for Album missing data if album support enabled
              if (currTrack.AlbumInfo.Count > 0 && !mvCentralCore.Settings.DisableAlbumSupport)
              {
                try
                {
                  logger.Debug("Checking for Artist missing deails " + currTrack.GetType().ToString() + " CurrMusicVideo.ID : " + currTrack.Track);
                  if (currTrack.ID == null)
                    continue;

                  if (currTrack.AlbumInfo[0].YearReleased.Trim().Length == 0)
                  {
                    mvCentralCore.DataProviderManager.GetAlbumDetail(currTrack);

                    // because this operation can take some time we check again
                    // if the artist/album/track was not deleted while we were getting artwork
                    if (currTrack.ID == null)
                      continue;

                    currTrack.Commit();
                  }
                }
                catch (Exception e)
                {
                  if (e is ThreadAbortException)
                    throw e;

                  logger.ErrorException("Error retrieving Album details for " + currTrack.Basic, e);
                }
              }

            }

            logger.Info("Background media info update process complete.");
        }
    }
}
