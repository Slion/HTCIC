﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using CodeProject.Dialog;
using System.Drawing.Imaging;


namespace SharpDisplayManager
{
    public partial class MainForm : Form
    {
        DateTime LastTickTime;
        Display iDisplay;
        System.Drawing.Bitmap iBmp;
        bool iCreateBitmap; //Workaround render to bitmap issues when minimized

        public MainForm()
        {
            LastTickTime = DateTime.Now;
            iDisplay = new Display();

            InitializeComponent();
            UpdateStatus();

            //Load settings
            marqueeLabelTop.Font = Properties.Settings.Default.DisplayFont;
            marqueeLabelBottom.Font = Properties.Settings.Default.DisplayFont;
            checkBoxShowBorders.Checked = Properties.Settings.Default.DisplayShowBorders;
            checkBoxConnectOnStartup.Checked = Properties.Settings.Default.DisplayConnectOnStartup;
            //
            tableLayoutPanel.CellBorderStyle = (checkBoxShowBorders.Checked ? TableLayoutPanelCellBorderStyle.Single : TableLayoutPanelCellBorderStyle.None);
            //We have a bug when drawing minimized and reusing our bitmap
            iBmp = new System.Drawing.Bitmap(tableLayoutPanel.Width, tableLayoutPanel.Height, PixelFormat.Format32bppArgb);
            iCreateBitmap = false;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.DisplayConnectOnStartup)
            {
                iDisplay.Open();
                UpdateStatus();
            }
        }


        private void buttonFont_Click(object sender, EventArgs e)
        {
            //fontDialog.ShowColor = true;
            //fontDialog.ShowApply = true;
            fontDialog.ShowEffects = true;
            fontDialog.Font = marqueeLabelTop.Font;
            //fontDialog.ShowHelp = true;

            //fontDlg.MaxSize = 40;
            //fontDlg.MinSize = 22;

            //fontDialog.Parent = this;
            //fontDialog.StartPosition = FormStartPosition.CenterParent;

            //DlgBox.ShowDialog(fontDialog);

            //if (fontDialog.ShowDialog(this) != DialogResult.Cancel)
            if (DlgBox.ShowDialog(fontDialog) != DialogResult.Cancel)
            {

                //MsgBox.Show("MessageBox MsgBox", "MsgBox caption");

                //MessageBox.Show("Ok");
                marqueeLabelTop.Font = fontDialog.Font;
                marqueeLabelBottom.Font = fontDialog.Font;
                Properties.Settings.Default.DisplayFont = fontDialog.Font;
                Properties.Settings.Default.Save();
                //label1.Font = fontDlg.Font;
                //textBox1.BackColor = fontDlg.Color;
                //label1.ForeColor = fontDlg.Color;
            }
        }

        private void buttonCapture_Click(object sender, EventArgs e)
        {
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(tableLayoutPanel.Width, tableLayoutPanel.Height);
            tableLayoutPanel.DrawToBitmap(bmp, tableLayoutPanel.ClientRectangle);
            //Bitmap bmpToSave = new Bitmap(bmp);
            bmp.Save("D:\\capture.png");

            /*
            string outputFileName = "d:\\capture.png";
            using (MemoryStream memory = new MemoryStream())
            {
                using (FileStream fs = new FileStream(outputFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    bmp.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                    byte[] bytes = memory.ToArray();
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
             */

        }

