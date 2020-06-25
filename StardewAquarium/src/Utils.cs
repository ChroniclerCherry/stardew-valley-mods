using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using xTile.Layers;
using xTile.Tiles;
using Object = StardewValley.Object;

namespace StardewAquarium
{
    internal static class Utils
    {
        private static NetStringList MasterPlayerMail => Game1.MasterPlayer.mailReceived;

        private static IModHelper _helper;
        private static IMonitor _monitor;

        private static ModEntry.ModData _data;

        private static LastDonatedFishSign _fishSign;
        private static LegendaryRecatch _recatch;

        public static void Initialize(IModHelper helper, IMonitor monitor, ModEntry.ModData data)
        {
            _helper = helper;
            _monitor = monitor;
            _data = data;
            _fishSign = new LastDonatedFishSign(helper, monitor, data);
            _recatch = new LegendaryRecatch(helper, monitor);
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

            _recatch.Donated(i);
            _fishSign.UpdateLastDonatedFish(i);

            return true;
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

        /**************************
         * Last donated fish stuff
         * ************************/



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
