using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Owns and manages all status effects on a unit.
/// Extracted from Unit to keep Unit focused on coordination, not effect bookkeeping.
/// </summary>
public class UnitStatusEffectComponent : MonoBehaviour
{
    public event Action<EffectStatusType> OnEffectGained;
    public event Action<EffectStatusType> OnEffectLost;

    private readonly List<StatusEffect> activeEffects = new();
    private Unit owner;

    public void Initialize(Unit unit)
    {
        owner = unit;
    }

    public void Apply(EffectStatusType effectType, int duration, Action tickAction = null, GameObject visualEffectPrefab = null)
    {
        var existing = activeEffects.Find(e => e.type == effectType);

        if (existing != null)
        {
            // Refresh to the longer of the two durations
            existing.duration = Mathf.Max(existing.duration, duration);
            return;
        }

        var newEffect = new StatusEffect(effectType, duration, tickAction, visualEffectPrefab);
        activeEffects.Add(newEffect);
        Debug.Log($"{owner.name} gained [{effectType}] for {duration} turns.");

        // Interrupt movement immediately for hard-CC
        if ((effectType == EffectStatusType.Stunned || effectType == EffectStatusType.Rooted) && owner.IsMoving)
        {
            owner.InterruptMovement();
        }

        newEffect.InstantiateVisualEffect(owner);
        OnEffectGained?.Invoke(effectType);
    }

    public bool Has(EffectStatusType effectType) => activeEffects.Exists(e => e.type == effectType);

    /// <summary>
    /// Ticks all active effects and removes expired ones. Call once per turn.
    /// </summary>
    /// <summary>Returns a shallow copy of active effects safe to store in a snapshot.</summary>
    public List<StatusEffect> CaptureEffects() => new List<StatusEffect>(activeEffects);

    /// <summary>
    /// Restores effects from a snapshot. Cleans up current visuals first.
    /// Does NOT fire OnEffectGained — this is a silent state restore, not a fresh application.
    /// </summary>
    public void RestoreEffects(List<StatusEffect> effects)
    {
        foreach (var effect in activeEffects)
            effect.RemoveVisualEffect(owner);

        activeEffects.Clear();

        if (effects != null)
        {
            foreach (var effect in effects)
            {
                activeEffects.Add(effect);
                effect.InstantiateVisualEffect(owner);
            }
        }

        SyncAnimationState();
    }

    /// <summary>
    /// Directly sets the correct animation after a silent state restore.
    /// Called instead of firing events, since this is not a fresh effect application.
    /// </summary>
    private void SyncAnimationState()
    {
        var bridge = owner.GetUnitVisualBridge();
        if (bridge == null) return;

        if (Has(EffectStatusType.Stunned) || Has(EffectStatusType.Rooted))
            bridge.StartDebuffAnimation();
        else
            bridge.StopDebuffAnimation();
    }

    public void UpdateAll()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            EffectStatusType expiredType = activeEffects[i].type;
            activeEffects[i].Tick(owner);

            if (activeEffects[i].duration < 0)
            {
                if (expiredType == EffectStatusType.Boosted)
                    owner.UnboostUnit();

                if (expiredType == EffectStatusType.Provoked)
                {
                    owner.UnprovokeUnit();
                }

                activeEffects[i].RemoveVisualEffect(owner);
                activeEffects.RemoveAt(i);
                OnEffectLost?.Invoke(expiredType);
            }
        }
    }
}