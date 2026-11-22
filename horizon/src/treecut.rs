use mod_api::{
    BlockState, BlockUpdateFlags, EquipmentSlot, Flow, ItemStack, LevelRef, PlayerRef, ServerHost,
};
use std::collections::{HashMap, HashSet, VecDeque};
use std::sync::atomic::{AtomicBool, Ordering};
use std::sync::Mutex;

const MAX_LOGS: usize = 256;
const MAX_LEAVES: usize = 512;

#[derive(Clone, Copy, PartialEq, Eq)]
pub enum TreeCutMode {
    Off,
    Fell,
    Chop,
}

impl TreeCutMode {
    pub fn as_str(self) -> &'static str {
        match self {
            Self::Off => "off",
            Self::Fell => "fell",
            Self::Chop => "chop",
        }
    }

    pub fn parse(s: &str) -> Option<Self> {
        match s {
            "off" => Some(Self::Off),
            "fell" => Some(Self::Fell),
            "chop" => Some(Self::Chop),
            _ => None,
        }
    }
}

pub struct TreeFellerState {
    mode: Mutex<TreeCutMode>,
    processing: AtomicBool,
}

impl Default for TreeFellerState {
    fn default() -> Self {
        Self {
            mode: Mutex::new(TreeCutMode::Fell),
            processing: AtomicBool::new(false),
        }
    }
}

impl TreeFellerState {
    pub fn set_mode(&self, mode: TreeCutMode) {
        *self.mode.lock().expect("treecut mode lock") = mode;
    }

    pub fn mode(&self) -> TreeCutMode {
        *self.mode.lock().expect("treecut mode lock")
    }

    pub fn on_break_block(
        &self,
        host: &mut dyn ServerHost,
        pos: [i32; 3],
        state: BlockState,
    ) -> Flow {
        if self.processing.load(Ordering::Relaxed) {
            return Flow::Continue;
        }
        if host.block_in_tag(state.block(), "minecraft:logs") != Some(true) {
            return Flow::Continue;
        }
        let Some(player) = host.players().acting_player() else {
            return Flow::Continue;
        };
        if player.entity().is_shift_key_down(host) != Some(true) {
            return Flow::Continue;
        }
        let Some(level) = player.level(host) else {
            return Flow::Continue;
        };

        let creative = player.is_creative(host) == Some(true);
        let tool = main_hand_stack(host, player);
        let active_mode = if creative {
            TreeCutMode::Chop
        } else {
            if !is_valid_axe(host, tool.as_ref()) {
                return Flow::Continue;
            }
            let m = self.mode();
            if m == TreeCutMode::Off {
                return Flow::Continue;
            }
            m
        };

        let Some((logs, leaves)) = scan_tree(host, level, pos) else {
            return Flow::Continue;
        };
        if logs.is_empty() || leaves.is_empty() {
            return Flow::Continue;
        }

        self.processing.store(true, Ordering::Relaxed);
        let flow = if active_mode == TreeCutMode::Fell {
            execute_fell(host, level, player, tool.as_ref(), pos, &logs, &leaves);
            Flow::Cancel
        } else {
            execute_chop(host, level, player, tool.as_ref(), pos, &logs);
            Flow::Continue
        };
        self.processing.store(false, Ordering::Relaxed);
        flow
    }
}

fn main_hand_stack(host: &dyn ServerHost, player: PlayerRef) -> Option<ItemStack> {
    let slot = player.selected_slot(host)? as u32;
    player
        .inventory_slots(host)?
        .into_iter()
        .find(|(idx, _)| *idx == slot)
        .map(|(_, stack)| stack)
}

fn is_valid_axe(host: &dyn ServerHost, tool: Option<&ItemStack>) -> bool {
    let Some(tool) = tool else {
        return false;
    };
    let id = tool.item().id(host);
    id.ends_with("_axe") && !id.contains("wooden_axe") && !id.contains("wood_axe")
}

