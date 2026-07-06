using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine.Events;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class DisableAPConfirmation
    {
        private static bool _enabled = Options.Experimental.DisableAPConfirmation.Value;
        private static FieldInfo _yesAction = typeof(ArbitraryController).GetField("yesAction", BindingFlags.Instance | BindingFlags.NonPublic);
        private static MethodInfo _invoke = typeof(UnityAction).GetMethod("Invoke");

        [HarmonyTranspiler,
            HarmonyPatch(typeof(ArbitraryController), "startEnergyPotion1AP"),
            HarmonyPatch(typeof(ArbitraryController), "startEnergyPotion2AP"),
            HarmonyPatch(typeof(ArbitraryController), "startEnergyPotion3AP"),
            HarmonyPatch(typeof(ArbitraryController), "startMagicPotion1AP"),
            HarmonyPatch(typeof(ArbitraryController), "startMagicPotion2AP"),
            HarmonyPatch(typeof(ArbitraryController), "startMagicPotion3AP"),
            HarmonyPatch(typeof(ArbitraryController), "startRes3Potion1AP"),
            HarmonyPatch(typeof(ArbitraryController), "startRes3Potion2AP"),
            HarmonyPatch(typeof(ArbitraryController), "startRes3Potion3AP"),
            HarmonyPatch(typeof(ArbitraryController), "startLootCharm1AP"),
            HarmonyPatch(typeof(ArbitraryController), "startEnergyBarBar1AP"),
            HarmonyPatch(typeof(ArbitraryController), "startMagicBarBar1AP"),
            HarmonyPatch(typeof(ArbitraryController), "startLootFilterAP"),
            HarmonyPatch(typeof(ArbitraryController), "startAutoMergeBoostAP"),
            HarmonyPatch(typeof(ArbitraryController), "startInstaTrainingAP"),
            HarmonyPatch(typeof(ArbitraryController), "start500ExpAP"),
            HarmonyPatch(typeof(ArbitraryController), "start200ExpAP"),
            HarmonyPatch(typeof(ArbitraryController), "start2KExpAP"),
            HarmonyPatch(typeof(ArbitraryController), "startHeartAP"),
            HarmonyPatch(typeof(ArbitraryController), "startCustomPercent1AP"),
            HarmonyPatch(typeof(ArbitraryController), "startCustomPercent2AP"),
            HarmonyPatch(typeof(ArbitraryController), "startCustomIdlePercent1AP"),
            HarmonyPatch(typeof(ArbitraryController), "startRes3Percent1AP"),
            HarmonyPatch(typeof(ArbitraryController), "startRes3Percent2AP"),
            HarmonyPatch(typeof(ArbitraryController), "startRes3IdlePercent1AP"),
            HarmonyPatch(typeof(ArbitraryController), "startYellowHeartAP"),
            HarmonyPatch(typeof(ArbitraryController), "startInventoryAP"),
            HarmonyPatch(typeof(ArbitraryController), "start10InventoryAP"),
            HarmonyPatch(typeof(ArbitraryController), "startStarterPackAP"),
            HarmonyPatch(typeof(ArbitraryController), "startAcc4AP"),
            HarmonyPatch(typeof(ArbitraryController), "startAcc5AP"),
            HarmonyPatch(typeof(ArbitraryController), "startAcc6AP"),
            HarmonyPatch(typeof(ArbitraryController), "startAcc7AP"),
            HarmonyPatch(typeof(ArbitraryController), "startAcc8AP"),
            HarmonyPatch(typeof(ArbitraryController), "startAcc9AP"),
            HarmonyPatch(typeof(ArbitraryController), "startPoop1AP"),
            HarmonyPatch(typeof(ArbitraryController), "startPoop10AP"),
            HarmonyPatch(typeof(ArbitraryController), "startPoop100AP"),
            HarmonyPatch(typeof(ArbitraryController), "startYggReminderAP"),
            HarmonyPatch(typeof(ArbitraryController), "startExtendedSpinBankAP"),
            HarmonyPatch(typeof(ArbitraryController), "startLoadoutSlotAP"),
            HarmonyPatch(typeof(ArbitraryController), "startBeardAP"),
            HarmonyPatch(typeof(ArbitraryController), "startCubeFilterAP"),
            HarmonyPatch(typeof(ArbitraryController), "startLootCharm2AP"),
            HarmonyPatch(typeof(ArbitraryController), "startHeartBrown"),
            HarmonyPatch(typeof(ArbitraryController), "startDaycareSpeedAP"),
            HarmonyPatch(typeof(ArbitraryController), "startHeartGreenAP"),
            HarmonyPatch(typeof(ArbitraryController), "startPill1AP"),
            HarmonyPatch(typeof(ArbitraryController), "startPill10AP"),
            HarmonyPatch(typeof(ArbitraryController), "startPill100AP"),
            HarmonyPatch(typeof(ArbitraryController), "startHeartBlueAP"),
            HarmonyPatch(typeof(ArbitraryController), "startLazyITOPODAP"),
            HarmonyPatch(typeof(ArbitraryController), "startDiggerSlotAP"),
            HarmonyPatch(typeof(ArbitraryController), "startMacguffinSlotAP"),
            HarmonyPatch(typeof(ArbitraryController), "startHeartPurpleAP"),
            HarmonyPatch(typeof(ArbitraryController), "startHeartGreyAP"),
            HarmonyPatch(typeof(ArbitraryController), "startMacguffinBooster1AP"),
            HarmonyPatch(typeof(ArbitraryController), "startBeastButter1AP"),
            HarmonyPatch(typeof(ArbitraryController), "startBeastButter10AP"),
            HarmonyPatch(typeof(ArbitraryController), "startBeastButter100AP"),
            HarmonyPatch(typeof(ArbitraryController), "startQuestLightAP"),
            HarmonyPatch(typeof(ArbitraryController), "startFasterQuests1AP"),
            HarmonyPatch(typeof(ArbitraryController), "startExtendedQuestBankAP"),
            HarmonyPatch(typeof(ArbitraryController), "startHeartOrangeAP"),
            HarmonyPatch(typeof(ArbitraryController), "start25ppAP"),
            HarmonyPatch(typeof(ArbitraryController), "start100ppAP"),
            HarmonyPatch(typeof(ArbitraryController), "start500ppAP"),
            HarmonyPatch(typeof(ArbitraryController), "startAutoNukeAP"),
            HarmonyPatch(typeof(ArbitraryController), "startDaycareArtAP"),
            HarmonyPatch(typeof(ArbitraryController), "startNGUCapModifierAP"),
            HarmonyPatch(typeof(ArbitraryController), "startRes3NameGeneratorAP"),
            HarmonyPatch(typeof(ArbitraryController), "startFasterWishAP"),
            HarmonyPatch(typeof(ArbitraryController), "startInvMergeSlotAP"),
            HarmonyPatch(typeof(ArbitraryController), "startHeartPinkAP"),
            HarmonyPatch(typeof(ArbitraryController), "startAdvLightAP"),
            HarmonyPatch(typeof(ArbitraryController), "startAdvAdvancerAP"),
            HarmonyPatch(typeof(ArbitraryController), "startGoToQuestAP"),
            HarmonyPatch(typeof(ArbitraryController), "startDeckSlotAP"),
            HarmonyPatch(typeof(ArbitraryController), "startMayoGenAP"),
            HarmonyPatch(typeof(ArbitraryController), "startTagSlotAP"),
            HarmonyPatch(typeof(ArbitraryController), "startMayoSpeedConsumableAP"),
            HarmonyPatch(typeof(ArbitraryController), "startCardTierConsumableAP"),
            HarmonyPatch(typeof(ArbitraryController), "startHeartRainbowAP"),
            HarmonyPatch(typeof(ArbitraryController), "startFoilUnlockAP")]
        private static IEnumerable<CodeInstruction> transpiler(IEnumerable<CodeInstruction> instructions)
        {
            if (!_enabled)
                return instructions;

            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Stfld, _yesAction))
                .InsertAndAdvance(Transpilers.EmitDelegate(autoSave))
                .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Callvirt, _invoke))
                .Insert(new CodeInstruction(OpCodes.Ret))
                .Advance(-5)
                .RemoveInstructions(1);

            return cm.InstructionEnumeration();
        }

        private static void autoSave()
        {
            AutoSaves.DoSave("AP_Purchase");
        }
    }
}
