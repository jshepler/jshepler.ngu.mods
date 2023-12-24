using System.Collections.Generic;

namespace jshepler.ngu.mods.GameData
{
    internal static class MoneyPit
    {
        public static Dictionary<int, List<string>> TierRewards = new()
        {
            // tier 1
            { 7, new() {
                "Power Boost"
                , "Toughness Boost"
                , "Special Boost"
                , "Adv Power"
                , "Adv Toughness"
                , "Max Health"
                , "Health Regen"
                , "Nothing" // code does random(1, 9) but only has 7 options, 8th drops to the default "Pit belches" - i.e. "nothing"
            } },

            // tier 2-4
            { 13, new() {
                "Power Boost"
                , "Toughness Boost"
                , "Special Boost"
                , "Random Equipped Item LevelUp"
                , "Exp"
                , "Adv Power"
                , "Adv Toughness"
                , "Max Health"
                , "Health Regen"
            } },

            // tier 5
            { 15, new() {
                "Power Boost"
                , "Toughness Boost"
                , "Special Boost"
                , "All Equipped Items LevelUp"
                , "Exp"
                , "Yggdrasil Seeds"
                , "Adv Power"
                , "Adv Toughness"
                , "Max Health"
                , "Health Regen"
            } },

            // tier 6
            { 18, new() {
                "Power Boost"
                , "Toughness Boost"
                , "Special Boost"
                , "All Equipped Items LevelUp"
                , "Exp"
                , "Yggdrasil Seeds"
                , "Adv Power"
                , "Adv Toughness"
                , "Max Health"
                , "Health Regen"
                , "Wandoos Pit Level"
            } },

            // tier 7-11
            { 50, new() {
                "Power Boost"
                , "Toughness Boost"
                , "Special Boost"
                , "All Equipped Items LevelUp"
                , "Exp"
                , "Yggdrasil Seeds"
                , "Adv Power"
                , "Adv Toughness"
                , "Max Health"
                , "Health Regen"
                , "Wandoos Pit Level"
                , "ALL Daycare Items LevelUp"
            } },

            // tier 12-16
            { int.MaxValue, new() {
                "Small Iron Pill (adv stats)"
                , "ALL Equipped Items LevelUp"
                , "Exp"
                , "Small Pomegranate (seeds)"
                , "ALL Daycare Items LevelUp"
            } },
        };
    }
}


