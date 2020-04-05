using ColossalFramework.UI;
using ColossalFramework;
using UnityEngine;

namespace TrafficLightReplacer
{
    public class TrafficLightReplacePanel : UIPanel
    {
        private UITitleBar m_title;

        private static TrafficLightReplacePanel _instance;

        public int segmentReplaceCount = 0;
        private UICheckBox oppositeSideToggle;
        private UIDropDown packDropdown;
        private UIButton confirmButton;

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
            atlas = UIUtils.GetAtlas("Ingame");
            backgroundSprite = "MenuPanel2";
            color = new Color32(255, 255, 255, 255);
            isVisible = false;
            canFocus = true;
            isInteractive = true;
            clipChildren = true;
            width = 370;
            height = 225;
            relativePosition = new Vector3(1550, 150);

            // Title Bar
            m_title = AddUIComponent<UITitleBar>();
            m_title.title = "Traffic Light Replacer";
            //m_title.isModal = true;

            UILabel areaTypeLabel = AddUIComponent<UILabel>();
            //"select from road panel"
            areaTypeLabel.autoSize = false;
            areaTypeLabel.width = 60;
            areaTypeLabel.height = 30;
            areaTypeLabel.relativePosition = new Vector2(20, 60);
            areaTypeLabel.text = "Pack:";

            packDropdown = UIUtils.CreateDropDown(this);
            packDropdown.width = 175;
            packDropdown.AddItem("Test Pack");
            packDropdown.selectedIndex = 0;
            packDropdown.relativePosition = new Vector3(80, 50);

            oppositeSideToggle = UIUtils.CreateCheckBox(this);
            oppositeSideToggle.text = "Place on opposite side of stop line";
            oppositeSideToggle.isChecked = false;
            oppositeSideToggle.relativePosition = new Vector2(20, 100);
            oppositeSideToggle.tooltip = "TBD";


            confirmButton = UIUtils.CreateButton(this);
            confirmButton.text = "Ok";
            confirmButton.relativePosition = new Vector2(20, 180);
            confirmButton.width = 200;


            confirmButton.eventClick += (c, p) =>
            {

            };
        }

    }

}
