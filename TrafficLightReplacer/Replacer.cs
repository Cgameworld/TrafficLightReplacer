using ColossalFramework;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using TrafficLightReplacer.Locale;
using TrafficLightReplacer.TranslationFramework;
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

            //read xml input file returning TLRConfig object (refactor 10/31/2020)
            TLRConfig XMLinput = ReadXMLInput(path);

            //start of asset pack reading
            //fill list with prop assets from XML
            foreach (var item in XMLinput.Assets)
            {
                result.Add(item);
            }


            //check for XML errors!
            for (int i = 0; i < result.Count; i++)
            {
                if (PrefabCollection<PropInfo>.FindLoaded(result[i].Prefab) == null && result[i].Prefab != "Blank")
                {
                    string errorstring;

                    if (result[i].Prefab == "Placeholder")
                    {
                        var message = Translation.Instance.GetTranslation(TranslationID.PLACEHOLDERXMLERROR).Split('*');
                        errorstring = message[0] + XMLinput.PackName + message[1] + path;
                    }
                    else
                    {
                        var message = Translation.Instance.GetTranslation(TranslationID.GENERICXMLERROR).Split('*');
                        errorstring = message[0] + result[i].Prefab + message[1] + path;
                    }

                    Tools.ShowErrorWindow(Translation.Instance.GetTranslation(TranslationID.MAINWINDOW_TITLE) + " XML Error", errorstring);

                    return;
                }
            }

            AssignValues(path, XMLinput);
            ModifyMainUI();

            UpdateLaneProps();
            //ModifyNodes();
        }

        private static TLRConfig ReadXMLInput(string path)
        {
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

            return XMLinput;
        }

        private static void AssignValues(string path, TLRConfig XMLinput)
        {
            //pack name is grabbed in in tools.getpacklist - refreshed only on every window open

            oneSizeMode = XMLinput.OneSize;
            Debug.Log("oneSizeMode: " + oneSizeMode);

            //set to blank asset on default
            typePedSignal = PrefabCollection<PropInfo>.FindLoaded(Tools.BlankProp);
            typeSignalPoleMirror = null;

            //set transform values
            transformOffset = TLRModSettings.instance.SelectedOffsetValues;

            for (int i = 0; i < result.Count; i++)
            {
                //helpful make toggable this debug info?
               // Debug.Log("Pack NAME! " + XMLinput.PackName);
               // Debug.Log("entry:" + i);
               // Debug.Log("prefabname:" + result[i].Prefab);
               // Debug.Log("prefabsize:" + result[i].Type);

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
                    typeMain = SetProp(i);
                }
                if (result[i].Type == "Mirror")
                {
                    typeMirror = SetProp(i);
                }
                if (result[i].Type == "Ped Signal")
                {
                    typePedSignal = SetProp(i);
                }
                //all
                if (result[i].Type == "Signal Pole")
                {
                    typeSignalPole = SetProp(i);
                }
                if (result[i].Type == "Signal Pole Mirror")
                {
                    typeSignalPoleMirror = SetProp(i);
                }
            }


            //run if no values initialized or changing dropdown
            if (TLRModSettings.instance.SmallLightIndex == -1)
            {
                //set intial selected index per category - if defined in the xml file
                if (XMLinput.DropdownSelectionIndex != null)
                {
                    //Debug.Log("xmldropdownselectindex not null");
                    TLRModSettings.instance.SmallLightIndex = XMLinput.DropdownSelectionIndex.SmallRoads;
                    TLRModSettings.instance.MediumLightIndex = XMLinput.DropdownSelectionIndex.MediumRoads;
                    TLRModSettings.instance.LargeLightIndex = XMLinput.DropdownSelectionIndex.LargeRoads;
                }
                else
                {
                    //Debug.Log("xmldropdownselectindex null!");
                    TLRModSettings.instance.SmallLightIndex = 0;
                    TLRModSettings.instance.MediumLightIndex = 0;
                    TLRModSettings.instance.LargeLightIndex = 0;
                }
                TLRModSettings.instance.Save();
            }
            //Debug.Log("SmallLightIndex: " + TLRModSettings.instance.SmallLightIndex);

            //set default lights for multisize (set from xml config in future)
            if (!oneSizeMode)
            {
                typeSmall = PrefabCollection<PropInfo>.FindLoaded(typeSmallOptions[TLRModSettings.instance.SmallLightIndex].Prefab);
                typeMedium = PrefabCollection<PropInfo>.FindLoaded(typeMediumOptions[TLRModSettings.instance.MediumLightIndex].Prefab);  //>6 width
                typeLarge = PrefabCollection<PropInfo>.FindLoaded(typeLargeOptions[TLRModSettings.instance.LargeLightIndex].Prefab);  //>11 width
            }

            //read optional transform settings
            TransformValues initOffset = new TransformValues()
            {
                Position = new Vector3(0, 0, 0),
                Angle = 0,
                Scale = 100
            };
         

            if (XMLinput.Transform != null && Tools.CheckTransformEqual(TLRModSettings.instance.SelectedOffsetValues, initOffset))
            {
               // Debug.Log("transform not null!");
                transformOffset = XMLinput.Transform;
            }

            //Debug.Log("transformOffset " + transformOffset.Position.x + " | ro " + transformOffset.Angle);
            SetTransformSliders(transformOffset, false);
        }

        private static PropInfo SetProp(int i)
        {
            PropInfo prop;
            if (result[i].Prefab == "Blank")
            {
                prop = PrefabCollection<PropInfo>.FindLoaded(Tools.BlankProp);
            }
            else
            {
                prop = PrefabCollection<PropInfo>.FindLoaded(result[i].Prefab);
            }

            return prop;
        }

      //this method is used for the reset button in the transform settings panel
        public static TransformValues GetXMLTransformValues(string path)
        {
            TransformValues defaultTransformValues = new TransformValues()
            {
                Position = new Vector3(0, 0, 0),
                Angle = 0,
                Scale = 100
            };
            
            TransformValues newXMLTransformValues;

            try
            {
                TLRConfig XMLinput = ReadXMLInput(path);


                if (XMLinput.Transform != null)
                {
                    newXMLTransformValues = XMLinput.Transform;
                }
                else
                {
                    //if xml does not have transform offsets defined
                    newXMLTransformValues = defaultTransformValues;
                }
            }
            //if the input path is bad or something else goes wrong - since this method can be called way after the initial xml data is loaded. the xml file could for example be moved/deleted by the user
            //main dropdown doesnt check for this? check there too?
            catch {
                Tools.ShowErrorWindow(Translation.Instance.GetTranslation(TranslationID.MAINWINDOW_TITLE), "Error: XML Transform values cannot be read due to " + path + " not being found. Default transform values will be used");
                 newXMLTransformValues = defaultTransformValues;
            }
           
            return newXMLTransformValues;
        }      
        public static void SetTransformSliders(TransformValues transformOffset, bool isReset)
        {
            //check if panel items exist
            if (TrafficLightReplacePanel.instance.oppositeSideToggle != null)
            {
                if (!isReset)
                {
                    //slider ui is at index 5-9
                    //Debug.Log("settrans sli pos x " +  transformOffset.Position.x);
                    SetTransformSlider(5, transformOffset.Position.x);
                    SetTransformSlider(6, transformOffset.Position.y);
                    SetTransformSlider(7, transformOffset.Position.z);
                    SetTransformSlider(8, transformOffset.Angle);
                    SetTransformSlider(9, transformOffset.Scale);
                }
                else
                {
                    var path = packList[TrafficLightReplacePanel.instance.packDropdown.selectedIndex].PackPath;
                    Debug.Log("currentselectedpath! " + path);
                    TransformValues xmlTransform = GetXMLTransformValues(path);
                   /// check to see if bad path variable! ^
                    //maybe do 000000 if fail

                    //slider ui is at index 5-9
                    SetTransformSlider(5, xmlTransform.Position.x);
                    SetTransformSlider(6, xmlTransform.Position.y);
                    SetTransformSlider(7, xmlTransform.Position.z);
                    SetTransformSlider(8, xmlTransform.Angle);
                    SetTransformSlider(9, xmlTransform.Scale);
                }
            }
            else {
               // Debug.Log("else settransformslider");
            }
        }

        private static void SetTransformSlider(int slidernum, float replaceto)
        {
            TrafficLightReplacePanel.instance.GetComponentsInChildren<UIPanel>()[slidernum].GetComponentsInChildren<UITextField>()[0].text = replaceto.ToString();
            TrafficLightReplacePanel.instance.GetComponentsInChildren<UIPanel>()[slidernum].GetComponentsInChildren<UISlider>()[0].value = replaceto;
        }

        public static void ModifyMainUI()
        {
            if (oneSizeMode)
            {
                if (TrafficLightReplacePanel.instance.oppositeSideToggle != null)
                {
                   // Debug.Log("ran! OSM1");
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
                                    if (TLRModSettings.instance.OppositeSideToggle)
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
                                    if (TLRModSettings.instance.OppositeSideToggle)
                                    {                     
                                        if (roadwidth >= 15 || isHighway)
                                        {
                                            ReplacePropFlipped(lane, propGroup, typeLarge, isOneWay, propGroupCounter);
                                        }
                                        else if (roadwidth > 6)
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
                                        else if (roadwidth > 6)
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

        public static void ModifyNodes()
        {
            var bufferLength = (ushort)NetManager.instance.m_nodes.m_buffer.Length;
            int idCount = 0;
            for (ushort i = 0; i < bufferLength; i++)
            {
                var node = NetManager.instance.m_nodes.m_buffer[i];
                if (node.Info == null)
                {
                    continue;
                }

                //find t-intersection (find if node has exactly 3 intersecting roads)

                List<ushort> neighborSegmentIds = new List<ushort>
                {
                    node.m_segment0,
                    node.m_segment1,
                    node.m_segment2,
                    node.m_segment3,
                    node.m_segment4,
                    node.m_segment5,
                    node.m_segment6,
                    node.m_segment7
                };

                int intersectingRoads = 0;
                foreach (var id in neighborSegmentIds)
                {
                    if (id != 0)
                    {
                        intersectingRoads++;
                    }
                }

                if (intersectingRoads == 3 && node.m_flags.IsFlagSet(NetNode.Flags.TrafficLights))
                {
                    Debug.Log("node " + i + " is a T-intersection with traffic lights!");

                    foreach (var segmentID in neighborSegmentIds)
                    {
                        //Debug.LogWarning("\nsegmentID: " + segmentID);
                        var segment = NetManager.instance.m_segments.m_buffer[segmentID];

                        var laneID = segment.m_lanes;
                        var segmentForward = false;

                        while (laneID != 0)
                        {
                            NetLane.Flags flags = (NetLane.Flags)NetManager.instance.m_lanes.m_buffer[laneID].m_flags;
                           // Debug.Log("flags of lane " + laneID + " | " + flags.ToString());
                            if (flags.IsFlagSet(NetLane.Flags.Forward)) segmentForward = true;
                            laneID = NetManager.instance.m_lanes.m_buffer[laneID].m_nextLane;
                        }

                        if (segmentID == 0) segmentForward = true;
                        //Debug.Log("Segment Forward? : " + segmentForward);

                        if (!segmentForward)
                        {
                            Debug.Log("Segment id " + segmentID + " is intersecting");

                            int sgfwrdloop = 0;

                            foreach (NetInfo.Lane lane in segment.Info.m_lanes)
                            {
                                if (lane?.m_laneProps?.m_props != null)
                                {
                                    foreach (NetLaneProps.Prop propGroup in lane.m_laneProps.m_props)
                                    {
                                        if (propGroup?.m_finalProp != null)
                                        {
                                            if (propGroup.m_prop.name.In("Traffic Light Pedestrian","Traffic Light 01", "Traffic Light Pedestrian European", "Traffic Light European 01"))
                                            {
                                                Debug.Log("Reachedinner");
                                               // propGroup.m_finalProp = typePedSignal ;
                                                //this replaces all 2L roads - need to make a harmony patch for this just segment instead 
                                            }
                                        }
                                        sgfwrdloop++;
                                    }
                                }
                            }
                            Debug.Log("sgfwrdloop count " + sgfwrdloop);
                        }

                    }

                   // Debug.Log("Trafficlight flag?: " + node.m_flags.IsFlagSet(NetNode.Flags.TrafficLights));
                }

                idCount++;
            }

            Debug.Log("idnodes count: " + idCount);
        }
        private static void OneSizeReplace(int propGroupCounter, NetLaneProps.Prop propGroup, NetInfo.Lane lane)
        {
            //Debug.Log("onesize mode on!");

            if (propGroup.m_prop.name.In("Traffic Light 02", "Traffic Light European 02"))
            {
                propGroup.m_finalProp = typeMain;
                OneSizeApplyProperties(propGroupCounter, propGroup, lane);
            }
            if (propGroup.m_prop.name.In("Traffic Light 02 Mirror", "Traffic Light European 02 Mirror"))
            {
                propGroup.m_finalProp = typeMirror;
                OneSizeApplyProperties(propGroupCounter, propGroup, lane, true);
            }

            if (propGroup.m_prop.name.In("Traffic Light Pedestrian", "Traffic Light Pedestrian European"))
            {
                propGroup.m_finalProp = typePedSignal;
                OneSizeApplyProperties(propGroupCounter, propGroup, lane);

            }

            if (propGroup.m_prop.name.In("Traffic Light 01", "Traffic Light European 01")) //see if mirror version comes up at all!
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
            //vehicle lane type for road hacks during loading
            if (lane.m_laneType.ToString() == "Pedestrian" || lane.m_laneType.ToString() == "Vehicle")
            {
                if (propGroup.m_prop.name.In("Traffic Light Pedestrian","Traffic Light 01", "Traffic Light Pedestrian European", "Traffic Light European 01"))
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
                if (propGroup.m_prop.name.In("Traffic Light Pedestrian","Traffic Light 01","Traffic Light 01 Mirror", "Traffic Light Pedestrian European", "Traffic Light European 01", "Traffic Light European 01 Mirror"))
                {

                    propGroup.m_finalProp = typePedSignal;
                    //propGroup.m_finalProp = PrefabCollection<PropInfo>.FindLoaded("pedlight1.pedlight1_Data");
                    MultiSizeFlippedApplyProperties(lane, propGroup, propGroupCounter, true, true);
                }
            }

            if (propGroup.m_prop.name.In("Traffic Light 02", "Traffic Light European 02"))
            {
                if (typeSignalPoleMirror != null)
                {
                    propGroup.m_finalProp = typeSignalPoleMirror;
                }
                else
                {
                    propGroup.m_finalProp = typeSignalPole;
                }


                propGroup.m_position.x = lane.m_position > 0
                ? propGroupCache[propGroupCounter].Position.x + transformOffset.Position.x + 1f
                : propGroupCache[propGroupCounter].Position.x - transformOffset.Position.x + -1f;
                MultiSizeFlippedApplyProperties(lane, propGroup, propGroupCounter,false,true);

            }
            else if (propGroup.m_prop.name.In("Traffic Light 02 Mirror", "Traffic Light European 02 Mirror"))
            {
                propGroup.m_finalProp = typePedSignal;
                //propGroup.m_finalProp = PrefabCollection<PropInfo>.FindLoaded("pedlight2.pedlight2_Data");
                MultiSizeFlippedApplyProperties(lane, propGroup, propGroupCounter, true, true);
            }

            //fix for one way roads with two ped lights!
            if (propGroup.m_prop.name.In("Traffic Light Pedestrian", "Traffic Light Pedestrian European") && isOneWay == true)
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

            if (propGroup.m_prop.name.In("Traffic Light 02", "Traffic Light European 02"))
            {
                propGroup.m_finalProp = newProp;
                MultiSizeFlippedApplyProperties(lane, propGroup, propGroupCounter, true, true);

            }
            else if (propGroup.m_prop.name.In("Traffic Light 02 Mirror", "Traffic Light European 02 Mirror"))
            {
                propGroup.m_finalProp = typePedSignal;
                if (propGroup.m_position.x > 0) //fix for median ped signal being flipped
                {
                    propGroup.m_angle = 270;
                }
            }

            if (propGroup.m_prop.name.In("Traffic Light Pedestrian","Traffic Light 01","Traffic Light 01 Mirror", "Traffic Light Pedestrian European", "Traffic Light European 01", "Traffic Light European 01 Mirror"))
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
