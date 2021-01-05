using NetworkSkins.Skins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TrafficLightReplacer.Compatibility
{
    public class NetworkSkins2
    {
        //public static List<NetworkSkin> CacheSkins = new List<NetworkSkin>();
        public static List<object> CacheSkins = new List<object>();
        public static void AddInitProps()
        {
            int netskinpropamount = 0;
            var skins = NetworkSkinManager.instance.AppliedSkins;

            for (int i = 0; i < skins.Count; i++)
            {
                var skin = skins[i];
                CacheSkins.Add(skin);

                if (skin.m_lanes == null) return;

                for (var l = 0; l < skin.m_lanes.Length; l++)
                {
                    var laneProps = skin.m_lanes[l]?.m_laneProps?.m_props;
                    if (laneProps == null) continue;

                    for (var p1 = 0; p1 < laneProps.Length; p1++)
                    {
                        if (skin.m_lanes[l].m_laneProps.m_props[p1]?.m_finalProp != null)
                        {
                            CachePropItem propGroupProperties = new CachePropItem()
                            {
                                Angle = skin.m_lanes[l].m_laneProps.m_props[p1].m_angle,
                                Position = skin.m_lanes[l].m_laneProps.m_props[p1].m_position,
                            };

                            Replacer.propGroupCache.Add(propGroupProperties);

                            netskinpropamount++;
                        }
                    }
                }
            }

        }

        public static void ReplaceNS2Props()
        {
            var skins = CacheSkins;
            var currentskins = NetworkSkinManager.instance.AppliedSkins;

            List<NetworkSkin> excludedskins = skins.Except(currentskins).ToList();

            for (int i = 0; i < skins.Count; i++)
            {
                var skin = skins[i];

                //check if loaded skin still exists
                bool exclude = excludedskins.Any(a => a.Equals(skin));

                Debug.Log("REPLACERSKIN hash: " + skin.GetHashCode());

                var prefab = skin.Prefab;
                float roadwidth = 0;
                bool isOneWay = false;
                bool isHighway = false;
                Replacer.GetRoadInformation(prefab, ref roadwidth, ref isOneWay);

                if (skin.m_lanes == null) return;

                for (var l = 0; l < skin.m_lanes.Length; l++)
                {
                    var laneProps = skin.m_lanes[l]?.m_laneProps?.m_props;
                    if (laneProps == null) continue;

                    for (var p1 = 0; p1 < laneProps.Length; p1++)
                    {
                        if (!exclude)
                        {
                            Replacer.CategoryReplacement(roadwidth, isOneWay, isHighway, skin.m_lanes[l], skin.m_lanes[l].m_laneProps.m_props[p1]);
                        }
                        else
                        {
                            Debug.Log(skin.GetHashCode() + " excluded");
                            Replacer.propGroupCounter++;
                        }
                    }
                }
            }


            foreach (var k in currentskins)
            {
                Debug.Log("CurrentSKIN hash: " + k.GetHashCode());
            }
        }


    }
}
