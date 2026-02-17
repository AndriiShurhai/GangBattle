using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Heal Ability")]
public class HealAbilitySO : AbilityBaseSO
{
    [Header("Heal Settings")]
    public bool canHealSelf = true;
    public float coefficent = 1.5f;

    public override void Execute(Unit caster, Vector3Int targetPosition, Action onAbilityInvoke)
    {
        IGridObject targetObject = GridObjectRegistry.Instance.GetObjectAt(targetPosition);

        if (targetObject is Unit targetUnit)
        {
            int healAmount = GetPower(caster);
            targetUnit.Heal(healAmount);

            int teamHealAmount = healAmount / 2;

            foreach (Unit unit in TurnManager.Instance.GetAlivePlayerUnits())
            {
                if (unit != targetUnit)
                {
                    unit.Heal(teamHealAmount);
                }
            }
            onAbilityInvoke.Invoke();

            Debug.Log($"{caster.name} healed {targetUnit.name} for {healAmount} HP!");

            if (abilityEffectPrefab != null)
            {
                Vector3 worldPos = GridManager.Instance.GridToWorld(targetPosition);
                GameObject effect = Instantiate(abilityEffectPrefab, worldPos, Quaternion.identity);
                Destroy(effect, 2f);
            }
        }
        else
        {
            Debug.LogWarning($"No unit to heal at {targetPosition}");
        }
    }

    public override bool IsValidTarget(Vector3Int casterPosition, Vector3Int targetPosition, Unit caster)
    {
        if (!GetTilesInRange(casterPosition).Contains(targetPosition))
            return false;

        IGridObject targetObject = GridObjectRegistry.Instance.GetObjectAt(targetPosition);
        if (!(targetObject is Unit targetUnit))
            return false;

        if (!canHealSelf && targetUnit == caster)
            return false;

        if (targetUnit.CurrentHealth <= 0)
            return false;


        // TODO: Add team check - only heal allies
        if (targetType == TargetType.Ally && targetUnit.UnitFaction != caster.UnitFaction)
        {
            return false;
        }

        return true;
    }
}