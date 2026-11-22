use crate::{keepinventory::KeepInventoryState, treecut::{TreeCutMode, TreeFellerState}};
use mod_api::{Command, CommandArgumentValue, RegisterCommandsEvent, RunModCommandEvent, ServerHost};

pub fn register(event: &mut RegisterCommandsEvent<'_>) {
    for usage in [
        "gameset treecut off",
        "gameset treecut fell",
        "gameset treecut chop",
        "gameset player keepinventory on <player:player>",
        "gameset player keepinventory off <player:player>",
    ] {
        event.commands.push(Command::op(usage, 2));
    }
}

pub fn handle(
    host: &mut dyn ServerHost,
    event: &RunModCommandEvent<'_>,
    treecut: &TreeFellerState,
    keepinv: &KeepInventoryState,
) -> Option<String> {
    match event.command.literals.as_slice() {
        [root, sub, mode_str] if root == "gameset" && sub == "treecut" => {
            let mode = TreeCutMode::parse(mode_str)?;
            treecut.set_mode(mode);
            Some(format!("TreeCut mode impostato su: {}.", mode.as_str()))
        }
        [root, sub, feat, toggle]
            if root == "gameset" && sub == "player" && feat == "keepinventory" =>
        {
            let enable = toggle == "on";
            let Some(CommandArgumentValue::Players(players)) = event.command.argument("player") else {
                return Some("Specificare un giocatore.".to_owned());
            };
            let mut count = 0usize;
            for target in players {
                keepinv.set_enabled(host, *target, enable);
                count += 1;
            }
            Some(format!(
                "KeepInventory {} per {} giocatore/i.",
                if enable { "attivato" } else { "disattivato" },
                count
            ))
        }
        _ => None,
    }
}
