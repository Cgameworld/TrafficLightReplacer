using ColossalFramework.IO;
using ICities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace TrafficLightReplacer
{
    public class ModInfo : IUserMod
    {
        public string Name
        {
            get { return "Traffic Light Replacer"; }
        }

        public string Description
        {
            get { return "Mod Description"; }
        }
    }
    public class ModLoading : LoadingExtensionBase
    {
        public override void OnLevelLoaded(LoadMode mode)
        {
            TrafficLightReplacePanel.instance.Show();  //initalize UI
            TransformSettingsPanel.instance.Show();

            NetInfo[] array = Resources.FindObjectsOfTypeAll<NetInfo>();
            //intialize networkWidthCategories lists!
            for (int i = 0; i < Replacer.networkWidthCategories.Length; i++)
            {
                Replacer.networkWidthCategories[i] = new List<bool>();

                foreach (var prefab in Resources.FindObjectsOfTypeAll<NetInfo>())
                {

                    foreach (NetInfo.Lane lane in prefab.m_lanes)
                    {
                        if (lane?.m_laneProps?.m_props != null)
                        {
                            foreach (NetLaneProps.Prop propGroup in lane.m_laneProps.m_props)
                            {
                                if (propGroup?.m_finalProp != null)
                                {

                                    if (propGroup.m_prop.name == "Traffic Light Pedestrian" ||
                                    propGroup.m_prop.name == "Traffic Light 01" ||
                                    propGroup.m_prop.name == "Traffic Light 02 Mirror" ||
                                    propGroup.m_prop.name == "Traffic Light 02")
                                    {

                                        Replacer.networkWidthCategories[i].Add(false);
                                    }
                                }
                            }
                        }
                    }
                }
                
            }

            Replacer.GetRoadPropPostions();

            string xmlfile1 = Path.Combine(DataLocation.addonsPath, "test.xml");
            Replacer.Start(xmlfile1);

            Debug.Log("STARTED LOADED");
        }
    }
}
