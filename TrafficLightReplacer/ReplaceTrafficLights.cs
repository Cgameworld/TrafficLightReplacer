using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace TrafficLightReplacer
{
    public class ReplaceTrafficLights
    {
        public List<Asset> result;
        public List<Asset> typeSmallOptions = new List<Asset>();
        public List<Asset> typeMediumOptions = new List<Asset>();
        public List<Asset> typeLargeOptions = new List<Asset>();
        public List<Asset> typePedSignalOptions = new List<Asset>();

        public void Replace(string path)
        {
            Debug.Log("modloaded");

            XmlSerializer serializer = new XmlSerializer(typeof(List<Asset>));
            StreamReader reader = new StreamReader(path);
            result = (List<Asset>)serializer.Deserialize(reader);
            reader.Close();

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
                if (result[i].Type == "Ped Signal")
                {
                    typePedSignalOptions.Add(result[i]);
                }
            }
            Debug.Log("addedallitems");

            //get index of ui!

            var typeSmall = PrefabCollection<PropInfo>.FindLoaded(typeSmallOptions[0].Prefab);
            var typeMedium = PrefabCollection<PropInfo>.FindLoaded(typeMediumOptions[0].Prefab);  //>6 width
            var typeLarge = PrefabCollection<PropInfo>.FindLoaded(typeLargeOptions[0].Prefab);  //>11 width
            var typePedSignal = PrefabCollection<PropInfo>.FindLoaded(typePedSignalOptions[0].Prefab);

            foreach (var prefab in Resources.FindObjectsOfTypeAll<NetInfo>())
            {
                Debug.Log("prefab name: " + prefab);
                if (prefab == null)
                {
                    Debug.Log("bad-not run");
                }

                else
                {
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
                                        ReplaceProp(typeLarge, propGroup);
                                    }
                                    else if (roadwidth >= 6)
                                    {
                                        ReplaceProp(typeMedium, propGroup);
                                    }
                                    else
                                    {
                                        ReplaceProp(typeSmall, propGroup);  //regular
                                    }
                                }
                            }
                        }

                    }
                }
            }
        }

        private void GetRoadInformation(NetInfo prefab, ref float roadwidth, ref float lanecount)
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

        private void ReplaceProp(PropInfo newProp, NetLaneProps.Prop propGroup)
        {
            //m_prop stays the same m_finalProp changes 

            if (propGroup.m_prop.name == "Traffic Light 02")
            {
                propGroup.m_finalProp = newProp;
                // Debug.Log("3Replacement Successful");
            }
            else if (propGroup.m_prop.name == "Traffic Light 02 Mirror")
            {
                propGroup.m_finalProp = null;
                //Debug.Log("3Delete Successful");
            }
        }
    }
}
