using HarmonyLib;
using SeamothAirBladder.Items;

namespace SeamothAirBladder.Patches
{
    /// <summary>
    /// Patches Equipment.AddItem to prevent duplicate air bladder installations.
    /// </summary>
    [HarmonyPatch]
    public class Equipment_AddItem_Patch
    {
        /// <summary>
        /// Prevents adding duplicate air bladder modules to the same vehicle.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Equipment), nameof(Equipment.AddItem))]
        public static bool AddItem_Prefix(Equipment __instance, string slot, InventoryItem newItem, ref bool __result)
        {
            // Only check if this is a vehicle equipment and the item is an air bladder module
            if (newItem?.item?.GetTechType() == SeamothAirBladderModule.TechType)
            {
                // Check if this equipment belongs to a vehicle
                var owner = __instance.owner;
                if (owner != null && owner.TryGetComponent<Vehicle>(out var vehicle))
                {
                    var slotNames = new System.Collections.Generic.List<string>();
                    __instance.GetSlots(EquipmentType.VehicleModule, slotNames);

                    // First check if this exact item is already equipped (indicating a move operation)
                    bool isAlreadyEquipped = false;
                    foreach (var slotName in slotNames)
                    {
                        var equippedItem = __instance.GetItemInSlot(slotName);
                        if (equippedItem == newItem)
                        {
                            isAlreadyEquipped = true;
                            break;
                        }
                    }

                    // If already equipped, this is a move - allow it
                    if (isAlreadyEquipped)
                        return true;

                    // Not a move - check if there's already an air bladder installed
                    foreach (var slotName in slotNames)
                    {
                        var equippedItem = __instance.GetItemInSlot(slotName);
                        if (equippedItem?.item?.GetTechType() == SeamothAirBladderModule.TechType)
                        {
                            // Already have one - block this new installation
                            ErrorMessage.AddMessage(Language.main.Get("SeamothAirBladderModule_MaxOneInstalled"));
                            __result = false;
                            return false;
                        }
                    }
                }
            }
            return true; // Continue with original method
        }
    }
}