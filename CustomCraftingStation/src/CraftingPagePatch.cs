using Harmony;
using StardewModdingAPI;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using StardewValley.Objects;

namespace CustomCraftingStation
{
    class CraftingPagePatch
    {
        private static IMonitor _monitor;
        private static ModEntry _mod;
        public static void Initialize(IMonitor monitor, ModEntry mod)
        {
            _monitor = monitor;
            _mod = mod;

            var harmony = HarmonyInstance.Create(_mod.ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Constructor(typeof(CraftingPage), new[] {
                                                                        typeof(int),
                                                                        typeof(int),
                                                                        typeof(int),
                                                                        typeof(int),
                                                                        typeof(bool),
                                                                        typeof(bool),
                                                                        typeof(List<Chest>)
               }),
               postfix: new HarmonyMethod(typeof(CraftingPagePatch), nameof(CraftingPagePatch.CraftingPageConstructor_Postfix))
            );
        }

        public static void CraftingPageConstructor_Postfix(bool cooking, CraftingPage __instance)
        {
            try
            {
                if (_mod.OpenedModdedStation)
                {
                    _mod.OpenedModdedStation = false;
                    return;
                }

                var layoutRecipes = _mod.Helper.Reflection.GetMethod(__instance, "layoutRecipes");
                layoutRecipes.Invoke(cooking ? _mod.ReducedCookingRecipes : _mod.ReducedCraftingRecipes);

            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CraftingPageConstructor_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }
    }
}
