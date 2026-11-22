"""Generate Horizon's elevator declarations from the original Fabric resources."""

import json
from pathlib import Path
import shutil


HORIZON = Path(__file__).resolve().parents[1]
FABRIC = HORIZON.parent / "src/main/resources"


def write_json(relative_path, contents):
    path = HORIZON / relative_path
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(contents, indent=2) + "\n", encoding="utf-8")


def main():
    fabric_recipes = FABRIC / "data/elevators/recipe"
    assets = HORIZON / "assets/elevators"
    shutil.copytree(FABRIC / "assets/elevators", assets, dirs_exist_ok=True)

    all_blocks = []
    picks = []
    wool = []
    outputs = HORIZON / "data"
    if outputs.exists():
        shutil.rmtree(outputs)
    for recipe in sorted(fabric_recipes.glob("*_elevator.json")):
        contents = json.loads(recipe.read_text(encoding="utf-8"))
        base = contents["ingredients"][0]
        block_id = contents["result"]["id"]
        all_blocks.append(block_id)
        (wool if base.endswith("_wool") else picks).append(block_id)
        write_json(f"data/elevators/recipe/{recipe.name}", contents)
        write_json(
            f"data/elevators/horizon/block/{recipe.name}",
            {"template": base, "item": True},
        )
        write_json(
            f"data/elevators/loot_table/blocks/{recipe.name}",
            {
                "type": "minecraft:block",
                "pools": [
                    {
                        "rolls": 1,
                        "entries": [{"type": "minecraft:item", "name": block_id}],
                        "conditions": [{"condition": "minecraft:survives_explosion"}],
                    }
                ],
            },
        )

    if not all_blocks:
        raise RuntimeError("No elevator recipes found")
    write_json("data/elevators/tags/block/elevators.json", {"values": all_blocks})
    write_json(
        "data/minecraft/tags/block/mineable/pickaxe.json",
        {"replace": False, "values": picks},
    )
    write_json(
        "data/minecraft/tags/block/wool.json",
        {"replace": False, "values": wool},
    )
    write_json(
        "data/elevators/horizon/creative_tab/elevators.json",
        {"title": "itemGroup.elevators", "icon": "elevators:copper_elevator", "items": all_blocks},
    )


if __name__ == "__main__":
    main()
