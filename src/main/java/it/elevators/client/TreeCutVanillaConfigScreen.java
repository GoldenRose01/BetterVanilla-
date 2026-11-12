package it.elevators.client;

import it.elevators.BetterVanillaConfig;
import it.elevators.tree.TreeCutMode;
import net.minecraft.client.gui.components.Button;
import net.minecraft.client.gui.components.CycleButton;
import net.minecraft.client.gui.screens.Screen;
import net.minecraft.network.chat.CommonComponents;
import net.minecraft.network.chat.Component;

public final class TreeCutVanillaConfigScreen extends Screen {
    private final Screen parent;
    private TreeCutMode currentMode;

    public TreeCutVanillaConfigScreen(Screen parent) {
        super(Component.literal("BetterVanilla+ Settings"));
        this.parent = parent;
        this.currentMode = BetterVanillaConfig.getTreeCutMode();
    }

    @Override
    protected void init() {
        int centerX = this.width / 2;
        int centerY = this.height / 2;

        java.util.function.Function<TreeCutMode, Component> nameProvider = mode -> Component.literal(mode.getId());
        this.addRenderableWidget(CycleButton.<TreeCutMode>builder(nameProvider, this.currentMode)
                .withValues(TreeCutMode.values())
                .create(centerX - 100, centerY - 20, 200, 20, Component.literal("Tree Cut Mode"),
                        (button, newMode) -> this.currentMode = newMode));

        this.addRenderableWidget(Button.builder(CommonComponents.GUI_DONE, button -> {
            BetterVanillaConfig.setTreeCutMode(this.currentMode);
            onClose();
        }).bounds(centerX - 100, centerY + 20, 200, 20).build());
    }

    @Override
    public void onClose() {
        if (this.minecraft != null) {
            this.minecraft.setScreenAndShow(this.parent);
        }
    }
}
