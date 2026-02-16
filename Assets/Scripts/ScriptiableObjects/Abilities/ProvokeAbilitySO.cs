using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Provoke Ability")]
public class ProvokeAbilitySO : AbilityBaseSO
{
    public int provokeDuration = 2;
    public override void Execute(Unit caster, Vector3Int targetPosition, Action onAbilityInvoke = null)
    {
        List<Vector3Int> reachableTiles = GetTilesInRange(targetPosition);

        foreach (var position in reachableTiles)
        {
            var gridObject = GridObjectRegistry.Instance.GetObjectAt(position);

            if (gridObject != null && gridObject is Unit unit && unit.UnitFaction != caster.UnitFaction)
            {
                unit.ProvokeUnit(caster);
                unit.ApplyEffect(EffectStatusType.Provoked, provokeDuration);

                Debug.Log($"Provoking Unit: {unit.name}");
            }
        }
    }
}

