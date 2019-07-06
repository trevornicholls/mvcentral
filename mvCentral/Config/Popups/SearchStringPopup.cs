using mvCentral.Database;
using mvCentral.LocalMediaManagement;

using System;
using System.Windows.Forms;

namespace mvCentral.ConfigScreen.Popups
{
  public partial class SearchStringPopup : Form
  {

    private MusicVideoMatch musicVideoMatch;

    public SearchStringPopup()
    {
      InitializeComponent();
    }

    public SearchStringPopup(MusicVideoMatch match)
    {
      InitializeComponent();
      foreach (DBLocalMedia currFile in match.LocalMedia)
      {
        fileListBox.Items.Add(currFile.File);
      }
      try
      {
        musicVideoMatch = match;
      }
      catch { };
      try
      {
        uxArtistName.Text = musicVideoMatch.Signature.Artist;
      }
      catch { };
      try
      {
        uxTrackName.Text = musicVideoMatch.Signature.Track;
      }
      catch { };
      try
      {
        uxAlbumName.Text = musicVideoMatch.Signature.Album;
      }
      catch { };
    }

    private void okButton_Click(object sender, EventArgs e)
    {
      musicVideoMatch.Signature.Artist = uxArtistName.Text;
      musicVideoMatch.Signature.Album = uxAlbumName.Text;
      musicVideoMatch.Signature.Track = uxTrackName.Text;
    }
  }
}
