using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.UI;
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
        private UISlider curbOffsetSlider;
        private UITextField curbOffsetField;

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
            height = 270;
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
            packDropdown.relativePosition = new Vector3(20, 40);
            packDropdown.selectedIndex = 0;

            packDropdown.eventSelectedIndexChanged += (c, p) =>
            {

            };

            UILabel curbOffsetLabel = AddUIComponent<UILabel>();
            curbOffsetLabel.text = "Offset X:";
            curbOffsetLabel.autoSize = false;
            curbOffsetLabel.width = 125f;
            curbOffsetLabel.height = 20f;
            curbOffsetLabel.relativePosition = new Vector2(15, 82);

            curbOffsetSlider = UIUtils.CreateSlider(this, "curboffsetslider", -9f, 9f, 0.05f, 0f);
            curbOffsetSlider.width = 105f;
            curbOffsetSlider.relativePosition = new Vector3(125, 85);

            curbOffsetSlider.eventValueChanged += (c, p) =>
            {
                curbOffsetField.text = curbOffsetSlider.value.ToString();
            };

            curbOffsetField = UIUtils.CreateTextField(this);
            curbOffsetField.text = curbOffsetSlider.value.ToString();
            curbOffsetField.width = 55f;
            curbOffsetField.height = 25f;
            curbOffsetField.padding = new RectOffset(0, 0, 6, 0);
            curbOffsetField.relativePosition = new Vector3(240, 80);

            curbOffsetField.eventTextSubmitted += (c, p) =>
            {
                curbOffsetSlider.value = float.Parse(curbOffsetField.text);
            };

            UILabel curbUnitsLabel = AddUIComponent<UILabel>();
            curbUnitsLabel.text = "u";
            curbUnitsLabel.autoSize = false;
            curbUnitsLabel.width = 125f;
            curbUnitsLabel.height = 20f;
            curbUnitsLabel.relativePosition = new Vector2(300, 85);

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
