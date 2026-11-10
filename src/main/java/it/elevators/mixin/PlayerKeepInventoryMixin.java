package it.elevators.mixin;

import it.elevators.command.KeepInventoryAccess;
import net.minecraft.server.level.ServerLevel;
import net.minecraft.server.level.ServerPlayer;
import net.minecraft.world.entity.player.Player;
import net.minecraft.world.level.storage.ValueInput;
import net.minecraft.world.level.storage.ValueOutput;
import org.spongepowered.asm.mixin.Mixin;
import org.spongepowered.asm.mixin.Unique;
import org.spongepowered.asm.mixin.injection.At;
import org.spongepowered.asm.mixin.injection.Inject;
import org.spongepowered.asm.mixin.injection.callback.CallbackInfo;
import org.spongepowered.asm.mixin.injection.callback.CallbackInfoReturnable;

@Mixin(Player.class)
abstract class PlayerKeepInventoryMixin implements KeepInventoryAccess {
    @Unique
    private boolean betterVanilla$keepInventoryEnabled;

    @Override
    public boolean betterVanilla$isKeepInventoryEnabled() {
        return betterVanilla$keepInventoryEnabled;
    }

    @Override
    public void betterVanilla$setKeepInventoryEnabled(boolean enabled) {
        betterVanilla$keepInventoryEnabled = enabled;
    }

    @Inject(method = "addAdditionalSaveData", at = @At("TAIL"))
    private void betterVanilla$saveKeepInventory(ValueOutput output, CallbackInfo ci) {
        output.putBoolean("BetterVanillaKeepInventory", betterVanilla$keepInventoryEnabled);
    }

    @Inject(method = "readAdditionalSaveData", at = @At("TAIL"))
    private void betterVanilla$loadKeepInventory(ValueInput input, CallbackInfo ci) {
        betterVanilla$keepInventoryEnabled = input.getBooleanOr("BetterVanillaKeepInventory", false);
    }

    @Inject(method = "dropEquipment", at = @At("HEAD"), cancellable = true)
    private void betterVanilla$keepPlayerInventory(ServerLevel level, CallbackInfo ci) {
        if ((Object) this instanceof ServerPlayer && betterVanilla$keepInventoryEnabled) {
            ci.cancel();
        }
    }

    @Inject(method = "getBaseExperienceReward", at = @At("HEAD"), cancellable = true)
    private void betterVanilla$keepPlayerExperience(ServerLevel level, CallbackInfoReturnable<Integer> cir) {
        if ((Object) this instanceof ServerPlayer && betterVanilla$keepInventoryEnabled) {
            cir.setReturnValue(0);
        }
    }
}
