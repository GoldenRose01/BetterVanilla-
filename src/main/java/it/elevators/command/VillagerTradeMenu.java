package it.elevators.command;

import net.minecraft.core.component.DataComponents;
import net.minecraft.network.chat.Component;
import net.minecraft.server.level.ServerPlayer;
import net.minecraft.world.MenuProvider;
import net.minecraft.world.SimpleContainer;
import net.minecraft.world.entity.player.Inventory;
import net.minecraft.world.entity.player.Player;
import net.minecraft.world.entity.npc.villager.Villager;
import net.minecraft.world.inventory.ChestMenu;
import net.minecraft.world.inventory.ContainerInput;
import net.minecraft.world.inventory.MenuType;
import net.minecraft.world.item.Item;
import net.minecraft.world.item.ItemStack;
import net.minecraft.world.item.Items;
import net.minecraft.world.item.DyeColor;
import net.minecraft.world.item.trading.ItemCost;
import net.minecraft.world.item.trading.MerchantOffer;
import net.minecraft.world.item.trading.MerchantOffers;

import java.util.Optional;

public final class VillagerTradeMenu extends ChestMenu {
    private static final int PAGE_SIZE = 45;
    private static final int SLOT_COST_A = 10;
    private static final int SLOT_COST_A_MINUS = 11;
    private static final int SLOT_COST_A_PLUS = 12;
    private static final int SLOT_COST_B = 14;
    private static final int SLOT_COST_B_MINUS = 15;
    private static final int SLOT_COST_B_PLUS = 16;
    private static final int SLOT_COST_B_CLEAR = 17;
    private static final int SLOT_RESULT = 20;
    private static final int SLOT_RESULT_MINUS = 21;
    private static final int SLOT_RESULT_PLUS = 22;
    private static final int SLOT_USES_MINUS = 30;
    private static final int SLOT_USES_VALUE = 31;
    private static final int SLOT_USES_PLUS = 32;
    private static final int SLOT_SAVE = 48;
    private static final int SLOT_DELETE = 49;
    private static final int SLOT_BACK = 50;
    private final ServerPlayer owner;
    private final Villager villager;
    private final SimpleContainer ui;
    private final int page;
    private final int editedOffer;
    private final TradeDraft draft;
    private final boolean editor;

    private VillagerTradeMenu(int syncId, Inventory inventory, SimpleContainer ui, ServerPlayer owner,
                              Villager villager, int page, int editedOffer, TradeDraft draft, boolean editor) {
        super(MenuType.GENERIC_9x6, syncId, inventory, ui, 6);
        this.ui = ui;
        this.owner = owner;
        this.villager = villager;
        this.page = page;
        this.editedOffer = editedOffer;
        this.draft = draft;
        this.editor = editor;
    }

    public static void open(ServerPlayer player, Villager villager) {
        openMain(player, villager, 0);
    }

    private static void openMain(ServerPlayer player, Villager villager, int page) {
        SimpleContainer ui = new SimpleContainer(54);
        MerchantOffers offers = villager.getOffers();
        int start = page * PAGE_SIZE;
        for (int index = start; index < Math.min(start + PAGE_SIZE, offers.size()); index++) {
            MerchantOffer offer = offers.get(index);
            ItemStack icon = offer.getResult().copy();
            icon.set(DataComponents.CUSTOM_NAME, Component.literal("Scambio " + (index + 1)
                    + " — " + offer.getBaseCostA().getCount() + "x "
                    + offer.getBaseCostA().getHoverName().getString()));
            ui.setItem(index - start, icon);
        }
        setButton(ui, 48, Items.EMERALD, "Crea nuovo scambio");
        if (page > 0) setButton(ui, 45, Items.ARROW, "Pagina precedente");
        if (start + PAGE_SIZE < offers.size()) setButton(ui, 53, Items.ARROW, "Pagina successiva");
        setButton(ui, 50, Items.BARRIER, "Chiudi menu");
        if (offers.isEmpty()) setButton(ui, 22, Items.BOOK, "Nessuno scambio: seleziona un oggetto dal tuo inventario per iniziare");

        openMenu(player, villager, page, -1, null, false, ui,
                Component.literal("Scambi villager — " + villager.getName().getString()));
    }

    private static void openEditor(ServerPlayer player, Villager villager, int page,
                                   int offerIndex, TradeDraft draft) {
        SimpleContainer ui = new SimpleContainer(54);
        refreshEditorItems(ui, draft, offerIndex);
        openMenu(player, villager, page, offerIndex, draft, true, ui,
                Component.literal(offerIndex < 0 ? "Crea scambio" : "Modifica scambio " + (offerIndex + 1)));
    }

    private static void openMenu(ServerPlayer player, Villager villager, int page, int offerIndex,
                                 TradeDraft draft, boolean editor, SimpleContainer ui, Component title) {
        MenuProvider provider = new MenuProvider() {
            @Override
            public Component getDisplayName() {
                return title;
            }

            @Override
            public VillagerTradeMenu createMenu(int syncId, Inventory inventory, Player menuPlayer) {
                return new VillagerTradeMenu(syncId, inventory, ui, player, villager,
                        page, offerIndex, draft, editor);
            }
        };
        player.openMenu(provider);
    }

