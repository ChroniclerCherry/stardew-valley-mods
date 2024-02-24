using System.Reflection;

namespace StardewAquarium.Models
{
    public interface ISpaceCoreAPI
    {
        // Must take (Event, GameLocation, GameTime, string[])
        void AddEventCommand(string command, MethodInfo info);
    }
}
