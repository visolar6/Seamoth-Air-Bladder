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

        private AudioSource inflateAudioSource = null!;
        private AudioClip inflateClip = null!;
        private Coroutine? fadeOutCoroutine;

        private AudioSource rechargeAudioSource = null!;
        private AudioClip rechargeClip = null!;

        private void Awake()
        {
            inflateAudioSource = SetupAudioSource(gameObject, 0.3f, false);
            string clipPath = System.IO.Path.Combine("Assets", "Audio", "sfx_tool_airbladder_use_01.wav");
            inflateClip = ResourceHandler.LoadAudioClipFromFile(clipPath)!;
            if (inflateClip == null)
                Plugin.Log?.LogError($"[{nameof(SeamothAirBladderAudio)}] Failed to load air bladder AudioClip from {clipPath}");

            rechargeAudioSource = SetupAudioSource(gameObject, 0.15f, false);
            string rechargeClipPath = System.IO.Path.Combine("Assets", "Audio", "sfx_tool_airbladder_equip_fillair_01.wav");
            rechargeClip = ResourceHandler.LoadAudioClipFromFile(rechargeClipPath)!;
            if (rechargeClip == null)
                Plugin.Log?.LogError($"[{nameof(SeamothAirBladderAudio)}] Failed to load recharge AudioClip from {rechargeClipPath}");
        }

        private static AudioSource SetupAudioSource(GameObject go, float volume, bool loop)
        {
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 1f;
            source.loop = loop;
            source.volume = volume;
            source.pitch = 1.0f;
            return source;
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
                Plugin.Log?.LogWarning($"[{nameof(SeamothAirBladderAudio)}] Cannot play air bladder sound: AudioSource or AudioClip is null.");
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
                rechargeAudioSource.PlayOneShot(rechargeClip);
            }
        }
    }
}
