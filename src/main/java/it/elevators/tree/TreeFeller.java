package it.elevators.tree;

import it.elevators.BetterVanillaConfig;
import net.fabricmc.fabric.api.event.player.PlayerBlockBreakEvents;
import net.fabricmc.loader.api.FabricLoader;
import net.minecraft.core.BlockPos;
import net.minecraft.core.registries.BuiltInRegistries;
import net.minecraft.core.registries.Registries;
import net.minecraft.resources.Identifier;
import net.minecraft.resources.ResourceKey;
import net.minecraft.server.level.ServerLevel;
import net.minecraft.server.level.ServerPlayer;
import net.minecraft.sounds.SoundEvents;
import net.minecraft.sounds.SoundSource;
import net.minecraft.tags.BlockTags;
import net.minecraft.tags.ItemTags;
import net.minecraft.world.entity.EquipmentSlot;
import net.minecraft.world.entity.player.Player;
import net.minecraft.world.item.ItemStack;
import net.minecraft.world.item.Items;
import net.minecraft.world.item.enchantment.Enchantment;
import net.minecraft.world.item.enchantment.EnchantmentHelper;
import net.minecraft.world.level.Level;
import net.minecraft.world.level.block.Block;
import net.minecraft.world.level.block.Blocks;
import net.minecraft.world.level.block.entity.BlockEntity;
import net.minecraft.world.level.block.state.BlockState;

import java.util.*;

public final class TreeFeller {
    private static final ThreadLocal<Boolean> IS_PROCESSING = ThreadLocal.withInitial(() -> false);

    private TreeFeller() {}

    public static void register() {
        PlayerBlockBreakEvents.BEFORE.register(TreeFeller::onBlockBreak);
    }

    public static boolean onBlockBreak(Level level, Player player, BlockPos pos, BlockState state, BlockEntity blockEntity) {
        if (IS_PROCESSING.get()) {
            return true;
        }
        if (level.isClientSide() || !(level instanceof ServerLevel serverLevel) || !(player instanceof ServerPlayer serverPlayer)) {
            return true;
        }
        if (!state.is(BlockTags.LOGS)) {
            return true;
        }
        // Treecut logic is active always when sneaking
        if (!serverPlayer.isShiftKeyDown()) {
            return true;
        }

        ItemStack tool = serverPlayer.getMainHandItem();
        boolean isCreative = serverPlayer.isCreative();

        TreeCutMode activeMode;
        if (isCreative) {
            // In creative if shift, whole tree is always chopped
            activeMode = TreeCutMode.CHOP;
        } else {
            // Valid only for axes that are above stone level (all axes except wooden)
            if (!isValidAxe(tool)) {
                return true;
            }

            if (FabricLoader.getInstance().isModLoaded("eldrich-surge")) {
                // With Eldrich mod installed, tree fells without enchantment; chops with Lumberjack
                boolean hasLumberjack = hasLumberjackEnchantment(serverLevel, tool);
                activeMode = hasLumberjack ? TreeCutMode.CHOP : TreeCutMode.FELL;
            } else {
                // If only enchlib or standalone, governed by config / /gameset treecut
                activeMode = BetterVanillaConfig.getTreeCutMode();
                if (activeMode == TreeCutMode.OFF) {
                    return true;
                }
            }
        }

        TreeScanResult tree = scanTree(serverLevel, pos, state);
        if (tree == null || tree.logs.isEmpty() || tree.leaves.isEmpty()) {
            // Must have attached leaves to qualify as a natural tree (protects player builds)
            return true;
        }

        IS_PROCESSING.set(true);
        try {
            if (activeMode == TreeCutMode.FELL) {
                return executeFell(serverLevel, serverPlayer, tool, pos, state, blockEntity, tree);
            } else {
                return executeChop(serverLevel, serverPlayer, tool, pos, state, blockEntity, tree);
            }
        } finally {
            IS_PROCESSING.set(false);
        }
    }

