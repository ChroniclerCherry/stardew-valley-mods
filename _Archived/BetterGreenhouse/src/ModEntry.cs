using GreenhouseUpgrades.Interaction;
using Harmony;
using StardewModdingAPI;

namespace GreenhouseUpgrades
{
    public class ModEntry : Mod
    {
        private InteractionDetection _interactionDetection;
        private Commands _commands;
        public override void Entry(IModHelper helper)
        {
            Main.Initialize(Helper,Monitor);
            Consts.ModUniqueID = ModManifest.UniqueID;
            Consts.Harmony = HarmonyInstance.Create(ModManifest.UniqueID);
            _interactionDetection = new InteractionDetection(Helper, Monitor);
            _commands = new Commands(Helper, Monitor);
        }
    }
}
