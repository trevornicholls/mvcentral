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
 
namespace mvCentral.Localizations {
    public static class Localization {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #region Private variables
        private static Dictionary<string, string> _translations;
        private static readonly string _path = string.Empty;
        private static readonly DateTimeFormatInfo _info;
        #endregion
         
        #region Constructor
        static Localization() { 
            try {
                Lang = GUILocalizeStrings.GetCultureName(GUILocalizeStrings.CurrentLanguage());
                _info = DateTimeFormatInfo.GetInstance(CultureInfo.CurrentUICulture);
            }
            catch (Exception) {
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

        /// <summary>
        /// Gets the translated strings collection in the active language
        /// </summary>
        public static Dictionary<string, string> Strings {
            get {
                if (_translations == null) {
                    _translations = new Dictionary<string, string>();
                    Type transType = typeof(Localization);
                    FieldInfo[] fields = transType.GetFields(BindingFlags.Public | BindingFlags.Static);
                    foreach (FieldInfo field in fields) {
                        _translations.Add(field.Name, field.GetValue(transType).ToString());
                    }
                }
                return _translations;
            }
        }
        #endregion

        #region Private methods
        private static int LoadTranslations() {
            XmlDocument doc = new XmlDocument();
            Dictionary<string, string> TranslatedStrings = new Dictionary<string, string>();
            string langPath = "";
            try {
                langPath = Path.Combine(_path, Lang + ".xml");
                doc.Load(langPath);
            }
            catch (Exception e) {
                if (Lang == "en-US")
                    return 0; // otherwise we are in an endless loop!

                if (e.GetType() == typeof(FileNotFoundException))
                    logger.Warn("Cannot find translation file {0}. Failing back to English (US)", langPath);
                else {
                    logger.Error("Error in translation xml file: {0}. Failing back to English (US)", Lang);
                    logger.Error(e);
                }

                Lang = "en-US";
                return LoadTranslations();
            }
            foreach (XmlNode stringEntry in doc.DocumentElement.ChildNodes) {
                if (stringEntry.NodeType == XmlNodeType.Element)
                    try {
                        TranslatedStrings.Add(stringEntry.Attributes.GetNamedItem("Field").Value, stringEntry.InnerText);
                    }
                    catch (Exception e) {
                        logger.Error("Error in Translation Engine:");
                        logger.Error(e);
                    }
            }

            Type TransType = typeof(Localization);
            FieldInfo[] fieldInfos = TransType.GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (FieldInfo fi in fieldInfos) {
                if (TranslatedStrings != null && TranslatedStrings.ContainsKey(fi.Name))
                    TransType.InvokeMember(fi.Name, BindingFlags.SetField, null, TransType, new object[] { TranslatedStrings[fi.Name] });
                else
                    logger.Info("Translation not found for field: {0}. Using hard-coded English default.", fi.Name);
            }
            return TranslatedStrings.Count;
        }
        #endregion

        #region Public Methods
        public static void Init() {
        }

        public static string GetByName(string name) {
            if (!Strings.ContainsKey(name))
                return name;

            return Strings[name];
        }

        public static string GetByName(string name, params object[] args) {
            return string.Format(GetByName(name), args);
        }

        /// <summary>
        /// Takes an input string and replaces all ${named} variables with the proper translation if available
        /// </summary>
        /// <param name="input">a string containing ${named} variables that represent the translation keys</param>
        /// <returns>translated input string</returns>
        public static string ParseString(string input) {
            Regex replacements = new Regex(@"\$\{([^\}]+)\}");
            MatchCollection matches = replacements.Matches(input);
            foreach (Match match in matches) {
                input = input.Replace(match.Value, GetByName(match.Groups[1].Value));
            }
            return input;
        }

        public static void TranslateSkin() {
            logger.Info("Translation: Translating skin");
            foreach (string name in Localization.Strings.Keys) {
                if (name.StartsWith("SkinTranslation")) {
                    GUIUtils.SetProperty("#mvCentral.Translation." + name.Replace("SkinTranslation", "") + ".Label", Localization.Strings[name], true);
                }
            }
        }

        public static string GetDayName(DayOfWeek dayOfWeek) {
            return _info.GetDayName(dayOfWeek);
        }

        public static string GetShortestDayName(DayOfWeek dayOfWeek) {
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
        public static string AboutText = "SubCentral is a download plug-in for the MediaPortal HTPC application." + 
            "The purpose of the plug-in is to allow user to download movies/series for movies or TV shows managed" +
            " by popular Moving Pictures and MP-TVSeries plug-ins. Best way to use the plug-in is by opening" +
            " Moving Pictures or MP-TVSeries plug-in, browsing to movie/TV show and selecting 'Download' item in (Hidden) menu." +
            " Please use the MediaPortal configuration to setup the plug-in more thoroughly. After the search is done, you can" +
            " sort the results using the menu item and of course, download the movie/serie. For more information, visit us" +
            " on our Google Code Download Central project page!"; // will be defined in XML file

        public static string Artists = "Artists";

        // B
        public static string Back = "Back";
        public static string ButtonAutoPlay = "Auto Play";

        // C
        public static string Cancel = "Cancel";
        public static string CannotLoadSkin = "Could not load the skin\nfile for mvCentral!";
        public static string ContinueToNextPartBody = "Do you wish to continue with part {0}?";
        public static string ContinueToNextPartHeader = "Continue to next part?";

        // D
        public static string DownloadS = "Download(s)";
        public static string Date = "Date";
        public static string DownloadStatus = "Download status";
        public static string DownloadQueued = "Download Queued!";
        public static string DownloadSearch = "Download search";

        // E
        public static string Error = "Error";
        public static string ErrorWhileRetrievingDownloads = "Error retrieving Downloads!";
        public static string ErrorConnection = "Error connecting to : {0}";
        public static string ErrorDownloadNotQueued = "Error download not Queued!";

        // F
        public static string FailedMountingImage = "Sorry, failed mounting DVD Image";

        // G

        // H

        // I
        public static string Initializing = "Initializing";
        public static string Info = "Info";

        // L

        // M
 
        public static string MediaIsMissing = "The media for the Music video you have selected is missing!\nVery sorry but something has gone wrong...";
        public static string MediaNotAvailableBody = "The media for the Music videoe you have selected is not\ncurrently available. Please insert or connect media\nlabeled: {0}";
        public static string MediaNotAvailableHeader = "Media Not Available";
        public static string MissingExternalPlayerExe = "The executable for HD playback is missing.\nPlease correct the path to the executable.";
 
        // N
        public static string NoResultsFound = "No results found";
        public static string Name = "Name";
        public static string NoDownloadsActive = "No downloads active";
        public static string NoPlaylistsFound = "No Playlists found in:";

        // O
        public static string OK = "OK";

        // P
        public static string PlaybackFailed = "Playback is not possible because the '{0}'\nextension is not listed in your mediaportal configuration.\nPlease add this extension or setup an external player\nand try again.";
        public static string PlaybackFailedHeader = "Playback Failed";

        // Q

        // R
        public static string Retry = "Retry";
        public static string ResumeFrom = "Resume from:";
        public static string ResumeFromLast = "Resume muicvideo from last time?";
        
        // S
        public static string Sites = "Site(s)";
        public static string SearchingSite = "Searching Site...";
        public static string Sorting = "Sorting";
        public static string Sort = "Sort";
        public static string SortBy = "Sort by: {0}";
        public static string SortByName = "Sort by Name";
        public static string SortByDate = "Sort by Date";
        public static string Status = "Status";

        // T

        public static string TorrentSearch = "Torrent Search";

        // U

        // V
        public static string VirtualDriveHeader = "Virtual drive not ready";
        public static string VirtualDriveMessage = "The virtual drive wasn't ready in time.\nPlease try again or cancel playback.";
        public static string Videos = "Videos";


        // W
        #endregion
    }
}