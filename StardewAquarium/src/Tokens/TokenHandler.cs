using System.Collections.Generic;
using System.Linq;
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
            api.RegisterToken(_manifest, "Donated", GetDonatedFish);
            api.RegisterToken(_manifest, "NumDonated", GetNumDonatedFishRange);
        }

        private static IEnumerable<string> GetDonatedFish()
        {
            return from mail in Game1.MasterPlayer.mailReceived
                where mail.StartsWith("AquariumDonated:")
                select mail.Split(':')[1];
        }

        private static IEnumerable<string> GetNumDonatedFishRange()
        {
            for (int i = 1; i < Utils.GetNumDonatedFish(); i++)
            {
                yield return i.ToString();
            }
        }
    }
}
