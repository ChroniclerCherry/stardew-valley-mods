using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using Netcode;
using StardewValley.Network;
using System.Collections.Generic;
using System.Linq;

namespace ShopTileFramework.Utility
{
    /// <summary>
    /// This class contains utility methods to do all the condition checking used by shops
    /// </summary>
    internal static class ConditionChecking
    {
        /// <summary>
        /// This is the vanilla method used to check preconditions
        /// Borriwing it to gain quick access to all existing preconditions
        /// </summary>
        public static IReflectedMethod VanillaPreconditionsMethod { get; private set; }

        /// <summary>
        /// Each string in the array can have multiple conditions seperated by the character '/'
        /// and each string is evaluated as true if all of the conditions are met
        /// The method returns true if any of the strings are true
        /// </summary>
        /// <param name="conditions">an array of condition strings</param>
        /// <returns>true if all conditions matches, otherwise false</returns>
        internal static bool CheckConditions(string[] conditions)
        {
            //if no conditions are supplied, then conditions are always met
            if (conditions == null)
                return true;

            VanillaPreconditionsMethod = ModEntry.helper.Reflection.GetMethod(Game1.currentLocation, "checkEventPrecondition");

            //if someone somewhow marked this fake ID as seen, 
            //unmark it so condition checking will actually work
            if (Game1.player.eventsSeen.Contains(-5005))
            {
                ModEntry.monitor.Log("STF uses the fake event ID of -5005 in order to use vanilla preconditions." +
                    " Somehow your save has marked this ID as seen. STF is freeing it back up.", LogLevel.Warn);
                Game1.player.eventsSeen.Remove(-5005);
            }

            //if any of the conditions are met, return true
            foreach (var con in conditions)
            {
                if (ModEntry.VerboseLogging)
                    ModEntry.monitor.Log($"Checking condition string: \"{con}\"", LogLevel.Debug);

                if (CheckIndividualConditions(con.Split('/')))
                {
                    if (ModEntry.VerboseLogging)
                        ModEntry.monitor.Log($"\tPlayer met the conditions: \"{con}\"", LogLevel.Debug);
                    return true;
                }
            }

            if (ModEntry.VerboseLogging)
                ModEntry.monitor.Log($"No conditions were met", LogLevel.Debug);

            //if no conditions are met, return false
            return false;
        }

        /// <summary>
        /// This method takes an array which was originally a single string condition, split at the '/' symbols
        /// If any of the conditions are false, then the entire expression evaluates as false
        /// If all conditions pass, then the whole expression evaluates as true
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        private static bool CheckIndividualConditions(string[] conditions)
        {
            //if any of the conditions fail, return false
            foreach (var condition in conditions)
            {
                if (ModEntry.VerboseLogging)
                    ModEntry.monitor.Log($"\t\tChecking individual condition: {condition}", LogLevel.Trace);
                //if condition starts with a ! return false if condition checking is true
                if (condition[0] == '!')
                {
                    if (CheckCustomConditions(condition.Substring(1)))
                    {
                        if (ModEntry.VerboseLogging)
                            ModEntry.monitor.Log($"\tFailed individual condition: {condition}", LogLevel.Trace);
                        return false;
                    }
                        

                }
                else if (!CheckCustomConditions(condition))
                {
                    if (ModEntry.VerboseLogging)
                        ModEntry.monitor.Log($"\t\tFailed individual condition: {condition}", LogLevel.Trace);

                    return false;
                }
                    
            }

            if (ModEntry.VerboseLogging)
                ModEntry.monitor.Log($"\t\tPassed all conditions: {conditions}", LogLevel.Trace);
            //passed all conditions
            return true;

        }

        /// <summary>
        /// takes a singular condition and evaluates it for true or false.
        /// If it matches none of the custom conditions defined by STF, it then checks the vanilla preconditions
        /// </summary>
        /// <param name="con"></param>
        /// <returns></returns>
        private static bool CheckCustomConditions(string con)
        {
            string[] conditionParams = con.Split(' ');
            //the first parameter at 0 is the command of which condition to check
            switch (conditionParams[0])
            {
                case "NPCAt":
                    return CheckNPCAt(conditionParams);
                case "HasMod":
                    return CheckHasMod(conditionParams);
                case "SkillLevel":
                    return CheckSkillLevel(conditionParams);
                case "CommunityCenterComplete":
                    return Game1.MasterPlayer.mailReceived.Contains("ccIsComplete") || Game1.MasterPlayer.hasCompletedCommunityCenter();
                case "JojaMartComplete":
                    return CheckJojaMartComplete();
                case "SeededRandom":
                    return CheckSeededRandom(conditionParams);
                case "HasCookingRecipe":
                    return CheckHasRecipe(conditionParams,Game1.player.cookingRecipes);
                case "HasCraftingRecipe":
                    return CheckHasRecipe(conditionParams,Game1.player.craftingRecipes);
                case "FarmHouseUpgradeLevel":
                    return CheckFarmHouseUpgrade(conditionParams);
                default:
                    // Note: "-5005" is a random event id cause the vanilla method is for events and needs one ¯\_(ツ)_/¯
                    // so it's the negative mod id
                    return (VanillaPreconditionsMethod.Invoke<int>("-5005/" + con) != -1);
            }
        }

