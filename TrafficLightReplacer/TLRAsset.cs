using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrafficLightReplacer
{
    public class TLRConfig
    {
        public string PackName { get; set; }
        public bool RemoveMirrorLights { get; set; }
        public bool OneSize { get; set; }
        public List<Asset> Assets { get; set; }

    }
    public class Asset
    {
        public string Prefab { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
    }

}