        private void CheckForRequestResults()
        {
            if (iDisplay.IsRequestPending())
            {
                switch (iDisplay.AttemptRequestCompletion())
                {
                    case Display.TMiniDisplayRequest.EMiniDisplayRequestPowerSupplyStatus:
                        if (iDisplay.PowerSupplyStatus())
                        {
                            toolStripStatusLabelPower.Text = "ON";
                        }
                        else
                        {
                            toolStripStatusLabelPower.Text = "OFF";
                        }
                        //Issue next request then
                        iDisplay.RequestDeviceId();
                        break;

                    case Display.TMiniDisplayRequest.EMiniDisplayRequestDeviceId:
                        toolStripStatusLabelConnect.Text += " - " + iDisplay.DeviceId();
                        //Issue next request then
                        iDisplay.RequestFirmwareRevision();
                        break;

                    case Display.TMiniDisplayRequest.EMiniDisplayRequestFirmwareRevision:
                        toolStripStatusLabelConnect.Text += " v" + iDisplay.FirmwareRevision();
                        //No more request to issue
                        break;
                }
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            //Update our animations
            DateTime NewTickTime = DateTime.Now;

            marqueeLabelTop.UpdateAnimation(LastTickTime, NewTickTime);
            marqueeLabelBottom.UpdateAnimation(LastTickTime, NewTickTime);

            //Update our display
            if (iDisplay.IsOpen())
            {
                CheckForRequestResults();

                //Draw to bitmap                
                if (iCreateBitmap)
                {
                    iBmp = new System.Drawing.Bitmap(tableLayoutPanel.Width, tableLayoutPanel.Height, PixelFormat.Format32bppArgb);
                }
                tableLayoutPanel.DrawToBitmap(iBmp, tableLayoutPanel.ClientRectangle);
                //iBmp.Save("D:\\capture.png");
                
                //iBmp.

                //Send it to our display
                for (int i = 0; i < iBmp.Width; i++)
                {
                    for (int j = 0; j < iBmp.Height; j++)
                    {
                        unchecked
                        {
                            uint color = (uint)iBmp.GetPixel(i, j).ToArgb();
                            //For some reason when the app is minimized in the task bar only the alpha of our color is set.
                            //Thus that strange test for rendering to work both when the app is in the task bar and when it isn't.
                            iDisplay.SetPixel(i, j, Convert.ToInt32(!(color != 0xFF000000)));
                        }
                    }
                }

                iDisplay.SwapBuffers();

            }

            //Compute instant FPS
            toolStripStatusLabelFps.Text = (1.0/NewTickTime.Subtract(LastTickTime).TotalSeconds).ToString("F0") + " FPS";

            LastTickTime = NewTickTime;

        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            if (iDisplay.Open())
            {
                UpdateStatus();
                iDisplay.RequestPowerSupplyStatus();
            }
            else
            {
                UpdateStatus();
                toolStripStatusLabelConnect.Text = "Connection error";
            }

        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            iDisplay.Close();
            UpdateStatus();
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            iDisplay.Clear();
            iDisplay.SwapBuffers();
        }

        private void buttonFill_Click(object sender, EventArgs e)
        {
            iDisplay.Fill();
            iDisplay.SwapBuffers();
        }

        private void trackBarBrightness_Scroll(object sender, EventArgs e)
        {
            Properties.Settings.Default.DisplayBrightness = trackBarBrightness.Value;
            Properties.Settings.Default.Save();
            iDisplay.SetBrightness(trackBarBrightness.Value);

        }

        private void UpdateStatus()
        {
            if (iDisplay.IsOpen())
            {
                buttonFill.Enabled = true;
                buttonClear.Enabled = true;
                buttonOpen.Enabled = false;
                buttonClose.Enabled = true;
                trackBarBrightness.Enabled = true;
                trackBarBrightness.Minimum = iDisplay.MinBrightness();
                trackBarBrightness.Maximum = iDisplay.MaxBrightness();
                trackBarBrightness.Value = Properties.Settings.Default.DisplayBrightness;
                trackBarBrightness.LargeChange = Math.Max(1,(iDisplay.MaxBrightness() - iDisplay.MinBrightness())/5);
                trackBarBrightness.SmallChange = 1;
                iDisplay.SetBrightness(Properties.Settings.Default.DisplayBrightness);

                toolStripStatusLabelConnect.Text = "Connected - " + iDisplay.Vendor() + " - " + iDisplay.Product();
                //+ " - " + iDisplay.SerialNumber();
            }
            else
            {
                buttonFill.Enabled = false;
                buttonClear.Enabled = false;
                buttonOpen.Enabled = true;
                buttonClose.Enabled = false;
                trackBarBrightness.Enabled = false;
                toolStripStatusLabelConnect.Text = "Disconnected";
            }
        }



        private void checkBoxShowBorders_CheckedChanged(object sender, EventArgs e)
        {
            tableLayoutPanel.CellBorderStyle = (checkBoxShowBorders.Checked ? TableLayoutPanelCellBorderStyle.Single : TableLayoutPanelCellBorderStyle.None);
            Properties.Settings.Default.DisplayShowBorders = checkBoxShowBorders.Checked;
            Properties.Settings.Default.Save();
        }

        private void checkBoxConnectOnStartup_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.DisplayConnectOnStartup = checkBoxConnectOnStartup.Checked;
            Properties.Settings.Default.Save();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                // Do some stuff
                //iBmp = new System.Drawing.Bitmap(tableLayoutPanel.Width, tableLayoutPanel.Height, PixelFormat.Format32bppArgb);
                iCreateBitmap = true;
            }
        }

    }
}
