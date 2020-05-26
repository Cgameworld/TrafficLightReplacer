﻿using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.Packaging;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using System.IO;
using System.Reflection;
using TrafficLightReplacer.Locale;
using TrafficLightReplacer.TranslationFramework;
using UnityEngine;

namespace TrafficLightReplacer
{
    public class TrafficLightReplacePanel : UIPanel
    {
        private UITitleBar m_title;

        private static TrafficLightReplacePanel _instance;

        public int segmentReplaceCount = 0;
        public UICheckBox oppositeSideToggle;
        public UIDropDown packDropdown;
        private UIButton confirmButton;
        public UIButton customizeButton;
        public UILabel customizeButtonToggle;
        private UITextureAtlas m_atlas;
        public UIPanel customizePanel;
        public UIDropDown smallRoadsDropdown;
        public UIDropDown mediumRoadsDropdown;
        public UIDropDown largeRoadsDropdown;
        private UIButton getmeditems;

        public static TrafficLightReplacePanel instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = UIView.GetAView().AddUIComponent(typeof(TrafficLightReplacePanel)) as TrafficLightReplacePanel;
                }
                return _instance;
            }
        }

        public override void Start()
        {
            LoadResources(); 

            atlas = UIUtils.GetAtlas("Ingame");
            backgroundSprite = "MenuPanel2";
            color = new Color32(255, 255, 255, 255);
            isVisible = false;
            canFocus = true;
            isInteractive = true;
            //clipChildren = true;
            width = 370;
            height = 100;
            relativePosition = new Vector3(1550, 150);

            // Title Bar
            m_title = AddUIComponent<UITitleBar>();
            m_title.title = Translation.Instance.GetTranslation(TranslationID.MAINWINDOW_TITLE);
            //m_title.isModal = true;

            UILabel areaTypeLabel = AddUIComponent<UILabel>();
            //"select from road panel"
            areaTypeLabel.autoSize = false;
            areaTypeLabel.width = 60;
            areaTypeLabel.height = 30;
            areaTypeLabel.relativePosition = new Vector2(20, 60);
            areaTypeLabel.text = "Pack:";

            packDropdown = UIUtils.CreateDropDown(this);
            packDropdown.width = 270;
            //add option to toggle between euro and generic vanilla!!!
            packDropdown.relativePosition = new Vector3(80, 53);
            packDropdown.AddItem("Empty");
            packDropdown.selectedIndex = 0;

            packDropdown.eventSelectedIndexChanged += (c, p) =>
            {
                Debug.Log("2dropdown change to: " + Replacer.packList[packDropdown.selectedIndex].PackPath);
                Replacer.Start(Replacer.packList[packDropdown.selectedIndex].PackPath);
               // Debug.Log("2dropdown change to: " + Replacer.xmlFileNames[packDropdown.selectedIndex]);
                //Replacer.Start(Replacer.xmlFileNames[packDropdown.selectedIndex]);
                ResetAllDropdowns();
                AddAllItemsToDropdowns();
                TLRModSettings.instance.CurrentPackIndex = packDropdown.selectedIndex;
                TLRModSettings.instance.LastLoadedXML = Replacer.packList[packDropdown.selectedIndex].PackPath;
                TLRModSettings.instance.Save();
            };

            oppositeSideToggle = UIUtils.CreateCheckBox(this);
            oppositeSideToggle.text = "Place on opposite side of stop line";
            oppositeSideToggle.isChecked = false;
            oppositeSideToggle.relativePosition = new Vector2(20, 100);
            oppositeSideToggle.tooltip = "Dummy Button - TBD";
            oppositeSideToggle.isVisible = false;

            oppositeSideToggle.eventCheckChanged += (c, p) =>
            {           
                Replacer.UpdateLaneProps();
                Debug.Log("checkboxchecked and updatelaneprops fired");
            };


            customizeButton = UIUtils.CreateButtonSpriteImage(this, m_atlas);
            customizeButton.normalBgSprite = "SubBarButtonBase";
            customizeButton.hoveredBgSprite = "SubBarButtonBaseHovered";
            customizeButton.textHorizontalAlignment = UIHorizontalAlignment.Left;
            customizeButton.textPadding.top = 4;
            customizeButton.textPadding.left = 40;
            customizeButton.text = "Customize Lights";
            customizeButton.textScale = 0.9f;
            customizeButton.relativePosition = new Vector2(20, 135);
            customizeButton.height = 25;
            customizeButton.width = 330;
            customizeButton.tooltip = "Select Traffic Light Variations";
            customizeButton.isVisible = false;

            customizeButtonToggle = UIUtils.CreateLabelSpriteImage(this, m_atlas);
            customizeButtonToggle.backgroundSprite = "PropertyGroupClosed";
            customizeButtonToggle.width = 18f;
            customizeButtonToggle.height = 18f;
            customizeButtonToggle.relativePosition = new Vector2(32, 139);
            customizeButtonToggle.isVisible = false;

            customizeButton.eventClick += (c, p) =>
            {
                if (isVisible)
                {

                    if (customizeButtonToggle.backgroundSprite == "PropertyGroupOpen")
                    {
                        customizePanel.isVisible = false;
                        customizeButtonToggle.backgroundSprite = "PropertyGroupClosed";
                        height = 180;
                    }
                    else
                    {
                        customizePanel.isVisible = true;
                        customizeButtonToggle.backgroundSprite = "PropertyGroupOpen";
                        height = 320;
                        ResetAllDropdowns();
                        AddAllItemsToDropdowns();
                    }
                }
            };

            customizePanel = AddUIComponent<UIPanel>();
            customizePanel.relativePosition = new Vector2(0, 170);
            customizePanel.size = new Vector2(260, 110);
            customizePanel.isVisible = false;

            UILabel smallRoadsDropdownLabel = customizePanel.AddUIComponent<UILabel>();
            //"select from road panel"
            smallRoadsDropdownLabel.autoSize = false;
            smallRoadsDropdownLabel.width = 110;
            smallRoadsDropdownLabel.height = 30;
            smallRoadsDropdownLabel.relativePosition = new Vector2(20, 7);
            smallRoadsDropdownLabel.text = "Small Roads:";

            smallRoadsDropdown = UIUtils.CreateDropDown(customizePanel);
            smallRoadsDropdown.width = 185;
            smallRoadsDropdown.AddItem("Empty A");
            smallRoadsDropdown.selectedIndex = 0;
            smallRoadsDropdown.relativePosition = new Vector3(135, 0);
            smallRoadsDropdown.tooltip = "";

            smallRoadsDropdown.eventSelectedIndexChanged += (c, p) =>
            {
                Debug.Log("smallroadsdropdown eventselectedindex called");
                Replacer.typeSmall = GetCurrentProp(Replacer.typeSmallOptions, smallRoadsDropdown);
                smallRoadsDropdown.tooltip = Replacer.typeSmallOptions[smallRoadsDropdown.selectedIndex].Description;
                Replacer.UpdateLaneProps();

            };

            UILabel mediumRoadsDropdownLabel = customizePanel.AddUIComponent<UILabel>();
            //"select from road panel"
            mediumRoadsDropdownLabel.autoSize = false;
            mediumRoadsDropdownLabel.width = 145;
            mediumRoadsDropdownLabel.height = 30;
            mediumRoadsDropdownLabel.relativePosition = new Vector2(20, 47);
            mediumRoadsDropdownLabel.text = "Medium Roads:";

            mediumRoadsDropdown = UIUtils.CreateDropDown(customizePanel);
            mediumRoadsDropdown.width = 185;
            mediumRoadsDropdown.AddItem("Empty B");
            mediumRoadsDropdown.selectedIndex = 0;
            mediumRoadsDropdown.relativePosition = new Vector3(155, 40);
            mediumRoadsDropdown.tooltip = "";

            mediumRoadsDropdown.eventSelectedIndexChanged += (c, p) =>
            {
                Replacer.typeMedium = GetCurrentProp(Replacer.typeMediumOptions, mediumRoadsDropdown);
                mediumRoadsDropdown.tooltip = Replacer.typeMediumOptions[mediumRoadsDropdown.selectedIndex].Description;
                Replacer.UpdateLaneProps();
            };

            UILabel largeRoadsDropdownLabel = customizePanel.AddUIComponent<UILabel>();
            //"select from road panel"
            largeRoadsDropdownLabel.autoSize = false;
            largeRoadsDropdownLabel.width = 145;
            largeRoadsDropdownLabel.height = 30;
            largeRoadsDropdownLabel.relativePosition = new Vector2(20, 87);
            largeRoadsDropdownLabel.text = "Large Roads:";

            largeRoadsDropdown = UIUtils.CreateDropDown(customizePanel);
            largeRoadsDropdown.width = 185;
            largeRoadsDropdown.AddItem("Empty C");
            largeRoadsDropdown.selectedIndex = 0;
            largeRoadsDropdown.relativePosition = new Vector3(155, 80);
            largeRoadsDropdown.tooltip = "";

            largeRoadsDropdown.eventSelectedIndexChanged += (c, p) =>
            {
                Replacer.typeLarge = GetCurrentProp(Replacer.typeLargeOptions, largeRoadsDropdown);
                largeRoadsDropdown.tooltip = Replacer.typeLargeOptions[largeRoadsDropdown.selectedIndex].Description;
                Replacer.UpdateLaneProps();
            };

            confirmButton = UIUtils.CreateButton(this);
            confirmButton.text = "deleteitems+add1";
            confirmButton.relativePosition = new Vector2(20, 400);
            confirmButton.width = 150;
            confirmButton.isVisible = false;

            confirmButton.eventClick += (c, p) =>
            {
                string[] blank = new string[0];
                mediumRoadsDropdown.items = blank;
                mediumRoadsDropdown.AddItem("TEST1");
            };

            getmeditems = UIUtils.CreateButton(this);
            getmeditems.text = "getpaths-d";
            getmeditems.relativePosition = new Vector2(220, 200);
            getmeditems.width = 120;
            //getmeditems.isVisible = false;

            getmeditems.eventClick += (c, p) =>
            {
                Debug.Log("\nassetdirpaths:");

                for (uint i = 0; i < PrefabCollection<PropInfo>.LoadedCount(); i++)
                {
                    var prefab = PrefabCollection<PropInfo>.GetLoaded(i);

                    if (prefab == null)
                        continue;

                    var asset = PackageManager.FindAssetByName(prefab.name);
                    if (asset == null || asset.package == null)
                        continue;

                    var crpPath = asset.package.packageName;

                    if (crpPath == "2032407437")
                    {
                        Debug.Log("CLUS tRafficLights!");
                    }
                    Debug.Log("crppath " + crpPath);
                }
            };

        }

        private static PropInfo GetCurrentProp(System.Collections.Generic.List<Asset> currentpropCategory, UIDropDown dropdown)
        {
            PropInfo currentProp = PrefabCollection<PropInfo>.FindLoaded(currentpropCategory[dropdown.selectedIndex].Prefab);
            Debug.Log("selectedIndex dropdown:" + dropdown.selectedIndex);
            return currentProp;
        }

        private void AddAllItemsToDropdowns()
        {
            if (!Replacer.oneSizeMode)
            {
                AddItemsToDropdown(smallRoadsDropdown, Replacer.typeSmallOptions);
                AddItemsToDropdown(mediumRoadsDropdown, Replacer.typeMediumOptions);
                AddItemsToDropdown(largeRoadsDropdown, Replacer.typeLargeOptions);
            }

            smallRoadsDropdown.tooltip = !Replacer.oneSizeMode ? Replacer.typeSmallOptions[0].Description : "OneSize Mode On! No Small Variations Loaded";
            mediumRoadsDropdown.tooltip = !Replacer.oneSizeMode ? Replacer.typeMediumOptions[0].Description : "OneSize Mode On! No Medium Variations Loaded";
            largeRoadsDropdown.tooltip = !Replacer.oneSizeMode ? Replacer.typeLargeOptions[0].Description : "OneSize Mode On! No Large Variations Loaded";
        }

        private void ResetAllDropdowns()
        {
            //add blank item before resetting index in case dropdown is null
            smallRoadsDropdown.AddItem("");
            mediumRoadsDropdown.AddItem("");
            largeRoadsDropdown.AddItem("");
            Debug.Log("bf sel0");
            smallRoadsDropdown.selectedIndex = 0;
            mediumRoadsDropdown.selectedIndex = 0;
            largeRoadsDropdown.selectedIndex = 0;
            Debug.Log("af sel0");
            ResetDropdown(smallRoadsDropdown);
            ResetDropdown(mediumRoadsDropdown);
            ResetDropdown(largeRoadsDropdown);
        }

        public static void ResetDropdown(UIDropDown dropdown)
        {
            string[] blank = new string[0];
            dropdown.items = blank;
        }

        private static void AddItemsToDropdown(UIDropDown a, System.Collections.Generic.List<Asset> b)
        {
            foreach (var sortedAsset in b)
            {
                a.AddItem(sortedAsset.Name);
            }
        }

        private void LoadResources()
        {
            string[] spriteNames = new string[]
            {
                "Folder"
            };

            m_atlas = ResourceLoader.CreateTextureAtlas("TrafficLightReplacer", spriteNames, "TrafficLightReplacer.Icons.");

            UITextureAtlas defaultAtlas = ResourceLoader.GetAtlas("Ingame");
            Texture2D[] textures = new Texture2D[13];

            textures[0] = defaultAtlas["ButtonMenu"].texture;
            textures[1] = defaultAtlas["ButtonMenuFocused"].texture;
            textures[2] = defaultAtlas["ButtonMenuHovered"].texture;
            textures[3] = defaultAtlas["ButtonMenuPressed"].texture;
            textures[4] = defaultAtlas["ButtonMenuDisabled"].texture;
            textures[5] = defaultAtlas["EmptySprite"].texture;
            textures[6] = defaultAtlas["ScrollbarTrack"].texture;
            textures[7] = defaultAtlas["ScrollbarThumb"].texture;

            UITextureAtlas mapAtlas = ResourceLoader.GetAtlas("InMapEditor");
            textures[8] = mapAtlas["SubBarButtonBase"].texture;
            textures[9] = mapAtlas["SubBarButtonBaseHovered"].texture;
            textures[10] = mapAtlas["SubBarButtonBaseDisabled"].texture;
            textures[11] = mapAtlas["PropertyGroupClosed"].texture;
            textures[12] = mapAtlas["PropertyGroupOpen"].texture;


            ResourceLoader.AddTexturesInAtlas(m_atlas, textures);

        }

    }

}
