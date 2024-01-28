using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary> SFX sounds. </summary>
public enum SFXTag
{
    ButtonHover,
    ButtonClick,
}

/// <summary> SFX Loop sounds. </summary>
public enum SFXLoopTag
{

}

public class SFXPlayer : MonoBehaviour
{

    #region Serializable Classes

    [Serializable]
    public class SFXClip
    {
        [SerializeField, HideInInspector] private string name;

        [SerializeField]
        private AudioClip clip;
        [Range(0f, 1f)]
        [SerializeField]
        private float volume = 1;

        [Header("2D - 3D")]
        [SerializeField]
        [Range(0f, 1f)]
        private float spatialBlend = 0;
        [SerializeField]
        private float range = 5;
        [SerializeField]
        private AudioMixerGroup mixer;

        public void Play(AudioSource source)
        {
            if (clip != null)
            {
                source.outputAudioMixerGroup = mixer;
                source.PlayOneShot(clip, volume * MasterVolume);
            }
        }

        public void Play(Transform parent, Vector3 position)
        {
            if (clip != null)
            {
                GameObject obj = new GameObject(clip.name);
                obj.transform.SetParent(parent);

                AudioSource source = obj.AddComponent<AudioSource>();

                source.clip = clip;
                source.loop = false;
                source.volume = volume * MasterVolume;
                source.outputAudioMixerGroup = mixer;

                source.transform.position = position;
                source.spatialBlend = spatialBlend;
                source.rolloffMode = AudioRolloffMode.Custom;
                source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, new AnimationCurve(new Keyframe(0, 1, 0, -2), new Keyframe(1, 0)));
                source.minDistance = 0;
                source.maxDistance = range;

                source.Play();

                Timer.CallOnDelay(() =>
                {
                    Destroy(source.gameObject);
                }, clip.length, $"Playing Sfx: {clip.name}");
            }
        }

