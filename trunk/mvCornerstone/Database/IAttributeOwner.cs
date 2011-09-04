using System;
using System.Collections.Generic;
using System.Text;
using mvCornerstone.Database.Tables;
using mvCornerstone.Database.CustomTypes;

namespace mvCornerstone.Database {
    public interface IAttributeOwner {
        RelationList<DatabaseTable, DBAttribute> Attributes {
            get;
        }
    }
}
