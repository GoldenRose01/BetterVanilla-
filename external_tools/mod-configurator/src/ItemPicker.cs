namespace ModConfigurator;
internal sealed class ItemPicker : Form
{
    readonly ListView list = new() { Dock = DockStyle.Fill, View = View.Details, CheckBoxes = true, FullRowSelect = true, HideSelection = false, BorderStyle = BorderStyle.None };
    readonly ImageList icons = new() { ImageSize = new Size(28,28), ColorDepth = ColorDepth.Depth32Bit };
    readonly TextBox search = new() { Dock = DockStyle.Top, PlaceholderText = "Cerca per nome, ID o gruppo…" };
    readonly HashSet<string> selected;
    readonly List<Image> ownedImages = new();
    readonly List<(string Name, string Id, string Icon)> choices = new();
    bool rebuilding;
    internal string[] Result => selected.Order().ToArray();
    internal ItemPicker(string[]? enchantments, IEnumerable<string> initial)
    {
        Text = enchantments == null ? "Oggetti e gruppi • catalogo Eldritch" : "Incantesimi incompatibili";
        Size = new Size(830, 670); MinimumSize = new Size(650, 500); StartPosition = FormStartPosition.CenterParent; Font = new Font("Segoe UI", 10); BackColor = Theme.Background;
        selected = new HashSet<string>(initial, StringComparer.Ordinal);
        if (enchantments == null) {
            foreach (var group in EldritchConfigTool.ConfigForm.Items) choices.Add(("Gruppo · " + group.Label, group.Value, group.IconKey));
            foreach (var id in AssetCatalog.Items) choices.Add((AssetCatalog.Name(id, false), id, id));
        } else foreach (var id in enchantments) choices.Add((AssetCatalog.Name(id), id, "enchanted_book"));
        foreach (var custom in selected.Where(id => choices.All(c => c.Id != id))) choices.Add(("Personalizzato", custom, ""));
        list.Columns.Add("Nome", 300); list.Columns.Add("ID / gruppo", 430); list.SmallImageList = icons;
        var header = Theme.Stack(); Theme.Add(header, Theme.Label(enchantments == null ? "Scegli oggetti o interi gruppi" : "Scegli le incompatibilità", 17, true));
        Theme.Add(header, Theme.Label("Le selezioni restano attive anche quando cambi ricerca.")); Theme.Add(header, search);
        var footer = Theme.Flow(); footer.Dock = DockStyle.Bottom; footer.Padding = new Padding(12);
        var manual = new TextBox { Width = 300, PlaceholderText = "ID personalizzato o #tag" };
        footer.Controls.Add(manual); footer.Controls.Add(Theme.Button("Aggiungi ID", () => {
            string id = manual.Text.Trim();
            if (!System.Text.RegularExpressions.Regex.IsMatch(id, "^#?[a-z0-9_.-]+:[a-z0-9_./-]+$")) { MessageBox.Show(this, "Inserisci un ID come minecraft:diamond_sword oppure #minecraft:swords."); return; }
            selected.Add(id); if (choices.All(c => c.Id != id)) choices.Add(("Personalizzato", id, "")); manual.Clear(); Reload();
        }));
        footer.Controls.Add(Theme.Button("Usa selezione", () => DialogResult = DialogResult.OK, true));
        footer.Controls.Add(Theme.Button("Annulla", () => DialogResult = DialogResult.Cancel));
        Controls.Add(list); Controls.Add(header); Controls.Add(footer);
        search.TextChanged += (_, _) => Reload();
        list.ItemChecked += (_, e) => { if (!rebuilding && e.Item.Tag is string id) { if (e.Item.Checked) selected.Add(id); else selected.Remove(id); } };
        Reload();
    }
    void Reload()
    {
        rebuilding = true; list.BeginUpdate(); list.Items.Clear();
        foreach (var choice in choices.Where(c => (c.Name + " " + c.Id).Contains(search.Text, StringComparison.OrdinalIgnoreCase))) {
            // ImageList may defer native image creation until the first paint. Keep originals alive.
            if (!icons.Images.ContainsKey(choice.Icon)) { var icon = AssetCatalog.Icon(choice.Icon); if (icon != null) { ownedImages.Add(icon); icons.Images.Add(choice.Icon, icon); } }
            var row = new ListViewItem(choice.Name) { Tag = choice.Id, Checked = selected.Contains(choice.Id), ImageKey = choice.Icon }; row.SubItems.Add(choice.Id); list.Items.Add(row);
        }
        list.EndUpdate(); rebuilding = false;
    }
    protected override void Dispose(bool disposing) { if (disposing) { icons.Dispose(); foreach (var image in ownedImages) image.Dispose(); } base.Dispose(disposing); }
}
