using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace TrafficLightReplacer
{
    public static class Replacer
    {
        public static List<Asset> result;
        public static List<Asset> typeSmallOptions = new List<Asset>();
        public static List<Asset> typeMediumOptions = new List<Asset>();
        public static List<Asset> typeLargeOptions = new List<Asset>();
        //public static List<Asset> typePedSignalOptions = new List<Asset>();

        public static PropInfo typeSmall;
        public static PropInfo typeMedium;
        public static PropInfo typeLarge;
        public static PropInfo typePedSignal;
        public static PropInfo typeSignalPole;

        public static bool setDefaultLights = false;
        

        public static void Start(string path)
        {
            Debug.Log("modloaded");

            XmlSerializer serializer = new XmlSerializer(typeof(List<Asset>));
            StreamReader reader = new StreamReader(path);
            result = (List<Asset>)serializer.Deserialize(reader);
            reader.Close();

            //clear list!
            //take care of setDefaultLights?

            typeSmallOptions.Clear();
            typeSmallOptions.TrimExcess();
            typeMediumOptions.Clear();
            typeMediumOptions.TrimExcess();
            typeLargeOptions.Clear();
            typeLargeOptions.TrimExcess();

            for (int i = 0; i < result.Count; i++)
            {
                Debug.Log("entry:" + i);
                Debug.Log("prefabname:" + result[i].Prefab);
                Debug.Log("prefabsize:" + result[i].Type);

                if (result[i].Type == "Small")
                {
                    typeSmallOptions.Add(result[i]);
                }
                if (result[i].Type == "Medium")
                {
                    typeMediumOptions.Add(result[i]);
                }
                if (result[i].Type == "Large")
                {
                    typeLargeOptions.Add(result[i]);
                }
                if (result[i].Type == "Signal Pole")
                {
                    typeSignalPole = PrefabCollection<PropInfo>.FindLoaded(result[i].Prefab);
                }
                if (result[i].Type == "Ped Signal")
                {
                    typePedSignal = PrefabCollection<PropInfo>.FindLoaded(result[i].Prefab);
                }
            }
            Debug.Log("\ntypeSignalPole: " + typeSignalPole + "\ntypePedSignal: " + typePedSignal);
            Debug.Log("addedallitems");

            //get index of ui!

            typeSmall = PrefabCollection<PropInfo>.FindLoaded(typeSmallOptions[0].Prefab);
            typeMedium = PrefabCollection<PropInfo>.FindLoaded(typeMediumOptions[0].Prefab);  //>6 width
            typeLarge = PrefabCollection<PropInfo>.FindLoaded(typeLargeOptions[0].Prefab);  //>11 width

            
            UpdateLaneProps();
        }

        public static void UpdateLaneProps()
        {
            Debug.Log(typeSmall);

            foreach (var prefab in Resources.FindObjectsOfTypeAll<NetInfo>())
            {
                float roadwidth = 0;
                bool isHighway = false;
                if (prefab.name.Contains("Highway"))
                {
                    isHighway = true;
                }

                GetRoadInformation(prefab, ref roadwidth);

                //Debug.Log("Total road width: " + roadwidth + " | lane count: " + lanecount);

                foreach (NetInfo.Lane lane in prefab.m_lanes)
                {
                    if (lane?.m_laneProps?.m_props != null)
                    {
                        //   Debug.Log("Lane Type:" + lane.m_laneType);
                        foreach (NetLaneProps.Prop propGroup in lane.m_laneProps.m_props)
                        {
                            if (propGroup?.m_finalProp != null)
                            {


                                if (TrafficLightReplacePanel.instance.oppositeSideToggle != null)
                                {
                                    if (TrafficLightReplacePanel.instance.oppositeSideToggle.isChecked)
                                    {
                                        if (roadwidth >= 15 || isHighway)
                                        {
                                            ReplacePropFlipped(lane, propGroup, typeLarge);
                                        }
                                        else if (roadwidth >= 6)
                                        {
                                            ReplacePropFlipped(lane, propGroup, typeMedium);
                                        }
                                        else
                                        {
                                            ReplacePropFlipped(lane, propGroup, typeSmall);  //regular
                                        }
                                        //Debug.Log("opposite side checked");
                                        

                                    }
                                    else
                                    {
                                        //Debug.Log("opposite side unchecked");
                                        //   Debug.Log("1prop name" + propGroup.m_finalProp.name);
                                        if (roadwidth >= 15 || isHighway)
                                        {
                                            ReplaceProp(lane, typeLarge, propGroup);
                                        }
                                        else if (roadwidth >= 6)
                                        {
                                            ReplaceProp(lane, typeMedium, propGroup);
                                        }
                                        else
                                        {
                                            ReplaceProp(lane, typeSmall, propGroup);  //regular
                                        }
                                    }

                                }
                                else
                                {
                                    //Debug.Log("panel is NULL");
                                    if (roadwidth >= 15 || isHighway)
                                    {
                                        ReplaceProp(lane, typeLarge, propGroup);
                                    }
                                    else if (roadwidth >= 6)
                                    {
                                        ReplaceProp(lane, typeMedium, propGroup);
                                    }
                                    else
                                    {
                                        ReplaceProp(lane, typeSmall, propGroup);  //regular
                                    }
                                }



                            }
                        }
                    }

                }

            }
        }

        private static void ReplacePropFlipped(NetInfo.Lane lane, NetLaneProps.Prop propGroup, PropInfo newProp)
        {

            if (lane.m_laneType.ToString() == "Pedestrian")
            {
                if (propGroup.m_prop.name == "Traffic Light Pedestrian" || propGroup.m_prop.name == "Traffic Light 01")
                {
                    propGroup.m_finalProp = newProp;

                    if (lane.m_position > 0)
                    {
                        propGroup.m_angle = -270f;
                    }
                    else
                    {
                        propGroup.m_angle = 270f;
                    }
                }
            }
            else
            {
                //change road median ped signal
                if (propGroup.m_prop.name == "Traffic Light Pedestrian")
                {
                    propGroup.m_finalProp = typePedSignal;
                }
            }

            if (propGroup.m_prop.name == "Traffic Light 02")
            {
                propGroup.m_finalProp = typeSignalPole;

                if (lane.m_position > 0)
                {
                    //propGroup.m_position.x = propGroup.m_position.x + 0.5f;
                    propGroup.m_position.x = -2f;
                }
                else
                {
                    //propGroup.m_position.x = propGroup.m_position.x - 0.5f;
                    propGroup.m_position.x = 2f;
                }
                // Debug.Log("3Replacement Successful");
            }
            else if (propGroup.m_prop.name == "Traffic Light 02 Mirror")
            {
                propGroup.m_finalProp = typePedSignal;
                //Debug.Log("3Delete Successful");
            }

        }

        private static void GetRoadInformation(NetInfo prefab, ref float roadwidth)
        {
            //what to do about asym roads?
            foreach (NetInfo.Lane lane in prefab.m_lanes)
            {
                if (lane.m_laneType.ToString() == "Parking" || lane.m_laneType.ToString() == "Vehicle")
                {
                    //detect one way roads - calculate width across whole road
                    if (prefab.m_hasBackwardVehicleLanes == false || prefab.m_hasForwardVehicleLanes == false)
                    {
                        //Debug.Log("oneway road!");
                        //Debug.Log("Lane width: " + lane.m_width + "|  Lanetype:" + lane.m_laneType);
                        roadwidth += lane.m_width;
                    }
                    //two way roads - add widths from positive lane positions
                    else if (lane.m_position > 0)
                    {
                        // Debug.Log("Lane width: " + lane.m_width + "|  Lanetype:" + lane.m_laneType);
                        roadwidth += lane.m_width;
                    }
                }


            }

        }

        private static void ReplaceProp(NetInfo.Lane lane, PropInfo newProp, NetLaneProps.Prop propGroup)
        {
            //m_prop stays the same m_finalProp changes 

            if (propGroup.m_prop.name == "Traffic Light 02")
            {
                propGroup.m_finalProp = newProp;
                // Debug.Log("3Replacement Successful");

            }
            else if (propGroup.m_prop.name == "Traffic Light 02 Mirror")
            {
                propGroup.m_finalProp = typePedSignal;
                //Debug.Log("3Delete Successful");
            }

            if (propGroup.m_prop.name == "Traffic Light Pedestrian" || propGroup.m_prop.name == "Traffic Light 01")
            {
                
                propGroup.m_finalProp = propGroup.m_prop;

                if (lane.m_position > 0)
                {
                    propGroup.m_angle = -90f;
                }
                else
                {
                    propGroup.m_angle = 90f;
                }
            }


        }
    }
}
