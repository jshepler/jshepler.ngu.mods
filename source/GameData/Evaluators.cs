using System;

namespace jshepler.ngu.mods.GameData
{
    internal class Evaluators
    {
        internal static long TitanQP(int zoneId) { return (long)(titanQP(zoneId) * titanVersionMulti(zoneId)); }
        internal static long TitanPP(int zoneId) { return (long)(titanPP(zoneId) * titanVersionMulti(zoneId)); }
        internal static long TitanAP(int zoneId) { return Plugin.Character.checkAPAdded(titanAP(zoneId)); }

        internal static long TitanExp(int zoneId)
        {
            var maxKillsWithBonus = Plugin.Character.adventure.itopod.perkLevel[34] * 3;
            var kills = titanKills(zoneId);
            var vf = titanVersionMulti(zoneId);
            var exp = titanExp(zoneId) * (kills < maxKillsWithBonus ? 1.5f : 1f) * vf;

            return Plugin.Character.checkExpAdded((long)exp);
        }

        private static int titanKills(int zoneId)
        {
            return zoneId switch
            {
                6 => Plugin.Character.adventure.titan1Kills,
                8 => Plugin.Character.adventure.titan2Kills,
                11 => Plugin.Character.adventure.titan3Kills,
                14 => Plugin.Character.adventure.titan4Kills,
                16 => Plugin.Character.adventure.titan5Kills,
                19 => Plugin.Character.adventure.titan6Kills,
                23 => Plugin.Character.adventure.titan7Kills,
                26 => Plugin.Character.adventure.titan8Kills,
                30 => Plugin.Character.adventure.titan9Kills,
                34 => Plugin.Character.adventure.titan10Kills,
                38 => Plugin.Character.adventure.titan11Kills,
                42 => Plugin.Character.adventure.titan12Kills,
                _ => 0
            };
        }

        private static long titanExp(int zoneId)
        {
            return zoneId switch
            {
                6 => Plugin.Character.adventureController.boss1Exp(),
                8 => Plugin.Character.adventureController.boss2Exp(),
                11 => Plugin.Character.adventureController.boss3Exp(),
                14 => Plugin.Character.adventureController.boss4Exp(),
                16 => Plugin.Character.adventureController.boss5Exp(),
                19 => Plugin.Character.adventureController.boss6Exp(),
                23 => Plugin.Character.adventureController.boss7Exp(),
                26 => Plugin.Character.adventureController.boss8Exp(),
                30 => Plugin.Character.adventureController.boss9Exp(),
                34 => Plugin.Character.adventureController.boss10Exp(),
                38 => Plugin.Character.adventureController.boss11Exp(),
                42 => Plugin.Character.adventureController.boss12Exp(),
                _ => 0
            };
        }

        private static long titanAP(int zoneId)
        {
            return zoneId switch
            {
                6 => Plugin.Character.adventureController.boss1AP(),
                8 => Plugin.Character.adventureController.boss2AP(),
                11 => Plugin.Character.adventureController.boss3AP(),
                14 => Plugin.Character.adventureController.boss4AP(),
                16 => Plugin.Character.adventureController.boss5AP(),
                _ => 0
            };
        }

        private static long titanQP(int zoneId)
        {
            return zoneId switch
            {
                19 => Plugin.Character.adventureController.boss6QP(),
                23 => Plugin.Character.adventureController.boss7QP(),
                26 => Plugin.Character.adventureController.boss8QP(),
                30 => Plugin.Character.adventureController.boss9QP(),
                34 => Plugin.Character.adventureController.boss10QP(),
                38 => Plugin.Character.adventureController.boss11QP(),
                42 => Plugin.Character.adventureController.boss12QP(),
                _ => 0
            };
        }

        private static long titanPP(int zoneId)
        {
            return zoneId switch
            {
                19 => Plugin.Character.adventureController.boss6PP(),
                23 => Plugin.Character.adventureController.boss7PP(),
                26 => Plugin.Character.adventureController.boss8PP(),
                30 => Plugin.Character.adventureController.boss9PP(),
                34 => Plugin.Character.adventureController.boss10PP(),
                38 => Plugin.Character.adventureController.boss11PP(),
                42 => Plugin.Character.adventureController.boss12PP(),
                _ => 0
            };
        }

        private static float titanVersionMulti(int zoneId)
        {
            enemyType et;

            switch (zoneId)
            {
                case 19:
                    et = Plugin.Character.adventure.titan6Version switch
                    {
                        0 => enemyType.bigBoss6V1,
                        1 => enemyType.bigBoss6V2,
                        2 => enemyType.bigBoss6V3,
                        3 => enemyType.bigBoss6V4,
                        _ => enemyType.normal
                    };

                    return Plugin.Character.adventureController.lootDrop.higherVFactor(et);

                case 23:
                    et = Plugin.Character.adventure.titan6Version switch
                    {
                        0 => enemyType.bigBoss7V1,
                        1 => enemyType.bigBoss7V2,
                        2 => enemyType.bigBoss7V3,
                        3 => enemyType.bigBoss7V4,
                        _ => enemyType.normal
                    };

                    return Plugin.Character.adventureController.lootDrop.higherVFactor(et);

                case 26:
                    et = Plugin.Character.adventure.titan6Version switch
                    {
                        0 => enemyType.bigBoss8V1,
                        1 => enemyType.bigBoss8V2,
                        2 => enemyType.bigBoss8V3,
                        3 => enemyType.bigBoss8V4,
                        _ => enemyType.normal
                    };

                    return Plugin.Character.adventureController.lootDrop.higherVFactor(et);

                case 30:
                    et = Plugin.Character.adventure.titan6Version switch
                    {
                        0 => enemyType.bigBoss9V1,
                        1 => enemyType.bigBoss9V2,
                        2 => enemyType.bigBoss9V3,
                        3 => enemyType.bigBoss9V4,
                        _ => enemyType.normal
                    };

                    return Plugin.Character.adventureController.lootDrop.higherVFactor(et);

                case 34:
                    et = Plugin.Character.adventure.titan6Version switch
                    {
                        0 => enemyType.bigBoss10V1,
                        1 => enemyType.bigBoss10V2,
                        2 => enemyType.bigBoss10V3,
                        3 => enemyType.bigBoss10V4,
                        _ => enemyType.normal
                    };

                    return Plugin.Character.adventureController.lootDrop.higherVFactor(et);

                case 38:
                    et = Plugin.Character.adventure.titan6Version switch
                    {
                        0 => enemyType.bigBoss11V1,
                        1 => enemyType.bigBoss11V2,
                        2 => enemyType.bigBoss11V3,
                        3 => enemyType.bigBoss11V4,
                        _ => enemyType.normal
                    };

                    return Plugin.Character.adventureController.lootDrop.higherVFactor(et);

                case 42:
                    et = Plugin.Character.adventure.titan6Version switch
                    {
                        0 => enemyType.bigBoss12V1,
                        1 => enemyType.bigBoss12V2,
                        2 => enemyType.bigBoss12V3,
                        3 => enemyType.bigBoss12V4,
                        _ => enemyType.normal
                    };

                    return Plugin.Character.adventureController.lootDrop.higherVFactor(et);

                default:
                    return 1f;
            }
        }
    }
}
