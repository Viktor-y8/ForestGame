using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundLibrary", menuName = "Scriptable Objects/SoundLibrary")]
public class SoundLibrary : ScriptableObject
{
    [Serializable]
    public class NamedSound
    {
        public string name;
        public AudioClip clip;
    }

    public NamedSound[] sounds;

    public AudioClip GetClip(string soundName)
    {
        foreach (NamedSound s in sounds)
        {
            if (s.name == soundName) return s.clip;
        }

        Debug.LogWarning($"Sound '{soundName}' not found in library");
        return null;
    }
}