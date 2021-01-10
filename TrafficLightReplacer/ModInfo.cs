using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.Packaging;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using Harmony;
using ICities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public static Assembly NetSkinCompatAssembly = null;

        public void OnEnabled()
        {
            harmony = HarmonyInstance.Create(harmonyId);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            if (UIView.GetAView() != null)
            {
                // when enabled in content manager
                Debug.Log("TLR Enabled!");
                CheckPacks();
            }
            else
            {
                // when game first loads if already enabled
                LoadingManager.instance.m_introLoaded += CheckPacks;
            }
        }

        public void OnDisabled()
        {
            harmony.UnpatchAll(harmonyId);
            harmony = null;
        }
        public void OnSettingsUI(UIHelperBase helper)
        {
            ModSettingsUI.GenerateMenuSettings(helper);
        }
        private static void CheckPacks()
        {
            var embedList = new List<string>();
            embedList.Add("default.xml");
            embedList.Add("none.xml");

            CheckMods(embedList);
            CheckAssets(embedList);

            embedList = Tools.AddResourcePrefix(embedList);

            TLRModSettings.instance.EmbeddedXMLActive = embedList;

            //detect if network skins 2 is installed
            if (PluginManager.instance.GetPluginsInfo().Any(mod => (
        mod.publishedFileID.AsUInt64 == 1758376843uL ||
        mod.name == "NetworkSkins"
) && mod.isEnabled))
            {
                //TrafficLightReplacer.lib.NetworkSkins2Compatibility.dll
                NetSkinCompatAssembly = LoadEmbeddedAssembly("TrafficLightReplacer.lib.NetworkSkins2Compatibility.dll");
                Type t = NetSkinCompatAssembly.GetType("NetworkSkins2Compatibility.NetworkSkins2");
                MethodInfo m = t.GetMethod("TestCall"); 
                m.Invoke(null, new object[] { });
                Debug.Log("afchanges");
            }
            else
            {
                Debug.Log("ns2 not found!");
            }



            Console.ReadLine();
        }

        public static Assembly LoadEmbeddedAssembly(string resource)
        {
            byte[] ba = null;
            Assembly curAsm = Assembly.GetExecutingAssembly();
            using (Stream stm = curAsm.GetManifestResourceStream(resource))
            {
                ba = new byte[(int)stm.Length];
                stm.Read(ba, 0, (int)stm.Length);

                return Assembly.Load(ba);
            }
        }

        private static void CheckAssets(List<string> embedList)
        {
            var assetList = PackageManager.FilterAssets(UserAssetType.CustomAssetMetaData);
            var foundTLRConfig = false;

            foreach (var asset in assetList)
            {
                var packageName = asset.package.packageName;

                if (packageName == "2032407437")
                {
                    embedList.Add("clus_lights.xml");
                }
                if (packageName == "2084863228")
                {
                    embedList.Add("USRP_Feare.xml");
                }
                if (packageName == "2268192312")
                {
                    embedList.Add("NAF_Greyflame.xml");
                }
                if (packageName == "2236570542")
                {
                    embedList.Add("BIGUrbanLights.xml");
                }

                var packagePath = asset.package.packagePath;
                if (packagePath == null)
                    continue;
                var propDirectory = Directory.GetParent(packagePath);

                if (propDirectory.ToString() != DataLocation.assetsPath)
                {
                    var folderFiles = propDirectory.GetFiles();

                    foreach (var filePath in folderFiles)
                    {
                        var filename = Path.GetFileName(filePath.ToString());
                        if (filename == "TLRConfig.xml")
                        {
                            foundTLRConfig = true;
                        }
                    }
                }
            }

            CheckIfEmpty(embedList, foundTLRConfig);
        }

        private static void CheckIfEmpty(List<string> embedList, bool foundTLRConfig)
        {
            if (embedList.Count == 2 && foundTLRConfig == false)
            {
                Tools.ShowErrorWindow(Translation.Instance.GetTranslation(TranslationID.MAINWINDOW_TITLE), "No traffic light packs installed! Go to the mod listing in the Steam Workshop to find packs to install");
            }
        }

        private static void CheckMods(List<string> embedList)
        {
            var modList = Singleton<PluginManager>.instance.GetPluginsInfo();
            foreach (PluginInfo mod in modList)
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
                    if (mod.name == "1251396095")
                    {
                        if (mod.isEnabled)
                        {
                            mod.isEnabled = false;
                        }

                        embedList.Add("TaiwanTL_ChianMingDang.xml");
                    }

                }
            }
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
