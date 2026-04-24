using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Drives all music transitions based on game state.
/// Place on any persistent GameObject in the battle scene.
///
/// Battle music is per-biome: each biome entry holds a SoundGroup so you can have
/// multiple thematically-matching battle tracks that rotate randomly within that biome.
/// A fallback group plays when no biome entry matches the current level's biome.
/// </summary>
public class GameStateAudio : MonoBehaviour
{
    [Header("Per-Biome Battle Music")]
    [Tooltip("One entry per biome. Each entry holds 1–N battle tracks that match that biome's theme. " +
             "A random one is picked each time a battle in that biome starts.")]
    [SerializeField] private List<BiomeBattleMusicEntry> biomeBattleMusic = new List<BiomeBattleMusicEntry>();

    [Tooltip("Played when no biome entry matches the current level. Add multiple tracks for variety.")]
    [SerializeField] private SoundGroup musicBattleFallback;

    [Header("Victory / Defeat Music")]
    [Tooltip("Usually one track, but you can add alternates.")]
    [SerializeField] private SoundGroup musicVictory;
    [SerializeField] private SoundGroup musicDefeat;

    [Header("Stingers (one-shot SFX on state change)")]
    [SerializeField] private SoundGroup sfxVictoryStinger;
    [SerializeField] private SoundGroup sfxDefeatStinger;

    [Tooltip("Plays at the start of each player turn. Add variants to avoid repetition.")]
    [SerializeField] private SoundGroup sfxTurnEnd;

    // ─────────────────────────────────────────────
    //  Lifecycle
    // ─────────────────────────────────────────────

    private void OnEnable()
    {
        TurnManager.OnUnitsInitialized += HandleBattleStarted;
    }

    private void Start()
    {

        TurnManager.OnLevelCompleted += HandleLevelCompleted;
        TurnManager.OnLevelFailed += HandleLevelFailed;
    }

    private void OnDisable()
    {
        TurnManager.OnUnitsInitialized -= HandleBattleStarted;

        TurnManager.OnLevelCompleted -= HandleLevelCompleted;
        TurnManager.OnLevelFailed -= HandleLevelFailed;
    }

    // ─────────────────────────────────────────────
    //  Handlers
    // ─────────────────────────────────────────────

    private void HandleBattleStarted()
    {
        if (AudioManager.Instance == null) return;

        string currentBiome = GetCurrentBiomeName();
        SoundGroup battleGroup = GetBattleMusicForBiome(currentBiome);
        PlayRandomMusic(battleGroup);
    }

    private void HandleLevelCompleted()
    {
        if (AudioManager.Instance == null) return;
        else Debug.Log("Audio Manager instance found.");
        Debug.Log("Level completed! Playing victory music and stinger.");
        PlayRandomSFX(sfxVictoryStinger);
        PlayRandomMusic(musicVictory);
    }

    private void HandleLevelFailed()
    {
        if (AudioManager.Instance == null) return;
        PlayRandomSFX(sfxDefeatStinger);
        PlayRandomMusic(musicDefeat);
    }

    private void HandlePlayerTurnStarted()
    {
        if (AudioManager.Instance == null) return;
        PlayRandomSFX(sfxTurnEnd);
    }

    // ─────────────────────────────────────────────
    //  Biome Resolution
    // ─────────────────────────────────────────────

    /// <summary>
    /// Returns the biome name for the currently loaded level.
    /// Replace the body with however your project exposes the current biome
    /// (e.g. TurnManager.Instance.CurrentLevel.biomeName, LevelManager.Instance.CurrentBiomeName, etc.)
    /// </summary>
    private string GetCurrentBiomeName()
    {
        return MapManager.Instance?.ActiveBiome?.biomName;
    }

    private SoundGroup GetBattleMusicForBiome(string biomeName)
    {
        if (!string.IsNullOrEmpty(biomeName))
        {
            foreach (BiomeBattleMusicEntry entry in biomeBattleMusic)
            {
                if (entry.biomeName == biomeName)
                    return entry.battleMusicGroup;
            }
        }

        return musicBattleFallback;
    }

    // ─────────────────────────────────────────────
    //  Helpers
    // ─────────────────────────────────────────────

    private void PlayRandomMusic(SoundGroup group)
    {
        if (group == null || group.IsEmpty) return;
        AudioManager.Instance.CrossfadeMusic(group.PickRandom());
    }

    private void PlayRandomSFX(SoundGroup group)
    {
        if (group == null || group.IsEmpty) return;
        Debug.Log($"Playing SFX: {group.First()}");
        AudioManager.Instance.PlaySFX(group.PickRandom());
    }
}

/// <summary>
/// Maps a biome name to a pool of thematically-matching battle music tracks.
/// Add one track for a fixed theme, several to rotate randomly within that biome.
/// </summary>
[Serializable]
public class BiomeBattleMusicEntry
{
    [Tooltip("Must match your biome identifier exactly (case-sensitive).")]
    public string biomeName;

    [Tooltip("Battle tracks that fit this biome's theme. " +
             "A random one is picked each battle — never the same one twice in a row.")]
    public SoundGroup battleMusicGroup;
}