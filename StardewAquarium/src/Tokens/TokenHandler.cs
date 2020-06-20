using StardewModdingAPI;

namespace StardewAquarium.Tokens
{
    class TokenHandler
    {
        private IModHelper _helper;
        private IMonitor _monitor;
        private IManifest _manifest;
        public TokenHandler(IModHelper helper, IMonitor monitor, IManifest manifest)
        {
            _helper = helper;
            _monitor = monitor;
            _manifest = manifest;
        }

        public void RegisterTokens()
        {
            var api = _helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");

            //TODO: register a token for each tank
        }
    }
}
