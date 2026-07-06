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
        private enum Stages { NotRunning, WaitingForSpawn, Fighting, Healing }
        private static Stages _stage = Stages.NotRunning;

        private static AdventureController Controller => Plugin.Character.adventureController;
        private static float _curHP => Plugin.Character.adventure.curHP;
        private static float _maxHP => Plugin.Character.totalAdvHP();
        private static float _hpStart;
        private static float _hpMaxLoss;

        private static int _snipeZone = -1;
        private static void GotoSafe() => Controller.zoneSelector.changeZone(-1);
        private static void GotoSnipe() => Controller.zoneSelector.changeZone(_snipeZone);

        [HarmonyPrepare]
        private static void prep(MethodBase original)
        {
            if (original != null)
                return;

            Plugin.OnUpdate += Update;
            Plugin.OnOfflineProgressionComplete += (o, e) => Rebirth_engage_bool_prefix();
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
            _stage = Stages.NotRunning;
            _snipeZone = -1;
        }

        [HarmonyPostfix
            , HarmonyPatch(typeof(IdleAttack), "setToggle")
            , HarmonyPatch(typeof(IdleAttack), "checkIdleAttackState")]
        private static void IdleAttack_setToggle_postfix(IdleAttack __instance)
        {
            if (_stage != Stages.NotRunning)
                Controller.idleAttackMove.Border.color = Color.red;
        }

        private static void ToggleRunning()
        {
            // ignore ITOPOD
            if (Controller.zone == 1000)
                return;

            if (_stage == Stages.NotRunning)
            {
                _stage = Stages.WaitingForSpawn;
                _snipeZone = Controller.zone;
                _hpMaxLoss = 0;
            }
            else
            {
                _stage = Stages.NotRunning;
                _snipeZone = -1;
            }

            // sets Idle Mode button visuals - hooked into above to set red border
            Controller.idleAttackMove.checkIdleAttackState();
        }

        private static void Update(object sender, EventArgs e)
        {
            switch (_stage)
            {
                case Stages.NotRunning:
                    return;

                case Stages.WaitingForSpawn:
                    waitingForSpawn();
                    break;

                case Stages.Fighting:
                    fighting();
                    break;

                case Stages.Healing:
                    healing();
                    break;
            }
        }

        private static bool _inSpawnDelay = false; // short delay so player can see the enemy being skipped
        private static void waitingForSpawn()
        {
            if (Controller.zone != _snipeZone)
                GotoSnipe();

            if (Controller.fightInProgress)
            {
                if (_inSpawnDelay)
                    return;

                if (SkipCurrentEnemy())
                    Plugin.BeginCoroutine(WaitThenGotoSafeZone()); // changing zones resets fightInProgress
                else
                {
                    _hpStart = _curHP;
                    _stage = Stages.Fighting;
                }
            }

            // else if fight not in progress, enemy hasn't spawned yet - do nothing
        }

        private static void fighting()
        {
            if (Controller.fightInProgress)
                return;

            var hpLoss = Math.Max(0, _hpStart - _curHP);
            if (hpLoss > _hpMaxLoss)
                _hpMaxLoss = hpLoss;

            if (_curHP <= _hpMaxLoss)// _maxHP * 0.5f)
                _stage = Stages.Healing;
            else
                _stage = Stages.WaitingForSpawn;
        }

        private static void healing()
        {
            if (Controller.zone != -1)
                GotoSafe();

            if (_curHP >= _maxHP)
                _stage = Stages.WaitingForSpawn;
        }

        private static bool SkipCurrentEnemy()
        {
            // snipe target enemy when in target zone
            var targetZone = Options.AutoSnipe.TargetZone.Value - 2;
            var targetEnemy = Options.AutoSnipe.TargetEnemy.Value;

            if (targetZone >= 0 && targetEnemy > 0 && Controller.zone == targetZone)
                return Controller.currentEnemy.spriteID != targetEnemy;

            return Controller.currentEnemy.enemyType != enemyType.boss;
        }

        private static WaitForSeconds _delay = new WaitForSeconds(.2f);
        private static IEnumerator WaitThenGotoSafeZone()
        {
            _inSpawnDelay = true;
            yield return _delay;
            GotoSafe();
            _inSpawnDelay = false;
        }
    }
}
