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
            UIHelperBase featuresGroup = helper.AddGroup(Translation.Instance.GetTranslation(TranslationID.FEATURESGROUPTITLE));

            featuresGroup.AddCheckbox(Translation.Instance.GetTranslation(TranslationID.PACKCREATOROPTION), TLRModSettings.instance.ShowCreatorTool, sel =>
            {
                TLRModSettings.instance.ShowCreatorTool = sel;
                TLRModSettings.instance.Save();

                TrafficLightReplacePanel.instance.isVisible = true;
                CreatorToolPanel.instance.isVisible = sel;
            });

            featuresGroup.AddCheckbox(Translation.Instance.GetTranslation(TranslationID.MAINBACKGROUNDOPTION), TLRModSettings.instance.EnableButtonBackground, sel =>
            {
                TLRModSettings.instance.EnableButtonBackground = sel;
                TLRModSettings.instance.Save();
                MainButton.instance.size = sel ? new Vector2(46f, 46f) : new Vector2(36f, 36f);
                MainButton.instance.normalBgSprite = sel ? "OptionBase" : null;
                MainButton.instance.normalFgSprite = sel ? "tlr-button-padding" : "tlr-button";
            });

            //helper.AddSpace(15);
            var resetmessage = Translation.Instance.GetTranslation(TranslationID.RESETGROUPMESSAGE).Split('*');
            UIHelperBase resetGroup = helper.AddGroup(resetmessage[0]);
            resetGroup.AddButton(resetmessage[0]+resetmessage[1], () => {
                MainButton.instance.SetDefaultPosition();
                Debug.Log(resetmessage[0] + resetmessage[2]);
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


                    if (mod.name == "1550720600")
                    {
                        if (mod.isEnabled)
                        {
                            mod.isEnabled = false;
                        }

                        embedList.Add("NATL_NYCNJ.xml");
                    }

                    if (mod.name == "1535107168")
                    {
                        if (mod.isEnabled)
                        {
                            mod.isEnabled = false;
                        }

                        embedList.Add("NATL_Yellow.xml");
                    }

                    if (mod.name == "1548117573")
                    {
                        if (mod.isEnabled)
                        {
                            mod.isEnabled = false;
                        }

                        embedList.Add("NATL_Grey.xml");
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
        public static bool isMainGame;
        private static MainButton m_mainbutton;

        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode == LoadMode.LoadAsset
                || mode == LoadMode.NewAsset
                || mode == LoadMode.NewMap
                || mode == LoadMode.LoadMap)
            {
                isMainGame = false;
            }
            else
            {
                isMainGame = true;
                TrafficLightReplacePanel.instance.Show();  //initalize UI
                CreatorToolPanel.instance.Show();
                m_mainbutton = UIView.GetAView().AddUIComponent(typeof(MainButton)) as MainButton;
            }
        }
       
    }
}
