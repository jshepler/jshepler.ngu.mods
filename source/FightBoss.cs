using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class FightBoss
    {
        public static bool CanFight = false;
        public static bool CanNuke = false;

        private static double _dmgDonePct;
        private static double _dmgTakenPct;
        private static Image _image;
        private static Text _text;

        private static GameObject _stopButton;

        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "Start")]
        private static void ButtonShower_Start_postfix(ButtonShower __instance)
        {
            _stopButton = GameObject.Find("Canvas/Boss Menu Canvas/Boss Menu/Stop Button");

            var btn = __instance.boss;
            _text = btn.GetComponentInChildren<Text>();

            var pb = new GameObject();

            var rt = pb.AddComponent<RectTransform>();
            rt.transform.SetParent(btn.transform);
            _text.transform.SetAsLastSibling();

            rt.localScale = Vector3.one;
            rt.anchoredPosition = new Vector2(0f, 0f);
            rt.sizeDelta = new Vector2(106, 20);

            _image = pb.AddComponent<Image>();
            var tex = new Texture2D(Mathf.CeilToInt(rt.rect.width), Mathf.CeilToInt(rt.rect.height), TextureFormat.Alpha8, false);
            _image.sprite = Sprite.Create(tex, rt.rect, new Vector2(0.5f, 0.5f));

            _image.type = Image.Type.Filled;
            _image.fillMethod = Image.FillMethod.Horizontal;
            _image.fillOrigin = 0;
            _image.fillAmount = 1f;

            Plugin.OnUpdate += OnUpdate;
            Plugin.onGUI += OnGUI;

            btn.gameObject.AddComponent<ClickHandlerComponent>()
                .OnRightClick(e => Plugin.Character.StartCoroutine(RunFight()));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AttackDefense), "updateAttackDef")]
        private static void AttackDefense_updateAttackDef_postfix(AttackDefense __instance)
        {
            var character = __instance.character;

            if (character.challenges.blindChallenge.inChallenge)
                return;

            __instance.attackText.text += $"\n(my dmg / boss hp): {_dmgDonePct:f1}%";
            __instance.defenseText.text += $"\n(boss dmg / my hp): {_dmgTakenPct:f1}%";
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(ButtonShower), "updateButtons")]
        private static IEnumerable<CodeInstruction> ButtonShower_updateButtons_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var bossTextField = typeof(ButtonShower).GetField("bossText", BindingFlags.Instance | BindingFlags.NonPublic);
            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldfld, bossTextField))
                .RemoveInstructionsWithOffsets(-1, 2);

            return cm.InstructionEnumeration();
        }

        private static void OnUpdate(object sender, CharacterEventArgs e)
        {
            var character = Plugin.Character;

            _dmgDonePct = 0.0;
            if (character.attack > character.bossDefense)
            {
                _dmgDonePct = (character.attack - character.bossDefense) / character.bossCurHP * 100.0;
                if (_dmgDonePct > 100.0)
                    _dmgDonePct = 100.0;
            }

            _dmgTakenPct = 0.0;
            if (character.defense < character.bossAttack)
            {
                _dmgTakenPct = (character.bossAttack - character.defense) / character.curHP * 100.0;
                if (_dmgTakenPct > 100.0)
                    _dmgTakenPct = 100.0;
            }

            // taken from BossController.nukeBosses() to know if boss is nukable
            CanNuke = (character.attack / 5.0 > character.bossDefense && character.defense / 5.0 > character.bossAttack);
            CanFight = (_dmgDonePct > _dmgTakenPct);

            if (Input.GetKeyDown(KeyCode.B))
                character.StartCoroutine(RunFight());
        }

        private static void OnGUI(object sender, CharacterEventArgs e)
        {
            var nuking = Plugin.Character.bossController.nukeBoss;
            var fighting = Plugin.Character.bossController.isFighting;

            if (Plugin.Character.challenges.blindChallenge.inChallenge)
            {
                _image.fillAmount = 0f;
                //_image.color = Color.white;
                _text.text = "Fight Boss";
            }
            else if (fighting || nuking)
            {
                _image.color = nuking ? Plugin.ButtonColor_Green : Color.red;
                _image.fillAmount = Plugin.Character.bossController.bossHPBar.value;
                _text.text = $"Boss {Plugin.Character.bossID + 1}";
            }
            else if (CanNuke || CanFight)
            {
                _image.fillAmount = 1f;
                _image.color = CanNuke ? Plugin.ButtonColor_Green : Plugin.ButtonColor_Yellow;
                _text.text = "Fight Boss";
            }
            else
            {
                _image.fillAmount = 0f;
                _text.text = "Fight Boss";
            }
        }

        private static WaitUntil _waitUntilNotNuking = new WaitUntil(() => Plugin.Character.bossController.nukeBoss == false);
        private static WaitUntil _waitUntilNotFighting = new WaitUntil(() => Plugin.Character.bossController.isFighting == false);
        internal static IEnumerator RunFight()
        {
            if (CanNuke)
            {
                Plugin.Character.bossController.startNuke();
                yield return _waitUntilNotNuking;
            }

            if (CanFight)
            {
                _stopButton.SetActive(true);
                Plugin.Character.bossController.beginFight();

                yield return _waitUntilNotFighting;
                _stopButton.SetActive(false);
            }
        }
    }
}
