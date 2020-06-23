using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using Object = StardewValley.Object;

namespace StardewAquarium
{
    internal static class Utils
    {
        private static NetStringList MasterPlayerMail => Game1.MasterPlayer.mailReceived;
        private static IModHelper _helper;
        private static IMonitor _monitor;

        public static void Initialize(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
        }

        public static bool IsUnDonatedFish(Item i)
        {
            if (i?.Category != -4)
                return false;

            return !MasterPlayerMail.Contains(GetDonatedMailFlag(i));
        }

        public static bool IsUnDonatedFish(string s)
        {
            return !MasterPlayerMail.Contains($"AquariumDonated:{s}");
        }

        public static bool DonateFish(Item i)
        {
            if (!IsUnDonatedFish(i))
                return false;

            MasterPlayerMail.Add(GetDonatedMailFlag(i)); 
            string numDonated = $"AquariumFishDonated:{GetNumDonatedFish()}";
            if (!MasterPlayerMail.Contains(numDonated))
                MasterPlayerMail.Add(numDonated);

            UpdateLastDonatedFish(i);

            return true;
        }

        public static Item LastDonatedFish { get; set; }

        public static void UpdateLastDonatedFish(Item i)
        {
            foreach (var flag in MasterPlayerMail.Where(flag => flag.StartsWith("AquariumLastDonated:")))
            {
                MasterPlayerMail.Remove(flag);
                break;
            }

            LastDonatedFish = i;
            MasterPlayerMail.Add($"AquariumLastDonated:{i.Name}");
        }

        public static void SetLastDonatedFish()
        {
            string fish = MasterPlayerMail
                .Where(flag => flag
                    .StartsWith("AquariumLastDonated:"))
                .Select(flag => flag.Split(':')[1])
                .FirstOrDefault();

            if (fish == null)
                return;

            foreach (var kvp in Game1.objectInformation)
            {
                if (kvp.Value.Split('/')[0] != fish) continue;

                LastDonatedFish = new Object(kvp.Key,1);
                break;

            }
        }

        public static int GetNumDonatedFish()
        {
            return MasterPlayerMail.Count(flag => flag.StartsWith("AquariumDonated:"));
        }

        public static string GetDonatedMailFlag(Item i)
        {
            return $"AquariumDonated:{i.Name.Replace(" ", string.Empty)}";
        }

        public static bool DoesPlayerHaveDonatableFish()
        {
            foreach (var item in Game1.player.Items)
            {
                if (IsUnDonatedFish(item)) return true;
            }

            return false;
        }

        /******************
         * CP Token helpers
         * *****************/

        public static IEnumerable<string> GetDonatedFish()
        {
            return from mail in MasterPlayerMail
                where mail.StartsWith("AquariumDonated:")
                select mail.Split(':')[1];
        }

        public static IEnumerable<string> GetNumDonatedFishRange()
        {
            for (int i = 1; i < GetNumDonatedFish(); i++)
            {
                yield return i.ToString();
            }
        }


    }
}
