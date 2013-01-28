using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using NLog;
using System.Web;
using System.Net;
using System.Threading;
using System.Collections;
using Cornerstone.Database;
using Cornerstone.Database.CustomTypes;
using Cornerstone.Database.Tables;
using mvCentral.LocalMediaManagement;
using System.Text.RegularExpressions;
using Cornerstone.Tools.Translate;
using System.Runtime.InteropServices;
using mvCentral.LocalMediaManagement.MusicVideoResources;
using Cornerstone.Extensions;

namespace mvCentral.Database
{
  [DBTableAttribute("basic_info")]
  public class DBBasicInfo : mvCentralDBTable, IComparable, IAttributeOwner
  {
    private static Logger logger = LogManager.GetCurrentClassLogger();
    private static readonly object lockList = new object();

    // A delegate type for hooking up change notifications.
    public delegate void ChangedEventHandler(object sender, EventArgs e);
    public event ChangedEventHandler Changed;





    public DBBasicInfo()
      : base()
    {

      //               this.Basic.Changed += new ChangedEventHandler(LocalMedia_Changed);

    }




    public override void AfterDelete()
    {
      if (ID == null)
      {
        while (AlternateArts.Count > 0)
          this.DeleteCurrentArt();
      }
    }


    // Invoke the Changed event; called whenever basic changes
    protected virtual void OnChanged(EventArgs e)
    {
      if (Changed != null)
        Changed(this, e);
    }




    #region Database Fields

    [DBField(AllowDynamicFiltering = false)]
    public string Basic
    {
      get { return _basic; }
      set
      {
        _basic = value;

        if (AlternateArts != null && AlternateArts.Count > 0)
          CheckRenameArt();
        PopulateSortBy();
        commitNeeded = true;
        OnChanged(EventArgs.Empty);
      }
    } private string _basic;

    [DBField(Filterable = false)]
    public string SortBy
    {
      get
      {
        if (_sortBy.Trim().Length == 0)
          PopulateSortBy();

        return _sortBy;
      }

      set
      {
        _sortBy = value;
        commitNeeded = true;
      }
    } private string _sortBy;


    [DBField]
    public String bioSummary
    {

      get { return _biosummary; }
      set
      {
        _biosummary = value;
        commitNeeded = true;
      }

    } private String _biosummary;


    [DBField]
    public String bioContent
    {
      get { return _biocontent; }

      set
      {
        _biocontent = value;
        commitNeeded = true;
      }
    } private String _biocontent;


    [DBField]
    public StringList Tag
    {
      get { return _tag; }

      set
      {
        _tag = value;
        commitNeeded = true;
      }
    } private StringList _tag;


    [DBField(AllowManualFilterInput = true)]
    public string Language
    {
      get { return _language; }

      set
      {
        _language = value;
        commitNeeded = true;
      }
    } private string _language;


    [DBField(AllowAutoUpdate = false, FieldName = "date_added")]
    public DateTime DateAdded
    {
      get { return _dateAdded; }

      set
      {
        _dateAdded = value;
        commitNeeded = true;
      }
    } private DateTime _dateAdded;


    [DBField(FieldName = "md_id", Filterable = false)]
    public string MdID
    {
      get { return _mdid; }

      set
      {
        _mdid = value;
        commitNeeded = true;
      }
    } private string _mdid;

    [DBRelation(AutoRetrieve = true, Filterable = false)]
    public RelationList<DatabaseTable, DBAttribute> Attributes
    {
      get
      {
        if (_attributes == null)
        {
          _attributes = new RelationList<DatabaseTable, DBAttribute>(this);
        }
        return _attributes;
      }
    } RelationList<DatabaseTable, DBAttribute> _attributes;

    [DBField(FieldName = "primary_source", Filterable = false, AllowAutoUpdate = false)]
    public DBSourceInfo PrimarySource
    {
      get { return _primarySource; }

      set
      {
        _primarySource = value;
        commitNeeded = true;
      }
    } private DBSourceInfo _primarySource;


    [DBField(AllowAutoUpdate = false, Filterable = false)]
    public StringList AlternateArts
    {
      get { return _alternatearts; }

      set
      {
        _alternatearts = value;
        commitNeeded = true;
      }
    } private StringList _alternatearts;

    [DBField(AllowAutoUpdate = false, Filterable = false)]
    public StringList ArtUrls
    {
      get { return _arturls; }

      set
      {
        _arturls = value;
        commitNeeded = true;
      }
    } private StringList _arturls;


