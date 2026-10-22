# Elevators Fabric 26.3

Mod Fabric per Minecraft Java 26.3. Adds one craftable elevator block and links it to other elevator blocks directly above or below it on the same X/Z coordinates.

## Use

- Right-click an elevator block to travel to the nearest elevator above.
- Sneak and right-click to travel to the nearest elevator below.
- The destination must have two clear blocks above it. Teleports are limited to 128 blocks per activation.
- A successful teleport plays both the Enderman and Shulker teleport sounds.
- No item frame, Shulker block, or other marker block is used.

Elevators use the vanilla quartz block texture so the project remains self-contained and avoids bundling copied game assets. Craft 4 elevators with 7 quartz blocks, 1 ender pearl, and 1 redstone using the included recipe.

## Build

Requires JDK 25. On Windows, run `gradlew.bat build`; the distributable mod jar is written to `build/libs/`.

## Notes

The referenced prior project archive and texture attachment were not available in the recovered conversation. This reconstruction uses the stated requirements and a vanilla quartz texture as a practical default. Replace the model texture reference in `src/main/resources/assets/elevators/models/block/elevator.json` if the original texture becomes available.
