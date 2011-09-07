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
using SQLite.NET;
using MediaPortal.Database;
using Cornerstone.Extensions;
//using Cornerstone.Extensions.IO;
using Cornerstone.Database;
using Cornerstone.Database.Tables;


namespace mvCentral.Database
{
  [DBTableAttribute("replacements")]
  public class DBReplacements : mvCentralDBTable
  {
    public const String cEnabled = "enabled";
    public const String cTagEnabled = "tagEnabled";
    public const String cToReplace = "toreplace";
    public const String cIsRegex = "isRegex";
    public const String cWith = "with";
    public const String cBefore = "before";

    public static Dictionary<String, String> s_FieldToDisplayNameMap = new Dictionary<String, String>();


    #region Database Fields

    [DBFieldAttribute(FieldName = "toreplace")]
    public string ToReplace
    {
      get
      { return _toreplace; }

      set
      {
        _toreplace = value;
        commitNeeded = true;
      }
    }
    private string _toreplace;

    [DBFieldAttribute(FieldName = "isregex")]
    public bool IsRegex
    {
      get
      { return _isregex; }

      set
      {
        _isregex = value;
        commitNeeded = true;
      }
    }
    private bool _isregex;

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

    [DBFieldAttribute(FieldName = "tagenabled")]
    public bool TagEnabled
    {
      get
      { return _tagenabled; }

      set
      {
        _tagenabled = value;
        commitNeeded = true;
      }
    }
    private bool _tagenabled;

    [DBFieldAttribute(FieldName = "with")]
    public string With
    {
      get
      { return _with; }

      set
      {
        _with = value;
        commitNeeded = true;
      }
    }
    private string _with;

    [DBFieldAttribute(FieldName = "before")]
    public bool Before
    {
      get
      { return _before; }

      set
      {
        _before = value;
        commitNeeded = true;
      }
    }
    private bool _before;


    #endregion

    /// <summary>
    /// Load Default replacement items
    /// </summary>
    public static void AddDefaults()
    {
      ClearAll();
      add(true, false, false, false, ".", @"<space>");
      add(true, false, false, false, "_", @"<space>");
      add(true, false, false, false, "-<space>", @"<empty>");

      //add(true, true, true, true, @"\s*\(.*?\)\s*", @"<empty>");
      //add(true, true, true, true, @"\s*\[.*?\]\s*", @"<empty>");
      add(true, true, true, true, @"\s*[\(\[].*?[\]\)]\s*", @"<empty>");

      add(true, true, true, false, "&", "and");
      add(true, true, true, false, "1080i", @"<empty>");
      add(true, true, true, false, "1080p", @"<empty>");
      add(true, true, true, false, "HDTV", @"<empty>");
      add(true, true, true, false, "HD", @"<empty>");
    }


    public static void add(bool enabled, bool tagenabled, bool before, bool isRegex, string toreplace, string with)
    {
      DBReplacements r1 = new DBReplacements();
      r1.Enabled = enabled;
      r1.TagEnabled = tagenabled;
      r1.Before = before;
      r1.IsRegex = isRegex;
      r1.ToReplace = toreplace;
      r1.With = with;
      r1.Commit();
    }

    public static String PrettyFieldName(String sFieldName)
    {
      if (s_FieldToDisplayNameMap.ContainsKey(sFieldName))
        return s_FieldToDisplayNameMap[sFieldName];
      else
        return sFieldName;
    }

    public DBReplacements()
      : base()
    {
      //            InitColumns();
      //            InitValues(-1,"");
    }

    //        public DBReplacements(long ID)
    //            : base(cTableName)
    //        {
    //            InitColumns();
    //            if (!ReadPrimary(ID.ToString()))
    //                InitValues(-1,"");
    //        }

    private void InitColumns()
    {
      // all mandatory fields. WARNING: INDEX HAS TO BE INCLUDED FIRST ( I suck at SQL )
      //            AddColumn(cIndex, new DBField(DBField.cTypeInt, true));
      //            AddColumn(cEnabled, new DBField(DBField.cTypeInt));
      //            AddColumn(cTagEnabled, new DBField(DBField.cTypeInt));
      //            AddColumn(cBefore, new DBField(DBField.cTypeInt));
      //            AddColumn(cToReplace, new DBField(DBField.cTypeString));
      //            AddColumn(cWith, new DBField(DBField.cTypeString));
      //            AddColumn(cIsRegex, new DBField(DBField.cTypeInt));
    }

    public static void Exchange(int i, int x)
    {
      DBReplacements db1 = Get(i);
      DBReplacements db2 = Get(x);
      if (db1 == null || db2 == null) return;
      DBReplacements tdb = new DBReplacements();
      tdb.Enabled = db1.Enabled;
      tdb.IsRegex = db1.IsRegex;
      tdb.ToReplace = db1.ToReplace;
      tdb.With = db1.With;
      tdb.Before = db1.Before;
      tdb.TagEnabled = db1.TagEnabled;

      db1.Enabled = db2.Enabled;
      db1.IsRegex = db2.IsRegex;
      db1.ToReplace = db2.ToReplace;
      db1.With = db2.With;
      db1.Before = db2.Before;
      db1.TagEnabled = db2.TagEnabled;
      db1.Commit();

      db2.Enabled = tdb.Enabled;
      db2.IsRegex = tdb.IsRegex;
      db2.ToReplace = tdb.ToReplace;
      db2.With = tdb.With;
      db2.Before = tdb.Before;
      db2.TagEnabled = tdb.TagEnabled;
      db2.Commit();
    }

    public static void ClearAll()
    {
      List<DBReplacements> rp = GetAll();
      foreach (DBReplacements r1 in rp)
      {
        r1.Delete();
        r1.Commit();
      }
      //           mvCentralCore.DatabaseManager.Delete(DBReplacements);
      //            String sqlQuery = "delete from "+ cTableName;
      //            DBTVSeries.Execute(sqlQuery);
    }

    public static DBReplacements Get(int index)
    {
      return mvCentralCore.DatabaseManager.Get<DBReplacements>(index);
    }

    public static List<DBReplacements> GetAll()
    {
      return mvCentralCore.DatabaseManager.Get<DBReplacements>(null);
    }
  }
}
