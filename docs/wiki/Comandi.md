# Comandi

Lingue: [Italiano](Comandi.md) | [English](en-Commands.md) | [Español](es-Commands.md) | [简体中文](zh-Commands.md) | [हिन्दी](hi-Commands.md) | [العربية](ar-Commands.md) | [Português](pt-Commands.md) | [বাংলা](bn-Commands.md) | [Русский](ru-Commands.md) | [اردو](ur-Commands.md)

I comandi `/gameset` richiedono il permesso amministrativo `COMMANDS_GAMEMASTER` di Minecraft. Il giocatore selezionato per KeepInventory deve essere online.

## KeepInventory individuale

```text
/gameset player keepinventory on <player>
/gameset player keepinventory off <player>
```

Attiva o disattiva la conservazione di inventario ed esperienza alla morte per il giocatore indicato. L’impostazione è personale e indipendente dalla gamerule globale `keepInventory`; viene salvata insieme ai dati del giocatore.

## Editor scambi villager

```text
/gameset villagertrade
```

Apre l’editor per il villager più vicino in un raggio di ricerca di circa 16 blocchi in orizzontale. L’editor permette di creare, modificare ed eliminare offerte, impostando:

- oggetto e quantità del costo principale;
- oggetto e quantità di un costo secondario facoltativo;
- oggetto e quantità ricevuti;
- numero massimo di utilizzi dell’offerta.

Per scegliere un oggetto, prendilo nell’inventario e clicca sul relativo slot dell’editor. Il menu ne copia il tipo e non consuma l’oggetto impugnato. Se il villager non viene trovato, il comando mostra un messaggio d’errore.

Le modifiche salvate vengono assegnate al villager e persistono nel mondo. L’editor richiede che il giocatore resti vicino al villager mentre il menu è aperto.
