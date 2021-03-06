﻿using System;
using System.Collections.Generic;
using System.Text;
using Cornerstone.Database;
using Cornerstone.Database.Tables;
using mvCentral.Database;
using Cornerstone.Tools.Translate;

namespace mvCentral.Settings
{
  public class mvCentralSettings : SettingsManager
  {

    public mvCentralSettings(DatabaseManager dbManager)
      : base(dbManager)
    {
    }

    #region Importer Settings

    #region Tweaks

    [CornerstoneSetting(
        Name = "Default FPS",
        Description = "The number of default FPS used by the DVD Extractor.",
        Groups = "|MusicVideo Importer|Tweaks|",
        Identifier = "importer_default_fps",
        Default = 25)]
    public int DefaultFPS
    {
      get { return _defaultfps; }
      set
      {
        _defaultfps = value;
        OnSettingChanged("importer_default_fps");
      }
    }
    private int _defaultfps;

    [CornerstoneSetting(
        Name = "Thread Count",
        Description = "The number of threads retrieving move details for local media. A higher number uses more system resources, but can help with slow data providers. Do not set this value higher than 10 threads.",
        Groups = "|MusicVideo Importer|Tweaks|",
        Identifier = "importer_thread_count",
        Default = 5)]
    public int ThreadCount
    {
      get { return _threadCount; }
      set
      {
        _threadCount = value;
        OnSettingChanged("importer_thread_count");
      }
    }
    private int _threadCount;


    [CornerstoneSetting(
        Name = "Regular Expression Noise Filter",
        Description = "A regular expression that removes common used keywords from the folder/filename.",
        Groups = "|MusicVideo Importer|Tweaks|",
        Identifier = "importer_filter",
        Default = @"(([\(\{\[]|\b)((576|720|1080)[pi]|dir(ectors )?cut|dvd([r59]|rip|scr(eener)?)|(avc)?hd|wmv|ntsc|pal|mpeg|dsr|r[1-5]|bd[59]|dts|ac3|blu(-)?ray|[hp]dtv|stv|hddvd|xvid|divx|x264|dxva|(?-i)FEST[Ii]VAL|L[iI]M[iI]TED|[WF]S|PROPER|REPACK|RER[Ii]P|REAL|RETA[Ii]L|EXTENDED|REMASTERED|UNRATED|CHRONO|THEATR[Ii]CAL|DC|SE|UNCUT|[Ii]NTERNAL|[DS]UBBED)([\]\)\}]|\b)(-[^\s]+$)?)")]
    public string NoiseFilter
    {
      get { return _noiseFilter; }
      set
      {
        _noiseFilter = value;
        OnSettingChanged("importer_filter");
      }
    }
    private string _noiseFilter;

    [CornerstoneSetting(
        Name = "Default User Agent",
        Description = "Default user agent MusicVideo uses for its web requests.",
        Groups = "|MusicVideo Importer|Tweaks|",
        Identifier = "useragent",
        Default = @"Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; Trident/4.0; SLCC2)")]
    public string UserAgent
    {
      get { return _useragent; }
      set
      {
        _useragent = value;
        OnSettingChanged("useragent");
      }
    }
    private string _useragent;

    [CornerstoneSetting(
        Name = "Enable Importer While In GUI",
        Description = "Enables the importer while in the GUI",
        Groups = "|MusicVideo Importer|Tweaks|",
        Identifier = "importer_gui_enabled",
        Default = true)]
    public bool EnableImporterInGUI
    {
      get { return _enableImporterWhileInGUI; }
      set
      {
        _enableImporterWhileInGUI = value;
        OnSettingChanged("importer_gui_enabled");
      }
    }
    private bool _enableImporterWhileInGUI;

    [CornerstoneSetting(
        Name = "JPG Compression Quality",
        Description = "Determines the quality that will be used for JPG compression of albums and backdrops. Value should be between 1 and 100.",
        Groups = "|MusicVideo Importer|Tweaks|",
        Identifier = "jpg_compress_quality",
        Default = 90)]
    public int JpgCompressionQuality
    {
      get { return _jpgCompressionQuality; }
      set
      {
        _jpgCompressionQuality = value;
        OnSettingChanged("jpg_compress_quality");
      }
    }
    private int _jpgCompressionQuality;

    #endregion

    #region Matching and Importing

    [CornerstoneSetting(
        Name = "Title Auto-Approve Threshold",
        Description = "This is the maximum value for the levenshtein distance that is used for triggering auto-approval on close matching titles.",
        Groups = "|MusicVideo Importer|Matching and Importing|",
        Identifier = "importer_autoapprove",
        Default = 1,
        Hidden = false)]
    public int AutoApproveThreshold
    {
      get { return _autoApproveThreshold; }
      set
      {
        _autoApproveThreshold = value;
        OnSettingChanged("importer_autoapprove");
      }
    }
    private int _autoApproveThreshold;

    [CornerstoneSetting(
        Name = "Year Auto-Approve Distance",
        Description = "This is the maximum of years the release date may be differ before triggering auto-approval on close matches.",
        Groups = "|MusicVideo Importer|Matching and Importing|",
        Identifier = "importer_autoapprove_year",
        Default = 1)]
    public int AutoApproveYearDifference
    {
      get { return _autoApproveYearDifference; }
      set
      {
        _autoApproveYearDifference = value;
        OnSettingChanged("importer_autoapprove_year");
      }
    }
    private int _autoApproveYearDifference;

    [CornerstoneSetting(
        Name = "Auto-approve on alternate titles",
        Description = "When enabled this option will auto-approve matches using alternate titles.",
        Groups = "|MusicVideo Importer|Matching and Importing|",
        Identifier = "importer_autoapprove_alternate_titles",
        Default = true,
        Hidden = false)]
    public bool AutoApproveOnAlternateTitle
    {
      get { return _autoApproveOnAlternateTitle; }
      set
      {
        _autoApproveOnAlternateTitle = value;
        OnSettingChanged("importer_autoapprove_alternate_titles");
      }
    }
    private bool _autoApproveOnAlternateTitle;

    [CornerstoneSetting(
        Name = "Only Auto Approve from Primary Data Source",
        Description = "When enabled this option will auto-approve matches only from the primary data source. Other matches will be available, but not auto approved.",
        Groups = "|MusicVideo Importer|Matching and Importing|",
        Identifier = "importer_autoapprove_primary_source",
        Default = true,
        Hidden = false)]
    public bool AutoApproveOnlyPrimarySource
    {
      get { return _autoApproveOnlyPrimarySource; }
      set
      {
        _autoApproveOnlyPrimarySource = value;
        OnSettingChanged("importer_autoapprove_primary_source");
      }
    }
    private bool _autoApproveOnlyPrimarySource;

    [CornerstoneSetting(
        Name = "Always Group Files In The Same Folder",
        Description = "When enabled this option will ALWAYS group multiple files in one folder together (assuming a multi-part musicvideo).",
        Groups = "|MusicVideo Importer|Matching and Importing|",
        Identifier = "importer_groupfolder",
        Default = false,
        Hidden = false)]
    public bool AlwaysGroupByFolder
    {
      get { return _alwaysGroupByFolder; }
      set
      {
        _alwaysGroupByFolder = value;
        OnSettingChanged("importer_groupfolder");
      }
    }
    private bool _alwaysGroupByFolder;


    [CornerstoneSetting(
        Name = "Prefer Folder Name for MusicVideo Matching",
        Description = "If a folder contains just one musicvideo file it will use the folder name for matching. If you are sure that the filenames are more accurate than the folder name disable this setting.",
        Groups = "|MusicVideo Importer|Matching and Importing|",
        Identifier = "importer_prefer_foldername",
        Default = true,
        Hidden = false)]
    public bool PreferFolderName
    {
      get { return _preferFolderName; }
      set
      {
        _preferFolderName = value;
        OnSettingChanged("importer_prefer_foldername");
      }
    }
    private bool _preferFolderName;

    [CornerstoneSetting(
        Name = "Split DVDs in chapters",
        Description = "Store DVDs as chapters in the database. This can include CDs, DVDs, HD-DVDs, and Bluray disks.",
        Groups = "|MusicVideo Importer|Matching and Importing|",
        Identifier = "importer_split_dvd",
        Default = true,
        Hidden = false)]
    public bool SplitDVD
    {
      get { return _splitdvd; }
      set
      {
        _splitdvd = value;
        OnSettingChanged("importer_split_dvd");
      }
    }
    private bool _splitdvd;

    [CornerstoneSetting(
        Name = "Automatically Import Inserted DVDs",
        Description = "Enables importation of media from all available optical drives. This can include CDs, DVDs, HD-DVDs, and Bluray disks. This also applies to \"loose video files\" on a data CD/DVD.",
        Groups = "|MusicVideo Importer|Matching and Importing|",
        Identifier = "importer_disc_enabled",
        Default = false,
        Hidden = false)]
    public bool AutomaticallyImportDVDs
    {
      get { return _automaticallyImportInsertedDVDs; }
      set
      {
        _automaticallyImportInsertedDVDs = value;
        OnSettingChanged("importer_disc_enabled");
      }
    }
    private bool _automaticallyImportInsertedDVDs;

    [CornerstoneSetting(
        Name = "Ignore Interactive Content on Video Disc",
        Description = "When this option is enabled the importer will ignore the so-called interactive folders on video disc which might contain video material to be played on your PC.",
        Groups = "|MusicVideo Importer|Matching and Importing|",
        Identifier = "importer_disc_ignore_interactive_content",
        Default = true)]
    public bool IgnoreInteractiveContentOnVideoDisc
    {
      get { return _ignoreInteractiveContentOnVideoDisc; }
      set
      {
        _ignoreInteractiveContentOnVideoDisc = value;
        OnSettingChanged("importer_disc_ignore_interactive_content");
      }
    }
    private bool _ignoreInteractiveContentOnVideoDisc;

    [CornerstoneSetting(
        Name = "Minimum Possible Match Threshold",
        Description = "The minimum number of possible matches that must be found before Music Videos will stop searching via additional data providers. Enter 0 to retrieve search results from all active data providers.",
        Groups = "|MusicVideo Importer|Matching and Importing|",
        Identifier = "importer_dataprovider_musicvideo_limit",
        Default = 3)]
    public int MinimumMatches
    {
      get { return _minimumMatches; }
      set
      {
        _minimumMatches = value;
        OnSettingChanged("importer_dataprovider_musicvideo_limit");
      }
    }
    private int _minimumMatches;

    [CornerstoneSetting(
        Name = "Data Provider Request Limit",
        Description = "The maximum number of data providers to use when updating missing music video details. Enter 0 to use all active data providers.",
        Groups = "|MusicVideo Importer|Matching and Importing|",
        Identifier = "importer_dataprovider_request_limit",
        Default = 3)]
    public int DataProviderRequestLimit
    {
      get { return _dataProviderRequestLimit; }
      set
      {
        _dataProviderRequestLimit = value;
        OnSettingChanged("importer_dataprovider_request_limit");
      }
    }
    private int _dataProviderRequestLimit;

    [CornerstoneSetting(
        Name = "Automatically Aquire MediaInfo Details From MusicVideo",
        Description = "If set to true, Music Videos will automatically scan files for various statistics including video file resolution and audio settings. If this option is turned off, this information will not be available to the skin unless manually retrieved by the user. This can improve the speed of the import process.",
        Groups = "|MusicVideo Importer|Matching and Importing|",
        Identifier = "importer_use_mediainfo",
        Default = true)]
    public bool AutoRetrieveMediaInfo
    {
      get { return _useMediaInfo; }
      set
      {
        _useMediaInfo = value;
        OnSettingChanged("importer_use_mediainfo");
      }
    }
    private bool _useMediaInfo;

    [CornerstoneSetting(
        Name = "\"Date Added\" Population Method",
        Description = "Determines which date to use for the 'Date Added' field when importing and adding a MusicVideo to the database. To update values for existing mvCentral you must refresh your Date Added values from the MusicVideo Manager.    Options are: created, modified, or current.",
        Groups = "|MusicVideo Importer|Matching and Importing|",
        Identifier = "date_import_option",
        Default = "created")]
    public string DateImportOption
    {
      get { return _dateImportOption; }
      set
      {
        _dateImportOption = value;
        OnSettingChanged("date_import_option");
      }
    }
    private string _dateImportOption;