fn scan_tree(
    host: &dyn ServerHost,
    level: LevelRef,
    origin: [i32; 3],
) -> Option<(Vec<[i32; 3]>, Vec<[i32; 3]>)> {
    let mut logs = Vec::new();
    let mut visited_logs = HashSet::new();
    let mut queue = VecDeque::new();
    visited_logs.insert(origin);
    queue.push_back(origin);

    while let Some(curr) = queue.pop_front() {
        logs.push(curr);
        if logs.len() >= MAX_LOGS {
            break;
        }
        for dx in -1..=1 {
            for dy in -1..=1 {
                for dz in -1..=1 {
                    if dx == 0 && dy == 0 && dz == 0 {
                        continue;
                    }
                    let next = [curr[0] + dx, curr[1] + dy, curr[2] + dz];
                    if visited_logs.contains(&next) {
                        continue;
                    }
                    if let Some(st) = level.block_state(host, next)
                        && host.block_in_tag(st.block(), "minecraft:logs") == Some(true)
                    {
                        visited_logs.insert(next);
                        queue.push_back(next);
                    }
                }
            }
        }
    }

    let mut leaves = Vec::new();
    let mut visited_leaves = HashSet::new();
    let mut leaf_queue = VecDeque::new();
    for &log in &logs {
        for dx in -1..=1 {
            for dy in -1..=1 {
                for dz in -1..=1 {
                    let next = [log[0] + dx, log[1] + dy, log[2] + dz];
                    if visited_logs.contains(&next) || visited_leaves.contains(&next) {
                        continue;
                    }
                    if let Some(st) = level.block_state(host, next)
                        && host.block_in_tag(st.block(), "minecraft:leaves") == Some(true)
                    {
                        visited_leaves.insert(next);
                        leaf_queue.push_back(next);
                    }
                }
            }
        }
    }

    while let Some(curr) = leaf_queue.pop_front() {
        leaves.push(curr);
        if leaves.len() >= MAX_LEAVES {
            break;
        }
        for [dx, dy, dz] in [
            [1, 0, 0],
            [-1, 0, 0],
            [0, 1, 0],
            [0, -1, 0],
            [0, 0, 1],
            [0, 0, -1],
        ] {
            let next = [curr[0] + dx, curr[1] + dy, curr[2] + dz];
            if visited_leaves.contains(&next) {
                continue;
            }
            if let Some(st) = level.block_state(host, next)
                && host.block_in_tag(st.block(), "minecraft:leaves") == Some(true)
            {
                visited_leaves.insert(next);
                leaf_queue.push_back(next);
            }
        }
    }

    Some((logs, leaves))
}

fn execute_fell(
    host: &mut dyn ServerHost,
    level: LevelRef,
    player: PlayerRef,
    tool: Option<&ItemStack>,
    origin: [i32; 3],
    logs: &[[i32; 3]],
    leaves: &[[i32; 3]],
) {
    let creative = player.is_creative(host) == Some(true);
    let _ = level.destroy_block(host, origin, !creative, tool, false);
    if !creative {
        if let Some(living) = host.living(player.entity()) {
            let _ = living.hurt_and_break(host, EquipmentSlot::MainHand, 1);
        }
    }

    let cutoff_y = origin[1];
    let mut columns: HashMap<(i32, i32), Vec<[i32; 3]>> = HashMap::new();
    for &p in logs.iter().chain(leaves.iter()) {
        if p[1] > cutoff_y {
            columns.entry((p[0], p[2])).or_default().push(p);
        }
    }

    let flags = BlockUpdateFlags::CLIENTS | BlockUpdateFlags::NEIGHBORS;

    for mut col in columns.into_values() {
        col.sort_by_key(|p| p[1]);
        for from in col {
            let to = [from[0], from[1] - 1, from[2]];
            let Some(moving_state) = level.block_state(host, from) else {
                continue;
            };
            let target_is_air = level
                .block_state(host, to)
                .is_some_and(|st| st.block().id(host) == "minecraft:air");
            if target_is_air {
                let _ = level.set_block(host, to, moving_state, flags);
                let _ = level.destroy_block(host, from, false, None, false);
            }
        }
    }
}

fn execute_chop(
    host: &mut dyn ServerHost,
    level: LevelRef,
    player: PlayerRef,
    tool: Option<&ItemStack>,
    origin: [i32; 3],
    logs: &[[i32; 3]],
) {
    let creative = player.is_creative(host) == Some(true);
    for &p in logs {
        if p == origin {
            continue;
        }
        if level.destroy_block(host, p, !creative, tool, false).is_ok_and(|v| v.is_some())
            && !creative
        {
            if host
                .living(player.entity())
                .and_then(|living| living.hurt_and_break(host, EquipmentSlot::MainHand, 1))
                .is_none()
            {
                break;
            }
        }
    }
}
