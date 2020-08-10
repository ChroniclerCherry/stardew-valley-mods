using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewValley;

namespace MultiplayerModChecker
{
    public class ModEntry : Mod
    {
        private IMultiplayerPeer _host;
        private List<MultiplayerReportData> _rawReports = new List<MultiplayerReportData>();
        private List<string> _reports = new List<string>();
        private Config Config;
        public override void Entry(IModHelper helper)
        {
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.Multiplayer.PeerContextReceived += Multiplayer_PeerContextReceived;
            Helper.Events.Multiplayer.ModMessageReceived += Multiplayer_ModMessageReceived;
            Config = Helper.ReadConfig<Config>();
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            if (!Context.IsMultiplayer || !Context.IsMainPlayer) return;
            _host = Helper.Multiplayer.GetConnectedPlayers().First(p => p.IsHost);
        }

        private void Multiplayer_PeerContextReceived(object sender, StardewModdingAPI.Events.PeerContextReceivedEventArgs e)
        {
            if (!Context.IsMainPlayer) return;

            var report = new MultiplayerReportData();
            report.TimeConnected = DateTime.Now;

            report.SmapiGameGameVersions.FarmhandHasSmapi = e.Peer.HasSmapi;
            report.SmapiGameGameVersions.HostSmapiVersion = _host.ApiVersion;
            report.SmapiGameGameVersions.HostGameVersion = _host.GameVersion;
            report.SmapiGameGameVersions.FarmhandSmapiVersion = e.Peer.ApiVersion;
            report.SmapiGameGameVersions.FarmhandGameVersion = e.Peer.GameVersion;

            report.FarmhandID = e.Peer.PlayerID;
            report.FarmhandName = Game1.getFarmer(e.Peer.PlayerID).Name;

            var allMods = _host.Mods.Union(e.Peer.Mods).Select(m => m.ID).Distinct();

            foreach (var mod in allMods)
            {
                if (Config.IgnoredMods.Contains(mod)) continue;

                var modVersionData = new ModVersions();
                modVersionData.ModUniqueID = mod;

                var hostMod = _host.GetMod(mod);
                if (hostMod != null)
                {
                    modVersionData.DoesHostHave = true;
                    modVersionData.HostModVersion = hostMod.Version;
                }
                

                var farmhandMod = e.Peer.GetMod(mod);
                if (farmhandMod != null)
                {
                    modVersionData.DoesFarmhandHave = true;
                    modVersionData.FarmhandModVersion = farmhandMod.Version;
                }

                if (hostMod != null) modVersionData.ModName = hostMod.Name;
                else if (farmhandMod != null) modVersionData.ModName = farmhandMod.Name;

                report.Mods.Add(modVersionData);
            }

            _rawReports.Add(report);
            Helper.Data.WriteJsonFile("MultiplayerModReportRaw", report);
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
                report.Add(Helper.Translation.Get("FarmhandMissingSMAPI"), LogLevel.Alert);
            }
            else
            {
                if (reportData.SmapiGameGameVersions.HostGameVersion != reportData.SmapiGameGameVersions.FarmhandGameVersion)
                    report.Add(Helper.Translation.Get("GameVersionMismatch",
                            new { HostVersion = reportData.SmapiGameGameVersions.HostGameVersion, FarmhandVersion = reportData.SmapiGameGameVersions.FarmhandGameVersion }),
                        LogLevel.Warn);

                if (reportData.SmapiGameGameVersions.HostSmapiVersion != reportData.SmapiGameGameVersions.FarmhandSmapiVersion)
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
                    } else if (modData.HostModVersion != modData.FarmhandModVersion)
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
                if (reportData == null)
                {
                    Monitor.Log(Helper.Translation.Get("FarmhandReport"),
                        Config.HideReportInTrace ? LogLevel.Trace : LogLevel.Warn);
                }
                else
                {
                    Monitor.Log( Helper.Translation.Get("HostReport",new { FarmhandID = reportData.FarmhandID, FarmhandName = reportData.FarmhandName, ConnectionTime = reportData.TimeConnected }),
                        Config.HideReportInTrace ? LogLevel.Trace : LogLevel.Warn);
                }
                
                foreach (var log in report)
                {
                    Monitor.Log(log.Key, Config.HideReportInTrace ? LogLevel.Trace : log.Value);
                }

                _reports.Add(String.Join("\n", report.Keys));
                Helper.Data.WriteJsonFile("MultiplayerModReports", _reports);
            }
            else
            {
                if (reportData == null)
                {

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
        public String[] IgnoredMods { get; set; }
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

        public ISemanticVersion HostGameVersion { get; set; }
        public ISemanticVersion FarmhandGameVersion { get; set; }
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
