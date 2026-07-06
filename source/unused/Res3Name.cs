using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class Res3Name
    {
		[HarmonyPrefix, HarmonyPatch(typeof(GenericInputSetting), "parseRes3NameInput")]
		private static bool GenericInputSetting_parseRes3NameInput_prefix(GenericInputSetting __instance)
		{
			var character = __instance.character;
			if (character.res3.capRes3 < 10000)
				return true;

			var name = __instance.input.text;
			if (string.IsNullOrEmpty(name))
				name = "Butts";

			character.res3.res3Name = name;
			__instance.updateInputText();

			return false;
        }
    }
}
