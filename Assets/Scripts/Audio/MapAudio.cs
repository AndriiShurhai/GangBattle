using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Drives all audio on the world map scene.
///
/// Responsibilities:
///   • Crossfades to the default map theme when the map scene loads.
///   • Crossfades to a per-biome theme (or random theme from a group) when zooming in.
///   • Returns to the default map theme when zooming back out.
///   • Manages a looping ambient SFX layer (wind, crowd, etc.).
///   • Plays stinger SFX on biome zoom in/out — picked randomly from a group.
///
/// Setup:
///   1. Drop onto the same persistent GameObject as AudioManager.
///   2. Set mapSceneName to a partial match of your map scene name (e.g. "Map").
///   3. Add one BiomeMusicEntry per biome — biomeName must match BiomeController.biomName exactly.
///   4. Each BiomeMusicEntry holds a SoundGroup: add one track for fixed music,
///      several tracks to rotate randomly through the biome's theme variants.
///   5. Register all referenced music tracks and SFX in AudioManager's inspector lists.
/// </summary>
public class MapAudio : MonoBehaviour
{
    [Header("Scene")]
    [Tooltip("Partial name match — 'Map' matches 'WorldMap', 'OverworldMap', etc.")]
    [SerializeField] private string mapSceneName = "Map";

    [Header("Default Map Music")]
    [Tooltip("Plays when the map scene loads and when the player zooms back to world view. Add variants to randomise.")]
    [SerializeField] private SoundGroup musicDefaultMap;

    [Header("Per-Biome Music")]
    [Tooltip("Map each BiomeController.biomName to one or more music tracks. Multiple tracks → random pick on zoom-in.")]
    [SerializeField] private List<BiomeMusicEntry> biomeMusic = new List<BiomeMusicEntry>();

    [Header("Ambient SFX")]
    [Tooltip("Looping ambient sound for the map (e.g. wind, distant crowd). Uses the first entry only. Leave empty to disable.")]
    [SerializeField] private SoundGroup sfxAmbientLoop;

    [Header("Biome Transition SFX")]
    [Tooltip("One-shot stinger when the camera zooms into a biome. Add variants for variety.")]
    [SerializeField] private SoundGroup sfxBiomeZoomIn;

    [Tooltip("One-shot sound when the player zooms back out to world view. Add variants for variety.")]
    [SerializeField] private SoundGroup sfxBiomeZoomOut;

    private bool isOnMapScene = false;

    // ─────────────────────────────────────────────
    //  Lifecycle
    // ─────────────────────────────────────────────

    private void OnEnable()
    {
        SceneLoader.OnSceneLoadCompleted += HandleSceneLoadCompleted;
        SceneLoader.OnSceneLoadStarted += HandleSceneLoadStarted;
        MapManager.OnBiomeZoomedIn += HandleBiomeZoomedIn;
        MapManager.OnBiomeZoomedOut += HandleBiomeZoomedOut;
    }

    private void OnDisable()
    {
        SceneLoader.OnSceneLoadCompleted -= HandleSceneLoadCompleted;
        SceneLoader.OnSceneLoadStarted -= HandleSceneLoadStarted;
        MapManager.OnBiomeZoomedIn -= HandleBiomeZoomedIn;
        MapManager.OnBiomeZoomedOut -= HandleBiomeZoomedOut;
    }

    // ─────────────────────────────────────────────
    //  Scene Events
    // ─────────────────────────────────────────────

    private void HandleSceneLoadCompleted(string sceneName)
    {
        if (!sceneName.Contains(mapSceneName)) return;

        isOnMapScene = true;

        if (AudioManager.Instance == null) return;

        PlayRandomMusic(musicDefaultMap);
        StartAmbient();
    }

    private void HandleSceneLoadStarted(string sceneName)
    {
        if (!isOnMapScene) return;

        isOnMapScene = false;
        StopAmbient();
    }

    // ─────────────────────────────────────────────
    //  Biome Events  (fired by MapManager)
    // ─────────────────────────────────────────────

    private void HandleBiomeZoomedIn(BiomeController biome)
    {
        if (!isOnMapScene || AudioManager.Instance == null) return;

        PlayRandomSFX(sfxBiomeZoomIn);

        SoundGroup biomeGroup = GetMusicGroupForBiome(biome.biomName);
        if (biomeGroup != null && !biomeGroup.IsEmpty)
            PlayRandomMusic(biomeGroup);
        // No configured track → keep current map theme playing, which is fine.
    }

    private void HandleBiomeZoomedOut()
    {
        if (!isOnMapScene || AudioManager.Instance == null) return;

        PlayRandomSFX(sfxBiomeZoomOut);
        PlayRandomMusic(musicDefaultMap);
    }

    // ─────────────────────────────────────────────
    //  Ambient Helpers
    // ─────────────────────────────────────────────

    private void StartAmbient()
    {
        if (AudioManager.Instance == null || sfxAmbientLoop.IsEmpty) return;
        // Ambient loops always use the same (first) clip — randomising a loop mid-play is jarring.
        string ambientName = sfxAmbientLoop.First();
        if (!AudioManager.Instance.IsSFXPlaying(ambientName))
            AudioManager.Instance.PlaySFX(ambientName);
    }

    private void StopAmbient()
    {
        if (AudioManager.Instance == null || sfxAmbientLoop.IsEmpty) return;
        AudioManager.Instance.StopSFX(sfxAmbientLoop.First());
    }

    // ─────────────────────────────────────────────
    //  Lookup
    // ─────────────────────────────────────────────

    private SoundGroup GetMusicGroupForBiome(string biomeName)
    {
        foreach (BiomeMusicEntry entry in biomeMusic)
        {
            if (entry.biomeName == biomeName)
                return entry.musicGroup;
        }
        return null;
    }

    // ─────────────────────────────────────────────
    //  Playback Helpers
    // ─────────────────────────────────────────────

    private void PlayRandomMusic(SoundGroup group)
    {
        if (group == null || group.IsEmpty) return;
        AudioManager.Instance.CrossfadeMusic(group.PickRandom());
    }

    private void PlayRandomSFX(SoundGroup group)
    {
        if (group == null || group.IsEmpty) return;
        AudioManager.Instance.PlaySFX(group.PickRandom());
    }

    // ─────────────────────────────────────────────
    //  Public API
    // ─────────────────────────────────────────────

    /// <summary>
    /// Force a return to the default map theme from code (e.g. when closing a level info panel).
    /// </summary>
    public void ReturnToDefaultMapMusic()
    {
        if (AudioManager.Instance == null) return;
        PlayRandomMusic(musicDefaultMap);
    }
}

/// <summary>
/// Maps a BiomeController.biomName to a group of AudioManager music tracks.
/// Add one track name for a fixed theme, or several to rotate randomly on each zoom-in.
/// </summary>
[Serializable]
public class BiomeMusicEntry
{
    [Tooltip("Must match BiomeController.biomName exactly (case-sensitive).")]
    public string biomeName;

    [Tooltip("One or more AudioManager music track names. Multiple entries → random pick on zoom-in.")]
    public SoundGroup musicGroup;
}