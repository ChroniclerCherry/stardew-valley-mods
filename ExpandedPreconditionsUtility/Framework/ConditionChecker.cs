using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Network;

namespace ExpandedPreconditionsUtility.Framework
{
    internal class ConditionChecker
    {
        /// <summary>
        /// This is the vanilla method used to check preconditions
        /// Borrowing it to gain quick access to all vanilla preconditions
        /// </summary>
        public Func<string, bool> VanillaPreconditionsMethod { get; private set; }

        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly bool _verboseLogging;
        private readonly string _uniqueId;

        public ConditionChecker(IModHelper helper, IMonitor monitor, bool verbose = false, string uniqueId = null)
        {
            this._helper = helper;
            this._monitor = monitor;
            this._verboseLogging = verbose;
            this._uniqueId = uniqueId;
        }

        /// <summary>
        /// Each string in the array can have multiple conditions separated by the character '/'
        /// and each string is evaluated as true if all of the conditions are met
        /// The method returns true if any of the strings are true
        /// </summary>
        /// <returns>true if all conditions matches, otherwise false</returns>
        internal bool CheckConditions(string[] conditions)
        {
            //if no conditions are supplied, then conditions are always met
            if (conditions == null)
                return true;

            //using the farm because it should be available for every player at any point in the game. Current location sometimes doesn't exist for farmhands
            this.VanillaPreconditionsMethod = condition => Game1.getFarm().checkEventPrecondition(condition) != "-1";

            //if someone somehow marked this fake ID as seen, unmark it so condition checking will actually work
            if (Game1.player.eventsSeen.Remove("-6529"))
            {
                this._monitor.Log($"{this._uniqueId} / Expanded Preconditions Utility uses the fake event ID of -6529 in order to use vanilla preconditions." +
                    " Somehow your save has marked this ID as seen. Expanded Preconditions is freeing it back up.", LogLevel.Warn);
            }

            //if any of the conditions are met, return true
            foreach (var con in conditions)
            {
                if (this._verboseLogging)
                    this._monitor.Log($"{this._uniqueId}: Checking condition string: \"{con}\"", LogLevel.Debug);

                if (this.CheckIndividualConditions(con.Split('/')))
                {
                    if (this._verboseLogging)
                        this._monitor.Log($"-{this._uniqueId}: Player met the conditions: \"{con}\"", LogLevel.Debug);
                    return true;
                }
            }

            if (this._verboseLogging)
                this._monitor.Log($"-{this._uniqueId}: No conditions were met", LogLevel.Debug);

            //if no conditions are met, return false
            return false;
        }

        /// <summary>
        /// This method takes an array which was originally a single string condition, split at the '/' symbols
        /// If any of the conditions are false, then the entire expression evaluates as false
        /// If all conditions pass, then the whole expression evaluates as true
        /// </summary>
        private bool CheckIndividualConditions(string[] conditions)
        {
            //if any of the conditions fail, return false
            foreach (var condition in conditions)
            {
                if (this._verboseLogging)
                    this._monitor.Log($"--{this._uniqueId} / Checking individual condition: {condition}", LogLevel.Debug);
                //if condition starts with a ! return false if condition checking is true
                if (condition[0] == '!')
                {
                    if (this.CheckCustomConditions(condition.Substring(1)))
                    {
                        if (this._verboseLogging)
                            this._monitor.Log($"--{this._uniqueId} / Failed individual condition: {condition}", LogLevel.Debug);
                        return false;
                    }
                }
                else if (!this.CheckCustomConditions(condition))
                {
                    if (this._verboseLogging)
                        this._monitor.Log($"--{this._uniqueId} / Failed individual condition: {condition}", LogLevel.Debug);

                    return false;
                }
            }

            if (this._verboseLogging)
                this._monitor.Log($"--{this._uniqueId} / Passed all conditions: {conditions}", LogLevel.Debug);
            //passed all conditions
            return true;

        }

        /// <summary>
        /// takes a singular condition and evaluates it for true or false.
        /// If it matches none of the custom conditions defined, it then checks the vanilla preconditions
        /// </summary>
        private bool CheckCustomConditions(string con)
        {
            string[] conditionParams = con.Split(' ');
            //the first parameter at 0 is the command of which condition to check
            switch (conditionParams[0])
            {
                case "NPCAt":
                    return this.CheckNpcAt(conditionParams);
                case "HasMod":
                    return this.CheckHasMod(conditionParams);
                case "SkillLevel":
                    return this.CheckSkillLevel(conditionParams);
                case "CommunityCenterComplete":
                    return Game1.MasterPlayer.mailReceived.Contains("ccIsComplete") || Game1.MasterPlayer.hasCompletedCommunityCenter();
                case "JojaMartComplete":
                    return this.CheckJojaMartComplete();
                case "SeededRandom":
                    return this.CheckSeededRandom(conditionParams);
                case "HasCookingRecipe":
                    return this.CheckHasRecipe(conditionParams, Game1.player.cookingRecipes);
                case "HasCraftingRecipe":
                    return this.CheckHasRecipe(conditionParams, Game1.player.craftingRecipes);
                case "FarmHouseUpgradeLevel":
                    return this.CheckFarmHouseUpgrade(conditionParams);
                case "HasItem":
                    return this.HasItem(conditionParams);

                default:
                    // Note: "-5005" is a random event id cause the vanilla method is for events and needs one ¯\_(ツ)_/¯
                    // so it's the negative mod id
                    return this.VanillaPreconditionsMethod("-5005/" + con);
            }
        }

