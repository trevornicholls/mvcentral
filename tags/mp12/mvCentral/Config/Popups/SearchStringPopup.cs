using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using mvCentral.LocalMediaManagement;
using mvCentral.Database;

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
      musicVideoMatch = match;
      uxArtistName.Text = musicVideoMatch.Signature.Artist;
      uxTrackName.Text = musicVideoMatch.Signature.Track;
      uxAlbumName.Text = musicVideoMatch.Signature.Album;
    }

    private void okButton_Click(object sender, EventArgs e)
    {
      musicVideoMatch.Signature.Artist = uxArtistName.Text;
      musicVideoMatch.Signature.Album = uxAlbumName.Text;
      musicVideoMatch.Signature.Track = uxTrackName.Text;

    }
  }
}