    [DBField(AllowAutoUpdate = false, Filterable = false)]
    public String ArtFullPath
    {
      get
      {
        if (AlternateArts == null)
          return string.Empty;

        if ((_artfullpath == null || _artfullpath.Trim().Length == 0) && AlternateArts.Count > 0)
          _artfullpath = AlternateArts[0];
        return _artfullpath;
      }

      set
      {
        _artfullpath = value;
        _thumbfullpath = "";
        commitNeeded = true;
      }
    } private String _artfullpath;


    [DBField(AllowAutoUpdate = false, Filterable = false)]
    public String ArtThumbFullPath
    {
      get
      {
        if (_thumbfullpath.Trim().Length == 0)
          GenerateThumbnail();
        return _thumbfullpath;
      }

      set
      {
        _thumbfullpath = value;
        commitNeeded = true;
      }
    } private String _thumbfullpath;


    #endregion

    #region General Management Methods

    public override void Delete()
    {
      if (this.ID == null)
      {
        base.Delete();
        return;
      }


      base.Delete();
    }

    // this should be changed to reflectively commit all sub objects and relation lists
    // and moved down to the DatabaseTable class.
    public override void Commit()
    {
      if (this.ID == null)
      {
        base.Commit();
        commitNeeded = true;
      }

      base.Commit();
    }


    #endregion

    #region Art Management Methods

    // rotates the selected track art to the next available track
    public void NextArt()
    {
      if (AlternateArts.Count <= 1)
        return;

      int index = AlternateArts.IndexOf(ArtFullPath) + 1;
      if (index >= AlternateArts.Count)
        index = 0;

      ArtFullPath = AlternateArts[index];
      commitNeeded = true;
    }

    // rotates the selected track art to the previous available track
    public void PreviousArt()
    {
      if (AlternateArts.Count <= 1)
        return;

      int index = AlternateArts.IndexOf(ArtFullPath) - 1;
      if (index < 0)
        index = AlternateArts.Count - 1;

      ArtFullPath = AlternateArts[index];
      commitNeeded = true;
    }

    // removes the current trackimage from the selection list and deletes it and it's thumbnail 
    // from disk
    public void DeleteCurrentArt()
    {
      string FilePath = ArtFullPath;
      string ThumbFilePath = ArtThumbFullPath;


      // delete thumbnail
      if (ThumbFilePath.Trim().Length > 0)
      {
        FileInfo thumbFile = new FileInfo(ThumbFilePath);
        if (thumbFile.Exists)
        {
          try
          {
            thumbFile.Delete();
          }
          catch (Exception e)
          {
            if (e.GetType() == typeof(ThreadAbortException))
              throw e;
          }
        }
      }

      // If using a custom artwork folder then dont delete the artwork.... 
      if ((ArtFullPath.Contains(mvCentralCore.Settings.CustomArtistArtFolder) && mvCentralCore.Settings.SearchCustomFolderForArtistArt) ||
           (ArtFullPath.Contains(mvCentralCore.Settings.CustomAlbumArtFolder) && mvCentralCore.Settings.SearchCustomFolderForAlbumArt) ||
           (ArtFullPath.Contains(mvCentralCore.Settings.CustomTrackArtFolder) && mvCentralCore.Settings.SearchCustomFolderForTrackArt))
      {
        ArtFullPath = "";
        AlternateArts.Remove(FilePath);
        commitNeeded = true;
        return;
      }

      // delete trackimage
      if (FilePath.Trim().Length > 0)
      {
        FileInfo File = new FileInfo(FilePath);
        if (File.Exists)
        {
          try
          {
            File.Delete();
          }
          catch (Exception e)
          {
            if (e.GetType() == typeof(ThreadAbortException))
              throw e;
          }
        }
      }

      ArtFullPath = "";
      AlternateArts.Remove(FilePath);
      commitNeeded = true;
    }

    // renames the current trackimage from the selection list and deletes old ones and it's thumbnail 
    // from disk
    public void CheckRenameArt()
    {
      string FilePath = _artfullpath;
      string ThumbFilePath = _thumbfullpath;

      // delete thumbnail
      if (ThumbFilePath.Trim().Length > 0)
      {
        FileInfo thumbFile = new FileInfo(ThumbFilePath);
        if (thumbFile.Exists)
        {
          try
          {
            string newname = GenerateName(ThumbFilePath);
            thumbFile.CopyTo(newname);
            thumbFile.Delete();
            _thumbfullpath = newname;
          }
          catch (Exception e)
          {
            if (e.GetType() == typeof(ThreadAbortException))
              throw e;
          }
        }
      }

      StringList temp = new StringList();
      foreach (string str in AlternateArts)
      {
        FileInfo File = new FileInfo(str);
        if (File.Exists)
        {
          try
          {
            string newname = GenerateName(str);
            File.CopyTo(newname);
            File.Delete();
            temp.Add(newname);

          }
          catch (Exception e)
          {
            if (e.GetType() == typeof(ThreadAbortException))
              throw e;
          }
        }

      }
      if (temp.Count > 0)
      {
        AlternateArts.Clear();
        AlternateArts.AddRange(temp);
        _artfullpath = AlternateArts[0];
        commitNeeded = true;
      }
    }

