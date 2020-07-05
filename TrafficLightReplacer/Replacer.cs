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
        public static PropInfo typeSignalPoleMirror;

        public static List<CachePropItem> propGroupCache = new List<CachePropItem>();
        public static TransformValues transformOffset = new TransformValues();

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

            AssignValues(path, XMLinput);
            ModifyMainUI();

            if (!oneSizeMode)
            {
                typeSmall = PrefabCollection<PropInfo>.FindLoaded(typeSmallOptions[0].Prefab);
                typeMedium = PrefabCollection<PropInfo>.FindLoaded(typeMediumOptions[0].Prefab);  //>6 width
                typeLarge = PrefabCollection<PropInfo>.FindLoaded(typeLargeOptions[0].Prefab);  //>11 width
            }
            UpdateLaneProps();
        }

        private static void AssignValues(string path, TLRConfig XMLinput)
        {
            //pack name is grabbed in in tools.getpacklist - refreshed only on every window open

            oneSizeMode = XMLinput.OneSize;
            Debug.Log("oneSizeMode: " + oneSizeMode);

            //start of asset pack reading
            //fill list with prop assets from XML
            foreach (var item in XMLinput.Assets)
            {
                result.Add(item);
            }

            //set to blank asset on default
            typePedSignal = PrefabCollection<PropInfo>.FindLoaded(Tools.BlankProp);
            typeSignalPoleMirror = null;

            //set default transform values
            transformOffset = new TransformValues()
            {
                Position = new Vector3(0, 0, 0),
                Angle = 0,
                Scale = 100
            };

            for (int i = 0; i < result.Count; i++)
            {
                Debug.Log("Pack NAME! " + XMLinput.PackName);
                Debug.Log("entry:" + i);
                Debug.Log("prefabname:" + result[i].Prefab);
                Debug.Log("prefabsize:" + result[i].Type);

                //mutlisize config
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
                //vanilla config
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
                if (result[i].Type == "Signal Pole Mirror")
                {
                    typeSignalPoleMirror = PrefabCollection<PropInfo>.FindLoaded(result[i].Prefab);
                }
            }

            //read optional transform settings
            if (XMLinput.Transform != null)
            {
                Debug.Log("transform not null!");
                transformOffset = XMLinput.Transform;
            }
        }

        public static void ModifyMainUI()
        {
            if (oneSizeMode)
            {
                if (TrafficLightReplacePanel.instance.oppositeSideToggle != null)
                {
                    //Debug.Log("ran! OSM1");
                    //add code here to move dropdown2 up and change height
                    TrafficLightReplacePanel.instance.oppositeSideToggle.isVisible = true;
                    TrafficLightReplacePanel.instance.customizeButton.isVisible = false;
                    TrafficLightReplacePanel.instance.customizeButtonToggle.isVisible = false;
                    TrafficLightReplacePanel.instance.customizePanel.isVisible = false;

                    TrafficLightReplacePanel.instance.dropdown2.relativePosition = new Vector2(0, 140);
                    TrafficLightReplacePanel.instance.vanillaConfigOffset = -45;
                    TrafficLightReplacePanel.instance.dropdown2_init = TrafficLightReplacePanel.instance.dropdown2.relativePosition;
                    TrafficLightReplacePanel.instance.height = 170;
                }
            }
            else
            {

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
        }
        public static void UpdateLaneProps()
        {
            int propGroupCounter = 0;
            foreach (var prefab in Resources.FindObjectsOfTypeAll<NetInfo>())
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
                                    if (TrafficLightReplacePanel.instance.oppositeSideToggle != null && TrafficLightReplacePanel.instance.oppositeSideToggle.isChecked)
                                    {
                                        ReplacePropFlipped(lane, propGroup, typeMain, isOneWay, propGroupCounter);
                                    }
                                    else
                                    {
                                        OneSizeReplace(propGroupCounter, propGroup, lane);
                                    }
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
                                            ReplaceProp(lane, typeLarge, propGroup, propGroupCounter);
                                        }
                                        else if (roadwidth >= 6)
                                        {
                                            ReplaceProp(lane, typeMedium, propGroup, propGroupCounter);
                                        }
                                        else
                                        {
                                            ReplaceProp(lane, typeSmall, propGroup, propGroupCounter);
                                        }
                                    }

                                }

                                propGroupCounter++;

                            }
                        }
                    }
                }

            }
            Debug.Log("propGroupCounterTotal" + propGroupCounter);
        }

        private static void OneSizeReplace(int propGroupCounter, NetLaneProps.Prop propGroup, NetInfo.Lane lane)
        {
            //Debug.Log("onesize mode on!");

            if (propGroup.m_prop.name == "Traffic Light 02")
            {
                propGroup.m_finalProp = typeMain;
                OneSizeApplyProperties(propGroupCounter, propGroup, lane);
            }
            if (propGroup.m_prop.name == "Traffic Light 02 Mirror")
            {
                propGroup.m_finalProp = typeMirror;
                OneSizeApplyProperties(propGroupCounter, propGroup, lane, true);
            }

            if (propGroup.m_prop.name == "Traffic Light Pedestrian")
            {
                propGroup.m_finalProp = typePedSignal;
                OneSizeApplyProperties(propGroupCounter, propGroup, lane);

            }

            if (propGroup.m_prop.name == "Traffic Light 01") //see if mirror version comes up at all!
            {
                propGroup.m_finalProp = typeSignalPole;
                OneSizeApplyProperties(propGroupCounter, propGroup, lane);
            }
        }

        private static void OneSizeApplyProperties(int propGroupCounter, NetLaneProps.Prop propGroup, NetInfo.Lane lane, bool isMirror = false)
        {
            propGroup.m_angle = isMirror
                ? propGroupCache[propGroupCounter].Angle - transformOffset.Angle
                : propGroupCache[propGroupCounter].Angle + transformOffset.Angle;

            propGroup.m_position.x = lane.m_position > 0
                ? propGroupCache[propGroupCounter].Position.x + transformOffset.Position.x
                : propGroupCache[propGroupCounter].Position.x - transformOffset.Position.x;

            propGroup.m_position.y = propGroupCache[propGroupCounter].Position.y + transformOffset.Position.y;

            propGroup.m_position.z = propGroup.m_segmentOffset < 0
                ? propGroupCache[propGroupCounter].Position.z + transformOffset.Position.z
                : propGroupCache[propGroupCounter].Position.z - transformOffset.Position.z;

            var scale = 1 + ((transformOffset.Scale - 100) / 100);
            //Debug.Log("OSAP scale: " + scale);

            propGroup.m_finalProp.m_minScale = scale;
            propGroup.m_finalProp.m_maxScale = scale;
        }

        private static void ReplacePropFlipped(NetInfo.Lane lane, NetLaneProps.Prop propGroup, PropInfo newProp, bool isOneWay, int propGroupCounter)
        {
            if (lane.m_laneType.ToString() == "Pedestrian")
            {
                if (propGroup.m_prop.name == "Traffic Light Pedestrian" || propGroup.m_prop.name == "Traffic Light 01")
                {
                    propGroup.m_finalProp = newProp;

                    //check mirroring rotating?
                    propGroup.m_angle = -(propGroupCache[propGroupCounter].Angle - transformOffset.Angle);
                    propGroup.m_position.x = lane.m_position > 0
                    ? propGroupCache[propGroupCounter].Position.x + transformOffset.Position.x
                    : propGroupCache[propGroupCounter].Position.x - transformOffset.Position.x;

                    MultiSizeFlippedApplyProperties(lane, propGroup, propGroupCounter);

                }
            }
            else
            {
                //change road median ped signal
                if (propGroup.m_prop.name == "Traffic Light Pedestrian")
                {

                    propGroup.m_finalProp = typePedSignal;
                    //propGroup.m_finalProp = PrefabCollection<PropInfo>.FindLoaded("pedlight1.pedlight1_Data");
                    MultiSizeFlippedApplyProperties(lane, propGroup, propGroupCounter, true, true);
                }
            }

            if (propGroup.m_prop.name == "Traffic Light 02")
            {
                if (typeSignalPoleMirror != null)
                {
                    Debug.Log("signalpole mirror selected!");
                    propGroup.m_finalProp = typeSignalPoleMirror;
                }
                else
                {
                    Debug.Log("signalpole selected!");
                    propGroup.m_finalProp = typeSignalPole;
                }


                propGroup.m_position.x = lane.m_position > 0
                ? propGroupCache[propGroupCounter].Position.x + transformOffset.Position.x + 1f
                : propGroupCache[propGroupCounter].Position.x - transformOffset.Position.x + -1f;
                MultiSizeFlippedApplyProperties(lane, propGroup, propGroupCounter,false,true);

            }
            else if (propGroup.m_prop.name == "Traffic Light 02 Mirror")
            {
                propGroup.m_finalProp = typePedSignal;
                //propGroup.m_finalProp = PrefabCollection<PropInfo>.FindLoaded("pedlight2.pedlight2_Data");
                MultiSizeFlippedApplyProperties(lane, propGroup, propGroupCounter, true, true);
            }

            //fix for one way roads with two ped lights!
            if (propGroup.m_prop.name == "Traffic Light Pedestrian" && isOneWay == true)
            {
                if (lane.m_position < 0)
                {
                    propGroup.m_finalProp = typePedSignal;
                    //propGroup.m_finalProp = PrefabCollection<PropInfo>.FindLoaded("pedlight3.pedlight3_Data");
                    MultiSizeFlippedApplyProperties(lane, propGroup, propGroupCounter, true, true);
                }

            }
        }

        private static void MultiSizeFlippedApplyProperties(NetInfo.Lane lane, NetLaneProps.Prop propGroup, int propGroupCounter, bool includeX = false, bool includeAngle = false)
        {
            if (includeAngle)
            {
                propGroup.m_angle = propGroupCache[propGroupCounter].Angle + transformOffset.Angle;
            }

            if (includeX)
            {
                propGroup.m_position.x = lane.m_position > 0
    ? propGroupCache[propGroupCounter].Position.x + transformOffset.Position.x
    : propGroupCache[propGroupCounter].Position.x - transformOffset.Position.x;
            }

            propGroup.m_position.y = propGroupCache[propGroupCounter].Position.y + transformOffset.Position.y;

            propGroup.m_position.z = propGroup.m_segmentOffset < 0
                ? propGroupCache[propGroupCounter].Position.z + transformOffset.Position.z
                : propGroupCache[propGroupCounter].Position.z - transformOffset.Position.z;

            var scale = 1 + ((transformOffset.Scale - 100) / 100);
            //Debug.Log("OSAP scale: " + scale);

            propGroup.m_finalProp.m_minScale = scale;
            propGroup.m_finalProp.m_maxScale = scale;
        }

        private static void ReplaceProp(NetInfo.Lane lane, PropInfo newProp, NetLaneProps.Prop propGroup, int propGroupCounter)
        {
            //m_prop stays the same m_finalProp changes 

            if (propGroup.m_prop.name == "Traffic Light 02")
            {
                propGroup.m_finalProp = newProp;
                MultiSizeFlippedApplyProperties(lane, propGroup, propGroupCounter, true, true);

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
