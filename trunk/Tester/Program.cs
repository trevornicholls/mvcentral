using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using mvCentral;

namespace ConfigTester
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            ConfigForm config = new ConfigForm();
            config.ShowPlugin();
        }
    }
}