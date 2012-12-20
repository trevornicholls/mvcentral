namespace mvCentral.ConfigScreen.Popups
{
  partial class LastFMSetup
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
      this.label2 = new System.Windows.Forms.Label();
      this.btTestLogin = new System.Windows.Forms.Button();
      this.btClose = new System.Windows.Forms.Button();
      this.tbLastFMUsername = new Cornerstone.GUI.Controls.SettingsTextBox();
      this.tbLastFMPassword = new Cornerstone.GUI.Controls.SettingsTextBox();
      this.cbSubmitToLastFM = new Cornerstone.GUI.Controls.SettingCheckBox();
      this.cbShowOnLastFM = new Cornerstone.GUI.Controls.SettingCheckBox();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(26, 19);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(58, 13);
      this.label1.TabIndex = 10;
      this.label1.Text = "Username:";
      // 
      // label2
      // 
      this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(28, 48);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(56, 13);
      this.label2.TabIndex = 11;
      this.label2.Text = "Password:";
      // 
      // btTestLogin
      // 
      this.btTestLogin.Location = new System.Drawing.Point(303, 78);
      this.btTestLogin.Name = "btTestLogin";
      this.btTestLogin.Size = new System.Drawing.Size(124, 23);
      this.btTestLogin.TabIndex = 12;
      this.btTestLogin.Text = "Test Login";
      this.btTestLogin.UseVisualStyleBackColor = true;
      this.btTestLogin.Click += new System.EventHandler(this.btTestLogin_Click);
      // 
      // btClose
      // 
      this.btClose.Location = new System.Drawing.Point(303, 106);
      this.btClose.Name = "btClose";
      this.btClose.Size = new System.Drawing.Size(124, 23);
      this.btClose.TabIndex = 13;
      this.btClose.Text = "Close";
      this.btClose.UseVisualStyleBackColor = true;
      this.btClose.Click += new System.EventHandler(this.btClose_Click);
      // 
      // tbLastFMUsername
      // 
      this.tbLastFMUsername.Location = new System.Drawing.Point(92, 16);
      this.tbLastFMUsername.Name = "tbLastFMUsername";
      this.tbLastFMUsername.Setting = null;
      this.tbLastFMUsername.Size = new System.Drawing.Size(335, 20);
      this.tbLastFMUsername.TabIndex = 110;
      // 
      // tbLastFMPassword
      // 
      this.tbLastFMPassword.Location = new System.Drawing.Point(92, 45);
      this.tbLastFMPassword.Name = "tbLastFMPassword";
      this.tbLastFMPassword.PasswordChar = '*';
      this.tbLastFMPassword.Setting = null;
      this.tbLastFMPassword.Size = new System.Drawing.Size(335, 20);
      this.tbLastFMPassword.TabIndex = 111;
      // 
      // cbSubmitToLastFM
      // 
      this.cbSubmitToLastFM.AutoSize = true;
      this.cbSubmitToLastFM.IgnoreSettingName = true;
      this.cbSubmitToLastFM.Location = new System.Drawing.Point(31, 110);
      this.cbSubmitToLastFM.Name = "cbSubmitToLastFM";
      this.cbSubmitToLastFM.Setting = null;
      this.cbSubmitToLastFM.Size = new System.Drawing.Size(233, 17);
      this.cbSubmitToLastFM.TabIndex = 112;
      this.cbSubmitToLastFM.Text = "Submit now playing video to Last.FM Library\r\n";
      this.cbSubmitToLastFM.UseVisualStyleBackColor = true;
      // 
      // cbShowOnLastFM
      // 
      this.cbShowOnLastFM.AutoSize = true;
      this.cbShowOnLastFM.IgnoreSettingName = true;
      this.cbShowOnLastFM.Location = new System.Drawing.Point(31, 82);
      this.cbShowOnLastFM.Name = "cbShowOnLastFM";
      this.cbShowOnLastFM.Setting = null;
      this.cbShowOnLastFM.Size = new System.Drawing.Size(197, 17);
      this.cbShowOnLastFM.TabIndex = 113;
      this.cbShowOnLastFM.Text = "Show now playing video on Last.FM";
      this.cbShowOnLastFM.UseVisualStyleBackColor = true;
      // 
      // LastFMSetup
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(455, 133);
      this.Controls.Add(this.cbShowOnLastFM);
      this.Controls.Add(this.cbSubmitToLastFM);
      this.Controls.Add(this.tbLastFMPassword);
      this.Controls.Add(this.tbLastFMUsername);
      this.Controls.Add(this.btClose);
      this.Controls.Add(this.btTestLogin);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Name = "LastFMSetup";
      this.Text = "Last.FM User Details";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Button btTestLogin;
    private System.Windows.Forms.Button btClose;
    private Cornerstone.GUI.Controls.SettingsTextBox tbLastFMUsername;
    private Cornerstone.GUI.Controls.SettingsTextBox tbLastFMPassword;
    private Cornerstone.GUI.Controls.SettingCheckBox cbSubmitToLastFM;
    private Cornerstone.GUI.Controls.SettingCheckBox cbShowOnLastFM;
  }
}