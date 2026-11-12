# Comandos

Idiomas: [Italiano](Comandi.md) | [English](en-Commands.md) | [Español](es-Commands.md) | [简体中文](zh-Commands.md) | [हिन्दी](hi-Commands.md) | [العربية](ar-Commands.md) | [Português](pt-Commands.md) | [বাংলা](bn-Commands.md) | [Русский](ru-Commands.md) | [اردو](ur-Commands.md)

Los comandos `/gameset` requieren el permiso administrativo `COMMANDS_GAMEMASTER`. El jugador seleccionado para KeepInventory debe estar conectado.

## KeepInventory individual

```text
/gameset player keepinventory on <player>
/gameset player keepinventory off <player>
```

Activa o desactiva la conservación del inventario y experiencia al morir para el jugador indicado. Es personal, independiente de la regla global `keepInventory`, y se guarda con los datos del jugador.

## Editor de intercambios

```text
/gameset villagertrade
```

Abre el editor del aldeano más cercano dentro de unos 16 bloques en horizontal. Permite crear, modificar y eliminar ofertas, estableciendo:

- Objeto y cantidad del coste principal.
- Objeto y cantidad del segundo coste opcional.
- Objeto y cantidad recibidos.
- Número máximo de usos.

Toma un objeto del inventario y pulsa la casilla correspondiente. El menú copia el tipo sin consumir el objeto sostenido. Si no encuentra un aldeano, muestra un error.

Los cambios guardados se asignan al aldeano y persisten en el mundo. Permanece cerca mientras el menú esté abierto.

---

## Contenido

- [Inicio](es-Home.md)
- [Instalación](es-Installation.md)
- [Ascensores](es-Elevators.md)
- [Recetas](es-Recipes.md)
- [Calderos](es-Cauldrons.md)
- [Bonificación del faro](es-Beacon.md)
- [Comandos](es-Commands.md)
- [Versión 1.1.0](es-Release-1.1.0.md)
- [Compatibilidad](es-Compatibility.md)
- [Preguntas frecuentes](es-FAQ.md)

[Repositorio](https://github.com/GoldenRose01/BetterVanilla-) | [Informar de un problema](https://github.com/GoldenRose01/BetterVanilla-/issues)
