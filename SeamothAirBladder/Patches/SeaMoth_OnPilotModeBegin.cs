using HarmonyLib;
using SeamothAirBladder.Mono;

namespace SeamothAirBladder.Patches
{
    /// <summary>
    /// Patches SeaMoth.OnPilotModeBegin to refresh bar position when entering seamoths.
    /// </summary>
    [HarmonyPatch(typeof(SeaMoth), "OnPilotModeBegin")]
    public static class SeaMoth_OnPilotModeBegin
    {
        /// <summary>
        /// Called when player enters a seamoth (pilot mode begins).
        /// Refreshes bar position to show it when entering Seamoth with air bladder module.
        /// Uses retry logic to handle UI initialization delays.
        /// </summary>
        [HarmonyPostfix]
        public static void Postfix(SeaMoth __instance)
        {
            // Check if this vehicle has the air bladder behavior
            if (__instance.TryGetComponent<SeamothAirBladderBehavior>(out var behavior))
            {
                behavior.RefreshBarPositionWithRetry();
            }
        }
    }
}
