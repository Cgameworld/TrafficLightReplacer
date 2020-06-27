using ColossalFramework.IO;
using ColossalFramework.UI;
using ICities;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TrafficLightReplacer
{
    public class ModThreading : ThreadingExtensionBase
    {
        bool processed = false;

        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {

            if (Input.GetKey(KeyCode.F3))
            {
                if (processed == false)
                {
                    Debug.Log("F3 Pressed TLR");
                    Tools.CheckPanels();
                    processed = true;
                    Debug.Log("is visible? " + TrafficLightReplacePanel.instance.isVisible);
                }
            }

            else
            {
                processed = false;
            }
        }
    }
}