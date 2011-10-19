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
using Cornerstone.Extensions;
//using Cornerstone.Extensions.IO;
using Cornerstone.Database;
using Cornerstone.Database.Tables;
using MediaPortal.Database;

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


    public static void add(bool enabled, string genre)
    {
      DBGenres r1 = new DBGenres();
      r1.Enabled = enabled;
      r1.Genre = genre;
      r1.Commit();
    }

    public static void ClearAll()
    {
      List<DBGenres> rp = GetAll();
      foreach (DBGenres r1 in rp)
      {
        r1.Delete();
        r1.Commit();
      }
    }

    public static DBGenres Get(int index)
    {
      return mvCentralCore.DatabaseManager.Get<DBGenres>(index);
    }

    public static List<DBGenres> GetAll()
    {
      return mvCentralCore.DatabaseManager.Get<DBGenres>(null);
    }

  }
}
