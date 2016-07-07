using System;
using System.Collections.Generic;
using System.Text;
using MusicVideos.LocalMediaManagement;
using MusicVideos.SignatureBuilders;
using MusicVideos.Database;
using MediaPortal.Configuration;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows.Forms;
using SQLite.NET;
using System.Threading;


namespace MusicVideos.DataProviders
{
    public class MyVideosProvider: IMusicVideoProvider {
        #region IMusicVideoProvider Members

        public string Name {
            get { return "MyVideos (Local)"; }
        }

        public string Version {
            get { return "Internal"; }
        }

        public string Author {
            get { return "Moving Pictures Team"; }
        }

        public string Description {
            get { return "Retrieves mv cover artwork previously downloaded via MyVideos."; }
        }

        public string Language {
            get { return ""; }
        }
        
        public string LanguageCode {
            get { return ""; }
        }

        public bool ProvidesMusicVideoDetails {
            get { return true; }
        }

        public bool ProvidesCoverArt {
            get { return true; }
        }

        public bool ProvidesBackdrops {
            get { return false; }
        }

        public List<DBTrackInfo> Get(MusicVideoSignature mvSignature) {
            List<DBTrackInfo> results = new List<DBTrackInfo>();
            if (mvSignature == null)
                return results;

            string idMusicVideo = getMusicVideoID(mvSignature.File);
            if (idMusicVideo == String.Empty)
                return results;

            DBTrackInfo mv = this.getMusicVideoInfo(idMusicVideo);
            if (mv == null)
                return results;
            
            results.Add(mv);
            return results;
        }

        public UpdateResults Update(DBTrackInfo mv) {
            if (mv == null)
                return UpdateResults.FAILED;

            if (mv != null)
            {
                mv.CopyUpdatableValues(mv);
                return UpdateResults.SUCCESS;
            }
            return UpdateResults.FAILED;
        }

        public bool GetArtwork(DBTrackInfo mv) {
            string myVideoCoversFolder = Thumbs.MovieTitle;
            string cleanTitle = Util.Utils.MakeFileName(mv.Title);

            string filename = myVideoCoversFolder + @"\" + cleanTitle + "L.jpg";

            if (System.IO.File.Exists(filename)) 
                return mv.AddCoverFromFile(filename);
            
            return false;
        }

        public bool GetBackdrop(DBTrackInfo mv) {
            throw new NotImplementedException();
        }

        #endregion

        /// <summary>
        /// Method to find the file's unique id in the MyVideo's database
        /// </summary>
        /// <param name="fileName">Filename to look for in the MyVideo's database</param>
        /// <returns>unique id as string</returns>
        private string getMusicVideoID(string fileName)
        {
            string idMusicVideo = String.Empty;
            fileName = fileName.Replace("'", "''");
            try
            {
                SQLiteClient mp_db = new SQLiteClient(Config.GetFile(Config.Dir.Database, @"VideoDatabaseV5.db3"));
                SQLiteResultSet results = mp_db.Execute("SELECT idMovie FROM files WHERE strFilename LIKE '\\" + fileName + "'");
                idMusicVideo = results.GetField(0, 0);
                mp_db.Close();
            }
            catch
            {
            }
            return idMusicVideo;
        }

        private DBTrackInfo getMusicVideoInfo(string idMusicVideo)
        {
            DBTrackInfo mvRes = new DBTrackInfo();
            try
            {
                SQLiteClient mp_db = new SQLiteClient(Config.GetFile(Config.Dir.Database, @"VideoDatabaseV5.db3"));
                SQLiteResultSet sqlResults = mp_db.Execute("SELECT * FROM mvinfo WHERE idMovie LIKE '" + idMusicVideo + "'");

                SQLiteResultSet.Row sqlRow = sqlResults.GetRow(0);
                System.Collections.Hashtable columns = sqlResults.ColumnIndices;

                mvRes.Popularity = int.Parse(sqlResults.GetField(0, int.Parse(columns["strVotes"].ToString())));
                mvRes.Runtime = int.Parse(sqlResults.GetField(0, int.Parse(columns["runtime"].ToString())));
                mvRes.Score = float.Parse(sqlResults.GetField(0, int.Parse(columns["fRating"].ToString())));
                mvRes.Year = int.Parse(sqlResults.GetField(0, int.Parse(columns["iYear"].ToString())));

                string Title = sqlResults.GetField(0, int.Parse(columns["strTitle"].ToString()));
                if (!Title.Contains("unknown"))
                    mvRes.Title = Title;

                string Certification = sqlResults.GetField(0, int.Parse(columns["mpaa"].ToString()));
                if (!Certification.Contains("unknown"))
                {
                    try {
                        Regex certParse = new Regex(@"Rated\s(?<cert>.+)\sfor");
                        Match match = certParse.Match(Certification);
                        mvRes.Certification = match.Groups["cert"].Value;
                    }
                    catch (Exception e) {
                        // if an error happens in the try we will not set a value for the Certification
                        if (e is ThreadAbortException)
                            throw e;
                    }
                }

                string Tagline = sqlResults.GetField(0, int.Parse(columns["strTagLine"].ToString()));
                if (!Tagline.Contains("unknown"))
                    mvRes.Tagline = Tagline;

                string Summary = sqlResults.GetField(0, int.Parse(columns["strPlotOutline"].ToString()));
                if (!Summary.Contains("unknown"))
                    mvRes.Summary = Summary;

                string imdb_id = sqlResults.GetField(0, int.Parse(columns["IMDBID"].ToString()));
                if (!imdb_id.Contains("unknown"))
                    mvRes.ImdbID = imdb_id;

                string genreMain = sqlResults.GetField(0, int.Parse(columns["strGenre"].ToString()));
                if (!genreMain.Contains("unknown"))
                {
                    string[] genreSplit = genreMain.Split('/');
                    foreach (string genre in genreSplit)
                    {
                        mvRes.Genres.Add(genre.Trim());
                    }
                }

                string castMain = sqlResults.GetField(0, int.Parse(columns["strCast"].ToString()));
                if (!castMain.Contains("unknown"))
                {
                    string[] castSplit = castMain.Split('\n');
                    foreach (string cast in castSplit)
                    {
                        string castFinal = cast;
                        if (cast.Contains(" as "))
                            castFinal = cast.Remove(cast.IndexOf(" as "));
                        mvRes.Actors.Add(castFinal.Trim());
                    }
                }

                string idDirector = sqlResults.GetField(0, int.Parse(columns["idDirector"].ToString()));
                if (!castMain.Contains("unknown"))
                {
                    SQLiteResultSet sqlDirector = mp_db.Execute("SELECT strActor FROM actors WHERE idActor LIKE '" + idDirector + "'");
                    mvRes.Directors.Add(sqlDirector.GetField(0, 0));
                }

                mp_db.Close();
            }
            catch
            {
                return null;
            }

            return mvRes;
        }

    }
}
