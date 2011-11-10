using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using mvCentral.LocalMediaManagement;
using mvCentral.Database;

using Lastfm.Services;
using Lastfm;
using Lastfm.Scrobbling;

using NLog;


namespace mvCentral.Utils
{
  class LastFMScrobble
  {
    // Create logger
    private static Logger logger = LogManager.GetCurrentClassLogger();

    private string username;
    private string password;
    public Session Session;
    private Lastfm.Scrobbling.Connection connection;
    private Lastfm.Scrobbling.ScrobbleManager manager;

    private const string apiKey = "eadfb84ac56eddbf072efbfc18a90845";
    private const string apiSecret = "88b9694c60b240bd97ac1f02959f17c4";

    public bool IsLoged { get; set; }


    public LastFMScrobble()
    {
      Session = new Session(apiKey, apiSecret);
      IsLoged = false;
    }

    public bool Login(string username, string password)
    {
      this.username = username;
      this.password = password;

      Session.Authenticate(this.username, Lastfm.Utilities.MD5(this.password));
      if (Session.Authenticated)
      {
        connection = new Lastfm.Scrobbling.Connection("mpm", Assembly.GetEntryAssembly().GetName().Version.ToString(),this.username, Session);
        manager = new Lastfm.Scrobbling.ScrobbleManager(connection);
        IsLoged = true;
        return true;
      }
      return false;
    }

    public bool Handshake()
    {
      connection.Initialize();
      return Session.Authenticated;
    }

    public void NowPlaying(string Artist, string Title)
    {
      if (!IsLoged)
        return;
      try
      {
        int length = 0;


        if (!string.IsNullOrEmpty(Artist) && !string.IsNullOrEmpty(Title) && length > 0)
        {
          NowplayingTrack track1 = new NowplayingTrack(Artist, Title, new TimeSpan(0, 0, length));
          manager.ReportNowplaying(track1);
        }
      }
      catch (Exception exception)
      {
        logger.ErrorException("Error with Nowplayin",exception);
      }
    }

    public void Submit(string Artist, string Title)
    {
      if (!IsLoged)
        return;
      try
      {

        int length = 0;

        if (!string.IsNullOrEmpty(Artist) && !string.IsNullOrEmpty(Title) && length > 0)
        {
          Entry entry = new Entry(Artist, Title, DateTime.Now, PlaybackSource.User, new TimeSpan(0, 0, length), ScrobbleMode.Played);
          manager.Queue(entry);
          //manager.Submit();
        }
      }
      catch (Exception exception)
      {
        logger.Error("Error in Submit",exception);
      }
    }

  }
}
