using System;
using System.Collections;
using UnityEngine;

namespace SeamothAirBladder.Utilities
{
    public static class AudioHandler
    {
        public static AudioClip WavToAudioClip(byte[] wavFileBytes, int offsetSamples = 0, string name = "wav")
        {
            int channels, sampleRate;
            float[] data = ConvertWavToFloatArray(wavFileBytes, out channels, out sampleRate);
            var clip = AudioClip.Create(name, data.Length / channels, channels, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        // Adapted from open-source WavUtility implementations
        private static float[] ConvertWavToFloatArray(byte[] wav, out int channels, out int sampleRate)
        {
            // WAV header parsing
            if (System.Text.Encoding.ASCII.GetString(wav, 0, 4) != "RIFF")
                throw new Exception("Invalid WAV file: missing RIFF");
            if (System.Text.Encoding.ASCII.GetString(wav, 8, 4) != "WAVE")
                throw new Exception("Invalid WAV file: missing WAVE");
            int pos = 12;
            channels = 1;
            sampleRate = 44100;
            int bitsPerSample = 16;
            int dataChunkPos = -1;
            int dataChunkSize = 0;
            while (pos + 8 <= wav.Length)
            {
                string chunkId = System.Text.Encoding.ASCII.GetString(wav, pos, 4);
                int chunkSize = BitConverter.ToInt32(wav, pos + 4);
                if (chunkId == "fmt ")
                {
                    int audioFormat = BitConverter.ToInt16(wav, pos + 8);
                    channels = BitConverter.ToInt16(wav, pos + 10);
                    sampleRate = BitConverter.ToInt32(wav, pos + 12);
                    bitsPerSample = BitConverter.ToInt16(wav, pos + 22);
                    if (audioFormat != 1)
                        throw new Exception("Only PCM WAV files are supported");
                }
                else if (chunkId == "data")
                {
                    dataChunkPos = pos + 8;
                    dataChunkSize = chunkSize;
                    break;
                }
                pos += 8 + chunkSize;
            }
            if (dataChunkPos < 0)
                throw new Exception("WAV file missing data chunk");
            int sampleCount = dataChunkSize / (bitsPerSample / 8);
            float[] data = new float[sampleCount];
            if (bitsPerSample == 16)
            {
                for (int i = 0; i < sampleCount; i++)
                {
                    short sample = BitConverter.ToInt16(wav, dataChunkPos + i * 2);
                    data[i] = sample / 32768f;
                }
            }
            else if (bitsPerSample == 8)
            {
                for (int i = 0; i < sampleCount; i++)
                {
                    data[i] = (wav[dataChunkPos + i] - 128) / 128f;
                }
            }
            else
            {
                throw new Exception($"Unsupported WAV bit depth: {bitsPerSample}");
            }
            return data;
        }

        public static IEnumerator FadeOutAndStop(AudioSource source, float duration)
        {
            float startVolume = source.volume;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, t / duration);
                yield return null;
            }
            source.Stop();
            source.volume = startVolume;
        }
    }
}
