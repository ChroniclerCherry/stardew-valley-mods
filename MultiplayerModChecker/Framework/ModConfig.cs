namespace MultiplayerModChecker.Framework;

internal class ModConfig
{
    public string[] IgnoredMods { get; set; } = { "Cherry.MultiplayerModChecker" };
    public bool HideReportInTrace { get; set; } = false;
}
