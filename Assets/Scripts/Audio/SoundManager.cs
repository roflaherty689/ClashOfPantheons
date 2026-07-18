using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class SoundManager : MonoBehaviour
{
    [Serializable]
    public struct CueDefinition
    {
        public SoundCue cue;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume;
        [Min(0f)] public float cooldown;
        [Range(0f, 1f)] public float spatialBlend;
        [Range(0f, 0.25f)] public float pitchVariation;
    }

    private const string ResourcePath = "Audio/SoundManager";
    private const int VoiceCount = 16;
    private static SoundManager instance;

    [SerializeField] private CueDefinition[] cues;
    [SerializeField, Range(0f, 1f)] private float masterVolume = 0.8f;

    private readonly Dictionary<SoundCue, CueDefinition> definitions = new();
    private readonly Dictionary<SoundCue, float> nextAllowedTimes = new();
    private readonly HashSet<Button> boundButtons = new();
    private AudioSource[] voices;
    private int nextVoice;
    private float nextButtonScanTime;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null) return;
        SoundManager prefab = Resources.Load<SoundManager>(ResourcePath);
        if (prefab == null)
        {
            Debug.LogWarning("SoundManager resource is missing. Run Tools > Clash of Pantheons > Audio > Build Initial SFX Setup.");
            return;
        }

        instance = Instantiate(prefab);
        instance.name = "SoundManager";
        DontDestroyOnLoad(instance.gameObject);
    }

    public static void Play(SoundCue cue)
    {
        instance?.PlayInternal(cue, null);
    }

    public static void PlayAt(SoundCue cue, Vector3 position)
    {
        instance?.PlayInternal(cue, position);
    }

    public static void SuppressGenericClick(Button button)
    {
        if (button != null && button.GetComponent<SuppressGenericClickSound>() == null)
        {
            button.gameObject.AddComponent<SuppressGenericClickSound>();
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        definitions.Clear();
        if (cues != null)
        {
            foreach (CueDefinition definition in cues)
            {
                if (definition.clip != null) definitions[definition.cue] = definition;
            }
        }

        voices = new AudioSource[VoiceCount];
        for (int i = 0; i < voices.Length; i++)
        {
            AudioSource voice = gameObject.AddComponent<AudioSource>();
            voice.playOnAwake = false;
            voice.loop = false;
            voices[i] = voice;
        }

        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void Start()
    {
        BindSceneButtons();
    }

    private void Update()
    {
        if (Time.unscaledTime < nextButtonScanTime) return;
        nextButtonScanTime = Time.unscaledTime + 0.5f;
        BindSceneButtons();
    }

    private void OnDestroy()
    {
        if (instance != this) return;
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        instance = null;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        boundButtons.Clear();
        BindSceneButtons();
    }

    private void BindSceneButtons()
    {
        foreach (Button button in FindObjectsByType<Button>(FindObjectsInactive.Include))
        {
            if (button == null || !boundButtons.Add(button)) continue;
            button.onClick.AddListener(() =>
            {
                if (button.GetComponent<SuppressGenericClickSound>() == null) Play(SoundCue.UiClick);
            });
        }
    }

    private void PlayInternal(SoundCue cue, Vector3? position)
    {
        if (!definitions.TryGetValue(cue, out CueDefinition definition)) return;
        if (nextAllowedTimes.TryGetValue(cue, out float nextAllowed) && Time.unscaledTime < nextAllowed) return;

        nextAllowedTimes[cue] = Time.unscaledTime + definition.cooldown;
        AudioSource voice = GetVoice();
        voice.transform.position = position ?? Vector3.zero;
        voice.spatialBlend = position.HasValue ? definition.spatialBlend : 0f;
        voice.volume = Mathf.Clamp01(definition.volume * masterVolume);
        voice.pitch = 1f + UnityEngine.Random.Range(-definition.pitchVariation, definition.pitchVariation);
        voice.PlayOneShot(definition.clip);
    }

    private AudioSource GetVoice()
    {
        for (int i = 0; i < voices.Length; i++)
        {
            int index = (nextVoice + i) % voices.Length;
            if (voices[index].isPlaying) continue;
            nextVoice = (index + 1) % voices.Length;
            return voices[index];
        }

        AudioSource fallback = voices[nextVoice];
        nextVoice = (nextVoice + 1) % voices.Length;
        return fallback;
    }
}

public sealed class SuppressGenericClickSound : MonoBehaviour
{
}
