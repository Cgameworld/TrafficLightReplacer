using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.IO;
using TrafficLightReplacer.Locale;
using TrafficLightReplacer.TranslationFramework;
using UnityEngine;

namespace TrafficLightReplacer
{
    public class PerRoadPanel : UIPanel
    {
        private UITitleBar m_title;

        public static PerRoadPanel _instance;

        private UITextureAtlas m_atlas;
        private UIButton copyButton;
        private UIButton openXMLFolderButton;
        private UIDropDown netNameDropdown;
        private UIScrollablePanel mainScroll;
        int rownum = 0;
        private UIPanel mainPanel;

        public static PerRoadPanel instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = UIView.GetAView().AddUIComponent(typeof(PerRoadPanel)) as PerRoadPanel;
                }
                return _instance;
            }
        }

        public UICheckBox TLRLocalLoad { get; private set; }

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
            width = 560;
            height = 830;
            relativePosition = new Vector3(970, 115);
            name = "PerRoadPanel";

            // Title Bar
            m_title = AddUIComponent<UITitleBar>();
            m_title.title = "Assign Traffic Lights";
            //m_title.isModal = true;

            mainPanel = AddUIComponent<UIPanel>();
            mainPanel.relativePosition = new Vector2(15, 45);
            mainPanel.size = new Vector2(545, 785);
            mainScroll = UIUtils.CreateScrollBox(mainPanel, m_atlas);
            mainScroll.size = new Vector2(545, 785);
            mainScroll.relativePosition = new Vector2(0, 0);

            foreach (var prefab in Resources.FindObjectsOfTypeAll<NetInfo>())
            {
                if (prefab.m_netAI is RoadAI && !prefab.name.ToLower().Contains("toll"))
                {
                    //Debug.Log("PerRoadWIndow: " + prefab.GetUncheckedLocalizedTitle().Replace("_Data",""));
                    var roadname = prefab.GetUncheckedLocalizedTitle().Replace("_Data", "");
                    AddDropDownRow(roadname + ":");
                }
            }
              
            // AddDropDownRow("Road 2");
            //AddDropDownRow("Road 4 with Trees and Bike Lanes 2 sfsfd");
        }

        private void AddDropDownRow(string netNameText)
        {
            int spaceamount = rownum * 40;

            UILabel netNameLabel = mainScroll.AddUIComponent<UILabel>();
            netNameLabel.text = netNameText;
            netNameLabel.clipChildren = true;
            netNameLabel.autoSize = false;
            netNameLabel.width = 305f;
            netNameLabel.height = 20f;
            netNameLabel.relativePosition = new Vector2(0, 60 + spaceamount);

            netNameDropdown = UIUtils.CreateDropDown(mainScroll);
            netNameDropdown.width = 185;
            netNameDropdown.AddItem("Default");
            netNameDropdown.selectedIndex = 0;
            netNameDropdown.relativePosition = new Vector3(345, 55 + spaceamount);
            netNameDropdown.tooltip = "";

            TrafficLightReplacePanel.AddItemsToDropdown(netNameDropdown, Replacer.typeSmallOptions);
            TrafficLightReplacePanel.AddItemsToDropdown(netNameDropdown, Replacer.typeMediumOptions);
            TrafficLightReplacePanel.AddItemsToDropdown(netNameDropdown, Replacer.typeLargeOptions);

            rownum++;
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