using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class ShepsTheme
    {
        //[HarmonyPostfix, HarmonyPatch(typeof(UIThemeController), "Start")]
        private static void UIThemeController_Start_postfix(UIThemeController __instance)
        {
            var index = 0;
            var lastIndex = __instance.ui.Count - 1;

            Plugin.OnUpdate += (o, e) =>
            {
                var dir = 0;
                if (Input.GetKeyDown(KeyCode.UpArrow))
                    dir = -1;
                else if (Input.GetKeyDown(KeyCode.DownArrow))
                    dir = 1;
                else
                    dir = 0;

                if (dir == 0)
                    return;

                var image = __instance.ui[index].GetComponent<Image>();
                image.sprite = __instance.normalTheme[index];

                while (true)
                {
                    index += dir;
                    if (index == -1)
                        index = lastIndex;
                    else if (index == lastIndex + 1)
                        index = 0;

                    image = __instance.ui[index]?.GetComponent<Image>();
                    if (image != null)
                        break;
                }

                image.sprite = __instance.darkTheme[index];
                Plugin.LogInfo($"index: {index}");
            };
        }

        [HarmonyPostfix, HarmonyPatch(typeof(UIThemeController), "changeTheme")]
        private static void UIThemeController_changeTheme_postfix(int newID, UIThemeController __instance)
        {
            if (Options.Experimental.ShepsTheme.Value == false)
                return;

            if (newID == 0)
            {
                //__instance.ui[84].GetComponent<Image>().sprite = __instance.darkTheme[84];
                __instance.ui[84].GetComponent<Image>().color = new Color(0.53f, 0.70f, 0.85f, 1f);
                __instance.ui[85].GetComponent<Image>().sprite = __instance.darkTheme[85];
                __instance.ui[86].GetComponent<Image>().sprite = __instance.darkTheme[86];
                __instance.ui[87].GetComponent<Image>().sprite = __instance.darkTheme[87];

                __instance.ui[63].GetComponent<Image>().color = new Color(0.53f, 0.70f, 0.85f, 1f);
                __instance.ui[64].GetComponent<Image>().sprite = __instance.darkTheme[64];
                __instance.ui[65].GetComponent<Image>().sprite = __instance.darkTheme[65];
            }

            else
                __instance.ui[84].GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
        }
    }
}
