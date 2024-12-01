using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class WishOrderTTL
    {
        [HarmonyPrefix, HarmonyPatch(typeof(WishesController), "advanceOrderType")]
        private static bool WishesController_advanceOrderType_prefix(WishesController __instance)
        {
            var wishes = __instance.character.wishes;

            var curValue = (int)wishes.orderType;
            var newValue = (curValue + 1) % 4;
            wishes.orderType = (orderWish)newValue;

            __instance.onOrderChange();

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(WishesController), "updateOrderTypeText")]
        private static bool WishesController_updateOrderTypeText_prefix(WishesController __instance)
        {
            if (!__instance.character.InMenu(Menu.Wishes))
                return false;

            __instance.orderTypeText.text = __instance.character.wishes.orderType switch
            {
                orderWish.Default => "Order By:\n<b>DEFAULT</b>",
                orderWish.SpeedCost => "Order By:\n<b>BASE COST</b>",
                orderWish.totalCost => "Order By:\n<b>TOTAL COST</b>",
                _ => "Order By:\n<b>TTL</b>"
            };

            return false;
        }

        private static FieldInfo _dictDouble = typeof(WishesController).GetField("dictDouble", BindingFlags.Instance | BindingFlags.NonPublic);

        [HarmonyPrefix, HarmonyPatch(typeof(WishesController), "orderList")]
        private static bool WishesController_orderList_prefix(WishesController __instance)
        {
            if ((int)__instance.character.wishes.orderType < 3)
                return true;

            var byTTL = __instance.curValidUpgradesList
                .Select(id => new { id, ttl = TimeToNextLevel(id) })
                .OrderBy(w => w.ttl);

            _dictDouble.SetValue(__instance, byTTL.ToDictionary(w => w.id, w => w.ttl));
            __instance.curValidUpgradesList = byTTL.Select(w => w.id).ToList();

            return false;
        }

        private static double TimeToNextLevel(int id)
        {
            var character = Plugin.Character;
            var controller = character.wishesController;

            var wish = Wishes.AllWishes[id];
            if (wish.Level >= wish.MaxLevel)
                return 0.0;

            var ppt = controller.rawProgressPerTickMax(id);
            if (ppt < 1e-8f)
                return double.MaxValue;

            var ttl = (1.0 - wish.Progress) / ppt / 50.0;
            return ttl;
        }
    }
}
