using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrafficLightReplacer
{
    [ConfigurationPath("TLRModSettings.xml")]
    public class TLRModSettings
    {

        public bool ShowCreatorTool { get; set; } = false;

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
