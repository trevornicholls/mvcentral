namespace mvCentral.ConfigScreen.Popups
{
  partial class CreateAlbumForTrack
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
      this.label1 = new System.Windows.Forms.Label();
      this.tbArtistName = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.tbAlbumName = new System.Windows.Forms.TextBox();
      this.btLookUp = new System.Windows.Forms.Button();
      this.btOK = new System.Windows.Forms.Button();
      this.btCancel = new System.Windows.Forms.Button();
      this.label3 = new System.Windows.Forms.Label();
      this.tbTrackName = new System.Windows.Forms.TextBox();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(29, 15);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(33, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "Artist:";
      // 
      // tbArtistName
      // 
      this.tbArtistName.Location = new System.Drawing.Point(71, 12);
      this.tbArtistName.Name = "tbArtistName";
      this.tbArtistName.Size = new System.Drawing.Size(378, 20);
      this.tbArtistName.TabIndex = 1;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(26, 88);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(39, 13);
      this.label2.TabIndex = 3;
      this.label2.Text = "Album:";
      // 
      // tbAlbumName
      // 
      this.tbAlbumName.Location = new System.Drawing.Point(71, 85);
      this.tbAlbumName.Name = "tbAlbumName";
      this.tbAlbumName.Size = new System.Drawing.Size(305, 20);
      this.tbAlbumName.TabIndex = 4;
      // 
      // btLookUp
      // 
      this.btLookUp.Location = new System.Drawing.Point(382, 82);
      this.btLookUp.Name = "btLookUp";
      this.btLookUp.Size = new System.Drawing.Size(67, 23);
      this.btLookUp.TabIndex = 5;
      this.btLookUp.Text = "Lookup";
      this.btLookUp.UseVisualStyleBackColor = true;
      this.btLookUp.Click += new System.EventHandler(this.btLookUp_Click);
      // 
      // btOK
      // 
      this.btOK.Image = global::mvCentral.Properties.Resources.OK;
      this.btOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.btOK.Location = new System.Drawing.Point(310, 117);
      this.btOK.Name = "btOK";
      this.btOK.Size = new System.Drawing.Size(66, 23);
      this.btOK.TabIndex = 6;
      this.btOK.Text = "OK";
      this.btOK.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.btOK.UseVisualStyleBackColor = true;
      this.btOK.Click += new System.EventHandler(this.btOK_Click);
      // 
      // btCancel
      // 
      this.btCancel.Image = global::mvCentral.Properties.Resources.Cancel;
      this.btCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.btCancel.Location = new System.Drawing.Point(383, 117);
      this.btCancel.Name = "btCancel";
      this.btCancel.Size = new System.Drawing.Size(66, 23);
      this.btCancel.TabIndex = 7;
      this.btCancel.Text = "Cancel";
      this.btCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.btCancel.UseVisualStyleBackColor = true;
      this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(24, 42);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(38, 13);
      this.label3.TabIndex = 8;
      this.label3.Text = "Track:";
      // 
      // tbTrackName
      // 
      this.tbTrackName.Location = new System.Drawing.Point(71, 39);
      this.tbTrackName.Name = "tbTrackName";
      this.tbTrackName.Size = new System.Drawing.Size(378, 20);
      this.tbTrackName.TabIndex = 9;
      // 
      // CreateAlbumForTrack
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(492, 152);
      this.Controls.Add(this.tbTrackName);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.btCancel);
      this.Controls.Add(this.btOK);
      this.Controls.Add(this.btLookUp);
      this.Controls.Add(this.tbAlbumName);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.tbArtistName);
      this.Controls.Add(this.label1);
      this.Name = "CreateAlbumForTrack";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Add Track to Album";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox tbArtistName;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox tbAlbumName;
    private System.Windows.Forms.Button btLookUp;
    private System.Windows.Forms.Button btOK;
    private System.Windows.Forms.Button btCancel;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TextBox tbTrackName;
  }
}