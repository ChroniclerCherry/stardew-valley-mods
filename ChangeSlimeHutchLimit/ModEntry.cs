using ChangeSlimeHutchLimit.Framework;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace ChangeSlimeHutchLimit
{
    public class ModEntry : Mod
    {
        private static Config _config;
        public override void Entry(IModHelper helper)
        {
            _config = this.Helper.ReadConfig<Config>();

            this.Helper.ConsoleCommands.Add("SetSlimeHutchLimit", "Changes the max number of slimes that can inhabit a slime hutch.\n\nUsage: SetSlimeHutchLimit <value>\n- value: the number of slimes", this.ChangeMaxSlimes);

            Harmony harmony = new Harmony(this.ModManifest.UniqueID);
            harmony.Patch(AccessTools.Method(typeof(SlimeHutch), nameof(SlimeHutch.isFull)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.SlimeHutch_isFull_postfix)));

        }

        private static void SlimeHutch_isFull_postfix(GameLocation __instance, ref bool __result)
        {
            __result = __instance.characters.Count >= (_config?.MaxSlimesInHutch ?? 20);
        }

        private void ChangeMaxSlimes(string arg1, string[] arg2)
        {
            if (int.TryParse(arg2[0], out int newLimit))
            {
                _config.MaxSlimesInHutch = newLimit;
                this.Helper.WriteConfig(_config);
                this.Monitor.Log($"The new Slime limit is: {_config.MaxSlimesInHutch}");
            }
            else
            {
                this.Monitor.Log($"Invalid input.");
            }
        }
    }
}
