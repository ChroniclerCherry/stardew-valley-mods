using System;
using HarmonyLib;
using SnackAnything.Framework;
using StardewModdingAPI;
using StardewValley;
using Object = StardewValley.Object;

namespace SnackAnything
{
    class ModEntry : Mod
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mod configuration.</summary>
        private static ModConfig Config;


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<ModConfig>();

            var harmony = new Harmony(this.ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.pressActionButton)),
                prefix: new HarmonyMethod(this.GetType(), nameof(Before_Game1_PressActionButton)),
                postfix: new HarmonyMethod(this.GetType(), nameof(After_Game1_PressActionButton))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.eatObject)),
                prefix: new HarmonyMethod(this.GetType(), nameof(Before_Farmer_EatObject)),
                postfix: new HarmonyMethod(this.GetType(), nameof(After_Farmer_EatObject))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Mark the held object edible when pressing the action button, if applicable.</summary>
        /// <param name="__state">The previous object edibility, or <c>null</c> if it didn't change.</param>
        private static void Before_Game1_PressActionButton(out int? __state)
        {
            if (Config.HoldToActivate.IsDown())
                TryReplaceEdibility(Game1.player.ActiveObject, out __state);
            else
                __state = null;
        }

        /// <summary>Restore the held object's original edibility after handling the action button, if applicable.</summary>
        /// <param name="__state">The previous object edibility, or <c>null</c> if it didn't change.</param>
        private static void After_Game1_PressActionButton(int? __state)
        {
            if (Config.HoldToActivate.IsDown())
                TryRestoreEdibility(Game1.player.ActiveObject, __state);
        }

        /// <summary>Mark the held object edible when it's eaten, if applicable.</summary>
        /// <param name="o">The item being eaten.</param>
        /// <param name="__state">The previous object edibility, or <c>null</c> if it didn't change.</param>
        private static void Before_Farmer_EatObject(Object o, out int? __state)
        {
            TryReplaceEdibility(o, out __state);
        }

        /// <summary>Restore the held object's original edibility after the eat handler, and mark the item-to-eat edible if needed.</summary>
        /// <param name="o">The item being eaten.</param>
        /// <param name="__state">The previous object edibility, or <c>null</c> if it didn't change.</param>
        private static void After_Farmer_EatObject(Object o, int? __state)
        {
            TryRestoreEdibility(o, __state);

            Game1.player.itemToEat = Game1.player.itemToEat?.getOne();
            TryReplaceEdibility(Game1.player.itemToEat as Object, out _);
        }

        /// <summary>Make the item snackable if needed.</summary>
        /// <param name="obj">The object to change.</param>
        /// <param name="previousEdibility">The object's previous edibility, or <c>null</c> if it didn't change.</param>
        private static void TryReplaceEdibility(Object obj, out int? previousEdibility)
        {
            if (obj?.Edibility < 0 && (obj.Type != "Arch" || Config.YummyArtefacts))
            {
                previousEdibility = obj.Edibility;
                obj.Edibility = Math.Max(obj.Price / 3, 1);
            }
            else
                previousEdibility = null;
        }

        /// <summary>Restore the object's original edibility if applicable.</summary>
        /// <param name="obj">The object to change.</param>
        /// <param name="originalEdibility">The object's previous edibility, or <c>null</c> it wasn't changed.</param>
        private static void TryRestoreEdibility(Object obj, int? originalEdibility)
        {
            if (obj != null && originalEdibility != null)
                obj.Edibility = originalEdibility.Value;
        }
    }
}
