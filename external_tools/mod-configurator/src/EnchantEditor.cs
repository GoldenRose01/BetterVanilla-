using System.Text.Json.Nodes;
using Legacy = EldritchConfigTool.ConfigForm;

namespace ModConfigurator;
internal sealed class EnchantEditor : UserControl
{
    internal EnchantDocument? Document { get; private set; }
    internal readonly ListBox Entries = new() { Dock = DockStyle.Fill, IntegralHeight = false, SelectionMode = SelectionMode.MultiExtended, BorderStyle = BorderStyle.None, HorizontalScrollbar = true };
    readonly TextBox filter = new() { Dock = DockStyle.Top, PlaceholderText = "Cerca nome o ID…" };
    readonly ComboBox category = new() { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList };
    readonly Panel detail = new() { Dock = DockStyle.Fill, BackColor = Color.White };
    readonly Label summary = Theme.Label("Seleziona un incantesimo", 10);
    readonly Dictionary<string, Control> fields = new();
    bool loading;
    internal event Action<string>? Changed;
    internal EnchantEditor()
    {
        Dock = DockStyle.Fill; BackColor = Theme.Background;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34)); layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var side = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(12), BackColor = Color.White };
        side.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        side.RowStyles.Add(new RowStyle(SizeType.AutoSize)); side.RowStyles.Add(new RowStyle(SizeType.AutoSize)); side.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); side.RowStyles.Add(new RowStyle(SizeType.AutoSize)); side.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        category.Items.AddRange(["Tutte le categorie", "Armi", "Armature", "Archi", "Attrezzi", "Altro"]); category.SelectedIndex = 0;
        Theme.Combo(category);
        side.Controls.Add(filter, 0, 0); side.Controls.Add(category, 0, 1); side.Controls.Add(Entries, 0, 2); side.Controls.Add(summary, 0, 3);
        var batch = Theme.Button("Modifica selezionati…", BatchDialog); batch.Dock = DockStyle.Top; side.Controls.Add(batch, 0, 4);
        layout.Controls.Add(side, 0, 0); layout.Controls.Add(detail, 1, 0); Controls.Add(layout);
        filter.TextChanged += (_, _) => RefreshEntries(); category.SelectedIndexChanged += (_, _) => RefreshEntries();
        Entries.SelectedIndexChanged += (_, _) => { if (!loading) BuildDetails(); };
    }
    internal void LoadDocument(string text, string kind)
    {
        string? previous = (Entries.SelectedItem as EnchantChoice)?.Id;
        Document = new EnchantDocument(text, kind); RefreshEntries(previous);
    }
    void RefreshEntries(string? previous = null)
    {
        if (Document == null) return;
        previous ??= (Entries.SelectedItem as EnchantChoice)?.Id;
        loading = true; Entries.BeginUpdate(); Entries.Items.Clear();
        foreach (string id in Document.Entries.Keys.OrderBy(id => AssetCatalog.Name(id))) {
            if (!(id + " " + AssetCatalog.Name(id)).Contains(filter.Text, StringComparison.OrdinalIgnoreCase)) continue;
            if (category.SelectedIndex > 0 && Legacy.Category(id, string.Join(' ', Document.Strings(id, "supportedItems"))) != (string)category.SelectedItem!) continue;
            Entries.Items.Add(new EnchantChoice(id));
        }
        for (int i = 0; i < Entries.Items.Count; i++) if (((EnchantChoice)Entries.Items[i]).Id == previous) Entries.SelectedIndex = i;
        if (Entries.SelectedIndex < 0 && Entries.Items.Count > 0) Entries.SelectedIndex = 0;
        Entries.EndUpdate(); loading = false; BuildDetails();
    }
    internal string? SelectedId => (Entries.SelectedItem as EnchantChoice)?.Id;
    internal void SetField(string key, JsonNode? value)
    {
        if (loading || Document == null || SelectedId == null) return;
        if (JsonNode.DeepEquals(Document.Get(SelectedId, key), value)) return;
        Document.Set(SelectedId, key, value); Changed?.Invoke(Document.Serialize());
    }
    void BuildDetails()
    {
        loading = true;
        foreach (Control control in detail.Controls.Cast<Control>().ToArray()) control.Dispose();
        detail.Controls.Clear(); fields.Clear();
        summary.Text = $"{Entries.Items.Count} visibili · {Entries.SelectedItems.Count} selezionati";
        var body = Theme.Stack(); body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); detail.Controls.Add(body);
        if (Document == null || SelectedId is not string id) { Theme.Add(body, Theme.Label("Nessun incantesimo da mostrare.", 12, true)); loading = false; return; }
        body.Padding = new Padding(16, 6, 16, 4);
        Theme.Add(body, Theme.Label(AssetCatalog.Name(id), 15, true)); var idLabel = Theme.Label(id, 9); idLabel.ForeColor = Theme.Muted; Theme.Add(body, idLabel);
        if (Document.Kind != "details") Bool(body, "enabled", "Incantesimo attivo", true);
        if (Document.Kind != "available") {
            var tabs = new TabControl { Dock = DockStyle.Fill };
            var rules = new TabPage("Regole e livelli") { BackColor = Color.White, AutoScroll = true };
            var items = new TabPage(Document.Kind == "details" ? "Categorie e compatibilità" : "Oggetti e compatibilità") { BackColor = Color.White, AutoScroll = true };
            var rulesBody = Theme.Stack(); rulesBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); rules.Controls.Add(rulesBody);
            var itemsBody = Theme.Stack(); itemsBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); items.Controls.Add(itemsBody);
            Theme.Add(rulesBody, Theme.Label("Livelli massimi", 12, true));
            Number(rulesBody, "anvilMaxLevel", Document.Kind == "eldritch" ? "Incudine · 0 = valore della mod" : "Livello massimo", Document.Kind == "details" ? 1 : 0, 255, Document.Kind == "details" ? 1 : 0);
            if (Document.Kind == "eldritch") Number(rulesBody, "enchantingTableMaxLevel", "Tavolo · 0 = valore della mod", 0, 255, 0);
            Theme.Add(rulesBody, Theme.Label("Dove si può ottenere", 12, true));
            Bool(rulesBody, "normalTable", "Tavolo degli incantesimi", Document.Kind == "details");
            Bool(rulesBody, "advancedTable", "Tavolo avanzato", Document.Kind == "details");
            if (Document.Kind == "eldritch") Bool(rulesBody, "loot", "Bottino");
            Number(rulesBody, "normalTableChance", "Probabilità al tavolo (%)", 0, 100, 100);
            Number(rulesBody, "advancedTableChance", "Probabilità al tavolo avanzato (%)", 0, 100, 100);
            if (Document.Kind == "eldritch") {
                var presets = Theme.Flow();
                presets.Controls.Add(Theme.Button("Solo tavolo", () => Preset(true, false, false)));
                presets.Controls.Add(Theme.Button("Solo avanzato", () => Preset(false, true, false)));
                presets.Controls.Add(Theme.Button("Solo bottino", () => Preset(false, false, true)));
                presets.Controls.Add(Theme.Button("Tutti", () => Preset(true, true, true))); Theme.Add(rulesBody, presets);
                Number(rulesBody, "rarity", "Peso di rarità · più alto = più frequente", 0, int.MaxValue, 100);
                var rarityPresets = Theme.Flow(); foreach (var r in Legacy.Rarities) { var option = r; rarityPresets.Controls.Add(Theme.Button(option.Label.Split('(')[0].Trim(), () => { SetField("rarity", JsonValue.Create(option.Value)); BuildDetails(); })); } Theme.Add(rulesBody, rarityPresets);
            }
            Theme.Add(rulesBody, Theme.Label("Classe di rarità"));
            var rarity = new ComboBox { Width = 240, DropDownStyle = ComboBoxStyle.DropDown };
            rarity.Items.AddRange(["common", "uncommon", "rare", "very_rare"]); rarity.Text = (string?)Document.Get(id, "rarityClass") ?? "common";
            string initialRarity = rarity.Text;
            rarity.Validated += (_, _) => { if (!loading && rarity.Text != initialRarity) { SetField("rarityClass", JsonValue.Create(rarity.Text)); initialRarity = rarity.Text; } }; Theme.Add(rulesBody, rarity);
            ListField(itemsBody, "supportedItems", Document.Kind == "details" ? "Categorie di oggetti" : "Oggetti e gruppi ammessi", false);
            ListField(itemsBody, "incompatibleEnchantments", "Incantesimi incompatibili", true);
            if (Document.Kind == "details") {
                Bool(rulesBody, "villager_books", "Libri dai villager", true); Bool(rulesBody, "villager_items", "Oggetti dai villager", true);
                ListField(itemsBody, "mob_category", "Categorie mob (una per riga)", false, false);
            }
            tabs.TabPages.AddRange([rules, items]); detail.Controls.Add(tabs); detail.Controls.SetChildIndex(tabs, 0);
        }
        loading = false;
    }
    void Bool(TableLayoutPanel body, string key, string label, bool fallback = false)
    {
        var c = new CheckBox { Text = label, AutoSize = true, Checked = (bool?)Document!.Get(SelectedId!, key) ?? fallback, Margin = new Padding(0, 6, 0, 6) };
        c.CheckedChanged += (_, _) => SetField(key, JsonValue.Create(c.Checked)); fields[key] = c; Theme.Add(body, c);
    }
    void Number(TableLayoutPanel body, string key, string label, int min, int max, int fallback)
    {
        var row = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2 };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        row.Controls.Add(Theme.Label(label), 0, 0);
        decimal value = (int?)Document!.Get(SelectedId!, key) ?? fallback;
        var c = new NumericUpDown { Minimum = Math.Min(min, value), Maximum = Math.Max(max, value), Value = value, Width = 100, ThousandsSeparator = true };
        c.ValueChanged += (_, _) => SetField(key, JsonValue.Create((int)c.Value)); fields[key] = c; row.Controls.Add(c, 1, 0); Theme.Add(body, row);
    }
    void Preset(bool normal, bool advanced, bool loot) { SetField("normalTable", JsonValue.Create(normal)); SetField("advancedTable", JsonValue.Create(advanced)); SetField("loot", JsonValue.Create(loot)); BuildDetails(); }
    void ListField(TableLayoutPanel body, string key, string label, bool enchantments, bool picker = true)
    {
        Theme.Add(body, Theme.Label(label, 12, true));
        var box = new TextBox { Multiline = true, ScrollBars = ScrollBars.Vertical, Height = 104, Dock = DockStyle.Top, Text = string.Join(Environment.NewLine, Document!.Strings(SelectedId!, key)), PlaceholderText = "Un ID per riga; lascia vuoto per usare le regole della mod." };
        box.TextChanged += (_, _) => SetField(key, new JsonArray(ParseList(box.Text).Select(s => (JsonNode?)JsonValue.Create(s)).ToArray())); Theme.Add(body, box);
        if (picker) Theme.Add(body, Theme.Button(enchantments ? "Scegli incantesimi…" : "Sfoglia oggetti e gruppi…", () => {
            using var dialog = new ItemPicker(enchantments ? Document!.Entries.Keys.Where(i => i != SelectedId).ToArray() : null, ParseList(box.Text));
            if (dialog.ShowDialog(this) == DialogResult.OK) box.Text = string.Join(Environment.NewLine, dialog.Result);
        }));
    }
    internal static string[] ParseList(string text) => text.Split(['\r','\n',',',';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct(StringComparer.Ordinal).ToArray();
    void BatchDialog()
    {
        if (Document == null || Entries.SelectedItems.Count == 0) return;
        string[] targets = Entries.SelectedItems.Cast<EnchantChoice>().Select(x => x.Id).ToArray();
        using var dialog = new Form { Text = "Modifica più incantesimi", Size = new Size(650, 500), StartPosition = FormStartPosition.CenterParent, BackColor = Theme.Background, Font = Font, MinimizeBox = false, MaximizeBox = false };
        var body = Theme.Stack(); body.Dock = DockStyle.Fill; body.AutoScroll = true; dialog.Controls.Add(body);
        Theme.Add(body, Theme.Label($"{targets.Length} incantesimi selezionati", 17, true));
        Theme.Add(body, Theme.Label("L'operazione cambia solo il campo scelto. Poi premi Salva."));
        var operation = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 470 };
        operation.Items.AddRange(Document.Kind == "available" ? ["Attiva", "Disattiva"] : Document.Kind == "details" ? ["Aggiungi gruppo", "Rimuovi gruppo", "Abilita tavolo", "Disabilita tavolo", "Abilita tavolo avanzato", "Disabilita tavolo avanzato"] : ["Aggiungi gruppo", "Rimuovi gruppo", "Attiva", "Disattiva", "Abilita tavolo", "Disabilita tavolo", "Abilita tavolo avanzato", "Disabilita tavolo avanzato", "Abilita bottino", "Disabilita bottino", "Imposta peso di rarità"]);
        operation.SelectedIndex = 0; Theme.Add(body, operation);
        var groups = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 470 }; groups.Items.AddRange(Legacy.Items); groups.SelectedIndex = 0; Theme.Add(body, groups);
        var weight = new NumericUpDown { Minimum = 0, Maximum = int.MaxValue, Value = 100, Width = 180 }; Theme.Add(body, weight);
        void State() { groups.Visible = operation.Text.Contains("gruppo"); weight.Visible = operation.Text.Contains("rarità"); } operation.SelectedIndexChanged += (_, _) => State(); State();
        var preview = new TextBox { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Width = 480, Height = 100, Text = string.Join(Environment.NewLine, targets) }; Theme.Add(body, preview);
        Theme.Add(body, Theme.Button("Applica all'anteprima", () => {
            string op = operation.Text;
            if (op.Contains("gruppo")) Document.ChangeGroup(targets, ((EldritchConfigTool.ItemChoice)groups.SelectedItem!).Value, op.StartsWith("Aggiungi"));
            else {
                string key = op.Contains("rarità") ? "rarity" : op.Contains("avanzato") ? "advancedTable" : op.Contains("tavolo") ? "normalTable" : op.Contains("bottino") ? "loot" : "enabled";
                JsonNode value = key == "rarity" ? JsonValue.Create((int)weight.Value)! : JsonValue.Create(op.StartsWith("Abilita") || op == "Attiva")!;
                foreach (var id in targets) Document.Set(id, key, value);
            }
            Changed?.Invoke(Document.Serialize()); dialog.DialogResult = DialogResult.OK;
        }, true));
        if (dialog.ShowDialog(this) == DialogResult.OK) BuildDetails();
    }
}
