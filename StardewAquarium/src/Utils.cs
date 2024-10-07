using System.Collections.Generic;
using System.Linq;
using Netcode;
using StardewAquarium.Editors;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Objects;
using StardewValley.Menus;
using StardewValley.TokenizableStrings;

using Object = StardewValley.Object;

namespace StardewAquarium
{
    internal static class Utils
    {
        internal const string StatsKey = "Cherry.StardewAquarium.FishDonated";

        private static IModHelper _helper;
        private static IMonitor _monitor;
        private static IManifest _manifest;

        private static NetStringHashSet MasterPlayerMail => Game1.MasterPlayer.mailReceived;

        /// <summary>
        /// Maps the InternalName of the fish to its internalname without spaces, eg. Rainbow Trout to RainbowTrout
        /// </summary>
        /// 
        public static Dictionary<string, string> InternalNameToDonationName { get; set; } = [];

        public static List<string> FishIDs { get; set; } = [];

        /// <summary>
        /// Maps the internal name without spaces to its localized display name
        /// </summary>
        public static Dictionary<string, string> FishDisplayNames { get; set; } = [];

        private static LastDonatedFishSign _fishSign;

        public static void Initialize(IModHelper helper, IMonitor monitor, IManifest modManifest)
        {
            _helper = helper;
            _monitor = monitor;
            _manifest = modManifest;

            _helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            _helper.Events.Multiplayer.ModMessageReceived += Multiplayer_ModMessageReceived;

            _fishSign = new LastDonatedFishSign(helper, monitor);
        }

        private static void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            //clear these dictionaries
            InternalNameToDonationName.Clear();
            FishDisplayNames.Clear();

            foreach ((string key, ObjectData info) in Game1.objectData)
            {
                string fishName = info.Name;
                if (info.Category == Object.FishCategory)
                {
                    FishIDs.Add(key);
                    InternalNameToDonationName.Add(fishName, fishName.Replace(" ", string.Empty));
                    FishDisplayNames.Add(fishName.Replace(" ", string.Empty), TokenParser.ParseText(info.DisplayName));
                }
            }

            // update stats
            Game1.player.stats.Set(StatsKey, GetNumDonatedFish());
        }

        public static bool IsUnDonatedFish(Item i)
        {
            if (i?.Category != -4)
                return false;

            try
            {
                return !MasterPlayerMail.Contains(GetDonatedMailFlag(i));
            }
            catch
            {
                _monitor.Log($"An item in the inventory \"{i.Name}\" has a category of Fish but is not a valid fish object.", LogLevel.Error);
                return false;
            }

        }

        public static bool IsUnDonatedFish(string s)
        {
            if (s is null)
                return false;

            if (InternalNameToDonationName.ContainsValue(s))
            {
                return !MasterPlayerMail.Contains($"AquariumDonated:{s}");
            }
            return false;
        }

        private const string DonateFishMessageType = "DonateFish";

        public static bool DonateFish(Item i)
        {
            if (!IsUnDonatedFish(i))
                return false;

            string donatedFlag = GetDonatedMailFlag(i);
            if (!MasterPlayerMail.Contains(donatedFlag))
            {
                _fishSign.UpdateLastDonatedFish(i);
                if (!Context.IsMainPlayer)
                {
                    _helper.Multiplayer.SendMessage(i.Name, DonateFishMessageType,
                        modIDs: new[] { _manifest.UniqueID });
                    return true;
                }

                ProcessDonationOnHost(donatedFlag);
                
            }

            return true;
        }

        public static int GetNumDonatedFish()
        {
            return MasterPlayerMail.Count(flag => flag.StartsWith("AquariumDonated:"));
        }

        public static string GetDonatedMailFlag(Item i)
        {
            return GetDonatedMailFlag(i.Name);
        }

        public static string GetDonatedMailFlag(string name)
        {
            return $"AquariumDonated:{InternalNameToDonationName[name]}";
        }

        public static bool DoesPlayerHaveDonatableFish()
        {
            foreach (Item item in Game1.player.Items)
            {
                if (IsUnDonatedFish(item))
                    return true;
            }

            return false;
        }

        public static IEnumerable<string> GetUndonatedFishInInventory()
        {
            foreach (Item item in Game1.player.Items)
            {
                if (IsUnDonatedFish(item))
                    yield return item.ItemId;
            }
        }

        public static void DonationMenuExit(bool donated, bool pufferchickDonated)
        {
            string mainMessage;

            if (CheckAchievement())
            {
                mainMessage = I18n.AchievementCongratulations();
                UnlockAchievement();
                _helper.Multiplayer.SendMessage(true, AchievementMessageType, modIDs: new[] { _manifest.UniqueID });
                Game1.Multiplayer.globalChatInfoMessage("StardewAquarium.AchievementUnlocked");
            }
            else
            {
                mainMessage = donated ? I18n.MenuCloseFishDonated() : I18n.MenuCloseNoFishDonated();
            }
            Game1.drawObjectDialogue(mainMessage);
            
            if (pufferchickDonated)
            {
                (Game1.activeClickableMenu as DialogueBox)?.dialogues?.Add(I18n.PufferchickDonated());
            }
        }

        private const string AchievementMessageType = "Achievement";
        public static bool CheckAchievement()
        {
            return GetNumDonatedFish() >= InternalNameToDonationName.Count;
        }

        public static void UnlockAchievement()
        {
            if (Game1.player.achievements.Contains(AchievementEditor.AchievementId))
                return;

            Game1.addHUDMessage(HUDMessage.ForAchievement(I18n.AchievementName()));
            Game1.playSound("achievement");
            Game1.player.achievements.Add(AchievementEditor.AchievementId);

            // defer adding this flag until the night.
            if (Context.IsMainPlayer && !MasterPlayerMail.Contains("AquariumCompleted"))
            {
                _helper.Events.GameLoop.Saving += AddCompletionFlag;
            }
        }

        private static void AddCompletionFlag(object sender, SavingEventArgs e)
        {
            //adding this at the end of the day so that the event won't trigger until the next day
            MasterPlayerMail.Add("AquariumCompleted");
            _helper.Events.GameLoop.Saving -= AddCompletionFlag;
        }

        private static void Multiplayer_ModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID != _manifest.UniqueID)
            {
                return;
            }
            switch (e.Type)
            {
                case AchievementMessageType:
                    UnlockAchievement();
                    break;
                case DonateFishMessageType:
                    FarmhandDonated(e);
                    break;
            }
        }

        private static void FarmhandDonated(ModMessageReceivedEventArgs e)
        {
            if (!Context.IsMainPlayer)
                return;

            string fishName = e.ReadAs<string>();
            ProcessDonationOnHost(fishName);
        }

        private static void ProcessDonationOnHost(string fishName)
        {
            string donatedFlag = GetDonatedMailFlag(fishName);
            MasterPlayerMail.Add(donatedFlag);
            string numDonated = $"AquariumFishDonated:{GetNumDonatedFish()}";
            MasterPlayerMail.Add(numDonated);
            if (ModEntry.Data.ConversationTopicsOnDonate.Contains(fishName))
            {
                foreach (Farmer farmer in Game1.getAllFarmers())
                {
                    farmer.activeDialogueEvents[donatedFlag] = 3;
                }
            }
        }
    }
}
