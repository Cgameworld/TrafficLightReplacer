using ColossalFramework.IO;
using ColossalFramework.UI;
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
            var embedList = new List<string>();
            embedList.Add("default.xml");
            Tools.ExtractEmbeddedResource(Path.Combine(DataLocation.localApplicationData, "TLRLocal"), "TrafficLightReplacer.DefaultXMLS", embedList);
        }
    }
    public class ModLoading : LoadingExtensionBase
    {
        private static UIMainButton m_mainbutton;

        public override void OnLevelLoaded(LoadMode mode)
        {
            TrafficLightReplacePanel.instance.Show();  //initalize UI
            CreatorToolPanel.instance.Show();

            m_mainbutton = UIView.GetAView().AddUIComponent(typeof(UIMainButton)) as UIMainButton;

            string xmlfile1 = Path.Combine(Path.Combine(DataLocation.localApplicationData, "TLRLocal"), "default.xml");
            Replacer.Start(xmlfile1);
        }
    }
}
