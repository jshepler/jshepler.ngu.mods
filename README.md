# jshepler.ngu.mods
Collection of mods I wrote for myself.

I had not intended to make these public, as I don't want to support them long-term (i.e. when no longer playing the game). A few people have asked that I share the code, so here you go.

The majority are QoL enhancements. There are a couple bug fixes, and few things that are somewhat cheaty-ish but nothing majorly so.

# Installation
These mods are written for [bepinex](https://github.com/BepInEx/BepInEx) v5.4.21, which can be downloaded [here](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.21). Download the x64 version and extract the contents of the zip file:

![bepinex zip contents](bepinex_zip_contents.png)

to NGU Idle's game folder `...\Steam\steamapps\common\NGU IDLE`:

![NGU folder after extraction](bepinex_extracted.png)

***NOTE: if using Linux, need to add `WINEDLLOVERRIDES="winhttp=n,b" %command%` to the game's launch options in steam.***

Bepinex will set itself up the first time the game is run after extracting the zip. So start the game, then exit the game.

Download the latest `jshepler.ngu.mods.dll` file from [releases](https://github.com/jshepler/jshepler.ngu.mods/releases) and put it in `...\Steam\steamapps\common\NGU IDLE\BepInEx\plugins`.

![dll file location](dll.png)

# Configuration
The first time the game is run after installing the mods, a configuration file is created: `...\Steam\steamapps\common\NGU IDLE\BepInEx\config\jshepler.ngu.mods.cfg`.

The config file is only read when the game starts, so exit the game before making changes. If an update adds new config options, you will need to start the game for the new options to be added to the file, then exit the game to configure them.

There are only a few config options at the moment. I'll probably add options in the future for the more questionable things.

# GO integration bookmarklets
***NOTE (unrelated to bookmarklets): ~~importing saves from modded NGU into GO won't work unless you import a clean save - see mods 57 and 139 below.~~*** *Since GO version 0.10.0, it no longer fails to load saves from modded NGU.*

Bookmarklets are bookmarks that execute javascript instead of taking you to a specified URL. They run in the active browser tab as part of the site being viewed.

The bookmarklets below are used in the [Gear Optimizer](https://gmiclotte.github.io/gear-optimizer) site to talk to NGU. They require you to use GO v0.9.2 or higher - check the [About](https://gmiclotte.github.io/gear-optimizer/#/about/) tab in GO. **You must be on GO when using these bookmarklets or they won't work.**

Create a new bookmark. I like using the bookmark bar for easy access and in a sub-folder. You can name them whatever you want. Here's what I named mine:

![what I called mine](GO_bookmarklets.png)

For the URL field, you're going to paste some javascript instead of a URL:

![bookmarklet URL](GO_bookmarklets_URL.png)

## sending loadouts from GO to NGU
`javascript:fetch("http://localhost:8088/ngu/go2ngu/loadouts",{method:"POST",body:JSON.stringify(appState.savedequip)});`

This will find matching loadouts in NGU (where loadout name = GO save slot name) and changes the items to match GO. If you have more than one of that item, it will use the one with the highest level.

## sending current equipped items from NGU to GO
`javascript:fetch("http://localhost:8088/ngu/ngu2go/equipped").then(e=>e.json()).then(e=>{let n=appState.savedequip;Object.assign(n.find(e=>"current"==e.name),e),appHandlers.handleSettings("savedequip",n)});`

If you have a save slot named `current` in GO, that will be updated to match your currently equipped gear. If you want to use a different name than `current` in GO, edit the above to specificy a different slot name.

## sending naked EMR3 from NGU to GO
`javascript:fetch("http://localhost:8088/ngu/ngu2go/nakedemr").then(t=>t.json()).then(t=>{let a=appState.capstats;Object.assign(a,t),appHandlers.handleSettings("capstats",a)});`

Sends naked E/M/R3 stats (i.e. without equipment bonuses) to GO for `Gear` tab, on the right for "Hardcap Input".

## sending aug stats from NGU to GO
`javascript:fetch("http://localhost:8088/ngu/NGU2GO/augstats").then(t=>t.json()).then(t=>{let s=appState.augstats;Object.assign(s,t),appHandlers.handleSettings("augstats",s)});`

This sends current data from NGU to GO for the `Augments` tab.

## sending ngu stats from NGU to GO
`javascript:fetch("http://localhost:8088/ngu/NGU2GO/ngustats").then(t=>t.json()).then(t=>{let s=appState.ngustats;Object.assign(s,t),appHandlers.handleSettings("ngustats",s)});`

This sends current data from NGU to GO for the `NGUs` tab.

## sending hack stats from NGU to GO
`javascript:fetch("http://localhost:8088/ngu/ngu2go/hacks").then(e=>e.json()).then(e=>{let a=appState.hackstats;a.rpow=e.rpow,a.rcap=e.rcap,a.hackspeed=e.hackspeed;for(let c=0;c<15;c++)a.hacks[c].goal=a.hacks[c].level=e.hacks[c].level,a.hacks[c].reducer=e.hacks[c].reducer;appHandlers.handleSettings("hackstats",a)});`

This sends current hack stats from NGU to GO for the `Hacks` tab: stats at the top, current reducer counts, current hack levels, reset targets to match current levels.

## sending hack stats from NGU to GO (no targets)
`javascript:fetch("http://localhost:8088/ngu/ngu2go/hacks").then(e=>e.json()).then(e=>{let a=appState.hackstats;a.rpow=e.rpow,a.rcap=e.rcap,a.hackspeed=e.hackspeed;for(let c=0;c<15;c++)a.hacks[c].level=e.hacks[c].level,a.hacks[c].reducer=e.hacks[c].reducer;appHandlers.handleSettings("hackstats",a)});`

Same as above except doesn't reset targets. This is useful if stats changed before starting HD and might need to do some adjustments without having to start completly over. Or if in the middle of HD and times didn't work out how you thought and want to make adjustments for the remaining hacks.

## sending hack targets from GO to NGU
`javascript:fetch("http://localhost:8088/ngu/go2ngu/hacks",{method:"POST",body:JSON.stringify(appState.hackstats.hacks.map(a=>a.goal))});`

This sends the hack goals from GO to NGU which sets hack targets.

## sending wish stats from NGU to GO
`javascript:fetch("http://localhost:8088/ngu/NGU2GO/wishstats").then(s=>s.json()).then(s=>{let t=appState.wishstats;Object.assign(t,s),appHandlers.handleSettings("wishstats",t)});`

This sends current wish stats from NGU to GO for the `Wishes` tab.

# Twitch Integration

## setup

To setup twitch integration, you will need to [register your app](https://dev.twitch.tv/docs/authentication/register-app/):
- the redirect url MUST be `http://localhost:8088/ngu/twitch/oauth`
- the client type MUST be `Confidential`
- the client id and client secret go into the `jshepler.ngu.mods.cfg` file in the `[Twitch]` section

After setting up custom rewards, you can map them to remote triggers in the cfg file, in the `[Twitch.RewardTriggers]` section. ***The reward names in the cfg file must exactly match, i.e. are case-sensitive.***

In game, right-click the gear (settings) button in the lower-left of the screen to bring up the remote triggers config panel - which now includes twitch options at the bottom. Enabling `Twitch Integration` will add a row to show the connection status and buttons to connect/disconnect and reset.

If configured correctly, click the `Connect` button to connect to twitch and start the authorization process. Your default browser should open a new tab asking you to login to twitch and grant permission for NGU to connect to your channel.

## troubleshooting
If twitch integration is enabled, the color of the gear icon will change to red if not connected.

If NGU gets disconnected from Twitch, you should just be able to click the `Connect` button to re-connect. If that doesn't work, click the `Reset` button which ditches the current authorization and starts over.

If it continues to stay disconnected, please send me the `...\Steam\steamapps\common\NGU IDLE\BepInEx\LogOutput.log` file. The best way to do this is to DM it to me on discord. I'm on the NGU discord and can be found in the `scripting` or `bug-reports-and-complaints` channels.

# Mods
(in no particular order)

1. fix basic training when right-click to evenly split assigned energy and no more than needed per offense and defense

2. rebirth resets auto merge/boost timers

3. include auto merge/boost timers when adding offline progression when loading a save

4. displays effective net gold per second under Gold: main screen: TM net - current consumption per second from TM, BM, Augs

5. auto-save on rebirth, challenge, pit throw, changing wandoos OS

6. press F5 to do quick save, F6 to load latest quick save, ignores clean quick saves, those that have "(CLEAN)" in the name

7. the lazy ITOPOD shifter can raise max floor

8. calculates and displays player damage % and boss damage % to determine if current boss can be fought

    (changes Fight Boss button color: green = can nuke, yellow = can win fight)

9. (WIP) displays "total work" at bottom of equipment bonuses: cap * power per E/M/R3

10. during Walderp fight, displays number of remining regular attacks before "walderp says" attack

11. function to reset total time played by pressing RightControl-F1 when on Misc. Stats page

12. displays total blood gain per second under "Blood:" on blood magic page

13. alters tooltips for "Blood Spaghetti" and "Counterfeit Gold" spells to show amount gained when clicked (like Iron Pill does)

14. calculates amount of advanced training needed to autokill titans and displays in the Adventure button's tooltip
    - shows number of remaining kills for T5 AK requirement
    - shows number of remaining optional kills for T9+ AK

15. fixes bug with Target boxes on the advanced training page to allow user to continue changing value when bar completes

16. when doing troll challenge, the Rebirth button's tooltip includes time left before big troll (before big troll is next)

    (and changes Rebirth button's color to red when <= 20 seconds)

17. auto merge/transform unprotected pendants and looties in inventory

    *option to disable in config file*

18. when in the spend exp menu, on the energy tab:
    - shift-clicking the "Buy ALL Custom" button will buy all custom E/M
    - control-shift-clicking the "Buy ALL Custom" button will buy all custom E/M/R3
    - shift-right-click or control-shift-right-click will repeat buying until not enough EXP

    (just holding those keys down will change the button text to indicate this will happen as well as the total EXP cost)

19. skips offline progression calculations when loading a save:
    - shift-click the load button on start screen (top) or main game screen (lower-left), the open file window's title bar will say "SKIPPING OFFLINE PROGRESS"
    - shift-click the load auto save button on start screen
    - shift-click the load cloud save button on start screen
    - shift-F6 to skip offline progression when loading last quick save

20. appends to adventure zone's tooltip the AT power needed to kill current enemy in 1-5 hits

21. show current quest item drop rate under the quest item drop modifier on the questing screen

22. during manual questing, set questing button color to yellow if the number of quest items in inventory plus the number of quest items already collected are within 5 of the target number (only if got the AP purchase "Quest Reminder!")

23. auto harvest/eat fruits when attain full growth (max tier) - shift-right-click the yggdrasil button to toggle

    on rebirth, will always (ignores toggle above) auto harvest/eat any fruit >= tier 1

24. when starting a manual quest, if current zone has unlocked quest item, that item is assigned as the quest (instead of random quest item)

25. improved bar tooltips, adds:
    - current speed cap: augs, AT, TM, wandoos (all OS at same time), NGUs ***(displays real cap, even if over hard cap)***
    - % of cap allocated: augs, AT, TM, BM, wandoos, NGUs, beards\*
    - over-capped duration (time until no longer over-capped): augs, AT, TM, wandoos, NGUs, beards\*
    - current speed - progress per tick & ticks per bar: augs, AT, TM, wandoos, NGUs, beards
    - time to target: augs, AT, TM, NGUs, hacks
    - banked amounts to: AT, TM, beards

    \* I know you don't allocate resources to beards, but the game still calculates progress per tick, which can be used to show a % "allocated" in order to show how much over "cap" the bar is, and to calc/display how long until no longer capped (i.e. how long it will be BB'd) - this helps to project when you won't need beard diggers running to stay BB'd for 24 hour rebirths
        
26. shows current wandoos speed caps for all 3 OS types simultaneously on each bar's tooltip

27. right-click on questing button adds quest items from inventory and completes the quest if target amount met

28. replaces input parser to allow sci/eng notation, with or without the '+' (e.g. can do 1e9 instead of 1e+9)
    - also honors current culture's decimal seperator character (e.g. , instead of .)
    - currently for: EMR3 input box at top of screen, target boxes for AT and NGUs

29. added boss # to displayed "Highest Boss Multiplier" on time machine page (mostly as an exercise of transpiler patch injecting delegate call)

30. added display of digger gps drain diff between current and next level - because the current net gps needs to be enough to cover the diff, not the total

31. auto-allocator - shift-click the + button to toggle, changes button to ++ to indicate being enabled
    - every tick, evenly splits idle resources between the enabled allocators up to cap
    - if cap gets reduced, excess resource will get returned to idle to be distributed to other enabled allocators
    - if resource is de-allocated (hits target, press r/t/f key, rebirth, etc), allocator is turned off
    - alt-shift-click will toggle the allocators on all bars on current screen (e.g. all eNGUs)
    - current bars: BT, augs, AT, TM, BM, Wandoos, NGUs, Hacks, Wishes
    - BT: if the setting "Sync Training" is enabled, will toggle the appropriate other skill
    - augs: alt-shift-click only does the aug+upgrade pair instead of all bars
    - hacks: alt-shift-click toggles all hacks, not just the ones on current page
    - wishes: 
        - shift-click toggles the allocator for the one resource on the one wish
        - alt-shift-click toggles the allocators for the one resource on all slotted wishes
        - ctrl-alt-shift-click toggles the allocators for all 3 resources on all slotted wishes
        - shift-clicking the resume button (F1 popup) will resume plus enable (not toggle) the allocators for all 3 resources on the resumed wishes
        - if a wish that has an enabled allocator finishes and auto advance is enabled (F1 popup), the allocator will get moved to the next wish started (even if finished offline) 

32. alt-shift-click enables auto-allocate on multiple bars: all ngu energy, all ngu magic, augment+upgrade pair, basic training offense/defense pair (if sync training enabled), TM pair, wandoows pair

33. directly modify loadouts without having to change currently equipped gear:

    right-click loadout item to clear it

    left-click loadout item to enter "selection mode": closes loadout panel, left-click inventory or equipped item to assign it to the slot

    press escape to cancel "selection mode"

34. shift-right-click questing button to toggle automate manual major quest: auto-collect items, auto-complete quest, auto-start new major quest, auto-use butter (option in cfg file)

    if have the AP purchase "Go To Quest Zone", will auto change to quest zone

    if enabled, will auto use butter (shift-right-click the Use Beast Butter button to toggle)

    while running, uncheck the "User Major Quests" box to not start a new major when current finishes, acts as if ran out of majors

    when run out of major quests, will start minor idle quest and change zone to ITOPOD

    disables itself if player changes to idle

35. left-alt-click + button to split resources into all runnable bars (i.e. target = 0 or level < target), such that bar speeds are equal

    currently: augment pairs (aug+upgrade), advanced training, NGUs

36. added total boost from recycling to boosts' tooltips - accounts for current recylce chance and shows the average value when chance < 100%

37. added boost modifier breakdown to stat breakdowns / misc

38. control-click the + button to calculate and allocate over-cap amount based on target level or target rebirth time (in minutes)

    target: -1 = ignore, 0 = target rebirth time, >0 = target level

    target rebirth time by entering total minutes into the resource input box at top of screen (e.g. to target rebirth time 24:00:00, enter 1440)

    current bars: time machine (both bars), advanced training, augments, NGUs (control-alt-click to do all energy or magic bars)

39. LeftShift-R, LeftShift-T, LeftShift-F removes all of the respective resource from the currently viewed feature (instead of all features)

    currently implemented: basic training, augs, adv training, time machine, blood magic rituals, wandoos, ngu, hacks, wishes

40. Upgrade All Diggers by right-clicking the "Gold Digger" button; will upgrade all diggers in order of cost, repeating until not enough gold for any upgrade

41. fruit tooltips show what was last gained (persisted in save)

42. added the current soft caps to infinity cube's power and toughness stats (red means < soft cap, green means >= soft cap)

    also removed the soft cap warnings from the tooltip because it adds clutter for no benefit (for those that already know about cube soft caps)

    added uncapped power and toughness (what's used to determine tier) and amount needed for next tier

43. temp loadout saves currently equipped gear when loading a loadout (regular or temp), load temp loadout by pressing the X key while on inventory screen

    (persisted in save)

44. change daycare bars from "Levels Gained" to show the level the item will be if taken out; if non-macguffin, shows time remaining until level 100. also adds item's current time per level to tooltip

45. fixed bug with basic training's auto advance toggle state not being saved or applied on load

46. test fix (only for energy NGUs) for when calculating how much of cap value to allocate when clicking an NGU's cap button to not clip the cap value to hard-cap

47. when a piece of equipment is maxed (level 100), automatically enables the item filter for that item (except looties and pendants)

    exceptions: looties, pendants, flubber, wanderer's cane, gerbil (sad-only)

48. display all Fibonacci perk unlocks, green are unlocked, red are locked

49. (WIP) changed how y position of tooltips changes when it would go off the top of the screen to stay up against the top instead of flipping down because some tooltips (e.g. Fibonacci perk) would end up going below bottom of screen

50. add tab navigation to various fields, supports shift-tab and wrap-around (tabbing past first/last field, wraps to other end)

    currently: augment targets, advanced training targets, time machine targets, NGUs targets, digger levels, Hacks targets, EMR3 custom purchase amounts, rich jerks custom purchase amounts

51. shift-click cap button to get a popup of buttons for the partial caps 10-50%, click to allocate that amount

    (note: these are the amounts for bar fill speeds: 2/3/4/5/6/7/8/9/10 ticks per bar and is what the existing cap buttons allocate)

    currently: blood rituals

52. added tooltip to Money Pit button that shows what the next reward will be as well as the time remaining, AP gained, and toss multiplier (for rewards that get it)
    
    hold alt to see the rewards for each tier group - useful for planning pit days

53. press control-shift-x to open the hidden Krismuss 2020 event screen

54. unlocked Krismuss 2019 ui theme and daycare kitty

55. fixed ygg seeds gained display format bug - not all fruits used current display option (e.g. scientific notation) for number of seeds gained

    patched: all harvested fruits, eaten fruits: FoG, FoPa, FoA

56. replaced hacks' tooltip to include more data related to milestone bonuses and benefits from milestone reducers (hold alt to see the bonuses per reducer)

57. implemented framework to persist additional data to save file, done in a way that allows such a save to be loadable in vanilla game

    holding shift when clicking the save button will do a clean save (save file dialog's title has "(CLEAN)" appended)

    current data being persisted:
    - auto questing enabled (mod 34)
    - last loadout (mod 43)
    - last ygg rewards (mod 41)
    - last iron pill gain
    - enabled auto-allocators (mod 31)
    - wish queue (mod 67)

58. number keys changes page on: ygg, diggers, beards, hacks, wishes, item list

59. press F11 to toggle fullscreen mode

    (WIP) shift-F11 to toggle maximized window

    (WIP) start work on supporting custom resolutions

60. copy/paste wish allocations: allocate EMR3 to a wish, press control-c to copy, select another wish, press control-v to paste the allocations

61. improved perk and quirk tooltips:
    - shows how much PP/QP will be used when right-clicking and what level/bonus it will be
    - shows PP/QP to next level and max level
    - shows estimated days to next level and max level

62. added shift-right-click on fib perk to only buy up to next unlock
    - tooltip shows how much PP will be spent to gain next bonus if shift-right-click
    - if not enough PP yet, will show much PP remaining and est days to get it

63. right-click on the enter ITOPOD button to directly enter the tower, skipping the popup

    (if shifter is enabled, will set optimal floor first)

64. (WIP) press control-r to restore all unassigned resources from doing things like pressing R or swapping loadouts and having unassign E/M enabled

    can also press alt-r to take a current "snapshot" to restore later

    [currently disabled - not working quite right]

65. can re-arrange daycare items directly and without resetting timers (i.e. losing progress)

66. (WIP) adds tooltips to the Adv Training, Time Machine, and Beards of Power buttons to show total bank %

    (temp placement, plan to move this info somewhere else)

67. wish list (previously wish queue) that let's you specify which wishes to run and in what order, with various options:
    - shift-click a wish to add/remove to/from the list
    - press F1 to toggle the list/config popup
    - disabled by default - enable either in cfg file or in F1 popup
    - auto advance (enabled by default) - if enabled, will start the next wish when a wish stops
    - single level (disabled by default) - if enabled, a wish will stop after gaining a single level
    - blacklist (disabled by default) - if enabled, wishes in the list are ignored (see below)
    - "start" will start running as many wishes as it can
    - "resume" will attempt to start wishes that were last running
    - "clear" clears the list
    - the text box after wish's current/max level is used to set the target for that wish
    - only wishes in the list have targets, blacklist mode disables targets
    - when a wish hits its target or max level, it is removed from the list
    - in single level mode, when a wish in the list levels but has not reached target/max level, it gets moved to bottom of the list
    - if auto advance is enabled and blacklist is disabled and list is empty - will start first runnable wish going by current filter/order
    - if auto advance is enabled and blacklist is enabled - will start first runnable wish going by current filter/order, ignoring wishes in the list
    - blacklisted wishes are shaded red

    lazy mode = auto advance, single level, empty list, TTL order - will always run fastest wish levels; use blacklist to skip unwanted wishes

    there is no support for offline progress, targets are ignored, will not start up new wishes "in the middle" of offline progression

    after offline progression completes, new wishes will get started for each wish that finished

    there may still be bugs, testing wishes is slow, so I added an auto save when a wish is about to level... if you encounter any issues, please send me that save so I can use it for testing

68. added number of clicks remaining before button swap on small troll's "click ok 50 times" popup

69. adds "time to max level" to wish tooltips

    hold alt key to see breakdown of times per level

70. the Cap Saved Diggers button uses alternate method
    - sets saved diggers to level 0
    - finds saved digger with lowest drain increase for next level that doesn't go over gross gps
    - increase its level by 1
    - do again until none found

    (hold alt when clicking to use original method)

71. modified basic training tooltips to be a little cleaner and show more information

    - displays the cap after next rebirth as [new]/[min] so know what max reduction will be before getting there

    - display at what level max reduction will be reached

    - display time remaining before that level

72. clicking the cap button on a basic training skill while having the sync training setting enabled will split evenly between the two skills

73. press B or right-click Fight Boss button to start a boss fight (will nuke if able)

74. on wishes screen, can use arrow keys to move current wish selection

75. press F2 to open a popup to change titans' version without having to go to their zone

76. on an item's tooltip, if there is another of that item in daycare, show the level of the item in daycare including levels gained and displays the total of the 2 items - helpful for looties and pendents to know when to pull daycare item out and merge

77. auto-sniping: skips normal enemies on adventure screen

    keeps track of max hp loss and after an enemy dies and if hp <= that max loss, goes to safe zone to fully heal

    go to desired zone and right-click idle button to toggle auto-snipe (changes border of the Idle Mode button to red)

    there's a config option to snipe specified target when in specified zone, instead of that zone's bosses

78. on the rebirth screen, the rebirth button will be red if there is "Crap to do before rebirthing"
    - adds a check for if any digger can be upgraded (increase max level)

79. removed "not less than 10k" limit on buy custom energy/magic/res3 cap, lowest is what you can get for 1/3/100k exp (i.e. 250 E/M/R3 cap)

80. fixed bug with Jake's (T3) locusts attack - the attacks aren't supposed to start until the turn after the warning, currently it's the next frame

    (also changed the color of the warning to blue so it's more noticeable)

81. added remote triggers (using HttpListener) that accepts specific requests to trigger functions in game, `http://localhost:8088/ngu/<command>/`

    current commands:
    - autoBoost: executes in-game auto-boost code
    - autoMerge: executes in-game auto-merge code
    - tossGold: executes in-game money pit's "Feed Me"
    - fightBoss: if able to win fight, switches to boss screen, nukes (if able), fight boss (if winnable), returns to prev screen
    - kitty: starts troll challenge big troll's kitty event (for fun)
    - totalTimePlayed: generates html to display timer starting from current Total Time Player

    has config options:
    - Enabled: if disabled, ignores commands (except totalTimePlayed)
    - UrlPrefix: what appears before \<command\>, can be used to specify machine name or IP
    - enable/disable individual commands

    the gear (settings) button in the lower-left of the screen (next to Info 'n Stuff):
    - green/white = master enabled/disabled
    - F10 to toggle enabled/disabled
    - right-click the button (or press shift-F10) to open the in-game config panel

82. added config file:
    - option to control remote triggers
    - option to enable/disable auto-harvest/eat fruits when fully grown
    - options to control how zone drop table tooltip displays items
    - option to target zone/enemy for auto-sniping
    - option to replace default player portrait with specified boss id

83. separated notifications (aka timed tooltips) from tooltips (doesn't share same window) and made them toasts - allowing multiple notifications

    (ignores boss kills - nuking 50+ bosses generates way too many toasts)

    options to disable this mod or to change orientation (top-down or bottom-up)

84. evenly split resources between selected wishes

    - alt-click to select multiple wishes (up to max wish slots)
    - alt-click any of the resource + buttons to remove all resources from all wishes, and then split all idle resources to selected wishes
    - if no wishes are multi-selected, alt-click resource allocation will be done on any any wish that has any resource currently allocated

85. improved ITOPOD description (zone tooltip)
    - cleaned up the text
    - seconds per kill
    - kills per PP (if less than 1 PP per kill) or PP per kill (if 1 or more PP per kill)
    - PP per hour and day
    - kills per EXP drop
    - EXP per drop
    - EXP per day
    - AP per day

    in alt tooltip (hold alt):
    - poop per day
    - guffs per day
    - the optimal floor even if > max floor
    - AT power and adventure power needed for optimal floors: next, next 50th (next exp increase), next boost (e.g. when boost drops change from 1k to 2k)

    time to next PP uses 2 calcs: one for when floor \<= optimal floor and another > optimal floor

    \<= optimal floor, 1-shot kills using respawn time + idle attack time

    \> optimal floor, uses rolling average times of last 5 kills

86. adds current zone's drop table as an alternate zone tooltip (hold alt key while hovering mouse over zone description)

    (has options in config file to control when/how some items are displayed)

    hold shift to show what the DCs would be if activated a charm

    press left/right arrow keys to change which zone is being viewed

    can also view drop table via bestiary - hold alt when hovering over an enemy

87. fixed bug with clock zone's drops - it's supposed to have a chance to drop A Busted Copy of Wandoos 98

88. on the start screen, added timestamps to the autosave and steam cloud details

89. when fighting bosses, the Fight Boss button turns red, shows current boss #, and depletes in sync with boss hp

90. tracks total time played per difficulty and displays each on the Misc Stats page

    if installing mod into an existing game that's no longer in normal, there's no way to know how much of the existing playtime is normal, so it assigns all of it to normal

91. modified manual combat moves to be colored based on state:
     - disabled/paralyzed (red)
     - buff running (blue)
     - on cooldown (yellow)
     - on GCD (grey)
     - ready (green)

92. ~~fixed zone dropdown bug that prevented selecting safe zone~~ (replaced by mod 215)

93. adds hotkeys to do an merge all (shift-m) and boost all (shift-b)

94. notifications are generated for zones being unlocked

95. right-clicking the Auto Transform buttons on inventory screen will transform all unprotected boosts in inventory

96. show Total Attack/Defense Modifier from augs to the Augments stat breakdown screen

97. display inventory count (used / max) next to the "INVENTORY" label above the inventory grid

98. compare items: while hovering over an item in inventory, press c to lock its tooltip to the top-left of screen, press esc to close it

99. option to change default player portrait to boss portrait specified in cfg file

    in `[DefaultPlayerPortait]` section, set `BossId` to the number from the in-game bestiary

100. hold alt when viewing an item's tooltip to see a list of where that item drops

101. highlight currently equipped loadout button on inventory screen

102. options to enable/disable each of the 3 allocators (defaults to disabled)

103. when right-clicking an accessory, it will get equipped in the first open slot

     order follows same order as auto merge/boost, described in [the wiki](https://ngu-idle.fandom.com/wiki/Inventory#AutoMerge_&_AutoBoost)

104. save pruning: when an autosave or quick save happens, saves older than DaysToKeep (in cfg file) are deleted

     defaults to being disabled, edit cfg file to change DaysToKeep from 0 to something else

105. right-click the `Adventure` button to change to highest zone - same as right-clicking the right-arrow button on the Adventure page

106. right-click the `Loadouts` button to toggle "mark used" - similar to GO's "mark unused", but highlights items that are in any loadout

107. adds support for my [ratio tool](https://jshepler.github.io/) to import base EMR3 directly from NGU

108. GO ([gear optimizer](https://gmiclotte.github.io/gear-optimizer)) integration:
     - transfer loadouts from GO to NGU
     - transfer current equipped gear from NGU to GO a save slot that's named "current"
     - transfer aug stats from NGU to GO's augments tab (ecap, aug speed, net gps, normal LAC, and normal LSC)
     - transfer ngu stats from NGU to GO's NGUs tab (em cap, em ngu speed, quirks, blue heart, and current levels for all NGUs)
     - transfer hack stats from NGU to GO's hacks tab (rpower, rcap, hack speed, current level and reducer counts for all hacks)
     - transfer hack goals from GO's hacks tab to NGU hacks' targets
     - transfer wish stats from NGO to GO's wishes tab (EMR3-PC, wish speed, and blue heart)

    **requires adding bookmarklets to your browser - see [GO integration bookmarklets](https://github.com/jshepler/jshepler.ngu.mods?tab=readme-ov-file#go-integration-bookmarklets) above**

109. adds dual-wield effectiveness to the second weapon slot's item tooltip and adjusts current stats displayed to account for current effectiveness

110. auto-allocation of mayo generators - hold alt key when on cards screen to change the "CAST", "PROTECT", "YEET" buttons to:
      - OFF: disable auto-allocation of mayo generators
      - LOW: runs 1 or more generators only on lowest mayo amounts
      - ALL: runs all generators in order of lowest to highest mayo amounts

111. options to replace the images for: default player portrait, default daycare kitty, troll kitty

112. shift-right-click on zone forward (right-arrow) button to advance to the zone marked as the "autoadvancer" zone from the "Adventure Advancer" AP purchase

113. adds titan bonus exp kills remaining (from perk 34) to zone drop table tooltip

114. shift-clicking the "-" button to remove a resource from a bar will remove all of the resource, ignoring the "Input" field

115. when using the "buy all custom EMR3" mod, right-click instead of left-click to spend all exp (in multiples of the custom purchase) similar to right-clicking perks and quirks

116. twitch integration to map custom rewards to remote triggers, read [Twitch Integration](https://github.com/jshepler/jshepler.ngu.mods?tab=readme-ov-file#twitch-integration) above for setup

117. cooking helper:
      - prefixes ingredient names with pair number to see which ingredients are paired
      - suffixes ingredient names with targets (`ingred target`:`pair target`)
      - shift-click the `-` button to reset all ingredients to 0
      - shift-click the `+` button to set optimal levels to all ingredients to give 100% meal efficiency
      - hold alt to highlight ingredient pairs that are at optimal values

118. can name games by putting `-game "game name"` in Steam's launch options (right-click game, properties, general tab)

     ![steam launch options](steam_launch_options.png)

     save files are kept in a separate folder named with the game name

119. adventure button changes to yellow to indicate when "Move 69" is ready. Starts when move is unlocked and stops when no longer need to use it.

120. right-click on inventory button to merge/boost all (same that happens with auto merge/boost timer triggers)

121. displays excess EMPC under `EQUIPMENT BONUSES` - how much of the bonus from equipment is "wasted" as it puts you over hard cap

122. switches to basic training screen when rebirthing and don't yet have the Instant Training Cap AP purchase

123. BT skill bars are faded until reach max cap reduction - visualization to help prevent rebirthing too soon and/or getting skills out of sync

124. BT button changes to yellow when a skill is unlocked and stays at level 0

125. option to config file to set custom resolution - the game keeps a 16:10 scaling, so if you don't set a 16:10 resolution you will have black bars

     ***this is an alternative to using the launch options* `-screen-width 1280 -screen-height 800`**

126. tracks some resources gained this/last rebirth:
      - EXP shown on `Spend EXP` button tooltip
      - AP shown on `4G's SELLOUT SHOP` button tooltip
      - PP shown on the PP icon's tooltip (perks screen)
      - QP shown on QP icon's tooltip (quirks screen)
      - seeds shown on seeds icon tooltip (ygg screen)
      - poop shown on poop icon tooltip (ygg screen)

127. shift-click equipped items or items in daycare to toggle PROTECTED (like it does when shift-clicking items in inventory)

128. capping wish R3 - when auto-allocating resources to wishes, R3 will be capped to be no more than what's needed for the minimum wish completion time

     auto-allocation happens when:
      - alt-clicking + button to split resources between multi-selected wishes
      - when a wish completes a level < max level
      - when a wish completes all levels and wish queue is enabled, resources get transferred to new wish and R3 is rebalanced/capped amongst all running wishes

     the excess R3 will be used in remaining wishes and if all wishes have their R3 capped, excess will return to idle

     ***R3 capping can be disabled in config file***

129. when wish queue is disable and wish reaches max level, its resources will be reallocated to the remaining wishes - R3 capping will happen if enabled

130. new regular cards (not chonker or THE END cards) are inserted before first chonker card in an attempt to help keep them organized/separated and to make it a little easier to cast/yeet normal cards

131. fix game bug with cap buttons that manifests in sadistic, when cap > hard cap

     TLDR; the cap buttons allocate based on the cap that's clamped to hard cap, but the progress a bar gains per tick uses the real cap that can be higher than hard cap

132. cube boosting info:
      - cube tooltip shows current boost divider
      - boost tooltips show how much will be added to cube

133. prepends item id to item name on item tooltips

134. digger loadouts
      - the save and load diggers button text changed to show which loadout will be saved/loaded
      - holding alt changes the digger page buttons to digger loadout buttons and highlights current loadout
      - while holding alt, click the loadout buttons to change loadouts (can also just press alt-1, alt-2, or alt-3)
      - when saving a digger loadout, the current levels will be saved and used as soft-caps when loading a loadout using alt-click or alt-#

135. added indicator for when any fruit needs manual activation - Yggdrasil button turns red (option in cfg to enable, defaults to false)

136. added indicator for when any digger can be upgraded - Gold Diggers button turns yellow (option in cfg to enable, defaults to false)

137. shift-click the "Use" button on some consumables to convert them to another consumable:
      - 24 EMR3 potion alphas to 1 potion delta
      - 24 lucky charms to 1 super lucky charm
      - 5 EM bar bars to 1 muffin

138. shift-click the + button next to a hack's target to previous milestone

     alt-click to set target to hard cap
 
     alt-shift-click to set all targets to hard cap

139. pressing shift-F5 will do a clean quick save - doesn't include any save data from my mods (~~useful for loading into GO~~ no longer needed)

140. when in a challenge and not at max zone, adventure button lights up yellow (except when in ITOPOD)

141. ~~shift-click the buy button on SOME consumables to sell them back. I tried to make sure nothing could be cheesed to make more AP (e.g. selling 24 epot1 and buying 1 epot3 results in gaining 20 AP).~~ ***DISABLED***
      - energy potions beta and delta (not alpha)
      - magic potions beta and delta (not alpha)
      - res3 potions beta and delta (not alpha)
      - energy bar bars
      - magic bar bars
      - super lucky charms
      - muffins
      - infusers
      - pens

142. tooltip to quest description text that shows breakdown of qp reward

143. tooltip to fruits' eat/harvest button that shows what will be gained if eaten/harvested now

144. fixes game bug with walderp fight that if player survives the explosion from doing the wrong attack, walderp's next attack would be another explosion due to the code thinking the player didn't do any attack in time

145. added the cube root of total drop chance modifier to Stats Breakdown | Misc Adventure

146. added tracking infinity cube's power and toughness gained this/last rebirth, and estimates number of days until soft cap and next tier (based on p/t gained last rebirth and last rebirth time)

147. saves last rebirth time (for cube estimated days calc), shown on rebirth button tooltip

148. option for quests to be always random and if disabled, alt-click the start quest button to get a random one

149. keys for working with cards:
      - enter (cast), delete (yeet), space (toggle protected)
      - arrow keys to move selection around the page
      - page up/down to change pages

150. adds time per level breakdown to daycare items' tooltip (hold alt)

151. adds daycare time modifier breakdown to stat breakdown - misc

152. press F7 to open saves folder in windows explorer

153. cooking math breakdown shown in meal tooltip

154. achievement ids are shown on achievement tooltips

155. T8 combat helper - the color of the warning text for the explosions attack is set to blue

156. the stat breakdown pages auto refresh every 0.5 seconds

157. when a troll resets energy and you have the `Instant Training Cap` AP purchase, 12 energy will auto allocate to BT (just like it does when swapping loadouts and have the `Unassign E/M on Loadout Swap` setting enabled)

158. options to auto sort/yeet cards, sorts by: rarity > bonus type > bonus amount
      - press F1 while on cards screen to open in-game config panel
      - enable/disable auto yeet/sort
      - set sort by: rarity first, bonus type first, mayo efficiency, bonus variance
      - set sort direction
      - set auto yeet option: max rarity, max efficiency, max variance
      - auto yeet options ignore END cards as they would alywas get yeeted no matter the setting
      - select which card(s) to always yeet regardless of rarity, efficiency, or variance
      - enable/disable automatically protecting chonker cards (vanilla always protects, this makes it optional)
      - enabled or not, can press the s/y keys to sort/yeet cards

159. blood gain modifiers breakdown to `Stats Breakdown - Misc`

160. breakdown of resource gain sources to their tooltips (hold alt):
     - EXP sources on the `Spend EXP` button tooltip
     - PP sources on the PP gained this/last rebirth tooltip, on the Perks page over the PP icon top-right
     - QP sources on the QP gained this/last rebirth tooltip, on the Quirks page over the QP icon top-right
     - AP sources on the `4G's Sellout Shop` button tooltip

161. LSC reminder - under certain conditions, the `Challenges` button on the `Rebirth` screen will highlight yellow:
      - Laser Sword Challenge is unlocked
      - < 20 completions in the current difficulty
      - the total time to target of both `Laser Sword` and `Quadruple Laser Sword` from 0 to current challenge target level is less than the configured amount (in cfg file), default is 5 minutes

162. hides EMR3 speed purchase buttons when base hits 50

163. AP gain to rebirth screen, quest reward text, and next pit reward tooltip

164. additional info to idle progress bar tooltip: time per drop, total quest time, average quests per day, time remaining to complete quest

165. TM speed modifiers to stat breakdowns - misc

166. additional info to mayo generator tooltips: time per mayo and mayo generated per day

167. mayo generation speed modifiers to stats breakdown - misc

168. option in config file to override the computer's current culture - used when formatting numbers

169. total spin count to daily spin rewards table tooltip after reaching max tier - vanilla stops showing the spin count

170. new wish order, TTL (time to level), orders by the time remaining to wish's next level (based on max resources, ignores min time) - added after TOTAL COST

171. tooltip added to cards to show mayo efficiency and bonus variance, hold alt to see more info

172. in-game config panel for card auto sort/yeet - press F1 on cards screen, no longer need to exit game and edit cfg file for these options

173. combat helper for the traitor (T14) - shows IC (invincible counter), GR (growth rate), GC (grow count)

174. shows time per major quest generated, number per day, and time to full bank on the quests screen, under the current timer for next major quest

175. on the cards screen, replaces the mayo generator number running/max with total mayo generated per day

176. time targets for augs, AT, TM, NGUs - calculates and sets target level where time to target is at (or close to) specified time
      - set number of minutes in the resource input box at the top of the screen
      - ctrl-click the - button to set the target for that bar
      - alt-ctrl-click any of the - buttons to set the targets for all the bars (except augs, which only do the aug+upgrade pair)

177. help to prevent accidentally allocating resources to hidden wishes - by default, when the game is launched, wish 0 is pre-selected and is probably being filtered out
      - when loading a save, the first visible wish will be pre-selected
      - prevents allocating resources to a maxed wish

178. fixes game bug that would start showing T10's spawn timer after beating boss 125 instead of 175

179. fixes game bug where "Welcome to Sadistic Difficulty" perk wasn't included in stats breakdown for augs and NGUs

180. adds [NGU YIELD FH] to fruit tooltips to as a reminder of which bonuses affect that fruit
     - NGU = NGU YGGDRASIL
     - YIELD = Yggdrasil Yield bonus from equipment + quirk 92
     - FH = First Harvest, perk 51
     - if bonus doesn't apply, it'll be grey
     - if bonus does apply, it'll be blue

181. right-clicking the E/M/R3 stats (upper-left of game screen) does the same as pressing the r, t, and f keys

182. when clicking `On To The Game` from the offline progression summary screen, will change to inventory screen (if unlocked) instead of basic training screen

183. exploder helper - gives 2 second warning when about to explode

     ***this may get removed if makes the fight way too easy***

184. tooltip to base adventure power (in spend exp menu) that shows total base power gained and sources - this is for all time, not this/last rebirth

185. in addition to the shift-right-click and ctrl-shift-right-click in mod 18, right-clicking (no shift or ctrl-shift) the buy all custom energy, magic, resource 3 buttons will repeat buying until not enough exp

186. shift-click the `Clear Wish` button on the wishes screen to clear all resources from all wishes

187. removed restrictions for r3 name - any character allowed, not just letters and spaces, but still requires at least 1 character

188. highlights the page button for current page in various menus

189. idle quest speed breakdown in the idle quest progress bar tooltip - hold alt

190. hardcore (HC) game mode
     - one life
     - local saves disabled - only steam cloud allowed
     - if die in fight boss or adventure zone (including ITOPOD), game over and steam cloud save deleted
     - enabled in cfg file and must start new game
     - can disable to revert to normal game, but then it's no longer a hardcore save and will not be loadable if hardcore is re-enabled
     - not fully tested, please let me know of any issues or if you find where game is soft locked because a death is required to progress
     - **T4 and T10 DO NOT normally require player death to progress so no soft lock**
     - various aspects are open to change based on feedback
     - disables the A/D damage % numbers
     - disables right-click fight boss, pressing b key, and remote trigger - these would start fight boss but only when will win
     - launch option to force hardcore, overriding cfg file: `-HC`
     - *can be enabled at the same time as the PermaTC mod below*

191. permanent troll challenge (PermaTC)
     - trolls constantly spawn every 2 minutes, every 5th a big troll, just like regular troll challenge
     - since it's not the actual challenge, you will get these trolls during regular challenges
     - yes, that means 2x trolls in regular troll challenges
     - yes, that means getting trolls in NORB challenges
     - enabled in cfg file and must start a new game - will not work for an existing non-permaTC save
     - can disable to revert to normal game, but then it's no longer a permaTC save and will not work after re-enabling the option
     - not fully tested, please let me know of any issues or if you find where this mod soft locks progression
     - various aspects are open to change based on feedback
     - *can be enabled at the same time as the HC mod above*

192. support for command-line arguments (aka launch options in steam) for framerate control:
     - `-targetFrameRate [rate]` limits FPS to specified limit (disables vsync), ex: `-targetFrameRate 60` to limit to 60 FPS
     - `-vSyncCount [count]` sets how many frames to sync at, ex: `-vSyncCount 1` (this is game's default setting)
     - if vSyncCount is set and is > 0, targetFrameRate will be ignored

193. show time to next bonus increase to blood spaghetti and counterfeit gold

194. right-click money pit to toss gold and do daily spin

195. shows hack bonus summary in a tooltip when hovering over the WTF button on hacks page

     hold alt to see the times to targets with total time

196. wish tooltips show resource allocation as a % of cap, where 100% = min wish time (e.g. 4 hours)

     if over 100%, you have more than you need to min wish time

     hold alt to see current progress, progress per tick, and min progress per tick - useful in seeing the effect of floats on wishes that are "too slow"

     min progress per tick is the smallest value needed to gain any progress at all - based on current progress value, will change at higher progress

197. shows current time factor for guffs and beards on their tooltips; guffs also show indication if muffin is active

198. on the tooltip for A Giant Seed (item 92), shows how many seeds you'll gain if consumed

199. when an item in daycare (that's not a guff) reaches 100, the daycare button lights up green

200. fixes bugs with walderp's hide-n-seek game:
     - while hiding, going offline would still advance walderp's respawn timer (sounds great, but causes some conflicts where he's both hiding and spawned)
     - the 3 minute timer for moving where walderp hides is done in a way that prevents him from being in any menu for up to 3 minutes after being killed
     - when found, the timer isn't reset so on next kill don't get the full 3 minutes

201. inventory search - press s key while on inventory screen, will already have focus so can just start typing, press ESC to close

202. resource allocation summaries - hold alt while looking at EMR3 tooltips, top-left of the game

203. augment ratio display - hold alt while on augments screen

204. highest evil/sad boss defeated added to misc stats display

205. ratio tool on spend exp - energy, magic, R3 screen - press F1

206. tracks time spent offline and display on misc stats screen

207. % chance to play fart sound when gain poop - chance set in cfg file, defaults to 0

208. displays notification when gain poop: shows +gained, total gained current rebirth, total on hand

209. option to always show full resource names (top-left) instead of first letter, set in cfg file, defaults to none

210. hold shift to buy 10x deck size in AP shop (skips the confirmation popup)

211. fix game bug that wasn't displaying total PP gained on misc stats screen

212. fix game bug that would show empty tooltip on loadout items that were in daycare

213. warning indication of when potions are about to expire
     - sellout shop button flashes red when less than configred time is remaining
     - time set in cfg file, default is 60 seconds

214. boss required for titans are shown on the adventure button's tooltip

215. replaced code that builds zone dropdown list
     - shows boss required to unlock zone
     - shows all zones in current difficulty
     - zones that you haven't ever unlocked are displayed as ???? but can still see boss required
     - zones that you haven't reached in current rebirth are disabled
     - **real fix for not being able to select safe zone** - replaces mod 92

216. tracks player deaths and is displayed on misc stats (info 'n stuff) page

217. blood ritual tooltips now show what the gold per second and blood per second would be if max magic is allocated

218. loot item names in combat log are in bold and can be colored via cfg option, default is #000000 (black)

219. shift-click the buy muffin button to buy a muffin with 1 billion seeds

220. can play music when idling/manualing titans, you can use whatever music you want:
     - must be mp3 files
     - must be placed in config folder
     - filenames must start with this pattern `(Tx)` where `x` is the titan number, e.g. `(T5) Thrill of the Hunt.mp3`
     - some titans have guardians, you can play different file by appending `g` to the titan number, e.g. `(T6g) Stockade Blockade.mp3`
     - if don't have a file for the guardian, will play the file for that titan instead
     - if no matching file found, nothing happens - no music is played
     - discord user Natalie has made theme music for a few titans:
        - [(T1) Nightmare Sauce.mp3](https://discord.com/channels/406611885312704512/554159938990243871/1306328980474953738)
        - [(T5) Thrill of the Hunt.mp3](https://discord.com/channels/406611885312704512/554159938990243871/1292907304038957138)
        - [(T6) Your Heroic Quest.mp3](https://discord.com/channels/406611885312704512/554159938990243871/1329495242872852584)
        - [(T6g) Stockade Blockade.mp3](https://discord.com/channels/406611885312704512/554159938990243871/1329495242872852584)

221. hold alt on the cards screen to change the current mayo amounts to show the total mayo needed for all the cards in the deck

222. shows number of open inventory spaces on the inventory button (ignores merge slots) and changes the button to yellow if < 6 open slots or red if 0 open slots

223. 5 of the ygg fruits have levels that vanilla doesn't show and those levels are used in the bonus calc, this mod adds those levels (and the math) to their tooltips

224. cfg option to show sci/eng notation numbers as suffix numbers when below configured threshold

225. the tooltip on "Total Digger Levels Bonus" now shows the total digger levels

226. replaced beard levels/bonus display with new layout that includes perm level and total bonus

227. adds hp regen to boss hp bar tooltip (fight boss screen)

228. fixes game bug with daily spin tier 0, the 500 AP reward could never be awarded

229. GPS breakdown to stat breakdowns - misc adventure; basically the same that's shown on the TM screen but in a nice list with total

230. fixed game bug when clicking a digger's cap button would often result in 1 level lower than it could

231. fixed game bug that preventing being able to click the "Auto Spell" text for blood number to toggle the checkbox

232. changes manual combat moves to not start on cooldown when game starts (was added at the same time as mod 91, forgot to put in README)

233. shows estimated time to complete current banked major quests on questing screen

     and shows estimated time to complete current manual quest in questing button's tooltip

234. cfg option to specify which blood spells (IP, GuffA, GuffB) get the purple indicator for being off cooldown, defaults to all of them

235. experimental option to show AT button before unlocked from BT - allows you to pre-assign energy, but won't run until BT unlocks AT

236. experimental option for auto allocators to not get disabled when doing a loadout swap and have the game optin enabled to unassign EM on loadout swap

237. fixed game bug with BT where it could think a skill is locked when it wasn't

238. warning indicators for FoK and FoR when the exp/pp digger isn't active

239. shows notifications when in-game achievements are achieved

240. in the card tags panel, changed the display of current tier to current/max; and changed to use short names to remove word-wrapping

241. modified the NUMBER breakdown on rebirth screen to indicate +/- gains from previous rebirth

242. shows estimated time to boost an item - hold alt on item tooltip for items that can be boosted

243. adds keybinds to equip loadouts - press alt-# to equip loadout#, works in:
  - inventory
  - adventure
  - ygg
  - cooking

244. (experimental) move cooldowns and buffs reset when player/enemy dies - option in cfg file, defaults to disabled

245. heirloom mod - start a new game and take an item with you. ***Not fully tested. Please report any issues or things not properly reset for a new game***
  - to take an item with you, put it in the trash before triggering THE END screen
  - on THE END screen, on the last page, right-click THE END button
  - the mod will also add the following purchased packs: the 5 newbie packs, the R3 pack, and the portraits pack
  - also fixes the glitch that let players view THE END from the cooking screen

246. combat helper for T6 (The Beast) - shows current aura under enemy stats

247. allows you to swap in another copy of the same equipped accessory item - works for loadouts when item is in same slot

248. experimental option to disable the AP purchase confirmation box, and if enabled will do an auto save so can still recover if screw up

249. fixed game bug with 24 hour challenges not properly applying the bonus exp to fight boss kills - was doing 2/0/0% per normal/evil/sad completions instead of the expected 10/4/2%

250. autosave 30s before muffin ends - useful if missed/forgot the rebirth before it ends

251. ygg breakdowns - hold alt on eat/harvest tooltips - uses current numbers/gear/settings

252. proper scaling of mod popups based on game window resolution and windows dpi scaling
  - experimental option in cfg file to apply an additional modifier to scale mod pops more/less

253. changed when advancing hack targets to stay at hardcap instead of wrapping around back to 0

254. added total gold tossed (into money pit) to the Info 'n Stuff | Misc Stats screen (vanilla already tracks it, may as well display it)

255. pause/unpause game using the pause key - only pauses game timers, can still go around looking at stuff and doing things that don't rely on timers

256. shift-click the `Set Autoadvancer Zone` to toggle if it triggers 20s after start of rebirth or not (red = disabled); useful during challenges

257. shows a second row of page buttons for wishes using pgdn/pgup keys - there are 11 pages of wishes in total, but vanilla only shows 9 page buttons

258. added a 4th wish filter - search; press the s key on the wishes screen, press esc to close, matches against wish names and descriptions

259. adds perk/quirk lists - right now, mostly useful to plan future purchases, but I might add more features to this later
  - shift-click perks/quirks to add/remove to the list (has aqua border when in list)
  - press F1 to toggle the popup to manage the list
  - buy = buy 1 level, bulk = buy all it can (same as right-click)
  - for fib perk, bulk only buys up to next reward (same as shift-right-click)

260. on adventure screen, press F3 to go to the last zone you were in; "last zone" is set whenever your zone changes, including from pressing F3

261. when player/enemy dies, shows the total damage player did and dps in combat log

262. on the purchase EMR3 screens (exp shop), the tooltip over the base power value shows the minimum number of levels to buy

      This has to do with floats and when they get big enough, the spacing between values get larger and rounding starts to happen. It becomes possible to spend exp and not get anything for it because it wasn't enough levels to get to the next valid float value.
  
      This mod will show the minimum amount to add that will cause it to round up to the next possible value. This takes advantage of the rounding in our favor - you can spend less exp to get the full jump to the next valid float value.

      This is done by taking the current spacing between valid float values, dividing by 2, and adding 1 to ensure it always rounds up.

      The min starts going up > 1 near the end of evil, at around 16m base power. This is mostly helpful in sadistic.

263. changes the display of base EMR3 power in the exp shop to show the actual value (when spacing >= 1) instead of the rounded value .net normally shows; doesn't affect the base powers displayed in stat breakdowns - those are still rounded.

264. fixes bug that would start showing T12's spawn timer (adventure button tooltip) upon defeating boss 246 instead of 248

265. fixes bug that wouldn't let daycare shockwaves give a level to guffs when their (original) level >= 100

266. fixes bug with perk 34 that would only give the bonus for 2/5/8 **online** manual/idle kills instead of 3/6/9

267. filter and order options to perk and quirk lists and affect the list of perks/quirks on the screen (not the f1 popup)
  - checkboxes are added to the perks/quirks screen (next to the filter options) as well as the f1 popup
  - if filter is enabled, an *additional* filter is applied to include on those perks/quirks in the list
  - if order is enabled, the perks/quirks in the list are shown in list order before the other perks which will be in the selected base order
  - if filter is enabled, but order is not, the displayed perks/quirks will be in the selected base order, *not in list order*

268. adds total ITOPOD kills to the info 'n stuff - misc stats screen, replacing Highest Damage in a Single Hit

269. modified the normal theme for the rebirth and adventure screens to make the background blue and the info panels the dark theme

      added as an experimental option (ShepsTheme), default disabled

270. perks and quirks search, press s to open, esc to close

      the perk search mod will be disabled if the other [perk search mod](https://github.com/postEntropy/NGU_Mods) is installed so won't conflict with it

271. added alt-tooltip to the cards button to show tag order and next card/chonker and if it comes from being tagged or not
