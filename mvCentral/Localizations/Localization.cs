using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Text.RegularExpressions;


using mvCentral.Utils; 


using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using NLog;

namespace mvCentral.Localizations
{
  public static class Localization
  {
    private static Logger logger = LogManager.GetCurrentClassLogger();

    #region Private variables

    private static Dictionary<string, string> _translations;
    private static readonly string _path = string.Empty;
    private static readonly DateTimeFormatInfo _info;

    private static Regex _isNumber = new Regex(@"^\d+$");

    #endregion

    #region Constructor

    static Localization()
    {
      try
      {
        Lang = GUILocalizeStrings.GetCultureName(GUILocalizeStrings.CurrentLanguage());
        _info = DateTimeFormatInfo.GetInstance(CultureInfo.CurrentUICulture);
      }
      catch (Exception)
      {
        Lang = CultureInfo.CurrentUICulture.Name;
        _info = DateTimeFormatInfo.GetInstance(CultureInfo.CurrentUICulture);
      }

      logger.Info("Using language: " + Lang);

      _path = MediaPortal.Configuration.Config.GetSubFolder(MediaPortal.Configuration.Config.Dir.Language, "mvCentral");

      if (!Directory.Exists(_path))
        Directory.CreateDirectory(_path);

      LoadTranslations();
    }
    #endregion

    #region Public Properties
    // Gets the language actually used (after checking for localization file and fallback).
    public static string Lang { get; private set; }
    public static Dictionary<string, string> FixedTranslations = new Dictionary<string, string>();

    /// <summary>
    /// Gets the translated strings collection in the active language
    /// </summary>
    public static Dictionary<string, string> Strings
    {
      get
      {
        if (_translations == null)
        {
          _translations = new Dictionary<string, string>();
          Type transType = typeof(Localization);
          FieldInfo[] fields = transType.GetFields(BindingFlags.Public | BindingFlags.Static);
          foreach (FieldInfo field in fields)
          {
            _translations.Add(field.Name, field.GetValue(transType).ToString());
          }
        }
        return _translations;
      }
    }
    #endregion

    #region Private methods
    private static int LoadTranslations()
    {
      XmlDocument doc = new XmlDocument();
      Dictionary<string, string> TranslatedStrings = new Dictionary<string, string>();
      string langPath = "";
      try
      {
        langPath = Path.Combine(_path, Lang + ".xml");
        doc.Load(langPath);
      }
      catch (Exception e)
      {
        if (Lang == "en-US")
          return 0; // otherwise we are in an endless loop!

        if (e.GetType() == typeof(FileNotFoundException))
          logger.Warn("Cannot find translation file {0}. Failing back to English (US)", langPath);
        else
        {
          logger.Error("Error in translation xml file: {0}. Failing back to English (US)", Lang);
          logger.Error(e);
        }

        Lang = "en-US";
        return LoadTranslations();
      }
      foreach (XmlNode stringEntry in doc.DocumentElement.ChildNodes)
      {
        if (stringEntry.NodeType == XmlNodeType.Element)
          try
          {
            if (stringEntry.Attributes.GetNamedItem("Field").Value.StartsWith("#"))
            {
              FixedTranslations.Add(stringEntry.Attributes.GetNamedItem("Field").Value, stringEntry.InnerText);
            }
            else
              TranslatedStrings.Add(stringEntry.Attributes.GetNamedItem("Field").Value, stringEntry.InnerText);
          }
          catch (Exception e)
          {
            logger.Error("Error in Translation Engine:");
            logger.Error(e);
          }
      }

      Type TransType = typeof(Localization);
      FieldInfo[] fieldInfos = TransType.GetFields(BindingFlags.Public | BindingFlags.Static);
      foreach (FieldInfo fi in fieldInfos)
      {
        if (fi.Name == "FixedTranslations") // Skip this as invalid, picked as it is a public static string
          continue;

        if (TranslatedStrings != null && TranslatedStrings.ContainsKey(fi.Name))
          TransType.InvokeMember(fi.Name, BindingFlags.SetField, null, TransType, new object[] { TranslatedStrings[fi.Name] });
        else
          logger.Info("Translation not found for field: {0}. Using hard-coded English default.", fi.Name);
      }
      return TranslatedStrings.Count;
    }
    #endregion

