using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            Debug.Log("modloaded");
            var newProp = PrefabCollection<PropInfo>.FindLoaded("1541164608.New Traffic Light 12_Data");
            var newPropLong = PrefabCollection<PropInfo>.FindLoaded("1541164608.New Traffic Light 7_Data");  //>3 lanes
            var newPropXL = PrefabCollection<PropInfo>.FindLoaded("1541164608.New Traffic Light 11_Data");  //>5 lanes


            foreach (var prefab in Resources.FindObjectsOfTypeAll<NetInfo>())
            {
                Debug.Log("prefab name: " + prefab);
                if (prefab == null)
                {
                    Debug.Log("bad-not run");
                }

                else
                {
                    //Debug.Log("good run");
                    //var prefab = PrefabCollection<NetInfo>.FindLoaded("Medium Road");
                  //  Debug.Log("roadloaded");

                    float roadwidth = 0;

                    //what to do about asym roads?
                    foreach (NetInfo.Lane lane in prefab.m_lanes)
                    {
                        if (lane.m_laneType.ToString() == "Parking" || lane.m_laneType.ToString() == "Vehicle")
                        {
                            if (lane.m_position > 0)
                            {
                                Debug.Log("Lane width: " + lane.m_width + "|  Lanetype:" + lane.m_laneType);
                                roadwidth += lane.m_width;
                            }
                        }
                    }

                    Debug.Log("Total road width: " + roadwidth);



                    foreach (NetInfo.Lane lane in prefab.m_lanes)
                    {
                        if (lane?.m_laneProps?.m_props != null)
                        {
                         //   Debug.Log("Lane Type:" + lane.m_laneType);
                            foreach (NetLaneProps.Prop propGroup in lane.m_laneProps.m_props)
                            {
                                if (propGroup?.m_finalProp != null)
                                {
                                 //   Debug.Log("1prop name" + propGroup.m_finalProp.name);

                                    ReplaceProp(newProp, propGroup);
                                }
                            }
                        }

                    }
                }
            }
        }

        private static void ReplaceProp(PropInfo newProp, NetLaneProps.Prop propGroup)
        {
            if (propGroup.m_finalProp.name == "Traffic Light 02")
            {
                propGroup.m_finalProp = newProp;
               // Debug.Log("3Replacement Successful");
            }
            else if (propGroup.m_finalProp.name == "Traffic Light 02 Mirror")
            {
                propGroup.m_finalProp = null;
                //Debug.Log("3Delete Successful");
            }
        }
    }
}
