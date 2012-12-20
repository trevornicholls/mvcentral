namespace mvCentral
{
    partial class loadingDisplay
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
      this.lbTask = new System.Windows.Forms.Label();
      this.artists = new System.Windows.Forms.Label();
      this.videos = new System.Windows.Forms.Label();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.version = new System.Windows.Forms.Label();
      this.albums = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.SuspendLayout();
      // 
      // lbTask
      // 
      this.lbTask.AutoSize = true;
      this.lbTask.BackColor = System.Drawing.Color.Transparent;
      this.lbTask.Location = new System.Drawing.Point(216, 155);
      this.lbTask.Name = "lbTask";
      this.lbTask.Size = new System.Drawing.Size(163, 13);
      this.lbTask.TabIndex = 0;
      this.lbTask.Text = "Please wait, loading your library...";
      this.lbTask.UseWaitCursor = true;
      // 
      // artists
      // 
      this.artists.AutoSize = true;
      this.artists.BackColor = System.Drawing.Color.Transparent;
      this.artists.Location = new System.Drawing.Point(138, 172);
      this.artists.Name = "artists";
      this.artists.Size = new System.Drawing.Size(35, 13);
      this.artists.TabIndex = 3;
      this.artists.Text = "label3";
      this.artists.UseWaitCursor = true;
      // 
      // videos
      // 
      this.videos.AutoSize = true;
      this.videos.BackColor = System.Drawing.Color.Transparent;
      this.videos.Location = new System.Drawing.Point(415, 172);
      this.videos.Name = "videos";
      this.videos.Size = new System.Drawing.Size(35, 13);
      this.videos.TabIndex = 5;
      this.videos.Text = "label3";
      this.videos.UseWaitCursor = true;
      // 
      // pictureBox1
      // 
      this.pictureBox1.ErrorImage = null;
      this.pictureBox1.Image = global::mvCentral.Properties.Resources.mvCentralLogo;
      this.pictureBox1.InitialImage = global::mvCentral.Properties.Resources.mvCentralLogo;
      this.pictureBox1.Location = new System.Drawing.Point(12, 4);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(560, 148);
      this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.pictureBox1.TabIndex = 1;
      this.pictureBox1.TabStop = false;
      this.pictureBox1.UseWaitCursor = true;
      // 
      // version
      // 
      this.version.AutoSize = true;
      this.version.BackColor = System.Drawing.Color.Transparent;
      this.version.Location = new System.Drawing.Point(277, 131);
      this.version.Name = "version";
      this.version.Size = new System.Drawing.Size(41, 13);
      this.version.TabIndex = 6;
      this.version.Text = "version";
      this.version.UseWaitCursor = true;
      // 
      // albums
      // 
      this.albums.AutoSize = true;
      this.albums.BackColor = System.Drawing.Color.Transparent;
      this.albums.Location = new System.Drawing.Point(277, 172);
      this.albums.Name = "albums";
      this.albums.Size = new System.Drawing.Size(35, 13);
      this.albums.TabIndex = 7;
      this.albums.Text = "label3";
      this.albums.UseWaitCursor = true;
      // 
      // loadingDisplay
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.ClientSize = new System.Drawing.Size(584, 194);
      this.Controls.Add(this.albums);
      this.Controls.Add(this.version);
      this.Controls.Add(this.lbTask);
      this.Controls.Add(this.videos);
      this.Controls.Add(this.artists);
      this.Controls.Add(this.pictureBox1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.Name = "loadingDisplay";
      this.Opacity = 0.9D;
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Please wait...";
      this.TransparencyKey = System.Drawing.Color.Maroon;
      this.UseWaitCursor = true;
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbTask;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label artists;
        private System.Windows.Forms.Label videos;
        private System.Windows.Forms.Label version;
        private System.Windows.Forms.Label albums;
    }
}