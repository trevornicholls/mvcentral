using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SQLite.NET;
using NLog;
using MediaPortal.Configuration;
using System.Collections;
using MusicVideos;

namespace MusicVideos.Data
{
    partial class DataManager
    {
        public bool addDVD(DVDItem dvd)
        {
            return true;
        }

        public ArrayList getAllDVD()
        {
            ArrayList output = new ArrayList();
            SQLiteResultSet rs = dbConn.Execute("SELECT id FROM DVD");
            foreach (SQLiteResultSet.Row row in rs.Rows)
            {
                output.Add(DVDItem.FromDB(int.Parse(row.fields[0]), dbConn));
            }
            return output;
        }


    }
}
