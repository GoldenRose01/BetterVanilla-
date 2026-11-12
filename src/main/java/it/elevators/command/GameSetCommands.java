package it.elevators.command;

import com.mojang.brigadier.CommandDispatcher;
import it.elevators.BetterVanillaConfig;
import it.elevators.tree.TreeCutMode;
import net.fabricmc.fabric.api.command.v2.CommandRegistrationCallback;
import net.minecraft.commands.CommandSourceStack;
import net.minecraft.commands.Commands;
import net.minecraft.commands.arguments.EntityArgument;
import net.minecraft.network.chat.Component;
import net.minecraft.server.level.ServerPlayer;
import net.minecraft.server.permissions.Permissions;
import net.minecraft.world.entity.npc.villager.Villager;
import net.minecraft.world.phys.AABB;

import java.util.Comparator;

public final class GameSetCommands {
    private GameSetCommands() {}

    public static void register() {
        CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) ->
                register(dispatcher));
    }

    private static void register(CommandDispatcher<CommandSourceStack> dispatcher) {
        dispatcher.register(Commands.literal("gameset")
                .requires(source -> source.permissions().hasPermission(Permissions.COMMANDS_GAMEMASTER))
                .then(Commands.literal("player")
                        .then(Commands.literal("keepinventory")
                                .then(setKeepInventoryBranch("on", true))
                                .then(setKeepInventoryBranch("off", false))))
                .then(Commands.literal("villagertrade")
                        .executes(context -> openVillagerTrade(context.getSource())))
                .then(Commands.literal("treecut")
                        .then(Commands.literal("off")
                                .executes(context -> setTreeCut(context.getSource(), TreeCutMode.OFF)))
                        .then(Commands.literal("fell")
                                .executes(context -> setTreeCut(context.getSource(), TreeCutMode.FELL)))
                        .then(Commands.literal("chop")
                                .executes(context -> setTreeCut(context.getSource(), TreeCutMode.CHOP)))));
    }

    private static int setTreeCut(CommandSourceStack source, TreeCutMode mode) {
        BetterVanillaConfig.setTreeCutMode(mode);
        source.sendSuccess(() -> Component.literal("TreeCut mode impostato su: " + mode.getId() + "."), true);
        return 1;
    }

    private static com.mojang.brigadier.builder.LiteralArgumentBuilder<CommandSourceStack>
    setKeepInventoryBranch(String toggle, boolean enabled) {
        return Commands.literal(toggle)
                .then(Commands.argument("player", EntityArgument.player())
                        .executes(context -> {
                            ServerPlayer target = EntityArgument.getPlayer(context, "player");
                            ((KeepInventoryAccess) target).betterVanilla$setKeepInventoryEnabled(enabled);
                            target.sendSystemMessage(Component.literal("KeepInventory personale "
                                    + (enabled ? "attivato" : "disattivato") + "."));
                            context.getSource().sendSuccess(() -> Component.literal("KeepInventory "
                                    + (enabled ? "attivato" : "disattivato") + " per "
                                    + target.getGameProfile().name() + "."), true);
                            return 1;
                        }));
    }

    private static int openVillagerTrade(CommandSourceStack source) throws com.mojang.brigadier.exceptions.CommandSyntaxException {
        ServerPlayer player = source.getPlayerOrException();
        AABB searchArea = player.getBoundingBox().inflate(16.0, 8.0, 16.0);
        Villager nearest = player.level().getEntitiesOfClass(Villager.class, searchArea).stream()
                .min(Comparator.comparingDouble(player::distanceToSqr))
                .orElse(null);
        if (nearest == null) {
            source.sendFailure(Component.literal("Non trovo villager entro 16 blocchi."));
            return 0;
        }

        VillagerTradeMenu.open(player, nearest);
        return 1;
    }
}
