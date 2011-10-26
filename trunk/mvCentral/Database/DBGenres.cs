#region GNU license
// MP-TVSeries - Plugin for Mediaportal
// http://www.team-mediaportal.com
// Copyright (C) 2006-2007
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
//+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using NLog;
using System.Web;
using System.Net;
using System.Threading;
using System.Collections;
using Cornerstone.Database;
using Cornerstone.Database.CustomTypes;
using Cornerstone.Database.Tables;
using mvCentral.LocalMediaManagement;
using System.Text.RegularExpressions;
using Cornerstone.Tools.Translate;
using System.Runtime.InteropServices;
using mvCentral.LocalMediaManagement.MusicVideoResources;
using Cornerstone.Extensions;

namespace mvCentral.Database
{
  [DBTableAttribute("genres")]
  class DBGenres : mvCentralDBTable
  {
    public const String cTableName = "genres";
    public const int cDBVersion = 4;

    public const String cIndex = "ID";
    public const String cEnabled = "enabled";
    public const String cGenre = "genre";


    public DBGenres()
      : base()
    {
    }

    #region Database Fields

    [DBFieldAttribute(FieldName = "genre")]
    public string Genre
    {
      get
      { return _genre; }

      set
      {
        _genre = value;
        commitNeeded = true;
      }
    }
    private string _genre;

    [DBFieldAttribute(FieldName = "enabled")]
    public bool Enabled
    {
      get
      { return _enabled; }

      set
      {
        _enabled = value;
        commitNeeded = true;
      }
    }
    private bool _enabled;

    #endregion

    #region Database Management Methods

    /// <summary>
    /// Add tag to Genre DB
    /// </summary>
    /// <param name="enabled"></param>
    /// <param name="genre"></param>
    public static void add(bool enabled, string genre)
    {
      DBGenres r1 = new DBGenres();
      r1.Enabled = enabled;
      r1.Genre = genre;
      r1.Commit();
    }
    /// <summary>
    /// Remove all entries
    /// </summary>
    public static void ClearAll()
    {
      List<DBGenres> rp = GetAll();
      foreach (DBGenres r1 in rp)
      {
        r1.Delete();
        r1.Commit();
      }
    }
    /// <summary>
    /// Get enabled entries only
    /// </summary>
    /// <returns></returns>
    public static List<DBGenres> GetSelected()
    {
      List<DBGenres> selectList = new List<DBGenres>();
      foreach (DBGenres db1 in GetAll())
      {
        if (db1._enabled)
          selectList.Add(db1);

      }
      return selectList;
    }
    /// <summary>
    /// Get Specific entry
    /// </summary>
    /// <param name="Genre"></param>
    /// <returns></returns>
    public static DBGenres Get(string Genre)
    {
      if (Genre.Trim().Length == 0) return null;
      foreach (DBGenres db1 in GetAll())
      {
        if (String.Equals(Genre, db1.Genre)) return db1;
      }
      return null;
    }
    /// <summary>
    /// Get by Index
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public static DBGenres Get(int index)
    {
      return mvCentralCore.DatabaseManager.Get<DBGenres>(index);
    }
    /// <summary>
    /// Get all
    /// </summary>
    /// <returns></returns>
    public static List<DBGenres> GetAll()
    {
      return mvCentralCore.DatabaseManager.Get<DBGenres>(null);
    }
    /// <summary>
    /// alwats return Genre value
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return Genre;
    }

    #endregion

  }
}
