# BetterVanilla+ Fabric 26.3

A Fabric mod for Minecraft Java 26.3, organized around separate vanilla-style features.

📚 **[Wiki del progetto](docs/wiki/Home.md)** — installazione, ascensori, ricette, calderoni, beacon e comandi.

## Elevators

Adds elevator blocks based on the existing vanilla wool colors and selected chiseled blocks. Each elevator can connect vertically to any elevator variant. Elevator textures and cube models use the mod's own `elevators` namespace, so resource packs that change vanilla blocks do not alter elevator appearance. Resource packs can still override elevator assets explicitly.

The **Elevators** creative tab contains all elevator variants and uses the Copper Elevator as its icon.

## Use

- Right-click an elevator to travel to the nearest elevator above it.
- Sneak and right-click to travel to the nearest elevator below it.
- Elevators must share the same X/Z coordinates and be within 128 blocks.
- The destination needs two clear blocks above it.
- A successful teleport plays both the Enderman and Shulker teleport sounds.
- Elevators emit matching block particles while a player is within 15 blocks.
- No item frame or marker block is required.

## Beacon bonus

Drop a Golden Apple directly on top of a fully built level 4 beacon with Regeneration selected as either its primary or secondary power. The beacon consumes one apple and grants Saturation V to players within 50 blocks.

## Cauldron interactions

Drop dirt, coarse dirt, or rooted dirt into a water-filled cauldron to convert it to mud. Drop any concrete powder color into a water-filled cauldron to convert it to the matching concrete block. The entire dropped stack converts at once, matching the supplied Vanilla Tweaks datapacks.

## Commands

These commands require operator permissions.

- `/gameset player keepinventory on <player>` enables personal keep-inventory for that player.
- `/gameset player keepinventory off <player>` disables it.
- `/gameset villagertrade` opens an editor for the nearest villager within 16 blocks.

Personal keep-inventory is saved with the player and retains their inventory and experience on death, independently of the world's `keepInventory` gamerule. The villager editor can create, edit, and delete offers; select input and result item types from your inventory cursor, adjust quantities and maximum uses, then save. Selecting an item does not consume it.

## Recipes

Each variant has its own shapeless recipe: one matching vanilla base block, one Ender Pearl, and one Redstone produce one matching elevator. For example, White Wool + Ender Pearl + Redstone produces one White Wool Elevator.

## Build and run

Requires JDK 25. On Windows, run `gradlew.bat build` to build the project or `gradlew.bat runClient` to start a development client. The distributable mod jar is written to `build/libs/`.

The project bundles the elevator texture assets in `src/main/resources/assets/elevators/textures/block/` so they stay independent of vanilla texture overrides.
