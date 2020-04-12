using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.UI;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TrafficLightReplacer
{
    public class TransformSettingsPanel : UIPanel
    {
        private UITitleBar m_title;

        private static TransformSettingsPanel _instance;

        public UICheckBox oppositeSideToggle;
        private UIDropDown packDropdown;
        private UITextureAtlas m_atlas;
        public UIDropDown smallRoadsDropdown;
        public UIDropDown mediumRoadsDropdown;
        public UIDropDown largeRoadsDropdown;
        private UIButton clearButton;
        private UIButton saveButton;

        public static TransformSettingsPanel instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = UIView.GetAView().AddUIComponent(typeof(TransformSettingsPanel)) as TransformSettingsPanel;
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
            width = 330;
            height = 355;
            relativePosition = new Vector3(1205, 150);

            // Title Bar
            m_title = AddUIComponent<UITitleBar>();
            m_title.title = "Transform Settings";
            //m_title.isModal = true;

            packDropdown = UIUtils.CreateDropDown(this);
            packDropdown.width = 290;
            packDropdown.AddItem("Small Roads");
            packDropdown.AddItem("Medium Roads");
            packDropdown.AddItem("Large Roads");
            packDropdown.AddItem("Signal Pole");
            packDropdown.relativePosition = new Vector3(20, 50);
            packDropdown.selectedIndex = 0;

            packDropdown.eventSelectedIndexChanged += (c, p) =>
            {
                Debug.Log("packDropdown.selectedIndex: " + packDropdown.selectedIndex);

                for (int i = 0; i < Replacer.transformSettings[packDropdown.selectedIndex].Count; i++)
                {
                    GetComponentsInChildren<UIPanel>()[i+2].GetComponentsInChildren<UITextField>()[0].text = Replacer.transformSettings[packDropdown.selectedIndex][i].ToString();
                    GetComponentsInChildren<UIPanel>()[i+2].GetComponentsInChildren<UISlider>()[0].value = Replacer.transformSettings[packDropdown.selectedIndex][i];
                }

            };


            CreateSliderRow("Offset X:", 9f,0,"u");
            CreateSliderRow("Offset Y:", 9f,1, "u");
            CreateSliderRow("Offset Z:", 9f,2, "u");
            CreateSliderRow("Rotate X:", 180f, 3, "\x00B0");
            CreateSliderRow("Rotate Y:", 180f, 4, "\x00B0");
            //CreateSliderRow("Scale:", 100f, 5, "%");

            clearButton = UIUtils.CreateButton(this);
            clearButton.text = "Reset";
            clearButton.relativePosition = new Vector2(20, 305);
            clearButton.width = 115;

            clearButton.eventClick += (c, p) =>
            {
                for (int i = 0; i < GetComponentsInChildren<UIPanel>().Length; i++)
                {
                    if (GetComponentsInChildren<UIPanel>()[i].name == "sliderrow")
                    {
                        Debug.Log("index of UIPANELS" + i);
                        GetComponentsInChildren<UIPanel>()[i].GetComponentsInChildren<UITextField>()[0].text = "0";
                        GetComponentsInChildren<UIPanel>()[i].GetComponentsInChildren<UISlider>()[0].value = 0f;
                    }
                }
            };

            saveButton = UIUtils.CreateButton(this);
            //saveButton.text = "Save and Apply";
            saveButton.text = "Read Settings";
            saveButton.relativePosition = new Vector2(155, 305);
            saveButton.width = 155;
            
            saveButton.eventClick += (c, p) =>
            {
                List<float> items = new List<float>();

                for (int i = 0; i < GetComponentsInChildren<UIPanel>().Length; i++)
                {
                    if (GetComponentsInChildren<UIPanel>()[i].name == "sliderrow")
                    {
                        items.Add(float.Parse(GetComponentsInChildren<UIPanel>()[i].GetComponentsInChildren<UITextField>()[0].text));
                    }
                }

                Debug.Log("values!");
                foreach (var item in items)
                {
                    Debug.Log(item);
                }

                Replacer.transformSettings[packDropdown.selectedIndex] = items;
            };
        }

        private void CreateSliderRow(string rowLabel, float bound, int rownum, string unit)
        {
            int spaceamount = rownum * 40;

            UIPanel sliderRowUIPanel = AddUIComponent<UIPanel>();
            sliderRowUIPanel.relativePosition = new Vector2(0, 100+spaceamount);
            sliderRowUIPanel.size = new Vector2(width, 0);
            sliderRowUIPanel.name = "sliderrow";

            UILabel sliderOffsetLabel = sliderRowUIPanel.AddUIComponent<UILabel>();
            sliderOffsetLabel.text = rowLabel;
            sliderOffsetLabel.autoSize = false;
            sliderOffsetLabel.width = 125f;
            sliderOffsetLabel.height = 20f;
            sliderOffsetLabel.relativePosition = new Vector2(15, 2);

            UISlider sliderOffsetSlider = UIUtils.CreateSlider(sliderRowUIPanel, "slideroffsetslider", -bound, bound, 0.05f, 0f);
            sliderOffsetSlider.width = 105f;
            sliderOffsetSlider.relativePosition = new Vector3(125, 5);

            UITextField sliderOffsetField = UIUtils.CreateTextField(sliderRowUIPanel);
            sliderOffsetField.text = sliderOffsetSlider.value.ToString();
            sliderOffsetField.width = 55f;
            sliderOffsetField.height = 25f;
            sliderOffsetField.padding = new RectOffset(0, 0, 6, 0);
            sliderOffsetField.relativePosition = new Vector3(240, 0);

            sliderOffsetSlider.eventValueChanged += (c, p) =>
            {
                sliderOffsetField.text = sliderOffsetSlider.value.ToString();
            };

            sliderOffsetField.eventTextSubmitted += (c, p) =>
            {
                sliderOffsetSlider.value = float.Parse(sliderOffsetField.text);
            };

            UILabel sliderUnitsLabel = sliderRowUIPanel.AddUIComponent<UILabel>();
            sliderUnitsLabel.text = unit;
            sliderUnitsLabel.autoSize = false;
            sliderUnitsLabel.width = 125f;
            sliderUnitsLabel.height = 20f;
            sliderUnitsLabel.relativePosition = new Vector2(300, 5);
        }

        private static void ResetDropdown(UIDropDown dropdown)
        {
            string[] blank = new string[0];
            dropdown.items = blank;
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
