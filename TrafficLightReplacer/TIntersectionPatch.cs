using ColossalFramework;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace TrafficLightReplacer
{
    [HarmonyPatch(typeof(NetLane))]
    [HarmonyPatch("RenderInstance")]
    public static class TIntersectionPatch
    {
        public static List<uint> replaceIds = new List<uint>() {0 };
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = new List<CodeInstruction>(instructions);

            var fixedInstructions = new[]
            {
                new CodeInstruction(OpCodes.Ldarg_3),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TIntersectionPatch), nameof(GetFinalProp))),
            };

            for (int i = 0; i < codes.Count; i++)
            {
                //checking if object is null before converting it to string
                var operandString = codes[i].operand?.ToString() ?? String.Empty;

                if (operandString == "PropInfo m_finalProp")
                {
                    codes.InsertRange(i + 1, fixedInstructions);
                }
            }
            return codes.AsEnumerable();
        }

        // NetLane
        public static PropInfo GetFinalProp(PropInfo prop, uint laneID)
        {
            //Debug.Log("98Transpilation Worked!");

            //figure out flipped/non flipped lights!

            if (replaceIds.Contains(laneID))
            {
                //Debug.Log("LaneHere!!" + laneID);
                if (prop.name == "Traffic Light Pedestrian" || prop.name == "Traffic Light 01")
                {
                    Debug.Log("prophere");
                   prop = PrefabCollection<PropInfo>.FindLoaded("Air Source Heat Pump 02");
                }
            }

            return prop;
        }
    }


    [HarmonyPatch(typeof(NetManager))]
    [HarmonyPatch("UpdateSegmentRenderer")]
    public class TIntersectionFinder
    {
        static void Postfix()
        {
            if (ModInfo.enableTProcess)
            {
                Debug.Log("3Segment Updated!!!");
                ModifyNodes();
            }
        }


        public static void ModifyNodes()
        {
            var bufferLength = (ushort)NetManager.instance.m_nodes.m_buffer.Length;
            int idCount = 0;
            List<uint> foundIds = new List<uint>();

            for (ushort i = 0; i < bufferLength; i++)
            {
                var node = NetManager.instance.m_nodes.m_buffer[i];
                if (node.Info == null)
                {
                    continue;
                }

                //find t-intersection (find if node has exactly 3 intersecting roads)

                List<ushort> neighborSegmentIds = new List<ushort>
                {
                    node.m_segment0,
                    node.m_segment1,
                    node.m_segment2,
                    node.m_segment3,
                    node.m_segment4,
                    node.m_segment5,
                    node.m_segment6,
                    node.m_segment7
                };

                int intersectingRoads = 0;
                foreach (var id in neighborSegmentIds)
                {
                    if (id != 0)
                    {
                        intersectingRoads++;
                    }
                }

                if (intersectingRoads == 3 && node.m_flags.IsFlagSet(NetNode.Flags.TrafficLights))
                {
                    //Debug.Log("node " + i + " is a T-intersection with traffic lights!");
                    ushort foundSegment = 0;

                    foreach (var segmentID in neighborSegmentIds)
                    {
                        //Debug.LogWarning("\nsegmentID: " + segmentID);
                        var segment = NetManager.instance.m_segments.m_buffer[segmentID];

                        var laneID = segment.m_lanes;
                        var segmentForward = false;

                        while (laneID != 0)
                        {
                            NetLane.Flags flags = (NetLane.Flags)NetManager.instance.m_lanes.m_buffer[laneID].m_flags;
                            // Debug.Log("flags of lane " + laneID + " | " + flags.ToString());
                            if (flags.IsFlagSet(NetLane.Flags.Forward))
                            {
                                segmentForward = true;
                            }
                            laneID = NetManager.instance.m_lanes.m_buffer[laneID].m_nextLane;
                        }

                        if (segmentID == 0) segmentForward = true;
                        //Debug.Log("Segment Forward? : " + segmentForward);

                        if (!segmentForward)
                        {
                            foundSegment = segmentID;
                        }
                    }

                    Debug.Log("intersecting road:" + foundSegment);

                    //add lane ids of intersecting segment!
                    if (foundSegment != 0)
                    {
                        var segment = NetManager.instance.m_segments.m_buffer[foundSegment];
                        var laneID = segment.m_lanes;
                        while (laneID != 0)
                        {

                            foundIds.Add(laneID);
                            laneID = NetManager.instance.m_lanes.m_buffer[laneID].m_nextLane;
                        }
                       
                    }

                    }
                idCount++;
            }

            // Debug.Log("idnodes count: " + idCount);
            TIntersectionPatch.replaceIds = foundIds;
        }
    }
}
