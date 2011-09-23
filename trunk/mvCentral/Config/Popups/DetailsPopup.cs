using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using mvCentral.DataProviders;

namespace mvCentral.ConfigScreen.Popups
{
  public partial class DetailsPopup : Form
  {
    public DetailsPopup()
    {
      InitializeComponent();
    }

    public DetailsPopup(List<Release> r1)
    {
      InitializeComponent();
      listBox1.DataSource = r1;
      // Define the field to be displayed
      listBox1.DisplayMember = "title";

      // Define the field to be used as the value
      listBox1.ValueMember = "title";

      label1.DataBindings.Add("Text", r1, "type");
      label2.DataBindings.Add("Text", r1, "status");
      label3.DataBindings.Add("Text", r1, "title");
      label4.DataBindings.Add("Text", r1, "format");
      label5.DataBindings.Add("Text", r1, "label");
      label6.DataBindings.Add("Text", r1, "uri");
      label7.DataBindings.Add("Text", r1, "summary");
      label8.DataBindings.Add("Text", r1, "id");
      textBox1.DataBindings.Add("Text", r1, "title");
    }


  }
}
