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

            /* 
           foreach (PluginInfo mod in Singleton<PluginManager>.instance.GetPluginsInfo())
           {
               if (mod.GetInstances<IUserMod>().Length != 0)
               {
                   Debug.Log("modname s: " + mod.name);

                   Debug.Log("bfenabled: " + mod.name + " | " + mod.isEnabled);

                   if (mod.name == "UnlimitedSoil")
                   {
                       mod.isEnabled = true;
                   }

                   Debug.Log("afenabled: " + mod.name + " | " + mod.isEnabled);

               }
           }


                  //doesnt work this early in loading - use harmony to inject slightly later..
                   if (PluginManager.instance.GetPluginsInfo().Any(mod => (
       mod.publishedFileID.AsUInt64 == 1812157090uL ||
       mod.name.Contains("1812157090"))
       && mod.isEnabled)
           )
                   {
                       Debug.Log("Dutch Traffic Lights Subscribed!");
                       //figure out how to turn mod off!

                   }
                   */
        }
    }
    public class ModLoading : LoadingExtensionBase
    {
        private static MainButton m_mainbutton;

        public override void OnLevelLoaded(LoadMode mode)
        {
            TrafficLightReplacePanel.instance.Show();  //initalize UI
            CreatorToolPanel.instance.Show();


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
                                Replacer.propCache.Add(propGroupProperties);
                            }
                        }
                    }
                }
            }

            Debug.Log("Replacer.propCache.Count: " + Replacer.propCache.Count);
            //add propgroup counter!


            m_mainbutton = UIView.GetAView().AddUIComponent(typeof(MainButton)) as MainButton;

            string xmlfile1 = Path.Combine(Path.Combine(DataLocation.localApplicationData, "TLRLocal"), "default.xml");
            Replacer.Start(xmlfile1);
        }
    }
}
