using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Provoke Ability")]
public class ProvokeAbilitySO : AbilityBaseSO
{
    public int provokeDuration = 2;
    public override void Execute(Unit caster, Vector3Int targetPosition, Action onAbilityInvoke = null)
    {
        List<Vector3Int> reachableTiles = GetTilesInRange(targetPosition);

        ProvokeVisualEffect highlight = Instantiate(abilityEffectPrefab, caster.transform.position, Quaternion.identity).GetComponent<ProvokeVisualEffect>();

        highlight.Execute(caster.GridPosition, reachableTiles);

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
        onAbilityInvoke?.Invoke();
    }
}

