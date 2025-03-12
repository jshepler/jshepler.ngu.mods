using System;
using SimpleJSON;

namespace jshepler.ngu.mods.WebService.GO
{
    internal class Wishes
    {
        private static Action _uiUpdateAction;

        static Wishes()
        {
            Plugin.OnUpdate += (o, e) =>
            {
                if (_uiUpdateAction != null)
                {
                    _uiUpdateAction();
                    _uiUpdateAction = null;
                }
            };
        }

        internal static string BuildWishStats()
        {
            var character = Plugin.Character;

            var root = new JSONObject();
            root.Add("blueHeart", character.inventory.itemList.itemMaxxed[(int)GameData.Items.Heart_Blue]);
            root.Add("epow", character.totalEnergyPower());
            root.Add("ecap", character.totalCapEnergy());
            root.Add("mpow", character.totalMagicPower());
            root.Add("mcap", character.totalCapMagic());
            root.Add("rpow", character.totalRes3Power());
            root.Add("rcap", character.totalCapRes3());
            root.Add("wishspeed", character.wishesController.totalWishSpeedBonuses());

            return root.ToString();
        }
    }
}
