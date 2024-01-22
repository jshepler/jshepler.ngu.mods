using System.Collections.Generic;
using jshepler.ngu.mods.GameData.DropConditions;

namespace jshepler.ngu.mods.GameData
{
    internal class DropTable
    {
        internal static List<ZoneDrops> Zones;

        private static Items[] _boosts1 = new[] { Items.PowerBoost_1, Items.ToughnessBoost_1, Items.SpecialBoost_1 };
        private static Items[] _boosts2 = new[] { Items.PowerBoost_2, Items.ToughnessBoost_2, Items.SpecialBoost_2 };
        private static Items[] _boosts5 = new[] { Items.PowerBoost_5, Items.ToughnessBoost_5, Items.SpecialBoost_5 };
        private static Items[] _boosts10 = new[] { Items.PowerBoost_10, Items.ToughnessBoost_10, Items.SpecialBoost_10 };
        private static Items[] _boosts20 = new[] { Items.PowerBoost_20, Items.ToughnessBoost_20, Items.SpecialBoost_20 };
        private static Items[] _boosts50 = new[] { Items.PowerBoost_50, Items.ToughnessBoost_50, Items.SpecialBoost_50 };
        private static Items[] _boosts100 = new[] { Items.PowerBoost_100, Items.ToughnessBoost_100, Items.SpecialBoost_100 };
        private static Items[] _boosts200 = new[] { Items.PowerBoost_200, Items.ToughnessBoost_200, Items.SpecialBoost_200 };
        private static Items[] _boosts500 = new[] { Items.PowerBoost_500, Items.ToughnessBoost_500, Items.SpecialBoost_500 };
        private static Items[] _boosts1k = new[] { Items.PowerBoost_1k, Items.ToughnessBoost_1k, Items.SpecialBoost_1k };
        private static Items[] _boosts2k = new[] { Items.PowerBoost_2k, Items.ToughnessBoost_2k, Items.SpecialBoost_2k };
        private static Items[] _boosts5k = new[] { Items.PowerBoost_5k, Items.ToughnessBoost_5k, Items.SpecialBoost_5k };
        private static Items[] _boosts10k = new[] { Items.PowerBoost_10k, Items.ToughnessBoost_10k, Items.SpecialBoost_10k };

        //internal ZoneDrops this[int zoneId] => Zones[zoneId];
        //internal int Count => Zones.Count;

