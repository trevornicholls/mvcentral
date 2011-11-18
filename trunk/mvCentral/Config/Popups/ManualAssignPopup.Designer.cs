namespace mvCentral.ConfigScreen.Popups
{
    partial class ManualAssignPopup {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
      this.okButton = new System.Windows.Forms.Button();
      this.cancelButton = new System.Windows.Forms.Button();
      this.uxTrack = new System.Windows.Forms.TextBox();
      this.lblTrack = new System.Windows.Forms.Label();
      this.lblArtist = new System.Windows.Forms.Label();
      this.fileListBox = new System.Windows.Forms.ListBox();
      this.descriptionLabel = new System.Windows.Forms.Label();
      this.uxAlbum = new System.Windows.Forms.TextBox();
      this.lblAlbum = new System.Windows.Forms.Label();
      this.uxArtist = new System.Windows.Forms.TextBox();
      this.SuspendLayout();
      // 
      // okButton
      // 
      this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.okButton.Location = new System.Drawing.Point(316, 190);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 4;
      this.okButton.Text = "OK";
      this.okButton.UseVisualStyleBackColor = true;
      // 
      // cancelButton
      // 
      this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.Location = new System.Drawing.Point(397, 190);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(75, 23);
      this.cancelButton.TabIndex = 5;
      this.cancelButton.Text = "Cancel";
      this.cancelButton.UseVisualStyleBackColor = true;
      // 
      // uxTrack
      // 
      this.uxTrack.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.uxTrack.Location = new System.Drawing.Point(62, 162);
      this.uxTrack.Name = "uxTrack";
      this.uxTrack.Size = new System.Drawing.Size(409, 20);
      this.uxTrack.TabIndex = 3;
      // 
      // lblTrack
      // 
      this.lblTrack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.lblTrack.AutoSize = true;
      this.lblTrack.Location = new System.Drawing.Point(26, 165);
      this.lblTrack.Name = "lblTrack";
      this.lblTrack.Size = new System.Drawing.Size(35, 13);
      this.lblTrack.TabIndex = 12;
      this.lblTrack.Text = "Track";
      // 
      // lblArtist
      // 
      this.lblArtist.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.lblArtist.AutoSize = true;
      this.lblArtist.Location = new System.Drawing.Point(32, 113);
      this.lblArtist.Name = "lblArtist";
      this.lblArtist.Size = new System.Drawing.Size(30, 13);
      this.lblArtist.TabIndex = 14;
      this.lblArtist.Text = "Artist";
      // 
      // fileListBox
      // 
      this.fileListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.fileListBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.fileListBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.fileListBox.FormattingEnabled = true;
      this.fileListBox.ItemHeight = 14;
      this.fileListBox.Location = new System.Drawing.Point(10, 50);
      this.fileListBox.Name = "fileListBox";
      this.fileListBox.SelectionMode = System.Windows.Forms.SelectionMode.None;
      this.fileListBox.Size = new System.Drawing.Size(461, 44);
      this.fileListBox.TabIndex = 15;
      // 
      // descriptionLabel
      // 
      this.descriptionLabel.Location = new System.Drawing.Point(11, 13);
      this.descriptionLabel.Name = "descriptionLabel";
      this.descriptionLabel.Size = new System.Drawing.Size(461, 34);
      this.descriptionLabel.TabIndex = 16;
      this.descriptionLabel.Text = "You are about to add these files as a custom and blank Music Video. You can edit " +
    "full details through the music video Manager after you click OK.";
      // 
      // uxAlbum
      // 
      this.uxAlbum.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.uxAlbum.Location = new System.Drawing.Point(63, 136);
      this.uxAlbum.Name = "uxAlbum";
      this.uxAlbum.Size = new System.Drawing.Size(409, 20);
      this.uxAlbum.TabIndex = 2;
      // 
      // lblAlbum
      // 
      this.lblAlbum.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.lblAlbum.AutoSize = true;
      this.lblAlbum.Location = new System.Drawing.Point(26, 139);
      this.lblAlbum.Name = "lblAlbum";
      this.lblAlbum.Size = new System.Drawing.Size(36, 13);
      this.lblAlbum.TabIndex = 18;
      this.lblAlbum.Text = "Album";
      // 
      // uxArtist
      // 
      this.uxArtist.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.uxArtist.Location = new System.Drawing.Point(62, 110);
      this.uxArtist.Name = "uxArtist";
      this.uxArtist.Size = new System.Drawing.Size(409, 20);
      this.uxArtist.TabIndex = 1;
      // 
      // ManualAssignPopup
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(483, 218);
      this.ControlBox = false;
      this.Controls.Add(this.uxArtist);
      this.Controls.Add(this.lblAlbum);
      this.Controls.Add(this.uxAlbum);
      this.Controls.Add(this.descriptionLabel);
      this.Controls.Add(this.fileListBox);
      this.Controls.Add(this.lblArtist);
      this.Controls.Add(this.uxTrack);
      this.Controls.Add(this.lblTrack);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.cancelButton);
      this.Name = "ManualAssignPopup";
      this.ShowInTaskbar = false;
      this.Text = "Manual MusicVideo Import";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ManualAssignPopup_FormClosing);
      this.ResumeLayout(false);
      this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TextBox uxTrack;
        private System.Windows.Forms.Label lblTrack;
        private System.Windows.Forms.Label lblArtist;
        private System.Windows.Forms.ListBox fileListBox;
        private System.Windows.Forms.Label descriptionLabel;
        private System.Windows.Forms.TextBox uxAlbum;
        private System.Windows.Forms.Label lblAlbum;
        private System.Windows.Forms.TextBox uxArtist;
    }
}