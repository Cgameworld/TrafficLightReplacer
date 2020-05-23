using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using Harmony;
using ICities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using static ColossalFramework.Plugins.PluginManager;

namespace TrafficLightReplacer
{
    public class ModInfo : IUserMod
    {
        private readonly string harmonyId = "cgameworld.trafficlightreplacer";
        private HarmonyInstance harmony;

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
            harmony = HarmonyInstance.Create(harmonyId);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

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

        public void OnDisabled()
        {
            harmony.UnpatchAll(harmonyId);
            harmony = null;
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
                MainButton.instance.normalFgSprite = sel ? "tlr-button-padding" : "tlr-button";
            });

            //helper.AddSpace(15);
            UIHelperBase resetGroup = helper.AddGroup("Reset");
            resetGroup.AddButton("Reset Icon Position", () => {
                MainButton.instance.SetDefaultPosition();
                Debug.Log("Reset Pos Button clicked!");
            });

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
            m_mainbutton = UIView.GetAView().AddUIComponent(typeof(MainButton)) as MainButton;
        }
       
    }
}
