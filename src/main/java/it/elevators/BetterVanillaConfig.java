package it.elevators;

import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import it.elevators.tree.TreeCutMode;
import net.fabricmc.loader.api.FabricLoader;
import java.io.InputStreamReader;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;

/** Bundled defaults are the source of truth; existing instance files are updated safely. */
public final class BetterVanillaConfig {
    private static JsonObject values;
    private BetterVanillaConfig() {}

    public static void load() {
        try (var stream = BetterVanillaConfig.class.getResourceAsStream("/config/bettervanilla/defaults.json")) {
            if (stream == null) throw new IllegalStateException("Missing BetterVanilla defaults");
            values = JsonParser.parseReader(new InputStreamReader(stream, StandardCharsets.UTF_8)).getAsJsonObject();
            Path path = FabricLoader.getInstance().getConfigDir().resolve("bettervanilla/config.json");
            if (!Files.exists(path)) {
                Files.createDirectories(path.getParent());
                Files.writeString(path, new com.google.gson.GsonBuilder().setPrettyPrinting().create().toJson(values));
            } else {
                try (var reader = Files.newBufferedReader(path)) {
                    JsonObject custom = JsonParser.parseReader(reader).getAsJsonObject();
                    // Validate all known fields before applying any override.
                    for (var entry : custom.entrySet()) {
                        if (!values.has(entry.getKey())) continue;
                        var baseline = values.getAsJsonPrimitive(entry.getKey());
                        if (!entry.getValue().isJsonPrimitive()) throw new IllegalArgumentException(entry.getKey());
                        var value = entry.getValue().getAsJsonPrimitive();
                        if (baseline.isBoolean()) {
                            if (!value.isBoolean()) throw new IllegalArgumentException(entry.getKey());
                        } else if (baseline.isNumber()) {
                            if (!value.isNumber()) throw new IllegalArgumentException(entry.getKey());
                        } else if (baseline.isString()) {
                            if (!value.isString()) throw new IllegalArgumentException(entry.getKey());
                        }
                    }
                    custom.entrySet().forEach(e -> { if (values.has(e.getKey())) values.add(e.getKey(), e.getValue()); });
                } catch (Exception error) {
                    ElevatorsMod.LOGGER.error("Invalid BetterVanilla config; using bundled defaults without overwriting the file", error);
                }
            }
        } catch (java.io.IOException error) {
            ElevatorsMod.LOGGER.error("Could not create BetterVanilla config", error);
        }
    }

    public static synchronized void save() {
        if (values == null) return;
        try {
            Path path = FabricLoader.getInstance().getConfigDir().resolve("bettervanilla/config.json");
            Files.createDirectories(path.getParent());
            Files.writeString(path, new com.google.gson.GsonBuilder().setPrettyPrinting().create().toJson(values));
        } catch (Exception error) {
            ElevatorsMod.LOGGER.error("Could not save BetterVanilla config", error);
        }
    }

    public static int number(String key, int min, int max) {
        if (values == null) load();
        if (!values.has(key) || !values.get(key).isJsonPrimitive() || !values.get(key).getAsJsonPrimitive().isNumber()) {
            return min;
        }
        return Math.clamp(values.get(key).getAsInt(), min, max);
    }

    public static boolean enabled(String key) {
        if (values == null) load();
        if (!values.has(key) || !values.get(key).isJsonPrimitive() || !values.get(key).getAsJsonPrimitive().isBoolean()) {
            return false;
        }
        return values.get(key).getAsBoolean();
    }

    public static String string(String key, String fallback) {
        if (values == null) load();
        if (values.has(key) && values.get(key).isJsonPrimitive() && values.get(key).getAsJsonPrimitive().isString()) {
            return values.get(key).getAsString();
        }
        return fallback;
    }

    public static TreeCutMode getTreeCutMode() {
        return TreeCutMode.fromString(string("treecut", "fell"));
    }

    public static void setTreeCutMode(TreeCutMode mode) {
        if (values == null) load();
        values.addProperty("treecut", mode.getId());
        save();
    }
}
