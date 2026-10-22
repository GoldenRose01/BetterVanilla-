package it.elevators;

import net.fabricmc.api.ModInitializer;
import net.fabricmc.fabric.api.event.player.UseBlockCallback;
import net.minecraft.core.BlockPos;
import net.minecraft.core.Registry;
import net.minecraft.core.registries.BuiltInRegistries;
import net.minecraft.core.registries.Registries;
import net.minecraft.resources.Identifier;
import net.minecraft.resources.ResourceKey;
import net.minecraft.server.level.ServerPlayer;
import net.minecraft.sounds.SoundEvents;
import net.minecraft.sounds.SoundSource;
import net.minecraft.world.InteractionResult;
import net.minecraft.world.entity.player.Player;
import net.minecraft.world.item.BlockItem;
import net.minecraft.world.item.Item;
import net.minecraft.world.level.Level;
import net.minecraft.world.level.block.Block;
import net.minecraft.world.level.block.Blocks;
import net.minecraft.world.level.block.state.BlockBehaviour;
import net.minecraft.world.phys.BlockHitResult;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.util.Map;
import java.util.WeakHashMap;

public final class ElevatorsMod implements ModInitializer {
    public static final String MOD_ID = "elevators";
    public static final Logger LOGGER = LoggerFactory.getLogger(MOD_ID);
    public static final Block ELEVATOR = registerBlock("elevator");
    private static final int MAX_SEARCH_DISTANCE = 128;
    private static final Map<Player, Long> LAST_USE = new WeakHashMap<>();

    private static Block registerBlock(String name) {
        Identifier id = Identifier.fromNamespaceAndPath(MOD_ID, name);
        ResourceKey<Block> blockKey = ResourceKey.create(Registries.BLOCK, id);
        Block block = new Block(BlockBehaviour.Properties.ofFullCopy(Blocks.QUARTZ_BLOCK)
                .setId(blockKey));
        Registry.register(BuiltInRegistries.BLOCK, blockKey, block);

        ResourceKey<Item> itemKey = ResourceKey.create(Registries.ITEM, id);
        Item item = new BlockItem(block, new Item.Properties().setId(itemKey));
        Registry.register(BuiltInRegistries.ITEM, itemKey, item);
        return block;
    }

    @Override
    public void onInitialize() {
        UseBlockCallback.EVENT.register(ElevatorsMod::onBlockUse);
        LOGGER.info("Elevators Fabric initialized");
    }

    private static InteractionResult onBlockUse(Player player, Level level,
                                                 net.minecraft.world.InteractionHand hand,
                                                 BlockHitResult hit) {
        if (level.isClientSide() || !(player instanceof ServerPlayer serverPlayer)) {
            return InteractionResult.PASS;
        }
        BlockPos source = hit.getBlockPos();
        if (!level.getBlockState(source).is(ELEVATOR)) return InteractionResult.PASS;

        long now = level.getGameTime();
        Long previous = LAST_USE.get(serverPlayer);
        if (previous != null && now - previous < 8) return InteractionResult.CONSUME;
        LAST_USE.put(serverPlayer, now);

        int direction = player.isShiftKeyDown() ? -1 : 1;
        BlockPos destination = findDestination(level, source, direction);
        if (destination == null) return InteractionResult.CONSUME;

        double x = destination.getX() + 0.5;
        double y = destination.getY() + 1.0;
        double z = destination.getZ() + 0.5;
        if (!level.getBlockState(destination.above()).getCollisionShape(level, destination.above()).isEmpty()
                || !level.getBlockState(destination.above(2)).getCollisionShape(level, destination.above(2)).isEmpty()) {
            return InteractionResult.CONSUME;
        }

        serverPlayer.teleportTo(x, y, z);
        level.playSound(null, destination, SoundEvents.ENDERMAN_TELEPORT,
                SoundSource.PLAYERS, 0.8F, 1.0F);
        level.playSound(null, destination, SoundEvents.SHULKER_TELEPORT,
                SoundSource.PLAYERS, 0.65F, 1.0F);
        return InteractionResult.CONSUME;
    }

    private static BlockPos findDestination(Level level, BlockPos source, int direction) {
        int minY = level.getMinY();
        int maxY = level.getMaxY() - 2;
        for (int distance = 1; distance <= MAX_SEARCH_DISTANCE; distance++) {
            int y = source.getY() + direction * distance;
            if (y < minY || y > maxY) break;
            BlockPos candidate = new BlockPos(source.getX(), y, source.getZ());
            if (level.getBlockState(candidate).is(ELEVATOR)) return candidate;
        }
        return null;
    }

    public static Identifier id(String path) {
        return Identifier.fromNamespaceAndPath(MOD_ID, path);
    }
}
