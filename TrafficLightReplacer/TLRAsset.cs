using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TrafficLightReplacer
{
    public class TLRConfig
    {
        public string PackName { get; set; }
        public bool OneSize { get; set; }
        public List<Asset> Assets { get; set; }
        public TransformValues Transform { get; set; }
        public DSelectionIndex DropdownSelectionIndex { get; set; }

    }
    public class Asset
    {
        public string Prefab { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
    }

    public class TransformValues
    {
        public Vector3 Position { get; set; }
        public float Angle { get; set; }
        public float Scale { get; set; }
    }
    public class DSelectionIndex
    {
        public int SmallRoads { get; set; }
        public int MediumRoads { get; set; }
        public int LargeRoads { get; set; }
    }

}