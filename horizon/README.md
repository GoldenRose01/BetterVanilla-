# BetterVanilla+ per Horizon

Anteprima degli ascensori per Horizon Nightly 26.3 del 22 novembre 2026, build `2b77287`.

Installa il file `.hmod` nella cartella `horizon-mods/` dell'istanza o del server e aggiungi `mod.elevators:true` a `horizon-mods.txt` nella stessa directory che contiene `horizon-mods/`. In multiplayer installa la stessa versione su Horizon client e server. Riavvia dopo l'installazione.

- 29 varianti con ricette, texture, identificativi e scheda creativa.
- Clic destro per salire. Accovacciati e fai clic destro per scendere.
- Cerca la prima piattaforma allineata entro 128 blocchi. Richiede due blocchi liberi sopra la destinazione.
- Il teletrasporto è stato provato su un mondo di test con lana e rame.

Questa anteprima copre gli ascensori. Particelle ambientali, configurazione, beacon, calderoni, abbattimento alberi, protezione spawn e comandi `/gameset` sono fuori da questa build.

## Build

Da `horizon/`, genera dati e risorse con `python3 tools/prepare_assets.py`, poi esegui `cargo horizon-mod build --wasm` e `cargo horizon-mod check target/horizon-mod/elevators.hmod`.
