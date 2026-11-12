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
            var preserved = "{\"configVersion\":4,\"futureRoot\":9,\"enchantments\":{\"test:a\":{\"enabled\":false,\"normalTableChance\":37,\"futureField\":{\"x\":3},\"supportedItems\":[\"minecraft:stick\"]},\"test:b\":{\"rarity\":53}}}";
            var imported = LegacyImport.Merge(preserved, "{\"configVersion\":2,\"enchantments\":{\"test:a\":{\"anvilMaxLevel\":12}}}");
            var importedRoot = System.Text.Json.Nodes.JsonNode.Parse(imported)!;
            if ((int?)importedRoot["configVersion"] != 4 || (int?)importedRoot["futureRoot"] != 9 || (int?)importedRoot["enchantments"]?["test:a"]?["futureField"]?["x"] != 3 || (int?)importedRoot["enchantments"]?["test:a"]?["normalTableChance"] != 37) throw new Exception("Legacy import dropped current fields");
            var doc = new EnchantDocument(imported, "eldritch");
            doc.ChangeGroup(["test:a"], "#minecraft:swords", true);
            doc.ChangeGroup(["test:a"], "#minecraft:swords", true);
            if (doc.Strings("test:a", "supportedItems").Length != 2 || (int?)doc.Get("test:b", "rarity") != 53 || (bool?)doc.Get("test:a", "enabled") != false) throw new Exception("Bulk group edit modified unrelated values");
            doc.ChangeGroup(["test:a"], "#minecraft:swords", false);
            if (doc.Strings("test:a", "supportedItems").Single() != "minecraft:stick") throw new Exception("Group removal failed");
            var details = new EnchantDocument("{\"other\":true,\"enchantments\":[{\"id\":\"test:a\",\"table_rules\":{\"mod:table\":{\"enabled\":false}}}]}", "details");
            details.Set("test:a", "normalTableChance", System.Text.Json.Nodes.JsonValue.Create(42));
            details.Set("test:a", "anvilMaxLevel", System.Text.Json.Nodes.JsonValue.Create(7));
            if ((int?)details.Get("test:a", "anvilMaxLevel") != 7 || (int?)details.Get("test:a", "normalTableChance") != 42 || (bool?)details.Root["enchantments"]?[0]?["table_rules"]?["mod:table"]?["enabled"] != false) throw new Exception("EnchLib bridge failed");
            if (!AssetCatalog.Items.Any() || EldritchConfigTool.ConfigForm.Items.Length < 10) throw new Exception("Legacy item catalog missing");
            using var form = new MainForm(false); form.Show(); System.Windows.Forms.Application.DoEvents();
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            var projects = (System.Windows.Forms.TextBox)typeof(MainForm).GetField("projects", flags)!.GetValue(form)!;
            projects.Text = dir;
            string source = Path.Combine(dir, better.Source);
            Directory.CreateDirectory(Path.GetDirectoryName(source)!);
            File.WriteAllText(source, Storage.Template(better));
            var list = (System.Windows.Forms.ListBox)typeof(MainForm).GetField("files", flags)!.GetValue(form)!;
            list.SelectedIndex = 0;
            typeof(MainForm).GetMethod("OpenSelected", flags)!.Invoke(form, null);
            System.Windows.Forms.Application.DoEvents();
            using var screenshot = new System.Drawing.Bitmap(form.Width, form.Height);
            form.DrawToBitmap(screenshot, new System.Drawing.Rectangle(0, 0, form.Width, form.Height));
            screenshot.Save(report + ".png");
            var mode = (System.Windows.Forms.ComboBox)typeof(MainForm).GetField("mode", flags)!.GetValue(form)!;
            var instance = (System.Windows.Forms.TextBox)typeof(MainForm).GetField("instance", flags)!.GetValue(form)!;
            mode.SelectedIndex = 1;
            instance.Text = dir;
            typeof(MainForm).GetMethod("OpenSelected", flags)!.Invoke(form, null);
            var grid = (System.Windows.Forms.DataGridView)typeof(MainForm).GetField("grid", flags)!.GetValue(form)!;
            grid.CurrentCell = grid.Rows[0].Cells[1];
            grid.Rows[0].Cells[1].Value = "256";
            typeof(MainForm).GetMethod("ApplyGrid", flags)!.Invoke(form, null);
            typeof(MainForm).GetMethod("SaveCurrent", flags)!.Invoke(form, null);
            var saved = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(Path.Combine(dir, "config/bettervanilla/config.json")))!;
            if (saved["elevatorRange"]!.GetValue<int>() != 256 || saved["beaconRange"]!.GetValue<int>() != 50) throw new Exception("UI round trip failed");
            list.SelectedItem = eld;
            typeof(MainForm).GetMethod("OpenSelected", flags)!.Invoke(form, null);
            if (form.enchantEditor?.Document?.Entries.Count < 100) throw new Exception("Guided Eldritch editor missing");
            var selectedId = form.enchantEditor!.SelectedId!;
            form.enchantEditor.SetField("normalTableChance", System.Text.Json.Nodes.JsonValue.Create(42));
            typeof(MainForm).GetMethod("SaveCurrent", flags)!.Invoke(form, null);
            var savedEld = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(Path.Combine(dir, eld.Instance)))!;
            if ((int?)savedEld["enchantments"]?[selectedId]?["normalTableChance"] != 42) throw new Exception("Guided editor save failed");
            System.Windows.Forms.Application.DoEvents();
            using (var eldShot = new System.Drawing.Bitmap(form.Width, form.Height)) { form.DrawToBitmap(eldShot, new System.Drawing.Rectangle(0, 0, form.Width, form.Height)); eldShot.Save(report + ".eldritch.png"); }
            using (var picker = new ItemPicker(null, ["minecraft:diamond_sword", "mod:custom"])) {
                picker.Show(); System.Windows.Forms.Application.DoEvents();
                if (!picker.Result.Contains("mod:custom")) throw new Exception("Picker lost unknown item ID");
                using var shot = new System.Drawing.Bitmap(picker.Width, picker.Height); picker.DrawToBitmap(shot, new System.Drawing.Rectangle(0, 0, picker.Width, picker.Height)); shot.Save(report + ".items.png"); picker.Close();
            }
            form.Close();
            File.WriteAllText(report, $"PASS: {catalog.Count} templates; 3 mods; validation, backups, conflict protection, JSON5; source and instance round trips; legacy import with unknown fields; bulk add/remove isolation; EnchLib bridge; legacy item catalog; guided Eldritch edit/save; custom item preservation.\n");
        } catch (Exception error) { File.WriteAllText(report, "FAIL: " + error); Environment.ExitCode = 1; }
        finally { Directory.Delete(dir, true); }
    }
}
