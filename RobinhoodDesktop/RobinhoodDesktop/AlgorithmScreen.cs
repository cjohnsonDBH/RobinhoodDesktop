﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace RobinhoodDesktop
{
    public class AlgorithmScreen
    {
        public AlgorithmScreen()
        {
            GuiPanel = new Panel();

            GuiBox = new Panel();
            GuiBox.AutoSize = true;
            GuiBox.BackColor = GuiStyle.BACKGROUND_COLOR;

            // Add the GUI elements for configurating the script
            DataFileButton = new GuiButton("Open...");
            DataFileButton.Location = new Point(5, 30);
            DataFileButton.MouseUp += (sender, e) =>
            {
                System.Windows.Forms.OpenFileDialog diag = new System.Windows.Forms.OpenFileDialog();
                diag.Multiselect = true;
                diag.Title = "Open Stock Data File...";
                if(diag.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if((DataFileTextbox.Text.Length > 0) && (!DataFileTextbox.Text.EndsWith("\n"))) DataFileTextbox.Text += "\r\n";
                    foreach(string dataPath in diag.FileNames)
                    {
                        DataFileTextbox.Text += dataPath + "\r\n";
                    }
                    DataFileTextbox.Text = DataFileTextbox.Text.Remove(DataFileTextbox.Text.Length - 2, 2);
                    DataFileTextbox.SelectionStart = DataFileTextbox.Text.Length;
                    DataFileTextbox.ScrollToCaret();
                    DataFileButton.SetImage(GuiButton.ButtonImage.GREEN_WHITE);
                    DataLiveButton.SetImage(GuiButton.ButtonImage.GREEN_TRANSPARENT);
                }
            };
            GuiBox.Controls.Add(DataFileButton);
            GuiButton clearButton = new GuiButton("Clear");
            clearButton.Location = new Point(DataFileButton.Bounds.Right + 5, DataFileButton.Location.Y);
            clearButton.SetImage(GuiButton.ButtonImage.GREEN_TRANSPARENT);
            clearButton.MouseUp += (sender, e) =>
            {
                DataFileTextbox.Text = "";
            };
            GuiBox.Controls.Add(clearButton);
            DataLiveButton = new GuiButton("Live");
            DataLiveButton.Location = new Point(clearButton.Bounds.Right + 5, clearButton.Location.Y);
            DataLiveButton.SetImage(GuiButton.ButtonImage.GREEN_WHITE);
            DataLiveButton.MouseUp += (sender, e) =>
            {
                DataFileTextbox.Text = "";
                DataLiveButton.SetImage(GuiButton.ButtonImage.GREEN_WHITE);
                DataFileButton.SetImage(GuiButton.ButtonImage.GREEN_TRANSPARENT);
            };
            GuiBox.Controls.Add(DataLiveButton);
            Label sourceDataLabel = new Label();
            sourceDataLabel.Text = "Source Data";
            sourceDataLabel.ForeColor = GuiStyle.PRICE_COLOR_POSITIVE;
            sourceDataLabel.Font = GuiStyle.Font;
            sourceDataLabel.Location = new Point(DataFileButton.Bounds.Left, (DataFileButton.Bounds.Top - sourceDataLabel.Height) + 5);
            GuiBox.Controls.Add(sourceDataLabel);
            DataFileTextbox = new TextBox();
            DataFileTextbox.Location = new Point(DataFileButton.Bounds.Left, DataFileButton.Bounds.Bottom + 5);
            DataFileTextbox.Size = new Size(300, 300);
            DataFileTextbox.Multiline = true;
            DataFileTextbox.WordWrap = false;
            DataFileTextbox.BackColor = GuiStyle.DARK_GREY;
            DataFileTextbox.ForeColor = GuiStyle.TEXT_COLOR;
            GuiBox.Controls.Add(DataFileTextbox);

            // Add the GUI for selecting the data scripts
            DataScriptListPanel = new Panel();
            DataScriptListPanel.Location = new Point(DataFileTextbox.Bounds.Right + 25, DataFileTextbox.Location.Y);
            DataScriptListPanel.Size = new Size(150, 150);
            DataScriptListPanel.BorderStyle = BorderStyle.FixedSingle;
            DataScriptListPanel.ForeColor = GuiStyle.PRICE_COLOR_POSITIVE;
            GuiBox.Controls.Add(DataScriptListPanel);
            Label dataScriptsLabel = new Label();
            dataScriptsLabel.Text = "Data Analysis Scripts";
            dataScriptsLabel.ForeColor = GuiStyle.PRICE_COLOR_POSITIVE;
            dataScriptsLabel.Font = GuiStyle.Font;
            dataScriptsLabel.Location = new Point(DataScriptListPanel.Bounds.Left, (DataScriptListPanel.Bounds.Top - dataScriptsLabel.Height) + 5);
            GuiBox.Controls.Add(dataScriptsLabel);
            DataScriptListScrollbar = new CustomControls.CustomScrollbar();
            DataScriptListScrollbar.Minimum = 0;
            DataScriptListScrollbar.Maximum = DataScriptListPanel.Height;
            DataScriptListScrollbar.LargeChange = DataScriptListScrollbar.Maximum / DataScriptListScrollbar.Height;
            DataScriptListScrollbar.SmallChange = 15;
            DataScriptListScrollbar.Value = 0;
            DataScriptListScrollbar.Attach(DataScriptListPanel);
            GuiBox.Controls.Add(DataScriptListScrollbar);
            DataScriptAddButton = new GuiButton("Add...");
            DataScriptAddButton.Location = new Point(DataScriptListPanel.Location.X + 5, DataScriptListPanel.Bounds.Bottom + 5);
            DataScriptAddButton.MouseUp += (sender, e) =>
            {
                System.Windows.Forms.OpenFileDialog diag = new System.Windows.Forms.OpenFileDialog();
                diag.Multiselect = true;
                diag.Filter = "C# Files|*.cs";
                diag.Title = "Open stock data script(s)...";
                string prevPath = diag.InitialDirectory;
                diag.InitialDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Script\\Data";
                diag.RestoreDirectory = false;
                if(diag.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    foreach(string scriptPath in diag.FileNames)
                    {
                        // Skip this item if it has already been loaded
                        var script = scriptPath;
                        if(DataScripts.Contains(script)) continue;

                        // Add a label for the script
                        int yPos = (DataScriptListPanel.Controls.Count > 0) ? DataScriptListPanel.Controls[DataScriptListPanel.Controls.Count - 1].Bounds.Bottom + 5 : 5;
                        var scriptLabel = new Label();
                        scriptLabel.Text = script.Substring(script.Replace('\\', '/').LastIndexOf("/") + 1);
                        scriptLabel.Width = (DataScriptListPanel.Width - 5) - scriptLabel.Location.X;
                        scriptLabel.ForeColor = GuiStyle.PRICE_COLOR_POSITIVE;
                        scriptLabel.Width = (DataScriptListPanel.Width - scriptLabel.Location.X - 30);
                        DataScriptListPanel.Controls.Add(scriptLabel);

                        // Add a remove button for the script
                        var removeButton = new PictureBox();
                        removeButton.Image = Bitmap.FromFile("Content/GUI/Button_Close.png");
                        removeButton.Size = new Size(removeButton.Image.Width, removeButton.Image.Height);
                        scriptLabel.LocationChanged += (sScript, eScript) =>
                        {
                            removeButton.Location = new Point(DataScriptListPanel.Width - removeButton.Width - 5, scriptLabel.Location.Y - ((removeButton.Height - scriptLabel.Height) / 2));
                        };
                        removeButton.MouseUp += (subSender, subE) =>
                        {
                            if(DataScriptListPanel.Controls.Contains(scriptLabel))
                            {
                                DataScriptListPanel.Controls.Remove(scriptLabel);
                                DataScriptListPanel.Controls.Remove(removeButton);

                                // Pack the list to fill the gaps
                                int packPos = 5;
                                foreach(Control c in DataScriptListPanel.Controls)
                                {
                                    if(c.GetType().Equals(typeof(Label)))
                                    {
                                        c.Location = new Point(c.Location.X, packPos);
                                        packPos = c.Bounds.Bottom + 5;
                                    }
                                }
                            }
                            var pathIdx = DataScripts.IndexOf(script);
                            if(pathIdx >= 0) DataScripts.RemoveAt(pathIdx);
                        };
                        DataScriptListPanel.Controls.Add(removeButton);

                        scriptLabel.Location = new Point(5, yPos);
                        DataScripts.Add(script);
                    }
                }
            };
            GuiBox.Controls.Add(DataScriptAddButton);

            // Create the GUI to select the decision and result script
            DecisionScriptTextbox = new TextBox();
            DecisionScriptTextbox.Location = new Point(DataScriptListPanel.Bounds.Right + 50, DataScriptListPanel.Location.Y);
            DecisionScriptTextbox.Width = DataScriptListPanel.Width;
            DecisionScriptTextbox.Multiline = false;
            DecisionScriptTextbox.WordWrap = false;
            DecisionScriptTextbox.BackColor = GuiStyle.DARK_GREY;
            DecisionScriptTextbox.ForeColor = GuiStyle.TEXT_COLOR;
            GuiBox.Controls.Add(DecisionScriptTextbox);
            var DecisionScriptButton = new GuiButton("Open...");
            DecisionScriptButton.Location = new Point(DecisionScriptTextbox.Bounds.Left, DecisionScriptTextbox.Bounds.Top - DecisionScriptButton.Height - 5);
            DecisionScriptButton.MouseUp += (sender, e) =>
            {
                System.Windows.Forms.OpenFileDialog diag = new System.Windows.Forms.OpenFileDialog();
                diag.Multiselect = false;
                diag.Title = "Open Decision Script File...";
                diag.Filter = "C# Files|*.cs";
                if(diag.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    DecisionScriptTextbox.Text = diag.FileName;
                }
            };
            GuiBox.Controls.Add(DecisionScriptButton);
            Label decisionScriptLabel = new Label();
            decisionScriptLabel.Text = "Decision Script";
            decisionScriptLabel.ForeColor = GuiStyle.PRICE_COLOR_POSITIVE;
            decisionScriptLabel.Font = GuiStyle.Font;
            decisionScriptLabel.Location = new Point(DecisionScriptTextbox.Bounds.Left, (DecisionScriptButton.Bounds.Top - decisionScriptLabel.Height) + 5);
            GuiBox.Controls.Add(decisionScriptLabel);

            // Create the start button
            var StartButton = new GuiButton("Start");
            StartButton.Location = new Point(DecisionScriptTextbox.Bounds.Right + 50, DecisionScriptTextbox.Bounds.Top);
            StartButton.MouseUp += (sender, e) =>
            {
                Script.StockSession.Start(DataFileTextbox.Text.Replace("\r", "").Split('\n').ToList(), DataScripts, DecisionScriptTextbox.Text);
            };
            GuiPanel.Controls.Add(StartButton);


            // Create the back button to leave the screen
            BackButton = new PictureBox();
            BackButton.Image = Bitmap.FromFile("Content/GUI/Back.png");
            BackButton.Size = BackButton.Image.Size;
            BackButton.Location = new Point(5, 0);
            GuiPanel.Controls.Add(BackButton);             

            // Define the overall panel that everything else is contained inside
            GuiPanel.BackColor = GuiStyle.BACKGROUND_COLOR;
            GuiPanel.Controls.Add(GuiBox);
            GuiPanel.Resize += (sender, e) =>
            {
                GuiBox.Size = new Size(GuiPanel.Width - BackButton.Width, 300);
                GuiBox.Location = new System.Drawing.Point(BackButton.Width + 10, 10);
            };
        }

        #region Variables
        /// <summary>
        /// Stores the configuration used to generate the session
        /// </summary>
        public Script.StockSession Session;

        /// <summary>
        /// The background panel for the login screen
        /// </summary>
        public Panel GuiPanel;

        /// <summary>
        /// Button that exits this screen
        /// </summary>
        public PictureBox BackButton;

        /// <summary>
        /// List of scripts that analyze the stock point data
        /// </summary>
        public List<string> DataScripts = new List<string>();

        /// <summary>
        /// A box containing the main GUI elements (to make them easier to re-position)
        /// </summary>
        private Panel GuiBox;

        private GuiButton DataFileButton;
        private GuiButton DataLiveButton;
        private TextBox DataFileTextbox;

        private GuiButton DataScriptAddButton;
        private Panel DataScriptListPanel;
        private CustomControls.CustomScrollbar DataScriptListScrollbar;

        private TextBox DecisionScriptTextbox;
        #endregion

        private void RefreshDataListPanel()
        {

        }
    }
}
