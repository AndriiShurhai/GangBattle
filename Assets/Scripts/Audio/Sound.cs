using UnityEngine;

/// <summary>
/// Data container for a single audio track or sound effect.
/// Assign in the AudioManager inspector lists.
/// </summary>
[System.Serializable]
public class Sound
{
    [Tooltip("Unique identifier used to reference this sound in code.")]
    public string name;

    [Tooltip("The audio clip to play.")]
    public AudioClip clip;

    [Range(0f, 1f)]
    [Tooltip("Base volume for this sound. Final volume is multiplied by the AudioManager volume settings.")]
    public float volume = 1f;

    [Range(0.1f, 3f)]
    [Tooltip("Playback pitch. 1 = normal speed.")]
    public float pitch = 1f;

    [Tooltip("Loop this track continuously (recommended for music, not SFX).")]
    public bool loop = false;

    [Tooltip("Play this track automatically when the AudioManager initialises. Only one music track should have this enabled.")]
    public bool playOnAwake = false;

    /// <summary>
    /// Runtime-assigned AudioSource. Do not set this in the inspector.
    /// </summary>
    [HideInInspector]
    public AudioSource source;
}