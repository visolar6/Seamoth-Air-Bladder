using HarmonyLib;
using SeamothAirBladder.Mono;

namespace SeamothAirBladder.Patches
{
    /// <summary>
    /// Patches Vehicle.OnPilotModeBegin to refresh bar position when entering vehicles.
    /// </summary>
    [HarmonyPatch]
    public static class Vehicle_OnPilotModeBegin_Patch
    {
        /// <summary>
        /// Called when player enters a vehicle (pilot mode begins).
        /// Refreshes bar position to show it when entering Seamoth with air bladder module.
        /// Uses retry logic to handle UI initialization delays.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Vehicle), "OnPilotModeBegin")]
        public static void OnPilotModeBegin_Postfix(Vehicle __instance)
        {
            // Check if this vehicle has the air bladder behavior
            if (__instance.TryGetComponent<SeamothAirBladderBehavior>(out var behavior))
            {
                behavior.RefreshBarPositionWithRetry();
            }
        }
    }
}
