using System.Text.Json;
using System.Text.Json.Nodes;

namespace ModConfigurator;
internal static class Program
{
    [STAThread] static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        if (args.Contains("--self-test")) { Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException); SelfTest.Run(args.Last()); return; }
        Application.Run(new MainForm());
    }
}
internal sealed partial class MainForm : Form
{
    readonly ComboBox mode = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 300 };
    readonly TextBox projects = new() { Dock = DockStyle.Fill };
    readonly TextBox instance = new() { Dock = DockStyle.Fill };
    readonly TextBox world = new() { Dock = DockStyle.Fill };
    readonly TextBox search = new() { PlaceholderText = "Cerca file o mod…", Dock = DockStyle.Top };
    readonly ListBox files = new() { Dock = DockStyle.Fill };
    readonly TextBox raw = new() { Multiline = true, AcceptsTab = true, AcceptsReturn = true, ScrollBars = ScrollBars.Both, WordWrap = false, Dock = DockStyle.Fill, Font = new Font("Consolas", 11) };
    readonly DataGridView grid = new() { Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false };
    readonly TabControl tabs = new() { Dock = DockStyle.Fill };
    readonly Label pathLabel = new() { Dock = DockStyle.Top, Height = 48, AutoEllipsis = true, Padding = new Padding(5) };
    readonly Label status = new() { Dock = DockStyle.Bottom, Height = 44, Padding = new Padding(8) };
    readonly List<ConfigFile> catalog = Storage.Catalog();
    string? original, path;
    ConfigFile? current;
    bool dirty, updating;
    JsonNode? tree;
    readonly string settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GoldenRoseModConfigurator", "paths.json");

    public MainForm(bool persistPaths = true)
    {
        BuildShell();
        string? parent = FindProjects(); projects.Text = parent ?? "";
        instance.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft");
        if (persistPaths && File.Exists(settingsPath)) try { var settings = JsonNode.Parse(File.ReadAllText(settingsPath)); projects.Text = (string?)settings?["projects"] ?? projects.Text; instance.Text = (string?)settings?["instance"] ?? instance.Text; world.Text = (string?)settings?["world"] ?? ""; useWorld = (bool?)settings?["useWorld"] ?? false; } catch { }
        RefreshList();
        destinationHint.Text = "Scegli un file e premi Apri configurazione.";
        search.TextChanged += (_, _) => RefreshList();
        files.DoubleClick += (_, _) => Guard(OpenSelected);
        raw.TextChanged += (_, _) => { if (!updating) { dirty = true; UpdateTitle(); } };
        grid.CellEndEdit += (_, _) => Guard(ApplyGrid);
        tabs.SelectedIndexChanged += (_, _) => { if (tabs.SelectedIndex == 1) Guard(BuildGrid); else if (tabs.SelectedIndex == 0) Guard(BuildGuided); };
        FormClosing += (_, e) => { if (!CanDiscard()) { e.Cancel = true; return; } if (!persistPaths) return; try { Directory.CreateDirectory(Path.GetDirectoryName(settingsPath)!); File.WriteAllText(settingsPath, new JsonObject { ["projects"] = projects.Text, ["instance"] = instance.Text, ["world"] = world.Text, ["useWorld"] = useWorld }.ToJsonString()); } catch { } };
    }
    void AddFolder(TableLayoutPanel panel, int row, string label, TextBox box)
    {
        panel.Controls.Add(new Label { Text = label, AutoSize = true }, 0, row); panel.Controls.Add(box, 1, row);
        var button = new Button { Text = "Sfoglia…", AutoSize = true }; button.Click += (_, _) => { using var dialog = new FolderBrowserDialog { InitialDirectory = Directory.Exists(box.Text) ? box.Text : "", Description = label, UseDescriptionForTitle = true }; if (dialog.ShowDialog() == DialogResult.OK) box.Text = dialog.SelectedPath; }; panel.Controls.Add(button, 2, row);
    }
    void AddButton(Control panel, string label, Action action) { var button = new Button { Text = label, AutoSize = true }; button.Click += (_, _) => Guard(action); panel.Controls.Add(button); }
    void Guard(Action action) { try { action(); } catch (Exception error) { MessageBox.Show(this, error.Message, "Operazione non completata", MessageBoxButtons.OK, MessageBoxIcon.Warning); } }
    void RequireCurrent() { if (current == null || path == null) throw new InvalidOperationException("Apri prima un file dall'elenco."); }
    void RefreshList() { var selected = files.SelectedItem; files.Items.Clear(); foreach (var f in catalog.Where(f => (f.ToString() + " " + FriendlyName(f)).Contains(search.Text, StringComparison.OrdinalIgnoreCase) && (modFilter.SelectedIndex == 0 || f.Mod == modFilter.Text))) files.Items.Add(f); if (selected != null && files.Items.Contains(selected)) files.SelectedItem = selected; }
    static string? FindProjects() { for (var d = new DirectoryInfo(AppContext.BaseDirectory); d != null; d = d.Parent) if (Directory.Exists(Path.Combine(d.FullName, "BetterVanilla+", "src"))) return d.FullName; return null; }
    bool CanDiscard() => !dirty || MessageBox.Show(this, "Ci sono modifiche non salvate. Vuoi scartarle?", "Modifiche in sospeso", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    void OpenSelected()
    {
        if (files.SelectedItem is not ConfigFile selected) throw new InvalidOperationException("Seleziona un file dall'elenco.");
        if (!CanDiscard()) return;
        string resolved;
        if (mode.SelectedIndex == 0)
        {
            if (!Directory.Exists(Path.Combine(projects.Text, selected.Source.Split('/')[0], "src", "main", "resources"))) throw new IOException("Seleziona la cartella che contiene i repository BetterVanilla+, enchlib ed Eldrich-Surge.");
            resolved = Path.GetFullPath(Path.Combine(projects.Text, selected.Source));
        }
        else if (selected.NeedsWorld || (selected.Mod == "Eldritch Surge" && useWorld))
        {
            if (!Directory.Exists(world.Text) || !File.Exists(Path.Combine(world.Text, "level.dat"))) throw new IOException("Seleziona un mondo Minecraft valido (cartella contenente level.dat). EnchLib usa configurazioni per mondo.");
            resolved = Path.GetFullPath(Path.Combine(world.Text, selected.Mod == "Eldritch Surge" ? "eldrich-surge/" + selected.Name : selected.Instance));
        }
        else
        {
            if (!Directory.Exists(instance.Text)) throw new IOException("Seleziona la cartella dell'installazione Minecraft o del server.");
            resolved = Path.GetFullPath(Path.Combine(instance.Text, selected.Instance));
        }
        string? disk = File.Exists(resolved) ? File.ReadAllText(resolved) : null;
        current = selected; path = resolved; original = disk;
        SetText(disk ?? Storage.Template(selected)); dirty = false; UpdateTitle();
        pathLabel.Text = (mode.SelectedIndex == 0 ? "PREDEFINITI SORGENTI — " : "INSTALLAZIONE / MONDO — ") + path;
        status.Text = disk == null ? "File non presente: anteprima dei predefiniti. Premi Salva / crea file per crearlo." : "File caricato. Il salvataggio conserva una copia di backup accanto all'originale.";
        BuildGrid();
        BuildGuided();
        documentTitle.Text = FriendlyName(selected);
        documentState.Text = disk == null ? "Nuovo file · anteprima" : "Pronto per la modifica";
        destinationHint.Text = mode.SelectedIndex == 0 ? "Predefiniti nei sorgenti · richiedono una nuova compilazione" : selected.NeedsWorld || (selected.Mod == "Eldritch Surge" && useWorld) ? "Configurazione del mondo selezionato" : "Configurazione globale dell’installazione";
    }
    void SetText(string text) { updating = true; raw.Text = text; updating = false; }
    void UpdateTitle() { Text = "GoldenRose • " + (current?.ToString() ?? "Configuratore mod") + (dirty ? " *" : ""); documentState.Text = dirty ? "● Modifiche da salvare" : "✓ Nessuna modifica in sospeso"; }
    void CommitGrid() { if (grid.IsCurrentCellInEditMode) { grid.EndEdit(); ApplyGrid(); } }
    void SaveCurrent()
    {
        RequireCurrent(); ValidateChildren(); CommitGrid(); Storage.Validate(current!, raw.Text);
        string backup = Storage.Save(path!, raw.Text, original); original = raw.Text; dirty = false; UpdateTitle();
        status.Text = backup.Length > 0 ? "Salvato. Backup: " + backup : "File creato: " + path;
    }
    void LoadDefaults() { RequireCurrent(); if (!CanDiscard()) return; raw.Text = Storage.Template(current!); dirty = true; UpdateTitle(); BuildGrid(); BuildGuided(); status.Text = "Predefiniti inclusi nell'EXE caricati in anteprima. Salva per applicarli."; }
    void BuildGrid()
    {
        grid.Rows.Clear(); tree = null; if (current == null) return;
        if (current.Name.EndsWith(".config"))
        {
            var lines = raw.Lines;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i]; if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#')) continue;
                int sep = line.IndexOf('=');
                int row = grid.Rows.Add(sep >= 0 ? line[..sep].Trim() : $"Riga {i + 1}", sep >= 0 ? line[(sep + 1)..] : line);
                grid.Rows[row].Tag = i;
            }
        }
        else
        {
            try { tree = JsonNode.Parse(raw.Text, documentOptions: Storage.JsonOptions); Walk(tree, new List<string>(), ""); }
            catch { status.Text = "Testo JSON non valido: correggilo nella scheda Testo completo."; tabs.SelectedIndex = 2; }
        }
    }
    void Walk(JsonNode? node, List<string> route, string label)
    {
        if (node is JsonObject obj && obj.Count > 0) foreach (var (key, value) in obj) Walk(value, [..route, key], label.Length == 0 ? key : label + " / " + key);
        else if (node is JsonArray array && array.Any(x => x is JsonObject)) for (int i = 0; i < array.Count; i++) Walk(array[i], [..route, i.ToString()], label + " / " + i);
        else { int row = grid.Rows.Add(label.Length == 0 ? "Documento" : label, node?.ToJsonString() ?? "null"); grid.Rows[row].Tag = route; }
    }
    void ApplyGrid()
    {
        if (current == null || grid.CurrentCell == null) return;
        var row = grid.Rows[grid.CurrentCell.RowIndex]; string value = row.Cells[1].Value?.ToString() ?? "";
        if (row.Tag is int line)
        {
            var lines = raw.Lines; int sep = lines[line].IndexOf('='); lines[line] = sep < 0 ? value : lines[line][..(sep + 1)] + value; raw.Lines = lines;
        }
        else if (row.Tag is List<string> route)
        {
            JsonNode? edited;
            try { edited = JsonNode.Parse(value, documentOptions: Storage.JsonOptions); }
            catch { BuildGrid(); throw new FormatException("Valore JSON non valido: usa true/false, un numero, una stringa fra virgolette o una lista [\"valore\"]."); }
            if (route.Count == 0) tree = edited;
            else {
                JsonNode? parent = tree;
                foreach (string key in route.Take(route.Count - 1)) parent = parent is JsonArray a ? a[int.Parse(key)] : parent![key];
                if (parent is JsonArray arr) arr[int.Parse(route[^1])] = edited; else parent![route[^1]] = edited;
            }
            raw.Text = tree?.ToJsonString(Storage.Pretty) ?? "null";
            status.Text = "Modifiche in sospeso. L'editor a campi formatta il JSON; il testo completo conserva i commenti se modificato direttamente.";
        }
        dirty = true; UpdateTitle();
    }
}
