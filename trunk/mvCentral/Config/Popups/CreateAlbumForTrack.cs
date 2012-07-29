using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Xml;

using mvCentral.LocalMediaManagement;
using mvCentral.Database;
using mvCentral.Utils;
using mvCentral.DataProviders;

using NLog;


namespace mvCentral.ConfigScreen.Popups
{
  public partial class CreateAlbumForTrack : Form
  {
    private static Logger logger = LogManager.GetCurrentClassLogger();

    #region API variables



    private const string apiMusicVideoUrl = "http://ws.audioscrobbler.com/2.0/?method={0}&api_key={1}";
    private const string apikey = "eadfb84ac56eddbf072efbfc18a90845";
    private const string apiSecret = "88b9694c60b240bd97ac1f02959f17c4";

    //        http://ws.audioscrobbler.com/2.0/?method=track.getinfo&api_key=b25b959554ed76058ac220b7b2e0a026&artist=cher&track=believe
    //        http://ws.audioscrobbler.com/2.0/?method=album.getinfo&api_key=b25b959554ed76058ac220b7b2e0a026&artist=Cher&album=Believe

    // Artist Info
    private static string apiArtistGetInfo = string.Format(apiMusicVideoUrl, "artist.getinfo&artist={0}&lang={1}", apikey);
    private static string apiArtistmbidGetInfo = string.Format(apiMusicVideoUrl, "artist.getinfo&mbid={0}&lang={1}", apikey);
    private static string apiArtistNameGetInfo = string.Format(apiMusicVideoUrl, "artist.getinfo&artist={0}&lang={1}", apikey);
    //Images
    private static string apiArtistmbidGetImagesInfo = string.Format(apiMusicVideoUrl, "artist.getimages&mbid={0}", apikey);
    private static string apiArtistNameGetImagesInfo = string.Format(apiMusicVideoUrl, "artist.getimages&artist={0}", apikey);
    private static string apiArtistGetImagesInfo = string.Format(apiMusicVideoUrl, "artist.getimages&artist={0}", apikey);
    // Albums
    private static string apiAlbumGetInfo = string.Format(apiMusicVideoUrl, "album.getinfo&album={0}&lang={1}", apikey);
    private static string apiAlbummbidGetInfo = string.Format(apiMusicVideoUrl, "album.getinfo&mbid={0}&lang={1}", apikey);
    private static string apiArtistAlbumGetInfo = string.Format(apiMusicVideoUrl, "album.getinfo&artist={0}&album={1}&lang={2}", apikey);
    private static string apiArtistTopAlbums = string.Format(apiMusicVideoUrl, "artist.gettopalbums&artist={0}", apikey);
    // Tracks
    private static string apiArtistTopTracks = string.Format(apiMusicVideoUrl, "artist.gettoptracks&artist={0}", apikey);
    private static string apiTrackGetInfo = string.Format(apiMusicVideoUrl, "set&track={0}&lang={1}", apikey);
    private static string apiTrackmbidGetInfo = string.Format(apiMusicVideoUrl, "track.getinfo&mbid={0}&lang={1}", apikey);
    private static string apiArtistTrackGetInfo = string.Format(apiMusicVideoUrl, "track.getinfo&artist={0}&track={1}&lang={2}", apikey);
    // Search
    private static string apiTrackSearch = string.Format(apiMusicVideoUrl, "track.search&track={0}", apikey);
    private static string apiArtistSearch = string.Format(apiMusicVideoUrl, "artist.search&artist={0}", apikey);
    private static string apiArtistTrackSearch = string.Format(apiMusicVideoUrl, "track.search&artist={0}&track={1}", apikey);

    #endregion

    public string Album { get { return tbAlbumName.Text; } }
    public string Track { get { return tbTrackName.Text; } }
    public string AlbumMBID { get; set; }
    public bool exitStatus { get; set; }

    public CreateAlbumForTrack(DBTrackInfo trackToAdd)
    {
      InitializeComponent();
      DBArtistInfo theArtist = DBArtistInfo.Get(trackToAdd);
      tbArtistName.Text = theArtist.Artist;
      tbTrackName.Text = trackToAdd.Track;
    }
    /// <summary>
    /// Retrive a list of albums from Last.FM linked with the artist
    /// </summary>
    /// <param name="artist"></param>
    /// <returns></returns>
    private bool getAlbumsByArtist(string artist)
    {
      XmlNodeList xml = null;

      if (artist != null)
        xml = getXML(string.Format(apiArtistTopAlbums, artist));
      else
        return false;

      if (xml == null)
        return false;

      XmlNode root = xml.Item(0).ParentNode;
      if (root.Attributes != null && root.Attributes["status"].Value != "ok") return false;
      XmlNode albumList = root.SelectSingleNode(@"/lfm/topalbums");

      List<Release> albumReleases = new List<Release>();
      foreach (XmlNode album in albumList.ChildNodes)
      {
        Release releasedAlbum = new Release(album);
        albumReleases.Add(releasedAlbum);
      }
      albumReleases.Sort(Release.TitleComparison);
      DetailsPopup albumListDialog = new DetailsPopup(albumReleases);

      if (albumListDialog.ShowDialog() == DialogResult.OK)
      {
        tbAlbumName.Text = albumListDialog.textBox1.Text;
        AlbumMBID = albumListDialog.label8.Text;
        return true;
      }
      return false;
    }
    /// <summary>
    /// Grab a list of albums linked to the artist
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btLookUp_Click(object sender, EventArgs e)
    {
      getAlbumsByArtist(tbArtistName.Text);
    }

    private void btOK_Click(object sender, EventArgs e)
    {
      exitStatus = true;
      this.Hide();
    }

    private void btCancel_Click(object sender, EventArgs e)
    {
      exitStatus = false;
      this.Hide();
    }

    /// <summary>
    /// given a url, retrieves the xml result set and returns the nodelist of Item objects
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    private static XmlNodeList getXML(string url)
    {
      XmlDocument xmldoc = new XmlDocument();

      logger.Debug("Sending the request: " + url.Replace("3b40fddfaeaf4bf786fad7e4a42ac81c","<apiKey>"));
      //logger.Debug("Sending the request: " + url);

      mvWebGrabber grabber = Utility.GetWebGrabberInstance(url);
      grabber.Encoding = Encoding.UTF8;
      grabber.Timeout = 5000;
      grabber.TimeoutIncrement = 10;
      if (grabber.GetResponse())
      {
        return grabber.GetXML();
      }
      else
      {
        logger.Debug("***** API ERROR *****: Code:{0} ({1})", grabber.errorCode, grabber.errorText);
        return null;
      }
    }
  }
}
