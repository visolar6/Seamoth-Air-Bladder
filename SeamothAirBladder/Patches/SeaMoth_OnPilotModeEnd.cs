using HarmonyLib;
using SeamothAirBladder.Mono;

namespace SeamothAirBladder.Patches
{
    /// <summary>
    /// Patches SeaMoth.OnPilotModeEnd to hide the air bladder bar when exiting seamoths.
    /// </summary>
    [HarmonyPatch(typeof(SeaMoth), "OnPilotModeEnd")]
    public static class SeaMoth_OnPilotModeEnd
    {
        /// <summary>
        /// Called when player exits a seamoth (pilot mode ends).
        /// Hides the bar when exiting Seamoth.
        /// </summary>
        [HarmonyPostfix]
        public static void Postfix(SeaMoth __instance)
        {
            // Check if this vehicle has the air bladder behavior
            if (__instance.TryGetComponent<SeamothAirBladderBehavior>(out var behavior))
            {
                behavior.HideBar();
            }
        }
    }
}
