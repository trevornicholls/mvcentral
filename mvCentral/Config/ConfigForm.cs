﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Cornerstone.Database;
using Cornerstone.Database.Tables;
using Cornerstone.GUI.Dialogs;
using Cornerstone.Tools;
using Cornerstone.Extensions;

using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using NLog;
//using SQLite.NET;

using mvCentral.Database;
using mvCentral.Properties;
using mvCentral.SignatureBuilders;
using mvCentral.DataProviders;
using mvCentral.ConfigScreen.Popups;
using mvCentral.LocalMediaManagement;
using mvCentral.LocalMediaManagement.MusicVideoResources;
using mvCentral.Utils;
using mvCentral.Extractors;
using System.Threading;

namespace mvCentral
{
  [PluginIcons("mvCentral.Config.Images.mvCentral_Icon_Enabled.png", "mvCentral.Config.Images.mvCentral_Icon_Disabled.png")]
  public partial class ConfigForm : Form, ISetupForm
  {
    #region Declarations

    // Create logger
    private static Logger logger = LogManager.GetCurrentClassLogger();
    // Auto Expand Node
    TreeNode lastDragDestination = null;
    DateTime lastDragDestinationTime;
    // Node being dragged
    private TreeNode dragNode = null;
    private bool _dragStarted = false;
    // Temporary drop node for selection
    private TreeNode tempDropNode = null;
    // Timer for scrolling
    private System.Windows.Forms.Timer lvtimer = new System.Windows.Forms.Timer();
    private bool splitMode;
    private int lastSplitJoinLocation;
    private bool clearSelection;
    private readonly object lockList = new object();
    private delegate void InvokeDelegate();
    loadingDisplay load = null;
    // parser  
    Stack parsedStack = new Stack();
    public string extensions;
    delegate void SetDGVCallback(string shortFilename, string Artist, string Title, string longFilename);
    delegate void ClearDGVCallback();
    //import tab
    private List<DBImportPath> paths = new List<DBImportPath>();
    private BindingSource pathBindingSource;
    private Color normalColor;
    private Bitmap blank = new Bitmap(1, 1);

    #endregion

    #region Public Methods

    /// <summary>
    /// Retrun Current Artist
    /// </summary>
    public DBArtistInfo CurrentArtist
    {
      get
      {
        if (mvLibraryTreeView.Nodes.Count == 0 || mvLibraryTreeView.SelectedNode == null || mvLibraryTreeView.SelectedNode.Level != 0)
          return null;

        return (DBArtistInfo)mvLibraryTreeView.SelectedNode.Tag;
      }
    }
    /// <summary>
    /// Return Current Album
    /// </summary>
    public DBAlbumInfo CurrentAlbum
    {
      get
      {
        if (mvLibraryTreeView.Nodes.Count == 0 || mvLibraryTreeView.SelectedNode == null || mvLibraryTreeView.SelectedNode.Level != 1)
          return null;

        return (DBAlbumInfo)mvLibraryTreeView.SelectedNode.Tag;
      }
    }
    /// <summary>
    /// Return Current Track
    /// </summary>
    public DBTrackInfo CurrentTrack
    {
      get
      {
        if (mvLibraryTreeView.Nodes.Count == 0 || mvLibraryTreeView.SelectedNode == null || mvLibraryTreeView.SelectedNode.Level == 0)
          return null;

        return (DBTrackInfo)mvLibraryTreeView.SelectedNode.Tag;
      }
    }

    /// <summary>
    /// Construtor
    /// </summary>
    public ConfigForm()
    {
      InitializeComponent();
      lvtimer.Interval = 200;
      lvtimer.Tick += new EventHandler(lvtimer_Tick);
      // if we are in designer, break to prevent errors with rendering, it cant access the DB...
      if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
        return;
      // Start om Import tab
      mainTab.SelectedIndex = 1;
      // Grab settings from DB
      tbHomeScreen.Setting = mvCentralCore.Settings["home_name"];
      cbUseMDAlbum.Setting = mvCentralCore.Settings["use_md_album"];
      cbAutoApprove.Setting = mvCentralCore.Settings["auto_approve"];
      cbSplitDVD.Setting = mvCentralCore.Settings["importer_split_dvd"];
      tbLatestVideos.Setting = mvCentralCore.Settings["oldAfter_days"];
      tbMinArtWidth.Setting = mvCentralCore.Settings["min_artist_width"];
      tbMinArtHeight.Setting = mvCentralCore.Settings["min_artist_height"];
      tbTrackArtWidth.Setting = mvCentralCore.Settings["min_track_width"];
      tbTrackArtHeight.Setting = mvCentralCore.Settings["min_track_height"];
      // Set up display tables
      artistDetailsList.FieldDisplaySettings.Table = typeof(mvCentral.Database.DBArtistInfo);
      albumDetailsList.FieldDisplaySettings.Table = typeof(mvCentral.Database.DBAlbumInfo);
      trackDetailsList.FieldDisplaySettings.Table = typeof(mvCentral.Database.DBTrackInfo);
      fileDetailsList.FieldDisplaySettings.Table = typeof(mvCentral.Database.DBLocalMedia);
    }

    #endregion

    #region ISetupForm Members

    public string PluginName()
    {
      return mvCentralCore.Settings.HomeScreenName;
    }

    public string Description()
    {
      return "A music video plugin for MediaPortal";
    }

    public string Author()
    {
      return "Gup/Trevor";
    }

