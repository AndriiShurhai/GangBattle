using UnityEngine;

/// <summary>
/// Handles all unit-related sound effects.
/// Place on any persistent GameObject in the battle scene.
/// Subscribes to Unit static events — no references needed in the inspector.
/// </summary>
public class UnitAudio : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private string sfxPlayerHit = "sfx_player_hit";
    [SerializeField] private string sfxEnemyHit = "sfx_enemy_hit";
    [SerializeField] private string sfxCriticalHit = "sfx_critical_hit";

    [Header("Death")]
    [SerializeField] private string sfxPlayerDeath = "sfx_player_death";
    [SerializeField] private string sfxEnemyDeath = "sfx_enemy_death";

    [Header("Healing")]
    [SerializeField] private string sfxHeal = "sfx_heal";

    [Header("Movement")]
    [SerializeField] private string sfxUnitMove = "sfx_unit_move";

    [Header("Abilities")]
    // Fallback SFX if the AbilityBaseSO.SfxOnCast field is empty
    [SerializeField] private string sfxAbilityFallback = "sfx_ability_generic";

    [Header("Status Effects — Gained")]
    [SerializeField] private string sfxGainedStunned = "sfx_status_stunned";
    [SerializeField] private string sfxGainedRooted = "sfx_status_rooted";
    [SerializeField] private string sfxGainedBurned = "sfx_status_burned";
    [SerializeField] private string sfxGainedProvoked = "sfx_status_provoked";
    [SerializeField] private string sfxGainedBoosted = "sfx_status_boosted";

    [Header("Status Effects — Lost")]
    [SerializeField] private string sfxLostDebuff = "sfx_status_expired";

    // ─────────────────────────────────────────────
    //  Lifecycle
    // ─────────────────────────────────────────────

    private void OnEnable()
    {
        Unit.OnAnyUnitTookDamage += HandleUnitTookDamage;
        Unit.OnAnyUnitDied += HandleUnitDied;
        Unit.OnAnyUnitHealed += HandleUnitHealed;
        Unit.OnAnyUnitStartMoving += HandleUnitStartMoving;
        Unit.OnAnyUnitFinishedMoving += HandleUnitFinishedMoving;
        Unit.OnAnyUnitCastingAbility += HandleUnitCastingAbility;
        Unit.OnAnyUnitUsedAbility += HandleUnitUsedAbility;
        Unit.OnAnyUnitGainedStatusEffect += HandleUnitGainedStatusEffect;
        Unit.OnAnyUnitLostStatusEffect += HandleUnitLostStatusEffect;
    }

    private void OnDisable()
    {
        Unit.OnAnyUnitTookDamage -= HandleUnitTookDamage;
        Unit.OnAnyUnitDied -= HandleUnitDied;
        Unit.OnAnyUnitHealed -= HandleUnitHealed;
        Unit.OnAnyUnitStartMoving -= HandleUnitStartMoving;
        Unit.OnAnyUnitFinishedMoving -= HandleUnitFinishedMoving;
        Unit.OnAnyUnitCastingAbility -= HandleUnitCastingAbility;
        Unit.OnAnyUnitUsedAbility -= HandleUnitUsedAbility;
        Unit.OnAnyUnitGainedStatusEffect -= HandleUnitGainedStatusEffect;
        Unit.OnAnyUnitLostStatusEffect -= HandleUnitLostStatusEffect;
    }

    // ─────────────────────────────────────────────
    //  Handlers
    // ─────────────────────────────────────────────

    private void HandleUnitTookDamage(Unit unit, int damage, int currentHealth)
    {
        if (AudioManager.Instance == null) return;

        // Use positional audio so hits sound like they come from the unit's location
        bool isPlayer = unit.UnitFaction == Faction.Player;
        string sfx = isPlayer ? sfxPlayerHit : sfxEnemyHit;

        AudioManager.Instance.PlaySFXAtPosition(sfx, unit.transform.position);
    }

    private void HandleUnitDied(Unit unit)
    {
        if (AudioManager.Instance == null) return;

        bool isPlayer = unit.UnitFaction == Faction.Player;
        string sfx = isPlayer ? sfxPlayerDeath : sfxEnemyDeath;

        AudioManager.Instance.PlaySFXAtPosition(sfx, unit.transform.position);
    }

    private void HandleUnitHealed(Unit unit, int amount, int currentHealth)
    {
        if (AudioManager.Instance == null) return;
        AudioManager.Instance.PlaySFXAtPosition(sfxHeal, unit.transform.position);
    }

    private void HandleUnitStartMoving(Unit unit, Vector3 destination)
    {
        if (AudioManager.Instance == null) return;
        AudioManager.Instance.PlaySFX(sfxUnitMove);
    }

    private void HandleUnitFinishedMoving(Unit unit)
    {
        if (AudioManager.Instance == null) return;
        AudioManager.Instance.StopSFX(sfxUnitMove);
    }

    private void HandleUnitCastingAbility(Unit unit, AbilityBaseSO ability)
    {
        if (AudioManager.Instance == null) return;

        string sfx = !string.IsNullOrEmpty(ability.SfxOnCast)
            ? ability.SfxOnCast
            : sfxAbilityFallback;
    }
    private void HandleUnitUsedAbility(Unit unit, AbilityBaseSO ability)
    {
        if (AudioManager.Instance == null) return;

        // Use the SFX set on the ScriptableObject; fall back to the generic one
        string sfx = !string.IsNullOrEmpty(ability.SfxOnUse)
            ? ability.SfxOnUse
            : sfxAbilityFallback;

        AudioManager.Instance.PlaySFXAtPosition(sfx, unit.transform.position);
    }

    private void HandleUnitGainedStatusEffect(Unit unit, EffectStatusType effectType)
    {
        if (AudioManager.Instance == null) return;

        string sfx = effectType switch
        {
            EffectStatusType.Stunned => sfxGainedStunned,
            EffectStatusType.Rooted => sfxGainedRooted,
            EffectStatusType.Burned => sfxGainedBurned,
            EffectStatusType.Provoked => sfxGainedProvoked,
            EffectStatusType.Boosted => sfxGainedBoosted,
            _ => string.Empty
        };

        if (!string.IsNullOrEmpty(sfx))
            AudioManager.Instance.PlaySFXAtPosition(sfx, unit.transform.position);
    }

    private void HandleUnitLostStatusEffect(Unit unit, EffectStatusType effectType)
    {
        if (AudioManager.Instance == null) return;

        // Debuffs expiring get a neutral "expired" sound;
        // Boosted expiring gets nothing (boost ending is silent — tweak as needed)
        if (effectType != EffectStatusType.Boosted && !string.IsNullOrEmpty(sfxLostDebuff))
            AudioManager.Instance.PlaySFXAtPosition(sfxLostDebuff, unit.transform.position);
    }
}