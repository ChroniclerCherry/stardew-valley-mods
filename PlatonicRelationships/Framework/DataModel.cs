using System.Collections.Generic;

namespace PlatonicRelationships.Framework
{
    /// <summary>The data model for the <c>assets/data.json</c> file.</summary>
    internal class DataModel
    {
        /// <summary>The events considered romantic, which should be disabled if the player isn't dating the NPC.</summary>
        /// <remarks>This maps event asset name -&gt; event ID -&gt; NPC who must be dated.</remarks>
        public Dictionary<string, Dictionary<string, string>> RomanticEvents { get; set; } = new();

        /// <summary>The trigger actions considered romantic, which should be disabled if the player isn't dating the NPC.</summary>
        /// <remarks>This maps trigger ID -&gt; NPC who must be dated.</remarks>
        public Dictionary<string, string> RomanticTriggerActions { get; set; } = new();
    }
}
