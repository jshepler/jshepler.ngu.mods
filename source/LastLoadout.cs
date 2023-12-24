using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class LastLoadout
    {
        private static InventoryController _controller;
        
        private static Loadout _lastEquipped
        {
            get => ModSave.Data.LastLoadout;
            set => ModSave.Data.LastLoadout = value;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "Start")]
        private static void InventoryController_Start_prefix(InventoryController __instance)
        {
            _controller = __instance;

            //Plugin.OnSaveLoaded += (o, e) => _lastEquipped = null;

            Plugin.OnUpdate += (o, e) =>
            {
                if (!_controller.character.InMenu(Menu.Inventory) || !Input.GetKeyDown(KeyCode.X)) return;

                if (Input.GetKey(KeyCode.LeftShift))
                {
                    _lastEquipped = GetCurrentEquip();
                    _controller.character.tooltip.showOverrideTooltip("Equipped items saved to temp loadout.", 1f);
                    return;
                }

                if (_lastEquipped != null)
                {
                    var last = _lastEquipped;
                    _lastEquipped = GetCurrentEquip();

                    SetCurrentEquip(last);
                }
            };
        }

        [HarmonyPrefix, HarmonyPatch(typeof(InventoryController), "equipLoadout")]
        private static void InventoryController_equipLoadout_prefix()
        {
            _lastEquipped = GetCurrentEquip();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Inventory), "markLoadoutIDAsDeleted", typeof(int))]
        private static void Inventory_markLoadoutIDAsDeleted_int_postfix(int id)
        {
            if (_lastEquipped == null) return;

            if (_lastEquipped.head == id) _lastEquipped.head = -1000;
            else if (_lastEquipped.chest == id) _lastEquipped.chest = -1000;
            else if (_lastEquipped.legs == id) _lastEquipped.legs = -1000;
            else if (_lastEquipped.boots == id) _lastEquipped.boots = -1000;
            else if (_lastEquipped.weapon == id) _lastEquipped.weapon = -1000;
            else if (_lastEquipped.weapon2 == id) _lastEquipped.weapon2 = -1000;
            else
            {
                for (var i = 0; i < _lastEquipped.accessories.Count; i++)
                {
                    if (_lastEquipped.accessories[i] == id)
                    {
                        _lastEquipped.accessories[i] = -1000;
                        break;
                    }
                }
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Inventory), "markLoadoutIDSwap", typeof(int), typeof(int))]
        private static void Inventory_markLoadoutIDSwap_int_int(int id1, int id2)
        {
            if (_lastEquipped == null) return;

            if (_lastEquipped.head == id1) _lastEquipped.head = id2;
            else if (_lastEquipped.head == id2) _lastEquipped.head = id1;

            else if (_lastEquipped.chest == id1) _lastEquipped.chest = id2;
            else if (_lastEquipped.chest == id2) _lastEquipped.chest = id1;

            else if (_lastEquipped.legs == id1) _lastEquipped.legs = id2;
            else if (_lastEquipped.legs == id2) _lastEquipped.legs = id1;

            else if (_lastEquipped.boots == id1) _lastEquipped.boots = id2;
            else if (_lastEquipped.boots == id2) _lastEquipped.boots = id1;

            else if (_lastEquipped.weapon == id1) _lastEquipped.weapon = id2;
            else if (_lastEquipped.weapon == id2) _lastEquipped.weapon = id1;

            else if (_lastEquipped.weapon2 == id1) _lastEquipped.weapon2 = id2;
            else if (_lastEquipped.weapon2 == id2) _lastEquipped.weapon2 = id1;

            else
            {
                for (var i = 0; i < _lastEquipped.accessories.Count; i++)
                {
                    if (_lastEquipped.accessories[i] == id1)
                    {
                        _lastEquipped.accessories[i] = id2;
                        break;
                    }

                    if (_lastEquipped.accessories[i] == id2)
                    {
                        _lastEquipped.accessories[i] = id1;
                        break;
                    }
                }
            }
        }

        // from InventoryController.assignCurrentEquipToLoadout()
        private static Loadout GetCurrentEquip()
        {
            var inventory = _controller.character.inventory;
            var loadout = new Loadout();

            loadout.head = inventory.head.id == 0 ? -1000 : -1;
            loadout.chest = inventory.chest.id == 0 ? -1000 : -2;
            loadout.legs = inventory.legs.id == 0 ? -1000 : -3;
            loadout.boots = inventory.boots.id == 0 ? -1000 : -4;
            loadout.weapon = inventory.weapon.id == 0 ? -1000 : -5;
            loadout.weapon2 = inventory.weapon2.id == 0 ? -1000 : -6;

            loadout.accessories = Enumerable.Range(0, 16).Select(i => -1000).ToList();
            for (int i = 0; i < inventory.accs.Count; i++)
            {
                loadout.accessories[i] = inventory.accs[i].id == 0 ? -1000 : (i + 10000);
            }

            return loadout;
        }

        // from InventoryController.equipLoadout()
        private static void SetCurrentEquip(Loadout loadout)
        {
            var character = _controller.character;
            var inventory = character.inventory;
            
            //if (character.settings.unassignWhenSwapping)
            //{
            //    character.removeAllEnergyAndMagic();
            //    if (character.arbitrary.instaTrain)
            //    {
            //        character.idleEnergy -= 12L;
            //        character.training.attackEnergy[0] += 6L;
            //        character.training.defenseEnergy[0] += 6L;
            //    }
            //}

            if (loadout.head >= 0 && loadout.head < 10000)
            {
                inventory.item1 = -1;
                inventory.item2 = loadout.head;
                _controller.swapHead();
                _controller.updateBonuses();
            }

            if (loadout.chest >= 0 && loadout.chest < 10000)
            {
                inventory.item1 = -2;
                inventory.item2 = loadout.chest;
                _controller.swapChest();
                _controller.updateBonuses();
            }

            if (loadout.legs >= 0 && loadout.legs < 10000)
            {
                inventory.item1 = -3;
                inventory.item2 = loadout.legs;
                _controller.swapLegs();
                _controller.updateBonuses();
            }

            if (loadout.boots >= 0 && loadout.boots < 10000)
            {
                inventory.item1 = -4;
                inventory.item2 = loadout.boots;
                _controller.swapBoots();
                _controller.updateBonuses();
            }

            if (loadout.weapon >= 0 && loadout.weapon < 10000)
            {
                inventory.item1 = -5;
                inventory.item2 = loadout.weapon;
                _controller.swapWeapon();
                _controller.updateBonuses();
            }

            if (loadout.weapon2 >= 0 && loadout.weapon2 < 10000)
            {
                inventory.item1 = -6;
                inventory.item2 = loadout.weapon2;
                _controller.swapWeapon2();
                _controller.updateBonuses();
            }

            for (var i = 0; i < inventory.accs.Count; i++)
            {
                if (loadout.accessories[i] >= 0 && loadout.accessories[i] < 100000 && loadout.accessories[i] != i + 10000)
                {
                    inventory.item1 = i + 10000;
                    inventory.item2 = loadout.accessories[i];
                    _controller.swapAcc();
                    _controller.updateBonuses();
                }
            }

            _controller.updateInventory();
        }
    }
}
