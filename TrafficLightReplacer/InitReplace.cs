using ColossalFramework.IO;
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
   

            try
            {
                Replacer.Start(xmlfile);
            }
            catch
            {
                TLRModSettings.instance.CurrentPackIndex = 0;
                string defaultfile = "RESOURCE.TrafficLightReplacer.DefaultXMLS.default.xml";
                Replacer.Start(defaultfile);
            }
        }
    }
}
