using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using mvCentral.Database;

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
