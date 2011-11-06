namespace mvCentral.ConfigScreen.Popups
{
    partial class SearchStringPopup {
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
      this.descriptionLabel = new System.Windows.Forms.TextBox();
      this.fileListBox = new System.Windows.Forms.ListBox();
      this.uxArtistName = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.uxTrackName = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.uxAlbumName = new System.Windows.Forms.TextBox();
      this.SuspendLayout();
      // 
      // okButton
      // 
      this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.okButton.Location = new System.Drawing.Point(311, 193);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 3;
      this.okButton.Text = "OK";
      this.okButton.UseVisualStyleBackColor = true;
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // cancelButton
      // 
      this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.Location = new System.Drawing.Point(396, 193);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(75, 23);
      this.cancelButton.TabIndex = 2;
      this.cancelButton.Text = "Cancel";
      this.cancelButton.UseVisualStyleBackColor = true;
      // 
      // descriptionLabel
      // 
      this.descriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.descriptionLabel.BackColor = System.Drawing.SystemColors.ActiveBorder;
      this.descriptionLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.descriptionLabel.Location = new System.Drawing.Point(12, 12);
      this.descriptionLabel.Multiline = true;
      this.descriptionLabel.Name = "descriptionLabel";
      this.descriptionLabel.ReadOnly = true;
      this.descriptionLabel.Size = new System.Drawing.Size(459, 30);
      this.descriptionLabel.TabIndex = 5;
      this.descriptionLabel.Text = "Please enter a new search string for the following file(s).";
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
      this.fileListBox.Location = new System.Drawing.Point(12, 49);
      this.fileListBox.Name = "fileListBox";
      this.fileListBox.SelectionMode = System.Windows.Forms.SelectionMode.None;
      this.fileListBox.Size = new System.Drawing.Size(459, 58);
      this.fileListBox.TabIndex = 6;
      // 
      // uxArtistName
      // 
      this.uxArtistName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.uxArtistName.Location = new System.Drawing.Point(82, 114);
      this.uxArtistName.Name = "uxArtistName";
      this.uxArtistName.Size = new System.Drawing.Size(389, 20);
      this.uxArtistName.TabIndex = 7;
      // 
      // label1
      // 
      this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(38, 114);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(30, 13);
      this.label1.TabIndex = 17;
      this.label1.Text = "Artist";
      // 
      // uxTrackName
      // 
      this.uxTrackName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.uxTrackName.Location = new System.Drawing.Point(82, 167);
      this.uxTrackName.Name = "uxTrackName";
      this.uxTrackName.Size = new System.Drawing.Size(389, 20);
      this.uxTrackName.TabIndex = 18;
      // 
      // label2
      // 
      this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(13, 170);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(58, 13);
      this.label2.TabIndex = 19;
      this.label2.Text = "Track Title";
      // 
      // label3
      // 
      this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(32, 144);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(36, 13);
      this.label3.TabIndex = 21;
      this.label3.Text = "Album";
      // 
      // uxAlbumName
      // 
      this.uxAlbumName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.uxAlbumName.Location = new System.Drawing.Point(82, 141);
      this.uxAlbumName.Name = "uxAlbumName";
      this.uxAlbumName.Size = new System.Drawing.Size(389, 20);
      this.uxAlbumName.TabIndex = 20;
      // 
      // SearchStringPopup
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.cancelButton;
      this.ClientSize = new System.Drawing.Size(483, 218);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.uxAlbumName);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.uxTrackName);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.uxArtistName);
      this.Controls.Add(this.fileListBox);
      this.Controls.Add(this.descriptionLabel);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.cancelButton);
      this.Name = "SearchStringPopup";
      this.ShowInTaskbar = false;
      this.Text = "Enter New  Search Options";
      this.ResumeLayout(false);
      this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TextBox descriptionLabel;
        private System.Windows.Forms.ListBox fileListBox;
        private System.Windows.Forms.TextBox uxArtistName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox uxTrackName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox uxAlbumName;
    }
}