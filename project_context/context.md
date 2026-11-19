# 📜 Project Context — BetterVanilla+

> Questo documento è la **fonte unica di verità e comprensione** del progetto **BetterVanilla+** (precedentemente *Elevators Mod*).
> È collocato nella cartella `project_context/` (completamente disaccoppiata dal sistema di build Gradle) per consentire a qualunque assistente o sviluppatore di comprendere immediatamente l'architettura della mod, tutte le feature in stile Vanilla-plus, la configurazione, i comandi, le convenzioni di codice e le istruzioni di build e rilascio.

---

## 1. Panoramica del Progetto (Project Overview)

* **Nome Mod:** BetterVanilla+ (`elevators` / `bettervanilla`)
* **Piattaforma e Mod Loader:** Fabric (Minecraft 26.3 e Minecraft 26.4 Snapshot)
* **Versione Mod Corrente:** `1.1.0`
* **Linguaggio:** Java 25 (`options.release = 25`)
* **Dipendenze Fondamentali:**
  * Fabric Loader (`0.19.5+`)
  * Fabric API (`0.164.0+26.3`)
  * Java 25
* **Mod Esterne di Appoggio (`external_mod/`):**
  * `fabric-api-0.166.2+26.3.jar`
  * `modmenu-21.0.1.jar`
  * `cloth-config-fabric-26.3.159.jar`
  * `yet_another_config_lib_v3-3.9.7+26.3-fabric.jar`
  * `modelfix-1.21.10-1.12-fabric.jar`
* **JAR di Rilascio (`jar/`):**
  * `BetterVanilla+-1.1.0-26.3.jar`
  * `BetterVanilla+-1.1.0-26.4-snapshot-7.jar`
  * *(e i relativi archivi storici)*

---

## 2. Architettura del Codice e Mappa dei Package

Il codice della mod è strutturato in:
* `src/main/java`: logica di gioco comune e server.
* `src/client/java`: interfacce client, schermi di configurazione e mixin grafici.
* `src/main/resources`: blocchi, ricette JSON, texture, modelli dei blocchi ascensore e file di configurazione predefiniti.

### A. Package Comuni (`src/main/java/it/elevators` e `com/example`)

| Package / Classe | Scopo e Funzionalità |
|---|---|
| `it.elevators.ElevatorsMod` | Entry point principale della mod (registrazione blocchi ascensore, tab creativa, eventi del server, handler interazioni). |
| `it.elevators.BeaconAppleHandler` | Intercetta gli oggetti a terra sui beacon: gettando una Mela d'Oro su un beacon di livello 4 con effetto Rigenerazione, la mela viene consumata e applica **Saturazione V** a tutti i giocatori entro 50 blocchi. |
| `it.elevators.CauldronInteractionHandler` | Interazioni con calderoni pieni d'acqua:<br>• Terra (dirt, coarse dirt, rooted dirt) lanciata nel calderone si trasforma istantaneamente in **Fango (Mud)**.<br>• Polvere di calcestruzzo (qualsiasi colore) lanciata nel calderone si solidifica nel rispettivo blocco di **Calcestruzzo (Concrete)** per l'intero stack. |
| `it.elevators.BetterVanillaConfig` | Gestione configurazione della mod (salvataggio/caricamento JSON in `config/bettervanilla/`). |
| `it.elevators.command.GameSetCommands` | Implementazione della suite di comandi `/gameset`:<br>• `/gameset player keepinventory <on\|off> <player>`: keep-inventory personale per giocatore.<br>• `/gameset villagertrade`: apre l'editor GUI per modificare le offerte del villico più vicino entro 16 blocchi.<br>• `/gameset treecut <off\|fell\|chop>`: seleziona la modalità di taglio alberi. |
| `it.elevators.command.KeepInventoryAccess` | Interfaccia per il salvataggio persistente del keepInventory per singolo giocatore. |
| `it.elevators.command.VillagerTradeMenu` | Menu container e logica per l'editor delle offerte dei villici senza consumo dei template. |
| `it.elevators.tree.TreeFeller` e `TreeCutMode` | Logica di taglio alberi con ascia in sneak (modalità `OFF`, `CHOP`, `RECURSIVE`, `MINE`). |
| `it.elevators.mixin` | Mixin critici:<br>• `BeaconBlockEntityAccessor`: accesso ai dati del beacon.<br>• `ItemEntityMixin`: intercettazione caduta mele e terra su calderoni/beacon.<br>• `PlayerKeepInventoryMixin` e `ServerPlayerKeepInventoryMixin`: gestione del keepInventory personale al momento della morte e respawn.<br>• `NaturalSpawnerMixin`: ottimizzazione e controllo spawn. |

