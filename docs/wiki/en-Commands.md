# Commands

Languages: [Italiano](Comandi.md) | [English](en-Commands.md) | [Español](es-Commands.md) | [简体中文](zh-Commands.md) | [हिन्दी](hi-Commands.md) | [العربية](ar-Commands.md) | [Português](pt-Commands.md) | [বাংলা](bn-Commands.md) | [Русский](ru-Commands.md) | [اردو](ur-Commands.md)

The `/gameset` commands require Minecraft's `COMMANDS_GAMEMASTER` administrative permission. The player targeted by KeepInventory must be online.

## Personal KeepInventory

```text
/gameset player keepinventory on <player>
/gameset player keepinventory off <player>
```

Enable or disable retaining the selected player's inventory and experience on death. This personal setting is independent of the global `keepInventory` gamerule and is saved with player data.

## Villager trade editor

```text
/gameset villagertrade
```

Opens the editor for the nearest villager within approximately 16 blocks horizontally. Create, edit or delete offers, setting:

- Main cost item and quantity.
- Optional second cost item and quantity.
- Result item and quantity.
- Maximum number of uses.

Pick up an item in your inventory and click the corresponding editor slot. The menu copies its type without consuming the held item. If no villager is found, the command displays an error.

Saved changes are assigned to the villager and persist in the world. Stay near the villager while the menu is open.

---

## Contents

- [Home](en-Home.md)
- [Installation](en-Installation.md)
- [Elevators](en-Elevators.md)
- [Recipes](en-Recipes.md)
- [Cauldrons](en-Cauldrons.md)
- [Beacon bonus](en-Beacon.md)
- [Commands](en-Commands.md)
- [Release 1.1.0](en-Release-1.1.0.md)
- [Compatibility](en-Compatibility.md)
- [Frequently asked questions](en-FAQ.md)

[Repository](https://github.com/GoldenRose01/BetterVanilla-) | [Report an issue](https://github.com/GoldenRose01/BetterVanilla-/issues)
