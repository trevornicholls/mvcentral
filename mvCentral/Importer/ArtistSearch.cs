using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Collections;
using System.Drawing;
using System.Net;
using MediaPortal.Configuration;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MusicVideos.Utils;

namespace MusicVideos.Importer
{
    class ArtistSearch
    {
      //private string api_key = "eadfb84ac56eddbf072efbfc18a90845";
      private string api_key = "166f3084d3dc07f015c42c497b42d12b";
        public ArtistSearch()
        {

        }

        public string[] GetArtists(string parsedArtist)
        {
            ArrayList artists = new ArrayList();
            XmlDocument xmldoc = new XmlDocument();
            try
            {
                xmldoc.Load("http://ws.audioscrobbler.com/2.0/?method=artist.search&artist=" + parsedArtist + "&api_key=" + api_key);
            }
            catch (XmlException e)
            {
                //try cleaning the xml
                try
                {
                    xmldoc = MusicVideosUtils.Get("http://ws.audioscrobbler.com/2.0/?method=artist.search&artist=" + parsedArtist + "&api_key=" + api_key);
                }
                catch (Exception e1) { }
            }
            catch (Exception)
            {
                MessageBox.Show("Error: You probably dont have internet connectivity");
            }
            XmlNodeList list = xmldoc.GetElementsByTagName("artist");
            
            foreach (XmlNode searchNode in list)
            {
                string mbid = searchNode.ChildNodes[1].InnerText;
                string artist_name = searchNode.ChildNodes[0].InnerText;
                artists.Add(artist_name + ";" + mbid);
            }
            return (string[])artists.ToArray(typeof(string));
        }

        public string[] GetArtistInfo(string MBID, string artistName)
        {
            XmlDocument bioDoc = new XmlDocument();
            XmlDocument imgDoc = new XmlDocument();
            bool mbid_fail = false;
            if (!(MBID == ""))
              try
              {
                bioDoc.Load("http://ws.audioscrobbler.com/2.0/?method=artist.getinfo&mbid=" + MBID + "&api_key=" + api_key);
              }
              catch
              {
                mbid_fail = true;
              }


            if ((MBID == "") || mbid_fail)
                bioDoc.Load("http://ws.audioscrobbler.com/2.0/?method=artist.getinfo&artist=" + artistName + "&api_key=" + api_key);


            XmlNodeList bioDatas = bioDoc.GetElementsByTagName("bio");
            XmlNode bioData = bioDatas[0];
            imgDoc.Load("http://ws.audioscrobbler.com/2.0/?method=artist.getimages&artist=" + artistName + "&api_key=" + api_key);
            XmlNodeList imgDatas = imgDoc.GetElementsByTagName("image");
            XmlNode imgData = imgDatas[0];
            WebClient Client = new WebClient();
            string imgFilename = Config.GetFile(Config.Dir.Thumbs, "Music Vids\\Artists\\" + artistName.Replace("\\", "").Replace("/", "") + ".jpg");

            try
            {
                for (int i = 0; i < imgData.ChildNodes.Count; i++)
                {
                    if (imgData.ChildNodes[i].Name == "sizes")
                    {
                        Client.DownloadFile(imgData.ChildNodes[i].FirstChild.InnerText, imgFilename);
                        ResizeImage(imgFilename, 400, 600);
                    }
                }
            }
            catch { }
            string bio = Regex.Replace(bioData.ChildNodes[2].InnerText, "<[^>]*>", "");
            return new string[]{
                (artistName),
                (bio),
                (imgFilename)
            };
        }

        public void ResizeImage(string OriginalFile, int NewWidth, int MaxHeight)
        {
            Image FullsizeImage =Image.FromFile(OriginalFile);

            // Prevent using images internal thumbnail
            FullsizeImage.RotateFlip(RotateFlipType.Rotate180FlipNone);
            FullsizeImage.RotateFlip(RotateFlipType.Rotate180FlipNone);

            int NewHeight = FullsizeImage.Height * NewWidth / FullsizeImage.Width;
            if (NewHeight > MaxHeight)
            {
                // Resize with height instead
                NewWidth = FullsizeImage.Width * MaxHeight / FullsizeImage.Height;
                NewHeight = MaxHeight;
            }

            Image NewImage = FullsizeImage.GetThumbnailImage(NewWidth, NewHeight, null, IntPtr.Zero);

            // Clear handle to original file so that we can overwrite it if necessary
            FullsizeImage.Dispose();

            // Save resized picture
            NewImage.Save(OriginalFile);
        }

    }
}
