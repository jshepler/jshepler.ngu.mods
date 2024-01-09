using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class ManualCombatMovesVFX
    {
        private static AdventureController ac;
        private static PlayerController pc;
        private static Character character;

        private static ButtonBar bbRegularAttack;
        private static ButtonBar bbStrongAttack;
        private static ButtonBar bbParry;
        private static ButtonBar bbPiercingAttack;
        private static ButtonBar bbUltimateAttack;
        private static ButtonBar bbBlock;
        private static ButtonBar bbDefensiveBuff;
        private static ButtonBar bbHeal;
        private static ButtonBar bbOffensiveBuff;
        private static ButtonBar bbCharge;
        private static ButtonBar bbUltimateBuff;
        private static ButtonBar bbParalyze;
        private static ButtonBar bbHyperRegen;
        private static ButtonBar bbMegaBuff;
        private static ButtonBar bbOhShit;

        private static float paralyzeDuration;
        private static float enemyParalyzeDuration = 0f;

        private static FieldInfo _enemyParalyzeTime = typeof(EnemyAI).GetField("paralyzeTime", BindingFlags.Instance | BindingFlags.NonPublic);
        private static Func<float> EnemyParalyzeTime = () => Math.Max(0f, (float)_enemyParalyzeTime.GetValue(ac.enemyAI));

        private static Func<float, float, float> pctCooldown = (cur, max) => Mathf.Clamp01((max - cur) / max);

        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "Start")]
        private static void AdventureController_Start_postfix(AdventureController __instance)
        {
            ac = __instance;
            pc = ac.playerController;
            character = ac.character;

            bbRegularAttack = new ButtonBar(__instance.regularAttackMove.button)
            {
                IsLocked = () => character.training.attackTraining[0] < 5000
                , CooldownDuration = character.regAttackCooldown
            };

            bbStrongAttack = new ButtonBar(__instance.strongAttackMove.button)
            {
                IsLocked = () => character.training.attackTraining[1] < 10000
                , CooldownDuration = character.strongAttackCooldown
                , IsDisabled = () => pc.strongDisabled
            };

            bbParry = new ButtonBar(__instance.parryMove.button)
            {
                IsLocked = () => character.training.attackTraining[2] < 15000
                , CooldownDuration = character.parryCooldown
                , IsEffectActive = () => pc.isParrying
            };

            bbPiercingAttack = new ButtonBar(__instance.pierceMove.button)
            {
                IsLocked = () => character.training.attackTraining[3] < 20000
                , CooldownDuration = character.pierceAttackCooldown
                , IsDisabled = () => pc.pierceDisabled
            };

            bbUltimateAttack = new ButtonBar(__instance.ultimateAttackMove.button)
            {
                IsLocked = () => character.training.attackTraining[4] < 25000
                , CooldownDuration = character.ultimateAttackCooldown
                , IsDisabled = () => pc.ultimateDisabled
            };

            bbBlock = new ButtonBar(__instance.blockMove.button)
            {
                IsLocked = () => false
                , CooldownDuration = character.blockCooldown
                , EffectDuraction = character.blockDuration
                , EffectTimer = () => pc.blockTime
            };

            bbDefensiveBuff = new ButtonBar(__instance.defenseBuffMove.button)
            {
                IsLocked = () => character.training.defenseTraining[0] < 5000
                , CooldownDuration = character.defenseBuffCooldown
                , EffectDuraction = character.defenseBuffDuration
                , EffectTimer = () => pc.defenseBuffTime
            };

            bbHeal = new ButtonBar(__instance.healMove.button)
            {
                IsLocked = () => character.training.defenseTraining[1] < 10000
                , CooldownDuration = character.healCooldown
                , IsDisabled = () => pc.healDisabled
            };

            bbOffensiveBuff = new ButtonBar(__instance.offenseBuffMove.button)
            {
                IsLocked = () => character.training.defenseTraining[2] < 15000
                , CooldownDuration = character.offenseBuffCooldown
                , EffectDuraction = character.offenseBuffDuration
                , EffectTimer = () => pc.offenseBuffTime
                , IsDisabled = () => pc.offBuffDisabled
            };

            bbCharge = new ButtonBar(__instance.chargeMove.button)
            {
                IsLocked = () => character.training.defenseTraining[3] < 20000
                , CooldownDuration = character.chargeCooldown
                , IsEffectActive = () => pc.chargeFactor > 1f
            };

            bbUltimateBuff = new ButtonBar(__instance.ultimateBuffMove.button)
            {
                IsLocked = () => character.training.defenseTraining[4] < 25000
                , CooldownDuration = character.ultimateBuffCooldown
                , EffectDuraction = character.ultimateBuffDuration
                , EffectTimer = () => pc.ultimateBuffTime
                , IsDisabled = () => pc.ultiBuffDisabled
            };

            bbParalyze = new ButtonBar(__instance.paralyzeMove.button)
            {
                IsLocked = () => !character.allChallenges.hasParalyze()
                , CooldownDuration = character.paralyzeCooldown
                , EffectDuraction = () => enemyParalyzeDuration
                , EffectTimer = () => enemyParalyzeDuration - EnemyParalyzeTime()
            };

            bbHyperRegen = new ButtonBar(__instance.hyperRegenMove.button)
            {
                IsLocked = () => !character.settings.hasHyperRegen
                , CooldownDuration = character.hyperRegenCooldown
                , EffectDuraction = () => 5f // hard-coded in PlayerController.hyperRegen()
                , EffectTimer = () => 5f - pc.hyperRegenTime
            };

            bbMegaBuff = new ButtonBar(__instance.megaBuffMove.button);

            bbOhShit = new ButtonBar(__instance.ohShitMove.button);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerController), "paralyzed")]
        private static void PlayerController_paralyzed_postfix(float time)
        {
            paralyzeDuration = time;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(EnemyAI), "paralyzed")]
        private static void EnemyAI_paralyzed_postfix(float time)
        {
            enemyParalyzeDuration = time;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(RegularAttack), "Update")]
        private static void RegularAttack_Update_postfix(float ___regularAttackTimer)
        {
            bbRegularAttack.Update(___regularAttackTimer);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(StrongAttack), "Update")]
        private static void StrongAttack_Update_postfix(float ___strongAttackTimer)
        {
            bbStrongAttack.Update(___strongAttackTimer);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Parry), "Update")]
        private static void Parry_Update_postfix(float ___parryTimer)
        {
            bbParry.Update(___parryTimer);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PiercingAttack), "Update")]
        private static void PiercingAttack_Update_postfix(float ___attackTimer)
        {
            bbPiercingAttack.Update(___attackTimer);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(UltimateAttack), "Update")]
        private static void UltimateAttack_Update_postfix(float ___ultimateAttackTimer)
        {
            bbUltimateAttack.Update(___ultimateAttackTimer);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Block), "Update")]
        private static void Block_Update_postfix(float ___blockTimer)
        {
            bbBlock.Update(___blockTimer);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(DefenseBuff), "Update")]
        private static void DefenseBuff_Update_postfix(float ___defenseBuffTimer)
        {
            bbDefensiveBuff.Update(___defenseBuffTimer);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Heal), "Update")]
        private static void Heal_Update_postfix(float ___healTimer)
        {
            bbHeal.Update(___healTimer);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(OffenseBuff), "Update")]
        private static void OffenseBuff_Update_postfix(float ___offenseBuffTimer)
        {
            bbOffensiveBuff.Update(___offenseBuffTimer);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Charge), "Update")]
        private static void Charge_Update_postfix(float ___chargeTimer)
        {
            bbCharge.Update(___chargeTimer);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(UltimateBuff), "Update")]
        private static void UltimateBuff_Update_postfix(float ___ultimateBuffTimer)
        {
            bbUltimateBuff.Update(___ultimateBuffTimer);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Paralyze), "Update")]
        private static void Paralyze_Update_postfix(float ___attackTimer)
        {
            bbParalyze.Update(___attackTimer);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(HyperRegen), "Update")]
        private static void HyperRegen_Update_postfix(float ___healTimer)
        {
            bbHyperRegen.Update(___healTimer);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(MegaBuff), "Update")]
        private static void MegaBuff_Update_postfix(MegaBuff __instance, float ___megaBuffDuration, float ___megaBuffTimer)
        {
            // idle mode / locked / buff running
            if (character.adventure.autoattacking
                || character.training.defenseTraining[4] < 25000
                || character.wishes.wishes[8].level < 1)
                bbMegaBuff.Clear();

            // buff running
            else if (___megaBuffDuration <= character.megaBuffDuration())
            {
                bbMegaBuff.SetColor(Plugin.ButtonColor_LightBlue, pctCooldown(___megaBuffDuration, character.megaBuffDuration()));
                ac.megaBuffMove.buttonText.text = $"{character.megaBuffDuration() - ___megaBuffDuration:0.0} s";
            }

            // on cooldown
            else if (!__instance.button.interactable)
            {
                var maxPctCooldown = Mathf.Max(
                    pctCooldown(___megaBuffTimer, character.megaBuffCooldown())
                    , pctCooldown(__instance.offenseBuff.offenseBuffTimer, character.offenseBuffCooldown())
                    , pctCooldown(__instance.defenseBuff.defenseBuffTimer, character.defenseBuffCooldown())
                    , pctCooldown(__instance.ultiBuff.ultimateBuffTimer, character.ultimateBuffCooldown()));

                bbMegaBuff.SetColor(Plugin.ButtonColor_Yellow, maxPctCooldown);
            }

            // paralyzed / disabled
            else if (pc.paralyzeTime > 0f || pc.megaBuffDisabled)
                bbMegaBuff.SetColor(Plugin.ButtonColor_Red);

            // GCD
            else if (!pc.canUseMove)
                bbMegaBuff.SetColor(Color.grey);

            // ready
            else
                bbMegaBuff.SetColor(Plugin.ButtonColor_Green);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(OhShit), "Update")]
        private static void OhShit_Update_postfix(OhShit __instance)
        {
            // idle mode / locked
            if (character.adventure.autoattacking
                || character.wishes.wishes[58].level < 1
                || !character.allChallenges.hasParalyze()
                || character.training.defenseTraining[1] < 10000
                || !character.settings.hasHyperRegen)
                bbOhShit.Clear();

            // on cooldown
            else if (!__instance.button.interactable)
            {
                var maxPctCooldown = Mathf.Max(
                    pctCooldown(__instance.hyperRegen.healTimer, character.hyperRegenCooldown())
                    , pctCooldown(__instance.heal.healTimer, character.healCooldown())
                    , pctCooldown(__instance.paralyze.attackTimer, character.paralyzeCooldown()));

                bbOhShit.SetColor(Plugin.ButtonColor_Yellow, maxPctCooldown);
            }

            // paralyzed
            else if (pc.paralyzeTime > 0f)
                bbOhShit.SetColor(Plugin.ButtonColor_Red);

            // GCD
            else if (!pc.canUseMove)
                bbOhShit.SetColor(Color.grey);

            // ready
            else
                bbOhShit.SetColor(Plugin.ButtonColor_Green);
        }

        private class ButtonBar
        {
            private Image _image;
            private Text _text;

            internal Func<bool> IsLocked = () => false;
            internal Func<bool> IsDisabled = () => false;

            internal Func<bool> IsEffectActive = () => false;
            internal Func<float> EffectDuraction = () => -1f;
            internal Func<float> EffectTimer = () => -1f;

            internal Func<float> CooldownDuration = () => 0f;

            internal ButtonBar(Button btn)
            {
                _text = btn.GetComponentInChildren<Text>();
                var bar = new GameObject();
                var rt = bar.AddComponent<RectTransform>();

                rt.transform.SetParent(btn.transform);
                _text.transform.SetAsLastSibling();

                rt.localScale = Vector3.one;
                rt.anchoredPosition = new Vector2(0f, 0f);
                rt.sizeDelta = new Vector2(92, 22);

                var tex = new Texture2D(Mathf.CeilToInt(rt.rect.width), Mathf.CeilToInt(rt.rect.height), TextureFormat.RGB24, false);
                for (var x = 0; x < tex.width; x++)
                    for (var y = 0; y < tex.height; y++)
                        tex.SetPixel(x, y, Color.white);
                tex.Apply();

                _image = bar.AddComponent<Image>();
                _image.sprite = Sprite.Create(tex, rt.rect, new Vector2(0.5f, 0.5f));
                _image.type = Image.Type.Filled;
                _image.fillMethod = Image.FillMethod.Horizontal;
                _image.fillOrigin = 0;
                _image.fillAmount = 0f;
            }

            internal void Update(float cooldownTimer)
            {
                var effTimer = EffectTimer();
                var effDuration = EffectDuraction();

                // idle mode / locked
                if (character.adventure.autoattacking || IsLocked())
                    Clear();

                // disabled
                else if (IsDisabled())
                    SetColor(Plugin.ButtonColor_Red);

                // effect running
                else if (IsEffectActive() || (effTimer > 0 && effTimer < effDuration))
                {
                    if (effTimer > 0f && effTimer < effDuration)
                    {
                        SetColor(Plugin.ButtonColor_LightBlue, pctCooldown(effTimer, effDuration));
                        _text.text = $"{effDuration - effTimer:0.0} s";
                    }
                    else
                        SetColor(Plugin.ButtonColor_LightBlue);
                }

                // on cooldown
                else if (cooldownTimer <= CooldownDuration())
                    SetColor(Plugin.ButtonColor_Yellow, pctCooldown(cooldownTimer, CooldownDuration()));

                // paralyzed
                else if (pc.paralyzeTime > 0f)
                {
                    SetColor(Plugin.ButtonColor_Red, Mathf.Clamp01(pc.paralyzeTime / paralyzeDuration));
                    _text.text = $"{pc.paralyzeTime:0.0} s";
                }

                // GCD
                else if (!pc.canUseMove)
                    SetColor(Color.grey);

                // ready
                else
                    SetColor(Plugin.ButtonColor_Green);
            }

            internal void Clear()
            {
                _image.fillAmount = 0f;
            }

            internal void SetColor(Color color, float fillAmount = 1f)
            {
                _image.color = color;
                _image.fillAmount = fillAmount;
            }
        }
    }
}