    public void ShowPlugin()
    {

      try
      {
        this.ShowDialog();
      }
      catch (Exception e)
      {
        MessageBox.Show("There was an unexpected error in the Music Videos Configuration screen!", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        logger.FatalException("Unexpected error from the Configuration Screen!", e);
        return;
      }

    }

    public bool CanEnable()
    {
      return true;
    }

    public int GetWindowId()
    {
      return mvCentralCore.PluginID;
    }

    public bool DefaultEnabled()
    {
      return true;
    }

    public bool HasSetup()
    {
      return true;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage,
      out string strButtonImageFocus, out string strPictureImage)
    {
      strButtonText = PluginName();
      strButtonImage = string.Empty;
      strButtonImageFocus = string.Empty;
      strPictureImage = "hover_MusicVids.png";
      return true;
    }

    #endregion

    #region listeners

    private void progressListener(int percentDone, int taskCount, int taskTotal, string taskDescription)
    {
      // This ensures we are thread safe. Makes sure this method is run by
      // the thread that created this panel.
      if (InvokeRequired)
      {
        Invoke(new MusicVideoImporter.ImportProgressHandler(progressListener), new object[] { percentDone, taskCount, taskTotal, taskDescription });
        return;
      }

      // set visibility of progressbar and info labels
      gbProgress.Visible = true;
      currentTaskDesc.Visible = true;
      countProgressLabel.Visible = true;

      // set the values of the progress bar and info labels
      progressBar.Value = percentDone;
      currentTaskDesc.Text = taskDescription;
      countProgressLabel.Text = "(" + taskCount + "/" + taskTotal + ")";

      // if finished hide the progress info components

      if (percentDone == 100)
      {
        //               gbProgress.Visible = false;

        //               currentTaskDesc.Visible = false;
        //               countProgressLabel.Visible = false;
        //               progressBar.Value = 0;
      }
    }

    private void mvStatusChangedListener(MusicVideoMatch obj, MusicVideoImporterAction action)
    {
      // This ensures we are thread safe. Makes sure this method is run by
      // the thread that created this panel.
      if (InvokeRequired)
      {
        Delegate method = new MusicVideoImporter.MusicVideoStatusChangedHandler(mvStatusChangedListener);
        object[] parameters = new object[] { obj, action };
        this.Invoke(method, parameters);
        return;
      }

      if (action == MusicVideoImporterAction.STARTED)
        return;

      if (action == MusicVideoImporterAction.COMMITED)
      {
        ReloadList();
      }

      if (action == MusicVideoImporterAction.MANUAL)
      {
        ReloadList();
      }

      if (action == MusicVideoImporterAction.STOPPED)
      {
        unapprovedMatchesBindingSource.Clear();
        return;
      }

      if (action == MusicVideoImporterAction.REMOVED_FROM_SPLIT ||
          action == MusicVideoImporterAction.REMOVED_FROM_JOIN)
      {

        lastSplitJoinLocation = unapprovedMatchesBindingSource.IndexOf(obj);
        unapprovedMatchesBindingSource.Remove(obj);
        clearSelection = true;
        return;
      }


      // add the match if necessary and grab the row number
      int rowNum;
      if (action == MusicVideoImporterAction.ADDED)
        rowNum = unapprovedMatchesBindingSource.Add(obj);
      else if (action == MusicVideoImporterAction.ADDED_FROM_SPLIT ||
               action == MusicVideoImporterAction.ADDED_FROM_JOIN)
      {

        unapprovedMatchesBindingSource.Insert(lastSplitJoinLocation, obj);
        if (clearSelection)
        {
          unapprovedGrid.ClearSelection();
          clearSelection = false;
        }
        unapprovedGrid.Rows[lastSplitJoinLocation].Selected = true;

        rowNum = lastSplitJoinLocation;
        lastSplitJoinLocation++;
      }
      else
        rowNum = unapprovedMatchesBindingSource.IndexOf(obj);

      if (action == MusicVideoImporterAction.PARSERDONE)
      {
        mvCentralCore.Importer.StopParse();
        return;
      }

      if (action == MusicVideoImporterAction.PARSER)
      {
        //                TestParsingFillList();
        //                lvParsingResults.ListViewItemSorter = parseResult.Comparer;
        //                lvParsingResults.Sort();
        parseResult p = mvCentralCore.Importer.ParseResult[dgvParser.RowCount];
        if (p != null)
        {
          rowNum = ParserBindingSource.Add(p);
          //                dgvParser.CurrentCell. .Style.BackColor item.UseItemStyleForSubItems = false;
          if (p.failedArtist)
            dgvParser.Rows[rowNum].Cells["colParseArtist"].Style.BackColor = System.Drawing.Color.Tomato;
          if (p.failedTrack)
            dgvParser.Rows[rowNum].Cells["colParseTrack"].Style.BackColor = System.Drawing.Color.Tomato;
        }
        return;
      }


      // setup tooltip for filename
      DataGridViewTextBoxCell filenameCell = (DataGridViewTextBoxCell)unapprovedGrid.Rows[rowNum].Cells["unapprovedLocalMediaColumn"];
      filenameCell.ToolTipText = obj.LongLocalMediaString;

      // setup the combo box of possible matches
      DataGridViewComboBoxCell movieListCombo = (DataGridViewComboBoxCell)unapprovedGrid.Rows[rowNum].Cells["unapprovedPossibleMatchesColumn"];
      movieListCombo.Items.Clear();
      foreach (PossibleMatch currMatch in obj.PossibleMatches)
        movieListCombo.Items.Add(currMatch);

      // set the status icon
      DataGridViewImageCell imageCell = (DataGridViewImageCell)unapprovedGrid.Rows[rowNum].Cells["statusColumn"];
      switch (action)
      {
        case MusicVideoImporterAction.ADDED:
        case MusicVideoImporterAction.ADDED_FROM_SPLIT:
        case MusicVideoImporterAction.ADDED_FROM_JOIN:
          imageCell.Value = blank;
          break;
        case MusicVideoImporterAction.PENDING:
          imageCell.Value = Resources.arrow_rotate_clockwise;
          break;
        case MusicVideoImporterAction.GETTING_MATCHES:
          imageCell.Value = Resources.arrow_down_small;
          break;
        case MusicVideoImporterAction.NEED_INPUT:
          imageCell.Value = Resources.Information;
          break;
        case MusicVideoImporterAction.APPROVED:
          imageCell.Value = Resources.approved;
          break;
        case MusicVideoImporterAction.GETTING_DETAILS:
          imageCell.Value = Resources.approved;
          break;
        case MusicVideoImporterAction.COMMITED:
          imageCell.Value = Resources.accept;
          break;
        case MusicVideoImporterAction.IGNORED:
          imageCell.Value = Resources.ignored;
          break;
        case MusicVideoImporterAction.MANUAL:
          imageCell.Value = Resources.accept; // @TODO change icon
          break;
      }
      tcImport.SelectedTab = tpMatch;

      unapprovedGrid.Focus();
      updateButtons();
    }

    #endregion

    #region Load / Save / Cancel

    private void initConfigForm()
    {
      lblProductVersion.Text = "v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
    }

    private void ConfigForm_Load(object sender, EventArgs e)
    {
      load = new loadingDisplay();


      //needs to be here otherwise nlog won't use the richtextbox for logging.
      mvCentralCore.Initialize(this.rtbLog);


      logger.Info("Load Paths, Replacements and Expressions and the Library");
      LoadPaths();
      LoadReplacements();
      LoadExpressions();
      //Load the library
      ReloadList();
      logger.Info("Compile Parsing and Replacement Expressions");
      FilenameParser.reLoadExpressions();

      load.updateStats(DBArtistInfo.GetAll().Count, DBTrackInfo.GetAll().Count);
      load.Close();


      if (!DesignMode)
      {
        mvCentralCore.Importer.Progress += new MusicVideoImporter.ImportProgressHandler(progressListener);
        mvCentralCore.Importer.MusicVideoStatusChanged += new MusicVideoImporter.MusicVideoStatusChangedHandler(mvStatusChangedListener);

        //mvCentralCore.Importer.Start();
      }
      this.unapprovedGrid.AutoGenerateColumns = false;
      this.unapprovedGrid.DataSource = this.unapprovedMatchesBindingSource;

      this.unapprovedPossibleMatchesColumn.DisplayMember = "DisplayMember";
      this.unapprovedPossibleMatchesColumn.ValueMember = "ValueMember";

      this.dgvParser.AutoGenerateColumns = false;


      dgvParser.Columns[1].HeaderCell.Style.BackColor = Color.Blue;
      dgvParser.Columns[3].HeaderCell.Style.BackColor = Color.Blue;
      dgvParser.Columns[1].HeaderCell.Style.ForeColor = Color.White;
      dgvParser.Columns[3].HeaderCell.Style.ForeColor = Color.White;


      Cursor.Current = Cursors.WaitCursor;
      try
      {
        // clears the components and stuff
        initConfigForm();
      }
      finally
      {
        Cursor.Current = Cursors.Default;
      }
    }


    private void ConfigForm_FormClosing(object sender, FormClosingEventArgs e)
    {
      try
      {
        pathsGridView.EndEdit();
        mvCentralCore.Importer.Stop();
        mvCentralCore.Shutdown();
      }
      catch (Exception ex)
      {
        logger.FatalException("Unexpected error from plug-in shutdown!", ex);
      }

    }

    private void LoadPaths()
    {
      // grab all user defined paths
      paths = DBImportPath.GetAllUserDefined();

      // get normal row color
      normalColor = pathsGridView.DefaultCellStyle.ForeColor;

      // set up the binding for the on screen control
      pathBindingSource = new BindingSource();
      pathBindingSource.DataSource = paths;
      pathBindingSource.ListChanged += new ListChangedEventHandler(pathBindingSource_ListChanged);

      // assign the bound list of paths to the control
      pathsGridView.AutoGenerateColumns = false;
      pathsGridView.DataSource = pathBindingSource;

      // link the checkbox to db settings
      importDvdCheckBox.Setting = mvCentralCore.Settings["importer_disc_enabled"];


    }

    private void LoadExpressions()
    {
      List<DBExpression> expressions = DBExpression.GetAll();


      if (dgvExpressions.Columns.Count == 0)
      {
        DataGridViewCheckBoxColumn columnEnabled = new DataGridViewCheckBoxColumn();
        columnEnabled.Name = DBExpression.cEnabled;
        columnEnabled.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
        dgvExpressions.Columns.Add(columnEnabled);

        DataGridViewComboBoxColumn columnType = new DataGridViewComboBoxColumn();
        columnType.Name = DBExpression.cType;
        columnType.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        DataGridViewComboBoxCell comboCellTemplate = new DataGridViewComboBoxCell();
        comboCellTemplate.Items.Add(DBExpression.cType_Simple);
        comboCellTemplate.Items.Add(DBExpression.cType_Regexp);
        columnType.CellTemplate = comboCellTemplate;
        dgvExpressions.Columns.Add(columnType);

        DataGridViewTextBoxColumn columnExpression = new DataGridViewTextBoxColumn();
        columnExpression.Name = DBExpression.cExpression;
        columnExpression.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        columnExpression.SortMode = DataGridViewColumnSortMode.NotSortable;
        dgvExpressions.Columns.Add(columnExpression);
      }

      // check if there were no valid expression returned
      // this shouldnt happen as the constructor should add defaults if null
      if (expressions.Count == 0)
      {
        DBExpression.AddDefaults();
        expressions = DBExpression.GetAll();
        if (expressions == null) return;
      }

      dgvExpressions.Rows.Clear();
      dgvExpressions.Rows.Add(expressions.Count);

      // load each expression into the grid
      foreach (DBExpression expression in expressions)
      {
        DataGridViewRow row = dgvExpressions.Rows[(int)expression.ID - 1];
        row.Cells[DBExpression.cEnabled].Value = expression.Enabled;
        DataGridViewComboBoxCell comboCell = new DataGridViewComboBoxCell();
        comboCell.Items.Add(DBExpression.cType_Simple);
        comboCell.Items.Add(DBExpression.cType_Regexp);
        comboCell.Value = expression.Type;
        row.Cells[DBExpression.cType] = comboCell;
        row.Cells[DBExpression.cExpression].Value = expression.Expression;
      }
    }

    private void LoadReplacements()
    {
      List<DBReplacements> replacements = DBReplacements.GetAll();

      // load them up in the datagrid

      if (dgvReplace.Columns.Count == 0)
      {
        DataGridViewCheckBoxColumn columnEnabled = new DataGridViewCheckBoxColumn();
        columnEnabled.Name = DBReplacements.cEnabled;
        columnEnabled.HeaderText = DBReplacements.PrettyFieldName(DBReplacements.cEnabled);
        columnEnabled.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
        dgvReplace.Columns.Add(columnEnabled);

        DataGridViewCheckBoxColumn columnTagEnabled = new DataGridViewCheckBoxColumn();
        columnTagEnabled.Name = DBReplacements.cTagEnabled;
        columnTagEnabled.HeaderText = DBReplacements.PrettyFieldName(DBReplacements.cTagEnabled);
        columnTagEnabled.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
        dgvReplace.Columns.Add(columnTagEnabled);

        DataGridViewCheckBoxColumn columnBefore = new DataGridViewCheckBoxColumn();
        columnBefore.Name = DBReplacements.cBefore;
        columnBefore.HeaderText = DBReplacements.PrettyFieldName(DBReplacements.cBefore);
        columnBefore.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
        dgvReplace.Columns.Add(columnBefore);

        DataGridViewCheckBoxColumn columnRegex = new DataGridViewCheckBoxColumn();
        columnRegex.Name = DBReplacements.cIsRegex;
        columnRegex.HeaderText = DBReplacements.PrettyFieldName(DBReplacements.cIsRegex);
        columnRegex.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
        dgvReplace.Columns.Add(columnRegex);

        DataGridViewTextBoxColumn columnToReplace = new DataGridViewTextBoxColumn();
        columnToReplace.Name = DBReplacements.cToReplace;
        columnToReplace.HeaderText = DBReplacements.PrettyFieldName(DBReplacements.cToReplace);
        columnToReplace.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        columnToReplace.SortMode = DataGridViewColumnSortMode.NotSortable;
        dgvReplace.Columns.Add(columnToReplace);

        DataGridViewTextBoxColumn columnWith = new DataGridViewTextBoxColumn();
        columnWith.Name = DBReplacements.cWith;
        columnWith.HeaderText = DBReplacements.PrettyFieldName(DBReplacements.cWith);
        columnWith.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        columnWith.SortMode = DataGridViewColumnSortMode.NotSortable;
        dgvReplace.Columns.Add(columnWith);
      }

      if (replacements.Count == 0)
      {
        DBReplacements.AddDefaults();
        replacements = DBReplacements.GetAll();
        if (replacements == null) return;
      }

      dgvReplace.DataSource = null;
      dgvReplace.Rows.Clear();
      dgvReplace.Rows.Add(replacements.Count);

      foreach (DBReplacements replacement in replacements)
      {
        DataGridViewRow row = dgvReplace.Rows[(int)replacement.ID - 1];
        row.Cells[DBReplacements.cEnabled].Value = replacement.Enabled;
        row.Cells[DBReplacements.cTagEnabled].Value = replacement.TagEnabled;
        row.Cells[DBReplacements.cBefore].Value = replacement.Before;
        row.Cells[DBReplacements.cToReplace].Value = replacement.ToReplace;
        row.Cells[DBReplacements.cWith].Value = replacement.With;
        row.Cells[DBReplacements.cIsRegex].Value = replacement.IsRegex;
      }
    }

    #endregion

    #region Settings  obsolete
    #endregion

    #region Expressions Handling

    private void SaveAllExpressions()
    {
      // need to save back all the rows
      DBExpression.ClearAll();

      foreach (DataGridViewRow row in dgvExpressions.Rows)
      {
        if (row.Index != dgvExpressions.NewRowIndex)
        {
          DBExpression expression = new DBExpression();
          foreach (DataGridViewCell cell in row.Cells)
          {
            if (cell.Value == null)
              return;
            if (cell.OwningColumn.Name == "enabled") { expression.Enabled = (bool)cell.Value; }
            if (cell.OwningColumn.Name == "type") { expression.Type = (string)cell.Value; }
            if (cell.OwningColumn.Name == "expression") { expression.Expression = (string)cell.Value; }
          }
          expression.Commit();
        }
      }
      FilenameParser.reLoadExpressions();
    }

    private void dgvExpressions_Leave(object sender, EventArgs e)
    {
      SaveAllExpressions();
    }

    private void btnExpUpDown_Click(object sender, EventArgs e)
    {
      int nCurrentRow = dgvExpressions.CurrentCellAddress.Y;
      int nCurrentCol = dgvExpressions.CurrentCellAddress.X;
      if (sender == btnExpUp)
      {
        if (nCurrentRow > 0)
        {
          DBExpression.Exchange(nCurrentRow + 1, nCurrentRow);
          LoadExpressions();
          dgvExpressions.CurrentCell = dgvExpressions.Rows[nCurrentRow - 1].Cells[nCurrentCol];
        }
      }

      // don't take in account the new line
      if (sender == btnExpDown)
      {
        if (nCurrentRow < dgvExpressions.Rows.Count - 2)
        {
          DBExpression.Exchange(nCurrentRow + 1, nCurrentRow + 2);
          LoadExpressions();
          dgvExpressions.CurrentCell = dgvExpressions.Rows[nCurrentRow + 1].Cells[nCurrentCol];
        }
      }
    }

    private void resetExpr_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      if (DialogResult.Yes ==
       MessageBox.Show("You are about to delete all parsing expressions, and replace" + Environment.NewLine +
           "them with the plugin's defaults." + Environment.NewLine + Environment.NewLine +
           "Any custom Expressions will be lost, would you like to proceed?", "Reset Expressions", MessageBoxButtons.YesNo))
      {
        dgvExpressions.Rows.Clear();

        DBExpression.ClearAll();
        DBExpression.AddDefaults();

        LoadExpressions();
        logger.Info("Expressions reset to defaults");
      }
    }

