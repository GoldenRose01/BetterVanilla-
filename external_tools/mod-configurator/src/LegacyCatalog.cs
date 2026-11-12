// Reuse the existing Eldritch configurator's actual catalogs, rather than maintaining a second copy.
namespace EldritchConfigTool;
internal sealed partial class ConfigForm
{
    internal static ItemChoice[] Items => GroupChoices;
    internal static RarityOption[] Rarities => RarityOptions;
    internal static string Category(string id, string context)
    {
        string Canonical(string s) => new(s.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
        bool Named(string[] names) => names.Any(n => Canonical(id.Split(':').Last()).Contains(Canonical(n)));
        bool Has(params string[] words) => words.Any(w => context.Contains(w, StringComparison.OrdinalIgnoreCase));
        if (Named(ArcheryEnchantments) || Has("bow", "projectile")) return "Archi";
        if (Named(DefenseEnchantments) || Has("armor", "helmet", "chestplate", "leggings", "boots", "elytra", "shield")) return "Armature";
        if (Named(ToolEnchantments) || Has("pickaxe", "shovel", "hoe", "shears", "mining", "fishing")) return "Attrezzi";
        if (Named(WeaponEnchantments) || Has("sword", "trident", "mace", "weapon", "melee")) return "Armi";
        return "Altro";
    }
}
