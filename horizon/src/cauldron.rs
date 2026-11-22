use mod_api::{EntityRef, Identifier, ItemStack, ServerHost};

const CONCRETE_COLORS: &[&str] = &[
    "white",
    "orange",
    "magenta",
    "light_blue",
    "yellow",
    "lime",
    "pink",
    "gray",
    "light_gray",
    "cyan",
    "purple",
    "blue",
    "brown",
    "green",
    "red",
    "black",
];

fn converted_item_id(id: &str) -> Option<String> {
    match id {
        "minecraft:dirt" | "minecraft:coarse_dirt" | "minecraft:rooted_dirt" => {
            Some("minecraft:mud".to_owned())
        }
        _ => {
            let color = id
                .strip_prefix("minecraft:")?
                .strip_suffix("_concrete_powder")?;
            CONCRETE_COLORS
                .contains(&color)
                .then(|| format!("minecraft:{color}_concrete"))
        }
    }
}

pub fn try_convert(host: &mut dyn ServerHost, entity: EntityRef) {
    let Some(item_entity) = host.item_entity(entity) else {
        return;
    };
    let Some(stack) = item_entity.stack(host) else {
        return;
    };
    let item_id = stack.item().id(host);
    let Some(target_id) = converted_item_id(&item_id) else {
        return;
    };
    let (Some(level), Some(pos)) = (entity.level(host), entity.block_position(host)) else {
        return;
    };
    let Some(state) = level.block_state(host, pos) else {
        return;
    };
    if state.block().id(host) != "minecraft:water_cauldron" {
        return;
    }
    let Ok(identifier) = Identifier::parse(&target_id) else {
        return;
    };
    let Some(target_key) = host.item_key(&identifier) else {
        return;
    };
    let Some(new_stack) = ItemStack::new(target_key, stack.count()) else {
        return;
    };
    let _ = item_entity.set_stack(host, new_stack);
}
