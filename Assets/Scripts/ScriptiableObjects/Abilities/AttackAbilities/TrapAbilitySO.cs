using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Trap Ability")]
public class TrapAbilitySO : AbilityBaseSO
{
    [Header("Trap settings")]
    [UnityEngine.Serialization.FormerlySerializedAs("trapPrefab")]
    [SerializeField] private GameObject _trapPrefab;
    public GameObject TrapPrefab => _trapPrefab;

    [UnityEngine.Serialization.FormerlySerializedAs("trapDamage")]
    [SerializeField] private int _trapDamage = 15;
    public int TrapDamage => _trapDamage;

    [UnityEngine.Serialization.FormerlySerializedAs("duration")]
    [SerializeField] private int _duration = 3;
    public int Duration => _duration;

    public override void Execute(Unit caster, Vector3Int targetPosition, Action onAbilityInvoke)
    {
        IGridObject gridObject = GridObjectRegistry.Instance.GetObjectAt(targetPosition);   

        if (gridObject != null)
        {
            Debug.LogWarning("Cannot place trap because tile is occupied");
            return;
        }

        CreateTrap(targetPosition);
        onAbilityInvoke?.Invoke();

        Debug.Log($"{caster.name} placed a trap at {targetPosition}");

        if (AbilityEffectPrefab != null)
        {
            Vector3 worldPosition = GridManager.Instance.GridToWorld(targetPosition);
            GameObject effect = Instantiate(AbilityEffectPrefab, worldPosition, Quaternion.identity);
            Destroy(effect, 1);
        }
    }

    private void CreateTrap(Vector3Int position)
    {
        if (TrapPrefab == null)
        {
            Debug.LogError("Trap prefab is not assigned in the TrapAbilitySO!");
            return;
        }
        Vector3 worldPosition = GridManager.Instance.GridToWorld(position);
        GameObject trapObject = Instantiate(TrapPrefab, worldPosition, Quaternion.identity);

        Trap trap = trapObject.GetComponent<Trap>();
        if (trap != null) trap.Initialize(position, TrapDamage, Duration);
    }
}
