using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class ColoredItemDropNames
    {
        [HarmonyPostfix,
            HarmonyPatch(typeof(ItemNameDesc), "makeLoot", [typeof(int)]),
            HarmonyPatch(typeof(ItemNameDesc), "makeLevelledLoot"),
            HarmonyPatch(typeof(ItemNameDesc), "makeTitanLoot"),
            HarmonyPatch(typeof(ItemNameDesc), "makeTitanLevelledLoot")]
        private static void ItemNameDesc_makeLoot_postfix(int id, ref string __result)
        {
            var index = 0;

            // when LootDrop spits out quest item names, it does a Substring(40) to skip over "<b><color=blue>[QUEST ITEM]</color></b>\n"
            if ((id >= 278 && id <= 287))
                index = 40;

            var newResult = __result.Insert(index, $"<b><color={Options.Colors.LootItemNames.Value}>") + " </color></b>";
            __result = newResult;
        }
    }
}
