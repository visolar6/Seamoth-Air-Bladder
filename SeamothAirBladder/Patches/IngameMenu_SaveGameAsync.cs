using HarmonyLib;
using SeamothAirBladder.Mono;

namespace SeamothAirBladder.Patches
{
    /// <summary>
    /// Patches IngameMenu.SaveGameAsync to save air bladder state when the player saves the game.
    /// </summary>
    [HarmonyPatch(typeof(IngameMenu), "SaveGameAsync")]
    public class IngameMenu_SaveGameAsync
    {
        [HarmonyPrefix]
        public static void Prefix()
        {
            // Update and save all air bladder states when game saves
            SeamothAirBladderStateManager.SaveAllAirStates();
        }
    }
}
