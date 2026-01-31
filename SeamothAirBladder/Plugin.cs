using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Handlers;
using SeamothAirBladder.Items;
using SeamothAirBladder.Utilities;

namespace SeamothAirBladder
{
    [BepInPlugin(GUID, Name, Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static Options Options { get; } = OptionsPanelHandler.RegisterModOptions<Options>();

        public static ManualLogSource? Log;

        internal const string GUID = "com.visolar6.seamothairbladder";

        internal const string Name = "Seamoth Air Bladder";

        internal const string Version = "0.1.0";

        private readonly Harmony _harmony = new(GUID);

        /// <summary>
        /// Awakes the plugin (on game start).
        /// </summary>
        public void Awake()
        {
            Log = Logger;
        }

        public void Start()
        {
            Log?.LogInfo($"Patching hooks...");
            _harmony.PatchAll();

            Log?.LogInfo($"Patching localization...");
            LanguagesHandler.GlobalPatch();

            Log?.LogInfo($"Patching seamoth air bladder module...");
            SeamothAirBladderModule seamothAirBladderModule = new();
            seamothAirBladderModule.Patch();

            Log?.LogInfo($"Initialized!");
        }
    }
}
