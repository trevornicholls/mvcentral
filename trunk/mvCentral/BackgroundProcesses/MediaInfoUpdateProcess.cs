using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornerstone.Tools;
using mvCentral.Database;
using NLog;
using System.Threading;

namespace mvCentral.BackgroundProcesses
{
  internal class MediaInfoUpdateProcess : AbstractBackgroundProcess
  {
    private static Logger logger = LogManager.GetCurrentClassLogger();

    public override string Name
    {
      get { return "MediaInfo Updater"; }
    }

    public override string Description
    {
      get
      {
        return "This process updates MediaInfo details for all media in the " +
               "database that currently does not have MediaInfo retrieved.";
      }
    }

    public override void Work()
    {
      if (!mvCentralCore.Settings.AutoRetrieveMediaInfo)
        return;

      logger.Info("Begining background media info update process.");

      List<DBLocalMedia> allLocalMedia = DBLocalMedia.GetAll();
      foreach (DBLocalMedia lm in allLocalMedia)
      {
        if (lm.ID != null && !lm.HasMediaInfo)
        {
          lm.UpdateMediaInfo();
          lm.Commit();
        }
      }


      foreach (DBTrackInfo currTrack in DBTrackInfo.GetAll())
      {
        // Check for Artist missing data
        try
        {
          if (currTrack.ID == null)
            continue;

          if (currTrack.ArtistInfo[0].DisallowBackgroundUpdate && !mvCentralCore.Settings.BackgroundScanAlways)
            continue;


          logger.Debug("Checking for Artist missing deails " + currTrack.GetType().ToString() + " CurrMusicVideo.ID : " + currTrack.Track);
          mvCentralCore.DataProviderManager.GetArtistDetail(currTrack);

          // because this operation can take some time we check again
          // if the artist/album/track was not deleted while we were getting artwork
          if (currTrack.ID == null)
            continue;

          currTrack.Commit();

        }
        catch (Exception e)
        {
          if (e is ThreadAbortException)
            throw e;

          logger.ErrorException("Error retrieving Artist details for " + currTrack.Basic, e);
        }
        // Check for Album missing data if album support enabled
        if (currTrack.AlbumInfo.Count > 0 && !mvCentralCore.Settings.DisableAlbumSupport)
        {
          try
          {
            if (currTrack.ID == null)
              continue;

            if (currTrack.ArtistInfo[0].DisallowBackgroundUpdate && !mvCentralCore.Settings.BackgroundScanAlways)
              continue;

            logger.Debug("Checking for Album missing deails " + currTrack.GetType().ToString() + " Title : " + currTrack.AlbumInfo[0].Album);
            mvCentralCore.DataProviderManager.GetAlbumDetail(currTrack);

            // because this operation can take some time we check again
            // if the artist/album/track was not deleted while we were getting artwork
            if (currTrack.ID == null)
              continue;

            currTrack.Commit();

          }
          catch (Exception e)
          {
            if (e is ThreadAbortException)
              throw e;

            logger.ErrorException("Error retrieving Album details for " + currTrack.Basic, e);
          }
        }

        // Prevent further background updates
        currTrack.ArtistInfo[0].DisallowBackgroundUpdate = true;
        if (currTrack.AlbumInfo.Count > 0)
          currTrack.AlbumInfo[0].DisallowBackgroundUpdate = true;

        currTrack.Commit();

      }

      logger.Info("Background media info update process complete.");
    }
  }
}
