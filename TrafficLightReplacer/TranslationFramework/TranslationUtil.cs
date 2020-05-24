﻿using System;
using System.Linq;
using ColossalFramework.Plugins;
using ICities;

//using BloodyPenguin's Translation Framework with TPB's Theme Mixer 2 implementation
namespace TrafficLightReplacer.TranslationFramework
{
    public static class TranslationUtil
    {
        public static string AssemblyPath => PluginInfo.modPath;

        private static PluginManager.PluginInfo PluginInfo
        {
            get
            {
                var pluginManager = PluginManager.instance;
                var plugins = pluginManager.GetPluginsInfo();

                foreach (var item in plugins)
                {
                    try
                    {
                        var instances = item.GetInstances<IUserMod>();
                        if (!(instances.FirstOrDefault() is ModInfo))
                            continue;

                        return item;
                    }
                    catch
                    {

                    }
                }
                throw new Exception($"Failed to find assembly!");
            }
        }
    }
}