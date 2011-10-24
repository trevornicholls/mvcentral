using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NLog;
using mvCentral.Localizations;
using mvCentral.Database;
using mvCentral.Utils;
// MediaPortal
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Video;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.Video.Database;
using Action = MediaPortal.GUI.Library.Action;
using Layout = MediaPortal.GUI.Library.GUIFacadeControl.Layout;
using WindowPlugins;

namespace mvCentral.GUI
{
  public class GUImvStatsAndInfo : WindowPluginBase
  {
    #region variables

    private static Logger logger = LogManager.GetCurrentClassLogger();

    const int windowID = 112013;

    #endregion

    #region Skin Connection

    private enum GUIControls
    {
      exitScreen = 14,
      versionLabel = 15,
      videoCountLabel = 16,
      favVideoImage = 18,
      favArtistImage = 20,
      topTen1 = 30,
      topTen2 = 31,
      topTen3 = 32,
      topTen4 = 33,
      topTen5 = 34,
      topTen6 = 35,
      topTen7 = 36,
      topTen8 = 37,
      topTen9 = 38,
      topTen10 = 39

    }

    [SkinControl((int)GUIControls.exitScreen)] protected GUIButtonControl skinInfo_exit = null;
    [SkinControl((int)GUIControls.versionLabel)] protected GUILabelControl version_label = null;
    [SkinControl((int)GUIControls.videoCountLabel)] protected GUILabelControl videoCount_label = null;

    [SkinControl((int)GUIControls.favArtistImage)] protected GUIImage favArtistImage = null;
    [SkinControl((int)GUIControls.favVideoImage)] protected GUIImage favVideoImage = null;


    [SkinControl((int)GUIControls.topTen1)] protected GUIFadeLabel topTen1 = null;
    [SkinControl((int)GUIControls.topTen2)] protected GUIFadeLabel topTen2 = null;
    [SkinControl((int)GUIControls.topTen3)] protected GUIFadeLabel topTen3 = null;
    [SkinControl((int)GUIControls.topTen4)] protected GUIFadeLabel topTen4 = null;
    [SkinControl((int)GUIControls.topTen5)] protected GUIFadeLabel topTen5 = null;
    [SkinControl((int)GUIControls.topTen6)] protected GUIFadeLabel topTen6 = null;
    [SkinControl((int)GUIControls.topTen7)] protected GUIFadeLabel topTen7 = null;
    [SkinControl((int)GUIControls.topTen8)] protected GUIFadeLabel topTen8 = null;
    [SkinControl((int)GUIControls.topTen9)] protected GUIFadeLabel topTen9 = null;
    [SkinControl((int)GUIControls.topTen10)] protected GUIFadeLabel topTen10 = null;

    #endregion

    #region Constructor

    public GUImvStatsAndInfo()
    {
    }

    #endregion

    #region Base overrides

    public override int GetID
    {
      get { return windowID; }
    }

    public override string GetModuleName()
    {
      return mvCentralCore.Settings.HomeScreenName;
    }

    public override bool Init()
    {
      string xmlSkin = GUIGraphicsContext.Skin + @"\mvCentral.StatsAndInfo.xml";
      logger.Info("Loading main skin window: " + xmlSkin);
      return Load(xmlSkin);
    }

