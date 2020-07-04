using ColossalFramework.IO;
using ColossalFramework.Packaging;
using Harmony;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            //look for prop packs to add
            FindActiveEmbeddedPropXMLs();

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

            // catch
            //  {
            //      TLRModSettings.instance.CurrentPackIndex = 0;
            //      string defaultfile = "RESOURCE.TrafficLightReplacer.DefaultXMLS.default.xml";
            //     Replacer.Start(defaultfile);
            //   }
        }

        private static void FindActiveEmbeddedPropXMLs()
        {
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
                    Debug.Log("CLUS traffic Lights!");
                    propEmbedList.Add("clus_lights.xml");
                }

                propEmbedList = Tools.AddResourcePrefix(propEmbedList);

                TLRModSettings.instance.EmbeddedXMLActive.AddRange(propEmbedList);

            }
        }
    }
}