    #region Public Methods
    public static void Init()
    {
    }

    public static string GetByName(string name)
    {
      if (!Strings.ContainsKey(name))
        return name;

      return Strings[name];
    }

    public static string GetByName(string name, params object[] args)
    {
      return string.Format(GetByName(name), args);
    }

    /// <summary>
    /// Takes an input string and replaces all ${named} variables with the proper translation if available
    /// </summary>
    /// <param name="input">a string containing ${named} variables that represent the translation keys</param>
    /// <returns>translated input string</returns>
    public static string ParseString(string input)
    {
      Regex replacements = new Regex(@"\$\{([^\}]+)\}");
      MatchCollection matches = replacements.Matches(input);
      foreach (Match match in matches)
      {
        input = input.Replace(match.Value, GetByName(match.Groups[1].Value));
      }
      return input;
    }

    public static void TranslateSkin()
    {
      logger.Info("Translation: Translating skin");
      foreach (string name in Localization.Strings.Keys)
      {
        if (name != "FixedTranslations")
          GUIUtils.SetProperty("#mvCentral.Translation." + name + ".Label", Localization.Strings[name], true);
      }

      logger.Info("Translation: Translating Mediaportal Localised IDs");
      foreach (string propName in Localization.FixedTranslations.Keys)
      {
        if (!string.IsNullOrEmpty(propName))
        {
          string propValue;
          Localization.FixedTranslations.TryGetValue(propName, out propValue);
          if (IsInteger(propValue))
          {
            GUIUtils.SetProperty(propName + ".Label", GUILocalizeStrings.Get(int.Parse(propValue)), true);
            logger.Debug("Set Localised IDs" + propName + ".Label to " + GUILocalizeStrings.Get(int.Parse(propValue)));
          }
          else
          {
            logger.Info(propName + ": " + propValue);
            GUIUtils.SetProperty(propName, propValue, true);
          }
        }
      }
    }
    /// <summary>
    /// Return true if Interger
    /// </summary>
    /// <param name="theValue"></param>
    /// <returns></returns>
    static bool IsInteger(string theValue)
    {
      if (string.IsNullOrEmpty(theValue)) return false;
      Match m = _isNumber.Match(theValue);
      return m.Success;
    }

    public static string GetDayName(DayOfWeek dayOfWeek)
    {
      return _info.GetDayName(dayOfWeek);
    }

    public static string GetShortestDayName(DayOfWeek dayOfWeek)
    {
      return _info.GetShortestDayName(dayOfWeek);
    }
    #endregion

    #region Translations / Strings
    /// <summary>
    /// These will be loaded with the language files content
    /// if the selected lang file is not found, it will first try to load en(us).xml as a backup
    /// if that also fails it will use the hardcoded strings as a last resort.
    /// </summary>

    // #

    // A
    public static string About = "About";
    public static string AboutText = "mvCentral is a Music Videos plug-in for the MediaPortal HTPC application.";
    public static string Artist = "Artist";
    public static string Artists = "Artists";
    public static string Album = "Album";
    public static string Albums = "Albums";
    public static string AllVideos = "All Videos";
    public static string AddToPlaylist = "Add To Playlist";
    public static string AddAllToPlaylist = "Add All To Playlist";
    public static string AddToPlaylistNext = "Add To Playlist as Next Item";

    // B
    public static string Back = "Back";
    public static string ButtonAutoPlay = "Auto Play";