    private void linkImpParsingExpressions_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      OpenFileDialog fd = new OpenFileDialog();
      fd.Filter = "Exported Parsing Expressions (*.expr)|*.expr";
      if (fd.ShowDialog() == DialogResult.OK && System.IO.File.Exists(fd.FileName))
      {
        StreamReader r = new StreamReader(fd.FileName);
        DBExpression expr;

        //Dialog box to make sure they want to clear out current expressions to import new ones.
        DialogResult result = MessageBox.Show("Press Yes to delete all current parsing expressions," + Environment.NewLine +
                "and replace them with the imported file." + Environment.NewLine + Environment.NewLine +
                "Press No to append the import parsing expressions." + Environment.NewLine + Environment.NewLine +
                "Press cancel to Quit", "Import Expressions", MessageBoxButtons.YesNoCancel);
        switch (result)
        {
          case DialogResult.No:
            break;
          case DialogResult.Yes:
            dgvExpressions.Rows.Clear();
            DBExpression.ClearAll();
            logger.Info("Expressions cleared");
            break;
          case DialogResult.Cancel:
            return;
        }

        string line = string.Empty;
        string[] parts;

        // now set watched for all in file
        while ((line = r.ReadLine()) != null)
        {
          char[] c = { ';' };
          parts = line.Split(c, 3);
          if (parts.Length != 3) continue;
          expr = new DBExpression();
          try
          {
            if (Convert.ToInt32(parts[0]) == 0 || Convert.ToInt32(parts[0]) == 1) expr.Enabled = Convert.ToBoolean(parts[0].Equals("1"));
            if (parts[1] == DBExpression.cType_Regexp || parts[1] == DBExpression.cType_Simple) expr.Type = parts[1];
            expr.Expression = parts[2];
          }
          catch (Exception ex)
          {
            logger.ErrorException("error in importing : ", ex);
            r.Close();
            LoadExpressions();
            return;
          }
          expr.Commit();
        }
        r.Close();
        logger.Info("Parsing Expressions succesfully imported!");
        LoadExpressions();
      }

    }

    private void linkExParsingExpressions_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      List<DBExpression> expressions = DBExpression.GetAll();
      if (expressions == null || expressions.Count == 0)
      {
        MessageBox.Show("No vaild expressions to export!");
        return;
      }

