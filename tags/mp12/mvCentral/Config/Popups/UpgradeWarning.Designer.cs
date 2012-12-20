namespace mvCentral.ConfigScreen.Popups
{
  partial class UpgradeWarning
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
      this.rtbUpdateNotice = new System.Windows.Forms.RichTextBox();
      this.gbFolderLayout = new System.Windows.Forms.GroupBox();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.customFolderLayout = new System.Windows.Forms.RadioButton();
      this.stdFolderLayout = new System.Windows.Forms.RadioButton();
      this.btSave = new System.Windows.Forms.Button();
      this.panel1 = new System.Windows.Forms.Panel();
      this.gbFolderLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // rtbUpdateNotice
      // 
      this.rtbUpdateNotice.BackColor = System.Drawing.SystemColors.ButtonHighlight;
      this.rtbUpdateNotice.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.rtbUpdateNotice.Location = new System.Drawing.Point(12, 12);
      this.rtbUpdateNotice.Name = "rtbUpdateNotice";
      this.rtbUpdateNotice.ReadOnly = true;
      this.rtbUpdateNotice.Size = new System.Drawing.Size(582, 192);
      this.rtbUpdateNotice.TabIndex = 0;
      this.rtbUpdateNotice.Text = "";
      // 
      // gbFolderLayout
      // 
      this.gbFolderLayout.Controls.Add(this.label2);
      this.gbFolderLayout.Controls.Add(this.label1);
      this.gbFolderLayout.Controls.Add(this.customFolderLayout);
      this.gbFolderLayout.Controls.Add(this.stdFolderLayout);
      this.gbFolderLayout.Location = new System.Drawing.Point(12, 217);
      this.gbFolderLayout.Name = "gbFolderLayout";
      this.gbFolderLayout.Size = new System.Drawing.Size(464, 106);
      this.gbFolderLayout.TabIndex = 1;
      this.gbFolderLayout.TabStop = false;
      this.gbFolderLayout.Text = "Folder Layout Selection";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(10, 65);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(47, 13);
      this.label2.TabIndex = 4;
      this.label2.Text = "Option 2";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(10, 32);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(47, 13);
      this.label1.TabIndex = 3;
      this.label1.Text = "Option 1";
      // 
      // customFolderLayout
      // 
      this.customFolderLayout.AutoSize = true;
      this.customFolderLayout.Location = new System.Drawing.Point(63, 63);
      this.customFolderLayout.Name = "customFolderLayout";
      this.customFolderLayout.Size = new System.Drawing.Size(364, 17);
      this.customFolderLayout.TabIndex = 1;
      this.customFolderLayout.TabStop = true;
      this.customFolderLayout.Text = "Parse Folder Layout - (../<artist>/<album>/<track>.<ext>) layout is used.";
      this.customFolderLayout.UseVisualStyleBackColor = true;
      // 
      // stdFolderLayout
      // 
      this.stdFolderLayout.AutoSize = true;
      this.stdFolderLayout.Location = new System.Drawing.Point(63, 30);
      this.stdFolderLayout.Name = "stdFolderLayout";
      this.stdFolderLayout.Size = new System.Drawing.Size(372, 17);
      this.stdFolderLayout.TabIndex = 0;
      this.stdFolderLayout.TabStop = true;
      this.stdFolderLayout.Text = "Parse Filename Only - Folder layout is not used for Artist, Album and Track";
      this.stdFolderLayout.UseVisualStyleBackColor = true;
      // 
      // btSave
      // 
      this.btSave.Location = new System.Drawing.Point(482, 299);
      this.btSave.Name = "btSave";
      this.btSave.Size = new System.Drawing.Size(112, 23);
      this.btSave.TabIndex = 2;
      this.btSave.Text = "Save";
      this.btSave.UseVisualStyleBackColor = true;
      this.btSave.Click += new System.EventHandler(this.btSave_Click);
      // 
      // panel1
      // 
      this.panel1.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
      this.panel1.Location = new System.Drawing.Point(11, 11);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(584, 194);
      this.panel1.TabIndex = 3;
      // 
      // UpgradeWarning
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(606, 339);
      this.ControlBox = false;
      this.Controls.Add(this.btSave);
      this.Controls.Add(this.gbFolderLayout);
      this.Controls.Add(this.rtbUpdateNotice);
      this.Controls.Add(this.panel1);
      this.Name = "UpgradeWarning";
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Database Upgrade";
      this.gbFolderLayout.ResumeLayout(false);
      this.gbFolderLayout.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.RichTextBox rtbUpdateNotice;
    private System.Windows.Forms.GroupBox gbFolderLayout;
    private System.Windows.Forms.RadioButton customFolderLayout;
    private System.Windows.Forms.RadioButton stdFolderLayout;
    private System.Windows.Forms.Button btSave;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Panel panel1;
  }
}