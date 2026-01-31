using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Nautilus.Handlers;

namespace SeamothAirBladder.Utilities
{
    namespace LocalizationHandler
    {
        [XmlRoot("LocalizationPackages")]
        public class LocalizationPackages
        {
            [XmlElement("LocalizationPackage")]
            public LocalizationPackage[]? Localizations;
        }

        /// <summary>
        /// LocalizationPackage is a Localization Package ref - 
        /// </summary>
        public class LocalizationPackage
        {
            [XmlAttribute("Lang")]
            public string? Lang;

            [XmlElement("Text")]
            public Text[]? Texts;
        }

        public class Text
        {
            [XmlAttribute]
            public string? key;
            [XmlText]
            public string? value;
        }
    }

    public class LanguagesHandler
    {
        private static readonly string ModPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static readonly string filename = "Localizations.xml";
        public static void LanguagePatch()
        {
            XmlSerializer serializer = new(typeof(LocalizationHandler.LocalizationPackages));

            FileStream fs = new(Path.Combine(ModPath, filename), FileMode.Open, FileAccess.Read);
            LocalizationHandler.LocalizationPackages lps;

            lps = (LocalizationHandler.LocalizationPackages)serializer.Deserialize(fs);

            var localizations = lps.Localizations;
            if (localizations != null)
            {
                var selectedPackage = localizations.SingleOrDefault(lp => localizations.Any(lp1 => lp1.Lang == Language.main.GetCurrentLanguage()) ? lp.Lang == Language.main.GetCurrentLanguage() : lp.Lang == Language.defaultLanguage);
                if (selectedPackage != null && selectedPackage.Texts != null)
                {
                    foreach (LocalizationHandler.Text text in selectedPackage.Texts)
                    {
                        if (Language.main.Get(text.key) != null)
                        {
                            LanguageHandler.SetLanguageLine(text.key, text.value);
                        }
                        else
                        {
                            Plugin.Log?.LogWarning($"Key {text.key} does not reference any key in game.");
                        }
                    }
                }
                else
                {
                    Plugin.Log?.LogWarning("No matching localization package or texts found.");
                }
            }
            else
            {
                Plugin.Log?.LogWarning("No localizations found in the XML file.");
            }
        }

        public static void GlobalPatch()
        {
            XmlSerializer serializer = new(typeof(LocalizationHandler.LocalizationPackages));

            FileStream fs = new(Path.Combine(ModPath, filename), FileMode.Open, FileAccess.Read);
            LocalizationHandler.LocalizationPackages lps;

            lps = (LocalizationHandler.LocalizationPackages)serializer.Deserialize(fs);

            if (lps.Localizations != null)
            {
                foreach (LocalizationHandler.LocalizationPackage locpack in lps.Localizations)
                {
                    foreach (LocalizationHandler.Text text in locpack.Texts ?? Enumerable.Empty<LocalizationHandler.Text>())
                    {
                        if (string.IsNullOrEmpty(text.value))
                            continue;
                        LanguageHandler.SetLanguageLine(text.key, text.value, locpack.Lang);
                    }
                }
            }
            else
            {
                Plugin.Log?.LogWarning("No localizations found in the XML file.");
            }
        }
    }
}
