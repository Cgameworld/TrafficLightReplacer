using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.Packaging;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using UnityEngine;

namespace TrafficLightReplacer
{
    public class Tools
    {
        //prefab name of the blankprop file for the beta listing
        public static string BlankProp = "blankprop.blankprop_Data"; 
        public static void ShowErrorWindow(string header, string message)
        {
            ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
            panel.SetMessage(header, message, false);
            panel.GetComponentInChildren<UISprite>().spriteName = "IconError";
            Debug.Log("--TLR: Error Window Shown--\n" + header + "\n" + message);
        }
        public static void ShowAlertWindow(string header, string message)
        {
            ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
            panel.SetMessage(header, message, false);
            Debug.Log("--TLR: Alert Window Shown--\n" + header + "\n" + message);
        }
        public static void CheckPanels()
        {
            if (TrafficLightReplacePanel.instance.isVisible == false)
            {
                TrafficLightReplacePanel.instance.Show();
                if (TLRModSettings.instance.ShowCreatorTool)
                {
                    CreatorToolPanel.instance.Show();
                }
                Tools.RefreshXMLPacks();
                TrafficLightReplacePanel.instance.CloseDropdowns();
                Replacer.ModifyMainUI();
            }
            else
            {
                TrafficLightReplacePanel.instance.Hide();
                CreatorToolPanel.instance.Hide();
            }
        }
        public static void RefreshXMLPacks()
        {
            List<Pack> packList = GetPackList();

            TrafficLightReplacePanel.ResetDropdown(TrafficLightReplacePanel.instance.packDropdown);

            foreach (var xmlItem in packList)
            {
                TrafficLightReplacePanel.instance.packDropdown.AddItem(xmlItem.PackName);
            }

            Replacer.packList = packList;

            Debug.Log("TLRModSettings.instance.CurrentPackIndex: " + TLRModSettings.instance.CurrentPackIndex);
            //add check that current pack index matches name or something?
            TrafficLightReplacePanel.instance.packDropdown.selectedIndex = TLRModSettings.instance.CurrentPackIndex;

        }

        public static List<Pack> GetPackList()
        {
            List<string> files = new List<string>();
            //get XMLs from TLR Local Folder
            if (TLRModSettings.instance.LoadTLRLocalFolder)
            {
                files = Directory.GetFiles(Path.Combine(DataLocation.localApplicationData, "TLRLocal"), "*.xml").ToList();
            }
            List<string> xmlPackNames = new List<string>();
            List<Pack> packList = new List<Pack>();

            //gets embedded resource xml files
            var embeddedxmlnames = Assembly.GetExecutingAssembly().GetManifestResourceNames();

            foreach (string file in embeddedxmlnames)
            {
                if (file.Contains("TrafficLightReplacer.DefaultXMLS") && TLRModSettings.instance.EmbeddedXMLActive.Contains(file))
                {
                    Debug.Log("resource file added: " + file);
                    files.Add("RESOURCE." + file);
                }
            }

            //get XMLs from ws asset folder
            for (uint i = 0; i < PrefabCollection<PropInfo>.LoadedCount(); i++)
            {
                var prefab = PrefabCollection<PropInfo>.GetLoaded(i);

                if (prefab == null)
                    continue;

                var asset = PackageManager.FindAssetByName(prefab.name);
                if (asset == null || asset.package == null)
                    continue;

                var crpPath = asset.package.packagePath;
                if (crpPath == null)
                    continue;

                var propDirectory = Directory.GetParent(crpPath);

                if (propDirectory.ToString() != DataLocation.assetsPath)
                {
                    var folderFiles = propDirectory.GetFiles();

                    foreach (var filePath in folderFiles)
                    {
                        var filename = Path.GetFileName(filePath.ToString());
                        if (filename == "TLRConfig.xml")
                        {
                            Debug.Log("Found TLRConfig at: " + filePath);
                            files.Add(filePath.ToString());
                        }
                    }
                }
            }

            //add xml files from ws/local/embedded folders

            foreach (var xmlFilePath in files)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(TLRConfig));
                TLRConfig XMLinput;

                if (xmlFilePath.Contains("RESOURCE."))
                {
                    var resourcePath = xmlFilePath.Replace("RESOURCE.", string.Empty);
                    Stream reader = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
                    XMLinput = (TLRConfig)serializer.Deserialize(reader);
                    reader.Close();
                }
                else
                {
                    StreamReader reader = new StreamReader(xmlFilePath);
                    XMLinput = (TLRConfig)serializer.Deserialize(reader);
                    reader.Close();
                }
                Console.WriteLine("packname: " + XMLinput.PackName);
                xmlPackNames.Add(XMLinput.PackName);
            }