      SaveFileDialog fd = new SaveFileDialog();
      fd.Filter = "Exported Parsing Expressions (*.expr)|*.expr";
      if (fd.ShowDialog() == DialogResult.OK)
      {
        StreamWriter w = new StreamWriter(fd.FileName);
        foreach (DBExpression expression in expressions)
        {
          String val = "";
          //val += expression[DBExpression.cIndex];
          //val += ";";
          val += Convert.ToInt16(expression.Enabled);
          val += ";";
          val += (String)expression.Type;
          val += ";";
          val += (String)expression.Expression;

          try
          {
            w.WriteLine((string)val);
          }
          catch (IOException exception)
          {
            logger.Info("Parsing Expressions NOT exported!  Error: " + exception.ToString());
            return;
          }
        }
        w.Close();
        logger.Info("Parsing Expressions succesfully exported!");
      }

    }

    #endregion

    #region Replacements Handling

    private void SaveAllReplacements()
    {
      // need to save back all the rows
      DBReplacements.ClearAll();

      foreach (DataGridViewRow row in dgvReplace.Rows)
      {
        if (row.Index != dgvReplace.NewRowIndex)
        {
          DBReplacements replacement = new DBReplacements();
          foreach (DataGridViewCell cell in row.Cells)
          {
            switch (cell.OwningColumn.Name)
            {
              case null:
                break;
              case "enabled":
                if (cell.Value == null) cell.Value = true;
                replacement.Enabled = (bool)cell.Value;
                break;
              case "isRegex":
                if (cell.Value == null) cell.Value = true;
                replacement.IsRegex = (bool)cell.Value;
                break;
              case "toreplace":
                if (cell.Value == null) cell.Value = "";
                replacement.ToReplace = (string)cell.Value;
                break;
              case "with":
                if (cell.Value == null) cell.Value = "";
                replacement.With = (string)cell.Value;
                break;
              case "before":
                if (cell.Value == null) cell.Value = true;
                replacement.Before = (bool)cell.Value;
                break;
              case "tagEnabled":
                if (cell.Value == null) cell.Value = true;
                replacement.TagEnabled = (bool)cell.Value;
                break;

            }
          }
          replacement.Commit();
        }
      }
      FilenameParser.reLoadExpressions();
    }

    /// <summary>
    /// Handle expression re-order
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnReplUpDown_Click(object sender, EventArgs e)
    {
      int nCurrentRow = dgvReplace.CurrentCellAddress.Y;
      int nCurrentCol = dgvReplace.CurrentCellAddress.X;

      if (sender == btnReplDown)
      {
        // don't take in account the new line
        if (nCurrentRow < dgvReplace.Rows.Count - 2)
        {
          DBReplacements.Exchange(nCurrentRow + 1, nCurrentRow + 2);
          LoadReplacements();
          dgvReplace.CurrentCell = dgvReplace.Rows[nCurrentRow + 1].Cells[nCurrentCol];
        }
      }
      if (sender == btnReplUp)
      {
        if (nCurrentRow > 0)
        {
          DBReplacements.Exchange(nCurrentRow + 1, nCurrentRow);
          LoadReplacements();
          dgvReplace.CurrentCell = dgvReplace.Rows[nCurrentRow - 1].Cells[nCurrentCol];
        }

      }


    }

    private void dgvReplace_Leave(object sender, EventArgs e)
    {
      SaveAllReplacements();

    }


    private void linkLabelExportStringReplacements_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      List<DBReplacements> replacements = DBReplacements.GetAll();
      if (replacements == null || replacements.Count == 0)
      {
        MessageBox.Show("No valid string replacements to export!");
        return;
      }

      SaveFileDialog fd = new SaveFileDialog();
      fd.Filter = "Exported String Replacements (*.strrep)|*.strrep";
      if (fd.ShowDialog() == DialogResult.OK)
      {
        StreamWriter w = new StreamWriter(fd.FileName);

        foreach (DBReplacements replacement in replacements)
        {
          String val = "";
          val += Convert.ToInt16(replacement.Enabled);
          val += ";";
          val += Convert.ToInt16(replacement.Before);
          val += ";";
          val += Convert.ToInt16(replacement.TagEnabled);
          val += ";";
          val += Convert.ToInt16(replacement.IsRegex);
          val += ";";
          val += (String)replacement.ToReplace;
          val += ";";
          val += (String)replacement.With;

          try
          {
            w.WriteLine((string)val);
          }
          catch (IOException exception)
          {
            logger.Info("String Replacements NOT exported!  Error: " + exception.ToString());
            return;
          }
        }
        w.Close();
        logger.Info("String Replacements succesfully exported!");
      }
    }

    private void linkLabelImportStringReplacements_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      OpenFileDialog fd = new OpenFileDialog();
      fd.Filter = "Exported String Replacements (*.strrep)|*.strrep";
      if (fd.ShowDialog() == DialogResult.OK && System.IO.File.Exists(fd.FileName))
      {
        StreamReader r = new StreamReader(fd.FileName);
        //Dialog box to make sure they want to clear out current replacements to import new ones.
        DialogResult result = MessageBox.Show("Press Yes to delete all current string replacements," + Environment.NewLine +
                "and replace them with the imported file." + Environment.NewLine + Environment.NewLine +
                "Press No to append the import string replacements." + Environment.NewLine + Environment.NewLine +
                "Press cancel to Quit", "Import Replacements", MessageBoxButtons.YesNoCancel);
        switch (result)
        {
          case DialogResult.No:
            break;
          case DialogResult.Yes:
            dgvReplace.Rows.Clear();
            DBReplacements.ClearAll();
            logger.Info("Replacements cleared");
            break;
          case DialogResult.Cancel:
            return;
        }

        string line = string.Empty;
        string[] parts;

        // read file and import into database
        while ((line = r.ReadLine()) != null)
        {
          char[] c = { ';' };
          parts = line.Split(c, 6);

          if (parts.Length == 6)
          {
            DBReplacements repl = new DBReplacements();
            try
            {
              if (Convert.ToInt32(parts[0]) == 0 || Convert.ToInt32(parts[0]) == 1) repl.Enabled = Convert.ToBoolean(parts[0].Equals("1"));
              if (Convert.ToInt32(parts[1]) == 0 || Convert.ToInt32(parts[1]) == 1) repl.Before = Convert.ToBoolean(parts[1].Equals("1"));
              if (Convert.ToInt32(parts[2]) == 0 || Convert.ToInt32(parts[2]) == 1) repl.TagEnabled = Convert.ToBoolean(parts[2].Equals("1"));
              if (Convert.ToInt32(parts[3]) == 0 || Convert.ToInt32(parts[3]) == 1) repl.IsRegex = Convert.ToBoolean(parts[3].Equals("1"));
              repl.ToReplace = parts[4];
              repl.With = parts[5];
            }
            catch (Exception ex)
            {
              logger.ErrorException("replacement failed :", ex);
              LoadReplacements();
              r.Close();
              return;
            }
            repl.Commit();
          }
        }
        r.Close();
        logger.Info("String Replacements succesfully imported!");
        LoadReplacements();
      }
    }

    private void linkLabelResetStringReplacements_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      if (DialogResult.Yes ==
                  MessageBox.Show("You are about to delete all string replacements, and replace" + Environment.NewLine +
                                  "them with the plugin's defaults." + Environment.NewLine + Environment.NewLine +
                                  "Any custom Replacements will be lost, would you like to proceed?", "Reset Replacements", MessageBoxButtons.YesNo))
      {
        dgvReplace.Rows.Clear();

        DBReplacements.ClearAll();
        DBReplacements.AddDefaults();

        LoadReplacements();
        logger.Info("Replacements reset to defaults");
      }
    }


    #endregion

    #region Test Parsing Handling
    private void btnTestReparse_Click(object sender, EventArgs e)
    {
      ParserBindingSource.Clear();
      logger.Info("Starting Parsing test, getting all files");
      mvCentralCore.Importer.StartParse();

    }

    /*        void TestParsingFillList()
            {
    //            List<parseResult> results = mvCentralCore.Importer.ParseResult;
                lvParsingResults.SuspendLayout();
                //IComparer sorter = lvParsingResults.ListViewItemSorter;
                lvParsingResults.ListViewItemSorter = null;
                foreach (parseResult progress in mvCentralCore.Importer.ParseResult)
                {
                    foreach (KeyValuePair<String, String> MatchPair in progress.parser.Matches)
                    {
                        //workaround for bug in containskey function of visual c (property name doesn't get set using the Tag prperty instead) 
                       bool InHeaders = false;
                       foreach (ColumnHeader col in lvParsingResults.Columns)
                       {
                           if (col.Tag.ToString() == MatchPair.Key) InHeaders = true;
                       }
                       if (!InHeaders)
                       {
                                        // add a column for that match
    //                      ColumnHeader newcolumn = new ColumnHeader();
    //                      newcolumn.Name = MatchPair.Key;
    //                      newcolumn.Tag = MatchPair.Key;
    //                      newcolumn.Text = MatchPair.Key;
    //                      lvParsingResults.Columns.Add(newcolumn);
                        }
                    }

                    ColumnHeader col1 = lvParsingResults.Columns[0];
                    ListViewItem item = new ListViewItem(progress.parser.Matches[col1.Tag.ToString()]);
                    item.Tag = progress;
                    item.SubItems[0].Name = col1.Tag.ToString();

                    foreach (ColumnHeader column in lvParsingResults.Columns)
                    {
                        if (column.Index > 0)
                        {
                            ListViewItem.ListViewSubItem subItem = null;

                            if (progress.parser.Matches.ContainsKey(column.Tag.ToString()))
                            {
                                subItem = new ListViewItem.ListViewSubItem(item, progress.parser.Matches[column.Tag.ToString()]);

                                //                        else
                                //                            subItem = new ListViewItem.ListViewSubItem(item, "");
                                subItem.Name = column.Tag.ToString();
                                item.SubItems.Add(subItem);
                            }
                        }
                    }

                    // add in the full filename as a subitem of the list item. this is not used for the
                    // actual list but is needed by the manual parser that is launched from a list listener
    //                ListViewItem.ListViewSubItem fullFileName = new ListViewItem.ListViewSubItem(item, progress.full_filename);
    //                fullFileName.Name = "FullFileName";
    //                item.SubItems.Add(fullFileName);

                    if (progress.failedTrack)
                    {
                        item.UseItemStyleForSubItems = false;
                        //                   item.SubItems[DBEpisode.cSeasonIndex].ForeColor = System.Drawing.Color.White;
                        //                   item.SubItems[DBEpisode.cSeasonIndex].BackColor = System.Drawing.Color.Tomato;
                    }

                    if (progress.failedArtist)
                    {
                        item.UseItemStyleForSubItems = false;
                        //                    item.SubItems[DBEpisode.cEpisodeIndex].ForeColor = System.Drawing.Color.White;
                        //                    item.SubItems[DBEpisode.cEpisodeIndex].BackColor = System.Drawing.Color.Tomato;
                    }

                    if (!progress.success && !progress.failedArtist && !progress.failedTrack)
                    {
                        item.ForeColor = System.Drawing.Color.White;
                        item.BackColor = System.Drawing.Color.Tomato;
                    }

                    if (!progress.success)
                        logger.Info("Parsing failed for " + progress.match_filename);
                    if (progress.failedAlbum || progress.failedArtist || progress.failedTrack)
                        logger.Info(progress.exception + " for " + progress.match_filename);
                    lvParsingResults.Items.Add(item);
                }

                lvParsingResults.ListViewItemSorter = parseResult.Comparer;
                lvParsingResults.Sort();
                lvParsingResults.ResumeLayout();
                lvParsingResults.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

                foreach (ColumnHeader header in lvParsingResults.Columns)
                {
                    header.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
                    header.Width += 10;
                    if (header.Width < 80)
                        header.Width = 80;
                    if (header.Width > 200)
                        header.Width = 200;
                }

            }
            */


    #endregion

    #region Library

    /// <summary>
    /// Loads from scratch all musicvideos in the database into the side panel
    /// </summary>
    public void ReloadList()
    {
      // turn off redraws temporarily and clear the list
      mvLibraryTreeView.BeginUpdate();
      mvLibraryTreeView.Nodes.Clear();
      splitContainer3.Panel2Collapsed = true;

      lock (lockList)
      {
        foreach (DBTrackInfo currmv in DBTrackInfo.GetAll())
          addMusicVideo(currmv);
      }
      mvLibraryTreeView.EndUpdate();

      if (mvLibraryTreeView.Nodes.Count > 0)
        splitContainer3.Panel2Collapsed = false;

      mvLibraryTreeView.Sort();
      mvLibraryTreeView.SelectedNode = mvLibraryTreeView.TopNode;
    }
    /// <summary>
    /// Adds the given musicvideo and it's related files to the tree view
    /// </summary>
    /// <param name="mv"></param>
    private void addMusicVideo(DBTrackInfo mv)
    {
      TreeNode artistItem = null;
      TreeNode albumItem = null;
      bool ArtistNodeExist = true;
      bool AlbumNodeExist = true;
      foreach (TreeNode t1 in mvLibraryTreeView.Nodes)
      {
        if (t1.Level == 0)
        {
          DBArtistInfo trtag = (DBArtistInfo)t1.Tag;
          if (mv.ArtistInfo[0] == trtag)
          {
            artistItem = t1;
            break;
          }
        }
      }

      if (artistItem == null)
      {
        artistItem = new TreeNode(mv.ArtistInfo[0].Artist, 1, 1);
        mv.ArtistInfo[0].Changed -= new DBBasicInfo.ChangedEventHandler(basicInfoChanged);

        mv.ArtistInfo[0].Changed += new DBBasicInfo.ChangedEventHandler(basicInfoChanged);
        artistItem.Tag = mv.ArtistInfo[0];
        ArtistNodeExist = false;
      }

      if (mv.AlbumInfo.Count > 0)
      {
        foreach (TreeNode t1 in mvLibraryTreeView.Nodes)
          foreach (TreeNode t2 in t1.Nodes)
          {
            if (t2.Level == 1)
            {
              if (t2.Tag.GetType() == typeof(DBAlbumInfo))
              {
                DBAlbumInfo trtag = (DBAlbumInfo)t2.Tag;

                if (mv.AlbumInfo[0] == trtag)
                {
                  albumItem = t2;
                  break;
                }
              }
            }

          }

        if (albumItem == null)
        {
          AlbumNodeExist = false;
          albumItem = new TreeNode(mv.AlbumInfo[0].Album, 2, 2);
          albumItem.Tag = mv.AlbumInfo[0];
          mv.AlbumInfo[0].Changed -= new DBBasicInfo.ChangedEventHandler(basicInfoChanged);

          mv.AlbumInfo[0].Changed += new DBBasicInfo.ChangedEventHandler(basicInfoChanged);
        }
      }
      else AlbumNodeExist = false;
      TreeNode trackItem = new TreeNode(mv.Track, 3, 3);
      mv.Changed -= new DBBasicInfo.ChangedEventHandler(basicInfoChanged);

      mv.Changed += new DBBasicInfo.ChangedEventHandler(basicInfoChanged);

      trackItem.Tag = mv;


      if (albumItem != null)
        albumItem.Nodes.Add(trackItem);
      else albumItem = trackItem;


      if (!AlbumNodeExist) artistItem.Nodes.Add(albumItem);

      if (!ArtistNodeExist) mvLibraryTreeView.Nodes.Add(artistItem);


      // if the movie is offline color it red
      if (mv.LocalMedia.Count > 0)
      {
        if (!mv.LocalMedia[0].IsAvailable)
        {
          trackItem.ForeColor = Color.Red;
          trackItem.ToolTipText = "This musicvideo is currently offline.";
        }
      }
    }


    private void tvLibrary_ItemDrag(object sender, System.Windows.Forms.ItemDragEventArgs e)
    {

      // Get drag node and select it
      this.dragNode = (TreeNode)e.Item;
      this.mvLibraryTreeView.SelectedNode = this.dragNode;
      //            if (this.dragNode.Tag.GetType() == typeof(DBArtistInfo)) return;
      // Reset image list used for drag image
      this.imageListDrag.Images.Clear();
      int maximagesizeWidth = this.dragNode.Bounds.Size.Width + this.mvLibraryTreeView.Indent;
      if (maximagesizeWidth > 256) maximagesizeWidth = 256;
      this.imageListDrag.ImageSize = new Size(maximagesizeWidth, this.dragNode.Bounds.Height);

      // Create new bitmap
      // This bitmap will contain the tree node image to be dragged
      Bitmap bmp = new Bitmap(maximagesizeWidth, this.dragNode.Bounds.Height);

      // Get graphics from bitmap
      Graphics gfx = Graphics.FromImage(bmp);

      // Draw node icon into the bitmap
      gfx.DrawImage(this.imageListTreeView.Images[this.dragNode.ImageIndex], 0, 0);

      // Draw node label into bitmap
      gfx.DrawString(this.dragNode.Text,
          this.mvLibraryTreeView.Font,
          new SolidBrush(this.mvLibraryTreeView.ForeColor),
          (float)this.mvLibraryTreeView.Indent, 1.0f);

      // Add bitmap to imagelist
      this.imageListDrag.Images.Add(bmp);

      // Get mouse position in client coordinates
      Point p = this.mvLibraryTreeView.PointToClient(Control.MousePosition);

      // Compute delta between mouse position and node bounds
      int dx = p.X + this.mvLibraryTreeView.Indent - this.dragNode.Bounds.Left;
      int dy = p.Y - this.dragNode.Bounds.Top;

      // Begin dragging image
      if (DragHelper.ImageList_BeginDrag(this.imageListDrag.Handle, 0, dx, dy))
      {
        // Begin dragging
        this.mvLibraryTreeView.DoDragDrop(bmp, DragDropEffects.Move);
        // End dragging image
        DragHelper.ImageList_EndDrag();
      }

    }

    private void tvLibrary_DragOver(object sender, System.Windows.Forms.DragEventArgs e)
    {

      // Compute drag position and move image
      Point formP = this.PointToClient(new Point(e.X, e.Y));
      DragHelper.ImageList_DragMove(formP.X - this.mvLibraryTreeView.Left, formP.Y - this.mvLibraryTreeView.Top);

      // Get actual drop node
      TreeNode dropNode = this.mvLibraryTreeView.GetNodeAt(this.mvLibraryTreeView.PointToClient(new Point(e.X, e.Y)));
      if (dropNode == null || this.dragNode == null)
      {
        e.Effect = DragDropEffects.None;
        return;
      }

      e.Effect = DragDropEffects.Move;

      // if mouse is on a new node select it
      if (this.tempDropNode != dropNode)
      {
        DragHelper.ImageList_DragShowNolock(false);
        this.mvLibraryTreeView.SelectedNode = dropNode;
        this.mvLibraryTreeView.Focus();
        DragHelper.ImageList_DragShowNolock(true);
        tempDropNode = dropNode;
      }

      // Avoid that drop node is child of drag node 
      TreeNode tmpNode = dropNode;
      if (this.dragNode.Parent == dropNode) e.Effect = DragDropEffects.None;

      if (dropNode.Tag.GetType() == typeof(DBTrackInfo)) e.Effect = DragDropEffects.None;
      if (dragNode.Tag.GetType() == typeof(DBArtistInfo)) e.Effect = DragDropEffects.None;
      if (this.dragNode.Tag.GetType() == dropNode.Tag.GetType()) e.Effect = DragDropEffects.None;

      while (tmpNode.Parent != null)
      {

        if (tmpNode == this.dragNode) e.Effect = DragDropEffects.None;
        tmpNode = tmpNode.Parent;
      }

      //if we are on a new object, reset our timer
      //otherwise check to see if enough time has passed and expand the destination node
      if (dropNode != lastDragDestination)
      {
        lastDragDestination = dropNode;
        lastDragDestinationTime = DateTime.Now;
      }
      else
      {
        TimeSpan hoverTime = DateTime.Now.Subtract(lastDragDestinationTime);
        if (hoverTime.TotalSeconds > 1)
        {
          dropNode.Expand();
        }

      }
    }

    private void tvLibrary_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
    {
      // Unlock updates
      DragHelper.ImageList_DragLeave(this.mvLibraryTreeView.Handle);
      // Get drop node
      TreeNode dropNode = this.mvLibraryTreeView.GetNodeAt(this.mvLibraryTreeView.PointToClient(new Point(e.X, e.Y)));

      // If drop node isn't equal to drag node, add drag node as child of drop node
      if (this.dragNode != dropNode)
      {
        // Remove drag node from parent
        if (this.dragNode.Parent == null)
        {
          this.mvLibraryTreeView.Nodes.Remove(this.dragNode);
        }
        else
        {
          this.dragNode.Parent.Nodes.Remove(this.dragNode);
        }

        // Add drag node to drop node
        dropNode.Nodes.Add(this.dragNode);
        dropNode.ExpandAll();
        UpdateDB(this.dragNode);
        // Set drag node to null
        this.dragNode = null;

        // remove orphaned nodes
        foreach (TreeNode t1 in mvLibraryTreeView.Nodes)
        {
          foreach (TreeNode t2 in t1.Nodes)
          {
            if (t2.Tag.GetType() == typeof(DBTrackInfo)) continue;
            if (t2.FirstNode == null)
            {
              this.mvLibraryTreeView.Nodes.Remove(t2);
            }
          }
          if (t1.FirstNode == null)
          {
            this.mvLibraryTreeView.Nodes.Remove(t1);
          }

        }
        _dragStarted = false;

        // Disable scroll timer
        this.lvtimer.Enabled = false;
      }
    }


    private void processNode(TreeNode node)
    {
      foreach (TreeNode subnode in node.Nodes)
      {
        processNode(subnode);
        if (subnode.Tag.GetType() == typeof(DBAlbumInfo))
        {
          if (subnode.Nodes.Count == 0) this.mvLibraryTreeView.Nodes.Remove(subnode);
        }

      }
    }

    private void tvLibrary_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
    {
      DragHelper.ImageList_DragEnter(this.mvLibraryTreeView.Handle, e.X - this.mvLibraryTreeView.Left,
          e.Y - this.mvLibraryTreeView.Top);

      // Enable timer for scrolling dragged item
      _dragStarted = true;
      this.lvtimer.Enabled = true;
    }

    private void tvLibrary_DragLeave(object sender, System.EventArgs e)
    {
      DragHelper.ImageList_DragLeave(this.mvLibraryTreeView.Handle);
      _dragStarted = false;
      // Disable timer for scrolling dragged item
      this.lvtimer.Enabled = false;
      TreeNode tr = mvLibraryTreeView.SelectedNode;
      mvLibraryTreeView.SelectedNode = null;
      mvLibraryTreeView.SelectedNode = tr;

    }

    private void tvLibrary_GiveFeedback(object sender, System.Windows.Forms.GiveFeedbackEventArgs e)
    {
      if (e.Effect == DragDropEffects.Move)
      {
        // Show pointer cursor while dragging
        e.UseDefaultCursors = false;
        this.mvLibraryTreeView.Cursor = Cursors.Default;
      }
      else e.UseDefaultCursors = true;

    }

    private void lvtimer_Tick(object sender, EventArgs e)
    {
      // get node at mouse position
      Point pt = mvLibraryTreeView.PointToClient(Control.MousePosition);
      TreeNode node = this.mvLibraryTreeView.GetNodeAt(pt);

      if (node == null) return;

      // if mouse is near to the top, scroll up
      if (pt.Y < 30)
      {
        // set actual node to the upper one
        if (node.PrevVisibleNode != null)
        {
          node = node.PrevVisibleNode;

          // hide drag image
          DragHelper.ImageList_DragShowNolock(false);
          // scroll and refresh
          node.EnsureVisible();
          this.mvLibraryTreeView.Refresh();
          // show drag image
          DragHelper.ImageList_DragShowNolock(true);

        }
      }
      // if mouse is near to the bottom, scroll down
      else if (pt.Y > this.mvLibraryTreeView.Size.Height - 30)
      {
        if (node.NextVisibleNode != null)
        {
          node = node.NextVisibleNode;

          DragHelper.ImageList_DragShowNolock(false);
          node.EnsureVisible();
          this.mvLibraryTreeView.Refresh();
          DragHelper.ImageList_DragShowNolock(true);
        }
      }
    }

    private void UpdateDB(TreeNode t)
    {
      if (t.Tag.GetType() == typeof(DBTrackInfo))
      {
        DBTrackInfo d1 = (DBTrackInfo)t.Tag;
        DBArtistInfo a1 = null;
        DBAlbumInfo a2 = null;
        if (t.Parent.Parent != null)
          if ((DBArtistInfo)t.Parent.Parent.Tag != null)
          {
            a1 = (DBArtistInfo)t.Parent.Parent.Tag;
          }
          else
          {
            a2 = (DBAlbumInfo)t.Parent.Tag;

          }

        if (a1 == null)
        {
          a1 = (DBArtistInfo)t.Parent.Tag;
        }


        d1.ArtistInfo.Clear();
        d1.AlbumInfo.Clear();
        d1.ArtistInfo.Add(a1);
        if (a2 != null) d1.AlbumInfo.Add(a2);
        d1.Commit();
      }



      if (t.Tag.GetType() == typeof(DBAlbumInfo))
      {
        DBAlbumInfo a3 = (DBAlbumInfo)t.Tag;
        DBArtistInfo a1 = (DBArtistInfo)t.Parent.Tag;
        foreach (TreeNode t1 in t.Nodes)
        {
          ((DBTrackInfo)t1.Tag).ArtistInfo.Clear();
          ((DBTrackInfo)t1.Tag).ArtistInfo.Add(a1);
          ((DBTrackInfo)t1.Tag).Commit();
        }
      }

      foreach (DBArtistInfo a3 in DBArtistInfo.GetAll())
      {
        List<DBTrackInfo> a7 = DBTrackInfo.GetEntriesByArtist(a3);
        if (a7.Count == 0)
        {
          a3.Delete();
          a3.Commit();
          break;
        }
      }

      foreach (DBAlbumInfo a3 in DBAlbumInfo.GetAll())
      {
        List<DBTrackInfo> a7 = DBTrackInfo.GetEntriesByAlbum(a3);
        if (a7.Count == 0)
        {
          a3.Delete();
          a3.Commit();
          break;
        }
      }
    }



    private void basicInfoChanged(object sender, EventArgs e)
    {
      // This ensures we are thread safe. Makes sure this method is run by
      // the thread that created this panel.

      if (InvokeRequired)
      {
        Invoke(new DBBasicInfo.ChangedEventHandler(basicInfoChanged), new object[] { sender, e });
        return;
      }
      mvLibraryTreeView.SelectedNode.Text = (sender as DBBasicInfo).Basic;
    }

    #endregion

    #region Mainpanel

    public void reLoad()
    {
      //Reload the main window to show contents of DB
      //            rs = dm.getAll();
      Stack<string> artistStack = new Stack<string>();
      //            listArtists.Items.Clear();
      //            foreach (SQLiteResultSet.Row row in rs.Rows)
      {
        //                Artist artist = new Artist(row.fields[0], row.fields[1], row.fields[2], row.fields[3]);
        //                if (!(artistStack.Contains(row.fields[0])))
        {
          //                    listArtists.Items.Add(artist);
          //                    artistStack.Push(row.fields[0]);
        }
      }
      try
      {
        //                listArtists.SelectedIndex = 0;
      }
      catch
      {

      }
    }

    private void btnClearRescan_Click(object sender, EventArgs e)
    {
      try
      {
        System.IO.Directory.Delete(MediaPortal.Configuration.Config.GetFile(MediaPortal.Configuration.Config.Dir.Thumbs, "Music Vids\\Thumbs\\"), true);
        System.IO.Directory.Delete(MediaPortal.Configuration.Config.GetFile(MediaPortal.Configuration.Config.Dir.Thumbs, "Music Vids\\Artists\\"), true);
      }
      catch { }
      //            dm.Execute("DELETE FROM Artists");
      //            dm.Execute("DELETE FROM Videos");

      //            Rescan rs = new Rescan();
      //            if (rs.ShowDialog() == DialogResult.OK)
      //                reLoad();

      //            doScan();
    }
    /// <summary>
    /// Generate new image dimensions
    /// </summary>
    /// <param name="currW"></param>
    /// <param name="currH"></param>
    /// <param name="destW"></param>
    /// <param name="destH"></param>
    /// <returns></returns>
    public Size GenerateImageDimensions(int currW, int currH, int destW, int destH)
    {
      double multiplier = 0;
      string layout;
      if (currH > currW) layout = "portrait";
      else layout = "landscape";

      switch (layout.ToLower())
      {
        case "portrait":
          if (destH > destW)
            multiplier = (double)destW / (double)currW;
          else
            multiplier = (double)destH / (double)currH;
          break;
        case "landscape":
          if (destH > destW)
            multiplier = (double)destW / (double)currW;
          else
            multiplier = (double)destH / (double)currH;
          break;
      }
      return new Size((int)(currW * multiplier), (int)(currH * multiplier));
    }
    /// <summary>
    /// Set the image in the picturebox
    /// </summary>
    /// <param name="pb"></param>
    private void SetImage(PictureBox pb)
    {
      try
      {
        Image img = pb.Image;
        Size imgSize = GenerateImageDimensions(img.Width, img.Height, pb.Width, pb.Height);
        Bitmap finalImg = new Bitmap(img, imgSize.Width, imgSize.Height);
        Graphics gfx = Graphics.FromImage(img);
        gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
        pb.Image = null;
        pb.SizeMode = PictureBoxSizeMode.CenterImage;
        pb.Image = finalImg;
      }
      catch (System.Exception e)
      {
        logger.ErrorException("Error in setImage function", e);
      }
    }


    #endregion

    #region About
    private void LabelFFMpeg_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start(@"http://www.ffmpeg.org/");
    }

    private void labelGoogleCode_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start(@"http://code.google.com/p/mvcentral/");
    }

    private void labelManual_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start(@"http://code.google.com/p/mvcentral/");
    }

    private void labelForum_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start(@"http://forum.team-mediaportal.com/mediaportal-plugins-47/my-music-videos-v0-41-a-68337/");
    }

    #endregion

    #region Import tab
    // Commits new and existing itmes on addition or modification.
    void pathBindingSource_ListChanged(object sender, ListChangedEventArgs e)
    {
      if (e.ListChangedType != ListChangedType.ItemDeleted)
      {
        DBImportPath changedObj = (DBImportPath)pathBindingSource[e.NewIndex];
        changedObj.Commit();
      }
    }

    private void addSourceButton_Click(object sender, EventArgs e)
    {
      FolderBrowserDialog folderDialog = new FolderBrowserDialog();
      DialogResult result = folderDialog.ShowDialog();
      if (result == DialogResult.OK)
      {
        DBImportPath newPath = DBImportPath.Get(folderDialog.SelectedPath);

        if (newPath.GetDriveType() == DriveType.CDRom)
        {
          MessageBox.Show("Importing from this drive is controlled through the setting 'Enable Import Paths For Optical Drives'", "Not Allowed!");
          return;
        }

        if (newPath.ID != null)
        {
          MessageBox.Show("This import path is already loaded.");
          return;
        }

        if (newPath.InternallyManaged != true)
        {
          pathBindingSource.Add(newPath);
          mvCentralCore.Importer.RestartScanner();
        }

      }
    }
    private void removeSourceButton_ButtonClick(object sender, EventArgs e)
    {
      if (pathBindingSource.Current != null)
      {
        DialogResult result = MessageBox.Show("This will remove from Music Videos all music videos retrieved from this import path, are you sure?", "Warning!", MessageBoxButtons.YesNo);
        if (result == DialogResult.Yes)
        {
          // stop the importer
          mvCentralCore.Importer.Stop(); ;

          // remove the import path
          ((DBImportPath)pathBindingSource.Current).Delete();
          pathBindingSource.RemoveCurrent();

          // clean the database of the old music videos using our progress bar popup
          ProgressPopup progressPopup = new ProgressPopup(new WorkerDelegate(DatabaseMaintenanceManager.RemoveInvalidFiles));
          DatabaseMaintenanceManager.MaintenanceProgress += new ProgressDelegate(progressPopup.Progress);
          progressPopup.Owner = ParentForm;
          progressPopup.Text = "Removing related music videos...";
          progressPopup.ShowDialog();

          // restart the importer
          mvCentralCore.Importer.RestartScanner();

        }
      }

    }

    private void helpButton_Click(object sender, EventArgs e)
    {
      ProcessStartInfo processInfo = new ProcessStartInfo(Resources.MediaSourcesHelpURL);
      Process.Start(processInfo);

    }

    private void manuallyEnterMediaSourceToolStripMenuItem_Click(object sender, EventArgs e)
    {
      AddImportPathPopup addPopup = new AddImportPathPopup();
      addPopup.Owner = ParentForm;
      DialogResult result = addPopup.ShowDialog();
      if (result == DialogResult.OK)
      {
        DBImportPath newPath;
        try
        {
          newPath = DBImportPath.Get(addPopup.SelectedPath);
        }
        catch (Exception ex)
        {
          if (ex is ThreadAbortException)
            throw ex;

          MessageBox.Show("The path that you have entered is invalid.");
          return;
        }

        if (newPath.Directory == null || !newPath.Directory.Exists)
        {
          MessageBox.Show("The path that you have entered is invalid.");
          return;
        }

        if (newPath.GetDriveType() == DriveType.CDRom)
        {
          MessageBox.Show("Importing from this drive is controlled through the setting 'Enable Import Paths For Optical Drives'", "Not Allowed!");
          return;
        }

        if (newPath.ID != null)
        {
          MessageBox.Show("This import path is already loaded.");
          return;
        }

        if (newPath.InternallyManaged != true)
        {
          pathBindingSource.Add(newPath);
          mvCentralCore.Importer.RestartScanner();
        }

      }

    }

    private void markAsReplacedToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (pathBindingSource.Current != null)
      {
        DBImportPath importPath = pathBindingSource.Current as DBImportPath;

        string message = "This will mark the import path as replaced. You can use this option to recover from a hardware replacement and are unable to recreate the same import path again. mvCentral that were previously on this import path will be moved to a new import path once they are detected during a scan. Continue?";
        if (importPath.Replaced)
        {
          message = "This wil remove the replaced flag and return the import path back to normal. Continue?";
        }

        DialogResult result = MessageBox.Show(message, "Warning!", MessageBoxButtons.YesNo);
        if (result == DialogResult.Yes)
        {
          // stop the importer
          mvCentralCore.Importer.Stop(); ;

          // mark as replaced
          importPath.Replaced = !(importPath.Replaced);
          importPath.Commit();

          // restart the importer
          mvCentralCore.Importer.RestartScanner();
        }
      }

    }

    private void pathsGridView_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
    {
      DataGridViewRow row = pathsGridView.Rows[e.RowIndex];
      DBImportPath path = row.DataBoundItem as DBImportPath;

      string toolTipText = path.FullPath + "\n" + "Drive Type: " + path.GetDriveType().ToString();
      if (path.IsRemovable)
        toolTipText += " (Removable)\nOnline: " + path.IsAvailable;

      if (path.Replaced)
      {
        toolTipText += " (Replaced)";
        row.DefaultCellStyle.ForeColor = Color.DarkGray;
      }
      else
      {
        row.DefaultCellStyle.ForeColor = normalColor;
      }

      // add tooltip text
      row.Cells[0].ToolTipText = toolTipText;
    }
    #endregion

    #region matches tab
    private void unapprovedGrid_SelectionChanged(object sender, EventArgs e)
    {
      updateButtons();
    }


    // updates enabled/disabled status of buttons on media importer based on 
    // the rows selected in the list
    private void updateButtons()
    {
      bool approveButtonEnabled = false;
      bool validMatchSelected = false;
      bool ignoreButtonEnabled = false;

      foreach (DataGridViewRow currRow in unapprovedGrid.SelectedRows)
      {
        MusicVideoMatch selectedMatch = (MusicVideoMatch)currRow.DataBoundItem;

        if (selectedMatch == null)
          break;

        validMatchSelected = true;

        // check if this row is either approved or commited
        if (!approveButtonEnabled &&
            !mvCentralCore.Importer.ApprovedMatches.Contains(selectedMatch) &&
            !mvCentralCore.Importer.RetrievingDetailsMatches.Contains(selectedMatch) &&
            !mvCentralCore.Importer.CommitedMatches.Contains(selectedMatch) &&
            selectedMatch.Selected != null)
        {

          approveButtonEnabled = true;
        }

        if (selectedMatch.LocalMedia.Count > 0 && !selectedMatch.LocalMedia[0].Ignored)
          ignoreButtonEnabled = true;
      }

      // if the selection has changed, enable the approve button
      approveButton.Enabled = approveButtonEnabled;
      rescanButton.Enabled = validMatchSelected;
      ignoreButton.Enabled = ignoreButtonEnabled;

      if (unapprovedGrid.SelectedRows.Count > 0)
        manualAssignButton.Enabled = true;
      else
        manualAssignButton.Enabled = false;

      // check if we have multiple rows selected to join
      if (unapprovedGrid.SelectedRows.Count > 1)
      {
        splitJoinButton.Image = Resources.arrow_join;
        splitJoinButton.ToolTipText = "Join Selected Files";
        splitJoinButton.Enabled = true;
        splitMode = false;
      }

      // check if we have one row with multiple files we can split
      else if (unapprovedGrid.SelectedRows.Count == 1 && unapprovedGrid.SelectedRows[0] != null)
      {
        MusicVideoMatch match = (MusicVideoMatch)unapprovedGrid.SelectedRows[0].DataBoundItem;
        if (match.LocalMedia.Count > 1)
        {
          splitJoinButton.Image = Resources.arrow_divide;
          splitJoinButton.ToolTipText = "Split Selected File Group";
          splitJoinButton.Enabled = true;
          splitMode = true;
        }
        else
          splitJoinButton.Enabled = false;
      }

      // split join button cant be used now, so disable it.
      else splitJoinButton.Enabled = false;
    }

    private void splitJoinButton_Click(object sender, EventArgs e)
    {
      if (splitMode)
      {
        MusicVideoMatch match = (MusicVideoMatch)unapprovedGrid.SelectedRows[0].DataBoundItem;
        mvCentralCore.Importer.Split(match);
      }
      else
      {
        List<MusicVideoMatch> mediaList = new List<MusicVideoMatch>();
        foreach (DataGridViewRow currRow in unapprovedGrid.SelectedRows)
          mediaList.Add((MusicVideoMatch)currRow.DataBoundItem);

        mvCentralCore.Importer.Join(mediaList);
      }

    }

    private void unapprovedGrid_DataError(object sender, DataGridViewDataErrorEventArgs e)
    {
      //            DataGridViewComboBoxCell movieListCombo = (DataGridViewComboBoxCell)unapprovedGrid.Rows[e.RowIndex].Cells["unapprovedPossibleMatchesColumn"];
      //            movieListCombo.Items.Clear();

    }

    private void unapprovedMatchesBindingSource_ListChanged(object sender, ListChangedEventArgs e)
    {
      if (e.ListChangedType == ListChangedType.ItemChanged)
      {
        MusicVideoMatch match = (MusicVideoMatch)unapprovedGrid.Rows[e.NewIndex].DataBoundItem;
        mvCentralCore.Importer.Approve(match);
      }

    }

    private void approveButton_Click(object sender, EventArgs e)
    {
      unapprovedGrid.EndEdit();

      foreach (DataGridViewRow currRow in unapprovedGrid.SelectedRows)
      {
        MusicVideoMatch selectedMatch = (MusicVideoMatch)currRow.DataBoundItem;
        selectedMatch.HighPriority = true;
        mvCentralCore.Importer.Approve(selectedMatch);
      }


    }

    private void restartImporterToolStripMenuItem_Click(object sender, EventArgs e)
    {
      DialogResult result = MessageBox.Show("You are about to restart the MusicVideo Importer. This means that\n" +
                                            "all uncommitted matches from this import session will have to\n" +
                                            "be reapproved. Do you want to continue?\n", "Warning", MessageBoxButtons.YesNo);

      if (result == DialogResult.Yes)
        mvCentralCore.Importer.RestartScanner();
    }

    private void rescanButton_Click(object sender, EventArgs e)
    {
      unapprovedGrid.EndEdit();

      foreach (DataGridViewRow currRow in unapprovedGrid.SelectedRows)
      {
        MusicVideoMatch selectedMatch = (MusicVideoMatch)currRow.DataBoundItem;

        // Check if the first file belonging to the movie is available.
        // Because the identification process uses the first file and it's "environment"
        bool continueRescan = false;
        while (!continueRescan)
        {
          continueRescan = true;
          DBLocalMedia localMedia = selectedMatch.LocalMedia[0];

          // if the file is offline
          if (!localMedia.IsAvailable)
          {
            // do not continue
            continueRescan = false;

            // Prompt the user to insert the media containing the files
            string connect = string.Empty;
            if (localMedia.DriveLetter != null)
            {
              if (localMedia.ImportPath.GetDriveType() == DriveType.CDRom)
                connect = "Please insert the disc labeled '" + localMedia.MediaLabel + "'.";
              else
                connect = "Please reconnect the media labeled '" + localMedia.MediaLabel + "' to " + localMedia.DriveLetter;
            }
            else
            {
              connect = "Please make sure the network share '" + localMedia.FullPath + "' is available.";
            }

            // Show dialog
            DialogResult resultInsert = MessageBox.Show(
            "The file or files you want to rescan are currently not available.\n\n" + connect,
            "File(s) not available.", MessageBoxButtons.RetryCancel);

            // if cancel is pressed stop the rescan process.
            if (resultInsert == DialogResult.Cancel)
              return;
          }

        }

        SearchStringPopup popup = new SearchStringPopup(selectedMatch);
        popup.ShowDialog(this);

        // reprocess
        if (popup.DialogResult == DialogResult.OK)
          mvCentralCore.Importer.Reprocess(selectedMatch);

      }
    }

    private void manualAssignButton_Click(object sender, EventArgs e)
    {
      unapprovedGrid.EndEdit();

      foreach (DataGridViewRow currRow in unapprovedGrid.SelectedRows)
      {
        MusicVideoMatch selectedMatch = (MusicVideoMatch)currRow.DataBoundItem;

        DBLocalMedia mediaToPlay = selectedMatch.LocalMedia[0];

        if (mediaToPlay.State != MediaState.Online) mediaToPlay.Mount();
        while (mediaToPlay.State != MediaState.Online) { Thread.Sleep(1000); };

        ManualAssignPopup popup = new ManualAssignPopup(selectedMatch);

        popup.ShowDialog(this);

        if (popup.DialogResult == DialogResult.OK)
        {

          // create a musicvideo with the user supplied information
          DBTrackInfo mv = new DBTrackInfo();
          mv.Track = popup.Track;
          if (popup.Album.Trim().Length > 0)
          {
            DBAlbumInfo db1 = DBAlbumInfo.Get(popup.Album);
            if (db1 == null) db1 = new DBAlbumInfo();
            db1.Album = popup.Album;
            mv.AlbumInfo.Add(db1);
          }
          DBArtistInfo db2 = DBArtistInfo.Get(popup.Artist);
          if (db2 == null)
          {
            db2 = new DBArtistInfo();
            db2.Artist = popup.Artist;
          }
          mv.ArtistInfo.Add(db2);

          foreach (DBSourceInfo r1 in mvCentralCore.DataProviderManager.AllSources)
          {
            if (r1.Provider is ManualProvider)
            {
              mv.PrimarySource = r1;
              mv.ArtistInfo[0].PrimarySource = r1;
              if (mv.AlbumInfo != null && mv.AlbumInfo.Count > 0) mv.AlbumInfo[0].PrimarySource = r1;
            }
          }




          if (selectedMatch.LocalMedia[0].IsDVD && cbSplitDVD.Checked)
          {

            string videoPath = mediaToPlay.GetVideoPath();
            if (videoPath == null) videoPath = selectedMatch.LongLocalMediaString;
            string pathlower = Path.GetDirectoryName(videoPath).ToLower();
            pathlower = pathlower.Replace("\\video_ts", "");
            pathlower = pathlower.Replace("\\adv_obj", "");
            pathlower = pathlower.Replace("\\bdmv", "");
            if (pathlower.Length < 3) pathlower = pathlower + "\\";
            ChapterExtractor ex =
            Directory.Exists(Path.Combine(pathlower, "VIDEO_TS")) ?
            new DvdExtractor() as ChapterExtractor :
            Directory.Exists(Path.Combine(pathlower, "ADV_OBJ")) ?
            new HddvdExtractor() as ChapterExtractor :
            Directory.Exists(Path.Combine(Path.Combine(pathlower, "BDMV"), "PLAYLIST")) ?
            new BlurayExtractor() as ChapterExtractor :
            null;

            if (ex != null)
            {
              List<ChapterInfo> rp = ex.GetStreams(pathlower, 1);

              if (mediaToPlay.IsMounted)
              {
                mediaToPlay.UnMount();
                while (mediaToPlay.IsMounted) { Thread.Sleep(1000); };
              }
              foreach (ChapterInfo ci in rp)
              {
                foreach (ChapterEntry c1 in ci.Chapters)
                {
                  //skip menus/small crap
                  //                                    if (c1.Time.TotalSeconds < 20) continue;
                  DBTrackInfo db1 = new DBTrackInfo();
                  db1.Copy(mv);
                  db1.TitleID = ci.TitleID;
                  db1.Track = "Chapter " + ci.TitleID.ToString("##") + " - " + c1.chId.ToString("##");
                  db1.Chapter = "Chapter " + c1.chId.ToString("##");
                  db1.ChapterID = c1.chId;
                  db1.PlayTime = c1.Time.ToString();
                  db1.OffsetTime = c1.OffsetTime.ToString();
                  db1.ArtistInfo.Add(mv.ArtistInfo[0]);
                  if (mv.AlbumInfo != null && mv.AlbumInfo.Count > 0) db1.AlbumInfo.Add(mv.AlbumInfo[0]);
                  db1.LocalMedia.Add(selectedMatch.LocalMedia[0]);
                  db1.Commit();
                }
              }
              selectedMatch.LocalMedia[0].UpdateMediaInfo();
              selectedMatch.LocalMedia[0].Commit();
              mvStatusChangedListener(selectedMatch, MusicVideoImporterAction.COMMITED);
              //                        ReloadList();
              return;
            }
          }
          // update the match

          PossibleMatch selectedMV = new PossibleMatch();
          selectedMV.MusicVideo = mv;

          MatchResult result = new MatchResult();
          result.TitleScore = 0;
          result.YearScore = 0;
          result.MdMatch = true;

          selectedMV.Result = result;

          selectedMatch.PossibleMatches.Add(selectedMV);
          selectedMatch.Selected = selectedMV;

          ThreadStart actions = delegate
          {
            // Manually Assign Movie
            mvCentralCore.Importer.ManualAssign(selectedMatch);
          };

          Thread thread = new Thread(actions);
          thread.Name = "ManualUpdateThread";
          thread.Start();
        }
      }

    }


    public static List<ChapterInfo> ReadPgcListFromFile(string file)
    {
      ChapterExtractor ex = null;
      string fileLower = file.ToLower();
      if (fileLower.EndsWith("txt"))
        ex = new TextExtractor();
      else if (fileLower.EndsWith("xpl"))
        ex = new XplExtractor();
      else if (fileLower.EndsWith("ifo"))
        ex = new Ifo2Extractor();
      else if (fileLower.EndsWith("mpls"))
        ex = new MplsExtractor();
      else if (fileLower.EndsWith("xml"))
        throw new Exception("Format not yet supported.");
      else if (fileLower.EndsWith("chapters"))
      {
        List<ChapterInfo> ret = new List<ChapterInfo>();
        ret.Add(ChapterInfo.Load(file));
        return ret;
      }
      else
      {
        throw new Exception("The selected file is not a recognized format.");
      }

      return ex.GetStreams(file, 1);
    }

    #endregion

    #region Artist/Album/Track tab

    private void tvLibrary_AfterSelect(object sender, TreeViewEventArgs e)
    {
      if (_dragStarted) return;
      if (e.Node.Tag.GetType() == typeof(DBArtistInfo)) tcMusicVideo.SelectTab("tpArtist");
      if (e.Node.Tag.GetType() == typeof(DBAlbumInfo)) tcMusicVideo.SelectTab("tpAlbum");
      if (e.Node.Tag.GetType() == typeof(DBTrackInfo)) tcMusicVideo.SelectTab("tpTrack");
      updateDBPage();
      setArtImage();
    }


    private void updateDBPage()
    {
      if (InvokeRequired)
      {
        this.Invoke(new InvokeDelegate(updateDBPage));
        return;
      }

      switch (tcMusicVideo.SelectedTab.Name)
      {
        case "tpArtist":
          artistDetailsList.DatabaseObject = CurrentArtist;
          break;
        case "tpAlbum":
          albumDetailsList.DatabaseObject = CurrentAlbum;
          break;
        case "tpTrack":
          trackDetailsList.DatabaseObject = CurrentTrack;
          fileDetailsList.DatabaseObject = CurrentTrack.LocalMedia[0];
          trackDetailsList.DatabaseObject.CommitNeeded = true;
          fileDetailsList.DatabaseObject.CommitNeeded = true;

          break;
      }
    }

    private void btnArtPrevNext_Click(object sender, EventArgs e)
    {
      DBBasicInfo mv = null;
      switch (tcMusicVideo.SelectedTab.Name)
      {
        case "tpArtist":
          mv = CurrentArtist;
          break;
        case "tpAlbum":
          mv = CurrentAlbum;
          break;
        case "tpTrack":
          mv = CurrentTrack;
          break;
      }
      if (mv == null) return;
      if (sender == btnNextArt) mv.NextArt();
      if (sender == btnPrevArt) mv.PreviousArt();
      setArtImage();
    }

    private void setArtImage()
    {
      DBBasicInfo mv = null;
      btnNextArt.Enabled = false;
      btnPrevArt.Enabled = false;
      btnArtZoom.Enabled = false;
      btnArtDelete.Enabled = false;
      lblArtResolution.Text = "";
      lblArtNum.Text = "";
      switch (tcMusicVideo.SelectedTab.Name)
      {
        case "tpArtist":
          mv = CurrentArtist;
          break;
        case "tpAlbum":
          mv = CurrentAlbum;
          break;
        case "tpTrack":
          mv = CurrentTrack;
          break;
      }

      if (mv == null) return;

      if (InvokeRequired)
      {
        this.Invoke(new InvokeDelegate(setArtImage));
        return;
      }
      try
      {
        Image newArt = null;
        int ArtIndexNum = 0;
        int ArtCount = 0;

        MemoryStream ms = new MemoryStream(File.ReadAllBytes(mv.ArtFullPath));
        newArt = Image.FromStream(ms);
        ArtIndexNum = mv.AlternateArts.IndexOf(mv.ArtFullPath);
        ArtCount = mv.AlternateArts.Count;
        Image oldArt = artImage.Image;
        artImage.Image = newArt;
        if (oldArt != null) oldArt.Dispose();
        lblArtNum.Text = ArtIndexNum + 1 + " / " + ArtCount;
        lblArtResolution.Text = newArt.Width + " x " + newArt.Height;
        if (ArtIndexNum == ArtCount - 1)
          btnNextArt.Enabled = false;
        else
          btnNextArt.Enabled = true;
        if (ArtIndexNum == 0)
          btnPrevArt.Enabled = false;
        else
          btnPrevArt.Enabled = true;
        if (ArtCount > 0)
        {
          btnArtZoom.Enabled = true;
          btnArtDelete.Enabled = true;
        }
      }
      catch (Exception)
      {
        artImage.Image = null;
        btnNextArt.Enabled = false;
        btnPrevArt.Enabled = false;
        btnArtZoom.Enabled = false;
        btnArtDelete.Enabled = false;
        lblArtResolution.Text = "";
        lblArtNum.Text = "";
      }
    }
    /// <summary>
    /// Zoom the art
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnArtZoom_Click(object sender, EventArgs e)
    {
      DBBasicInfo mv = null;
      switch (tcMusicVideo.SelectedTab.Name)
      {
        case "tpArtist":
          mv = CurrentArtist;
          break;
        case "tpAlbum":
          mv = CurrentAlbum;
          break;
        case "tpTrack":
          mv = CurrentTrack;
          break;
      }
      if (mv == null || mv.ArtFullPath.Trim().Length == 0) return;
      ArtPopup popup = new ArtPopup(mv.ArtFullPath);
      popup.Owner = this.ParentForm;
      popup.ShowDialog();
    }
    /// <summary>
    /// Load Art from event handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void loadArtFromFileToolStripMenuItem_Click(object sender, EventArgs e)
    {
      loadArtFromFile();
    }
    /// <summary>
    /// Load Art from File
    /// </summary>
    private void loadArtFromFile()
    {
      DBBasicInfo mv = null;
      switch (tcMusicVideo.SelectedTab.Name)
      {
        case "tpArtist":
          mv = CurrentArtist;
          break;
        case "tpAlbum":
          mv = CurrentAlbum;
          break;
        case "tpTrack":
          mv = CurrentTrack;
          break;
      }
      if (mv == null) return;
      // get the result of the dialog box and only update if the user clicked OK
      DialogResult answerArt = ArtFileDialog.ShowDialog(this);
      if (ArtFileDialog.FileName.Length != 0 && answerArt == DialogResult.OK)
      {
        bool success = mv.AddArtFromFile(ArtFileDialog.FileName);
        if (success)
        {
          // set new art to current and update screen
          mv.ArtFullPath = mv.AlternateArts[mv.AlternateArts.Count - 1];
          mv.Commit();
          setArtImage();
          updateDBPage();
        }
        else
          MessageBox.Show("Failed loading art from specified location.");
      }
    }
    /// <summary>
    /// Load art form URL
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void loadArtFromURLToolStripMenuItem_Click(object sender, EventArgs e)
    {
      DBBasicInfo mv = null;
      switch (tcMusicVideo.SelectedTab.Name)
      {
        case "tpArtist":
          mv = CurrentArtist;
          break;
        case "tpAlbum":
          mv = CurrentAlbum;
          break;
        case "tpTrack":
          mv = CurrentTrack;
          break;
      }
      if (mv == null) return;
      ArtURLPopup popup = new ArtURLPopup();
      popup.ShowDialog(this);
      // do not waste time processing any more if there is no length to the URL when the user clicks OK
      if (popup.GetURL().Trim().Length > 0 && popup.DialogResult == DialogResult.OK)
      {
        // the retrieval process can take a little time, so spawn it off in another thread
        ThreadStart actions = delegate
        {
          startArtProgressBar();
          ImageLoadResults result = mv.AddArtFromURL(popup.GetURL(), true);
          stopArtProgressBar();
          switch (result)
          {
            case ImageLoadResults.SUCCESS:
            case ImageLoadResults.SUCCESS_REDUCED_SIZE:
              // set new cover to current and update screen
              mv.ArtFullPath = mv.AlternateArts[mv.AlternateArts.Count - 1];
              mv.Commit();
              break;
            case ImageLoadResults.FAILED_ALREADY_LOADED:
              MessageBox.Show("Art from the specified URL has already been loaded.");
              break;
            case ImageLoadResults.FAILED:
              MessageBox.Show("Failed loading art from specified URL.");
              break;
          }
        };

        Thread thread = new Thread(actions);
        thread.Name = "ArtUpdater";
        thread.Start();
      }
    }
    /// <summary>
    /// Delete Artwork
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnArtDelete_Click(object sender, EventArgs e)
    {
      DBBasicInfo mv = null;
      switch (tcMusicVideo.SelectedTab.Name)
      {
        case "tpArtist":
          mv = CurrentArtist;
          break;
        case "tpAlbum":
          mv = CurrentAlbum;
          break;
        case "tpTrack":
          mv = CurrentTrack;
          break;
      }
      if (mv == null || mv.AlternateArts.Count == 0) return;

      DialogResult result;
      result = MessageBox.Show("Permanently delete selected art?", "Delete Art", MessageBoxButtons.YesNo);
      if (result == DialogResult.Yes)
      {
        //needed otherwise image gets blocked for deletion
        if (artImage.Image != null) artImage.Image.Dispose();
        mv.DeleteCurrentArt();
        mv.Commit();
        setArtImage();
        updateDBPage();
      }
    }
    /// <summary>
    /// Refresh 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnArtRefresh_Click(object sender, EventArgs e)
    {
      DBBasicInfo mv = null;
      switch (tcMusicVideo.SelectedTab.Name)
      {
        case "tpArtist":
          mv = CurrentArtist;
          break;
        case "tpAlbum":
          mv = CurrentAlbum;
          break;
        case "tpTrack":
          mv = CurrentTrack;
          break;
      }
      if (mv == null) return;

      // the update process can take a little time, so spawn it off in another thread
      ThreadStart actions = delegate
      {
        startArtProgressBar();
        mvCentralCore.DataProviderManager.GetArt(mv);
        stopArtProgressBar();
      };
      Thread thread = new Thread(actions);
      thread.Name = "ArtUpdater";
      thread.Start();
    }
    /// <summary>
    /// Start  progress bar
    /// </summary>
    private void startArtProgressBar()
    {
      if (InvokeRequired)
      {
        Invoke(new InvokeDelegate(startArtProgressBar));
        return;
      }
      artworkProgressBar.Visible = true;
    }
    /// <summary>
    /// Stop progress bar
    /// </summary>
    private void stopArtProgressBar()
    {
      if (InvokeRequired)
      {
        Invoke(new InvokeDelegate(stopArtProgressBar));
        return;
      }
      setArtImage();
      updateDBPage();
      artworkProgressBar.Visible = false;
    }
    /// <summary>
    /// Resend the file to the importer
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void sentToImporterToolStripMenuItem_Click(object sender, EventArgs e)
    {
      switch (tcMusicVideo.SelectedTab.Name)
      {
        case "tpTrack":
          if (CurrentTrack == null) return;
          if (checkTrackForRemoval(CurrentTrack))
          {

            // If we made it this far all files are available and we can notify the user
            // about what the reassign process is going to do.
            DialogResult result = MessageBox.Show(
                    "You are about to reassign this file or set of files\n" +
                    "to a new music video. This will send the file(s) back to\n" +
                    "the importer and you will loose all custom modifications\n" +
                    "to metadata and all user settings.\n\n" +
                    "Are you sure you want to continue?",
                    "Reassign Musicvideos", MessageBoxButtons.YesNo);

            if (result == System.Windows.Forms.DialogResult.Yes)
            {
              List<DBLocalMedia> localMedia = new List<DBLocalMedia>(CurrentTrack.LocalMedia);
              //needed otherwise image gets blocked for deletion
              if (artImage.Image != null) artImage.Image.Dispose();
              CurrentTrack.Delete();
              mvCentralCore.Importer.Start();
              mvCentralCore.Importer.Reprocess(localMedia);
              ReloadList();
              setArtImage();
            }
          }
          break;
        case "tpArtist":
          if (CurrentArtist == null) return;
          List<DBTrackInfo> a1 = DBTrackInfo.GetEntriesByArtist(CurrentArtist);
          if (a1 != null || a1.Count > 0)
          {
            // If we made it this far all files are available and we can notify the user
            // about what the reassign process is going to do.
            DialogResult result = MessageBox.Show(
                    "You are about to reassign this file or set of files\n" +
                    "to a new music video. This will send the file(s) back to\n" +
                    "the importer and you will loose all custom modifications\n" +
                    "to metadata and all user settings.\n\n" +
                    "Are you sure you want to continue?",
                    "Reassign Musicvideos", MessageBoxButtons.YesNo);
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
              //needed otherwise image gets blocked for deletion
              if (artImage.Image != null) artImage.Image.Dispose();
              List<DBLocalMedia> localMedia = new List<DBLocalMedia>();
              foreach (DBTrackInfo d1 in a1)
              {
                if (checkTrackForRemoval(d1))
                {
                  foreach (DBLocalMedia l1 in d1.LocalMedia)
                    localMedia.Add(l1);
                  d1.Delete();
                }
              }
              mvCentralCore.Importer.Start();
              mvCentralCore.Importer.Reprocess(localMedia);
              ReloadList();
              setArtImage();
            }
          }
          break;
        case "tpAlbum":
          if (CurrentAlbum == null) return;
          List<DBTrackInfo> a2 = DBTrackInfo.GetEntriesByAlbum(CurrentAlbum);
          if (a2 != null || a2.Count > 0)
          {
            // If we made it this far all files are available and we can notify the user
            // about what the reassign process is going to do.
            DialogResult result = MessageBox.Show(
                    "You are about to reassign this file or set of files\n" +
                    "to a new music video. This will send the file(s) back to\n" +
                    "the importer and you will loose all custom modifications\n" +
                    "to metadata and all user settings.\n\n" +
                    "Are you sure you want to continue?",
                    "Reassign Musicvideos", MessageBoxButtons.YesNo);
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
              //needed otherwise image gets blocked for deletion
              if (artImage.Image != null) artImage.Image.Dispose();
              List<DBLocalMedia> localMedia = new List<DBLocalMedia>();
              foreach (DBTrackInfo d1 in a2)
              {
                if (checkTrackForRemoval(d1))
                {
                  foreach (DBLocalMedia l1 in d1.LocalMedia)
                    localMedia.Add(l1);
                  d1.Delete();
                }
              }
              mvCentralCore.Importer.Start();
              mvCentralCore.Importer.Reprocess(localMedia);
              ReloadList();
              setArtImage();
            }
          }
          break;
      }
      tcImport.SelectedTab = tpMatch;
      tcMusicVideo.SelectedTab = tpImport;
      tcMusicVideo.Focus();
    }

    private bool checkTrackForRemoval(DBTrackInfo mv)
    {
      if (mv == null) return false;
      // Check if all files belonging to the music video are available.
      bool continueReassign = false;
      while (!continueReassign)
      {
        continueReassign = true;
        foreach (DBLocalMedia localMedia in mv.LocalMedia)
        {
          // if the file is offline
          if (!localMedia.IsAvailable)
          {
            // do not continue
            continueReassign = false;

            // Prompt the user to insert the media containing the files
            string connect = string.Empty;
            if (localMedia.DriveLetter != null)
            {
              if (localMedia.ImportPath.GetDriveType() == DriveType.CDRom)
                connect = "Please insert the disc labeled '" + localMedia.MediaLabel + "'.";
              else
                connect = "Please reconnect the media labeled '" + localMedia.MediaLabel + "' to " + localMedia.DriveLetter;
            }
            else
            {
              connect = "Please make sure the network share '" + localMedia.FullPath + "' is available.";
            }

            // Show dialog
            DialogResult resultInsert = MessageBox.Show(
            "The file or files you want to reassign are currently not available.\n\n" + connect,
            "File(s) not available.", MessageBoxButtons.RetryCancel);

            // if cancel is pressed stop the reassign process.
            if (resultInsert == DialogResult.Cancel)
              return false;

            // break foreach loop (and recheck condition)
            break;
          }
        }
      }
      return true;
    }
    /// <summary>
    /// Allow grabbing of fram from actual video
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void loadArtFromMusicVideoToolStripMenuItem_Click(object sender, EventArgs e)
    {
      string artFolder = mvCentralCore.Settings.TrackArtFolder;
      string safeName = CurrentTrack.Track.Replace(' ', '.').ToValidFilename();
      string filename1 = artFolder + "\\{" + safeName + "} [" + safeName.GetHashCode() + "].jpg";
      string tempFilename = Path.Combine(Path.GetTempPath(), "mvCentralGrabImage.jpg");

      FrameGrabber fr = new FrameGrabber();
      fr.GrabFrame(CurrentTrack.LocalMedia[0].File.FullName, tempFilename, 10);
      ResizeImageWithAspect(tempFilename, filename1, 400);
      ArtPopup popup1 = new ArtPopup(filename1);
      popup1.Owner = this.ParentForm;
      popup1.ShowDialog();
      CurrentTrack.AlternateArts.Add(filename1);
    }
    /// <summary>
    /// Grab a frame 30 seconds in from the video
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void autoGrabFrame30SecsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      DBBasicInfo mv = null;
      mv = CurrentTrack;
      if (mv != null)
      {
        string artFolder = mvCentralCore.Settings.TrackArtFolder;
        string safeName = CurrentTrack.Track.Replace(' ', '.').ToValidFilename();
        string filename1 = artFolder + "\\{" + safeName + "} [" + safeName.GetHashCode() + "].jpg";
        string tempFilename = Path.Combine(Path.GetTempPath(), "mvCentralGrabImage.jpg");

        FrameGrabber fr = new FrameGrabber();
        fr.GrabFrame(CurrentTrack.LocalMedia[0].File.FullName, tempFilename, 10);
        ResizeImageWithAspect(tempFilename, filename1, 400);
        mv.AlternateArts.Add(filename1);
        mv.Commit();
        setArtImage();
        updateDBPage();
      }
    }
    /// <summary>
    /// Resize the grabbed image, with is fixed and ascpect ratio mantained
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="outputFileName"></param>
    /// <param name="newWidth"></param>
    private void ResizeImageWithAspect(string fileName, string outputFileName, int newWidth)
    {
      Image original = Image.FromFile(fileName);
      float aspect = (float)original.Height / (float)original.Width;
      int newHeight = (int)(newWidth * aspect);
      Bitmap temp = new Bitmap(newWidth, newHeight, original.PixelFormat);
      Graphics newImage = Graphics.FromImage(temp);
      newImage.DrawImage(original, 0, 0, newWidth, newHeight);
      temp.Save(outputFileName);
      original.Dispose();
      temp.Dispose();
      newImage.Dispose();
      File.Delete(fileName);
    }

    private void btnPlay_Click(object sender, EventArgs e)
    {
      GrabberPopup p1 = new GrabberPopup(CurrentTrack);
      p1.ShowDialog(this);
      setArtImage();
      updateDBPage();
    }

    private void cmLibrary_Opened(object sender, EventArgs e)
    {
      tsmGrabFrame.Enabled = false;
      tsmRemove.Enabled = false;
      autoGrabFrame30SecsToolStripMenuItem.Enabled = false;

      switch (tcMusicVideo.SelectedTab.Name)
      {
        case "tpArtist":
          break;
        case "tpAlbum":
          //                    tsmGetInfo.Enabled = true;
          break;
        case "tpTrack":
          if (CurrentTrack != null)
          {
            tsmGrabFrame.Enabled = true;
            tsmRemove.Enabled = true;
            autoGrabFrame30SecsToolStripMenuItem.Enabled = true;
          }
          break;
      }
    }

    private void settingsButton_ButtonClick(object sender, EventArgs e)
    {

      settingsButton.ShowDropDown();

    }

    private void unignoreAllFilesToolStripMenuItem_Click(object sender, EventArgs e)
    {
      DialogResult result = MessageBox.Show(
    "This will unignore ALL previously ignored files, and restart\n" +
    "the Importer. This means all uncommitted matches from this\n" +
    "import session will have to be reapproved. Do you want to\n" +
    "continue?\n", "Warning", MessageBoxButtons.YesNo);

      if (result == DialogResult.Yes)
      {
        ProgressPopup popup = new ProgressPopup(new WorkerDelegate(unignoreAllFiles));
        popup.Owner = this.ParentForm;
        popup.Show();
      }
    }

    private void unignoreAllFiles()
    {
      foreach (DBLocalMedia currFile in DBLocalMedia.GetAll())
        if (currFile.Ignored)
          currFile.Delete();

      mvCentralCore.Importer.RestartScanner();
    }
    private void automaticMediaInfoMenuItem_Click(object sender, EventArgs e)
    {
      automaticMediaInfoMenuItem.Checked = !automaticMediaInfoMenuItem.Checked;
      mvCentralCore.Settings.AutoRetrieveMediaInfo = automaticMediaInfoMenuItem.Checked;
    }

    private void btnShowFileDetails_Click(object sender, EventArgs e)
    {
      scTrackDetails.Panel2Collapsed = !scTrackDetails.Panel2Collapsed;
    }

    private void btnShowArtistDetails_Click_1(object sender, EventArgs e)
    {
      scArtistList.Panel2Collapsed = !scArtistList.Panel2Collapsed;
    }

    private void tsmRemove_Click(object sender, EventArgs e)
    {
      CurrentTrack.DeleteAndIgnore();
      mvLibraryTreeView.SelectedNode.Remove();
    }

    private void tsmGetInfo_Click(object sender, EventArgs e)
    {
      DBBasicInfo mv = null;
      switch (tcMusicVideo.SelectedTab.Name)
      {
        case "tpArtist":
          mv = CurrentArtist;
          break;
        case "tpAlbum":
          mv = CurrentAlbum;
          break;
        case "tpTrack":
          mv = CurrentTrack;
          break;
      }
      if (mv == null) return;


      List<DBSourceInfo> r1 = new List<DBSourceInfo>();
      foreach (DBSourceInfo r2 in mvCentralCore.DataProviderManager.AllSources)
      {

        if (r2.Provider is LastFMProvider || r2.Provider is DGProvider)
        {
          if (mv.GetType() == typeof(DBArtistInfo) && r2.Provider is DGProvider)
          { }
          else r1.Add(r2);
        }
      }

      SourcePopup sp = new SourcePopup(r1);
      if (sp.ShowDialog() == DialogResult.OK)
      {

        mv.PrimarySource = r1[sp.listBox1.SelectedIndex];

        // the update process can take a little time, so spawn it off in another thread
        ThreadStart actions = delegate
         {
           startArtProgressBar();
           mv.PrimarySource.Provider.GetDetails(mv);
           mv.Commit();
           stopArtProgressBar();
         };

        Thread thread = new Thread(actions);
        thread.Name = "DetailsUpdater";
        thread.Start();
      }
    }

    #endregion
  }
}