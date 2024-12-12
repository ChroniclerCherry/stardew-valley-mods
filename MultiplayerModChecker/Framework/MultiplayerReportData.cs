using System;
using System.Collections.Generic;

namespace MultiplayerModChecker.Framework;

public class MultiplayerReportData
{
    public string FarmhandName { get; set; }
    public DateTime TimeConnected { get; set; }
    public long FarmhandId { get; set; }

    public SmapiGameVersionDifference SmapiGameGameVersions { get; set; } = new SmapiGameVersionDifference();

    internal List<ModVersions> Mods { get; set; } = new List<ModVersions>();

    public List<string> MissingOnHost { get; set; } = new List<string>();
    public List<string> MissingOnFarmhand { get; set; } = new List<string>();

    public List<string> VersionMismatch { get; set; } = new List<string>();
}