        static DropTable()
        {
            Zones = new();

            // 0 - tutorial
            Zones.Add(new ZoneDrops
            {
                NormalDrops = new DropGroup(100f
                    , new DropItems(0.15f, _boosts1)
                    , new DropItems(0.25f, Items.Tutorial_Stick))

                , BossDrops = new DropGroup(200f
                    , new DropItems(0.07f, 0.08f, Items.Exp)
                    , new DropItems(1.00f
                        , Items.Tutorial_ClothHat
                        , Items.Tutorial_ClothShirt
                        , Items.Tutorial_ClothLeggings
                        , Items.Tutorial_ClothBoots))

                // flubber has custom DC math, hard-coded it in ZoneDropChance.cs
            });

            // 1 - sewers
            Zones.Add(new ZoneDrops
            {
                NormalDrops = new DropGroup(400f
                    , new DropItems(0.15f, _boosts1))

                , BossDrops = new DropGroup(600f
                    , new DropItems(0.085f, 0.10f, Items.Exp)
                    , new DropItems(0.10f, Items.Sewers_TutorialCube)
                    , new DropItems(0.65f
                        , Items.Sewers_CrappHelmet
                        , Items.Sewers_CrappyBoots
                        , Items.Sewers_CrappyLeggings
                        , Items.Sewers_GrossRing
                        , Items.Sewers_RustySword
                        , Items.Sewers_CrackedAmulet))

                , MacGuffinDrop = new MacGuffinDrop(Items.Guff_EnergyPower)
                , QuestItemDrop = new QuestItemDrop(Items.Quest_BitsOfString, ItemSets.Sewers)
            });

            // 2 - forest
            Zones.Add(new ZoneDrops
            {
                NormalDrops = new DropGroup(900f
                    , new DropItems(0.12f, _boosts1)
                    , new DropItems(0.08f, _boosts2)
                    , new DropItems(0.013f, Items.Forest_TubaOfTime))

                , BossDrops = new DropGroup(1500f
                    , new DropItems(0.10f, 0.12f, Items.Exp)
                    , new DropItems(0.013f, Items.Forest_TubaOfTime)
                    , new DropItems(0.50f
                        , Items.Forest_ForestBoots
                        , Items.Forest_ForestChestplate
                        , Items.Forest_ForestHelmet
                        , Items.Forest_ForestLeggings
                        , Items.Forest_KokiriBlade
                        , Items.Forest_MossyRing
                        , Items.Pendant_ForestPendant))

                , EnemyDrops = new[]
                {
                    new EnemyDropGroup(Enemies.Goblin
                        , new DropItems(0.008f, Items.Forest_RingOfApathy)
                        {
                            Condition = new CustomDropCondition(() => Plugin.Character.bossID >= 100 || Plugin.Character.inventory.itemList.itemDropped[(int)Items.Forest_RingOfApathy])
                        })
                }

                , MacGuffinDrop = new MacGuffinDrop(Items.Guff_MagicPower)
                , QuestItemDrop = new QuestItemDrop(Items.Quest_Dreamcatcher, ItemSets.Forest)
            });

            // 3 - cave
            Zones.Add(new ZoneDrops
            {
                NormalDrops = new DropGroup(2200f
                    , new DropItems(0.13f, _boosts1)
                    , new DropItems(0.12f, _boosts2)
                    , new DropItems(0.0125f, Items.Cave_CheeseGrater))

                , BossDrops = new DropGroup(3000f
                    , new DropItems(0.12f, 0.15f, Items.Exp)
                    , new DropItems(0.0125f, Items.Cave_CheeseGrater)
                    , new DropItems(0.75f
                        , Items.Cave_BlueCheeseHelmet
                        , Items.Cave_CheddarAmulet
                        , Items.Cave_CombatCheese
                        , Items.Cave_GoudaChestplate
                        , Items.Cave_HavartiRing
                        , Items.Cave_LimburgerBoots
                        , Items.Cave_MoleHammer
                        , Items.Cave_SwissLeggings
                        , Items.Pendant_ForestPendant))

                , MacGuffinDrop = new MacGuffinDrop(Items.Guff_EnergyCap)
            });

            // 4 - sky
            Zones.Add(new ZoneDrops
            {
                NormalDrops = new DropGroup(4000f
                    , new DropItems(0.08f, _boosts2)
                    , new DropItems(0.08f, _boosts5)
                    , new DropItems(0.01f, Items.Sky_DragonsLeftBall))

                , BossDrops = new DropGroup(6000f
                    , new DropItems(0.16f, 0.2f, Items.Exp)
                    , new DropItems(0.003f, Items.BustedCopyOfWandoos98)
                    , new DropItems(0.01f, Items.Looty_LootyMcLootFace)
                    , new DropItems(0.01f, Items.Sky_PissedOffKey)
                    , new DropItems(0.40f, Items.Pendant_ForestPendant)
                    , new DropItems(0.01f, Items.Sky_DragonsLeftBall))

                , EnemyDrops = new[] { new EnemyDropGroup(Enemies.Icarus_Proudbottom, new DropItems(0.0005f, 0.005f, Items.Poop)) }
                , MacGuffinDrop = new MacGuffinDrop(Items.Guff_MagicCap)
            });

            // 5 - HSB
            Zones.Add(new ZoneDrops
            {
                NormalDrops = new DropGroup(10000f
                    , new DropItems(0.06f, _boosts2)
                    , new DropItems(0.015f, _boosts5)
                    , new DropItems(0.007f, Items.HSB_MagiciteCrystal))

                , BossDrops = new DropGroup(16000f
                    , new DropItems(0.09f, 0.12f, 2, Items.Exp)
                    , new DropItems(0.007f, Items.HSB_MagiciteCrystal)
                    , new DropItems(0.40f
                        , Items.HSB_MagitechAmulet
                        , Items.HSB_MagitechBlade
                        , Items.HSB_MagitechBoots
                        , Items.HSB_MagitechChestplate
                        , Items.HSB_MagitechHelmet
                        , Items.HSB_MagitechLeggings
                        , Items.HSB_MagitechRing
                        , Items.Pendant_ForestPendant))

                , MacGuffinDrop = new MacGuffinDrop(Items.Guff_EnergyNGU)
                , QuestItemDrop = new QuestItemDrop(Items.Quest_MissingPuzzlePiece, ItemSets.HSB)
            });

            // 6 - T1
            Zones.Add(new ZoneDrops
            {
                TitanV1Drops = new DropGroup(250000f
                    , new DropItems(1.00f, 35, Items.Exp)
                    , new DropItems(1.00f, 10, Items.AP)
                    , new DropItems(1.00f, Items.T1_ANumber)
                    , new DropItems(0.10f, Items.Pendant_ForestPendant)
                    , new DropItems(1.00f, Items.BustedCopyOfWandoos98)
                    , new DropItems(1.00f
                        , Items.T1_BloodyCleaver
                        , Items.T1_ChefsApron
                        , Items.T1_ChefsHat
                        , Items.T1_NonSlipShoes
                        , Items.T1_RegularPants)
                    , new DropItems(0.50f
                        , Items.T1_BloodyCleaver
                        , Items.T1_ChefsApron
                        , Items.T1_ChefsHat
                        , Items.T1_NonSlipShoes
                        , Items.T1_RegularPants)
                    , new DropItems(0.15f
                        , Items.T1_BloodyCleaver
                        , Items.T1_ChefsApron
                        , Items.T1_ChefsHat
                        , Items.T1_NonSlipShoes
                        , Items.T1_RegularPants
                        , Items.T1_RawSlabOfMeat
                        , Items.T1_SuspiciousSausageNecklace))
            });

            // 7 - clock
            Zones.Add(new ZoneDrops
            {
                NormalDrops = new DropGroup(30000f
                    , new DropItems(0.03f, 0.15f, _boosts5)
                    , new DropItems(0.03f, 0.15f, _boosts10)
                    , new DropItems(0.005f, Items.Clock_GiantWindupGear))

                , BossDrops = new DropGroup(40000f
                    , new DropItems(0.10f, 0.16f, 2, Items.Exp)
                    , new DropItems(0.005f, Items.Clock_GiantWindupGear)
                    , new DropItems(0.012f, Items.BustedCopyOfWandoos98)
                    , new DropItems(0.30f
                        , Items.Clock_AlarmClock
                        , Items.Clock_ClockworkBoots
                        , Items.Clock_ClockworkChest
                        , Items.Clock_ClockworkHat
                        , Items.Clock_ClockworkPants
                        , Items.Clock_ComicallyOversizedMinuteHand
                        , Items.Clock_SandsOfTime))

                , EnemyDrops = new[]
                {
                    new EnemyDropGroup(Enemies.SUNDAE_BOSS
                        , new DropItems(0f, 0.18f, Items.T10_Puzzle_PickleIceCream)
                        {
                            BonuseDC = 0.18f
                            , Condition = PuzzleDropConditions.T10PuzzleStarted
                        })
                }

                , MacGuffinDrop = new MacGuffinDrop(Items.Guff_MagicNGU)
            });

            // 8 - T2
            Zones.Add(new ZoneDrops
            {
                TitanV1Drops = new DropGroup(400000f
                    , new DropItems(1.00f, 60, Items.Exp)
                    , new DropItems(1.00f, 15, Items.AP)
                    , new DropItems(1.00f, Items.PowerBoost_10)
                    , new DropItems(1.00f, Items.ToughnessBoost_10)
                    , new DropItems(1.00f, Items.SpecialBoost_10)
                    , new DropItems(0.10f, Items.PowerBoost_10)
                    , new DropItems(0.10f, Items.ToughnessBoost_10)
                    , new DropItems(0.10f, Items.SpecialBoost_10)
                    , new DropItems(0.08f, Items.PowerBoost_20)
                    , new DropItems(0.08f, Items.ToughnessBoost_20)
                    , new DropItems(0.08f, Items.SpecialBoost_20)
                    , new DropItems(0.05f, Items.PowerBoost_50)
                    , new DropItems(0.05f, Items.ToughnessBoost_50)
                    , new DropItems(0.05f, Items.SpecialBoost_50)
                    , new DropItems(0.05f, Items.PowerBoost_100)
                    , new DropItems(0.05f, Items.ToughnessBoost_100)
                    , new DropItems(0.05f, Items.SpecialBoost_100)
                    , new DropItems(1.00f, Items.T2_GiantSeed)
                    , new DropItems(0.01f, Items.GCT_MysteriousRedLiquid)
                    , new DropItems(0.10f, Items.Pendant_ForestPendant)
                    , new DropItems(1.00f, Items.BustedCopyOfWandoos98))
            });

            // 9 - 2D
            Zones.Add(new ZoneDrops
            {
                NormalDrops = new DropGroup(65000f
                    , new DropItems(0.07f, 0.15f, _boosts10)
                    , new DropItems(0.07f, 0.15f, _boosts20)
                    , new DropItems(0.005f, Items.TwoD_SinusoidalWave))

                , BossDrops = new DropGroup(90000f
                    , new DropItems(0.05f, 0.15f, 3, Items.Exp)
                    , new DropItems(0.005f, Items.TwoD_SinusoidalWave)
                    , new DropItems(0.32f
                        , Items.TwoD_CircleHelmet
                        , Items.TwoD_KingCirclesAmuletOfHelpingRandomStuff
                        , Items.TwoD_PolygonBoots
                        , Items.TwoD_RectanglePants
                        , Items.TwoD_SquareChestplate
                        , Items.TwoD_TheCube
                        , Items.TwoD_Triangle))

                , MacGuffinDrop = new MacGuffinDrop(Items.Guff_EnergyBar)
                , QuestItemDrop = new QuestItemDrop(Items.Quest_EntireStateOfNorthDakota, ItemSets.TwoD)
            });

            // 10 - AB
            Zones.Add(new ZoneDrops
            {
                NormalDrops = new DropGroup(100000f
                    , new DropItems(0.06f, 0.20f, _boosts10)
                    , new DropItems(0.06f, 0.20f, _boosts20)
                    , new DropItems(0.45f, Items.AB_GhostTypewriter))

                , BossDrops = new DropGroup(140000f
                    , new DropItems(0.03f, 0.10f, 5, Items.Exp)
                    , new DropItems(0.45f, Items.AB_GhostTypewriter)
                    , new DropItems(0.002f, Items.BustedCopyOfWandoos98)
                    , new DropItems(0.30f
                        , Items.AB_AmuletOfSunshineSparklesAndGore
                        , Items.AB_CursedRing
                        , Items.AB_GhostlyChest
                        , Items.AB_PantsOfHorror
                        , Items.AB_SpectralBoots
                        , Items.AB_SpookySword
                        , Items.AB_SpoopyHelmet))

                , EnemyDrops = new[] { new EnemyDropGroup(Enemies.MYSTERIOUS_FIGURE, new DropItems(0.0015f, Items.AB_DragonWings)) }
                , MacGuffinDrop = new MacGuffinDrop(Items.Guff_MagicBar)
            });

            // 11 - T3
            Zones.Add(new ZoneDrops
            {
                TitanV1Drops = new DropGroup(300000f
                    , new DropItems(1.00f, 200, Items.Exp)
                    , new DropItems(1.00f, 50, Items.AP)
                    , new DropItems(0.10f, Items.PowerBoost_100)
                    , new DropItems(0.10f, Items.ToughnessBoost_100)
                    , new DropItems(0.10f, Items.SpecialBoost_100)
                    , new DropItems(1.00f, Items.T3_ScrapOfPaper)
                    , new DropItems(1.00f
                        , Items.T3_OfficeHat
                        , Items.T3_OfficePants
                        , Items.T3_OfficeShirt
                        , Items.T3_OfficeShoes
                        , Items.T3_ThePenIs)
                    , new DropItems(0.60f
                        , Items.T3_OfficeHat
                        , Items.T3_OfficePants
                        , Items.T3_OfficeShirt
                        , Items.T3_OfficeShoes
                        , Items.T3_ThePenIs)
                    , new DropItems(0.10f, Items.T3_OfficeHat)
                    , new DropItems(0.10f, Items.T3_OfficePants)
                    , new DropItems(0.10f, Items.T3_OfficeShirt)
                    , new DropItems(0.10f, Items.T3_OfficeShoes)
                    , new DropItems(0.10f, Items.T3_ThePenIs)
                    , new DropItems(0.25f, Items.T3_RegularTie, Items.T3_GenericPaperweight)
                    , new DropItems(0.02f, Items.T3_Stapler)
                    , new DropItems(0.10f, Items.Pendant_A1P))
            });

            // 12 - AVSP
            Zones.Add(new ZoneDrops
            {
                NormalDrops = new DropGroup(180000f
                    , new DropItems(0.03f, 0.25f, _boosts20)
                    , new DropItems(0.03f, 0.25f, _boosts50)
                    , new DropItems(0.004f, Items.AVSP_GaudyEpaulettes))

                , BossDrops = new DropGroup(240000f
                    , new DropItems(0.01f, 0.10f, 10, Items.Exp)
                    , new DropItems(0.004f, Items.AVSP_GaudyEpaulettes)
                    , new DropItems(0.0015f, Items.AVSP_Beanie)
                    , new DropItems(0.0025f, Items.BustedCopyOfWandoos98)
                    , new DropItems(0.20f
                        , Items.AVSP_GaudyBoots
                        , Items.AVSP_GaudyHat
                        , Items.AVSP_GaudyPants
                        , Items.AVSP_GaudyShirt
                        , Items.AVSP_PaperFan))

                , MacGuffinDrop = new MacGuffinDrop(Items.Guff_Sexy)
                , QuestItemDrop = new QuestItemDrop(Items.Quest_RandomCanadianCoins, ItemSets.Gaudy)
            });

            // 13 - mega
            Zones.Add(new ZoneDrops
            {
                NormalDrops = new DropGroup(220000f
                    , new DropItems(0.011f, 0.15f, _boosts50)
                    , new DropItems(0.011f, 0.15f, _boosts100)
                    , new DropItems(0.002f, Items.Mega_TheFTank))

                , BossDrops = new DropGroup(290000f
                    , new DropItems(0.005f, 0.10f, 15, Items.Exp)
                    , new DropItems(0.002f, Items.Mega_TheFTank)
                    , new DropItems(0.01f, Items.Pendant_A1P)
                    , new DropItems(0.08f
                        , Items.Mega_BeamLaserSword
                        , Items.Mega_MegaBlueJeans
                        , Items.Mega_MegaBoots
                        , Items.Mega_MegaChestplate
                        , Items.Mega_MegaHelmet))

                , MacGuffinDrop = new MacGuffinDrop(Items.Guff_SMART)
                , QuestItemDrop = new QuestItemDrop(Items.Quest_UselssCollegeDiploma, ItemSets.Mega)
            });

            // 14 - T4
            Zones.Add(new ZoneDrops
            {
                TitanV1Drops = new DropGroup(500000f
                    , new DropItems(1.00f, 300, Items.Exp)
                    , new DropItems(1.00f, 60, Items.AP)
                    , new DropItems(1.00f, Items.T4_UUGsArmpitHair)
                    , new DropItems(0.002f, Items.Pendant_ForestPendant)
                    , new DropItems(0.02f, Items.T4_RingOfGreed)
                    , new DropItems(0.02f, Items.T4_RingOfMight)
                    , new DropItems(0.02f, Items.T4_RingOfUtility)
                    , new DropItems(0.02f, Items.T4_RingOfWayTooMuchEnergy)
                    , new DropItems(0.02f, Items.T4_RingOFWayTooMuchMagic)
                    , new DropItems(0.001f, Items.T4_UUGsSpecialRing)
                    {
                        Condition = new SetCompleteDropCondition(ItemSets.UUGsRings)
                    })
            });

            // 15 - BV
            Zones.Add(new ZoneDrops
            {
                NormalDrops = new DropGroup(220000f
                    , new DropItems(0.0035f, 0.25f, _boosts50)
                    , new DropItems(0.0035f, 0.25f, _boosts100)
                    , new DropItems(0.0002f, Items.BV_BeardComb))

                , BossDrops = new DropGroup(400000f
                    , new DropItems(0.002f, 0.10f, 20, Items.Exp)
                    , new DropItems(0.0002f, Items.BV_BeardComb)
                    , new DropItems(0.0002f, Items.BV_AnInfinitelyLongStrandOfBeardHair)
                    , new DropItems(0.006f, Items.Pendant_A1P)
                    , new DropItems(0.01f
                        , Items.BV_BeardedAxe
                        , Items.BV_BraidedBeardLegs
                        , Items.BV_FuzzyOrangeCheetoSlippers
                        , Items.BV_GossamerChest
                        , Items.BV_GrouchoMarxDisguise))

                , EnemyDrops = new[]
                {
                    new EnemyDropGroup(Enemies.ORANGE_TOUPEE_WITH_FISTS
                        , new DropItems(0f, 0.36f, Items.T10_Puzzle_WellDoneSteakWithKetchup)
                        {
                            BonuseDC = 0.36f
                            , Condition = PuzzleDropConditions.T10PuzzleStarted
                        })
                }

                , MacGuffinDrop = new MacGuffinDrop(Items.Guff_DropChance)
                , QuestItemDrop = new QuestItemDrop(Items.Quest_Spittoon, ItemSets.Beardverse)
            });

            // 16 - T5
            Zones.Add(new ZoneDrops
            {
                TitanV1Drops = new DropGroup(1000000f
                    , new DropItems(1.00f, 500, Items.Exp)
                    , new DropItems(1.00f, 70, Items.AP)
                    , new DropItems(1.00f, Items.T5_WanderersCane)
                    , new DropItems(0.005f, Items.T5_WanderersCane)
                    
                    , new DropItems(0.005f, Items.T5_WanderersBoots)
                    , new DropItems(0.005f, Items.T5_WanderersChest)
                    , new DropItems(0.005f, Items.T5_WanderersHat)
                    , new DropItems(0.005f, Items.T5_WanderersPants)

                    , new DropItems(0.005f, Items.T5_stooBsrerednaW)
                    , new DropItems(0.005f, Items.T5_tsehCsrerednaW)
                    , new DropItems(0.005f, Items.T5_taHsrerednaW)
                    , new DropItems(0.005f, Items.T5_stnaPsrerednaW)

                    , new DropItems(0.005f, Items.BustedCopyOfWandoosXL)
                    , new DropItems(0.005f, Items.Pendant_A1P)
                    , new DropItems(0.0001f, Items.T5_FannyPack) { Condition = new SetCompleteDropCondition(ItemSets.Waldo) }
                    , new DropItems(0.0001f, Items.T5_DorkyGlasses) { Condition = new SetCompleteDropCondition(ItemSets.AntiWaldo) })
            });

            // 17 - BDW
            Zones.Add(new ZoneDrops
            {
                NormalDrops = new DropGroup(220000f
                    , new DropItems(0.001f, 0.20f, _boosts100)
                    , new DropItems(0.001f, 0.20f, _boosts200)
                    , new DropItems(1.2E-05f, 0.03f, Items.BDW_RandomCrayons)
                    , new DropItems(6E-05f, 0.05f
                        , Items.BDW_BadlyDrawnChest
                        , Items.BDW_BadlyDrawnFoot
                        , Items.BDW_BadlyDrawnGun
                        , Items.BDW_BadlyDrawnPants
                        , Items.BDW_BadlyDrawnSmileyFace))

                , BossDrops = new DropGroup(500000f
                    , new DropItems(0.0005f, 0.10f, 25, Items.Exp)
                    , new DropItems(1.2E-05f, 0.03f, Items.BDW_RandomCrayons)
                    , new DropItems(0.0005f, 0.10f, Items.Looty_LootyMcLootFace)
                    , new DropItems(1E-05f, 0.01f, Items.Looty_SirLootyMcLootingtonIII)
                    , new DropItems(0.0001f, 0.01f, Items.Pendant_A2P)
                    , new DropItems(5E-05f, 0.01f, Items.BustedCopyOfWandoosXL)
                    , new DropItems(0.00018f, 0.15f
                        , Items.BDW_BadlyDrawnChest
                        , Items.BDW_BadlyDrawnFoot
                        , Items.BDW_BadlyDrawnGun
                        , Items.BDW_BadlyDrawnPants
                        , Items.BDW_BadlyDrawnSmileyFace))

                , MacGuffinDrop = new MacGuffinDrop(Items.Guff_Golden)
            });

            // 18 - BAE
            Zones.Add(new ZoneDrops
            {
                NormalDrops = new DropGroup(280000f
                    , new DropItems(0.00012f, 0.20f, _boosts200)
                    , new DropItems(0.00012f, 0.20f, _boosts500)
                    , new DropItems(6E-06f, 0.02f, Items.BAE_RedLipstick)
                    , new DropItems(3E-05f, 0.04f
                        , Items.BAE_GiantBazooka
                        , Items.BAE_HighHeeledBoots
                        , Items.BAE_NoPants
                        , Items.BAE_StealthyChest
                        , Items.BAE_StealthyHat))

                , BossDrops = new DropGroup(600000f
                    , new DropItems(0.0003f, 0.10f, 30, Items.Exp)
                    , new DropItems(6E-06f, 0.02f, Items.BAE_RedLipstick)
                    , new DropItems(7E-05f, 0.01f, Items.Pendant_A2P)
                    , new DropItems(3E-05f, 0.01f, Items.BustedCopyOfWandoosXL)
                    , new DropItems(7E-06f, 0.01f, Items.Looty_SirLootyMcLootingtonIII)
                    , new DropItems(1E-06f, 0.005f, Items.BAE_TheStealhiestArmor) { Condition = new SetCompleteDropCondition(ItemSets.Stealth) }
                    , new DropItems(9E-05f, 0.10f
                        , Items.BAE_GiantBazooka
                        , Items.BAE_HighHeeledBoots
                        , Items.BAE_NoPants
                        , Items.BAE_StealthyChest
                        , Items.BAE_StealthyHat))

                , MacGuffinDrop = new MacGuffinDrop(Items.Guff_Augment)
            });

            // 19 - T6
            Zones.Add(new ZoneDrops
            {
                TitanV1Drops = new DropGroup(5000000f
                    , new DropItems(1.00f, 750, Items.Exp)
                    , new DropItems(1.00f, 250000, Items.PP)
                    , new DropItems(1.00f, 1, Items.QP) { Condition = WishDropCondition.T6QP }
                    , new DropItems(1.00f, Items.T6_HeroicSigil)
                    , new DropItems(0.0005f, Items.Pendant_A3P)
                    , new DropItems(0.0002f, Items.T6_BaldEgg)
                    , new DropItems(0.0005f, Items.T6_SlimyBoots)
                    , new DropItems(0.0005f, Items.T6_SlimyChest)
                    , new DropItems(0.0005f, Items.T6_SlimyHelmet)
                    , new DropItems(0.0005f, Items.T6_SlimyPants)
                    , new DropItems(0.0005f, Items.T6_TheFistsOfFlubber))

                , TitanV2Drops = new DropGroup(
                      new DropItems(5E-05f, Items.T6_ShrunkenVoodooDoll)
                    , new DropItems(2E-05f, Items.T6_MysteriousPurpleLiquid))

                , TitanV3Drops = new DropGroup(
                      new DropItems(1E-05f, Items.T6_PricelessVanGoghPainting)
                    , new DropItems(5E-06f, Items.T6_GiantApple))

                , TitanV4Drops = new DropGroup(
                      new DropItems(2E-06f, Items.T6_PowerPill)
                    , new DropItems(1E-06f, Items.T6_SmallGerbil))
            });

            // 20 - choco
            Zones.Add(new ZoneDrops
            {
                NormalDrops = new DropGroup(600000f
                    , new DropItems(0.00055f, 0.10f, _boosts200)
                    , new DropItems(0.00055f, 0.10f, _boosts500)
                    , new DropItems(8E-05f, 0.016f, Items.Choco_CandyCornNecklace)
                    , new DropItems(0.00018f, 0.08f
                        , Items.Choco_ChocolateBoots
                        , Items.Choco_ChocolateChest
                        , Items.Choco_ChocolateCrowbar
                        , Items.Choco_ChocolateHelmet
                        , Items.Choco_ChocolatePants))

                , BossDrops = new DropGroup(900000f
                    , new DropItems(0.0002f, 0.03f, 30, Items.Exp)
                    , new DropItems(8E-05f, 0.016f, Items.Choco_CandyCornNecklace)
                    , new DropItems(1E-09f, 0.01f, Items.Pendant_A3P) { BonuseDC = 0.001f }
                    , new DropItems(0.00055f, 0.12f
                        , Items.Choco_ChocolateBoots
                        , Items.Choco_ChocolateChest
                        , Items.Choco_ChocolateCrowbar
                        , Items.Choco_ChocolateHelmet
                        , Items.Choco_ChocolatePants)
                    , new DropItems(0.00018f, 0.12f
                        , Items.Choco_EnergyBarBar
                        , Items.Choco_MagicBarBar)
                        {
                            Condition = new SetCompleteDropCondition(ItemSets.Choco)
                        })

                , EnemyDrops = new[]
                {
                    new EnemyDropGroup(Enemies.Screaming_Chocolate_Fish
                        , new DropItems(0f, 0.48f, Items.T10_Puzzle_CanOfSurstromming)
                        {
                            BonuseDC = 0.48f
                            , Condition = PuzzleDropConditions.T10PuzzleStarted
                        })
                }

                , MacGuffinDrop = new MacGuffinDrop(Items.Guff_Stat)
                , QuestItemDrop = new QuestItemDrop(Items.Quest_Toothbrush, ItemSets.Choco)
            });

            // 21 - EV
            Zones.Add(new ZoneDrops
            {
                NormalDrops = new DropGroup(2.8E+08f
                    , new DropItems(0.00012f, 0.10f, _boosts200)
                    , new DropItems(0.00012f, 0.10f, _boosts500)
                    , new DropItems(2E-05f, 0.011f, Items.EV_EdgyMagiciteCrystal)
                    , new DropItems(7E-05f, 0.08f
                        , Items.EV_EdgyChest
                        , Items.EV_EdgyHelmet
                        , Items.EV_EdgyJawAxe
                        , Items.EV_EdgyPants
                        , Items.EV_LeftEdgyBoot
                        , Items.EV_RightEdgyBoot
                        , Items.EV_CheapPlasticAmulet))

                , BossDrops = new DropGroup(6E+08f
                    , new DropItems(0.0001f, 0.03f, 30, Items.Exp)
                    , new DropItems(2E-05f, 0.011f, Items.EV_EdgyMagiciteCrystal)
                    , new DropItems(1E-10f, 0.015f, Items.Pendant_A3P) { BonuseDC = 0.0015f }
                    , new DropItems(0.00021f, 0.12f
                        , Items.EV_EdgyChest
                        , Items.EV_EdgyHelmet
                        , Items.EV_EdgyJawAxe
                        , Items.EV_EdgyPants
                        , Items.EV_LeftEdgyBoot
                        , Items.EV_RightEdgyBoot
                        , Items.EV_CheapPlasticAmulet)
                    , new DropItems(1.8E-05f, 0.12f, Items.EV_BothEdgyBoots) { Condition = new SetCompleteDropCondition(ItemSets.EdgyBoots) })

                , MacGuffinDrop = new MacGuffinDrop(Items.Guff_EnergyWandoos)
                , QuestItemDrop = new QuestItemDrop(Items.Quest_SeveredHumanThumb, ItemSets.Edgy)
            });

            // 22 - PPPL
            Zones.Add(new ZoneDrops
            {
                NormalDrops = new DropGroup(1E+09f
                    , new DropItems(0.0001f, 0.08f, _boosts500)
                    , new DropItems(0.0001f, 0.08f, _boosts1k)
                    , new DropItems(1.2E-05f, 0.013f, Items.PPPL_CreepyDoll)
                    , new DropItems(3E-05f, 0.08f
                        , Items.PPPL_ClownHat
                        , Items.PPPL_CrappyTutu
                        , Items.PPPL_FabulousSuperChest
                        , Items.PPPL_GiantStickyFoot
                        , Items.PPPL_PretyyPinkSlippers
                        , Items.PPPL_PrettyPinkBow))

                , BossDrops = new DropGroup(5E+09f
                    , new DropItems(3E-05f, 0.03f, 30, Items.Exp)
                    , new DropItems(1.2E-05f, 0.013f, Items.PPPL_CreepyDoll)
                    , new DropItems(2E-11f, 0.02f, Items.Pendant_A3P) { BonuseDC = 0.0015f }
                    , new DropItems(0.0001f, 0.12f
                        , Items.PPPL_ClownHat
                        , Items.PPPL_CrappyTutu
                        , Items.PPPL_FabulousSuperChest
                        , Items.PPPL_GiantStickyFoot
                        , Items.PPPL_PretyyPinkSlippers
                        , Items.PPPL_PrettyPinkBow))

                , EnemyDrops = new[]
                {
                    new EnemyDropGroup(Enemies.Barry_the_Beer_Fairy
                        , new DropItems(0f, 0.3f, Items.T10_Puzzle_JarOfMarmite)
                        {
                            BonuseDC = 0.3f
                            , Condition = PuzzleDropConditions.T10PuzzleStarted
                        })
                }

                , MacGuffinDrop = new MacGuffinDrop(Items.Guff_MagicWandoos)
                , QuestItemDrop = new QuestItemDrop(Items.Quest_SmallerCaterpillar, ItemSets.Pretty)
            });

            // 23 - T7
            Zones.Add(new ZoneDrops
            {
                TitanV1Drops = new DropGroup(1E+10f
                    , new DropItems(1.00f, 1100, Items.Exp)
                    , new DropItems(1.00f, 250000, Items.PP)
                    , new DropItems(1.00f, 1, Items.QP) { Condition = WishDropCondition.T7QP }
                    , new DropItems(1.00f, Items.T7_IncriminatingEvidence)
                    , new DropItems(1.00f
                        , Items.T7_WornOutFedora
                        , Items.T7_SweatStainedNGUShirt
                        , Items.T7_NotSweatStainedUnderpants
                        , Items.T7_NerdyShoes
                        , Items.T7_SuperiorJapaneseKatana
                        , Items.T7_OrdinaryCalculator
                        , Items.T7_AnimeFigurine)
                    , new DropItems(0.00035f, 0.25f, Items.T7_WornOutFedora)
                    , new DropItems(0.00035f, 0.25f, Items.T7_SweatStainedNGUShirt)
                    , new DropItems(0.00035f, 0.25f, Items.T7_NotSweatStainedUnderpants)
                    , new DropItems(0.00035f, 0.25f, Items.T7_NerdyShoes)
                    , new DropItems(0.00035f, 0.25f, Items.T7_SuperiorJapaneseKatana)
                    , new DropItems(0.00023f, 0.25f, Items.T7_OrdinaryCalculator)
                    , new DropItems(0.00035f, 0.25f, Items.T7_AnimeFigurine)
                    , new DropItems(0.00035f, 0.25f, Items.Pendant_A4P)
                    , new DropItems(1.00f, Items.Guff_Adventure) { Condition = EnemiesKilledDropCondition.Walerp5Killed })

                , TitanV2Drops = new DropGroup(
                      new DropItems(0.00027f, 0.25f, Items.T7_D20)
                    , new DropItems(0.00027f, 0.25f, Items.T7_D8))

                , TitanV3Drops = new DropGroup(
                      new DropItems(0.00022f, 0.25f, Items.T7_AnimeBodypillow)
                    , new DropItems(0.00022f, 0.25f, Items.T7_RedMeepleThingy))

                , TitanV4Drops = new DropGroup(
                      new DropItems(0.00017f, 0.25f, Items.T7_BagOfTrash)
                    , new DropItems(0.00017f, 0.25f, Items.T7_HeartShapedPanties))
            });

            // 24 - meta
            Zones.Add(new ZoneDrops
            {
                NormalDrops = new DropGroup(5E+09f
                    , new DropItems(5E-05f, 0.07f, _boosts1k)
                    , new DropItems(5E-05f, 0.07f, _boosts2k)
                    , new DropItems(6E-06f, 0.017f, Items.Meta_TheExponential)
                    , new DropItems(1.5E-05f, 0.04f
                        , Items.Meta_NumericalBoots
                        , Items.Meta_NumericalChest
                        , Items.Meta_NumericalHead
                        , Items.Meta_NumericalLegs
                        , Items.Meta_Number7
                        , Items.Meta_69Charm
                        , Items.Meta_InfinityCharm))

                , BossDrops = new DropGroup(1E+10f
                    , new DropItems(1E-05f, 0.03f, 30, Items.Exp)
                    , new DropItems(6E-06f, 0.017f, Items.Meta_TheExponential)
                    , new DropItems(5E-05f, 0.03f, Items.Pendant_A3P)
                    , new DropItems(1.2E-05f, 0.03f, Items.Looty_SirLootyMcLootingtonIII)
                    , new DropItems(5E-05f, 0.12f
                        , Items.Meta_NumericalBoots
                        , Items.Meta_NumericalChest
                        , Items.Meta_NumericalHead
                        , Items.Meta_NumericalLegs
                        , Items.Meta_Number7
                        , Items.Meta_69Charm
                        , Items.Meta_InfinityCharm))

                , MacGuffinDrop = new MacGuffinDrop(Items.Guff_Number)
            });

            // 25 - IP
            Zones.Add(new ZoneDrops
            {
                NormalDrops = new DropGroup(1E+10f
                    , new DropItems(3E-05f, 0.08f, _boosts1k)
                    , new DropItems(3E-05f, 0.08f, _boosts2k)
                    , new DropItems(1.4E-05f, 0.017f, Items.PT_MtRushmooreRooseveltsNose)
                    , new DropItems(1.1E-05f, 0.04f
                        , Items.IP_PartyHat
                        , Items.IP_PartyWhistle
                        , Items.IP_PizzaBoots
                        , Items.IP_PlasticRedCup
                        , Items.IP_PogmailChest
                        , Items.IP_TearAwayPants
                        , Items.IP_TheGodOfThundersHammer))

                , BossDrops = new DropGroup(3E+10f
                    , new DropItems(3E-05f, 0.03f, 30, Items.Exp)
                    , new DropItems(1.4E-05f, 0.017f, Items.PT_MtRushmooreRooseveltsNose)
                    , new DropItems(3.5E-05f, 0.12f, Items.Pendant_A3P)
                    , new DropItems(1E-05f, 0.12f, Items.Looty_SirLootyMcLootingtonIII)
                    , new DropItems(3.5E-05f, 0.12f
                        , Items.IP_PartyHat
                        , Items.IP_PartyWhistle
                        , Items.IP_PizzaBoots
                        , Items.IP_PlasticRedCup
                        , Items.IP_PogmailChest
                        , Items.IP_TearAwayPants
                        , Items.IP_TheGodOfThundersHammer))

                , MacGuffinDrop = new MacGuffinDrop(Items.Guff_Blood)
            });

            // 26 - T8
            Zones.Add(new ZoneDrops
            {
                TitanV1Drops = new DropGroup(1E+11f
                    , new DropItems(1.00f, 1500, Items.Exp)
                    , new DropItems(1.00f, 250000, Items.PP)
                    , new DropItems(1.00f, 2, Items.QP) { Condition = WishDropCondition.T8QP }
                    , new DropItems(1.00f, Items.T8_SeveredUnicornsHead)
                    , new DropItems(0.0001f, 0.25f, Items.T8_MobsterHat)
                    , new DropItems(0.0001f, 0.25f, Items.T8_MobsterPants)
                    , new DropItems(0.0001f, 0.25f, Items.T8_MobsterVest)
                    , new DropItems(0.0001f, 0.25f, Items.T8_CementBoots)
                    , new DropItems(0.0001f, 0.25f, Items.T8_TommyGun)
                    , new DropItems(0.0001f, 0.25f, Items.T8_Garrote)
                    , new DropItems(0.0001f, 0.25f, Items.T8_BrassKnuckles)
                    , new DropItems(0.0001f, 0.25f, Items.Pendant_A4P)
                    , new DropItems(0.0001f, 0.25f, Items.Looty_KingLooty)
                    , new DropItems(0.0001f, 0.25f, Items.Guff_R3Power) { Condition = EnemiesKilledDropCondition.Walerp5Killed })

                , TitanV2Drops = new DropGroup(
                      new DropItems(7.5E-05f, 0.25f, Items.T8_ViolinCase)
                    , new DropItems(7.5E-05f, 0.25f, Items.T8_MolotovCocktail)
                    , new DropItems(7.5E-05f, 0.25f, Items.Guff_R3Cap) { Condition = EnemiesKilledDropCondition.Walerp5Killed })

                , TitanV3Drops = new DropGroup(
                      new DropItems(6E-05f, 0.25f, Items.T8_TheGodmothersRing)
                    , new DropItems(6E-05f, 0.25f, Items.T8_TheGodmothersWand)
                    , new DropItems(6E-05f, 0.25f, Items.Guff_R3Bar) { Condition = EnemiesKilledDropCondition.Walerp5Killed })

                , TitanV4Drops = new DropGroup(
                      new DropItems(4.5E-05f, 0.25f, Items.T8_LeftFairyWing)
                    , new DropItems(4.5E-05f, 0.25f, Items.T8_RightFairyWing))
            });

            // 27 - typo
            Zones.Add(new ZoneDrops
            {
                NormalDrops = new DropGroup(3E+10f
                    , new DropItems(2.2E-05f, 0.09f, _boosts1k)
                    , new DropItems(2.2E-05f, 0.09f, _boosts2k)
                    , new DropItems(4E-06f, 0.017f, Items.Typo_ThroOdignslug)
                    , new DropItems(9E-06f, 0.04f
                        , Items.Typo_Booms
                        , Items.Typo_ChessPlate
                        , Items.Typo_EyeOfElxu
                        , Items.Typo_Hamlet
                        , Items.Typo_Logs
                        , Items.Typo_TheAsscessory
                        , Items.Typo_WeePin))

                , BossDrops = new DropGroup(5E+10f
                    , new DropItems(2.2E-05f, 0.03f, 35, Items.Exp)
                    , new DropItems(4E-06f, 0.017f, Items.Typo_ThroOdignslug)
                    , new DropItems(2.5E-05f, 0.12f, Items.Pendant_A3P)
                    , new DropItems(6E-06f, 0.12f, Items.Looty_SirLootyMcLootingtonIII)
                    , new DropItems(2.5E-05f, 0.12f
                        , Items.Typo_Booms
                        , Items.Typo_ChessPlate
                        , Items.Typo_EyeOfElxu
                        , Items.Typo_Hamlet
                        , Items.Typo_Logs
                        , Items.Typo_TheAsscessory
                        , Items.Typo_WeePin))
            });

            // 28 - fad
            Zones.Add(new ZoneDrops
            {
                NormalDrops = new DropGroup(6E+10f
                    , new DropItems(1.8E-05f, 0.10f, _boosts2k)
                    , new DropItems(1.8E-05f, 0.10f, _boosts5k)
                    , new DropItems(2.5E-06f, 0.017f, Items.Fad_LinkCable)
                    , new DropItems(7E-06f, 0.04f
                        , Items.Fad_AAABatteryLegs
                        , Items.Fad_DemonicFlurbieChestplate
                        , Items.Fad_HandfulOfKrazyBonez
                        , Items.Fad_RareFoilPokeymanCard
                        , Items.Fad_SlinkyBoots
                        , Items.Fad_SpinningTophat
                        , Items.Fad_TheMalfSlammer))

                , BossDrops = new DropGroup(1E+11f
                    , new DropItems(1.8E-06f, 0.03f, 40, Items.Exp)
                    , new DropItems(2.5E-06f, 0.017f, Items.Fad_LinkCable)
                    , new DropItems(2.1E-05f, 0.08f, Items.Pendant_A3P)
                    , new DropItems(7E-06f, 0.08f, Items.Looty_SirLootyMcLootingtonIII)
                    , new DropItems(2.1E-05f, 0.12f
                        , Items.Fad_AAABatteryLegs
                        , Items.Fad_DemonicFlurbieChestplate
                        , Items.Fad_HandfulOfKrazyBonez
                        , Items.Fad_RareFoilPokeymanCard
                        , Items.Fad_SlinkyBoots
                        , Items.Fad_SpinningTophat
                        , Items.Fad_TheMalfSlammer))
            });

            // 29 - JRPG
            Zones.Add(new ZoneDrops
            {
                NormalDrops = new DropGroup(1E+11f
                    , new DropItems(1.5E-05f, 0.10f, _boosts2k)
                    , new DropItems(1.5E-05f, 0.10f, _boosts5k)
                    , new DropItems(2E-06f, 0.017f, Items.JRPG_HandCursor)
                    , new DropItems(5.5E-06f, 0.04f
                        , Items.JRPG_AnimeHeroWing
                        , Items.JRPG_BusterSwordBottom
                        , Items.JRPG_BusterSwordLower
                        , Items.JRPG_BusterSwordTop
                        , Items.JRPG_BusterSwordUpper
                        , Items.JRPG_GiftShopBusterSwordReplica
                        , Items.JRPG_GiganticZipper))

                , BossDrops = new DropGroup(1.3E+11f
                    , new DropItems(1.8E-05f, 0.03f, 45, Items.Exp)
                    , new DropItems(1.8E-05f, 0.12f, Items.Pendant_A3P)
                    , new DropItems(5.5E-06f, 0.12f, Items.Looty_SirLootyMcLootingtonIII)
                    , new DropItems(2E-06f, 0.017f, Items.JRPG_HandCursor)
                    , new DropItems(1.8E-05f, 0.12f
                        , Items.JRPG_AnimeHeroWing
                        , Items.JRPG_BusterSwordBottom
                        , Items.JRPG_BusterSwordLower
                        , Items.JRPG_BusterSwordTop
                        , Items.JRPG_BusterSwordUpper
                        , Items.JRPG_GiftShopBusterSwordReplica
                        , Items.JRPG_GiganticZipper))

                , EnemyDrops = new[]
                {
                    new EnemyDropGroup(Enemies.The_Annoying_Fan
                        , new DropItems(0f, 0.3f, Items.T10_Puzzle_PizzleWithPineapple)
                        {
                            BonuseDC = 0.3f
                            , Condition = PuzzleDropConditions.T10PuzzleStarted
                        })
                }
            });

            // 30 - T9
            Zones.Add(new ZoneDrops
            {
                TitanV1Drops = new DropGroup(1E+12f
                    , new DropItems(1.00f, 2500, Items.Exp)
                    , new DropItems(1.00f, 400000, Items.PP)
                    , new DropItems(1.00f, 3, Items.QP) { Condition = WishDropCondition.T9QP }
                    , new DropItems(1.00f, Items.T9_StillBeatingHeart)
                    , new DropItems(2E-05f, 0.25f, Items.T9_HatOfGreed)
                    , new DropItems(2E-05f, 0.25f, Items.T9_BlueEyesWhiteChestplate)
                    , new DropItems(2E-05f, 0.25f, Items.T9_TrapPants)
                    , new DropItems(2E-05f, 0.25f, Items.T9_AllTheOtherTitansMissingShoes)
                    , new DropItems(2E-05f, 0.25f, Items.T9_TheDiskOfDueling)
                    , new DropItems(2E-05f, 0.25f, Items.T9_TheJoker)
                    , new DropItems(2E-05f, 0.25f, Items.T9_AntlersOfTheExile)
                    , new DropItems(1.5E-05f, 0.25f, Items.Pendant_A4P)
                    , new DropItems(1.5E-05f, 0.25f, Items.Looty_KingLooty)
                    , new DropItems(0f, 0.02f, Items.T9_Clue_SackOfTheExile)
                        {
                            BonuseDC = 0.02f
                            , Condition = new SetCompleteDropCondition(ItemSets.Exile)
                        }
                    , new DropItems(0f, 0.25f, Items.T9_FaceOfTheExile)
                        {
                            BonuseDC = 0.25f
                            , Condition = new SetCompleteDropCondition(ItemSets.Exile)
                        }
                    , new DropItems(1E-06f, 0.25f, Items.T9_BlueEyesUltimateChestplate)
                        {
                            Condition = new CustomDropCondition(() => Plugin.Character.adventure.titan9SpecialReward)
                        })

                , TitanV2Drops = new DropGroup(
                    new DropItems(1E-05f, 0.25f, Items.T9_TheCreditCard)
                    , new DropItems(1E-05f, 0.25f, Items.T9_TentacleOfTheExile))

                , TitanV3Drops = new DropGroup(
                    new DropItems(6E-06f, 0.25f, Items.T9_TheSKipCard)
                    , new DropItems(6E-06f, 0.25f, Items.T9_AntennaeOfTheExile))

                , TitanV4Drops = new DropGroup(
                    new DropItems(4E-06f, 0.25f, Items.T9_TheBlackLotus)
                    , new DropItems(4E-06f, 0.25f, Items.T9_BusterOfTheExile))
            });

            // 31 - rad
            Zones.Add(new ZoneDrops
            {
                NormalDrops = new DropGroup(2E+11f
                    , new DropItems(6E-07f, 0.15f, _boosts2k)
                    , new DropItems(6E-07f, 0.15f, _boosts5k)
                    , new DropItems(8E-08f, 0.017f, Items.Rad_RadMixtape)
                    , new DropItems(2E-07f, 0.05f
                        , Items.Rad_CoolShades
                        , Items.Rad_FlaminHotShorts
                        , Items.Rad_LeatherJacket
                        , Items.Rad_NotDrugs
                        , Items.Rad_Nunchuks
                        , Items.Rad_Skateboard
                        , Items.Rad_TheGloveOfPower))

                , BossDrops = new DropGroup(3E+11f
                    , new DropItems(6E-07f, 0.15f, 450, Items.Exp)
                    , new DropItems(1.2E-06f, 0.12f, Items.Pendant_A4P)
                    , new DropItems(4E-07f, 0.12f, Items.Looty_KingLooty)
                    , new DropItems(8E-08f, 0.017f, Items.Rad_RadMixtape)
                    , new DropItems(6E-07f, 0.15f
                        , Items.Rad_CoolShades
                        , Items.Rad_FlaminHotShorts
                        , Items.Rad_LeatherJacket
                        , Items.Rad_NotDrugs
                        , Items.Rad_Nunchuks
                        , Items.Rad_Skateboard
                        , Items.Rad_TheGloveOfPower))
            });

            // 32 - BTS
            Zones.Add(new ZoneDrops
            {
                NormalDrops = new DropGroup(1.5E+14f
                    , new DropItems(4E-07f, 0.10f, _boosts5k)
                    , new DropItems(4E-07f, 0.10f, _boosts10k)
                    , new DropItems(1.5E-07f, 0.05f
                        , Items.BTS_DunceCap
                        , Items.BTS_FloppyElasticRule
                        , Items.BTS_SchoolJersey
                        , Items.BTS_ShoesWithWheels
                        , Items.BTS_TheS
                        , Items.BTS_UltrawidePants
                        , Items.BTS_Walkman))

                , BossDrops = new DropGroup(1.7E+14f
                    , new DropItems(4.5E-07f, 0.15f, 500, Items.Exp)
                    , new DropItems(4.5E-07f, 0.12f, Items.Pendant_A5P)
                    , new DropItems(1.5E-07f, 0.12f, Items.Looty_EmperorLooty)
                    , new DropItems(4.5E-07f, 0.15f
                        , Items.BTS_DunceCap
                        , Items.BTS_FloppyElasticRule
                        , Items.BTS_SchoolJersey
                        , Items.BTS_ShoesWithWheels
                        , Items.BTS_TheS
                        , Items.BTS_UltrawidePants
                        , Items.BTS_Walkman))
            });

            // 33 - WW
            Zones.Add(new ZoneDrops
            {
                NormalDrops = new DropGroup(3E+14f
                    , new DropItems(2.5E-07f, 0.15f, _boosts5k)
                    , new DropItems(2.5E-07f, 0.15f, _boosts10k)
                    , new DropItems(1E-07f, 0.04f
                        , Items.WW_10LitreHat
                        , Items.WW_AsfulChaps
                        , Items.WW_AsslessVest
                        , Items.WW_BattleCorgi
                        , Items.WW_ExtraSpikySpurs
                        , Items.WW_PinkBandanna
                        , Items.WW_TheSixShooter)
                    , new DropItems(2E-08f, 0.12f, Items.WW_9mmBeretta) { Condition = new SetCompleteDropCondition(ItemSets.Western) })

                , BossDrops = new DropGroup(4E+14f
                    , new DropItems(3E-07f, 0.15f, 600, Items.Exp)
                    , new DropItems(1E-06f, 0.12f, Items.Pendant_A5P)
                    , new DropItems(3E-07f, 0.12f, Items.Looty_EmperorLooty)
                    , new DropItems(3E-07f, 0.15f
                        , Items.WW_10LitreHat
                        , Items.WW_AsfulChaps
                        , Items.WW_AsslessVest
                        , Items.WW_BattleCorgi
                        , Items.WW_ExtraSpikySpurs
                        , Items.WW_PinkBandanna
                        , Items.WW_TheSixShooter)
                    , new DropItems(6E-08f, 0.12f, Items.WW_9mmBeretta) { Condition = new SetCompleteDropCondition(ItemSets.Western) })
            });

            // 34 - T10
            Zones.Add(new ZoneDrops
            {
                TitanV1Drops = new DropGroup(2E+15f
                    , new DropItems(1.00f, 4000, Items.Exp)
                    , new DropItems(1.00f, 500000, Items.PP)
                    , new DropItems(1.00f, 4, Items.QP) { Condition = WishDropCondition.T10QP }
                    , new DropItems(1E-06f, 0.25f, Items.T10_SpaceHelmet)
                    , new DropItems(1E-06f, 0.25f, Items.T10_SpaceSuitChest)
                    , new DropItems(1E-06f, 0.25f, Items.T10_SpaceSuitLegs)
                    , new DropItems(1E-06f, 0.25f, Items.T10_SpaceBoots)
                    , new DropItems(1E-06f, 0.25f, Items.T10_SpaceGun)
                    , new DropItems(1E-06f, 0.25f, Items.T10_Manhole)
                    , new DropItems(1E-06f, 0.25f, Items.T10_RedShirt)
                    , new DropItems(1E-06f, 0.25f, Items.T10_TheCricket)
                    , new DropItems(1E-06f, 0.25f, Items.Pendant_A5P)
                    , new DropItems(1E-06f, 0.25f, Items.Looty_EmperorLooty))

                , TitanV2Drops = new DropGroup(
                    new DropItems(6E-07f, 0.25f, Items.T10_EvilRubberDucky)
                    , new DropItems(6E-07f, 0.25f, Items.T10_GasGiant))

                , TitanV3Drops = new DropGroup(
                    new DropItems(4E-07f, 0.25f, Items.T10_InanimateCarbonRod)
                    , new DropItems(4E-07f, 0.25f, Items.T10_FunkyKleinBottle))

                , TitanV4Drops = new DropGroup(
                    new DropItems(3E-07f, 0.25f, Items.T10_GiantAlienBugNest)
                    , new DropItems(3E-07f, 0.25f, Items.T10_TheKey))
            });

            // 35 - breadverse
            Zones.Add(new ZoneDrops
            {
                NormalDrops = new DropGroup(1.2E+15f
                    , new DropItems(1E-07f, 0.15f, _boosts5k)
                    , new DropItems(1E-07f, 0.15f, _boosts10k)
                    , new DropItems(4E-08f, 0.04f
                        , Items.Bread_BreadBowlHelmet
                        , Items.Bread_CreamPie
                        , Items.Bread_DayOldBaguette
                        , Items.Bread_FlourSackPants
                        , Items.Bread_GingerbreadBoots
                        , Items.Bread_PaperThinCrepeCape
                        , Items.Bread_RollingPin
                        , Items.Bread_SpoonfulOfYeast))

                , BossDrops = new DropGroup(2E+15f
                    , new DropItems(1.2E-07f, 0.15f, 800, Items.Exp)
                    , new DropItems(4E-07f, 0.12f, Items.Pendant_A5P)
                    , new DropItems(1.2E-07f, 0.12f, Items.Looty_EmperorLooty)
                    , new DropItems(1.2E-07f, 0.15f
                        , Items.Bread_BreadBowlHelmet
                        , Items.Bread_CreamPie
                        , Items.Bread_DayOldBaguette
                        , Items.Bread_FlourSackPants
                        , Items.Bread_GingerbreadBoots
                        , Items.Bread_PaperThinCrepeCape
                        , Items.Bread_RollingPin
                        , Items.Bread_SpoonfulOfYeast))
            });

            // 39 - 70's
            Zones.Add(new ZoneDrops
            {
                NormalDrops = new DropGroup(2.5E+15f
                    , new DropItems(6E-08f, 0.15f, _boosts10k)
                    , new DropItems(6E-08f, 0.15f, _boosts10k)
                    , new DropItems(2.5E-08f, 0.04f
                        , Items.Seventies_BellBottoms
                        , Items.Seventies_BitOfWhitePowder
                        , Items.Seventies_DiscoBallHelmet
                        , Items.Seventies_DiscoShirt
                        , Items.Seventies_RollerSkates
                        , Items.Seventies_RustyOldSabre
                        , Items.Seventies_SomeRollingPaper
                        , Items.Seventies_VinylRecordShard))

                , BossDrops = new DropGroup(3E+15f
                    , new DropItems(8E-08f, 0.15f, 1000, Items.Exp)
                    , new DropItems(2.5E-07f, 0.12f, Items.Pendant_A5P)
                    , new DropItems(8E-08f, 0.12f, Items.Looty_EmperorLooty)
                    , new DropItems(8E-08f, 0.15f
                        , Items.Seventies_BellBottoms
                        , Items.Seventies_BitOfWhitePowder
                        , Items.Seventies_DiscoBallHelmet
                        , Items.Seventies_DiscoShirt
                        , Items.Seventies_RollerSkates
                        , Items.Seventies_RustyOldSabre
                        , Items.Seventies_SomeRollingPaper
                        , Items.Seventies_VinylRecordShard))
            });

            // 37 - halloweenies
            Zones.Add(new ZoneDrops
            {
                NormalDrops = new DropGroup(5E+15f
                    , new DropItems(4E-08f, 0.15f, _boosts10k)
                    , new DropItems(4E-08f, 0.15f, _boosts10k)
                    , new DropItems(1.6E-08f, 0.04f
                        , Items.Halloweenies_Broomstick
                        , Items.Halloweenies_FuzzyBoots
                        , Items.Halloweenies_GiantScythe
                        , Items.Halloweenies_NeckBolts
                        , Items.Halloweenies_OrdinaryApple
                        , Items.Halloweenies_PandorasBox
                        , Items.Halloweenies_RoleOfToiletPaper
                        , Items.Halloweenies_SkeletonShirt))

                , BossDrops = new DropGroup(6E+15f
                    , new DropItems(5E-08f, 0.15f, 1200, Items.Exp)
                    , new DropItems(1.6E-07f, 0.12f, Items.Pendant_A5P)
                    , new DropItems(6E-08f, 0.12f, Items.Looty_EmperorLooty)
                    , new DropItems(5E-08f, 0.15f
                        , Items.Halloweenies_Broomstick
                        , Items.Halloweenies_FuzzyBoots
                        , Items.Halloweenies_GiantScythe
                        , Items.Halloweenies_NeckBolts
                        , Items.Halloweenies_OrdinaryApple
                        , Items.Halloweenies_PandorasBox
                        , Items.Halloweenies_RoleOfToiletPaper
                        , Items.Halloweenies_SkeletonShirt))
            });

            // 38 - T11
            Zones.Add(new ZoneDrops
            {
                TitanV1Drops = new DropGroup(2E+16f
                    , new DropItems(1.00f, 6000, Items.Exp)
                    , new DropItems(1.00f, 700000, Items.PP)
                    , new DropItems(1.00f, 5, Items.QP) { Condition = WishDropCondition.T11QP }
                    , new DropItems(1E-07f, 0.25f, Items.T11_Bandanna)
                    , new DropItems(1E-07f, 0.25f, Items.T11_BrokenDrum)
                    , new DropItems(1E-07f, 0.25f, Items.T11_StonehengePants)
                    , new DropItems(1E-07f, 0.25f, Items.T11_PlatformBoots)
                    , new DropItems(1E-07f, 0.25f, Items.T11_Rocket)
                    , new DropItems(1E-07f, 0.25f, Items.T11_PetRock)
                    , new DropItems(1E-07f, 0.25f, Items.T11_RollingStone)
                    , new DropItems(1E-07f, 0.25f, Items.T11_GiantDrumsticks)
                    , new DropItems(1E-07f, 0.25f, Items.Pendant_A6P)
                    , new DropItems(1E-07f, 0.25f, Items.Looty_GalacticHeraldLooty))

                , TitanV2Drops = new DropGroup(
                    new DropItems(6.5E-08f, 0.25f, Items.T11_SkippingStone)
                    , new DropItems(6.5E-08f, 0.25f, Items.T11_BedRock))

                , TitanV3Drops = new DropGroup(
                    new DropItems(4E-08f, 0.25f, Items.T11_RockCandy)
                    , new DropItems(4E-08f, 0.25f, Items.T11_BrokenPairOfScissors))

                , TitanV4Drops = new DropGroup(
                    new DropItems(3E-08f, 0.25f, Items.T11_PortableStairwayToHeaven)
                    , new DropItems(3E-08f, 0.25f, Items.T11_Amplifier))
            });

            // 39 - construction
            Zones.Add(new ZoneDrops
            {
                NormalDrops = new DropGroup(1E+16f
                    , new DropItems(2.5E-08f, 0.16f, _boosts10k)
                    , new DropItems(2.5E-08f, 0.16f, _boosts10k)
                    , new DropItems(1E-08f, 0.04f
                        , Items.Contruction_GiantWreckingBall
                        , Items.Contruction_Hardhat
                        , Items.Contruction_HighVisibilityVest
                        , Items.Contruction_LevelLevel
                        , Items.Contruction_SteelToedBoots
                        , Items.Contruction_Toolbox
                        , Items.Contruction_WoodenHammer
                        , Items.Contruction_YetAnotherGenericPairOfJeans))

                , BossDrops = new DropGroup(1.2E+16f
                    , new DropItems(4E-08f, 0.15f, 1200, Items.Exp)
                    , new DropItems(1E-07f, 0.12f, Items.Pendant_A6P)
                    , new DropItems(4E-08f, 0.12f, Items.Looty_GalacticHeraldLooty)
                    , new DropItems(3E-08f, 0.15f
                        , Items.Contruction_GiantWreckingBall
                        , Items.Contruction_Hardhat
                        , Items.Contruction_HighVisibilityVest
                        , Items.Contruction_LevelLevel
                        , Items.Contruction_SteelToedBoots
                        , Items.Contruction_Toolbox
                        , Items.Contruction_WoodenHammer
                        , Items.Contruction_YetAnotherGenericPairOfJeans))
            });

            // 40 - duck
            Zones.Add(new ZoneDrops
            {
                NormalDrops = new DropGroup(2E+16f
                    , new DropItems(2E-08f, 0.17f, _boosts10k)
                    , new DropItems(2E-08f, 0.17f, _boosts10k)
                    , new DropItems(8E-09f, 0.05f
                        , Items.Duck_DuckCaller
                        , Items.Duck_DuckDuckShorts
                        , Items.Duck_DuckSlippers
                        , Items.Duck_FakeDuckbill
                        , Items.Duck_InflatableDuckyInnertube
                        , Items.Duck_Shotgun
                        , Items.Duck_SomeDucktTape
                        , Items.Duck_TheZapper))

                , BossDrops = new DropGroup(2.4E+16f
                    , new DropItems(3.3E-08f, 0.15f, 1200, Items.Exp)
                    , new DropItems(8E-08f, 0.12f, Items.Pendant_A6P)
                    , new DropItems(3E-08f, 0.12f, Items.Looty_GalacticHeraldLooty)
                    , new DropItems(2.4E-08f, 0.15f
                        , Items.Duck_DuckCaller
                        , Items.Duck_DuckDuckShorts
                        , Items.Duck_DuckSlippers
                        , Items.Duck_FakeDuckbill
                        , Items.Duck_InflatableDuckyInnertube
                        , Items.Duck_Shotgun
                        , Items.Duck_SomeDucktTape
                        , Items.Duck_TheZapper))
            });

            // 41 - nether
            Zones.Add(new ZoneDrops
            {
                NormalDrops = new DropGroup(4E+16f
                    , new DropItems(1.6E-08f, 0.17f, _boosts10k)
                    , new DropItems(1.6E-08f, 0.17f, _boosts10k)
                    , new DropItems(6E-09f, 0.05f
                        , Items.Nether_BlackTulip
                        , Items.Nether_Clogs
                        , Items.Nether_DutchHat
                        , Items.Nether_PocketNetherlands
                        , Items.Nether_RestOfTheCombatCheese
                        , Items.Nether_StoopwaffelPants
                        , Items.Nether_WeaponizedHollandaiseSauce
                        , Items.Nether_WindmillShirt))

                , BossDrops = new DropGroup(5E+16f
                    , new DropItems(1.8E-08f, 0.15f, 1200, Items.Exp)
                    , new DropItems(6E-08f, 0.12f, Items.Pendant_A6P)
                    , new DropItems(2.4E-08f, 0.12f, Items.Looty_GalacticHeraldLooty)
                    , new DropItems(1.8E-08f, 0.15f
                        , Items.Nether_BlackTulip
                        , Items.Nether_Clogs
                        , Items.Nether_DutchHat
                        , Items.Nether_PocketNetherlands
                        , Items.Nether_RestOfTheCombatCheese
                        , Items.Nether_StoopwaffelPants
                        , Items.Nether_WeaponizedHollandaiseSauce
                        , Items.Nether_WindmillShirt))
            });

            // 42 - T12
            Zones.Add(new ZoneDrops
            {
                TitanV1Drops = new DropGroup(1.5E+17f
                    , new DropItems(1.00f, 8000, Items.Exp)
                    , new DropItems(1.00f, 1e+6f, Items.PP)
                    , new DropItems(1.00f, 6, Items.QP) { Condition = WishDropCondition.T12QP }
                    , new DropItems(1.4E-08f, 0.25f, Items.T12_ChofficeHatOfGreed)
                    , new DropItems(1.4E-08f, 0.25f, Items.T12_WoodenOfficeApronOfMight)
                    , new DropItems(1.4E-08f, 0.25f, Items.T12_PapapapantstststsOfUtility)
                    , new DropItems(1.4E-08f, 0.25f, Items.T12_Shoe)
                    , new DropItems(1.4E-08f, 0.25f, Items.T12_TheDeathstick)
                    , new DropItems(1.4E-08f, 0.25f, Items.T12_CorruptedLeaf)
                    , new DropItems(1.4E-08f, 0.25f, Items.T12_8OldAccessoriesGluedTogether)
                    , new DropItems(1.4E-08f, 0.25f, Items.T12_UUGsBigBookOfInsults)
                    , new DropItems(1.4E-08f, 0.25f, Items.Pendant_A7P)
                    , new DropItems(1.4E-08f, 0.25f, Items.Looty_SupremeIntelligenceLooty)
                    , new DropItems(1.4E-08f, 0.25f, Items.TheEnd_T12V1))

                , TitanV2Drops = new DropGroup(
                    new DropItems(1E-08f, 0.25f, Items.T12_RawSlabOfWood)
                    , new DropItems(1E-08f, 0.25f, Items.TheEnd_T12V2))

                , TitanV3Drops = new DropGroup(
                    new DropItems(8E-09f, 0.25f, Items.T12_TieOfApathy)
                    , new DropItems(8E-09f, 0.25f, Items.TheEnd_T12V3))

                , TitanV4Drops = new DropGroup(
                    new DropItems(6E-09f, 0.25f, Items.T12_TheTitanEffigy)
                    , new DropItems(6E-09f, 0.25f, Items.TheEnd_T12V4))
            });

            // 43 - TAS
            Zones.Add(new ZoneDrops
            {
                NormalDrops = new DropGroup(8E+16f
                    , new DropItems(1E-08f, 0.17f, _boosts10k)
                    , new DropItems(1E-08f, 0.17f, _boosts10k)
                    , new DropItems(4E-09f, 0.05f
                        , Items.TAS_Compass
                        , Items.TAS_GiantsEyepatch
                        , Items.TAS_PirateHat
                        , Items.TAS_PirateyPants
                        , Items.TAS_PirateyPeglegs
                        , Items.TAS_SwashbucklerChest
                        , Items.TAS_TheCutlass
                        , Items.TAS_TheFlintlock))

                , BossDrops = new DropGroup(1.6E+17f
                    , new DropItems(1.2E-08f, 0.15f, 1200, Items.Exp)
                    , new DropItems(4E-08f, 0.12f, Items.Pendant_A6P)
                    , new DropItems(1.8E-08f, 0.12f, Items.Looty_GalacticHeraldLooty)
                    , new DropItems(1.2E-08f, 0.15f
                        , Items.TAS_Compass
                        , Items.TAS_GiantsEyepatch
                        , Items.TAS_PirateHat
                        , Items.TAS_PirateyPants
                        , Items.TAS_PirateyPeglegs
                        , Items.TAS_SwashbucklerChest
                        , Items.TAS_TheCutlass
                        , Items.TAS_TheFlintlock))
            });

            // 44 - T13
            Zones.Add(new ZoneDrops()); // no drops

            // 45 - T14
            Zones.Add(new ZoneDrops
            {
                TitanV1Drops = new DropGroup(new DropItems(1.00f, Items.TheEnd_T14))
            });
        }
    }
}
