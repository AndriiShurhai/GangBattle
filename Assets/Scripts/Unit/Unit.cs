using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public enum Faction
{
    Player, 
    Enemy
}
public class Unit : MonoBehaviour, IMoveable
{
    public static event Action<Unit, Vector3Int> OnUnitEnteredTile;

    public event Action<Unit> OnUnitDied;

    [Header("Components")]
    [SerializeField] private HealthComponent healthComponent;
    [SerializeField] private MovementComponent movementComponent;

    [Header("Unit Settings")]
    [SerializeField] private Faction faction;

    [Header("Class Defenition")]
    [SerializeField] private CharacterClassSO characterClassSO;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private Transform healthBarAttachPoint;

    private Vector3Int gridPosition;
    public List<AbilityBaseSO> Abilities {  get { return characterClassSO.abilities; } }
    public int CurrentHealth { get { return healthComponent.CurrentHealth; } }
    public int MaxHealth { get { return healthComponent.MaxHealth; } }
    public HealthComponent Health { get { return healthComponent; } }
    
    public Vector3Int GridPosition
    {
        get => gridPosition;
        set => gridPosition = value;
    }

    public bool BlocksMovement
    {
        get => true;
    }

    public int MovementRange => movementComponent.MovementRange;
    public bool IsMoving => movementComponent.IsMoving;

    public bool HasTakenActionThisTurn { get; set; }
    public Faction UnitFaction { get => faction; }

    private Dictionary<AbilityBaseSO, int> usedAbilitiesAmountPerTurn = new Dictionary<AbilityBaseSO, int>();
    private int movedPerTurn = 0;

    public void Initialize()
    {
        if (healthComponent == null)
        {
            healthComponent = GetComponent<HealthComponent>();
        }

        if (movementComponent == null)
        {
            movementComponent = GetComponent<MovementComponent>();
        }

        healthComponent.OnDeath += HandleDeath;

        InitializeFromClass();

        foreach (AbilityBaseSO ability in Abilities)
        {
            usedAbilitiesAmountPerTurn[ability] = 0;
        }
    }

    public void PlaceUnit(Vector3 position)
    {
        gridPosition = GridManager.Instance.WorldToGrid(position);
        GridObjectRegistry.Instance.RegisterObject(this);
        transform.position = GridManager.Instance.GridToWorld(gridPosition);
    }
    private void InitializeFromClass()
    {
        if (characterClassSO == null) return;

        healthComponent.Initialize(characterClassSO);
        movementComponent.Initialize(characterClassSO);
    }

    private void Start()
    {
        if(healthBarPrefab != null && healthBarAttachPoint != null)
        {
            GameObject healthBarInstance = Instantiate(healthBarPrefab, healthBarAttachPoint.position, Quaternion.identity, healthBarAttachPoint);
            healthBarInstance.GetComponent<UnitHealthUI>().Initialize(healthComponent);
        }
    }

    private void OnDestroy()
    {
        if (GridObjectRegistry.Instance != null)
        {
            GridObjectRegistry.Instance.UnregisterObject(this, gridPosition);
        }

        if (healthComponent != null)
        {
            healthComponent.OnDeath -= HandleDeath; 
        }
    }

    public bool CanMoveTo(Vector3Int position)
    {
        return movedPerTurn <= characterClassSO.movementAmountPerTurn && movementComponent.CanMoveTo(position);
    }

    public void MoveTo(Vector3Int targetPosition, Action onComplete = null)
    {
        movementComponent.MoveTo(targetPosition, onComplete);
        movedPerTurn++;
    }
    public void OnGridPositionChanged(Vector3Int newGridPosition)
    {
        Debug.Log($"Unit moved to a new position: {newGridPosition}");
    }

    public void TakeDamage(int damage, Unit attacker)
    {
        healthComponent.TakeDamage(damage);
    }

    public void Heal(int amount)
    {
        healthComponent.Heal(amount);
    }

    private void HandleDeath()
    {
        
        GridObjectRegistry.Instance.UnregisterObject(this, gridPosition);
        OnUnitDied?.Invoke(this);
        Destroy(gameObject);
    }

    public bool CanUseAbility(AbilityBaseSO abilitySO)
    {
        if (!characterClassSO.abilities.Contains(abilitySO))
        {
            return false;
        }

        if (abilitySO.howMuchCanBeUsed <= usedAbilitiesAmountPerTurn[abilitySO])
        {
            return false;
        }

        return abilitySO.CanUse(this);
    }

    public void UseAbility(AbilityBaseSO abilitySO, Vector3Int targetPosition)
    {
        if (!CanUseAbility(abilitySO))
        {
            Debug.LogWarning("Unit can't use ability");
            return;
        }

        if (!abilitySO.IsValidTarget(gridPosition, targetPosition, this))
        {
            Debug.Log("Target is invlaid");
            return;
        }

        abilitySO.Execute(this, targetPosition);
        usedAbilitiesAmountPerTurn[abilitySO]++;
    }

    public void ResetUsedAbilities()
    {
        foreach (AbilityBaseSO ability in usedAbilitiesAmountPerTurn.Keys.ToList())
        {
            usedAbilitiesAmountPerTurn[ability] = 0;
        }
    }

    public void ResetUsedMovement()
    {
        movedPerTurn = 0;
    }

    public static void InvokeUnitEnteredTile(Unit unit, Vector3Int tilePosition)
    {
        OnUnitEnteredTile?.Invoke(unit, tilePosition);
    }
}
