using System;
using System.Collections.Generic;
using System.Text;
using Cornerstone.Database;
using Cornerstone.Database.Tables;

namespace mvCentral.Database {
    [DBTableAttribute("users")]
    public class DBUser: mvCentralDBTable {

        public override void AfterDelete() {
        }

        #region Database Fields

        [DBFieldAttribute(Default="New User")]
        public string Name {
            get { return name; }
            set { 
                name = value;
                commitNeeded = true;
            }
        } private string name;

        #endregion

        #region Database Management Methods

        public static DBUser Get(int id) {
            return mvCentralCore.DatabaseManager.Get<DBUser>(id);
        }

        public static List<DBUser> GetAll() {
            return mvCentralCore.DatabaseManager.Get<DBUser>(null);
        }

        #endregion

        public override string ToString() {
            return "DBUser: " + Name;
        }
    }
}
