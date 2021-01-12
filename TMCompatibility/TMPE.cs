using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TrafficManager.Custom.AI;
using UnityEngine;

namespace TMCompatibility
{
    public class TMPE
    {

        public static void StartHarmony()
        {
            string harmonyId = "cgameworld.trafficlightreplacer.TMPECompat";
            HarmonyInstance harmony;
            harmony = HarmonyInstance.Create(harmonyId);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

        }

        public static void Hello()
        {
            Debug.Log("TMPE custom code works!");
        }
    }

    [HarmonyPatch(typeof(CustomRoadBaseAI))]
    [HarmonyPatch("CustomClickNodeButton")]
    public static class Patch
    {

        static void Postfix()
        {
            Debug.Log("TLR TMPE TL Click PATCH WORKS!!");
        }

    }
}
