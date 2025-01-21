using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace PlatonicRelationships.Framework;

/// <summary>Adjusts event data to disable vanilla 10-heart events for NPCs the player isn't dating.</summary>
internal class EventEditor
{
    /*********
    ** Fields
    *********/
    /// <inheritdoc cref="DataModel.RomanticEvents" />
    private readonly Dictionary<IAssetName, Dictionary<string, string>> RomanticEvents;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="data">The data loaded from the mod files.</param>
    /// <param name="parseAssetName">Parse a raw asset name.</param>
    public EventEditor(DataModel data, Func<string, IAssetName> parseAssetName)
    {
        this.RomanticEvents = this.ReadRomanticEvents(data, parseAssetName);
    }

    /// <inheritdoc cref="IContentEvents.AssetRequested" />
    public void OnAssetRequested(AssetRequestedEventArgs e)
    {
        // TODO: add kill contexts for platonic versions of event ("/k <event id>")

        if (this.RomanticEvents.TryGetValue(e.NameWithoutLocale, out Dictionary<string, string> events))
        {
            e.Edit(asset =>
            {
                var data = asset.AsDictionary<string, string>().Data;

                foreach ((string eventId, string npcName) in events)
                {
                    if (data.Remove(eventId, out string script))
                        data[this.AddDatingRequirement(eventId, npcName)] = script;
                }
            });
        }
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Read the romantic events from the raw data model.</summary>
    /// <param name="data">The data loaded from the mod files.</param>
    /// <param name="parseAssetName">Parse a raw asset name.</param>
    private Dictionary<IAssetName, Dictionary<string, string>> ReadRomanticEvents(DataModel data, Func<string, IAssetName> parseAssetName)
    {
        var romanticEvents = new Dictionary<IAssetName, Dictionary<string, string>>();

        foreach ((string rawAssetName, Dictionary<string, string> rawEvents) in data.RomanticEvents)
        {
            IAssetName assetName = parseAssetName(rawAssetName);

            Dictionary<string, string> events = new();
            foreach ((string eventId, string npcName) in rawEvents)
                events.Add(eventId, npcName);

            romanticEvents[assetName] = events;
        }

        return romanticEvents;
    }

    /// <summary>Add a dating requirement to an event ID.</summary>
    /// <param name="eventId">The event ID with preconditions to edit.</param>
    /// <param name="npcName">The NPC whom the NPC must date to see the event.</param>
    /// <returns>Returns the modified event ID.</returns>
    private string AddDatingRequirement(string eventId, string npcName)
    {
        // Add right after the event ID, to avoid conflicting with preconditions that immediately end evaluation like
        // SendMail.

        string[] split = Event.SplitPreconditions(eventId);
        return split.Length == 1
            ? $"{eventId}/D {npcName}"
            : $"{split[0]}/D {npcName}/{string.Join('/', split.Skip(1))}";
    }
}
