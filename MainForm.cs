﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodeProject.Dialog;

namespace SharpDisplayManager
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void buttonFont_Click(object sender, EventArgs e)
        {
            //fontDialog.ShowColor = true;
            //fontDialog.ShowApply = true;
            fontDialog.ShowEffects = true;
            //fontDialog.ShowHelp = true;

            //fontDlg.MaxSize = 40;
            //fontDlg.MinSize = 22;

            //fontDialog.Parent = this;
            //fontDialog.StartPosition = FormStartPosition.CenterParent;

            //DlgBox.ShowDialog(fontDialog);

            //if (fontDialog.ShowDialog(this) != DialogResult.Cancel)
            if (DlgBox.ShowDialog(fontDialog) != DialogResult.Cancel)
            {

                MsgBox.Show("MessageBox MsgBox", "MsgBox caption");

                //MessageBox.Show("Ok");
                //textBox1.Font = fontDlg.Font;
                //label1.Font = fontDlg.Font;
                //textBox1.BackColor = fontDlg.Color;
                //label1.ForeColor = fontDlg.Color;
            }
        }

        private void buttonCapture_Click(object sender, EventArgs e)
        {
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(tableLayoutPanel.Width, tableLayoutPanel.Height);
            tableLayoutPanel.DrawToBitmap(bmp, tableLayoutPanel.ClientRectangle);
            bmp.Save("c:\\capture.png");
        }
    }
}
