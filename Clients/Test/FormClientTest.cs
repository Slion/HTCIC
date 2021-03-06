﻿//
// Copyright (C) 2014-2015 Stéphane Lenclud.
//
// This file is part of SharpDisplayManager.
//
// SharpDisplayManager is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// SharpDisplayManager is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with SharpDisplayManager.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Diagnostics;
using SharpLib.Display;


namespace SharpDisplayClient
{
    public partial class FormClientTest : Form
    {
		public StartParams Params { get; set; }

		//
        Client iClient;
		//
        ContentAlignment Alignment;
        TextField iTextFieldTop;

		
		/// <summary>
		/// Constructor
		/// </summary>
        public FormClientTest()
        {
            InitializeComponent();
            Alignment = ContentAlignment.MiddleLeft;
            iTextFieldTop = new TextField();
        }

        public void OnCloseOrder()
        {
            CloseThreadSafe();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            iClient = new Client();
            iClient.CloseOrderEvent += OnCloseOrder;
            iClient.Open();

            //Connect using unique name
            //string name = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt");
            string name = "Client-" + (iClient.ClientCount() - 1);
            iClient.SetName(name);
            //Text = Text + ": " + name;
            Text = "[[" + name + "]]  " + iClient.SessionId;

            //
            textBoxTop.Text = iClient.Name;
            textBoxBottom.Text = iClient.SessionId;

			if (Params != null)
			{
				//Parameters where specified use them
				if (Params.TopText != "")
				{
					textBoxTop.Text = Params.TopText;
				}

				if (Params.BottomText != "")
				{
					textBoxBottom.Text = Params.BottomText;
				}

				Location = Params.Location;
				//
				SetBasicLayoutAndText();
			}

        }



        public delegate void CloseConnectionDelegate();
        public delegate void CloseDelegate();

