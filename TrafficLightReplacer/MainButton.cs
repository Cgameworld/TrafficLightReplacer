using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

namespace TrafficLightReplacer
{
    public class MainButton : UIButton
    {
        public static MainButton instance;
        private bool dragging = false;
        private UITextureAtlas toggleButtonAtlas;

        //from American Road Signs
        public override void Start()
        {
            base.Start();
            instance = this;
            LoadResources();

            const int buttonSize = 36;

            var freeCameraButton = UIView.GetAView().FindUIComponent<UIButton>("Freecamera");
            verticalAlignment = UIVerticalAlignment.Middle;

            if (TLRModSettings.instance.ButtonPosition.y == -9999)
            {
            absolutePosition = new Vector2(freeCameraButton.absolutePosition.x - (6 * buttonSize) - 5, freeCameraButton.absolutePosition.y);
           }
           else
           {
               absolutePosition = TLRModSettings.instance.ButtonPosition;
           }            
            size = new Vector2(36f, 36f);
            playAudioEvents = true;
            atlas = toggleButtonAtlas;
            tooltip = "Traffic Light Replacer\nRight Click to Move";
            normalFgSprite = "tlr-button";
            hoveredBgSprite = "OptionBasePressed";
            pressedBgSprite = "OptionBasePressed";

            if (TLRModSettings.instance.EnableButtonBackground)
            {
                size = new Vector2(46f, 46f);
                normalBgSprite = "OptionBase";
                normalFgSprite = "tlr-button-padding";
            }
        }

        protected override void OnClick(UIMouseEventParameter p)
        {
            if (p.buttons.IsFlagSet(UIMouseButton.Left))
            {
                if (TrafficLightReplacePanel.instance.isVisible == false)
                {
                    TrafficLightReplacePanel.instance.Show();
                    if (TLRModSettings.instance.ShowCreatorTool)
                    {
                        CreatorToolPanel.instance.Show();
                    }
                    Tools.RefreshXMLPacks();
                }
                else
                {
                    TrafficLightReplacePanel.instance.Hide();
                    CreatorToolPanel.instance.Hide();
                }
            }

            base.OnClick(p);
        }

        protected override void OnMouseDown(UIMouseEventParameter p)
        {
            if (p.buttons.IsFlagSet(UIMouseButton.Right))
            {
                dragging = true;
            }
            base.OnMouseDown(p);
        }

        protected override void OnMouseUp(UIMouseEventParameter p)
        {
            if (p.buttons.IsFlagSet(UIMouseButton.Right))
            {
                dragging = false;
                TLRModSettings.instance.ButtonPosition = absolutePosition;
                TLRModSettings.instance.Save();
            }
            base.OnMouseUp(p);
        }

        protected override void OnMouseMove(UIMouseEventParameter p)
        {
            if (p.buttons.IsFlagSet(UIMouseButton.Right))
            {
                var ratio = UIView.GetAView().ratio;
                position = new Vector3(position.x + (p.moveDelta.x * ratio), position.y + (p.moveDelta.y * ratio), position.z);
                
            }
            base.OnMouseMove(p);
        }


        private void LoadResources()
        {
            string[] spriteNames = new string[]
            {
                "tlr-button",
                "tlr-button-padding"
            };

            toggleButtonAtlas = ResourceLoader.CreateTextureAtlas("TrafficLightReplacer", spriteNames, "TrafficLightReplacer.Icons.");

            UITextureAtlas defaultAtlas = ResourceLoader.GetAtlas("Ingame");
            Texture2D[] textures = new Texture2D[2];

            textures[0] = defaultAtlas["OptionBase"].texture;
            textures[1] = defaultAtlas["OptionBasePressed"].texture;

            ResourceLoader.AddTexturesInAtlas(toggleButtonAtlas, textures);

        }
    }
}