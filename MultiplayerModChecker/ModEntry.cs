using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MultiplayerModChecker.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace MultiplayerModChecker;

/// <summary>The mod entry point.</summary>
internal class ModEntry : Mod
{
    /*********
    ** Fields
    *********/
    private readonly List<MultiplayerReportData> RawReports = [];
    private readonly List<string> Reports = [];

    /// <summary>The mod settings.</summary>
    private ModConfig Config = null!; // set in Entry


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);

        this.Config = helper.ReadConfig<ModConfig>();

        helper.Events.Multiplayer.PeerContextReceived += this.OnPeerContextReceived;
        helper.Events.Multiplayer.ModMessageReceived += this.OnModMessageReceived;
    }


    /*********
    ** Private methods
    *********/
    /// <inheritdoc cref="IMultiplayerEvents.PeerContextReceived" />
    private void OnPeerContextReceived(object? sender, PeerContextReceivedEventArgs e)
    {
        if (!Context.IsMainPlayer)
            return;

        long farmhandId = e.Peer.PlayerID;
        string farmhandName =
            Game1.getAllFarmers().FirstOrDefault(f => f.UniqueMultiplayerID == e.Peer.PlayerID)?.Name
            ?? I18n.UnnamedFarmhand();


        var report = new MultiplayerReportData(farmhandId, farmhandName, DateTime.Now, Constants.ApiVersion, e.Peer.ApiVersion);

        if (e.Peer.HasSmapi)
        {
            var hostMods = this.Helper.ModRegistry.GetAll().Select(m => m.Manifest.UniqueID);
            var farmHandMods = e.Peer.Mods.Select(m => m.ID);
            var allMods = hostMods.Union(farmHandMods).Distinct();

            foreach (string modId in allMods)
            {
                if (this.Config.IgnoredMods.Contains(modId))
                    continue;

                IModInfo? hostMod = this.Helper.ModRegistry.Get(modId);
                IMultiplayerPeerMod? farmhandMod = e.Peer.GetMod(modId);

                string? modName = null;
                if (hostMod != null)
                    modName = hostMod.Manifest.Name;
                else if (farmhandMod != null)
                    modName = farmhandMod.Name;

                report.Mods.Add(
                    new ModVersions(modId, modName ?? modId, hostMod?.Manifest.Version, farmhandMod?.Version)
                );
            }
        }

        this.RawReports.Add(report);
        this.GenerateReport(report);
    }

    /// <inheritdoc cref="IMultiplayerEvents.ModMessageReceived" />
    private void OnModMessageReceived(object? sender, ModMessageReceivedEventArgs e)
    {
        if (e.FromModID != this.ModManifest.UniqueID && e.Type != "MultiplayerReport")
            return;

        Dictionary<string, LogLevel> reportData = e.ReadAs<Dictionary<string, LogLevel>>();

        this.PublishReport(null, reportData);
    }

    private void GenerateReport(MultiplayerReportData reportData)
    {
        Dictionary<string, LogLevel> report = [];

        if (!reportData.SmapiGameGameVersions.FarmhandHasSmapi)
        {
            report.Add(I18n.FarmhandMissingSMAPI(farmhandId: reportData.FarmhandId, farmhandName: reportData.FarmhandName, connectionTime: reportData.TimeConnected), LogLevel.Alert);
        }
        else
        {
            if (!reportData.SmapiGameGameVersions.HostSmapiVersion.Equals(reportData.SmapiGameGameVersions.FarmhandSmapiVersion))
                report.Add(I18n.SMAPIVersionMismatch(hostVersion: reportData.SmapiGameGameVersions.HostSmapiVersion, farmhandVersion: reportData.SmapiGameGameVersions.FarmhandSmapiVersion), LogLevel.Warn);

            foreach (ModVersions modData in reportData.Mods)
            {
                if (!modData.DoesHostHave)
                {
                    reportData.MissingOnHost.Add($"{modData.ModName} ({modData.ModUniqueId})");
                }
                else if (!modData.DoesFarmhandHave)
                {
                    reportData.MissingOnFarmhand.Add($"{modData.ModName} ({modData.ModUniqueId})");
                }
                else if (!modData.HostModVersion.Equals(modData.FarmhandModVersion))
                {
                    reportData.VersionMismatch.Add(I18n.ModMismatch_Version_Each(modName: modData.ModName, modId: modData.ModUniqueId, hostVersion: modData.HostModVersion, farmhandVersion: modData.FarmhandModVersion));
                }
            }
        }

        if (reportData.MissingOnHost.Count > 0)
            report.Add(I18n.ModMismatch_Host(modList: string.Join(",", reportData.MissingOnHost)), LogLevel.Warn);

        if (reportData.MissingOnFarmhand.Count > 0)
            report.Add(I18n.ModMismatch_Farmhand(modList: string.Join(",", reportData.MissingOnFarmhand)), LogLevel.Warn);

        if (reportData.VersionMismatch.Count > 0)
            report.Add(I18n.ModMismatch_Version(modList: string.Join(",", reportData.VersionMismatch)), LogLevel.Warn);

        this.Helper.Multiplayer.SendMessage(report, "MultiplayerReport", [this.ModManifest.UniqueID], [reportData.FarmhandId]);
        this.Helper.Data.WriteJsonFile("LatestMultiplayerModReport-Host.json", this.RawReports);
        this.PublishReport(reportData, report);
    }

    private void PublishReport(MultiplayerReportData? reportData, Dictionary<string, LogLevel> report)
    {
        if (report.Count > 0)
        {
            string preface = reportData == null
                ? I18n.FarmhandReport()
                : I18n.HostReport(farmhandId: reportData.FarmhandId, farmhandName: reportData.FarmhandName, connectionTime: reportData.TimeConnected);

            this.Monitor.Log(preface, this.Config.HideReportInTrace ? LogLevel.Trace : LogLevel.Warn);

            foreach ((string message, LogLevel logLevel) in report)
            {
                this.Monitor.Log(message, this.Config.HideReportInTrace ? LogLevel.Trace : logLevel);
            }

            this.Reports.Add($"{preface}\n--------------------------------\n{string.Join("\n", report.Keys)}");
            File.WriteAllText(Path.Combine(this.Helper.DirectoryPath, reportData == null ? "LatestMultiplayerModReports-Farmhand.txt" : "LatestMultiplayerModReports-Host.txt"), string.Join("\n\n", this.Reports));
        }
        else
        {
            string message = reportData == null
                ? I18n.SuccessfulConnectionFarmhandSide()
                : I18n.SuccessfulConnectionHostSide(farmhandId: reportData.FarmhandId, farmhandName: reportData.FarmhandName);

            this.Monitor.Log(message, LogLevel.Info);
        }
    }
}
