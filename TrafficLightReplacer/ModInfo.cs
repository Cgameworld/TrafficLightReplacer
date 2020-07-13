using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.Packaging;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using Harmony;
using ICities;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TrafficLightReplacer.Locale;
using TrafficLightReplacer.TranslationFramework;
using UnityEngine;
using static ColossalFramework.Plugins.PluginManager;

namespace TrafficLightReplacer
{
    public class ModInfo : IUserMod
    {
        private readonly string harmonyId = "cgameworld.trafficlightreplacer";
        private HarmonyInstance harmony;

        public string Name => "Traffic Light Replacer";

        public string Description => Translation.Instance.GetTranslation(TranslationID.MOD_DESCRIPTION);

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
            embedList.Add("none.xml");

            foreach (PluginInfo mod in Singleton<PluginManager>.instance.GetPluginsInfo())
            {
                if (mod.GetInstances<IUserMod>().Length != 0)
                {
                    if (mod.name == "1812157090")
                    {
                        if (mod.isEnabled)
                        {
                            mod.isEnabled = false;
                        }

                        embedList.Add("NL_Lights.xml");
                    }

                    if (mod.name == "694123443")
                    {
                        if (mod.isEnabled)
                        {
                            mod.isEnabled = false;
                        }

                        embedList.Add("BP_American.xml");
                    }

                    if (mod.name == "1108278552")
                    {
                        if (mod.isEnabled)
                        {
                            mod.isEnabled = false;
                        }

                        embedList.Add("USHorizontal_Cristolisto.xml");
                    }

                }
            }

            embedList = Tools.AddResourcePrefix(embedList);

            TLRModSettings.instance.EmbeddedXMLActive = embedList;

            //prop pack dectection in FindActiveEmbeddedPropXMLs() in InitReplace.cs
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
