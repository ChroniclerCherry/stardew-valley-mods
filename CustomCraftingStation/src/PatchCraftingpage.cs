using Harmony;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CustomCraftingStation.src
{
    class PatchCraftingpage
    {
        private static IMonitor Monitor;
        private static ModEntry Instance;
        public static void Initialize(IMonitor monitor, ModEntry mod)
        {
            Monitor = monitor;
            Instance = mod;

            HarmonyInstance harmony = HarmonyInstance.Create(Instance.ModManifest.UniqueID);

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

                if (Instance.OpenedModdedStation)
                {
                    Instance.OpenedModdedStation = false;
                    return true;
                }

                __state = true;

                if (cooking)
                {
                    CraftingRecipe.cookingRecipes = CraftingRecipe.cookingRecipes.Intersect(Instance.ReducedCookingRecipes).ToDictionary(x => x.Key, x => x.Value);
                } else
                {
                    CraftingRecipe.craftingRecipes = CraftingRecipe.craftingRecipes.Intersect(Instance.ReducedCraftingRecipes).ToDictionary(x => x.Key, x => x.Value);
                }

            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(CraftingPageConstructor_Prefix)}:\n{ex}", LogLevel.Error);
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
                    CraftingRecipe.cookingRecipes = Instance.AllCookingRecipes;
                }
                else
                {
                    CraftingRecipe.craftingRecipes = Instance.AllCraftingRecipes;
                }


            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(CraftingPageConstructor_Prefix)}:\n{ex}", LogLevel.Error);
            }
        }

    }
}