    [CornerstoneSetting(
        Name = "Always scan for provider updates in background",
        Description = "If set to true, Music Videos will automatically check all relevent providers for missing information",
        Groups = "|MusicVideo Importer|Matching and Importing|",
        Identifier = "background_missing_data_scan",
        Default = false)]
    public bool BackgroundScanAlways
    {
      get { return _backgroundScanAlways; }
      set
      {
        _backgroundScanAlways = value;
        OnSettingChanged("background_missing_data_scan");
      }
    }
    private bool _backgroundScanAlways;



    #endregion

    #region Preprocessing

    [CornerstoneSetting(
        Name = "Enable NFO Scanner",
        Description = "Scan for NFO file and if available parse out the IMDB id.",
        Groups = "|MusicVideo Importer|Preprocessing|",
        Identifier = "importer_nfoscan",
        Default = true,
        Hidden = false)]
    public bool NfoScannerEnabled
    {
      get { return _nfoScannerEnabled; }
      set
      {
        _nfoScannerEnabled = value;
        OnSettingChanged("importer_nfoscan");
      }
    }
    private bool _nfoScannerEnabled;


    [CornerstoneSetting(
        Name = "NFO Scanner File Extensions",
        Description = "The extensions that are used when scanning for nfo files. Seperate multiple extensions with , or ;",
        Groups = "|MusicVideo Importer|Preprocessing|",
        Identifier = "importer_nfoext",
        Default = "nfo;txt",
        Hidden = false)]
    public string NfoScannerFileExtensions
    {
      get { return _fileExtensions; }
      set
      {
        _fileExtensions = value;
        OnSettingChanged("importer_nfoext");
      }
    }
    private string _fileExtensions;


    [CornerstoneSetting(
        Name = "Auto-Approve on MD Match",
        Description = "If we found a match for the MD id always auto-approve this match even if the other criteria doesn't match closely enough. ",
        Groups = "|MusicVideo Importer|Preprocessing|",
        Identifier = "importer_automd",
        Default = true,
        Hidden = false)]
    public bool AutoApproveOnMDMatch
    {
      get { return _autoApproveOnmdMatch; }
      set
      {
        _autoApproveOnmdMatch = value;
        OnSettingChanged("importer_automd");
      }
    }
    private bool _autoApproveOnmdMatch;

    [CornerstoneSetting(
        Name = "Auto-Approve on ArtistMD Match",
        Description = "If we found a match on ArtistMD id always auto-approve this match even if the other criteria doesn't match closely enough. ",
        Groups = "|MusicVideo Importer|Preprocessing|",
        Identifier = "importer_autoartistmd",
        Default = true,
        Hidden = false)]
    public bool AutoApproveOnArtistMDMatch
    {
      get { return _autoApproveOnartistmdMatch; }
      set
      {
        _autoApproveOnartistmdMatch = value;
        OnSettingChanged("importer_autoartistmd");
      }
    }
    private bool _autoApproveOnartistmdMatch;

    [CornerstoneSetting(
        Name = "Auto-Approve on Album Match",
        Description = "If we found a match on Album MD id always auto-approve this match even if the other criteria doesn't match closely enough. ",
        Groups = "|MusicVideo Importer|Preprocessing|",
        Identifier = "importer_autoalbummd",
        Default = true,
        Hidden = false)]
    public bool AutoApproveOnAlbumMDMatch
    {
      get { return _autoApproveOnalbummdMatch; }
      set
      {
        _autoApproveOnalbummdMatch = value;
        OnSettingChanged("importer_autoalbummd");
      }
    }
    private bool _autoApproveOnalbummdMatch;

    [CornerstoneSetting(
        Name = "Enable DiscID Lookup for DVDs",
        Description = "Enables pre-search lookup for title by using the unique disc id of the DVD.",
        Groups = "|MusicVideo Importer|Preprocessing|",
        Identifier = "importer_lookup_discid",
        Default = true)]
    public bool EnableDiscIdLookup
    {
      get { return _enableDiscIdLookup; }
      set
      {
        _enableDiscIdLookup = value;
        OnSettingChanged("importer_lookup_discid");
      }
    }
    private bool _enableDiscIdLookup;

    [CornerstoneSetting(
        Name = "Enable IMDB Lookup",
        Description = "Enables pre-search lookup for title and year from imdb.com when an imdbid is available. This generally improves results from data providers that don't support imdb id searches.",
        Groups = "|MusicVideo Importer|Preprocessing|",
        Identifier = "importer_lookup_imdb",
        Default = true)]
    public bool EnableImdbPreSearch
    {
      get { return _enableIMDBLookup; }
      set
      {
        _enableIMDBLookup = value;
        OnSettingChanged("importer_lookup_imdb");
      }
    }
    private bool _enableIMDBLookup;


    [CornerstoneSetting(
        Name = "Enable TheMusicVideoDb.org Hash Lookup",
        Description = "Enables pre-search lookup for title, year and imdbid by using the hash/musicvideo match.",
        Groups = "|MusicVideo Importer|Preprocessing|",
        Identifier = "importer_lookup_hash",
        Default = false)]
    public bool EnableHashLookup
    {
      get { return _enableHashLookup; }
      set
      {
        _enableHashLookup = value;
        OnSettingChanged("importer_lookup_hash");
      }
    }
    private bool _enableHashLookup;

    [CornerstoneSetting(
        Name = "Disable Album Support",
        Description = "Disable the processing of Albums in Configuration and GUI",
        Groups = "|MusicVideo Importer|Preprocessing|",
        Identifier = "disable_album_support",
        Default = false,
        Hidden = false)]
    public bool DisableAlbumSupport
    {
      get { return _disablealbumsupport; }
      set
      {
        _disablealbumsupport = value;
        OnSettingChanged("disable_album_support");
      }
    }
    private bool _disablealbumsupport;

    [CornerstoneSetting(
        Name = "Use MD for Album",
        Description = "Use the scrapper found album instead of the parsed one. ",
        Groups = "|MusicVideo Importer|Preprocessing|",
        Identifier = "use_md_album",
        Default = true,
        Hidden = false)]
    public bool UseMDAlbum
    {
      get { return _usemdalbum; }
      set
      {
        _usemdalbum = value;
        OnSettingChanged("use_md_album");
      }
    }
    private bool _usemdalbum;

    [CornerstoneSetting(
        Name = "Enable album scraping from track data",
        Description = "Enable the scraping of the Album from the track data, this can get it very wrong ",
        Groups = "|MusicVideo Importer|Preprocessing|",
        Identifier = "album_from_trackdata",
        Default = true,
        Hidden = false)]
    public bool SetAlbumFromTrackData
    {
      get { return _setalbumfromtrackdata; }
      set
      {
        _setalbumfromtrackdata = value;
        OnSettingChanged("album_from_trackdata");
      }
    }
    private bool _setalbumfromtrackdata;

    [CornerstoneSetting(
        Name = "Ignore the folder structure when parsing",
        Description = "Ignore folder structure when parsing, tick this if folders are nor in format artist\album\track.ext  ",
        Groups = "|MusicVideo Importer|Preprocessing|",
        Identifier = "ignore_folders_when_Parsing",
        Default = true,
        Hidden = false)]
    public bool IgnoreFoldersWhenParsing
    {
      get { return _ignorefolderswhenparsing; }
      set
      {
        _ignorefolderswhenparsing = value;
        OnSettingChanged("ignore_folders_when_Parsing");
      }
    }
    private bool _ignorefolderswhenparsing;


    [CornerstoneSetting(
        Name = "Autoapprove musicvideos",
        Description = "Autoapprove if match found on scrapper. ",
        Groups = "|MusicVideo Importer|Preprocessing|",
        Identifier = "auto_approve",
        Default = true,
        Hidden = false)]
    public bool AutoApprove
    {
      get { return _autoapprove; }
      set
      {
        _autoapprove = value;
        OnSettingChanged("auto_approve");
      }
    }
    private bool _autoapprove;


    [CornerstoneSetting(
       Name = "Latest musicvideos",
       Description = "Consider new if added with these number of days. ",
       Groups = "|MusicVideo|Player|",
       Identifier = "oldAfter_days",
       Default = 7,
       Hidden = false)]
    public int OldAFterDays
    {
      get { return _oldafterdays; }
      set
      {
        _oldafterdays = value;
        OnSettingChanged("oldAfter_days");
      }
    }
    private int _oldafterdays;


    [CornerstoneSetting(
       Name = "Video Thumbnail Columns",
       Description = "Number of Columns for Video Thumnail Preview ",
       Groups = "|MusicVideo|GUI|",
       Identifier = "videoThumbNail_cols",
       Default = 2,
       Hidden = false)]
    public int VideoThumbnailColumns
    {
      get { return _videoThumbailColumns; }
      set
      {
        _videoThumbailColumns = value;
        OnSettingChanged("videoThumbNail_cols");
      }
    }
    private int _videoThumbailColumns;


    [CornerstoneSetting(
       Name = "Video Thumbnail Rows",
       Description = "Number of Rows for Video Thumnail Preview ",
       Groups = "|MusicVideo|GUI|",
       Identifier = "videoThumbNail_rows",
       Default = 2,
       Hidden = false)]
    public int VideoThumbnailRows
    {
      get { return _videoThumbailRows; }
      set
      {
        _videoThumbailRows = value;
        OnSettingChanged("videoThumbNail_rows");
      }
    }
    private int _videoThumbailRows;

    [CornerstoneSetting(
        Name = "Prefer Video Thumbnail",
        Description = "Prefer video thumbnail over scraper image for videos. ",
        Groups = "|MusicVideo Importer|Preprocessing|",
        Identifier = "prefer_thumbnail",
        Default = true,
        Hidden = false)]
    public bool PreferThumbnail
    {
      get { return _preferthumbnail; }
      set
      {
        _preferthumbnail = value;
        OnSettingChanged("prefer_thumbnail");
      }
    }
    private bool _preferthumbnail;


    [CornerstoneSetting(
    Name = "Enable Video Start Info",
    Description = "This will enable #mvCentral.PlayStart. This allows supporting skins to display a info box each time a video starts. ",
    Groups = "|MusicVideo|GUI|",
    Identifier = "enable_video_start_info",
    Default = true,
    Hidden = false)]
    public bool EnableVideoStartInfo
    {
      get { return _enablevideostartinfo; }
      set
      {
        _enablevideostartinfo = value;
        OnSettingChanged("enable_video_start_info");
      }
    }
    private bool _enablevideostartinfo;


    [CornerstoneSetting(
    Name = "Enable Video Start Info Timer",
    Description = "This setting is the number of millseconds #mvCentral.PlayStart remains set after a video starts, the default is 5000 miliseconds (5 seconds).",
    Groups = "|MusicVideo|GUI|",
    Identifier = "video_start_info_timer",
    Default = 5000,
    Hidden = false)]
    public int VideoInfoStartTimer
    {
      get { return _videostartinfotimer; }
      set
      {
        _videostartinfotimer = value;
        OnSettingChanged("video_start_info_timer");
      }
    }
    private int _videostartinfotimer;


    #endregion

    #region SampleFilter

    [CornerstoneSetting(
        Name = "Regular Expression Filter",
        Description = "a regular expression that matches keywords in the filename or it's parent folder indicating that the file is possible sample.",
        Groups = "|MusicVideo Importer|Sample Filter|",
        Identifier = "importer_sample_keyword",
        Default = "sample")]
    public string SampleRegExFilter
    {
      get { return _sampleRegExFilter; }
      set
      {
        _sampleRegExFilter = value;
        OnSettingChanged("importer_sample_keyword");
      }
    }
    private string _sampleRegExFilter;

