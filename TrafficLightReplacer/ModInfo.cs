using ColossalFramework.IO;
using ICities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace TrafficLightReplacer
{
    public class ModInfo : IUserMod
    {
        public string Name
        {
            get { return "Traffic Light Replacer"; }
        }

        public string Description
        {
            get { return "Mod Description"; }
        }

        public void OnEnabled()
        {
            var a = new List<string>();
            a.Add("default.xml");
            Tools.ExtractEmbeddedResource(Path.Combine(DataLocation.localApplicationData, "TLRLocal"), "TrafficLightReplacer.DefaultXMLS", a);
        }
    }
    public class ModLoading : LoadingExtensionBase
    {
        public override void OnLevelLoaded(LoadMode mode)
        {
            TrafficLightReplacePanel.instance.Show();  //initalize UI
            CreatorToolPanel.instance.Show();
            string xmlfile1 = Path.Combine(DataLocation.addonsPath, "default.xml");
            Replacer.Start(xmlfile1);
        }
    }
}
