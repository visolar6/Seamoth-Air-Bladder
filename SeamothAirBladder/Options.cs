using Nautilus.Options.Attributes;

namespace SeamothAirBladder
{
    [Menu("Seamoth Air Bladder")]
    public class Options : Nautilus.Json.ConfigFile
    {
        [Slider(LabelLanguageId = "Options.SeamothAirBladder_BuoyancyForce", TooltipLanguageId = "Options.SeamothAirBladder_BuoyancyForce.Tooltip", DefaultValue = 3500f, Min = 2000f, Max = 5000f, Step = 10f)]
        public float BuoyancyForce = 3500f;
    }
}
