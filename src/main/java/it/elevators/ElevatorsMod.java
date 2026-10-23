package it.elevators;

import net.fabricmc.api.ModInitializer;
import net.minecraft.core.BlockPos;
import net.minecraft.core.Registry;
import net.minecraft.core.registries.BuiltInRegistries;
import net.minecraft.core.registries.Registries;
import net.minecraft.resources.Identifier;
import net.minecraft.resources.ResourceKey;
import net.minecraft.server.level.ServerPlayer;
import net.minecraft.network.chat.Component;
import net.minecraft.sounds.SoundEvents;
import net.minecraft.sounds.SoundSource;
import net.minecraft.world.InteractionResult;
import net.minecraft.world.InteractionHand;
import net.minecraft.world.entity.player.Player;
import net.minecraft.world.item.BlockItem;
import net.minecraft.world.item.CreativeModeTab;
import net.minecraft.world.item.Item;
import net.minecraft.world.item.ItemStack;
import net.minecraft.world.level.Level;
import net.minecraft.world.level.block.Block;
import net.minecraft.world.level.block.Blocks;
import net.minecraft.world.level.block.state.BlockState;
import net.minecraft.world.level.block.state.BlockBehaviour;
import net.minecraft.world.phys.BlockHitResult;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.Set;
import java.util.WeakHashMap;

public final class ElevatorsMod implements ModInitializer {
    public static final String MOD_ID = "elevators";
    public static final Logger LOGGER = LoggerFactory.getLogger(MOD_ID);
    private static final List<Variant> VARIANTS = List.of(
            new Variant("white_wool", Blocks.WOOL.white()),
            new Variant("orange_wool", Blocks.WOOL.orange()),
            new Variant("magenta_wool", Blocks.WOOL.magenta()),
            new Variant("light_blue_wool", Blocks.WOOL.lightBlue()),
            new Variant("yellow_wool", Blocks.WOOL.yellow()),
            new Variant("lime_wool", Blocks.WOOL.lime()),
            new Variant("pink_wool", Blocks.WOOL.pink()),
            new Variant("gray_wool", Blocks.WOOL.gray()),
            new Variant("light_gray_wool", Blocks.WOOL.lightGray()),
            new Variant("cyan_wool", Blocks.WOOL.cyan()),
            new Variant("purple_wool", Blocks.WOOL.purple()),
            new Variant("blue_wool", Blocks.WOOL.blue()),
            new Variant("brown_wool", Blocks.WOOL.brown()),
            new Variant("green_wool", Blocks.WOOL.green()),
            new Variant("red_wool", Blocks.WOOL.red()),
            new Variant("black_wool", Blocks.WOOL.black()),
            new Variant("copper", Blocks.COPPER_BLOCK.weathering().unaffected()),
            new Variant("chiseled_sandstone", Blocks.CHISELED_SANDSTONE),
            new Variant("chiseled_stone_bricks", Blocks.CHISELED_STONE_BRICKS),
            new Variant("chiseled_quartz_block", Blocks.CHISELED_QUARTZ_BLOCK),
            new Variant("chiseled_red_sandstone", Blocks.CHISELED_RED_SANDSTONE),
            new Variant("chiseled_polished_blackstone", Blocks.CHISELED_POLISHED_BLACKSTONE),
            new Variant("chiseled_nether_bricks", Blocks.CHISELED_NETHER_BRICKS),
            new Variant("chiseled_tuff", Blocks.CHISELED_TUFF),
            new Variant("chiseled_tuff_bricks", Blocks.CHISELED_TUFF_BRICKS),
            new Variant("chiseled_resin_bricks", Blocks.CHISELED_RESIN_BRICKS),
            new Variant("chiseled_deepslate", Blocks.CHISELED_DEEPSLATE),
            new Variant("chiseled_sulfur", Blocks.CHISELED_SULFUR),
            new Variant("chiseled_cinnabar", Blocks.CHISELED_CINNABAR)
    );
    private static final List<Block> ELEVATOR_BLOCKS = registerBlocks();
    private static final Set<Block> ELEVATOR_BLOCK_SET = Set.copyOf(ELEVATOR_BLOCKS);
    private static final CreativeModeTab ELEVATORS_TAB = registerCreativeTab();
    private static final int MAX_SEARCH_DISTANCE = 128;
    private static final Map<Player, Long> LAST_USE = new WeakHashMap<>();

    private static List<Block> registerBlocks() {
        List<Block> blocks = new ArrayList<>(VARIANTS.size());
        for (Variant variant : VARIANTS) {
            blocks.add(registerBlock(variant));
        }
        return List.copyOf(blocks);
    }

