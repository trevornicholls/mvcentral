using Cornerstone.Database.Tables;

namespace mvCentral.Database {
    public abstract class mvCentralDBTable: DatabaseTable {

        public mvCentralDBTable()
            : base() {
        }

        public override void Commit() {
            if (DBManager == null)
                DBManager = mvCentralCore.DatabaseManager;

            base.Commit();
        }

        public override void Delete() {
            if (DBManager == null)
                DBManager = mvCentralCore.DatabaseManager;

            base.Delete();
        }
    }
}
