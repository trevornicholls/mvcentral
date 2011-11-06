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
  public partial class CustomArtworkFolders : Form
  {
    public CustomArtworkFolders()
    {
      InitializeComponent();
      // Artist Custom Folder
      cbLocalArtistArtSearch.Setting = mvCentralCore.Settings["artist_artwork_from_custom_folder"];
      tbCustomArtistArtFolder.Setting = mvCentralCore.Settings["local_artistart_folder"];
      tbArtistPatternMask.Setting = mvCentralCore.Settings["local_artistart_pattern"];
      // Album Custom Folder
      cbLocalAlbumArtSearch.Setting = mvCentralCore.Settings["album_artwork_from_custom_folder"];
      tbCustomAlbumArtFolder.Setting = mvCentralCore.Settings["local_albumart_folder"];
      tbAlbumPatternMask.Setting = mvCentralCore.Settings["local_albumart_pattern"];
      // Track Custom Folder
      cbLocalTrackArtSearch.Setting = mvCentralCore.Settings["track_artwork_from_custom_folder"];
      tbCustomTrackArtFolder.Setting = mvCentralCore.Settings["local_trackart_folder"];
      tbTrackPatternMask.Setting = mvCentralCore.Settings["local_trackart_pattern"];

      enableCheckCustomFolderArtworkControls();
    }


    #region Private Methods

    private void cbLocalArtistArtSearch_CheckedChanged(object sender, EventArgs e)
    {
      enableCheckCustomFolderArtworkControls();
    }

    private void cbLocalAlbumArtSearch_CheckedChanged(object sender, EventArgs e)
    {
      enableCheckCustomFolderArtworkControls();
    }

    private void cbLocalTrackArtSearch_CheckedChanged(object sender, EventArgs e)
    {
      enableCheckCustomFolderArtworkControls();
    }

    void enableCheckCustomFolderArtworkControls()
    {
      if (cbLocalArtistArtSearch.Checked)
      {
        tbCustomArtistArtFolder.Enabled = true;
        tbArtistPatternMask.Enabled = true;
        btSelectLocalArtistArtFolder.Enabled = true;
      }
      else
      {
        tbCustomArtistArtFolder.Enabled = false;
        tbArtistPatternMask.Enabled = false; ;
        btSelectLocalArtistArtFolder.Enabled = false;
      }

      if (cbLocalAlbumArtSearch.Checked)
      {
        tbCustomAlbumArtFolder.Enabled = true;
        tbAlbumPatternMask.Enabled = true;
        btSelectLocalAlbumArtFolder.Enabled = true;
      }
      else
      {
        tbCustomAlbumArtFolder.Enabled = false;
        tbAlbumPatternMask.Enabled = false;
        btSelectLocalAlbumArtFolder.Enabled = false;
      }

      if (cbLocalTrackArtSearch.Checked)
      {
        tbCustomTrackArtFolder.Enabled = true;
        tbTrackPatternMask.Enabled = true;
        btSelectLocalTrackArtFolder.Enabled = true;
      }
      else
      {
        tbCustomTrackArtFolder.Enabled = false;
        tbTrackPatternMask.Enabled = false;
        btSelectLocalTrackArtFolder.Enabled = false;
      }

    }

    private void btSelectLocalArtistArtFolder_Click(object sender, EventArgs e)
    {
      FolderBrowserDialog customFolder = new FolderBrowserDialog();
      if (customFolder.ShowDialog() == DialogResult.OK)
      {
        tbCustomArtistArtFolder.Text = customFolder.SelectedPath;
        mvCentralCore.Settings.CustomArtistArtFolder = tbCustomArtistArtFolder.Text;
      }
    }

    private void btSelectLocalAlbumArtFolder_Click(object sender, EventArgs e)
    {
      FolderBrowserDialog customFolder = new FolderBrowserDialog();
      if (customFolder.ShowDialog() == DialogResult.OK)
      {
        tbCustomAlbumArtFolder.Text = customFolder.SelectedPath;
        mvCentralCore.Settings.CustomAlbumArtFolder = tbCustomAlbumArtFolder.Text;
      }
    }

    private void btSelectLocalTrackArtFolder_Click(object sender, EventArgs e)
    {
      FolderBrowserDialog customFolder = new FolderBrowserDialog();
      if (customFolder.ShowDialog() == DialogResult.OK)
      {
        tbCustomTrackArtFolder.Text = customFolder.SelectedPath;
        mvCentralCore.Settings.CustomTrackArtFolder = tbCustomTrackArtFolder.Text;
      }
    }

    private void btClose_Click(object sender, EventArgs e)
    {
      this.Hide();
    }

    #endregion

  }
}
