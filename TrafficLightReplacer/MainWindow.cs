using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.IO;
using ColossalFramework.Packaging;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
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
        public Vector3 dropdown2_init;
        private UIButton transformButton;
        private UILabel transformButtonToggle;
        private UIPanel transformPanel;
        private bool changingDropdown = false;
        private UIButton clearButton;

        public bool dropdownFlag = false;

        public UIPanel dropdown1;
        public UIPanel dropdown2;

        int dropdownOffset = 0;
        int transformOffset = 0;

        public int vanillaConfigOffset = 0;
        private bool initLoad = true;

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
            height = 220;
            relativePosition = new Vector3(1550, 115);

            // Title Bar
            m_title = AddUIComponent<UITitleBar>();
            m_title.title = Translation.Instance.GetTranslation(TranslationID.MAINWINDOW_TITLE);

            UILabel areaTypeLabel = AddUIComponent<UILabel>();
            //"select from road panel"
            areaTypeLabel.autoSize = false;
            areaTypeLabel.width = 80;
            areaTypeLabel.height = 30;
            areaTypeLabel.relativePosition = new Vector2(20, 60);
            areaTypeLabel.text = Translation.Instance.GetTranslation(TranslationID.MAINWINDOW_PACKLABEL);

            packDropdown = UIUtils.CreateDropDown(this);
            packDropdown.width = 270;
            //add option to toggle between euro and generic vanilla?
            packDropdown.relativePosition = new Vector3(80, 53);
            packDropdown.AddItem(Translation.Instance.GetTranslation(TranslationID.NULLDROPDOWN));
            packDropdown.selectedIndex = -1;

            packDropdown.eventSelectedIndexChanged += (c, p) =>
            {
                Debug.Log("TLR: Dropdown Changed To: " + Replacer.packList[packDropdown.selectedIndex].PackPath);
                if (!initLoad) ResetDropdownIndices();

                ResetDropdowns();
                Replacer.Start(Replacer.packList[packDropdown.selectedIndex].PackPath);
                AddAllItemsToDropdowns();
                TLRModSettings.instance.CurrentPackIndex = packDropdown.selectedIndex;
                TLRModSettings.instance.LastLoadedXML = Replacer.packList[packDropdown.selectedIndex].PackPath;
                TLRModSettings.instance.Save();

                if (initLoad)
                {
                    Debug.Log("initload true?");
                    if (!Replacer.oneSizeMode)
                    {
                        Debug.Log("initload-onesize");
                        smallRoadsDropdown.selectedIndex = TLRModSettings.instance.SmallLightIndex;
                        mediumRoadsDropdown.selectedIndex = TLRModSettings.instance.MediumLightIndex;
                        largeRoadsDropdown.selectedIndex = TLRModSettings.instance.LargeLightIndex;
                    }
                    initLoad = false;
                }
                CloseDropdowns();
            };

            oppositeSideToggle = UIUtils.CreateCheckBox(this);
            oppositeSideToggle.text = Translation.Instance.GetTranslation(TranslationID.OPPOSITESIDETOGGLE);
            oppositeSideToggle.isChecked = TLRModSettings.instance.OppositeSideToggle;
            oppositeSideToggle.relativePosition = new Vector2(20, 100);
            oppositeSideToggle.tooltip = Translation.Instance.GetTranslation(TranslationID.OPPOSITESIDETOOLTIP);
            oppositeSideToggle.width = 340;
            oppositeSideToggle.isVisible = false;

            oppositeSideToggle.eventCheckChanged += (c, p) =>
            {
                TLRModSettings.instance.OppositeSideToggle = p;
                TLRModSettings.instance.Save();
                Replacer.UpdateLaneProps();

            };

            #region customizeDropdown
            customizeButton = UIUtils.CreateButtonSpriteImage(this, m_atlas);
            customizeButton.normalBgSprite = "SubBarButtonBase";
            customizeButton.hoveredBgSprite = "SubBarButtonBaseHovered";
            customizeButton.textHorizontalAlignment = UIHorizontalAlignment.Left;
            customizeButton.textPadding.top = 4;
            customizeButton.textPadding.left = 40;
            customizeButton.text = Translation.Instance.GetTranslation(TranslationID.CUSTOMIZEBUTTONTEXT);
            customizeButton.textScale = 0.9f;
            customizeButton.relativePosition = new Vector2(20, 135);
            customizeButton.height = 25;
            customizeButton.width = 330;
            customizeButton.tooltip = Translation.Instance.GetTranslation(TranslationID.CUSTOMIZEBUTTONTEXTTOOLTIP);
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
                        dropdownOffset = 0;
                    }
                    else
                    {
                        customizePanel.isVisible = true;
                        customizeButtonToggle.backgroundSprite = "PropertyGroupOpen";
                        dropdownOffset = 120;

                        //update ui dropdowns
                        smallRoadsDropdown.selectedIndex = TLRModSettings.instance.SmallLightIndex;
                        instance.mediumRoadsDropdown.selectedIndex = TLRModSettings.instance.MediumLightIndex;
                        largeRoadsDropdown.selectedIndex = TLRModSettings.instance.LargeLightIndex;

                        //ResetAllDropdowns();
                        //AddAllItemsToDropdowns();
                    }
                }
                RefreshFooterItems();
            };

            customizePanel = AddUIComponent<UIPanel>();
            customizePanel.relativePosition = new Vector2(0, 170);
            customizePanel.size = new Vector2(260, 110);
            customizePanel.isVisible = false;

            UILabel smallRoadsDropdownLabel = customizePanel.AddUIComponent<UILabel>();
            //"select from road panel"
            smallRoadsDropdownLabel.autoSize = false;
            smallRoadsDropdownLabel.width = 145;
            smallRoadsDropdownLabel.height = 30;
            smallRoadsDropdownLabel.relativePosition = new Vector2(20, 7);
            smallRoadsDropdownLabel.text = Translation.Instance.GetTranslation(TranslationID.SMALLROADSDROPDOWNLABEL);

            smallRoadsDropdown = UIUtils.CreateDropDown(customizePanel);
            smallRoadsDropdown.width = 185;
            smallRoadsDropdown.AddItem("Empty A");
            smallRoadsDropdown.selectedIndex = TLRModSettings.instance.SmallLightIndex;
            smallRoadsDropdown.relativePosition = new Vector3(135, 0);
            smallRoadsDropdown.tooltip = "";


            smallRoadsDropdown.eventSelectedIndexChanged += (c, p) =>
            {
                if (dropdownFlag)
                {
                    return;
                }
                //Debug.Log("smallroadsdropdown eventselectedindex called");
                Replacer.typeSmall = GetCurrentProp(Replacer.typeSmallOptions, smallRoadsDropdown);
                smallRoadsDropdown.tooltip = Replacer.typeSmallOptions[smallRoadsDropdown.selectedIndex].Description;
                TLRModSettings.instance.SmallLightIndex = smallRoadsDropdown.selectedIndex;
                TLRModSettings.instance.Save();
                Replacer.UpdateLaneProps();
            };

            UILabel mediumRoadsDropdownLabel = customizePanel.AddUIComponent<UILabel>();
            //"select from road panel"
            mediumRoadsDropdownLabel.autoSize = false;
            mediumRoadsDropdownLabel.width = 145;
            mediumRoadsDropdownLabel.height = 30;
            mediumRoadsDropdownLabel.relativePosition = new Vector2(20, 47);
            mediumRoadsDropdownLabel.text = Translation.Instance.GetTranslation(TranslationID.MEDIUMROADSDROPDOWNLABEL);

            mediumRoadsDropdown = UIUtils.CreateDropDown(customizePanel);
            mediumRoadsDropdown.width = 185;
            mediumRoadsDropdown.AddItem("Empty B");
            mediumRoadsDropdown.selectedIndex = TLRModSettings.instance.MediumLightIndex;
            mediumRoadsDropdown.relativePosition = new Vector3(155, 40);
            mediumRoadsDropdown.tooltip = "";

            mediumRoadsDropdown.eventSelectedIndexChanged += (c, p) =>
            {
                if (dropdownFlag)
                {
                    return;
                }

                Replacer.typeMedium = GetCurrentProp(Replacer.typeMediumOptions, mediumRoadsDropdown);
                mediumRoadsDropdown.tooltip = Replacer.typeMediumOptions[mediumRoadsDropdown.selectedIndex].Description;
                TLRModSettings.instance.MediumLightIndex = mediumRoadsDropdown.selectedIndex;
                TLRModSettings.instance.Save();
                Replacer.UpdateLaneProps();
            };

            UILabel largeRoadsDropdownLabel = customizePanel.AddUIComponent<UILabel>();
            //"select from road panel"
            largeRoadsDropdownLabel.autoSize = false;
            largeRoadsDropdownLabel.width = 145;
            largeRoadsDropdownLabel.height = 30;
            largeRoadsDropdownLabel.relativePosition = new Vector2(20, 87);
            largeRoadsDropdownLabel.text = Translation.Instance.GetTranslation(TranslationID.LARGEROADSDROPDOWNLABEL);

            largeRoadsDropdown = UIUtils.CreateDropDown(customizePanel);
            largeRoadsDropdown.width = 185;
            largeRoadsDropdown.AddItem("Empty C");
            largeRoadsDropdown.selectedIndex = TLRModSettings.instance.LargeLightIndex;
            largeRoadsDropdown.relativePosition = new Vector3(155, 80);
            largeRoadsDropdown.tooltip = "";

            largeRoadsDropdown.eventSelectedIndexChanged += (c, p) =>
            {
                if (dropdownFlag)
                {
                    return;
                }

                Replacer.typeLarge = GetCurrentProp(Replacer.typeLargeOptions, largeRoadsDropdown);
                largeRoadsDropdown.tooltip = Replacer.typeLargeOptions[largeRoadsDropdown.selectedIndex].Description;
                TLRModSettings.instance.LargeLightIndex = largeRoadsDropdown.selectedIndex;
                TLRModSettings.instance.Save();
                Replacer.UpdateLaneProps();
            };

            if (LocaleManager.instance.language == "de")
            {
                smallRoadsDropdown.relativePosition = new Vector3(155, 0);
                mediumRoadsDropdown.relativePosition = new Vector3(170, 40);
                largeRoadsDropdown.relativePosition = new Vector3(160, 80);
            }
            if (LocaleManager.instance.language == "zh")
            {
                packDropdown.relativePosition = new Vector3(50f, 53f);
                smallRoadsDropdown.relativePosition = new Vector3(105, 0);
                mediumRoadsDropdown.relativePosition = new Vector3(105, 40);
                largeRoadsDropdown.relativePosition = new Vector3(105, 80);
            }
            if (LocaleManager.instance.language == "nl")
            {
                packDropdown.width = 260;
                packDropdown.relativePosition = new Vector3(90f, 53f);
                smallRoadsDropdown.relativePosition = new Vector3(155, 0);
                mediumRoadsDropdown.relativePosition = new Vector3(160, 40);
                largeRoadsDropdown.relativePosition = new Vector3(150, 80);
            }
            if (LocaleManager.instance.language == "pl")
            {
                packDropdown.width = 260;
                packDropdown.relativePosition = new Vector3(90f, 53f);
                smallRoadsDropdown.relativePosition = new Vector3(125, 0);
                mediumRoadsDropdown.relativePosition = new Vector3(145, 40);
                largeRoadsDropdown.relativePosition = new Vector3(125, 80);
            }
            if (LocaleManager.instance.language == "pt" || LocaleManager.instance.language == "ru")
            {
                packDropdown.width = 260;
                packDropdown.relativePosition = new Vector3(90f, 53f);

                smallRoadsDropdownLabel.textScale = 0.85f;
                smallRoadsDropdownLabel.padding.top = 1;
                smallRoadsDropdown.relativePosition = new Vector3(170, 0);

                mediumRoadsDropdownLabel.textScale = 0.85f;
                mediumRoadsDropdownLabel.padding.top = 1;
                mediumRoadsDropdown.relativePosition = new Vector3(155, 40);

                largeRoadsDropdownLabel.textScale = 0.85f;
                largeRoadsDropdownLabel.padding.top = 1;
                largeRoadsDropdown.relativePosition = new Vector3(170, 80);
            }
            if (LocaleManager.instance.language == "ru")
            {
                smallRoadsDropdown.relativePosition = new Vector3(140, 0);
                largeRoadsDropdown.relativePosition = new Vector3(165, 80);
            }

            #endregion

            #region transformDropdown 

            dropdown2 = AddUIComponent<UIPanel>();
            dropdown2.relativePosition = new Vector2(0, 0); //placeholder
            dropdown2.size = new Vector2(260, 10);
            dropdown2.isVisible = true;
            dropdown2_init = dropdown2.relativePosition;

            transformButton = UIUtils.CreateButtonSpriteImage(dropdown2, m_atlas);
            transformButton.normalBgSprite = "SubBarButtonBase";
            transformButton.hoveredBgSprite = "SubBarButtonBaseHovered";
            transformButton.textHorizontalAlignment = UIHorizontalAlignment.Left;
            transformButton.textPadding.top = 4;
            transformButton.textPadding.left = 40;
            transformButton.text = Translation.Instance.GetTranslation(TranslationID.TRANSFORMBUTTONTEXT);
            transformButton.textScale = 0.9f;
            transformButton.relativePosition = new Vector2(20, 0);
            transformButton.height = 25;
            transformButton.width = 330;
            transformButton.tooltip = Translation.Instance.GetTranslation(TranslationID.TRANSFORMBUTTONTEXTTOOLTIP);

            transformButtonToggle = UIUtils.CreateLabelSpriteImage(dropdown2, m_atlas);
            transformButtonToggle.backgroundSprite = "PropertyGroupClosed";
            transformButtonToggle.width = 18f;
            transformButtonToggle.height = 18f;
            transformButtonToggle.relativePosition = new Vector2(32, 4);

            transformButton.eventClick += (c, p) =>
            {
                if (isVisible)
                {

                    if (transformButtonToggle.backgroundSprite == "PropertyGroupOpen")
                    {
                        transformPanel.isVisible = false;
                        transformButtonToggle.backgroundSprite = "PropertyGroupClosed";
                        transformOffset = 0;
                    }
                    else
                    {
                        transformPanel.isVisible = true;
                        transformButtonToggle.backgroundSprite = "PropertyGroupOpen";
                        transformOffset = 240;
                        Replacer.SetTransformSliders(Replacer.transformOffset, false);
                    }
                }
                RefreshFooterItems();
            };

            transformPanel = dropdown2.AddUIComponent<UIPanel>();
            transformPanel.relativePosition = new Vector2(5, 30);
            transformPanel.size = new Vector2(260, 110);
            transformPanel.isVisible = false;

            CreateSliderRow(Translation.Instance.GetTranslation(TranslationID.OFFSET) + " X:", 9f, 0, "u", UpdateTransformSettings);
            CreateSliderRow(Translation.Instance.GetTranslation(TranslationID.OFFSET) + " Y:", 9f, 1, "u", UpdateTransformSettings);
            CreateSliderRow(Translation.Instance.GetTranslation(TranslationID.OFFSET) + " Z:", 9f, 2, "u", UpdateTransformSettings);
            CreateSliderRow(Translation.Instance.GetTranslation(TranslationID.ANGLE) + ":", 180f, 3, "\x00B0", UpdateTransformSettings);
            CreateSliderRow(Translation.Instance.GetTranslation(TranslationID.SCALE) + ":", 180f, 4, "%", UpdateTransformSettings, 1, 200, 100);

            clearButton = UIUtils.CreateButton(transformPanel);
            clearButton.text = "Reset";
            clearButton.relativePosition = new Vector2(20, 205);
            clearButton.width = 115;

            clearButton.eventClick += (c, p) =>
            {
                UpdateTransformSettings();
                Replacer.SetTransformSliders(null, true);
                Debug.Log("clearbuttonend");
            };

            #endregion

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
            getmeditems.text = "EnableTIntersection";
            getmeditems.relativePosition = new Vector2(170, -40);
            getmeditems.width = 130;
            getmeditems.isVisible = false;

            getmeditems.eventClick += (c, p) =>
            {
                foreach (var item in TIntersectionPatch.replaceIds)
                {
                    Debug.Log("replaceid loaded " + item);
                }

            };
        }
        public void RefreshFooterItems()
        {
            dropdown2.relativePosition = dropdown2_init + new Vector3(0, dropdownOffset);
            height = 220 + dropdownOffset + transformOffset + vanillaConfigOffset;

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

        public void CloseDropdowns()
        {
            //close all collapsible menus
            customizePanel.isVisible = false;
            customizeButtonToggle.backgroundSprite = "PropertyGroupClosed";
            dropdownOffset = 0;

            transformPanel.isVisible = false;
            transformButtonToggle.backgroundSprite = "PropertyGroupClosed";
            transformOffset = 0;


            RefreshFooterItems();
        }
        private void ResetDropdownIndices()
        {

            if (!Replacer.oneSizeMode)
            {
                dropdownFlag = true;
                smallRoadsDropdown.selectedIndex = 0;
                mediumRoadsDropdown.selectedIndex = 0;
                largeRoadsDropdown.selectedIndex = 0;
                dropdownFlag = false;
            }

            TLRModSettings.instance.SelectedOffsetValues = new TransformValues()
            {
                Position = new Vector3(0, 0, 0),
                Angle = 0,
                Scale = 100
            };


            TLRModSettings.instance.SmallLightIndex = -1;
            TLRModSettings.instance.MediumLightIndex = -1;
            TLRModSettings.instance.LargeLightIndex = -1;
            TLRModSettings.instance.Save();
        }

        private void ResetDropdowns()
        {
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

        private void UpdateTransformSettings()
        {
            List<float> items = new List<float>();

            for (int i = 0; i < GetComponentsInChildren<UIPanel>().Length; i++)
            {
                if (GetComponentsInChildren<UIPanel>()[i].name == "sliderrow")
                {
                    var item = float.Parse(GetComponentsInChildren<UIPanel>()[i].GetComponentsInChildren<UITextField>()[0].text);
                    items.Add(item);                   
                   // Debug.Log("i: " + i + "  |  item: " + item); //add this to logging setting?
                }
            }

            //if z offset values negative, highlight box red
            GetComponentsInChildren<UIPanel>()[7].GetComponentsInChildren<UITextField>()[0].color = items[2] < 0 ? new Color32(255, 175, 175, 255) : new Color32(255, 255, 255, 255);

            TransformValues offset = new TransformValues()
            {
                Position = new Vector3(items[0],items[1],items[2]),
                Angle = items[3],
                Scale = items[4]
            };
            Replacer.transformOffset = offset;
            TLRModSettings.instance.SelectedOffsetValues = offset;
            TLRModSettings.instance.Save();

            Replacer.UpdateLaneProps();
        }
        private void CreateSliderRow(string rowLabel, float bound, int rownum, string unit, Action Update, float lower = -1, float upper = -1, float defaultValue = 0)
        {
            if (upper == -1 && lower == -1)
            {
                upper = bound;
                lower = -bound;
            }
            int spaceamount = rownum * 40;

            UIPanel sliderRowUIPanel = transformPanel.AddUIComponent<UIPanel>();
            sliderRowUIPanel.relativePosition = new Vector2(0, 10 + spaceamount);
            sliderRowUIPanel.size = new Vector2(width, 0);
            sliderRowUIPanel.name = "sliderrow";

            UILabel sliderOffsetLabel = sliderRowUIPanel.AddUIComponent<UILabel>();
            sliderOffsetLabel.text = rowLabel;
            sliderOffsetLabel.autoSize = false;
            sliderOffsetLabel.width = 125f;
            sliderOffsetLabel.height = 20f;
            sliderOffsetLabel.relativePosition = new Vector2(15, 2);

            UISlider sliderOffsetSlider = UIUtils.CreateSlider(sliderRowUIPanel, "slideroffsetslider", lower, upper, 0.05f, defaultValue);
            sliderOffsetSlider.width = 110f;
            sliderOffsetSlider.relativePosition = new Vector3(120, 5);
            //sliderOffsetSlider.tooltip = "Hold alt to snap";

            UITextField sliderOffsetField = UIUtils.CreateTextField(sliderRowUIPanel);
            sliderOffsetField.text = sliderOffsetSlider.value.ToString();
            sliderOffsetField.width = 55f;
            sliderOffsetField.height = 25f;
            sliderOffsetField.padding = new RectOffset(0, 0, 6, 0);
            sliderOffsetField.relativePosition = new Vector3(240, 0);

            sliderOffsetSlider.eventValueChanged += (c, p) =>
            {
                if (!changingDropdown)
                {
                    if (Input.GetKey(KeyCode.LeftAlt))
                    {
                        //if angle/scale
                        if (bound == 180)
                        {
                            sliderOffsetSlider.stepSize = 5f;
                        }
                        else
                        {
                            sliderOffsetSlider.stepSize = 0.5f;
                        }
                    }
                    else
                    {
                        sliderOffsetSlider.stepSize = 0.05f;
                    }
                    sliderOffsetField.text = sliderOffsetSlider.value.ToString();
                    Update();
                }
            };

            sliderOffsetField.eventTextSubmitted += (c, p) =>
            {
                if (!changingDropdown)
                {
                    sliderOffsetSlider.value = float.Parse(sliderOffsetField.text);
                    Update();
                }
            };

            UILabel sliderUnitsLabel = sliderRowUIPanel.AddUIComponent<UILabel>();
            sliderUnitsLabel.text = unit;
            sliderUnitsLabel.autoSize = false;
            sliderUnitsLabel.width = 125f;
            sliderUnitsLabel.height = 20f;
            sliderUnitsLabel.relativePosition = new Vector2(300, 5);

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
