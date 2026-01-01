using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class ItemSetCompletionTexts
    {
        private static Character character => Plugin.Character;

        private static string expText(long gained) => character.display(gained) + " EXP";
        private static string apText(long gained) => character.display(gained) + " AP";

        private static string modExpText(long baseExp) => expText(character.checkExpAdded(baseExp)) + $" ({character.display(baseExp)} base)";
        private static string modApText(long baseAp) => apText(character.checkAPAdded(baseAp)) + $" ({character.display(baseAp)} base)";

        private static bool hasFasterPaceMod = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("fasterPace");

        [HarmonyPrefix, HarmonyPatch(typeof(ItemListController), "setBonusText")]
        private static bool setBonusText(ItemListController __instance, ref string __result)
        {
            if (hasFasterPaceMod)
                return true;

            var text = __instance.setID switch
            {
                -1 => string.Empty,
                0 => "\n\n<b>Training Set:</b>\nItems 62, 63, 64, 65, and 75.\n\n<b>Completion Bonus (All items level 100):</b>\n2 Energy Speed\n" + modExpText(10L) + ".",
                1 => "\n\n<b>Sewers Set</b>\nItems 40-46.\n\n<b>Completion Bonus (All items level 100):</b>\n+5 Power and Toughness\n+15 max Health\n+0.2 regen\n" + modExpText(20L) + ".",
                2 => "\n\n<b>Forest Set:</b>\nItems 47-53.\n\n<b>Completion Bonus (All items level 100):</b>\n2 Energy Potion α\n2 Energy Potion β\n2 Energy Bar Bar\n5 Energy Power\n" + modExpText(200L) + ".",
                3 => "\n\n<b>Cave Set:</b>\nItems 54-61.\n\n<b>Completion Bonus (All items level 100):</b>\n2 Magic Power\n40000 Magic Cap\n2 Magic Per Bar\n" + modExpText(300L) + ".",
                4 => "\n\n<b>HSB Set:</b>\nItems 68-74.\n\n<b>Completion Bonus (All items level 100):</b>\n3 Magic Power\n30000 Magic Cap\n3 Magic Bars\n1 Magic Potion α\n1 Magic Potion β\n1 Magic Bar Bar\n" + modExpText(500L) + ".",
                5 => "\n\n<b>GRB Set:</b>\nItems 78-84.\n\n<b>Completion Bonus (All items level 100):</b>\n" + modExpText(2000L) + "\nA small perk: The Safe Zone will now regenerate health 10x faster, instead of 5x!",
                6 => "\n\n<b>Clock Set:</b>\nItems 85-91.\n\n<b>Completion Bonus (All items level 100):</b>\n" + modExpText(1000L) + "\nA small perk: Enemies in Adventure will now spawn 5% Faster!",
                7 => "\n\n<b>2D Set:</b>\nItems 95-101.\n\n<b>Completion Bonus (All items level 100):</b>\n" + modExpText(2000L) + "\nGain a permanent 7.43% bonus drop chance for loot!",
                8 => "\n\n<b>Spoopy Set:</b>\nItems 103-109.\n\n<b>Completion Bonus (All items level 100):</b>\n" + modExpText(3000L) + "\nIdle attack will gain the same damage multiplier as Regular Attack!",
                9 => "\n\n<b>Jake Set:</b>\nItems 111-117.\n\n<b>Completion Bonus (All items level 100):</b>\n" + modExpText(7000L) + "\nYou'll also unlock a new Wandoos OS: Wandoos MEH! This is a much stronger OS, provided you have the energy and magic to spare!",
                10 => "\n\n<b>Gaudy Set:</b>\nItems 122-126.\n\n<b>Completion Bonus (All items level 100):</b>\n" + modExpText(5000L) + "\n2 Lucky Charms!\nItems that drop at level 1 or higher have a 10% chance to gain an additional level!",
                11 => "\n\n<b>Mega Set:</b>\nItems 130-134.\n\n<b>Completion Bonus (All items level 100):</b>\n" + modExpText(6000L) + "\nCharge will now give a 2.2x boost to the next skill used!",
                12 => "\n\n<b>Beardverse Set:</b>\nItems 143-147.\n\n<b>Completion Bonus (All items level 100):</b>\n" + modExpText(8000L) + "\n10% reduced penalty to levelling speed, when equipping multiple beards that use Energy or Magic at the same time!",
                13 => "\n\n<b>Wanderer's Set:</b>\nItems 150-153.\n\n<b>Completion Bonus (All items level 100):</b>\n" + modExpText(50000L) + "!" + "\n" + modApText(10000L) + "!" + "\nA new, ultra-rare accessory is now dropped by WALDERP!",
                14 => "\n\n<b>s'rerednaW Set:</b>\nItems 155-158.\n\n<b>Completion Bonus (All items level 100):</b>\n" + modExpText(50000L) + "!" + "\n" + modApText(10000L) + "!" + "\nA new, ultra-rare accessory is now dropped by WALDERP!",
                15 => "\n\n<b>Badly Drawn Set:</b>\nItems 164-168.\n\n<b>Completion Bonus (All items level 100):</b>\n" + modExpText(30000L) + "!" + "\n" + modApText(5000L) + "!" + "\nBoosts are now 20% more effective!",
                16 => "\n\n<b>Stealth Set:</b>\nItems 173-177.\n\n<b>Completion Bonus (All items level 100):</b>\n" + modExpText(50000L) + "!" + "\n" + modApText(10000L) + "!" + "\nUnlock an ultra-rare chest drop in Boring-Ass Earth!",
                17 => "\n\n<b>Slimy Set:</b>\nItems 184-188.\n\n<b>Completion Bonus (All items level 100):</b>\n" + modExpText(100000L) + "!" + "\n" + modApText(10000L) + "!" + "\nParry's reflected attack is now 3x stronger!",
                18 => "\n\n<b>Edgy Set:</b>\nItems 213-215, 217, and 218.\n\n<b>Completion Bonus (All items level 100):</b>\n" + modExpText(250000L) + "!\nGain a MacGuffin slot!",
                19 => "\n\n<b>Edgy Boots Set:</b>\nItems 216 and 219.\n\n<b>Completion Bonus (All items level 100):</b>\nUnlock a special drop in The Evilverse!",
                20 => "\n\n<b>Choco Set:</b>\nItems 221-225.\n\n<b>Completion Bonus (All items level 100):</b>\nUnlock 2 special drops in Chocolate World, plus a new MacGuffin! Also: Reduce the number of kills needed per MacGuffin drop outside of the ITOPOD by 10%! Chocolate is some powerful stuff.",
                21 => "\n\n<b>Pretty Pink Princess Set:</b>\nItems 231-236.\n\n<b>Completion Bonus (All items level 100):</b>\nEarn 10% more PP!",
                22 => "\n\n<b>Greasy Nerd Set:</b>\nItems 237-241.\n\n<b>Completion Bonus (All items level 100):</b>\nAll MacGuffins drop 1 level higher!",
                23 => "\n\n<b>Meta Set:</b>\nItems 251-257.\n\n<b>Completion Bonus (All items level 100):</b>\n+20% NGU Speed! Gotta get those Numbers Going Up!",
                24 => "\n\n<b>Party Set:</b>\nItems 258-264.\n\n<b>Completion Bonus (All items level 100):</b>\n+5% Global Digger Bonus!",
                25 => "\n\n<b>Mobster Set:</b>\nItems 265-271.\n\n<b>Completion Bonus (All items level 100):</b>\n+15% QP earned while Questing!",
                26 => "\n\n<b>Typo Set:</b>\nItems 301-307.\n\n<b>Completion Bonus (All items level 100):</b>\n+20% Wish Speed!",
                27 => "\n\n<b>Fad Set:</b>\nItems 308-314.\n\n<b>Completion Bonus (All items level 100):</b>\n10% Faster Major Quests!",
                28 => "\n\n<b>JRPG Set:</b>\nItems 315-321.\n\n<b>Completion Bonus (All items level 100):</b>\nA better Ultimate Attack!",
                29 => "\n\n<b>Exile Set:</b>\nItems 322-326.\n\n<b>Completion Bonus (All items level 100):</b>\nUnlocks something secret!",
                30 => "\n\n<b>Rad Set:</b>\nItems 345-351.\n\n<b>Completion Bonus (All items level 100):</b>\n+5 Max Deck Size!",
                31 => "\n\n<b>Back To School Set:</b>\nItems 352-358.\n\n<b>Completion Bonus (All items level 100):</b>\n+15% NGU Speed!",
                32 => "\n\n<b>Western Set:</b>\nItems 359-365.\n\n<b>Completion Bonus (All items level 100):</b>\nAn Extra Drop in this zone!",
                33 => "\n\n<b>Space Set:</b>\nItems 373-379.\n\n<b>Completion Bonus (All items level 100):</b>\n+10% Cooking EXP Bonus!",
                34 => "\n\n<b>Bread Set:</b>\nItems 392-399.\n\n<b>Completion Bonus (All items level 100):</b>\nFaster Cooks!!",
                35 => "\n\n<b>Disco Set:</b>\nItems 400-407.\n\n<b>Completion Bonus (All items level 100):</b>\nLess crappy cards!",
                36 => "\n\n<b>Halloweenie Set:</b>\nItems 408-415.\n\n<b>Completion Bonus (All items level 100):</b>\n+45% PP gain!",
                37 => "\n\n<b>Rock Set:</b>\nItems 416-423.\n\n<b>Completion Bonus (All items level 100):</b>\n+1 tier to ALL CARDS!",
                38 => "\n\n<b>Construction Set:</b>\nItems 453-460.\n\n<b>Completion Bonus (All items level 100):</b>\n20% Boostier Boosts!",
                39 => "\n\n<b>Duck Set:</b>\nItems 496-503.\n\n<b>Completion Bonus (All items level 100):</b>\n+6% Mayo and Card Speed!",
                40 => "\n\n<b>Dutch Set:</b>\nItems 461-468.\n\n<b>Completion Bonus (All items level 100):</b>\nFaster Ritual Speed?",
                41 => "\n\n<b>Amalgamate Set:</b>\nItems 469-476.\n\n<b>Completion Bonus (All items level 100):</b>\n+10 Max Deck size!",
                42 => "\n\n<b>Pirate Set:</b>\nItems 507-514.\n\n<b>Completion Bonus (All items level 100):</b>\nPride and Accomplishment.",
                1000 => "\n\n<b>Wandoos Set</b>\nJust this Item. Like, only this. Just level this up to 100, that's it. It's that simple.\n\n<b>Completion Bonus (All items level 100):</b>\nWhen Wandoos completes the booting process, gain an additional 10% speed bonus. Also, " + modExpText(300L) + "!",
                1001 => "\n\n<b>Tutorial Cube Set:</b>\nJust this cube, easy peasy.\n\n<b>Completion Bonus (All items level 100):</b>\nSomething special!",
                1002 => "\n\n<b>Number Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\n+10% NGU Speed!",
                1003 => "\n\n<b>Flubber Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\n" + modApText(30000L) + "!",
                1004 => "\n\n<b>Seed Set ;):</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\n10 Premium samples of Icarus Proudbottom's Homemade Boom Boom Fertilizers! Check out the Sellout shop for more info on what these poops do.",
                1005 => "\n\n<b>Armpit Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\n+10% Beard Speed!",
                1006 => "\n\n<b>Red Heart Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\nYou will receive the max heart EXP bonus (10%) even when the heart is not equipped!",
                1007 => "\n\n<b>Yellow Heart Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\nYou will receive the max heart AP bonus (20%) even when the heart is not equipped!",
                1008 => "\n\n<b>UUG's Rings Set:</b>\nItems 136-140\n\n<b>Completion Bonus (All items level 100):</b>\n" + modExpText(20000L) + "\n" + modApText(20000L) + "!" + "\nUnlock a new, super-ultra rare drop from UUG! ",
                1009 => "\n\n<b>Boosts Set:</b>\nItems 1-39\n\n<b>Completion Bonus(For each item maxxed):</b>\n+2% permanent boosting power to ALL boosts!",
                1010 => "\n\n<b>Red Liquid Set:</b>\nJust this thing.\n\n<b>Completion Bonus(All items level 100):</b>\n-20% on the global cooldown timer, AND for idle attack speed! The global cooldown timer is the cooldown between using different moves, if you didn't know!",
                1011 => "\n\n<b>Brown Heart Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\nEvery 10th poop you use on a fruit will not be consumed!",
                1012 => "\n\n<b>Wandoos XL Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\nWandoos now boots up 10% faster!",
                1013 => "\n\n<b>Green Heart Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\nGain 20% faster progress towards Perk Points (PP) in the I.T.O.P.O.D!",
                1014 => "\n\n<b>Pissed Off Key Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\nGain 10% faster progress towards Perk Points (PP) in the I.T.O.P.O.D!",
                1015 => "\n\n<b>Purple Liquid Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\nBEAST MODE now grants +50% to your Power, instead of +40%!",
                1016 => "\n\n<b>Blue Heart Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\nAll consumables give 10% better effects!",
                1017 => "\n\n<b>Scrap of Paper Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\nGain a Digger Slot!",
                1018 => "\n\n<b>Purple Heart Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\nMacGuffins drop 20% more often!",
                1019 => "\n\n<b>Quest Items Set</b>\nItems 278-287\n\n<b>Completion Bonus (For each item maxxed):</b>\n+2% QP rewards in Questing!",
                1020 => "\n\n<b>Orange Heart Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\nQuests give 20% more QP!",
                1021 => "\n\n<b>Heroic Sigil Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\nQuests Items drop 10% more often!",
                1022 => "\n\n<b>Grey Heart Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\n25% Faster Hacks!",
                1023 => "\n\n<b>Incriminating Evidence Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\n+2 base " + character.res3.res3Name + " Power\n+80K base " + character.res3.res3Name + " Cap\n+2 base " + character.res3.res3Name + " Bars\n+1 of every Resource 3 Potion!",
                1024 => "\n\n<b>Pink Heart Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\nGain an additional Wish slot!",
                1025 => "\n\n<b>Severed Head Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\n+13.37% Wish Speed!",
                1026 => "\n\n<b>Rainbow Heart Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\n+10% Mayo and Card Generation Speed!",
                1027 => "\n\n<b>Still-Beating Heart Set:</b>\nJust this thing.\n\n<b>Completion Bonus (All items level 100):</b>\n+1% Tag Effect!",
                1028 => "\n\n<b>Normal Bonus Accs Set:</b>\nItems 432-444.\n\n<b>Completion Bonus (All items level 100):</b>\n+25% Drop Chance",
                1029 => "\n\n<b>Evil Bonus Accs Set:</b>\nItems 445-452.\n\n<b>Completion Bonus (All items level 100):</b>\n+20% Adventure Stats!",
                _ => "\n\nCongratulations! If you see this, 4G messed something else up in the game too!"
            };

            var il = character.inventory.itemList;
            if ((__instance.setID == 0 && il.trainingComplete)
                || (__instance.setID == 1 && il.sewersComplete)
                || (__instance.setID == 2 && il.forestComplete)
                || (__instance.setID == 3 && il.caveComplete)
                || (__instance.setID == 4 && il.HSBComplete)
                || (__instance.setID == 5 && il.GRBComplete)
                || (__instance.setID == 6 && il.clockComplete)
                || (__instance.setID == 7 && il.twoDComplete)
                || (__instance.setID == 8 && il.ghostComplete)
                || (__instance.setID == 9 && il.jakeComplete)
                || (__instance.setID == 10 && il.gaudyComplete)
                || (__instance.setID == 11 && il.megaComplete)
                || (__instance.setID == 12 && il.beardverseComplete)
                || (__instance.setID == 13 && il.waldoComplete)
                || (__instance.setID == 14 && il.antiWaldoComplete)
                || (__instance.setID == 15 && il.badlyDrawnComplete)
                || (__instance.setID == 16 && il.stealthComplete)
                || (__instance.setID == 17 && il.beast1complete)
                || (__instance.setID == 18 && il.edgyComplete)
                || (__instance.setID == 19 && il.edgyBootsComplete)
                || (__instance.setID == 20 && il.chocoComplete)
                || (__instance.setID == 21 && il.prettyComplete)
                || (__instance.setID == 22 && il.nerdComplete)
                || (__instance.setID == 23 && il.metaComplete)
                || (__instance.setID == 24 && il.partyComplete)
                || (__instance.setID == 25 && il.godmotherComplete)
                || (__instance.setID == 26 && il.typoComplete)
                || (__instance.setID == 27 && il.fadComplete)
                || (__instance.setID == 28 && il.jrpgComplete)
                || (__instance.setID == 29 && il.exileComplete)
                || (__instance.setID == 30 && il.radComplete)
                || (__instance.setID == 31 && il.schoolComplete)
                || (__instance.setID == 32 && il.westernComplete)
                || (__instance.setID == 33 && il.spaceComplete)
                || (__instance.setID == 34 && il.breadverseComplete)
                || (__instance.setID == 35 && il.that70sComplete)
                || (__instance.setID == 36 && il.halloweeniesComplete)
                || (__instance.setID == 37 && il.rockLobsterComplete)
                || (__instance.setID == 38 && il.constructionComplete)
                || (__instance.setID == 39 && il.duckComplete)
                || (__instance.setID == 40 && il.netherComplete)
                || (__instance.setID == 41 && il.amalgamateComplete)
                || (__instance.setID == 1000 && il.wandoosComplete)
                || (__instance.setID == 1001 && il.tutorialCubeComplete)
                || (__instance.setID == 1002 && il.numberComplete)
                || (__instance.setID == 1003 && il.flubberComplete)
                || (__instance.setID == 1004 && il.seedComplete)
                || (__instance.setID == 1005 && il.uugComplete)
                || (__instance.setID == 1006 && il.itemMaxxed[119])
                || (__instance.setID == 1007 && il.itemMaxxed[129])
                || (__instance.setID == 1008 && il.uugRingComplete)
                || (__instance.setID == 1009 && il.itemMaxxed[__instance.id])
                || (__instance.setID == 1010 && il.itemMaxxed[93])
                || (__instance.setID == 1011 && il.itemMaxxed[162])
                || (__instance.setID == 1012 && il.xlComplete)
                || (__instance.setID == 1013 && il.greenHeartComplete)
                || (__instance.setID == 1014 && il.itopodKeyComplete)
                || (__instance.setID == 1015 && il.purpleLiquidComplete)
                || (__instance.setID == 1016 && il.blueHeartComplete)
                || (__instance.setID == 1017 && il.jakeNoteComplete)
                || (__instance.setID == 1018 && il.purpleHeartComplete)
                || (__instance.setID == 1019 && il.itemMaxxed[__instance.id])
                || (__instance.setID == 1020 && il.orangeHeartComplete)
                || (__instance.setID == 1021 && il.sigilComplete)
                || (__instance.setID == 1022 && il.greyHeartComplete)
                || (__instance.setID == 1023 && il.evidenceComplete)
                || (__instance.setID == 1024 && il.pinkHeartComplete)
                || (__instance.setID == 1025 && il.severedHeadComplete)
                || (__instance.setID == 1026 && il.rainbowHeartComplete)
                || (__instance.setID == 1027 && il.beatingHeartComplete)
                || (__instance.setID == 1028 && il.normalBonusAccComplete)
                || (__instance.setID == 1029 && il.evilBonusAccComplete))
            {
                text += "\n\n<color=green><b>COMPLETE</b></color>";
            }

            __result = text;
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AllItemListController), "checkforBonuses")]
        private static bool checkforBonuses(AllItemListController __instance)
        {
            if (hasFasterPaceMod)
                return true;

            var il = character.inventory.itemList;
            if (!il.trainingComplete && il.maxxedTraining())
            {
                il.trainingComplete = true;
                character.inventoryController.updateInvCount();
                character.energySpeed += 2f;

                long exp = character.addExp(10L);

                __instance.tooltip.showTooltip(
                    "You've maxxed out every item in the training set, congrats! You've been awarded 2 Energy Speed and "
                    + expText(exp)
                    + "! You also unlocked a new player portrait in the Fight Boss Menu!",
                    5f);

                character.portraits.portraitUnlocked[11] = true;
            }

            else if (!il.sewersComplete && il.maxxedSewers())
            {
                il.sewersComplete = true;
                character.inventoryController.updateInvCount();
                character.adventure.attack += 5f;
                character.adventure.defense += 5f;
                character.adventure.maxHP += 15f;
                character.adventure.regen += 0.2f;

                long exp = character.addExp(20L);

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out every item in the sewers set, congrats! You unlocked a new player portrait in the Fight Boss Menu! You've also been awarded:\n+5 to Power and Toughness\n15 max Health\n0.2 regen\nAnd "
                    + expText(exp)
                    + "!",
                    5f);

                character.portraits.portraitUnlocked[12] = true;
            }

            else if (!il.forestComplete && il.maxxedForest())
            {
                il.forestComplete = true;
                character.inventoryController.updateInvCount();
                character.arbitrary.energyPotion1Count += 2;
                character.arbitrary.energyPotion2Count += 2;
                character.arbitrary.energyBarBar1Count += 2;
                character.energyPower += 5f;

                long exp = character.addExp(200L);

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out every item in the forest set, congrats! You unlocked a new player portrait in the Fight Boss Menu! You've been awarded:\n\n2 Energy Potion α\n2 Energy Potion β\n2 Energy Bar Bar\n5 Energy Power\nAnd "
                    + expText(exp)
                    + "!",
                    5f);

                character.portraits.portraitUnlocked[13] = true;
                character.portraits.portraitUnlocked[14] = true;
                character.portraits.portraitUnlocked[15] = true;
            }

            else if (!il.caveComplete && il.maxxedCave())
            {
                il.caveComplete = true;
                character.inventoryController.updateInvCount();

                long exp = character.addExp(300L);

                character.magic.magicPower += 2f;
                character.magic.capMagic += 40000L;
                character.magic.magicPerBar += 2L;
                character.portraits.portraitUnlocked[16] = true;

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out every item in the cave set, congrats! You unlocked a new player portrait in the Fight Boss Menu! You've been awarded:\n2 Magic Power\n40,000 Magic Cap\n2 Magic Per Bar\nAnd "
                    + expText(exp)
                    + "!",
                    5f);
            }

            else if (!il.HSBComplete && il.maxxedHSB())
            {
                il.HSBComplete = true;

                long exp = character.addExp(500L);

                character.magic.magicPerBar += 3L;
                character.magic.magicPower += 3f;
                character.magic.capMagic += 30000L;
                character.arbitrary.magicBarBar1Count++;
                character.arbitrary.magicPotion1Count++;
                character.arbitrary.magicPotion2Count++;
                character.portraits.portraitUnlocked[17] = true;

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out every item in the HSB set, congrats! You unlocked a new player portrait in the Fight Boss Menu! You've been awarded:\n3 Magic Power\n30000 magic Cap\n3 Magic Bars\n1 Magic Bar Bar\n1 Magic Potion α\n1 Magic Potion β\nAnd "
                    + expText(exp)
                    + "!",
                    5f);
            }

            else if (!il.GRBComplete && il.maxxedGRB())
            {
                il.GRBComplete = true;

                long exp = character.addExp(2000L);

                character.portraits.portraitUnlocked[18] = true;

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out every item in the GRB set, congrats! You unlocked a new player portrait in the Fight Boss Menu! You've been awarded:\n"
                    + expText(exp)
                    + "!\n Also, the Safe Zone will now provide a 10x HP Regen boost instead of 5x!",
                    5f);
            }

            else if (!il.clockComplete && il.maxxedClock())
            {
                il.clockComplete = true;

                long exp = character.addExp(1000L);

                character.portraits.portraitUnlocked[19] = true;

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out every item in the Clockwork set, congrats! You unlocked a new player portrait in the Fight Boss Menu! You've been awarded:\n"
                    + expText(exp)
                    + "!\n Also, enemies will now spawn 5% Faster!",
                    5f);
            }

            else if (!il.twoDComplete && il.maxxed2D())
            {
                il.twoDComplete = true;

                long exp = character.addExp(2000L);

                character.portraits.portraitUnlocked[20] = true;

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out every item in the 2D set, congrats! You unlocked a new player portrait in the Fight Boss Menu! You've been awarded:\n"
                    + expText(exp)
                    + "!\nYour drop chance in Adventure has permanently increased by 7.43%! Why that weird number? Ask room 1 of the NGU Idle chat!",
                    5f);
            }

            else if (!il.ghostComplete && il.maxxedGhost())
            {
                il.ghostComplete = true;

                long exp = character.addExp(3000L);

                character.portraits.portraitUnlocked[21] = true;

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out every item in the Ghost set, congrats! You unlocked a new player portrait in the Fight Boss Menu! You unlocked a new player portrait in the Fight Boss Menu! You've been awarded:\n"
                    + expText(exp)
                    + "!\nAlso, Idle attack now has the damage multiplier of regular Attack!",
                    5f);
            }

            else if (!il.jakeComplete && il.maxxedJake())
            {
                il.jakeComplete = true;

                long exp = character.addExp(7000L);

                character.portraits.portraitUnlocked[22] = true;

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out every item in the Jake set, congrats! You unlocked a new player portrait in the Fight Boss Menu! You've been awarded:\n"
                    + expText(exp)
                    + "!\nAlso, you've unlocked wandoos MEH!",
                    5f);
            }

            else if (!il.gaudyComplete && il.maxxedGaudy())
            {
                il.gaudyComplete = true;

                long exp = character.addExp(5000L);

                character.arbitrary.lootCharm1Count += 2;
                character.portraits.portraitUnlocked[23] = true;

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out every item in the Gaudy set, congrats! You unlocked a new player portrait in the Fight Boss Menu! You've been awarded:\n"
                    + expText(exp)
                    + "!\n2 Lucky charms!\nAlso, any item that drops at level 1 or higher has a 10% chance of dropping at +1 level!",
                    5f);
            }

            else if (!il.megaComplete && il.maxxedMega())
            {
                il.megaComplete = true;

                long exp = character.addExp(6000L);

                character.portraits.portraitUnlocked[24] = true;

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out every item in the Mega set, congrats! You unlocked a new player portrait in the Fight Boss Menu! You've been awarded:\n"
                    + expText(exp)
                    + "!\nAlso, Charge attack now gives a 2.2x bonus to your next move, instead of 2.0!",
                    5f);
            }

            else if (!il.beardverseComplete && il.maxxedBeardverse())
            {
                il.beardverseComplete = true;

                long exp = character.addExp(8000L);

                character.portraits.portraitUnlocked[25] = true;

                __instance.tooltip.showOverrideTooltip(
                    "You completed the Beardverse Set, congrats! You unlocked a new player portrait in the Fight Boss Menu! You've been awarded:\n"
                    + expText(exp)
                    + "!\nAlso, Equipping multiple beards that use Energy or Magic at the same time have a 10% reduced penalty to levelling speed!",
                    4f);
            }

            else if (!il.uugRingComplete && il.maxxedRingUUG())
            {
                il.uugRingComplete = true;

                long exp = character.addExp(20000L);
                long ap = character.addAP(20000);

                __instance.tooltip.showOverrideTooltip(
                    "You completed the UUG's Rings Set, congrats! You've been awarded:\n"
                    + expText(exp) + "!\n"
                    + apText(ap) + "!\n"
                    + "Also, you've unlocked a sixth and ULTRA rare ring drop from UUG! Happy grinding! :D",
                    4f);
            }

            else if (!il.wandoosComplete && il.maxxedWandoos())
            {
                il.wandoosComplete = true;

                long exp = character.addExp(300L);

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out your Wandoos item, congrats! You've been awarded "
                    + expText(exp)
                    + " and a special little perk: When wandoos finishes booting up, you will receive a 10% bonus to its leveling speed! And sure, let's add 300 EXP to the pot.",
                    5f);
            }

            else if (!il.tutorialCubeComplete && il.maxxedTutorialCube())
            {
                il.tutorialCubeComplete = true;

                long ap = character.addAP(10000);

                __instance.tooltip.showOverrideTooltip(
                    "You've merged the Tutorial Cube to level 100, and it cracks open! You look inside and see... :o! "
                    + apText(ap)
                    + "! These meaningless points can buy cool items in the 4G Sellout shop! You'll earn AP for a lot of things as you continue to play.",
                    8f);
            }

            else if (!il.numberComplete && il.maxxedNumber())
            {
                il.numberComplete = true;
                __instance.tooltip.showOverrideTooltip("For creating a level 100 Number, you've been awarded a 10% speed boost to the NGU feature!", 4f);
            }

            else if (!il.flubberComplete && il.maxxedFlubber())
            {
                il.flubberComplete = true;

                long ap = character.addAP(30000);

                __instance.tooltip.showOverrideTooltip(
                    "For creating a level 100 Triple Flubber (:o), you've been awarded "
                    + apText(ap)
                    + "!",
                    4f);
            }

            else if (!il.seedComplete && il.maxxedSeed())
            {
                il.seedComplete = true;
                __instance.tooltip.showOverrideTooltip("For creating a level 100 Seed (:o), you've been awarded 10 Premium samples of Icarus Proudbottom's Homeamde Boom Boom Fertilizers! Check out the Sellout shop for more info on what these poops do.", 4f);
                character.arbitrary.poop1Count += 10;
            }

            else if (!il.uugComplete && il.maxxedUUG())
            {
                il.uugComplete = true;
                __instance.tooltip.showOverrideTooltip("For creating a level 100 piece of Armpit Hair (gross), you've been awarded a 10% boost to your Beard Speed!", 4f);
            }

            else if (!il.redLiquidComplete && il.maxxedRedLiquid())
            {
                il.redLiquidComplete = true;
                character.adventure.setFasterIdleAttack();
                __instance.tooltip.showOverrideTooltip("For creating a level 100 Red Liquid, the global cooldown timer is reduced by 20%! Yes, this also means Idle Attack! Yaaaaay!", 4f);
            }

            else if (!il.waldoComplete && il.maxxedWaldo())
            {
                il.waldoComplete = true;

                long exp = character.addExp(50000L);
                long ap = character.addAP(10000);

                character.portraits.portraitUnlocked[26] = true;

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out every item in the Wanderer's set, congrats! You unlocked a new player portrait in the Fight Boss Menu! You've been awarded:\n"
                    + expText(exp) + "!\n"
                    + apText(ap) + "!\n"
                    + "A new, ultra-rare accessory can now drop from WALDERP!",
                    5f);
            }

            else if (!il.antiWaldoComplete && il.maxxedAntiWaldo())
            {
                il.antiWaldoComplete = true;

                long exp = character.addExp(50000L);
                long ap = character.addAP(10000);

                character.portraits.portraitUnlocked[27] = true;

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out every item in the s'rerednaW set, congrats! You unlocked a new player portrait in the Fight Boss Menu! You've been awarded:\n"
                    + expText(exp) + "!\n"
                    + apText(ap) + "!\n"
                    + "A new, ultra-rare accessory can now drop from WALDERP!",
                    5f);
            }

            else if (!il.badlyDrawnComplete && il.maxxedBadlyDrawn())
            {
                il.badlyDrawnComplete = true;
                character.portraits.portraitUnlocked[28] = true;

                long exp = character.addExp(30000L);
                long ap = character.addAP(5000);

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out every item in the Badly Drawn set, congrats! You unlocked a new player portrait in the Fight Boss Menu! You've been awarded:\n"
                    + expText(exp) + "!\n"
                    + apText(ap) + "!\n"
                    + "Boosts now provide 20% more boostification! (Hey, that's not even a word!)",
                    5f);
            }

            else if (!il.stealthComplete && il.maxxedStealth())
            {
                il.stealthComplete = true;
                character.portraits.portraitUnlocked[29] = true;

                long exp = character.addExp(50000L);
                long ap = character.addAP(10000);

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out every item in the Stealth Set set, congrats! You unlocked a new player portrait in the Fight Boss Menu! You've been awarded:\n"
                    + expText(exp) + "!\n"
                    + apText(ap) + "!\n"
                    + "You can now also find a SUPER rare chest drop in Boring-Ass Earth!",
                    5f);
            }

            else if (!il.beast1complete && il.maxxedBeast1())
            {
                il.beast1complete = true;
                character.portraits.portraitUnlocked[30] = true;

                long exp = character.addExp(100000L);
                long ap = character.addAP(10000);

                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out every item in the Beast Set set, congrats! You unlocked a new player portrait in the Fight Boss Menu! You've been awarded:\n"
                    + expText(exp) + "!\n"
                    + apText(ap) + "!\n"
                    + "Parry now performs an attack that does 3x the damage!",
                    5f);
            }

            else if (!il.brownHeartComplete && il.maxxedBrownHeart())
            {
                il.brownHeartComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Brown Heart Set, Congrats. Now, once every 10 poops you use on a fruit will not be consumed!", 5f);
            }

            else if (!character.inventory.itemList.xlComplete && character.inventory.itemList.maxxedXL())
            {
                character.inventory.itemList.xlComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Wandoos XL Set, Congrats! Wandoos bootup time has now been reduced by 10%!", 5f);
            }

            else if (!il.greenHeartComplete && il.maxxedGreenHeart())
            {
                il.greenHeartComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Green Heart Set, Congrats! Progress towards your next Perk Point (PP) in the I.T.O.P.O.D is now 20% faster!", 5f);
                character.refreshMenus();
            }

            else if (!character.inventory.itemList.itopodKeyComplete && character.inventory.itemList.maxxedItopodKey())
            {
                character.inventory.itemList.itopodKeyComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Pissed Off Key Set, Congrats! Progress towards your next Perk Point (PP) in the I.T.O.P.O.D is now 10% faster!", 5f);
            }

            else if (!character.inventory.itemList.purpleLiquidComplete && character.inventory.itemList.maxxedPurpleLiquid())
            {
                character.inventory.itemList.purpleLiquidComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Purple Liquid Set, Congrats! Beast Mode will now increase your power by 50% instead of 40%!", 5f);
            }

            else if (!il.blueHeartComplete && il.maxxedBlueHeart())
            {
                il.blueHeartComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Blue Heart Set, Congrats! All consumables now grant 10% better effects!", 5f);
            }

            else if (!il.jakeNoteComplete && il.maxxedJakeNote())
            {
                il.jakeNoteComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Scrap of Paper Set, Congrats! You earned a new digger slot!", 5f);
            }

            else if (!il.purpleHeartComplete && il.maxxedPurpleHeart())
            {
                il.purpleHeartComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Purple Heart Set, Congrats! All MacGuffins will now drop 20% more often!", 5f);
            }

            else if (!il.edgyComplete && il.maxxedEdgy())
            {
                il.edgyComplete = true;
                character.addExp(250000L);
                character.portraits.portraitUnlocked[32] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Edgy Set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! You've been awarded " + character.display((double)character.checkExpAdded(250000L)) + " EXP! You also gained a free MacGuffin slot!", 5f);
                character.inventoryController.updateMacguffinCount();
            }

            else if (!il.edgyBootsComplete && il.maxxedEdgyBoots())
            {
                il.edgyBootsComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Boots Set, Congrats! You've unlocked a special drop in The Evilverse!", 5f);
            }

            else if (!il.chocoComplete && il.maxxedChoco())
            {
                il.chocoComplete = true;
                character.portraits.portraitUnlocked[31] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Choco Set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! You've unlocked:\n\n2 rare accessory drops in Chocolate World!\nA new MacGuffin drops in Chocolate World!\nMacGuffins require 10% fewer kills per drop outside of the ITOPOD!\n\nChocolate is awesome!", 5f);
            }

            else if (!il.prettyComplete && il.maxxedPretty())
            {
                il.prettyComplete = true;
                character.portraits.portraitUnlocked[33] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Pretty Pink Princess Set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! You'll now earn PP 10% Faster", 5f);
            }

            else if (!il.nerdComplete && il.maxxedNerd())
            {
                il.nerdComplete = true;
                character.portraits.portraitUnlocked[34] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Greasy Nerd Set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! Now every MacGuffin will drop 1 level higher!", 5f);
            }

            else if (!il.metaComplete && il.maxxedMeta())
            {
                il.metaComplete = true;
                character.portraits.portraitUnlocked[35] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Meta Set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! You've gained +20% NGU Speed!", 5f);
            }

            else if (!il.partyComplete && il.maxxedParty())
            {
                il.partyComplete = true;
                character.portraits.portraitUnlocked[36] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Party Set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! You've gained +5% to your Global Digger Bonus!", 5f);
            }

            else if (!il.godmotherComplete && il.maxxedGodmother())
            {
                il.godmotherComplete = true;
                character.portraits.portraitUnlocked[37] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Mobster Set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! Quests will now reward 15% more QP!", 5f);
            }

            else if (!il.orangeHeartComplete && il.maxxedOrangeHeart())
            {
                il.orangeHeartComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Orange Heart Set, Congrats! Quests will now reward 20% more QP!", 5f);
            }

            else if (!il.sigilComplete && il.maxxedHeroicSigil())
            {
                il.sigilComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Heroic Sigil Set, congrats! Quest Items will now drop 10% more often.", 5f);
            }

            else if (!il.greyHeartComplete && il.maxxedGreyHeart())
            {
                il.greyHeartComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Grey Heart Set, Congrats! Now your Hacks will be 25% faster", 5f);
            }

            else if (!il.evidenceComplete && il.maxxedEvidence() && character.res3.res3On)
            {
                il.evidenceComplete = true;
                character.res3.res3Power += 2f;
                character.res3.capRes3 += 80000L;
                character.res3.res3PerBar += 2L;
                character.arbitrary.res3Potion1Count++;
                character.arbitrary.res3Potion2Count++;
                character.arbitrary.res3Potion3Count++;
                __instance.tooltip.showOverrideTooltip(
                    "You've maxxed out every item in the Incriminating Evidence Set, Congrats! You've gained +2 base "
                    + character.res3.res3Name
                    + " Power, 80K base"
                    + character.res3.res3Name
                    + " Cap and +2 base "
                    + character.res3.res3Name
                    + " Bars! you also gained +1 to each Resource 3 Potion"
                , 5f);
            }

            else if (!il.typoComplete && il.maxxedTypo())
            {
                il.typoComplete = true;
                character.portraits.portraitUnlocked[38] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Typo set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! You've also gained +20% Wish Speed!", 5f);
            }

            else if (!il.fadComplete && il.maxxedFad())
            {
                il.fadComplete = true;
                character.arbitrary.beastButterCount += 3;
                character.portraits.portraitUnlocked[39] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Fad set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! You'll now gain Major Quests 10% faster! You also gained 3 Beast Butter!", 5f);
            }

            else if (!il.jrpgComplete && il.maxxedJRPG())
            {
                il.jrpgComplete = true;
                character.portraits.portraitUnlocked[40] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the JRPG set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! Your ultimate attack is even more ultimate-r now!", 5f);
            }

            else if (!il.exileComplete && il.maxxedExile())
            {
                il.exileComplete = true;
                character.portraits.portraitUnlocked[41] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Exile set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! You gained nothing else though...or have you? Up to you to figure out this mystery!", 5f);
            }

            else if (!il.pinkHeartComplete && il.maxxedPinkHeart())
            {
                il.pinkHeartComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Pink Heart set, Congrats! You unlocked an additional Wish Slot!", 5f);
            }

            else if (!il.severedHeadComplete && il.maxxedSeveredHead())
            {
                il.severedHeadComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Severed Head set, Congrats! You've gained +13.37% Wish Speed! Wouldn't this joke be better used for Hacks though?", 5f);
            }

            else if (!il.radComplete && il.maxxedRad())
            {
                il.radComplete = true;
                character.portraits.portraitUnlocked[47] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Rad set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! you've also gained +5 Max Deck Size", 5f);
            }

            else if (!il.schoolComplete && il.maxxedSchool())
            {
                il.schoolComplete = true;
                character.portraits.portraitUnlocked[48] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Back To School set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! you also gained +15% NGU Speed", 5f);
            }

            else if (!il.westernComplete && il.maxxedWestern())
            {
                il.westernComplete = true;
                character.portraits.portraitUnlocked[49] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Western set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! You've also unlocked a new drop in The West World", 5f);
            }

            else if (!il.spaceComplete && il.maxxedSpace())
            {
                il.spaceComplete = true;
                character.portraits.portraitUnlocked[50] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Space set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! You've also gained 10% improved cook results!", 5f);
            }

            else if (!il.rainbowHeartComplete && il.maxxedRainbowHeart())
            {
                il.rainbowHeartComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Rainbow Heart set, Congrats! You've gained +10% Mayo and Card Generation Speed!", 5f);
            }

            else if (!il.beatingHeartComplete && il.maxxedBeatingHeart())
            {
                il.beatingHeartComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Still-Beating Heart set, Congrats! You've gained +1% Tag Effect!", 5f);
            }

            else if (!il.breadverseComplete && il.maxxedBread())
            {
                il.breadverseComplete = true;
                character.portraits.portraitUnlocked[52] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Bread set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! You can now eat 30 minutes faster!", 5f);
            }

            else if (!il.that70sComplete && il.maxxed70sZone())
            {
                il.that70sComplete = true;
                character.portraits.portraitUnlocked[53] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Disco set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! You also generate slightly less crappier cards!", 5f);
            }

            else if (!il.halloweeniesComplete && il.maxxedHalloweenies())
            {
                il.halloweeniesComplete = true;
                character.portraits.portraitUnlocked[54] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Halloweenies set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! You've also earned +45% PP gain!", 5f);
            }

            else if (!il.rockLobsterComplete && il.maxxedRockLobster())
            {
                il.rockLobsterComplete = true;
                character.portraits.portraitUnlocked[55] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Rock Lobster set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! You also gained +1 tier to ALL CARDS. Hoo yeah!", 5f);
            }

            else if (!il.constructionComplete && il.maxxedConstruction())
            {
                il.constructionComplete = true;
                character.portraits.portraitUnlocked[59] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Construction set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! You also gained 20% Boostier Boosts!", 5f);
            }

            else if (!il.duckComplete && il.maxxedDuck())
            {
                il.duckComplete = true;
                character.portraits.portraitUnlocked[60] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Duck set, Conquacks! You unclucked a new player portrait in the Fight Goss Menu! You also generate 6% Faster Mayo and Cards! QUACK.", 5f);
            }

            else if (!il.netherComplete && il.maxxedNether())
            {
                il.netherComplete = true;
                character.portraits.portraitUnlocked[61] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Nether set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! You've also earned +25% Faster Blood Magic Rituals!", 5f);
            }

            else if (!il.amalgamateComplete && il.maxxedAmalgamate())
            {
                il.amalgamateComplete = true;
                character.portraits.portraitUnlocked[62] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Amalgamate set, Congrats! You unlocked a new player portrait in the Fight Boss Menu! You also gain +10 max deck size!", 5f);
            }

            else if (!il.pirateComplete && il.maxxedPirate())
            {
                il.pirateComplete = true;
                character.portraits.portraitUnlocked[66] = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Pirate set, Congrats! You unlocked a new player portrait in the Fight Boss Menu!", 5f);
            }

            else if (!il.normalBonusAccComplete && il.maxxedNormalBonusAcc())
            {
                il.normalBonusAccComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Bonus Shinies Set (Normal), Congrats! You gained +25% Drop Chance!", 5f);
            }

            else if (!il.evilBonusAccComplete && il.maxxedEvilBonusAcc())
            {
                il.evilBonusAccComplete = true;
                __instance.tooltip.showOverrideTooltip("You've maxxed out every item in the Bonus Shinies Set (Evil), Congrats! You gained 20% to adventure Stats", 5f);
            }

            else
                return false;

            character.refreshMenus();
            return false;
        }
    }
}
