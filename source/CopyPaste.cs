using System;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class CopyPaste
    {
        [HarmonyPostfix, HarmonyPatch(typeof(WishesController), "Start")]
        private static void WishesController_Start_postfix(WishesController __instance)
        {
            Wish copiedWish = null;

            Plugin.OnUpdate += (o, e) =>
            {
                if (!e.Character.InMenu(Menu.Wishes))
                    return;

                if (Input.GetKeyDown(KeyCode.C) && Input.GetKey(KeyCode.LeftControl))
                {
                    copiedWish = e.Character.wishes.wishes[__instance.curSelectedWish];
                    __instance.tooltip.showOverrideTooltip("wish resource allocations copied", 1);
                }

                if (Input.GetKeyDown(KeyCode.V) && Input.GetKey(KeyCode.LeftControl))
                {
                    if (copiedWish == null)
                        return;

                    var character = e.Character;
                    var id = __instance.curSelectedWish;
                    var wish = character.wishes.wishes[id];

                    character.idleEnergy += wish.energy;
                    var amount = Math.Min(copiedWish.energy, character.idleEnergy);
                    character.idleEnergy -= amount;
                    wish.energy = amount;

                    var magic = character.magic;
                    magic.idleMagic += wish.magic;
                    amount = Math.Min(magic.idleMagic, copiedWish.magic);
                    magic.idleMagic -= amount;
                    wish.magic = amount;

                    var res3 = character.res3;
                    res3.idleRes3 += wish.res3;
                    amount = Math.Min(res3.idleRes3, copiedWish.res3);
                    res3.idleRes3 -= amount;
                    wish.res3 = amount;

                    __instance.updateEnergyPodText();
                    __instance.updateMagicPodText();
                    __instance.updateRes3PodText();
                    __instance.updatebyID(id);
                    __instance.updateWishSlotText();
                }
            };
        }
    }
}
