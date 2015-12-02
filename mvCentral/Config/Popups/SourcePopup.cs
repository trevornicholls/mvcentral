using mvCentral.Database;

using System.Collections.Generic;
using System.Windows.Forms;

namespace mvCentral.ConfigScreen.Popups
{
    public partial class SourcePopup : Form
    {
        public SourcePopup()
        {
            InitializeComponent();
        }

        public SourcePopup(List<DBSourceInfo> r1)
        {
            InitializeComponent();
            listBox1.DataSource = r1;
            // Define the field to be displayed
            // listBox1.DisplayMember = "Provider";

            // Define the field to be used as the value
            listBox1.ValueMember = "";
        }
    }
}
