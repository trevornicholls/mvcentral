using System;
using System.Collections.Generic;
using System.Text;
using mvCornerstone.Database.Tables;
using System.ComponentModel;

namespace mvCornerstone.GUI.Controls {
    public interface IDBBackedControl {
        // The database object type that the control displays data about.
        Type Table {
            get;
            set;
        }

        // The object cotnaining the data to be displayed.
        DatabaseTable DatabaseObject {
            get;
            set;
        }
    }
}
