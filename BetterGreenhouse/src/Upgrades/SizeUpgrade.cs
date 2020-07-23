using System.IO;
using Newtonsoft.Json;
using StardewModdingAPI;
using xTile;

namespace BetterGreenhouse.Upgrades
{
    public class SizeUpgrade : Upgrade, IAssetEditor
    {
        public override UpgradeTypes Type => UpgradeTypes.SizeUpgrade;
        public override string Name { get; } = "SizeUpgrade";
        public override bool Active { get; set; } = false;
        public override bool DisableOnFarmhand { get; set; } = false;
        public override int Cost => State.Config.SizeUpgradeCost;

        private readonly string[] _mapExtensions = { ".xnb",".tbin",".tmx" };

        public override void Start()
        {
            if (_helper.Content.AssetEditors.Contains(this))
            {
                _monitor.Log("Tried to add SizeUpgrade twice?");
                return;
            }

            if (Active)
                _helper.Content.AssetEditors.Add(this);
        }

        public new void Stop()
        {
            Active = false;
            _helper.Content.AssetEditors.Remove(this);
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return Active && asset.AssetNameEquals(Consts.GreenhouseMapPath);
        }

        public void Edit<T>(IAssetData asset)
        {
            if (!Active || !asset.AssetNameEquals(Consts.GreenhouseMapPath)) return;

            var mapEditor = asset.AsMap();
            string assetKey = null;

            bool fileExists = false;
            foreach (var extension in _mapExtensions)
            {
                assetKey = Path.Combine(_helper.DirectoryPath, Consts.GreenhouseUpgradePath + extension);
                if (File.Exists(assetKey))
                {
                    assetKey = Consts.GreenhouseUpgradePath + extension; //gets rid of absolute pathing for smapi
                    fileExists = true;
                    break;
                }
            }

            if (!fileExists)
            {
                _monitor.Log("No map file was found. Please make sure there is a GreenhouseUpgrade map file in the assets folder. If there isn't, redownload this mod or follow instructions on the mod page for adding a custom greenhouse.", LogLevel.Error);
                return;
            }
                
            var sourceMap = _helper.Content.Load<Map>(assetKey, ContentSource.ModFolder);
            mapEditor.PatchMap(sourceMap);
        }
    }
}
