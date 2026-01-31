using Nautilus.Options.Attributes;

namespace SeamothAirBladder
{
    [Menu("Seamoth Air Bladder")]
    public class Options : Nautilus.Json.ConfigFile
    {
        [Toggle(LabelLanguageId = "Options.SeamothAirBladder_AutoInflate", TooltipLanguageId = "Options.SeamothAirBladder_AutoInflate.Tooltip")]
        public bool AutoInflate = true;

        [Slider(LabelLanguageId = "Options.SeamothAirBladder_AutoInflateHealthThreshold", TooltipLanguageId = "Options.SeamothAirBladder_AutoInflateHealthThreshold.Tooltip", DefaultValue = 25f, Min = 5f, Max = 75f, Step = 5f)]
        public float AutoInflateHealthThreshold = 50f;

        public static Options? Instance { get; private set; }

        public Options()
        {
            Instance = this;
        }
    }
}