    [CornerstoneSetting(
        Name = "Include Parent Foldername When Matching",
        Description = "Include the parent foldername when checking for sample keywords.",
        Groups = "|MusicVideo Importer|Sample Filter|",
        Identifier = "importer_sample_include_foldername",
        Default = false)]
    public bool SampleIncludeFolderName
    {
      get { return _sampleIncludeFolderName; }
      set
      {
        _sampleIncludeFolderName = value;
        OnSettingChanged("importer_sample_include_foldername");
      }
    }
    private bool _sampleIncludeFolderName;

    [CornerstoneSetting(
        Name = "Max Filesize (MB)",
        Description = "If the filesize of the potential sample file is below this value it will be skipped.",
        Groups = "|MusicVideo Importer|Sample Filter|",
        Identifier = "importer_sample_maxsize",
        Default = 150)]
    public int MaxSampleFilesize
    {
      get { return _maxSampleFilesize; }
      set
      {
        _maxSampleFilesize = value;
        OnSettingChanged("importer_sample_maxsize");
      }
    }
    private int _maxSampleFilesize;

    #endregion

    #region Artist Art

    [CornerstoneSetting(
        Name = "Artist Art Folder",
        Description = "The folder in which artist art should be saved to disk.",
        Groups = "|MusicVideo Importer|Artist Art|",
        Identifier = "artist_art_folder",
        Default = "")]
    public string ArtistArtFolder
    {
      get { return _artistArtFolder; }
      set
      {
        _artistArtFolder = value;
        OnSettingChanged("artist_art_folder");
      }
    }
    private string _artistArtFolder;


    [CornerstoneSetting(
        Name = "Artist Art Thumbnails Folder",
        Description = "The folder in which artist art thumbnails should be saved to disk.",
        Groups = "|MusicVideo Importer|Artist Art|",
        Identifier = "artist_thumbs_folder",
        Default = "")]
    public string ArtistArtThumbsFolder
    {
      get { return _artistArtworkThumbnailsFolder; }
      set
      {
        _artistArtworkThumbnailsFolder = value;
        OnSettingChanged("artist_thumbs_folder");
      }
    }
    private string _artistArtworkThumbnailsFolder;


    [CornerstoneSetting(
        Name = "Redownload Artist Artwork on Rescan",
        Description = "When a full rescan is performed this setting determines if artistart that has already been downloaded will be reretrieved and the local copy updated.",
        Groups = "|MusicVideo Importer|Artist Art|",
        Identifier = "redownload_artistart",
        Default = false)]
    public bool RedownloadArtistArtwork
    {
      get { return _redownloadArtistArtworkonRescan; }
      set
      {
        _redownloadArtistArtworkonRescan = value;
        OnSettingChanged("redownload_artistart");
      }
    }
    private bool _redownloadArtistArtworkonRescan;


    [CornerstoneSetting(
        Name = "Max Artist arts",
        Description = "When the MusicVideo importer automatically downloads artist art, it will not retrieve more than the given number of artist arts.",
        Groups = "|MusicVideo Importer|Artist Art|",
        Identifier = "max_artist_arts",
        Default = 9,
        Hidden = false)]
    public int MaxArtistArts
    {
      get { return _maxArtistArts; }
      set
      {
        _maxArtistArts = value;
        OnSettingChanged("max_artist_arts");
      }
    }
    private int _maxArtistArts;


    [CornerstoneSetting(
        Name = "Max Artist arts per Session",
        Description = "When the musicvideo importer automatically downloads artist art it will not retrieve more than the given number of artists art in a single update / import session. Next time a full update is done, if there are additional artists to download, it will grab those as well.",
        Groups = "|MusicVideo Importer|Artist Art|",
        Identifier = "max_artist_arts_per_session",
        Default = 3)]
    public int MaxArtistArtsPerSession
    {
      get { return _maxArtistArtsperSession; }
      set
      {
        _maxArtistArtsperSession = value;
        OnSettingChanged("max_artist_arts_per_session");
      }
    }
    private int _maxArtistArtsperSession;


    [CornerstoneSetting(
        Name = "Artist Artwork Filename Pattern",
        Description = "The importer will look in your artist art folder and try to find a file that matches this pattern. If one is found, it will be used as a artist. If none is found, an online data provider will be used to auto download artwork.",
        Groups = "|MusicVideo Importer|Artist Art|",
        Identifier = "local_artistart_pattern",
        Default = "%artist%.jpg|%artist%.png|%artist%.bmp")]
    public string ArtistArtworkFilenamePattern
    {
      get { return _artistArtworkFilenamePattern; }
      set
      {
        _artistArtworkFilenamePattern = value;
        OnSettingChanged("local_artistart_pattern");
      }
    }
    private string _artistArtworkFilenamePattern;


    [CornerstoneSetting(
        Name = "Search MusicVideo Folder for Artist Art",
        Description = "If set to true the local media data provider will use files matching a specified pattern for artist artwork. This setting should only be used if you have all musicvideos in their own folders.",
        Groups = "|MusicVideo Importer|Artist Art|",
        Identifier = "local_artist_from_musicvideo_folder",
        Default = false,
        Hidden = false)]
    public bool SearchMusicVideoFolderForArtistArt
    {
      get { return _searchMusicVideoFolderforArtistArt; }
      set
      {
        _searchMusicVideoFolderforArtistArt = value;
        OnSettingChanged("local_artist_from_musicvideo_folder");
      }
    }
    private bool _searchMusicVideoFolderforArtistArt;

    [CornerstoneSetting(
        Name = "Search Custom Folder for Artist Art",
        Description = "If set to true the local media data provider will use files matching a specified pattern for artist artwork.",
        Groups = "|MusicVideo Importer|Local Artist Art|",
        Identifier = "artist_artwork_from_custom_folder",
        Default = false,
        Hidden = false)]
    public bool SearchCustomFolderForArtistArt
    {
      get { return _searchcustomfolderforartistart; }
      set
      {
        _searchcustomfolderforartistart = value;
        OnSettingChanged("artist_artwork_from_custom_folder");
      }
    }
    private bool _searchcustomfolderforartistart;


    [CornerstoneSetting(
        Name = "Local Folder Artist Art",
        Description = "The folder for local artist artwork",
        Groups = "|MusicVideo Importer|Local Artist Art|",
        Identifier = "local_artistart_folder",
        Default = "")]
    public string CustomArtistArtFolder
    {
      get { return _customartistartworkFolder; }
      set
      {
        _customartistartworkFolder = value;
        OnSettingChanged("local_artistart_folder");
      }
    }
    private string _customartistartworkFolder;



    [CornerstoneSetting(
        Name = "MusicVideo Folder Artist Artwork Filename Pattern",
        Description = "The importer will look in the folder the given musicvideo was found in, and try to find a file that matches this pattern. If one is found, it will be used as a artist. DB field names can be used, surrounded by % symbols. e.g. %imdb_id%.jpg",
        Groups = "|MusicVideo Importer|Artist Art|",
        Identifier = "local_musicvideofolder_artistart_pattern",
        Default = "folder.jpg|folder.png|folder.bmp",
        Hidden = false)]
    public string MusicVideoFolderArtistArtworkFilenamePattern
    {
      get { return _musicvideoFolderArtistArtworkFilenamePattern; }
      set
      {
        _musicvideoFolderArtistArtworkFilenamePattern = value;
        OnSettingChanged("local_musicvideofolder_artistart_pattern");
      }
    }
    private string _musicvideoFolderArtistArtworkFilenamePattern;

    #region Minimum Size

    [CornerstoneSetting(
        Name = "Width",
        Description = "The minimum width in pixels for any given artist. If a artist from any data provider is smaller than this value it will not be downloaded and saved.",
        Groups = "|MusicVideo Importer|Artist Art|Minimum Size",
        Identifier = "min_artist_width",
        Default = 400)]
    public int MinimumArtistWidth
    {
      get { return _minimumArtistWidth; }
      set
      {
        _minimumArtistWidth = value;
        OnSettingChanged("min_artist_width");
      }
    }
    private int _minimumArtistWidth;


    [CornerstoneSetting(
        Name = "Height",
        Description = "The minimum height in pixels for any given artist. If a artist from any data provider is smaller than this value it will not be downloaded and saved.",
        Groups = "|MusicVideo Importer|Artist Art|Minimum Size",
        Identifier = "min_artist_height",
        Default = 400)]
    public int MinimumArtistHeight
    {
      get { return _minimumArtistHeight; }
      set
      {
        _minimumArtistHeight = value;
        OnSettingChanged("min_artist_height");
      }
    }
    private int _minimumArtistHeight;

    #endregion

    #region Maximum Size

    [CornerstoneSetting(
        Name = "Width",
        Description = "The maximum width in pixels for any given artist. If a artist from any data provider is larger than this value it will be resized.",
        Groups = "|MusicVideo Importer|Artist Art|Maximum Size",
        Identifier = "max_artist_width",
        Default = 1680)]
    public int MaximumArtistWidth
    {
      get { return _maximumArtistWidth; }
      set
      {
        _maximumArtistWidth = value;
        OnSettingChanged("max_artist_width");
      }
    }
    private int _maximumArtistWidth;


    [CornerstoneSetting(
        Name = "Height",
        Description = "The maximum height in pixels for any given artist. If a artist from any data provider is larger than this value it will be resized.",
        Groups = "|MusicVideo Importer|Artist Art|Maximum Size",
        Identifier = "max_artist_height",
        Default = 1960)]
    public int MaximumArtistHeight
    {
      get { return _maximumArtistHeight; }
      set
      {
        _maximumArtistHeight = value;
        OnSettingChanged("max_artist_height");
      }
    }
    private int _maximumArtistHeight;

    #endregion

    #endregion

    #region Album Art

    [CornerstoneSetting(
        Name = "Album Artwork Folder",
        Description = "The folder in which album art should be saved to disk.",
        Groups = "|MusicVideo Importer|Album Art|",
        Identifier = "album_art_folder",
        Default = "")]
    public string AlbumArtFolder
    {
      get { return _albumArtworkFolder; }
      set
      {
        _albumArtworkFolder = value;
        OnSettingChanged("album_art_folder");
      }
    }
    private string _albumArtworkFolder;


    [CornerstoneSetting(
        Name = "Album Artwork Thumbnails Folder",
        Description = "The folder in which album art thumbnails should be saved to disk.",
        Groups = "|MusicVideo Importer|Album Art|",
        Identifier = "album_thumbs_folder",
        Default = "")]
    public string AlbumArtThumbsFolder
    {
      get { return _albumArtworkThumbnailsFolder; }
      set
      {
        _albumArtworkThumbnailsFolder = value;
        OnSettingChanged("album_thumbs_folder");
      }
    }
    private string _albumArtworkThumbnailsFolder;


    [CornerstoneSetting(
        Name = "Redownload Album Artwork on Rescan",
        Description = "When a full rescan is performed this setting determines if albumart that has already been downloaded will be reretrieved and the local copy updated.",
        Groups = "|MusicVideo Importer|Album Art|",
        Identifier = "redownload_albumart",
        Default = false)]
    public bool RedownloadAlbumArtwork
    {
      get { return _redownloadAlbumArtworkonRescan; }
      set
      {
        _redownloadAlbumArtworkonRescan = value;
        OnSettingChanged("redownload_albumart");
      }
    }
    private bool _redownloadAlbumArtworkonRescan;


    [CornerstoneSetting(
        Name = "Max Album Arts per MusicVideo",
        Description = "When the MusicVideo importer automatically downloads album arts, it will not retrieve more than the given number of album arts for a MusicVideo.",
        Groups = "|MusicVideo Importer|Album Art|",
        Identifier = "max_album_arts",
        Default = 9,
        Hidden = false)]
    public int MaxAlbumArts
    {
      get { return _maxAlbumarts; }
      set
      {
        _maxAlbumarts = value;
        OnSettingChanged("max_album_arts");
      }
    }
    private int _maxAlbumarts;


