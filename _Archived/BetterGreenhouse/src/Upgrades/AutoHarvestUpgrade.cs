using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace GreenhouseUpgrades.Upgrades
{
    class AutoHarvestUpgrade : Upgrade
    {
        public override UpgradeTypes Type { get; } = UpgradeTypes.AutoHarvestUpgrade;
        public override bool Active { get; set; } = false;
        public override bool Unlocked { get; set; } = false;
        public override bool DisableOnFarmhand { get; } = true;

        private static bool IsHarvesting = false;

        private static GameLocation currentHarvestingMap;

        private static bool _patchesApplied = false;

        private void Patch()
        {
            _patchesApplied = true;
            //this stops the 456342564256 harvests from spamming sound and new item notifications
            Consts.Harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.playSound)),
                prefix: new HarmonyMethod(typeof(AutoHarvestUpgrade), nameof(AutoHarvestUpgrade.DisableWhenHarvesting)));

            Consts.Harmony.Patch(
                original: AccessTools.Method(typeof(DelayedAction), nameof(DelayedAction.playSoundAfterDelay)),
                prefix: new HarmonyMethod(typeof(AutoHarvestUpgrade), nameof(AutoHarvestUpgrade.DisableWhenHarvesting)));

            Consts.Harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.addHUDMessage)),
                prefix: new HarmonyMethod(typeof(AutoHarvestUpgrade), nameof(AutoHarvestUpgrade.DisableWhenHarvesting)));

            Consts.Harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.createItemDebris)),
                prefix: new HarmonyMethod(typeof(AutoHarvestUpgrade), nameof(AutoHarvestUpgrade.Game1_createItemDebris_prefix)));
        }

        public static void Game1_createItemDebris_prefix(ref GameLocation location)
        {
            if (!IsHarvesting) return;
            location = currentHarvestingMap;
        }

        public static bool DisableWhenHarvesting()
        {
            return !IsHarvesting;
        }

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
            IsHarvesting = true;
            currentHarvestingMap = greenhouse;

            try
            {
                var chests = GetChests(greenhouse).ToArray();
                if (!chests.Any())
                {
                    Monitor.Log($"To enable auto harvesting in {greenhouse.Name}, place a chest on the map",
                        LogLevel.Info);
                    return;
                }

                var items = Game1.player.items;
                int maxItems = Game1.player.MaxItems;
                Game1.player.MaxItems = chests.Length * Chest.capacity;
                var objects = new NetObjectList<Item>();

                for (int i = 0; i < Game1.player.MaxItems * 10; i++)
                {
                    objects.Add(null);
                }

                Helper.Reflection.GetField<NetObjectList<Item>>(Game1.player, "items").SetValue(objects);

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

                foreach (var item in objects)
                {
                    AttemptToAddToChest(chests, item, greenhouse);
                }

                CollectDebris(chests, greenhouse);

                Game1.player.MaxItems = maxItems;
                Helper.Reflection.GetField<NetObjectList<Item>>(Game1.player, "items").SetValue(items);
                CollectDebris(chests, greenhouse);
            }
            catch (Exception e)
            {
                Monitor.Log(e.ToString(),LogLevel.Error);
            }

            IsHarvesting = false;
            currentHarvestingMap = null;
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
            foreach (var obj in greenhouse.debris)
            {
                AttemptToAddToChest(chests, obj.item.getOne(), greenhouse);
            }
        }

        private static void AttemptToAddToChest(Chest[] chests, Item item, GameLocation greenhouse)
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

        private static IEnumerable<Chest> GetChests(GameLocation greenhouse)
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
            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            if (!_patchesApplied)
                Patch();
        }

        public override void Stop()
        {
            Active = false;
        }
    }
}
