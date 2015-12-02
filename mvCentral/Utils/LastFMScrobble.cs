using NLog;

using System;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace mvCentral.Utils
{
  class LastFMScrobble
  {
    // Create logger
    private static Logger logger = LogManager.GetCurrentClassLogger();

    private string username;
    private string password;
    public Session Session;

    private const string apiKey = "eadfb84ac56eddbf072efbfc18a90845";
    private const string apiSecret = "88b9694c60b240bd97ac1f02959f17c4";

    public bool IsLoged { get; set; }
    public DateTime trackStartTime { get; set; }


    public LastFMScrobble()
    {
      Session = new Session(apiKey, apiSecret);
      IsLoged = false;
    }

    public bool Login(string username, string password)
    {
      if (string.IsNullOrEmpty(username.Trim()) && string.IsNullOrEmpty(password.Trim()))
        return false;

      logger.Debug("Login to Last.FM and establish session");
      this.username = username;
      this.password = password;
      //Create the session
      try
      {
        Session.Authenticate(this.username, MD5(this.password));
      }
      catch (Exception e)
      {
        logger.DebugException("Error creating session", e);
      }
      finally
      {
        if (Session.Authenticated)
        {
          IsLoged = true;
        }
      }
      if (IsLoged)
        return true;
      else
        return false;
    }
    /// <summary>
    /// Update now playing track on Last.FM
    /// </summary>
    /// <param name="Artist"></param>
    /// <param name="Title"></param>
    /// <param name="seconds"></param>
    public void NowPlaying(string Artist, string Title, int seconds)
    {
      if (!IsLoged)
        return;
      try
      {
        if (!string.IsNullOrEmpty(Artist) && !string.IsNullOrEmpty(Title) && seconds > 0)
        {
          RequestParameters nowPlayingParameters = new RequestParameters();
          nowPlayingParameters["track"] = Title;
          nowPlayingParameters["artist"] = Artist;
          nowPlayingParameters["duration"] = seconds.ToString();
          Request nowplaying = new Request("track.updateNowPlaying", Session, nowPlayingParameters);
          XmlDocument doc = nowplaying.execute();
          // Check the status
          XmlNode n = doc.GetElementsByTagName("lfm")[0];
          string status = n.Attributes[0].InnerText;
          logger.Debug("Call to track.updateNowPlaying : " + status);
          trackStartTime = DateTime.Now;
        }
      }
      catch (Exception exception)
      {
        logger.DebugException("Error with Nowplayin", exception);
      }
    }
    /// <summary>
    /// Submit track to Last.FM Library
    /// </summary>
    /// <param name="Artist"></param>
    /// <param name="Title"></param>
    public void Submit(string Artist, string Title, int Seconds)
    {

      // Check how much of track was played if less than 50% do not submit
      TimeSpan playTime = DateTime.Now - trackStartTime;
      logger.Debug("Last.FM Submit: {0} {1}", playTime.Seconds.ToString(), (Seconds / 2).ToString());
      if (playTime.Seconds < (Seconds / 2))
        return;

      if (!IsLoged)
        return;
      try
      {
        if (!string.IsNullOrEmpty(Artist) && !string.IsNullOrEmpty(Title))
        {
          RequestParameters submitParameters = new RequestParameters();
          submitParameters["artist"] = Artist;
          submitParameters["track"] = Title;
          submitParameters["timestamp"] = calculateSeconds(trackStartTime).ToString();
          Request nowplaying = new Request("track.scrobble", Session, submitParameters);
          XmlDocument doc = nowplaying.execute();
          // Check the Status
          XmlNode n = doc.GetElementsByTagName("lfm")[0];
          string status = n.Attributes[0].InnerText;
          logger.Debug("Call to track.scrobble : " + status);
        }
      }
      catch (Exception exception)
      {
        logger.DebugException("Error in Submit",exception);
      }
    }
    /// <summary>
    /// Seconds from 01 Jan 1970 to suppiled date/time
    /// </summary>
    /// <param name="startTime"></param>
    /// <returns></returns>
    public int calculateSeconds(DateTime startTime)
    {
      TimeSpan timeStamp = (DateTime.UtcNow - new DateTime(1970, 1, 1));
      return (int)timeStamp.TotalSeconds;
    }
    /// <summary>
    /// Returns the md5 hash of a string.
    /// </summary>
    /// <param name="text">
    /// A <see cref="System.String"/>
    /// </param>
    /// <returns>
    /// A <see cref="System.String"/>
    /// </returns>
    public static string MD5(string text)
    {
      byte[] buffer = Encoding.UTF8.GetBytes(text);

      MD5CryptoServiceProvider c = new MD5CryptoServiceProvider();
      buffer = c.ComputeHash(buffer);

      StringBuilder builder = new StringBuilder();
      foreach (byte b in buffer)
        builder.Append(b.ToString("x2").ToLower());

      return builder.ToString();
    }

  }
}
