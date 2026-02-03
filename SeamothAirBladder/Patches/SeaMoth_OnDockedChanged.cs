using HarmonyLib;
using SeamothAirBladder.Mono;

namespace SeamothAirBladder.Patches
{
    /// <summary>
    /// Patches SeaMoth.OnDockedChanged to manage docked recharge state.
    /// </summary>
    [HarmonyPatch(typeof(SeaMoth), "OnDockedChanged")]
    public static class SeaMoth_OnDockedChanged
    {
        /// <summary>
        /// Called when SeaMoth docking state changes.
        /// Starts or stops docked recharge based on docking state.
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="docked"></param>
        [HarmonyPostfix]
        public static void Postfix(SeaMoth __instance, bool docked)
        {
            if (__instance.TryGetComponent<SeamothAirBladderBehavior>(out var behavior))
            {
                if (docked)
                {
                    behavior.StartDockedRecharge();
                }
            }
        }
    }
}