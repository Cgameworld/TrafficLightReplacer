using NetworkSkins.Skins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;

namespace TrafficLightReplacer.Compatibility
{
    public class NetworkSkins2
    {
        public static ObjectIDGenerator idgen = new ObjectIDGenerator();
        public static List<int> propCount = new List<int>();

        public static void AddInitProps()
        {
            var skins = NetworkSkinManager.instance.AppliedSkins;

            for (int i = 0; i < skins.Count; i++)
            {
                var skin = skins[i];
                int propAmount = 0;

                long idnum = idgen.GetId(skin, out _);
                Debug.Log("loadskinid: " + idnum);

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

                            propAmount++;
                        }
                    }
                }

                propCount.Add(propAmount);

            }

        }

        public static void ReplaceNS2Props()
        {
            var skins = NetworkSkinManager.instance.AppliedSkins;

            List<long> remainingskinIDs = new List<long>();

            for (int i = 0; i < skins.Count; i++)
            {
                var skin = skins[i];

                bool exclude;
                long number = idgen.HasId(skin, out exclude);
                Debug.Log("skin " + number + " is " + exclude);
                if (!exclude)
                {
                    remainingskinIDs.Add(number);
                }
            }

            foreach (var item in remainingskinIDs)
            {
                Debug.Log("remaining " + item);
            }


           /*
            for (int i = 0; i < skins.Count; i++)
            {
                var skin = skins[i];



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
                            Debug.Log("skipped " + number);
                            Replacer.propGroupCounter++;
                        }
                    }
                }
            }

            */
        }


    }
}
