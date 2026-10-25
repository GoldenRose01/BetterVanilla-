package it.elevators;

import it.elevators.mixin.BeaconBlockEntityAccessor;
import net.minecraft.core.BlockPos;
import net.minecraft.server.level.ServerLevel;
import net.minecraft.server.level.ServerPlayer;
import net.minecraft.world.effect.MobEffectInstance;
import net.minecraft.world.effect.MobEffects;
import net.minecraft.world.entity.item.ItemEntity;
import net.minecraft.world.item.ItemStack;
import net.minecraft.world.item.Items;
import net.minecraft.world.level.block.Blocks;
import net.minecraft.world.level.block.entity.BeaconBlockEntity;

import java.util.Map;
import java.util.WeakHashMap;

public final class BeaconAppleHandler {
    private static final int MAX_BEACON_LEVEL = 4;
    private static final int APPLE_COOLDOWN_TICKS = 20;
    private static final int SATURATION_AMPLIFIER = 4;
    private static final int BEACON_RANGE = 50;
    private static final Map<ItemEntity, Long> NEXT_USE_TICK = new WeakHashMap<>();

    private BeaconAppleHandler() {}

    public static void tryActivate(ItemEntity item) {
        if (!(item.level() instanceof ServerLevel level)) return;

        ItemStack stack = item.getItem();
        if (!stack.is(Items.GOLDEN_APPLE)) return;

        long gameTime = level.getGameTime();
        Long nextUse = NEXT_USE_TICK.get(item);
        if (nextUse != null && gameTime < nextUse) return;

        BlockPos itemPos = item.blockPosition();
        BlockPos beaconPos = itemPos.below();
        if (item.getX() < beaconPos.getX() || item.getX() >= beaconPos.getX() + 1.0
                || item.getZ() < beaconPos.getZ() || item.getZ() >= beaconPos.getZ() + 1.0
                || !level.getBlockState(beaconPos).is(Blocks.BEACON)) {
            return;
        }

        if (!(level.getBlockEntity(beaconPos) instanceof BeaconBlockEntity beacon)) {
            return;
        }

        BeaconBlockEntityAccessor access = (BeaconBlockEntityAccessor) beacon;
        if (access.elevators$getLevels() < MAX_BEACON_LEVEL) return;
        if (!isRegeneration(access.elevators$getPrimaryPower())
                && !isRegeneration(access.elevators$getSecondaryPower())) {
            return;
        }

        NEXT_USE_TICK.put(item, gameTime + APPLE_COOLDOWN_TICKS);
        stack.shrink(1);
        if (stack.isEmpty()) {
            item.discard();
        } else {
            item.setItem(stack);
        }

        double centerX = beaconPos.getX() + 0.5;
        double centerY = beaconPos.getY() + 0.5;
        double centerZ = beaconPos.getZ() + 0.5;
        double rangeSquared = (double) BEACON_RANGE * BEACON_RANGE;
        for (ServerPlayer player : level.players()) {
            if (player.distanceToSqr(centerX, centerY, centerZ) <= rangeSquared) {
                player.addEffect(new MobEffectInstance(MobEffects.SATURATION, 1, SATURATION_AMPLIFIER));
            }
        }
    }

    private static boolean isRegeneration(net.minecraft.core.Holder<net.minecraft.world.effect.MobEffect> effect) {
        return effect != null && effect.is(MobEffects.REGENERATION);
    }
}
