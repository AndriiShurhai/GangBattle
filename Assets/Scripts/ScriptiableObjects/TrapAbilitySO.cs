using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Trap Ability")]
public class TrapAbilitySO : AbilityBaseSO
{
    [Header("Trap settings")]
    public int trapDamage = 15;
    public int duration = 3;
    public GameObject trapVisualPrefab;

    public override void Execute(Unit caster, Vector3Int targetPosition)
    {
        IGridObject gridObject = GridObjectRegistry.Instance.GetObjectAt(targetPosition);   

        if (gridObject != null)
        {
            Debug.LogWarning("Cannot place trap because tile is occupied");
            return;
        }

        Trap trap = CreateTrap(targetPosition);

        Debug.Log($"{caster.name} placed a trap at {targetPosition}");

        if (abilityEffectPrefab != null)
        {
            Vector3 worldPosition = GridManager.Instance.GridToWorld(targetPosition);
            GameObject effect = Instantiate(abilityEffectPrefab, worldPosition, Quaternion.identity);
            Destroy(effect, 1);
        }
    }

    private Trap CreateTrap(Vector3Int position)
    {
        Vector3 worldPosition = GridManager.Instance.GridToWorld(position);
        GameObject trapObject = new GameObject($"trap_{position}");
        trapObject.transform.position = worldPosition;

        Trap trap = trapObject.AddComponent<Trap>();
        trap.Initialize(position, trapDamage, duration, trapVisualPrefab);

        return trap;
    }
}
