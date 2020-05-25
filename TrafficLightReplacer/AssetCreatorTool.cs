using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
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
        private UIButton openXMLFolderButton;
        private UIButton gentempXML;

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
            height = 185;
            relativePosition = new Vector3(1520, 550);

            // Title Bar
            m_title = AddUIComponent<UITitleBar>();
            m_title.title = "Pack Creator Helper";
            //m_title.isModal = true;

            UILabel netNameLabel = AddUIComponent<UILabel>();
            netNameLabel.text = "Name:";
            netNameLabel.autoSize = false;
            netNameLabel.width = 125f;
            netNameLabel.height = 20f;
            netNameLabel.relativePosition = new Vector2(15, 60);

            netNameField = UIUtils.CreateTextField(this);
            netNameField.text = "Press update to grab prop name";
            netNameField.width = 270f;
            netNameField.height = 25f;
            netNameField.padding = new RectOffset(0, 0, 6, 0);
            netNameField.relativePosition = new Vector3(80, 55);

            updateButton = UIUtils.CreateButton(this);
            updateButton.text = "Update";
            updateButton.relativePosition = new Vector2(20, 100);
            updateButton.tooltip = "Select a prop using Find It! and click update"; 
            updateButton.width = 146;

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
            copyButton.relativePosition = new Vector2(188, 100);
            copyButton.width = 100;

            copyButton.eventClick += (c, p) =>
            {
                GUIUtility.systemCopyBuffer = netNameField.text;
            };

            openXMLFolderButton = UIUtils.CreateButtonSpriteImage(this, m_atlas);
            openXMLFolderButton.normalBgSprite = "ButtonMenu";
            openXMLFolderButton.hoveredBgSprite = "ButtonMenuHovered";
            openXMLFolderButton.pressedBgSprite = "ButtonMenuPressed";
            openXMLFolderButton.disabledBgSprite = "ButtonMenuDisabled";
            openXMLFolderButton.normalFgSprite = "Folder";
            openXMLFolderButton.relativePosition = new Vector2(310, 100);
            openXMLFolderButton.height = 25;
            openXMLFolderButton.width = 31;
            openXMLFolderButton.tooltip = "Open local XML config folder";

            openXMLFolderButton.eventClick += (c, p) =>
            {
                if (isVisible)
                {
                    Utils.OpenInFileBrowser(Path.Combine(DataLocation.localApplicationData, "TLRLocal"));
                }
            };

            gentempXML = UIUtils.CreateButton(this);
            gentempXML.text = "Generate Template Pack XMLs";
            gentempXML.relativePosition = new Vector2(20, 140);
            gentempXML.width = 325;

            gentempXML.eventClick += (c, p) =>
            {
                List<string> xmltemplates = new List<string>();
                xmltemplates.Add("onesize-template.xml");
                xmltemplates.Add("multisize-template.xml");
                var tlrlocal = Path.Combine(DataLocation.localApplicationData, "TLRLocal");
                Tools.ExtractEmbeddedResource(tlrlocal, "TrafficLightReplacer.Templates", xmltemplates);
                Tools.ShowAlertWindow("Template XML Files Exported", "Template XML files exported to " + tlrlocal + "\n\nClick on the folder icon in the Pack Creator Helper window to open the TLRLocal Folder");
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