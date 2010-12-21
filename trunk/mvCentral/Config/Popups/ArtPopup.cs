using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace mvCentral.ConfigScreen.Popups {
    public partial class ArtPopup : Form {
        public ArtPopup() {
            InitializeComponent();
        }

        public ArtPopup(string filename) {
            InitializeComponent();
            try {
                pictureBox.Image = Image.FromFile(filename);
            }
            catch (Exception) { }
        }

        private void pictureBox_Click(object sender, EventArgs e) {
            this.Close();
            pictureBox.Image.Dispose();
        }

        private void ArtPopup_KeyPress(object sender, KeyPressEventArgs e) {
            this.Close();
            pictureBox.Image.Dispose();
        }

        private void ArtPopup_Deactivate(object sender, EventArgs e) {
            this.Close();
            pictureBox.Image.Dispose();
        }

        // if we have been launched as a dialog and we have an owner, center
        // on the owning form.
        private void ArtPopup_Shown(object sender, EventArgs e) {

        }

        private void ArtPopup_Load(object sender, EventArgs e) {
            if (Owner == null)
                return;

            Point center = new Point();
            center.X = Owner.Location.X + (Owner.Width / 2);
            center.Y = Owner.Location.Y + (Owner.Height / 2);

            Point newLocation = new Point();
            newLocation.X = center.X - (Width / 2);
            newLocation.Y = center.Y - (Height / 2);

            Location = newLocation;
        }


    }
}