        public void SetName(string name)
        {
            string state = (clip == null) ? "No clip" : clip.name;
            string mixer = this.mixer != null ? $" ({this.mixer.name})" : "";

            this.name = $"{name}: {state}{mixer}";
        }
    }

    [Serializable]
    public class SFXLoopClip
    {
        [SerializeField, HideInInspector] private string name;

        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1;

        [Header("2D - 3D")]
        [SerializeField]
        [Range(0f, 1f)]
        private float spatialBlend = 0;
        [SerializeField]
        private float range = 5;

        public float transitionDuration = 1;
        public AudioMixerGroup mixer;

        [ReadOnly]
        public bool enabled;
        [ReadOnly]
        public bool isOnTransition;
        [ReadOnly]
        [Range(0f, 1f)]
        public float weight;

        public void SetName(string name)
        {
            string state = (clip == null) ? "No clip" : clip.name;
            string mixer = this.mixer != null ? $" ({this.mixer.name})" : "";

            this.name = $"{name}: {state}{mixer}";
        }

        public void SetupAudioSource(AudioSource source)
        {
            source.clip = clip;
            source.loop = true;
            source.playOnAwake = true;
            source.volume = enabled ? volume : 0;
            source.outputAudioMixerGroup = mixer;

            source.spatialBlend = spatialBlend;
            source.rolloffMode = AudioRolloffMode.Custom;
            source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, new AnimationCurve(new Keyframe(0, 1, 0, -2), new Keyframe(1, 0)));
            source.minDistance = 0;
            source.maxDistance = range;

            if (!source.isPlaying) source.Play();
        }
    }

    [Serializable]
    private class CacheSFX
    {
        public SFXTag tag;
        public float timer;

        public CacheSFX(SFXTag tag, float timer)
        {
            this.tag = tag;
            this.timer = timer;
        }
    }

    #endregion

    [Header("Components")]
    [ReadOnly, SerializeField]
    private Transform sfxLoopParent;
    [ReadOnly, SerializeField]
    private Transform sfxParent;

    [Header("Settings")]
    [SerializeField]
    private bool updateAudioSourcesInEditor = true;
    [SerializeField]
    [Range(0f, 1f)]
    private float masterVolume = 1;
    [SerializeField]
    private bool muteVolume;
    [SerializeField]
    private AnimationCurve transitionCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    [SerializeField]
    [Range(0, 0.2f)]
    private float doubleSoundsSeparation = 0.05f;

    [Header("Clips")]
    [SerializeField]
    private List<SFXClip> sfxClips = new List<SFXClip>();
    [SerializeField]
    private List<SFXLoopClip> sfxLoopClips = new List<SFXLoopClip>();

    [Header("Info")]
    [ReadOnly, SerializeField]
    private List<AudioSource> sfxLoopAudioSources = new List<AudioSource>();
    [SerializeField]
    private List<CacheSFX> cacheSFX = new List<CacheSFX>();

    private AudioSource defaultAudioSource;

    #region Properties

    private Transform SFXLoopParent
    {
        get
        {
            if (sfxLoopParent == null)
            {
                sfxLoopParent = new GameObject("LoopsParent").transform;
                sfxLoopParent.SetParent(transform);
            }

            return sfxLoopParent;
        }
    }

    private Transform SFXParent
    {
        get
        {
            if (sfxParent == null)
            {
                sfxParent = new GameObject("SFXParent").transform;
                sfxParent.SetParent(transform);
            }

            return sfxParent;
        }
    }

    private AudioSource DefaultAudioSource
    {
        get
        {
            if (defaultAudioSource == null)
            {
                defaultAudioSource = gameObject.AddComponent<AudioSource>();

                defaultAudioSource.playOnAwake = false;
                defaultAudioSource.loop = false;
                defaultAudioSource.spatialBlend = 0;
            }

            return defaultAudioSource;
        }
    }

    public static float MasterVolume { get => Instance.muteVolume ? 0 : Instance.masterVolume; set => Instance.masterVolume = Mathf.Clamp01(value); }

    #endregion

    #region Singleton

    private static SFXPlayer instance = null;

    public static SFXPlayer Instance => GambaFunctions.GetSingleton(ref instance);

    private void Awake()
    {
        GambaFunctions.OnSingletonInit(ref instance, this);
    }

    #endregion

    #region Start

    private void Start()
    {
        sfxClips.Resize(typeof(SFXTag));
        sfxLoopClips.Resize(typeof(SFXLoopTag));

        UpdateLoopsParent();
        ClearSFXParent();
    }

    private void ClearSFXParent()
    {
        for (int i = SFXParent.childCount - 1; i >= 0; i--)
        {
            Destroy(SFXParent.GetChild(i).gameObject);
        }
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Update

    private void Update()
    {
        SFXLoopUpdate();
        UpdateCacheSFX();
    }

    private void SFXLoopUpdate()
    {
        for (int i = 0; i < sfxLoopAudioSources.Count; i++)
        {
            SFXLoopClip sfx = sfxLoopClips[i];

            if (sfx.isOnTransition)
            {
                int dir = sfx.enabled ? 1 : -1;
                int targetWeight = sfx.enabled ? 1 : 0;

                if (sfx.transitionDuration > 0)
                {
                    sfx.weight += Time.deltaTime / sfx.transitionDuration * dir;
                    sfx.weight = Mathf.Clamp01(sfx.weight);
                }
                else
                {
                    sfx.weight = targetWeight;
                }

                sfxLoopAudioSources[i].volume = transitionCurve.Evaluate(sfx.weight) * sfx.volume * MasterVolume;

                if (sfx.weight == targetWeight)
                {
                    sfx.isOnTransition = false;
                }
            }
            else
            {
                sfxLoopAudioSources[i].volume = sfx.weight * sfx.volume * MasterVolume;
            }
        }
    }

    private void UpdateLoopsParent()
    {
        List<AudioSource> existingAudioSources = new List<AudioSource>(SFXLoopParent.GetComponentsInChildren<AudioSource>());

        if (existingAudioSources.Count > sfxLoopClips.Count) // Destroy excess of AudioSources
        {
            for (int i = existingAudioSources.Count - 1; i >= 0; i--)
            {
                if (i >= sfxLoopClips.Count)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(existingAudioSources[i].gameObject);
                    }
                    else
                    {
#if UNITY_EDITOR
                        GambaFunctions.DestroyInEditor(existingAudioSources[i].gameObject);
#endif
                    }
                    existingAudioSources.RemoveAt(i);
                }
                else
                {
                    break;
                }
            }
        }
        else // Add missing AudioSources
        {
            for (int i = 0; i < sfxLoopClips.Count; i++)
            {
                if (i >= existingAudioSources.Count)
                {
                    GameObject obj = new GameObject($"{i}: {Enum.GetName(typeof(SFXLoopTag), i)}");
                    obj.transform.SetParent(sfxLoopParent);

                    existingAudioSources.Add(obj.AddComponent<AudioSource>());
                }
            }
        }

        sfxLoopAudioSources = existingAudioSources;

        // Setup AudioSources
        for (int i = 0; i < sfxLoopAudioSources.Count; i++)
        {
            sfxLoopClips[i].SetupAudioSource(sfxLoopAudioSources[i]);
        }
    }

    private bool CheckLoopsSources()
    {
        List<AudioSource> existingAudioSources = new List<AudioSource>(SFXLoopParent.GetComponentsInChildren<AudioSource>());

        if (sfxLoopAudioSources.Count != sfxLoopClips.Count || existingAudioSources.Count != sfxLoopClips.Count)
        {
            return false;
        }

        for (int i = 0; i < sfxLoopAudioSources.Count; i++)
        {
            if (sfxLoopAudioSources[i] == null)
            {
                return false;
            }
        }

        return true;
    }

    private void UpdateCacheSFX()
    {
        for (int i = 0; i < cacheSFX.Count; i++)
        {
            CacheSFX cache = cacheSFX[i];

            Timer.ReduceCooldownUnscaled(ref cache.timer, () => cacheSFX.Remove(cache));
        }
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Static Methods

    /// <summary> Play SFX with PlayOneShot. </summary>
    public static void PlaySFX(SFXTag tag)
    {
        if (Instance.CheckCacheSFX(tag)) return;

        if (Instance.sfxClips != null && (int)tag < Instance.sfxClips.Count)
        {
            SFXClip clip = Instance.sfxClips[(int)tag];

            clip.Play(Instance.DefaultAudioSource);

            Instance.AddCacheSFX(tag);
        }
    }

    /// <summary> Play SFX with an AudioSource at a 3D position. </summary>
    public static void PlaySFX(SFXTag tag, Vector3 position)
    {
        if (Instance.sfxClips != null && (int)tag < Instance.sfxClips.Count)
        {
            SFXClip clip = Instance.sfxClips[(int)tag];

            clip.Play(Instance.SFXParent, position);
        }
    }

    /// <summary> Play a random SFX with PlayOneShot. </summary>
    public static void PlayRandomSFX(List<SFXTag> randomTags)
    {
        if (randomTags.Count == 0) return;

        SFXTag tag = randomTags[UnityEngine.Random.Range(0, randomTags.Count)];

        PlaySFX(tag);
    }

    /// <summary> Play a random SFX with PlayOneShot. </summary>
    public static void PlayRandomSFX(params SFXTag[] randomTags)
    {
        PlayRandomSFX(new List<SFXTag>(randomTags));
    }

    /// <summary> Play a random SFX with an AudioSource at a 3D position. </summary>
    public static void PlayRandomSFX(List<SFXTag> randomTags, Vector3 position)
    {
        if (randomTags.Count == 0) return;

        SFXTag tag = randomTags[UnityEngine.Random.Range(0, randomTags.Count)];

        PlaySFX(tag, position);
    }

    /// <summary> Play a random SFX with an AudioSource at a 3D position. </summary>
    public static void PlayRandomSFX(Vector3 position, params SFXTag[] randomTags)
    {
        PlayRandomSFX(new List<SFXTag>(randomTags), position);
    }

    /// <summary> Set SFXLoop on or off. </summary> <param name = "resetAudio"> Restart AudioSource's time. </param>
    public static void SetSFXLoop(SFXLoopTag tag, bool enabled, bool resetAudio = false)
    {
        if (Instance.sfxLoopClips != null && (int)tag < Instance.sfxLoopClips.Count)
        {
            SFXLoopClip clip = Instance.sfxLoopClips[(int)tag];

            clip.enabled = enabled;
            clip.isOnTransition = true;

            if (resetAudio)
            {
                AudioSource source = Instance.sfxLoopAudioSources[(int)tag];

                source.time = 0;
            }
        }
    }

    /// <summary> Set SFXLoop's 3D position. </summary>
    public static void SetSFXLoop(SFXLoopTag tag, Vector3 position)
    {
        if (Instance.sfxLoopAudioSources != null && (int)tag < Instance.sfxLoopAudioSources.Count)
        {
            AudioSource source = Instance.sfxLoopAudioSources[(int)tag];

            source.transform.position = position;
        }
    }

    /// <summary> Set SFXLoop's volume. </summary>
    public static void SetSFXLoop(SFXLoopTag tag, float volume)
    {
        if (Instance.sfxLoopClips != null && (int)tag < Instance.sfxLoopClips.Count)
        {
            SFXLoopClip clip = Instance.sfxLoopClips[(int)tag];

            clip.volume = volume;
        }
    }

    public static void SetMute(bool mute) => Instance.muteVolume = mute;

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Other

    private bool CheckCacheSFX(SFXTag tag) => cacheSFX.Exists(sfx => sfx.tag == tag);

    private void AddCacheSFX(SFXTag tag) => cacheSFX.Add(new CacheSFX(tag, doubleSoundsSeparation));

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Editor

#if UNITY_EDITOR

    private void OnValidate()
    {

        #region ListUpdate

        // SFX ----------------------------------------------------------

        sfxClips.Resize(typeof(SFXTag));

        for (int i = 0; i < sfxClips.Count; i++)
        {
            sfxClips[i].SetName(((SFXTag)i).ToString());
        }

        // Loops --------------------------------------------------------

        sfxLoopClips.Resize(typeof(SFXLoopTag));

        for (int i = 0; i < sfxLoopClips.Count; i++)
        {
            sfxLoopClips[i].SetName(((SFXLoopTag)i).ToString());
        }

        #endregion

        #region LoopAudioSources

        if (updateAudioSourcesInEditor)
        {
            if (!CheckLoopsSources())
            {
                UpdateLoopsParent();
            }
        }

        #endregion

    }

#endif

    #endregion

}