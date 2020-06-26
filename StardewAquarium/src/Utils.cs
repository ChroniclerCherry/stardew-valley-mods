using System;
using System.Collections.Generic;
using System.Linq;
using Netcode;
using StardewModdingAPI;
using StardewValley;

namespace StardewAquarium
{
    internal static class Utils
    {
        private static NetStringList MasterPlayerMail => Game1.MasterPlayer.mailReceived;

        private static IModHelper _helper;
        private static IMonitor _monitor;

        /// <summary>
        /// Maps the InternalName of the fish to its internalname without spaces, eg. Tranbow Trout to RainbowTrout
        /// </summary>
        public static Dictionary<string, string> InternalNameToDonationName { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Maps the internal name without spaces to its localized display name
        /// </summary>
        public static Dictionary<string, string> FishDisplayNames { get; set; } = new Dictionary<string, string>();

        private static LastDonatedFishSign _fishSign;

        public static void Initialize(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;

            _helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;

            _fishSign = new LastDonatedFishSign(helper, monitor);
        }

        private static void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            foreach (var kvp in Game1.objectInformation)
            {
                var info = kvp.Value.Split('/');
                var fishName = info[0];
                if (info[3].Contains("Fish") && !info[3].Contains("-20"))
                {
                    InternalNameToDonationName.Add(fishName, fishName.Replace(" ",String.Empty));
                    FishDisplayNames.Add(fishName.Replace(" ", String.Empty),info[4]);
                }
            }
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

            _fishSign.UpdateLastDonatedFish(i);

            return true;
        }

        public static int GetNumDonatedFish()
        {
            return MasterPlayerMail.Count(flag => flag.StartsWith("AquariumDonated:"));
        }

        public static string GetDonatedMailFlag(Item i)
        {
            return $"AquariumDonated:{InternalNameToDonationName[i.Name]}";
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
