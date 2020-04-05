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
        public static List<Asset> result;

        public override void OnLevelLoaded(LoadMode mode)
        {
            string path = Path.Combine(DataLocation.addonsPath,"test.xml");
            XmlSerializer serializer = new XmlSerializer(typeof(List<Asset>));
            StreamReader reader = new StreamReader(path);
            result = (List<Asset>)serializer.Deserialize(reader);
            reader.Close();

            for (int i = 0; i < result.Count; i++)
            {
                Debug.Log("entry:" + i);
                Debug.Log("prefabname:" + result[i].Prefab);
                Debug.Log("prefabsize:" + result[i].Size);
            }

            TrafficLightReplacePanel.instance.Show();  //initalize UI

            ReplaceTrafficLights();
        }

        private static void ReplaceTrafficLights()
        {
            Debug.Log("modloaded");

            var newProp = PrefabCollection<PropInfo>.FindLoaded(result[0].Prefab); 
            var newPropLong = PrefabCollection<PropInfo>.FindLoaded(result[1].Prefab);  //>6 width
            var newPropXL = PrefabCollection<PropInfo>.FindLoaded(result[2].Prefab);  //>11 width

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
                    float lanecount = 0;

                    GetRoadInformation(prefab, ref roadwidth, ref lanecount);

                    Debug.Log("Total road width: " + roadwidth + " | lane count: " + lanecount);

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
                                    if (roadwidth >= 12 && lanecount > 3)
                                    {
                                        ReplaceProp(newPropXL, propGroup);
                                    }
                                    else if (roadwidth >= 6)
                                    {
                                        ReplaceProp(newPropLong, propGroup);
                                    }
                                    else
                                    {
                                        ReplaceProp(newProp, propGroup);  //regular
                                    }
                                }
                            }
                        }

                    }
                }
            }
        }

        private static void GetRoadInformation(NetInfo prefab, ref float roadwidth, ref float lanecount)
        {
            //what to do about asym roads?
            foreach (NetInfo.Lane lane in prefab.m_lanes)
            {
                if (lane.m_laneType.ToString() == "Parking" || lane.m_laneType.ToString() == "Vehicle")
                {
                    //detect one way roads - calculate width across whole road
                    if (prefab.m_hasBackwardVehicleLanes == false || prefab.m_hasForwardVehicleLanes == false)
                    {
                        Debug.Log("oneway road!");
                        Debug.Log("Lane width: " + lane.m_width + "|  Lanetype:" + lane.m_laneType);
                        roadwidth += lane.m_width;
                        lanecount++;
                    }
                    //two way roads - add widths from positive lane positions
                    else if (lane.m_position > 0)
                    {
                        Debug.Log("Lane width: " + lane.m_width + "|  Lanetype:" + lane.m_laneType);
                        roadwidth += lane.m_width;
                        lanecount++;
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
