using System.Collections;
using System.Runtime.CompilerServices;
using StardewModdingAPI;
using StardewValley;

namespace StardewAquarium.Tokens
{
    class TokenHandler
    {
        private readonly IModHelper _helper;
        private readonly IManifest _manifest;
        public TokenHandler(IModHelper helper, IManifest manifest)
        {
            _helper = helper;
            _manifest = manifest;
        }

        public void RegisterTokens()
        {
            var api = _helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");
            api.RegisterToken(_manifest, "Donated", Utils.GetDonatedFish);
            api.RegisterToken(_manifest, "NumDonated", Utils.GetNumDonatedFishRange);
        }
    }
}
