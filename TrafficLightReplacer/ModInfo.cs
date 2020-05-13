using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static ColossalFramework.Plugins.PluginManager;

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


            if (UIView.GetAView() != null)
            {
                // when enabled in content manager
                Debug.Log("TLR Enabled!");
                CheckMods();
            }
            else
            {
                // when game first loads if already enabled
                LoadingManager.instance.m_introLoaded += CheckMods;
            }
        }

        private static void CheckMods()
        {
            foreach (PluginInfo mod in Singleton<PluginManager>.instance.GetPluginsInfo())
            {
                if (mod.GetInstances<IUserMod>().Length != 0)
                {
                    if (mod.name == "1812157090" && mod.isEnabled)
                    {
                        mod.isEnabled = false;
                        Tools.ShowErrorWindow("Disabled Mod", ((IUserMod)mod.userModInstance).Name);
                    }


                }
            }

        }
    }
    public class ModLoading : LoadingExtensionBase
    {
        private static MainButton m_mainbutton;

        public override void OnLevelLoaded(LoadMode mode)
        {
            TrafficLightReplacePanel.instance.Show();  //initalize UI
            CreatorToolPanel.instance.Show();

            FillPropGroupCache();

            m_mainbutton = UIView.GetAView().AddUIComponent(typeof(MainButton)) as MainButton;

            string xmlfile1 = Path.Combine(Path.Combine(DataLocation.localApplicationData, "TLRLocal"), "default.xml");
            Replacer.Start(xmlfile1);
        }

        private static void FillPropGroupCache()
        {
            foreach (var prefab in Resources.FindObjectsOfTypeAll<NetInfo>())
            {
                foreach (NetInfo.Lane lane in prefab.m_lanes)
                {
                    if (lane?.m_laneProps?.m_props != null)
                    {
                        foreach (NetLaneProps.Prop propGroup in lane.m_laneProps.m_props)
                        {
                            if (propGroup?.m_finalProp != null)
                            {
                                CachePropItem propGroupProperties = new CachePropItem();
                                propGroupProperties.Angle = propGroup.m_angle;
                                Replacer.propGroupCache.Add(propGroupProperties);
                            }
                        }
                    }
                }
            }

            //Debug.Log("Replacer.propCache.Count: " + Replacer.propGroupCache.Count);
        }
    }
}
