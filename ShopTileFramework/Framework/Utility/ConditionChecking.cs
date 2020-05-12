using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace ShopTileFramework.Framework.Utility
{
    internal static class ConditionChecking
    {
        public static IReflectedMethod VanillaPreconditionsMethod { get; private set; }

        /// <summary>
        /// Reflects into the game's event preconditions method to do condition checking
        /// </summary>
        /// <param name="conditions"></param>
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
                if (CheckIndividualConditions(con.Split('/')))
                {
                    ModEntry.monitor.Log($"Player met the conditions: \"{con}\"");
                    return true;
                }
            }

            //if no conditions are met, return false
            return false;
        }

        private static bool CheckIndividualConditions(string[] conditions)
        {
            //if any of the conditions fail, return false
            foreach (var condition in conditions)
            {
                ModEntry.monitor.Log($"Checking conditions: {condition}");
                //if condition starts with a ! return false if condition checking is true
                if (condition[0] == '!')
                {
                    if (CheckCustomConditions(condition.Substring(1)))
                        return false;

                }
                else if (!CheckCustomConditions(condition))
                    return false;
            }
            //passed all conditions
            return true;

        }

        private static bool CheckCustomConditions(string con)
        {
            string[] ConditionParams = con.Split(' ');
            //the first parameter at 0 is the command of which condition to check
            switch (ConditionParams[0])
            {
                case "NPCAt":
                    return CheckNPCAt(ConditionParams);
                case "HasMod":
                    return CheckHasMod(ConditionParams);
                case "SkillLevel":
                    return CheckSkillLevel(ConditionParams);
                case "CommunityCenterComplete":
                    return Game1.MasterPlayer.mailReceived.Contains("ccIsComplete") || Game1.MasterPlayer.hasCompletedCommunityCenter();
                case "JojaMartComplete":
                    return CheckJojaMartComplete();
                default:
                    // Note: "-5005" is a random event id cause the vanilla method is for events and needs one ¯\_(ツ)_/¯
                    // so it's the negative mod id
                    return (VanillaPreconditionsMethod.Invoke<int>("-5005/" + con) != -1);
            }
        }

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

        private static bool CheckJojaMartComplete()
        {
            //I pretty much c&p'd this straight from CP, thanks Pathos
            if (!Game1.MasterPlayer.mailReceived.Contains("JojaMember"))
                return false;

            GameLocation town = Game1.getLocationFromName("Town");
            return ModEntry.helper.Reflection.GetMethod(town, "checkJojaCompletePrerequisite").Invoke<bool>();
        }

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
                        ModEntry.monitor.Log($"\"{conditionParams[i]}\" is not a valid paramater for SkillLevel. Skipping check.");
                        break;
                }
            }

            return true;
        }

    }
}
