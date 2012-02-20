using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace mvCentral.ConfigScreen.Popups
{
  public partial class UpgradeWarning : Form
  {
    public UpgradeWarning()
    {
      InitializeComponent();
      if (mvCentralCore.Settings.IgnoreFoldersWhenParsing)
      {
        stdFolderLayout.Checked = true;
        customFolderLayout.Checked = false;
      }
      else
      {
        stdFolderLayout.Checked = false;
        customFolderLayout.Checked = true;
      }
    }

    private void btSave_Click(object sender, EventArgs e)
    {
      if (stdFolderLayout.Checked)
        mvCentralCore.Settings.IgnoreFoldersWhenParsing = true;
      else if (customFolderLayout.Checked)
        mvCentralCore.Settings.IgnoreFoldersWhenParsing = false;

      this.Hide();
    }
  }
}
