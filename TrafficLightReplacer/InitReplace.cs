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
            int count = PrefabCollection<NetInfo>.LoadedCount();

            for (uint index = 0; index < count; index++)
            {
                var prefab = PrefabCollection<NetInfo>.GetLoaded(index);

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

                //makes compatibility tweaks to specfic traffic light meshes if found
                PreloadPropModify();

                //makes compatibility tweaks to specfic road meshes if found
                PreloadRoadModify();

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
                if (crpPath == "2268192312")
                {
                    propEmbedList.Add("NAF_Greyflame.xml");
                }
                if (crpPath == "2236570542")
                {
                    propEmbedList.Add("BIGUrbanLights.xml");
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
            Tools.ModifyPropMeshPreload(PrefabCollection<PropInfo>.FindLoaded("1251396095.TWTL_TYPE3_Ped_Data"), true);
        }
        public static void PreloadRoadModify()
        {
            //badpeanut 4+1 asym roads compatibility hack
            //replace inner lane manhole covers with ped light props
            ReplaceBPAsymNet(PrefabCollection<NetInfo>.FindLoaded("1205312481.4+1 Lane Asymmetric Road_Data"));
            ReplaceBPAsymNet(PrefabCollection<NetInfo>.FindLoaded("1205312481.4+1 Lane Asymmetric Road w/Grass_Data"));
            ReplaceBPAsymNet(PrefabCollection<NetInfo>.FindLoaded("1205312481.4+1 Lane Asymmetric Road w/Trees_Data"));
        }

        private static void ReplaceBPAsymNet(NetInfo network)
        {
            if (network != null) { 
            foreach (NetInfo.Lane lane in network.m_lanes)
            {
                    if (lane?.m_laneProps?.m_props != null)
                    {
                        if (lane.m_laneType.ToString() == "Vehicle")
                        {
                            Debug.Log("A     vehdete");

                            if (lane.m_position == -4.7f)
                            {
                                var manhole1 = lane.m_laneProps.m_props[7];
                                manhole1.m_prop = PrefabCollection<PropInfo>.FindLoaded("Traffic Light Pedestrian");
                                // manhole1.m_finalProp = PrefabCollection<PropInfo>.FindLoaded("pedlight3.pedlight3_Data");
                                manhole1.m_probability = 100;
                                manhole1.m_segmentOffset = 1;
                                manhole1.m_angle = 90f;
                                manhole1.m_position = new Vector3(-15f, 0f, 0f);
                                manhole1.m_flagsForbidden = NetLane.Flags.Inverted | NetLane.Flags.JoinedJunction;
                                manhole1.m_endFlagsRequired = NetNode.Flags.TrafficLights;
                                manhole1.m_endFlagsForbidden = NetNode.Flags.LevelCrossing;
                                manhole1.m_colorMode = NetLaneProps.ColorMode.StartState;

                                lane.m_laneProps.m_props[7] = manhole1;
                            }

                            if (lane.m_position == 1.7f)
                            {
                                var manhole2 = lane.m_laneProps.m_props[7];
                                manhole2.m_prop = PrefabCollection<PropInfo>.FindLoaded("Traffic Light Pedestrian");
                                // manhole1.m_finalProp = PrefabCollection<PropInfo>.FindLoaded("pedlight2.pedlight2_Data");
                                manhole2.m_probability = 100;
                                manhole2.m_segmentOffset = 1;
                                manhole2.m_angle = 90f;
                                manhole2.m_position = new Vector3(-12f, 0f, 0f);
                                manhole2.m_flagsForbidden = NetLane.Flags.Inverted | NetLane.Flags.JoinedJunction;
                                manhole2.m_endFlagsRequired = NetNode.Flags.TrafficLights;
                                manhole2.m_endFlagsForbidden = NetNode.Flags.LevelCrossing;
                                manhole2.m_colorMode = NetLaneProps.ColorMode.EndState;

                                lane.m_laneProps.m_props[7] = manhole2;
                            }

                        }
                    }

                }
            }
        }
    }
}
