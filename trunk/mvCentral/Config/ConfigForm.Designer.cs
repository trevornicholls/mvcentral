﻿namespace mvCentral
{
    partial class ConfigForm
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
          this.components = new System.ComponentModel.Container();
          System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigForm));
          System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle14 = new System.Windows.Forms.DataGridViewCellStyle();
          System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle15 = new System.Windows.Forms.DataGridViewCellStyle();
          System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle16 = new System.Windows.Forms.DataGridViewCellStyle();
          System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle17 = new System.Windows.Forms.DataGridViewCellStyle();
          System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle18 = new System.Windows.Forms.DataGridViewCellStyle();
          System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle19 = new System.Windows.Forms.DataGridViewCellStyle();
          System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle20 = new System.Windows.Forms.DataGridViewCellStyle();
          System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle21 = new System.Windows.Forms.DataGridViewCellStyle();
          System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle22 = new System.Windows.Forms.DataGridViewCellStyle();
          System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle23 = new System.Windows.Forms.DataGridViewCellStyle();
          System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle24 = new System.Windows.Forms.DataGridViewCellStyle();
          System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle25 = new System.Windows.Forms.DataGridViewCellStyle();
          System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle26 = new System.Windows.Forms.DataGridViewCellStyle();
          this.imageList = new System.Windows.Forms.ImageList(this.components);
          this.textBox6 = new System.Windows.Forms.TextBox();
          this.label9 = new System.Windows.Forms.Label();
          this.label10 = new System.Windows.Forms.Label();
          this.label11 = new System.Windows.Forms.Label();
          this.listBox2 = new System.Windows.Forms.ListBox();
          this.button6 = new System.Windows.Forms.Button();
          this.button7 = new System.Windows.Forms.Button();
          this.getWatchFolder = new System.Windows.Forms.FolderBrowserDialog();
          this.getDT = new System.Windows.Forms.OpenFileDialog();
          this.pictureBox4 = new System.Windows.Forms.PictureBox();
          this.scMain = new System.Windows.Forms.SplitContainer();
          this.mainTab = new System.Windows.Forms.TabControl();
          this.tpLibrary = new System.Windows.Forms.TabPage();
          this.splitContainer3 = new System.Windows.Forms.SplitContainer();
          this.mvLibraryTreeView = new System.Windows.Forms.TreeView();
          this.cmLibrary = new System.Windows.Forms.ContextMenuStrip(this.components);
          this.sentToImporterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
          this.getArtToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
          this.fromFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
          this.fromURLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
          this.fromOnlinjeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
          this.asVideoThumnailToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
          this.autoGrabFrame30SecsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
          this.tsmGrabFrame = new System.Windows.Forms.ToolStripMenuItem();
          this.tsmRemove = new System.Windows.Forms.ToolStripMenuItem();
          this.tsmGetInfo = new System.Windows.Forms.ToolStripMenuItem();
          this.imageListTreeView = new System.Windows.Forms.ImageList(this.components);
          this.lblArtResolution = new System.Windows.Forms.Label();
          this.lblArtNum = new System.Windows.Forms.Label();
          this.artworkProgressBar = new System.Windows.Forms.ProgressBar();
          this.artImage = new System.Windows.Forms.PictureBox();
          this.coverToolStrip = new System.Windows.Forms.ToolStrip();
          this.btnPrevArt = new System.Windows.Forms.ToolStripButton();
          this.btnNextArt = new System.Windows.Forms.ToolStripButton();
          this.btnArtDelete = new System.Windows.Forms.ToolStripButton();
          this.btnArtZoom = new System.Windows.Forms.ToolStripButton();
          this.tcMusicVideo = new System.Windows.Forms.TabControl();
          this.tpArtist = new System.Windows.Forms.TabPage();
          this.scArtistList = new System.Windows.Forms.SplitContainer();
          this.artistDetailsList = new Cornerstone.GUI.Controls.DBObjectEditor();
          this.tpAlbum = new System.Windows.Forms.TabPage();
          this.scAlbumDetails = new System.Windows.Forms.SplitContainer();
          this.albumDetailsList = new Cornerstone.GUI.Controls.DBObjectEditor();
          this.tpTrack = new System.Windows.Forms.TabPage();
          this.scTrackDetails = new System.Windows.Forms.SplitContainer();
          this.fileDetailsList = new Cornerstone.GUI.Controls.DBObjectEditor();
          this.btnShowFileDetails = new System.Windows.Forms.Button();
          this.trackDetailsList = new Cornerstone.GUI.Controls.DBObjectEditor();
          this.tpImport = new System.Windows.Forms.TabPage();
          this.tcImport = new System.Windows.Forms.TabControl();
          this.tpImportPathParser = new System.Windows.Forms.TabPage();
          this.dgvParser = new System.Windows.Forms.DataGridView();
          this.ColParseFileName = new System.Windows.Forms.DataGridViewTextBoxColumn();
          this.colParseArtist = new System.Windows.Forms.DataGridViewTextBoxColumn();
          this.colParseAlbum = new System.Windows.Forms.DataGridViewTextBoxColumn();
          this.colParseTrack = new System.Windows.Forms.DataGridViewTextBoxColumn();
          this.colParseExt = new System.Windows.Forms.DataGridViewTextBoxColumn();
          this.colParsePath = new System.Windows.Forms.DataGridViewTextBoxColumn();
          this.colParseVolumeLabel = new System.Windows.Forms.DataGridViewTextBoxColumn();
          this.pathsGroupBox = new System.Windows.Forms.GroupBox();
          this.toolStrip = new System.Windows.Forms.ToolStrip();
          this.addSourceButton = new System.Windows.Forms.ToolStripSplitButton();
          this.manuallyEnterMediaSourceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
          this.removeSourceButton = new System.Windows.Forms.ToolStripSplitButton();
          this.markAsReplacedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
          this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
          this.helpButton = new System.Windows.Forms.ToolStripButton();
          this.label68 = new System.Windows.Forms.Label();
          this.btnTestReparse = new System.Windows.Forms.Button();
          this.importDvdCheckBox = new Cornerstone.GUI.Controls.SettingCheckBox();
          this.groupBox2 = new System.Windows.Forms.GroupBox();
          this.label21 = new System.Windows.Forms.Label();
          this.pathsGridView = new System.Windows.Forms.DataGridView();
          this.pathColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
          this.tpMatch = new System.Windows.Forms.TabPage();
          this.splitContainer4 = new System.Windows.Forms.SplitContainer();
          this.toolStrip1 = new System.Windows.Forms.ToolStrip();
          this.filterSplitButton = new System.Windows.Forms.ToolStripSplitButton();
          this.allMatchesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
          this.processingMatchesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
          this.unapprovedMatchesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
          this.approvedCommitedMatchesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
          this.approveButton = new System.Windows.Forms.ToolStripButton();
          this.manualAssignButton = new System.Windows.Forms.ToolStripButton();
          this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
          this.rescanButton = new System.Windows.Forms.ToolStripButton();
          this.splitJoinButton = new System.Windows.Forms.ToolStripButton();
          this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
          this.ignoreButton = new System.Windows.Forms.ToolStripButton();
          this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
          this.settingsButton = new System.Windows.Forms.ToolStripSplitButton();
          this.unignoreAllFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
          this.restartImporterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
          this.automaticMediaInfoMenuItem = new System.Windows.Forms.ToolStripMenuItem();
          this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
          this.unapprovedGrid = new System.Windows.Forms.DataGridView();
          this.statusColumn = new System.Windows.Forms.DataGridViewImageColumn();
          this.unapprovedLocalMediaColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
          this.unapprovedPossibleMatchesColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
          this.tpStringReplacements = new System.Windows.Forms.TabPage();
          this.splitContainer2 = new System.Windows.Forms.SplitContainer();
          this.dgvReplace = new System.Windows.Forms.DataGridView();
          this.btnReplUp = new System.Windows.Forms.Button();
          this.btnReplDown = new System.Windows.Forms.Button();
          this.linkLabelResetStringReplacements = new System.Windows.Forms.LinkLabel();
          this.linkLabelImportStringReplacements = new System.Windows.Forms.LinkLabel();
          this.linkLabelExportStringReplacements = new System.Windows.Forms.LinkLabel();
          this.label69 = new System.Windows.Forms.Label();
          this.tpParsingExpressions = new System.Windows.Forms.TabPage();
          this.splitContainer1 = new System.Windows.Forms.SplitContainer();
          this.dgvExpressions = new System.Windows.Forms.DataGridView();
          this.linkExParsingExpressions = new System.Windows.Forms.LinkLabel();
          this.linkImpParsingExpressions = new System.Windows.Forms.LinkLabel();
          this.linkExpressionHelp = new System.Windows.Forms.LinkLabel();
          this.buildExpr = new System.Windows.Forms.LinkLabel();
          this.resetExpr = new System.Windows.Forms.LinkLabel();
          this.btnExpUp = new System.Windows.Forms.Button();
          this.btnExpDown = new System.Windows.Forms.Button();
          this.label70 = new System.Windows.Forms.Label();
          this.tbSettings = new System.Windows.Forms.TabPage();
          this.label29 = new System.Windows.Forms.Label();
          this.label28 = new System.Windows.Forms.Label();
          this.label27 = new System.Windows.Forms.Label();
          this.label26 = new System.Windows.Forms.Label();
          this.label2 = new System.Windows.Forms.Label();
          this.groupBox7 = new System.Windows.Forms.GroupBox();
          this.label1 = new System.Windows.Forms.Label();
          this.tbLatestVideos = new Cornerstone.GUI.Controls.SettingsTextBox();
          this.label14 = new System.Windows.Forms.Label();
          this.label6 = new System.Windows.Forms.Label();
          this.tbMinArtWidth = new Cornerstone.GUI.Controls.SettingsTextBox();
          this.groupBox4 = new System.Windows.Forms.GroupBox();
          this.groupBox6 = new System.Windows.Forms.GroupBox();
          this.label4 = new System.Windows.Forms.Label();
          this.tbMinArtHeight = new Cornerstone.GUI.Controls.SettingsTextBox();
          this.tbHomeScreen = new Cornerstone.GUI.Controls.SettingsTextBox();
          this.label15 = new System.Windows.Forms.Label();
          this.label5 = new System.Windows.Forms.Label();
          this.groupBox5 = new System.Windows.Forms.GroupBox();
          this.label8 = new System.Windows.Forms.Label();
          this.label3 = new System.Windows.Forms.Label();
          this.cbSplitDVD = new Cornerstone.GUI.Controls.SettingCheckBox();
          this.label16 = new System.Windows.Forms.Label();
          this.cbAutoApprove = new Cornerstone.GUI.Controls.SettingCheckBox();
          this.label12 = new System.Windows.Forms.Label();
          this.cbUseMDAlbum = new Cornerstone.GUI.Controls.SettingCheckBox();
          this.label13 = new System.Windows.Forms.Label();
          this.tbTrackArtWidth = new Cornerstone.GUI.Controls.SettingsTextBox();
          this.tbTrackArtHeight = new Cornerstone.GUI.Controls.SettingsTextBox();
          this.tpAbout = new System.Windows.Forms.TabPage();
          this.pictureBox3 = new System.Windows.Forms.PictureBox();
          this.groupBox1 = new System.Windows.Forms.GroupBox();
          this.label24 = new System.Windows.Forms.Label();
          this.label23 = new System.Windows.Forms.Label();
          this.label17 = new System.Windows.Forms.Label();
          this.label22 = new System.Windows.Forms.Label();
          this.label7 = new System.Windows.Forms.Label();
          this.label18 = new System.Windows.Forms.Label();
          this.label19 = new System.Windows.Forms.Label();
          this.label20 = new System.Windows.Forms.Label();
          this.labelForum = new System.Windows.Forms.LinkLabel();
          this.labelManual = new System.Windows.Forms.LinkLabel();
          this.labelGoogleCode = new System.Windows.Forms.LinkLabel();
          this.label36 = new System.Windows.Forms.Label();
          this.groupBox3 = new System.Windows.Forms.GroupBox();
          this.label25 = new System.Windows.Forms.Label();
          this.lblProductVersion = new System.Windows.Forms.Label();
          this.gbProgress = new System.Windows.Forms.GroupBox();
          this.progressBar = new System.Windows.Forms.ProgressBar();
          this.currentTaskDesc = new System.Windows.Forms.Label();
          this.countProgressLabel = new System.Windows.Forms.Label();
          this.rtbLog = new System.Windows.Forms.RichTextBox();
          this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
          this.btnPlay = new System.Windows.Forms.ToolStripButton();
          this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
          this.btnArtRefresh = new System.Windows.Forms.ToolStripButton();
          this.btnArtloadNew = new System.Windows.Forms.ToolStripSplitButton();
          this.loadArtFromFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
          this.loadArtFromURLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
          this.loadArtFromMusicVideoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
          this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
          this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
          this.ArtFileDialog = new System.Windows.Forms.OpenFileDialog();
          this.imageListDrag = new System.Windows.Forms.ImageList(this.components);
          this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
          this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
          this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
          this.tbVideoPreviewRows = new Cornerstone.GUI.Controls.SettingsTextBox();
          this.tbVideoPreviewCols = new Cornerstone.GUI.Controls.SettingsTextBox();
          this.fileNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
          this.artistDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
          this.albumDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
          this.trackDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
          this.extDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
          this.volumeLabelDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
          this.pathDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
          this.ParserBindingSource = new System.Windows.Forms.BindingSource(this.components);
          this.autoDataSourcesPanel1 = new mvCentral.AutoDataSourcesPanel();
          this.unapprovedMatchesBindingSource = new System.Windows.Forms.BindingSource(this.components);
          this.cbPreferThumbnail = new Cornerstone.GUI.Controls.SettingCheckBox();
          this.label30 = new System.Windows.Forms.Label();
          ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
          this.scMain.Panel1.SuspendLayout();
          this.scMain.Panel2.SuspendLayout();
          this.scMain.SuspendLayout();
          this.mainTab.SuspendLayout();
          this.tpLibrary.SuspendLayout();
          this.splitContainer3.Panel1.SuspendLayout();
          this.splitContainer3.Panel2.SuspendLayout();
          this.splitContainer3.SuspendLayout();
          this.cmLibrary.SuspendLayout();
          ((System.ComponentModel.ISupportInitialize)(this.artImage)).BeginInit();
          this.coverToolStrip.SuspendLayout();
          this.tcMusicVideo.SuspendLayout();
          this.tpArtist.SuspendLayout();
          this.scArtistList.Panel2.SuspendLayout();
          this.scArtistList.SuspendLayout();
          this.tpAlbum.SuspendLayout();
          this.scAlbumDetails.Panel2.SuspendLayout();
          this.scAlbumDetails.SuspendLayout();
          this.tpTrack.SuspendLayout();
          this.scTrackDetails.Panel1.SuspendLayout();
          this.scTrackDetails.Panel2.SuspendLayout();
          this.scTrackDetails.SuspendLayout();
          this.tpImport.SuspendLayout();
          this.tcImport.SuspendLayout();
          this.tpImportPathParser.SuspendLayout();
          ((System.ComponentModel.ISupportInitialize)(this.dgvParser)).BeginInit();
          this.pathsGroupBox.SuspendLayout();
          this.toolStrip.SuspendLayout();
          ((System.ComponentModel.ISupportInitialize)(this.pathsGridView)).BeginInit();
          this.tpMatch.SuspendLayout();
          this.splitContainer4.Panel1.SuspendLayout();
          this.splitContainer4.Panel2.SuspendLayout();
          this.splitContainer4.SuspendLayout();
          this.toolStrip1.SuspendLayout();
          ((System.ComponentModel.ISupportInitialize)(this.unapprovedGrid)).BeginInit();
          this.tpStringReplacements.SuspendLayout();
          this.splitContainer2.Panel1.SuspendLayout();
          this.splitContainer2.Panel2.SuspendLayout();
          this.splitContainer2.SuspendLayout();
          ((System.ComponentModel.ISupportInitialize)(this.dgvReplace)).BeginInit();
          this.tpParsingExpressions.SuspendLayout();
          this.splitContainer1.Panel1.SuspendLayout();
          this.splitContainer1.Panel2.SuspendLayout();
          this.splitContainer1.SuspendLayout();
          ((System.ComponentModel.ISupportInitialize)(this.dgvExpressions)).BeginInit();
          this.tbSettings.SuspendLayout();
          this.tpAbout.SuspendLayout();
          ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
          this.groupBox1.SuspendLayout();
          this.gbProgress.SuspendLayout();
          ((System.ComponentModel.ISupportInitialize)(this.ParserBindingSource)).BeginInit();
          ((System.ComponentModel.ISupportInitialize)(this.unapprovedMatchesBindingSource)).BeginInit();
          this.SuspendLayout();
          // 
          // imageList
          // 
          this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
          this.imageList.TransparentColor = System.Drawing.Color.Transparent;
          this.imageList.Images.SetKeyName(0, "Add.png");
          this.imageList.Images.SetKeyName(1, "ArrowDown.png");
          this.imageList.Images.SetKeyName(2, "ArrowUp.png");
          this.imageList.Images.SetKeyName(3, "Cancel.png");
          this.imageList.Images.SetKeyName(4, "CogAdd.png");
          this.imageList.Images.SetKeyName(5, "Delete.png");
          this.imageList.Images.SetKeyName(6, "Edit.png");
          this.imageList.Images.SetKeyName(7, "FolderPageWhite.png");
          this.imageList.Images.SetKeyName(8, "Folders.png");
          this.imageList.Images.SetKeyName(9, "Help.png");
          this.imageList.Images.SetKeyName(10, "Information.png");
          this.imageList.Images.SetKeyName(11, "Languages.png");
          this.imageList.Images.SetKeyName(12, "OK.png");
          this.imageList.Images.SetKeyName(13, "PageWhiteText.png");
          this.imageList.Images.SetKeyName(14, "folder_image.png");
          this.imageList.Images.SetKeyName(15, "page_gear.png");
          this.imageList.Images.SetKeyName(16, "pencil_add.png");
          // 
          // textBox6
          // 
          this.textBox6.Location = new System.Drawing.Point(416, 16);
          this.textBox6.Name = "textBox6";
          this.textBox6.Size = new System.Drawing.Size(197, 20);
          this.textBox6.TabIndex = 7;
          // 
          // label9
          // 
          this.label9.AutoSize = true;
          this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.label9.Location = new System.Drawing.Point(370, 41);
          this.label9.Name = "label9";
          this.label9.Size = new System.Drawing.Size(67, 13);
          this.label9.TabIndex = 6;
          this.label9.Text = "Biography:";
          // 
          // label10
          // 
          this.label10.AutoSize = true;
          this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.label10.Location = new System.Drawing.Point(370, 19);
          this.label10.Name = "label10";
          this.label10.Size = new System.Drawing.Size(40, 13);
          this.label10.TabIndex = 5;
          this.label10.Text = "Artist:";
          // 
          // label11
          // 
          this.label11.AutoSize = true;
          this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.label11.Location = new System.Drawing.Point(10, 3);
          this.label11.Name = "label11";
          this.label11.Size = new System.Drawing.Size(51, 16);
          this.label11.TabIndex = 3;
          this.label11.Text = "Artists";
          // 
          // listBox2
          // 
          this.listBox2.FormattingEnabled = true;
          this.listBox2.Location = new System.Drawing.Point(10, 19);
          this.listBox2.Name = "listBox2";
          this.listBox2.Size = new System.Drawing.Size(165, 524);
          this.listBox2.TabIndex = 2;
          // 
          // button6
          // 
          this.button6.Location = new System.Drawing.Point(535, 545);
          this.button6.Name = "button6";
          this.button6.Size = new System.Drawing.Size(75, 23);
          this.button6.TabIndex = 1;
          this.button6.Text = "Close";
          this.button6.UseVisualStyleBackColor = true;
          // 
          // button7
          // 
          this.button7.Location = new System.Drawing.Point(6, 546);
          this.button7.Name = "button7";
          this.button7.Size = new System.Drawing.Size(170, 23);
          this.button7.TabIndex = 0;
          this.button7.Text = "Rescan watch folders";
          this.button7.UseVisualStyleBackColor = true;
          // 
          // getDT
          // 
          this.getDT.FileName = "openFileDialog1";
          // 
          // pictureBox4
          // 
          this.pictureBox4.Location = new System.Drawing.Point(181, 19);
          this.pictureBox4.Name = "pictureBox4";
          this.pictureBox4.Size = new System.Drawing.Size(183, 167);
          this.pictureBox4.TabIndex = 4;
          this.pictureBox4.TabStop = false;
          // 
          // scMain
          // 
          this.scMain.Dock = System.Windows.Forms.DockStyle.Fill;
          this.scMain.IsSplitterFixed = true;
          this.scMain.Location = new System.Drawing.Point(0, 0);
          this.scMain.Name = "scMain";
          this.scMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
          // 
          // scMain.Panel1
          // 
          this.scMain.Panel1.BackColor = System.Drawing.Color.White;
          this.scMain.Panel1.Controls.Add(this.mainTab);
          this.scMain.Panel1.Controls.Add(this.gbProgress);
          // 
          // scMain.Panel2
          // 
          this.scMain.Panel2.Controls.Add(this.rtbLog);
          this.scMain.Panel2Collapsed = true;
          this.scMain.Size = new System.Drawing.Size(619, 603);
          this.scMain.SplitterDistance = 300;
          this.scMain.SplitterWidth = 1;
          this.scMain.TabIndex = 94;
          // 
          // mainTab
          // 
          this.mainTab.Controls.Add(this.tpLibrary);
          this.mainTab.Controls.Add(this.tpImport);
          this.mainTab.Controls.Add(this.tbSettings);
          this.mainTab.Controls.Add(this.tpAbout);
          this.mainTab.Dock = System.Windows.Forms.DockStyle.Fill;
          this.mainTab.ImageList = this.imageList;
          this.mainTab.Location = new System.Drawing.Point(0, 0);
          this.mainTab.Name = "mainTab";
          this.mainTab.SelectedIndex = 0;
          this.mainTab.Size = new System.Drawing.Size(619, 555);
          this.mainTab.TabIndex = 1;
          // 
          // tpLibrary
          // 
          this.tpLibrary.Controls.Add(this.splitContainer3);
          this.tpLibrary.Location = new System.Drawing.Point(4, 23);
          this.tpLibrary.Name = "tpLibrary";
          this.tpLibrary.Padding = new System.Windows.Forms.Padding(3);
          this.tpLibrary.Size = new System.Drawing.Size(611, 528);
          this.tpLibrary.TabIndex = 6;
          this.tpLibrary.Text = "Library";
          this.tpLibrary.UseVisualStyleBackColor = true;
          // 
          // splitContainer3
          // 
          this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
          this.splitContainer3.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
          this.splitContainer3.Location = new System.Drawing.Point(3, 3);
          this.splitContainer3.Name = "splitContainer3";
          // 
          // splitContainer3.Panel1
          // 
          this.splitContainer3.Panel1.Controls.Add(this.mvLibraryTreeView);
          // 
          // splitContainer3.Panel2
          // 
          this.splitContainer3.Panel2.BackColor = System.Drawing.SystemColors.Control;
          this.splitContainer3.Panel2.Controls.Add(this.lblArtResolution);
          this.splitContainer3.Panel2.Controls.Add(this.lblArtNum);
          this.splitContainer3.Panel2.Controls.Add(this.artworkProgressBar);
          this.splitContainer3.Panel2.Controls.Add(this.artImage);
          this.splitContainer3.Panel2.Controls.Add(this.coverToolStrip);
          this.splitContainer3.Panel2.Controls.Add(this.tcMusicVideo);
          this.splitContainer3.Size = new System.Drawing.Size(605, 522);
          this.splitContainer3.SplitterDistance = 201;
          this.splitContainer3.TabIndex = 0;
          // 
          // mvLibraryTreeView
          // 
          this.mvLibraryTreeView.AllowDrop = true;
          this.mvLibraryTreeView.ContextMenuStrip = this.cmLibrary;
          this.mvLibraryTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
          this.mvLibraryTreeView.ImageIndex = 0;
          this.mvLibraryTreeView.ImageList = this.imageListTreeView;
          this.mvLibraryTreeView.Location = new System.Drawing.Point(0, 0);
          this.mvLibraryTreeView.Name = "mvLibraryTreeView";
          this.mvLibraryTreeView.SelectedImageIndex = 0;
          this.mvLibraryTreeView.Size = new System.Drawing.Size(201, 522);
          this.mvLibraryTreeView.TabIndex = 1;
          this.mvLibraryTreeView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.tvLibrary_ItemDrag);
          this.mvLibraryTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvLibrary_AfterSelect);
          this.mvLibraryTreeView.DragDrop += new System.Windows.Forms.DragEventHandler(this.tvLibrary_DragDrop);
          this.mvLibraryTreeView.DragEnter += new System.Windows.Forms.DragEventHandler(this.tvLibrary_DragEnter);
          this.mvLibraryTreeView.DragOver += new System.Windows.Forms.DragEventHandler(this.tvLibrary_DragOver);
          this.mvLibraryTreeView.DragLeave += new System.EventHandler(this.tvLibrary_DragLeave);
          this.mvLibraryTreeView.GiveFeedback += new System.Windows.Forms.GiveFeedbackEventHandler(this.tvLibrary_GiveFeedback);
          // 
          // cmLibrary
          // 
          this.cmLibrary.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sentToImporterToolStripMenuItem,
            this.getArtToolStripMenuItem,
            this.autoGrabFrame30SecsToolStripMenuItem,
            this.tsmGrabFrame,
            this.tsmRemove,
            this.tsmGetInfo});
          this.cmLibrary.Name = "cmLibrary";
          this.cmLibrary.Size = new System.Drawing.Size(214, 136);
          this.cmLibrary.Opened += new System.EventHandler(this.cmLibrary_Opened);
          // 
          // sentToImporterToolStripMenuItem
          // 
          this.sentToImporterToolStripMenuItem.Name = "sentToImporterToolStripMenuItem";
          this.sentToImporterToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
          this.sentToImporterToolStripMenuItem.Text = "Sent To Importer";
          this.sentToImporterToolStripMenuItem.Click += new System.EventHandler(this.sentToImporterToolStripMenuItem_Click);
          // 
          // getArtToolStripMenuItem
          // 
          this.getArtToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fromFileToolStripMenuItem,
            this.fromURLToolStripMenuItem,
            this.fromOnlinjeToolStripMenuItem,
            this.asVideoThumnailToolStripMenuItem});
          this.getArtToolStripMenuItem.Name = "getArtToolStripMenuItem";
          this.getArtToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
          this.getArtToolStripMenuItem.Text = "Get Art";
          // 
          // fromFileToolStripMenuItem
          // 
          this.fromFileToolStripMenuItem.Name = "fromFileToolStripMenuItem";
          this.fromFileToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
          this.fromFileToolStripMenuItem.Text = "From File";
          this.fromFileToolStripMenuItem.Click += new System.EventHandler(this.loadArtFromFileToolStripMenuItem_Click);
          // 
          // fromURLToolStripMenuItem
          // 
          this.fromURLToolStripMenuItem.Name = "fromURLToolStripMenuItem";
          this.fromURLToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
          this.fromURLToolStripMenuItem.Text = "From URL";
          this.fromURLToolStripMenuItem.Click += new System.EventHandler(this.loadArtFromURLToolStripMenuItem_Click);
          // 
          // fromOnlinjeToolStripMenuItem
          // 
          this.fromOnlinjeToolStripMenuItem.Name = "fromOnlinjeToolStripMenuItem";
          this.fromOnlinjeToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
          this.fromOnlinjeToolStripMenuItem.Text = "From Online";
          this.fromOnlinjeToolStripMenuItem.Click += new System.EventHandler(this.btnArtRefresh_Click);
          // 
          // asVideoThumnailToolStripMenuItem
          // 
          this.asVideoThumnailToolStripMenuItem.Name = "asVideoThumnailToolStripMenuItem";
          this.asVideoThumnailToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
          this.asVideoThumnailToolStripMenuItem.Text = "As Video Thumnail";
          this.asVideoThumnailToolStripMenuItem.Click += new System.EventHandler(this.asVideoThumnailToolStripMenuItem_Click);
          // 
          // autoGrabFrame30SecsToolStripMenuItem
          // 
          this.autoGrabFrame30SecsToolStripMenuItem.Name = "autoGrabFrame30SecsToolStripMenuItem";
          this.autoGrabFrame30SecsToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
          this.autoGrabFrame30SecsToolStripMenuItem.Text = "Auto Grab Frame (30 Secs)";
          this.autoGrabFrame30SecsToolStripMenuItem.Click += new System.EventHandler(this.autoGrabFrame30SecsToolStripMenuItem_Click);
          // 
          // tsmGrabFrame
          // 
          this.tsmGrabFrame.Name = "tsmGrabFrame";
          this.tsmGrabFrame.Size = new System.Drawing.Size(213, 22);
          this.tsmGrabFrame.Text = "Play / Grab Frame";
          this.tsmGrabFrame.Click += new System.EventHandler(this.btnPlay_Click);
          // 
          // tsmRemove
          // 
          this.tsmRemove.Name = "tsmRemove";
          this.tsmRemove.Size = new System.Drawing.Size(213, 22);
          this.tsmRemove.Text = "Remove";
          this.tsmRemove.Click += new System.EventHandler(this.tsmRemove_Click);
          // 
          // tsmGetInfo
          // 
          this.tsmGetInfo.Name = "tsmGetInfo";
          this.tsmGetInfo.Size = new System.Drawing.Size(213, 22);
          this.tsmGetInfo.Text = "Get Info";
          this.tsmGetInfo.Click += new System.EventHandler(this.tsmGetInfo_Click);
          // 
          // imageListTreeView
          // 
          this.imageListTreeView.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListTreeView.ImageStream")));
          this.imageListTreeView.TransparentColor = System.Drawing.Color.Transparent;
          this.imageListTreeView.Images.SetKeyName(0, "");
          this.imageListTreeView.Images.SetKeyName(1, "album.png");
          this.imageListTreeView.Images.SetKeyName(2, "artist.png");
          this.imageListTreeView.Images.SetKeyName(3, "track.png");
          // 
          // lblArtResolution
          // 
          this.lblArtResolution.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
          this.lblArtResolution.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.lblArtResolution.Location = new System.Drawing.Point(134, 15);
          this.lblArtResolution.Name = "lblArtResolution";
          this.lblArtResolution.Size = new System.Drawing.Size(87, 11);
          this.lblArtResolution.TabIndex = 17;
          this.lblArtResolution.Text = "419 x 600";
          this.lblArtResolution.TextAlign = System.Drawing.ContentAlignment.TopRight;
          // 
          // lblArtNum
          // 
          this.lblArtNum.AutoSize = true;
          this.lblArtNum.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.lblArtNum.Location = new System.Drawing.Point(199, 3);
          this.lblArtNum.Name = "lblArtNum";
          this.lblArtNum.Size = new System.Drawing.Size(22, 12);
          this.lblArtNum.TabIndex = 16;
          this.lblArtNum.Text = "1 / 3";
          // 
          // artworkProgressBar
          // 
          this.artworkProgressBar.Location = new System.Drawing.Point(19, 130);
          this.artworkProgressBar.Name = "artworkProgressBar";
          this.artworkProgressBar.Size = new System.Drawing.Size(202, 18);
          this.artworkProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
          this.artworkProgressBar.TabIndex = 18;
          this.artworkProgressBar.Visible = false;
          // 
          // artImage
          // 
          this.artImage.BackColor = System.Drawing.SystemColors.ControlDark;
          this.artImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
          this.artImage.Location = new System.Drawing.Point(222, 3);
          this.artImage.MaximumSize = new System.Drawing.Size(175, 260);
          this.artImage.Name = "artImage";
          this.artImage.Size = new System.Drawing.Size(175, 151);
          this.artImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
          this.artImage.TabIndex = 15;
          this.artImage.TabStop = false;
          this.artImage.DoubleClick += new System.EventHandler(this.btnArtZoom_Click);
          // 
          // coverToolStrip
          // 
          this.coverToolStrip.Dock = System.Windows.Forms.DockStyle.None;
          this.coverToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
          this.coverToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnPrevArt,
            this.btnNextArt,
            this.btnArtDelete,
            this.btnArtZoom});
          this.coverToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
          this.coverToolStrip.Location = new System.Drawing.Point(189, 31);
          this.coverToolStrip.Name = "coverToolStrip";
          this.coverToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
          this.coverToolStrip.Size = new System.Drawing.Size(24, 94);
          this.coverToolStrip.TabIndex = 14;
          // 
          // btnPrevArt
          // 
          this.btnPrevArt.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
          this.btnPrevArt.Image = global::mvCentral.Properties.Resources.resultset_previous;
          this.btnPrevArt.ImageTransparentColor = System.Drawing.Color.Magenta;
          this.btnPrevArt.Name = "btnPrevArt";
          this.btnPrevArt.Size = new System.Drawing.Size(22, 20);
          this.btnPrevArt.Text = "toolStripButton2";
          this.btnPrevArt.ToolTipText = "Previous Art";
          this.btnPrevArt.Click += new System.EventHandler(this.btnArtPrevNext_Click);
          // 
          // btnNextArt
          // 
          this.btnNextArt.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
          this.btnNextArt.Image = global::mvCentral.Properties.Resources.resultset_next;
          this.btnNextArt.ImageTransparentColor = System.Drawing.Color.Magenta;
          this.btnNextArt.Name = "btnNextArt";
          this.btnNextArt.Size = new System.Drawing.Size(22, 20);
          this.btnNextArt.Text = "toolStripButton3";
          this.btnNextArt.ToolTipText = "Next Art";
          this.btnNextArt.Click += new System.EventHandler(this.btnArtPrevNext_Click);
          // 
          // btnArtDelete
          // 
          this.btnArtDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
          this.btnArtDelete.Image = global::mvCentral.Properties.Resources.cross;
          this.btnArtDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
          this.btnArtDelete.Name = "btnArtDelete";
          this.btnArtDelete.Size = new System.Drawing.Size(22, 20);
          this.btnArtDelete.Text = "toolStripButton2";
          this.btnArtDelete.ToolTipText = "Delete Art";
          this.btnArtDelete.Click += new System.EventHandler(this.btnArtDelete_Click);
          // 
          // btnArtZoom
          // 
          this.btnArtZoom.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
          this.btnArtZoom.Image = global::mvCentral.Properties.Resources.zoom;
          this.btnArtZoom.ImageTransparentColor = System.Drawing.Color.Magenta;
          this.btnArtZoom.Name = "btnArtZoom";
          this.btnArtZoom.Size = new System.Drawing.Size(22, 20);
          this.btnArtZoom.Text = "toolStripButton2";
          this.btnArtZoom.ToolTipText = "Zoom Art";
          this.btnArtZoom.Click += new System.EventHandler(this.btnArtZoom_Click);
          // 
          // tcMusicVideo
          // 
          this.tcMusicVideo.Controls.Add(this.tpArtist);
          this.tcMusicVideo.Controls.Add(this.tpAlbum);
          this.tcMusicVideo.Controls.Add(this.tpTrack);
          this.tcMusicVideo.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
          this.tcMusicVideo.ItemSize = new System.Drawing.Size(42, 1);
          this.tcMusicVideo.Location = new System.Drawing.Point(12, 149);
          this.tcMusicVideo.Name = "tcMusicVideo";
          this.tcMusicVideo.SelectedIndex = 0;
          this.tcMusicVideo.Size = new System.Drawing.Size(395, 355);
          this.tcMusicVideo.TabIndex = 13;
          // 
          // tpArtist
          // 
          this.tpArtist.Controls.Add(this.scArtistList);
          this.tpArtist.Location = new System.Drawing.Point(4, 5);
          this.tpArtist.Name = "tpArtist";
          this.tpArtist.Padding = new System.Windows.Forms.Padding(3);
          this.tpArtist.Size = new System.Drawing.Size(387, 346);
          this.tpArtist.TabIndex = 0;
          this.tpArtist.Text = "Artist";
          this.tpArtist.UseVisualStyleBackColor = true;
          // 
          // scArtistList
          // 
          this.scArtistList.Dock = System.Windows.Forms.DockStyle.Fill;
          this.scArtistList.Location = new System.Drawing.Point(3, 3);
          this.scArtistList.Name = "scArtistList";
          this.scArtistList.Orientation = System.Windows.Forms.Orientation.Horizontal;
          this.scArtistList.Panel1Collapsed = true;
          // 
          // scArtistList.Panel2
          // 
          this.scArtistList.Panel2.Controls.Add(this.artistDetailsList);
          this.scArtistList.Size = new System.Drawing.Size(381, 340);
          this.scArtistList.SplitterDistance = 25;
          this.scArtistList.TabIndex = 14;
          // 
          // artistDetailsList
          // 
          this.artistDetailsList.DatabaseObject = null;
          this.artistDetailsList.Dock = System.Windows.Forms.DockStyle.Fill;
          this.artistDetailsList.ForeColor = System.Drawing.SystemColors.ControlText;
          this.artistDetailsList.Location = new System.Drawing.Point(0, 0);
          this.artistDetailsList.Name = "artistDetailsList";
          this.artistDetailsList.Size = new System.Drawing.Size(381, 340);
          this.artistDetailsList.TabIndex = 14;
          // 
          // tpAlbum
          // 
          this.tpAlbum.Controls.Add(this.scAlbumDetails);
          this.tpAlbum.Location = new System.Drawing.Point(4, 5);
          this.tpAlbum.Name = "tpAlbum";
          this.tpAlbum.Padding = new System.Windows.Forms.Padding(3);
          this.tpAlbum.Size = new System.Drawing.Size(387, 346);
          this.tpAlbum.TabIndex = 1;
          this.tpAlbum.Text = "Album";
          this.tpAlbum.UseVisualStyleBackColor = true;
          // 
          // scAlbumDetails
          // 
          this.scAlbumDetails.Dock = System.Windows.Forms.DockStyle.Fill;
          this.scAlbumDetails.Location = new System.Drawing.Point(3, 3);
          this.scAlbumDetails.Name = "scAlbumDetails";
          this.scAlbumDetails.Orientation = System.Windows.Forms.Orientation.Horizontal;
          this.scAlbumDetails.Panel1Collapsed = true;
          // 
          // scAlbumDetails.Panel2
          // 
          this.scAlbumDetails.Panel2.Controls.Add(this.albumDetailsList);
          this.scAlbumDetails.Size = new System.Drawing.Size(381, 340);
          this.scAlbumDetails.SplitterDistance = 32;
          this.scAlbumDetails.TabIndex = 0;
          // 
          // albumDetailsList
          // 
          this.albumDetailsList.DatabaseObject = null;
          this.albumDetailsList.Dock = System.Windows.Forms.DockStyle.Fill;
          this.albumDetailsList.ForeColor = System.Drawing.SystemColors.ControlText;
          this.albumDetailsList.Location = new System.Drawing.Point(0, 0);
          this.albumDetailsList.Name = "albumDetailsList";
          this.albumDetailsList.Size = new System.Drawing.Size(381, 340);
          this.albumDetailsList.TabIndex = 15;
          // 
          // tpTrack
          // 
          this.tpTrack.Controls.Add(this.scTrackDetails);
          this.tpTrack.Location = new System.Drawing.Point(4, 5);
          this.tpTrack.Name = "tpTrack";
          this.tpTrack.Padding = new System.Windows.Forms.Padding(3);
          this.tpTrack.Size = new System.Drawing.Size(387, 346);
          this.tpTrack.TabIndex = 2;
          this.tpTrack.Text = "Track";
          this.tpTrack.UseVisualStyleBackColor = true;
          // 
          // scTrackDetails
          // 
          this.scTrackDetails.Dock = System.Windows.Forms.DockStyle.Fill;
          this.scTrackDetails.Location = new System.Drawing.Point(3, 3);
          this.scTrackDetails.Name = "scTrackDetails";
          this.scTrackDetails.Orientation = System.Windows.Forms.Orientation.Horizontal;
          // 
          // scTrackDetails.Panel1
          // 
          this.scTrackDetails.Panel1.Controls.Add(this.fileDetailsList);
          this.scTrackDetails.Panel1.Controls.Add(this.btnShowFileDetails);
          this.scTrackDetails.Panel1MinSize = 10;
          // 
          // scTrackDetails.Panel2
          // 
          this.scTrackDetails.Panel2.Controls.Add(this.trackDetailsList);
          this.scTrackDetails.Size = new System.Drawing.Size(381, 340);
          this.scTrackDetails.SplitterDistance = 10;
          this.scTrackDetails.TabIndex = 0;
          // 
          // fileDetailsList
          // 
          this.fileDetailsList.DatabaseObject = null;
          this.fileDetailsList.Dock = System.Windows.Forms.DockStyle.Fill;
          this.fileDetailsList.Location = new System.Drawing.Point(0, 0);
          this.fileDetailsList.Name = "fileDetailsList";
          this.fileDetailsList.Size = new System.Drawing.Size(381, 0);
          this.fileDetailsList.TabIndex = 98;
          // 
          // btnShowFileDetails
          // 
          this.btnShowFileDetails.Dock = System.Windows.Forms.DockStyle.Bottom;
          this.btnShowFileDetails.Image = global::mvCentral.Properties.Resources.arrow_up_small;
          this.btnShowFileDetails.Location = new System.Drawing.Point(0, -5);
          this.btnShowFileDetails.Name = "btnShowFileDetails";
          this.btnShowFileDetails.Size = new System.Drawing.Size(381, 15);
          this.btnShowFileDetails.TabIndex = 97;
          this.btnShowFileDetails.UseVisualStyleBackColor = true;
          this.btnShowFileDetails.Click += new System.EventHandler(this.btnShowFileDetails_Click);
          // 
          // trackDetailsList
          // 
          this.trackDetailsList.DatabaseObject = null;
          this.trackDetailsList.Dock = System.Windows.Forms.DockStyle.Fill;
          this.trackDetailsList.ForeColor = System.Drawing.SystemColors.ControlText;
          this.trackDetailsList.Location = new System.Drawing.Point(0, 0);
          this.trackDetailsList.Name = "trackDetailsList";
          this.trackDetailsList.Size = new System.Drawing.Size(381, 326);
          this.trackDetailsList.TabIndex = 16;
          // 
          // tpImport
          // 
          this.tpImport.Controls.Add(this.tcImport);
          this.tpImport.Location = new System.Drawing.Point(4, 23);
          this.tpImport.Name = "tpImport";
          this.tpImport.Padding = new System.Windows.Forms.Padding(3);
          this.tpImport.Size = new System.Drawing.Size(611, 528);
          this.tpImport.TabIndex = 5;
          this.tpImport.Text = "Import";
          this.tpImport.UseVisualStyleBackColor = true;
          // 
          // tcImport
          // 
          this.tcImport.Alignment = System.Windows.Forms.TabAlignment.Left;
          this.tcImport.Controls.Add(this.tpImportPathParser);
          this.tcImport.Controls.Add(this.tpMatch);
          this.tcImport.Controls.Add(this.tpStringReplacements);
          this.tcImport.Controls.Add(this.tpParsingExpressions);
          this.tcImport.Dock = System.Windows.Forms.DockStyle.Fill;
          this.tcImport.ImageList = this.imageList;
          this.tcImport.Location = new System.Drawing.Point(3, 3);
          this.tcImport.Multiline = true;
          this.tcImport.Name = "tcImport";
          this.tcImport.SelectedIndex = 0;
          this.tcImport.ShowToolTips = true;
          this.tcImport.Size = new System.Drawing.Size(605, 522);
          this.tcImport.TabIndex = 2;
          // 
          // tpImportPathParser
          // 
          this.tpImportPathParser.BackColor = System.Drawing.SystemColors.Control;
          this.tpImportPathParser.Controls.Add(this.dgvParser);
          this.tpImportPathParser.Controls.Add(this.pathsGroupBox);
          this.tpImportPathParser.ImageIndex = 14;
          this.tpImportPathParser.Location = new System.Drawing.Point(23, 4);
          this.tpImportPathParser.Name = "tpImportPathParser";
          this.tpImportPathParser.Padding = new System.Windows.Forms.Padding(3);
          this.tpImportPathParser.Size = new System.Drawing.Size(578, 514);
          this.tpImportPathParser.TabIndex = 0;
          this.tpImportPathParser.ToolTipText = "Import Paths / Parser";
          // 
          // dgvParser
          // 
          this.dgvParser.AllowUserToAddRows = false;
          this.dgvParser.AllowUserToDeleteRows = false;
          this.dgvParser.AllowUserToResizeRows = false;
          this.dgvParser.AutoGenerateColumns = false;
          this.dgvParser.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
          this.dgvParser.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColParseFileName,
            this.colParseArtist,
            this.colParseAlbum,
            this.colParseTrack,
            this.colParseExt,
            this.colParsePath,
            this.colParseVolumeLabel,
            this.fileNameDataGridViewTextBoxColumn,
            this.artistDataGridViewTextBoxColumn,
            this.albumDataGridViewTextBoxColumn,
            this.trackDataGridViewTextBoxColumn,
            this.extDataGridViewTextBoxColumn,
            this.volumeLabelDataGridViewTextBoxColumn,
            this.pathDataGridViewTextBoxColumn});
          this.dgvParser.DataSource = this.ParserBindingSource;
          this.dgvParser.Dock = System.Windows.Forms.DockStyle.Fill;
          this.dgvParser.EnableHeadersVisualStyles = false;
          this.dgvParser.Location = new System.Drawing.Point(3, 197);
          this.dgvParser.Name = "dgvParser";
          this.dgvParser.ReadOnly = true;
          this.dgvParser.RowHeadersVisible = false;
          this.dgvParser.Size = new System.Drawing.Size(572, 314);
          this.dgvParser.TabIndex = 5;
          // 
          // ColParseFileName
          // 
          this.ColParseFileName.DataPropertyName = "FileName";
          this.ColParseFileName.HeaderText = "FileName";
          this.ColParseFileName.Name = "ColParseFileName";
          this.ColParseFileName.ReadOnly = true;
          this.ColParseFileName.Resizable = System.Windows.Forms.DataGridViewTriState.True;
          // 
          // colParseArtist
          // 
          this.colParseArtist.DataPropertyName = "Artist";
          this.colParseArtist.HeaderText = "Artist";
          this.colParseArtist.Name = "colParseArtist";
          this.colParseArtist.ReadOnly = true;
          this.colParseArtist.Resizable = System.Windows.Forms.DataGridViewTriState.True;
          // 
          // colParseAlbum
          // 
          this.colParseAlbum.DataPropertyName = "Album";
          this.colParseAlbum.HeaderText = "Album";
          this.colParseAlbum.Name = "colParseAlbum";
          this.colParseAlbum.ReadOnly = true;
          // 
          // colParseTrack
          // 
          this.colParseTrack.DataPropertyName = "Track";
          this.colParseTrack.HeaderText = "Track";
          this.colParseTrack.Name = "colParseTrack";
          this.colParseTrack.ReadOnly = true;
          // 
          // colParseExt
          // 
          this.colParseExt.DataPropertyName = "Ext";
          this.colParseExt.HeaderText = "Ext";
          this.colParseExt.Name = "colParseExt";
          this.colParseExt.ReadOnly = true;
          // 
          // colParsePath
          // 
          this.colParsePath.DataPropertyName = "Path";
          this.colParsePath.HeaderText = "Path";
          this.colParsePath.Name = "colParsePath";
          this.colParsePath.ReadOnly = true;
          // 
          // colParseVolumeLabel
          // 
          this.colParseVolumeLabel.DataPropertyName = "VolumeLabel";
          this.colParseVolumeLabel.HeaderText = "VolumeLabel";
          this.colParseVolumeLabel.Name = "colParseVolumeLabel";
          this.colParseVolumeLabel.ReadOnly = true;
          // 
          // pathsGroupBox
          // 
          this.pathsGroupBox.BackColor = System.Drawing.SystemColors.Control;
          this.pathsGroupBox.Controls.Add(this.toolStrip);
          this.pathsGroupBox.Controls.Add(this.label68);
          this.pathsGroupBox.Controls.Add(this.btnTestReparse);
          this.pathsGroupBox.Controls.Add(this.importDvdCheckBox);
          this.pathsGroupBox.Controls.Add(this.groupBox2);
          this.pathsGroupBox.Controls.Add(this.label21);
          this.pathsGroupBox.Controls.Add(this.pathsGridView);
          this.pathsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
          this.pathsGroupBox.Location = new System.Drawing.Point(3, 3);
          this.pathsGroupBox.Name = "pathsGroupBox";
          this.pathsGroupBox.Size = new System.Drawing.Size(572, 194);
          this.pathsGroupBox.TabIndex = 3;
          this.pathsGroupBox.TabStop = false;
          this.pathsGroupBox.Text = "Media Sources";
          // 
          // toolStrip
          // 
          this.toolStrip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
          this.toolStrip.Dock = System.Windows.Forms.DockStyle.None;
          this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
          this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addSourceButton,
            this.removeSourceButton,
            this.toolStripSeparator8,
            this.helpButton});
          this.toolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
          this.toolStrip.Location = new System.Drawing.Point(539, 34);
          this.toolStrip.Name = "toolStrip";
          this.toolStrip.Size = new System.Drawing.Size(33, 77);
          this.toolStrip.TabIndex = 155;
          this.toolStrip.Text = "toolStrip1";
          // 
          // addSourceButton
          // 
          this.addSourceButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
          this.addSourceButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.manuallyEnterMediaSourceToolStripMenuItem});
          this.addSourceButton.Image = global::mvCentral.Properties.Resources.Add;
          this.addSourceButton.ImageTransparentColor = System.Drawing.Color.Magenta;
          this.addSourceButton.Name = "addSourceButton";
          this.addSourceButton.Size = new System.Drawing.Size(31, 20);
          this.addSourceButton.Text = "toolStripButton1";
          this.addSourceButton.ToolTipText = "Add Watch Folder";
          this.addSourceButton.ButtonClick += new System.EventHandler(this.addSourceButton_Click);
          // 
          // manuallyEnterMediaSourceToolStripMenuItem
          // 
          this.manuallyEnterMediaSourceToolStripMenuItem.Name = "manuallyEnterMediaSourceToolStripMenuItem";
          this.manuallyEnterMediaSourceToolStripMenuItem.Size = new System.Drawing.Size(228, 22);
          this.manuallyEnterMediaSourceToolStripMenuItem.Text = "Manually Enter Media Source";
          this.manuallyEnterMediaSourceToolStripMenuItem.Click += new System.EventHandler(this.manuallyEnterMediaSourceToolStripMenuItem_Click);
          // 
          // removeSourceButton
          // 
          this.removeSourceButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
          this.removeSourceButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.markAsReplacedToolStripMenuItem});
          this.removeSourceButton.Image = global::mvCentral.Properties.Resources.Delete;
          this.removeSourceButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
          this.removeSourceButton.ImageTransparentColor = System.Drawing.Color.Magenta;
          this.removeSourceButton.Name = "removeSourceButton";
          this.removeSourceButton.Size = new System.Drawing.Size(31, 20);
          this.removeSourceButton.Text = "toolStripButton2";
          this.removeSourceButton.ToolTipText = "Remove Watch Folder";
          this.removeSourceButton.ButtonClick += new System.EventHandler(this.removeSourceButton_ButtonClick);
          // 
          // markAsReplacedToolStripMenuItem
          // 
          this.markAsReplacedToolStripMenuItem.Name = "markAsReplacedToolStripMenuItem";
          this.markAsReplacedToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
          this.markAsReplacedToolStripMenuItem.Text = "Toggle Replaced";
          this.markAsReplacedToolStripMenuItem.Click += new System.EventHandler(this.markAsReplacedToolStripMenuItem_Click);
          // 
          // toolStripSeparator8
          // 
          this.toolStripSeparator8.Name = "toolStripSeparator8";
          this.toolStripSeparator8.Size = new System.Drawing.Size(31, 6);
          // 
          // helpButton
          // 
          this.helpButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
          this.helpButton.Image = global::mvCentral.Properties.Resources.Help;
          this.helpButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
          this.helpButton.ImageTransparentColor = System.Drawing.Color.Magenta;
          this.helpButton.Name = "helpButton";
          this.helpButton.Size = new System.Drawing.Size(31, 20);
          this.helpButton.Text = "toolStripButton3";
          this.helpButton.ToolTipText = "Help";
          this.helpButton.Click += new System.EventHandler(this.helpButton_Click);
          // 
          // label68
          // 
          this.label68.Location = new System.Drawing.Point(3, 148);
          this.label68.Name = "label68";
          this.label68.Size = new System.Drawing.Size(519, 45);
          this.label68.TabIndex = 154;
          this.label68.Text = resources.GetString("label68.Text");
          // 
          // btnTestReparse
          // 
          this.btnTestReparse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
          this.btnTestReparse.Image = ((System.Drawing.Image)(resources.GetObject("btnTestReparse.Image")));
          this.btnTestReparse.Location = new System.Drawing.Point(536, 148);
          this.btnTestReparse.Name = "btnTestReparse";
          this.btnTestReparse.Size = new System.Drawing.Size(28, 31);
          this.btnTestReparse.TabIndex = 153;
          this.btnTestReparse.UseVisualStyleBackColor = true;
          this.btnTestReparse.Click += new System.EventHandler(this.btnTestReparse_Click);
          // 
          // importDvdCheckBox
          // 
          this.importDvdCheckBox.AutoSize = true;
          this.importDvdCheckBox.IgnoreSettingName = false;
          this.importDvdCheckBox.Location = new System.Drawing.Point(6, 128);
          this.importDvdCheckBox.Name = "importDvdCheckBox";
          this.importDvdCheckBox.Setting = null;
          this.importDvdCheckBox.Size = new System.Drawing.Size(192, 17);
          this.importDvdCheckBox.TabIndex = 8;
          this.importDvdCheckBox.Text = "Automatically Import Inserted DVDs";
          this.importDvdCheckBox.UseVisualStyleBackColor = true;
          // 
          // groupBox2
          // 
          this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
          this.groupBox2.Location = new System.Drawing.Point(3, 115);
          this.groupBox2.Name = "groupBox2";
          this.groupBox2.Size = new System.Drawing.Size(562, 5);
          this.groupBox2.TabIndex = 7;
          this.groupBox2.TabStop = false;
          // 
          // label21
          // 
          this.label21.AutoSize = true;
          this.label21.Location = new System.Drawing.Point(6, 16);
          this.label21.Name = "label21";
          this.label21.Size = new System.Drawing.Size(267, 13);
          this.label21.TabIndex = 6;
          this.label21.Text = "Music Videos in the following folders will be processed :";
          // 
          // pathsGridView
          // 
          this.pathsGridView.AllowUserToAddRows = false;
          this.pathsGridView.AllowUserToDeleteRows = false;
          this.pathsGridView.AllowUserToResizeRows = false;
          this.pathsGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                      | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
          this.pathsGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
          this.pathsGridView.ColumnHeadersVisible = false;
          this.pathsGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.pathColumn});
          this.pathsGridView.Location = new System.Drawing.Point(6, 34);
          this.pathsGridView.MultiSelect = false;
          this.pathsGridView.Name = "pathsGridView";
          this.pathsGridView.RowHeadersVisible = false;
          this.pathsGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
          this.pathsGridView.Size = new System.Drawing.Size(530, 77);
          this.pathsGridView.TabIndex = 0;
          this.pathsGridView.RowPrePaint += new System.Windows.Forms.DataGridViewRowPrePaintEventHandler(this.pathsGridView_RowPrePaint);
          // 
          // pathColumn
          // 
          this.pathColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
          this.pathColumn.DataPropertyName = "FullPath";
          this.pathColumn.HeaderText = "Watch Folders";
          this.pathColumn.Name = "pathColumn";
          this.pathColumn.ReadOnly = true;
          // 
          // tpMatch
          // 
          this.tpMatch.BackColor = System.Drawing.SystemColors.Control;
          this.tpMatch.Controls.Add(this.splitContainer4);
          this.tpMatch.ImageIndex = 12;
          this.tpMatch.Location = new System.Drawing.Point(23, 4);
          this.tpMatch.Name = "tpMatch";
          this.tpMatch.Size = new System.Drawing.Size(578, 514);
          this.tpMatch.TabIndex = 3;
          this.tpMatch.ToolTipText = "Matches";
          // 
          // splitContainer4
          // 
          this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
          this.splitContainer4.Location = new System.Drawing.Point(0, 0);
          this.splitContainer4.Name = "splitContainer4";
          this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
          // 
          // splitContainer4.Panel1
          // 
          this.splitContainer4.Panel1.Controls.Add(this.toolStrip1);
          // 
          // splitContainer4.Panel2
          // 
          this.splitContainer4.Panel2.Controls.Add(this.unapprovedGrid);
          this.splitContainer4.Size = new System.Drawing.Size(578, 514);
          this.splitContainer4.SplitterDistance = 37;
          this.splitContainer4.TabIndex = 13;
          // 
          // toolStrip1
          // 
          this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
          this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.filterSplitButton,
            this.approveButton,
            this.manualAssignButton,
            this.toolStripSeparator3,
            this.rescanButton,
            this.splitJoinButton,
            this.toolStripSeparator2,
            this.ignoreButton,
            this.toolStripSeparator4,
            this.settingsButton,
            this.toolStripButton1});
          this.toolStrip1.Location = new System.Drawing.Point(0, 0);
          this.toolStrip1.Name = "toolStrip1";
          this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
          this.toolStrip1.Size = new System.Drawing.Size(578, 25);
          this.toolStrip1.TabIndex = 6;
          this.toolStrip1.Text = "toolStrip1";
          // 
          // filterSplitButton
          // 
          this.filterSplitButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
          this.filterSplitButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.allMatchesToolStripMenuItem,
            this.processingMatchesToolStripMenuItem,
            this.unapprovedMatchesToolStripMenuItem,
            this.approvedCommitedMatchesToolStripMenuItem});
          this.filterSplitButton.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.filterSplitButton.ImageTransparentColor = System.Drawing.Color.Magenta;
          this.filterSplitButton.Name = "filterSplitButton";
          this.filterSplitButton.Size = new System.Drawing.Size(119, 23);
          this.filterSplitButton.Text = "All Matches";
          this.filterSplitButton.ToolTipText = "Filter Match List";
          this.filterSplitButton.Visible = false;
          // 
          // allMatchesToolStripMenuItem
          // 
          this.allMatchesToolStripMenuItem.Name = "allMatchesToolStripMenuItem";
          this.allMatchesToolStripMenuItem.Size = new System.Drawing.Size(320, 24);
          this.allMatchesToolStripMenuItem.Text = "All Matches";
          // 
          // processingMatchesToolStripMenuItem
          // 
          this.processingMatchesToolStripMenuItem.Name = "processingMatchesToolStripMenuItem";
          this.processingMatchesToolStripMenuItem.Size = new System.Drawing.Size(320, 24);
          this.processingMatchesToolStripMenuItem.Text = "Processing Matches";
          // 
          // unapprovedMatchesToolStripMenuItem
          // 
          this.unapprovedMatchesToolStripMenuItem.Name = "unapprovedMatchesToolStripMenuItem";
          this.unapprovedMatchesToolStripMenuItem.Size = new System.Drawing.Size(320, 24);
          this.unapprovedMatchesToolStripMenuItem.Text = "Unapproved Matches";
          // 
          // approvedCommitedMatchesToolStripMenuItem
          // 
          this.approvedCommitedMatchesToolStripMenuItem.Name = "approvedCommitedMatchesToolStripMenuItem";
          this.approvedCommitedMatchesToolStripMenuItem.Size = new System.Drawing.Size(320, 24);
          this.approvedCommitedMatchesToolStripMenuItem.Text = "Approved/Commited Matches";
          // 
          // approveButton
          // 
          this.approveButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
          this.approveButton.Image = global::mvCentral.Properties.Resources.OK;
          this.approveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
          this.approveButton.Name = "approveButton";
          this.approveButton.Size = new System.Drawing.Size(23, 22);
          this.approveButton.Text = "toolStripButton1";
          this.approveButton.ToolTipText = "Approve Selected File(s)";
          this.approveButton.Click += new System.EventHandler(this.approveButton_Click);
          // 
          // manualAssignButton
          // 
          this.manualAssignButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
          this.manualAssignButton.Image = global::mvCentral.Properties.Resources.page_white_edit;
          this.manualAssignButton.ImageTransparentColor = System.Drawing.Color.Magenta;
          this.manualAssignButton.Name = "manualAssignButton";
          this.manualAssignButton.Size = new System.Drawing.Size(23, 22);
          this.manualAssignButton.Text = "manualAssignButton";
          this.manualAssignButton.ToolTipText = "Add as Blank (Editable) Movie";
          this.manualAssignButton.Click += new System.EventHandler(this.manualAssignButton_Click);
          // 
          // toolStripSeparator3
          // 
          this.toolStripSeparator3.Name = "toolStripSeparator3";
          this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
          // 
          // rescanButton
          // 
          this.rescanButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
          this.rescanButton.Image = global::mvCentral.Properties.Resources.find;
          this.rescanButton.ImageTransparentColor = System.Drawing.Color.Magenta;
          this.rescanButton.Name = "rescanButton";
          this.rescanButton.Size = new System.Drawing.Size(23, 22);
          this.rescanButton.Text = "toolStripButton1";
          this.rescanButton.ToolTipText = "Rescan Selected File(s) with Custom Search String";
          this.rescanButton.Click += new System.EventHandler(this.rescanButton_Click);
          // 
          // splitJoinButton
          // 
          this.splitJoinButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
          this.splitJoinButton.Image = global::mvCentral.Properties.Resources.arrow_divide;
          this.splitJoinButton.ImageTransparentColor = System.Drawing.Color.Magenta;
          this.splitJoinButton.Name = "splitJoinButton";
          this.splitJoinButton.Size = new System.Drawing.Size(23, 22);
          this.splitJoinButton.Text = "toolStripButton1";
          this.splitJoinButton.ToolTipText = "Split Selected File Group";
          this.splitJoinButton.Click += new System.EventHandler(this.splitJoinButton_Click);
          // 
          // toolStripSeparator2
          // 
          this.toolStripSeparator2.Name = "toolStripSeparator2";
          this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
          // 
          // ignoreButton
          // 
          this.ignoreButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
          this.ignoreButton.Image = global::mvCentral.Properties.Resources.cross;
          this.ignoreButton.ImageTransparentColor = System.Drawing.Color.Magenta;
          this.ignoreButton.Name = "ignoreButton";
          this.ignoreButton.Size = new System.Drawing.Size(23, 22);
          this.ignoreButton.Text = "toolStripButton2";
          this.ignoreButton.ToolTipText = "Ignore Selected File(s)";
          // 
          // toolStripSeparator4
          // 
          this.toolStripSeparator4.Name = "toolStripSeparator4";
          this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
          // 
          // settingsButton
          // 
          this.settingsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
          this.settingsButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.unignoreAllFilesToolStripMenuItem,
            this.restartImporterToolStripMenuItem,
            this.automaticMediaInfoMenuItem});
          this.settingsButton.Image = global::mvCentral.Properties.Resources.cog;
          this.settingsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
          this.settingsButton.Name = "settingsButton";
          this.settingsButton.Size = new System.Drawing.Size(32, 22);
          this.settingsButton.ToolTipText = "Advanced Actions";
          this.settingsButton.ButtonClick += new System.EventHandler(this.settingsButton_ButtonClick);
          // 
          // unignoreAllFilesToolStripMenuItem
          // 
          this.unignoreAllFilesToolStripMenuItem.Name = "unignoreAllFilesToolStripMenuItem";
          this.unignoreAllFilesToolStripMenuItem.Size = new System.Drawing.Size(250, 22);
          this.unignoreAllFilesToolStripMenuItem.Text = "Unignore All Files";
          this.unignoreAllFilesToolStripMenuItem.Click += new System.EventHandler(this.unignoreAllFilesToolStripMenuItem_Click);
          // 
          // restartImporterToolStripMenuItem
          // 
          this.restartImporterToolStripMenuItem.Name = "restartImporterToolStripMenuItem";
          this.restartImporterToolStripMenuItem.Size = new System.Drawing.Size(250, 22);
          this.restartImporterToolStripMenuItem.Text = "Restart Importer";
          this.restartImporterToolStripMenuItem.Click += new System.EventHandler(this.restartImporterToolStripMenuItem_Click);
          // 
          // automaticMediaInfoMenuItem
          // 
          this.automaticMediaInfoMenuItem.Name = "automaticMediaInfoMenuItem";
          this.automaticMediaInfoMenuItem.Size = new System.Drawing.Size(250, 22);
          this.automaticMediaInfoMenuItem.Text = "Automatically Retrieve MediaInfo";
          this.automaticMediaInfoMenuItem.Click += new System.EventHandler(this.automaticMediaInfoMenuItem_Click);
          // 
          // toolStripButton1
          // 
          this.toolStripButton1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
          this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
          this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
          this.toolStripButton1.Name = "toolStripButton1";
          this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
          this.toolStripButton1.ToolTipText = "Help";
          // 
          // unapprovedGrid
          // 
          this.unapprovedGrid.AllowUserToAddRows = false;
          this.unapprovedGrid.AllowUserToDeleteRows = false;
          this.unapprovedGrid.AllowUserToResizeRows = false;
          dataGridViewCellStyle14.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
          dataGridViewCellStyle14.BackColor = System.Drawing.SystemColors.Control;
          dataGridViewCellStyle14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          dataGridViewCellStyle14.ForeColor = System.Drawing.SystemColors.WindowText;
          dataGridViewCellStyle14.SelectionBackColor = System.Drawing.SystemColors.Highlight;
          dataGridViewCellStyle14.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
          dataGridViewCellStyle14.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
          this.unapprovedGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle14;
          this.unapprovedGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
          this.unapprovedGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.statusColumn,
            this.unapprovedLocalMediaColumn,
            this.unapprovedPossibleMatchesColumn});
          dataGridViewCellStyle15.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
          dataGridViewCellStyle15.BackColor = System.Drawing.SystemColors.Window;
          dataGridViewCellStyle15.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          dataGridViewCellStyle15.ForeColor = System.Drawing.SystemColors.ControlText;
          dataGridViewCellStyle15.SelectionBackColor = System.Drawing.SystemColors.Highlight;
          dataGridViewCellStyle15.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
          dataGridViewCellStyle15.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
          this.unapprovedGrid.DefaultCellStyle = dataGridViewCellStyle15;
          this.unapprovedGrid.Dock = System.Windows.Forms.DockStyle.Fill;
          this.unapprovedGrid.Location = new System.Drawing.Point(0, 0);
          this.unapprovedGrid.Name = "unapprovedGrid";
          dataGridViewCellStyle16.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
          dataGridViewCellStyle16.BackColor = System.Drawing.SystemColors.Control;
          dataGridViewCellStyle16.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          dataGridViewCellStyle16.ForeColor = System.Drawing.SystemColors.WindowText;
          dataGridViewCellStyle16.SelectionBackColor = System.Drawing.SystemColors.Highlight;
          dataGridViewCellStyle16.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
          dataGridViewCellStyle16.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
          this.unapprovedGrid.RowHeadersDefaultCellStyle = dataGridViewCellStyle16;
          this.unapprovedGrid.RowHeadersVisible = false;
          this.unapprovedGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
          this.unapprovedGrid.Size = new System.Drawing.Size(578, 473);
          this.unapprovedGrid.TabIndex = 8;
          this.unapprovedGrid.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.unapprovedGrid_DataError);
          this.unapprovedGrid.SelectionChanged += new System.EventHandler(this.unapprovedGrid_SelectionChanged);
          // 
          // statusColumn
          // 
          this.statusColumn.HeaderText = "";
          this.statusColumn.Name = "statusColumn";
          this.statusColumn.Width = 20;
          // 
          // unapprovedLocalMediaColumn
          // 
          this.unapprovedLocalMediaColumn.DataPropertyName = "LocalMediaString";
          this.unapprovedLocalMediaColumn.HeaderText = "File(s)";
          this.unapprovedLocalMediaColumn.Name = "unapprovedLocalMediaColumn";
          this.unapprovedLocalMediaColumn.ReadOnly = true;
          this.unapprovedLocalMediaColumn.Width = 200;
          // 
          // unapprovedPossibleMatchesColumn
          // 
          this.unapprovedPossibleMatchesColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
          this.unapprovedPossibleMatchesColumn.DataPropertyName = "Selected";
          this.unapprovedPossibleMatchesColumn.HeaderText = "Possible Matches";
          this.unapprovedPossibleMatchesColumn.Name = "unapprovedPossibleMatchesColumn";
          // 
          // tpStringReplacements
          // 
          this.tpStringReplacements.BackColor = System.Drawing.SystemColors.Control;
          this.tpStringReplacements.Controls.Add(this.splitContainer2);
          this.tpStringReplacements.Controls.Add(this.label69);
          this.tpStringReplacements.ImageIndex = 16;
          this.tpStringReplacements.Location = new System.Drawing.Point(23, 4);
          this.tpStringReplacements.Name = "tpStringReplacements";
          this.tpStringReplacements.Padding = new System.Windows.Forms.Padding(3);
          this.tpStringReplacements.Size = new System.Drawing.Size(578, 514);
          this.tpStringReplacements.TabIndex = 1;
          this.tpStringReplacements.ToolTipText = "String Replacements";
          // 
          // splitContainer2
          // 
          this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
          this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
          this.splitContainer2.IsSplitterFixed = true;
          this.splitContainer2.Location = new System.Drawing.Point(3, 79);
          this.splitContainer2.Name = "splitContainer2";
          // 
          // splitContainer2.Panel1
          // 
          this.splitContainer2.Panel1.Controls.Add(this.dgvReplace);
          // 
          // splitContainer2.Panel2
          // 
          this.splitContainer2.Panel2.Controls.Add(this.btnReplUp);
          this.splitContainer2.Panel2.Controls.Add(this.btnReplDown);
          this.splitContainer2.Panel2.Controls.Add(this.linkLabelResetStringReplacements);
          this.splitContainer2.Panel2.Controls.Add(this.linkLabelImportStringReplacements);
          this.splitContainer2.Panel2.Controls.Add(this.linkLabelExportStringReplacements);
          this.splitContainer2.Size = new System.Drawing.Size(572, 432);
          this.splitContainer2.SplitterDistance = 520;
          this.splitContainer2.TabIndex = 8;
          // 
          // dgvReplace
          // 
          this.dgvReplace.AllowUserToResizeColumns = false;
          this.dgvReplace.AllowUserToResizeRows = false;
          dataGridViewCellStyle17.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
          this.dgvReplace.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle17;
          this.dgvReplace.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
          this.dgvReplace.BackgroundColor = System.Drawing.SystemColors.Window;
          this.dgvReplace.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
          dataGridViewCellStyle18.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
          dataGridViewCellStyle18.BackColor = System.Drawing.SystemColors.Control;
          dataGridViewCellStyle18.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          dataGridViewCellStyle18.ForeColor = System.Drawing.SystemColors.WindowText;
          dataGridViewCellStyle18.SelectionBackColor = System.Drawing.SystemColors.Highlight;
          dataGridViewCellStyle18.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
          dataGridViewCellStyle18.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
          this.dgvReplace.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle18;
          this.dgvReplace.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
          dataGridViewCellStyle19.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
          dataGridViewCellStyle19.BackColor = System.Drawing.SystemColors.Window;
          dataGridViewCellStyle19.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          dataGridViewCellStyle19.ForeColor = System.Drawing.SystemColors.ControlText;
          dataGridViewCellStyle19.SelectionBackColor = System.Drawing.SystemColors.Highlight;
          dataGridViewCellStyle19.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
          dataGridViewCellStyle19.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
          this.dgvReplace.DefaultCellStyle = dataGridViewCellStyle19;
          this.dgvReplace.Dock = System.Windows.Forms.DockStyle.Fill;
          this.dgvReplace.Location = new System.Drawing.Point(0, 0);
          this.dgvReplace.MultiSelect = false;
          this.dgvReplace.Name = "dgvReplace";
          this.dgvReplace.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Sunken;
          dataGridViewCellStyle20.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
          dataGridViewCellStyle20.BackColor = System.Drawing.SystemColors.Control;
          dataGridViewCellStyle20.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          dataGridViewCellStyle20.ForeColor = System.Drawing.SystemColors.WindowText;
          dataGridViewCellStyle20.SelectionBackColor = System.Drawing.SystemColors.Highlight;
          dataGridViewCellStyle20.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
          dataGridViewCellStyle20.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
          this.dgvReplace.RowHeadersDefaultCellStyle = dataGridViewCellStyle20;
          this.dgvReplace.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToDisplayedHeaders;
          dataGridViewCellStyle21.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
          this.dgvReplace.RowsDefaultCellStyle = dataGridViewCellStyle21;
          this.dgvReplace.RowTemplate.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
          this.dgvReplace.RowTemplate.Height = 18;
          this.dgvReplace.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.True;
          this.dgvReplace.Size = new System.Drawing.Size(520, 432);
          this.dgvReplace.StandardTab = true;
          this.dgvReplace.TabIndex = 4;
          this.dgvReplace.Leave += new System.EventHandler(this.dgvReplace_Leave);
          // 
          // btnReplUp
          // 
          this.btnReplUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
          this.btnReplUp.Image = ((System.Drawing.Image)(resources.GetObject("btnReplUp.Image")));
          this.btnReplUp.Location = new System.Drawing.Point(10, 127);
          this.btnReplUp.Name = "btnReplUp";
          this.btnReplUp.Size = new System.Drawing.Size(28, 29);
          this.btnReplUp.TabIndex = 20;
          this.btnReplUp.UseVisualStyleBackColor = true;
          this.btnReplUp.Click += new System.EventHandler(this.btnReplUpDown_Click);
          // 
          // btnReplDown
          // 
          this.btnReplDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
          this.btnReplDown.Image = ((System.Drawing.Image)(resources.GetObject("btnReplDown.Image")));
          this.btnReplDown.Location = new System.Drawing.Point(10, 175);
          this.btnReplDown.Name = "btnReplDown";
          this.btnReplDown.Size = new System.Drawing.Size(28, 29);
          this.btnReplDown.TabIndex = 21;
          this.btnReplDown.UseVisualStyleBackColor = true;
          this.btnReplDown.Click += new System.EventHandler(this.btnReplUpDown_Click);
          // 
          // linkLabelResetStringReplacements
          // 
          this.linkLabelResetStringReplacements.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
          this.linkLabelResetStringReplacements.AutoSize = true;
          this.linkLabelResetStringReplacements.Location = new System.Drawing.Point(9, 53);
          this.linkLabelResetStringReplacements.Name = "linkLabelResetStringReplacements";
          this.linkLabelResetStringReplacements.Size = new System.Drawing.Size(35, 13);
          this.linkLabelResetStringReplacements.TabIndex = 10;
          this.linkLabelResetStringReplacements.TabStop = true;
          this.linkLabelResetStringReplacements.Text = "Reset";
          this.linkLabelResetStringReplacements.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelResetStringReplacements_LinkClicked);
          // 
          // linkLabelImportStringReplacements
          // 
          this.linkLabelImportStringReplacements.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
          this.linkLabelImportStringReplacements.AutoSize = true;
          this.linkLabelImportStringReplacements.Location = new System.Drawing.Point(9, 12);
          this.linkLabelImportStringReplacements.Name = "linkLabelImportStringReplacements";
          this.linkLabelImportStringReplacements.Size = new System.Drawing.Size(36, 13);
          this.linkLabelImportStringReplacements.TabIndex = 9;
          this.linkLabelImportStringReplacements.TabStop = true;
          this.linkLabelImportStringReplacements.Text = "Import";
          this.linkLabelImportStringReplacements.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelImportStringReplacements_LinkClicked);
          // 
          // linkLabelExportStringReplacements
          // 
          this.linkLabelExportStringReplacements.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
          this.linkLabelExportStringReplacements.AutoSize = true;
          this.linkLabelExportStringReplacements.Location = new System.Drawing.Point(9, 32);
          this.linkLabelExportStringReplacements.Name = "linkLabelExportStringReplacements";
          this.linkLabelExportStringReplacements.Size = new System.Drawing.Size(37, 13);
          this.linkLabelExportStringReplacements.TabIndex = 8;
          this.linkLabelExportStringReplacements.TabStop = true;
          this.linkLabelExportStringReplacements.Text = "Export";
          this.linkLabelExportStringReplacements.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelExportStringReplacements_LinkClicked);
          // 
          // label69
          // 
          this.label69.Dock = System.Windows.Forms.DockStyle.Top;
          this.label69.Location = new System.Drawing.Point(3, 3);
          this.label69.Name = "label69";
          this.label69.Size = new System.Drawing.Size(572, 76);
          this.label69.TabIndex = 2;
          this.label69.Text = resources.GetString("label69.Text");
          // 
          // tpParsingExpressions
          // 
          this.tpParsingExpressions.BackColor = System.Drawing.SystemColors.Control;
          this.tpParsingExpressions.Controls.Add(this.splitContainer1);
          this.tpParsingExpressions.Controls.Add(this.label70);
          this.tpParsingExpressions.ImageIndex = 15;
          this.tpParsingExpressions.Location = new System.Drawing.Point(23, 4);
          this.tpParsingExpressions.Name = "tpParsingExpressions";
          this.tpParsingExpressions.Padding = new System.Windows.Forms.Padding(3);
          this.tpParsingExpressions.Size = new System.Drawing.Size(578, 514);
          this.tpParsingExpressions.TabIndex = 2;
          this.tpParsingExpressions.ToolTipText = "Parsing Expressions";
          // 
          // splitContainer1
          // 
          this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
          this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
          this.splitContainer1.IsSplitterFixed = true;
          this.splitContainer1.Location = new System.Drawing.Point(3, 79);
          this.splitContainer1.Name = "splitContainer1";
          // 
          // splitContainer1.Panel1
          // 
          this.splitContainer1.Panel1.Controls.Add(this.dgvExpressions);
          // 
          // splitContainer1.Panel2
          // 
          this.splitContainer1.Panel2.Controls.Add(this.linkExParsingExpressions);
          this.splitContainer1.Panel2.Controls.Add(this.linkImpParsingExpressions);
          this.splitContainer1.Panel2.Controls.Add(this.linkExpressionHelp);
          this.splitContainer1.Panel2.Controls.Add(this.buildExpr);
          this.splitContainer1.Panel2.Controls.Add(this.resetExpr);
          this.splitContainer1.Panel2.Controls.Add(this.btnExpUp);
          this.splitContainer1.Panel2.Controls.Add(this.btnExpDown);
          this.splitContainer1.Size = new System.Drawing.Size(572, 432);
          this.splitContainer1.SplitterDistance = 520;
          this.splitContainer1.TabIndex = 18;
          // 
          // dgvExpressions
          // 
          this.dgvExpressions.AllowUserToResizeColumns = false;
          this.dgvExpressions.AllowUserToResizeRows = false;
          dataGridViewCellStyle22.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
          this.dgvExpressions.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle22;
          this.dgvExpressions.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
          this.dgvExpressions.BackgroundColor = System.Drawing.SystemColors.Window;
          this.dgvExpressions.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
          dataGridViewCellStyle23.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
          dataGridViewCellStyle23.BackColor = System.Drawing.SystemColors.Control;
          dataGridViewCellStyle23.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          dataGridViewCellStyle23.ForeColor = System.Drawing.SystemColors.WindowText;
          dataGridViewCellStyle23.SelectionBackColor = System.Drawing.SystemColors.Highlight;
          dataGridViewCellStyle23.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
          dataGridViewCellStyle23.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
          this.dgvExpressions.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle23;
          this.dgvExpressions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
          dataGridViewCellStyle24.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
          dataGridViewCellStyle24.BackColor = System.Drawing.SystemColors.Window;
          dataGridViewCellStyle24.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          dataGridViewCellStyle24.ForeColor = System.Drawing.SystemColors.ControlText;
          dataGridViewCellStyle24.SelectionBackColor = System.Drawing.SystemColors.Highlight;
          dataGridViewCellStyle24.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
          dataGridViewCellStyle24.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
          this.dgvExpressions.DefaultCellStyle = dataGridViewCellStyle24;
          this.dgvExpressions.Dock = System.Windows.Forms.DockStyle.Fill;
          this.dgvExpressions.Location = new System.Drawing.Point(0, 0);
          this.dgvExpressions.MultiSelect = false;
          this.dgvExpressions.Name = "dgvExpressions";
          this.dgvExpressions.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Sunken;
          dataGridViewCellStyle25.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
          dataGridViewCellStyle25.BackColor = System.Drawing.SystemColors.Control;
          dataGridViewCellStyle25.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          dataGridViewCellStyle25.ForeColor = System.Drawing.SystemColors.WindowText;
          dataGridViewCellStyle25.SelectionBackColor = System.Drawing.SystemColors.Highlight;
          dataGridViewCellStyle25.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
          dataGridViewCellStyle25.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
          this.dgvExpressions.RowHeadersDefaultCellStyle = dataGridViewCellStyle25;
          this.dgvExpressions.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToDisplayedHeaders;
          dataGridViewCellStyle26.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
          this.dgvExpressions.RowsDefaultCellStyle = dataGridViewCellStyle26;
          this.dgvExpressions.RowTemplate.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
          this.dgvExpressions.RowTemplate.Height = 18;
          this.dgvExpressions.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.True;
          this.dgvExpressions.Size = new System.Drawing.Size(520, 432);
          this.dgvExpressions.StandardTab = true;
          this.dgvExpressions.TabIndex = 10;
          this.dgvExpressions.Leave += new System.EventHandler(this.dgvExpressions_Leave);
          // 
          // linkExParsingExpressions
          // 
          this.linkExParsingExpressions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
          this.linkExParsingExpressions.AutoSize = true;
          this.linkExParsingExpressions.Location = new System.Drawing.Point(9, 32);
          this.linkExParsingExpressions.Name = "linkExParsingExpressions";
          this.linkExParsingExpressions.Size = new System.Drawing.Size(37, 13);
          this.linkExParsingExpressions.TabIndex = 24;
          this.linkExParsingExpressions.TabStop = true;
          this.linkExParsingExpressions.Text = "Export";
          this.linkExParsingExpressions.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkExParsingExpressions_LinkClicked);
          // 
          // linkImpParsingExpressions
          // 
          this.linkImpParsingExpressions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
          this.linkImpParsingExpressions.AutoSize = true;
          this.linkImpParsingExpressions.Location = new System.Drawing.Point(9, 12);
          this.linkImpParsingExpressions.Name = "linkImpParsingExpressions";
          this.linkImpParsingExpressions.Size = new System.Drawing.Size(36, 13);
          this.linkImpParsingExpressions.TabIndex = 23;
          this.linkImpParsingExpressions.TabStop = true;
          this.linkImpParsingExpressions.Text = "Import";
          this.linkImpParsingExpressions.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkImpParsingExpressions_LinkClicked);
          // 
          // linkExpressionHelp
          // 
          this.linkExpressionHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
          this.linkExpressionHelp.AutoSize = true;
          this.linkExpressionHelp.Location = new System.Drawing.Point(9, 73);
          this.linkExpressionHelp.Name = "linkExpressionHelp";
          this.linkExpressionHelp.Size = new System.Drawing.Size(29, 13);
          this.linkExpressionHelp.TabIndex = 22;
          this.linkExpressionHelp.TabStop = true;
          this.linkExpressionHelp.Text = "Help";
          // 
          // buildExpr
          // 
          this.buildExpr.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
          this.buildExpr.AutoSize = true;
          this.buildExpr.Location = new System.Drawing.Point(9, 92);
          this.buildExpr.Name = "buildExpr";
          this.buildExpr.Size = new System.Drawing.Size(30, 13);
          this.buildExpr.TabIndex = 21;
          this.buildExpr.TabStop = true;
          this.buildExpr.Text = "Build";
          this.buildExpr.Visible = false;
          // 
          // resetExpr
          // 
          this.resetExpr.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
          this.resetExpr.AutoSize = true;
          this.resetExpr.Location = new System.Drawing.Point(9, 53);
          this.resetExpr.Name = "resetExpr";
          this.resetExpr.Size = new System.Drawing.Size(35, 13);
          this.resetExpr.TabIndex = 20;
          this.resetExpr.TabStop = true;
          this.resetExpr.Text = "Reset";
          this.resetExpr.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.resetExpr_LinkClicked);
          // 
          // btnExpUp
          // 
          this.btnExpUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
          this.btnExpUp.Image = ((System.Drawing.Image)(resources.GetObject("btnExpUp.Image")));
          this.btnExpUp.Location = new System.Drawing.Point(10, 127);
          this.btnExpUp.Name = "btnExpUp";
          this.btnExpUp.Size = new System.Drawing.Size(28, 29);
          this.btnExpUp.TabIndex = 18;
          this.btnExpUp.UseVisualStyleBackColor = true;
          this.btnExpUp.Click += new System.EventHandler(this.btnExpUpDown_Click);
          // 
          // btnExpDown
          // 
          this.btnExpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
          this.btnExpDown.Image = ((System.Drawing.Image)(resources.GetObject("btnExpDown.Image")));
          this.btnExpDown.Location = new System.Drawing.Point(10, 175);
          this.btnExpDown.Name = "btnExpDown";
          this.btnExpDown.Size = new System.Drawing.Size(28, 29);
          this.btnExpDown.TabIndex = 19;
          this.btnExpDown.UseVisualStyleBackColor = true;
          this.btnExpDown.Click += new System.EventHandler(this.btnExpUpDown_Click);
          // 
          // label70
          // 
          this.label70.Dock = System.Windows.Forms.DockStyle.Top;
          this.label70.Location = new System.Drawing.Point(3, 3);
          this.label70.Name = "label70";
          this.label70.Size = new System.Drawing.Size(572, 76);
          this.label70.TabIndex = 14;
          this.label70.Text = resources.GetString("label70.Text");
          // 
          // tbSettings
          // 
          this.tbSettings.Controls.Add(this.label30);
          this.tbSettings.Controls.Add(this.cbPreferThumbnail);
          this.tbSettings.Controls.Add(this.tbVideoPreviewCols);
          this.tbSettings.Controls.Add(this.tbVideoPreviewRows);
          this.tbSettings.Controls.Add(this.label29);
          this.tbSettings.Controls.Add(this.label28);
          this.tbSettings.Controls.Add(this.label27);
          this.tbSettings.Controls.Add(this.label26);
          this.tbSettings.Controls.Add(this.label2);
          this.tbSettings.Controls.Add(this.groupBox7);
          this.tbSettings.Controls.Add(this.label1);
          this.tbSettings.Controls.Add(this.tbLatestVideos);
          this.tbSettings.Controls.Add(this.label14);
          this.tbSettings.Controls.Add(this.label6);
          this.tbSettings.Controls.Add(this.tbMinArtWidth);
          this.tbSettings.Controls.Add(this.groupBox4);
          this.tbSettings.Controls.Add(this.groupBox6);
          this.tbSettings.Controls.Add(this.label4);
          this.tbSettings.Controls.Add(this.tbMinArtHeight);
          this.tbSettings.Controls.Add(this.tbHomeScreen);
          this.tbSettings.Controls.Add(this.label15);
          this.tbSettings.Controls.Add(this.label5);
          this.tbSettings.Controls.Add(this.groupBox5);
          this.tbSettings.Controls.Add(this.label8);
          this.tbSettings.Controls.Add(this.label3);
          this.tbSettings.Controls.Add(this.cbSplitDVD);
          this.tbSettings.Controls.Add(this.label16);
          this.tbSettings.Controls.Add(this.cbAutoApprove);
          this.tbSettings.Controls.Add(this.label12);
          this.tbSettings.Controls.Add(this.cbUseMDAlbum);
          this.tbSettings.Controls.Add(this.label13);
          this.tbSettings.Controls.Add(this.tbTrackArtWidth);
          this.tbSettings.Controls.Add(this.tbTrackArtHeight);
          this.tbSettings.Controls.Add(this.autoDataSourcesPanel1);
          this.tbSettings.Location = new System.Drawing.Point(4, 23);
          this.tbSettings.Name = "tbSettings";
          this.tbSettings.Padding = new System.Windows.Forms.Padding(3);
          this.tbSettings.Size = new System.Drawing.Size(611, 528);
          this.tbSettings.TabIndex = 7;
          this.tbSettings.Text = "Settings";
          this.tbSettings.UseVisualStyleBackColor = true;
          // 
          // label29
          // 
          this.label29.AutoSize = true;
          this.label29.Location = new System.Drawing.Point(285, 294);
          this.label29.Name = "label29";
          this.label29.Size = new System.Drawing.Size(34, 13);
          this.label29.TabIndex = 70;
          this.label29.Text = "Rows";
          // 
          // label28
          // 
          this.label28.AutoSize = true;
          this.label28.Location = new System.Drawing.Point(362, 294);
          this.label28.Name = "label28";
          this.label28.Size = new System.Drawing.Size(47, 13);
          this.label28.TabIndex = 68;
          this.label28.Text = "Columns";
          // 
          // label27
          // 
          this.label27.AutoSize = true;
          this.label27.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.label27.Location = new System.Drawing.Point(54, 262);
          this.label27.Name = "label27";
          this.label27.Size = new System.Drawing.Size(105, 13);
          this.label27.TabIndex = 67;
          this.label27.Text = "Video Thumbnail:";
          // 
          // label26
          // 
          this.label26.AutoSize = true;
          this.label26.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.label26.Location = new System.Drawing.Point(71, 101);
          this.label26.Name = "label26";
          this.label26.Size = new System.Drawing.Size(88, 13);
          this.label26.TabIndex = 65;
          this.label26.Text = "Data Sources:";
          // 
          // label2
          // 
          this.label2.AutoSize = true;
          this.label2.Location = new System.Drawing.Point(427, 233);
          this.label2.Name = "label2";
          this.label2.Size = new System.Drawing.Size(37, 13);
          this.label2.TabIndex = 56;
          this.label2.Text = "Day(s)";
          // 
          // groupBox7
          // 
          this.groupBox7.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
          this.groupBox7.Location = new System.Drawing.Point(12, 422);
          this.groupBox7.Name = "groupBox7";
          this.groupBox7.Size = new System.Drawing.Size(585, 2);
          this.groupBox7.TabIndex = 50;
          this.groupBox7.TabStop = false;
          // 
          // label1
          // 
          this.label1.AutoSize = true;
          this.label1.Location = new System.Drawing.Point(172, 233);
          this.label1.Name = "label1";
          this.label1.Size = new System.Drawing.Size(214, 13);
          this.label1.TabIndex = 55;
          this.label1.Text = "Video is considered new if added in the  last";
          // 
          // tbLatestVideos
          // 
          this.tbLatestVideos.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
          this.tbLatestVideos.Location = new System.Drawing.Point(396, 230);
          this.tbLatestVideos.MaxLength = 2;
          this.tbLatestVideos.Name = "tbLatestVideos";
          this.tbLatestVideos.Setting = null;
          this.tbLatestVideos.Size = new System.Drawing.Size(25, 20);
          this.tbLatestVideos.TabIndex = 54;
          this.tbLatestVideos.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
          // 
          // label14
          // 
          this.label14.AutoSize = true;
          this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.label14.Location = new System.Drawing.Point(15, 385);
          this.label14.Name = "label14";
          this.label14.Size = new System.Drawing.Size(144, 13);
          this.label14.TabIndex = 63;
          this.label14.Text = "Track Art Minimum Size:";
          // 
          // label6
          // 
          this.label6.AutoSize = true;
          this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.label6.Location = new System.Drawing.Point(76, 233);
          this.label6.Name = "label6";
          this.label6.Size = new System.Drawing.Size(83, 13);
          this.label6.TabIndex = 53;
          this.label6.Text = "GUI Settings:";
          // 
          // tbMinArtWidth
          // 
          this.tbMinArtWidth.Location = new System.Drawing.Point(175, 355);
          this.tbMinArtWidth.Name = "tbMinArtWidth";
          this.tbMinArtWidth.Setting = null;
          this.tbMinArtWidth.Size = new System.Drawing.Size(41, 20);
          this.tbMinArtWidth.TabIndex = 0;
          // 
          // groupBox4
          // 
          this.groupBox4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
          this.groupBox4.Location = new System.Drawing.Point(12, 211);
          this.groupBox4.Name = "groupBox4";
          this.groupBox4.Size = new System.Drawing.Size(585, 2);
          this.groupBox4.TabIndex = 48;
          this.groupBox4.TabStop = false;
          // 
          // groupBox6
          // 
          this.groupBox6.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
          this.groupBox6.Location = new System.Drawing.Point(12, 334);
          this.groupBox6.Name = "groupBox6";
          this.groupBox6.Size = new System.Drawing.Size(585, 2);
          this.groupBox6.TabIndex = 49;
          this.groupBox6.TabStop = false;
          // 
          // label4
          // 
          this.label4.AutoSize = true;
          this.label4.Location = new System.Drawing.Point(172, 176);
          this.label4.Name = "label4";
          this.label4.Size = new System.Drawing.Size(146, 13);
          this.label4.TabIndex = 52;
          this.label4.Text = "Plug-in name in home screen:";
          // 
          // tbMinArtHeight
          // 
          this.tbMinArtHeight.Location = new System.Drawing.Point(261, 355);
          this.tbMinArtHeight.Name = "tbMinArtHeight";
          this.tbMinArtHeight.Setting = null;
          this.tbMinArtHeight.Size = new System.Drawing.Size(41, 20);
          this.tbMinArtHeight.TabIndex = 1;
          // 
          // tbHomeScreen
          // 
          this.tbHomeScreen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
          this.tbHomeScreen.Location = new System.Drawing.Point(324, 173);
          this.tbHomeScreen.Name = "tbHomeScreen";
          this.tbHomeScreen.Setting = null;
          this.tbHomeScreen.Size = new System.Drawing.Size(173, 20);
          this.tbHomeScreen.TabIndex = 51;
          // 
          // label15
          // 
          this.label15.AutoSize = true;
          this.label15.Location = new System.Drawing.Point(310, 385);
          this.label15.Name = "label15";
          this.label15.Size = new System.Drawing.Size(38, 13);
          this.label15.TabIndex = 62;
          this.label15.Text = "Height";
          // 
          // label5
          // 
          this.label5.AutoSize = true;
          this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.label5.Location = new System.Drawing.Point(63, 176);
          this.label5.Name = "label5";
          this.label5.Size = new System.Drawing.Size(96, 13);
          this.label5.TabIndex = 48;
          this.label5.Text = "Plugin Settings:";
          // 
          // groupBox5
          // 
          this.groupBox5.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
          this.groupBox5.Location = new System.Drawing.Point(12, 159);
          this.groupBox5.Name = "groupBox5";
          this.groupBox5.Size = new System.Drawing.Size(585, 2);
          this.groupBox5.TabIndex = 47;
          this.groupBox5.TabStop = false;
          // 
          // label8
          // 
          this.label8.AutoSize = true;
          this.label8.Location = new System.Drawing.Point(220, 358);
          this.label8.Name = "label8";
          this.label8.Size = new System.Drawing.Size(35, 13);
          this.label8.TabIndex = 3;
          this.label8.Text = "Width";
          // 
          // label3
          // 
          this.label3.AutoSize = true;
          this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.label3.Location = new System.Drawing.Point(21, 19);
          this.label3.Name = "label3";
          this.label3.Size = new System.Drawing.Size(138, 13);
          this.label3.TabIndex = 47;
          this.label3.Text = "Auto Approve Settings:";
          // 
          // cbSplitDVD
          // 
          this.cbSplitDVD.AutoSize = true;
          this.cbSplitDVD.IgnoreSettingName = true;
          this.cbSplitDVD.Location = new System.Drawing.Point(175, 65);
          this.cbSplitDVD.Name = "cbSplitDVD";
          this.cbSplitDVD.Setting = null;
          this.cbSplitDVD.Size = new System.Drawing.Size(127, 17);
          this.cbSplitDVD.TabIndex = 45;
          this.cbSplitDVD.Text = "Split DVD in chapters";
          this.cbSplitDVD.UseVisualStyleBackColor = true;
          // 
          // label16
          // 
          this.label16.AutoSize = true;
          this.label16.Location = new System.Drawing.Point(220, 385);
          this.label16.Name = "label16";
          this.label16.Size = new System.Drawing.Size(35, 13);
          this.label16.TabIndex = 61;
          this.label16.Text = "Width";
          // 
          // cbAutoApprove
          // 
          this.cbAutoApprove.AutoSize = true;
          this.cbAutoApprove.IgnoreSettingName = true;
          this.cbAutoApprove.Location = new System.Drawing.Point(175, 19);
          this.cbAutoApprove.Name = "cbAutoApprove";
          this.cbAutoApprove.Setting = null;
          this.cbAutoApprove.Size = new System.Drawing.Size(219, 17);
          this.cbAutoApprove.TabIndex = 41;
          this.cbAutoApprove.Text = "Autoapprove if match found on scrapper.";
          this.cbAutoApprove.UseVisualStyleBackColor = true;
          // 
          // label12
          // 
          this.label12.AutoSize = true;
          this.label12.Location = new System.Drawing.Point(310, 358);
          this.label12.Name = "label12";
          this.label12.Size = new System.Drawing.Size(38, 13);
          this.label12.TabIndex = 4;
          this.label12.Text = "Height";
          // 
          // cbUseMDAlbum
          // 
          this.cbUseMDAlbum.AutoSize = true;
          this.cbUseMDAlbum.IgnoreSettingName = true;
          this.cbUseMDAlbum.Location = new System.Drawing.Point(175, 42);
          this.cbUseMDAlbum.Name = "cbUseMDAlbum";
          this.cbUseMDAlbum.Setting = null;
          this.cbUseMDAlbum.Size = new System.Drawing.Size(294, 17);
          this.cbUseMDAlbum.TabIndex = 42;
          this.cbUseMDAlbum.Text = "Use the scrapper found album instead of the parsed one.";
          this.cbUseMDAlbum.UseVisualStyleBackColor = true;
          // 
          // label13
          // 
          this.label13.AutoSize = true;
          this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.label13.Location = new System.Drawing.Point(22, 358);
          this.label13.Name = "label13";
          this.label13.Size = new System.Drawing.Size(137, 13);
          this.label13.TabIndex = 57;
          this.label13.Text = "Artist Art Minmum Size:";
          // 
          // tbTrackArtWidth
          // 
          this.tbTrackArtWidth.Location = new System.Drawing.Point(175, 382);
          this.tbTrackArtWidth.Name = "tbTrackArtWidth";
          this.tbTrackArtWidth.Setting = null;
          this.tbTrackArtWidth.Size = new System.Drawing.Size(41, 20);
          this.tbTrackArtWidth.TabIndex = 58;
          // 
          // tbTrackArtHeight
          // 
          this.tbTrackArtHeight.Location = new System.Drawing.Point(261, 382);
          this.tbTrackArtHeight.Name = "tbTrackArtHeight";
          this.tbTrackArtHeight.Setting = null;
          this.tbTrackArtHeight.Size = new System.Drawing.Size(41, 20);
          this.tbTrackArtHeight.TabIndex = 59;
          // 
          // tpAbout
          // 
          this.tpAbout.Controls.Add(this.pictureBox3);
          this.tpAbout.Controls.Add(this.groupBox1);
          this.tpAbout.Controls.Add(this.lblProductVersion);
          this.tpAbout.ImageIndex = 10;
          this.tpAbout.Location = new System.Drawing.Point(4, 23);
          this.tpAbout.Name = "tpAbout";
          this.tpAbout.Padding = new System.Windows.Forms.Padding(3);
          this.tpAbout.Size = new System.Drawing.Size(611, 528);
          this.tpAbout.TabIndex = 4;
          this.tpAbout.Text = "About";
          this.tpAbout.UseVisualStyleBackColor = true;
          // 
          // pictureBox3
          // 
          this.pictureBox3.Image = global::mvCentral.Properties.Resources.mvCentralLogo;
          this.pictureBox3.Location = new System.Drawing.Point(3, 3);
          this.pictureBox3.Name = "pictureBox3";
          this.pictureBox3.Size = new System.Drawing.Size(603, 103);
          this.pictureBox3.TabIndex = 23;
          this.pictureBox3.TabStop = false;
          // 
          // groupBox1
          // 
          this.groupBox1.Controls.Add(this.label24);
          this.groupBox1.Controls.Add(this.label23);
          this.groupBox1.Controls.Add(this.label17);
          this.groupBox1.Controls.Add(this.label22);
          this.groupBox1.Controls.Add(this.label7);
          this.groupBox1.Controls.Add(this.label18);
          this.groupBox1.Controls.Add(this.label19);
          this.groupBox1.Controls.Add(this.label20);
          this.groupBox1.Controls.Add(this.labelForum);
          this.groupBox1.Controls.Add(this.labelManual);
          this.groupBox1.Controls.Add(this.labelGoogleCode);
          this.groupBox1.Controls.Add(this.label36);
          this.groupBox1.Controls.Add(this.groupBox3);
          this.groupBox1.Controls.Add(this.label25);
          this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.groupBox1.Location = new System.Drawing.Point(3, 152);
          this.groupBox1.Name = "groupBox1";
          this.groupBox1.Size = new System.Drawing.Size(603, 379);
          this.groupBox1.TabIndex = 22;
          this.groupBox1.TabStop = false;
          this.groupBox1.Text = "About  ";
          // 
          // label24
          // 
          this.label24.AutoSize = true;
          this.label24.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.label24.Location = new System.Drawing.Point(180, 177);
          this.label24.Name = "label24";
          this.label24.Size = new System.Drawing.Size(50, 13);
          this.label24.TabIndex = 33;
          this.label24.Text = "RoChess";
          // 
          // label23
          // 
          this.label23.AutoSize = true;
          this.label23.Location = new System.Drawing.Point(51, 177);
          this.label23.Name = "label23";
          this.label23.Size = new System.Drawing.Size(123, 13);
          this.label23.TabIndex = 32;
          this.label23.Text = "Parsing expressions:";
          // 
          // label17
          // 
          this.label17.AutoSize = true;
          this.label17.Location = new System.Drawing.Point(12, 151);
          this.label17.Name = "label17";
          this.label17.Size = new System.Drawing.Size(162, 13);
          this.label17.TabIndex = 31;
          this.label17.Text = "Testing and encourgement:";
          // 
          // label22
          // 
          this.label22.AutoSize = true;
          this.label22.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.label22.Location = new System.Drawing.Point(180, 151);
          this.label22.Name = "label22";
          this.label22.Size = new System.Drawing.Size(65, 13);
          this.label22.TabIndex = 30;
          this.label22.Text = "Lyfesaver74";
          // 
          // label7
          // 
          this.label7.AutoSize = true;
          this.label7.Location = new System.Drawing.Point(12, 123);
          this.label7.Name = "label7";
          this.label7.Size = new System.Drawing.Size(351, 13);
          this.label7.TabIndex = 29;
          this.label7.Text = "Thanks also go to the follwoing for help during development.";
          // 
          // label18
          // 
          this.label18.AutoSize = true;
          this.label18.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.label18.Location = new System.Drawing.Point(12, 62);
          this.label18.Name = "label18";
          this.label18.Size = new System.Drawing.Size(390, 13);
          this.label18.TabIndex = 27;
          this.label18.Text = "Based on MP-TV-series / Moving Pictures code . Many thanks to the developers.";
          // 
          // label19
          // 
          this.label19.AutoSize = true;
          this.label19.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.label19.Location = new System.Drawing.Point(241, 85);
          this.label19.Name = "label19";
          this.label19.Size = new System.Drawing.Size(64, 13);
          this.label19.TabIndex = 26;
          this.label19.Text = "Trevor, Gup";
          // 
          // label20
          // 
          this.label20.AutoSize = true;
          this.label20.Location = new System.Drawing.Point(9, 85);
          this.label20.Name = "label20";
          this.label20.Size = new System.Drawing.Size(226, 13);
          this.label20.TabIndex = 25;
          this.label20.Text = "Project coordination and development:";
          // 
          // labelForum
          // 
          this.labelForum.AutoSize = true;
          this.labelForum.Enabled = false;
          this.labelForum.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.labelForum.Location = new System.Drawing.Point(12, 343);
          this.labelForum.Name = "labelForum";
          this.labelForum.Size = new System.Drawing.Size(175, 13);
          this.labelForum.TabIndex = 18;
          this.labelForum.TabStop = true;
          this.labelForum.Text = "mvCentral MediaPortal forum thread";
          // 
          // labelManual
          // 
          this.labelManual.AutoSize = true;
          this.labelManual.Enabled = false;
          this.labelManual.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.labelManual.Location = new System.Drawing.Point(12, 320);
          this.labelManual.Name = "labelManual";
          this.labelManual.Size = new System.Drawing.Size(239, 13);
          this.labelManual.TabIndex = 17;
          this.labelManual.TabStop = true;
          this.labelManual.Text = "mvCentral User/Developer/Skin designer manual";
          // 
          // labelGoogleCode
          // 
          this.labelGoogleCode.AutoSize = true;
          this.labelGoogleCode.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.labelGoogleCode.Location = new System.Drawing.Point(12, 298);
          this.labelGoogleCode.Name = "labelGoogleCode";
          this.labelGoogleCode.Size = new System.Drawing.Size(154, 13);
          this.labelGoogleCode.TabIndex = 16;
          this.labelGoogleCode.TabStop = true;
          this.labelGoogleCode.Text = "mvCentral Google Code project";
          this.labelGoogleCode.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.labelGoogleCode_LinkClicked);
          // 
          // label36
          // 
          this.label36.AutoSize = true;
          this.label36.Location = new System.Drawing.Point(12, 275);
          this.label36.Name = "label36";
          this.label36.Size = new System.Drawing.Size(96, 13);
          this.label36.TabIndex = 15;
          this.label36.Text = "Help resources:";
          // 
          // groupBox3
          // 
          this.groupBox3.Location = new System.Drawing.Point(12, 269);
          this.groupBox3.Name = "groupBox3";
          this.groupBox3.Size = new System.Drawing.Size(580, 3);
          this.groupBox3.TabIndex = 14;
          this.groupBox3.TabStop = false;
          // 
          // label25
          // 
          this.label25.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.label25.Location = new System.Drawing.Point(9, 16);
          this.label25.Name = "label25";
          this.label25.Size = new System.Drawing.Size(588, 46);
          this.label25.TabIndex = 3;
          this.label25.Text = "mvCentral is a plugin for the MediaPortal HTPC application. The purpose of the pl" +
              "ugin is to allow the user to play music videos. \r\n\r\n";
          // 
          // lblProductVersion
          // 
          this.lblProductVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
          this.lblProductVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.lblProductVersion.Location = new System.Drawing.Point(536, 136);
          this.lblProductVersion.Name = "lblProductVersion";
          this.lblProductVersion.Size = new System.Drawing.Size(72, 13);
          this.lblProductVersion.TabIndex = 21;
          this.lblProductVersion.Text = "v1.x.x.xxx";
          this.lblProductVersion.TextAlign = System.Drawing.ContentAlignment.TopRight;
          this.lblProductVersion.UseCompatibleTextRendering = true;
          // 
          // gbProgress
          // 
          this.gbProgress.BackColor = System.Drawing.SystemColors.Control;
          this.gbProgress.Controls.Add(this.progressBar);
          this.gbProgress.Controls.Add(this.currentTaskDesc);
          this.gbProgress.Controls.Add(this.countProgressLabel);
          this.gbProgress.Dock = System.Windows.Forms.DockStyle.Bottom;
          this.gbProgress.Location = new System.Drawing.Point(0, 555);
          this.gbProgress.Name = "gbProgress";
          this.gbProgress.Size = new System.Drawing.Size(619, 48);
          this.gbProgress.TabIndex = 6;
          this.gbProgress.TabStop = false;
          this.gbProgress.Text = "Processing";
          // 
          // progressBar
          // 
          this.progressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
          this.progressBar.Location = new System.Drawing.Point(3, 35);
          this.progressBar.Name = "progressBar";
          this.progressBar.Size = new System.Drawing.Size(613, 10);
          this.progressBar.TabIndex = 4;
          // 
          // currentTaskDesc
          // 
          this.currentTaskDesc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
          this.currentTaskDesc.AutoSize = true;
          this.currentTaskDesc.ForeColor = System.Drawing.SystemColors.ControlDark;
          this.currentTaskDesc.Location = new System.Drawing.Point(6, 19);
          this.currentTaskDesc.Name = "currentTaskDesc";
          this.currentTaskDesc.Size = new System.Drawing.Size(115, 13);
          this.currentTaskDesc.TabIndex = 5;
          this.currentTaskDesc.Text = "Currently Processing ...";
          this.currentTaskDesc.Visible = false;
          // 
          // countProgressLabel
          // 
          this.countProgressLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
          this.countProgressLabel.ForeColor = System.Drawing.SystemColors.ControlDark;
          this.countProgressLabel.Location = new System.Drawing.Point(523, 20);
          this.countProgressLabel.Name = "countProgressLabel";
          this.countProgressLabel.Size = new System.Drawing.Size(93, 13);
          this.countProgressLabel.TabIndex = 6;
          this.countProgressLabel.Text = "(0/99)";
          this.countProgressLabel.TextAlign = System.Drawing.ContentAlignment.BottomRight;
          this.countProgressLabel.Visible = false;
          // 
          // rtbLog
          // 
          this.rtbLog.Dock = System.Windows.Forms.DockStyle.Fill;
          this.rtbLog.Location = new System.Drawing.Point(0, 0);
          this.rtbLog.Name = "rtbLog";
          this.rtbLog.Size = new System.Drawing.Size(150, 46);
          this.rtbLog.TabIndex = 0;
          this.rtbLog.Text = "";
          // 
          // toolStripSeparator5
          // 
          this.toolStripSeparator5.Name = "toolStripSeparator5";
          this.toolStripSeparator5.Size = new System.Drawing.Size(6, 6);
          // 
          // btnPlay
          // 
          this.btnPlay.Name = "btnPlay";
          this.btnPlay.Size = new System.Drawing.Size(23, 23);
          // 
          // toolStripSeparator6
          // 
          this.toolStripSeparator6.Name = "toolStripSeparator6";
          this.toolStripSeparator6.Size = new System.Drawing.Size(6, 6);
          // 
          // btnArtRefresh
          // 
          this.btnArtRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
          this.btnArtRefresh.Image = global::mvCentral.Properties.Resources.arrow_rotate_clockwise;
          this.btnArtRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
          this.btnArtRefresh.Name = "btnArtRefresh";
          this.btnArtRefresh.Size = new System.Drawing.Size(31, 20);
          this.btnArtRefresh.Text = "toolStripButton2";
          this.btnArtRefresh.ToolTipText = "Refesh Art Selection from Online Data Sources";
          this.btnArtRefresh.Visible = false;
          this.btnArtRefresh.Click += new System.EventHandler(this.btnArtRefresh_Click);
          // 
          // btnArtloadNew
          // 
          this.btnArtloadNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
          this.btnArtloadNew.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadArtFromFileToolStripMenuItem,
            this.loadArtFromURLToolStripMenuItem,
            this.loadArtFromMusicVideoToolStripMenuItem});
          this.btnArtloadNew.Image = global::mvCentral.Properties.Resources.folder_image;
          this.btnArtloadNew.ImageTransparentColor = System.Drawing.Color.Magenta;
          this.btnArtloadNew.Name = "btnArtloadNew";
          this.btnArtloadNew.Size = new System.Drawing.Size(31, 20);
          this.btnArtloadNew.ToolTipText = "Load Art From File";
          this.btnArtloadNew.Visible = false;
          // 
          // loadArtFromFileToolStripMenuItem
          // 
          this.loadArtFromFileToolStripMenuItem.Name = "loadArtFromFileToolStripMenuItem";
          this.loadArtFromFileToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
          this.loadArtFromFileToolStripMenuItem.Text = "Load  Art From File";
          this.loadArtFromFileToolStripMenuItem.Click += new System.EventHandler(this.loadArtFromFileToolStripMenuItem_Click);
          // 
          // loadArtFromURLToolStripMenuItem
          // 
          this.loadArtFromURLToolStripMenuItem.Name = "loadArtFromURLToolStripMenuItem";
          this.loadArtFromURLToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
          this.loadArtFromURLToolStripMenuItem.Text = "Load Art From URL";
          this.loadArtFromURLToolStripMenuItem.Click += new System.EventHandler(this.loadArtFromURLToolStripMenuItem_Click);
          // 
          // loadArtFromMusicVideoToolStripMenuItem
          // 
          this.loadArtFromMusicVideoToolStripMenuItem.Name = "loadArtFromMusicVideoToolStripMenuItem";
          this.loadArtFromMusicVideoToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
          this.loadArtFromMusicVideoToolStripMenuItem.Text = "Load Art From MusicVideo";
          this.loadArtFromMusicVideoToolStripMenuItem.Click += new System.EventHandler(this.loadArtFromMusicVideoToolStripMenuItem_Click);
          // 
          // toolStripSeparator7
          // 
          this.toolStripSeparator7.Name = "toolStripSeparator7";
          this.toolStripSeparator7.Size = new System.Drawing.Size(22, 6);
          // 
          // toolStripSeparator1
          // 
          this.toolStripSeparator1.Name = "toolStripSeparator1";
          this.toolStripSeparator1.Size = new System.Drawing.Size(31, 6);
          // 
          // ArtFileDialog
          // 
          this.ArtFileDialog.Filter = "\"Image Files|*.jpg;*.png;*.bmp;*.gif\"";
          // 
          // imageListDrag
          // 
          this.imageListDrag.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
          this.imageListDrag.ImageSize = new System.Drawing.Size(16, 16);
          this.imageListDrag.TransparentColor = System.Drawing.Color.Transparent;
          // 
          // dataGridViewTextBoxColumn1
          // 
          this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
          this.dataGridViewTextBoxColumn1.DataPropertyName = "FullPath";
          this.dataGridViewTextBoxColumn1.HeaderText = "Watch Folders";
          this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
          this.dataGridViewTextBoxColumn1.ReadOnly = true;
          // 
          // dataGridViewTextBoxColumn2
          // 
          this.dataGridViewTextBoxColumn2.DataPropertyName = "LocalMediaString";
          this.dataGridViewTextBoxColumn2.HeaderText = "File(s)";
          this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
          this.dataGridViewTextBoxColumn2.ReadOnly = true;
          this.dataGridViewTextBoxColumn2.Width = 200;
          // 
          // dataGridViewTextBoxColumn3
          // 
          this.dataGridViewTextBoxColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
          this.dataGridViewTextBoxColumn3.DataPropertyName = "FullPath";
          this.dataGridViewTextBoxColumn3.HeaderText = "Watch Folders";
          this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
          this.dataGridViewTextBoxColumn3.ReadOnly = true;
          // 
          // tbVideoPreviewRows
          // 
          this.tbVideoPreviewRows.Location = new System.Drawing.Point(325, 291);
          this.tbVideoPreviewRows.Name = "tbVideoPreviewRows";
          this.tbVideoPreviewRows.Setting = null;
          this.tbVideoPreviewRows.Size = new System.Drawing.Size(25, 20);
          this.tbVideoPreviewRows.TabIndex = 71;
          // 
          // tbVideoPreviewCols
          // 
          this.tbVideoPreviewCols.Location = new System.Drawing.Point(416, 291);
          this.tbVideoPreviewCols.Name = "tbVideoPreviewCols";
          this.tbVideoPreviewCols.Setting = null;
          this.tbVideoPreviewCols.Size = new System.Drawing.Size(25, 20);
          this.tbVideoPreviewCols.TabIndex = 72;
          // 
          // fileNameDataGridViewTextBoxColumn
          // 
          this.fileNameDataGridViewTextBoxColumn.DataPropertyName = "FileName";
          this.fileNameDataGridViewTextBoxColumn.HeaderText = "FileName";
          this.fileNameDataGridViewTextBoxColumn.Name = "fileNameDataGridViewTextBoxColumn";
          this.fileNameDataGridViewTextBoxColumn.ReadOnly = true;
          // 
          // artistDataGridViewTextBoxColumn
          // 
          this.artistDataGridViewTextBoxColumn.DataPropertyName = "Artist";
          this.artistDataGridViewTextBoxColumn.HeaderText = "Artist";
          this.artistDataGridViewTextBoxColumn.Name = "artistDataGridViewTextBoxColumn";
          this.artistDataGridViewTextBoxColumn.ReadOnly = true;
          // 
          // albumDataGridViewTextBoxColumn
          // 
          this.albumDataGridViewTextBoxColumn.DataPropertyName = "Album";
          this.albumDataGridViewTextBoxColumn.HeaderText = "Album";
          this.albumDataGridViewTextBoxColumn.Name = "albumDataGridViewTextBoxColumn";
          this.albumDataGridViewTextBoxColumn.ReadOnly = true;
          // 
          // trackDataGridViewTextBoxColumn
          // 
          this.trackDataGridViewTextBoxColumn.DataPropertyName = "Track";
          this.trackDataGridViewTextBoxColumn.HeaderText = "Track";
          this.trackDataGridViewTextBoxColumn.Name = "trackDataGridViewTextBoxColumn";
          this.trackDataGridViewTextBoxColumn.ReadOnly = true;
          // 
          // extDataGridViewTextBoxColumn
          // 
          this.extDataGridViewTextBoxColumn.DataPropertyName = "Ext";
          this.extDataGridViewTextBoxColumn.HeaderText = "Ext";
          this.extDataGridViewTextBoxColumn.Name = "extDataGridViewTextBoxColumn";
          this.extDataGridViewTextBoxColumn.ReadOnly = true;
          // 
          // volumeLabelDataGridViewTextBoxColumn
          // 
          this.volumeLabelDataGridViewTextBoxColumn.DataPropertyName = "VolumeLabel";
          this.volumeLabelDataGridViewTextBoxColumn.HeaderText = "VolumeLabel";
          this.volumeLabelDataGridViewTextBoxColumn.Name = "volumeLabelDataGridViewTextBoxColumn";
          this.volumeLabelDataGridViewTextBoxColumn.ReadOnly = true;
          // 
          // pathDataGridViewTextBoxColumn
          // 
          this.pathDataGridViewTextBoxColumn.DataPropertyName = "Path";
          this.pathDataGridViewTextBoxColumn.HeaderText = "Path";
          this.pathDataGridViewTextBoxColumn.Name = "pathDataGridViewTextBoxColumn";
          this.pathDataGridViewTextBoxColumn.ReadOnly = true;
          // 
          // ParserBindingSource
          // 
          this.ParserBindingSource.DataSource = typeof(mvCentral.LocalMediaManagement.parseResult);
          // 
          // autoDataSourcesPanel1
          // 
          this.autoDataSourcesPanel1.AutoCommit = true;
          this.autoDataSourcesPanel1.Location = new System.Drawing.Point(172, 97);
          this.autoDataSourcesPanel1.Name = "autoDataSourcesPanel1";
          this.autoDataSourcesPanel1.Size = new System.Drawing.Size(433, 44);
          this.autoDataSourcesPanel1.TabIndex = 64;
          // 
          // unapprovedMatchesBindingSource
          // 
          this.unapprovedMatchesBindingSource.DataSource = typeof(mvCentral.LocalMediaManagement.MusicVideoMatch);
          this.unapprovedMatchesBindingSource.ListChanged += new System.ComponentModel.ListChangedEventHandler(this.unapprovedMatchesBindingSource_ListChanged);
          // 
          // cbPreferThumbnail
          // 
          this.cbPreferThumbnail.AutoSize = true;
          this.cbPreferThumbnail.IgnoreSettingName = true;
          this.cbPreferThumbnail.Location = new System.Drawing.Point(175, 265);
          this.cbPreferThumbnail.Name = "cbPreferThumbnail";
          this.cbPreferThumbnail.Setting = null;
          this.cbPreferThumbnail.Size = new System.Drawing.Size(304, 17);
          this.cbPreferThumbnail.TabIndex = 73;
          this.cbPreferThumbnail.Text = "Prefer Video Thumbnail over parsed image for video image.";
          this.cbPreferThumbnail.UseVisualStyleBackColor = true;
          // 
          // label30
          // 
          this.label30.AutoSize = true;
          this.label30.Location = new System.Drawing.Point(176, 294);
          this.label30.Name = "label30";
          this.label30.Size = new System.Drawing.Size(103, 13);
          this.label30.TabIndex = 74;
          this.label30.Text = "Preview Apperance:";
          // 
          // ConfigForm
          // 
          this.AllowDrop = true;
          this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
          this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
          this.ClientSize = new System.Drawing.Size(619, 603);
          this.Controls.Add(this.scMain);
          this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
          this.HelpButton = true;
          this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
          this.MaximizeBox = false;
          this.MinimizeBox = false;
          this.Name = "ConfigForm";
          this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
          this.Text = "mvCentral Configuration";
          this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ConfigForm_FormClosing);
          this.Load += new System.EventHandler(this.ConfigForm_Load);
          ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
          this.scMain.Panel1.ResumeLayout(false);
          this.scMain.Panel2.ResumeLayout(false);
          this.scMain.ResumeLayout(false);
          this.mainTab.ResumeLayout(false);
          this.tpLibrary.ResumeLayout(false);
          this.splitContainer3.Panel1.ResumeLayout(false);
          this.splitContainer3.Panel2.ResumeLayout(false);
          this.splitContainer3.Panel2.PerformLayout();
          this.splitContainer3.ResumeLayout(false);
          this.cmLibrary.ResumeLayout(false);
          ((System.ComponentModel.ISupportInitialize)(this.artImage)).EndInit();
          this.coverToolStrip.ResumeLayout(false);
          this.coverToolStrip.PerformLayout();
          this.tcMusicVideo.ResumeLayout(false);
          this.tpArtist.ResumeLayout(false);
          this.scArtistList.Panel2.ResumeLayout(false);
          this.scArtistList.ResumeLayout(false);
          this.tpAlbum.ResumeLayout(false);
          this.scAlbumDetails.Panel2.ResumeLayout(false);
          this.scAlbumDetails.ResumeLayout(false);
          this.tpTrack.ResumeLayout(false);
          this.scTrackDetails.Panel1.ResumeLayout(false);
          this.scTrackDetails.Panel2.ResumeLayout(false);
          this.scTrackDetails.ResumeLayout(false);
          this.tpImport.ResumeLayout(false);
          this.tcImport.ResumeLayout(false);
          this.tpImportPathParser.ResumeLayout(false);
          ((System.ComponentModel.ISupportInitialize)(this.dgvParser)).EndInit();
          this.pathsGroupBox.ResumeLayout(false);
          this.pathsGroupBox.PerformLayout();
          this.toolStrip.ResumeLayout(false);
          this.toolStrip.PerformLayout();
          ((System.ComponentModel.ISupportInitialize)(this.pathsGridView)).EndInit();
          this.tpMatch.ResumeLayout(false);
          this.splitContainer4.Panel1.ResumeLayout(false);
          this.splitContainer4.Panel1.PerformLayout();
          this.splitContainer4.Panel2.ResumeLayout(false);
          this.splitContainer4.ResumeLayout(false);
          this.toolStrip1.ResumeLayout(false);
          this.toolStrip1.PerformLayout();
          ((System.ComponentModel.ISupportInitialize)(this.unapprovedGrid)).EndInit();
          this.tpStringReplacements.ResumeLayout(false);
          this.splitContainer2.Panel1.ResumeLayout(false);
          this.splitContainer2.Panel2.ResumeLayout(false);
          this.splitContainer2.Panel2.PerformLayout();
          this.splitContainer2.ResumeLayout(false);
          ((System.ComponentModel.ISupportInitialize)(this.dgvReplace)).EndInit();
          this.tpParsingExpressions.ResumeLayout(false);
          this.splitContainer1.Panel1.ResumeLayout(false);
          this.splitContainer1.Panel2.ResumeLayout(false);
          this.splitContainer1.Panel2.PerformLayout();
          this.splitContainer1.ResumeLayout(false);
          ((System.ComponentModel.ISupportInitialize)(this.dgvExpressions)).EndInit();
          this.tbSettings.ResumeLayout(false);
          this.tbSettings.PerformLayout();
          this.tpAbout.ResumeLayout(false);
          ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
          this.groupBox1.ResumeLayout(false);
          this.groupBox1.PerformLayout();
          this.gbProgress.ResumeLayout(false);
          this.gbProgress.PerformLayout();
          ((System.ComponentModel.ISupportInitialize)(this.ParserBindingSource)).EndInit();
          ((System.ComponentModel.ISupportInitialize)(this.unapprovedMatchesBindingSource)).EndInit();
          this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox textBox6;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.PictureBox pictureBox4;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ListBox listBox2;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.FolderBrowserDialog getWatchFolder;
        private System.Windows.Forms.OpenFileDialog getDT;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.SplitContainer scMain;
        private System.Windows.Forms.TabControl mainTab;
        private System.Windows.Forms.TabPage tpAbout;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.LinkLabel labelForum;
        private System.Windows.Forms.LinkLabel labelManual;
        private System.Windows.Forms.LinkLabel labelGoogleCode;
        private System.Windows.Forms.Label label36;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.Label lblProductVersion;
        private System.Windows.Forms.TabPage tpImport;
        private System.Windows.Forms.RichTextBox rtbLog;
        private System.Windows.Forms.TabControl tcImport;
        private System.Windows.Forms.TabPage tpImportPathParser;
        private System.Windows.Forms.TabPage tpStringReplacements;
        private System.Windows.Forms.GroupBox pathsGroupBox;
        private Cornerstone.GUI.Controls.SettingCheckBox importDvdCheckBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.DataGridView pathsGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn pathColumn;
        private System.Windows.Forms.TabPage tpParsingExpressions;
        private System.Windows.Forms.Label label69;
        private System.Windows.Forms.Label label70;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView dgvExpressions;
        private System.Windows.Forms.LinkLabel linkExParsingExpressions;
        private System.Windows.Forms.LinkLabel linkImpParsingExpressions;
        private System.Windows.Forms.LinkLabel linkExpressionHelp;
        private System.Windows.Forms.LinkLabel buildExpr;
        private System.Windows.Forms.LinkLabel resetExpr;
        private System.Windows.Forms.Button btnExpUp;
        private System.Windows.Forms.Button btnExpDown;
        private System.Windows.Forms.Button btnTestReparse;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.DataGridView dgvReplace;
        private System.Windows.Forms.LinkLabel linkLabelResetStringReplacements;
        private System.Windows.Forms.LinkLabel linkLabelImportStringReplacements;
        private System.Windows.Forms.LinkLabel linkLabelExportStringReplacements;
        private System.Windows.Forms.Button btnReplUp;
        private System.Windows.Forms.Button btnReplDown;
        private System.Windows.Forms.Label label68;
        private System.Windows.Forms.BindingSource unapprovedMatchesBindingSource;
        private System.Windows.Forms.TabPage tpMatch;
        private System.Windows.Forms.GroupBox gbProgress;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label currentTaskDesc;
        private System.Windows.Forms.Label countProgressLabel;
        private System.Windows.Forms.DataGridView dgvParser;
        private System.Windows.Forms.BindingSource ParserBindingSource;
        private System.Windows.Forms.TabPage tpLibrary;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.TreeView mvLibraryTreeView;
        private System.Windows.Forms.SplitContainer splitContainer4;

 

        private System.Windows.Forms.DataGridView unapprovedGrid;
        private System.Windows.Forms.DataGridViewImageColumn statusColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn unapprovedLocalMediaColumn;
        private System.Windows.Forms.DataGridViewComboBoxColumn unapprovedPossibleMatchesColumn;
        private System.Windows.Forms.TabPage tbSettings;
        private Cornerstone.GUI.Controls.SettingCheckBox cbAutoApprove;
        private Cornerstone.GUI.Controls.SettingCheckBox cbUseMDAlbum;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColParseFileName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colParseArtist;
        private System.Windows.Forms.DataGridViewTextBoxColumn colParseAlbum;
        private System.Windows.Forms.DataGridViewTextBoxColumn colParseTrack;
        private System.Windows.Forms.DataGridViewTextBoxColumn colParseExt;
        private System.Windows.Forms.DataGridViewTextBoxColumn colParsePath;
        private System.Windows.Forms.DataGridViewTextBoxColumn colParseVolumeLabel;
        private System.Windows.Forms.DataGridViewTextBoxColumn fileNameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn artistDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn albumDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn trackDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn extDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn volumeLabelDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn pathDataGridViewTextBoxColumn;
        private System.Windows.Forms.TabControl tcMusicVideo;
        private System.Windows.Forms.TabPage tpArtist;
        private System.Windows.Forms.TabPage tpAlbum;
        private System.Windows.Forms.TabPage tpTrack;
        private System.Windows.Forms.SplitContainer scArtistList;
        private Cornerstone.GUI.Controls.DBObjectEditor artistDetailsList;
        private System.Windows.Forms.SplitContainer scAlbumDetails;
        private Cornerstone.GUI.Controls.DBObjectEditor albumDetailsList;
        private System.Windows.Forms.SplitContainer scTrackDetails;
        private Cornerstone.GUI.Controls.DBObjectEditor trackDetailsList;

        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;

        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripButton btnArtRefresh;
        private System.Windows.Forms.ToolStripSplitButton btnArtloadNew;
        private System.Windows.Forms.ToolStripMenuItem loadArtFromFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadArtFromURLToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;

        private System.Windows.Forms.PictureBox artImage;
        private System.Windows.Forms.Label lblArtResolution;
        private System.Windows.Forms.Label lblArtNum;
        private System.Windows.Forms.ProgressBar artworkProgressBar;
        private System.Windows.Forms.OpenFileDialog ArtFileDialog;
        private System.Windows.Forms.ContextMenuStrip cmLibrary;
        private System.Windows.Forms.ToolStripMenuItem sentToImporterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadArtFromMusicVideoToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton btnPlay;
        private System.Windows.Forms.ToolStripMenuItem getArtToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fromFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fromURLToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fromOnlinjeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tsmGrabFrame;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripSplitButton addSourceButton;
        private System.Windows.Forms.ToolStripMenuItem manuallyEnterMediaSourceToolStripMenuItem;
        private System.Windows.Forms.ToolStripSplitButton removeSourceButton;
        private System.Windows.Forms.ToolStripMenuItem markAsReplacedToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripButton helpButton;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripSplitButton filterSplitButton;
        private System.Windows.Forms.ToolStripMenuItem allMatchesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem processingMatchesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem unapprovedMatchesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem approvedCommitedMatchesToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton approveButton;
        private System.Windows.Forms.ToolStripButton manualAssignButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton rescanButton;
        private System.Windows.Forms.ToolStripButton splitJoinButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton ignoreButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripSplitButton settingsButton;
        private System.Windows.Forms.ToolStripMenuItem unignoreAllFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem restartImporterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem automaticMediaInfoMenuItem;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStrip coverToolStrip;
        private System.Windows.Forms.ToolStripButton btnPrevArt;
        private System.Windows.Forms.ToolStripButton btnNextArt;
        private System.Windows.Forms.ToolStripButton btnArtDelete;
        private System.Windows.Forms.ToolStripButton btnArtZoom;
        private System.Windows.Forms.Button btnShowFileDetails;
        private Cornerstone.GUI.Controls.DBObjectEditor fileDetailsList;
        private System.Windows.Forms.ToolStripMenuItem tsmRemove;
        private System.Windows.Forms.ImageList imageListDrag;
        private System.Windows.Forms.ImageList imageListTreeView;
        private Cornerstone.GUI.Controls.SettingCheckBox cbSplitDVD;
        private System.Windows.Forms.ToolStripMenuItem tsmGetInfo;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private Cornerstone.GUI.Controls.SettingsTextBox tbLatestVideos;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label4;
        private Cornerstone.GUI.Controls.SettingsTextBox tbHomeScreen;
        private System.Windows.Forms.Label label5;
        private Cornerstone.GUI.Controls.SettingsTextBox tbMinArtHeight;
        private Cornerstone.GUI.Controls.SettingsTextBox tbMinArtWidth;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label16;
        private Cornerstone.GUI.Controls.SettingsTextBox tbTrackArtHeight;
        private Cornerstone.GUI.Controls.SettingsTextBox tbTrackArtWidth;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.ToolStripMenuItem autoGrabFrame30SecsToolStripMenuItem;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Label label7;
        private AutoDataSourcesPanel autoDataSourcesPanel1;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.ToolStripMenuItem asVideoThumnailToolStripMenuItem;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private Cornerstone.GUI.Controls.SettingsTextBox tbVideoPreviewCols;
        private Cornerstone.GUI.Controls.SettingsTextBox tbVideoPreviewRows;
        private Cornerstone.GUI.Controls.SettingCheckBox cbPreferThumbnail;
        private System.Windows.Forms.Label label30;
    }
}