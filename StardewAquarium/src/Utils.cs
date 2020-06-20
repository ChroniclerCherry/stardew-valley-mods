using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Netcode;
using StardewModdingAPI;
using StardewValley;

namespace StardewAquarium
{
    static class Utils
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

            return !MasterPlayerMail.Contains(GetDonatedMailFlash(i));
        }

        public static bool DonateFish(Item i)
        {
            if (!IsUnDonatedFish(i))
                return false;

            MasterPlayerMail.Add(GetDonatedMailFlash(i));
            return true;
        }

        public static int GetNumDonatedFish()
        {
            return MasterPlayerMail.Count(flag => flag.StartsWith("AquariumDonated:"));
        }

        public static string GetDonatedMailFlash(Item i)
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
    }
}