    public string GenerateName(string filename)
    {
      string ext = Path.GetExtension(filename);
      Random random = new Random();
      int val = random.Next(0xFFFFFFF);
      string result = Path.GetDirectoryName(filename) + "\\{" + Basic + "} " + "[" + val.ToString() + "]" + ext;
      return result;
    }


    /*        public bool AddArtFromFile(string filename) {
                int minWidth = mvCentralCore.Settings.MinimumBasicWidth;
                int minHeight = mvCentralCore.Settings.MinimumBasicHeight;
                string Folder = mvCentralCore.Settings.BasicArtFolder;

                Image newImage = null;
                try {
                    newImage = Image.FromFile(filename);
                }
                catch (OutOfMemoryException) {
                    logger.Debug("Invalid image or image format not supported: " + filename);
                }
                catch (FileNotFoundException) {
                    logger.Debug("File not found: " + filename);
                }

                if (newImage == null) {
                    logger.Error("Failed loading track artwork for '" + Basic + "' [" + ID + "] from " + filename + ".");
                    return false;
                }

            
                // check if the image file is already in the track folder
                FileInfo newFile = new FileInfo(filename);
                bool alreadyInFolder = newFile.Directory.FullName.Equals(new DirectoryInfo(Folder).FullName);

                // if the file isnt in the track folder, generate a name and save it there
                if (!alreadyInFolder) {
                    string safeName = Basic.Replace(' ', '.').ToValidFilename();
                    string newFileName = Folder + "\\{" + safeName + "} [" + filename.GetHashCode() + "].jpg";
                
                    if (!File.Exists(newFileName) && saveImage(newFileName, newImage)) {
                        AlternateArts.Add(newFileName); 
                        commitNeeded = true;
                    }
                    else
                        return false;
                }

                // if it's already in the folder, just store the filename in the db
                else {
                    if (!AlternateArts.Contains(filename)) {
                        AlternateArts.Add(filename);
                        commitNeeded = true;
                    }
                    else
                        return false;
                }

                // create a thumbnail for the track
                newImage.Dispose();
                commitNeeded = true;
                GenerateThumbnail();
                logger.Info("Added track art for '" + Basic + "' from: " + filename);
            
                return true;
            }
            */

    // Attempts to load track art for this Basic from a given URL. Optionally
    // ignores minimum resolution restrictions
    public ImageLoadResults AddArtFromFile(string path, bool ignoreRestrictions)
    {
      ImageLoadResults status;
      BasicArt newBasic = BasicArt.FromFile(this, path, ignoreRestrictions, out status);

      if (status != ImageLoadResults.SUCCESS && status != ImageLoadResults.SUCCESS_REDUCED_SIZE)
        return status;
      AlternateArts.Add(newBasic.Filename);
      GenerateThumbnail();
      commitNeeded = true;
      return ImageLoadResults.SUCCESS;
    }

    // Attempts to load track art for this Basic from a given URL. Honors 
    // minimum resolution restrictions
    public bool AddArtFromFile(string path)
    {
      ImageLoadResults status;
      status = AddArtFromFile(path, false);
      if (status != ImageLoadResults.SUCCESS && status != ImageLoadResults.SUCCESS_REDUCED_SIZE)
        return false;
      else return true;
    }


    // Attempts to load track art for this Basic from a given URL. Optionally
    // ignores minimum resolution restrictions
    public ImageLoadResults AddArtFromURL(string url, bool ignoreRestrictions)
    {
      ImageLoadResults status;
      BasicArt newBasic = BasicArt.FromUrl(this, url, ignoreRestrictions, out status);

      if (status != ImageLoadResults.SUCCESS && status != ImageLoadResults.SUCCESS_REDUCED_SIZE)
        return status;
      AlternateArts.Add(newBasic.Filename);
      GenerateThumbnail();
      commitNeeded = true;
      return ImageLoadResults.SUCCESS;
    }

    // Attempts to load track art for this Basic from a given URL. Honors 
    // minimum resolution restrictions
    public ImageLoadResults AddArtFromURL(string url)
    {
      return AddArtFromURL(url, false);
    }


