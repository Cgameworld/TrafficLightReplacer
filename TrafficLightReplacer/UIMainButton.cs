using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

namespace TrafficLightReplacer
{
    public class UIMainButton : UIButton
    {
        public static UIMainButton instance;
        private bool dragging = false;
        private UITextureAtlas toggleButtonAtlas;

        //from American Road Signs
        public override void Start()
        {
            base.Start();
            instance = this;
            LoadResources();

            const int buttonSize = 36;

            //  Positioned relative to Freecamera Button:
            var freeCameraButton = UIView.GetAView().FindUIComponent<UIButton>("Freecamera");
            verticalAlignment = UIVerticalAlignment.Middle;
            //  
            //if (AmericanRoadsignsTool.config.buttonposition.y == -9999)
           // {
                absolutePosition = new Vector2(freeCameraButton.absolutePosition.x - (6 * buttonSize) - 5, freeCameraButton.absolutePosition.y);
           // }
           // else
           // {
           //     absolutePosition = AmericanRoadsignsTool.config.buttonposition;
           //}            
            size = new Vector2(36f, 36f);
            playAudioEvents = true;
            tooltip = "TLR test";
            //  Apply custom sprite:
            atlas = toggleButtonAtlas;
            normalFgSprite = "tlr-button";
            hoveredBgSprite = "OptionBasePressed";
            pressedBgSprite = "OptionBasePressed";
            //normalBgSprite = null;
        }

        protected override void OnClick(UIMouseEventParameter p)
        {
            if (p.buttons.IsFlagSet(UIMouseButton.Left))
            {
                TrafficLightReplacePanel.instance.Show();
                CreatorToolPanel.instance.Show();
                Tools.RefreshXMLPacks();
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
            }
            base.OnMouseUp(p);
        }

        protected override void OnMouseMove(UIMouseEventParameter p)
        {
            if (p.buttons.IsFlagSet(UIMouseButton.Right))
            {
                var ratio = UIView.GetAView().ratio;
                position = new Vector3(position.x + (p.moveDelta.x * ratio), position.y + (p.moveDelta.y * ratio), position.z);
                //  
               // AmericanRoadsignsTool.config.buttonposition = absolutePosition;
               // AmericanRoadsignsTool.SaveConfig();
                //  
            }
            base.OnMouseMove(p);
        }


        private void LoadResources()
        {
            string[] spriteNames = new string[]
            {
                "tlr-button"
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