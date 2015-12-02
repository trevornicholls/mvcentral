using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace mvCentral.ConfigScreen.Popups
{
  public partial class UpgradeWarning : Form
  {
    public UpgradeWarning()
    {
      InitializeComponent();
      Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("mvCentral.Config.rtfFiles.UpdateNotice.rtf");
      rtbUpdateNotice.LoadFile(stream, RichTextBoxStreamType.RichText);
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
