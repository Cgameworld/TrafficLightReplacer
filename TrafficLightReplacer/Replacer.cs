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
        public static List<Asset> result = new List<Asset>();
        public static List<string> packnames = new List<string>();
        public static List<Asset> typeSmallOptions = new List<Asset>();
        public static List<Asset> typeMediumOptions = new List<Asset>();
        public static List<Asset> typeLargeOptions = new List<Asset>();

        public static PropInfo typeSmall;
        public static PropInfo typeMedium;
        public static PropInfo typeLarge;
        public static PropInfo typePedSignal;
        public static PropInfo typeSignalPole;

        public static bool setDefaultLights = false;
       
        public static void Start(string path)
        {
            Debug.Log("modloaded");

            XmlSerializer serializer = new XmlSerializer(typeof(TLRConfig));
            StreamReader reader = new StreamReader(path);
            TLRConfig XMLinput = (TLRConfig)serializer.Deserialize(reader);
            reader.Close();

            foreach (var item in XMLinput.Assets)
            {
                result.Add(item);
            }

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
                Debug.Log("Pack NAME! " + XMLinput.PackName);
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
            }
            Debug.Log("\ntypeSignalPole: " + typeSignalPole + "\ntypePedSignal: " + typePedSignal);
            Debug.Log("addedallitems");

            typeSmall = PrefabCollection<PropInfo>.FindLoaded(typeSmallOptions[0].Prefab);
            typeMedium = PrefabCollection<PropInfo>.FindLoaded(typeMediumOptions[0].Prefab);  //>6 width
            typeLarge = PrefabCollection<PropInfo>.FindLoaded(typeLargeOptions[0].Prefab);  //>11 width

            //set to blank asset
            typePedSignal = PrefabCollection<PropInfo>.FindLoaded("1535107168.New Blank Traffic Light_Data");

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

                foreach (NetInfo.Lane lane in prefab.m_lanes)
                {
                    if (lane?.m_laneProps?.m_props != null)
                    {
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
                                        

                                    }
                                    else
                                    {
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
                                    //panel is NULL
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
            }
            else if (propGroup.m_prop.name == "Traffic Light 02 Mirror")
            {
                propGroup.m_finalProp = typePedSignal;
                if (propGroup.m_position.x > 0) //fix for median ped signal being flipped
                {
                    propGroup.m_angle = 270;
                }
                

            }

        }

        private static void GetRoadInformation(NetInfo prefab, ref float roadwidth)
        {
            //to do - take into account asym roads?
            foreach (NetInfo.Lane lane in prefab.m_lanes)
            {
                if (lane.m_laneType.ToString() == "Parking" || lane.m_laneType.ToString() == "Vehicle")
                {
                    //detect one way roads - calculate width across whole road
                    if (prefab.m_hasBackwardVehicleLanes == false || prefab.m_hasForwardVehicleLanes == false)
                    {
                        roadwidth += lane.m_width;
                    }
                    //two way roads - add widths from positive lane positions
                    else if (lane.m_position > 0)
                    {
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

            }
            else if (propGroup.m_prop.name == "Traffic Light 02 Mirror")
            {
                propGroup.m_finalProp = typePedSignal;
                if (propGroup.m_position.x > 0) //fix for median ped signal being flipped
                {
                    propGroup.m_angle = 270;
                }
            }

            if (propGroup.m_prop.name == "Traffic Light Pedestrian" || propGroup.m_prop.name == "Traffic Light 01")
            {
                
                propGroup.m_finalProp = typePedSignal;

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
