namespace ToolUpgradeCosts.Framework;

public class Upgrade
{
    public int Cost { get; set; }

    public string MaterialName { get; set; }

    internal string MaterialId { get; set; }

    public int MaterialStack { get; set; }
}
