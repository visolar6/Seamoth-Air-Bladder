using System;
using System.Reflection;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using SeamothAirBladder.Mono;
using SeamothAirBladder.Utilities;
using UnityEngine;

namespace SeamothAirBladder.Items
{
    public class SeamothAirBladderModule
    {
        public static string ClassID = "SeamothAirBladderModule";

        public static TechType TechType { get; private set; } = 0;

        public PrefabInfo Info { get; private set; }

        public SeamothAirBladderModule()
        {
            Sprite? sprite = ResourceHandler.LoadSpriteFromFile("Assets/Sprite/seamothairbladder.png");
            if (sprite != null)
            {
                sprite.name = "seamothairbladder";
            }
            else
            {
                Plugin.Log?.LogError("Failed to load Seamoth Air Bladder module icon sprite.");
            }

            Info = PrefabInfo.WithTechType(classId: ClassID, displayName: null, description: null, techTypeOwner: Assembly.GetExecutingAssembly())
                .WithSizeInInventory(new(1, 1))
                .WithIcon(sprite);

            TechType = Info.TechType;
        }

        public void Patch()
        {
            RecipeData recipe = new()
            {
                craftAmount = 1,
                Ingredients = [
                    new(TechType.WiringKit, 1),
                    new(TechType.Silicone, 4),
                    new(TechType.Bladderfish, 4),
                ]
            };

            CustomPrefab customPrefab = new(Info);

            CloneTemplate cloneTemplate = new(Info, TechType.SeamothTorpedoModule);

            customPrefab.SetGameObject(cloneTemplate);

            var scanningGadget = customPrefab
                .SetUnlock(TechType.WiringKit)
                .WithPdaGroupCategoryAfter(TechGroup.VehicleUpgrades, TechCategory.VehicleUpgrades, TechType.SeamothSonarModule)
                .WithCompoundTechsForUnlock([TechType.Silicone, TechType.Bladderfish]);

            customPrefab.SetVehicleUpgradeModule(EquipmentType.SeamothModule, QuickSlotType.Selectable)
                .WithEnergyCost(0f)
                .WithOnModuleAdded(OnModuleAdded)
                .WithOnModuleRemoved(OnModuleRemoved)
                .WithOnModuleUsed(OnModuleUsed);

            customPrefab.SetRecipe(recipe)
                .WithCraftingTime(2.5f)
                .WithFabricatorType(CraftTree.Type.SeamothUpgrades)
                .WithStepsToFabricatorTab(["SeamothModules"]);

            customPrefab.Register();
        }

        private void OnModuleAdded(Vehicle vehicle, int slotID)
        {
            if (!vehicle.gameObject.TryGetComponent(out SeamothAirBladderBehavior mono))
                mono = vehicle.gameObject.EnsureComponent<SeamothAirBladderBehavior>();

            // Refresh bar position to match the new slot
            mono?.RefreshBarPosition();
        }

        private void OnModuleRemoved(Vehicle vehicle, int slotID)
        {
            // Don't destroy the behavior - let the bar's continuous position checking handle hiding/showing
            // This avoids race conditions when moving modules between slots
            if (vehicle.gameObject.TryGetComponent(out SeamothAirBladderBehavior mono))
                mono?.RefreshBarPosition();
        }

        private void OnModuleUsed(Vehicle vehicle, int slotID, float charge, float chargeScalar)
        {
            if (!vehicle.gameObject.TryGetComponent(out SeamothAirBladderBehavior mono))
                mono = vehicle.gameObject.EnsureComponent<SeamothAirBladderBehavior>();

            try
            {
                if (mono.IsInflated)
                {
                    mono.Deflate();
                }
                else
                {
                    mono.Inflate();
                }
            }
            catch (Exception e)
            {
                Plugin.Log?.LogError($"Error while using seamoth air bladder module: {e.Message}\n{e.StackTrace}");
            }
        }
    }
}