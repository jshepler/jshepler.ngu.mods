using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class EMPurchasesHideSpeed
    {
        private static GameObject _e_purchText;
        private static GameObject _e_speed01Button;
        private static GameObject _e_speed1Button;
        private static GameObject _e_specialOffersText;
        private static GameObject _m_purchText;
        private static GameObject _m_speed01Button;
        private static GameObject _m_speed1Button;
        private static GameObject _r3_purchText;
        private static GameObject _r3_speed01Button;
        private static GameObject _r3_speed1Button;

        [HarmonyPrepare]
        private static void prep(MethodBase method)
        {
            if (method != null)
                return;

            var e = GameObject.Find("Canvas/Exp Energy Canvas /Exp Menu 1/Scroll Rect/Content/Energy Speed").transform;
            _e_purchText = e.Find("Purchases Text").gameObject;
            _e_speed01Button = e.Find("Energy Speed 0.1 Button").gameObject;
            _e_speed1Button = e.Find("Energy Speed 1 Button").gameObject;
            _e_specialOffersText = e.Find("Special Offers").gameObject;

            var m = GameObject.Find("Canvas/Exp Magic Canvas/Exp Menu 1/Scroll Rect/Content/Magic Speed").transform;
            _m_purchText = m.Find("Purchases Text").gameObject;
            _m_speed01Button = m.Find("Energy Speed 0.1 Button").gameObject;
            _m_speed1Button = m.Find("Energy Speed 1 Button").gameObject;

            var r3 = GameObject.Find("Canvas/Exp Res 3 Canvas/Res3 Exp Menu/Scroll Rect/Content/Res3 Speed").transform;
            _r3_purchText = r3.Find("Purchases Text").gameObject;
            _r3_speed01Button = r3.Find("Res3 Speed 0.1 Button").gameObject;
            _r3_speed1Button = r3.Find("Res3 Speed 1 Button").gameObject;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(EnergyPurchases), "updateEnergyPurchases")]
        private static void EnergyPurchases_updateEnergyPurchases_postfix(EnergyPurchases __instance)
        {
            var character = __instance.character;
            if (!character.InMenu(Menu.EXP_Energy))
                return;

            _e_specialOffersText.SetActive(!(character.settings.special1Bought && character.settings.special2Bought));

            var show = character.energySpeed < 50f;
            _e_purchText.SetActive(show);
            _e_speed01Button.SetActive(show);
            _e_speed1Button.SetActive(show);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(MagicPurchases), "updateMagicPurchases")]
        private static void MagicPurchases_updateMagicPurchases_postfix(MagicPurchases __instance)
        {
            var character = __instance.character;
            if (!character.InMenu(Menu.EXP_Magic))
                return;

            var show = character.magic.magicBarSpeed < 50f;
            _m_purchText.SetActive(show);
            _m_speed01Button.SetActive(show);
            _m_speed1Button.SetActive(show);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Resource3Purchases), "updateRes3Purchases")]
        private static void Resource3Purchases_updateRes3Purchases_postfix(Resource3Purchases __instance)
        {
            var character = __instance.character;
            if (!character.InMenu(Menu.EXP_R3))
                return;

            var show = character.res3.res3BarSpeed < 50f;
            _r3_purchText.SetActive(show);
            _r3_speed01Button.SetActive(show);
            _r3_speed1Button.SetActive(show);
        }
    }
}

/*

Canvas/Exp Energy Canvas /Exp Menu 1/Scroll Rect/Content/Energy Speed
    Purchases Text
    Energy Speed 0.1 Button
    Energy Speed 1 Button
    Special Offers

Canvas/Exp Magic Canvas/Exp Menu 1/Scroll Rect/Content/Magic Speed
    Purchases Text
    Energy Speed 0.1 Button
    Energy Speed 1 Button

Canvas/Exp Res 3 Canvas/Res3 Exp Menu/Scroll Rect/Content/Res3 Speed
    Purchases Text
    Res3 Speed 0.1 Button
    Res3 Speed 1 Button

 */
