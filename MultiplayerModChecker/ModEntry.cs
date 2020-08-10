using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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
            Helper.Data.WriteJsonFile("LatestMultiplayerModReportRaw.json", _rawReports);
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
                    //Helper.Translation.Get("",new {})

                    if (!modData.DoesHostHave)
                    {
                        report.Add(Helper.Translation.Get("ModMismatch.Host", new { ModName = modData.ModName, ModID = modData.ModUniqueID }), LogLevel.Warn);
                    } else if (!modData.DoesFarmhandHave)
                    {
                        report.Add(Helper.Translation.Get("ModMismatch.Farmhand", new { ModName = modData.ModName, ModID = modData.ModUniqueID }), LogLevel.Warn);
                    } else if (!modData.HostModVersion.Equals(modData.FarmhandModVersion))
                    {
                        report.Add(Helper.Translation.Get("ModMismatch.Version",
                            new { ModName = modData.ModName, ModID = modData.ModUniqueID, HostVersion = modData.HostModVersion, FarmhandVersion = modData.FarmhandModVersion }),
                            LogLevel.Warn);
                    }
                }
            }

            Helper.Multiplayer.SendMessage(report,"MultiplayerReport",new []{ModManifest.UniqueID},new []{reportData.FarmhandID});
            PublishReport(reportData, report);
        }

        private void PublishReport(MultiplayerReportData reportData, Dictionary<string, LogLevel> report)
        {
            if (report.Count > 0)
            {
                string preface = "";
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
                File.WriteAllText(Path.Combine(Helper.DirectoryPath, "LatestMultiplayerModReports.txt"), string.Join("\n\n", _reports));
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

    public class Config
    {
        public String[] IgnoredMods { get; set; } = { "Cherry.MultiplayerModChecker" };
        public bool HideReportInTrace { get; set; } = false;
    }

    public class MultiplayerReportData
    {
        public string FarmhandName { get; set; }
        public DateTime TimeConnected { get; set; }
        public long FarmhandID { get; set; }

        public SmapiGameVersionDifference SmapiGameGameVersions { get; set; } = new SmapiGameVersionDifference();

        public List<ModVersions> Mods { get; set; } = new List<ModVersions>();
    }

    public class SmapiGameVersionDifference
    {
        public bool FarmhandHasSmapi { get; set; }
        public ISemanticVersion HostSmapiVersion { get; set; }
        public ISemanticVersion FarmhandSmapiVersion { get; set; }

    }

    public class ModVersions
    {
        public string ModName { get; set; }

        public string ModUniqueID { get; set; }
        public bool DoesHostHave { get; set; } = false;
        public bool DoesFarmhandHave { get; set; } = false;
        public ISemanticVersion HostModVersion { get; set; }
        public ISemanticVersion FarmhandModVersion { get; set; }
    }
}
