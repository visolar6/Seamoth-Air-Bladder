using System.Collections;
using System.Collections.Generic;
using SeamothAirBladder.Config;
using SeamothAirBladder.UI;
using SeamothAirBladder.Utilities;
using UnityEngine;

namespace SeamothAirBladder.Mono
{
    public class SeamothAirBladderBehavior : MonoBehaviour
    {

        // --- Constants ---
        private const int MaxFramesWithoutModule = 10; // Allow brief gaps during slot movement

        // --- Fields ---
        private SeamothAirBladderBar? bar = null;
        private SeamothAirBladderAudio? audio = null;
        private Rigidbody? seamothRigidbody;
        private float prevYPosition = float.MinValue;
        private Vehicle? cachedVehicle;
        private int framesWithoutModule = 0;
        private Coroutine? barPositionRetryCoroutine = null;

        // --- Properties ---
        public float AirCapacity { get; } = 100f;
        public float AirDischargeRate { get; } = 10f;
        public float AirRechargeRate { get; } = 40f;
        public float AirRemaining { get; private set; } = 100f;
        public bool IsInflated { get; private set; } = false;

        // --- Unity Methods ---
        private void Awake()
        {
            bar = new SeamothAirBladderBar();
            bar.Create(); // Create immediately so RefreshBarPosition works

            // Restore air state for this vehicle if it was previously saved
            var prefabId = GetComponent<PrefabIdentifier>();
            if (prefabId != null && SeamothAirBladderStateManager.TryRestoreAirState(prefabId.Id, out float restoredAir))
            {
                AirRemaining = restoredAir;
            }

            // Cache Vehicle and Rigidbody components
            cachedVehicle = GetComponent<Vehicle>();
            seamothRigidbody = GetComponentInParent<Rigidbody>();

            // Setup audio component
            audio = gameObject.AddComponent<SeamothAirBladderAudio>();

            prevYPosition = transform.position.y;
        }

        private void Update()
        {
            // Check if the air bladder module still exists on the vehicle
            if (!HasAirBladderModule())
            {
                framesWithoutModule++;
                if (framesWithoutModule >= MaxFramesWithoutModule)
                {
                    Destroy(this);
                    return;
                }
            }
            else
            {
                framesWithoutModule = 0;
            }

            HandleInflation();
            HandleRechargeWithSurfaceCrossing();
            bar?.Update(AirRemaining, AirCapacity);
            prevYPosition = transform.position.y;
        }

        private void OnDestroy()
        {
            // Update air state in memory (will be saved to disk when game saves)
            var prefabId = GetComponent<PrefabIdentifier>();
            if (prefabId != null)
            {
                SeamothAirBladderStateManager.UpdateAirState(prefabId.Id, AirRemaining);
            }
        }


        // --- Public API ---

        /// <summary>
        /// Inflates the Seamoth's air bladder, increasing its buoyancy until it runs out of air. Discharges air over time.
        /// </summary>
        public void Inflate()
        {
            if (IsBelowSurface() && AirRemaining > 0f && !IsInflated)
            {
                IsInflated = true;
                audio?.PlayInflateSound();
            }
            else
            {
                Plugin.Log?.LogWarning("Cannot inflate Seamoth air bladder: either already inflated, no air remaining, or not below surface.");
            }
        }

        /// <summary>
        /// Deflates the Seamoth's air bladder, returning its buoyancy to normal. Does not recharge air, only stops inflation.
        /// </summary>
        public void Deflate()
        {
            if (IsInflated)
            {
                IsInflated = false;
                audio?.StopInflateSound();
            }
            else
            {
                Plugin.Log?.LogWarning("Cannot deflate Seamoth air bladder: already deflated.");
            }
        }

        /// <summary>
        /// Instantly recharges the air bladder to full capacity when docked.
        /// </summary>
        public void StartDockedRecharge()
        {
            if (AirRemaining < AirCapacity)
            {
                StartCoroutine(PlayRechargeSoundWithDelay(1f));
            }
            AirRemaining = AirCapacity;
        }

        /// <summary>
        /// Updates this vehicle's air state in the config dictionary.
        /// </summary>
        public void UpdateAirStateInConfig()
        {
            var prefabId = GetComponent<PrefabIdentifier>();
            if (prefabId != null)
            {
                SeamothAirBladderStateManager.UpdateAirState(prefabId.Id, AirRemaining);
            }
        }

