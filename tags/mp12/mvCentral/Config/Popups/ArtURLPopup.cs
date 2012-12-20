using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace mvCentral.ConfigScreen.Popups {
    public partial class ArtURLPopup : Form {
        private Color defaultColor;
        
        public ArtURLPopup() {
            InitializeComponent();

            defaultColor = urlTextBox.ForeColor;
        }

        public string GetURL() {
            return urlTextBox.Text;
        }

        private void okButton_Click(object sender, EventArgs e) {
            Close();
        }

    }
}
