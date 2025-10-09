using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Attack Ability")]
public class AttackAbilitySO : AbilityBaseSO
{
    [Header("Attack Settings")]
    public int damage = 10;
    public bool canAttackDiagonally = true;

    public override void Execute(Unit caster, Vector3Int targetPosition)
    {
        IGridObject targetObject = GridObjectRegistry.Instance.GetObjectAt(targetPosition);

        if (targetObject is Unit targetUnit)
        {
            targetUnit.TakeDamage(damage, caster);

            Debug.Log($"{caster.name} attacked {targetUnit.name} for {damage} damage!");

            // TODO: Play attack animation
            // TODO: Show damage numbers
            // TODO: Play sound effect

            // Spawn effect if available
            if (abilityEffectPrefab != null)
            {
                Vector3 worldPos = GridManager.Instance.GridToWorld(targetPosition);
                GameObject effect = Instantiate(abilityEffectPrefab, worldPos, Quaternion.identity);
                Destroy(effect, 2f); 
            }
        }
        else
        {
            Debug.LogWarning($"No valid target at {targetPosition}");
        }
    }

    public override bool IsValidTarget(Vector3Int casterPosition, Vector3Int targetPosition, Unit caster)
    {
        if (!base.IsValidTarget(casterPosition, targetPosition, caster))
            return false;

        if (!canAttackDiagonally)
        {
            int dx = Mathf.Abs(targetPosition.x - casterPosition.x);
            int dy = Mathf.Abs(targetPosition.y - casterPosition.y);

            // Must be orthogonal (either dx or dy should be 0)
            if (dx > 0 && dy > 0)
                return false;
        }

        return true;
    }
}