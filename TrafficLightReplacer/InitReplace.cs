using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.Packaging;
using Harmony;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TrafficLightReplacer.Locale;
using TrafficLightReplacer.TranslationFramework;
using UnityEngine;

namespace TrafficLightReplacer
{
    public static class InitReplace
    {
        public static void CachePropFill()
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
                                CachePropItem propGroupProperties = new CachePropItem()
                                {
                                    Angle = propGroup.m_angle,
                                    Position = propGroup.m_position,
                                };

                                Replacer.propGroupCache.Add(propGroupProperties);
                            }
                        }
                    }
                }

            }
        }
    }

    //runs customizations after LoadingWrapper instead at OnLevelLoaded
    //need this because of NeXT2 loads inserts extra props after load :<
    [HarmonyPatch(typeof(LoadingWrapper))]
    [HarmonyPatch("OnLevelLoaded")]
    public class Patch
    {
        static void Postfix()
        {
            if (ModLoading.isMainGame)
            {

                //check if LHD (currently not supported)
                if (Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic == SimulationMetaData.MetaBool.True)
                {
                    Tools.ShowErrorWindow(Translation.Instance.GetTranslation(TranslationID.MAINWINDOW_TITLE), "Error: Left Hand Drive mode is currently not supported");
                }

                //look for prop packs to add
                FindActiveEmbeddedPropXMLs();

                //makes tweaks to specfic traffic light meshes if found
                PreloadPropModify();

                //grab initial prop postions
                InitReplace.CachePropFill();

                string xmlfile = TLRModSettings.instance.LastLoadedXML;

                //change dropdown index based on xml 
                List<Pack> packList = Tools.GetPackList();
                for (int i = 0; i < packList.Count; i++)
                {
                    if (packList[i].PackPath == xmlfile)
                    {
                        Debug.Log("i is packindex " + i);
                        TLRModSettings.instance.CurrentPackIndex = i;
                    }
                }

                Replacer.Start(xmlfile);
            }
        }
        private static void FindActiveEmbeddedPropXMLs()
        {
            //mod detection part in CheckMods() in ModInfo.cs
            var propEmbedList = new List<string>();
            for (uint i = 0; i < PrefabCollection<PropInfo>.LoadedCount(); i++)
            {
                var prefab = PrefabCollection<PropInfo>.GetLoaded(i);

                if (prefab == null)
                    continue;

                var asset = PackageManager.FindAssetByName(prefab.name);
                if (asset == null || asset.package == null)
                    continue;

                var crpPath = asset.package.packageName;

                Debug.Log("crppath: " + crpPath);
                if (crpPath == "2032407437")
                {
                    propEmbedList.Add("clus_lights.xml");
                }
                if (crpPath == "2084863228")
                {
                    propEmbedList.Add("USRP_Feare.xml");
                }

            }

            propEmbedList = Tools.AddResourcePrefix(propEmbedList);

            TLRModSettings.instance.EmbeddedXMLActive.AddRange(propEmbedList);
        }
        private static void PreloadPropModify()
        {
            Tools.ModifyPropMeshPreload(PrefabCollection<PropInfo>.FindLoaded("1108278552.HorizontalTrafficLights2_Data"), false);
            Tools.ModifyPropMeshPreload(PrefabCollection<PropInfo>.FindLoaded("1108278552.HorizontalTrafficLights3_Data"), true);
            Tools.ModifyPropMeshPreload(PrefabCollection<PropInfo>.FindLoaded("1108278552.HorizontalTrafficLights4_Data"), true);
        }
    }
}
