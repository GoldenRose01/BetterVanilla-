use mod_api::{
    Compound, ItemStack, LivingDeathEvent, LivingDropsEvent, PlayerRef, PlayerRespawnedEvent,
    ServerHost, Tag,
};
use std::collections::HashMap;
use std::sync::Mutex;

const KEY: &str = "bettervanilla:keep_inventory";

struct SavedInventory {
    slots: Vec<(u32, ItemStack)>,
    exp_level: i32,
    exp_points: i32,
}

#[derive(Default)]
pub struct KeepInventoryState {
    saved: Mutex<HashMap<u128, SavedInventory>>,
}

impl KeepInventoryState {
    pub fn set_enabled(&self, host: &mut dyn ServerHost, player: PlayerRef, enabled: bool) {
        let mut data = player.persistent_data(host).unwrap_or_else(Compound::new);
        data.insert(KEY.into(), Tag::Byte(i8::from(enabled)));
        let _ = player.set_persistent_data(host, data);
    }

    pub fn is_enabled(&self, host: &dyn ServerHost, player: PlayerRef) -> bool {
        player
            .persistent_data(host)
            .and_then(|d| match d.get(&KEY.into()) {
                Some(Tag::Byte(v)) => Some(*v != 0),
                _ => None,
            })
            .unwrap_or(false)
    }

    pub fn on_living_death(&self, host: &mut dyn ServerHost, event: &LivingDeathEvent<'_>) {
        if event.kind != "minecraft:player" {
            return;
        }
        let Some(player) = host
            .players()
            .server_players()
            .into_iter()
            .find(|p| p.entity().id() == event.entity)
        else {
            return;
        };
        if !self.is_enabled(host, player) {
            return;
        }
        let Some(slots) = player.inventory_slots(host) else {
            return;
        };
        let Some(exp) = player.experience(host) else {
            return;
        };
        let Some(uuid) = player.entity().uuid(host) else {
            return;
        };
        self.saved.lock().expect("keepinv lock").insert(
            uuid,
            SavedInventory {
                slots,
                exp_level: exp.level,
                exp_points: exp.total,
            },
        );
    }

    pub fn on_living_drops(&self, host: &mut dyn ServerHost, event: &mut LivingDropsEvent<'_>) {
        if event.kind != "minecraft:player" {
            return;
        }
        let Some(uuid) = event.player else {
            return;
        };
        if self.saved.lock().expect("keepinv lock").contains_key(&uuid) {
            event.drops.clear();
        }
        let _ = host;
    }

    pub fn on_player_respawned(&self, host: &mut dyn ServerHost, event: &PlayerRespawnedEvent<'_>) {
        let Some(saved) = self.saved.lock().expect("keepinv lock").remove(&event.player) else {
            return;
        };
        let Some(player) = host.player(event.player) else {
            return;
        };
        for (slot, stack) in saved.slots {
            let _ = player.set_inventory_slot(host, slot, Some(stack));
        }
        let _ = player.set_experience_levels(host, saved.exp_level);
        let _ = player.set_experience_points(host, saved.exp_points);
    }
}
