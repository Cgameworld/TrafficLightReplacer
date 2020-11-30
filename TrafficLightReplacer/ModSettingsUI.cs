using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrafficLightReplacer.Locale;
using TrafficLightReplacer.TranslationFramework;
using UnityEngine;

namespace TrafficLightReplacer
{
    static class ModSettingsUI
    {
        public static void GenerateMenuSettings(UIHelperBase helper)
        {
            UIHelperBase featuresGroup = helper.AddGroup(Translation.Instance.GetTranslation(TranslationID.FEATURESGROUPTITLE));

            featuresGroup.AddCheckbox(Translation.Instance.GetTranslation(TranslationID.PACKCREATOROPTION), TLRModSettings.instance.ShowCreatorTool, sel =>
            {
                TLRModSettings.instance.ShowCreatorTool = sel;
                TLRModSettings.instance.Save();

                TrafficLightReplacePanel.instance.isVisible = true;
                CreatorToolPanel.instance.isVisible = sel;
            });

            featuresGroup.AddCheckbox(Translation.Instance.GetTranslation(TranslationID.MAINBACKGROUNDOPTION), TLRModSettings.instance.EnableButtonBackground, sel =>
            {
                TLRModSettings.instance.EnableButtonBackground = sel;
                TLRModSettings.instance.Save();
                MainButton.instance.size = sel ? new Vector2(46f, 46f) : new Vector2(36f, 36f);
                MainButton.instance.normalBgSprite = sel ? "OptionBase" : null;
                MainButton.instance.normalFgSprite = sel ? "tlr-button-padding" : "tlr-button";
            });

            UIHelperBase tweaksGroup = helper.AddGroup(Translation.Instance.GetTranslation(TranslationID.TWEAKSGROUPTITLE));

            tweaksGroup.AddCheckbox(Translation.Instance.GetTranslation(TranslationID.DEFAULTSIDESIGNALPOLE), TLRModSettings.instance.DefaultSideSignalPole, sel =>
            {
                TLRModSettings.instance.DefaultSideSignalPole = sel;
                TLRModSettings.instance.Save();
                Replacer.defaultSideSignalPole = sel;
                Replacer.UpdateLaneProps();
            });

            //helper.AddSpace(15);
            var resetmessage = Translation.Instance.GetTranslation(TranslationID.RESETGROUPMESSAGE).Split('*');
            UIHelperBase resetGroup = helper.AddGroup(resetmessage[0]);
            resetGroup.AddButton(resetmessage[0] + resetmessage[1], () =>
            {
                MainButton.instance.SetDefaultPosition();
                Debug.Log(resetmessage[0] + resetmessage[2]);
            });
        }
    }
}
