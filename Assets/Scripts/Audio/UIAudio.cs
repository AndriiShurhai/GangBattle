using UnityEngine;

/// <summary>
/// Handles all UI and menu-related sound effects.
/// Place on any persistent GameObject (same one as AudioManager is fine).
/// Uses GameInput.Instance — no inspector wiring needed.
/// </summary>
public class UIAudio : MonoBehaviour
{
    [Header("Menu / Navigation")]
    [SerializeField] private string sfxButtonClick = "sfx_ui_click";
    [SerializeField] private string sfxPause = "sfx_ui_pause";
    [SerializeField] private string sfxUnpause = "sfx_ui_unpause";
    [SerializeField] private string sfxLevelNodeClick = "sfx_ui_level_select";

    [Header("Scene Transitions")]
    [SerializeField] private string sfxSceneTransition = "sfx_scene_transition";

    [Header("Music — Main Menu")]
    [SerializeField] private string musicMainMenu = "music_main_menu";

    // Track pause state so we can play different sounds for pause vs unpause
    private bool isPaused = false;

    // ─────────────────────────────────────────────
    //  Lifecycle
    // ─────────────────────────────────────────────

    private void Start()
    {
        // GameInput is a singleton — safe to grab in Start
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnPauseAction += HandlePauseToggled;
            GameInput.Instance.OnInteractAction += HandleInteract;
        }

        LevelNode.OnLevelNodeClick += HandleLevelNodeClick;
        SceneLoader.OnSceneLoadStarted += HandleSceneLoadStarted;
        SceneLoader.OnSceneLoadCompleted += HandleSceneLoadCompleted;
    }

    private void OnDestroy()
    {
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnPauseAction -= HandlePauseToggled;
            GameInput.Instance.OnInteractAction -= HandleInteract;
        }

        LevelNode.OnLevelNodeClick -= HandleLevelNodeClick;
        SceneLoader.OnSceneLoadStarted -= HandleSceneLoadStarted;
        SceneLoader.OnSceneLoadCompleted -= HandleSceneLoadCompleted;
    }

    // ─────────────────────────────────────────────
    //  Handlers
    // ─────────────────────────────────────────────

    private void HandlePauseToggled()
    {
        if (AudioManager.Instance == null) return;

        isPaused = !isPaused;
        AudioManager.Instance.PlaySFX(isPaused ? sfxPause : sfxUnpause);
    }

    private void HandleInteract()
    {
        if (AudioManager.Instance == null) return;
        AudioManager.Instance.PlaySFX(sfxButtonClick);
    }

    private void HandleLevelNodeClick(LevelNode node)
    {
        if (AudioManager.Instance == null) return;
        AudioManager.Instance.PlaySFX(sfxLevelNodeClick);
    }

    private void HandleSceneLoadStarted(string sceneName)
    {
        if (AudioManager.Instance == null) return;
        AudioManager.Instance.PlaySFX(sfxSceneTransition);
    }

    private void HandleSceneLoadCompleted(string sceneName)
    {
        if (AudioManager.Instance == null) return;

        // When the main menu loads, crossfade to menu music
        // Adjust the scene name to match yours
        if (sceneName.Contains("MainMenu") || sceneName.Contains("Menu"))
            AudioManager.Instance.CrossfadeMusic(musicMainMenu);
    }
}