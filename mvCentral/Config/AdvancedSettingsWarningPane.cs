using System;
using System.Windows.Forms;

namespace mvCentral
{
    public partial class AdvancedSettingsWarningPane : UserControl {
        public AdvancedSettingsWarningPane() {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e) {
            warningPanel.Visible = false;
        }

        private void AdvancedSettingsWarningPane_Load(object sender, EventArgs e) {
            if (!DesignMode) {
              if (!mvCentralCore.Settings.ShowAdvancedSettingsWarning)
                    warningPanel.Visible = false;
            }
        }
    }
}
