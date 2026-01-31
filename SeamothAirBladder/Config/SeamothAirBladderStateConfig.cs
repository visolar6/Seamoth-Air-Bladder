using System.Collections.Generic;
using Nautilus.Json;

namespace SeamothAirBladder.Config
{
    /// <summary>
    /// Persists vehicle air bladder states across game sessions.
    /// Maps vehicle instance IDs to their remaining air values.
    /// </summary>
    public class SeamothAirBladderStateConfig : ConfigFile
    {
        public SeamothAirBladderStateConfig() : base("seamothairbladder")
        {
        }

        /// <summary>
        /// Dictionary mapping vehicle PrefabIdentifier ID to remaining air amount.
        /// Using PrefabIdentifier.Id ensures unique identification per vehicle across save files.
        /// </summary>
        public Dictionary<string, float> VehicleAirStates { get; set; } = [];
    }
}