        /// <summary>
        /// Returns true if any items matching the given name is found in the player's inventory. Only takes a single item name, since spaces can cause problems
        /// </summary>
        private bool HasItem(string[] conditionParams)
        {
            string itemName = conditionParams[1];

            for (int i = 2; i < conditionParams.Length; i++)
            {
                itemName += $" {conditionParams[i]}";
            }

            return Game1.player.Items.Any(item => item?.Name == itemName);
        }

        /// <summary>
        /// Takes a list of ints and if any of them matches the house upgrade level of the current player, return true
        /// </summary>
        private bool CheckFarmHouseUpgrade(string[] conditionParams)
        {
            for (int i = 1; i < conditionParams.Length; i++)
            {
                if (int.Parse(conditionParams[i]) == Game1.player.HouseUpgradeLevel)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Takes a list of recipes and check if the player has learned it from the given list of recipes ( either cooking or crafting )
        /// All spaces in given recipe names should be replaced with `-` because spaces are used as a delimiter (i.e. Seafoam Pudding should be entered as Seafoam-Pudding )
        /// </summary>
        private bool CheckHasRecipe(string[] conditionParams,
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

        /// <summary>
        /// Allows a random number check to be done against a number seeded by the game ID and period of time, allowing random checks to persist across time
        /// </summary>
        private bool CheckSeededRandom(string[] conditionParams)
        {
            int offset = Convert.ToInt32(conditionParams[1]);

            //time period can be given as either the number of days in int, or as a string mapped to a number of days
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
                interval = (int)(Game1.Date.TotalDays / interval);
            }


            ulong seed = Game1.uniqueIDForThisGame + (ulong)offset + (ulong)interval;

            var rng = new Random((int)seed);
            double roll = rng.NextDouble();
            return lowerCheck <= roll && roll < higherCheck;

        }

        /// <summary>
        /// Checks that a given NPC is at the tile indexes
        /// </summary>
        /// <param name="conditionParams">The condition string split by spaces, with the first parameter being the NPC 
        /// name and every two after that is the X and Y coordinates</param>
        /// <returns>true if the npc was found at the given tile, false if not</returns>
        private bool CheckNpcAt(string[] conditionParams)
        {
            //the second parameter at index 1 is the name of an npc
            var npc = Game1.getCharacterFromName(conditionParams[1]);

            //after that the expected parameter are sets of x y coordinates
            for (int i = 2; i < conditionParams.Length; i += 2)
            {
                if (npc.currentLocation == Game1.currentLocation &&
                    npc.Tile == new Vector2(int.Parse(conditionParams[i]), int.Parse(conditionParams[i + 1])))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if Joja route was completed
        /// Borrowed code from Content Patcher
        /// </summary>
        /// <returns>true if jojamart is complete, false if not</returns>
        private bool CheckJojaMartComplete()
        {
            //I pretty much c&p'd this straight from CP, thanks Pathos
            if (!Game1.MasterPlayer.mailReceived.Contains("JojaMember"))
                return false;

            GameLocation town = Game1.getLocationFromName("Town");
            return this._helper.Reflection.GetMethod(town, "checkJojaCompletePrerequisite").Invoke<bool>();
        }

        /// <summary>
        /// Returns true if the given mods are present
        /// </summary>
        /// <param name="conditionParams">The condition string split by spaces, with each parameter being a mod uniqueID</param>
        /// <returns>True if every mod is loaded, otherwise return false</returns>
        private bool CheckHasMod(string[] conditionParams)
        {
            //each string is the UniqueID of a mod that is required
            //if any isn't loaded, returns false
            for (int i = 1; i < conditionParams.Length; i++)
            {
                if (!this._helper.ModRegistry.IsLoaded(conditionParams[i]))
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
        private bool CheckSkillLevel(string[] conditionParams)
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
                        this._monitor.Log($"{this._uniqueId} / \"{conditionParams[i]}\" is not a valid parameter for SkillLevel. Skipping check.", LogLevel.Warn);
                        break;
                }
            }

            return true;
        }
    }
}
