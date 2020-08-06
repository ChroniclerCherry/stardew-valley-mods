using System;
using System.IO;
using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using xTile;

namespace BetterGreenhouse.Upgrades
{
    public class SizeUpgrade : Upgrade, IAssetEditor
    {
        public override UpgradeTypes Type => UpgradeTypes.SizeUpgrade;
        public override string Name { get; } = "SizeUpgrade";
        public override bool Active { get; set; } = false;
        public override bool Unlocked { get; set; } = false;
        public override int Cost => State.Config.SizeUpgradeCost;
        public override void Initialize(IModHelper helper, IMonitor monitor)
        {
            base.Initialize(helper,monitor);
            _helper.Content.AssetEditors.Add(this);
        }

        public override void Start()
        {
            if (!Unlocked) return;
            Active = true;
            _helper.Content.InvalidateCache(Consts.GreenhouseMapPath);

        }

        public override void Stop()
        {
            Active = false;
            _helper.Content.InvalidateCache(Consts.GreenhouseMapPath);
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return Unlocked && Active && asset.AssetNameEquals(Consts.GreenhouseMapPath);
        }

        public void Edit<T>(IAssetData asset)
        {
            if (!Unlocked || !Active || !asset.AssetNameEquals(Consts.GreenhouseMapPath)) return;

            var mapEditor = asset.AsMap();
            var sourceMap = _helper.Content.Load<Map>($"{Consts.GreenhouseMapPath}_Upgrade", ContentSource.GameContent);
            mapEditor.PatchMap(sourceMap);
        }

    }
}
