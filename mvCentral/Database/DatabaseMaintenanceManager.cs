using Cornerstone.Database;
using Cornerstone.Database.Tables;
using Cornerstone.GUI.Dialogs;

using NLog;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace mvCentral.Database
{
    public class DatabaseMaintenanceManager {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        public static event ProgressDelegate MaintenanceProgress;

        // Loops through all local files in the system and removes anything that's invalid.
        public static void RemoveInvalidFiles() 
        {
           
            logger.Info("Checking for invalid file entries in the database.");

            float count = 0;
            List<DBLocalMedia> files = DBLocalMedia.GetAll();
            float total = files.Count;
            
            int cleaned = 0;
            
            foreach (DBLocalMedia currFile in files) 
            {
                if (MaintenanceProgress != null) MaintenanceProgress("", (int)(count * 100 / total));
                count++;

                // Skip previously deleted files
                if (currFile.ID == null)
                    continue;

                // Remove Orphan Files
                if (currFile.AttachedmvCentral.Count == 0 && !currFile.Ignored)
                {
                  logger.Info("Removing: {0} (orphan)", currFile.FullPath);
                  currFile.Delete();
                  cleaned++;
                  continue;
                }

                // remove files without an import path
                if (currFile.ImportPath == null || currFile.ImportPath.ID == null) {
                    logger.Info("Removing: {0} (no import path)", currFile.FullPath);
                    currFile.Delete();
                    cleaned++;
                    continue;
                }

                // Remove entries from the database that have their file removed
                if (currFile.IsRemoved) {
                    logger.Info("Removing: {0} (file is removed)", currFile.FullPath);
                    currFile.Delete();
                    cleaned++;
                }

            }
            
            logger.Info("Removed {0} file entries.", cleaned.ToString());
            if (MaintenanceProgress != null) MaintenanceProgress("", 100);

            // Remove Orphan albums
            cleaned = 0;
            List<DBAlbumInfo> albumObjectList = DBAlbumInfo.GetAll();
            foreach (DBAlbumInfo albumObject in albumObjectList)
            {
              if (albumObject.Album.Trim() == string.Empty)
              {
                albumObject.Album = "Unknow Album";
                albumObject.Commit();
              }
              List<DBTrackInfo> mvs = DBTrackInfo.GetEntriesByAlbum(albumObject);
              if (mvs.Count == 0)
              {
                logger.Info("Removing: {0} (albuminfo orphan)", albumObject.Album);
                albumObject.Delete();
                cleaned++;
              }

            }
            logger.Info("Removed {0} Album orphan entries.", cleaned.ToString());
            if (MaintenanceProgress != null) MaintenanceProgress("", 100);

            // Remove Orphan artist
            cleaned = 0;
            List<DBArtistInfo> allArtistList = DBArtistInfo.GetAll();
            foreach (DBArtistInfo artist in allArtistList)
            {
              List<DBTrackInfo> mvs = DBTrackInfo.GetEntriesByArtist(artist);
              if (mvs.Count == 0)
              {
                logger.Info("Removing: {0} (artistinfo orphan)", artist.Artist);
                artist.Delete();
                cleaned++;
              }
            }
            logger.Info("Removed {0} Artist orphan entries.", cleaned.ToString());
            if (MaintenanceProgress != null) MaintenanceProgress("", 100);


        }
        
        // Loops through all mvCentral in the system to verify them
        public static void VerifyMusicVideoInformation()
        {
            logger.Info("Updating MusicVideo Information...");

            float count = 0;
            List<DBTrackInfo> mvs = DBTrackInfo.GetAll();
            List<DBUser> users = DBUser.GetAll();
            float total = mvs.Count;

            int removed = 0;
            int settings = 0;
            foreach (DBTrackInfo mv in mvs)
            {
                if (MaintenanceProgress != null) MaintenanceProgress("", (int)(count * 100 / total));
                count++;

                // Skip uncommited files
                if (mv.ID == null)
                    continue;

                #region Remove MusicVideo without attached local media

                // Remove mvCentral with no files
                if (mv.LocalMedia.Count == 0) {
                    logger.Info("'{0}' was removed from the system because it had no local media.", mv.Track);
                    mv.Delete();
                    removed++;
                    continue;
                }

                #endregion

                #region Add missing user settings

                if (mv.UserSettings.Count == 0) {
                    logger.Info("'{0}' was missing UserMovingSettings, adding now.", mv.Track);
                    foreach (DBUser currUser in users) {
                        DBUserMusicVideoSettings userSettings = new DBUserMusicVideoSettings();
                        userSettings.User = currUser;
                        userSettings.Commit();
                        mv.UserSettings.Add(userSettings);
                        userSettings.CommitNeeded = false;
                    }
//                    lock (mv)
                    {
                        mv.Commit();
                    }
                    settings++;
                }

                #endregion

            }

            logger.Info("Removed {0} MusicVideo entries.", removed.ToString());
            logger.Info("Updated {0} MusicVideo entries with default user setting.", settings.ToString());
            if (MaintenanceProgress != null) MaintenanceProgress("", 100);
        }

        // Update System Managed Import Paths
        public static void UpdateImportPaths() {
            
            // remove obsolete or invalid import paths
            foreach (DBImportPath currPath in DBImportPath.GetAll()) {
                if (currPath.Directory == null)
                    currPath.Delete();

                if (currPath.InternallyManaged && currPath.GetDriveType() == DriveType.NoRootDirectory) {
                    currPath.Delete();
                    logger.Info("Removed system managed import path: {0} (drive does not exist)", currPath.FullPath);
                }

                // Automatically remove import paths that were marked as replaced and don't have any related media (left)
                if (currPath.Replaced) {
                    // get related local media (we append % so we get all paths starting with the import path)
                    List<DBLocalMedia> attachedLocalMedia = DBLocalMedia.GetAll(currPath.FullPath + "%");
                    if (attachedLocalMedia.Count == 0) {
                        currPath.Delete();
                        logger.Info("Removed import path: {0} (was marked as replaced and doesn't have any related files)", currPath.FullPath);
                    }
                }

            }

            float count = 0;
            float total = DriveInfo.GetDrives().Length;

            bool daemonEnabled = mvCentralCore.MediaPortalSettings.GetValueAsBool("daemon", "enabled", false);
            string virtualDrive = mvCentralCore.MediaPortalSettings.GetValueAsString("daemon", "drive", "?:");
            
            // Get all drives
            foreach (DriveInfo drive in DriveInfo.GetDrives()) {
                if (MaintenanceProgress != null) MaintenanceProgress("", (int)(count * 100 / total));
                count++;

                // Add the import path if it does not exist and 
                // is not marked virtual by MediaPortal.
                DBImportPath importPath = DBImportPath.Get(drive.Name);
                bool isVirtual = drive.Name.StartsWith(virtualDrive, StringComparison.OrdinalIgnoreCase) && daemonEnabled;
                bool isCDRom = (drive.DriveType == DriveType.CDRom);

                if (importPath.ID != null) {
                    // Remove an system managed path if for any reason it's not of type CDRom
                    if (!isCDRom && importPath.InternallyManaged) {
                        importPath.Delete();
                        logger.Info("Removed system managed import path: {0} (drive type has changed)", importPath.FullPath);
                        continue;
                    }

                    // Remove an existing path if it's defined as the virtual drive
                    if (isVirtual) {
                        importPath.Delete();
                        logger.Info("Removed import path: {0} (drive is marked as virtual)", importPath.FullPath);
                        continue;
                    }

                    // Update an existing import path to a system managed import path
                    // if the drive type is CDRom but the system flag isn't set
                    if (isCDRom && !importPath.InternallyManaged) {
                        importPath.InternallyManaged = true;
                        importPath.Commit();
                        logger.Info("{0} was updated to a system managed import path.", importPath.FullPath);
                    }

                }
                else {
                    if (isCDRom && !isVirtual) {
                        importPath.InternallyManaged = true;
                        importPath.Commit();
                        logger.Info("Added system managed import path: {0}", importPath.FullPath);
                    }
                }
            }

            if (MaintenanceProgress != null) MaintenanceProgress("", 100);
        }
        
        // One time upgrade tasks for movie information
        public static void PerformMusicVideoInformationUpgradeCheck()
        {
            Version ver = Assembly.GetExecutingAssembly().GetName().Version;
            logger.Info("Performing MusicVideo Information Upgrade Check...");

            float count = 0;
            List<DBTrackInfo> mvs = DBTrackInfo.GetAll();
            float total = mvs.Count;

            foreach (DBTrackInfo mv in mvs)
            {
                if (MaintenanceProgress != null) MaintenanceProgress("", (int)(count * 100 / total));
                count++;

                // Skip uncommited files
                if (mv.ID == null)
                    continue;

                #region Upgrades required for 0.7.1

                if (mvCentralCore.GetDbVersionNumber() < new Version("0.7.1"))
                {

                    if (mv.LocalMedia.Count > 0 && mv.LocalMedia[0].ImportPath != null) {
                        if (mv.LocalMedia[0].ImportPath.IsOpticalDrive && mv.LocalMedia[0].IsAvailable) {
                            mv.DateAdded = mv.LocalMedia[0].File.CreationTime;
                        }
                        else {
                            mv.DateAdded = mv.DateAdded.AddSeconds((double)mv.ID);
                        }
                    }
                }

                #endregion

                // commit MusicVideo
                mv.Commit();
            }

            if (MaintenanceProgress != null) MaintenanceProgress("", 100);
        }

        // One time upgrades tasks for file information
        public static void PerformFileInformationUpgradeCheck() {
            Version ver = Assembly.GetExecutingAssembly().GetName().Version;
            logger.Info("Performing File Information Upgrade Check...");

            float count = 0;
            List<DBLocalMedia> files = DBLocalMedia.GetAll();
            float total = files.Count;

            foreach (DBLocalMedia currFile in files) {
                if (MaintenanceProgress != null) MaintenanceProgress("", (int)(count * 100 / total));
                count++;

                // Skip uncommited files
                if (currFile.ID == null)
                    continue;


                // commit file
                currFile.Commit();
            }

            if (MaintenanceProgress != null) MaintenanceProgress("", 100);
        }

        // All other one time upgrades
        public static void PerformArtworkUpgradeCheck() {
            Version ver = Assembly.GetExecutingAssembly().GetName().Version;
            logger.Info("Performing Artwork Upgrade Check...");

        }

        public static void VerifyFilterMenu() {
            DBMenu<DBTrackInfo> menu = mvCentralCore.Settings.FilterMenu;

            if (menu.RootNodes.Count == 0) {
                int position = 1;

                DBNode<DBTrackInfo> unwatchedNode = new DBNode<DBTrackInfo>();
                unwatchedNode.Name = "${UnwatchedmvCentral}";
                unwatchedNode.DynamicNode = false;
                unwatchedNode.Filter = new DBFilter<DBTrackInfo>();
                DBCriteria<DBTrackInfo> criteria = new DBCriteria<DBTrackInfo>();
                criteria.Field = DBField.GetFieldByDBName(typeof(DBUserMusicVideoSettings), "watched");
                criteria.Relation = DBRelation.GetRelation(typeof(DBTrackInfo), typeof(DBUserMusicVideoSettings), "");
                criteria.Operator = DBCriteria<DBTrackInfo>.OperatorEnum.EQUAL;
                criteria.Value = "0";
                unwatchedNode.Filter.Criteria.Add(criteria);
                unwatchedNode.SortPosition = position++;
                unwatchedNode.DBManager = mvCentralCore.DatabaseManager;
                menu.RootNodes.Add(unwatchedNode);

                DBNode<DBTrackInfo> genreNode = new DBNode<DBTrackInfo>();
                genreNode.DynamicNode = true;
                genreNode.BasicFilteringField = DBField.GetFieldByDBName(typeof(DBTrackInfo), "genres");
                genreNode.Name = "${Genre}";
                genreNode.DBManager = mvCentralCore.DatabaseManager;
                genreNode.SortPosition = position++;
                menu.RootNodes.Add(genreNode);

                DBNode<DBTrackInfo> dateNode = new DBNode<DBTrackInfo>();
                dateNode.DynamicNode = true;
                dateNode.BasicFilteringField = DBField.GetFieldByDBName(typeof(DBTrackInfo), "date_added");
                dateNode.Name = "${DateAdded}";
                dateNode.DBManager = mvCentralCore.DatabaseManager;
                dateNode.SortPosition = position++;
                menu.RootNodes.Add(dateNode);

                menu.Commit();
            }

            foreach (DBNode<DBTrackInfo> currNode in menu.RootNodes)
            {
                currNode.UpdateDynamicNode();
                currNode.Commit();
            }
        }

        public static void VerifyCategoryMenu() {
            DBMenu<DBTrackInfo> menu = mvCentralCore.Settings.CategoriesMenu;

            if (menu.RootNodes.Count == 0) {
                int position = 1;

                DBNode<DBTrackInfo> allNode = new DBNode<DBTrackInfo>();
                allNode.Name = "${AllmvCentral}";
                allNode.DynamicNode = false;
                allNode.Filter = new DBFilter<DBTrackInfo>();
                allNode.SortPosition = position++;
                allNode.DBManager = mvCentralCore.DatabaseManager;
                menu.RootNodes.Add(allNode);

                DBNode<DBTrackInfo> unwatchedNode = new DBNode<DBTrackInfo>();
                unwatchedNode.Name = "${UnwatchedMusicVideo}";
                unwatchedNode.DynamicNode = false;
                unwatchedNode.Filter = new DBFilter<DBTrackInfo>();
                DBCriteria<DBTrackInfo> criteria = new DBCriteria<DBTrackInfo>();
                criteria.Field = DBField.GetFieldByDBName(typeof(DBUserMusicVideoSettings), "watched");
                criteria.Relation = DBRelation.GetRelation(typeof(DBTrackInfo), typeof(DBUserMusicVideoSettings), "");
                criteria.Operator = DBCriteria<DBTrackInfo>.OperatorEnum.EQUAL;
                criteria.Value = "0";
                unwatchedNode.Filter.Criteria.Add(criteria);
                unwatchedNode.SortPosition = position++;
                unwatchedNode.DBManager = mvCentralCore.DatabaseManager;
                menu.RootNodes.Add(unwatchedNode);


                DBNode<DBTrackInfo> recentNode = new DBNode<DBTrackInfo>();
                recentNode.Name = "${RecentlyAddedmvCentral}";
                recentNode.DynamicNode = false;
                recentNode.Filter = new DBFilter<DBTrackInfo>();
                recentNode.SortPosition = position++;
                recentNode.DBManager = mvCentralCore.DatabaseManager;
                
                DBCriteria<DBTrackInfo> recentCriteria = new DBCriteria<DBTrackInfo>();
                recentCriteria.Field = DBField.GetFieldByDBName(typeof(DBTrackInfo), "date_added");
                recentCriteria.Operator = DBCriteria<DBTrackInfo>.OperatorEnum.GREATER_THAN;
                recentCriteria.Value = "-30d";
                recentNode.Filter.Criteria.Add(recentCriteria);

                DBMusicVideoNodeSettings additionalSettings = new DBMusicVideoNodeSettings();
                additionalSettings.UseDefaultSorting = false;
//                additionalSettings.SortField = SortingFields.DateAdded;
//                additionalSettings.SortDirection = SortingDirections.Descending;
                recentNode.AdditionalSettings = additionalSettings;

                menu.RootNodes.Add(recentNode);


                DBNode<DBTrackInfo> genreNode = new DBNode<DBTrackInfo>();
                genreNode.DynamicNode = true;
                genreNode.BasicFilteringField = DBField.GetFieldByDBName(typeof(DBTrackInfo), "genres");
                genreNode.Name = "${Genres}";
                genreNode.SortPosition = position++;
                genreNode.DBManager = mvCentralCore.DatabaseManager;
                menu.RootNodes.Add(genreNode);

                menu.Commit();
            }

            foreach (DBNode<DBTrackInfo> currNode in menu.RootNodes) {
                currNode.UpdateDynamicNode();
                currNode.Commit();
            }
        }
    }
}