    private static CreativeModeTab registerCreativeTab() {
        Identifier id = Identifier.fromNamespaceAndPath(MOD_ID, "elevators");
        ResourceKey<CreativeModeTab> tabKey = ResourceKey.create(Registries.CREATIVE_MODE_TAB, id);
        CreativeModeTab tab = CreativeModeTab.builder(CreativeModeTab.Row.TOP, 7)
                .title(Component.translatable("itemGroup.elevators"))
                .icon(() -> new ItemStack(BuiltInRegistries.ITEM.getValue(
                        Identifier.fromNamespaceAndPath(MOD_ID, "copper_elevator"))))
                .displayItems((parameters, output) -> ELEVATOR_BLOCKS.forEach(block -> output.accept(block.asItem())))
                .build();
        return Registry.register(BuiltInRegistries.CREATIVE_MODE_TAB, tabKey, tab);
    }

    private static Block registerBlock(Variant variant) {
        String name = variant.name() + "_elevator";
        Identifier id = Identifier.fromNamespaceAndPath(MOD_ID, name);
        ResourceKey<Block> blockKey = ResourceKey.create(Registries.BLOCK, id);
        Block block = new ElevatorBlock(BlockBehaviour.Properties.ofFullCopy(variant.baseBlock())
                .setId(blockKey));
        Registry.register(BuiltInRegistries.BLOCK, blockKey, block);

        ResourceKey<Item> itemKey = ResourceKey.create(Registries.ITEM, id);
        Item item = new BlockItem(block, new Item.Properties().setId(itemKey));
        Registry.register(BuiltInRegistries.ITEM, itemKey, item);
        return block;
    }

    @Override
    public void onInitialize() {
        LOGGER.info("Elevators Fabric initialized with {} elevator variants", ELEVATOR_BLOCKS.size());
    }

    private static InteractionResult useElevator(Player player, Level level, BlockPos source) {
        if (level.isClientSide()) return InteractionResult.SUCCESS;
        if (!(player instanceof ServerPlayer serverPlayer)) return InteractionResult.PASS;

        long now = level.getGameTime();
        Long previous = LAST_USE.get(serverPlayer);
        if (previous != null && now - previous < 8) return InteractionResult.CONSUME;
        LAST_USE.put(serverPlayer, now);

        int direction = player.isShiftKeyDown() ? -1 : 1;
        BlockPos destination = findDestination(level, source, direction);
        if (destination == null) {
            serverPlayer.sendOverlayMessage(Component.translatable("message.elevators.no_destination"));
            return InteractionResult.CONSUME;
        }

        double x = destination.getX() + 0.5;
        double y = destination.getY() + 1.0;
        double z = destination.getZ() + 0.5;
        if (!level.getBlockState(destination.above()).getCollisionShape(level, destination.above()).isEmpty()
                || !level.getBlockState(destination.above(2)).getCollisionShape(level, destination.above(2)).isEmpty()) {
            serverPlayer.sendOverlayMessage(Component.translatable("message.elevators.blocked_destination"));
            return InteractionResult.CONSUME;
        }

        serverPlayer.teleportTo(x, y, z);
        level.playSound(null, destination, SoundEvents.ENDERMAN_TELEPORT,
                SoundSource.PLAYERS, 0.8F, 1.0F);
        level.playSound(null, destination, SoundEvents.SHULKER_TELEPORT,
                SoundSource.PLAYERS, 0.65F, 1.0F);
        return InteractionResult.CONSUME;
    }

    private static final class ElevatorBlock extends Block {
        private ElevatorBlock(BlockBehaviour.Properties properties) {
            super(properties);
        }

        @Override
        protected InteractionResult useWithoutItem(BlockState state, Level level, BlockPos pos,
                                                     Player player, BlockHitResult hit) {
            return useElevator(player, level, pos);
        }

        @Override
        protected InteractionResult useItemOn(ItemStack itemStack, BlockState state, Level level,
                                               BlockPos pos, Player player, InteractionHand hand,
                                               BlockHitResult hit) {
            return useElevator(player, level, pos);
        }
    }

    private static BlockPos findDestination(Level level, BlockPos source, int direction) {
        int minY = level.getMinY();
        int maxY = level.getMaxY() - 2;
        for (int distance = 1; distance <= MAX_SEARCH_DISTANCE; distance++) {
            int y = source.getY() + direction * distance;
            if (y < minY || y > maxY) break;
            BlockPos candidate = new BlockPos(source.getX(), y, source.getZ());
            if (isElevator(level.getBlockState(candidate).getBlock())) return candidate;
        }
        return null;
    }

    private static boolean isElevator(Block block) {
        return ELEVATOR_BLOCK_SET.contains(block);
    }

    private record Variant(String name, Block baseBlock) {}
}
