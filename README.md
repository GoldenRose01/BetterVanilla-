# Elevators Fabric 26.3

A Fabric mod for Minecraft Java 26.3. It adds elevator blocks based on the existing vanilla wool colors and the selected chiseled blocks. Each elevator uses the matching vanilla block texture and can connect vertically to any elevator variant.

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

## Recipes

Each variant has its own shapeless recipe: one matching vanilla base block, one Ender Pearl, and one Redstone produce one matching elevator. For example, White Wool + Ender Pearl + Redstone produces one White Wool Elevator.

## Build and run

Requires JDK 25. On Windows, run `gradlew.bat build` to build the project or `gradlew.bat runClient` to start a development client. The distributable mod jar is written to `build/libs/`.

The block models reference vanilla Minecraft textures; no copied game texture files are bundled.
