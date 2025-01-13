using System.Collections.Generic;
using System.Linq;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Objects;
using StardewValley.Menus;
using StardewValley.TokenizableStrings;
using Object = StardewValley.Object;

namespace StardewAquarium.Framework;

internal static class Utils
{
    internal const string StatsKey = "Cherry.StardewAquarium.FishDonated";

    internal const string AquariumPrefix = "AquariumDonated:";

    private static IModHelper _helper;
    private static IMonitor _monitor;
    private static IManifest _manifest;

    private static NetStringHashSet MasterPlayerMail => Game1.MasterPlayer.mailReceived;

    /// <summary>
    /// Maps the InternalName of the fish to its internalname without spaces, eg. Rainbow Trout to RainbowTrout
    /// </summary>
    /// 
    public static Dictionary<string, string> InternalNameToDonationName { get; private set; } = [];

    public static List<string> FishIDs { get; set; } = [];

    /// <summary>
    /// Maps the internal name without spaces to its localized display name
    /// </summary>
    internal static Dictionary<string, string> FishDisplayNames { get; private set; } = [];

    private static LastDonatedFishSign _fishSign;

    public static void Initialize(IModHelper helper, IMonitor monitor, IManifest modManifest)
    {
        _helper = helper;
        _monitor = monitor;
        _manifest = modManifest;

        _helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
        _helper.Events.Multiplayer.ModMessageReceived += Multiplayer_ModMessageReceived;

        _fishSign = new LastDonatedFishSign(helper.Events, monitor);
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

    /// <summary>Get whether a fish can be donated.</summary>
    /// <param name="name">the internal name of the fish.</param>
    public static bool IsUnDonatedFish(string? name)
    {
        if (name is null)
            return false;

        if (InternalNameToDonationName.TryGetValue(name, out string donationName))
        {
            return !MasterPlayerMail.Contains($"{AquariumPrefix}{donationName}");
        }

        return false;
    }

    internal static bool HasDonatedFishKey(string internalFishKey)
    {
        return MasterPlayerMail.Contains($"{AquariumPrefix}{internalFishKey}");
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
                    modIDs: [_manifest.UniqueID]);
                return true;
            }

            ProcessDonationOnHost(i.Name);
        }

        return true;
    }

    public static int GetNumDonatedFish()
    {
        return MasterPlayerMail.Count(flag => flag.StartsWith(AquariumPrefix));
    }

    public static string GetDonatedMailFlag(Item i)
    {
        return GetDonatedMailFlag(i.Name);
    }

    public static string GetDonatedMailFlag(string name)
    {
        return $"{AquariumPrefix}{InternalNameToDonationName[name]}";
    }

    public static bool DoesPlayerHaveDonatableFish(Farmer farmer)
    {
        return farmer.Items?.Any(static item => IsUnDonatedFish(item)) == true;
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
            mainMessage = ContentPackHelper.LoadString("AchievementCongratulations");

            UnlockAchievement();
            _helper.Multiplayer.SendMessage(true, AchievementMessageType, modIDs: [_manifest.UniqueID]);
            Game1.Multiplayer.globalChatInfoMessage($"{ContentPackHelper.ContentPackId}_AchievementUnlocked");
        }
        else
        {
            mainMessage = ContentPackHelper.LoadString(donated ? "MenuCloseFishDonated" : "MenuCloseNoFishDonated");
        }
        Game1.drawObjectDialogue(mainMessage);

        if (pufferchickDonated)
        {
            (Game1.activeClickableMenu as DialogueBox)?.dialogues?.Add(ContentPackHelper.LoadString("PufferchickDonated"));
        }
    }

    private const string AchievementMessageType = "Achievement";
    public static bool CheckAchievement()
    {
        return GetNumDonatedFish() >= InternalNameToDonationName.Count;
    }

    public static void UnlockAchievement()
    {
        if (Game1.player.achievements.Contains(ContentPackHelper.AchievementId))
            return;

        string achievementName = Game1.achievements.GetValueOrDefault(ContentPackHelper.AchievementId)?.Split('^')[0];

        Game1.addHUDMessage(HUDMessage.ForAchievement(achievementName));
        Game1.playSound("achievement");
        Game1.player.achievements.Add(ContentPackHelper.AchievementId);

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
            return;

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

        foreach (Farmer farmer in Game1.getAllFarmers())
            farmer.activeDialogueEvents[donatedFlag] = 3;
    }
}
