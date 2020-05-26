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
        public int CurrentPackIndex { get; set; } = 0;
        public string LastLoadedXML { get; set; } = Path.Combine(Path.Combine(DataLocation.localApplicationData, "TLRLocal"), "test.xml");
        public Vector3 ButtonPosition { get; set; } = new Vector3(-9999, -9999, 0);
        public bool LoadTLRLocalFolder { get; set; } = false;
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
