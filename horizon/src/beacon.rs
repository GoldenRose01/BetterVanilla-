use mod_api::{ActiveEffect, Compound, EntityRef, ItemStack, ServerHost, Tag};
use std::collections::HashMap;
use std::sync::Mutex;

const REQUIRED_LEVEL: i32 = 4;
const COOLDOWN_TICKS: i64 = 20;
const RANGE_SQR: f64 = 50.0 * 50.0;
const SATURATION_AMPLIFIER: i32 = 4;

#[derive(Default)]
pub struct BeaconState {
    next_use_tick: Mutex<HashMap<u32, i64>>,
}

impl BeaconState {
    pub fn try_activate(&self, host: &mut dyn ServerHost, entity_id: u32, entity: EntityRef) {
        let Some(item_entity) = host.item_entity(entity) else {
            return;
        };
        let Some(stack) = item_entity.stack(host) else {
            return;
        };
        if stack.item().id(host) != "minecraft:golden_apple" {
            return;
        }
        let (Some(level), Some(pos), Some(exact_pos)) = (
            entity.level(host),
            entity.block_position(host),
            entity.position(host),
        ) else {
            return;
        };
        let Some(now) = level.game_time(host) else {
            return;
        };
        {
            let guard = self.next_use_tick.lock().expect("beacon cooldown lock");
            if guard.get(&entity_id).is_some_and(|&next| now < next) {
                return;
            }
        }

        let beacon_pos = [pos[0], pos[1] - 1, pos[2]];
        if exact_pos[0] < f64::from(beacon_pos[0])
            || exact_pos[0] >= f64::from(beacon_pos[0]) + 1.0
            || exact_pos[2] < f64::from(beacon_pos[2])
            || exact_pos[2] >= f64::from(beacon_pos[2]) + 1.0
        {
            return;
        }
        let Some(state) = level.block_state(host, beacon_pos) else {
            return;
        };
        if state.block().id(host) != "minecraft:beacon" {
            return;
        }
        let Some(be) = host.block_entity(level, beacon_pos) else {
            return;
        };
        let Some(data) = be.data(host) else {
            return;
        };
        if !beacon_active_with_regen(&data) {
            return;
        }
        let Some(saturation_key) = host.effect_key("minecraft:saturation") else {
            return;
        };

        {
            let mut guard = self.next_use_tick.lock().expect("beacon cooldown lock");
            guard.insert(entity_id, now + COOLDOWN_TICKS);
        }

        let remaining = stack.count().saturating_sub(1);
        if remaining == 0 {
            let _ = entity.discard(host);
        } else if let Some(new_stack) = ItemStack::new(stack.item(), remaining) {
            let _ = item_entity.set_stack(host, new_stack);
        }

        let center = [
            f64::from(beacon_pos[0]) + 0.5,
            f64::from(beacon_pos[1]) + 0.5,
            f64::from(beacon_pos[2]) + 0.5,
        ];
        let effect = ActiveEffect::new(saturation_key, 1, SATURATION_AMPLIFIER, false, true, true);
        for player in host.players().server_players() {
            if player.level(host) == Some(level)
                && player
                    .entity()
                    .distance_to_sqr(host, center)
                    .is_some_and(|d| d <= RANGE_SQR)
            {
                if let Some(living) = host.living(player.entity()) {
                    let _ = living.add_effect(host, &effect);
                }
            }
        }
    }
}

fn beacon_active_with_regen(data: &Compound) -> bool {
    let levels = match data.get(&"Levels".into()) {
        Some(Tag::Int(n)) => *n,
        Some(Tag::Byte(n)) => i32::from(*n),
        Some(Tag::Short(n)) => i32::from(*n),
        _ => 0,
    };
    if levels < REQUIRED_LEVEL {
        return false;
    }
    is_regeneration(data.get(&"primary_effect".into()))
        || is_regeneration(data.get(&"secondary_effect".into()))
        || is_regeneration(data.get(&"Primary".into()))
        || is_regeneration(data.get(&"Secondary".into()))
}

fn is_regeneration(tag: Option<&Tag>) -> bool {
    match tag {
        Some(Tag::String(s)) => {
            let s = s.to_string_lossy();
            s == "minecraft:regeneration" || s == "regeneration"
        }
        Some(Tag::Int(10)) => true,
        _ => false,
    }
}
