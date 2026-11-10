package it.elevators;

import net.minecraft.core.BlockPos;
import net.minecraft.world.entity.item.ItemEntity;
import net.minecraft.world.item.Item;
import net.minecraft.world.item.ItemStack;
import net.minecraft.world.level.block.Blocks;

import java.util.HashMap;
import java.util.Map;

public final class CauldronInteractionHandler {
    private static final Map<Item, Item> CONVERSIONS = createConversions();

    private CauldronInteractionHandler() {}

    private static Map<Item, Item> createConversions() {
        Map<Item, Item> conversions = new HashMap<>();
        conversions.put(Blocks.DIRT.asItem(), Blocks.MUD.asItem());
        conversions.put(Blocks.COARSE_DIRT.asItem(), Blocks.MUD.asItem());
        conversions.put(Blocks.ROOTED_DIRT.asItem(), Blocks.MUD.asItem());
        net.minecraft.world.level.block.ColorCollection.zipApply(
                Blocks.CONCRETE_POWDER,
                Blocks.CONCRETE,
                (powder, concrete) -> conversions.put(powder.asItem(), concrete.asItem())
        );
        return Map.copyOf(conversions);
    }

    public static void tryConvert(ItemEntity itemEntity) {
        if (itemEntity.level().isClientSide() || !BetterVanillaConfig.enabled("cauldronEnabled")) return;

        ItemStack stack = itemEntity.getItem();
        Item convertedItem = CONVERSIONS.get(stack.getItem());
        if (convertedItem == null) return;

        BlockPos itemPos = itemEntity.blockPosition();
        if (!itemEntity.level().getBlockState(itemPos).is(Blocks.WATER_CAULDRON)) return;

        itemEntity.setItem(new ItemStack(convertedItem, stack.getCount()));
    }
}