    [CornerstoneSetting(
        Name = "Max Album Arts per Session",
        Description = "When the musicvideo importer automatically downloads album arts it will not retrieve more than the given number of albums arts in a single update / import session. Next time a full update is done, if there are additional albums to download, it will grab those as well.",
        Groups = "|MusicVideo Importer|Album Art|",
        Identifier = "max_album_arts_per_session",
        Default = 3)]
    public int MaxAlbumArtsPerSession
    {
      get { return _maxAlbumartsperSession; }
      set
      {
        _maxAlbumartsperSession = value;
        OnSettingChanged("max_album_arts_per_session");
      }
    }
    private int _maxAlbumartsperSession;


    [CornerstoneSetting(
        Name = "Album Artwork Filename Pattern",
        Description = "The importer will look in your album art folder and try to find a file that matches this pattern. If one is found, it will be used as a album. If none is found, an online data provider will be used to auto download artwork.",
        Groups = "|MusicVideo Importer|Album Art|",
        Identifier = "local_albumart_pattern",
        Default = "%album%.jpg|%album%.png|%album%.bmp")]
    public string AlbumArtworkFilenamePattern
    {
      get { return _albumArtworkFilenamePattern; }
      set
      {
        _albumArtworkFilenamePattern = value;
        OnSettingChanged("local_albumart_pattern");
      }
    }
    private string _albumArtworkFilenamePattern;


    [CornerstoneSetting(
        Name = "Search MusicVideo Folder for Album Art",
        Description = "If set to true the local media data provider will use files matching a specified pattern for album artwork. This setting should only be used if you have all musicvideos in their own folders.",
        Groups = "|MusicVideo Importer|Album Art|",
        Identifier = "local_album_from_musicvideo_folder",
        Default = false,
        Hidden = false)]
    public bool SearchMusicVideoFolderForAlbumArt
    {
      get { return _searchMusicVideoFolderforAlbumArt; }
      set
      {
        _searchMusicVideoFolderforAlbumArt = value;
        OnSettingChanged("local_album_from_musicvideo_folder");
      }
    }
    private bool _searchMusicVideoFolderforAlbumArt;

    [CornerstoneSetting(
        Name = "Search Custom Folder for Album Art",
        Description = "If set to true the local media data provider will use files matching a specified pattern for album artwork.",
        Groups = "|MusicVideo Importer|Local Album Art|",
        Identifier = "album_artwork_from_custom_folder",
        Default = false,
        Hidden = false)]
    public bool SearchCustomFolderForAlbumArt
    {
      get { return _searchCustomfolderforalbumArt; }
      set
      {
        _searchCustomfolderforalbumArt = value;
        OnSettingChanged("album_artwork_from_custom_folder");
      }
    }
    private bool _searchCustomfolderforalbumArt;


    [CornerstoneSetting(
        Name = "Local Folder for Album Artwork",
        Description = "The folder for local album artwork",
        Groups = "|MusicVideo Importer|Local Album Art|",
        Identifier = "local_albumart_folder",
        Default = "")]
    public string CustomAlbumArtFolder
    {
      get { return _customalbumartworkfolder; }
      set
      {
        _customalbumartworkfolder = value;
        OnSettingChanged("local_albumart_folder");
      }
    }
    private string _customalbumartworkfolder;



    [CornerstoneSetting(
        Name = "MusicVideo Folder Album Artwork Filename Pattern",
        Description = "The importer will look in the folder the given musicvideo was found in, and try to find a file that matches this pattern. If one is found, it will be used as a album. DB field names can be used, surrounded by % symbols. e.g. %imdb_id%.jpg",
        Groups = "|MusicVideo Importer|Album Art|",
        Identifier = "local_musicvideofolder_albumart_pattern",
        Default = "folder.jpg|folder.png|folder.bmp",
        Hidden = false)]
    public string MusicVideoFolderAlbumArtworkFilenamePattern
    {
      get { return _musicvideoFolderAlbumArtworkFilenamePattern; }
      set
      {
        _musicvideoFolderAlbumArtworkFilenamePattern = value;
        OnSettingChanged("local_musicvideofolder_albumart_pattern");
      }
    }
    private string _musicvideoFolderAlbumArtworkFilenamePattern;

    #region Minimum Size

    [CornerstoneSetting(
        Name = "Width",
        Description = "The minimum width in pixels for any given album. If a album from any data provider is smaller than this value it will not be downloaded and saved.",
        Groups = "|MusicVideo Importer|Album Art|Minimum Size",
        Identifier = "min_album_width",
        Default = 175)]
    public int MinimumAlbumWidth
    {
      get { return _minimumAlbumWidth; }
      set
      {
        _minimumAlbumWidth = value;
        OnSettingChanged("min_album_width");
      }
    }
    private int _minimumAlbumWidth;


    [CornerstoneSetting(
        Name = "Height",
        Description = "The minimum height in pixels for any given album. If a album from any data provider is smaller than this value it will not be downloaded and saved.",
        Groups = "|MusicVideo Importer|Album Art|Minimum Size",
        Identifier = "min_album_height",
        Default = 260)]
    public int MinimumAlbumHeight
    {
      get { return _minimumAlbumHeight; }
      set
      {
        _minimumAlbumHeight = value;
        OnSettingChanged("min_album_height");
      }
    }
    private int _minimumAlbumHeight;

    #endregion

    #region Maximum Size

    [CornerstoneSetting(
        Name = "Width",
        Description = "The maximum width in pixels for any given album. If a album from any data provider is larger than this value it will be resized.",
        Groups = "|MusicVideo Importer|Album Art|Maximum Size",
        Identifier = "max_album_width",
        Default = 680)]
    public int MaximumAlbumWidth
    {
      get { return _maximumAlbumWidth; }
      set
      {
        _maximumAlbumWidth = value;
        OnSettingChanged("max_album_width");
      }
    }
    private int _maximumAlbumWidth;


    [CornerstoneSetting(
        Name = "Height",
        Description = "The maximum height in pixels for any given album. If a album from any data provider is larger than this value it will be resized.",
        Groups = "|MusicVideo Importer|Album Art|Maximum Size",
        Identifier = "max_album_height",
        Default = 960)]
    public int MaximumAlbumHeight
    {
      get { return _maximumAlbumHeight; }
      set
      {
        _maximumAlbumHeight = value;
        OnSettingChanged("max_album_height");
      }
    }
    private int _maximumAlbumHeight;

    #endregion

    #endregion

    #region Track Art

    [CornerstoneSetting(
        Name = "Track Artwork Folder",
        Description = "The folder in which track art should be saved to disk.",
        Groups = "|MusicVideo Importer|Track Art|",
        Identifier = "track_art_folder",
        Default = "")]
    public string TrackArtFolder
    {
      get { return _trackArtworkFolder; }
      set
      {
        _trackArtworkFolder = value;
        OnSettingChanged("track_art_folder");
      }
    }
    private string _trackArtworkFolder;


    [CornerstoneSetting(
        Name = "Track Artwork Thumbnails Folder",
        Description = "The folder in which track art thumbnails should be saved to disk.",
        Groups = "|MusicVideo Importer|Track Art|",
        Identifier = "track_thumbs_folder",
        Default = "")]
    public string TrackArtThumbsFolder
    {
      get { return _trackArtworkThumbnailsFolder; }
      set
      {
        _trackArtworkThumbnailsFolder = value;
        OnSettingChanged("track_thumbs_folder");
      }
    }
    private string _trackArtworkThumbnailsFolder;


    [CornerstoneSetting(
        Name = "Redownload Track Artwork on Rescan",
        Description = "When a full rescan is performed this setting determines if trackart that has already been downloaded will be reretrieved and the local copy updated.",
        Groups = "|MusicVideo Importer|Track Art|",
        Identifier = "redownload_trackart",
        Default = false)]
    public bool RedownloadTrackArtwork
    {
      get { return _redownloadTrackArtworkonRescan; }
      set
      {
        _redownloadTrackArtworkonRescan = value;
        OnSettingChanged("redownload_trackart");
      }
    }
    private bool _redownloadTrackArtworkonRescan;


    [CornerstoneSetting(
        Name = "Max Track Arts per MusicVideo",
        Description = "When the MusicVideo importer automatically downloads track arts, it will not retrieve more than the given number of track arts for a MusicVideo.",
        Groups = "|MusicVideo Importer|Track Art|",
        Identifier = "max_track_arts",
        Default = 9,
        Hidden = false)]
    public int MaxTrackArts
    {
      get { return _maxTrackarts; }
      set
      {
        _maxTrackarts = value;
        OnSettingChanged("max_track_arts");
      }
    }
    private int _maxTrackarts;


    [CornerstoneSetting(
        Name = "Max Track arts per MusicVideo per Session",
        Description = "When the musicvideo importer automatically downloads track arts it will not retrieve more than the given number of track arts for a musicvideo in a single update / import session. Next time a full update is done, if there are additional tracks to download, it will grab those as well.",
        Groups = "|MusicVideo Importer|Track Art|",
        Identifier = "max_track_arts_per_session",
        Default = 3)]
    public int MaxTrackArtsPerSession
    {
      get { return _maxTrackartsperSession; }
      set
      {
        _maxTrackartsperSession = value;
        OnSettingChanged("max_track_arts_per_session");
      }
    }
    private int _maxTrackartsperSession;


    [CornerstoneSetting(
        Name = "Track Artwork Filename Pattern",
        Description = "The importer will look in your track art folder and try to find a file that matches this pattern. If one is found, it will be used as a track. If none is found, an online data provider will be used to auto download artwork.",
        Groups = "|MusicVideo Importer|Track Art|",
        Identifier = "local_trackart_pattern",
        Default = "%track%.jpg|%track%.png|%track%.bmp")]
    public string TrackArtworkFilenamePattern
    {
      get { return _trackArtworkFilenamePattern; }
      set
      {
        _trackArtworkFilenamePattern = value;
        OnSettingChanged("local_trackart_pattern");
      }
    }
    private string _trackArtworkFilenamePattern;


    [CornerstoneSetting(
        Name = "Search MusicVideo Folder for Track Art",
        Description = "If set to true the local media data provider will use files matching a specified pattern for track artwork. This setting should only be used if you have all musicvideos in their own folders.",
        Groups = "|MusicVideo Importer|Track Art|",
        Identifier = "local_track_from_musicvideo_folder",
        Default = false,
        Hidden = false)]
    public bool SearchMusicVideoFolderForTrackArt
    {
      get { return _searchMusicVideoFolderforTrackArt; }
      set
      {
        _searchMusicVideoFolderforTrackArt = value;
        OnSettingChanged("local_track_from_musicvideo_folder");
      }
    }
    private bool _searchMusicVideoFolderforTrackArt;

    [CornerstoneSetting(
        Name = "Search Custom Folder for Track Art",
        Description = "If set to true the local media data provider will use files matching a specified pattern for track artwork.",
        Groups = "|MusicVideo Importer|Local Track Art|",
        Identifier = "track_artwork_from_custom_folder",
        Default = false,
        Hidden = false)]
    public bool SearchCustomFolderForTrackArt
    {
      get { return _searchcustomfolderfortrackart; }
      set
      {
        _searchcustomfolderfortrackart = value;
        OnSettingChanged("track_artwork_from_custom_folder");
      }
    }
    private bool _searchcustomfolderfortrackart;


    [CornerstoneSetting(
        Name = "Local Folder for Track Artwork",
        Description = "The folder for local track artwork",
        Groups = "|MusicVideo Importer|Local Track Art Folder|",
        Identifier = "local_trackart_folder",
        Default = "")]
    public string CustomTrackArtFolder
    {
      get { return _customtrackartworkfolder; }
      set
      {
        _customtrackartworkfolder = value;
        OnSettingChanged("local_trackart_folder");
      }
    }
    private string _customtrackartworkfolder;



    [CornerstoneSetting(
        Name = "MusicVideo Folder Track Artwork Filename Pattern",
        Description = "The importer will look in the folder the given musicvideo was found in, and try to find a file that matches this pattern. If one is found, it will be used as a track. DB field names can be used, surrounded by % symbols. e.g. %imdb_id%.jpg",
        Groups = "|MusicVideo Importer|Track Art|",
        Identifier = "local_musicvideofolder_trackart_pattern",
        Default = "folder.jpg|folder.png|folder.bmp",
        Hidden = false)]
    public string MusicVideoFolderTrackArtworkFilenamePattern
    {
      get { return _musicvideoFolderTrackArtworkFilenamePattern; }
      set
      {
        _musicvideoFolderTrackArtworkFilenamePattern = value;
        OnSettingChanged("local_musicvideofolder_trackart_pattern");
      }
    }
    private string _musicvideoFolderTrackArtworkFilenamePattern;

