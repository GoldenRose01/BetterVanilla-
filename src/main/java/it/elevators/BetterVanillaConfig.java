package it.elevators;

import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import net.fabricmc.loader.api.FabricLoader;
import java.io.InputStreamReader;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;

/** Bundled defaults are the source of truth; existing instance files are never rewritten. */
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
                        if (baseline.isBoolean() ? !value.isBoolean() : !value.isNumber())
                            throw new IllegalArgumentException(entry.getKey());
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

    public static int number(String key, int min, int max) {
        if (values == null) load();
        return Math.clamp(values.get(key).getAsInt(), min, max);
    }
    public static boolean enabled(String key) {
        if (values == null) load();
        return values.get(key).getAsBoolean();
    }
}
