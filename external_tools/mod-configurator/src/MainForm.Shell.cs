using System.Text.Json.Nodes;
namespace ModConfigurator;
internal sealed partial class MainForm
{
    readonly Panel guided = new() { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White };
    readonly Label documentTitle = Theme.Label("Le tue mod, in un unico posto", 16, true);
    readonly Label documentState = Theme.Label("Nessun file aperto", 9);
    readonly Label destinationHint = Theme.Label("", 9);
    readonly ComboBox modFilter = new() { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList };
    internal EnchantEditor? enchantEditor;
    bool useWorld;
    void BuildShell()
    {
        Text = "GoldenRose • Configuratore mod"; Font = new Font("Segoe UI", 9); BackColor = Theme.Background; ForeColor = Theme.Ink;
        AutoScaleMode = AutoScaleMode.Dpi; ClientSize = new Size(1400, 920); MinimumSize = new Size(1060, 740); StartPosition = FormStartPosition.CenterScreen;
        documentTitle.Text = "Le tue mod, in un unico posto"; documentState.Text = "Nessun file aperto";
        mode.Items.AddRange(["Predefiniti delle mod", "Installazione / mondo"]); mode.SelectedIndex = 0; mode.Width = 280; Theme.Combo(mode);
        var page = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Padding = new Padding(18) };
        page.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 275)); page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        page.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); page.RowStyles.Add(new RowStyle(SizeType.AutoSize)); Controls.Add(page);
        var sidebar = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(24, 39, 57), Padding = new Padding(18), ColumnCount = 1, RowCount = 7 };
        sidebar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < 7; i++) sidebar.RowStyles.Add(new RowStyle(i == 4 ? SizeType.Percent : SizeType.AutoSize, i == 4 ? 100 : 0));
        var brand = Theme.Label("GOLDENROSE", 13, true); brand.ForeColor = Color.White; sidebar.Controls.Add(brand, 0, 0);
        var caption = Theme.Label("Configuratore mod", 10); caption.ForeColor = Color.FromArgb(169, 189, 211); sidebar.Controls.Add(caption, 0, 1);
        modFilter.Items.AddRange(["Tutte le mod", "BetterVanilla+", "Eldritch Surge", "EnchLib"]); modFilter.SelectedIndex = 0; sidebar.Controls.Add(modFilter, 0, 2);
        Theme.Combo(modFilter);
        search.Margin = new Padding(0, 14, 0, 10); sidebar.Controls.Add(search, 0, 3);
        files.BackColor = sidebar.BackColor; files.ForeColor = Color.White; files.BorderStyle = BorderStyle.None; files.IntegralHeight = false; files.HorizontalScrollbar = true;
        files.FormattingEnabled = true; files.Format += (_, e) => { if (e.ListItem is ConfigFile f) e.Value = FriendlyName(f); }; sidebar.Controls.Add(files, 0, 4);
        var open = Theme.Button("Apri configurazione", () => Guard(OpenSelected), true); open.Dock = DockStyle.Fill; sidebar.Controls.Add(open, 0, 5);
        var help = Theme.Label("1  Scegli la destinazione\n2  Apri una configurazione\n3  Modifica e salva", 9); help.ForeColor = caption.ForeColor; sidebar.Controls.Add(help, 0, 6); page.Controls.Add(sidebar, 0, 0);
        var workspace = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(20, 0, 0, 0) };
        workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < 5; i++) workspace.RowStyles.Add(new RowStyle(i == 3 ? SizeType.Percent : SizeType.AutoSize, i == 3 ? 100 : 0));
        var top = Theme.Flow(); top.Controls.Add(mode); top.Controls.Add(Theme.Button("Scegli cartelle…", DestinationDialog)); top.Controls.Add(Theme.Button("Importa vecchio config…", ImportLegacy)); workspace.Controls.Add(top, 0, 0);
        var heading = Theme.Stack(); heading.Padding = new Padding(12, 4, 12, 4);
        var headingLine = Theme.Flow(); headingLine.Controls.Add(documentTitle); headingLine.Controls.Add(documentState);
        Theme.Add(heading, headingLine); Theme.Add(heading, destinationHint); workspace.Controls.Add(heading, 0, 1);
        pathLabel.Height = 36; pathLabel.ForeColor = Theme.Muted; pathLabel.Text = "La destinazione esatta del salvataggio apparirà qui."; workspace.Controls.Add(pathLabel, 0, 2);
        var guideTab = new TabPage("Editor guidato"); guideTab.Controls.Add(guided);
        var gridTab = new TabPage("Tabella avanzata"); gridTab.Controls.Add(grid); var rawTab = new TabPage("Testo completo"); rawTab.Controls.Add(raw);
        tabs.TabPages.AddRange([guideTab, gridTab, rawTab]); workspace.Controls.Add(tabs, 0, 3);
        grid.Columns.Add("key", "Impostazione / ID"); grid.Columns[0].ReadOnly = true; grid.Columns.Add("value", "Valore"); Theme.Grid(grid);
        var actions = Theme.Flow();
        actions.Controls.Add(Theme.Button("Salva modifiche", () => Guard(SaveCurrent), true));
        actions.Controls.Add(Theme.Button("Ripristina predefiniti", () => Guard(LoadDefaults)));
        actions.Controls.Add(Theme.Button("Controlla", () => Guard(() => { RequireCurrent(); ValidateChildren(); CommitGrid(); Storage.Validate(current!, raw.Text); status.Text = "✓ Configurazione valida. Nessun file è stato modificato."; })));
        actions.Controls.Add(Theme.Button("Apri cartella", () => Guard(() => { RequireCurrent(); string folder = Path.GetDirectoryName(path!)!; if (!Directory.Exists(folder)) { status.Text = "La cartella verrà creata al primo salvataggio."; return; } System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("explorer.exe", folder) { UseShellExecute = true }); })));
        workspace.Controls.Add(actions, 0, 4); page.Controls.Add(workspace, 1, 0);
        status.AutoSize = true; status.Dock = DockStyle.Fill; status.MaximumSize = new Size(1300, 0); status.ForeColor = Theme.Muted; status.Text = "I file esistenti vengono salvati con backup automatico."; page.Controls.Add(status, 0, 1); page.SetColumnSpan(status, 2);
        modFilter.SelectedIndexChanged += (_, _) => RefreshList();
        mode.SelectedIndexChanged += (_, _) => destinationHint.Text = "Destinazione cambiata. Premi Apri configurazione per caricare il file in questa modalità.";
        var welcome = Theme.Stack(); Theme.Add(welcome, Theme.Label("Inizia dalla tua configurazione", 22, true)); Theme.Add(welcome, Theme.Label("Scegli una mod a sinistra, imposta le cartelle e apri il file.", 12));
        Theme.Add(welcome, Theme.Label("Eldritch Surge e EnchLib", 14, true)); Theme.Add(welcome, Theme.Label("Editor per incantesimo, categorie, oggetti, incompatibilità e modifiche multiple."));
        Theme.Add(welcome, Theme.Label("BetterVanilla+", 14, true)); Theme.Add(welcome, Theme.Label("Controlli semplici per ascensori, particelle, beacon e calderoni.")); guided.Controls.Add(welcome);
    }
    internal static string FriendlyName(ConfigFile f) => f.Mod == "BetterVanilla+" ? "BetterVanilla+ · Generale" : f.Mod == "Eldritch Surge" ? "Eldritch · Incantesimi" : f.Name switch {
        "AviableEnch.json5" => "EnchLib · Attivazione", "EnchantmentsDetails.json5" => "EnchLib · Dettagli incantesimi", "EnchLVLmax.config" => "Preset · Livelli massimi", "MobCategory.config" => "Preset · Categorie mob", "EnchCategories.config" => "Preset · Categorie incantesimi", "EnchUNcompatibility.config" => "Preset · Incompatibilità", _ => f.Name };
    void DestinationDialog()
    {
        using var dialog = new Form { Text = "Dove vuoi salvare le configurazioni?", Size = new Size(850, 530), MinimumSize = new Size(700, 460), StartPosition = FormStartPosition.CenterParent, Font = Font, BackColor = Theme.Background };
        var body = Theme.Stack(); body.Dock = DockStyle.Fill; body.AutoScroll = true; body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); dialog.Controls.Add(body);
        Theme.Add(body, Theme.Label(mode.SelectedIndex == 0 ? "Predefiniti delle prossime compilazioni" : "Configurazione dell'installazione e del mondo", 17, true));
        var projectCopy = new TextBox { Text = projects.Text, Dock = DockStyle.Fill }; var instanceCopy = new TextBox { Text = instance.Text, Dock = DockStyle.Fill }; var worldCopy = new TextBox { Text = world.Text, Dock = DockStyle.Fill };
        void Folder(string label, TextBox box) { Theme.Add(body, Theme.Label(label)); var row = new TableLayoutPanel { ColumnCount = 2, Dock = DockStyle.Top, AutoSize = true }; row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); row.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); row.Controls.Add(box); row.Controls.Add(Theme.Button("Sfoglia…", () => { using var picker = new FolderBrowserDialog { InitialDirectory = Directory.Exists(box.Text) ? box.Text : "", Description = label, UseDescriptionForTitle = true }; if (picker.ShowDialog(dialog) == DialogResult.OK) box.Text = picker.SelectedPath; })); Theme.Add(body, row); }
        if (mode.SelectedIndex == 0) Folder("Cartella che contiene i tre repository", projectCopy);
        else { Folder("Cartella Minecraft / server (contiene mods e config)", instanceCopy); Folder("Cartella del mondo (contiene level.dat) · obbligatoria per EnchLib", worldCopy); }
        var worldCheck = new CheckBox { Text = "Eldritch Surge: modifica questo mondo invece dei valori globali", AutoSize = true, Checked = useWorld, Visible = mode.SelectedIndex == 1 }; Theme.Add(body, worldCheck);
        Theme.Add(body, Theme.Label("BetterVanilla+ usa l'installazione. EnchLib usa sempre il mondo.\nLe modifiche ai predefiniti sorgenti richiedono nuove compilazioni."));
        Theme.Add(body, Theme.Button("Usa queste cartelle", () => { projects.Text = projectCopy.Text; instance.Text = instanceCopy.Text; world.Text = worldCopy.Text; useWorld = worldCheck.Checked; destinationHint.Text = "Cartelle aggiornate. Apri una configurazione per usarle."; dialog.DialogResult = DialogResult.OK; }, true));
        dialog.ShowDialog(this);
    }
    void ImportLegacy() => Guard(() => {
        RequireCurrent(); if (current!.Mod != "Eldritch Surge") throw new InvalidOperationException("Apri prima la configurazione Incantesimi di Eldritch Surge.");
        using var picker = new OpenFileDialog { Title = "Importa il file del configuratore Eldritch precedente", Filter = "Configurazione JSON|*.json", InitialDirectory = Path.Combine(projects.Text, "Eldrich-Surge", "run", "config", "eldrich-surge") };
        if (picker.ShowDialog(this) != DialogResult.OK) return;
        ValidateChildren(); CommitGrid();
        string merged = LegacyImport.Merge(raw.Text, File.ReadAllText(picker.FileName));
        Storage.Validate(current, merged); raw.Text = merged; BuildGrid(); BuildGuided();
        status.Text = "Importazione in anteprima: le voci del vecchio file sono unite alle impostazioni correnti. Premi Salva per confermare.";
    });
    void BuildGuided()
    {
        if (current == null) return;
        enchantEditor = null;
        foreach (Control c in guided.Controls.Cast<Control>().ToArray()) c.Dispose(); guided.Controls.Clear();
        string? kind = current.Mod == "Eldritch Surge" ? "eldritch" : current.Name == "EnchantmentsDetails.json5" ? "details" : current.Name == "AviableEnch.json5" ? "available" : null;
        if (kind != null) {
            try { var editor = new EnchantEditor(); editor.LoadDocument(raw.Text, kind); editor.Changed += text => raw.Text = text; enchantEditor = editor; guided.Controls.Add(editor); }
            catch (Exception e) { var note = Theme.Stack(); Theme.Add(note, Theme.Label("Questo file richiede una correzione nel testo completo.", 12, true)); Theme.Add(note, Theme.Label(e.Message)); guided.Controls.Add(note); }
            return;
        }
        var body = Theme.Stack(); body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); guided.Controls.Add(body);
        if (current.Mod != "BetterVanilla+") {
            Theme.Add(body, Theme.Label(FriendlyName(current), 18, true));
            Theme.Add(body, Theme.Label("Questi preset usano coppie ID / valore. I commenti originali vengono conservati."));
            Theme.Add(body, Theme.Button("Modifica nella tabella", () => tabs.SelectedIndex = 1, true));
            Theme.Add(body, Theme.Label("Per aggiungere righe usa Testo completo.\nI preset non sostituiscono le impostazioni già salvate nei mondi."));
            return;
        }
        JsonObject values;
        try { values = JsonNode.Parse(raw.Text, documentOptions: Storage.JsonOptions)!.AsObject(); }
        catch { Theme.Add(body, Theme.Label("JSON non valido: correggi il file in Testo completo.")); return; }
        foreach (var (key, node) in values) {
            var (label, hint, min, max) = BetterField(key);
            Theme.Add(body, Theme.Label(label, 12, true));
            if (hint.Length > 0) { var help = Theme.Label(hint, 9); help.ForeColor = Theme.Muted; Theme.Add(body, help); }
            if (node is JsonValue value && value.TryGetValue<bool>(out bool enabled)) {
                var check = new CheckBox { Text = "Attivo", AutoSize = true, Checked = enabled }; check.CheckedChanged += (_, _) => { values[key] = check.Checked; raw.Text = values.ToJsonString(Storage.Pretty); }; Theme.Add(body, check);
            } else if (node is JsonValue number && number.TryGetValue<int>(out int n)) {
                var input = new NumericUpDown { Width = 180, Minimum = Math.Min(min, n), Maximum = Math.Max(max, n), Value = n }; input.ValueChanged += (_, _) => { values[key] = (int)input.Value; raw.Text = values.ToJsonString(Storage.Pretty); }; Theme.Add(body, input);
            }
        }
    }
    static (string Label, string Hint, int Min, int Max) BetterField(string key) => key switch {
        "elevatorRange" => ("Distanza massima degli ascensori", "Blocchi in verticale entro cui cercare la destinazione.", 1, 4096),
        "elevatorCooldownTicks" => ("Pausa tra i teletrasporti", "Tick di gioco: 20 tick corrispondono a un secondo.", 0, 1200),
        "particleRadius" => ("Distanza delle particelle", "Raggio in blocchi. 0 disattiva la scansione a distanza.", 0, 32),
        "particleIntervalTicks" => ("Intervallo delle particelle", "Tick fra una scansione e la successiva.", 1, 1200),
        "particlesPerElevator" => ("Particelle per ascensore", "0 nasconde le particelle.", 0, 32),
        "spawnProtectionRadius" => ("Protezione dallo spawn", "Raggio in blocchi attorno agli ascensori; 0 disattiva la protezione.", 0, 8),
        "beaconEnabled" => ("Bonus della mela sul beacon", "Abilita il bonus saturazione.", 0, 1),
        "beaconRequiredLevel" => ("Livello richiesto del beacon", "Dimensione minima della piramide, da 1 a 4.", 1, 4),
        "beaconCooldownTicks" => ("Pausa tra le mele", "Tick prima di consumare un'altra mela.", 1, 72000),
        "beaconRange" => ("Raggio del bonus beacon", "Distanza massima dei giocatori in blocchi.", 1, 512),
        "saturationAmplifier" => ("Potenza della saturazione", "0 = livello I, 4 = livello V.", 0, 255),
        "cauldronEnabled" => ("Conversioni nei calderoni", "Consenti la conversione di terra e polvere di cemento.", 0, 1),
        _ => (key, "", 0, int.MaxValue) };
}
internal static class LegacyImport
{
    internal static string Merge(string current, string legacy)
    {
        var target = JsonNode.Parse(current, documentOptions: Storage.JsonOptions)!.AsObject();
        var imported = JsonNode.Parse(legacy, documentOptions: Storage.JsonOptions)!.AsObject();
        if (imported["enchantments"] is not JsonObject entries) throw new FormatException("Il file non contiene un elenco enchantments.");
        if (target["enchantments"] is not JsonObject targetEntries) throw new FormatException("Configurazione corrente non valida.");
        foreach (var (id, entry) in entries) {
            if (entry is not JsonObject fields) throw new FormatException("Voce non valida: " + id);
            if (targetEntries[id] is not JsonObject existing) targetEntries[id] = existing = new JsonObject();
            foreach (var (key, value) in fields) existing[key] = value?.DeepClone();
        }
        // Old tools wrote v2 and omitted newer fields. Retain the current root and all untouched fields.
        target["configVersion"] = 4;
        return target.ToJsonString(Storage.Pretty);
    }
}
