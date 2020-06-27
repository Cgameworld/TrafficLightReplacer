using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace TrafficLightReplacer
{
    public static class Replacer
    {

        public static List<Asset> result = new List<Asset>();
        public static List<Pack> packList = new List<Pack>();
        public static List<Asset> typeSmallOptions = new List<Asset>();
        public static List<Asset> typeMediumOptions = new List<Asset>();
        public static List<Asset> typeLargeOptions = new List<Asset>();

        public static PropInfo typeSmall;
        public static PropInfo typeMedium;
        public static PropInfo typeLarge;

        public static PropInfo typePedSignal;
        public static PropInfo typePedSignalMirror;

        public static PropInfo typeMain;
        public static PropInfo typeMirror;

        public static PropInfo typeSignalPole;

        public static List<CachePropItem> propGroupCache = new List<CachePropItem>();

        public static bool oneSizeMode = false;
       
        public static void Start(string path)
        {
            //clear list!
            //take care of setDefaultLights?
            result.Clear();
            result.TrimExcess();
            typeSmallOptions.Clear();
            typeSmallOptions.TrimExcess();
            typeMediumOptions.Clear();
            typeMediumOptions.TrimExcess();
            typeLargeOptions.Clear();
            typeLargeOptions.TrimExcess();

            XmlSerializer serializer = new XmlSerializer(typeof(TLRConfig));
            TLRConfig XMLinput;

            if (path.Contains("RESOURCE."))
            {
                var resourcePath = path.Replace("RESOURCE.", string.Empty);
                Stream reader = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
                XMLinput = (TLRConfig)serializer.Deserialize(reader);
                reader.Close();
            }
            else
            {
                StreamReader reader = new StreamReader(path);
                XMLinput = (TLRConfig)serializer.Deserialize(reader);
                reader.Close();
            }

            foreach (var item in XMLinput.Assets)
            {
                result.Add(item);
            }


            //set to blank asset on default?
            typePedSignal = PrefabCollection<PropInfo>.FindLoaded(Tools.BlankProp);

            for (int i = 0; i < result.Count; i++)
            {
                Debug.Log("Pack NAME! " + XMLinput.PackName);
                Debug.Log("entry:" + i);
                Debug.Log("prefabname:" + result[i].Prefab);
                Debug.Log("prefabsize:" + result[i].Type);

                //custom sizes config
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
                //option 2 - vanilla config
                if (result[i].Type == "Main")
                {
                    typeMain = PrefabCollection<PropInfo>.FindLoaded(result[i].Prefab);
                }
                if (result[i].Type == "Mirror")
                {
                    typeMirror = PrefabCollection<PropInfo>.FindLoaded(result[i].Prefab);
                }
                if (result[i].Type == "Ped Signal")
                {
                    typePedSignal = PrefabCollection<PropInfo>.FindLoaded(result[i].Prefab);
                }
                //all
                if (result[i].Type == "Signal Pole")
                {
                    typeSignalPole = PrefabCollection<PropInfo>.FindLoaded(result[i].Prefab);
                }
            }
            //Debug.Log("\ntypeSignalPole: " + typeSignalPole + "\ntypePedSignal: " + typePedSignal);
            oneSizeMode = XMLinput.OneSize;
            Debug.Log("oneSizeMode: " + oneSizeMode);

            if (oneSizeMode)
            {
                if (TrafficLightReplacePanel.instance.oppositeSideToggle != null)
                {
                    //add code here to move dropdown2 up and change height
                    TrafficLightReplacePanel.instance.oppositeSideToggle.isVisible = false;
                    TrafficLightReplacePanel.instance.customizeButton.isVisible = false;
                    TrafficLightReplacePanel.instance.customizeButtonToggle.isVisible = false;
                    TrafficLightReplacePanel.instance.customizePanel.isVisible = false;

                    TrafficLightReplacePanel.instance.dropdown2.relativePosition = new Vector2(0, 100);
                    TrafficLightReplacePanel.instance.vanillaConfigOffset = -75;
                    TrafficLightReplacePanel.instance.dropdown2_init = TrafficLightReplacePanel.instance.dropdown2.relativePosition;
                    TrafficLightReplacePanel.instance.height = 140;
                }
            }
            else
            {
                typeSmall = PrefabCollection<PropInfo>.FindLoaded(typeSmallOptions[0].Prefab);
                typeMedium = PrefabCollection<PropInfo>.FindLoaded(typeMediumOptions[0].Prefab);  //>6 width
                typeLarge = PrefabCollection<PropInfo>.FindLoaded(typeLargeOptions[0].Prefab);  //>11 width

                if (TrafficLightReplacePanel.instance.oppositeSideToggle != null)
                {
                    TrafficLightReplacePanel.instance.oppositeSideToggle.isVisible = true;
                    TrafficLightReplacePanel.instance.customizeButton.isVisible = true;
                    TrafficLightReplacePanel.instance.customizeButtonToggle.isVisible = true;

                    TrafficLightReplacePanel.instance.dropdown2.relativePosition = new Vector2(0, 175);
                    TrafficLightReplacePanel.instance.vanillaConfigOffset = 0;
                    TrafficLightReplacePanel.instance.dropdown2_init = TrafficLightReplacePanel.instance.dropdown2.relativePosition;
                    TrafficLightReplacePanel.instance.height = 220;
                }
            }

            UpdateLaneProps();
        }

        public static void UpdateLaneProps()
        {
            int propGroupCounter = 0;
            foreach (var prefab in Resources.FindObjectsOfTypeAll<NetInfo>())
            {
                if (prefab.m_vehicleTypes == VehicleInfo.VehicleType.Car)
                {
                    //Debug.Log("prefab.name: " + prefab.name  + " || propgroup counter: " + propGroupCounter);

                    float roadwidth = 0;
                    bool isOneWay = false;
                    bool isHighway = false;
                    if (prefab.name.Contains("Highway"))
                    {
                        isHighway = true;
                    }

                    GetRoadInformation(prefab, ref roadwidth, ref isOneWay);

                    foreach (NetInfo.Lane lane in prefab.m_lanes)
                    {
                        if (lane?.m_laneProps?.m_props != null)
                        {
                            foreach (NetLaneProps.Prop propGroup in lane.m_laneProps.m_props)
                            {
                                if (propGroup?.m_finalProp != null)
                                {
                                    if (oneSizeMode)
                                    {
                                        OneSizeReplace(propGroupCounter, propGroup);
                                    }

                                    else
                                    {
                                        //Debug.Log("onesize mode off!");
                                        if (TrafficLightReplacePanel.instance.oppositeSideToggle != null && TrafficLightReplacePanel.instance.oppositeSideToggle.isChecked)
                                        {
                                            if (roadwidth >= 15 || isHighway)
                                            {
                                                ReplacePropFlipped(lane, propGroup, typeLarge, isOneWay, propGroupCounter);
                                            }
                                            else if (roadwidth >= 6)
                                            {
                                                ReplacePropFlipped(lane, propGroup, typeMedium, isOneWay, propGroupCounter);
                                            }
                                            else
                                            {
                                                ReplacePropFlipped(lane, propGroup, typeSmall, isOneWay, propGroupCounter);  //regular
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

                                    propGroupCounter++;

                                }
                            }
                        }

                    }
                }

            }
            Debug.Log("propGroupCounterTotal" + propGroupCounter);
        }

        private static void OneSizeReplace(int propGroupCounter, NetLaneProps.Prop propGroup)
        {
            Debug.Log("onesize mode on!");

            if (propGroup.m_prop.name == "Traffic Light 02")
            {
                propGroup.m_finalProp = typeMain;
                propGroup.m_angle = propGroupCache[propGroupCounter].Angle;
                propGroup.m_position = propGroupCache[propGroupCounter].Position;
            }
            if (propGroup.m_prop.name == "Traffic Light 02 Mirror")
            {
                propGroup.m_finalProp = typeMirror;
                propGroup.m_angle = propGroupCache[propGroupCounter].Angle;
                propGroup.m_position = propGroupCache[propGroupCounter].Position;
            }

            if (propGroup.m_prop.name == "Traffic Light Pedestrian")
            {
                propGroup.m_finalProp = typePedSignal;
                propGroup.m_angle = propGroupCache[propGroupCounter].Angle;
                propGroup.m_position = propGroupCache[propGroupCounter].Position;
            }

            if (propGroup.m_prop.name == "Traffic Light 01") //see if mirror version comes up at all!
            {
                propGroup.m_finalProp = typeSignalPole;
                propGroup.m_angle = propGroupCache[propGroupCounter].Angle;
                propGroup.m_position = propGroupCache[propGroupCounter].Position;
            }
        }

        private static void ReplacePropFlipped(NetInfo.Lane lane, NetLaneProps.Prop propGroup, PropInfo newProp, bool isOneWay, int propGroupCounter)
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
                    propGroup.m_position.x = propGroupCache[propGroupCounter].Position.x + 1f;
                }
                else
                {
                    propGroup.m_position.x = propGroupCache[propGroupCounter].Position.x - 1f;
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

            //fix for one way roads with two ped lights!
            if (propGroup.m_prop.name == "Traffic Light Pedestrian" && isOneWay == true)
            {
                if (lane.m_position < 0)
                {
                    propGroup.m_finalProp = typePedSignal;
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

        private static void GetRoadInformation(NetInfo prefab, ref float roadwidth, ref bool isOneWay)
        {
            //to do - take into account asym roads?
            foreach (NetInfo.Lane lane in prefab.m_lanes)
            {
                if (lane.m_laneType.ToString() == "Parking" || lane.m_laneType.ToString() == "Vehicle")
                {
                    //detect one way roads - calculate width across whole road
                    if (prefab.m_hasBackwardVehicleLanes == false || prefab.m_hasForwardVehicleLanes == false)
                    {
                        isOneWay = true;
                        roadwidth += lane.m_width;
                    }
                    //two way roads - add widths from positive lane positions
                    else if (lane.m_position > 0)
                    {
                        isOneWay = false;
                        roadwidth += lane.m_width;
                    }
                }


            }

        }

    }
}