    #region Minimum Size

    [CornerstoneSetting(
        Name = "Width",
        Description = "The minimum width in pixels for any given track. If a track from any data provider is smaller than this value it will not be downloaded and saved.",
        Groups = "|MusicVideo Importer|Track Art|Minimum Size",
        Identifier = "min_track_width",
        Default = 300)]
    public int MinimumTrackWidth
    {
      get { return _minimumTrackWidth; }
      set
      {
        _minimumTrackWidth = value;
        OnSettingChanged("min_track_width");
      }
    }
    private int _minimumTrackWidth;


    [CornerstoneSetting(
        Name = "Height",
        Description = "The minimum height in pixels for any given track. If a track from any data provider is smaller than this value it will not be downloaded and saved.",
        Groups = "|MusicVideo Importer|Track Art|Minimum Size",
        Identifier = "min_track_height",
        Default = 300)]
    public int MinimumTrackHeight
    {
      get { return _minimumTrackHeight; }
      set
      {
        _minimumTrackHeight = value;
        OnSettingChanged("min_track_height");
      }
    }
    private int _minimumTrackHeight;

    #endregion

    #region Maximum Size

    [CornerstoneSetting(
        Name = "Width",
        Description = "The maximum width in pixels for any given track. If a track from any data provider is larger than this value it will be resized.",
        Groups = "|MusicVideo Importer|Track Art|Maximum Size",
        Identifier = "max_track_width",
        Default = 1080)]
    public int MaximumTrackWidth
    {
      get { return _maximumTrackWidth; }
      set
      {
        _maximumTrackWidth = value;
        OnSettingChanged("max_track_width");
      }
    }
    private int _maximumTrackWidth;


    [CornerstoneSetting(
        Name = "Height",
        Description = "The maximum height in pixels for any given track. If a track from any data provider is larger than this value it will be resized.",
        Groups = "|MusicVideo Importer|Track Art|Maximum Size",
        Identifier = "max_track_height",
        Default = 300)]
    public int MaximumTrackHeight
    {
      get { return _maximumTrackHeight; }
      set
      {
        _maximumTrackHeight = value;
        OnSettingChanged("max_track_height");
      }
    }
    private int _maximumTrackHeight;

    #endregion

    #endregion

    #region themusicvideodb.org

    [CornerstoneSetting(
        Name = "Max Timeouts",
        Description = "The maximum number of timeouts received from the server before a thread returns an error condition.",
        Groups = "|MusicVideo Importer|Network Timeout Settings|",
        Identifier = "tmdb_max_timeouts",
        Default = 10)]
    public int MaxTimeouts
    {
      get { return _maxTimeouts; }
      set
      {
        _maxTimeouts = value;
        OnSettingChanged("tmdb_max_timeouts");
      }
    }
    private int _maxTimeouts;


    [CornerstoneSetting(
        Name = "Timeout Length",
        Description = "The base length of time (in milliseconds) for a timeout when connecting to themusicvideodb.org data service.",
        Groups = "|MusicVideo Importer|Network Timeout Settings|",
        Identifier = "tmdb_timeout_length",
        Default = 5000)]
    public int TimeoutLength
    {
      get { return _timeoutLength; }
      set
      {
        _timeoutLength = value;
        OnSettingChanged("tmdb_timeout_length");
      }
    }
    private int _timeoutLength;


    [CornerstoneSetting(
        Name = "Timeout Increment",
        Description = "The amount of time (in milliseconds) added to the timeout limit  after each timeout failure. A non-zero value will help when the server is experience a large amount of congestion.",
        Groups = "|MusicVideo Importer|Network Timeout Settings|",
        Identifier = "tmdb_timeout_increment",
        Default = 1000)]
    public int TimeoutIncrement
    {
      get { return _timeoutIncrement; }
      set
      {
        _timeoutIncrement = value;
        OnSettingChanged("tmdb_timeout_increment");
      }
    }
    private int _timeoutIncrement;

    #endregion

    #region Importer Language Options

    [CornerstoneSetting(
        Name = "Data Provider Management",
        Description = "Method used to manage data providers. Valid options are 'auto', and 'manual'.",
        Groups = "|MusicVideo Importer|Importer Language Options|",
        Identifier = "dataprovider_management",
        Default = "undefined",
        Hidden = false)]
    public string DataProviderManagementMethod
    {
      get { return _dataProviderManagementMethod; }
      set
      {
        _dataProviderManagementMethod = value;
        OnSettingChanged("dataprovider_management");
      }
    }
    private string _dataProviderManagementMethod;

    [CornerstoneSetting(
        Name = "Automatic Data Provider Language",
        Description = "The language that the automatic data provider management service will optimize for.",
        Groups = "|MusicVideo Importer|Importer Language Options|",
        Identifier = "dataprovider_auto_language",
        Default = "en",
        Hidden = false)]
    public string DataProviderAutoLanguage
    {
      get { return _dataProviderAutoLanguage; }
      set
      {
        _dataProviderAutoLanguage = value;
        OnSettingChanged("dataprovider_auto_language");
      }
    }
    private string _dataProviderAutoLanguage;

    [CornerstoneSetting(
        Name = "Use Translator Service",
        Description = "Service that will translate scraped musicvideo information to a specified language. This service translates the following musicvideo detail fields: genres, tagline, summary.",
        Groups = "|MusicVideo Importer|Importer Language Options|",
        Identifier = "use_translator",
        Default = false,
        Hidden = false)]
    public bool UseTranslator
    {
      get { return _useTranslator; }
      set
      {
        _useTranslator = value;
        OnSettingChanged("use_translator");
      }
    }
    private bool _useTranslator;

    [CornerstoneSetting(
        Name = "Translator Service Configured",
        Description = "Service that will translate scraped musicvideo information to a specified language. This service translates the following musicvideo detail fields: genres, tagline, summary.",
        Groups = "|MusicVideo Importer|Importer Language Options|",
        Identifier = "translator_configured",
        Default = false,
        Hidden = false)]
    public bool TranslatorConfigured
    {
      get { return _translatorConfigured; }
      set
      {
        _translatorConfigured = value;
        OnSettingChanged("translator_configured");
      }
    }
    private bool _translatorConfigured;

    [CornerstoneSetting(
        Name = "Translation Language",
        Description = "The language that the translator service will attempt to tranlate scraped musicvideo details into.",
        Groups = "|MusicVideo Importer|Importer Language Options|",
        Identifier = "translate_to",
        Default = "English",
        Hidden = false)]
    public string TranslationLanguageStr
    {
      get { return _translateTo; }
      set
      {
        _translateTo = value;
        OnSettingChanged("translate_to");
      }
    }
    private string _translateTo;

    public TranslatorLanguage TranslationLanguage
    {
      get
      {
        if (_translationLanguage == null)
        {
          foreach (TranslatorLanguage currLang in LanguageUtility.TranslatableCollection)
          {
            if (LanguageUtility.ToString(currLang).ToLower() == TranslationLanguageStr.ToLower())
            {
              _translationLanguage = currLang;
              break;
            }
          }

          if (_translationLanguage == null)
            _translationLanguage = TranslatorLanguage.English;
        }

        return (TranslatorLanguage)_translationLanguage;
      }

      set
      {
        _translationLanguage = value;
        TranslationLanguageStr = LanguageUtility.ToString(value);
      }
    } private TranslatorLanguage? _translationLanguage = null;

    #endregion

    #endregion

    #region GUI Settings

    #region Interface Options

    [CornerstoneSetting(
        Name = "Default View",
        Description = "The default view used in the MediaPortal GUI when the plug-in is first opened. Valid options are \"lastused\", \"list\", \"thumbs\", \"largethumbs\", and \"filmstrip\".",
        Groups = "|MediaPortal GUI|Interface Options|",
        Identifier = "default_view",
        Default = "lastused",
        Hidden = false)]
    public string DefaultView
    {
      get { return _defaultView; }
      set
      {
        _defaultView = value;
        OnSettingChanged("default_view");
      }
    }
    private string _defaultView;


    [CornerstoneSetting(
        Name = "Default Playlist View",
        Description = "The default view used in the MediaPortal GUI when the plug-in is first opened. Valid options are \"lastused\", \"list\", \"thumbs\", \"largethumbs\", and \"filmstrip\".",
        Groups = "|MediaPortal GUI|Interface Options|",
        Identifier = "default_playlist_view",
        Default = "lastused",
        Hidden = false)]
    public string DefaultPlaylistView
    {
      get { return _defaultPlaylistView; }
      set
      {
        _defaultPlaylistView = value;
        OnSettingChanged("default_playlist_view");
      }
    }
    private string _defaultPlaylistView;

    [CornerstoneSetting(
        Name = "Default View As",
        Description = "The default view as when the plugin is first opened.",
        Groups = "|MediaPortal GUI|Interface Options|",
        Identifier = "default_view_as",
        Default = "2",
        Hidden = false)]
    public string DefaultViewAs
    {
      get { return _defaultviewas; }
      set
      {
        _defaultviewas = value;
        OnSettingChanged("default_view_as");
      }
    }
    private string _defaultviewas;


    [CornerstoneSetting(
        Name = "View-AS",
        Description = "The last selected view-as, ir Artists, Albums, Tracks, Genres or DVD.",
        Groups = "|MediaPortal GUI|Interface Options|",
        Identifier = "view_as",
        Default = "0",
        Hidden = false)]
    public string ViewAs
    {
      get { return _viewas; }
      set
      {
        _viewas = value;
        OnSettingChanged("view_as");
      }
    }
    private string _viewas;



    [CornerstoneSetting(
        Name = "Click Shows Details",
        Description = "Determines behavior when a musicvideo in the musicvideo browser is clicked. If true, the details view appears. If false the musicvideo starts playback.",
        Groups = "|MediaPortal GUI|Interface Options|",
        Identifier = "click_to_details",
        Default = true,
        Hidden = false)]
    public bool ClickShowsDetails
    {
      get { return _clickShowsDetails; }
      set
      {
        _clickShowsDetails = value;
        OnSettingChanged("click_to_details");
      }
    }
    private bool _clickShowsDetails;


    [CornerstoneSetting(
        Name = "Max Actors, Genres, etc. to Display",
        Description = "This determines the number of actors, genres, directors, etc to display on the GUI. This applies to all string based list fields.",
        Groups = "|MediaPortal GUI|Interface Options|",
        Identifier = "max_string_list_items",
        Default = 5)]
    public int MaxElementsToDisplay
    {
      get { return _maxElementsToDisplay; }
      set
      {
        _maxElementsToDisplay = value;
        OnSettingChanged("max_string_list_items");
      }
    }
    private int _maxElementsToDisplay;


    [CornerstoneSetting(
        Name = "Name for Home Screen",
        Description = "The name that appears on the home screen for the plugin.",
        Groups = "|MediaPortal GUI|Interface Options|",
        Identifier = "home_name",
        Default = "mvCentral",
        Hidden = false)]
    public string HomeScreenName
    {
      get { return _homeScreenName; }
      set
      {
        _homeScreenName = value;
        OnSettingChanged("home_name");
      }
    }
    private string _homeScreenName;


    [CornerstoneSetting(
        Name = "Default Sort Field",
        Description = "The default sort field used in the MediaPortal GUI when the plug-in is first opened. Valid options are \"title\", \"dateadded\", \"year\", \"certification\", \"language\", \"score\", \"userscore\", \"popularity\", \"runtime\", \"filepath\".",
        Groups = "|MediaPortal GUI|Interface Options|",
        Identifier = "default_sort_field",
        Default = "Title",
        Hidden = false)]
    public string DefaultSortField
    {
      get { return _defaultSortField; }
      set
      {
        _defaultSortField = value;
        OnSettingChanged("default_sort_field");
      }
    }
    private string _defaultSortField;


