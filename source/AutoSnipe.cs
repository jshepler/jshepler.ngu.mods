using System;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class AutoSnipe
    {
        private static bool _running = false;
        private static bool _waiting = false;
        private static int _snipeZone = -1;
        private static WaitForSeconds _wait = new WaitForSeconds(.2f);

        private static AdventureController Controller => Plugin.Character.adventureController;
        private static void GotoSafe() => Controller.zoneSelector.changeZone(-1);
        private static void GotoSnipe() => Controller.zoneSelector.changeZone(_snipeZone);
        private static float CurrentHP => Plugin.Character.adventure.curHP;
        private static float MaxHP => Plugin.Character.totalAdvHP();


        [HarmonyPrepare]
        private static void prep(MethodBase original)
        {
            if (original != null)
                return;

            Plugin.OnUpdate += Update;
            Plugin.OnSaveLoaded += (o, e) => Rebirth_engage_bool_prefix();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "Start")]
        private static void AdventureController_Start_postfix(AdventureController __instance)
        {
            __instance.idleAttackMove.gameObject.AddComponent<ClickHandlerComponent>()
                .OnRightClick(e => ToggleRunning());
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Rebirth), "engage", typeof(bool))]
        private static void Rebirth_engage_bool_prefix()
        {
            _running = false;
            _snipeZone = -1;
        }

        [HarmonyPostfix
            , HarmonyPatch(typeof(IdleAttack), "setToggle")
            , HarmonyPatch(typeof(IdleAttack), "checkIdleAttackState")]
        private static void IdleAttack_setToggle_postfix(IdleAttack __instance)
        {
            if (_running)
                Controller.idleAttackMove.Border.color = Color.red;
        }

        private static void ToggleRunning()
        {
            _running = !_running;
            _snipeZone = _running ? Controller.zone : -1;
            Controller.idleAttackMove.checkIdleAttackState();
        }

        private static void Update(object sender, EventArgs e)
        {
            // F1 pressed this frame, only in Adventure screen, not in safe zone, not in tower
            if (Input.GetKeyDown(KeyCode.F1) && Plugin.Character.menuID == 3 && Controller.zone >= 0 && Controller.zone < 1000)
                ToggleRunning();

            if (!_running)
                return;

            if (Controller.zone == -1 && CurrentHP < MaxHP)
                return;

            if (!Controller.fightInProgress && Controller.zone == _snipeZone && CurrentHP < MaxHP * 0.5)
                GotoSafe();

            if (Controller.zone != _snipeZone && CurrentHP >= MaxHP)
                GotoSnipe();

            if (Controller.fightInProgress && SkipCurrentEnemy())
            {
                Plugin.Character.StartCoroutine(WaitBeforeGoingToSafeZone());
                _waiting = true;
            }
        }

        private static bool SkipCurrentEnemy()
        {
            if (_waiting)
                return false;

            // snipe target enemy when in target zone
            var targetZone = Options.AutoSnipe.TargetZone.Value - 2;
            var targetEnemy = Options.AutoSnipe.TargetEnemy.Value;

            if (targetZone >= 0 && targetEnemy > 0 && Controller.zone == targetZone)
                return Controller.currentEnemy.spriteID != targetEnemy;

            return Controller.currentEnemy.enemyType != enemyType.boss;
        }

        private static IEnumerator WaitBeforeGoingToSafeZone()
        {
            yield return _wait;
            GotoSafe();
            _waiting = false;
        }
    }
}