    private static void refreshEditorItems(SimpleContainer ui, TradeDraft draft, int offerIndex) {
        setItemSelector(ui, SLOT_COST_A, draft.costA, "Costo principale — clicca con un oggetto in mano per sostituire");
        setButton(ui, 9, Items.PAPER, "Costo principale");
        setButton(ui, SLOT_COST_A_MINUS, Items.DYE.red(), "Riduci quantità costo principale");
        setButton(ui, SLOT_COST_A_PLUS, Items.DYE.lime(), "Aumenta quantità costo principale");

        setButton(ui, 13, Items.PAPER, "Costo secondario (facoltativo)");
        setItemSelector(ui, SLOT_COST_B, draft.costB, "Costo secondario — clicca con un oggetto in mano per sostituire");
        setButton(ui, SLOT_COST_B_MINUS, Items.DYE.red(), "Riduci quantità costo secondario");
        setButton(ui, SLOT_COST_B_PLUS, Items.DYE.lime(), "Aumenta quantità costo secondario");
        setButton(ui, SLOT_COST_B_CLEAR, Items.BARRIER, "Rimuovi costo secondario");

        setButton(ui, 19, Items.PAPER, "Oggetto ottenuto");
        setItemSelector(ui, SLOT_RESULT, draft.result, "Oggetto ottenuto — clicca con un oggetto in mano per sostituire");
        setButton(ui, SLOT_RESULT_MINUS, Items.DYE.red(), "Riduci quantità ottenuta");
        setButton(ui, SLOT_RESULT_PLUS, Items.DYE.lime(), "Aumenta quantità ottenuta");

        setButton(ui, SLOT_USES_MINUS, Items.DYE.red(), "Riduci usi massimi");
        setButton(ui, SLOT_USES_VALUE, Items.PAPER, "Usi massimi: " + draft.maxUses);
        setButton(ui, SLOT_USES_PLUS, Items.DYE.lime(), "Aumenta usi massimi");
        setButton(ui, SLOT_SAVE, Items.EMERALD, "Salva scambio");
        if (offerIndex >= 0) setButton(ui, SLOT_DELETE, Items.TNT, "Elimina scambio");
        setButton(ui, SLOT_BACK, Items.ARROW, "Torna agli scambi");
        setButton(ui, 45, Items.BOOK, "Per impostare un item: prendilo dal tuo inventario, poi clicca sul relativo slot");
    }

    private static void setItemSelector(SimpleContainer ui, int slot, ItemStack selected, String label) {
        if (selected.isEmpty()) {
            setButton(ui, slot, Items.STAINED_GLASS_PANE.pick(DyeColor.GRAY), label);
        } else {
            ui.setItem(slot, selected.copy());
        }
    }

    private static void setButton(SimpleContainer ui, int slot, Item item, String name) {
        ItemStack icon = new ItemStack(item);
        icon.set(DataComponents.CUSTOM_NAME, Component.literal(name));
        ui.setItem(slot, icon);
    }

    private static TradeDraft fromOffer(MerchantOffer offer) {
        TradeDraft draft = new TradeDraft();
        draft.costA = offer.getItemCostA().itemStack().copy();
        draft.costB = offer.getItemCostB().map(cost -> cost.itemStack().copy()).orElse(ItemStack.EMPTY);
        draft.result = offer.getResult().copy();
        draft.costACount = offer.getItemCostA().count();
        draft.costBCount = offer.getItemCostB().map(cost -> cost.count()).orElse(1);
        draft.resultCount = offer.getResult().getCount();
        draft.maxUses = offer.getMaxUses();
        draft.experience = offer.getXp();
        draft.priceMultiplier = offer.getPriceMultiplier();
        draft.uses = offer.getUses();
        return draft;
    }

    private void handleEditorClick(int slotId, int button, ContainerInput input, Player player) {
        if (slotId == SLOT_COST_A || slotId == SLOT_COST_B || slotId == SLOT_RESULT) {
            if (input == ContainerInput.PICKUP && !getCarried().isEmpty()) {
                ItemStack held = getCarried();
                if (slotId == SLOT_COST_A) {
                    draft.costA = held.copyWithCount(Math.max(1, draft.costACount));
                } else if (slotId == SLOT_COST_B) {
                    draft.costB = held.copyWithCount(Math.max(1, draft.costBCount));
                } else {
                    draft.result = held.copyWithCount(Math.max(1, draft.resultCount));
                }
                refreshEditor();
            }
            return;
        }

        if (input != ContainerInput.PICKUP) return;
        switch (slotId) {
            case SLOT_COST_A_MINUS -> draft.costACount = adjust(draft.costACount, -1, 1, 64);
            case SLOT_COST_A_PLUS -> draft.costACount = adjust(draft.costACount, 1, 1, 64);
            case SLOT_COST_B_MINUS -> {
                if (!draft.costB.isEmpty()) draft.costBCount = adjust(draft.costBCount, -1, 1, 64);
            }
            case SLOT_COST_B_PLUS -> {
                if (!draft.costB.isEmpty()) draft.costBCount = adjust(draft.costBCount, 1, 1, 64);
            }
            case SLOT_COST_B_CLEAR -> {
                draft.costB = ItemStack.EMPTY;
                draft.costBCount = 1;
            }
            case SLOT_RESULT_MINUS -> {
                if (!draft.result.isEmpty()) draft.resultCount = adjust(draft.resultCount, -1, 1, draft.result.getMaxStackSize());
            }
            case SLOT_RESULT_PLUS -> {
                if (!draft.result.isEmpty()) draft.resultCount = adjust(draft.resultCount, 1, 1, draft.result.getMaxStackSize());
            }
            case SLOT_USES_MINUS -> draft.maxUses = adjust(draft.maxUses, -1, 1, 999);
            case SLOT_USES_PLUS -> draft.maxUses = adjust(draft.maxUses, 1, 1, 999);
            case SLOT_SAVE -> saveOffer(player);
            case SLOT_DELETE -> deleteOffer(player);
            case SLOT_BACK -> openMain(owner, villager, page);
            default -> { return; }
        }
        refreshEditor();
    }

