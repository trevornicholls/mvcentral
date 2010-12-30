using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornerstone.Tools;
using NLog;
using mvCentral.Database;
using System.IO;
using System.Threading;
//using mvCentral.DataProviders;

namespace mvCentral.BackgroundProcesses
{
    internal class UpdateArtworkProcess: AbstractBackgroundProcess {
        private static Logger logger = LogManager.GetCurrentClassLogger();


        public override string Name {
            get { return "Artwork Updater"; }
        }

        public override string Description {
            get { return "This process removes invalid artwork references and attempts to " +
                         "retrieve artwork for movies currently missing a cover or backdrop."; }
        }

        public override void Work() {
            logger.Info("Beginging artwork updater background process.");

            RemoveOrphanArtwork();
            LookForMissingArtwork();

            logger.Info("Background artwork updater process complete.");
        }

        // Removes Artwork From a musicvideo
        public void RemoveOrphanArtwork() {
            float count = 0;
            float total = DBTrackInfo.GetAll().Count;

            foreach (DBTrackInfo currMusicVideo in DBTrackInfo.GetAll())
            {
                //OnProgress(count / total);
                count++;

                if (currMusicVideo.ID == null)
                    continue;

                // get the list of elements to remove
                List<string> toRemove = new List<string>();
                foreach (string currTrackArtPath in currMusicVideo.AlternateArts)
                {
                    if (!new FileInfo(currTrackArtPath).Exists)
                        toRemove.Add(currTrackArtPath);
                }

                // remove them
                foreach (string currItem in toRemove) {
                    currMusicVideo.AlternateArts.Remove(currItem);
                }

                // reset default cover is needed
                if (!currMusicVideo.AlternateArts.Contains(currMusicVideo.ArtFullPath))
                    if (currMusicVideo.AlternateArts.Count == 0)
                        currMusicVideo.ArtFullPath = " ";
                    else
                        currMusicVideo.ArtFullPath = currMusicVideo.AlternateArts[0];

                // get rid of the backdrop link if it doesnt exist
                if (currMusicVideo.ArtFullPath.Trim().Length > 0 && !new FileInfo(currMusicVideo.ArtFullPath).Exists)
                    currMusicVideo.ArtFullPath = " ";

                currMusicVideo.Commit();
            }

            //OnProgress(1.0);
        }

        private void LookForMissingArtwork() {
            foreach (DBTrackInfo currMusicVideo in DBTrackInfo.GetAll())
            {
                try {
                    if (currMusicVideo.ID == null)
                        continue;

                    if (currMusicVideo.ArtFullPath.Trim().Length == 0)
                    {
                        mvCentralCore.DataProviderManager.GetTrackArt(currMusicVideo);
                        
                        // because this operation can take some time we check again
                        // if the movie was not deleted while we were getting artwork
                        if (currMusicVideo.ID == null)
                            continue;

                        currMusicVideo.Commit();
                    }

                    if (currMusicVideo.ArtistInfo[0].ArtFullPath.Trim().Length == 0)
                    {
//                        new LocalProvider().GetArtistArt(currMusicVideo.ArtistInfo[0]);
                        mvCentralCore.DataProviderManager.GetArtistArt(currMusicVideo.ArtistInfo[0]);
                        
                        // because this operation can take some time we check again
                        // if the movie was not deleted while we were getting the artwork
                        if (currMusicVideo.ID == null)
                            continue;

                        currMusicVideo.Commit();
                    }
                    if (currMusicVideo.AlbumInfo.Count>0 && currMusicVideo.AlbumInfo[0].ArtFullPath.Trim().Length == 0)
                    {
                        //                        new LocalProvider().GetArtistArt(currMusicVideo.ArtistInfo[0]);
                        mvCentralCore.DataProviderManager.GetAlbumArt(currMusicVideo.AlbumInfo[0]);

                        // because this operation can take some time we check again
                        // if the movie was not deleted while we were getting the artwork
                        if (currMusicVideo.ID == null)
                            continue;

                        currMusicVideo.Commit();
                    }
                }
                catch (Exception e) {
                    if (e is ThreadAbortException)
                        throw e;

                    logger.ErrorException("Error retrieving artwork for " + currMusicVideo.Track, e);
                }
            }
        }
    }
}