    [CornerstoneSetting(
        Name = "Allow user to delete files from the GUI context menu",
        Description = "Enables a delete menu item, which allows you to delete musicvideos from your hard drive.",
        Groups = "|MediaPortal GUI|Interface Options|",
        Identifier = "enable_delete_musicvideo",
        Default = false,
        Hidden = false)]
    public bool AllowDelete
    {
      get { return _allowDelete; }
      set
      {
        _allowDelete = value;
        OnSettingChanged("enable_delete_musicvideo");
      }
    }
    private bool _allowDelete;

    [CornerstoneSetting(
        Name = "Auto-Prompt For User Rating",
        Description = "Music Videos will prompt you for your rating of a musicvideo after the musicvideo ends",
        Groups = "|MediaPortal GUI|Interface Options|",
        Identifier = "auto_prompt_for_rating",
        Default = false)]
    public bool AutoPromptForRating
    {
      get { return _autoPromptForRating; }
      set
      {
        _autoPromptForRating = value;
        OnSettingChanged("auto_prompt_for_rating");
      }
    }
    private bool _autoPromptForRating;

    [CornerstoneSetting(
        Name = "Allow Grouping",
        Description = "Show group headers when sorting the musicvideos",
        Groups = "|MediaPortal GUI|Interface Options|",
        Identifier = "allow_grouping",
        Default = false)]
    public bool AllowGrouping
    {
      get { return _allow_grouping; }
      set
      {
        _allow_grouping = value;
        OnSettingChanged("allow_grouping");
      }
    }
    private bool _allow_grouping;

    [CornerstoneSetting(
        Name = "Display the actual runtime of a musicvideo",
        Description = "If enabled this setting will display the actual runtime of the musicvideo instead of the runtime imported from the data provider. If there's no actual runtime information available it will default to the imported runtime.",
        Groups = "|MediaPortal GUI|Interface Options|",
        Identifier = "gui_display_actual_runtime",
        Default = true)]
    public bool DisplayActualRuntime
    {
      get { return _displayActualRuntime; }
      set
      {
        _displayActualRuntime = value;
        OnSettingChanged("gui_display_actual_runtime");
      }
    }
    private bool _displayActualRuntime;

    [CornerstoneSetting(
        Name = "Reset the selected musicvideo when switching categories",
        Description = "If enabled this setting will reset the selected musicvideo when you switch between categories.",
        Groups = "|MediaPortal GUI|Interface Options|",
        Identifier = "gui_category_reset_selection",
        Default = true)]
    public bool ResetSelectedMusicVideoWhenSwitchingCategories
    {
      get { return _resetSelectedMusicVideoWhenSwitchingCategories; }
      set
      {
        _resetSelectedMusicVideoWhenSwitchingCategories = value;
        OnSettingChanged("gui_category_reset_selection");
      }
    }
    private bool _resetSelectedMusicVideoWhenSwitchingCategories;

    [CornerstoneSetting(
        Name = "Display the filename track text",
        Description = "If enabled this setting will display the raw track name instead of the cleaned version.",
        Groups = "|MediaPortal GUI|Interface Options|",
        Identifier = "gui_raw_track_text",
        Default = false)]
    public bool DisplayRawTrackText
    {
      get { return _displayrawtracktext; }
      set
      {
        _displayrawtracktext = value;
        OnSettingChanged("gui_raw_track_text");
      }
    }
    private bool _displayrawtracktext;

    [CornerstoneSetting(
        Name = "Custom Replacement Filter Regex",
        Description = "Custom replacement regex filter to be appplied before Artist Bio or Album/Track description text is displayed. Matched text is removed from the source string",
        Groups = "|MediaPortal GUI|Interface Options|",
        Identifier = "custom_regex_for_desciptions",
        Default = "")]
    public string CustomBioRegex
    {
      get { return _customebioregex; }
      set
      {
        _customebioregex = value;
        OnSettingChanged("custom_regex_for_desciptions");
      }
    }
    private string _customebioregex;

    #endregion

    #region Tweaks

    [CornerstoneSetting(
        Name = "Artwork Loading Delay",
        Description = "The number of milliseconds that Music Videos waits before it loads new artwork (backdrops and albums) when traversing musicvideos in the GUI. Increasing this value can improve performance if you are experiencing slow down with rapid movements in the GUI.",
        Groups = "|MediaPortal GUI|Tweaks|",
        Identifier = "gui_artwork_delay",
        Default = 250)]
    public int ArtworkLoadingDelay
    {
      get { return _artworkLoadingDelay; }
      set
      {
        _artworkLoadingDelay = value;
        OnSettingChanged("gui_artwork_delay");
      }
    }
    private int _artworkLoadingDelay;

    [CornerstoneSetting(
        Name = "Details Loading Delay",
        Description = "The number of milliseconds that Music Videos waits before it show the details about the current selection when traversing musicvideos in the GUI. Increasing this value can improve performance if you are experiencing slow down with rapid movements in the GUI.",
        Groups = "|MediaPortal GUI|Tweaks|",
        Identifier = "gui_details_delay",
        Default = 250)]
    public int DetailsLoadingDelay
    {
      get { return _detailsLoadingDelay; }
      set
      {
        _detailsLoadingDelay = value;
        OnSettingChanged("gui_details_delay");
      }
    }
    private int _detailsLoadingDelay;

    [CornerstoneSetting(
        Name = "Category Random Artwork Refresh Interval",
        Description = "The number of seconds that Music Videos waits before renewing the backdrop when a category (using random backdrop) is selected.",
        Groups = "|MediaPortal GUI|Tweaks|",
        Identifier = "gui_artwork_random_refresh",
        Default = 120)]
    public int CategoryRandomArtworkRefreshInterval
    {
      get { return _categoryRandomArtworkRefreshInterval; }
      set
      {
        _categoryRandomArtworkRefreshInterval = value;
        OnSettingChanged("gui_artwork_random_refresh");
      }
    }
    private int _categoryRandomArtworkRefreshInterval;

    [CornerstoneSetting(
        Name = "Use Remote Control Filtering",
        Description = "Enables the Remote Controle Filter, set to false if you want to use the default mediaportal remote control functionality.",
        Groups = "|MediaPortal GUI|Tweaks|",
        Identifier = "enable_rc_filter",
        Default = true,
        Hidden = false)]
    public bool UseRemoteControlFiltering
    {
      get { return _useRemoteControlFiltering; }
      set
      {
        _useRemoteControlFiltering = value;
        OnSettingChanged("enable_rc_filter");
      }
    }
    private bool _useRemoteControlFiltering;

    [CornerstoneSetting(
    Name = "Play Properities Setting Delay",
    Description = "The number of milliseconds that Music Videos waits after starting to play a video and setting the #Play properities. The delay is neccesary because Player tries to use metadata from the MyVideos database and we want to update this after that happens so the correct info is there ",
    Groups = "|MediaPortal GUI|Tweaks|",
    Identifier = "gui_play_properities_delay",
    Default = 500)]
    public int PlayProperitiesSetDelay
    {
      get { return _playproperitiessetdelay; }
      set
      {
        _playproperitiessetdelay = value;
        OnSettingChanged("gui_play_properities_delay");
      }
    }
    private int _playproperitiessetdelay;

    #endregion

    #region Playback Options

    [CornerstoneSetting(
        Name = "GUI Watch Percentage",
        Description = "The percentage of a musicvideo that must be watched before it will be flagged as watched. This also affects whether resume data is stored.",
        Groups = "|MediaPortal GUI|Playback Options|",
        Identifier = "gui_watch_percentage",
        Default = 90,
        Hidden = false)]
    public int MinimumWatchPercentage
    {
      get { return _minimumWatchPercentage; }
      set
      {
        _minimumWatchPercentage = value;
        OnSettingChanged("gui_watch_percentage");
      }
    }
    private int _minimumWatchPercentage;


    [CornerstoneSetting(
         Name = "Disk Insertion Behavior",
         Description = "Action to take when a DVD, Bluray, or HDDVD disk is inserted. (\"DETAILS\": Goto the details page for the DVD. \"PLAY\": Start playback immediately. \"NOTHING\": Take no action).",
         Groups = "|MediaPortal GUI|Playback Options|",
         Identifier = "on_disc_loaded",
         Default = "DETAILS",
         Hidden = false)]
    public string DiskInsertionBehavior
    {
      get { return _diskInsertionBehavior; }
      set
      {
        _diskInsertionBehavior = value;
        OnSettingChanged("on_disc_loaded");
      }
    }
    private string _diskInsertionBehavior;


    [CornerstoneSetting(
        Name = "Custom Intro Location",
        Description = "Location of a custom intro that will play before each musicvideo.  This should be the full path including the musicvideo. For example: c:\\custom_intro\\into.mpg",
        Groups = "|MediaPortal GUI|Playback Options|",
        Identifier = "custom_intro_location",
        Default = " ")]
    public string CustomIntroLocation
    {
      get { return _customIntroLocation; }
      set
      {
        _customIntroLocation = value;
        OnSettingChanged("custom_intro_location");
      }
    }
    private string _customIntroLocation;

    [CornerstoneSetting(
        Name = "Auto fullscreen on video start",
        Description = "Switch to fullscreen when video starts to play.",
        Groups = "|MediaPortal GUI|Playback Options|",
        Identifier = "auto_fullscreen",
        Default = true)]
    public bool AutoFullscreen
    {
      get { return _autoFullscreen; }
      set
      {
        _autoFullscreen = value;
        OnSettingChanged("auto_fullscreen");
      }
    }
    private bool _autoFullscreen;



    #endregion

    #region Playlist

    [CornerstoneSetting(
        Name = "Playlist Location",
        Description = "Location of mvCentral Playlist files.",
        Groups = "|MediaPortal GUI|Playlist|",
        Identifier = "playlist_folder",
        Default = "",
        Hidden = false)]
    public string PlayListFolder
    {
      get { return _playlistfolder; }
      set
      {
        _playlistfolder = value;
        OnSettingChanged("playlist_folder");
      }
    }
    private string _playlistfolder;

    [CornerstoneSetting(
        Name = "Repeat Playlist",
        Description = "Repeat the playlist.",
        Groups = "|MediaPortal GUI|Playlist|",
        Identifier = "general_repeatplaylist",
        Default = true,
        Hidden = false)]
    public bool repeatPlayList
    {
      get { return _repeatplaylist; }
      set
      {
        _repeatplaylist = value;
        OnSettingChanged("general_repeatplaylist");
      }
    }
    private bool _repeatplaylist;

    [CornerstoneSetting(
        Name = "Autoplay Playlist",
        Description = "Play the playlist after load.",
        Groups = "|MediaPortal GUI|Playlist|",
        Identifier = "general_playlistautoplay",
        Default = true,
        Hidden = false)]
    public bool playlistAutoPlay
    {
      get { return _playlistautoplay; }
      set
      {
        _playlistautoplay = value;
        OnSettingChanged("general_playlistautoplay");
      }
    }
    private bool _playlistautoplay;


    [CornerstoneSetting(
        Name = "Autoshuffle playlist",
        Description = "Automatic shuffle of playlists.",
        Groups = "|MediaPortal GUI|Playlist|",
        Identifier = "general_playlistautoshuffle",
        Default = true,
        Hidden = false)]
    public bool playlistAutoShuffle
    {
      get { return _playlistautoshuffle; }
      set
      {
        _playlistautoshuffle = value;
        OnSettingChanged("general_playlistautoshuffle");
      }
    }
    private bool _playlistautoshuffle;

    [CornerstoneSetting(
        Name = "Autoshuffle generated playlist",
        Description = "Automatic shuffle of generated playlists.",
        Groups = "|MediaPortal GUI|Playlist|",
        Identifier = "generated_playlistautoshuffle",
        Default = true,
        Hidden = false)]
    public bool GeneratedPlaylistAutoShuffle
    {
      get { return _generatedplaylistautoshuffle; }
      set
      {
        _generatedplaylistautoshuffle = value;
        OnSettingChanged("generated_playlistautoshuffle");
      }
    }
    private bool _generatedplaylistautoshuffle;