        /// <summary>
        ///
        /// </summary>
        public void CloseConnectionThreadSafe()
        {
            if (this.InvokeRequired)
            {
                //Not in the proper thread, invoke ourselves
                CloseConnectionDelegate d = new CloseConnectionDelegate(CloseConnectionThreadSafe);
                this.Invoke(d, new object[] { });
            }
            else
            {
                //We are in the proper thread
                if (IsClientReady())
                {
                    string sessionId = iClient.SessionId;
                    Trace.TraceInformation("Closing client: " + sessionId);
                    iClient.Close();
                    Trace.TraceInformation("Closed client: " + sessionId);
                }

                iClient = null;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void CloseThreadSafe()
        {
            if (this.InvokeRequired)
            {
                //Not in the proper thread, invoke ourselves
                CloseDelegate d = new CloseDelegate(CloseThreadSafe);
                this.Invoke(d, new object[] { });
            }
            else
            {
                //We are in the proper thread
                Close();
            }
        }


        private void FormClientTest_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseConnectionThreadSafe();
        }

        public bool IsClientReady()
        {
            return (iClient != null && iClient.IsReady());
        }

        private void buttonAlignLeft_Click(object sender, EventArgs e)
        {
            Alignment = ContentAlignment.MiddleLeft;
            textBoxTop.TextAlign = HorizontalAlignment.Left;
            textBoxBottom.TextAlign = HorizontalAlignment.Left;
        }

        private void buttonAlignCenter_Click(object sender, EventArgs e)
        {
            Alignment = ContentAlignment.MiddleCenter;
            textBoxTop.TextAlign = HorizontalAlignment.Center;
            textBoxBottom.TextAlign = HorizontalAlignment.Center;
        }

        private void buttonAlignRight_Click(object sender, EventArgs e)
        {
            Alignment = ContentAlignment.MiddleRight;
            textBoxTop.TextAlign = HorizontalAlignment.Right;
            textBoxBottom.TextAlign = HorizontalAlignment.Right;
        }

        private void buttonSetTopText_Click(object sender, EventArgs e)
        {
            //TextField top = new TextField(0, textBoxTop.Text, ContentAlignment.MiddleLeft);
            iTextFieldTop.Text = textBoxTop.Text;
            iTextFieldTop.Alignment = Alignment;
            bool res = iClient.SetField(iTextFieldTop);

            if (!res)
            {
                MessageBox.Show("Create you fields first", "Field update error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }

        private void buttonSetText_Click(object sender, EventArgs e)
        {
			SetBasicLayoutAndText();
        }

		void SetBasicLayoutAndText()
		{
			//Set one column two lines layout
			TableLayout layout = new TableLayout(1, 2);
			iClient.SetLayout(layout);

			//Set our fields
			iClient.CreateFields(new DataField[]
            {
                new TextField(textBoxTop.Text, Alignment, 0, 0),
                new TextField(textBoxBottom.Text, Alignment, 0, 1)
            });

		}

        private void buttonLayoutUpdate_Click(object sender, EventArgs e)
        {
            //Define a 2 by 2 layout
            TableLayout layout = new TableLayout(2,2);
            //Second column only takes up 25%
            layout.Columns[1].Width = 25F;
            //Send layout to server
            iClient.SetLayout(layout);

            //
            RecordingField recording = new RecordingField();
            recording.IsActive = true;
            recording.Text = "Recording Tame of Gone until 22:05";
            //Set texts
            iClient.CreateFields(new DataField[]
            {                
                new TextField(textBoxTop.Text, Alignment, 0, 0),
                new TextField(textBoxBottom.Text, Alignment, 0, 1),
                new TextField("Third text field", Alignment, 1, 0),
                new TextField("Forth text field", Alignment, 1, 1),
                recording
            });

        }

        private void buttonSetBitmap_Click(object sender, EventArgs e)
        {
            int x1 = 0;
            int y1 = 0;
            int x2 = 256;
            int y2 = 32;

            Bitmap bitmap = new Bitmap(x2,y2);
            Pen blackPen = new Pen(Color.Black, 3);

            // Draw line to screen.
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.DrawLine(blackPen, x1, y1, x2, y2);
                graphics.DrawLine(blackPen, x1, y2, x2, y1);
            }

            DataField field = new BitmapField(bitmap);
            //field.ColumnSpan = 2;
            iClient.SetField(field);
        }

        private void buttonBitmapLayout_Click(object sender, EventArgs e)
        {
            SetLayoutWithBitmap();
        }

        /// <summary>
        /// Define a layout with a bitmap field on the left and two lines of text on the right.
        /// </summary>
        private void SetLayoutWithBitmap()
        {
            //Define a 2 by 2 layout
            TableLayout layout = new TableLayout(2, 2);
            //First column only takes 25%
            layout.Columns[0].Width = 25F;
            //Second column takes up 75%
            layout.Columns[1].Width = 75F;
            //Send layout to server
            iClient.SetLayout(layout);

            //Set a bitmap for our first field
            int x1 = 0;
            int y1 = 0;
            int x2 = 64;
            int y2 = 64;

            Bitmap bitmap = new Bitmap(x2, y2);
            Pen blackPen = new Pen(Color.Black, 3);

            // Draw line to screen.
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.DrawLine(blackPen, x1, y1, x2, y2);
                graphics.DrawLine(blackPen, x1, y2, x2, y1);
            }

            //Create a bitmap field from the bitmap we just created
            //We want our bitmap field to span across two rows
            BitmapField bitmapField = new BitmapField(bitmap, 0, 0, 1, 2);
            
            //Set texts
            iClient.CreateFields(new DataField[]
            {
                bitmapField,
                new TextField(textBoxTop.Text, Alignment, 1, 0),
                new TextField(textBoxBottom.Text, Alignment, 1, 1)
            });

        }

        private void buttonIndicatorsLayout_Click(object sender, EventArgs e)
        {
            //Define a 2 by 4 layout
            TableLayout layout = new TableLayout(2, 4);
            //First column
            layout.Columns[0].Width = 87.5F;
            //Second column
            layout.Columns[1].Width = 12.5F;
            //Send layout to server
            iClient.SetLayout(layout);

            //Create a bitmap for our indicators field
            int x1 = 0;
            int y1 = 0;
            int x2 = 32;
            int y2 = 16;

            Bitmap bitmap = new Bitmap(x2, y2);
            Pen blackPen = new Pen(Color.Black, 3);

            // Draw line to screen.
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.DrawLine(blackPen, x1, y1, x2, y2);
                graphics.DrawLine(blackPen, x1, y2, x2, y1);
            }

            //Create a bitmap field from the bitmap we just created
            DataField indicator1 = new BitmapField(bitmap, 1, 0);
            //Create a bitmap field from the bitmap we just created
            DataField indicator2 = new BitmapField(bitmap, 1, 1);
            //Create a bitmap field from the bitmap we just created
            DataField indicator3 = new BitmapField(bitmap, 1, 2);
            //Create a bitmap field from the bitmap we just created
            DataField indicator4 = new BitmapField(bitmap, 1, 3);

            //
            TextField textFieldTop = new TextField(textBoxTop.Text, Alignment, 0, 0, 1, 2);
            TextField textFieldBottom = new TextField(textBoxBottom.Text, Alignment, 0, 2, 1, 2);

            //Set texts
            iClient.CreateFields(new DataField[]
            {
                textFieldTop,
                textFieldBottom,
                indicator1,
                indicator2,                
                indicator3,
                indicator4
            });

        }

        private void buttonUpdateTexts_Click(object sender, EventArgs e)
        {

            bool res = iClient.SetFields(new DataField[]
            {
                new TextField(textBoxTop.Text, Alignment,0,0),
                new TextField(textBoxBottom.Text, Alignment,0,1)
            });

            if (!res)
            {
                MessageBox.Show("Create you fields first", "Field update error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

		private void buttonLayoutOneTextField_Click(object sender, EventArgs e)
		{
			//Set one column one line layout
			TableLayout layout = new TableLayout(1, 1);
			iClient.SetLayout(layout);

			//Set our fields
			iClient.CreateFields(new DataField[]
            {
                new TextField(textBoxTop.Text, Alignment)
            });
		}

        private void numericUpDownPriority_ValueChanged(object sender, EventArgs e)
        {
            iClient.SetPriority((uint)numericUpDownPriority.Value);
        }

        private void buttonTriggerEvents_Click(object sender, EventArgs e)
        {
            iClient.TriggerEventsByName(textBoxEventName.Text);
        }

        private void buttonLayoutAudioVisualizer_Click(object sender, EventArgs e)
        {
            SetLayoutAudioVisualizer();
        }

        /// <summary>
        /// Define a layout with a single full screen visualizer.
        /// </summary>
        private void SetLayoutAudioVisualizer()
        {
            //Define layout dimension column by row (x,y)
            TableLayout layout = new TableLayout(1, 1);
            //First column take 100%
            layout.Columns[0].Width = 100F;
            //Send layout to server
            iClient.SetLayout(layout);

            //Create our full screen audio visualizer field 
            AudioVisualizerField field = new AudioVisualizerField();

            //Set fields
            iClient.CreateFields(new DataField[]
            {
                field,
            });

        }

        private void buttonLayoutMultipleAudioVisualizers_Click(object sender, EventArgs e)
        {
            SetLayoutMultipleAudioVisualizers();
        }

        /// <summary>
        /// Define a layout with a single full screen visualizer.
        /// </summary>
        private void SetLayoutMultipleAudioVisualizers()
        {
            //Define layout dimension column by row (x,y)
            TableLayout layout = new TableLayout(2, 2);
            //First column take 100%
            layout.Columns[0].Width = 25F;
            layout.Columns[1].Width = 75F;
            //Send layout to server
            iClient.SetLayout(layout);

            //Create our full screen audio visualizer field 
            AudioVisualizerField field1 = new AudioVisualizerField(0, 0);
            AudioVisualizerField field2 = new AudioVisualizerField(0, 1);
            AudioVisualizerField field3 = new AudioVisualizerField(1, 0);
            AudioVisualizerField field4 = new AudioVisualizerField(1, 1);

            //Set fields
            iClient.CreateFields(new DataField[]
            {
                field1,
                field2,
                field3,
                field4,
            });

        }

    }
}
