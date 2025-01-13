using GenericModConfigMenu;
using StardewModdingAPI;


namespace ChangeSlimeHutchLimit.Framework;
public static class GMCMHandler
{
    public static void Setup(IManifest manifest)
    {
        var api = ModEntry.ModHelper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (api == null) return;

        // Register the mod for GMCM
        api.Register(
            mod: manifest,
            reset: () => ModEntry.Config = new Config(), // Reset to default values
            save: () => ModEntry.ModHelper.WriteConfig(ModEntry.Config) // Save to file
        );

        // Add sections and options to the GMCM menu
        api.AddSectionTitle(
            mod: manifest,
            text: () => "Slime Hutch Settings"
        );

        api.AddNumberOption(
            mod: manifest,
            name: () => "Max # of Slimes in Hutch",
            tooltip: () => "Set the maximum number of slimes allowed in the slime hutch.",
            getValue: () => ModEntry.Config.MaxSlimesInHutch,
            setValue: value => ModEntry.Config.MaxSlimesInHutch = value,
            min: 20,
            interval: 5
        );

        api.AddSectionTitle(
            mod: manifest,
            text: () => "Slime Ball Settings"
        );

        api.AddBoolOption(
            mod: manifest,
            name: () => "Enable Slime Ball Override",
            tooltip: () => "Enable or disable the override for slime ball placement.",
            getValue: () => ModEntry.Config.EnableSlimeBallOverride,
            setValue: value => ModEntry.Config.EnableSlimeBallOverride = value
        );

        api.AddNumberOption(
            mod: manifest,
            name: () => "Max Daily Slime Balls",
            tooltip: () => "Set the maximum number of slime balls that can spawn daily (set to 0 for unlimited).",
            getValue: () => ModEntry.Config.MaxDailySlimeBalls,
            setValue: value => ModEntry.Config.MaxDailySlimeBalls = value,
            min: 0,
            max: 100,
            interval: 1
        );
    }
}
