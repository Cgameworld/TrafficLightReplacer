using Harmony;
using NetworkSkins.Skins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NS2Compatibility
{
    //addeds newly added skins ingame to cache list

    [HarmonyPatch(typeof(NetworkSkinManager))]
    [HarmonyPatch("UsageAdded")]
    public static class NetSkinManagerPatch { 
         static void Postfix(NetworkSkin skin)
        {
            var cacheSkinsList = NetworkSkins2.CacheSkins.Keys.ToList();
            Debug.Log("usageadded patchworking!");

            if (skin != null)
            {
                if (!cacheSkinsList.Any(a => a.Equals(skin)))
                {
                    Debug.Log("usageaddedpatch running!");

                    if (skin.m_lanes == null) return;
                    int count1 = 0;
                    for (var l = 0; l < skin.m_lanes.Length; l++)
                    {
                        var laneProps = skin.m_lanes[l]?.m_laneProps?.m_props;
                        if (laneProps == null) continue;

                        for (var p1 = 0; p1 < laneProps.Length; p1++)
                        {
                            if (skin.m_lanes[l].m_laneProps.m_props[p1]?.m_finalProp != null)
                            {
                                count1++;
                            }
                        }
                    }

                    NetworkSkins2.CacheSkins.Add(skin, count1);
                    Debug.Log("UNIQUE! propcount " + count1);
                }


                else
                {
                    Debug.Log("skin already exists!");
                }
            }
            
        }

        

    }



}
