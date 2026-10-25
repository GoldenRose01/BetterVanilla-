package it.elevators.mixin;

import it.elevators.BeaconAppleHandler;
import net.minecraft.world.entity.item.ItemEntity;
import org.spongepowered.asm.mixin.Mixin;
import org.spongepowered.asm.mixin.injection.At;
import org.spongepowered.asm.mixin.injection.Inject;
import org.spongepowered.asm.mixin.injection.callback.CallbackInfo;

@Mixin(ItemEntity.class)
abstract class ItemEntityMixin {
    @Inject(method = "tick", at = @At("HEAD"))
    private void elevators$activateRegenerationBeacon(CallbackInfo ci) {
        BeaconAppleHandler.tryActivate((ItemEntity) (Object) this);
    }
}
