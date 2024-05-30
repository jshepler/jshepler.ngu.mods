using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class CardsInsertedAtTopOfLIst
    {
        [HarmonyTranspiler, HarmonyPatch(typeof(CardsController), "addCard")]
        private static IEnumerable<CodeInstruction> CardsController_addCard_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cards = typeof(Cards).GetField("cards");
            var insert = cards.FieldType.GetMethod("Insert");

            var cm = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldloc_0))
                //.Insert(new CodeInstruction(OpCodes.Ldc_I4_0))
                //.Advance(2)
                //.SetOperandAndAdvance(insert);
                .Advance(1)
                .SetInstruction(Transpilers.EmitDelegate(insertCard));

            return cm.InstructionEnumeration();//.DumpToLog();
        }

        private static void insertCard(List<Card> list, Card card)
        {
            for (var x = 0; x < list.Count; x++)
            {
                if (list[x].cardRarity == rarity.BigChonker)
                {
                    list.Insert(x, card);
                    return;
                }
            }

            list.Add(card);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CardsController), "addCard")]
        private static void CardsController_addCard_postfix(CardsController __instance)
        {
            __instance.updateDeckPods();
            __instance.updateDeckButtons();
        }
    }
}

/*

changes:
	character.cards.cards.Add(item);

to:
    insertCard(character.cards.cards, item);

by modifying the IL from:
	IL_0025: ldarg.0
	IL_0026: ldfld class Character CardsController::character
	IL_002b: ldfld class Cards Character::cards
	IL_0030: ldfld class [mscorlib]System.Collections.Generic.List`1<class Card> Cards::cards
	IL_0035: ldloc.0
	IL_0036: callvirt instance void class [mscorlib]System.Collections.Generic.List`1<class Card>::Add(!0)

to:
	IL_0025: ldarg.0
	IL_0026: ldfld class Character CardsController::character
	IL_002b: ldfld class Cards Character::cards
	IL_0030: ldfld class [mscorlib]System.Collections.Generic.List`1<class Card> Cards::cards
	IL_0035: ldloc.0
	IL_0036: call static void jshepler.ngu.mods.CardsInsertedAtTopOfLIst::insertCard(System.Collections.Generic.List<Card> list, Card card)





previous version of the mod:

changes:
	character.cards.cards.Add(item);

to:
	character.cards.cards.Insert(0, item);

by modifying the IL from:
	// character.cards.cards.Add(item);
	IL_0025: ldarg.0
	IL_0026: ldfld class Character CardsController::character
	IL_002b: ldfld class Cards Character::cards
	IL_0030: ldfld class [mscorlib]System.Collections.Generic.List`1<class Card> Cards::cards
	IL_0035: ldloc.0
	IL_0036: callvirt instance void class [mscorlib]System.Collections.Generic.List`1<class Card>::Add(!0)

to:
	// character.cards.cards.Add(item);
	IL_0025: ldarg.0
	IL_0026: ldfld class Character CardsController::character
	IL_002b: ldfld class Cards Character::cards
	IL_0030: ldfld class [mscorlib]System.Collections.Generic.List`1<class Card> Cards::cards
             ldc.i4.0
	IL_0035: ldloc.0
	IL_0036: callvirt instance void class [mscorlib]System.Collections.Generic.List`1<class Card>::Insert(int,!0)
 */