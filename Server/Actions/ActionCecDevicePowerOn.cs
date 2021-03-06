﻿using CecSharp;
using SharpLib.Ear;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SharpDisplayManager
{
    [DataContract]
    [AttributeObject(Id = "Cec.Device.PowerOn", Name = "CEC Device Power On", Description = "Turns on the specified CEC device on your HDMI bus.")]
    public class ActionCecDevicePowerOn : ActionCecDevice
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string BriefBase()
        {
            return "CEC Power On " + DeviceName;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override async Task DoExecute(Context aContext)
        {
            if (Cec.Client.Static == null)
            {
                Trace.WriteLine("WARNING: No CEC client installed.");
                return;
            }

            Cec.Client.Static.Lib.PowerOnDevices(Device);
        }
    }
}
