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
  [DBTableAttribute("tonesandstyles")]
  class DBTonesAndStyles : mvCentralDBTable
  {
    public const String cTableName = "tonesandstyles";
    public const int cDBVersion = 4;

    public const String cIndex = "ID";
    public const String cType = "type";
    public const String cToneOrStyle = "toneorstyle";

    public DBTonesAndStyles()
      : base()
    {
    }

    #region Database Fields

    [DBFieldAttribute(FieldName = "type")]
    public string Type
    {
      get
      { return _type; }

      set
      {
        _type = value;
        commitNeeded = true;
      }
    }
    private string _type;


    [DBFieldAttribute(FieldName = "toneorstyle")]
    public string ToneOrStyle
    {
      get
      { return _toneorstyle; }

      set
      {
        _toneorstyle = value;
        commitNeeded = true;
      }
    }
    private string _toneorstyle;

    #endregion

    #region Database Management Methods

    /// <summary>
    /// Add tag to composer DB
    /// </summary>
    /// <param name="enabled"></param>
    /// <param name="composer"></param>
    public static void Add(string type, string toneOrStyle)
    {
      DBTonesAndStyles tsObject = new DBTonesAndStyles();
      tsObject.Type = type;
      tsObject.ToneOrStyle = toneOrStyle;
      tsObject.Commit();
    }
    /// <summary>
    /// Remove all entries
    /// </summary>
    public static void ClearAll()
    {
      List<DBTonesAndStyles> tonesOrStyles = GetAll();
      foreach (DBTonesAndStyles tsObject in tonesOrStyles)
      {
        tsObject.Delete();
        tsObject.Commit();
      }
    }
    /// <summary>
    /// Get Tones
    /// </summary>
    /// <returns></returns>
    public static List<string> GetAllTones()
    {
      List<string> selectList = new List<string>();
      foreach (DBTonesAndStyles db1 in GetAll())
      {
        if (db1.Type == "T")
          selectList.Add(db1.ToneOrStyle);
      }
      return selectList;
    }
    /// <summary>
    /// Get Styles
    /// </summary>
    /// <returns></returns>
    public static List<string> GetAllStyles()
    {
      List<string> selectList = new List<string>();
      foreach (DBTonesAndStyles db1 in GetAll())
      {
        if (db1.Type == "S")
          selectList.Add(db1.ToneOrStyle);
      }
      return selectList;
    }
    /// <summary>
    /// Get Specific entry
    /// </summary>
    /// <param name="composer"></param>
    /// <returns></returns>
    public static string Get(string type, string toneOrStyle)
    {
      if (toneOrStyle.Trim().Length == 0) 
        return null;

      if (type == "T")
      {
        foreach (string tone in GetAllTones())
        {
          if (String.Equals(toneOrStyle, tone)) 
            return tone;
        }
      }
      else if (type == "S")
      {
        foreach (string style in GetAllStyles())
        {
          if (String.Equals(toneOrStyle, style))
            return style;
        }
      }
      return null;
    }
    /// <summary>
    /// Get by Index
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public static DBTonesAndStyles Get(int index)
    {
      return mvCentralCore.DatabaseManager.Get<DBTonesAndStyles>(index);
    }
    /// <summary>
    /// Get all
    /// </summary>
    /// <returns></returns>
    public static List<DBTonesAndStyles> GetAll()
    {
      return mvCentralCore.DatabaseManager.Get<DBTonesAndStyles>(null);
    }
    /// <summary>
    /// alwats return composer value
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return ToneOrStyle;
    }

    #endregion

  }
}
