﻿using System;
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
    public partial class ManualAssignPopup : Form {
        public ManualAssignPopup() {
            InitializeComponent();
            uxTrack.Enabled = (bool)mvCentralCore.Settings["importer_split_dvd"].Value;
        }

        public ManualAssignPopup(MusicVideoMatch match)
        {
            InitializeComponent();
            foreach (DBLocalMedia currFile in match.LocalMedia)
            {
                fileListBox.Items.Add(currFile.File);
            }
            uxTrack.Text = match.Signature.Track;
            uxArtist.Text = match.Signature.Artist;
            uxAlbum.Text = match.Signature.Album;
            if (match.LocalMedia[0].IsDVD)
            {
                uxTrack.Enabled = !(bool)mvCentralCore.Settings["importer_split_dvd"].Value;
                lblTrack.Enabled = !(bool)mvCentralCore.Settings["importer_split_dvd"].Value;
            }
        }

        public string Track { get { return uxTrack.Text; } }
        public string Album { get { return uxAlbum.Text; } }
        public string Artist { get { return uxArtist.Text; } }

        private void ManualAssignPopup_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.Cancel) return;
            if (uxTrack.Enabled)
            if (Track.Trim().Length == 0 || Artist.Trim().Length == 0)
            {
                MessageBox.Show("Artist and track are mandatory!!", "Result",
                    MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                e.Cancel = true;
            }
        }

    }
} 
