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
//            if (SettingsManager.Properties == null || SettingsManager.Properties.GUISettings == null) return "dlCentral";
//            return string.IsNullOrEmpty(SettingsManager.Properties.GUISettings.PluginName) ? "dlCentral" : SettingsManager.Properties.GUISettings.PluginName;
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
