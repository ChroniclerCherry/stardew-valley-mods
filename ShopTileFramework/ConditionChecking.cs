using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopTileFramework
{
    internal static class ConditionChecking
    {
        internal static Dictionary<string, Func<string, bool>> Conditions;

        static ConditionChecking()
        {
            Conditions = new Dictionary<string, Func<string, bool>>();

        }

        internal static bool CheckCustomConditions(Dictionary<string, string>[] conditions)
        {
            if (conditions == null)
                return true;

            return false;
        }

        /// <summary>
        /// Reflects into the game's event preconditions method to do condition checking
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns>true if all conditions matches, otherwise false</returns>
        internal static bool CheckEventPreconditions(string[] conditions)
        {
            //if no conditions are supplied, then conditions are always met
            if (conditions == null)
                return true;

            //if any of the conditions are met, return true
            foreach (var con in conditions)
            {
                if (CheckIndividualEventPreconditions(con))
                    return true;
            }

            //if no conditions are met, return false
            return false;
        }

        internal static bool CheckIndividualEventPreconditions(string conditions)
        {
            //giving this a random event id cause the vanilla method is for events and needs one ¯\_(ツ)_/¯
            //so it's the negative mod id
            conditions = "-5005/" + conditions;

            int checkedCondition = ModEntry.helper.Reflection.GetMethod(Game1.currentLocation, "checkEventPrecondition").Invoke<int>(conditions);

            if (checkedCondition == -1)
            {
                ModEntry.monitor.Log("Player did not meet the event preconditions:\n" +
                    conditions);
                return false;
            }
            return true;
        }
    }
}
