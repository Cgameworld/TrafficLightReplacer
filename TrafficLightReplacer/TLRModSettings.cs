using ColossalFramework.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TrafficLightReplacer
{
    [ConfigurationPath("TLRModSettings.xml")]
    public class TLRModSettings
    {
        public bool ShowCreatorTool { get; set; } = false;
        public bool EnableButtonBackground { get; set; } = false;
        public bool DefaultSideSignalPole { get; set; } = false;
        public Vector3 ButtonPosition { get; set; } = new Vector3(-9999, -9999, 0);
        public bool LoadTLRLocalFolder { get; set; } = false;
        public string LastLoadedXML { get; set; } = "RESOURCE.TrafficLightReplacer.DefaultXMLS.default.xml";
        public int CurrentPackIndex { get; set; } = 0;
        public bool OppositeSideToggle{ get; set; } = false;
        public int SmallLightIndex { get; set; } = -1;
        public int MediumLightIndex { get; set; } = -1;
        public int LargeLightIndex { get; set; } = -1;
        public TransformValues SelectedOffsetValues { get; set; } = new TransformValues()
        {
            Position = new Vector3(0, 0, 0),
            Angle = 0,
            Scale = 100
        };
        public List<string> EmbeddedXMLActive { get; set; }

        //implementation from keallu's mods
        private static TLRModSettings _instance;
        public static TLRModSettings instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Configuration<TLRModSettings>.Load();
                }

                return _instance;
            }
        }

        public void Save()
        {
            Configuration<TLRModSettings>.Save();
        }
    }
}
