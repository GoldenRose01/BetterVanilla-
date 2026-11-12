using System.Reflection;
using System.Text.Json.Nodes;

namespace ModConfigurator;
internal sealed class EnchantDocument
{
    internal JsonNode Root { get; }
    internal string Kind { get; }
    internal Dictionary<string, JsonObject> Entries { get; } = new(StringComparer.Ordinal);
    internal EnchantDocument(string text, string kind)
    {
        Kind = kind; Root = JsonNode.Parse(text, documentOptions: Storage.JsonOptions) ?? throw new FormatException("Documento vuoto");
        if (kind == "eldritch") foreach (var (id, node) in Root["enchantments"]!.AsObject()) Entries.Add(id, node!.AsObject());
        else foreach (var node in (kind == "available" ? Root.AsArray() : Root["enchantments"]!.AsArray())) Entries.Add(node!["id"]!.GetValue<string>(), node.AsObject());
    }
    internal string Key(string key) => Kind == "eldritch" ? key : key switch {
        "anvilMaxLevel" => "max_level", "supportedItems" => "enc_category", "incompatibleEnchantments" => "incompatible_enchantments", "rarityClass" => "rarity", _ => key };
    internal JsonNode? Get(string id, string key)
    {
        var entry = Entries[id];
        if (Kind == "details" && key is "normalTable" or "advancedTable" or "normalTableChance" or "advancedTableChance")
            return entry["table_rules"]?[key.StartsWith("normal") ? "minecraft:enchanting_table" : "eldrich-surge:advanced_enchanting_table"]?[key.EndsWith("Chance") ? "chance" : "enabled"];
        return entry[Key(key)];
    }
    internal void Set(string id, string key, JsonNode? value)
    {
        var entry = Entries[id];
        if (Kind == "details" && key is "normalTable" or "advancedTable" or "normalTableChance" or "advancedTableChance") {
            var rules = entry["table_rules"] as JsonObject; if (rules == null) entry["table_rules"] = rules = new JsonObject();
            string table = key.StartsWith("normal") ? "minecraft:enchanting_table" : "eldrich-surge:advanced_enchanting_table";
            var rule = rules[table] as JsonObject; if (rule == null) rules[table] = rule = new JsonObject();
            rule[key.EndsWith("Chance") ? "chance" : "enabled"] = value?.DeepClone();
        } else entry[Key(key)] = value?.DeepClone();
    }
    internal string[] Strings(string id, string key) => Get(id, key) is JsonArray a ? a.Select(n => n!.GetValue<string>()).ToArray() : [];
    internal void ChangeGroup(IEnumerable<string> ids, string group, bool add)
    {
        foreach (string id in ids) {
            var list = Strings(id, "supportedItems").ToList();
            if (add && !list.Contains(group)) list.Add(group);
            if (!add) list.RemoveAll(s => s == group);
            Set(id, "supportedItems", new JsonArray(list.Select(s => (JsonNode?)JsonValue.Create(s)).ToArray()));
        }
    }
    internal string Serialize() => Root.ToJsonString(Storage.Pretty);
}
internal static class AssetCatalog
{
    static readonly JsonObject Data = Read();
    static JsonObject Read() { using var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("catalog.json")!; return JsonNode.Parse(s)!.AsObject(); }
    internal static string Name(string id, bool enchantment = true)
    {
        string key = (enchantment ? "enchantment." : "item.") + id.Replace(':', '.');
        return (string?)Data["names"]?[key] ?? System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(id.Split(':').Last().Replace('_', ' '));
    }
    internal static IEnumerable<string> Items => Data["items"]!.AsArray().Select(n => n!.GetValue<string>());
    internal static Image? Icon(string id)
    {
        string key = id.Contains(':') ? id.Split(':')[1] : id;
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("icons/" + key + ".png");
        if (stream == null) return null;
        using var original = Image.FromStream(stream); return new Bitmap(original, 28, 28);
    }
}
internal sealed record EnchantChoice(string Id)
{
    public override string ToString() => AssetCatalog.Name(Id) + "  ·  " + Id;
}
