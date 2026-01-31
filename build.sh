#!/bin/sh

dotnet build -c Release
if [ $? -ne 0 ]; then
    echo "✗ Build failed"
    exit 1
fi

# Copy DLL and assets to Subnautica BepInEx plugins folder
# Set SUBNAUTICA_PATH environment variable or edit here for your system
SUBNAUTICA_PATH="${SUBNAUTICA_PATH:-D:/SteamLibrary/steamapps/common/Subnautica}"
PLUGIN_DIR="$SUBNAUTICA_PATH/BepInEx/plugins/SeamothAirBladder"
DLL_PATH="SeamothAirBladder/bin/Release/net472/SeamothAirBladder.dll"
LOCALIZATION_PATH="SeamothAirBladder/Localizations.xml"
ASSETS_PATH="SeamothAirBladder/Assets"
if [ -f "$DLL_PATH" ]; then
    mkdir -p "$PLUGIN_DIR"
    cp "$DLL_PATH" "$PLUGIN_DIR/"
    echo "✓ DLL copied to $PLUGIN_DIR"

    if [ -f "$LOCALIZATION_PATH" ]; then
        cp "$LOCALIZATION_PATH" "$PLUGIN_DIR/"
        echo "✓ Localizations.xml copied to $PLUGIN_DIR"
    else
        echo "✗ Localizations.xml not found at $LOCALIZATION_PATH"
    fi

    if [ -d "$ASSETS_PATH" ]; then
        cp -r "$ASSETS_PATH" "$PLUGIN_DIR/"
        echo "✓ Assets copied to $PLUGIN_DIR/Assets"
    else
        echo "✗ Assets directory not found at $ASSETS_PATH"
    fi
else
    echo "✗ DLL not found at $DLL_PATH"
    echo "Make sure you have built the project in Release mode"
    exit 1
fi

echo "✓ Build and deployment complete!"