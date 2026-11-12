package it.elevators.client;

import it.elevators.BetterVanillaConfig;
import it.elevators.tree.TreeCutMode;
import me.shedaniel.clothconfig2.api.ConfigBuilder;
import me.shedaniel.clothconfig2.api.ConfigCategory;
import me.shedaniel.clothconfig2.api.ConfigEntryBuilder;
import net.minecraft.client.gui.screens.Screen;
import net.minecraft.network.chat.Component;

public final class TreeCutClothConfigScreen {
    private TreeCutClothConfigScreen() {}

    public static Screen create(Screen parent) {
        ConfigBuilder builder = ConfigBuilder.create()
                .setParentScreen(parent)
                .setTitle(Component.literal("BetterVanilla+ Settings"));

        ConfigCategory general = builder.getOrCreateCategory(Component.literal("General"));
        ConfigEntryBuilder entries = builder.entryBuilder();

        general.addEntry(entries.startEnumSelector(
                Component.literal("Tree Cut Mode"),
                TreeCutMode.class,
                BetterVanillaConfig.getTreeCutMode()
        )
                .setDefaultValue(TreeCutMode.FELL)
                .setTooltip(Component.literal("off: Vanilla break\nfell: Single log cut with tree falling log by log\nchop: Break the whole tree"))
                .setSaveConsumer(BetterVanillaConfig::setTreeCutMode)
                .build());

        builder.setSavingRunnable(BetterVanillaConfig::save);
        return builder.build();
    }
}
