using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace CustomCraftingStation
{
    class PatchCraftingpage
    {
        private static IMonitor _monitor;
        private static ModEntry _instance;
        public static void Initialize(IMonitor monitor, ModEntry mod)
        {
            _monitor = monitor;
            _instance = mod;

            var harmony = HarmonyInstance.Create(_instance.ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Constructor(typeof(CraftingPage), new Type[] {
                                                                        typeof(int),
                                                                        typeof(int),
                                                                        typeof(int),
                                                                        typeof(int),
                                                                        typeof(bool),
                                                                        typeof(bool),
                                                                        typeof(List<Chest>)
               }),
               prefix: new HarmonyMethod(typeof(PatchCraftingpage), nameof(PatchCraftingpage.CraftingPageConstructor_Prefix)),
               postfix: new HarmonyMethod(typeof(PatchCraftingpage), nameof(PatchCraftingpage.CraftingPageConstructor_Postfix))
            );
        }
        public static bool CraftingPageConstructor_Prefix(bool cooking, ref bool __state)
        {
            try
            {
                __state = false; //this is set to true if I modify anything

                if (_instance.OpenedModdedStation)
                {
                    _instance.OpenedModdedStation = false;
                    return true;
                }

                __state = true;

                if (cooking)
                {
                    CraftingRecipe.cookingRecipes = CraftingRecipe.cookingRecipes.Intersect(_instance.ReducedCookingRecipes).ToDictionary(x => x.Key, x => x.Value);
                } else
                {
                    CraftingRecipe.craftingRecipes = CraftingRecipe.craftingRecipes.Intersect(_instance.ReducedCraftingRecipes).ToDictionary(x => x.Key, x => x.Value);
                }

            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CraftingPageConstructor_Prefix)}:\n{ex}", LogLevel.Error);
            }

            return true; // run original logic
        }

        public static void CraftingPageConstructor_Postfix(bool cooking, ref bool __state)
        {
            try
            {
                if (!__state)
                    return;

                if (cooking)
                {
                    CraftingRecipe.cookingRecipes = _instance.AllCookingRecipes;
                }
                else
                {
                    CraftingRecipe.craftingRecipes = _instance.AllCraftingRecipes;
                }


            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CraftingPageConstructor_Prefix)}:\n{ex}", LogLevel.Error);
            }
        }

    }
}