### B. Package Client (`src/client/java` e `src/main/java/it/elevators/client`)
* `it.elevators.ElevatorsClient`: registrazione rendering blocchi traslucidi/ritagli e particelle.
* `it.elevators.client.BetterVanillaModMenuIntegration`: integrazione con ModMenu.
* `it.elevators.client.TreeCutClothConfigScreen` e `TreeCutVanillaConfigScreen`: schermate interattive per regolare il tree cutting e le opzioni di gioco da interfaccia utente.

---

## 3. Funzionalità Chiave della Mod

### A. Ascensori (Elevators)
* **Blocchi disponibili:** 16 varianti di colore per la Lana (`white_wool_elevator`, `red_wool_elevator`, ecc.), oltre ad ascensori ricavati da blocchi cesellati e rame (`copper_elevator`, `chiseled_stone_bricks_elevator`, `chiseled_deepslate_elevator`, ecc.).
* **Funzionamento:**
  * Click destro (o Salto) per salire all'ascensore successivo allineato sulla stessa colonna X/Z (entro 128 blocchi).
  * Sneak + Click destro (o Sneak) per scendere all'ascensore inferiore.
  * Richiede 2 blocchi d'aria sopra il blocco di destinazione per evitare soffocamenti.
  * Suoni combinati di teletrasporto Enderman e Shulker con emissione di particelle colorate.
* **Ricetta:** 1 blocco base (es. Lana Bianca) + 1 Perla dell'End + 1 Redstone (senza forma).

### B. Mela d'Oro su Beacon (Saturazione V)
* Buttando una Mela d'Oro sopra un Beacon di livello 4 (base di 164 blocchi) con Rigenerazione attiva, il beacon consuma la mela ed emette un'ondata di **Saturazione V** a tutti i giocatori entro 50 blocchi.

### C. Trasformazioni con Calderoni d'Acqua
* Trasformazione istantanea dello stack di Terra in Fango.
* Trasformazione istantanea dello stack di Polvere di Calcestruzzo in Calcestruzzo solido.

### D. KeepInventory Personale (`/gameset player keepinventory`)
* Permette di impostare la conservazione dell'inventario e dei livelli XP per singoli giocatori su server multiplayer, indipendentemente dalla gamerule globale del mondo.

### E. Editor Scambi Villici (`/gameset villagertrade`)
* Apre un'interfaccia intuitiva per visualizzare, creare, modificare costi e risultati delle offerte dei villici nelle vicinanze.

### F. Taglio Alberi (Tree Cutting)
* Quando il giocatore si abbassa (`sneak`) impugnando un'ascia compatibile, l'abbattimento del tronco si propaga all'intero albero rispettando la durabilità. Compatibile e coordinato con Eldritch Surge (`Lumberjack`).

---

## 4. Script e Istruzioni di Build

1. **Compilazione JAR di Rilascio:**
   ```powershell
   ./gradlew.bat jar
   ```
   Produce l'archivio nella directory `build/libs/` e/o `jar/`:
   `BetterVanilla+-1.1.0-26.3.jar`

2. **Avvio Ambiente di Sviluppo:**
   ```powershell
   ./gradlew.bat runClient
   ```

> [!IMPORTANT]
> **Non cancellare mai** i file JAR presenti nella cartella `jar/`.