    public static boolean isValidAxe(ItemStack tool) {
        if (tool == null || tool.isEmpty()) return false;
        if (!tool.is(ItemTags.AXES)) return false;
        if (tool.is(Items.WOODEN_AXE)) return false;
        String path = BuiltInRegistries.ITEM.getKey(tool.getItem()).getPath();
        return !path.contains("wooden_axe") && !path.startsWith("wood_axe");
    }

    public static boolean hasLumberjackEnchantment(ServerLevel level, ItemStack tool) {
        if (tool == null || tool.isEmpty()) return false;
        try {
            var registry = level.registryAccess().lookup(Registries.ENCHANTMENT);
            if (registry.isEmpty()) return false;
            var key = ResourceKey.create(Registries.ENCHANTMENT, Identifier.fromNamespaceAndPath("eldrich-surge", "lumberjack"));
            var holder = registry.get().get(key);
            return holder.map(h -> EnchantmentHelper.getItemEnchantmentLevel(h, tool) > 0).orElse(false);
        } catch (Exception e) {
            return false;
        }
    }

    private static boolean executeFell(ServerLevel level, ServerPlayer player, ItemStack tool,
                                      BlockPos pos, BlockState state, BlockEntity blockEntity,
                                      TreeScanResult tree) {
        // Mine the broken log at pos
        Block.dropResources(state, level, pos, blockEntity, player, tool);
        level.removeBlock(pos, false);
        level.levelEvent(2001, pos, Block.getId(state));
        if (!player.isCreative()) {
            tool.hurtAndBreak(1, player, EquipmentSlot.MAINHAND);
            player.awardStat(net.minecraft.stats.Stats.BLOCK_MINED.get(state.getBlock()));
        }

        // Find all tree blocks above pos.getY()
        int cutoffY = pos.getY();
        Map<Long, List<BlockPos>> columns = new LinkedHashMap<>();

        for (BlockPos logPos : tree.logs) {
            if (logPos.getY() > cutoffY) {
                long colKey = columnKey(logPos.getX(), logPos.getZ());
                columns.computeIfAbsent(colKey, k -> new ArrayList<>()).add(logPos.immutable());
            }
        }
        for (BlockPos leafPos : tree.leaves) {
            if (leafPos.getY() > cutoffY) {
                long colKey = columnKey(leafPos.getX(), leafPos.getZ());
                columns.computeIfAbsent(colKey, k -> new ArrayList<>()).add(leafPos.immutable());
            }
        }

        if (columns.isEmpty()) {
            return false;
        }

        // Shift each column down by 1 in Y
        for (List<BlockPos> column : columns.values()) {
            column.sort(Comparator.comparingInt(BlockPos::getY));
            int size = column.size();
            for (int i = 0; i < size; i++) {
                BlockPos from = column.get(i);
                BlockPos to = from.below();
                BlockState movingState = level.getBlockState(from);
                level.setBlock(to, movingState, Block.UPDATE_ALL);
            }
            BlockPos highest = column.get(size - 1);
            level.setBlock(highest, Blocks.AIR.defaultBlockState(), Block.UPDATE_ALL);
        }

        level.playSound(null, pos, SoundEvents.WOOD_PLACE, SoundSource.BLOCKS, 1.0f, 0.85f);
        return false;
    }

