using System;
using System.Reflection;

namespace StardewAquarium.Models
{
    public interface ISpaceCoreAPI
    {
        // Must take (Event, GameLocation, GameTime, string[])
        [Obsolete("Game now implements this functionality on it's own using Event.RegisterCommand")]
        void AddEventCommand(string command, MethodInfo info);
    }
}
