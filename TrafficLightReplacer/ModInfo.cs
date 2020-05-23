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

        public void OnSettingsUI(UIHelperBase helper)
        {
            UIHelperBase featuresGroup = helper.AddGroup("Mod Features");

            featuresGroup.AddCheckbox("Show Pack Creator Helper", TLRModSettings.instance.ShowCreatorTool, sel =>
            {
                TLRModSettings.instance.ShowCreatorTool = sel;
                TLRModSettings.instance.Save();

                TrafficLightReplacePanel.instance.isVisible = true;
                CreatorToolPanel.instance.isVisible = sel;
            });

            featuresGroup.AddCheckbox("Main Button Background", TLRModSettings.instance.EnableButtonBackground, sel =>
            {
                TLRModSettings.instance.EnableButtonBackground = sel;
                TLRModSettings.instance.Save();
                MainButton.instance.size = sel ? new Vector2(46f, 46f) : new Vector2(36f, 36f);
                MainButton.instance.normalBgSprite = sel ? "OptionBase" : null;
                MainButton.instance.normalFgSprite= sel ? "tlr-button-padding" : "tlr-button";
            });

            //helper.AddSpace(15);
            UIHelperBase resetGroup = helper.AddGroup("Reset");
            resetGroup.AddButton("Reset Icon Position", () => {
                MainButton.instance.SetDefaultPosition();
                Debug.Log("Reset Pos Button clicked!");
            });

        }

        public void OnEnabled()
        {
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
            var embedList = new List<string>();
            embedList.Add("default.xml");

            string changedMods = "";
            string copiedxmls = "";

            foreach (PluginInfo mod in Singleton<PluginManager>.instance.GetPluginsInfo())
            {
                if (mod.GetInstances<IUserMod>().Length != 0)
                {
                    if (mod.name == "1812157090")
                    {
                        if (mod.isEnabled)
                        {
                            mod.isEnabled = false;
                            changedMods += ((IUserMod)mod.userModInstance).Name + "\n";
                        }
                       
                        embedList.Add("NL_Lights.xml");
                    }

                }
            }


            Tools.ExtractEmbeddedResource(Path.Combine(DataLocation.localApplicationData, "TLRLocal"), "TrafficLightReplacer.DefaultXMLS", embedList);
           

            foreach (var str in embedList)
            {
                copiedxmls += str + "\n";
            }
            //add xml injected message to dialog
            Tools.ShowErrorWindow("Mod Detected!", "DisabledMods:\n" + changedMods + "\nCopied XMLs:\n" + copiedxmls);

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
                                propGroupProperties.Position = propGroup.m_position;
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
