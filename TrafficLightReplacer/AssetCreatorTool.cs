using ColossalFramework;
using ColossalFramework.Globalization;
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
        private UIButton refreshPack;
        private UIButton testButton;

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
            clipChildren = true;
            width = 370;
            height = 225;
            relativePosition = new Vector3(1550, 720);

            // Title Bar
            m_title = AddUIComponent<UITitleBar>();
            m_title.title = "Pack Creator Helper";
            //m_title.isModal = true;

            gentempXML = UIUtils.CreateButton(this);
            gentempXML.text = Translation.Instance.GetTranslation(TranslationID.GENTEMPXML);
            gentempXML.relativePosition = new Vector2(20, 50);
            gentempXML.width = 325;

            gentempXML.eventClick += (c, p) =>
            {
                var tlrlocal = Path.Combine(DataLocation.localApplicationData, "TLRLocal");

                if (File.Exists(Path.Combine(tlrlocal, "onesize-template.xml")) || File.Exists(Path.Combine(tlrlocal, "multisize-template.xml")))
                {
                    ConfirmPanel.ShowModal(Translation.Instance.GetTranslation(TranslationID.ERROROVERWRITE), Translation.Instance.GetTranslation(TranslationID.ERROROVERWRITEDESCRP), delegate (UIComponent comp, int ret)
                    {
                        if (ret != 1)
                        {
                            Debug.Log("TLR | canceled gentemp xml warning");
                            return;
                        }

                        WriteTemplateXMLs(tlrlocal);
                    });
                }
                else
                {
                    WriteTemplateXMLs(tlrlocal);
                }

               
            };

            UILabel netNameLabel = AddUIComponent<UILabel>();
            netNameLabel.text = Translation.Instance.GetTranslation(TranslationID.NETNAMELABEL);
            netNameLabel.autoSize = false;
            netNameLabel.width = 125f;
            netNameLabel.height = 20f;
            netNameLabel.relativePosition = new Vector2(15, 100);

            netNameField = UIUtils.CreateTextField(this);
            netNameField.text = Translation.Instance.GetTranslation(TranslationID.NETNAMEFIELD);
            netNameField.tooltip = Translation.Instance.GetTranslation(TranslationID.NETNAMEFIELD);
            netNameField.width = 270f;
            netNameField.height = 25f;
            netNameField.padding = new RectOffset(0, 0, 6, 0);
            netNameField.relativePosition = new Vector3(80, 95);

            if (LocaleManager.instance.language == "de")
            {
                netNameField.textScale = 0.75f;
                netNameField.padding.top = 8;
            }

            updateButton = UIUtils.CreateButton(this);
            updateButton.text = Translation.Instance.GetTranslation(TranslationID.UPDATEBUTTON);
            updateButton.relativePosition = new Vector2(20, 135);
            updateButton.tooltip = Translation.Instance.GetTranslation(TranslationID.UPDATEBUTTONTOOLTIP); 
            updateButton.width = 100;

            updateButton.eventClick += (c, p) =>
            {
                try
                {
                    netNameField.text = GameObject.FindObjectOfType<PropTool>().m_prefab.name;
                }
                catch (Exception e)
                {
                    var windowmessage = Translation.Instance.GetTranslation(TranslationID.UPDATEERRORTEXT).Split('*');
                    Tools.ShowErrorWindow(windowmessage[0], windowmessage[1] + e);
                }
            };

            testButton = UIUtils.CreateButton(this);
            testButton.text = Translation.Instance.GetTranslation(TranslationID.TESTBUTTON);
            testButton.relativePosition = new Vector2(130, 135);
            testButton.width = 80;

            testButton.eventClick += (c, p) =>
            {
                var testProp = PrefabCollection<PropInfo>.FindLoaded(netNameField.text);

                Replacer.typeSmall = testProp;
                Replacer.typeMedium = testProp;
                Replacer.typeLarge = testProp;
                Replacer.typePedSignal = testProp;
                Replacer.typePedSignalMirror = testProp;
                Replacer.typeMain = testProp;
                Replacer.typeMirror = testProp;
                Replacer.typeSignalPole = testProp;
                Replacer.typeSignalPoleMirror = testProp;

                Replacer.UpdateLaneProps();
            };

            copyButton = UIUtils.CreateButton(this);
            copyButton.text = Translation.Instance.GetTranslation(TranslationID.COPYBUTTON);
            copyButton.relativePosition = new Vector2(220, 135);
            copyButton.width = 80;

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
            openXMLFolderButton.relativePosition = new Vector2(310, 135);
            openXMLFolderButton.height = 25;
            openXMLFolderButton.width = 31;
            openXMLFolderButton.tooltip = Translation.Instance.GetTranslation(TranslationID.XMLFOLDERBUTTONTOOLTIP);

            openXMLFolderButton.eventClick += (c, p) =>
            {
                if (isVisible)
                {
                    Directory.CreateDirectory(Path.Combine(DataLocation.localApplicationData, "TLRLocal"));
                    Utils.OpenInFileBrowser(Path.Combine(DataLocation.localApplicationData, "TLRLocal"));
                }
            };

            TLRLocalLoad = UIUtils.CreateCheckBox(this);
            TLRLocalLoad.text = Translation.Instance.GetTranslation(TranslationID.TLRLOCALLOAD);
            TLRLocalLoad.tooltip = Translation.Instance.GetTranslation(TranslationID.TLRLOCALLOAD);
            TLRLocalLoad.isChecked = TLRModSettings.instance.LoadTLRLocalFolder;
            TLRLocalLoad.relativePosition = new Vector2(20, 185);
            TLRLocalLoad.tooltip = "";

            TLRLocalLoad.eventCheckChanged += (c, p) =>
            {
                TLRModSettings.instance.LoadTLRLocalFolder = p;
                TLRModSettings.instance.Save();
            };

            refreshPack = UIUtils.CreateButton(this);
            refreshPack.text = Translation.Instance.GetTranslation(TranslationID.REFRESHPACK);
            refreshPack.tooltip = Translation.Instance.GetTranslation(TranslationID.REFRESHPACK);
            refreshPack.relativePosition = new Vector2(210, 180);
            refreshPack.width = 135;

            if (LocaleManager.instance.language == "de")
            {
                refreshPack.relativePosition = new Vector3(230f, 180f);
                refreshPack.width = 120;
            }

            refreshPack.eventClick += (c, p) =>
            {
                if (isVisible)
                {
                    Tools.RefreshXMLPacks();
                }
            };

        }

        private static void WriteTemplateXMLs(string tlrlocal)
        {
            List<string> xmltemplates = new List<string>();
            xmltemplates.Add("onesize-template.xml");
            xmltemplates.Add("multisize-template.xml");
            Tools.ExtractEmbeddedResource(tlrlocal, "TrafficLightReplacer.Templates", xmltemplates);
            var windowmessage = Translation.Instance.GetTranslation(TranslationID.GENTEMPXMLMESSAGE).Split('*');
            Tools.ShowAlertWindow(windowmessage[0], windowmessage[1] + tlrlocal + windowmessage[2]);
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