using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley.GameData.Locations;

namespace StardewAquarium.src.Editors;
internal static class AssetEditor
{

    private static Dictionary<IAssetName, Action<IAssetData>> _handlers;
    private static Dictionary<IAssetName, string> _textureLoaders;
    private static IMonitor Monitor;

    internal static void Init(IGameContentHelper parser, IContentEvents events, IMonitor monitor)
    {
        Monitor = monitor;
        events.AssetRequested += Handle;
        _handlers[parser.ParseAssetName("Data/Locations")] = EditDataLocations;

        _textureLoaders[parser.ParseAssetName("Mods/StardewAquarium/Shirts")] = "assets/shirts.png";
        _textureLoaders[parser.ParseAssetName("Mods/StardewAquarium/Items")] = "assets/items.png";

    }

    private static void Handle(object sender, AssetRequestedEventArgs e)
    {
        if (_handlers.TryGetValue(e.NameWithoutLocale, out Action<IAssetData> action))
        {
            e.Edit(action, AssetEditPriority.Late);
        }
        if (_textureLoaders.TryGetValue(e.NameWithoutLocale, out string path))
        {
            e.LoadFromModFile<Texture2D>(path, AssetLoadPriority.Exclusive);
        }
    }

    #region editors

    /// <summary>
    /// copies over 1.5 data from beach data.
    /// </summary>
    /// <param name="asset"></param>
    private static void EditDataLocations(IAssetData asset)
    {
        var data = asset.AsDictionary<string, LocationData>().Data;
        if (!data.TryGetValue("Beach", out var beachData))
        {
            Monitor.Log("Beach data seems missing, cannot copy.", LogLevel.Warn);
            return;
        }

        if (!data.TryGetValue(ModEntry.Data.ExteriorMapName, out var museumData))
        {
            Monitor.Log("MuseumExterior data seems missing, cannot copy.", LogLevel.Warn);
            return;
        }

        museumData.ArtifactSpots ??= [];
        museumData.ArtifactSpots.AddRange(beachData.ArtifactSpots ?? Enumerable.Empty<ArtifactSpotDropData>());

        if (beachData.FishAreas is not null)
        {
            museumData.FishAreas ??= [];
            foreach (var (key, fisharea) in beachData.FishAreas)
            {
                beachData.FishAreas.TryAdd(key, fisharea);
            }
        }

        museumData.Fish ??= [];
        museumData.Fish.AddRange(beachData.Fish ?? Enumerable.Empty<SpawnFishData>());

        museumData.Forage ??= [];
        museumData.Forage.AddRange(beachData.Forage ?? Enumerable.Empty<SpawnForageData>());

    }

    #endregion
}
