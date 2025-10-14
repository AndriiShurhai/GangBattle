using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour, IMoveable
{
    [Header("Components")]
    [SerializeField] private HealthComponent healthComponent;
    [SerializeField] private MovementComponent movementComponent;

    [Header("Abilites")]
    [SerializeField] private List<AbilityBaseSO> abilities = new List<AbilityBaseSO>();

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private Transform healthBarAttachPoint;

    private Vector3Int gridPosition;
    public List<AbilityBaseSO> Abilities {  get { return abilities; } }
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

    private void Awake()
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
    }

    private void Start()
    {
        gridPosition = GridManager.Instance.WorldToGrid(transform.position);
        GridObjectRegistry.Instance.RegisterObject(this);
        transform.position = GridManager.Instance.GridToWorld(gridPosition);

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
        return movementComponent.CanMoveTo(position);
    }

    public void MoveTo(Vector3Int targetPosition, Action onComplete = null)
    {
        movementComponent.MoveTo(targetPosition, onComplete);
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
        Destroy(gameObject);
    }

    public bool CanUseAbility(AbilityBaseSO abilitySO)
    {
        if (!abilities.Contains(abilitySO))
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
    }
}
