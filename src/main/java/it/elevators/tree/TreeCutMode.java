package it.elevators.tree;

public enum TreeCutMode {
    OFF("off"),
    FELL("fell"),
    CHOP("chop");

    private final String id;

    TreeCutMode(String id) {
        this.id = id;
    }

    public String getId() {
        return id;
    }

    public static TreeCutMode fromString(String text) {
        if (text != null) {
            for (TreeCutMode mode : values()) {
                if (mode.id.equalsIgnoreCase(text.trim())) {
                    return mode;
                }
            }
        }
        return FELL;
    }
}
