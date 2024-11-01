# jshepler.ngu.mods
Collection of mods I wrote for myself.

I had not intended to make these public, as I don't want to support them long-term (i.e. when no longer playing the game). A few people have asked that I share the code, so here you go.

The majority are QoL enhancements. There are a couple bug fixes, and few thigns that are somewhat cheaty-ish but nothing majorly so.

# Installation
These mods are written for [bepinex](https://github.com/BepInEx/BepInEx) v5.4.21, which can be downloaded [here](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.21). Download the x64 version and extract the contents of the zip file:

![bepinex zip contents](bepinex_zip_contents.png)

to NGU Idle's game folder `...\Steam\steamapps\common\NGU IDLE`:

![NGU folder after extraion](bepinex_extracted.png)

***NOTE: if using linux, need to add `WINEDLLOVERRIDES="winhttp=n,b" %command%` to the game's launch options in steam.***

Bepinex will set itself up the first time the game is run after extracting the zip. So start the game, then exit the game.

Download the latest `jshepler.ngu.mods.dll` file from [releases](https://github.com/jshepler/jshepler.ngu.mods/releases) and put it in `...\Steam\steamapps\common\NGU IDLE\BepInEx\plugins`.

![dll file location](dll.png)

# Configuration
The first time the game is run after installing the mods, a configuration file is created: `...\Steam\steamapps\common\NGU IDLE\BepInEx\config\jshepler.ngu.mods.cfg`.

The config file is only read when the game starts, so exit the game before making changes. If an update adds new config options, you will need to start the game for the new options to be added to the file, then exit the game to configure them.

There are only a few config options at the moment. I'll probably add options in the future for the more questionable things.

# GO integration bookmarklets
***NOTE (unrelated to bookmarklets): importing saves from modded NGU into GO won't work unless you import a clean save - see mods 57 and 139 below.***

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

This sends current hack stats from NGU to GO for the `Hacks` tab.

## sending hack targets from GO to NGU
`javascript:fetch("http://localhost:8088/ngu/go2ngu/hacks",{method:"POST",body:JSON.stringify(appState.hackstats.hacks.map(a=>a.goal))});`

This sends the hack goals from GO to NGU which sets hack targets.

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
(in no partiular order)

1. fix basic training when right-click to evenly split assigned energy and no more than needed per offense and defense

2. rebirth resets auto merge/boost timers

3. include auto merge/boost timers when adding offline progression when loading a save

4. displays effective net gold per second under Gold: main screen: TM net - current consumption per second from TM, BM, Augs

5. auto-save on rebirth, challenge, pit throw, changing wandoos OS

6. press F5 to do quicksave, F6 to load latest quicksave, ignores clean quicksaves, those that have "(CLEAN)" in the name

7. the lazy ITOPOD shifter can raise max floor

8. calculates and displays player damage % and boss damage % to determine if current boss can be fought

    (changes Fight Boss button color: green = can nuke, yellow = can win fight)

9. (WIP) displays "total work" at bottom of equipment bonuses: cap * power per E/M/R3

10. during Walderp fight, displays number of remining regular attacks before "walderp says" attack

11. function to reset total time played by pressing RightControl-F1 when on Misc. Stats page

12. displays total blood gain per second under "Blood:" on blood magic page

13. alters tooltips for "Blood Spaghetti" and "Counterfeit Gold" spells to show amount gained when clicked (like Iron Pill does)

14. calculates amount of advanced training needed to autokill titans and displays in the Adventure button's tooltip

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

19. shift-click load button to skip offline time progression when loading a save (open file dialog title indicates when progress will be skipped)

20. appends to adventure zone's tooltip the AT power needed to kill current enemy in 1-5 hits

21. show current quest item drop rate under the quest item drop modifier on the questing screen

22. during manual questing, set questing button color to yellow if the number of quest items in inventory plus the number of quest items already collected are within 5 of the target number (only if got the AP purchase "Quest Reminder!")

23. auto harvest/eat fruits when attain full growth (max tier)

    also on rebirth any fruit >= tier 1

    (auto harvest max tier fruits now have an config option to disable)

24. when starting a manual quest, if current zone has unlocked quest item, that item is assigned as the quest (instead of random quest item)

25. improved bar tooltips, adds:
    - current speed cap: augs, AT, TM, wandoos (all OS at same time), NGUs ***(displays real cap, even if over hardcap)***
    - % of cap allocated: augs, AT, TM, BM, wandoos, NGUs, beards\*
    - over-capped duration (time until no longer over-capped): augs, AT, TM, wandoos, NGUs, beards\*
    - current speed - progress per tick & ticks per bar: augs, AT, TM, wandoos, NGUs, beards
    - time to target: augs, AT, TM, NGUs, hacks
    - banked amounts to: AT, TM, beards

    \* I know you don't allocate resources to beards, but the game still calculates progress per tick, which can be used to show a % "allocated" in order to show how much over "cap" the bar is, and to calc/display how long until no longer capped (i.e. how long it will be BB'd) - this helps to project when you won't need beard diggers running to stay BB'd for 24 hour rebirths
        
26. shows current wandoos speed caps for all 3 OS types simultaneously on each bar's tooltip

27. right-click on questing button adds quest items from inventory and completes the quest if target amount met

28. allow target inputs to accept/display scientific and engineering notations, based on number display style in settings

    currently: Advanced Training, NGUs

29. added boss # to displayed "Highest Boss Multiplier" on time machine page (mostly as an exercise of transpiler patch injecting delegate call)

30. added display of digger gps drain diff between current and next level - because the current net gps needs to be enough to cover the diff, not the total

31. toggle auto-allocate resources per bar (shift-click the + button) - idle resources evenly split between enabled bars, disables when hit target

    current bars: Basic Training, Augments, Advanced Training, Time Machine, Blood Magic Rituals, Wandoos, NGUs, Hacks

32. alt-shift-click enables auto-allocate on multiple bars: all ngu energy, all ngu magic, augment+upgrade pair, basic training offense/defense pair (if sync training enabled), TM pair, wandoows pair

33. directly modify loadouts without having to change currently equipped gear:

    right-click loadout item to clear it

    left-click loadout item to enter "selection mode": closes loadout panel, left-click inventory or equipped item to assign it to the slot

    press escape to cancel "selection mode"

34. shift-right-click questing button to toggle automate manual major quest: auto-collect items, auto-complete quest, auto-start new major quest, auto-use butter (option in cfg file)

    doesn't automatically switch adventure zone - assumes already in quest zone and the above mod that uses current zone when starting quest

    when run out of major quests, will start minor idle quest and change zone to ITOPOD

35. left-alt-click + button to split resources into all runnable bars (i.e. target = 0 or level < target), such that bar speeds are equal

    currently: augment pairs (aug+upgrade), advanced training, NGUs

36. added total boost from recycling to boosts' toolips

37. added boost modifier breakdown to stat breakdowns / misc

38. control-click the + button to calculate and allocate over-cap amount based on target level or target rebirth time (in minutes)

    target: -1 = ignore, 0 = target rebirth time, >0 = target level

    target rebirth time by entering total minutes into the resource input box at top of screen (e.g. to target rebirth time 24:00:00, enter 1440)

    current bars: time machine (both bars), advanced training, augments, NGUs (control-alt-click to do all energy or magic bars)

39. LeftShift-R, LeftShift-T, LeftShift-F removes all of the respective resource from the currently viewed feature (instead of all features)

    currently implemented: basic training, augs, adv training, time machine, blood magic rituals, wandoos, ngu, hacks

40. Upgrade All Diggers by right-clicking the "Gold Digger" button; will upgrade all diggers in order of cost, repeating until not enough gold for any upgrade

41. fruit tooltips show what was last gained (persisted in save)

42. added the current softcaps to infinity cube's power and toughness stats (red means < softcap, green means >= softcap)

    also removed the softcap warnings from the tooltip because it adds clutter for no benefit (for those that already know about cube softcaps)

    added uncapped power and toughness (what's used to determine tier) and amount needed for next tier

43. temp loadout saves currently equipped gear when loading a loadout (regular or temp), load temp loadout by pressing the X key while on inventory screen

    (persisted in save)

44. change daycare bars from "Levels Gained" to show the level the item will be if taken out; if non-macguffin, shows time remaining until level 100. also adds item's current time per level to tooltip

45. fixed bug with basic training's auto advance toggle state not being saved or applied on load

46. test fix (only for energy NGUs) for when calculating how much of cap value to allocate when clicking an NGU's cap button to not clip the cap value to hard-cap

47. when a piece of equipment is maxxed (level 100), automatically enables the item filter for that item (except looties and pendants)

    exceptions: looties, pendants, flubber, wanderer's cane

48. display all fibonacci perk unlocks, green are unlocked, red are locked

49. (WIP) changed how y position of tooltips changes when it would go off the top of the screen to stay up against the top instead of flipping down because some tooltips (e.g. fibonacci perk) would end up going below bottom of screen

50. add tab navigation to various fields, supports shift-tab and wrap-around (tabbing past first/last field, wraps to other end)

    currently: augment targets, advanced training targets, time machine targets, NGUs targets, digger levels, Hacks targets, EMR3 custom purchase amounts

51. shift-click cap button to get a popup of buttons for the partial caps 10-50%, click to allocate that amount

    (note: these are the amounts for bar fill speeds: 2/3/4/5/6/7/8/9/10 ticks per bar and is what the existing cap buttons allocate)

    currently: blood rituals

52. added tooltip to Money Pit button that shows what the next reward will be as well as the time remaining, AP gained, and toss multiplier (for rewards that get it)

53. press control-shift-x to open the hidden krissmuss 2020 event screen

54. unlocked krissmuss 2019 ui theme

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

61. added right-click costs and levels gained to perk and quirk tooltips

62. added shift-right-click on fib perk to only buy up to next unlock

63. right-click on the enter itopod button to directly enter the tower, skipping the popup

    (if shifter is enabled, will set optimal floor first)

64. (WIP) press control-r to restore all unassigned resources from doing things like pressing R or swapping loadouts and having unassign E/M enabled

    can also press alt-r to take a current "snapshot" to restore later

    [currently disabled - not working quite right]

65. can re-arrange daycare items directly and without resetting timers (i.e. losing progress)

66. (WIP) adds tooltips to the Adv Training, Time Machine, and Beards of Power buttons to show total bank %

    (temp placement, plan to move this info somewhere else)

67. added wish queue
    - shift-click a wish to add/remove to/from the queue
    
    - press q to toggle the queue window where can delete a wish, re-order wishes, and clear the queue
    
    - the queue window has a toggle to enable/disable the queue (setting saved in cfg file)
    
    - if queue is empty, next available wish (by current filter/order) will start

    - when loading a save, after offline progression finishes and any wishes have completed, if queue is enabled, will start next wish

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

77. auto-sniping: skips normal enemies on adventure screen, if < 50% health after boss dies (or if player dies) stays in safe zone until full health

    go to desired zone and press F1 (or right-click idle button) to toggle auto-snipe (changes border of the Idle Mode button to red)

    there's a config option to snipe specified target when in specified zone, instead of that zone's bosses

78. on the rebirth screen, the rebirth button will be red if there is "Crap to do before rebirthing"

79. removed "not less than 10k" limit on buy custom energy/magic cap, lowest is what you can get for 1 exp (e.g. 250 energy cap)

80. fixed bug with Jake's (T3) locusts attack - the attacks aren't supposed to start until the turn after the warning, currently it's the next frame

    (also changed the color of the warning to blue so it's more noticeable)

81. added remote triggers (using HttpListener) that accepts specific requests to trigger functions in game, `http://localhost:8088/ngu/<command>/`

    current commands:
    - autoBoost: executes in-game auto-boost code
    - autoMerge: executes in-game auto-merge code
    - tossGold: executes in-game money pit's "Feed Me"
    - fightBoss: if able to win fight, switches to boss screen, nukes (if able), fight boss (if winable), returns to prev screen
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
    - PP per day
    - kills per EXP drop
    - EXP per drop
    - EXP per day
    - the optimal floor even if > max floor

    time to next PP uses 2 calcs: one for when floor \<= optimal floor and another > optimal floor

    \<= optimal floor, 1-shot kills using respawn time + idle attack time

    \> optimal floor, uses rolling average times of last 5 kills

86. adds current zone's drop table as an alternate zone tooltip (hold alt key while hovering mouse over zone description)

    (has options in config file to control when/how some items are displayed)

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

92. fixed zone dropdown bug that prevented selecting safe zone

93. adds hotkeys to do an merge all (shift-m) and boost all (shift-b)

94. notifications are generated for zones being unlocked

95. right-clicking the Auto Transform buttons on inventory screen will transform all unprotected boosts in inventory

96. show Total Attack/Defense Modifier from augs to the Augments stat breakdown screen

97. display inventory count (used / max) next to the "INVENTORY" label above the inventory grid

98. compare items: while hovering over an item in inventory, press c to lock its tooltip to the top-left of screen, press esc to close it

99. option to change default player portrait to boss portait specified in cfg file

    in `[DefaultPlayerPortait]` section, set `BossId` to the number from the in-game bestiary

100. hold alt when viewing an item's tooltip to see a list of where that item drops

101. highlight currently equipped loadout button on inventory screen

102. options to enable/disable each of the 3 allocators (defaults to disabled)

103. when right-clicking an accessory, it will get equipped in the first open slot

     order follows same order as auto merge/boost, described in [the wiki](https://ngu-idle.fandom.com/wiki/Inventory#AutoMerge_&_AutoBoost)

104. save pruning: when an autosave or quicksave happens, saves older than DaysToKeep (in cfg file) are deleted

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

    **requires adding bookmarklets to your browser - see [GO integration bookmarklets](https://github.com/jshepler/jshepler.ngu.mods?tab=readme-ov-file#go-integration-bookmarklets) above**

109. adds dual-wield effectiveness to the second weaopon slot's item tooltip

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

  *the ingredient text will auto-resize to fit (thanks Ms. Rager)*

118. can name games by putting `-game "game name"` in Steam's launch options (right-click game, properties, general tab)

     ![steam launch options](steam_launch_options.png)

     save files are kept in a separate folder named with the game name

119. adventure button changes to yellow to indicate when "Move 69" is ready. Starts when move is unlocked and stops when no longer need to use it.

120. right-click on inventory button to merge/boost all (same that happens with auto merge/boost timer triggers)

121. displays excess EMPC under `EQUIPMENT BONUSES` - how much of the bonus from equipment is "wasted" as it puts you over hard cap

122. switches to basic training screen when rebirthing and don't yet have the Intant Training Cap AP purchase

123. BT skill bars are faded until reach max cap reduction - visualzation to help prevent rebirthing too soon and/or getting skills out of sync

124. BT button changes to yellow when a skill is unlocked and stays at level 0

125. option to config file to set custom resolution - the game keeps a 16:10 scaling, so if you don't set a 16:10 resolution you will have black bars

     ***this is an alternative to using the launch options* `-screen-width 1280 -screen-height 800`**

126. tracks some resources gained this/last rebirth:
  - EXP shown on `Spend EXP` button tooltip
  - AP shown on `4G's SELLOUT SHOP` button tooltip
  - PP shown on `I.T.O.P.O.D PERKS` button tooltip (adventure screen)
  - QP shown on `Questing` button tooltip
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

131. fix game bug with cap buttons that manifests in sadistic, when cap > hardcap

     TLDR; the cap buttons allocate based on the cap that's clamped to hardcap, but the progress a bar gains per tick uses the real cap that can be higher than hardcap

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

138. shift-click the + button next to a hack's target to set that hack's hard cap

139. pressing shift-F5 will do a clean quicksave - doesn't include any save data from my mods (useful for loading into GO)

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

143. tooltip to fruits' eat/harevest button that shows what will be gained if eaten/harvested now

144. fixes game bug with walderp fight that if player survives the explosion from doing the wrong attack, walderp's next attack would be another explosion due to the code thinking the player didn't do any attack in time

145. added the cube root of total drop chance modifier to Stats Breakdown | Misc Adventure

146. added tracking infinity cube's power and toughness gained this/last rebirth, and estimates number of days until softcap and next tier (based on p/t gained last rebirth and last rebirth time)

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
  - cfg file option to enable/disable auto yeet/sort
  - cfg file option to set sort by: rarity first, bonus type first, mayo efficiency, bonus variance
  - cfg file option to set sort direction
  - cfg file option to set max rarirty to yeet
  - cfg file option to set max efficiency to yeet (overrides max rarity)
  - enabled or not, can press the s/y keys to sort/yeet cards

159. blood gain modifiers breakdown to `Stats Breakdown - Misc`

160. breakdown of resource gain sources to their tooltips (hold alt):
  - EXP sources on the `Spend EXP` button tooltip
  - PP sources on the PP gained this/last rebirth tooltip, on the Perks page over the PP icon top-right
  - QP sources on the QP gained this/last rebirth tooltip, on the Quirks page over the QP icon top-right

161. LSC reminder - under certain conditions, the `Challenges` button on the `Rebirth` screen will highlight yellow:
  - Laser Sword Challenge is unlocked
  - < 20 completions in the current difficulty
  - the total time to target of both `Laser Sword` and `Quadruple Laser Sword` from 0 to current challenge target level is less than the configured amount (in cfg file), default is 5 minutes

162. hides EMR3 speed purchase buttons when base hits 50

163. AP gain to rebirth screen, quest reward text, and next pit reward tooltip

164. additional info to idle progress bar tooltip: time per drop, total quest time, average quests per day, time remaining to complete quest

165. TM speed modifiers to stat breakdowns - misc

166. addtional info to mayo generator tooltips: time per mayo and mayo generated per day

167. mayo generation speed modifiers to stats breakdown - misc

168. option in config file to override the computer's current culture - used when formatting numbers

169. total spin count to daily spin rewards table tooltip after reaching max tier - vanilla stops showing the spin count

170. new wish order, TTL (time to level), orders by the time remaining to wish's next level - added after TOTAL COST

171. tooltip added to cards to show mayo efficiency and bonus variance, hold alt to see more info

172. in-game config panel for card auto sort/yeet - press F1 on cards screen, no longer need to exit game and edit cfg file for these options

173. combat helper for the traitor (T14) - shows IC (invincible counter), GR (growth rate), GC (grow count)

174. shows time per major quest generated and number per day on the quests screen, under the current timer for next major quest

175. on the cards screen, replaces the mayo generator number running/max with total mayo generated per day

176. time targets for NGUs - calculates and sets target level for that NGU to run for x minutes
  - set number of minutes in the resource input box at the top of the screen
  - ctrl-click the cap button next to desired NGU or ctrl-click the cap all button to set targets for all

177. help to prevent accidentally allocating resources to hidden wishes - by default, when the game is launched, wish 0 is pre-selected and is probably being filtered out
  - when loading a save, the first visible wish will be pre-selected
  - prevents allocating resources to a maxed wish

178. fixes game bug that would start showing T10's spawn timer after beating boss 125 instead of 175

179. fixes game bug where "Welcome to Sadistic Difficulty" perk wasn't included in stats breakdown for augs and NGUs
