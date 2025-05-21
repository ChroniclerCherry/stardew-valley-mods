using System;
using System.Collections.Generic;
using StardewModdingAPI;

namespace MultiplayerModChecker.Framework;

internal class MultiplayerReportData
{
    public long FarmhandId { get; }
    public string FarmhandName { get; }
    public DateTime TimeConnected { get; }

    public SmapiGameVersionDifference SmapiGameGameVersions { get; }

    internal List<ModVersions> Mods { get; } = [];

    public List<string> MissingOnHost { get; } = [];
    public List<string> MissingOnFarmhand { get; } = [];

    public List<string> VersionMismatch { get; } = [];

    public MultiplayerReportData(long farmhandId, string farmhandName, DateTime timeConnected, ISemanticVersion hostApiVersion, ISemanticVersion? farmhandApiVersion)
    {
        this.FarmhandId = farmhandId;
        this.FarmhandName = farmhandName;
        this.TimeConnected = timeConnected;
        this.SmapiGameGameVersions = new SmapiGameVersionDifference(hostApiVersion, farmhandApiVersion);
    }
}
