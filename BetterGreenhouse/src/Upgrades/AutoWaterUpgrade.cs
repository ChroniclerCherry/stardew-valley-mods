using System.Linq;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace BetterGreenhouse.Upgrades
{
    public class AutoWaterUpgrade : Upgrade
    {
        public override UpgradeTypes Type => UpgradeTypes.AutoWaterUpgrade;
        public override string Name { get; } = "AutoWaterUpgrade";
        public override bool Active { get; set; } = false;
        public override bool DisableOnFarmhand { get; set; } = true;
        public override int Cost => State.Config.AutoWaterUpgradeCost;

        public override void Start()
        {
            if (!Context.IsMainPlayer && DisableOnFarmhand) return;
            if (!Active) return;

            _helper.Events.GameLoop.DayStarted += WaterAllGreenhouseDayStart;
            _helper.Events.GameLoop.DayEnding += WaterAllGreenhouseDayEnd;
        }

        public new void Stop()
        {
            base.Stop();
            _helper.Events.GameLoop.DayStarted -= WaterAllGreenhouseDayStart;
        }

        private void WaterAllGreenhouseDayStart(object sender, DayStartedEventArgs e)
        {
            WaterGreenhouse();
        }

        private void WaterAllGreenhouseDayEnd(object sender, DayEndingEventArgs e)
        {
            WaterGreenhouse();
        }

        private void WaterGreenhouse()
        {
            if (!Active) return;
            _monitor.Log($"{translatedName} : Watering the greenhouse");

            var greenhouse = Game1.getLocationFromName(Consts.GreenhouseMapName);
            foreach (var feature in greenhouse.terrainFeatures.Values)
            {
                if (feature is HoeDirt dirt)
                {
                    dirt.state.Value = HoeDirt.watered;
                }
            }

            //stole this from CJB cheats lol
            foreach (var pot in greenhouse.objects.Values.OfType<IndoorPot>())
            {
                var dirt = pot.hoeDirt.Value;
                if (dirt?.crop == null) continue;

                dirt.state.Value = HoeDirt.watered;
                pot.showNextIndex.Value = true;
            }
        }
    }
}
