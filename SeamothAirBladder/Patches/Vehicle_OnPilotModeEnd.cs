using HarmonyLib;
using SeamothAirBladder.Items;
using SeamothAirBladder.Mono;

namespace SeamothAirBladder.Patches
{
    /// <summary>
    /// Patches Vehicle.OnPilotModeEnd to hide the air bladder bar when exiting vehicles.
    /// </summary>
    [HarmonyPatch]
    public static class Vehicle_OnPilotModeEnd_Patch
    {
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
