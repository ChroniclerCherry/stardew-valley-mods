using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MultiplayerModChecker.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace MultiplayerModChecker
{
    public class ModEntry : Mod
    {
        private List<MultiplayerReportData> _rawReports = new List<MultiplayerReportData>();
        private readonly List<string> _reports = new List<string>();
        private Config _config;
        public override void Entry(IModHelper helper)
        {
            Helper.Events.Multiplayer.PeerContextReceived += PeerConnected;
            Helper.Events.Multiplayer.ModMessageReceived += Multiplayer_ModMessageReceived;
            _config = Helper.ReadConfig<Config>();
        }

        private void PeerConnected(object sender, PeerContextReceivedEventArgs e)
        {
            if (!Context.IsMainPlayer) return;

            var report = new MultiplayerReportData();
            report.TimeConnected = DateTime.Now;

            report.SmapiGameGameVersions.FarmhandHasSmapi = e.Peer.HasSmapi;

            report.FarmhandID = e.Peer.PlayerID;
            report.FarmhandName = Game1.getAllFarmers()
                .FirstOrDefault(f => f.UniqueMultiplayerID == e.Peer.PlayerID)
                ?.Name;

            if (string.IsNullOrEmpty(report.FarmhandName))
            {
                report.FarmhandName = Helper.Translation.Get("UnnamedFarmhand");
            }

            if (e.Peer.HasSmapi)
            {
                report.SmapiGameGameVersions.HostSmapiVersion = Constants.ApiVersion;
                report.SmapiGameGameVersions.FarmhandSmapiVersion = e.Peer.ApiVersion;

                var HostMods = Helper.ModRegistry.GetAll().Select(m => m.Manifest.UniqueID);
                var farmHandMods = e.Peer.Mods.Select(m => m.ID);
                var allMods = HostMods.Union(farmHandMods).Distinct();

                foreach (var mod in allMods)
                {
                    if (_config.IgnoredMods.Contains(mod)) continue;

                    var modVersionData = new ModVersions();

                    var hostMod = Helper.ModRegistry.Get(mod);
                    if (hostMod != null)
                    {
                        modVersionData.DoesHostHave = true;
                        modVersionData.HostModVersion = hostMod.Manifest.Version;
                    }


                    var farmhandMod = e.Peer.GetMod(mod);
                    if (farmhandMod != null)
                    {
                        modVersionData.DoesFarmhandHave = true;
                        modVersionData.FarmhandModVersion = farmhandMod.Version;
                    }

                    if (hostMod != null) modVersionData.ModName = hostMod.Manifest.Name;
                    else if (farmhandMod != null) modVersionData.ModName = farmhandMod.Name;

                    modVersionData.ModUniqueID = mod;
                    report.Mods.Add(modVersionData);
                }
            }

            _rawReports.Add(report);
            GenerateReport(report);
        }

        private void Multiplayer_ModMessageReceived(object sender, StardewModdingAPI.Events.ModMessageReceivedEventArgs e)
        {
            if (e.FromModID != ModManifest.UniqueID && e.Type != "MultiplayerReport") return;
            var reportData = e.ReadAs<Dictionary<string, LogLevel>>();

            PublishReport(null, reportData);
        }

        private void GenerateReport(MultiplayerReportData reportData)
        {
            Dictionary<string,LogLevel> report = new Dictionary<string, LogLevel>();

            if (!reportData.SmapiGameGameVersions.FarmhandHasSmapi)
            {
                report.Add(Helper.Translation.Get("FarmhandMissingSMAPI", new { FarmhandID = reportData.FarmhandID, FarmHandName = reportData.FarmhandName, ConnectionTime = reportData.TimeConnected }), LogLevel.Alert);
            }
            else
            {
                if (!reportData.SmapiGameGameVersions.HostSmapiVersion.Equals(reportData.SmapiGameGameVersions.FarmhandSmapiVersion))
                    report.Add(Helper.Translation.Get("SMAPIVersionMismatch",
                            new { HostVersion = reportData.SmapiGameGameVersions.HostSmapiVersion, FarmhandVersion = reportData.SmapiGameGameVersions.FarmhandSmapiVersion }),
                        LogLevel.Warn);

                foreach (var modData in reportData.Mods)
                {
                    if (!modData.DoesHostHave)
                    {
                        reportData.MissingOnHost.Add($"{modData.ModName} ({modData.ModUniqueID})");
                    } else if (!modData.DoesFarmhandHave)
                    {
                        reportData.MissingOnFarmhand.Add($"{modData.ModName} ({modData.ModUniqueID})");
                    } else if (!modData.HostModVersion.Equals(modData.FarmhandModVersion))
                    {
                        reportData.VersionMismatch.Add(Helper.Translation.Get("ModMismatch.Version.Each", new { ModName = modData.ModName, ModID = modData.ModUniqueID, HostVersion = modData.HostModVersion, FarmhandVersion = modData.FarmhandModVersion }));
                    }
                }
            }

            if (reportData.MissingOnHost.Count > 0)
                report.Add(Helper.Translation.Get("ModMismatch.Host",new { ModList = string.Join(",",reportData.MissingOnHost) }), LogLevel.Warn);

            if (reportData.MissingOnFarmhand.Count > 0)
                report.Add(Helper.Translation.Get("ModMismatch.Farmhand", new { ModList = string.Join(",", reportData.MissingOnFarmhand) }), LogLevel.Warn);

            if (reportData.VersionMismatch.Count > 0)
                report.Add(Helper.Translation.Get("ModMismatch.Version", new { ModList = string.Join(",", reportData.VersionMismatch) }), LogLevel.Warn);

            Helper.Multiplayer.SendMessage(report,"MultiplayerReport",new []{ModManifest.UniqueID},new []{reportData.FarmhandID});
            Helper.Data.WriteJsonFile("LatestMultiplayerModReport-Host.json", _rawReports);
            PublishReport(reportData, report);
        }

        private void PublishReport(MultiplayerReportData reportData, Dictionary<string, LogLevel> report)
        {
            string preface = "";
            if (report.Count > 0)
            {
                if (reportData == null)
                {
                    preface = Helper.Translation.Get("FarmhandReport");
                    Monitor.Log(preface,
                        _config.HideReportInTrace ? LogLevel.Trace : LogLevel.Warn);
                }
                else
                {
                    preface = Helper.Translation.Get("HostReport", new { FarmhandID = reportData.FarmhandID, FarmhandName = reportData.FarmhandName, ConnectionTime = reportData.TimeConnected });
                    Monitor.Log(preface,
                        _config.HideReportInTrace ? LogLevel.Trace : LogLevel.Warn);
                }
                
                foreach (var log in report)
                {
                    Monitor.Log(log.Key, _config.HideReportInTrace ? LogLevel.Trace : log.Value);
                }

                _reports.Add($"{preface}\n--------------------------------\n{string.Join("\n", report.Keys)}");
                File.WriteAllText(Path.Combine(Helper.DirectoryPath, reportData == null ? "LatestMultiplayerModReports-Farmhand.txt" : "LatestMultiplayerModReports-Host.txt"), string.Join("\n\n", _reports));
            }
            else
            {
                if (reportData == null)
                {
                    Monitor.Log(Helper.Translation.Get("SuccessfulConnectionFarmhandside"),
                        LogLevel.Info);
                }
                else
                {
                    Monitor.Log(Helper.Translation.Get("SuccessfulConnectionHostSide", new { FarmhandID = reportData.FarmhandID, FarmhandName = reportData.FarmhandName }),
                        LogLevel.Info);
                }

            }
        }
    }
}
