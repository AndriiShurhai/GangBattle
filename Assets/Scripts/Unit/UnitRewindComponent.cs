using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UnitRewindComponent
{
    private Unit rewindableUnit;

    public void Initialize(Unit unit)
    {
        rewindableUnit = unit;
    }

    public object CaptureState()
    {
        Debug.Log($"CaptureState called. GridPosition: {rewindableUnit.GridPosition}");

        return new UnitSnapshotState
        {
            gridPosition = rewindableUnit.GridPosition,
            hasTakenActionThisTurn = rewindableUnit.HasTakenActionThisTurn,
            usedAbilitiesAmountPerTurn = CopyAbilities(rewindableUnit.UsedAbilitiesAmountPerTurn),
            movedPerTurn = rewindableUnit.MovedPerTurn,
            currentHealth = rewindableUnit.CurrentHealth,
            maxHealth = rewindableUnit.MaxHealth,
            isMoving = rewindableUnit.IsMoving,
            abilities = rewindableUnit.Abilities,
            isAlive = rewindableUnit.IsAlive,
            activeEffects = rewindableUnit.StatusEffects.CaptureEffects()
        };
    }

    public object CaptureDeactivatedState()
    {
        return new UnitSnapshotState
        {
            gridPosition = rewindableUnit.GridPosition,
            hasTakenActionThisTurn = rewindableUnit.HasTakenActionThisTurn,
            usedAbilitiesAmountPerTurn = CopyAbilities(rewindableUnit.UsedAbilitiesAmountPerTurn),
            movedPerTurn = rewindableUnit.MovedPerTurn,
            currentHealth = 0,
            maxHealth = 0,
            isMoving = false,
            abilities = rewindableUnit.Abilities,
            isAlive = false
        };
    }

    public void RestoreState(object state)
    {
        var s = (UnitSnapshotState)state;

        // ── Alive/dead transitions ────────────────────────────────────────────
        if (!s.isAlive && rewindableUnit.IsAlive)
        {
            rewindableUnit.SetUnitDead(triggerEvent: false);
            return;
        }

        if (s.isAlive && !rewindableUnit.IsAlive)
        {
            rewindableUnit.RessurectUnit();
        }

        Debug.Log($"RestoreState: moving unit to {s.gridPosition}");

        // ── Position restore ─────────────────────────────────────────────────
        if (rewindableUnit.GridPosition != s.gridPosition)
        {
            // If another object now occupies the target tile, evict it first.
            // Only unregister if it's NOT this unit (guard against stale registry state).
            IGridObject occupant = GridObjectRegistry.Instance.GetObjectAt(s.gridPosition);
            if (occupant != null && !ReferenceEquals(occupant, rewindableUnit))
                GridObjectRegistry.Instance.UnregisterObject(occupant, s.gridPosition);

            Vector3 targetWorldPosition = GridManager.Instance.GridToWorld(s.gridPosition);
            rewindableUnit.transform.DOMove(targetWorldPosition, 1f);

            GridObjectRegistry.Instance.MoveObject(rewindableUnit, rewindableUnit.GridPosition, s.gridPosition);
        }

        // ── State restore ─────────────────────────────────────────────────────
        rewindableUnit.HasTakenActionThisTurn = s.hasTakenActionThisTurn;
        rewindableUnit.SetHealth(s.currentHealth, s.maxHealth);
        rewindableUnit.SetMovingState(s.isMoving);
        rewindableUnit.CopyUsedAbilities(s.usedAbilitiesAmountPerTurn);
        rewindableUnit.MovedPerTurn = s.movedPerTurn;
        rewindableUnit.StatusEffects.RestoreEffects(s.activeEffects);
    }

    private static Dictionary<AbilityBaseSO, int> CopyAbilities(Dictionary<AbilityBaseSO, int> source)
    {
        var copy = new Dictionary<AbilityBaseSO, int>(source.Count);
        foreach (var kvp in source)
            copy[kvp.Key] = kvp.Value;
        return copy;
    }
}