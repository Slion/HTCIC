﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Windows.Forms;
using Ear = SharpLib.Ear;
using Hid = SharpLib.Hid;

namespace SharpDisplayManager
{
    [DataContract]
    [Ear.AttributeObject(Id = "Event.Hid", Name = "HID", Description = "Handle input from Keyboards and Remotes.")]
    public class EventHid: Ear.Event
    {
        public EventHid()
        {
        }

        [DataMember]
        public ushort UsagePage { get; set; }

        [DataMember]
        public ushort UsageCollection { get; set; }

        [DataMember]
        public ushort Usage { get; set; }

        [DataMember]
        public Keys Key { get; set; }

        [DataMember]
        [Ear.AttributeObjectProperty
            (
            Id = "HID.Keyboard.IsKeyUp",
            Name = "Key Up",
            Description = "Key up if set, key down otherwise."
            )]
        public bool IsKeyUp { get; set; } = false;

        [DataMember]
        public bool IsMouse { get; set; } = false;

        [DataMember]
        public bool IsKeyboard { get; set; } = false;

        [DataMember]
        public bool IsGeneric { get; set; } = false;

        [DataMember]
        public bool HasModifierShift { get; set; } = false;

        [DataMember]
        public bool HasModifierControl { get; set; } = false;

        [DataMember]
        public bool HasModifierAlt { get; set; } = false;

        [DataMember]
        public bool HasModifierWindows { get; set; } = false;

        [DataMember]
        public string UsageName { get; set; } = "Press a key";



        protected override void DoConstruct()
        {
            base.DoConstruct();
            UpdateDynamicProperties();
        }

        private void UpdateDynamicProperties()
        {
            
        }

        /// <summary>
        /// Make sure we distinguish between various configuration of this event 
        /// </summary>
        /// <returns></returns>
        public override string Brief()
        {
            string brief = AttributeName + ": ";

            if (!IsValid())
            {
                brief += "Press a key";
                return brief;
            }

            if (IsKeyboard)
            {
                brief += Key.ToString();

                if (HasModifierAlt)
                {
                    brief += " + ALT";
                }

                if (HasModifierControl)
                {
                    brief += " + CTRL";
                }

                if (HasModifierShift)
                {
                    brief += " + SHIFT";
                }

                if (HasModifierWindows)
                {
                    brief += " + WIN";
                }
            }
            else if (IsGeneric)
            {
                brief += UsageName;
            }

            if (IsKeyUp)
            {
                brief += " (UP)";
            }
            else
            {
                brief += " (DOWN)";
            }

            return brief;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Matches(object obj)
        {
            if (obj is EventHid)
            {
                EventHid e = (EventHid)obj;
                return e.Key == Key
                    && e.Usage == Usage
                    && e.UsagePage == UsagePage
                    && e.UsageCollection == UsageCollection
                    && e.IsKeyUp == IsKeyUp
                    && e.IsGeneric == IsGeneric
                    && e.IsKeyboard == IsKeyboard
                    && e.IsMouse == IsMouse
                    && e.HasModifierAlt == HasModifierAlt
                    && e.HasModifierControl == HasModifierControl
                    && e.HasModifierShift == HasModifierShift
                    && e.HasModifierWindows == HasModifierWindows;
            }

            return false;
        }


        /// <summary>
        /// 
        /// </summary>
        protected override void OnStateLeave()
        {
            if (CurrentState == State.Edit)
            {
                // Leaving edit mode
                // Unhook HID events
                Program.HidHandler.OnHidEvent -= HandleHidEvent;

            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnStateEnter()
        {
            if (CurrentState == State.Edit)
            {
                // Enter edit mode
                // Hook-in HID events
                Program.HidHandler.OnHidEvent += HandleHidEvent;

            }
        }

        /// <summary>
        /// Here we receive HID events from our HID library.
        /// </summary>
        /// <param name="aSender"></param>
        /// <param name="aHidEvent"></param>
        public void HandleHidEvent(object aSender, SharpLib.Hid.Event aHidEvent)
        {
            if (CurrentState != State.Edit
                || aHidEvent.IsMouse
                || aHidEvent.IsButtonUp
                || !aHidEvent.IsValid
                || aHidEvent.IsBackground
                || aHidEvent.IsRepeat
                || aHidEvent.IsStray)
            {
                return;
            }

            PrivateCopy(aHidEvent);
            //

            //Tell observer the object itself changed
            OnPropertyChanged("Brief");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aHidEvent"></param>
        public void Copy(Hid.Event aHidEvent)
        {
            PrivateCopy(aHidEvent);
            //We need the key up/down too here
            IsKeyUp = aHidEvent.IsButtonUp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aHidEvent"></param>
        private void PrivateCopy(Hid.Event aHidEvent)
        {
            //Copy for scan
            UsagePage = aHidEvent.UsagePage;
            UsageCollection = aHidEvent.UsageCollection;
            IsGeneric = aHidEvent.IsGeneric;
            IsKeyboard = aHidEvent.IsKeyboard;
            IsMouse = aHidEvent.IsMouse;

            if (IsGeneric)
            {
                if (aHidEvent.Usages.Count > 0)
                {
                    Usage = aHidEvent.Usages[0];
                    UsageName = aHidEvent.UsageName(0);
                }

                Key = Keys.None;
                HasModifierAlt = false;
                HasModifierControl = false;
                HasModifierShift = false;
                HasModifierWindows = false;
            }
            else if (IsKeyboard)
            {
                Usage = 0;
                Key = aHidEvent.VirtualKey;
                HasModifierAlt = aHidEvent.HasModifierAlt;
                HasModifierControl = aHidEvent.HasModifierControl;
                HasModifierShift = aHidEvent.HasModifierShift;
                HasModifierWindows = aHidEvent.HasModifierWindows;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool IsValid()
        {
            return IsGeneric || IsKeyboard;
        }
    }
}
