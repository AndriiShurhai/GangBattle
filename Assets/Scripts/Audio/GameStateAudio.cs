using UnityEngine;

/// <summary>
/// Drives all music transitions based on game state.
/// Place on any persistent GameObject in the battle scene.
///
/// NOTE: TurnManager.StartPlayerTurn / StartEnemyTurn don't fire
/// events yet. To get per-turn music switching, add these two lines
/// to TurnManager.cs:
///
///   public event Action OnPlayerTurnStarted;
///   public event Action OnEnemyTurnStarted;
///
/// Then fire them at the top of StartPlayerTurn() and StartEnemyTurn().
/// Until then, this class plays battle music once on init and handles
/// win/lose transitions.
/// </summary>
public class GameStateAudio : MonoBehaviour
{
    [Header("Music Tracks — must match AudioManager.musicTracks names")]
    [SerializeField] private string musicBattle = "music_battle";
    [SerializeField] private string musicVictory = "music_victory";
    [SerializeField] private string musicDefeat = "music_defeat";

    [Header("Stingers (one-shot SFX on state change)")]
    [SerializeField] private string sfxVictoryStinger = "sfx_victory_stinger";
    [SerializeField] private string sfxDefeatStinger = "sfx_defeat_stinger";
    [SerializeField] private string sfxTurnEnd = "sfx_turn_end";

    // ─────────────────────────────────────────────
    //  Lifecycle
    // ─────────────────────────────────────────────

    private void OnEnable()
    {
        TurnManager.OnUnitsInitialized += HandleBattleStarted;

        // Subscribe via instance — TurnManager is a scene singleton
        // We wait until Start() so TurnManager.Instance is guaranteed to exist
    }

    private void Start()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnLevelCompleted += HandleLevelCompleted;
            TurnManager.Instance.OnLevelFailed += HandleLevelFailed;
        }
    }

    private void OnDisable()
    {
        TurnManager.OnUnitsInitialized -= HandleBattleStarted;

        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnLevelCompleted -= HandleLevelCompleted;
            TurnManager.Instance.OnLevelFailed -= HandleLevelFailed;
        }
    }

    // ─────────────────────────────────────────────
    //  Handlers
    // ─────────────────────────────────────────────

    private void HandleBattleStarted()
    {
        if (AudioManager.Instance == null) return;
        AudioManager.Instance.CrossfadeMusic(musicBattle);
    }

    private void HandleLevelCompleted()
    {
        if (AudioManager.Instance == null) return;
        AudioManager.Instance.PlaySFX(sfxVictoryStinger);
        AudioManager.Instance.CrossfadeMusic(musicVictory);
    }

    private void HandleLevelFailed()
    {
        if (AudioManager.Instance == null) return;
        AudioManager.Instance.PlaySFX(sfxDefeatStinger);
        AudioManager.Instance.CrossfadeMusic(musicDefeat);
    }
}