/*
        public static Dictionary<int, List<string>> PitTierRewards = new()
        {
            // tier 1
            { 7, new() {
                "Power Boost 1"
                , "Toughness Boost 1"
                , "Special Boost 1"
                , "+1 Adv Power"
                , "+1 Adv Toughness"
                , "+10 Max Health"
                , "+0.1 Health Regen"
                , "Nothing" // code does random(1, 9) but only has 7 options, 8th drops to the default "Pit belches" - i.e. "nothing"
            } },

            // tier 2
            { 9, new() {
                "Power Boost 2"
                , "Toughness Boost 2"
                , "Special Boost 2"
                , "Random Equipped Item Level + 1"
                , "+1 Exp"
                , "+2 Adv Power"
                , "+2 Adv Toughness"
                , "+20 Max Health"
                , "+0.1 Health Regen"
            } },

            // tier 3
            { 11, new() {
                "Power Boost 5"
                , "Toughness Boost 5"
                , "Special Boost 5"
                , "Random Equipped Item Level + 1"
                , "+2 Exp"
                , "+5 Adv Power"
                , "+5 Adv Toughness"
                , "+50 Max Health"
                , "+0.5 Health Regen"
            } },

            // tier 4
            { 13, new() {
                "Power Boost 10"
                , "Toughness Boost 10"
                , "Special Boost 10"
                , "2 Random Equipped Items Level + 1"
                , "+3 Exp"
                , "+10 Adv Power"
                , "+10 Adv Toughness"
                , "+75 Max Health"
                , "+1 Health Regen"
            } },

            // tier 5
            { 15, new() {
                "Infinity Cube +5 Power"
                , "Infinity Cube +5 Toughness"
                , "Infinity Cube +3 Power & Toughness"
                , "ALL Equipped Items Level + 1"
                , "+10 Exp"
                , "+10 Yggdrasil Seeds"
                , "+20 Adv Power"
                , "+20 Adv Toughness"
                , "+150 Max Health"
                , "+1.5 Health Regen"
            } },

            // tier 6
            { 18, new() {
                "Infinity Cube +10 Power"
                , "Infinity Cube +10 Toughness"
                , "Infinity Cube +5 Power & Toughness"
                , "ALL Equipped Items Level + 1"
                , "+25 Exp"
                , "+25 Yggdrasil Seeds"
                , "+50 Adv Power"
                , "+50 Adv Toughness"
                , "+200 Max Health"
                , "+2 Health Regen"
                , "Wandoos Pit Level + 1"
            } },

            // tier 7
            { 21, new() {
                "Infinity Cube +20 Power"
                , "Infinity Cube +20 Toughness"
                , "Infinity Cube +10 Power & Toughness"
                , "ALL Equipped Items Level + 1"
                , "+25 Exp"
                , "+100 Yggdrasil Seeds"
                , "+100 Adv Power"
                , "+100 Adv Toughness"
                , "+300 Max Health"
                , "+3 Health Regen"
                , "Wandoos Pit Level + 1"
                , "ALL Daycare Items Level + 1"
            } },

            // tier 8
            { 24, new() {
                "Infinity Cube +50 Power"
                , "Infinity Cube +50 Toughness"
                , "Infinity Cube +25 Power & Toughness"
                , "ALL Equipped Items Level + 1"
                , "+200 Exp"
                , "+200 Yggdrasil Seeds"
                , "+150 Adv Power"
                , "+150 Adv Toughness"
                , "+450 Max Health"
                , "+5 Health Regen"
                , "Wandoos Pit Level + 2"
                , "ALL Daycare Items Level + 1"
            } },

            // tier 9
            { 27, new() {
                "Infinity Cube +100 Power"
                , "Infinity Cube +100 Toughness"
                , "Infinity Cube +50 Power & Toughness"
                , "ALL Equipped Items Level + 1"
                , "+300 Exp"
                , "+300 Yggdrasil Seeds"
                , "+200 Adv Power"
                , "+200 Adv Toughness"
                , "+700 Max Health"
                , "6 Health Regen"
                , "Wandoos Pit Level + 2"
                , "ALL Daycare Items Level + 1"
            } },

            // tier 10
            { 30, new() {
                "Infinity Cube +150 Power"
                , "Infinity Cube +150 Toughness"
                , "Infinity Cube +75 Power & Toughness"
                , "ALL Equipped Items Level + 1"
                , "+400 Exp"
                , "+500 Yggdrasil Seeds"
                , "+250 Adv Power"
                , "+250 Adv Toughness"
                , "+750 Max Health"
                , "+7.5 Health Regen"
                , "Wandoos Pit Level + 2"
                , "ALL Daycare Items Level + 1"
            } },

            // tier 11
            { 30, new() {
                "Infinity Cube +200 Power"
                , "Infinity Cube +200 Toughness"
                , "Infinity Cube +100 Power & Toughness"
                , "ALL Equipped Items Level + 1"
                , "+500 Exp"
                , "+700 Yggdrasil Seeds"
                , "+300 Adv Power"
                , "+300 Adv Toughness"
                , "+900 Max Health"
                , "+9 Health Regen"
                , "Wandoos Pit Level + 3"
                , "ALL Daycare Items Level + 1"
            } },

            // tier 12
            { 30, new() {
                "Small Iron Pill (20%)"
                , "ALL Equipped Items Level + 1"
                , "+2500 Exp"
                , "Small Pomegranate (2%)"
                , "ALL Daycare Items Level + 1"
            } },

 */