# Installazione

## Requisiti

- Minecraft Java **26.3** oppure **26.4 Snapshot 7**, secondo il JAR scelto.
- Fabric Loader compatibile con la versione del gioco.
- Fabric API corrispondente alla stessa versione di Minecraft.
- Java 25 o successivo.

## Installare la mod

1. Installa Fabric Loader per la versione di Minecraft che vuoi avviare.
2. Aggiungi Fabric API compatibile nella cartella `mods`.
3. Copia un solo JAR BetterVanilla+ 1.1.0 nella cartella `mods`.
4. Avvia Minecraft con il profilo Fabric corrispondente.

Scegli [il JAR per 26.3](../../jar/BetterVanilla+-1.1.0-26.3.jar) oppure [il JAR per 26.4 Snapshot 7](../../jar/BetterVanilla+-1.1.0-26.4-snapshot-7.jar). Non installare entrambi nello stesso profilo.

## Server e client

Per usare gli ascensori e vedere gli oggetti personalizzati, installa la mod sul server e su ogni client che vi accede. In singleplayer, installala nel profilo locale: il server integrato usa la stessa installazione. Fabric API deve essere presente nello stesso ambiente.

I comandi e le modifiche al villager vengono eseguiti lato server. Le particelle di attività degli ascensori sono un effetto client e vengono visualizzate da chi ha la mod installata.

## Build del progetto

Con JDK 25 installato, esegui `gradlew.bat build` su Windows. Il JAR di sviluppo viene creato in `build/libs/`; le versioni distribuite sono nella cartella `jar/`.
