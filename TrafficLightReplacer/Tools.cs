using ColossalFramework.IO;
using ColossalFramework.UI;
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
    public class Tools
    {
        public static void ShowErrorWindow(string header, string message)
        {
            ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
            panel.SetMessage(header, message, false);
            panel.GetComponentInChildren<UISprite>().spriteName = "IconError";
        }
        public static void RefreshXMLPacks()
        {
           // Replacer.xmlNames.Clear();
           // Replacer.xmlFileNames.Clear();

            string[] files = Directory.GetFiles(Path.Combine(DataLocation.localApplicationData, "TLRLocal"), "*.xml");
            List<string> packnames = new List<string>();

            foreach (var xmlFilePath in files)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(TLRConfig));
                StreamReader reader = new StreamReader(xmlFilePath);
                TLRConfig XMLinput = (TLRConfig)serializer.Deserialize(reader);
                reader.Close();
                Debug.Log("packname: " + XMLinput.PackName);
                packnames.Add(XMLinput.PackName);
            }

            TrafficLightReplacePanel.ResetDropdown(TrafficLightReplacePanel.instance.packDropdown);

            foreach (var xmlNameItem in packnames)
            {
                TrafficLightReplacePanel.instance.packDropdown.AddItem(xmlNameItem);
            }
            Debug.Log("2-aa");
            TrafficLightReplacePanel.instance.packDropdown.selectedIndex = 0;
            Replacer.xmlNames = packnames;
            Replacer.xmlFileNames = files.ToList();
        }
        public static void ExtractEmbeddedResource(string outputDir, string resourceLocation, List<string> files)
        {
            Directory.CreateDirectory(outputDir);

            foreach (string file in files)
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceLocation + @"." + file))
                {
                    if (!File.Exists(Path.Combine(outputDir, file)))
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
        }

    }
}
