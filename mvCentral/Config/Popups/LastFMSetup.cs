using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using mvCentral.LocalMediaManagement;
using mvCentral.Database;
using mvCentral.Utils;


namespace mvCentral.ConfigScreen.Popups
{
  public partial class LastFMSetup : Form
  {
    public LastFMSetup()
    {
      InitializeComponent();
      tbLastFMUsername.Setting = mvCentralCore.Settings["last_fm_username"];
      tbLastFMPassword.Setting = mvCentralCore.Settings["last_fm_password"];
      cbShowOnLastFM.Setting = mvCentralCore.Settings["show_on_lastfm"];
      cbSubmitToLastFM.Setting = mvCentralCore.Settings["submit_to_lastfm"];
    }


    private void btTestLogin_Click(object sender, EventArgs e)
    {
      try
      {
        LastFMScrobble profile = new LastFMScrobble();
        if (profile.Login(tbLastFMUsername.Text,tbLastFMPassword.Text))
          MessageBox.Show("Login OK!");
        else
          MessageBox.Show("Invalid login data or no connection !");
      }
      catch (Exception exception)
      {
        MessageBox.Show(exception.Message);
      }

    }

    private void btClose_Click(object sender, EventArgs e)
    {
      this.Hide();
    }
  }
}
