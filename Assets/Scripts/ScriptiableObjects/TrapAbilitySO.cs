using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Trap Ability")]
public class TrapAbilitySO : AbilityBaseSO
{
    [Header("Trap settings")]
    public GameObject trapPrefab;
    public int trapDamage = 15;
    public int duration = 3;

    public override void Execute(Unit caster, Vector3Int targetPosition)
    {
        IGridObject gridObject = GridObjectRegistry.Instance.GetObjectAt(targetPosition);   

        if (gridObject != null)
        {
            Debug.LogWarning("Cannot place trap because tile is occupied");
            return;
        }

        CreateTrap(targetPosition);

        Debug.Log($"{caster.name} placed a trap at {targetPosition}");

        if (abilityEffectPrefab != null)
        {
            Vector3 worldPosition = GridManager.Instance.GridToWorld(targetPosition);
            GameObject effect = Instantiate(abilityEffectPrefab, worldPosition, Quaternion.identity);
            Destroy(effect, 1);
        }
    }

    private void CreateTrap(Vector3Int position)
    {
        if (trapPrefab == null)
        {
            Debug.LogError("Trap prefab is not assigned in the TrapAbilitySO!");
            return;
        }
        Vector3 worldPosition = GridManager.Instance.GridToWorld(position);
        GameObject trapObject = Instantiate(trapPrefab, worldPosition, Quaternion.identity);

        Trap trap = trapObject.GetComponent<Trap>();
        if (trap != null) trap.Initialize(position, trapDamage, duration);
    }
}
