using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ModConfigurator;

internal sealed record ConfigFile(string Mod, string Name, string Resource, string Source, string Instance, bool NeedsWorld)
{
    public override string ToString() => $"{Mod} / {Name}";
}
internal static class Storage
{
    public static readonly JsonDocumentOptions JsonOptions = new() { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true };
    public static readonly JsonSerializerOptions Pretty = new() { WriteIndented = true };
    public static List<ConfigFile> Catalog()
    {
        var files = new List<ConfigFile>();
        foreach (string resource in Assembly.GetExecutingAssembly().GetManifestResourceNames().Order())
        {
            var parts = resource.Replace('\\', '/').Split('/');
            if (parts.Length != 3 || parts[0] != "templates") continue;
            string name = parts[2];
            files.Add(parts[1] switch {
                "bettervanilla" => new("BetterVanilla+", name, resource, "BetterVanilla+/src/main/resources/config/bettervanilla/defaults.json", "config/bettervanilla/config.json", false),
                "eldritch" => new("Eldritch Surge", name, resource, "Eldrich-Surge/src/main/resources/config/eldrich-surge/" + name, "config/eldrich-surge/" + name, false),
                _ => new("EnchLib", name, resource, "enchlib/src/main/resources/config/" + name, "config/enchlib/" + name, true)
            });
        }
        return files;
    }
    public static string Template(ConfigFile file)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(file.Resource)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
    public static void Validate(ConfigFile file, string text)
    {
        if (file.Name.EndsWith(".config"))
        {
            int lineNumber = 0;
            foreach (string rawLine in text.Split('\n'))
            {
                lineNumber++;
                string line = rawLine.Trim();
                if (line.Length == 0 || line.StartsWith('#')) continue;
                if (file.Name == "EnchMultiplier.config")
                {
                    var parts = line.Split(',').Select(x => x.Trim()).ToArray();
                    if (parts.Length != 4 || !int.TryParse(parts[1], out int level) || level < 1 ||
                        !double.TryParse(parts[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double multiplier) || !double.IsFinite(multiplier) ||
                        !double.TryParse(parts[3], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double extra) || !double.IsFinite(extra))
                        throw new FormatException($"Riga {lineNumber}: formato richiesto ID, livello, moltiplicatore, extra.");
                    continue;
                }
                int separator = line.IndexOf('=');
                if (separator < 1) throw new FormatException($"Riga {lineNumber}: usa chiave=valore.");
                string value = line[(separator + 1)..].Trim();
                if (file.Name == "AviableEnch.config" && value is not "true" and not "false")
                    throw new FormatException($"Riga {lineNumber}: usa true o false.");
                if (file.Name == "EnchLVLmax.config" && value.Length > 0 && (!int.TryParse(value, out int n) || n < 1 || n > 255))
                    throw new FormatException($"Riga {lineNumber}: livello fra 1 e 255, oppure vuoto.");
            }
            return;
        }
        if (!file.Name.EndsWith(".json") && !file.Name.EndsWith(".json5")) return;
        var root = JsonNode.Parse(text, documentOptions: JsonOptions) ?? throw new FormatException("Il documento non può essere null.");
        if (file.Mod == "BetterVanilla+")
        {
            var baseline = JsonNode.Parse(Template(file))!.AsObject();
            var obj = root.AsObject();
            foreach (var (key, value) in obj)
            {
                if (!baseline.ContainsKey(key)) throw new FormatException($"Campo sconosciuto: {key}");
                if (baseline[key]!.GetValueKind() == JsonValueKind.True || baseline[key]!.GetValueKind() == JsonValueKind.False) { _ = value!.GetValue<bool>(); continue; }
                int n = value!.GetValue<int>();
                var (min, max) = key switch {
                    "elevatorRange" => (1, 4096), "elevatorCooldownTicks" => (0, 1200), "particleRadius" => (0, 32),
                    "particleIntervalTicks" => (1, 1200), "particlesPerElevator" => (0, 32), "spawnProtectionRadius" => (0, 8),
                    "beaconRequiredLevel" => (1, 4), "beaconCooldownTicks" => (1, 72000), "beaconRange" => (1, 512),
                    "saturationAmplifier" => (0, 255), _ => (0, int.MaxValue) };
                if (n < min || n > max) throw new FormatException($"{key}: inserisci un intero fra {min} e {max}.");
            }
        }
        else if (file.Mod == "Eldritch Surge")
        {
            if (root["configVersion"]?.GetValue<int>() != 4) throw new FormatException("configVersion deve essere 4.");
            foreach (var (id, entry) in root["enchantments"]!.AsObject())
            {
                if (!id.Contains(':') || entry is not JsonObject obj) throw new FormatException($"Incantesimo non valido: {id}");
                foreach (var (key, val) in obj)
                {
                    if (key is "enabled" or "normalTable" or "advancedTable" or "loot") _ = val!.GetValue<bool>();
                    if (key is "anvilMaxLevel" or "enchantingTableMaxLevel" or "normalTableChance" or "advancedTableChance" or "rarity")
                    {
                        int n = val!.GetValue<int>(); int max = key.EndsWith("Chance") ? 100 : key.EndsWith("Level") ? 255 : int.MaxValue;
                        if (n < 0 || n > max) throw new FormatException($"{id} / {key}: valore fuori intervallo 0–{max}.");
                    }
                    if (key is "supportedItems" or "incompatibleEnchantments") foreach (var item in val!.AsArray()) _ = item!.GetValue<string>();
                }
            }
        }
        else if (file.Name == "AviableEnch.json5")
        {
            foreach (var entry in root.AsArray()) { _ = entry!["id"]!.GetValue<string>(); _ = entry["enabled"]!.GetValue<bool>(); }
        }
        else if (file.Name == "EnchantmentsDetails.json5")
        {
            foreach (var entry in root["enchantments"]!.AsArray())
            {
                _ = entry!["id"]!.GetValue<string>();
                if (entry["max_level"] is {} level && (level.GetValue<int>() < 1 || level.GetValue<int>() > 255)) throw new FormatException("max_level: intervallo 1–255.");
            }
        }
    }
    public static string Save(string path, string text, string? original)
    {
        if (File.Exists(path) ? original == null || File.ReadAllText(path) != original : original != null)
            throw new IOException("Il file è cambiato fuori dall'app. Riaprilo prima di salvare.");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        string temp = path + ".tmp-" + Guid.NewGuid().ToString("N");
        string backup = path + ".bak-" + DateTime.Now.ToString("yyyyMMdd-HHmmss-fffffff");
        try {
            File.WriteAllText(temp, text, new UTF8Encoding(false));
            if (File.Exists(path)) File.Replace(temp, path, backup); else File.Move(temp, path);
        } finally { if (File.Exists(temp)) File.Delete(temp); }
        return File.Exists(backup) ? backup : "";
    }
}
