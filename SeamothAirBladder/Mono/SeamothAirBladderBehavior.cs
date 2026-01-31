using System.Collections.Generic;
using SeamothAirBladder.UI;
using SeamothAirBladder.Utilities;
using UnityEngine;

namespace SeamothAirBladder.Mono
{
    public class SeamothAirBladderBehavior : MonoBehaviour
    {
        // Static dictionary to persist air state per vehicle across module removal/addition
        private static readonly Dictionary<int, float> vehicleAirState = [];

        // Air bladder state
        public float AirCapacity { get; } = 100f;
        public float AirDischargeRate { get; } = 10f;
        public float AirRechargeRate { get; } = 25f;
        public float AirRemaining { get; private set; } = 0f;
        public bool IsInflated { get; private set; } = false;
        private bool isRecharging = false;
        private uGUI_SeamothAirBladderBar? bar = null;


        // Audio
        private AudioSource? airBladderAudioSource;
        private AudioClip? airBladderClip;
        private Coroutine? fadeOutCoroutine;
        private AudioSource? rechargeAudioSource;
        private AudioClip? rechargeClip;


        // Physics
        private Rigidbody? seamothRigidbody;
        private const float surfaceLevel = 0.0f;
        private float prevYPosition = float.MinValue;

        // Module existence check
        private int framesWithoutModule = 0;
        private const int MaxFramesWithoutModule = 10; // Allow brief gaps during slot movement

        private void Awake()
        {
            bar = new uGUI_SeamothAirBladderBar();
            bar.Create(); // Create immediately so RefreshBarPosition works

            // Restore air state for this vehicle if it was previously removed and re-added
            int vehicleId = gameObject.GetInstanceID();
            if (vehicleAirState.TryGetValue(vehicleId, out float savedAir))
            {
                AirRemaining = savedAir;
                Plugin.Log?.LogInfo($"[AirBladderBehavior] Restored air state: {AirRemaining}/{AirCapacity}");
            }

            // Try to get the Seamoth's Rigidbody (assumes this script is attached to the Seamoth or its child)
            seamothRigidbody = GetComponentInParent<Rigidbody>();

            // Setup AudioSource for air bladder sound
            airBladderAudioSource = gameObject.GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
            airBladderAudioSource.playOnAwake = false;
            airBladderAudioSource.spatialBlend = 1f; // 3D sound
            airBladderAudioSource.loop = false;
            airBladderAudioSource.volume = 0.3f;
            airBladderAudioSource.pitch = 1.0f; // Optionally lower pitch for softer sound

            // Load the AudioClip from Assets/Audio/sfx_tool_airbladder_use_01.wav (relative to mod folder)
            string clipPath = System.IO.Path.Combine("Assets", "Audio", "sfx_tool_airbladder_use_01.wav");
            airBladderClip = ResourceHandler.LoadAudioClipFromFile(clipPath);
            if (airBladderClip == null)
                Plugin.Log?.LogError($"Failed to load air bladder AudioClip from {clipPath}");

            // Setup recharge AudioSource and load clip
            rechargeAudioSource = gameObject.AddComponent<AudioSource>();
            rechargeAudioSource.playOnAwake = false;
            rechargeAudioSource.spatialBlend = 1f;
            rechargeAudioSource.loop = false;
            rechargeAudioSource.volume = 0.15f;
            rechargeAudioSource.pitch = 1.0f;
            string rechargeClipPath = System.IO.Path.Combine("Assets", "Audio", "sfx_tool_airbladder_equip_fillair_01.wav");
            rechargeClip = ResourceHandler.LoadAudioClipFromFile(rechargeClipPath);
            if (rechargeClip == null)
                Plugin.Log?.LogError($"Failed to load recharge AudioClip from {rechargeClipPath}");

            prevYPosition = transform.position.y;
        }

        /// <summary>
        /// Updates the bar position to match the current slot containing the air bladder module.
        /// Called from OnModuleAdded when the module is installed or moved.
        /// </summary>
        public void RefreshBarPosition()
        {
            bar?.RefreshPosition();
        }

