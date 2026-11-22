mod beacon;
mod cauldron;
mod commands;
mod keepinventory;
mod treecut;

use mod_api::{
    BreakBlockEvent, EntityTickListenedEvent, EntityTickPostEvent, Flow, Hand, Hooks,
    HostSide, InteractionResult, LivingDeathEvent, LivingDropsEvent, PlayerLoggedOutEvent,
    PlayerRespawnedEvent, RegisterCommandsEvent, RightClickBlockEvent, RunModCommandEvent,
    ServerHost, SoundSource, Text,
};
use std::collections::HashMap;
use std::sync::Mutex;

const ELEVATOR_TAG: &str = "elevators:elevators";
const RANGE: i32 = 128;
const COOLDOWN_TICKS: i64 = 8;

#[derive(Default)]
struct Elevators {
    last_use: Mutex<HashMap<u128, i64>>,
    beacon: beacon::BeaconState,
    treecut: treecut::TreeFellerState,
    keepinv: keepinventory::KeepInventoryState,
}

#[mod_api::horizon_mod]
impl Hooks for Elevators {
    fn register_commands(&self, _: &mut dyn ServerHost, event: &mut RegisterCommandsEvent<'_>) {
        commands::register(event);
    }

    fn run_mod_command(
        &self,
        host: &mut dyn ServerHost,
        event: &mut RunModCommandEvent<'_>,
    ) -> Flow {
        let Some(msg) = commands::handle(host, event, &self.treecut, &self.keepinv) else {
            return Flow::Continue;
        };
        let _ = host.commands().feedback(Text::literal(msg));
        Flow::Cancel
    }

    fn entity_tick_listened(&self, _: &mut dyn ServerHost, event: &mut EntityTickListenedEvent) {
        event.listened = true;
    }

    fn entity_tick_post(&self, host: &mut dyn ServerHost, event: &mut EntityTickPostEvent<'_>) {
        if event.kind != "minecraft:item" {
            return;
        }
        let Some(entity) = host.entity(event.entity) else {
            return;
        };
        cauldron::try_convert(host, entity);
        self.beacon.try_activate(host, event.entity, entity);
    }

    fn break_block(&self, host: &mut dyn ServerHost, event: &mut BreakBlockEvent<'_>) -> Flow {
        self.treecut.on_break_block(host, event.pos, event.state)
    }

    fn living_death(&self, host: &mut dyn ServerHost, event: &mut LivingDeathEvent<'_>) -> Flow {
        self.keepinv.on_living_death(host, event);
        Flow::Continue
    }

    fn living_drops(&self, host: &mut dyn ServerHost, event: &mut LivingDropsEvent<'_>) -> Flow {
        self.keepinv.on_living_drops(host, event);
        Flow::Continue
    }

    fn player_respawned(&self, host: &mut dyn ServerHost, event: &mut PlayerRespawnedEvent<'_>) {
        self.keepinv.on_player_respawned(host, event);
    }

    fn right_click_block(&self, host: HostSide<'_>, event: &mut RightClickBlockEvent<'_>) -> Flow {
        let HostSide::Server(host) = host else {
            return Flow::Continue;
        };
        let click = event.click;
        if click.hand != Hand::Main {
            return Flow::Continue;
        }
        let Some(player) = host.player(click.player) else {
            return Flow::Continue;
        };
        if player.is_spectator(host) == Some(true) {
            return Flow::Continue;
        }
        let Some(level) = player.level(host) else {
            return Flow::Continue;
        };
        let is_elevator = |host: &dyn ServerHost, pos| {
            level
                .block_state(host, pos)
                .and_then(|state| host.block_in_tag(state.block(), ELEVATOR_TAG))
                == Some(true)
        };
        if !is_elevator(host, click.pos) {
            return Flow::Continue;
        }

        *event.cancellation_result = InteractionResult::Consume;
        let (Some(now), Some((min_y, height))) = (level.game_time(host), level.height(host))
        else {
            return Flow::Cancel;
        };
        {
            let mut last_use = self
                .last_use
                .lock()
                .expect("elevator cooldown lock poisoned");
            if last_use
                .get(&click.player)
                .is_some_and(|previous| (0..COOLDOWN_TICKS).contains(&(now - previous)))
            {
                return Flow::Cancel;
            }
            last_use.insert(click.player, now);
        }

        let direction = if player.entity().is_shift_key_down(host) == Some(true) {
            -1
        } else {
            1
        };
        let [x, y, z] = click.pos;
        let landing = (1..=RANGE)
            .map(|distance| y + direction * distance)
            .take_while(|&candidate| (min_y..min_y + height - 2).contains(&candidate))
            .find(|&candidate| is_elevator(host, [x, candidate, z]));

        let Some(landing) = landing else {
            let line = Text::translate("message.elevators.no_destination", vec![]);
            let _ = player.action_bar(host, line);
            return Flow::Cancel;
        };

        let [x, floor, z] = [f64::from(x), f64::from(landing + 1), f64::from(z)];
        let clearance = [x, floor, z, x + 1.0, floor + 2.0, z + 1.0];
        if level.has_collisions(host, clearance) != Some(false) {
            let line = Text::translate("message.elevators.blocked_destination", vec![]);
            let _ = player.action_bar(host, line);
            return Flow::Cancel;
        }
        if player
            .entity()
            .teleport_to(host, [x + 0.5, floor, z + 0.5], None)
        {
            for (sound, volume) in [
                ("minecraft:entity.enderman.teleport", 0.8),
                ("minecraft:entity.shulker.teleport", 0.65),
            ] {
                level.play_sound(
                    host,
                    sound,
                    [x + 0.5, floor - 0.5, z + 0.5],
                    SoundSource::Players,
                    volume,
                    1.0,
                );
            }
        }
        Flow::Cancel
    }

    fn player_logged_out(&self, _: &mut dyn ServerHost, event: &mut PlayerLoggedOutEvent<'_>) {
        self.last_use
            .lock()
            .expect("elevator cooldown lock poisoned")
            .remove(&event.player);
    }
}
