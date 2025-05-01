using System.Linq;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class StaplerNoECap
    {
        [HarmonyPostfix, HarmonyPatch(typeof(Character), "Start")]
        private static void Character_Start_postfix(Character __instance)
        {
            var removeCap = Options.Experimental.StaplerNoECap.Value == true;
            if (removeCap)
            {
                __instance.itemInfo.capSpec1[118] = 0; //specType1[118] = specType.None;
                __instance.itemInfo.curSpec1[118] = 0;
            }

            Plugin.OnSaveLoaded += (o, e) =>
            {
                //__instance.inventory.inventory.Where(i => i.id == 118).Do(i => i.spec1Type = removeCap ? specType.None : specType.EnergyCap);
                //__instance.inventory.accs.Where(i => i.id == 118).Do(i => i.spec1Type = removeCap ? specType.None : specType.EnergyCap);

                __instance.inventory.inventory.Where(i => i.id == 118).Do(i =>
                    {
                        i.spec1Cap = removeCap ? 0 : 3000f;
                        i.spec1Cur = removeCap ? 0 : i.spec1Cur;
                    });

                __instance.inventory.accs.Where(i => i.id == 118).Do(i =>
                    {
                        i.spec1Cap = removeCap ? 0 : 3000f;
                        i.spec1Cur = removeCap ? 0 : i.spec1Cur;
                    });
            };
        }
    }
}