    [CornerstoneSetting(
        Name = "Clear playlist on add",
        Description = "Clear the current playlist when adding new",
        Groups = "|MediaPortal GUI|Playlist|",
        Identifier = "clear_playist_on_add",
        Default = true,
        Hidden = false)]
    public bool ClearPlaylistOnAdd
    {
      get { return _clearplaylistonadd; }
      set
      {
        _clearplaylistonadd = value;
        OnSettingChanged("clear_playist_on_add");
      }
    }
    private bool _clearplaylistonadd;

    [CornerstoneSetting(
        Name = "Shuffle SmartDJ Playlists",
        Description = "Shuffle playlists created with SmartDJ",
        Groups = "|MediaPortal GUI|Playlist|",
        Identifier = "_smartdjplaylistshuffle",
        Default = true,
        Hidden = false)]
    public bool SmartDJPlaylistShuffle
    {
      get { return _smartdjplaylistshuffle; }
      set
      {
        _smartdjplaylistshuffle = value;
        OnSettingChanged("_smartdjplaylistshuffle");
      }
    }
    private bool _smartdjplaylistshuffle;



    #endregion

    #region Bluray/HD-DVD Playback

    [CornerstoneSetting(
        Name = "Use External Player",
        Description = "Enable playback for Bluray/HD-DVD using an external player, When set to false the main video stream will be played in the internal player (no menu).",
        Groups = "|MediaPortal GUI|Bluray/HD-DVD Playback|",
        Identifier = "playback_hd_external",
        Default = false)]
    public bool UseExternalPlayer
    {
      get { return _useExternalPlayer; }
      set
      {
        _useExternalPlayer = value;
        OnSettingChanged("playback_hd_external");
      }
    }
    private bool _useExternalPlayer;

    [CornerstoneSetting(
        Name = "Enable Dynamic Refresh Rate Changer Settings",
        Description = "Enable the Dynamic Refresh Rate Changer when using an external player, this setting will only take effect if you have setup the Dynamic Refresh Rate Changer settings in your MediaPortal configuration.",
        Groups = "|MediaPortal GUI|Bluray/HD-DVD Playback|",
        Identifier = "playback_hd_change_refresh_rate",
        Default = false)]
    public bool UseDynamicRefreshRateChangerWithExternalPlayer
    {
      get { return _useDynamicRefreshRateChangerWithExternalPlayer; }
      set
      {
        _useDynamicRefreshRateChangerWithExternalPlayer = value;
        OnSettingChanged("playback_hd_change_refresh_rate");
      }
    }
    private bool _useDynamicRefreshRateChangerWithExternalPlayer;


    [CornerstoneSetting(
        Name = "External Player Path",
        Description = "The path to the executable of the external player that you want to use for playing back BR/HD-DVD.",
        Groups = "|MediaPortal GUI|Bluray/HD-DVD Playback|",
        Identifier = "playback_hd_executable",
        Default = "C:\\MyExternalPlayer\\MyExternalPlayer.exe")]
    public string ExternalPlayerExecutable
    {
      get { return _externalPlayerExecutable; }
      set
      {
        _externalPlayerExecutable = value;
        OnSettingChanged("playback_hd_executable");
      }
    }
    private string _externalPlayerExecutable;


    [CornerstoneSetting(
        Name = "External Player Arguements",
        Description = "The command-line arguments that should be appended when calling the executable. (available variables: %filename% will be replaced with the path to the musicvideo, %fps% will be replaced with the framerate for the musicvideo)",
        Groups = "|MediaPortal GUI|Bluray/HD-DVD Playback|",
        Identifier = "playback_hd_arguments",
        Default = "%filename%")]
    public string ExternalPlayerArguements
    {
      get { return _externalPlayerArguements; }
      set
      {
        _externalPlayerArguements = value;
        OnSettingChanged("playback_hd_arguments");
      }
    }
    private string _externalPlayerArguements;

    #endregion

    #region Sorting

    [CornerstoneSetting(
        Name = "Remove Title Articles",
        Description = "If enabled, articles such as \"the\", \"a\", and \"an\" will not be considered when sorting by title. This affects the Sort By field and for a change to take effect you must refresh your Sort By values from the MusicVideo Manager.",
        Groups = "|MediaPortal GUI|Sorting|",
        Identifier = "remove_title_articles",
        Default = true)]
    public bool RemoveTitleArticles
    {
      get { return _removeTitleArticles; }
      set
      {
        _removeTitleArticles = value;
        OnSettingChanged("remove_title_articles");
      }
    }
    private bool _removeTitleArticles;

    [CornerstoneSetting(
        Name = "Articles for Removal",
        Description = "The articles that will be removed from a title when found at the beginning of a title for sorting purposes. Seperate articles with a pipe \"|\". See the \"Remove Title Articles\" setting.",
        Groups = "|MediaPortal GUI|Sorting|",
        Identifier = "articles_for_removal",
        Default = "the|a|an|ein|das|die|der|les|la|le|el|une|de|het")]
    public string ArticlesForRemoval
    {
      get { return _articlesForRemoval; }
      set
      {
        _articlesForRemoval = value;
        OnSettingChanged("articles_for_removal");
      }
    }
    private string _articlesForRemoval;

    #endregion

    #region Parental Controls

    [CornerstoneSetting(
        Name = "Enable Parental Controls",
        Description = "Enables the Paretal Controls feature in the GUI.",
        Groups = "|MediaPortal GUI|Parental Controls|",
        Identifier = "enable_parental_controls",
        Default = false,
        Hidden = false)]
    public bool ParentalControlsEnabled
    {
      get { return _parentalControlsEnabled; }
      set
      {
        _parentalControlsEnabled = value;
        OnSettingChanged("enable_parental_controls");
      }
    }
    private bool _parentalControlsEnabled;

    [CornerstoneSetting(
        Name = "Parental Controls Filter ID",
        Description = "The filter attached to the Parental Controls functionality.",
        Groups = "|MediaPortal GUI|Parental Controls|",
        Identifier = "parental_controls_filter_id",
        Default = "null",
        Hidden = false)]
    public string ParentalContolsFilterID
    {
      get { return _parentalContolsFilterID; }
      set
      {
        _parentalContolsFilterID = value;
        OnSettingChanged("parental_controls_filter_id");
      }
    }
    private string _parentalContolsFilterID;

    public DBFilter<DBTrackInfo> ParentalControlsFilter
    {
      get
      {
        if (_parentalControlsFilter == null)
        {
          // grab or create the filter object attached to the parental controls
          string filterID = mvCentralCore.Settings.ParentalContolsFilterID;
          if (filterID == "null")
          {
            _parentalControlsFilter = new DBFilter<DBTrackInfo>();
            _parentalControlsFilter.Name = "Children's mvCentral";
            mvCentralCore.DatabaseManager.Commit(_parentalControlsFilter);
            ParentalContolsFilterID = _parentalControlsFilter.ID.ToString();
          }
          else
          {
            _parentalControlsFilter = mvCentralCore.DatabaseManager.Get<DBFilter<DBTrackInfo>>(int.Parse(filterID));
          }
        }

        return _parentalControlsFilter;
      }
    } private DBFilter<DBTrackInfo> _parentalControlsFilter = null;

    [CornerstoneSetting(
        Name = "Parental Controls Password",
        Description = "The password required to access musicvideos restricted by parental controls.",
        Groups = "|MediaPortal GUI|Parental Controls|",
        Identifier = "parental_controls_password",
        Default = "1111",
        Hidden = true,
        Sensitive = true)]
    public string ParentalContolsPassword
    {
      get { return _parentalContolsPassword; }
      set
      {
        _parentalContolsPassword = value;
        OnSettingChanged("parental_controls_password");
      }
    }
    private string _parentalContolsPassword;

    [CornerstoneSetting(
        Name = "Parental Controls Timeout",
        Description = "If set, this will reenable the parental filter after the system is idle for x minutes.  Use 0 to disable the timeout.",
        Groups = "|MediaPortal GUI|Parental Controls|",
        Identifier = "parental_controls_timeout",
        Default = 10,
        Hidden = false)]
    public int ParentalControlsTimeout
    {
      get { return _parentalControlsTimeout; }
      set
      {
        _parentalControlsTimeout = value;
        OnSettingChanged("parental_controls_timeout");
      }
    }
    private int _parentalControlsTimeout;
    #endregion

    #region Filtering

    [CornerstoneSetting(
        Name = "Filter Menu ID",
        Description = "The menu for the popup filtering menu.",
        Groups = "|MediaPortal GUI|Filtering|",
        Identifier = "filter_menu_id",
        Default = "null",
        Hidden = false)]
    public string FilterMenuID
    {
      get { return _filterMenuID; }
      set
      {
        _filterMenuID = value;
        OnSettingChanged("filter_menu_id");
      }
    }
    private string _filterMenuID;

    public DBMenu<DBTrackInfo> FilterMenu
    {
      get
      {
        if (_filterMenu == null)
        {
          // grab or create the menu for the filtering popup
          string menuID = FilterMenuID;
          if (menuID == "null")
          {
            _filterMenu = new DBMenu<DBTrackInfo>();
            _filterMenu.Name = "Filtering Menu";
            mvCentralCore.DatabaseManager.Commit(_filterMenu);
            FilterMenuID = _filterMenu.ID.ToString();
          }
          else
          {
            _filterMenu = mvCentralCore.DatabaseManager.Get<DBMenu<DBTrackInfo>>(int.Parse(menuID));
          }
        }

        return _filterMenu;
      }
    } private DBMenu<DBTrackInfo> _filterMenu = null;

    [CornerstoneSetting(
        Name = "Use Default Filter",
        Description = "If enabled the default filter will be used on initial launch.",
        Groups = "|MediaPortal GUI|Filtering|",
        Identifier = "use_default_filter",
        Default = false,
        Hidden = false)]
    public bool DefaultFilterEnabled
    {
      get { return _useDefaultFilter; }
      set
      {
        _useDefaultFilter = value;
        OnSettingChanged("use_default_filter");
      }
    }
    private bool _useDefaultFilter;

    [CornerstoneSetting(
        Name = "Default Filter ID",
        Description = "The database ID for the default filter on startup.",
        Groups = "|MediaPortal GUI|Filtering|",
        Identifier = "default_filter_id",
        Default = "null",
        Hidden = false)]
    public string DefaultFilterID
    {
      get { return _defaultFilterID; }
      set
      {
        _defaultFilterID = value;
        OnSettingChanged("default_filter_id");
      }
    }
    private string _defaultFilterID;

    public DBNode<DBTrackInfo> DefaultFilter
    {
      get
      {
        if (_defaultFilter == null)
        {
          // grab the default filter or assign the default
          string filterID = DefaultFilterID;
          if (filterID == "null")
          {
            _defaultFilter = null;

            List<DBNode<DBTrackInfo>> resultSet = FilterMenu.FindNode("${UnwatchedmvCentral}");
            if (resultSet.Count > 0)
            {
              _defaultFilter = resultSet[0];
              DefaultFilterID = _defaultFilter.ID.ToString();
            }
          }
          else
          {
            try { _defaultFilter = mvCentralCore.DatabaseManager.Get<DBNode<DBTrackInfo>>(int.Parse(filterID)); }
            catch (FormatException)
            {
              _defaultFilter = null;
              DefaultFilterID = "null";
            }
          }
        }

        return _defaultFilter;
      }

      set
      {
        if (value == null) DefaultFilterID = "null";
        else DefaultFilterID = value.ID.ToString();

        _defaultFilter = null;
      }
    } private DBNode<DBTrackInfo> _defaultFilter = null;

    [CornerstoneSetting(
        Name = "Category Menu ID",
        Description = "The menu for the categories functionality.",
        Groups = "|MediaPortal GUI|Filtering|",
        Identifier = "categories_menu_id",
        Default = "null",
        Hidden = false)]
    public string CategoriesMenuID
    {
      get { return _categoriesMenuID; }
      set
      {
        _categoriesMenuID = value;
        OnSettingChanged("categories_menu_id");
      }
    }
    private string _categoriesMenuID;

