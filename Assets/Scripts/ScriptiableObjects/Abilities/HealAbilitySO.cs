using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Heal Ability")]
public class HealAbilitySO : AbilityBaseSO
{
    [Header("Heal Settings")]
    public int healAmount = 20;
    public bool canHealSelf = true;

    public override void Execute(Unit caster, Vector3Int targetPosition)
    {
        IGridObject targetObject = GridObjectRegistry.Instance.GetObjectAt(targetPosition);

        if (targetObject is Unit targetUnit)
        {
            targetUnit.Heal(healAmount);

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