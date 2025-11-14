using UnityEngine;
using System.Collections.Generic;

namespace VRBoatCombat
{
    /// <summary>
    /// Centralized audio management system with 3D spatial audio and dynamic mixing.
    /// Handles music, sound effects, and ambient sounds.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        // Singleton instance
        public static AudioManager Instance { get; private set; }

        [System.Serializable]
        public class Sound
        {
            public string name;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume = 1f;
            [Range(0.1f, 3f)] public float pitch = 1f;
            public bool loop = false;
            public bool is3D = true;
            [Range(0f, 500f)] public float maxDistance = 100f;
            [HideInInspector] public AudioSource source;
        }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource ambientSource;
        [SerializeField] private int maxSFXSources = 20;

        [Header("Sound Library")]
        [SerializeField] private List<Sound> sounds = new List<Sound>();

        [Header("Volume Settings")]
        [SerializeField] [Range(0f, 1f)] private float masterVolume = 1f;
        [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.7f;
        [SerializeField] [Range(0f, 1f)] private float sfxVolume = 1f;
        [SerializeField] [Range(0f, 1f)] private float ambientVolume = 0.5f;

        [Header("Spatial Audio")]
        [SerializeField] private bool enableOcclusion = true;
        [SerializeField] private LayerMask occlusionLayers;
        [SerializeField] private float occlusionVolumeReduction = 0.5f;

        // SFX source pool
        private Queue<AudioSource> availableSFXSources;
        private List<AudioSource> allSFXSources;
        private Dictionary<string, Sound> soundDictionary;
        private Transform listenerTransform;

        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            Initialize();
        }

        private void Initialize()
        {
            // Create music source if not assigned
            if (musicSource == null)
            {
                GameObject musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                musicSource = musicObj.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
                musicSource.spatialBlend = 0f; // 2D
            }

            // Create ambient source if not assigned
            if (ambientSource == null)
            {
                GameObject ambientObj = new GameObject("AmbientSource");
                ambientObj.transform.SetParent(transform);
                ambientSource = ambientObj.AddComponent<AudioSource>();
                ambientSource.loop = true;
                ambientSource.playOnAwake = false;
                ambientSource.spatialBlend = 0f; // 2D
            }

            // Create SFX source pool
            availableSFXSources = new Queue<AudioSource>();
            allSFXSources = new List<AudioSource>();

            for (int i = 0; i < maxSFXSources; i++)
            {
                CreateSFXSource();
            }

            // Build sound dictionary
            soundDictionary = new Dictionary<string, Sound>();
            foreach (Sound sound in sounds)
            {
                if (!soundDictionary.ContainsKey(sound.name))
                {
                    soundDictionary[sound.name] = sound;
                }
                else
                {
                    Debug.LogWarning($"[AudioManager] Duplicate sound name: {sound.name}");
                }
            }

            // Get audio listener
            AudioListener listener = FindObjectOfType<AudioListener>();
            if (listener != null)
            {
                listenerTransform = listener.transform;
            }

            Debug.Log($"[AudioManager] Initialized with {sounds.Count} sounds and {maxSFXSources} SFX sources");
        }

        private void Update()
        {
            // Update occlusion for active 3D sounds
            if (enableOcclusion)
            {
                UpdateOcclusion();
            }

            // Return finished one-shot sources to pool
            ReturnFinishedSources();
        }

        private AudioSource CreateSFXSource()
        {
            GameObject sfxObj = new GameObject($"SFXSource_{allSFXSources.Count}");
            sfxObj.transform.SetParent(transform);
            AudioSource source = sfxObj.AddComponent<AudioSource>();
            source.playOnAwake = false;

            allSFXSources.Add(source);
            availableSFXSources.Enqueue(source);

            return source;
        }

        /// <summary>
        /// Play a sound effect
        /// </summary>
        public void PlaySound(string soundName, Vector3 position = default)
        {
            if (!soundDictionary.ContainsKey(soundName))
            {
                Debug.LogWarning($"[AudioManager] Sound '{soundName}' not found!");
                return;
            }

            Sound sound = soundDictionary[soundName];

            // Get available source
            AudioSource source = GetAvailableSFXSource();
            if (source == null)
            {
                Debug.LogWarning("[AudioManager] No available SFX sources!");
                return;
            }

            // Configure source
            ConfigureAudioSource(source, sound);

            // Set position for 3D sounds
            if (sound.is3D && position != default)
            {
                source.transform.position = position;
            }

            // Play sound
            source.Play();
        }

        /// <summary>
        /// Play a sound with variable pitch
        /// </summary>
        public void PlaySoundRandomPitch(string soundName, float minPitch, float maxPitch, Vector3 position = default)
        {
            if (!soundDictionary.ContainsKey(soundName))
            {
                Debug.LogWarning($"[AudioManager] Sound '{soundName}' not found!");
                return;
            }

            Sound sound = soundDictionary[soundName];
            AudioSource source = GetAvailableSFXSource();
            if (source == null) return;

            ConfigureAudioSource(source, sound);
            source.pitch = Random.Range(minPitch, maxPitch);

            if (sound.is3D && position != default)
            {
                source.transform.position = position;
            }

            source.Play();
        }

