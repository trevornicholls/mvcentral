using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Reflection;
using Microsoft.Win32;
using System.Collections;

namespace TranslationFileBuild
{
  public partial class TranslationBuilder : Form
  {

    List<TranslatedString> TranslatedStrings = new List<TranslatedString>();

    string cellBefore = string.Empty;
    string localLangCode = string.Empty;
    string localLangName = string.Empty;

    public TranslationBuilder()
    {
      InitializeComponent();
      if (Properties.Settings.Default.prevFolders.Count > 1)
      {
        ArrayList.Adapter(Properties.Settings.Default.prevFolders).Reverse();
      }
      cboPrimaryLangFile.DataSource = Properties.Settings.Default.prevFolders;
    }
    /// <summary>
    /// Split the code and counrty name
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void cbLocallang_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (cbLocallang.SelectedIndex != -1)
      {
        string[] fileSplit = cbLocallang.Text.Split(' ');
        localLangCode = fileSplit[0].Trim();
        localLangName = fileSplit[1].Trim();
      }
    }
    /// <summary>
    /// Load the files
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btLoadFiles_Click(object sender, EventArgs e)
    {
      int localIndex = 0;
      string fName = cboPrimaryLangFile.Text;

      if (!Properties.Settings.Default.prevFolders.Contains(cboPrimaryLangFile.Text))
      {
        Properties.Settings.Default.prevFolders.Add(cboPrimaryLangFile.Text);
        Properties.Settings.Default.Save();
        cboPrimaryLangFile.DataSource = null;
        cboPrimaryLangFile.DataSource = Properties.Settings.Default.prevFolders;
      }
      else
      {
        cboPrimaryLangFile.SelectedIndex = (Properties.Settings.Default.prevFolders.IndexOf(fName));
      }
      TranslatedStrings.Clear();
      lvLangFiles.Items.Clear();

      if (string.IsNullOrEmpty(cbLocallang.Text))
      {
        MessageBox.Show("No Local Langauge Selected - Please Selected Local Langauge");
        return;
      } 
      
      List<TranslatedString> primaryLang = LoadTranslations(cboPrimaryLangFile.Text);
      List<TranslatedString> localLang = LoadTranslations(Path.Combine(Path.GetDirectoryName(cboPrimaryLangFile.Text),  localLangCode + ".xml"));

      if (primaryLang == null || localLang == null)
        return;

      foreach (TranslatedString pTranString in primaryLang)
      {
        localIndex = -1;
        foreach (TranslatedString search in localLang)
        {
          if (pTranString.fieldName == search.fieldName)
          {
            localIndex = localLang.IndexOf(search);
            break;
          }
        }

        if (localIndex != -1)
        {
          ListViewItem item = new ListViewItem(new[] { pTranString.fieldName, pTranString.fieldValue, localLang[localIndex].fieldValue });
          lvLangFiles.Items.Add(item);
          if (string.IsNullOrEmpty(localLang[localIndex].fieldValue))
            item.BackColor = Color.FromKnownColor(KnownColor.Yellow);
        }
        else
        {
          if (pTranString.isFixed)
          {
            ListViewItem item = new ListViewItem(new[] { pTranString.fieldName, pTranString.fieldValue, pTranString.fieldValue });
            lvLangFiles.Items.Add(item);
          }
          else
          {
            ListViewItem item = new ListViewItem(new[] { pTranString.fieldName, pTranString.fieldValue, string.Empty });
            item.BackColor = Color.FromKnownColor(KnownColor.Yellow);
            lvLangFiles.Items.Add(item);
          }
        }    
      }
      lvLangFiles.Refresh();
    }
    /// <summary>
    /// load the translation files
    /// </summary>
    /// <param name="LanguageFile"></param>
    /// <returns></returns>
    private List<TranslatedString> LoadTranslations(string LanguageFile)
    {
      TranslatedStrings.Clear();
      XmlDocument doc = new XmlDocument();

      try
      {
        doc.Load(LanguageFile);
      }
      catch (Exception e)
      {
        if (e.GetType() == typeof(FileNotFoundException))
        {
          MessageBox.Show(string.Format("Cannot find translation file {0}. New file will be created.", Path.GetFileName(LanguageFile)));
        }
        else
        {
          MessageBox.Show(string.Format("Error in translation of xml file: {0}./n" + e.Message,LanguageFile));
          return null;
        }

        return TranslatedStrings.ToList();
      }
      foreach (XmlNode stringEntry in doc.DocumentElement.ChildNodes)
      {
        TranslatedString tString = new TranslatedString();

        if (stringEntry.NodeType == XmlNodeType.Element)
          try
          {
            if (stringEntry.Attributes.GetNamedItem("Field").Value.StartsWith("#"))
              tString.isFixed = true;

            tString.fieldName = stringEntry.Attributes.GetNamedItem("Field").Value;
            tString.fieldValue = stringEntry.InnerText;
            TranslatedStrings.Add(tString);
          }
          catch (Exception e)
          {
            Console.WriteLine("Error in Translation Engine:");
            Application.Exit();
          }
      }
      return TranslatedStrings.ToList();
    }
    /// <summary>
    /// cell clicked, check if correct coloum and start editing
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void lvLangFiles_SubItemClicked(object sender, ListViewEx.SubItemEventArgs e)
    {
      if (e.SubItem != 2)
        return;

      cellBefore = e.Item.SubItems[e.SubItem].Text;

      lvLangFiles.StartEditing(tbSubItemEdit, e.Item, e.SubItem);
    }
    /// <summary>
    /// start editing the cell
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void lvLangFiles_SubItemEndEditing(object sender, ListViewEx.SubItemEndEditingEventArgs e)
    {
      if (cellBefore != e.DisplayText)
        e.Item.BackColor = Color.FromKnownColor(KnownColor.WhiteSmoke);
    }
    /// <summary>
    /// write out the xml file
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btSave_Click(object sender, EventArgs e)
    {
      string inFileText = string.Empty;
      string xml = string.Empty;

      List<TranslatedString> primaryLang = LoadTranslations(cboPrimaryLangFile.Text);
      List<TranslatedString> newFile = new List<TranslatedString>();
      foreach (ListViewItem item in lvLangFiles.Items)
      {
        TranslatedString newTran = new TranslatedString();
        newTran.isFixed = primaryLang[lvLangFiles.Items.IndexOf(item)].isFixed;
        newTran.fieldName = item.SubItems[0].Text;
        newTran.fieldValue = item.SubItems[2].Text;
        newFile.Add(newTran);
      }

      if (File.Exists(Path.Combine(Path.GetDirectoryName(cboPrimaryLangFile.Text), localLangCode + ".xml")))
      {
        StreamReader inFile = new StreamReader(Path.Combine(Path.GetDirectoryName(cboPrimaryLangFile.Text), localLangCode + ".xml"));
        do
        {
          inFileText = inFile.ReadLine();
          xml += inFileText + "\n";
        } while (inFileText != "<strings>");

        inFile.Close();
      }
      else
      {

        // Now write out the xml
        xml = "<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>\n" +
               "<!-- Autogenerated Translation file -->\n" +
               "<!-- " + cbLocallang.Text + ") -->\n" +
               "<!-- Note: English is the fallback for any strings not found in other languages -->\n" +
               "<strings>\n" +
               "        <!-- MediaPortal Localised IDs -> Skin Properties -->\n";
      }
      foreach (TranslatedString tLine in newFile)
      {
        xml += "        <string Field=\"" + tLine.fieldName + "\">" + tLine.fieldValue + "</string>\n";
      }
      xml += "</strings>";
      // Write out the XML
      writeXMLFile(xml,localLangCode + ".xml");
      lvLangFiles.Items.Clear();
      cbLocallang.SelectedIndex = -1;
    }
    /// <summary>
    /// Write out the xml file
    /// </summary>
    /// <param name="xml"></param>
    /// <param name="xmlFileName"></param>
    public void writeXMLFile(string xml,string xmlFileName)
    {
      string filePath = Path.Combine(Path.GetDirectoryName(cboPrimaryLangFile.Text), xmlFileName);

      // Delete any existing file
      if (System.IO.File.Exists(filePath))
        System.IO.File.Delete(filePath);

      using (StreamWriter tmpwriter = new StreamWriter(filePath, true, Encoding.GetEncoding("iso-8859-1")))
      {
        tmpwriter.Write(xml);
        tmpwriter.Close();
      }
    }
    /// <summary>
    /// browser to file
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btBrowse_Click(object sender, EventArgs e)
    {
      using (OpenFileDialog primaryLanguageFilename = new OpenFileDialog())
      {
        primaryLanguageFilename.InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Team MediaPortal\MediaPortal");
        if (primaryLanguageFilename.ShowDialog() == DialogResult.OK)
        {
          cboPrimaryLangFile.Text = primaryLanguageFilename.FileName;
        }
      }
    }
    /// <summary>
    ///  handle drop to combo box
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void cboPrimaryLangFile_DragDrop(object sender, DragEventArgs e)
    {
      string[] FileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);
      cboPrimaryLangFile.Text = FileList[0];
    }
    /// <summary>
    /// check if file dragged over is and xml file and only 1 of them
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void cboPrimaryLangFile_DragEnter(object sender, DragEventArgs e)
    {
      string[] FileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);
      if (e.Data.GetDataPresent(DataFormats.FileDrop) && Path.GetExtension(FileList[0]) == ".xml" && FileList.Count() == 1)
        e.Effect = DragDropEffects.Copy;
      else
        e.Effect = DragDropEffects.None;
    }
    /// <summary>
    /// Resize the string coloums along with the window
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TranslationBuilder_Resize(object sender, EventArgs e)
    {
      int newWidth = lvLangFiles.Width;
      int colWith = (newWidth - 180) / 2;
      lvLangFiles.Columns[1].Width = colWith - 2;
      lvLangFiles.Columns[2].Width = colWith - 2;
      lvLangFiles.Width = newWidth;
      lvLangFiles.Refresh();
    }
    /// <summary>
    /// Clear the list and local language selection
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btReset_Click(object sender, EventArgs e)
    {
      lvLangFiles.Items.Clear();
      cbLocallang.SelectedIndex = -1;
    }

    #region Classes

    public class TranslatedString
    {
      public bool isFixed { get; set; }
      public string fieldName { get; set; }
      public string fieldValue { get; set; }
    }


    #endregion


  }
}