    private bool saveImage(string filename, Image image)
    {
      try
      {
        // try to save as a JPG
        image.Save(filename, ImageFormat.Jpeg);
        return true;
      }
      catch (ArgumentNullException)
      {
        logger.Debug("Error while trying to save BasicArt: filename is NULL.");
      }
      catch (ExternalException)
      {
        try
        {
          // if JPG saving failed for some reason, delete and try to resave as a PNG
          if (File.Exists(filename))
            File.Delete(filename);

          logger.Warn("Failed to save file as JPG, trying PNG: " + filename);
          image.Save(filename, ImageFormat.Png);
          return true;
        }
        catch (Exception ex)
        {
          // we are getting no where...
          logger.Error("Error trying to save image file: " + filename, ex);
          if (File.Exists(filename))
            File.Delete(filename);
        }
      }
      finally
      {
        image.Dispose();
      }

      return false;
    }

    public void GenerateThumbnail()
    {

      if (ArtFullPath.Trim().Length == 0)
        return;
      string thumbsFolder = null;
      if (this.GetType() == typeof(DBTrackInfo)) thumbsFolder = mvCentralCore.Settings.TrackArtThumbsFolder;
      if (this.GetType() == typeof(DBAlbumInfo)) thumbsFolder = mvCentralCore.Settings.AlbumArtThumbsFolder;
      if (this.GetType() == typeof(DBArtistInfo)) thumbsFolder = mvCentralCore.Settings.ArtistArtThumbsFolder;
      string filename = new FileInfo(ArtFullPath).Name;
      string fullname = thumbsFolder + '\\' + filename;

      if (File.Exists(fullname))
      {
        _thumbfullpath = fullname;
        return;
      }


      Image track = null;
      try
      {
        track = Image.FromFile(ArtFullPath);
      }
      catch (OutOfMemoryException e)
      {
        logger.DebugException("Invalid image or image format not supported.", e);
        return;
      }
      catch (FileNotFoundException e)
      {
        logger.DebugException("File not found.", e);
        return;
      }

      if (track == null)
      {
        logger.Error("Error while trying to create thumbnail.");
        return;
      }

      int width = 175;
      int height = (int)(track.Height * ((float)width / (float)track.Width));

      Image trackThumb = track.GetThumbnailImage(width, height, null, IntPtr.Zero);
      if (saveImage(fullname, trackThumb))
      {
        _thumbfullpath = fullname;
        commitNeeded = true;
      }

      track.Dispose();
      trackThumb.Dispose();
    }

    #endregion

    #region Database Management Methods

    //       public static DBBasicInfo Get(int id) {
    //           return mvCentralCore.DatabaseManager.Get<DBBasicInfo>(id);
    //       }

    public static List<DBBasicInfo> GetAll()
    {
      return mvCentralCore.DatabaseManager.Get<DBBasicInfo>(null);
    }

    #endregion

    public override int CompareTo(object obj)
    {
      if (obj.GetType() == typeof(DBBasicInfo))
      {
        return SortBy.CompareTo(((DBBasicInfo)obj).SortBy);
      }
      return 0;
    }

    public override string ToString()
    {
      return Basic;
    }

    public void PopulateSortBy()
    {
      // remove all non-word characters and replace them with spaces
      SortBy = Regex.Replace(_basic, @"[^\w\s]", "", RegexOptions.IgnoreCase).ToLower().Trim();

      // loop through and try to remove a preposition
      if (mvCentralCore.Settings.RemoveTitleArticles)
      {
        string[] prepositions = mvCentralCore.Settings.ArticlesForRemoval.Split('|');
        foreach (string currWord in prepositions)
        {
          string word = currWord + " ";
          if (_sortBy.ToLower().IndexOf(word) == 0)
          {
            SortBy = _sortBy.Substring(word.Length) + " " + _sortBy.Substring(0, currWord.Length);
            return;
          }
        }
      }
    }

    public bool PopulateDateAdded()
    {
      String dateOption = mvCentralCore.Settings.DateImportOption;

      if (dateOption == null)
        dateOption = "created";

      switch (dateOption)
      {
        case "current":
          DateAdded = DateTime.Now;
          break;
        default:
          DateAdded = DateTime.Now;
          break;
      }

      return true;
    }

    public void Translate()
    {
      Translate(mvCentralCore.Settings.TranslationLanguage);
    }

    public void Translate(TranslatorLanguage language)
    {
      Translator tr = new Translator();
      tr.ToLanguage = language;

      bioSummary = tr.Translate(bioSummary);
      bioContent = tr.Translate(bioContent);

    }
  }
}
