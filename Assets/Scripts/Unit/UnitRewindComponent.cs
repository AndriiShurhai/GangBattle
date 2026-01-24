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
        Dictionary<AbilityBaseSO, int> abilitiesCopy = new();

        foreach (var kvp in rewindableUnit.UsedAbilitiesAmountPerTurn)
        {
            abilitiesCopy[kvp.Key] = kvp.Value;
        }

        UnitSnapshotState state = new UnitSnapshotState
        {
            gridPosition = rewindableUnit.GridPosition,
            hasTakenActionThisTurn = rewindableUnit.HasTakenActionThisTurn,
            usedAbilitiesAmountPerTurn = abilitiesCopy,
            movedPerTurn = rewindableUnit.MovedPerTurn,

            currentHealth = rewindableUnit.CurrentHealth,
            maxHealth = rewindableUnit.MaxHealth,

            isMoving = rewindableUnit.IsMoving,

            abilities = rewindableUnit.Abilities,

            isAlive = rewindableUnit.IsAlive
        };

        return state;
    }

    public object CaptureDeactivatedState()
    {
        Dictionary<AbilityBaseSO, int> abilitiesCopy = new();

        foreach (var kvp in rewindableUnit.UsedAbilitiesAmountPerTurn)
        {
            abilitiesCopy[kvp.Key] = kvp.Value;
        }

        UnitSnapshotState state = new UnitSnapshotState
        {
            gridPosition = rewindableUnit.GridPosition,
            hasTakenActionThisTurn = rewindableUnit.HasTakenActionThisTurn,
            usedAbilitiesAmountPerTurn = abilitiesCopy,
            movedPerTurn = rewindableUnit.MovedPerTurn,

            currentHealth = 0,
            maxHealth = 0,

            isMoving = false,

            abilities = rewindableUnit.Abilities,

            isAlive = false
        };

        return state;
    }
    public void RestoreState(object state)
    {
        var s = (UnitSnapshotState)state;

        if (!s.isAlive && rewindableUnit.IsAlive)
        {
            rewindableUnit.SetUnitDead(false);
            return;
        }
        else if (s.isAlive && !rewindableUnit.IsAlive)
        {
            rewindableUnit.RessurectUnit();
        }

        Debug.Log($"RESTORE STATE HAS BEEN CALLED IN UNIT. \nGridPositionToMove: {s.gridPosition}");

        if (rewindableUnit.GridPosition != s.gridPosition)
        {
            Vector3 targetWorldPosition = GridManager.Instance.GridToWorld(s.gridPosition);
            rewindableUnit.transform.DOMove(targetWorldPosition, 1f);

            IGridObject obj = GridObjectRegistry.Instance.GetObjectAt(s.gridPosition);
            GridObjectRegistry.Instance.UnregisterObject(obj, s.gridPosition);
            GridObjectRegistry.Instance.MoveObject(rewindableUnit, rewindableUnit.GridPosition, s.gridPosition);
        }

        rewindableUnit.GridPosition = s.gridPosition;
        rewindableUnit.HasTakenActionThisTurn = s.hasTakenActionThisTurn;

        rewindableUnit.SetHealth(s.currentHealth, s.maxHealth);
        rewindableUnit.SetMovingState(s.isMoving);

        rewindableUnit.CopyUsedAbilities(s.usedAbilitiesAmountPerTurn);

        rewindableUnit.MovedPerTurn = s.movedPerTurn;
    }
}
