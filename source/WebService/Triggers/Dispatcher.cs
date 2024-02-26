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
                return () => Plugin.ShowOverrideNotification("remote triggers disabled");

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
                    return () => Plugin.ShowOverrideNotification($"unknown trigger: {trigger}");
            }
        }

        internal static Action HandleTwitchReward(Twitch.Reward reward)
        {
            if (!TriggerConfig.RemoteTriggersEnabled)
                return () => Plugin.ShowOverrideNotification("remote triggers disabled");

            if (reward.title == Options.Twitch.RewardTriggers.Merge.Value)
                return autoMerge();

            if (reward.title == Options.Twitch.RewardTriggers.Boost.Value)
                return autoBoost();

            if (reward.title == Options.Twitch.RewardTriggers.MergeBoost.Value)
                return () =>
                {
                    autoMerge()();
                    autoBoost()();
                };

            if (reward.title == Options.Twitch.RewardTriggers.FightBoss.Value)
                return fightBoss();

            if (reward.title == Options.Twitch.RewardTriggers.TossGold.Value)
                return tossGold();

            if (reward.title == Options.Twitch.RewardTriggers.Kitty.Value)
                return kitty();

            return () => Plugin.ShowOverrideNotification($"TWITCH: no match found for \"{reward.title}\"");
        }

        internal static Action autoBoost()
        {
            if(!TriggerConfig.AutoBoostEnabled)
                return () => Plugin.ShowOverrideNotification("trigger: autoboost disabled");

            return () =>
            {
                Plugin.ShowOverrideNotification("trigger: autoboost");
                Plugin.Character.inventoryController.autoBoost();
            };
        }

        internal static Action autoMerge()
        {
            if(!TriggerConfig.AutoMergeEnabled)
                return () => Plugin.ShowOverrideNotification("trigger: automerge disabed");

            return () =>
            {
                Plugin.ShowOverrideNotification("trigger: automerge");
                Plugin.Character.inventoryController.autoMerge();
            };
        }

        internal static Action tossGold()
        {
            if (!TriggerConfig.TossGoldEnabled)
                return () => Plugin.ShowOverrideNotification("trigger: toss gold disabed");

            if (InManualFight)
                return () => Plugin.ShowOverrideNotification("trigger: toss gold ignored - manual adventure fight in progress");

            if (TossGold.IsRunning)
                return () => Plugin.ShowOverrideNotification("trigger: toss gold ignored - already running");

            if (!Plugin.Character.pitController.canToss())
                return () => Plugin.ShowOverrideNotification("trigger: toss gold ignored - pit not ready");

            return () =>
            {
                Plugin.ShowOverrideNotification("trigger: toss gold");
                Plugin.Character.StartCoroutine(TossGold.Run());
            };
        }

        internal static Action fightBoss()
        {
            if (!TriggerConfig.FightBossEnabled)
                return () => Plugin.ShowOverrideNotification("trigger: fight boss disabled");

            if (InManualFight)
                return () => Plugin.ShowOverrideNotification("trigger: fight boss ignored - manual adventure fight in progress");

            if (FightBoss.IsRunning)
                return () => Plugin.ShowOverrideNotification("trigger: fight boss ignored - already running");

            if (Plugin.Character.bossController.isFighting)
                return () => Plugin.ShowOverrideNotification("trigger: fight boss ignored - fight in progress");

            if (!mods.FightBoss.CanFight)
                return () => Plugin.ShowOverrideNotification("trigger: fight boss ignored - boss not beatable");

            return () =>
            {
                Plugin.ShowOverrideNotification("trigger: fightboss");
                Plugin.Character.StartCoroutine(FightBoss.Run());
            };
        }

        internal static Action kitty()
        {
            if (!TriggerConfig.KittyEnabled)
                return () => Plugin.ShowOverrideNotification("trigger: kitty ignored - disabled");

            if (Kitty.IsRunning)
                return () => Plugin.ShowOverrideNotification("trigger: kitty ignored - already running");

            return () =>
            {
                Plugin.ShowOverrideNotification("trigger: kitty");
                Plugin.Character.StartCoroutine(Kitty.Run());
            };
        }
    }
}
