using StardewModdingAPI.Events;

namespace StardewAquarium.Editors
{
    internal interface IAssetManager
    {
        bool TryHandleAsset(AssetRequestedEventArgs e);
    }
}
