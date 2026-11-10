namespace ModConfigurator;
internal static class SelfTest
{
    public static void Run(string report)
    {
        string dir = Path.Combine(Path.GetTempPath(), "ModConfigurator-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try {
            var catalog = Storage.Catalog();
            if (catalog.Select(f => f.Mod).Distinct().Count() != 3) throw new Exception("Missing mods");
            foreach (var file in catalog) Storage.Validate(file, Storage.Template(file));
            string path = Path.Combine(dir, "config.json");
            Storage.Save(path, "{\"extra\":42}", null);
            string backup = Storage.Save(path, "{\"extra\":43}", "{\"extra\":42}");
            if (File.ReadAllText(backup) != "{\"extra\":42}") throw new Exception("Backup mismatch");
            bool rejected = false;
            try { Storage.Save(path, "{}", "stale"); } catch (IOException) { rejected = true; }
            if (!rejected || File.ReadAllText(path) != "{\"extra\":43}") throw new Exception("Conflict protection failed");
            var better = catalog.Single(f => f.Mod == "BetterVanilla+");
            rejected = false;
            try { Storage.Validate(better, "{\"elevatorRange\":-1}"); } catch { rejected = true; }
            if (!rejected) throw new Exception("Range validation failed");
            var eld = catalog.Single(f => f.Mod == "Eldritch Surge");
            rejected = false;
            try { Storage.Validate(eld, "{\"configVersion\":4,\"enchantments\":{\"test:ench\":{\"normalTableChance\":101}}}"); } catch { rejected = true; }
            if (!rejected) throw new Exception("Chance validation failed");
            Storage.Validate(catalog.Single(f => f.Name == "EnchantmentsDetails.json5"), "{/* comment */ \"enchantments\": [],}");
            using var form = new MainForm(); form.Show(); System.Windows.Forms.Application.DoEvents();
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            var list = (System.Windows.Forms.ListBox)typeof(MainForm).GetField("files", flags)!.GetValue(form)!;
            list.SelectedIndex = 0;
            typeof(MainForm).GetMethod("OpenSelected", flags)!.Invoke(form, null);
            System.Windows.Forms.Application.DoEvents();
            using var screenshot = new System.Drawing.Bitmap(form.Width, form.Height);
            form.DrawToBitmap(screenshot, new System.Drawing.Rectangle(0, 0, form.Width, form.Height));
            screenshot.Save(report + ".png");
            form.Close();
            File.WriteAllText(report, $"PASS: {catalog.Count} embedded templates, 3 mods, JSON validation, backups, conflict protection, ranges, JSON5 comments, window construction.\n");
        } catch (Exception error) { File.WriteAllText(report, "FAIL: " + error); Environment.ExitCode = 1; }
        finally { Directory.Delete(dir, true); }
    }
}
