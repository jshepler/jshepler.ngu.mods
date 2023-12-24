using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace jshepler.ngu.mods.AutoAllocator
{
    [HarmonyPatch]
    internal class AutoAllocator
    {
        [HarmonyPrepare]
        private static void prep(MethodBase original)
        {
            if (original != null)
                return;

            Plugin.OnSaveLoaded += (o, e) => ClearAllAllocators();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Character), "removeAllEnergyAndMagic")]
        private static void Character_removeAllEnergyAndMagic_prefix()
        {
            ClearAllAllocators();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "removeMostEnergy")]
        private static void ClearEnergyAllocators()
        {
            Allocators.Energy.Values.Do(a => a.DisableAll());
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "removeMostMagic")]
        private static void ClearMagicAllocators()
        {
            Allocators.Magic.Values.Do(a => a.DisableAll());
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "removeAllRes3")]
        private static void ClearRes3Allocators()
        {
            Allocators.Res3.Values.Do(a => a.DisableAll());
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Rebirth), "engage", typeof(bool))]
        private static void ClearAllAllocators()
        {
            ClearEnergyAllocators();
            ClearMagicAllocators();
            ClearRes3Allocators();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Energy), "addEnergy")]
        private static void Energy_addEnergy_postfix(Energy __instance)
        {
            var enabled = Allocators.Energy.Values.Where(a => a.EnabledIDs.Any()).ToList();
            if (enabled.Count == 0)
                return;

            var amountDistributed = DistributeResource(enabled, __instance.character.idleEnergy);
            __instance.character.idleEnergy -= amountDistributed;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(MagicDisplay), "updateMagicBar")]
        private static void MagicDisplay_updateMagicBar_postfix(MagicDisplay __instance)
        {
            var enabled = Allocators.Magic.Values.Where(a => a.EnabledIDs.Any()).ToList();
            if (enabled.Count == 0)
                return;

            var amountDistributed = DistributeResource(enabled, __instance.character.magic.idleMagic);
            __instance.character.magic.idleMagic -= amountDistributed;
            __instance.updateMagicText();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Resource3Display), "updateRes3Bar")]
        private static void Resource3Display_updateRes3Bar_postfix(Resource3Display __instance)
        {
            var enabled = Allocators.Res3.Values.Where(a => a.EnabledIDs.Any()).ToList();
            if (enabled.Count == 0)
                return;

            var amountDistributed = DistributeResource(enabled, __instance.character.res3.idleRes3);
            __instance.character.res3.idleRes3 -= amountDistributed;
            __instance.updateRes3Text();
        }

        private static long DistributeResource(List<BaseAllocator> allocators, long amountToDistribute)
        {
            // disable any that have reached target
            //allocators.DoMany(a => a.EnabledIDs.Where(i => a.IsTargetReached(i)), (a, i) => a[i] = false);

            // base OnTargetReached disables the allocator but can be overridden to do additional work
            // (e.g. auto advance the allocator)
            allocators.DoMany(a => a.EnabledIDs.Where(i => a.IsTargetReached(i)), (a, i) => a.OnTargetReached(i));

            var maxAmounts = allocators
                .SelectMany(a => a.EnabledIDs, (a, id) => new { allocator = a, id, max = a.CalcCapDelta(id) })
                .ToList();

            var amountLeft = amountToDistribute;
            while (maxAmounts.Count > 0)
            {
                var distribution = amountLeft / maxAmounts.Count;

                // if max < dist amount, then allocate the max instead;
                // cap delta could be negative (i.e. bar is over-capped) and so "refunds" the overage
                var LEQ = maxAmounts.Where(m => m.max <= distribution).ToList();
                if (LEQ.Count > 0)
                {
                    foreach (var m in LEQ)
                    {
                        m.allocator.Allocate(m.id, m.max);
                        amountLeft -= m.max;
                        maxAmounts.Remove(m);
                    }

                    // because max < distrubution, including possibly being negative (which increases amountLeft),
                    // need to re-calc distribution amount and check for LEQ again
                    continue;
                }

                foreach (var m in maxAmounts)
                {
                    m.allocator.Allocate(m.id, distribution);
                    amountLeft -= distribution;
                }

                maxAmounts.Clear();
            }

            return (amountToDistribute - amountLeft);
        }
    }
}
