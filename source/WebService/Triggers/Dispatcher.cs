using System;
using System.Net;

namespace jshepler.ngu.mods.WebService.Triggers
{
    internal class Dispatcher
    {
        private static bool InManualFight => Plugin.Character.adventureController.fightInProgress && !Plugin.Character.adventure.autoattacking;

        internal static Action HandleRequest(HttpListenerContext context, string trigger)
        {
            if (!TriggerConfig.RemoteTriggersEnabled)
                return () => Plugin.ShowNotification("remote triggers disabled");

            switch (trigger)
            {
                case "autoboost":
                    return autoBoost();

                case "automerge":
                    return autoMerge();

                case "tossgold":
                    return tossGold();

                case "fightboss":
                    return fightBoss();

                case "kitty":
                    return kitty();

                default:
                    return () => Plugin.ShowNotification($"unknown trigger: {trigger}");
            }
        }

        private static Action autoBoost()
        {
            if(!TriggerConfig.AutoBoostEnabled)
                return () => Plugin.ShowNotification("trigger: autoboost disabled");

            return () =>
            {
                Plugin.ShowNotification("trigger: autoboost");
                Plugin.Character.inventoryController.autoBoost();
            };
        }

        private static Action autoMerge()
        {
            if(!TriggerConfig.AutoMergeEnabled)
                return () => Plugin.ShowNotification("trigger: automerge disabed");

            return () =>
            {
                Plugin.ShowNotification("trigger: automerge");
                Plugin.Character.inventoryController.autoMerge();
            };
        }

        private static Action tossGold()
        {
            if (!TriggerConfig.TossGoldEnabled)
                return () => Plugin.ShowNotification("trigger: toss gold disabed");

            if (InManualFight)
                return () => Plugin.ShowNotification("trigger: toss gold ignored - manual adventure fight in progress");

            if (TossGold.IsRunning)
                return () => Plugin.ShowNotification("trigger: toss gold ignored - already running");

            if (!Plugin.Character.pitController.canToss())
                return () => Plugin.ShowNotification("trigger: toss gold ignored - pit not ready");

            return () =>
            {
                Plugin.ShowNotification("trigger: toss gold");
                Plugin.Character.StartCoroutine(TossGold.Run());
            };
        }

        private static Action fightBoss()
        {
            if (!TriggerConfig.FightBossEnabled)
                return () => Plugin.ShowNotification("trigger: fight boss disabled");

            if (InManualFight)
                return () => Plugin.ShowNotification("trigger: fight boss ignored - manual adventure fight in progress");

            if (FightBoss.IsRunning)
                return () => Plugin.ShowNotification("trigger: fight boss ignored - already running");

            if (Plugin.Character.bossController.isFighting)
                return () => Plugin.ShowNotification("trigger: fight boss ignored - fight in progress");

            if (!mods.FightBoss.CanFight)
                return () => Plugin.ShowNotification("trigger: fight boss ignored - boss not beatable");

            return () =>
            {
                Plugin.ShowNotification("trigger: fightboss");
                Plugin.Character.StartCoroutine(FightBoss.Run());
            };
        }

        private static Action kitty()
        {
            if (!TriggerConfig.KittyEnabled)
                return () => Plugin.ShowNotification("trigger: kitty ignored - disabled");

            if (Kitty.IsRunning)
                return () => Plugin.ShowNotification("trigger: kitty ignored - already running");

            return () =>
            {
                Plugin.ShowNotification("trigger: kitty");
                Plugin.Character.StartCoroutine(Kitty.Run());
            };
        }
    }
}
