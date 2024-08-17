using System;
using System.Linq;
using System.Net;
using SimpleJSON;

namespace jshepler.ngu.mods.WebService.GO
{
    internal class NGU2GO
    {
        internal static Action HandleRequest(HttpListenerContext context, string resource)
        {
            string json;

            switch (resource)
            {
                case "augstats":
                    json = BuildAugStats();
                    context.Response.SendResponse(HttpStatusCode.OK, json, ContentTypes.JSON);
                    return () => Plugin.ShowOverrideNotification("NGU2GO: aug stats");

                case "ngustats":
                    json = BuildNGUStats();
                    context.Response.SendResponse(HttpStatusCode.OK, json, ContentTypes.JSON);
                    return () => Plugin.ShowOverrideNotification("NGU2GO: ngu stats");

                case "nakedemr":
                    json = NakedEMR3.BuildNakedEMR3();
                    context.Response.SendResponse(HttpStatusCode.OK, json, ContentTypes.JSON);
                    return () => Plugin.ShowOverrideNotification("NGU2GO: naked EMR3");

                case "hacks":
                    json = Hacks.BuildHackStats();
                    context.Response.SendResponse(HttpStatusCode.OK, json, ContentTypes.JSON);
                    return () => Plugin.ShowOverrideNotification("NGU2GO: hacks");

                case "equipped":
                    json = Loadouts.BuildCurrentEquipJson();
                    context.Response.SendResponse(HttpStatusCode.OK, json, ContentTypes.JSON);
                    return () => Plugin.ShowOverrideNotification("NGU2GO: equipped");

                default:
                    context.Response.SendResponse(HttpStatusCode.BadRequest, $"unknown resource: {resource}");
                    return () => Plugin.ShowOverrideNotification($"NGU2GO: unknown resource: {resource}");
            }
        }

        private static string BuildAugStats()
        {
            var root = new JSONObject();
            root.Add("augspeed", Plugin.Character.augmentsController.getTotalSpeedFactor());
            root.Add("ecap", Plugin.Character.totalCapEnergy());
            root.Add("gps", Plugin.Character.goldPerSecond());
            root.Add("lsc", Plugin.Character.challenges.laserSwordChallenge.curCompletions);
            root.Add("nac", Plugin.Character.challenges.noAugsChallenge.curCompletions);
            root.Add("version", (int)Plugin.Character.settings.rebirthDifficulty);

            return root.ToString();
        }

        private static string BuildNGUStats()
        {
            var character = Plugin.Character;

            var en = character.NGU.skills.Select(s => s.level).ToArray();
            var ee = character.NGU.skills.Select(s => s.evilLevel).ToArray();
            var es = character.NGU.skills.Select(s => s.sadisticLevel).ToArray();

            var eNGUs = new JSONArray();
            for (var x = 0; x < 9; x++)
            {
                var o = new JSONObject();
                o.Add("normal", en[x]);
                o.Add("evil", ee[x]);
                o.Add("sadistic", es[x]);
                eNGUs.Add(o);
            }

            var energy = new JSONObject();
            energy.Add("ngus", eNGUs);
            energy.Add("cap", Plugin.Character.totalCapEnergy());
            energy.Add("nguspeed", character.totalNGUSpeedBonus() * character.totalEnergyPower() * character.NGUController.energyNGUBonus() * character.allDiggers.totalEnergyNGUBonus() * character.adventureController.itopod.totalEnergyNGUBonus() * character.inventory.macguffinBonuses[4] * character.hacksController.totalEnergyNGUBonus() * character.beastQuestPerkController.totalEnergyNGUSpeed() * character.allChallenges.trollChallenge.totalEnergyNGUBonus() * character.wishesController.totalEnergyNGUSpeed() * character.cardsController.getBonus(cardBonus.energyNGUSpeed));


            var mn = character.NGU.magicSkills.Select(s => s.level).ToArray();
            var me = character.NGU.magicSkills.Select(s => s.evilLevel).ToArray();
            var ms = character.NGU.magicSkills.Select(s => s.sadisticLevel).ToArray();

            var mNGUs = new JSONArray();
            for (var x = 0; x < 7; x++)
            {
                var o = new JSONObject();
                o.Add("normal", mn[x]);
                o.Add("evil", me[x]);
                o.Add("sadistic", ms[x]);
                mNGUs.Add(o);
            }

            var magic = new JSONObject();
            magic.Add("ngus", mNGUs);
            magic.Add("cap", Plugin.Character.totalCapMagic());
            magic.Add("nguspeed", character.totalNGUSpeedBonus() * character.totalMagicPower() * character.NGUController.magicNGUBonus() * character.allDiggers.totalMagicNGUBonus() * character.adventureController.itopod.totalMagicNGUBonus() * character.allChallenges.trollChallenge.totalMagicNGUBonus() * character.inventory.macguffinBonuses[5] * character.hacksController.totalMagicNGUBonus() * character.beastQuestPerkController.totalMagicNGUSpeed() * character.wishesController.totalMagicNGUSpeed() * character.cardsController.getBonus(cardBonus.magicNGUSpeed));

            var quirks = new JSONObject();
            quirks.Add("e2n", (character.beastQuest.quirkLevel[14] == 1));
            quirks.Add("s2e", (character.beastQuest.quirkLevel[89] == 1));

            var root = new JSONObject();
            root.Add("energy", energy);
            root.Add("magic", magic);
            root.Add("quirk", quirks);
            root.Add("blueHeart", character.inventory.itemList.itemMaxxed[(int)GameData.Items.Heart_Blue]);

            return root.ToString();
        }
    }
}