    public DBMenu<DBTrackInfo> CategoriesMenu
    {
      get
      {
        if (_categoriesMenu == null)
        {
          // grab or create the menu for the filtering popup
          string menuID = CategoriesMenuID;
          if (menuID == "null")
          {
            _categoriesMenu = new DBMenu<DBTrackInfo>();
            _categoriesMenu.Name = "Categories Menu";
            mvCentralCore.DatabaseManager.Commit(_categoriesMenu);
            CategoriesMenuID = _categoriesMenu.ID.ToString();
          }
          else
          {
            _categoriesMenu = mvCentralCore.DatabaseManager.Get<DBMenu<DBTrackInfo>>(int.Parse(menuID));
          }
        }

        return _categoriesMenu;
      }
    } private DBMenu<DBTrackInfo> _categoriesMenu = null;

    [CornerstoneSetting(
        Name = "Enable Categories",
        Description = "Enables the Categories feature in the GUI.",
        Groups = "|MediaPortal GUI|Filtering|",
        Identifier = "enable_categories",
        Default = true,
        Hidden = false)]
    public bool CategoriesEnabled
    {
      get { return _categoriesEnabled; }
      set
      {
        _categoriesEnabled = value;
        OnSettingChanged("enable_categories");
      }
    }
    private bool _categoriesEnabled;

    [CornerstoneSetting(
        Name = "Dynamic Actor Category Limit",
        Description = "When an Actor based dynamic category is created, an actor must be in this number of musicvideos you own to be included in the list.",
        Groups = "|MediaPortal GUI|Filtering|",
        Identifier = "actor_limit",
        Default = 2)]
    public int ActorLimit
    {
      get { return _actorLimit; }
      set
      {
        _actorLimit = value;
        OnSettingChanged("actor_limit");
      }
    }
    private int _actorLimit;

    #endregion

    #region Debugging

    [CornerstoneSetting(
        Name = "Skinners Debug Mode",
        Description = "Enabling this will log all changes to skin properties when logging is set to debug.",
        Groups = "|Internal|",
        Identifier = "enable_debug_allskinproperties",
        Default = false)]
    public bool LogAllSkinPropertyChanges
    {
      get { return _logAllSkinPropertyChanges; }
      set
      {
        _logAllSkinPropertyChanges = value;
        OnSettingChanged("enable_debug_allskinproperties");
      }
    }
    private bool _logAllSkinPropertyChanges;

    [CornerstoneSetting(
        Name = "Disable Video Thumbnailer MTN Process",
        Description = "Enabling this option will prevent the running of of the Videothumbail process.",
        Groups = "|Internal|",
        Identifier = "disable_mtn",
        Default = false)]
    public bool DisableMTN
    {
      get { return _disablemtn; }
      set
      {
        _disablemtn = value;
        OnSettingChanged("disable_mtn");
      }
    }
    private bool _disablemtn;


    #endregion

    #endregion

    #region Extras
    #region File Renamer

    [CornerstoneSetting(
        Name = "File Rename Pattern",
        Description = "When using the file renamer functionality, musicvideo files will be renamed based on this pattern. The multi-part pattern will replace ${musicvideopart} if the musicvideo is made up of multipe files.",
        Groups = "|Extras|File Renamer|",
        Identifier = "file_rename_string",
        Default = @"${musicvideo.title} (${musicvideo.year})")]
    public string FileRenameString
    {
      get { return _fileRenameString; }
      set
      {
        _fileRenameString = value;
        OnSettingChanged("file_rename_string");
      }
    }
    private string _fileRenameString;

    [CornerstoneSetting(
        Name = "Directory Rename Pattern",
        Description = "When using the file renamer functionality, directories will be renamed based on this pattern.",
        Groups = "|Extras|File Renamer|",
        Identifier = "directory_rename_string",
        Default = @"${musicvideo.title} (${musicvideo.year})")]
    public string DirectoryRenameString
    {
      get { return _directoryRenameString; }
      set
      {
        _directoryRenameString = value;
        OnSettingChanged("directory_rename_string");
      }
    }
    private string _directoryRenameString;

    [CornerstoneSetting(
        Name = "Multi-Part Rename Pattern",
        Description = "When using the file renamer functionality, this filename pattern will be appended to the regular pattern for musicvideos made up of multiple files.",
        Groups = "|Extras|File Renamer|",
        Identifier = "file_multipart",
        Default = @" Part #")]
    public string FileMultipartString
    {
      get { return _file_multipart; }
      set
      {
        _file_multipart = value;
        OnSettingChanged("file_multipart");
      }
    }
    private string _file_multipart;

    [CornerstoneSetting(
         Name = "Additional Files Type to Rename",
         Description = "When using the file renamer functionality, files starting with the original filename and with one of these extension will be renamed too.",
         Groups = "|Extras|File Renamer|",
         Identifier = "file_rename_other_filetypes",
         Default = @".srt|.idx|.sub|.ac3|.dts|.nfo|.txt")]
    public string Rename_SecondaryFileTypes
    {
      get { return _file_rename_other_filetypes; }
      set
      {
        _file_rename_other_filetypes = value;
        OnSettingChanged("file_rename_other_filetypes");
      }
    }
    private string _file_rename_other_filetypes;

    [CornerstoneSetting(
         Name = "Include Folders When Renaming",
         Description = "If true, when renaming files for a musicvideo, the folder containing the files will be renamed as well.",
         Groups = "|Extras|File Renamer|",
         Identifier = "file_rename_folder",
         Default = true)]
    public bool RenameFolders
    {
      get { return _file_rename_folder; }
      set
      {
        _file_rename_folder = value;
        OnSettingChanged("file_rename_folder");
      }
    }
    private bool _file_rename_folder;

    [CornerstoneSetting(
      Name = "Include Files When Renaming",
      Description = "If true, when renaming, the musicvideo files will be renamed.",
      Groups = "|Extras|File Renamer|",
      Identifier = "rename_files",
      Default = true)]
    public bool RenameFiles
    {
      get { return _renameFiles; }
      set
      {
        _renameFiles = value;
        OnSettingChanged("rename_files");
      }
    }
    private bool _renameFiles;

    [CornerstoneSetting(
        Name = "Include Secondary Files When Renaming",
        Description = "If true, when renaming files for a musicvideo, thesecondary files will be renamed as well.",
        Groups = "|Extras|File Renamer|",
        Identifier = "rename_secondary_files",
        Default = true)]
    public bool RenameSecondaryFiles
    {
      get { return _renameSecondaryFiles; }
      set
      {
        _renameSecondaryFiles = value;
        OnSettingChanged("rename_secondary_files");
      }
    }
    private bool _renameSecondaryFiles;

    #region Last.FM Profile

    [CornerstoneSetting(
        Name = "Last.FM Username",
        Description = "Last.FM Username",
        Groups = "|Extras|Last.FM|",
        Identifier = "last_fm_username",
        Default = "")]
    public string LastFMUsername
    {
      get { return _lastfmusername; }
      set
      {
        _lastfmusername = value;
        OnSettingChanged("last_fm_username");
      }
    }
    private string _lastfmusername;

    [CornerstoneSetting(
        Name = "Last.FM Password",
        Description = "Last.FM Passowrd",
        Groups = "|Extras|Last.FM|",
        Identifier = "last_fm_password",
        Default = "")]
    public string LastFMPassword
    {
      get { return _lastfmpassword; }
      set
      {
        _lastfmpassword = value;
        OnSettingChanged("last_fm_password");
      }
    }
    private string _lastfmpassword;

    [CornerstoneSetting(
        Name = "Show Playing Video on Last.FM",
        Description = "Enabling this option will display the currently playing video on Last.FM",
        Groups = "|Extras|Last.FM|",
        Identifier = "show_on_lastfm",
        Default = true)]
    public bool ShowOnLastFM
    {
      get { return _showonlastfm; }
      set
      {
        _showonlastfm = value;
        OnSettingChanged("show_on_lastfm");
      }
    }
    private bool _showonlastfm;


    [CornerstoneSetting(
        Name = "Submit Playing Video to Last.FM Library",
        Description = "Enabling this option submit the currently playing video to your Last.FM Library",
        Groups = "|Extras|Last.FM|",
        Identifier = "submit_to_lastfm",
        Default = false)]
    public bool SubmitOnLastFM
    {
      get { return _submitonlastfm; }
      set
      {
        _submitonlastfm = value;
        OnSettingChanged("submit_to_lastfm");
      }
    }
    private bool _submitonlastfm;



    #endregion

    #endregion
    #endregion

    #region Internal Settings

    [CornerstoneSetting(
        Name = "Data Source Manager Enhanced Debug Mode",
        Description = "If set to true, additional logging will be written by the Scriptable Scraping Engine when the entire plug-in is in debug mode. Internal scripts stored in the DLL will also be reloaded on launch regardless of version number.",
        Groups = "|Internal|",
        Identifier = "source_manager_debug",
        Default = false,
        Hidden = false)]
    public bool DataSourceDebugActive
    {
      get { return _dataSourceManagerEnhancedDebugMode; }
      set
      {
        _dataSourceManagerEnhancedDebugMode = value;
        OnSettingChanged("source_manager_debug");
      }
    }
    private bool _dataSourceManagerEnhancedDebugMode;


    [CornerstoneSetting(
        Name = "Data Provider Manager Initialized",
        Description = "An internal flag to determine if an initial load of the Data Source Manager has been preformed.",
        Groups = "|Internal|",
        Identifier = "source_manager_init_done",
        Default = "True",
        Hidden = false)]
    public bool DataProvidersInitialized
    {
      get { return _dataProviderManagerInitialized; }
      set
      {
        _dataProviderManagerInitialized = value;
        OnSettingChanged("source_manager_init_done");
      }
    }
    private bool _dataProviderManagerInitialized;


    [CornerstoneSetting(
        Name = "Show Advanced Settings Warning",
        Description = "If set to false, the Advanced Settings warning screen will no longer be displayed when first clicking on the Advanced Settings tab.",
        Groups = "|Internal|",
        Identifier = "config_advanced_nag",
        Default = true)]
    public bool ShowAdvancedSettingsWarning
    {
      get { return _showAdvancedSettingsWarning; }
      set
      {
        _showAdvancedSettingsWarning = value;
        OnSettingChanged("config_advanced_nag");
      }
    }
    private bool _showAdvancedSettingsWarning;


    [CornerstoneSetting(
        Name = "Version Number",
        Description = "Version number of Music Videos. Used for database upgrade purposes, do not change.",
        Groups = "|Internal|",
        Identifier = "version",
        Default = "0.0.0.0",
        Hidden = false)]
    public string Version
    {
      get { return _versionNumber; }
      set
      {
        _versionNumber = value;
        OnSettingChanged("version");
      }
    }
    private string _versionNumber;

    [CornerstoneSetting(
        Name = "Allow Disk Monitor to Watch for Drive Changes",
        Description = "If disabled the disk monitor will not notify other aspects of the plug-in about disk events such as DVD insertions and newly connected network drives. Do not disable unless you are experiencing problems with the Disk Monitor.",
        Groups = "|Internal|",
        Identifier = "disk_monitor_enabled",
        Default = true)]
    public bool DeviceManagerEnabled
    {
      get { return _deviceManagerEnabled; }
      set
      {
        _deviceManagerEnabled = value;
        OnSettingChanged("disk_monitor_enabled");
      }
    }
    private bool _deviceManagerEnabled;

    [CornerstoneSetting(
        Name = "Upgrade warning message flag",
        Description = "This is set to False after the warning has been read and actioned",
        Groups = "|Internal|",
        Identifier = "do_upgrade_Warning",
        Default = true)]
    public bool UpgradeWarning
    {
      get { return _upgradewarning; }
      set
      {
        _upgradewarning = value;
        OnSettingChanged("do_upgrade_Warning");
      }
    }
    private bool _upgradewarning;

    
    #endregion


  }
}
