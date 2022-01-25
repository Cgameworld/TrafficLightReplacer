using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TrafficManager.UI.SubTools;
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

    }

    [HarmonyPatch(typeof(ToggleTrafficLightsTool))]
    [HarmonyPatch("OnPrimaryClickOverlay")]
    public static class TMPETLToggle
    {

        static void Postfix()
        {
            TrafficLightReplacer.TIntersectionFinder.ModifyNodes();
        }

    }
}
