using System;
using System.Linq;
using SimpleJSON;

namespace jshepler.ngu.mods.WebService.GO
{
    internal class Loadouts
    {
        private static Action _uiUpdateAction;

        static Loadouts()
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

        internal static string BuildCurrentEquipJson()
        {
            var inventory = Plugin.Character.inventory;

            var root = new JSONObject();

            var weapon = new JSONArray();
            root.Add("weapon", weapon);
            weapon.Add(new JSONNumber(GOID(inventory.weapon, 10000)));
            weapon.Add(new JSONNumber(GOID(inventory.weapon2, 10000)));

            var head = new JSONArray();
            root.Add("head", head);
            head.Add(new JSONNumber(GOID(inventory.head, 10001)));

            var armor = new JSONArray();
            root.Add("armor", armor);
            armor.Add(new JSONNumber(GOID(inventory.chest, 10002)));

            var pants = new JSONArray();
            root.Add("pants", pants);
            pants.Add(new JSONNumber(GOID(inventory.legs, 10003)));

            var boots = new JSONArray();
            root.Add("boots", boots);
            boots.Add(new JSONNumber(GOID(inventory.boots, 10004)));

            var accessory = new JSONArray();
            root.Add("accessory", accessory);
            foreach (var acc in inventory.accs)
                accessory.Add(new JSONNumber(GOID(acc, 10005)));

            return root.ToString();
        }

        private static int GOID(Equipment item, int emptyId)
        {
            return item.id == 0 ? emptyId : item.id;
        }

        internal static void ImportFromJSON(string json)
        {
            var saveSlots = JSON.Parse(json);
            var loadouts = Plugin.Character.inventory.loadouts;
            foreach (var ss in saveSlots.AsArray.Values)
            {
                var name = ss["name"].Value.ToLowerInvariant();
                var loadout = loadouts.FirstOrDefault(l => l.loadoutName.ToLowerInvariant() == name);
                if (loadout == null)
                    continue;

                var slot = Slot.Parse(ss);

                loadout.head = FindSlotId(slot.head);
                loadout.chest = FindSlotId(slot.armor);
                loadout.legs = FindSlotId(slot.pants);
                loadout.boots = FindSlotId(slot.boots);
                loadout.weapon = FindSlotId(slot.weapon1);
                loadout.weapon2 = FindSlotId(slot.weapon2);

                for (var x = 0; x < loadout.accessories.Count; x++)
                    if (x < slot.accs.Length)
                        loadout.accessories[x] = FindSlotId(slot.accs[x]);

                _uiUpdateAction = Plugin.Character.inventoryController.loadoutsController.refresh;
            }
        }

        private static int FindSlotId(int itemId)
        {
            if (itemId >= 10000)
                return -1000;

            var inv = Plugin.Character.inventory;

            if (inv.head.id == itemId)
                return -1;

            if (inv.chest.id == itemId)
                return -2;

            if (inv.legs.id == itemId)
                return -3;

            if (inv.boots.id == itemId)
                return -4;

            if (inv.weapon.id == itemId)
                return -5;

            if (inv.weapon2.id == itemId)
                return -6;

            for (var x = 0; x < inv.accs.Count; x++)
                if (inv.accs[x].id == itemId)
                    return x + 10000;

            var maxLevel = -1;
            var maxLevelIndex = -1;

            for (var x = 0; x < inv.inventory.Count; x++)
            {
                var item = inv.inventory[x];
                if (item.id == itemId && item.level > maxLevel)
                {
                    maxLevel = item.level;
                    maxLevelIndex = x;
                }
            }

            if (maxLevelIndex >= 0)
                return maxLevelIndex;

            return -1000;
        }

        private class Slot
        {
            public string name;
            public int head;
            public int armor;
            public int pants;
            public int boots;
            public int weapon1;
            public int weapon2;
            public int[] accs;

            internal static Slot Parse(JSONNode node)
            {
                Slot slot = new()
                {
                    name = node["name"].Value,
                    head = node["head"].AsArray[0].AsInt,
                    armor = node["armor"].AsArray[0].AsInt,
                    pants = node["pants"].AsArray[0].AsInt,
                    boots = node["boots"].AsArray[0].AsInt,
                    weapon1 = node["weapon"].AsArray[0].AsInt,
                    weapon2 = node["weapon"].AsArray[1].AsInt,
                    accs = node["accessory"].AsArray.Children.Select(n => n.AsInt).ToArray()
                };

                return slot;
            }
        }
    }
}