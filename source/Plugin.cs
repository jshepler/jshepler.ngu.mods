using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static Color ButtonColor_Green = new Color(0.5f, 0.827f, 0.235f);
        internal static Color ButtonColor_Yellow = new Color(1f, 0.827f, 0.235f);
        internal static Color ButtonColor_Red = new Color(0.925f, 0.204f, 0.204f);
        internal static Color ButtonColor_LightBlue = new Color32(127, 208, 255, 255);
        
        private readonly Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        private static ManualLogSource Log;
        internal static void LogInfo(string text) => Log.LogInfo(text);

        private static CharacterEventArgs _cea = null;
        internal static event EventHandler<CharacterEventArgs> OnUpdate;
        internal static event EventHandler<CharacterEventArgs> OnLateUpdate;
        internal static event EventHandler<CharacterEventArgs> OnSaveLoaded;
        internal static event EventHandler<CharacterEventArgs> OnPreSave;
        internal static event EventHandler<CharacterEventArgs> onGUI;

        internal static Character Character = null;

        private void Awake()
        {
            Log = base.Logger;
            Options.Init(base.Config);

            harmony.PatchAll();
            LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void Update()
        {
            if (!_cea)
                return;
                
            OnUpdate?.Invoke(null, _cea);

            // hidden "Krissmuss" screen from christmas 2020 event
            if (Input.GetKeyDown(KeyCode.X) && Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift))
                Character.menuSwapper.swapMenu((int)Menu.Krissmuss);
        }

        private void LateUpdate()
        {
            if (_cea) OnLateUpdate?.Invoke(null, _cea);
        }

        private void OnGUI()
        {
            if (_cea) onGUI?.Invoke(null, _cea);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "Start")]
        private static void Character_Start_postfix(Character __instance)
        {
            Character = __instance;
            _cea = new CharacterEventArgs(__instance);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ImportExport), "gameStateToData")]
        private static void ImportExport_gameStateToData_prefix()
        {
            OnPreSave?.Invoke(null, _cea);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Character), "addOfflineProgress")]
        private static void Character_addOfflineProgress_postfix(Character __instance)
        {
            OnSaveLoaded?.Invoke(null, _cea);

            // unlocks krissmuss ui theme from christmass 2019 event
            __instance.settings.prizePicked = 6;
        }

        // when starting a new game, there is no offline progress and mods that rely on this event
        // won't be called and could have bad side-effects
        [HarmonyPostfix, HarmonyPatch(typeof(MainMenuController), "startNewGame")]
        private static void MainMenuController_startNewGame_postfix()
        {
            OnSaveLoaded?.Invoke(null, _cea);
        }

        [HarmonyFinalizer, HarmonyPatch(typeof(Character), "addOfflineProgress")]
        private static void Character_addOfflineProgress_finalizer(Exception __exception)
        {
            if(__exception != null)
                LogInfo($"Character.addOfflineProgress threw exception:\n{__exception}");
        }
    }

    internal class CharacterEventArgs : EventArgs
    {
        public readonly Character Character;

        public CharacterEventArgs(Character c)
        {
            Character = c;
        }

        public static implicit operator bool(CharacterEventArgs cea)
        {
            return !object.ReferenceEquals(cea, null);
        }
    }
}

