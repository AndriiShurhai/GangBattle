using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A named list of AudioManager sound references (by name) with a non-repeat random picker.
///
/// Use in the inspector wherever a single sound string used to live.
/// • One entry  → always plays that entry (behaves exactly like the old string field).
/// • Many entries → picks at random, never repeating the same index twice in a row.
///
/// Usage:
///   [SerializeField] private SoundGroup sfxHitSounds;
///   ...
///   AudioManager.Instance.PlaySFX(sfxHitSounds.PickRandom());
/// </summary>
[System.Serializable]
public class SoundGroup
{
    [Tooltip("Names of Sound entries registered in AudioManager. Add one for a fixed sound, several for random variety.")]
    [SerializeField] private List<string> names = new List<string>();

    private int lastPickedIndex = -1;

    /// <summary>True when the group has no entries — callers should skip playback.</summary>
    public bool IsEmpty => names == null || names.Count == 0;

    /// <summary>
    /// Returns one sound name chosen at random.
    /// With more than one entry the same index is never picked twice in a row.
    /// Returns <see cref="string.Empty"/> when the group is empty.
    /// </summary>
    public string PickRandom()
    {
        if (IsEmpty) return string.Empty;
        if (names.Count == 1) return names[0];

        int index;
        int attempts = 0;
        do
        {
            index = Random.Range(0, names.Count);
            attempts++;
        }
        while (index == lastPickedIndex && attempts < 10); // cap prevents infinite loop on tiny lists

        lastPickedIndex = index;
        return names[index];
    }

    /// <summary>Convenience: returns the first entry, or empty if none. Useful for ambient/looping sounds that should never vary.</summary>
    public string First() => IsEmpty ? string.Empty : names[0];
}