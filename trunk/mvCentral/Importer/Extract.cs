using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MusicVideos;
using NLog;

namespace MusicVideos.Importer
{
    static class Extract
    {
        private static Logger logger = LogManager.GetCurrentClassLogger(); 
        private static string RemoveLeading(string Path, char leadingChars)
        {
            while (Path[0] == leadingChars)
                Path = Path.Substring(1, Path.Length - 1);
            return Path;
        }
        
        public static string[] GetData(string fullPath, string mtchExp, string watchFolderPath)
        {
            string extensions = MusicVideosCore.dm.getExtensions();
            string parsedFile = RemoveLeading(fullPath.Replace(watchFolderPath, ""), '\\');
            string[] matchFolders = mtchExp.Split('\\');
            string[] realFolders = parsedFile.Split('\\');
            try
            {
                //Is it within the extensions we want?
                if (parsedFile.Split('\\').Length == mtchExp.Split('\\').Length)
                {
                    //Are we at the right depth yet?
                    if (extensions.Contains(fullPath.Split('.')[fullPath.Split('.').Length-1].ToLower()))
                    {
                        //init the holder vars
                        string artist = "", title = "", album = "";    
                        //Find which folder relates to which label
                        int count = 0;
                        //See if parsing expression has a folder depth
                        if (realFolders.Length > 1)
                        {
                            foreach (string folder in matchFolders)
                            {
                                if (folder == "%artist%")
                                {
                                    artist = realFolders[count].Trim();
                                    break;
                                }
                                count++;
                            }
                            count = 0;
                            //Now get the album
                            foreach (string folder in matchFolders)
                            {
                                if (folder == "%album%")
                                {
                                    album = realFolders[count].Trim();
                                    break;
                                }
                                count++;
                            }
                            
                            //Get the real filename, and the parsing epxression for the file                     
                            string fileName = RemoveLeading(parsedFile.Substring(parsedFile.LastIndexOf('\\'), parsedFile.Length - parsedFile.LastIndexOf('\\')), '\\');                           
                            string expFilename = RemoveLeading(mtchExp.Substring(mtchExp.LastIndexOf('\\'), mtchExp.Length - mtchExp.LastIndexOf('\\')), '\\');
                            
                            //Is it nice and easy title.ext?
                            if (expFilename == "%title%.%ext%")
                                title = fileName.Split('.')[0];
                            else
                            {
                                //If not, we need something better than this. This assumes somethin - title.ext
                                string[] splitTitle = SplitByString(fileName, " - ");
                                //If artist has been identified, ignore. if not assume aritist - title.ext and get artist
                                if (artist == "")
                                    artist = splitTitle[0];
                                title = splitTitle[1].Split('.')[0];
                            }
                        }
                        else
                        {
                            //No folders in matching expression
                            string[] masks = mtchExp.Split('%');
                            string sep = masks[2];
                            //Remove any \'s, should be leading
                            parsedFile = RemoveLeading(parsedFile, '\\');
                            //Split the string by the seperator
                            masks = SplitByString(parsedFile, sep);
                            //Assign artist and title
                            if (mtchExp.Substring(0, 8) == "%artist%")
                            {
                                artist = masks[0].Trim();
                                title = masks[1].Substring(0, masks[1].LastIndexOf('.')).Trim().Replace(".", "");
                            }
                            else
                            {
                                title = masks[1].Trim();
                                artist = masks[0].Substring(0, masks[1].LastIndexOf('.')).Trim().Replace(".", "");
                            }
                        }
                        //Output here
                        return new string[] { artist, title, fullPath, parsedFile };
                    }
                }
            }
            catch (Exception e)
            {
                //Error output here, song doesnt match Criteria
                logger.Info("Had trouble processing file " + fullPath + " ERROR: " + e.ToString());
                return new string[] { "ERROR", "ERROR", fullPath, fullPath };
            }
            return null;
        }

        private static string[] SplitByString(string testString, string split) //Func to split a string by a string
        {
            int offset = 0;
            int index = 0;
            int[] offsets = new int[testString.Length + 1];

            while (index < testString.Length)
            {
                int indexOf = testString.IndexOf(split, index);
                if (indexOf != -1)
                {
                    offsets[offset++] = indexOf;
                    index = (indexOf + split.Length);
                }
                else
                {
                    index = testString.Length;
                }
            }

            string[] final = new string[offset + 1];
            if (offset == 0)
            {
                final[0] = testString;
            }
            else
            {
                offset--;
                final[0] = testString.Substring(0, offsets[0]);
                for (int i = 0; i < offset; i++)
                {
                    final[i + 1] = testString.Substring(offsets[i] + split.Length, offsets[i + 1] - offsets[i] - split.Length);
                }
                final[offset + 1] = testString.Substring(offsets[offset] + split.Length);
            }
            return final;
        }
    }
}