/*
notes:
    the load save button from startup screen calls: MainMenuController.loadFileSave() -> MainMenuController.loadFileKartridge() -> OpenFileDialog.loadFileMainMenuStandalone()
    the load save button bottom-left game screen calls: OpenFileDialog.startLoadStandalone()
    both eventually call ImportExport.loadData(SaveData)

    reg key: HKEY_CURRENT_USER\SOFTWARE\NGU Industries\NGU Idle

mods:
1 - fix basic training when right-click to evenly split assigned energy and no more than needed per offense and defense

2 - rebirth resets auto merge/boost timers

3 - include auto merge/boost timers when adding offline progression when loading a save

4 - displays effective net gold per second under Gold: main screen: TM net - current consumption per second from TM, BM, Augs

5 - auto-save on rebirth, challenge, pit throw, changing wandoos OS

6 - press F5 to do quicksave

7 - the lazy ITOPOD shifter can raise max floor

8 - calculates and displays player damage % and boss damage % to determine if current boss can be fought
    (changes Fight Boss button color: green = can nuke, yellow = can win fight)

9 - (WIP) displays "total work" at bottom of equipment bonuses: cap * power per E/M/R3

10 - during Walderp fight, displays number of remining regular attacks before "walderp says" attack

11 - function to reset total time played by pressing RightControl-F1 when on Misc. Stats page

12 - displays total blood gain per second under "Blood:" on blood magic page

13 - alters tooltips for "Blood Spaghetti" and "Counterfeit Gold" spells to show amount gained when clicked (like Iron Pill does)

14 - calculates amount of advanced training needed to autokill titans and displays in the Adventure button's tooltip

15 - fixes bug with Target boxes on the advanced training page to allow user to continue changing value when bar completes

16 - when doing troll challenge, the Rebirth button's tooltip includes time left before big troll (before big troll is next)
    (and changes Rebirth button's color to red when <= 20 seconds)

17 - auto merge/transform unprotected pendants and looties in inventory

18 - when in the spend exp menu, on the energy tab:
    shift-clicking the "Buy ALL Custom" button will buy all custom E/M
    control-shift-clicking the "Buy ALL Custom" button will buy all custom E/M/R3
    (just holding those keys down will change the button text to indicate this will happen as well as the total EXP cost)

19 - shift-click load button to skip offline time progression when loading a save (open file dialog title indicates when progress will be skipped)

20 - appends to adventure zone's tooltip the AT power needed to kill current enemy in 1-5 hits

21 - show current quest item drop rate under the quest item drop modifier on the questing screen

22 - during manual questing, set questing button color to yellow if the number of quest items in inventory plus the number of quest items already collected
    are within 5 of the target number (only if got the AP purchase "Quest Reminder!")

23 - auto harvest/eat fruits when attain full growth (max tier)
    also on rebirth any fruit >= tier 1
    (auto harvest max tier fruits now have an config option to disable)

24 - when starting a manual quest, if current zone has unlocked quest item, that item is assigned as the quest (instead of random quest item)

25 - improved bar tooltips, adds:
        current speed cap: augs, AT, TM, wandoos (all OS at same time), NGUs
        % of cap allocated: augs, AT, TM, BM, wandoos, NGUs, beards (*)
        over-capped duration (time until no longer over-capped): augs, AT, TM, wandoos, NGUs, beards (*)
        current speed - progress per tick & ticks per bar: augs, AT, TM, wandoos, NGUs, beards
        time to target: augs, AT, TM, NGUs

    * I know you don't allocate resources to beards, but the game still calculates progress per tick,
      which can be used to show a % "allocated" in order to show how much over "cap" the bar is,
      and to calc/display how long until no longer capped (i.e. how long it will be BB'd) - this helps to
      project when you won't need beard diggers running to stay BB'd for 24 hour rebirths
        
26 - shows current wandoos speed caps for all 3 OS types simultaneously on each bar's tooltip

27 - right-click on questing button adds quest items from inventory and completes the quest if target amount met

28 - allow target inputs to accept/display scientific and engineering notations, based on number display style in settings
    currently: Advanced Training, NGUs

29 - added boss # to displayed "Highest Boss Multiplier" on time machine page (mostly as an exercise of transpiler patch injecting delegate call)

30 - added display of digger gps drain diff between current and next level - because the current net gps needs to be enough to cover the diff, not the total

31 - toggle auto-allocate resources per bar (shift-click the + button) - idle resources evenly split between enabled bars, disables when hit target
    current bars: Basic Training, Augments, Advanced Training, Time Machine, Blood Magic Rituals, Wandoos, NGUs, Hacks

32 - alt-shift-click enables auto-allocate on multiple bars:
    all ngu enery, all ngu magic, augment+upgrade pair, basic training offense/defense pair (if sync training enabled)

33 - directly modify loadouts without having to change currently equipped gear:
    right-click loadout item to clear it
    left-click loadout item to enter "selection mode": closes loadout panel, left-click inventory or equipped item to assign it to the slot
    press escape to cancel "selection mode"

34 - shift-right-click questing button to toggle automate manual major quest: auto-collect items, auto-complete quest, auto-start new major quest
    doesn't automatically switch adventure zone - assumes already in quest zone and the above mod that uses current zone when starting quest
    when run out of major quests, will start minor idle quest

35 - left-alt-click + button to split resources into all runnable bars (i.e. target = 0 or level < target), such that bar speeds are equal
    currently: augment pairs (aug+upgrade), advanced training, NGUs

36 - added total boost from recycling to boosts' toolips

37 - added boost modifier breakdown to stat breakdowns / misc

38 - control-click the + button to calculate and allocate over-cap amount based on target level or target rebirth time (in minutes)
    target: -1 = ignore, 0 = target rebirth time, >0 = target level
    target rebirth time by entering total minutes into the resource input box at top of screen (e.g. to target rebirth time 24:00:00, enter 1440)
    current bars: time machine (both bars), advanced training, augments, NGUs (control-alt-click to do all energy or magic bars)

39 - LeftShift-R, LeftShift-T, LeftShift-F removes all of the respective resource from the currently viewed feature (instead of all features)
    currently implemented: basic training, augs, adv training, time machine, blood magic rituals, wandoos, ngu, hacks

40 - Upgrade All Diggers by right-clicking the "Gold Digger" button; will upgrade all diggers in order of cost, repeating until not enough gold for any upgrade

41 - fruit tooltips show what was last gained

42 - added the current softcaps to infinity cube's power and toughness stats (red means < softcap, green means >= softcap)
    also removed the softcap warnings from the tooltip because it adds clutter for no benefit (for those that already know about cube softcaps)
    added uncapped power and toughness (what's used to determine tier) and amount needed for next tier

43 - temp loadout saves currently equipped gear when loading a loadout (regular or temp), load temp loadout by pressing the X key while on inventory screen

44- change daycare bars from "Levels Gained" to show the level the item will be if taken out; if non-macguffin, shows time remaining until level 100
    (also adds item's current time per level to tooltip)

45 - fixed bug with basic training's auto advance toggle state not being saved or applied on load

46 - test fix (only for energy NGUs) for when calculating how much of cap value to allocate when clicking an NGU's cap button to not clip the cap value to hard-cap

47 - when a piece of equipment is maxxed (level 100), automatically enables the item filter for that item (except looties and pendants)
    exceptions: looties, pendants, flubber, wanderer's cane

48 - display all fibonacci perk unlocks, green are unlocked, red are locked

49 - (WIP) changed how y position of tooltips changes when it would go off the top of the screen to stay up against the top instead of
    flipping down because some tooltips (e.g. fibonacci perk) would end up going below bottom of screen

50 - add tab navigation to various fields, supports shift-tab and wrap-around (tabbing past first/last field, wraps to other end)
    currently: augment targets, advanced training targets, time machine targets, NGUs targets, digger levels, Hacks targets

51 - shift-click cap button to get a popup of buttons for the partial caps 10-50%, click to allocate that amount
    (note: these are the amounts for bar fill speeds: 2/3/4/5/6/7/8/9/10 ticks per bar and is what the existing cap buttons allocate)
    currently: blood rituals

52 - added tooltip to Money Pit button that shows what the next reward will be as well as the time remaining

53 - press control-shift-x to open the hidden krissmuss 2020 event screen

54 - unlocked krissmuss 2019 ui theme

55 - fixed ygg seeds gained display format bug - not all fruits used current display option (e.g. scientific notation) for number of seeds gained
    patched: all harvested fruits, eaten fruits: FoG, FoPa, FoA

56 - replaced hacks' tooltip to include more data related to milestone bonuses and benefits from milestone reducers

57 - implemented framework to persist additional data to save file
    done in a way that allows such a save to be loadable in vanilla game
    holding shift when clicking the save button will do a clean save (save file dialog's title has "(CLEAN)" appended)

    current data being persisted:
        auto questing enabled (mod 24)
        last loadout (mod 43)
        last ygg rewards (mod 41)
        last iron pill gain
        enabled auto-allocators (mod 31)
        wish queue (mod 67)

58 - number keys changes page on: ygg, diggers, beards, hacks, wishes

59 - press F11 to toggle fullscreen mode
    (WIP) shift-F11 to toggle maximized window
    (WIP) start work on supporting custom resolutions
    this was an exercise prompted by https://www.reddit.com/r/nguidle/comments/16lomn0/resize_the_window/
        question was deleted, but basically asked how to resize the window or go fullscreen

60 - copy/paste wish allocations
    allocate EMR3 to a wish, press control-c to copy, select another wish, press control-v to paste the allocations

61 - added right-click costs and levels gained to perk and quirk tooltips

62 - added shift-right-click on fib perk to only buy up to next unlock

63 - right-click on the enter itopod button to directly enter the tower, skipping the popup
    (if shifter is enabled, will set optimal floor first)

64 - (WIP) press control-r to restore all unassigned resources from doing things like pressing R or swapping loadouts and having unassign E/M enabled
    can also press alt-r to take a current "snapshot" to restore later
    [currently disabled - not working quite right]

65 - can re-arrange daycare items directly and without resetting timers (i.e. losing progress)

66 - (WIP) adds tooltips to the Adv Training, Time Machine, and Beards of Power buttons to show total bank %
    plan to add tooltips to the individual bars to show the resulting amounts that will carry over on rebirth

67 - added wish queue
    shift-click a wish to add/remove to/from the queue
    press q to toggle the queue window where can re-order wishes or clear the queue

68 - added number of clicks remaining before button swap on small troll's "click ok 50 times" popup

69 - adds "time to max level" to wish tooltips
    hold alt key to see breakdown of times per level

70 - the Cap Saved Diggers button uses alternate method
    sets saved diggers to level 0
    finds saved digger with lowest drain increase for next level that doesn't go over gross gps
    increase its level by 1
    do again until none found
    (hold alt when clicking to use original method)

71 - modified basic training tooltips to be a little cleaner and show more information
    displays the cap after next rebirth as [new]/[min] so know what max reduction will be before getting there
    display at what level max reduction will be reached
    display time remaining before that level

72 - click the cap button on a basic training skill while having the sync training setting enabled will split evenly between the two skills

73 - when on Fight Boss screen, press f to toggle fight on/off

74 - on wishes screen, can use number keys to select pages and arrow keys to move current wish selection

75 - press F2 to open a popup to change titans' version without having to go to their zone

76 - on an item's tooltip, if there is another of that item in daycare, show the level of the item in daycare including levels gained
    and displays the total of the 2 items - helpful for looties and pendents to know when to pull daycare item out and merge

77 - auto-sniping: skips normal enemies on adventure screen, if < 50% health after boss dies (or if player dies) stays in safe zone until full health
    go to desired zone and press F1 (or right-click idle button) to toggle auto-snipe
    (changes border of the Idle Mode button to red)

78 - on the rebirth screen, the rebirth button will be red if there is "Crap to do before rebirthing"

79 - removed "not less than 10k" limit on buy custom energy/magic cap, lowest is what you can get for 1 exp (250 cap)

80 - fixed bug with Jake's (T3) locusts attack - the attacks aren't supposed to start until the turn after the warning, currently it's the next frame
    (also changed the color of the warning to blue so it's more noticeable)

81 - added http listener that accepts specific requests to trigger functions in game, http://localhost:8088/ngu/<command>/
    current commands: autoBoost, autoMerge, tossGold, fightBoss, totalTimePlayed
        (totalTimePlayed generates html to display a timer starting from current total time played)

82 - added confg file:
        option to enable/disable remote triggers (totalTimePlayed ignores this setting)
        options to enable/disable autoBoost, autoMerge, tossGold, fightBoss
        option to enable/disable auto-harvest/eat fruits when fully grown

83 - separated notifications (aka timed tooltips) from tooltips (doesn't share same window) and made them toasts - allowing multiple notifications
    (ignores boss kills - nuking 50+ bosses generates way too many toasts)

84 - evenly split resources between selected wishes
    control-click to select multiple wishes (up to max wish slots)
    alt-click any of the resource + buttons to remove all resources from all wishes, and then split all idle resources to selected wishes

85 - improved ITOPOD description (zone tooltip) - cleaned up the text, added time to next PP, added max and optimal floors
    time to next PP uses 2 calcs: one for when floor <= optimal floor and another > optimal floor
        <= optimal floor, 1-shot kills using respawn time + idle attack time
        > optimal floor, uses rolling average times of last 5 kills

 */
