# Configuratore mod per Windows

Avvia **ModConfigurator.exe**. È un EXE portabile Windows x64 con runtime incluso: puoi copiarlo fuori dai repository e non serve installare .NET.

## Due modalità

- **Predefiniti nei sorgenti**: scegli la cartella che contiene `BetterVanilla+`, `enchlib` ed `Eldrich-Surge`. Le modifiche finiscono nelle risorse che saranno incluse nelle successive compilazioni delle mod. I file di configurazione già esistenti nelle installazioni e nei mondi restano prioritari.
- **Configurazione installazione / mondo**: scegli la cartella Minecraft o del server. Per EnchLib scegli anche un mondo esistente, contenente `level.dat`. Eldritch Surge modifica il mondo selezionato; lascia vuoto il campo mondo per modificare i suoi predefiniti globali. BetterVanilla+ usa sempre la configurazione dell'installazione.

Seleziona un file e premi **Apri selezione** (oppure doppio clic). Il percorso effettivo è sempre mostrato sopra l'editor. Cambiare cartelle o modalità non cambia la destinazione del documento già aperto: apri nuovamente la selezione.

La scheda **Campi** modifica numeri, booleani, liste e valori dei preset. Per JSON usa `true`, `false`, numeri, stringhe fra virgolette e liste come `["minecraft:sword"]`. Per aggiungere o eliminare voci usa **Testo completo**. I file `.config` mantengono commenti e righe non modificate. Modificare JSON dalla scheda Campi riformatta il documento e rimuove i commenti; l'editor testuale li conserva. I JSON5 supportano commenti e virgole finali, come il parser di EnchLib.

**Salva / crea file** crea i file mancanti e salva atomicamente quelli esistenti, conservando un backup `.bak-<data>`. Se il file è cambiato esternamente, il salvataggio viene bloccato: riaprilo. **Carica predefiniti** carica in anteprima i preset inclusi nell'EXE all'ultima compilazione, senza scrivere subito sul disco. I file invalidi restano disponibili nell'editor testuale e non vengono sostituiti automaticamente.

Chiudi Minecraft o arresta il server prima di modificare le configurazioni, per evitare che il gioco riscriva i file. Riavvia per applicare i cambiamenti. Le modifiche ai sorgenti richiedono nuove compilazioni delle mod; l'EXE non modifica JAR già compilati.

## File e precedenze

| Mod | Risorse predefinite | Configurazione in uso |
| --- | --- | --- |
| BetterVanilla+ | `src/main/resources/config/bettervanilla/defaults.json` | `<installazione>/config/bettervanilla/config.json` |
| Eldritch Surge | `src/main/resources/config/eldrich-surge/enchantment_caps.json` | `<installazione>/config/eldrich-surge/enchantment_caps.json`, oppure `<mondo>/eldrich-surge/enchantment_caps.json` |
| EnchLib | `src/main/resources/config/*` | `<mondo>/config/enchlib/*` |

BetterVanilla+ espone distanza e cooldown ascensori, particelle, protezione spawn, bonus beacon e conversioni nei calderoni. Le impostazioni particelle sono locali al client; le regole di gioco sono lette sul server. Le ricette e gli asset restano risorse del gioco.

Eldritch Surge carica i nuovi preset dalle risorse, poi applica la configurazione globale e infine quella del mondo. I suoi incantesimi aggiunti in futuro e assenti dal file usano ancora le definizioni come fallback. Con EnchLib installata, la sua configurazione viene acquisita alla prima creazione della configurazione del mondo: controlla entrambe le mod se cambi regole condivise.

EnchLib mantiene sia i preset `.config` sia i file effettivi `AviableEnch.json5` e `EnchantmentsDetails.json5`: i preset non sostituiscono automaticamente le voci già presenti nei mondi. Alcuni preset storici sono mantenuti per compatibilità; questo programma li rende modificabili ma non introduce nuovi effetti per file che la mod non usa. `EnchCategories.config` è il nome letto da ConfigManager; `EnchCategory.config` è il preset storico. Il nome delle incompatibilità è stato uniformato a `EnchUNcompatibility.config`, mantenendo la migrazione dal precedente nome nei mondi Linux.

## Ricompilare e verificare

Richiede .NET SDK 10 e i tre repository affiancati. Esegui `./build.ps1`: i preset correnti vengono incorporati nell'EXE. Per aggiornare i predefiniti portabili, ricompila anche l'EXE dopo aver cambiato i sorgenti.

Esegui `ModConfigurator.exe --self-test <percorso-assoluto-report.txt>` per verificare catalogo, formati, backup, conflitti, intervalli e costruzione dell'interfaccia. Il test genera anche una schermata `<report.txt>.png`; usa file temporanei e non salva configurazioni reali.
