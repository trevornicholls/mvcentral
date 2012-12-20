namespace TranslationFileBuild
{
  partial class TranslationBuilder
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TranslationBuilder));
      this.btLoadFiles = new System.Windows.Forms.Button();
      this.tbSubItemEdit = new System.Windows.Forms.TextBox();
      this.cbLocallang = new System.Windows.Forms.ComboBox();
      this.btSave = new System.Windows.Forms.Button();
      this.lbLocalLang = new System.Windows.Forms.Label();
      this.languageBaseFolder = new System.Windows.Forms.FolderBrowserDialog();
      this.lbBaseFolder = new System.Windows.Forms.Label();
      this.cboPrimaryLangFile = new System.Windows.Forms.ComboBox();
      this.btBrowse = new System.Windows.Forms.Button();
      this.lvLangFiles = new ListViewEx.ListViewEx();
      this.Field = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.Primary = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.local = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.btReset = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // btLoadFiles
      // 
      this.btLoadFiles.Location = new System.Drawing.Point(352, 40);
      this.btLoadFiles.Name = "btLoadFiles";
      this.btLoadFiles.Size = new System.Drawing.Size(105, 23);
      this.btLoadFiles.TabIndex = 2;
      this.btLoadFiles.Text = "Load";
      this.btLoadFiles.UseVisualStyleBackColor = true;
      this.btLoadFiles.Click += new System.EventHandler(this.btLoadFiles_Click);
      // 
      // tbSubItemEdit
      // 
      this.tbSubItemEdit.Location = new System.Drawing.Point(955, 1);
      this.tbSubItemEdit.Name = "tbSubItemEdit";
      this.tbSubItemEdit.Size = new System.Drawing.Size(341, 20);
      this.tbSubItemEdit.TabIndex = 3;
      this.tbSubItemEdit.Visible = false;
      // 
      // cbLocallang
      // 
      this.cbLocallang.FormattingEnabled = true;
      this.cbLocallang.Items.AddRange(new object[] {
            "aa Afar",
            "ab Abkhazian",
            "af Afrikaans",
            "am Amharic",
            "ar Arabic",
            "as Assamese",
            "ay Aymara",
            "az Azerbaijani",
            "",
            "ba Bashkir",
            "be Byelorussian",
            "bg Bulgarian",
            "bh Bihari",
            "bi Bislama",
            "bn Bengali; Bangla",
            "bo Tibetan",
            "br Breton",
            "",
            "ca Catalan",
            "co Corsican",
            "cs Czech",
            "cy Welsh",
            "",
            "da Danish",
            "de German",
            "dz Bhutani",
            "",
            "el Greek",
            "en English",
            "eo Esperanto",
            "es Spanish",
            "et Estonian",
            "eu Basque",
            "",
            "fa Persian",
            "fi Finnish",
            "fj Fiji",
            "fo Faroese",
            "fr French",
            "fy Frisian",
            "",
            "ga Irish",
            "gd Scots Gaelic",
            "gl Galician",
            "gn Guarani",
            "gu Gujarati",
            "",
            "ha Hausa",
            "he Hebrew (formerly iw)",
            "hi Hindi",
            "hr Croatian",
            "hu Hungarian",
            "hy Armenian",
            "",
            "ia Interlingua",
            "id Indonesian (formerly in)",
            "ie Interlingue",
            "ik Inupiak",
            "is Icelandic",
            "it Italian",
            "iu Inuktitut",
            "",
            "ja Japanese",
            "jw Javanese",
            "",
            "ka Georgian",
            "kk Kazakh",
            "kl Greenlandic",
            "km Cambodian",
            "kn Kannada",
            "ko Korean",
            "ks Kashmiri",
            "ku Kurdish",
            "ky Kirghiz",
            "",
            "la Latin",
            "ln Lingala",
            "lo Laothian",
            "lt Lithuanian",
            "lv Latvian, Lettish",
            "",
            "mg Malagasy",
            "mi Maori",
            "mk Macedonian",
            "ml Malayalam",
            "mn Mongolian",
            "mo Moldavian",
            "mr Marathi",
            "ms Malay",
            "mt Maltese",
            "my Burmese",
            "",
            "na Nauru",
            "ne Nepali",
            "nl Dutch",
            "no Norwegian",
            "",
            "oc Occitan",
            "om (Afan) Oromo",
            "or Oriya",
            "",
            "pa Punjabi",
            "pl Polish",
            "ps Pashto, Pushto",
            "pt Portuguese",
            "",
            "qu Quechua",
            "",
            "rm Rhaeto-Romance",
            "rn Kirundi",
            "ro Romanian",
            "ru Russian",
            "rw Kinyarwanda",
            "",
            "sa Sanskrit",
            "sd Sindhi",
            "sg Sangho",
            "sh Serbo-Croatian",
            "si Sinhalese",
            "sk Slovak",
            "sl Slovenian",
            "sm Samoan",
            "sn Shona",
            "so Somali",
            "sq Albanian",
            "sr Serbian",
            "ss Siswati",
            "st Sesotho",
            "su Sundanese",
            "sv Swedish",
            "sw Swahili",
            "",
            "ta Tamil",
            "te Telugu",
            "tg Tajik",
            "th Thai",
            "ti Tigrinya",
            "tk Turkmen",
            "tl Tagalog",
            "tn Setswana",
            "to Tonga",
            "tr Turkish",
            "ts Tsonga",
            "tt Tatar",
            "tw Twi",
            "",
            "ug Uighur",
            "uk Ukrainian",
            "ur Urdu",
            "uz Uzbek",
            "",
            "vi Vietnamese",
            "vo Volapuk",
            "",
            "wo Wolof",
            "",
            "xh Xhosa",
            "",
            "yi Yiddish (formerly ji)",
            "yo Yoruba",
            "",
            "za Zhuang",
            "zh Chinese",
            "zu Zulu"});
      this.cbLocallang.Location = new System.Drawing.Point(166, 42);
      this.cbLocallang.Name = "cbLocallang";
      this.cbLocallang.Size = new System.Drawing.Size(160, 21);
      this.cbLocallang.TabIndex = 5;
      this.cbLocallang.SelectedIndexChanged += new System.EventHandler(this.cbLocallang_SelectedIndexChanged);
      // 
      // btSave
      // 
      this.btSave.Location = new System.Drawing.Point(482, 40);
      this.btSave.Name = "btSave";
      this.btSave.Size = new System.Drawing.Size(105, 23);
      this.btSave.TabIndex = 6;
      this.btSave.Text = "Save";
      this.btSave.UseVisualStyleBackColor = true;
      this.btSave.Click += new System.EventHandler(this.btSave_Click);
      // 
      // lbLocalLang
      // 
      this.lbLocalLang.AutoSize = true;
      this.lbLocalLang.Location = new System.Drawing.Point(43, 45);
      this.lbLocalLang.Name = "lbLocalLang";
      this.lbLocalLang.Size = new System.Drawing.Size(117, 13);
      this.lbLocalLang.TabIndex = 7;
      this.lbLocalLang.Text = "Select Local Langauge";
      // 
      // lbBaseFolder
      // 
      this.lbBaseFolder.AutoSize = true;
      this.lbBaseFolder.Location = new System.Drawing.Point(17, 15);
      this.lbBaseFolder.Name = "lbBaseFolder";
      this.lbBaseFolder.Size = new System.Drawing.Size(144, 13);
      this.lbBaseFolder.TabIndex = 8;
      this.lbBaseFolder.Text = "Select Primary Language File";
      // 
      // cboPrimaryLangFile
      // 
      this.cboPrimaryLangFile.AllowDrop = true;
      this.cboPrimaryLangFile.FormattingEnabled = true;
      this.cboPrimaryLangFile.Location = new System.Drawing.Point(167, 12);
      this.cboPrimaryLangFile.Name = "cboPrimaryLangFile";
      this.cboPrimaryLangFile.Size = new System.Drawing.Size(469, 21);
      this.cboPrimaryLangFile.TabIndex = 9;
      this.cboPrimaryLangFile.DragDrop += new System.Windows.Forms.DragEventHandler(this.cboPrimaryLangFile_DragDrop);
      this.cboPrimaryLangFile.DragEnter += new System.Windows.Forms.DragEventHandler(this.cboPrimaryLangFile_DragEnter);
      // 
      // btBrowse
      // 
      this.btBrowse.Location = new System.Drawing.Point(642, 12);
      this.btBrowse.Name = "btBrowse";
      this.btBrowse.Size = new System.Drawing.Size(75, 23);
      this.btBrowse.TabIndex = 10;
      this.btBrowse.Text = "Browse";
      this.btBrowse.UseVisualStyleBackColor = true;
      this.btBrowse.Click += new System.EventHandler(this.btBrowse_Click);
      // 
      // lvLangFiles
      // 
      this.lvLangFiles.AllowColumnReorder = true;
      this.lvLangFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.lvLangFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Field,
            this.Primary,
            this.local});
      this.lvLangFiles.DoubleClickActivation = false;
      this.lvLangFiles.FullRowSelect = true;
      this.lvLangFiles.GridLines = true;
      this.lvLangFiles.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
      this.lvLangFiles.Location = new System.Drawing.Point(12, 78);
      this.lvLangFiles.Name = "lvLangFiles";
      this.lvLangFiles.Size = new System.Drawing.Size(984, 640);
      this.lvLangFiles.TabIndex = 4;
      this.lvLangFiles.UseCompatibleStateImageBehavior = false;
      this.lvLangFiles.View = System.Windows.Forms.View.Details;
      this.lvLangFiles.SubItemClicked += new ListViewEx.SubItemEventHandler(this.lvLangFiles_SubItemClicked);
      this.lvLangFiles.SubItemEndEditing += new ListViewEx.SubItemEndEditingEventHandler(this.lvLangFiles_SubItemEndEditing);
      // 
      // Field
      // 
      this.Field.Text = "Field";
      this.Field.Width = 180;
      // 
      // Primary
      // 
      this.Primary.Text = "Primay Language";
      this.Primary.Width = 374;
      // 
      // local
      // 
      this.local.Text = "Local Langauge";
      this.local.Width = 420;
      // 
      // btReset
      // 
      this.btReset.Location = new System.Drawing.Point(612, 42);
      this.btReset.Name = "btReset";
      this.btReset.Size = new System.Drawing.Size(105, 23);
      this.btReset.TabIndex = 11;
      this.btReset.Text = "Reset";
      this.btReset.UseVisualStyleBackColor = true;
      this.btReset.Click += new System.EventHandler(this.btReset_Click);
      // 
      // TranslationBuilder
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1008, 730);
      this.Controls.Add(this.btReset);
      this.Controls.Add(this.btLoadFiles);
      this.Controls.Add(this.btBrowse);
      this.Controls.Add(this.cboPrimaryLangFile);
      this.Controls.Add(this.lbBaseFolder);
      this.Controls.Add(this.lbLocalLang);
      this.Controls.Add(this.btSave);
      this.Controls.Add(this.cbLocallang);
      this.Controls.Add(this.lvLangFiles);
      this.Controls.Add(this.tbSubItemEdit);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Name = "TranslationBuilder";
      this.Text = "Translation File Builder";
      this.Resize += new System.EventHandler(this.TranslationBuilder_Resize);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button btLoadFiles;
    private System.Windows.Forms.TextBox tbSubItemEdit;
    private ListViewEx.ListViewEx lvLangFiles;
    private System.Windows.Forms.ColumnHeader Field;
    private System.Windows.Forms.ColumnHeader Primary;
    private System.Windows.Forms.ColumnHeader local;
    private System.Windows.Forms.ComboBox cbLocallang;
    private System.Windows.Forms.Button btSave;
    private System.Windows.Forms.Label lbLocalLang;
    private System.Windows.Forms.FolderBrowserDialog languageBaseFolder;
    private System.Windows.Forms.Label lbBaseFolder;
    private System.Windows.Forms.ComboBox cboPrimaryLangFile;
    private System.Windows.Forms.Button btBrowse;
    private System.Windows.Forms.Button btReset;
  }
}

