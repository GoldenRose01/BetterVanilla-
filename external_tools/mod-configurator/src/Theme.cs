namespace ModConfigurator;
internal static class Theme
{
    internal static readonly Color Background = Color.FromArgb(242, 245, 249);
    internal static readonly Color Ink = Color.FromArgb(25, 39, 57);
    internal static readonly Color Muted = Color.FromArgb(93, 109, 129);
    internal static readonly Color Accent = Color.FromArgb(0, 116, 106);
    internal static Label Label(string text, float size = 10, bool bold = false) => new() { Text = text, AutoSize = true, ForeColor = Ink, Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular), Margin = new Padding(0, 4, 8, 5) };
    internal static Button Button(string text, Action action, bool primary = false)
    {
        var b = new Button { Text = text, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(14, 7, 14, 7), FlatStyle = FlatStyle.Flat, BackColor = primary ? Accent : Color.White, ForeColor = primary ? Color.White : Ink, Cursor = Cursors.Hand, Margin = new Padding(0, 4, 8, 4) };
        b.FlatAppearance.BorderColor = primary ? Accent : Color.FromArgb(210, 219, 229);
        b.Click += (_, _) => action(); return b;
    }
    internal static FlowLayoutPanel Flow() => new() { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, Padding = new Padding(0, 4, 0, 4) };
    internal static TableLayoutPanel Stack() => new() { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, Padding = new Padding(16), BackColor = Color.White };
    internal static void Combo(ComboBox combo)
    {
        combo.DrawMode = DrawMode.OwnerDrawFixed;
        combo.ItemHeight = 24;
        combo.DrawItem += (_, e) => {
            e.DrawBackground();
            string text = (e.Index >= 0 ? combo.GetItemText(combo.Items[e.Index]) : combo.Text) ?? "";
            TextRenderer.DrawText(e.Graphics, text, combo.Font, e.Bounds, e.ForeColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            e.DrawFocusRectangle();
        };
    }
    internal static void Add(TableLayoutPanel panel, Control child) { child.Margin = new Padding(0, 2, 0, 4); panel.Controls.Add(child, 0, panel.RowCount++); }
    internal static void Grid(DataGridView grid)
    {
        grid.BackgroundColor = Color.White; grid.BorderStyle = BorderStyle.None; grid.GridColor = Color.FromArgb(229, 235, 241);
        grid.EnableHeadersVisualStyles = false; grid.ColumnHeadersDefaultCellStyle.BackColor = Background; grid.ColumnHeadersDefaultCellStyle.ForeColor = Ink;
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 241, 237); grid.DefaultCellStyle.SelectionForeColor = Ink;
        grid.RowTemplate.Height = 34; grid.ColumnHeadersHeight = 38; grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 251, 253);
    }
}
