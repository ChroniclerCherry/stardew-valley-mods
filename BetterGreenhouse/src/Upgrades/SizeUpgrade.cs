using System;
using Microsoft.Xna.Framework.Content;
using StardewModdingAPI;
using xTile;

namespace GreenhouseUpgrades.Upgrades
{
    public class SizeUpgrade : Upgrade, IAssetEditor, IAssetLoader
    {
        public override UpgradeTypes Type => UpgradeTypes.SizeUpgrade;
        public override bool Active { get; set; } = false;
        public override bool Unlocked { get; set; } = false;
        public override void Initialize(IModHelper helper, IMonitor monitor)
        {
            base.Initialize(helper,monitor);
            Helper.Content.AssetEditors.Add(this);
        }

        public override void Start()
        {
            base.Start();
            Helper.Content.InvalidateCache(Consts.GreenhouseMapPath);

        }

        public override void Stop()
        {
            Active = false;
            Helper.Content.InvalidateCache(Consts.GreenhouseMapPath);
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return Unlocked && Active && asset.AssetNameEquals(Consts.GreenhouseMapPath);
        }

        public void Edit<T>(IAssetData asset)
        {
            if (!Unlocked || !Active || !asset.AssetNameEquals(Consts.GreenhouseMapPath)) return;

            var mapEditor = asset.AsMap();
            var sourceMap =
                    Helper.Content.Load<Map>($"{Consts.GreenhouseMapPath}_Upgrade", ContentSource.GameContent);
                mapEditor.PatchMap(sourceMap);

            }

        public bool CanLoad<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals(Consts.GreenHouseSource);
        }

        public T Load<T>(IAssetInfo asset)
        {
            return (T)(object)Helper.Content.Load<Map>(Consts.GreenHouseSource);
        }
    }
}
