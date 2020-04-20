﻿using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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

        //modfied from https://stackoverflow.com/questions/13031778/how-can-i-extract-a-file-from-an-embedded-resource-and-save-it-to-disk
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