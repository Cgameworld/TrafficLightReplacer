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
        private UIScrollablePanel mainScroll;
        int rownum = 0;
        private UIPanel mainPanel;
        private UIPanel listPanel;
        private UIButton searchBoxFieldButton;
        private List<UILabel> roadLabels;
        private List<UIDropDown> roadDropdowns;
        private UITextField searchBoxField;

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

            roadLabels = new List<UILabel>();
            roadDropdowns = new List<UIDropDown>();

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
            m_title.title = "Assign Traffic Lights Per Road";
            //m_title.isModal = true;

            mainPanel = AddUIComponent<UIPanel>();
            mainPanel.relativePosition = new Vector2(15, 45);
            mainPanel.size = new Vector2(545, 785);
            mainScroll = UIUtils.CreateScrollBox(mainPanel, m_atlas);
            mainScroll.size = new Vector2(545, 775);
            mainScroll.relativePosition = new Vector2(0, 10);

            UILabel searchLabel = mainScroll.AddUIComponent<UILabel>();
            searchLabel.text = "Enter Road Name:";
            searchLabel.autoSize = false;
            searchLabel.width = 155f;
            searchLabel.height = 20f;
            searchLabel.relativePosition = new Vector2(0, 5);

            searchBoxField = UIUtils.CreateTextField(mainScroll);
            searchBoxField.text = "";
            searchBoxField.width = 300f;
            searchBoxField.height = 25f;
            searchBoxField.padding = new RectOffset(0, 0, 6, 0);
            searchBoxField.relativePosition = new Vector3(160, 0);

            searchBoxFieldButton = UIUtils.CreateButton(mainScroll);
            searchBoxFieldButton.text = "Search";
            searchBoxFieldButton.relativePosition = new Vector2(470, 0);
            searchBoxFieldButton.width = 60;

            searchBoxField.eventTextChanged += (c, p) =>
            {
                GenerateRoadList();
            };

            searchBoxFieldButton.eventClick += (c, p) =>
            {
                GenerateRoadList();
            };

            listPanel = mainScroll.AddUIComponent<UIPanel>();
            listPanel.relativePosition = new Vector2(0, 40);
            listPanel.size = new Vector2(545, 785);
        }

        private void GenerateRoadList()
        {
            //delete existing ui dropdowns
            rownum = 0;
            for (int i = 0; i < roadLabels.Count; i++)
            {
                Destroy(roadLabels[i].gameObject);
                Destroy(roadDropdowns[i].gameObject);
            }
            roadLabels.Clear();
            roadDropdowns.Clear();

            foreach (var prefab in Resources.FindObjectsOfTypeAll<NetInfo>())
            {
                if (prefab.name.ToLower().Contains(searchBoxField.text))
                {
                    if (prefab.m_netAI is RoadAI && !prefab.name.ToLower().Contains("toll"))
                    {
                        Debug.Log("added: " + prefab.GetUncheckedLocalizedTitle().Replace("_Data", ""));
                        var roadname = prefab.GetUncheckedLocalizedTitle().Replace("_Data", "");
                        AddDropDownRow(roadname + ":");
                    }
                }
            }
        }

        private void AddDropDownRow(string netNameText)
        {
            //to do 
            //have default/selected (small med large light option in dropdown)
            //and more connect to logic and save changes
            //have it react to different pack changes!!
            Debug.Log("rownum: " + rownum);

            int spaceamount = rownum * 40;

            roadLabels.Add(new UILabel());
            roadLabels[rownum] = listPanel.AddUIComponent<UILabel>();
            roadLabels[rownum].text = netNameText;
            roadLabels[rownum].clipChildren = true;
            roadLabels[rownum].autoSize = false;
            roadLabels[rownum].width = 325f;
            roadLabels[rownum].height = 20f;
            roadLabels[rownum].relativePosition = new Vector2(0, 5 + spaceamount);

            roadDropdowns.Add(new UIDropDown());
            roadDropdowns[rownum] = UIUtils.CreateDropDown(listPanel);
            roadDropdowns[rownum].width = 185;
            roadDropdowns[rownum].AddItem("Default");
            roadDropdowns[rownum].selectedIndex = 0;
            roadDropdowns[rownum].relativePosition = new Vector3(335, 0 + spaceamount);
            roadDropdowns[rownum].tooltip = "";

            TrafficLightReplacePanel.AddItemsToDropdown(roadDropdowns[rownum], Replacer.typeSmallOptions);
            TrafficLightReplacePanel.AddItemsToDropdown(roadDropdowns[rownum], Replacer.typeMediumOptions);
            TrafficLightReplacePanel.AddItemsToDropdown(roadDropdowns[rownum], Replacer.typeLargeOptions);

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