using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace StardewAquarium.src.Editors;
internal static class AssetEditor
{

    private static Dictionary<IAssetName, Action<IAssetData>> _handlers;

    internal static void Init(IGameContentHelper parser, IContentEvents events)
    {
        events.AssetRequested += Handle;
    }

    private static void Handle(object sender, AssetRequestedEventArgs e)
    {
        if (_handlers.TryGetValue(e.NameWithoutLocale, out Action<IAssetData> action))
        {
            e.Edit(action, AssetEditPriority.Late);
        }
    }
}
