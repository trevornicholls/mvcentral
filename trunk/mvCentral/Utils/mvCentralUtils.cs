using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using NLog;
using System.Xml;
using System.Xml.Schema;
using System.IO;
using System.Net;
using System.Web;



using DirectShowLib.Dvd;
//using dlCentral.PluginHandlers;
//using dlCentral.Settings;
//using dlCentral.Settings.Data;
using Cornerstone.Tools;

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

    }

}
