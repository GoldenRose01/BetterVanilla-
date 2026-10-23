package it.elevators;

import net.fabricmc.api.ClientModInitializer;
import net.fabricmc.fabric.api.client.event.lifecycle.v1.ClientTickEvents;
import net.minecraft.client.Minecraft;
import net.minecraft.client.multiplayer.ClientLevel;
import net.minecraft.core.BlockPos;
import net.minecraft.core.particles.BlockParticleOption;
import net.minecraft.core.particles.ParticleTypes;
import net.minecraft.world.level.block.state.BlockState;

import java.util.concurrent.ThreadLocalRandom;

public final class ElevatorsClient implements ClientModInitializer {
    private static final int ACTIVE_RADIUS = 15;
    private static final int SCAN_INTERVAL_TICKS = 10;
    private static final int PARTICLES_PER_ELEVATOR = 4;
    private static final int RADIUS_SQUARED = ACTIVE_RADIUS * ACTIVE_RADIUS;

    @Override
    public void onInitializeClient() {
        ClientTickEvents.END_CLIENT_TICK.register(ElevatorsClient::updateActiveElevatorEffects);
    }

    private static void updateActiveElevatorEffects(Minecraft client) {
        ClientLevel level = client.level;
        if (level == null || client.player == null || level.getGameTime() % SCAN_INTERVAL_TICKS != 0) return;

        BlockPos center = client.player.blockPosition();
        BlockPos.MutableBlockPos cursor = new BlockPos.MutableBlockPos();
        ThreadLocalRandom random = ThreadLocalRandom.current();

        for (int dx = -ACTIVE_RADIUS; dx <= ACTIVE_RADIUS; dx++) {
            for (int dy = -ACTIVE_RADIUS; dy <= ACTIVE_RADIUS; dy++) {
                for (int dz = -ACTIVE_RADIUS; dz <= ACTIVE_RADIUS; dz++) {
                    if (dx * dx + dy * dy + dz * dz > RADIUS_SQUARED) continue;

                    cursor.set(center.getX() + dx, center.getY() + dy, center.getZ() + dz);
                    BlockState state = level.getBlockState(cursor);
                    if (!ElevatorsMod.isElevatorBlock(state.getBlock())) continue;

                    BlockParticleOption particles = new BlockParticleOption(ParticleTypes.BLOCK, state);
                    double x = cursor.getX() + 0.5;
                    double y = cursor.getY() + 1.0;
                    double z = cursor.getZ() + 0.5;
                    for (int i = 0; i < PARTICLES_PER_ELEVATOR; i++) {
                        level.addParticle(particles,
                                x + random.nextDouble(-0.35, 0.35),
                                y + random.nextDouble(0.0, 0.25),
                                z + random.nextDouble(-0.35, 0.35),
                                random.nextDouble(-0.008, 0.008),
                                random.nextDouble(0.012, 0.035),
                                random.nextDouble(-0.008, 0.008));
                    }
                }
            }
        }
    }
}
