# SpawnInfra 
- Authors: 羽学
- Source:  [SpawnInfra](https://github.com/1242509682/SpawnInfra)
- This is a Tshock server plugin mainly used for：
- Constructs a prison complex to the right below the spawn point.
- Generates world platforms and tracks above the spawn point.
- Creates a direct access to the Underworld beneath the spawn point.
- Below the Underworld layer, a simple mob arena (200 * 200) * 2 sides is generated.
- The Glow Lake Direct Access can be found along the world track with rain clouds placed at the bottom to prevent fall damage.
- An 8*14 grid of chests is generated at the lower left corner from the spawn point.
- A Hell platform and track are generated 40 blocks deep from the Hell layer.
- Allows the use of commands to generate infrastructure.

## Update Log

```
### v1.7.2
- Changed the command for generating the prison complex to `/spi jy`.
- Added a command for generating wild cave huts: `/spi hs`.
- When on the surface, chest items will be surface items, otherwise they will be cave items (desert box cave items are independent).
- House types: Log Cabin(1), Desert(2), Snowfield(3), Jungle(4), Granite(5), Marble(6), Mushroom(7).
- Cave huts require an entity block underneath them to be generated.
- This command requires the permission: `spawninfra.admin`.

---

### v1.7.1
- Fixed bug where copying a structure once would prevent that Crystal Tower from being placed even after digging it up.
- Restoring structures no longer leaves entities on tiles, removed the "fix crystal towers" configuration option.
- Fixed issue where copying and restoring items such as Item Frames, Weapon Racks, Hat Racks, Plates, Mannequins, etc., could not be restored.
- These can now be worked on using the `/spi pt -f` parameter or the "Fix Furniture Items When Copying Structures" configuration option.

---

### v1.7.0
- Fixed bugs where signs, radio boxes, tombstones, and other readable furniture couldn't paste their information.
- The `/spi bk` restore command now supports restoring chest contents and furniture information like signs.
- Utilized GZipStream to greatly reduce the space occupied by structure files (meaning previous `.dat` files will become obsolete).
- It's recommended to use Shao Siming's CreateSpawn "Copy Structures" plugin for mutual copying to create structure files.
- Note: Change the `.map` suffix of the "Copy Structures" plugin to `.dat` to be used with the new version of "Infrastructure Generation".
- Both use the same read/write code; "Copy Structures" is a standalone version while "Infrastructure Generation" is an integrated version.

---

### v1.6.9
- Optimized the speed of chain placement and tile restoration.
- Added asynchronous thread protection to avoid multiple tasks running simultaneously.
- Using `/spi reset` no longer backs up `bk.dat` restoration files, only generating compressed packages when there is a `cp.dat`.
- Fixed issues where World Platforms and Combat Platforms wouldn't place items held in hand.
- Fixed problems with commands not generating corresponding structures when configurations for World Platforms, Express Tunnels, Warehouses, and Prisons were disabled.
- Fixed issues with Temples, Dungeons, and Pyramids causing players to get stuck inside (now generated underfoot).

---

### v1.6.8
- Removed the requirement for needing to join the server to use `/spi list` (for easier console viewing).
- Added "Chain Placement" mode for placing blocks (no need to draw selection zones):
  - `/spi t 4` — Chain replace blocks.
  - Hold an item and enter the command, then breaking a tile instantly replaces it (requires re-entering the command each time).
- Added a "Maximum Tile Replacements in Chains" configuration option.

---

### v1.6.7
- Added the "Fix Crystal Towers When Copying Structures" configuration option.
- Not recommended due to a bug where placed crystal towers would always appear on the map.
- When "Fix Chest Items When Copying Structures" configuration is off:
  - Admins can use `/spi pt -f` or `/spi pt name -f` to forcefully fix chests.
  - Non-admins will receive a permission error; enabling the configuration allows `-f` parameter and admin rights to forcefully fix without them.

---

### v1.6.6
- Added new configuration switches: "Fix Chest Items When Copying Structures", "Backup Structures on Reset", "Clean Structures on Reset", "Auto Infrastructure on Reset".
- Fix Chest Items When Copying Structures: Only affects copied and pasted chests, does not work with `/spi back` restores (this logic has yet to be handled).

---

### v1.6.5
- Fixed issues preventing interaction with copied furniture including:
  - Chests, Item Frames, Weapon Racks, Signs, Tombstones, Radio Boxes, Logic Sensors, Mannequins, Plates, Crystal Towers, Scarecrows, Coat Racks.

---

### v1.6.4
- Reduced memory consumption during "copy-paste undo".
- Copied and restored data is stored in the SPIData folder within the Tshock folder:
  - **.bk.dat** contains player-generated restoration tile data after pasting or modifying selections.
  - **.cp.dat** contains clipboard tile data.
- This addresses the loss of "copy data" upon server restart and supports cross-map copy-pasting.
- "Copying" now supports saving structures with specified names: `/spi cp name`
  - If no name is specified, it uses the clipboard under the player's own name (same for pasting).
- Added `/spi list` command to display available structures for copying.
- `/spi reset` not only automatically generates infrastructure at spawn but also backs up all structural data before clearing.

---

### v1.6.3
- Every operation on a selected region records tile cache, allowing restoration to the state before the last operation.
  - Restore Command: `/spi bk`
  - Copy Command: `/spi cp`
  - Paste Command: `/spi pt`
  - Pasting also supports restoration (pastes above the player's head)
- These commands require setting up the selection zone using `/spi s` to function.
- Currently does not support range restoration for non-selection-based commands.

---

### v1.6.2
- Fixed a bug with `/spi p` area painting where incorrect ID recognition led to incorrect coloring.

---

### v1.6.1
- Fixed mislabeling between `/spi t` and `/spi w` descriptions (1 preserves, 2 doesn't preserve).
- Fixed issues with `/spi sg` — generating Monster Arenas still having blocks covering Dungeon Bricks and Jungle Lihzahrd Bricks.
- Fixed issues with `/spi w` — area walls command, where parameter 2 shouldn't clear blocks but instead clears walls before placing.
- Fixed issues with `/spi t` — area blocks command, where parameter 3 shouldn't clear walls but should clear blocks before placing.
- Added 'replacement' parameters to `/spi t` — area blocks command, preserving half-brick characteristics:
  - 1 preserves placement, 2 replaces, 3 clears then places.
- Added more options to `/spi c` — area cleanup command, and fixed a bug where parameter 1 only cleared boundary points for blocks:
  - 1 blocks, 2 walls, 3 liquids, 4 paint, 5 non-ghost, 6 wires, 7 actuators, 8 clear all.
- Added functionality to toggle block ghosting to `/spi p` — area paint command: 1 paints blocks, 2 paints walls, 3 all, 4 toggles ghosting.
- Added full brick recovery to `/spi bz` — area half-brick command: 1 top-right slope, 2 top-left slope, 3 right half-brick, 4 left half-brick, 5 recover full brick.
- Modified `/spi wr` — area wiring command so parameter 4 no longer ghosts blocks but adds actuators after laying wires:
  - 1 lays wires only, 2 clears then lays, 3 clears then lays and ghosts, 4 clears then lays and adds actuators.
- Changed parameters in `/spi yc` from 0 to 3 to: 1 water, 2 lava, 3 honey, 4 glowing water.

### v1.6.0
- Added **[Block Placement Prohibition List]** configuration option, which applies during structure generation via commands:
  - When generating Monster Arena / World Platform / Combat Platform / Express Tunnel, placement will stop if it encounters a tile ID listed in this table (default: Dungeon Brick ID 3 and Jungle Lihzypad Brick ID).
- Added `/spi p` command for applying paint within selected regions, with further optimization of region-related editing commands:
  - Fixed bug where `/spi c` would generate natural dirt walls during placement.
  - Using `/spi s` with an empty input will display the following instructions:

> Make sure point s1 and s2 are not at the same location to form a line or square.  
> Usage: `/spi s 1` to place or dig the top-left block  
> Usage: `/spi s 2` to place or dig the bottom-right block  
> Clear area: `/spi c 1 to 4` (1=blocks, 2=walls, 3=liquids, 4=all)  
> Place held block in selection: `/spi t 1 or 2` (1=preserve existing, 2=clear then place)  
> Place held wall in selection: `/spi w 1 or 2` (1=preserve existing, 2=clear then place)  
> Apply paint from hand: `/spi p 1 or 2` (1=paint blocks, 2=paint walls, 3=all)  
> Clear and fill with liquid: `/spi yt 1 to 4` (1=water, 2=lava, 3=honey, 4=glowing water)  
> Set blocks to half-bricks: `/spi bz 1 to 4` (1=top-right slope, 2=top-left slope, 3=right half-brick, 4=left half-brick) [1/2 good for elevators; 3/4 for mob push platforms]  
> Add wiring to blocks: `/spi wr 1 to 4` (1=add wire only, 2=clear then add wire, 3=clear, wire + ghost effect, 4=clear, wire + ghost + actuator)

---

### v1.5.9
- Added support for generating Treasure Chests, Traps, Jars, Life Crystals, Enchanted Sword Graves, Pyramids, Demon Altars, and Corruption areas.
- Added `/spi s [1/2]` command to define a region used by the following commands:
  - `/spi c`: Clears all tiles in the selected region.
  - `/spi t`: Clears the region and places the block in your hand.
  - `/spi w`: Clears the region and places the wall in your hand.
  - `/spi yt`: Clears and fills with liquid (water/lava/honey/glows)
  - `/spi bz [1/2/3/4]`: Converts selected blocks into sloped/half bricks.
- Permission `spawninfra.use` allows usage of:
  - Region selection, clearing, placing blocks, liquids, walls, and setting half-bricks.
  - Commands: House, Fish Pond, Monster Arena, World Platform, Express Tunnel, Combat Platform, Demon Altar.
- Exclusive admin-only commands under `spawninfra.admin`:
  - Generate Prison, Warehouse, Dungeon, Temple, Trap, Jar, Glowing Lake,
  - Treasure Chest, Corruption Area, Pyramid, Life Crystal, Enchanted Sword Grave.

---

### v1.5.8
- Improved command system, allowing players to generate structures in-game.
- Command permissions now use: `spawninfra.use` and `spawninfra.admin`.
- Command name changed to: `/spi`.
- Supports generating:
  - Small houses, prison clusters, chest clusters, dungeons, temples, glowing lakes,
  - Monster arenas, fish ponds, express tunnels, world rail platforms, combat platforms.

---

### v1.5.7
- Current version compatible with TShock 5.2.4.
- Added **[Enable Walls and Item Frames]** option for Chest Clusters.
  - When enabled, each chest layer adds walls and item frames, increasing spacing by 3 extra blocks (compatible with Xi Jiang's AutoClassificationQuickStack plugin).
- Renamed command `rm` to `rms` (to avoid conflict with RolesModifying plugin by Si Siming).

---

### v1.5.6
- Removed alignment options for left/right solid blocks.
- Added logic to prevent World Platform from being built in space layer.
- Reordered platform generation to run last, avoiding index out-of-bounds errors in small maps that may interfere with other structures.
- Declared this plugin is not suitable for small maps. For medium maps, please manually adjust configuration parameters accordingly.

---

### v1.5.5
- Improved uninstallation function.
- Added **[Server Start Command List]** configuration option, used in conjunction with the [Reward Chest] plugin.
  - It executes commands before placing buildings.
  - Only runs once when **[Auto Generate Infrastructure on Server Start]** is enabled.

---

### v1.5.4
- Added method to increase monster spawn rate.
  - Requires defeating the Wall of Flesh and having all online players at the center of the Monster Arena.
- Changed monster spawning floor to conveyor belts, and spike traps to ghosted wires (removed actuators).

---

### v1.5.3
- Left Sea Platform will no longer be built into the space layer.
- Optimized World Platform behavior for Inverted World and 10th Anniversary Edition seeds.
- Added **[Surround Left Sea Platform With Solid Blocks]** configuration option (required for Inverted Worlds).
- Left-side solid blocks placed at X=0 axis position (only visible in map editors if less than 41 blocks wide), right side same height.
- Left/right alignment is optional — recommended off due to slow loading speed.
- Fixed bidirectional infinite dart trap in the Monster Arena.
  - Boundary darts will always appear as long as the option is enabled, regardless of whether the Express passes through.
- Added half-brick platforms in core area to allow darts to pass through walls into lava zones.

---

### v1.5.2
- Added self-built Glowing Lake functionality (enabling this disables the Glowing Lake Express).
- Optimized distance control for Left Sea/World Platform in 10th Anniversary Edition worlds.
- Improved block-clearing logic for the Left Sea (now preserves liquids) and added artificial sea generation logic for Inverted Worlds.
- Sealed both sides of the Monster Arena to prevent liquid from flowing in.
- When "Express Passes Through" is enabled, a layer of lava will be placed at the bottom to block mob spawns.
- If "Express Passes Through" is disabled, a line of dart traps will be placed on the right side of the Monster Arena.
- Users can now customize whether to place lava, darts, or spike traps in the Monster Arena.
- Enabling `Only Clear Monster Arena Area` will disable `Auto Generate Monster Arena`.

---

### v1.5.1
- Fixed an issue where the 10th Anniversary world did not follow the Glowing Lake Express logic for Inverted Worlds.
- Fixed an issue where the Hell Express could not reach the World Platform in Inverted and Upside-down World types.
- Added basic item placements inside the Monster Arena to match the scale factor.
- Added infinite dart traps and improved the “Fixed Monster Spawn Zone” functionality.
- **Note:** The default configuration parameters are designed for large maps; users of smaller maps should adjust them manually.

---

### v1.5.0
- Added support for Inverted World, Upside-down World, and 10th Anniversary Edition world seeds.

---

### v1.4.0
- Fixed an issue where the Left Sea Platform was too close to the ground, causing it to intersect with sand beaches.
- World Platform and Left Sea Platform no longer use spawn point as the height origin.
- Changed to calculate height from the space layer downward, mainly to better support small worlds.
- The Left Sea Platform now clears all blocks within each level by default.
- Added configuration option to prevent the World Platform from entering the Left Sea area.
- Added switch for "Express Passes Through Monster Arena".
- Added glowing walls to the Hell Express tunnel.
- Corrected the center point of the Monster Arena to align with the X-axis of the spawn point.
- Fixed scaling ratio for the Monster Arena.
- Placed a Garden Gnome in the Monster Arena.
- Increased plugin priority to avoid conflicts with CreateSpawn plugin.

---

### v1.3.0
- Added Glowing Lake Express.
- Added a simple monster arena below the cave layer under the Hell Express tunnel (no special environment, basic wiring only).
- Replaced the stepping stone under the spawn point chests with gray brick platforms.

---

### v1.2.0
- Automatically cleans up all dropped items on the map after infrastructure generation (to fix tile errors caused by tree replacements).
- Grouped toggleable configuration options into their own data groups.
- Added chest generation settings.

---

### v1.1.0
- Plugin renamed to: **Generate Infrastructure**.
- Added surface/Left Sea/Hell Platforms (Tracks), and Hell Express.

---

### v1.0.0
- Extracted the prison/small house method from the World Modifier plugin into a standalone feature.
- Supports customizing prison appearance via configuration options.
- After building the prison, the `Auto Generate Infrastructure on Server Start` option will be automatically disabled.
- To reset, enter `/rm reset` and then restart the server.
```

## Commands

| Syntax                             | Alias  |       Permission       |                   Description                   |
| -------------------------------- | :---: | :--------------: | :--------------------------------------: |
| /spi | /基建 | spawninfra.use | Command menu |
| /spi s [1/2] | /基建 select | spawninfra.use | Modify selected area (draw rectangle) |
| /spi L | /基建 list | spawninfra.use | List copyable buildings |
| /spi c | /基建 clear | spawninfra.use | Clear all tiles in selected area |
| /spi t | /基建 block | spawninfra.use | Place held block |
| /spi w | /基建 wall | spawninfra.use | Place held wall |
| /spi p | /基建 spray | spawninfra.use | Place held spray |
| /spi yt | /基建 liquid | spawninfra.use | Clear area and place liquid (water/honey/lava/glowing) |
| /spi bz | /基建 halfbrick | spawninfra.use | Set selected area blocks as half bricks |
| /spi wr | /基建 wiring | spawninfra.use | Add wiring to selected area blocks |
| /spi cp | /基建 copy | spawninfra.use | Copy selected area as your own building |
| /spi cp name | /基建 copyas | spawninfra.use | Copy selected area as saved building with specified name |
| /spi pt | /基建 paste | spawninfra.use | Paste your named building above head |
| /spi pt name | /基建 pastename | spawninfra.use | Paste specified named building above head |
| /spi pt name -f | /基建 pasteforce | spawninfra.admin | Force paste specific building with chest items restored |
| /spi r amount | /基建 house | spawninfra.use | Build small houses at current position |
| /spi jy | /基建 prison | spawninfra.admin | Generate prison below feet |
| /spi ck | /基建 warehouse | spawninfra.admin | Generate warehouse below feet |
| /spi dl | /基建 dungeon | spawninfra.admin | Generate dungeon (must use on surface for inverted worlds) |
| /spi sm | /基建 temple | spawninfra.admin | Generate temple |
| /spi wg | /基建 glowlake | spawninfra.admin | Generate glowing lake |
| /spi bx | /基建 treasurechest | spawninfra.admin | Generate treasure chests |
| /spi xj | /基建 trap | spawninfra.admin | Generate traps |
| /spi gz | /基建 jar | spawninfra.admin | Generate jars |
| /spi sj | /基建 life | spawninfra.admin | Generate life crystals |
| /spi jt | /基建 altar | spawninfra.use | Generate demon altars |
| /spi jz | /基建 enchantedsword | spawninfra.admin | Generate enchanted sword shrine |
| /spi jzt | /基建 pyramid | spawninfra.admin | Generate pyramid |
| /spi fh | /基建 corruption | spawninfra.admin | Generate corrupted area |
| /spi hs | /基建 cavehouse | spawninfra.admin | Generate cave house |
| /spi sg height width | /基建 monsterarena | spawninfra.use | Generate monster arena |
| /spi yc water | /基建 fishbowl | spawninfra.use | Generate fish bowl (water/honey/lava/glowing) |
| /spi zt | /基建 express | spawninfra.use | Generate express route (direct access) (for inverted worlds, it's above head) |
| /spi wp clearheight | /基建 worldplatform | spawninfra.use | Generate world platform using held block |
| /spi fp width height spacing | /基建 battleplatform | spawninfra.use | Generate battle platform using held block |
| /spi rs | /基建 reset | spawninfra.admin | Auto build infrastructure at spawn point/clear all building data |

## Configuration
> Configuration file location： tshock/生成基础建设.json
```json
{
  "Instructions": "[Glowing Lake Express] height is calculated from [World Platform] to the center of the lake. For [Self-built Glowing Lake], in inverted worlds, it's calculated from Hell upwards; otherwise, it's calculated downwards from the spawn point. Enabling this option disables [Glowing Lake Express].",
  "Instructions2": "[World Platform] for normal worlds is calculated upwards from the spawn point, and for inverted worlds, it's calculated downwards from the space layer. The [Monster Arena Scale Factor] is a multiplier for scaling up, but it's recommended not to set it too high.",
  "Instructions3": "When changing [Number of Chests], also adjust [Spawn Point Offset X]; when changing [Number of Chest Layers], also adjust [Spawn Point Offset Y]. It's not recommended to change [Layer Spacing] and [Chest Width]. Enabling [Walls and Item Frames] will add an extra 3 blocks to the spacing.",
  "Instructions4": "Use the command /rms quantity with permission name: room.use to build Crystal Towers for players (do not use at the spawn point as it may cause map corruption).",
  "Instructions5": "Before resetting the server, execute /rms reset once, or directly include this command in your reset plugin.",
  "Instructions6": "The following parameters are only suitable for large maps and need to be adjusted for medium-sized maps.",
  "Instructions7": "[Set Mob Spawn Rate] requires all online players to be at the center of the Monster Arena and must have defeated the Wall of Flesh for it to take effect.",
  "Plugin Switch": true,
  "Reset Auto Infrastructure": false,
  "Reset Clean Buildings": true,
  "Reset Backup Buildings": true,
  "Restore Furniture Items When Copying Buildings": true,
  "Chain Replacement Tile Limit": 500,
  "Server Startup Command List": [],
  "Block Placement Prohibition List": [],
  "Self-built Glowing Lake": [
    {
      "Build Glowing Lake": false,
      "Spawn Point Offset X": 0,
      "Spawn Point Offset Y": 200
    }
  ],
  "Prison Cluster": [
    {
      "Build Prison": false,
      "Spawn Point Offset X": -4,
      "Spawn Point Offset Y": 10,
      "Prison Cluster Width": 38,
      "Prison Cluster Height": 80,
      "Tile ID": 38,
      "Wall ID": 147,
      "Platform Style": 43,
      "Chair Style": 27,
      "Workbench Style": 18,
      "Torch Style": 13
    }
  ],
  "Chest Cluster": [
    {
      "Build Chests": false,
      "Enable Walls and Item Frames": false,
      "Spawn Point Offset X": -38,
      "Spawn Point Offset Y": 34,
      "Number of Chests": 18,
      "Number of Chest Layers": 8,
      "Layer Spacing": 0,
      "Clear Height": 2,
      "Chest Width": 2,
      "Chest ID": 21,
      "Chest Style": -1,
      "Platform ID": 19,
      "Platform Style": 43,
      "Wall ID": 318,
      "Item Frame ID": 395
    }
  ],
  "World/Left Sea Platforms": [
    {
      "Build World Platform": false,
      "Build World Track": false,
      "Build Left Sea Platform": false,
      "World Platform Tile": 19,
      "World Platform Style": 43,
      "World Platform Height": -100,
      "Platform Clear Height": 35,
      "Distance From World Platform to Left Sea": 150,
      "Distance From Left Sea Platform to Spawn Point": 50,
      "Left Sea Platform Length": 270,
      "Left Sea Platform Height": 200,
      "Left Sea Platform Spacing": 30,
      "Surround Left Sea Platform With Solid Blocks (Required for Inverted Worlds)": false,
      "Generate Liquid on Left Sea Platform (Not Needed for Sky Islands)": false,
      "Do Not Cover Dungeon Bricks and Jungle Lizard Bricks With World Platform": false
    }
  ],
  "Express/Monster Arena": [
    {
      "Build Hell Express": false,
      "Build Hell Platform": false,
      "Build Hell Track": false,
      "Build Glowing Lake Express": false,
      "Build Monster Arena": false,
      "Only Clear Monster Arena Area": false,
      "Express Passes Through Monster Arena": true,
      "Tile ID": 38,
      "Rope ID": 214,
      "Platform ID": 19,
      "Platform Style": 43,
      "Express Offset X": 0,
      "Express Offset Y": 0,
      "Hell Express Width": 5,
      "Hell Platform Depth": 25,
      "Glowing Lake Express Width": 2,
      "Monster Arena Clear Depth": 200,
      "Monster Arena Clear Width": 200,
      "Monster Arena Scale Factor": 2,
      "Place Lava": true,
      "Place Spikes": true,
      "Place Dart Traps": true,
      "Set Mob Spawn Rate": false,
      "Mob Spawn Interval/Frames": 60,
      "Mob Cap/Count": 100,
      "Do Not Cover Dungeon Bricks and Jungle Lizard Bricks With Express": false
    }
  ]
}
```
## FeedBack
- Github Issue -> TShockPlugin Repo: https://github.com/UnrealMultiple/TShockPlugin
- TShock QQ Group: 816771079
- China Terraria Forum: trhub.cn, bbstr.net, tr.monika.love