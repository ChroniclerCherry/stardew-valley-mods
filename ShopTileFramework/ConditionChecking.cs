using StardewModdingAPI;
using StardewValley;
using System;

namespace ShopTileFramework
{
    internal static class ConditionChecking
    {

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

            //if any of the conditions are met, return true
            foreach (var con in conditions)
            {
                if (con[0] == '!' && !CheckIndividualConditions(con.Substring(1).Split('/')))
                {
                    ModEntry.monitor.Log($"Player met the conditions: \"{con}\"");
                    return true;
                }
                else if (CheckIndividualConditions(con.Split('/')))
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
            var VanillaPreconditionsMethod = ModEntry.helper.Reflection.GetMethod(Game1.currentLocation, "checkEventPrecondition");

            //if any of the conditions fail, return false
            foreach (var condition in conditions)
            {
                //check custom conditions first
                if (!CheckCustomConditions(condition))
                    if (VanillaPreconditionsMethod.Invoke<int>("-5005/" + condition) == -1) //then check event preconditions if custom conditions failed
                    {
                        // Note: "-5005" is a random event id cause the vanilla method is for events and needs one ¯\_(ツ)_/¯
                        //so it's the negative mod id
                        return false;
                    }
            }
            //passed all conditions
            return true;

        }

        private static bool CheckCustomConditions(string con)
        {
            string[] ConditionParams = con.Split(' ');

            switch (ConditionParams[0])
            {
                case "NPCAt":
                    return CheckNPCAt(ConditionParams);
                case "HasMod":
                    return CheckHasMod(ConditionParams);
                case "HasSkill":
                    return CheckHasSkill(ConditionParams);
                case "OwnsAnimals":
                    return CheckOwnsAnimals(ConditionParams);
                case "CommunityCenterComplete":
                    return CheckJojaMartComplete(ConditionParams);
                case "JojaMartComplete":
                    return CheckJojaMartComplete(ConditionParams);
                default:
                    ModEntry.monitor.Log($"\"{con}\" is not a valid custom condition. Checking vanilla preconditions now!");
                    return false;
            }
        }

        private static bool CheckNPCAt(string[] conditionParams)
        {
            throw new NotImplementedException();
        }

        private static bool CheckJojaMartComplete(string[] conditionParams)
        {
            throw new NotImplementedException();
        }

        private static bool CheckOwnsAnimals(string[] conditionParams)
        {
            return true;
        }

        private static bool CheckHasMod(string[] conditionParams)
        {
            return true;
        }

        private static bool CheckHasSkill(string[] con)
        {
            return true;
        }

    }
}