    private static boolean executeChop(ServerLevel level, ServerPlayer player, ItemStack tool,
                                       BlockPos pos, BlockState state, BlockEntity blockEntity,
                                       TreeScanResult tree) {
        // First log at pos
        Block.dropResources(state, level, pos, blockEntity, player, tool);
        level.removeBlock(pos, false);
        level.levelEvent(2001, pos, Block.getId(state));
        if (!player.isCreative()) {
            tool.hurtAndBreak(1, player, EquipmentSlot.MAINHAND);
            player.awardStat(net.minecraft.stats.Stats.BLOCK_MINED.get(state.getBlock()));
        }

        // Break all other logs
        for (BlockPos logPos : tree.logs) {
            if (logPos.equals(pos)) continue;
            if (!player.isCreative()) {
                if (tool.getDamageValue() >= tool.getMaxDamage() - 1) {
                    break;
                }
                tool.hurtAndBreak(1, player, EquipmentSlot.MAINHAND);
            }
            BlockState logState = level.getBlockState(logPos);
            Block.dropResources(logState, level, logPos, level.getBlockEntity(logPos), player, tool);
            level.removeBlock(logPos, false);
            level.levelEvent(2001, logPos, Block.getId(logState));
            player.awardStat(net.minecraft.stats.Stats.BLOCK_MINED.get(logState.getBlock()));
        }

        // Break connected leaves
        for (BlockPos leafPos : tree.leaves) {
            BlockState leafState = level.getBlockState(leafPos);
            Block.dropResources(leafState, level, leafPos, level.getBlockEntity(leafPos), player, tool);
            level.removeBlock(leafPos, false);
            level.levelEvent(2001, leafPos, Block.getId(leafState));
        }

        level.playSound(null, pos, SoundEvents.WOOD_BREAK, SoundSource.BLOCKS, 1.0f, 0.9f);
        return false;
    }

    private static long columnKey(int x, int z) {
        return (((long) x) << 32) | (z & 0xFFFFFFFFL);
    }

    private static record TreeScanResult(Set<BlockPos> logs, Set<BlockPos> leaves) {}

    private static TreeScanResult scanTree(ServerLevel level, BlockPos startPos, BlockState startState) {
        int maxLogs = BetterVanillaConfig.number("treeMaxLogs", 10, 1024);
        int maxLeaves = BetterVanillaConfig.number("treeMaxLeaves", 10, 2048);

        Set<BlockPos> logs = new LinkedHashSet<>();
        Set<BlockPos> leaves = new LinkedHashSet<>();
        Queue<BlockPos> logQueue = new ArrayDeque<>();

        logQueue.add(startPos);
        logs.add(startPos);

        while (!logQueue.isEmpty() && logs.size() < maxLogs) {
            BlockPos current = logQueue.poll();

            for (int dx = -1; dx <= 1; dx++) {
                for (int dy = -1; dy <= 1; dy++) {
                    for (int dz = -1; dz <= 1; dz++) {
                        if (dx == 0 && dy == 0 && dz == 0) continue;
                        BlockPos neighbor = current.offset(dx, dy, dz);
                        if (neighbor.getY() < startPos.getY() - 1) continue;

                        if (!logs.contains(neighbor)) {
                            BlockState neighborState = level.getBlockState(neighbor);
                            if (neighborState.is(BlockTags.LOGS)) {
                                logs.add(neighbor);
                                logQueue.add(neighbor);
                                if (logs.size() >= maxLogs) break;
                            }
                        }
                    }
                    if (logs.size() >= maxLogs) break;
                }
                if (logs.size() >= maxLogs) break;
            }
        }

        for (BlockPos logPos : logs) {
            for (int dx = -3; dx <= 3; dx++) {
                for (int dy = -2; dy <= 3; dy++) {
                    for (int dz = -3; dz <= 3; dz++) {
                        if (dx * dx + dy * dy + dz * dz > 18) continue;
                        BlockPos candidate = logPos.offset(dx, dy, dz);
                        if (!leaves.contains(candidate)) {
                            BlockState candidateState = level.getBlockState(candidate);
                            if (candidateState.is(BlockTags.LEAVES)) {
                                leaves.add(candidate);
                                if (leaves.size() >= maxLeaves) break;
                            }
                        }
                    }
                    if (leaves.size() >= maxLeaves) break;
                }
                if (leaves.size() >= maxLeaves) break;
            }
        }

        return new TreeScanResult(logs, leaves);
    }
}
