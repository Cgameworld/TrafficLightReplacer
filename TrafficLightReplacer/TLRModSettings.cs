using System;
using System.Collections.Generic;
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
        public Vector3 ButtonPosition { get; set; } = new Vector3(-9999, -9999, 0);

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
