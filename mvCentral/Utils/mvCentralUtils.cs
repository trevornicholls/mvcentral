using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Text.RegularExpressions;
using NLog;
using System.Xml;
using System.Xml.Schema;
using System.IO;
using System.Net;
using System.Web;
using System.Drawing;




using DirectShowLib.Dvd;

using mvCentral.LocalMediaManagement;
//using dlCentral.PluginHandlers;
//using dlCentral.Settings;
//using dlCentral.Settings.Data;
using Cornerstone.Tools;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Ripper;

namespace mvCentral.Utils {
    public static class mvCentralUtils {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public static readonly string SettingsFileName = "mvCentral.xml";
        public static string LogFileName = "mvCentral.log";
        public static string OldLogFileName = "mvCentral.log.bak";
        private static readonly object syncRoot = new object();

        public static string PluginName() {
            return "mvCentral";
//            if (SettingsManager.Properties == null || SettingsManager.Properties.GUISettings == null) return "mvCentral";
//            return string.IsNullOrEmpty(SettingsManager.Properties.GUISettings.PluginName) ? "mvCentral" : SettingsManager.Properties.GUISettings.PluginName;
        }


        public static bool IsAssemblyAvailable(string name, Version ver) {
            return IsAssemblyAvailable(name, ver, false);
        }

        public static bool IsAssemblyAvailable(string name, Version ver, bool reflectionOnly) {
            Assembly[] assemblies = null;

            if (!reflectionOnly) 
                assemblies = AppDomain.CurrentDomain.GetAssemblies();
            else
                assemblies = AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies();

            if (assemblies != null) {
                foreach (Assembly a in assemblies) {
                    try {
                        if (a.GetName().Name == name && a.GetName().Version >= ver)
                            return true;
                    }
                    catch (Exception e) {
                        logger.ErrorException(string.Format("Assembly.GetName() call failed for '{0}'!\n", a.Location), e);
                    }
                }
            }

            return false;
        }

        public static bool IsValidAlphaNumeric(string inputStr) {
            if (string.IsNullOrEmpty(inputStr))
                return false;

            for (int i = 0; i < inputStr.Length; i++) {
                if (!(char.IsLetter(inputStr[i])) && (!(char.IsNumber(inputStr[i]))))
                    return false;
            }
            return true;
        }

        public static string TrimNonAlphaNumeric(string inputStr) {
            string result = string.Empty;

            if (string.IsNullOrEmpty(inputStr))
                return result;

            for (int i = 0; i < inputStr.Length; i++) {
                if (char.IsLetter(inputStr[i]) || char.IsNumber(inputStr[i]))
                    result += inputStr[i];
            }
            return result;
        }

        public static WebGrabber GetWebGrabberInstance(string url)
        {
            WebGrabber grabber = new WebGrabber(url);
            grabber.UserAgent = "musicvideos/" + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            return grabber;
        }

        //Download the file, save it as text
        public static XmlDocument Get(string url)
        {

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(@url);
            req.Method = "GET";
            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            using (StreamReader sr = new StreamReader(res.GetResponseStream()))
            {
                string xmlData = sr.ReadToEnd();
                xmlData = xmlData.Replace("\n", Environment.NewLine);
                if (File.Exists(@"C:\tmp.xml"))
                    File.Delete(@"C:\tmp.xml");
                File.WriteAllText(@"C:\tmp.xml", xmlData);
            }
            return clean();
        }

        //scrub and read it
        private static XmlDocument clean()
        {
            List<char> charsToSubstitute = new List<char>();
            charsToSubstitute.Add((char)0x19);
            charsToSubstitute.Add((char)0x1C);
            charsToSubstitute.Add((char)0x1D);

            string fileText = File.ReadAllText(@"C:\tmp.xml");
            foreach (char c in charsToSubstitute)
                fileText = fileText.Replace(Convert.ToString(c), string.Empty);

            XmlDocument doc = new XmlDocument();
            MemoryStream ms = new MemoryStream(System.Text.ASCIIEncoding.UTF8.GetBytes(fileText));
            XmlTextReader reader = new XmlTextReader(ms);
            doc.Load(reader);
            if (File.Exists(@"C:\tmp.xml"))
                File.Delete(@"C:\tmp.xml");
            return doc;
        }



        public static bool IsGuid(string guid)
        {
            try
            {
                Guid id = new Guid(guid.ToString());
            }
            catch (FormatException)
            {
                return false;
            }
            return true;
        }

        public static TimeSpan ConvertToTimeSpan(DvdHMSFTimeCode t)
        {
            string s = String.Format("{0:00}:{1:00}:{2:00}", t.bHours, t.bMinutes, t.bSeconds);
            TimeSpan result = TimeSpan.Parse(s);
            return result;

        }

        public static DvdHMSFTimeCode ConvertToDvdHMSFTimeCode(TimeSpan t)
        {
            DvdHMSFTimeCode result = new DvdHMSFTimeCode();
            result.bHours = (byte)t.Hours;
            result.bMinutes = (byte)t.Minutes;
            result.bSeconds = (byte)t.Seconds;
            return result;
        }