        /// <summary>
        /// Updates the bar position to match the current slot containing the air bladder module.
        /// Called from OnModuleAdded when the module is installed or moved.
        /// </summary>
        public void RefreshBarPosition()
        {
            bar?.RefreshPosition();
        }


        /// <summary>
        /// Hides the bar explicitly. Called when exiting vehicle.
        /// </summary>
        public void HideBar()
        {
            bar?.HideBar();
        }


        /// <summary>
        /// Attempts to refresh bar position with retries to handle UI initialization delays.
        /// Useful when entering vehicle - QuickSlots UI may not be ready immediately.
        /// </summary>
        public void RefreshBarPositionWithRetry()
        {
            // Cancel any existing retry coroutine
            if (barPositionRetryCoroutine != null)
            {
                StopCoroutine(barPositionRetryCoroutine);
            }
            barPositionRetryCoroutine = StartCoroutine(RetryRefreshBarPosition());
        }

        // --- Private Methods ---

        private IEnumerator RetryRefreshBarPosition()
        {
            const int maxAttempts = 20;
            const float delayBetweenAttempts = 0.1f; // 100ms

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                bar?.RefreshPosition();

                // Check if bar was successfully positioned
                if (bar != null && bar.IsPositioned())
                {
                    yield break;
                }

                yield return new WaitForSeconds(delayBetweenAttempts);
            }

            Plugin.Log?.LogWarning($"[AirBladderBehavior] Failed to position bar after {maxAttempts} attempts");
        }

        private bool HasAirBladderModule()
        {
            if (cachedVehicle == null)
                return false;

            var modules = cachedVehicle.GetSlotBinding();
            for (int i = 0; i < modules.Length; i++)
            {
                if (modules[i] == Items.SeamothAirBladderModule.TechType)
                    return true;
            }
            return false;
        }

        private void HandleInflation()
        {
            if (!IsInflated) return;

            // If at surface, apply downward force to counteract excess velocity, then deflate
            if (IsAboveSurface())
            {
                if (seamothRigidbody != null)
                {
                    float upwardVelocity = seamothRigidbody.velocity.y;
                    if (upwardVelocity > 0f)
                    {
                        float impulse = upwardVelocity * seamothRigidbody.mass * 0.8f; // 80% for a gentler effect
                        seamothRigidbody.AddForce(Vector3.down * impulse, ForceMode.Impulse);
                    }
                }
                Deflate();
            }
            else
            {
                AirRemaining -= AirDischargeRate * Time.deltaTime;
                if (AirRemaining <= 0f)
                {
                    AirRemaining = 0f;
                    Deflate();
                }
                else
                {
                    if (seamothRigidbody != null)
                    {
                        float buoyancyForce = Mathf.Clamp(Plugin.Options.BuoyancyForce, Constants.MinBuoyancyForce, Constants.MaxBuoyancyForce);
                        VehicleHandler.ApplyBuoyancy(seamothRigidbody, buoyancyForce);
                    }
                    else
                    {
                        Plugin.Log?.LogWarning("Seamoth Rigidbody not found, cannot apply buoyancy.");
                    }
                }
            }
        }

        private void HandleRechargeWithSurfaceCrossing()
        {
            float threshold = 1.0f;
            float surfaceThreshold = Constants.SurfaceLevel - threshold;
            bool wasBelow = prevYPosition < surfaceThreshold;
            bool isNowAbove = transform.position.y >= surfaceThreshold;

            if (isNowAbove)
            {
                // Play recharge sound only on crossing from below to above, and only if air is not full
                if (wasBelow && AirRemaining < AirCapacity)
                {
                    audio?.PlayRechargeSound();
                }
                AirRemaining = Mathf.Min(AirRemaining + AirRechargeRate * Time.deltaTime, AirCapacity);
            }
        }

        private IEnumerator PlayRechargeSoundWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            audio?.PlayRechargeSound();
        }

        private bool IsAboveSurface()
        {
            // Assumes Y axis is up; adjust if your game uses a different axis for depth
            return transform.position.y >= Constants.SurfaceLevel;
        }

        private bool IsBelowSurface()
        {
            return transform.position.y < Constants.SurfaceLevel;
        }
    }
}