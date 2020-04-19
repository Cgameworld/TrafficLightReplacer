using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.UI;
using System;
using System.IO;
using UnityEngine;

namespace TrafficLightReplacer
{
    public class CreatorToolPanel : UIPanel
    {
        private UITitleBar m_title;

        private static CreatorToolPanel _instance;

        private UITextureAtlas m_atlas;
        private UITextField netNameField;
        private UIButton updateButton;
        private UIButton copyButton;

        public static CreatorToolPanel instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = UIView.GetAView().AddUIComponent(typeof(CreatorToolPanel)) as CreatorToolPanel;
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
            height = 180;
            relativePosition = new Vector3(1550, 550);

            // Title Bar
            m_title = AddUIComponent<UITitleBar>();
            m_title.title = "Pack Creator Helper";
            //m_title.isModal = true;

            UILabel curbOffsetLabel = AddUIComponent<UILabel>();
            curbOffsetLabel.text = "Select a Network (using roads panel/find it/elektrix's road picker)";
            curbOffsetLabel.autoSize = false;
            curbOffsetLabel.width = 350f;
            curbOffsetLabel.height = 20f;
            curbOffsetLabel.relativePosition = new Vector2(15, 60);

            UILabel netNameLabel = AddUIComponent<UILabel>();
            netNameLabel.text = "Name:";
            netNameLabel.autoSize = false;
            netNameLabel.width = 125f;
            netNameLabel.height = 20f;
            netNameLabel.relativePosition = new Vector2(15, 100);

            netNameField = UIUtils.CreateTextField(this);
            netNameField.text = "";
            netNameField.width = 270f;
            netNameField.height = 25f;
            netNameField.padding = new RectOffset(0, 0, 6, 0);
            netNameField.relativePosition = new Vector3(80, 95);

            updateButton = UIUtils.CreateButton(this);
            updateButton.text = "Update";
            updateButton.relativePosition = new Vector2(20, 140);
            updateButton.width = 100;

            updateButton.eventClick += (c, p) =>
            {
                try
                {
                    netNameField.text = GameObject.FindObjectOfType<PropTool>().m_prefab.name;
                }
                catch (Exception e)
                {
                    Tools.ShowErrorWindow("Null Prefab", "Make sure a prop is selected!\n" + e);
                }
            };

            copyButton = UIUtils.CreateButton(this);
            copyButton.text = "Copy";
            copyButton.relativePosition = new Vector2(150, 140);
            copyButton.width = 100;

            copyButton.eventClick += (c, p) =>
            {
                GUIUtility.systemCopyBuffer = netNameField.text;
            };

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