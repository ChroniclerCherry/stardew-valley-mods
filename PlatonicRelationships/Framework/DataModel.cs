using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using ChroniclerCherry.Common;

namespace PlatonicRelationships.Framework
{
    /// <summary>The data model for the <c>assets/data.json</c> file.</summary>
    internal class DataModel
    {
        /// <summary>The events considered romantic, which should be disabled if the player isn't dating the NPC.</summary>
        /// <remarks>This maps event asset name -&gt; event ID -&gt; NPC who must be dated.</remarks>
        public Dictionary<string, Dictionary<string, string>> RomanticEvents { get; set; } = [];

        /// <summary>The trigger actions considered romantic, which should be disabled if the player isn't dating the NPC.</summary>
        /// <remarks>This maps trigger ID -&gt; NPC who must be dated.</remarks>
        public Dictionary<string, string> RomanticTriggerActions { get; set; } = [];


        /*********
        ** Private methods
        *********/
        /// <summary>The method called after the config file is deserialized.</summary>
        /// <param name="context">The deserialization context.</param>
        [OnDeserialized]
        [SuppressMessage("ReSharper", "NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract", Justification = SuppressReasons.UsedViaReflection)]
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = SuppressReasons.UsedViaReflection)]
        [SuppressMessage("ReSharper", "UnusedParameter.Local", Justification = SuppressReasons.UsedViaReflection)]
        private void OnDeserializedMethod(StreamingContext context)
        {
            this.RomanticEvents ??= [];
            this.RomanticTriggerActions ??= [];
        }
    }
}
