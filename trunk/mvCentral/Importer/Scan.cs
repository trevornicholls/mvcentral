using System;
using System.Collections;
using System.Linq;
using System.Text;
using NLog;
using MediaPortal.Database;
using SQLite.NET;
using System.IO;
using MusicVideos.Data;

namespace MusicVideos.Importer
{
    public static class Scan
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public static Stack getStack()
        {         
            //Initialise Datamanager, initial stack
            DataManager dm = MusicVideosCore.dm;           
            Stack theStack = new Stack();
            SQLiteResultSet watchFolders = dm.getWatch();

            //Cycle through all watchfolders, and grab all details within
            foreach (SQLiteResultSet.Row row in watchFolders.Rows)
            {
                //Get the path, and expression
                string videosPath = row.fields[1];
                string videosExp = row.fields[2];
                //Process the watch folder, hold onto the stack
                ProcessDir(ref theStack, videosPath, videosExp, videosPath);
            }
            //Send the stack back, re-init to get it in alphabetical order
            return new Stack(theStack);
        }

        public static void ProcessDir(ref Stack theStack, string sourceDir, string mtchExp, string origPath)
        {
            //Get all the files in the dir
            string[] fileEntries = Directory.GetFiles(sourceDir);
            
            logger.Info("Processing directory: " + sourceDir);

            foreach (string fileName in fileEntries)
            {
                string[] tmp = Extract.GetData(fileName, mtchExp, origPath);
                if (!(tmp == null))
                    theStack.Push(tmp);
            }
            
            //Recurse through dir structure
            string[] subdirEntries = Directory.GetDirectories(sourceDir);
            foreach (string subdir in subdirEntries)
                if ((File.GetAttributes(subdir) & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
                    ProcessDir(ref theStack, subdir, mtchExp, origPath);                    
        }
    }
}
