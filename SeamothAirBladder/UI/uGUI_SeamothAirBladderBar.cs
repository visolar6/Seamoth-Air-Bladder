using System.Linq;
using SeamothAirBladder.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace SeamothAirBladder.UI
{
    public class uGUI_SeamothAirBladderBar
    {
        private Image? barImage;
        private GameObject? barRoot;
        private int lastKnownSlotIndex = -1;
        private static Material? cachedIconBarMaterial;

        public float FillAmount
        {
            get => barImage != null ? barImage.fillAmount : 0f;
            set { barImage?.fillAmount = Mathf.Clamp01(value); }
        }

        public Color BarColor
        {
            get => barImage != null ? barImage.color : Color.white;
            set { barImage?.color = value; }
        }

        public void Create()
        {
            Transform? seamothHUDParent = FindSeamothHUDParent();

            if (seamothHUDParent != null)
            {
                // Center on the slot icon (0.5, 0.5 anchor) 
                CreateBarWithShaderProps(seamothHUDParent, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(67, 67), 37f, 8f, 0.1f);
            }
            else
            {
                Plugin.Log?.LogWarning("[AirBladderBar] Could not find slot on initial creation, will retry with backoff");
            }
        }

        private Transform? FindSeamothHUDParent()
        {
            // Find ScreenCanvas
            var allCanvases = Object.FindObjectsOfType<Canvas>();
            foreach (var canvas in allCanvases)
            {
                if (canvas.name == "ScreenCanvas")
                {
                    var hud = canvas.transform.Find("HUD");
                    if (hud != null)
                    {
                        var content = hud.Find("Content");
                        if (content != null)
                        {
                            // QuickSlots contains vehicle module icons when in a vehicle
                            var quickSlots = content.Find("QuickSlots");
                            if (quickSlots != null)
                            {
                                // Search all slots for the air bladder module
                                foreach (Transform child in quickSlots)
                                {
                                    if (child.name.StartsWith("QuickSlot Icon"))
                                    {
                                        // Check if this slot has the air bladder module
                                        var foreground = child.Find("Foreground");
                                        if (foreground != null)
                                        {
                                            var icon = foreground.GetComponent<uGUI_Icon>();
                                            // Only match our specific sprite name
                                            if (icon != null && icon.sprite != null &&
                                                icon.sprite.name.IndexOf("seamothairbladder", System.StringComparison.OrdinalIgnoreCase) >= 0)
                                            {
                                                // Verify it's actually the air bladder by checking parent has uGUI_ItemIcon
                                                var itemIcon = child.GetComponent<uGUI_ItemIcon>();
                                                if (itemIcon != null)
                                                {
                                                    return child;
                                                }
                                            }
                                        }
                                    }
                                }

                                // No fallback - only render on specific slot, never on container
                                Plugin.Log?.LogWarning("[AirBladderBar] Could not find air bladder module slot");
                                return null;
                            }
                        }
                    }
                }
            }

            Plugin.Log?.LogWarning("[AirBladderBar] QuickSlots not found");
            return null;
        }

        private void CreateBarWithShaderProps(Transform parent, Vector2 parentAnchor, Vector2 offset, Vector2 size, float radius, float width, float cut)
        {
            var bgGO = new GameObject($"AirBladderBar_BG");
            barRoot = bgGO; // Store reference to root GameObject
            bgGO.transform.SetParent(parent, false);
            var bgRect = bgGO.AddComponent<RectTransform>();
            bgRect.anchorMin = parentAnchor;
            bgRect.anchorMax = parentAnchor;
            bgRect.pivot = new Vector2(0.5f, 0.5f); // Center pivot
            bgRect.sizeDelta = size;
            bgRect.anchoredPosition = offset; // Offset from anchor
            var bgImage = bgGO.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0f); // Fully transparent
            bgImage.raycastTarget = false;

            var barGO = new GameObject($"AirBladderBar");
            barGO.transform.SetParent(bgGO.transform, false);
            var barRect = barGO.AddComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0.5f, 0.5f);
            barRect.anchorMax = new Vector2(0.5f, 0.5f);
            barRect.pivot = new Vector2(0.5f, 0.5f);
            barRect.sizeDelta = size;
            barRect.anchoredPosition = Vector2.zero;

            var img = barGO.AddComponent<Image>();
            img.sprite = null; // No sprite, like vanilla
            img.type = Image.Type.Simple; // Simple type, like vanilla
            img.color = Color.white; // White to allow material colors through

            var iconBarMat = cachedIconBarMaterial;
            if (iconBarMat == null)
            {
                iconBarMat = Resources.FindObjectsOfTypeAll<Material>()
                    .FirstOrDefault(m => m.name == "UI/IconBar");
                cachedIconBarMaterial = iconBarMat;
            }

            if (iconBarMat != null)
            {
                // Create material instance to avoid modifying the shared material
                var matInstance = new Material(iconBarMat);
                img.material = matInstance;

                // Set the shader properties to match vanilla
                matInstance.SetFloat("_Radius", radius);
                matInstance.SetFloat("_Width", width);
                matInstance.SetFloat("_Cut", cut);
                matInstance.SetFloat("_Value", 1f); // Full bar
                matInstance.SetVector("_Size", new Vector4(size.x, size.y, 0, 0));

                // Critical properties that were missing!
                matInstance.SetFloat("_Antialias", 1.5f);
                matInstance.SetFloat("_Edge", 2.3f);
                matInstance.SetColor("_ColorBackground", new Color(0.212f, 0.318f, 0.525f, 1f));
                matInstance.SetColor("_ColorEdge", new Color(0.867f, 0.961f, 0.996f, 0.7f));

                // Color gradient properties
                matInstance.SetFloat("_Value0", 0.2f);
                matInstance.SetFloat("_Value1", 0.5f);
                matInstance.SetFloat("_Value2", 0.8f);
                matInstance.SetColor("_Color0", new Color(0.816f, 0.447f, 0.325f, 1f)); // Red/orange
                matInstance.SetColor("_Color1", new Color(0.976f, 0.839f, 0.341f, 1f)); // Yellow
                matInstance.SetColor("_Color2", new Color(0.643f, 0.843f, 0.412f, 1f)); // Green

            }
            else
            {
                Plugin.Log?.LogError("UI/IconBar material not found!");
            }

            if (barImage == null)
                barImage = img;
        }

        public void Update(float airRemaining, float airCapacity)
        {
            // Only update shader fill value - position updates handled by callbacks
            if (barImage != null)
            {
                float fill = Mathf.Clamp01(airRemaining / airCapacity);
                barImage.material?.SetFloat("_Value", fill);
            }
        }

        public void RefreshPosition()
        {
            // Find which slot currently has the air bladder module
            var slotWithModule = FindSlotWithAirBladderModule();

            if (slotWithModule == null)
            {
                // Module not found - hide the bar
                HideBar();
                return;
            }

            // Get slot index
            int slotIndex = GetSlotIndex(slotWithModule);

            // Check if slot changed
            if (slotIndex != lastKnownSlotIndex)
            {
                // Create bar if it doesn't exist yet
                if (barRoot == null)
                {
                    CreateBarWithShaderProps(slotWithModule, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(67, 67), 37f, 8f, 0.1f);
                }
                else
                {
                    // Re-parent to new slot
                    barRoot.transform.SetParent(slotWithModule, false);
                    var bgRect = barRoot.GetComponent<RectTransform>();
                    if (bgRect != null)
                    {
                        // Reset anchors to match original setup
                        bgRect.anchorMin = new Vector2(0.5f, 0.5f);
                        bgRect.anchorMax = new Vector2(0.5f, 0.5f);
                        bgRect.pivot = new Vector2(0.5f, 0.5f);
                        bgRect.anchoredPosition = Vector2.zero;
                    }
                    barRoot.SetActive(true);
                }

                lastKnownSlotIndex = slotIndex;
            }
            else if (barRoot != null && !barRoot.activeSelf)
            {
                // Module is back, show the bar
                barRoot.SetActive(true);
            }
        }

        private Transform? FindSlotWithAirBladderModule()
        {
            var allCanvases = Object.FindObjectsOfType<Canvas>();
            foreach (var canvas in allCanvases)
            {
                if (canvas.name == "ScreenCanvas")
                {
                    var quickSlots = canvas.transform.Find("HUD")?.Find("Content")?.Find("QuickSlots");
                    if (quickSlots != null)
                    {
                        int slotCount = 0;
                        foreach (Transform child in quickSlots)
                        {
                            if (child.name.StartsWith("QuickSlot Icon"))
                            {
                                // Check if this slot has the air bladder module
                                var foreground = child.Find("Foreground");
                                if (foreground != null)
                                {
                                    var icon = foreground.GetComponent<uGUI_Icon>();

                                    // Only match our specific sprite name
                                    if (icon != null && icon.sprite != null &&
                                        icon.sprite.name.IndexOf("seamothairbladder", System.StringComparison.OrdinalIgnoreCase) >= 0)
                                    {
                                        // Verify it's actually the air bladder by checking parent has uGUI_ItemIcon
                                        var itemIcon = child.GetComponent<uGUI_ItemIcon>();
                                        if (itemIcon != null)
                                        {
                                            return child;
                                        }
                                    }
                                }
                                slotCount++;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private int GetSlotIndex(Transform slot)
        {
            if (slot.parent == null) return -1;

            int index = 0;
            foreach (Transform child in slot.parent)
            {
                if (child.name.StartsWith("QuickSlot Icon"))
                {
                    if (child == slot) return index;
                    index++;
                }
            }
            return -1;
        }

        /// <summary>
        /// Checks if the bar is currently positioned and visible.
        /// Used by retry logic to determine if positioning was successful.
        /// </summary>
        public bool IsPositioned()
        {
            return barRoot != null && barRoot.activeSelf && lastKnownSlotIndex >= 0;
        }

        /// <summary>
        /// Explicitly hides the bar. Called when exiting vehicle to ensure bar is hidden.
        /// </summary>
        public void HideBar()
        {
            if (barRoot != null && barRoot.activeSelf)
            {
                barRoot.SetActive(false);
            }
            lastKnownSlotIndex = -1;
        }
    }
}