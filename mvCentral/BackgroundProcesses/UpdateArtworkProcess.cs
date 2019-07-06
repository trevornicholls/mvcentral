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
  public class UpdateArtworkProcess : AbstractBackgroundProcess
  {
    private static Logger logger = LogManager.GetCurrentClassLogger();

    public delegate void ProcessUpdateDelegate(AbstractBackgroundProcess process, double progress, string activeFile);

    public override string Name
    {
      get { return "Artwork Updater"; }
    }

    public override string Description
    {
      get
      {
        return "This process removes invalid artwork references and attempts to " +
               "retrieve artwork for Artist, Alums or Tracks currently missing artwork.";
      }
    }

    public override void Work()
    {
      logger.Info("Beginging artwork updater background process.");
      Thread.Sleep(30000);
      RemoveOrphanArtwork();
      LookForMissingMetaData();

      logger.Info("Background artwork updater process complete.");
    }

    /// <summary>
    /// Remove Orphaned Artwork
    /// </summary>
    public void RemoveOrphanArtwork()
    {
      logger.Info("Checking for Orphaned Artwork....");

      // Artist
      logger.Debug("Checking for Orphaned Artists Artwork....");
      foreach (DBArtistInfo currMusicVideo in DBArtistInfo.GetAll())
      {
        if (currMusicVideo.ID == null)
          continue;

        logger.Debug("Checking " + currMusicVideo.GetType().ToString() + " CurrArtist.ID : " + currMusicVideo.Artist);

        // get the list of elements to remove
        List<string> toRemove = new List<string>();
        foreach (string currTrackArtPath in currMusicVideo.AlternateArts)
        {
          if (!new FileInfo(currTrackArtPath).Exists)
            toRemove.Add(currTrackArtPath);
        }

        // remove them
        foreach (string currItem in toRemove)
        {
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

      // Album
      logger.Debug("Checking for Orphaned Albums Artwork....");
      foreach (DBAlbumInfo currMusicVideo in DBAlbumInfo.GetAll())
      {
        if (currMusicVideo.ID == null)
          continue;

        logger.Debug("Checking " + currMusicVideo.GetType().ToString() + " CurrAlbum.ID : " + currMusicVideo.Album);
        // get the list of elements to remove
        List<string> toRemove = new List<string>();
        foreach (string currTrackArtPath in currMusicVideo.AlternateArts)
        {
          if (!new FileInfo(currTrackArtPath).Exists)
            toRemove.Add(currTrackArtPath);
        }

        // remove them
        foreach (string currItem in toRemove)
        {
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

      // Track
      logger.Debug("Checking for Orphaned Tracks Artwork....");
      foreach (DBTrackInfo currMusicVideo in DBTrackInfo.GetAll())
      {
        if (currMusicVideo.ID == null)
          continue;

        logger.Debug("Checking " + currMusicVideo.GetType().ToString() + " CurrMusicVideo.ID : " + currMusicVideo.Track);
        // get the list of elements to remove
        List<string> toRemove = new List<string>();
        foreach (string currTrackArtPath in currMusicVideo.AlternateArts)
        {
          if (!new FileInfo(currTrackArtPath).Exists)
            toRemove.Add(currTrackArtPath);
        }

        // remove them
        foreach (string currItem in toRemove)
        {
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
    }

    #region Missing Artwork/Info

    /// <summary>
    /// This methoded checks for missing artwork and medatdata
    /// </summary>
    private void LookForMissingMetaData()
    {
      float count = 0;
      float total = DBArtistInfo.GetAll().Count + DBAlbumInfo.GetAll().Count + DBTrackInfo.GetAll().Count;

      // Check for missing Artist Artwork
      logger.Info("Checking for Missing Artwork (Artists)");
      foreach (DBArtistInfo currArtist in DBArtistInfo.GetAll())
      {

        OnProgress((count * 100) / total);
        count++;

        try
        {
          logger.Debug("Checking " + currArtist.GetType().ToString() + " CurrArtist.ID : " + currArtist.Artist);
          if (currArtist.ID == null)
            continue;

          if (currArtist.ArtFullPath.Trim().Length == 0)
          {
            mvCentralCore.DataProviderManager.GetArt(currArtist, false);

            // because this operation can take some time we check again
            // if the artist/album/track was not deleted while we were getting artwork
            if (currArtist.ID == null)
              continue;

            currArtist.Commit();
          }
        }
        catch (Exception e)
        {
          if (e is ThreadAbortException)
            throw e;

          logger.ErrorException("Error retrieving Artist artwork for " + currArtist.Basic, e);
        }
      }
      // Check for Missing Album Artwork
      logger.Info("Checking for Missing Artwork (Albums)");
      foreach (DBAlbumInfo currAlbum in DBAlbumInfo.GetAll())
      {
        OnProgress((count * 100) / total);
        count++;

        try
        {
          logger.Debug("Checking " + currAlbum.GetType().ToString() + " CurrAlbum.ID : " + currAlbum.Album);
          if (currAlbum.ID == null)
            continue;

          if (currAlbum.ArtFullPath.Trim().Length == 0)
          {
            mvCentralCore.DataProviderManager.GetArt(currAlbum, false);

            // because this operation can take some time we check again
            // if the artist/album/track was not deleted while we were getting artwork
            if (currAlbum.ID == null)
              continue;

            currAlbum.Commit();
          }
        }
        catch (Exception e)
        {
          if (e is ThreadAbortException)
            throw e;

          logger.Error("Error retrieving Album artwork for " + currAlbum.Basic);
        }
      }
      // Check for missing video Artwork
      logger.Info("Checking for Missing Artwork (Videos)");
      foreach (DBTrackInfo currTrack in DBTrackInfo.GetAll())
      {
        OnProgress((count * 100) / total);
        count++;
        try
        {
          logger.Debug("Checking " + currTrack.GetType().ToString() + " CurrMusicVideo.ID : " + currTrack.Track);
          if (currTrack.ID == null)
            continue;

          if (currTrack.ArtFullPath.Trim().Length == 0)
          {
            mvCentralCore.DataProviderManager.GetArt(currTrack, false);

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

          logger.ErrorException("Error retrieving Video artwork for " + currTrack.Basic, e);
        }
      }
      OnProgress(100.0);
    }

    #endregion
  }
}
