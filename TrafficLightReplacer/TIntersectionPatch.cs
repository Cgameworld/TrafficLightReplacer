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
        public static List<uint> replaceIds = new List<uint>() {};
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

            //figure out flipped/non flipped lights!, get regular prop argument since this just reads/changes the final prop!
            //also figure out the significant lag when dragging nodes

            if (replaceIds.Contains(laneID))
            {
                if (prop != null)
                {
                    if (prop.name == "Traffic Light 02" || prop.name == "Traffic Light 02 European")
                    {
                        prop = PrefabCollection<PropInfo>.FindLoaded("Air Source Heat Pump 02");
                    }
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
            if (ModLoading.isMainGame)
            {
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

                List<Vector3> dirs = new List<Vector3>();
                List<ushort> segids = new List<ushort>();

                if (node.CountSegments() == 3 && node.m_flags.IsFlagSet(NetNode.Flags.TrafficLights))
                {
                    ushort foundSegment = 0;

                    foreach (var segmentID in neighborSegmentIds)
                    {
                        var segment = NetManager.instance.m_segments.m_buffer[segmentID];
                        var segmentForward = false;

                        if (segmentID == 0) segmentForward = true;

                        if (!segmentForward)
                        {
                            var dirtoNode = NetManager.instance.m_segments.m_buffer[segmentID].GetDirection(i);
                            //Debug.Log("seg:" + segmentID + " | to node: " + nodeID + " | dir " + dirtoNode);
                            dirs.Add(dirtoNode);
                            segids.Add(segmentID);
                        }

                    }

                    //angle correction!

                    List<float> angles = new List<float>();

                    angles.Add(Vector3.Angle(dirs[0], dirs[1]));
                    angles.Add(Vector3.Angle(dirs[1], dirs[2]));
                    angles.Add(Vector3.Angle(dirs[2], dirs[0]));

                    int locationofMax = angles.IndexOf(angles.Max());
                    Debug.Log("locationofMax" + locationofMax);
                    angles[locationofMax] = 360 - angles[locationofMax];

                    Debug.Log("Corr0-1: " + angles[0]);
                    Debug.Log("Corr1-2: " + angles[1]);
                    Debug.Log("Corr2-0: " + angles[2]);


                    switch (locationofMax)
                    {
                        case 0:
                            foundSegment = segids[2];
                            break;
                        case 1:
                            foundSegment = segids[0];
                            break;
                        case 2:
                            foundSegment = segids[1];
                            break;
                        default:
                            foundSegment = 0;
                            Tools.ShowErrorWindow("ERROR", "segid error");
                            break;
                    }


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

            }

            // Debug.Log("idnodes count: " + idCount);
            TIntersectionPatch.replaceIds = foundIds;
        }
    }
}
