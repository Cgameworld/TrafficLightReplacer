using ColossalFramework.IO;
using ICities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    }
    public class ModLoading : LoadingExtensionBase
    {
        public override void OnLevelLoaded(LoadMode mode)
        {
            TrafficLightReplacePanel.instance.Show();  //initalize UI
            TransformSettingsPanel.instance.Show();
            string xmlfile1 = Path.Combine(DataLocation.addonsPath, "test.xml");
            Replacer.Start(xmlfile1);
            Replacer.GetRoadPropPostions();
            Debug.Log("STARTED LOADED");

        }
    }
}
