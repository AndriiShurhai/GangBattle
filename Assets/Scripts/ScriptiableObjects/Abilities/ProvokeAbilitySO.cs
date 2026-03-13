using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Provoke Ability")]
public class ProvokeAbilitySO : AbilityBaseSO
{
    [UnityEngine.Serialization.FormerlySerializedAs("provokeDuration")]
    [SerializeField] private int _provokeDuration = 2;
    public int ProvokeDuration => _provokeDuration;
    public override void Execute(Unit caster, Vector3Int targetPosition, Action onAbilityInvoke = null)
    {
        List<Vector3Int> reachableTiles = GetTilesInRange(targetPosition);

        ProvokeVisualEffect highlight = Instantiate(AbilityEffectPrefab, caster.transform.position, Quaternion.identity).GetComponent<ProvokeVisualEffect>();

        highlight.Execute(caster.GridPosition, reachableTiles);

        foreach (var position in reachableTiles)
        {
            var gridObject = GridObjectRegistry.Instance.GetObjectAt(position);

            if (gridObject != null && gridObject is Unit unit && unit.UnitFaction != caster.UnitFaction)
            {
                unit.ProvokeUnit(caster);
                unit.ApplyEffect(EffectStatusType.Provoked, ProvokeDuration);

                Debug.Log($"Provoking Unit: {unit.name}");
            }
        }
        onAbilityInvoke?.Invoke();
    }
}

