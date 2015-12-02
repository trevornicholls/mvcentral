using System;
using System.Windows.Forms;

namespace mvCentral.ConfigScreen.Popups
{
  public partial class ArtworkImportOptions : Form
  {

    public ArtworkImportOptions()
    {
      InitializeComponent();

      // Artwork Settings
      tbMinArtWidth.Setting = mvCentralCore.Settings["min_artist_width"];
      tbMinArtHeight.Setting = mvCentralCore.Settings["min_artist_height"];
      tbMinAlbumWidth.Setting = mvCentralCore.Settings["min_album_width"];
      tbMinAlbumMinHeight.Setting = mvCentralCore.Settings["min_album_height"];
      tbTrackArtWidth.Setting = mvCentralCore.Settings["min_track_width"];
      tbTrackArtHeight.Setting = mvCentralCore.Settings["min_track_height"];
      tbMaxArtistArtwork.Setting = mvCentralCore.Settings["max_artist_arts"];
      tbMaxAlbumArtwork.Setting = mvCentralCore.Settings["max_album_arts"];
      tbMaxVideoArtwork.Setting = mvCentralCore.Settings["max_track_arts"];
    }

    private void btClose_Click(object sender, EventArgs e)
    {
      this.Hide();
    }
  }
}
