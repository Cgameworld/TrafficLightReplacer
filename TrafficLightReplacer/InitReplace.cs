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
                if (prefab.m_vehicleTypes == VehicleInfo.VehicleType.Car)
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

            string xmlfile1 = Path.Combine(Path.Combine(DataLocation.localApplicationData, "TLRLocal"), "default.xml");
            Replacer.Start(xmlfile1);
        }
    }
}