        public static string StripHTML(string source)
        {
          try
          {
            string result;

            // Remove HTML Development formatting
            // Replace line breaks with space
            // because browsers inserts space
            result = source.Replace("\r", " ");
            // Replace line breaks with space
            // because browsers inserts space
            result = result.Replace("\n", " ");
            // Remove step-formatting
            result = result.Replace("\t", string.Empty);
            // Remove repeating spaces because browsers ignore them
            result = System.Text.RegularExpressions.Regex.Replace(result,
                                                                  @"( )+", " ");

            // Remove the header (prepare first by clearing attributes)
            result = System.Text.RegularExpressions.Regex.Replace(result,
                     @"<( )*head([^>])*>", "<head>",
                     System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                     @"(<( )*(/)( )*head( )*>)", "</head>",
                     System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                     "(<head>).*(</head>)", string.Empty,
                     System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // remove all scripts (prepare first by clearing attributes)
            result = System.Text.RegularExpressions.Regex.Replace(result,
                     @"<( )*script([^>])*>", "<script>",
                     System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                     @"(<( )*(/)( )*script( )*>)", "</script>",
                     System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            //result = System.Text.RegularExpressions.Regex.Replace(result,
            //         @"(<script>)([^(<script>\.</script>)])*(</script>)",
            //         string.Empty,
            //         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                     @"(<script>).*(</script>)", string.Empty,
                     System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // remove all styles (prepare first by clearing attributes)
            result = System.Text.RegularExpressions.Regex.Replace(result,
                     @"<( )*style([^>])*>", "<style>",
                     System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                     @"(<( )*(/)( )*style( )*>)", "</style>",
                     System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                     "(<style>).*(</style>)", string.Empty,
                     System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // insert tabs in spaces of <td> tags
            result = System.Text.RegularExpressions.Regex.Replace(result,
                     @"<( )*td([^>])*>", "\t",
                     System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // insert line breaks in places of <BR> and <LI> tags
            result = System.Text.RegularExpressions.Regex.Replace(result,
                     @"<( )*br( )*>", "\r",
                     System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                     @"<( )*li( )*>", "\r",
                     System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // insert line paragraphs (double line breaks) in place
            // if <P>, <DIV> and <TR> tags
            result = System.Text.RegularExpressions.Regex.Replace(result,
                     @"<( )*div([^>])*>", "\r\r",
                     System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                     @"<( )*tr([^>])*>", "\r\r",
                     System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                     @"<( )*p([^>])*>", "\r\r",
                     System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // Remove remaining tags like <a>, links, images,
            // comments etc - anything that's enclosed inside < >
            result = System.Text.RegularExpressions.Regex.Replace(result,
                     @"<[^>]*>", string.Empty,
                     System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // replace special characters:
            result = System.Text.RegularExpressions.Regex.Replace(result,
                     @" ", " ",
                     System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            result = System.Text.RegularExpressions.Regex.Replace(result,
                     @"&bull;", " * ",
                     System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                     @"&lsaquo;", "<",
                     System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                     @"&rsaquo;", ">",
                     System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                     @"&trade;", "(tm)",
                     System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                     @"&frasl;", "/",
                     System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                     @"&lt;", "<",
                     System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                     @"&gt;", ">",
                     System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                     @"&copy;", "(c)",
                     System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                     @"&reg;", "(r)",
                     System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            // Remove all others. More can be added, see
            // http://hotwired.lycos.com/webmonkey/reference/special_characters/
            result = System.Text.RegularExpressions.Regex.Replace(result,
                     @"&(.{2,6});", string.Empty,
                     System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // for testing
            //System.Text.RegularExpressions.Regex.Replace(result,
            //       this.txtRegex.Text,string.Empty,
            //       System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // make line breaking consistent
            result = result.Replace("\n", "\r");

            // Remove extra line breaks and tabs:
            // replace over 2 breaks with 2 and over 4 tabs with 4.
            // Prepare first to remove any whitespaces in between
            // the escaped characters and remove redundant tabs in between line breaks
            result = System.Text.RegularExpressions.Regex.Replace(result,
                     "(\r)( )+(\r)", "\r\r",
                     System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                     "(\t)( )+(\t)", "\t\t",
                     System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                     "(\t)( )+(\r)", "\t\r",
                     System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                     "(\r)( )+(\t)", "\r\t",
                     System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            // Remove redundant tabs
            result = System.Text.RegularExpressions.Regex.Replace(result,
                     "(\r)(\t)+(\r)", "\r\r",
                     System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            // Remove multiple tabs following a line break with just one tab
            result = System.Text.RegularExpressions.Regex.Replace(result,
                     "(\r)(\t)+", "\r\t",
                     System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            // Initial replacement target string for line breaks
            string breaks = "\r\r\r";
            // Initial replacement target string for tabs
            string tabs = "\t\t\t\t\t";
            for (int index = 0; index < result.Length; index++)
            {
              result = result.Replace(breaks, "\r\r");
              result = result.Replace(tabs, "\t\t\t\t");
              breaks = breaks + "\r";
              tabs = tabs + "\t";
            }

            // That's it.
            return result;
          }
          catch
          {
            return source;
          }
        }

        /// <summary>
        /// Resize the grabbed image, with is fixed and ascpect ratio mantained
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="outputFileName"></param>
        /// <param name="newWidth"></param>
        public static void ResizeImageWithAspect(string fileName, string outputFileName, int newWidth)
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


        public static void ResizeImage(string OriginalFile, string NewFile, int NewWidth, int MaxHeight, bool OnlyResizeIfWider)
        {
          
          System.Drawing.Image FullsizeImage = System.Drawing.Image.FromFile(OriginalFile);

          // Prevent using images internal thumbnail
          FullsizeImage.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);
          FullsizeImage.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);

          if (OnlyResizeIfWider)
          {
            if (FullsizeImage.Width <= NewWidth)
            {
              NewWidth = FullsizeImage.Width;
            }
          }

          int NewHeight = FullsizeImage.Height * NewWidth / FullsizeImage.Width;
          if (NewHeight > MaxHeight)
          {
            // Resize with height instead
            NewWidth = FullsizeImage.Width * MaxHeight / FullsizeImage.Height;
            NewHeight = MaxHeight;
          }

          System.Drawing.Image NewImage = FullsizeImage.GetThumbnailImage(NewWidth, NewHeight, null, IntPtr.Zero);

          // Clear handle to original file so that we can overwrite it if necessary
          FullsizeImage.Dispose();

          // Save resized picture
          NewImage.Save(NewFile);
          NewImage.Dispose();
          NewImage = null;
          FullsizeImage = null; ;
        }


        #region Mounting

        // note: These methods are wrappers around the Daemon Tools class supplied by mediaportal
        // we use this wrappers so we can move to other mounting logic in the future more easily.

        public static MountResult MountImage(string imagePath)
        {
            // Max cycles to try/wait when the virtual drive is not ready
            // after mounting it. One cycle is roughly 1/10 of a second 
            return Utility.MountImage(imagePath, 100);
        }

        public static MountResult MountImage(string imagePath, int maxWaitCycles)
        {
            string drive;

            // Check if the current image is already mounted
            if (!Utility.IsMounted(imagePath))
            {
                logger.Info("Mounting image...");
                if (!DaemonTools.Mount(imagePath, out drive))
                {
                    // there was a mounting error
                    logger.Error("Mounting image failed.");
                    return MountResult.Failed;
                }
            }
            else
            {
                // if the image was already mounted grab the drive letter
                drive = DaemonTools.GetVirtualDrive();
                // only check the drive once before reporting that the mounting is still pending
                maxWaitCycles = 1;
            }

            // Check if the mounted drive is ready to be read
            logger.Info("Mounted: Image='{0}', Drive={1}", imagePath, drive);

            int driveCheck = 0;
            while (true)
            {
                driveCheck++;
                // Try to create a DriveInfo object with the returned driveletter
                try
                {
                    DriveInfo d = new DriveInfo(drive);
                    if (d.IsReady)
                    {
                        // This line will list the complete file structure of the image
                        // Output will only show when the log is set to DEBUG.
                        // Purpose of method is troubleshoot different image structures.
                        Utility.LogDirectoryStructure(drive);
                        return MountResult.Success;
                    }
                }
                catch (ArgumentNullException e)
                {
                    // The driveletter returned by Daemon Tools is invalid
                    logger.DebugException("Daemon Tools returned an invalid driveletter", e);
                    return MountResult.Failed;
                }
                catch (ArgumentException)
                {
                    // this exception happens when the driveletter is valid but the driveletter is not 
                    // finished mounting yet (at least not known to the system). We only need to catch
                    // this to stay in the loop
                }

                if (driveCheck == maxWaitCycles)
                {
                    return MountResult.Pending;
                }
                else if (maxWaitCycles == 1)
                {
                    logger.Info("Waiting for virtual drive to become available...");
                }

                // Sleep for a bit
                Thread.Sleep(100);
            }
        }




        public static bool IsImageFile(string imagePath)
        {
            return DaemonTools.IsImageFile(Path.GetExtension(imagePath));
        }

        public static bool IsMounted(string imagePath)
        {
            return DaemonTools.IsMounted(imagePath);
        }

        public static void UnMount(string imagePath)
        {
            if (IsMounted(imagePath))
            {
                DaemonTools.UnMount();
                logger.Info("Unmounted: Image='{0}'", imagePath);
            }
        }

        public static string GetMountedVideoDiscPath(string imagePath)
        {
            if (!IsMounted(imagePath))
                return null;

            string drive = DaemonTools.GetVirtualDrive();
            return VideoUtility.GetVideoPath(drive);
        }


        public static void disableNativeAutoplay()
        {
            logger.Info("Disabling native autoplay.");
            AutoPlay.StopListening();
        }

        public static void enableNativeAutoplay()
        {
            if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
            {
                logger.Info("Re-enabling native autoplay.");
                AutoPlay.StartListening();
            }
        }
        #endregion


    }

}