    // C
    public static string Cancel = "Cancel";
    public static string CannotLoadSkin = "Could not load the skin\nfile for mvCentral!";
    public static string ContinueToNextPartBody = "Do you wish to continue with part {0}?";
    public static string ContinueToNextPartHeader = "Continue to next part?";
    public static string ContextMenu = "Context Menu";
    public static string ConfigGenre = "Configure Genres";

    // D
    public static string Date = "Date";
    public static string DateAdded = "Date Added";
    public static string DBInfo = "Watched History Statistics & Database Info";

    // E
    public static string Error = "Error";

    // F
    public static string FailedMountingImage = "Sorry, failed mounting DVD Image";
    public static string FavouriteVideos = "Favourite Videos";

    // G
    public static string Genre = "Genre";

    // H
    public static string HighestRated = "Highest Rated";
    public static string History = "History";

    // I
    public static string Initializing = "Initializing";
    public static string Info = "Info";

    // L
    public static string LatestVideos = "Latest Videos";
    public static string LeastPlayed = "Least Played";
    public static string LeastPlayedArtists = "Least Played Artists";
    public static string LeastPlayedVideos = "Least Played Videos";

    // M
    public static string MediaIsMissing = "The media for the Music video you have selected is missing!\nVery sorry but something has gone wrong...";
    public static string MediaNotAvailableBody = "The media for the Music video you have selected is not\ncurrently available. Please insert or connect media\nlabeled: {0}";
    public static string MediaNotAvailableHeader = "Media Not Available";
    public static string MissingExternalPlayerExe = "The executable for HD playback is missing.\nPlease correct the path to the executable.";
    public static string MostPlayedArtists = "Most Played Artists";
    public static string MostPlayedVideos = "Most Played Videos";
    public static string MostPlayedArtist = "Most Played Artist";
    public static string MostPlayedVideo = "Most Played Video";

    // N
    public static string NoPlaylistsFound = "No Playlists found in:";
    public static string NoArtistBio = "No Biography Available for Artist {0}";
    public static string NoTrackInfo = "No Track Description Available";

    // O
    public static string OK = "OK";

    // P
    public static string PlaybackFailed = "Playback is not possible because the '{0}'\nextension is not listed in your mediaportal configuration.\nPlease add this extension or setup an external player\nand try again.";
    public static string PlaybackFailedHeader = "Playback Failed";
    public static string PlayAllRandom = "Play All (Random)";
    public static string Playlist = "Playlist";
    public static string PlayByTag = "Play By Tag";
    public static string PlayByGenre = "Play By Genre";

    // Q

    // R
    public static string Random = "Random";
    public static string Retry = "Retry";
    public static string ResumeFrom = "Resume from:";
    public static string ResumeFromLast = "Resume Music Video from last time?";
    public static string RefreshArtwork = "Refresh Artwork from Online Sources";

    // S
    public static string SmartPlaylistOptions = "Smart Playlists";
    public static string StatsAndInfo = "Stats and Info";
    public static string SmartPlaylistTag = "Select Tag for Playlist";
    public static string TagsToGenre = "Select Tags to use as Genre";
 
    // T
    public static string TotalRuntime = "Total Runtime";
    public static string TopTenVideos = "Top Ten Videos";
    public static string Tracks = "Tracks";

    // U

    // V
    public static string VirtualDriveHeader = "Virtual drive not ready";
    public static string VirtualDriveMessage = "The virtual drive wasn't ready in time.\nPlease try again or cancel playback.";
    public static string Video = "Video";
    public static string Videos = "Videos";
    public static string VideoTitle = "Video Title";
    public static string VideoCount = "Database has {0} Videos across {1} Artists";
    public static string ViewAs = "View by ";

    // W

    //User Messages
    //                                    |23456789|23456789|23456789|23456789|23456789|23456
    public static string NoGenreHeader = "No Genres configured ";
    public static string NoGenreLine1  = "Select tags to be use as Genres from";
    public static string NoGenreLine2  = "the 'Configure Genres' option on the menu. ";
    public static string NoGenreLine3  = "Artist view will now be selected. ";

   
    #endregion
  }
}