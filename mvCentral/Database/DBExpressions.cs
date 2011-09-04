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
using mvCornerstone.Extensions;
//using mvCornerstone.Extensions.IO;
using mvCornerstone.Database;
using mvCornerstone.Database.Tables;
using MediaPortal.Database;

namespace mvCentral.Database
{

    [DBTableAttribute("expressions")]
    public class DBExpression : mvCentralDBTable 
    {
        public const String cTableName = "expressions";
        public const int cDBVersion = 4;

        public const String cIndex = "ID";
        public const String cEnabled = "enabled";
        public const String cType = "type";
        public const String cExpression = "expression";

        public const String cType_Simple = "simple";
        public const String cType_Regexp = "regexp";

        public DBExpression()
            : base()
        {
//            InitColumns();
//            InitValues(-1,"");
        }

//        public DBExpression(long ID)
//            : base(cTableName)
//        {
//            InitColumns();
//            if (!ReadPrimary(ID.ToString()))
//                InitValues(-1,"");
//        }

        #region Database Fields

        [DBFieldAttribute(FieldName = "type")]
        public string Type
        {
            get 
            { return _type;}

            set
            {
                _type = value;
                commitNeeded = true;
            }
        }
        private string _type;

        [DBFieldAttribute(FieldName = "expression")]
        public string Expression
        {
            get
            { return _expression; }

            set
            {
                _expression = value;
                commitNeeded = true;
            }
        }
        private string _expression;

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


        public static void AddDefaults()
        {
          ClearAll();
          //            add(true, DBExpression.cType_Regexp,@"^.*?\\?(?<series>[^\\$]+?)[ _.\-\[]+(?:[s]?(?<season>\d+)[ _.\-\[\]]*[ex](?<episode>\d+)|(?:\#|\-\s)(?<season>\d+)\.(?<episode>\d+))(?:[ _.+-]+(?:[s]?\k<season>[ _.\-\[\]]*[ex](?<episode2>\d+)|(?:\#|\-\s)\k<season>\.(?<episode2>\d+))|(?:[ _.+-]*[ex+-]+(?<episode2>\d+)))*[ _.\-\[\]]*(?<title>(?![^\\]*?(?<!the)[ .(-]sample[ .)-]).*?)\.(?<ext>[^.]*)$"); 
          //            add(true, DBExpression.cType_Regexp,@"^.*?\\?(?<series>[^\\$]+?)(?:s(?<season>[0-3]?\d)\s?ep?(?<episode>\d\d)|(?<season>(?:[0-1]\d|(?<!\d)\d))x?(?<episode>\d\d))(?!\d)(?:[ .-]?(?:s\k<season>e?(?<episode2>\d{2}(?!\d))|\k<season>x?(?<episode2>\d{2}(?!\d))|(?<episode2>\d\d(?!\d))|E(?<episode2>\d\d))|)[ -.]*(?<title>(?![^\\]*?sample)[^\\]*?[^\\]*?)\.(?<ext>[^.]*)$");          
          //            add(true, DBExpression.cType_Regexp,@"^(?<series>[^\\$]+)\\[^\\$]*?(?:s(?<season>[0-1]?\d)ep?(?<episode>\d\d)|(?<season>(?:[0-1]\d|(?<!\d)\d))x?(?<episode>\d\d))(?!\d)(?:[ .-]?(?:s\k<season>e?(?<episode2>\d{2}(?!\d))|\k<season>x?(?<episode2>\d{2}(?!\d))|(?<episode2>\d\d(?!\d))|E(?<episode2>\d\d))|)[ -.]*(?<title>(?!.*sample)[^\\]*?[^\\]*?)\.(?<ext>[^.]*)$");
          //add(true, DBExpression.cType_Regexp,@"(?<artist>[^\\\[]*) - \[(?<season>[0-9]{1,2})x(?<episode>[0-9\W]+)\](( |)(-( |)|))(?<title>(?![^\\]*?sample)[^$]*?)\.(?<ext>[^.]*)");

          //            add(true, DBExpression.cType_Simple,@"<series> - <season>x<episode> - <title>.<ext>");
          //            add(true, DBExpression.cType_Simple,@"<series>\Season <season>\Episode <episode> - <title>.<ext>");
          //add(true, DBExpression.cType_Simple, @"<artist> - <track>");

          add(true, DBExpression.cType_Regexp, @"(?<artist>[^\\$]*) - (?<track>[^\\$]*)\.(?<ext>[^.]*)"); 
          add(true, DBExpression.cType_Simple, @"\<artist> - <track>.<ext>");
          add(true, DBExpression.cType_Simple, @"\<artist>\<track>.<ext>");
          add(true, DBExpression.cType_Simple, @"\<album>\<artist> - <track>.<ext>");

        }

        private void InitColumns()
        {
            // all mandatory fields. WARNING: INDEX HAS TO BE INCLUDED FIRST ( I suck at SQL )
//            AddColumn(cIndex, new DBField(DBField.cTypeInt, true));
//            AddColumn(cEnabled, new DBField(DBField.cTypeInt));
//            AddColumn(cType, new DBField(DBField.cTypeString));
//            AddColumn(cExpression, new DBField(DBField.cTypeString));
        }

        public static void add(bool enabled, string type, string expression)
        {
            DBExpression r1 = new DBExpression();
            r1.Enabled = enabled;
            r1.Expression = expression;
            r1.Type = type;
            r1.Commit();
        }

        public static void ClearAll()
        {
            List<DBExpression> rp = GetAll();
            foreach (DBExpression r1 in rp)
            {
                r1.Delete();
                r1.Commit();
            }
        }

        public static void Exchange(int i, int x)
        {
            DBExpression db1 = Get(i);
            DBExpression db2 = Get(x);
            if (db1 == null || db2 == null) return;
            DBExpression tdb = new DBExpression();
            tdb.Enabled = db1.Enabled;
            tdb.Type = db1.Type;
            tdb.Expression = db1.Expression;
            db1.Enabled = db2.Enabled;
            db1.Type = db2.Type;
            db1.Expression = db2.Expression;
            db1.Commit();
            db2.Enabled = tdb.Enabled;
            db2.Type = tdb.Type;
            db2.Expression = tdb.Expression;
            db2.Commit();
        }
        public static void Clear(int Index) {
 //           return mvCentralCore.DatabaseManager.C
//            DBExpression dummy = new DBExpression(Index);
//            Clear(dummy, new SQLCondition(dummy, DBExpression.cIndex, Index, SQLConditionType.Equal));
            
        }

        public static DBExpression Get(int index)
        {
            return mvCentralCore.DatabaseManager.Get<DBExpression>(index);
        }

        public static List<DBExpression> GetAll()
        {
            return mvCentralCore.DatabaseManager.Get<DBExpression>(null);
        }
    }
}
