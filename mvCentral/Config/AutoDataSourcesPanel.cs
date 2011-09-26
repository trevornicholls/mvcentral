using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using mvCentral.ConfigScreen.Popups;
using Cornerstone.Tools.Translate;

namespace mvCentral
{
    public partial class AutoDataSourcesPanel : UserControl {
        
        private bool   initializing = false;
        private int    lineOffset = 1;
        private string additionalOptionsText = "Additional Options...";

        public bool HostedDesignMode {
            get {
                Control parent = Parent;
                while (parent != null && parent.Site != null) {
                    if (parent.Site.DesignMode) return true;
                    parent = parent.Parent;
                }
                return DesignMode;
            }
        }

        public bool AutoCommit {
            get { return _autoCommit; }
            set { _autoCommit = value; }
        } private bool _autoCommit = true;

        public AutoDataSourcesPanel() {
            InitializeComponent();
            languageComboBox.DrawMode = DrawMode.OwnerDrawFixed;
            languageComboBox.DrawItem += new DrawItemEventHandler(comboBox1_DrawItem);
        }

        void comboBox1_DrawItem(object sender, DrawItemEventArgs e) {
            if (!languageComboBox.DroppedDown) {
                e.Graphics.DrawRectangle(Pens.White, e.Bounds);
            }
            else {
                e.DrawBackground();

                if (e.Index == languageComboBox.Items.Count - (1 + lineOffset)) {
                    e.Graphics.DrawLine(Pens.DarkGray, new Point(e.Bounds.Left, e.Bounds.Bottom - 1),
                                        new Point(e.Bounds.Right, e.Bounds.Bottom - 1));
                }
            }

            if (e.Index != -1) {
                string text;
                Color color;
                
                if (languageComboBox.Items[e.Index] is CultureInfo) {
                    text = ((CultureInfo)languageComboBox.Items[e.Index]).DisplayName;
                    color = languageComboBox.ForeColor;
                }
                else {
                    text = languageComboBox.Items[e.Index].ToString();
                    color = Color.DarkBlue;
                }

                TextRenderer.DrawText(e.Graphics, text, languageComboBox.Font, e.Bounds, 
                                      color, TextFormatFlags.Left);
            }

            e.DrawFocusRectangle();
        }

        private void AutoDataSourcesPanel_Load(object sender, EventArgs e) {
            UpdateControls();
        }

        public void Commit() {
            if (autoRadioButton.Checked) {
                mvCentralCore.Settings.DataProviderManagementMethod = "auto";

                if (languageComboBox.SelectedItem is CultureInfo) {
                    mvCentralCore.Settings.UseTranslator = false;
                    mvCentralCore.Settings.DataProviderAutoLanguage = ((CultureInfo)languageComboBox.SelectedItem).TwoLetterISOLanguageName;
                    mvCentralCore.DataProviderManager.AutoArrangeDataProviders();
                } else {
                    mvCentralCore.Settings.UseTranslator = true;
                    mvCentralCore.Settings.DataProviderAutoLanguage = "en";
                    mvCentralCore.DataProviderManager.AutoArrangeDataProviders();
                }
            }

            if (manualRadioButton.Checked) {
                mvCentralCore.Settings.DataProviderManagementMethod = "manual";
            }
        }

        private void UpdateControls() {
            if (HostedDesignMode)
                return;

            initializing = true;
            lineOffset = 1;

            languageComboBox.Items.Clear();
            languageComboBox.Items.AddRange(mvCentralCore.DataProviderManager.GetAvailableLanguages().ToArray());

            if (mvCentralCore.Settings.TranslatorConfigured) {
                languageComboBox.Items.Add("Translated: " + mvCentralCore.Settings.TranslationLanguage);
                lineOffset = 2;
            }
            
            languageComboBox.Items.Add(additionalOptionsText);

            if (mvCentralCore.Settings.UseTranslator && mvCentralCore.Settings.TranslatorConfigured) 
                languageComboBox.SelectedIndex = languageComboBox.Items.Count - lineOffset;
            else 
                languageComboBox.SelectedItem = new CultureInfo(mvCentralCore.Settings.DataProviderAutoLanguage);



            if (mvCentralCore.Settings.DataProviderManagementMethod == "auto") {
                autoRadioButton.Checked = true;
                manualRadioButton.Checked = false;
                languageComboBox.Enabled = true;
                languageComboBox.ForeColor = SystemColors.ControlText;
            }
            else if (mvCentralCore.Settings.DataProviderManagementMethod == "undefined") {
                mvCentralCore.Settings.DataProviderManagementMethod = "auto";

                autoRadioButton.Checked = true;
                manualRadioButton.Checked = false;
                languageComboBox.Enabled = true;
                languageComboBox.ForeColor = SystemColors.ControlText;
            }
            else if (mvCentralCore.Settings.DataProviderManagementMethod == "manual") {
                autoRadioButton.Checked = false;
                manualRadioButton.Checked = true;
                languageComboBox.Enabled = false;
                languageComboBox.ForeColor = Color.DarkGray;
            }

            initializing = false;           
        }

        private void autoRadioButton_CheckedChanged(object sender, EventArgs e) {
            if (autoRadioButton.Checked) {
                if (AutoCommit) {
                    mvCentralCore.Settings.DataProviderManagementMethod = "auto";

                    if (mvCentralCore.Settings.UseTranslator) 
                        mvCentralCore.Settings.DataProviderAutoLanguage = "en";

                    mvCentralCore.DataProviderManager.AutoArrangeDataProviders();
                }

                UpdateControls();
                languageComboBox.Enabled = true;
                languageComboBox.ForeColor = SystemColors.ControlText;
            }
        }

        private void manualRadioButton_CheckedChanged(object sender, EventArgs e) {
            if (manualRadioButton.Checked) {
                if (AutoCommit)
                    mvCentralCore.Settings.DataProviderManagementMethod = "manual";

                languageComboBox.Enabled = false;
                languageComboBox.ForeColor = Color.DarkGray;
            }
        }

        private void languageComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            if (initializing)
                return;

            if (languageComboBox.SelectedItem is CultureInfo) {
                mvCentralCore.Settings.UseTranslator = false;

                if (AutoCommit) {
                    mvCentralCore.Settings.DataProviderAutoLanguage = ((CultureInfo)languageComboBox.SelectedItem).TwoLetterISOLanguageName;
                    mvCentralCore.DataProviderManager.AutoArrangeDataProviders();
                }
            }
            else if (languageComboBox.SelectedItem is string && ((string)languageComboBox.SelectedItem) != additionalOptionsText) {
                if (AutoCommit) {
                    mvCentralCore.Settings.UseTranslator = true;
                    mvCentralCore.Settings.DataProviderAutoLanguage = "en";
                    mvCentralCore.DataProviderManager.AutoArrangeDataProviders();
                }
            }
            else {
                TranslationPopup popup = new TranslationPopup();
                popup.Owner = FindForm();
                DialogResult result = popup.ShowDialog();

                if (result == DialogResult.OK) {
                    if (AutoCommit) {
                        mvCentralCore.Settings.DataProviderAutoLanguage = "en";
                        mvCentralCore.DataProviderManager.AutoArrangeDataProviders();
                    }

                    mvCentralCore.Settings.TranslatorConfigured = true;

                }
                                
                UpdateControls();
            }         
        }
    }
}