        private void Update()
        {
            // Check if the air bladder module still exists on the vehicle
            if (!HasAirBladderModule())
            {
                framesWithoutModule++;
                if (framesWithoutModule >= MaxFramesWithoutModule)
                {
                    Plugin.Log?.LogInfo("[AirBladderBehavior] No module found for 10 frames, destroying behavior");
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
            // Save air state for this vehicle so it persists across module removal/addition
            int vehicleId = gameObject.GetInstanceID();
            vehicleAirState[vehicleId] = AirRemaining;
            Plugin.Log?.LogInfo($"[AirBladderBehavior] Saved air state: {AirRemaining}/{AirCapacity}");

            bar?.Destroy();
        }

        /// <summary>
        /// Checks if any vehicle slot currently has the air bladder module installed.
        /// </summary>
        private bool HasAirBladderModule()
        {
            if (!TryGetComponent<Vehicle>(out var vehicle))
                return false;

            var modules = vehicle.GetSlotBinding();
            for (int i = 0; i < modules.Length; i++)
            {
                if (modules[i] == Items.SeamothAirBladderModule.TechType)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Handles the inflation logic: air discharge, buoyancy, depletion, and surfacing.
        /// </summary>
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
                        float buoyancyForce = 3000f;
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
            float surfaceThreshold = surfaceLevel - threshold;
            bool wasBelow = prevYPosition < surfaceThreshold;
            bool isNowAbove = transform.position.y >= surfaceThreshold;

            if (isNowAbove)
            {
                isRecharging = true;
                // Play recharge sound only on crossing from below to above, and only if air is not full
                if (wasBelow && AirRemaining < AirCapacity && rechargeAudioSource != null && rechargeClip != null)
                {
                    rechargeAudioSource.clip = rechargeClip;
                    rechargeAudioSource.Play();
                }
                AirRemaining += AirRechargeRate * Time.deltaTime;
                if (AirRemaining > AirCapacity)
                {
                    AirRemaining = AirCapacity;
                }
            }
            else
            {
                isRecharging = false;
            }
        }

        /// <summary>
        /// Determines if the Seamoth is at the surface (can recharge air).
        /// </summary>
        private bool IsAboveSurface()
        {
            // Assumes Y axis is up; adjust if your game uses a different axis for depth
            return transform.position.y >= surfaceLevel;
        }

        /// <summary>
        /// Determines if the Seamoth is near the surface (within a certain threshold) to allow for recharging.
        /// </summary>
        /// <returns></returns>
        private bool IsNearSurface()
        {
            float threshold = 1.0f; // Define how close to the surface is considered "near"
            return transform.position.y >= (surfaceLevel - threshold);
        }

        /// <summary>
        /// Determines if the Seamoth is below the surface.
        /// </summary>
        private bool IsBelowSurface()
        {
            return transform.position.y < surfaceLevel;
        }

        /// <summary>
        /// Inflates the Seamoth's air bladder, increasing its buoyancy until it runs out of air. Discharges air over time.
        /// </summary>
        public void Inflate()
        {
            if (IsBelowSurface() && AirRemaining > 0f && !IsInflated)
            {
                IsInflated = true;
                // Play air bladder AudioClip
                if (airBladderAudioSource != null && airBladderClip != null)
                {
                    airBladderAudioSource.Stop();
                    airBladderAudioSource.clip = airBladderClip;
                    airBladderAudioSource.volume = 0.3f;
                    airBladderAudioSource.Play();
                }
                else
                {
                    Plugin.Log?.LogWarning("Cannot play air bladder sound: AudioSource or AudioClip is null.");
                }
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
                // Fade out and stop the air bladder sound
                if (airBladderAudioSource != null && airBladderAudioSource.isPlaying)
                {
                    if (fadeOutCoroutine != null)
                        StopCoroutine(fadeOutCoroutine);
                    fadeOutCoroutine = StartCoroutine(AudioHandler.FadeOutAndStop(airBladderAudioSource, 0.5f));
                }
            }
            else
            {
                Plugin.Log?.LogWarning("Cannot deflate Seamoth air bladder: already deflated.");
            }
        }
    }
}