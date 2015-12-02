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

using Cornerstone.Database;
using Cornerstone.Database.Tables;

using System;
using System.Collections.Generic;

namespace mvCentral.Database
{
  [DBTableAttribute("composers")]
  class DBComposers : mvCentralDBTable
  {
    public const String cTableName = "composers";
    public const int cDBVersion = 4;

    public const String cIndex = "ID";
    public const String cEnabled = "enabled";
    public const String ccomposer = "composer";


    public DBComposers()
      : base()
    {
    }

    #region Database Fields

    [DBFieldAttribute(FieldName = "composer")]
    public string composer
    {
      get
      { return _composer; }

      set
      {
        _composer = value;
        commitNeeded = true;
      }
    }
    private string _composer;

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
    /// Add tag to composer DB
    /// </summary>
    /// <param name="enabled"></param>
    /// <param name="composer"></param>
    public static void Add(string composer)
    {
      DBComposers r1 = new DBComposers();
      r1.composer = composer;
      r1.Commit();
    }
    /// <summary>
    /// Remove all entries
    /// </summary>
    public static void ClearAll()
    {
      List<DBComposers> rp = GetAll();
      foreach (DBComposers r1 in rp)
      {
        r1.Delete();
        r1.Commit();
      }
    }
    /// <summary>
    /// Get enabled entries only
    /// </summary>
    /// <returns></returns>
    public static List<DBComposers> GetSelected()
    {
      List<DBComposers> selectList = new List<DBComposers>();
      foreach (DBComposers db1 in GetAll())
      {
        selectList.Add(db1);
      }
      return selectList;
    }
    /// <summary>
    /// Get Specific entry
    /// </summary>
    /// <param name="composer"></param>
    /// <returns></returns>
    public static DBComposers Get(string composer)
    {
      if (composer.Trim().Length == 0) return null;
      foreach (DBComposers db1 in GetAll())
      {
        if (String.Equals(composer, db1.composer)) return db1;
      }
      return null;
    }
    /// <summary>
    /// Get by Index
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public static DBComposers Get(int index)
    {
      return mvCentralCore.DatabaseManager.Get<DBComposers>(index);
    }
    /// <summary>
    /// Get all
    /// </summary>
    /// <returns></returns>
    public static List<DBComposers> GetAll()
    {
      return mvCentralCore.DatabaseManager.Get<DBComposers>(null);
    }
    /// <summary>
    /// alwats return composer value
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return composer;
    }

    #endregion

  }
}