        /// <summary>
        /// Play background music
        /// </summary>
        public void PlayMusic(string soundName, bool fadeIn = true, float fadeDuration = 2f)
        {
            if (!soundDictionary.ContainsKey(soundName))
            {
                Debug.LogWarning($"[AudioManager] Music '{soundName}' not found!");
                return;
            }

            Sound sound = soundDictionary[soundName];

            if (fadeIn)
            {
                StartCoroutine(FadeMusicCoroutine(sound, fadeDuration));
            }
            else
            {
                musicSource.clip = sound.clip;
                musicSource.volume = sound.volume * musicVolume * masterVolume;
                musicSource.pitch = sound.pitch;
                musicSource.loop = true;
                musicSource.Play();
            }
        }

        /// <summary>
        /// Stop background music
        /// </summary>
        public void StopMusic(bool fadeOut = true, float fadeDuration = 2f)
        {
            if (fadeOut)
            {
                StartCoroutine(FadeOutMusicCoroutine(fadeDuration));
            }
            else
            {
                musicSource.Stop();
            }
        }

        /// <summary>
        /// Play ambient sound
        /// </summary>
        public void PlayAmbient(string soundName)
        {
            if (!soundDictionary.ContainsKey(soundName))
            {
                Debug.LogWarning($"[AudioManager] Ambient sound '{soundName}' not found!");
                return;
            }

            Sound sound = soundDictionary[soundName];

            ambientSource.clip = sound.clip;
            ambientSource.volume = sound.volume * ambientVolume * masterVolume;
            ambientSource.pitch = sound.pitch;
            ambientSource.loop = true;
            ambientSource.Play();
        }

        private void ConfigureAudioSource(AudioSource source, Sound sound)
        {
            source.clip = sound.clip;
            source.volume = sound.volume * sfxVolume * masterVolume;
            source.pitch = sound.pitch;
            source.loop = sound.loop;
            source.spatialBlend = sound.is3D ? 1f : 0f;

            if (sound.is3D)
            {
                source.maxDistance = sound.maxDistance;
                source.rolloffMode = AudioRolloffMode.Linear;
                source.dopplerLevel = 0.5f;
            }
        }

        private AudioSource GetAvailableSFXSource()
        {
            if (availableSFXSources.Count > 0)
            {
                return availableSFXSources.Dequeue();
            }

            // All sources in use - expand pool if possible
            if (allSFXSources.Count < maxSFXSources * 2)
            {
                return CreateSFXSource();
            }

            return null;
        }

        private void ReturnFinishedSources()
        {
            foreach (AudioSource source in allSFXSources)
            {
                if (!source.isPlaying && !availableSFXSources.Contains(source))
                {
                    availableSFXSources.Enqueue(source);
                }
            }
        }

        private void UpdateOcclusion()
        {
            if (listenerTransform == null) return;

            foreach (AudioSource source in allSFXSources)
            {
                if (!source.isPlaying || source.spatialBlend < 0.5f) continue;

                // Raycast from listener to source
                Vector3 direction = source.transform.position - listenerTransform.position;
                float distance = direction.magnitude;

                if (Physics.Raycast(listenerTransform.position, direction.normalized, distance, occlusionLayers))
                {
                    // Occluded - reduce volume
                    source.volume *= (1f - occlusionVolumeReduction);
                }
            }
        }

        private System.Collections.IEnumerator FadeMusicCoroutine(Sound sound, float duration)
        {
            // Fade out current music
            float startVolume = musicSource.volume;
            float elapsed = 0f;

            while (elapsed < duration / 2f)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (duration / 2f));
                yield return null;
            }

            // Switch music
            musicSource.clip = sound.clip;
            musicSource.pitch = sound.pitch;
            musicSource.loop = true;
            musicSource.Play();

            // Fade in new music
            float targetVolume = sound.volume * musicVolume * masterVolume;
            elapsed = 0f;

            while (elapsed < duration / 2f)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / (duration / 2f));
                yield return null;
            }

            musicSource.volume = targetVolume;
        }

        private System.Collections.IEnumerator FadeOutMusicCoroutine(float duration)
        {
            float startVolume = musicSource.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }

            musicSource.Stop();
            musicSource.volume = startVolume;
        }

        // Volume control methods
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateAllVolumes();
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            UpdateAllVolumes();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            UpdateAllVolumes();
        }

        public void SetAmbientVolume(float volume)
        {
            ambientVolume = Mathf.Clamp01(volume);
            UpdateAllVolumes();
        }

        private void UpdateAllVolumes()
        {
            if (musicSource != null && musicSource.clip != null)
            {
                string soundName = FindSoundNameByClip(musicSource.clip);
                if (!string.IsNullOrEmpty(soundName))
                {
                    Sound sound = soundDictionary[soundName];
                    musicSource.volume = sound.volume * musicVolume * masterVolume;
                }
            }

            if (ambientSource != null && ambientSource.clip != null)
            {
                string soundName = FindSoundNameByClip(ambientSource.clip);
                if (!string.IsNullOrEmpty(soundName))
                {
                    Sound sound = soundDictionary[soundName];
                    ambientSource.volume = sound.volume * ambientVolume * masterVolume;
                }
            }
        }

        private string FindSoundNameByClip(AudioClip clip)
        {
            foreach (var kvp in soundDictionary)
            {
                if (kvp.Value.clip == clip)
                {
                    return kvp.Key;
                }
            }
            return null;
        }

        public float GetMasterVolume() => masterVolume;
        public float GetMusicVolume() => musicVolume;
        public float GetSFXVolume() => sfxVolume;
        public float GetAmbientVolume() => ambientVolume;

#if UNITY_EDITOR
        private void OnValidate()
        {
            maxSFXSources = Mathf.Max(1, maxSFXSources);
        }
#endif
    }
}
