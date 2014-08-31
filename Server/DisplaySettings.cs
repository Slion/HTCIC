﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Xml;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Drawing;

namespace SharpDisplayManager
{
    /// <summary>
    /// Display settings for the specified hardware type
    /// </summary>
    [DataContract]
    public class DisplaySettings
    {
        public DisplaySettings()
        {
            Brightness = 1;
            DisplayType = 0;
            TimerInterval = 150;
            ReverseScreen = false;
            ShowBorders = false;
            FontName = "Microsoft Sans Serif, 9.75pt";
        }

        [DataMember]
        public int Brightness { get; set; }

        /// <summary>
        /// See Display.TMiniDisplayType
        /// </summary>
        [DataMember]
        public int DisplayType { get; set; }

        [DataMember]
        public int TimerInterval { get; set; }

        [DataMember]
        public bool ReverseScreen { get; set; }

        [DataMember]
        public bool ShowBorders { get; set; }

        [DataMember]
        public string FontName { get; set; }

        public Font Font
        {
            get
            {
                FontConverter cvt = new FontConverter();
                Font font = cvt.ConvertFromInvariantString(FontName) as Font;
                return font;
            }

            set
            {
                FontConverter cvt = new FontConverter();
                FontName = cvt.ConvertToInvariantString(value);
            }
        }
    };


    /// <summary>
    /// Contain settings for each of our display type. 
    /// </summary>
    [TypeConverter(typeof(DisplaySettingsConverter))]
    [DataContract]
    public class DisplaysSettings
    {
        public DisplaysSettings()
        {
            Init();
        }

        public void Init()
        {
            if (Displays == null)
            {
                Displays = new List<DisplaySettings>();
            }
        }

        //[DataMember]
        //public int CurrentSettingsIndex { get; set; }

        [DataMember]
        public List<DisplaySettings> Displays { get; set; }

        public override string ToString()
        {
            //Save settings into JSON string
            MemoryStream stream = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(DisplaysSettings));
            ser.WriteObject(stream, this);
            // convert stream to string
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream);
            string text = reader.ReadToEnd();
            return text;
        }
    }

    public class DisplaySettingsConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            else
                return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            string stringValue = value as string;
            if (stringValue != null)
            {   
                //Load settings form JSON string
                byte[] byteArray = Encoding.UTF8.GetBytes(stringValue);
                MemoryStream stream = new MemoryStream(byteArray);
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(DisplaysSettings));
                DisplaysSettings settings = (DisplaysSettings)ser.ReadObject(stream);
                settings.Init();
                return settings;
            }
            else
                return base.ConvertFrom(context, culture, value);
        }
    };


}
