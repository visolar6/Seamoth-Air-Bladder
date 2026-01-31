using HarmonyLib;
using SeamothAirBladder.Mono;

namespace SeamothAirBladder.Patches
{
    /// <summary>
    /// Patches Vehicle.OnPilotModeBegin and OnPilotModeEnd to refresh bar position
    /// when entering/exiting vehicles, eliminating the need for continuous checking.
    /// </summary>
    [HarmonyPatch]
    public static class VehiclePilotPatches
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

        /// <summary>
        /// Called when player exits a vehicle (pilot mode ends).
        /// Hides the bar when exiting Seamoth.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Vehicle), "OnPilotModeEnd")]
        public static void OnPilotModeEnd_Postfix(Vehicle __instance)
        {
            // Check if this vehicle has the air bladder behavior
            if (__instance.TryGetComponent<SeamothAirBladderBehavior>(out var behavior))
            {
                behavior.HideBar();
            }
        }
    }
}
