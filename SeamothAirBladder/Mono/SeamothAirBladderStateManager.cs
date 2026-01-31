using SeamothAirBladder.Config;
using UnityEngine;

namespace SeamothAirBladder.Mono
{
    /// <summary>
    /// Manages persistence of air bladder states across game sessions.
    /// </summary>
    public static class SeamothAirBladderStateManager
    {
        private static SeamothAirBladderStateConfig? stateConfig;

        /// <summary>
        /// Initializes the state config if not already loaded.
        /// </summary>
        public static void Initialize()
        {
            if (stateConfig == null)
            {
                stateConfig = new SeamothAirBladderStateConfig();
                stateConfig.Load();
            }
        }

        /// <summary>
        /// Attempts to restore the air state for a vehicle.
        /// </summary>
        /// <param name="vehicleId">The PrefabIdentifier.Id of the vehicle.</param>
        /// <param name="airRemaining">Output parameter for the restored air amount.</param>
        /// <returns>True if a saved state was found and restored, false otherwise.</returns>
        public static bool TryRestoreAirState(string vehicleId, out float airRemaining)
        {
            Initialize();

            if (stateConfig!.VehicleAirStates.TryGetValue(vehicleId, out float savedAir))
            {
                Plugin.Log?.LogInfo($"Restoring saved air for vehicle {vehicleId}: {savedAir}");
                airRemaining = savedAir;
                return true;
            }
            else
            {
                Plugin.Log?.LogInfo($"No saved air state for vehicle {vehicleId}, starting with full air.");
                airRemaining = 100f;
                return false;
            }
        }

        /// <summary>
        /// Updates the air state for a specific vehicle in memory.
        /// </summary>
        /// <param name="vehicleId">The PrefabIdentifier.Id of the vehicle.</param>
        /// <param name="airRemaining">The current air remaining value.</param>
        public static void UpdateAirState(string vehicleId, float airRemaining)
        {
            Initialize();
            stateConfig!.VehicleAirStates[vehicleId] = airRemaining;
        }

        /// <summary>
        /// Updates all active vehicles' air states and saves the config to disk.
        /// Called from the SaveGame patch.
        /// </summary>
        public static void SaveAllAirStates()
        {
            Initialize();

            // Find all active SeamothAirBladderBehavior instances and update their states
            var allBehaviors = Object.FindObjectsOfType<SeamothAirBladderBehavior>();
            foreach (var behavior in allBehaviors)
            {
                behavior.UpdateAirStateInConfig();
            }

            Plugin.Log?.LogInfo($"Saving Seamoth air bladder states for {allBehaviors.Length} vehicle(s)...");
            stateConfig!.Save();
        }
    }
}
