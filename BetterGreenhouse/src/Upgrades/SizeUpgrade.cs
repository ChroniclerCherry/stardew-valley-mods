using System.IO;
using StardewModdingAPI;
using xTile;

namespace BetterGreenhouse.Upgrades
{
    class SizeUpgrade : Upgrade, IAssetEditor
    {
        public override string UpgradeName { get; } = "SizeUpgrade";
        public override bool Active { get; set; } = false;
        public override bool DisableOnFarmhand { get; set; } = false;
        public override int Cost => State.Config.SizeUpgradeCost;

        private readonly string[] _mapExtensions = { ".xnb",".tin",".tmx" };

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
            foreach (var extension in _mapExtensions)
            {
                assetKey = Path.Combine(_helper.DirectoryPath, Consts.GreenhouseMapPath, extension);
                if (Directory.Exists(assetKey))
                    break;
            }
                
            var sourceMap = _helper.Content.Load<Map>(assetKey, ContentSource.ModFolder);
            mapEditor.PatchMap(sourceMap);
        }
    }
}
