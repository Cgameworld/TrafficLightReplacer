using Harmony;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrafficLightReplacer
{
    [HarmonyPatch(typeof(NetLane))]
    [HarmonyPatch("RenderInstance")]
    public class TIntersectionPatch
    {
        public static List<uint> replaceIds = new List<uint>() { 230467, 144045 };
        static void Prefix(uint laneID, ref NetInfo.Lane laneInfo)
        {
            if (replaceIds.Contains(laneID))
            {
                Debug.Log("DelLaneHere!!" + laneID);

                NetLaneProps preProps = laneInfo.m_laneProps;
                int len1 = preProps.m_props.Length;
                for (int i = 0; i < len1; i++)
                {
                    NetLaneProps.Prop prop1 = preProps.m_props[i];
                    if (prop1.m_prop.name == "Traffic Light Pedestrian" || prop1.m_prop.name == "Traffic Light 01")
                    {
                        prop1.m_finalProp = PrefabCollection<PropInfo>.FindLoaded(Tools.BlankProp);
                    }
                }
            }
        }
    }
}
