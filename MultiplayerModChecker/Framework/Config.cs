namespace MultiplayerModChecker.Framework;

public class Config
{
    public string[] IgnoredMods { get; set; } = { "Cherry.MultiplayerModChecker" };
    public bool HideReportInTrace { get; set; } = false;
}
