using System.Linq;
using SeamothAirBladder.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace SeamothAirBladder.UI
{
    public class uGUI_SeamothAirBladderBar
    {
        private Image barImage;
        private Canvas canvas;

        public float FillAmount
        {
            get => barImage != null ? barImage.fillAmount : 0f;
            set { if (barImage != null) barImage.fillAmount = Mathf.Clamp01(value); }
        }

        public Color BarColor
        {
            get => barImage != null ? barImage.color : Color.white;
            set { if (barImage != null) barImage.color = value; }
        }

        public void Create()
        {
            // Create Canvas
            canvas = new GameObject("SimpleAirBarCanvas").AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5000;
            Object.DontDestroyOnLoad(canvas.gameObject);

            // Create Bar Background
            var bgGO = new GameObject("BarBackground");
            bgGO.transform.SetParent(canvas.transform, false);
            var bgRect = bgGO.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.5f, 0.1f);
            bgRect.anchorMax = new Vector2(0.5f, 0.1f);
            bgRect.pivot = new Vector2(0.5f, 0.5f);
            bgRect.sizeDelta = new Vector2(67, 67);
            bgRect.anchoredPosition = Vector2.zero;
            var bgImage = bgGO.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.5f);

            // Create Bar Foreground
            var barGO = new GameObject("Bar");
            barGO.transform.SetParent(bgGO.transform, false);
            var barRect = barGO.AddComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0.5f, 0.5f);
            barRect.anchorMax = new Vector2(0.5f, 0.5f);
            barRect.pivot = new Vector2(0.5f, 0.5f);
            barRect.sizeDelta = new Vector2(67, 67);
            barRect.anchoredPosition = Vector2.zero;
            barImage = barGO.AddComponent<Image>();
            // Use a circular sprite - Radial360 needs proper geometry
            var circleSprite = ResourceHandler.LoadSpriteFromFile("Assets/Sprite/circle.png");
            barImage.sprite = circleSprite;
            barImage.type = Image.Type.Filled;
            barImage.fillMethod = Image.FillMethod.Radial360;
            barImage.fillOrigin = (int)Image.Origin360.Bottom; // Match vanilla (fillOrigin = 0)
            barImage.fillClockwise = true;
            barImage.fillAmount = 1f;
            barImage.color = Color.green;
            var iconBarMat = Resources.FindObjectsOfTypeAll<Material>()
                .FirstOrDefault(m => m.name == "UI/IconBar");
            if (iconBarMat != null)
            {
                barImage.material = iconBarMat;
                Debug.Log("[SeamothAirBladderBar] Successfully applied UI/IconBar material");
            }
            else
            {
                Debug.LogWarning("[SeamothAirBladderBar] UI/IconBar material not found!");
            }
        }

        public void Update(float airRemaining, float airCapacity)
        {
            if (barImage == null) return;
            float fill = Mathf.Clamp01(airRemaining / airCapacity);
            barImage.fillAmount = fill;

            // Change color based on fill level
            if (fill > 0.5f)
                barImage.color = Color.green;
            else if (fill > 0.25f)
                barImage.color = Color.yellow;
            else
                barImage.color = Color.red;
        }

        public void Destroy()
        {
            if (canvas != null)
                Object.Destroy(canvas.gameObject);
        }
    }
}