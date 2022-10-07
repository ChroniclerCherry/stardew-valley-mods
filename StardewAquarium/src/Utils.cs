using System.Collections.Generic;
using System.Linq;

using Netcode;

using StardewAquarium.Editors;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;

using Object = StardewValley.Object;

namespace StardewAquarium
{
    internal static class Utils
    {
        private static IModHelper _helper;
        private static IMonitor _monitor;
        private static IManifest _manifest;

        private static NetStringList MasterPlayerMail => Game1.MasterPlayer.mailReceived;

        /// <summary>
        /// Maps the InternalName of the fish to its internalname without spaces, eg. Rainbow Trout to RainbowTrout
        /// </summary>
        /// 
        public static Dictionary<string, string> InternalNameToDonationName { get; private set; } = new Dictionary<string, string>(64);
        public static List<int> FishIDs { get; private set; } = new List<int>(64);

        /// <summary>
        /// Maps the internal name without spaces to its localized display name
        /// </summary>
        public static Dictionary<string, string> FishDisplayNames { get; private set; } = new Dictionary<string, string>(64);

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

            foreach (var kvp in Game1.objectInformation)
            {
                var info = kvp.Value.Split('/', 6);
                var fishName = info[0];
                if (info[3].Contains("-4"))
                {
                    FishIDs.Add(kvp.Key);
                    InternalNameToDonationName.Add(fishName, fishName.Replace(" ",string.Empty));
                    FishDisplayNames.Add(fishName.Replace(" ", string.Empty),info[4]);
                }
            }
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
                if (!Context.IsMainPlayer){
                    _helper.Multiplayer.SendMessage(i.Name, DonateFishMessageType,
                        modIDs: new[] {_manifest.UniqueID});
                    _fishSign.UpdateLastDonatedFish(i);
                    return true;
                }

                MasterPlayerMail.Add(donatedFlag);
                string numDonated = $"AquariumFishDonated:{GetNumDonatedFish()}";
                MasterPlayerMail.Add(numDonated);
            }

            
            if (ModEntry.Data.ConversationTopicsOnDonate.Contains(i.Name))
            {
                foreach (var farmer in Game1.getAllFarmers())
                {
                    if (farmer.activeDialogueEvents.ContainsKey(donatedFlag))
                    {
                        farmer.activeDialogueEvents[donatedFlag] = 3;
                    }
                    else
                    {
                        farmer.activeDialogueEvents.Add(donatedFlag, 3);
                    }
                }
            }

            _fishSign.UpdateLastDonatedFish(i);

            return true;
        }

        internal static bool PlayerInventoryContains(int fishId)
        {
            foreach (Item item in Game1.player.Items)
            {
                if (item is Object obj && obj.ParentSheetIndex == fishId)
                    return true;
            }

            return false;
        }

        public static int GetNumDonatedFish()
        {
            return MasterPlayerMail.Count(flag => flag.StartsWith("AquariumDonated:"));
        }

        public static string GetDonatedMailFlag(Item i)
        {
            return $"AquariumDonated:{InternalNameToDonationName[i.Name]}";
        }

        public static string GetDonatedMailFlag(string name)
        {
            return $"AquariumDonated:{InternalNameToDonationName[name]}";
        }

        public static bool DoesPlayerHaveDonatableFish()
        {
            foreach (var item in Game1.player.Items)
            {
                if (IsUnDonatedFish(item)) return true;
            }

            return false;
        }

        public static IEnumerable<int> GetUndonatedFishInInventory()
        {
            foreach (var item in Game1.player.Items)
            {
                if (IsUnDonatedFish(item)) yield return item.ParentSheetIndex;
            }
        }

        public static void DonationMenuExit(bool donated, bool pufferchickDonated)
        {
            string mainMessage;

            if (CheckAchievement())
            {
                mainMessage = _helper.Translation.Get("AchievementCongratulations");
                UnlockAchievement();
                _helper.Multiplayer.SendMessage(true, AchievementMessageType, modIDs: new[] { _manifest.UniqueID });


                var mp = _helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
                mp.globalChatInfoMessage("StardewAquarium.AchievementUnlocked");

            }
            else if (donated)
            {
                mainMessage = _helper.Translation.Get("MenuCloseFishDonated");
            }
            else
            {
                mainMessage = _helper.Translation.Get("MenuCloseNoFishDonated");
            }

            Game1.drawObjectDialogue(mainMessage);
            var dialoguesField = _helper.Reflection.GetField<List<string>>(Game1.activeClickableMenu, "dialogues");

            if (pufferchickDonated)
            {
                var dialogues = dialoguesField.GetValue();
                dialogues.Add(_helper.Translation.Get("PufferchickDonated"));
                dialoguesField.SetValue(dialogues);
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

            Game1.addHUDMessage(new HUDMessage(_helper.Translation.Get("AchievementName"), true));
            Game1.playSound("achievement");
            Game1.player.achievements.Add(AchievementEditor.AchievementId);

            if (!Context.IsMainPlayer) return;
            if (!MasterPlayerMail.Contains("AquariumCompleted"))
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
            if (e.FromModID == _manifest.UniqueID)
            {
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
        }

        private static void FarmhandDonated(ModMessageReceivedEventArgs e)
        {
            if (!Context.IsMainPlayer) return;

            string fishName = e.ReadAs<string>();
            string donatedFlag = GetDonatedMailFlag(fishName);
            MasterPlayerMail.Add(donatedFlag);
            string numDonated = $"AquariumFishDonated:{GetNumDonatedFish()}";
            MasterPlayerMail.Add(numDonated);
            if (ModEntry.Data.ConversationTopicsOnDonate.Contains(fishName))
            {
                foreach (var farmer in Game1.getAllFarmers())
                {
                    if (farmer.activeDialogueEvents.ContainsKey(donatedFlag))
                    {
                        farmer.activeDialogueEvents[donatedFlag] = 3;
                    }
                    else
                    {
                        farmer.activeDialogueEvents.Add(donatedFlag, 3);
                    }
                }
            }
        }
    }
}
