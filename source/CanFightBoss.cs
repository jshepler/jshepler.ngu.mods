using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class CanFightBoss
    {
        public static bool CanFight = false;
        public static bool CanNuke = false;

        [HarmonyPostfix, HarmonyPatch(typeof(AttackDefense), "updateAttackDef")]
        private static void AttackDefense_updateAttackDef_postfix(AttackDefense __instance)
        {
            var character = __instance.character;
            var button = character.buttons.boss;

            if (character.challenges.blindChallenge.inChallenge)
            {
                button.image.color = Color.white;
                return;
            }

            var dmgDonePct = 0.0;
            if (character.attack > character.bossDefense)
            {
                dmgDonePct = (character.attack - character.bossDefense) / character.bossCurHP * 100.0;
                if (dmgDonePct > 100.0)
                    dmgDonePct = 100.0;
            }

            var dmgTakenPct = 0.0;
            if (character.defense < character.bossAttack)
            {
                dmgTakenPct = (character.bossAttack - character.defense) / character.curHP * 100.0;
                if (dmgTakenPct > 100.0)
                    dmgTakenPct = 100.0;
            }

            __instance.attackText.text += $"\n(my dmg / boss hp): {dmgDonePct:f1}%";
            __instance.defenseText.text += $"\n(boss dmg / my hp): {dmgTakenPct:f1}%";

            // taken from BossController.nukeBosses() to know if boss is nukable
            CanNuke = (character.attack / 5.0 > character.bossDefense && character.defense / 5.0 > character.bossAttack);
            CanFight = (dmgDonePct > dmgTakenPct);

            if (CanNuke)
                button.image.color = Plugin.ButtonColor_Green;

            else if (CanFight)
                button.image.color = Plugin.ButtonColor_Yellow;

            else
                button.image.color = Color.white;
        }
    }
}
