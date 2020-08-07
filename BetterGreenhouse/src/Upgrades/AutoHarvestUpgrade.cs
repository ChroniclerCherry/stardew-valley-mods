using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;

namespace BetterGreenhouse.Upgrades
{
    class AutoHarvestUpgrade : Upgrade
    {
        public override UpgradeTypes Type { get; } = UpgradeTypes.AutoHarvestUpgrade;
        public override bool Active { get; set; } = false;
        public override bool Unlocked { get; set; } = false;

        public override bool DisableOnFarmhand { get; } = true;

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            foreach (var location in Game1.locations)
            {
                if (location.IsGreenhouse)
                    Harvest(location);
            }
        }

        private void Harvest(GameLocation greenhouse)
        {
            var chests = GetChests(greenhouse).ToArray();
            if (!chests.Any())
            {
                _monitor.Log($"To enable auto harvesting in {greenhouse.Name}, place a chest on the map", LogLevel.Info);
                return;
            }

            var items = Game1.player.items;
            int maxItems = Game1.player.maxItems;
            Game1.player.MaxItems = chests.Length * Chest.capacity;
            var objects = new NetObjectList<Item>();

            for (int i = 0; i < Game1.player.MaxItems * 10; i++)
            {
                objects.Add(null);
            }

            _helper.Reflection.GetField<NetObjectList<Item>>(Game1.player,"items").SetValue(objects);

            foreach (var terrains in greenhouse.terrainFeatures)
            {
                foreach (var terrain in terrains)
                {
                    if (terrain.Value is HoeDirt dirt)
                    {
                        AttemptHarvest(greenhouse, dirt, terrain);
                    }
                }
            }

            CollectDebris(chests, greenhouse);

            foreach (var item in objects)
            {
                AttemptToAddToChest(chests, item, greenhouse);
            }

            _helper.Reflection.GetField<NetObjectList<Item>>(Game1.player, "items").SetValue(items);
        }

        private void AttemptHarvest(GameLocation greenhouse, HoeDirt dirt,
            KeyValuePair<Vector2, TerrainFeature> terrain)
        {
            if (dirt.crop != null && dirt.readyForHarvest())
                if (dirt.crop.harvest((int) terrain.Key.X, (int) terrain.Key.Y, dirt, null))
                    dirt.destroyCrop(terrain.Key, false, greenhouse);
        }

        private void CollectDebris(Chest[] chests, GameLocation greenhouse)
        {
            //collect any generated debris from harvests
            foreach (var obj in greenhouse.debris)
            {
                AttemptToAddToChest(chests, obj.item, greenhouse);
            }
        }

        private void AttemptToAddToChest(Chest[] chests, Item item, GameLocation greenhouse)
        {
            if (item == null) return;
            Item tempItem = item;
            foreach (var chest in chests)
            {
                tempItem = chest.addItem(tempItem);
                if (tempItem == null) return;
            }

            Game1.createItemDebris(tempItem, chests.Last().TileLocation, 0, greenhouse);
        }

        private IEnumerable<Chest> GetChests(GameLocation greenhouse)
        {
            foreach (var objects in greenhouse.Objects)
            {
                foreach (var obj in objects)
                {
                    if (obj.Value is Chest chest)
                        yield return chest;
                }
            }
        }

        public override void Start()
        {
            if (!Context.IsMainPlayer && DisableOnFarmhand) return;
            if (!Unlocked) return;
            Active = true;
            _helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
        }


        public override void Stop()
        {
            Active = false;
            _helper.Events.GameLoop.DayStarted -= GameLoop_DayStarted;
        }
    }
}
