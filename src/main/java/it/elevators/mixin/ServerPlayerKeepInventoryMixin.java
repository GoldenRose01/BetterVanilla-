package it.elevators.mixin;

import it.elevators.command.KeepInventoryAccess;
import net.minecraft.server.level.ServerPlayer;
import net.minecraft.world.entity.player.Player;
import org.spongepowered.asm.mixin.Mixin;
import org.spongepowered.asm.mixin.gen.Invoker;
import org.spongepowered.asm.mixin.injection.At;
import org.spongepowered.asm.mixin.injection.Inject;
import org.spongepowered.asm.mixin.injection.callback.CallbackInfo;

@Mixin(ServerPlayer.class)
abstract class ServerPlayerKeepInventoryMixin {
    @Invoker("transferInventoryXpAndScore")
    protected abstract void betterVanilla$transferInventoryXpAndScore(Player oldPlayer);

    @Inject(method = "restoreFrom", at = @At("TAIL"))
    private void betterVanilla$copyKeepInventorySetting(ServerPlayer oldPlayer, boolean alive, CallbackInfo ci) {
        KeepInventoryAccess current = (KeepInventoryAccess) this;
        boolean enabled = ((KeepInventoryAccess) oldPlayer).betterVanilla$isKeepInventoryEnabled();
        current.betterVanilla$setKeepInventoryEnabled(enabled);
        if (enabled && !alive) betterVanilla$transferInventoryXpAndScore(oldPlayer);
    }
}
