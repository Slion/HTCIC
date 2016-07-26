﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpLib.Display;
using SharpLib.Ear;
using System.Reflection;

namespace SharpDisplayManager
{
    /// <summary>
    /// Action edit dialog form.
    /// </summary>
    public partial class FormEditAction : Form
    {
        public SharpLib.Ear.Action Action = null;

        public FormEditAction()
        {
            InitializeComponent();
        }

        private void FormEditAction_Load(object sender, EventArgs e)
        {
            //Populate registered actions
            foreach (string key in ManagerEventAction.Current.ActionTypes.Keys)
            {
                ItemActionType item = new ItemActionType(ManagerEventAction.Current.ActionTypes[key]);
                comboBoxActionType.Items.Add(item);
            }

            comboBoxActionType.SelectedIndex = 0;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            FetchPropertiesValue(Action);
        }

        private void FormEditAction_Validating(object sender, CancelEventArgs e)
        {

        }

        private void comboBoxActionType_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Instantiate an action corresponding to our type
            Action = (SharpLib.Ear.Action)Activator.CreateInstance(((ItemActionType)comboBoxActionType.SelectedItem).Type);

            //Create input fields
            UpdateTableLayoutPanel(Action);
        }


        /// <summary>
        /// Get properties values from our generated input fields
        /// </summary>
        private void FetchPropertiesValue(SharpLib.Ear.Action aAction)
        {
            int ctrlIndex = 0;
            foreach (PropertyInfo pi in aAction.GetType().GetProperties())
            {
                AttributeActionProperty[] attributes =
                    ((AttributeActionProperty[]) pi.GetCustomAttributes(typeof(AttributeActionProperty), true));
                if (attributes.Length != 1)
                {
                    continue;
                }

                AttributeActionProperty attribute = attributes[0];

                if (!IsPropertyTypeSupported(pi))
                {
                    continue;
                }

                GetPropertyValueFromControl(iTableLayoutPanel.Controls[ctrlIndex+1], pi, aAction); //+1 otherwise we get the label

                ctrlIndex+=2; //Jump over the label too
            }
        }

        /// <summary>
        /// Extend this function to support reading new types of properties.
        /// </summary>
        /// <param name="aAction"></param>
        private void GetPropertyValueFromControl(Control aControl, PropertyInfo aInfo, SharpLib.Ear.Action aAction)
        {
            if (aInfo.PropertyType == typeof(int))
            {
                NumericUpDown ctrl=(NumericUpDown)aControl;
                aInfo.SetValue(aAction,(int)ctrl.Value);
            }
            else if (aInfo.PropertyType.IsEnum)
            {
                // Instantiate our enum
                object enumValue= Activator.CreateInstance(aInfo.PropertyType);
                // Parse our enum from combo box
                //enumValue = Enum.Parse(aInfo.PropertyType,((ComboBox)aControl).SelectedValue.ToString());
                enumValue = ((ComboBox)aControl).SelectedValue;
                // Set enum value
                aInfo.SetValue(aAction, enumValue);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aInfo"></param>
        /// <param name="action"></param>
        private Control CreateControlForProperty(PropertyInfo aInfo, AttributeActionProperty aAttribute, SharpLib.Ear.Action aAction)
        {
            if (aInfo.PropertyType == typeof(int))
            {
                //Integer properties are using numeric editor
                NumericUpDown ctrl = new NumericUpDown();                
                ctrl.Minimum = Int32.Parse(aAttribute.Minimum);
                ctrl.Maximum = Int32.Parse(aAttribute.Maximum);
                ctrl.Increment = Int32.Parse(aAttribute.Increment);
                ctrl.Value = (int)aInfo.GetValue(aAction);
                return ctrl;
            }
            else if (aInfo.PropertyType.IsEnum)
            {
                //Enum properties are using combo box
                ComboBox ctrl = new ComboBox();
                ctrl.DropDownStyle = ComboBoxStyle.DropDownList;
                ctrl.DataSource = Enum.GetValues(aInfo.PropertyType);
                return ctrl;
            }

            return null;
        }

        /// <summary>
        /// Don't forget to extend that one and adding types
        /// </summary>
        /// <returns></returns>
        private bool IsPropertyTypeSupported(PropertyInfo aInfo)
        {
            if (aInfo.PropertyType == typeof(int))
            {
                return true;
            }
            else if (aInfo.PropertyType.IsEnum)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Update our table layout.
        /// Will instantiated every field control as defined by our action.
        /// Fields must be specified by rows from the left.
        /// </summary>
        /// <param name="aLayout"></param>
        private void UpdateTableLayoutPanel(SharpLib.Ear.Action aAction)
        {
            toolTip.RemoveAll();
            //Debug.Print("UpdateTableLayoutPanel")
            //First clean our current panel
            iTableLayoutPanel.Controls.Clear();
            iTableLayoutPanel.RowStyles.Clear();
            iTableLayoutPanel.ColumnStyles.Clear();
            iTableLayoutPanel.RowCount = 0;

            //We always want two columns: one for label and one for the field
            iTableLayoutPanel.ColumnCount = 2;
            iTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            iTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));


            if (aAction == null)
            {
                //Just drop it
                return;
            }
            
            //IEnumerable<PropertyInfo> properties = aAction.GetType().GetProperties().Where(
            //    prop => Attribute.IsDefined(prop, typeof(AttributeActionProperty)));


            foreach (PropertyInfo pi in aAction.GetType().GetProperties())
            {
                AttributeActionProperty[] attributes = ((AttributeActionProperty[])pi.GetCustomAttributes(typeof(AttributeActionProperty), true));
                if (attributes.Length != 1)
                {
                    continue;
                }

                AttributeActionProperty attribute = attributes[0];

                //Before anything we need to check if that kind of property is supported by our UI
                //Create the editor
                Control ctrl = CreateControlForProperty(pi, attribute, aAction);
                if (ctrl == null)
                {
                    //Property type not supported
                    continue;
                }

                //Add a new row
                iTableLayoutPanel.RowCount++;
                iTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                //Create the label
                Label label = new Label();
                label.Text = attribute.Name;
                toolTip.SetToolTip(label, attribute.Description);
                iTableLayoutPanel.Controls.Add(label, 0, iTableLayoutPanel.RowCount-1);

                //Add our editor to our form
                iTableLayoutPanel.Controls.Add(ctrl, 1, iTableLayoutPanel.RowCount - 1);


            }        

        }

    }
}
