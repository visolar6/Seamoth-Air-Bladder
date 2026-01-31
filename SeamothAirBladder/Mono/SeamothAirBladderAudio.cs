using SeamothAirBladder.Utilities;
using System.Collections;
using UnityEngine;

namespace SeamothAirBladder.Mono
{
    /// <summary>
    /// Manages audio playback for the Seamoth air bladder module.
    /// </summary>
    public class SeamothAirBladderAudio : MonoBehaviour
    {
        private AudioSource? inflateAudioSource;
        private AudioClip? inflateClip;
        private Coroutine? fadeOutCoroutine;

        private AudioSource? rechargeAudioSource;
        private AudioClip? rechargeClip;

        public void Initialize()
        {
            // Setup AudioSource for inflate sound
            inflateAudioSource = gameObject.GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
            inflateAudioSource.playOnAwake = false;
            inflateAudioSource.spatialBlend = 1f; // 3D sound
            inflateAudioSource.loop = false;
            inflateAudioSource.volume = 0.3f;
            inflateAudioSource.pitch = 1.0f;

            // Load the inflate AudioClip
            string clipPath = System.IO.Path.Combine("Assets", "Audio", "sfx_tool_airbladder_use_01.wav");
            inflateClip = ResourceHandler.LoadAudioClipFromFile(clipPath);
            if (inflateClip == null)
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
        }

        /// <summary>
        /// Plays the air bladder inflate sound.
        /// </summary>
        public void PlayInflateSound()
        {
            if (inflateAudioSource != null && inflateClip != null)
            {
                inflateAudioSource.Stop();
                inflateAudioSource.clip = inflateClip;
                inflateAudioSource.volume = 0.3f;
                inflateAudioSource.Play();
            }
            else
            {
                Plugin.Log?.LogWarning("Cannot play air bladder sound: AudioSource or AudioClip is null.");
            }
        }

        /// <summary>
        /// Stops the inflate sound with a fade out effect.
        /// </summary>
        public void StopInflateSound()
        {
            if (inflateAudioSource != null && inflateAudioSource.isPlaying)
            {
                if (fadeOutCoroutine != null)
                    StopCoroutine(fadeOutCoroutine);
                fadeOutCoroutine = StartCoroutine(AudioHandler.FadeOutAndStop(inflateAudioSource, 0.5f));
            }
        }

        /// <summary>
        /// Plays the air recharge sound.
        /// </summary>
        public void PlayRechargeSound()
        {
            if (rechargeAudioSource != null && rechargeClip != null)
            {
                rechargeAudioSource.clip = rechargeClip;
                rechargeAudioSource.Play();
            }
        }
    }
}
