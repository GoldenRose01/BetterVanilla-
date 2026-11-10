package it.elevators.mixin;

import it.elevators.ElevatorsMod;
import net.minecraft.core.BlockPos;
import net.minecraft.server.level.ServerLevel;
import net.minecraft.world.entity.MobCategory;
import net.minecraft.world.level.NaturalSpawner;
import net.minecraft.world.level.StructureManager;
import net.minecraft.world.level.biome.MobSpawnSettings;
import net.minecraft.world.level.chunk.ChunkGenerator;
import org.spongepowered.asm.mixin.Mixin;
import org.spongepowered.asm.mixin.injection.At;
import org.spongepowered.asm.mixin.injection.Inject;
import org.spongepowered.asm.mixin.injection.callback.CallbackInfoReturnable;

@Mixin(NaturalSpawner.class)
abstract class NaturalSpawnerMixin {
    @Inject(method = "isValidSpawnPostitionForType", at = @At("HEAD"), cancellable = true)
    private static void elevators$preventSpawningNearElevators(
            ServerLevel level,
            MobCategory category,
            StructureManager structures,
            ChunkGenerator generator,
            MobSpawnSettings.SpawnerData spawnerData,
            BlockPos.MutableBlockPos spawnPos,
            double distance,
            CallbackInfoReturnable<Boolean> cir
    ) {
        int radius = it.elevators.BetterVanillaConfig.number("spawnProtectionRadius", 0, 8);
        if (radius == 0) return;
        for (int dx = -radius; dx <= radius; dx++) {
            for (int dy = -radius; dy <= radius; dy++) {
                for (int dz = -radius; dz <= radius; dz++) {
                    if (dx * dx + dy * dy + dz * dz > radius * radius) continue;
                    BlockPos nearby = new BlockPos(
                            spawnPos.getX() + dx,
                            spawnPos.getY() + dy,
                            spawnPos.getZ() + dz
                    );
                    if (ElevatorsMod.isElevatorAt(level, nearby)) {
                        cir.setReturnValue(false);
                        return;
                    }
                }
            }
        }
    }
}