            //adding xml file path and name to pack object
            for (int i = 0; i < xmlPackNames.Count; i++)
            {
                Pack item = new Pack();
                item.PackName = xmlPackNames[i];
                item.PackPath = files[i];
                packList.Add(item);
            }


            //sorts alphabetically and removes square brackets from packname for sorter
            packList = packList.OrderBy(o => Regex.Replace(o.PackName, @"\[.*\] ", "")).ToList();

            //places vanilla lights xml to top and none xml to the bottom
            var vanillatrafficindex = packList.FindIndex(a => a.PackName == "Vanilla Traffic Lights");
            var nonetrafficindex = packList.FindIndex(a => a.PackName == "None");

            //add vanilla first
            List<Pack> tempPackList = new List<Pack>
            {
                packList[vanillatrafficindex]
            };
            for (int j = 0; j < xmlPackNames.Count; j++)
            {
                if (j != vanillatrafficindex && j != nonetrafficindex)
                {
                    tempPackList.Add(packList[j]);
                }
            }
            //add none last
            tempPackList.Add(packList[nonetrafficindex]);

            packList = tempPackList;
            return packList;
        }

        public static void ExtractEmbeddedResource(string outputDir, string resourceLocation, List<string> files)
        {
            Directory.CreateDirectory(outputDir);

            foreach (string file in files)
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceLocation + @"." + file))
                {
                        using (FileStream fileStream = new FileStream(Path.Combine(outputDir, file), FileMode.Create))
                        {
                            for (int i = 0; i < stream.Length; i++)
                            {

                                fileStream.WriteByte((byte)stream.ReadByte());
                            }
                            fileStream.Close();
                    }
                }
            }
        }

        public static List<string> AddResourcePrefix(List<string> embedList)
        {
            var tempEmbedList = new List<string>();
            foreach (var item in embedList)
            {
                tempEmbedList.Add("TrafficLightReplacer.DefaultXMLS." + item);
            }

            embedList = tempEmbedList;
            return embedList;
        }

        public static void ModifyPropMeshPreload(PropInfo cris1, bool flipping)
        {
            if (cris1 != null)
            {
                Mesh meshcopy = RotateMesh180(cris1, flipping);
                cris1.m_mesh = meshcopy;
                cris1.m_mesh.RecalculateBounds();
                cris1.m_mesh.RecalculateNormals();
            }

        }
        private static Mesh RotateMesh180(PropInfo cris1, bool flip)
        {
            var mesh = cris1.m_mesh;
            var newvertices = mesh.vertices;
            var newtris = mesh.triangles;

            Vector3 center = new Vector3(0, 0, 0);
            Quaternion newRotation = new Quaternion();
            newRotation.eulerAngles = new Vector3(0, 180, 0);

            for (int i = 0; i < newvertices.Length; i++)
            {
                if (flip) newvertices[i].x *= -1;
                newvertices[i] = newRotation * (newvertices[i] - center) + center;
            }

            if (flip) newtris = FlipNormals(newtris);

            Mesh meshcopy = new Mesh
            {
                vertices = newvertices,
                colors = mesh.colors,
                triangles = newtris,
                normals = mesh.normals,
                tangents = mesh.tangents,
                uv = mesh.uv,
                uv2 = mesh.uv,
                name = mesh.name
            };
            return meshcopy;
        }
        private static int[] FlipNormals(int[] tris)
        {
            for (int i = 0; i < tris.Length / 3; i++)
            {
                int a = tris[i * 3 + 0];
                int b = tris[i * 3 + 1];
                int c = tris[i * 3 + 2];
                tris[i * 3 + 0] = c;
                tris[i * 3 + 1] = b;
                tris[i * 3 + 2] = a;
            }
            return tris;
        }


        public static bool CheckTransformEqual(TransformValues Obj1, TransformValues Obj2)
        {
            var haveSameData = false;

            foreach (PropertyInfo prop in Obj1.GetType().GetProperties())
            {
                haveSameData = prop.GetValue(Obj1, null).Equals(prop.GetValue(Obj2, null));

                if (!haveSameData)
                    return false;
            }
            return true;
        }

    }

    //to avoid having to write multiple || statements 
    //https://stackoverflow.com/questions/3907299/if-statements-matching-multiple-values
    public static class Ext
    {
        public static bool In<T>(this T t, params T[] values)
        {
            return values.Contains(t);
        }
    }
}
