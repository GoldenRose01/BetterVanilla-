# Problemi frequenti

Lingue: [Italiano](FAQ.md) | [English](en-FAQ.md) | [Español](es-FAQ.md) | [简体中文](zh-FAQ.md) | [हिन्दी](hi-FAQ.md) | [العربية](ar-FAQ.md) | [Português](pt-FAQ.md) | [বাংলা](bn-FAQ.md) | [Русский](ru-FAQ.md) | [اردو](ur-FAQ.md)

## Gli ascensori non si attivano

Controlla che il blocco sia una variante di BetterVanilla+, che la destinazione si trovi sulla stessa coordinata X/Z e nella direzione scelta, e che sia entro 128 blocchi. Servono inoltre due blocchi liberi sopra la destinazione.

## Non vedo le particelle

Le particelle sono un effetto client: verifica di avere la mod nel client e di trovarti entro 15 blocchi dall’ascensore. L’aggiornamento dell’effetto non avviene ogni singolo tick.

## Il comando `/gameset` è sconosciuto o non autorizzato

Verifica che la mod e Fabric API siano caricate nel server e che tu abbia il permesso `COMMANDS_GAMEMASTER`. Per modificare un villager, avvicinati: il comando cerca solo nelle vicinanze.

## Il beacon non concede Saturazione

Verifica livello 4, Rigenerazione come effetto primario o secondario e mela d’oro gettata direttamente sul beacon. L’effetto raggiunge i giocatori entro 50 blocchi.

## Il calderone non trasforma l’oggetto

Serve un calderone d’acqua. Le conversioni supportate sono terra, terra sterile, terra con radici e polvere di cemento.

## Il JAR non si avvia

Controlla che Minecraft, Fabric Loader, Fabric API e JAR corrispondano tutti alla stessa versione: 26.3 oppure 26.4 Snapshot 7. Consulta [Installazione](Installazione.md) e [Compatibilità](Compatibilita.md).