    protected override void OnPageLoad()
    {
      GUILabelControl.SetControlLabel(GetID, (int)GUIControls.versionLabel, System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
      List<DBTrackInfo> videoList = DBTrackInfo.GetAll();
      List<DBArtistInfo> artistList = DBArtistInfo.GetAll();
      // Set stats
      GUILabelControl.SetControlLabel(GetID, (int)GUIControls.videoCountLabel, string.Format(Localization.VideoCount, videoList.Count, artistList.Count));
      // Set Hierachy
      GUIPropertyManager.SetProperty("#mvCentral.Hierachy", Localization.History);
      // Get the most viewed video
      videoList.Sort(delegate(DBTrackInfo p1, DBTrackInfo p2) { return p2.ActiveUserSettings.WatchedCount.CompareTo(p1.ActiveUserSettings.WatchedCount); });
      if (videoList[0].ActiveUserSettings.WatchedCount == 0)
        GUIPropertyManager.SetProperty("#mvCentral.MostPlayed", " ");
      else
        GUIPropertyManager.SetProperty("#mvCentral.MostPlayed", videoList[0].Track);

      favVideoImage.FileName = videoList[0].ArtFullPath;

      try
      {
        // Set the Top ten list - sure there is a neater way of doing this....
        if (videoList[0].ActiveUserSettings.WatchedCount > 0)
          topTen1.Label = string.Format(" 1 - {0} - {1}", videoList[0].ArtistInfo[0].Artist.ToString(), videoList[0].Track.ToString());

        if (videoList[1].ActiveUserSettings.WatchedCount > 0)
          topTen2.Label = string.Format(" 2 - {0} - {1}", videoList[1].ArtistInfo[0].Artist, videoList[1].Track);

        if (videoList[2].ActiveUserSettings.WatchedCount > 0)
          topTen3.Label = string.Format(" 3 - {0} - {1}", videoList[2].ArtistInfo[0].Artist, videoList[2].Track);

        if (videoList[3].ActiveUserSettings.WatchedCount > 0)
          topTen4.Label = string.Format(" 4 - {0} - {1}", videoList[3].ArtistInfo[0].Artist, videoList[3].Track);

        if (videoList[4].ActiveUserSettings.WatchedCount > 0)
          topTen5.Label = string.Format(" 5 - {0} - {1}", videoList[4].ArtistInfo[0].Artist, videoList[4].Track);

        if (videoList[5].ActiveUserSettings.WatchedCount > 0)
          topTen6.Label = string.Format(" 6 - {0} - {1}", videoList[5].ArtistInfo[0].Artist, videoList[5].Track);

        if (videoList[6].ActiveUserSettings.WatchedCount > 0)
          topTen7.Label = string.Format(" 7 - {0} - {1}", videoList[6].ArtistInfo[0].Artist, videoList[6].Track);

        if (videoList[7].ActiveUserSettings.WatchedCount > 0)
          topTen8.Label = string.Format(" 8 - {0} - {1}", videoList[7].ArtistInfo[0].Artist, videoList[7].Track);

        if (videoList[8].ActiveUserSettings.WatchedCount > 0)
          topTen9.Label = string.Format(" 9 - {0} - {1}", videoList[8].ArtistInfo[0].Artist, videoList[8].Track);

        if (videoList[9].ActiveUserSettings.WatchedCount > 0)
          topTen10.Label = string.Format("10 - {0} - {1}", videoList[9].ArtistInfo[0].Artist, videoList[9].Track);
      }
      catch { }

      // Get the most viewed artist
      int watchedCount = 0;
      int higestWatchCount = 0;
      DBArtistInfo mostWatchedArtist = null;

      foreach (DBArtistInfo artist in artistList)
      {
        List<DBTrackInfo> artistTracks = DBTrackInfo.GetEntriesByArtist(artist);
        watchedCount = 0;
        foreach (DBTrackInfo track in artistTracks)
        {
          watchedCount += track.ActiveUserSettings.WatchedCount;
        }
        if (watchedCount > higestWatchCount)
        {
          higestWatchCount = watchedCount;
          mostWatchedArtist = artist;
        }
      }
      if (mostWatchedArtist != null)
      {
        GUIPropertyManager.SetProperty("#mvCentral.FavArtist", mostWatchedArtist.Artist);
        favArtistImage.FileName = mostWatchedArtist.ArtThumbFullPath;
      }
      else
        GUIPropertyManager.SetProperty("#mvCentral.FavArtist", " ");
    }


    protected override void OnPageDestroy(int new_windowId)
    {

    }
    #endregion

    #region Public Methods

    public static int GetWindowId()
    {
      return windowID;
    }

    #endregion

  }
}