    private static int adjust(int current, int delta, int min, int max) {
        return Math.max(min, Math.min(max, current + delta));
    }

    private void saveOffer(Player player) {
        if (draft.costA.isEmpty() || draft.result.isEmpty()) {
            player.sendSystemMessage(Component.literal("Imposta almeno il costo principale e l'oggetto ottenuto."));
            return;
        }

        ItemCost costA = new ItemCost(draft.costA.getItem(), draft.costACount);
        Optional<ItemCost> costB = draft.costB.isEmpty()
                ? Optional.empty()
                : Optional.of(new ItemCost(draft.costB.getItem(), draft.costBCount));
        MerchantOffer offer = new MerchantOffer(costA, costB,
                draft.result.copyWithCount(draft.resultCount), draft.maxUses,
                draft.experience, draft.priceMultiplier);
        for (int i = 0; i < draft.uses && i < draft.maxUses; i++) offer.increaseUses();

        MerchantOffers offers = villager.getOffers().copy();
        if (editedOffer < 0) {
            offers.add(offer);
        } else if (editedOffer < offers.size()) {
            offers.set(editedOffer, offer);
        } else {
            player.sendSystemMessage(Component.literal("Questo scambio non esiste più."));
            openMain(owner, villager, page);
            return;
        }
        villager.setOffers(offers);
        villager.setPersistenceRequired();
        player.sendSystemMessage(Component.literal("Scambio salvato."));
        openMain(owner, villager, page);
    }

    private void deleteOffer(Player player) {
        if (editedOffer < 0) {
            openMain(owner, villager, page);
            return;
        }
        MerchantOffers offers = villager.getOffers().copy();
        if (editedOffer < offers.size()) {
            offers.remove(editedOffer);
            villager.setOffers(offers);
            villager.setPersistenceRequired();
            player.sendSystemMessage(Component.literal("Scambio eliminato."));
        }
        openMain(owner, villager, page);
    }

    private void refreshEditor() {
        refreshEditorItems(ui, draft, editedOffer);
        broadcastFullState();
    }

    @Override
    public void clicked(int slotId, int button, ContainerInput input, Player player) {
        if (player != owner || !stillValid(player)) return;
        if (editor) {
            if (slotId < 0 || slotId >= 54) {
                super.clicked(slotId, button, input, player);
                return;
            }
            handleEditorClick(slotId, button, input, player);
            return;
        }
        if (input != ContainerInput.PICKUP || slotId < 0 || slotId >= 54) return;

        if (slotId < PAGE_SIZE) {
            int offerIndex = page * PAGE_SIZE + slotId;
            MerchantOffers offers = villager.getOffers();
            if (offerIndex < offers.size()) {
                openEditor(owner, villager, page, offerIndex, fromOffer(offers.get(offerIndex)));
            }
            return;
        }
        if (slotId == 48) {
            openEditor(owner, villager, page, -1, new TradeDraft());
        } else if (slotId == 45 && page > 0) {
            openMain(owner, villager, page - 1);
        } else if (slotId == 53 && (page + 1) * PAGE_SIZE < villager.getOffers().size()) {
            openMain(owner, villager, page + 1);
        } else if (slotId == 50) {
            owner.closeContainer();
        }
    }

    @Override
    public ItemStack quickMoveStack(Player player, int index) {
        return ItemStack.EMPTY;
    }

    @Override
    public boolean stillValid(Player player) {
        return player == owner && villager.isAlive() && player.distanceToSqr(villager) <= 256.0;
    }

    @Override
    public void removed(Player player) {
        ui.clearContent();
        super.removed(player);
    }

    private static final class TradeDraft {
        private ItemStack costA = ItemStack.EMPTY;
        private ItemStack costB = ItemStack.EMPTY;
        private ItemStack result = ItemStack.EMPTY;
        private int costACount = 1;
        private int costBCount = 1;
        private int resultCount = 1;
        private int maxUses = 12;
        private int experience = 1;
        private int uses;
        private float priceMultiplier = 0.05F;
    }
}
