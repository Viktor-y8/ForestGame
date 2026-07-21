using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource; // for simple one-shots
    [SerializeField] private int sfxPoolSize = 8;    // for overlapping sfx

    [Header("Volume")]
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 0.5f;

    [Header("Clips")]
    [SerializeField] private SoundLibrary library;

    private List<AudioSource> sfxPool = new List<AudioSource>();
    private Coroutine fadeCoroutine;

    [Header("Audio")]
    public AudioClip menuMusic;

    private Dictionary<object, AudioSource> loopingSources = new Dictionary<object, AudioSource>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        PlayMusic(menuMusic);

        // Build a small pool of sources so multiple sfx can play at once
        for (int i = 0; i < sfxPoolSize; i++)
        {
            GameObject obj = new GameObject("SFXSource_" + i);
            obj.transform.parent = transform;
            AudioSource src = obj.AddComponent<AudioSource>();
            src.playOnAwake = false;
            sfxPool.Add(src);
        }

    }

    // --- MUSIC ---

    public void PlayMusic(AudioClip clip, bool loop = true, float fadeTime = 0.5f)
    {
        if (clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(CrossfadeMusic(clip, loop, fadeTime));
    }

    private IEnumerator CrossfadeMusic(AudioClip newClip, bool loop, float fadeTime)
    {
        float startVolume = musicSource.volume;

        // Fade out current
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeTime);
            yield return null;
        }

        musicSource.clip = newClip;
        musicSource.loop = loop;
        musicSource.Play();

        // Fade in new
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, musicVolume, t / fadeTime);
            yield return null;
        }

        musicSource.volume = musicVolume;
    }

    public void StopMusic(float fadeTime = 0.5f)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeOutMusic(fadeTime));
    }

    private IEnumerator FadeOutMusic(float fadeTime)
    {
        float startVolume = musicSource.volume;
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeTime);
            yield return null;
        }
        musicSource.Stop();
        musicSource.volume = musicVolume;
    }

    // --- SFX ---

    public void PlaySFX(AudioClip clip, float volumeScale = 1f, float pitchVariance = 0.2f)
    {
        if (clip == null) return;

        AudioSource src = GetAvailableSFXSource();
        src.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
        src.PlayOneShot(clip, sfxVolume * volumeScale);
    }

    // Convenience overload using named sounds from the library
    public void PlaySFX(string soundName, float volumeScale = 1f, float pitchVariance = 0.2f)
    {
        AudioClip clip = library.GetClip(soundName);
        PlaySFX(clip, volumeScale, pitchVariance);
    }

    private AudioSource GetAvailableSFXSource()
    {
        foreach (AudioSource src in sfxPool)
        {
            if (!src.isPlaying) return src;
        }

        // All busy — just reuse the first one (rare edge case)
        return sfxPool[0];
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = value;
        musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = value;
    }


    private const int maxSimultaneousLoops = 4; // hard cap on audible loop sources

    public void PlayLoopingSFX(object owner, AudioClip clip, Vector3 worldPosition, bool spatial = true, float volumeScale = 1f)
    {
        if (clip == null) return;
        if (loopingSources.ContainsKey(owner)) return;

        // If too many are already playing, reduce this new one's volume to compensate
        float crowdingFactor = Mathf.Clamp01(1f - (loopingSources.Count / (float)maxSimultaneousLoops));
        float adjustedVolume = sfxVolume * volumeScale * Mathf.Max(crowdingFactor, 0.3f); // never fully silent

        GameObject obj = new GameObject("LoopingSFX_" + owner.GetHashCode());
        obj.transform.parent = transform;
        obj.transform.position = worldPosition;

        AudioSource src = obj.AddComponent<AudioSource>();
        src.clip = clip;
        src.loop = true;
        src.volume = adjustedVolume;
        if (spatial) src.spatialBlend = 1f;
        else src.spatialBlend = 0f;
        src.minDistance = 6f;
        src.maxDistance = 32f;
        src.rolloffMode = AudioRolloffMode.Linear;

        src.Play();
        loopingSources[owner] = src;
    }

    public void StopLoopingSFX(object owner)
    {
        if (loopingSources.TryGetValue(owner, out AudioSource src))
        {
            src.Stop();
            Destroy(src.gameObject);
            loopingSources.Remove(owner);
        }
    }

    public bool IsLoopingSFXPlaying(object owner)
    {
        return loopingSources.ContainsKey(owner);
    }

    public void StopAllLoopingSFX()
    {
        foreach (var kvp in loopingSources)
        {
            if (kvp.Value != null)
            {
                kvp.Value.Stop();
                Destroy(kvp.Value.gameObject);
            }
        }
        loopingSources.Clear();
    }
}