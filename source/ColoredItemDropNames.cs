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
            __result = $"<b><color={Options.Colors.LootItemNames.Value}>{__result}</color></b>";
        }
    }
}