        private static bool CheckFarmHouseUpgrade(string[] conditionParams)
        {

            for (int i = 1; i < conditionParams.Length; i++)
            {
                if (int.Parse(conditionParams[i]) == Game1.player.HouseUpgradeLevel)
                    return true;
            }

            return false;
        }

        private static bool CheckHasRecipe(string[] conditionParams,
            NetStringDictionary<int, NetInt> craftingRecipes)
        {

            for (int i = 1; i < conditionParams.Length; i++)
            {
                if (!(from rec in craftingRecipes.Keys
                      select rec.Replace(" ", "-")).Contains(conditionParams[i]))
                    return false;
            }

            return true;
        }


        public static bool CheckSeededRandom(string[] conditionParams)
        {
            int offset = Convert.ToInt32(conditionParams[1]);
            string timePeriod = conditionParams[2];
            if (!int.TryParse(timePeriod, out var interval))
            {
                switch (timePeriod)
                {
                    case "Day":
                        interval = 1;
                        break;
                    case "Week":
                        interval = 7;
                        break;
                    case "Season":
                    case "Month":
                        interval = 28;
                        break;
                    case "Year":
                        interval = 112;
                        break;
                    case "Game":
                        interval = 0;
                        break;
                }
            }

            float lowerCheck = Convert.ToSingle(conditionParams[3]);
            float higherCheck = Convert.ToSingle(conditionParams[4]);

            if (interval != 0)
            {
                interval = (int)((Game1.MasterPlayer.stats.daysPlayed-1) / interval);
            }
            

            ulong seed = Game1.uniqueIDForThisGame + (ulong) offset + (ulong) interval;

            var rng = new Random((int) seed);
            double roll = rng.NextDouble();
            return (lowerCheck <= roll && roll < higherCheck);

        }

        /// <summary>
        /// Checks that a given NPC is at the tile indexes
        /// </summary>
        /// <param name="conditionParams">The condition string split by spaces, with the first parameter being the NPC 
        /// name and every two after that is the X and Y coordinates</param>
        /// <returns>true if the npc was found at the given tile, false if not</returns>
        private static bool CheckNPCAt(string[] conditionParams)
        {
            //the second paramter at index 1 is the name of an npc
            var npc = Game1.getCharacterFromName(conditionParams[1]);

            //after that the expected paramaters are sets of x y coordinates
            for (int i = 2; i < conditionParams.Length; i += 2)
            {
                if (npc.currentLocation == Game1.currentLocation &&
                    npc.getTileLocation() == new Vector2(int.Parse(conditionParams[i]), int.Parse(conditionParams[i + 1])))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if Joja route was completed
        /// Borrowed code from Content Patcher
        /// </summary>
        /// <returns>true if jojamart is complete, false if not</returns>
        private static bool CheckJojaMartComplete()
        {
            //I pretty much c&p'd this straight from CP, thanks Pathos
            if (!Game1.MasterPlayer.mailReceived.Contains("JojaMember"))
                return false;

            GameLocation town = Game1.getLocationFromName("Town");
            return ModEntry.helper.Reflection.GetMethod(town, "checkJojaCompletePrerequisite").Invoke<bool>();
        }

        /// <summary>
        /// Returns true if the given mods are present
        /// </summary>
        /// <param name="conditionParams">The condition string split by spaces, with each parameter being a mod uniqueID</param>
        /// <returns>True if every mod is loaded, otherwise return false</returns>
        private static bool CheckHasMod(string[] conditionParams)
        {
            //each string is the UniqueID of a mod that is required
            //if any isn't loaded, returns false
            for (int i = 1; i < conditionParams.Length; i++)
            {
                if (!ModEntry.helper.ModRegistry.IsLoaded(conditionParams[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if the player has at least the level given for each skill and level pair
        /// </summary>
        /// <param name="conditionParams">The condition string split by spaces,
        /// with each skill name paired with a level</param>
        /// <returns>Return true if the player has at least the level given for every skill, otherwise returns false</returns>
        private static bool CheckSkillLevel(string[] conditionParams)
        {
            //each paramater is a pair of skill name and its level
            for (int i = 1; i < conditionParams.Length; i += 2)
            {
                switch (conditionParams[i])
                {
                    case "combat":
                        if (Game1.player.CombatLevel < int.Parse(conditionParams[i + 1]))
                            return false;
                        break;
                    case "farming":
                        if (Game1.player.FarmingLevel < int.Parse(conditionParams[i + 1]))
                            return false;
                        break;
                    case "fishing":
                        if (Game1.player.FishingLevel < int.Parse(conditionParams[i + 1]))
                            return false;
                        break;
                    case "foraging":
                        if (Game1.player.ForagingLevel < int.Parse(conditionParams[i + 1]))
                            return false;
                        break;
                    case "luck":
                        if (Game1.player.LuckLevel < int.Parse(conditionParams[i + 1]))
                            return false;
                        break;
                    case "mining":
                        if (Game1.player.MiningLevel < int.Parse(conditionParams[i + 1]))
                            return false;
                        break;
                    default:
                        ModEntry.monitor.Log($"\"{conditionParams[i]}\" is not a valid paramater for SkillLevel. Skipping check.", LogLevel.Trace);
                        break;
                }
            }

            return true;
        }

    }
}
