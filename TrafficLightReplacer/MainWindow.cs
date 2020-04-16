using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.UI;
using System.IO;
using UnityEngine;

namespace TrafficLightReplacer
{
    public class TrafficLightReplacePanel : UIPanel
    {
        private UITitleBar m_title;

        private static TrafficLightReplacePanel _instance;

        public int segmentReplaceCount = 0;
        public UICheckBox oppositeSideToggle;
        private UIDropDown packDropdown;
        private UIButton customizeButton;
        private UILabel customizeButtonToggle;
        private UITextureAtlas m_atlas;
        private UIPanel customizePanel;
        public UIDropDown smallRoadsDropdown;
        public UIDropDown mediumRoadsDropdown;
        public UIDropDown largeRoadsDropdown;
        private UIButton getmeditems;
        private UIButton transformButton;
        private UIButton getTest2;

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
            clipChildren = true;
            width = 370;
            height = 200;
            relativePosition = new Vector3(1550, 150);

            // Title Bar
            m_title = AddUIComponent<UITitleBar>();
            m_title.title = "Traffic Light Replacer (Alpha 1)";
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
            packDropdown.AddItem("None (Nonfunctional for now)");
            packDropdown.AddItem("Test Pack - test.xml");
            packDropdown.AddItem("Test Pack - test2.xml");
            packDropdown.selectedIndex = 1;
            packDropdown.relativePosition = new Vector3(80, 53);

            packDropdown.eventSelectedIndexChanged += (c, p) =>
            {
                //for now xml file locations are hardcoded - will search through directories later
                if (packDropdown.selectedIndex == 0)
                {
                    //to do make default lights pack
                }
                else if (packDropdown.selectedIndex == 1)
                {
                    string xmlfile1 = Path.Combine(DataLocation.addonsPath, "test.xml");
                    Replacer.Start(xmlfile1);
                }
                else if (packDropdown.selectedIndex == 2)
                {
                    string xmlfile2 = Path.Combine(DataLocation.addonsPath, "test2.xml");
                    Replacer.Start(xmlfile2);
                }

                ResetAllDropdowns();
                AddAllItemsToDropdowns();
            };

            oppositeSideToggle = UIUtils.CreateCheckBox(this);
            oppositeSideToggle.text = "Place on opposite side of stop line";
            oppositeSideToggle.isChecked = false;
            oppositeSideToggle.relativePosition = new Vector2(20, 100);
            oppositeSideToggle.tooltip = "Dummy Button - TBD";

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

            customizeButtonToggle = UIUtils.CreateLabelSpriteImage(this, m_atlas);
            customizeButtonToggle.backgroundSprite = "PropertyGroupClosed";
            customizeButtonToggle.width = 18f;
            customizeButtonToggle.height = 18f;
            customizeButtonToggle.relativePosition = new Vector2(32, 139);

            customizeButton.eventClick += (c, p) =>
            {
                if (isVisible)
                {

                    if (customizeButtonToggle.backgroundSprite == "PropertyGroupOpen")
                    {
                        customizePanel.isVisible = false;
                        customizeButtonToggle.backgroundSprite = "PropertyGroupClosed";
                        height = 200;
                    }
                    else
                    {
                        customizePanel.isVisible = true;
                        customizeButtonToggle.backgroundSprite = "PropertyGroupOpen";
                        height = 520;
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
            smallRoadsDropdown.tooltip = Replacer.typeSmallOptions[0].Description;

            smallRoadsDropdown.eventSelectedIndexChanged += (c, p) =>
            {
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
            mediumRoadsDropdown.tooltip = Replacer.typeMediumOptions[0].Description;

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
            largeRoadsDropdown.tooltip = Replacer.typeLargeOptions[0].Description;

            largeRoadsDropdown.eventSelectedIndexChanged += (c, p) =>
            {
                Replacer.typeLarge = GetCurrentProp(Replacer.typeLargeOptions, largeRoadsDropdown);
                largeRoadsDropdown.tooltip = Replacer.typeLargeOptions[largeRoadsDropdown.selectedIndex].Description;
                Replacer.UpdateLaneProps();
            };

            transformButton = UIUtils.CreateButton(this);
            transformButton.text = "Transform Settings";
            transformButton.relativePosition = new Vector2(20, 290);
            transformButton.width = 220;

            transformButton.eventClick += (c, p) =>
            {
                TransformSettingsPanel.instance.Show();
            };

            getmeditems = UIUtils.CreateButton(this);
            getmeditems.text = "gettransformsettings";
            getmeditems.relativePosition = new Vector2(220, 400);
            getmeditems.width = 120;
            //getmeditems.isVisible = false;

            getmeditems.eventClick += (c, p) =>
            {
                for (int i = 0; i<Replacer.transformSettings.Length; i++)
                {
                    string total = "";
                    foreach (var settings in Replacer.transformSettings[i])
                    {
                        total = total + " | " + settings;
                    }

                    Debug.Log("Settings for " + i + ": " + total);
                }

            };

            getTest2 = UIUtils.CreateButton(this);
            getTest2.text = "triggergetindexs";
            getTest2.relativePosition = new Vector2(80, 400);
            getTest2.width = 120;
            //getTest2.isVisible = false;

            getTest2.eventClick += (c, p) =>
            {
                Replacer.GetRoadPropPostions();
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
            AddItemsToDropdown(smallRoadsDropdown, Replacer.typeSmallOptions);
            AddItemsToDropdown(mediumRoadsDropdown, Replacer.typeMediumOptions);
            AddItemsToDropdown(largeRoadsDropdown, Replacer.typeLargeOptions);
        }

        private void ResetAllDropdowns()
        {
            smallRoadsDropdown.selectedIndex = 0;
            mediumRoadsDropdown.selectedIndex = 0;
            largeRoadsDropdown.selectedIndex = 0;
            ResetDropdown(smallRoadsDropdown);
            ResetDropdown(mediumRoadsDropdown);
            ResetDropdown(largeRoadsDropdown);
        }

        private static void ResetDropdown(UIDropDown dropdown)
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
