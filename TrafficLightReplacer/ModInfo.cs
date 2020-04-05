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
            var newProp = PrefabCollection<PropInfo>.FindLoaded("1535107168.New Traffic Light 12_Data");
            Debug.Log("proploaded");

            foreach (var prefab1 in Resources.FindObjectsOfTypeAll<NetInfo>())
            {
                Debug.Log("initprefabname" + prefab1.name);
            }


                foreach (var prefab in Resources.FindObjectsOfTypeAll<NetInfo>())
            {
                Debug.Log("prefab name: " + prefab);
                if (prefab == null)
                {
                    Debug.Log("bad-not run");
                }

                else
                {
                    Debug.Log("good run");
                    //var prefab = PrefabCollection<NetInfo>.FindLoaded("Medium Road");
                    Debug.Log("roadloaded");
                    foreach (NetInfo.Lane lane in prefab.m_lanes)
                    {
                        if (lane?.m_laneProps?.m_props != null)
                        {
                            Debug.Log("Lane Type:" + lane.m_laneType);
                            foreach (NetLaneProps.Prop propGroup in lane.m_laneProps.m_props)
                            {
                                if (propGroup?.m_finalProp != null)
                                {
                                    Debug.Log("1prop name" + propGroup.m_finalProp.name);

                                    if (propGroup.m_finalProp.name == "Traffic Light 02")
                                    {
                                        propGroup.m_finalProp = newProp;
                                        Debug.Log("2Replacement Successful");
                                    }
                                    if (propGroup.m_finalProp.name == "Traffic Light 02 Mirror")
                                    {
                                        propGroup.m_finalProp = null;
                                        Debug.Log("2Delete Successful");
                                    }
                                }
                            }
                        }

                    }
                }


            }
        }

    }
}
