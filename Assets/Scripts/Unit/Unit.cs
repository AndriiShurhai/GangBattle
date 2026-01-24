using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class UnitSnapshotState
{
    public Vector3Int gridPosition;
    public List<AbilityBaseSO> abilities;

    public int currentHealth;
    public int maxHealth;

    public bool isMoving;

    public bool hasTakenActionThisTurn;
    public Dictionary<AbilityBaseSO, int> usedAbilitiesAmountPerTurn;
    public int movedPerTurn;

    public bool isAlive;
}
public enum Faction
{
    Player, 
    Enemy
}
public class Unit : MonoBehaviour, IMoveable, IRewindable
{
    public static event Action<Unit, Vector3Int> OnUnitEnteredTile;

    public event Action<Unit> OnUnitDied;
    public event Action OnUnitMadeAction;

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

    public string RewindID
    {
        get => id.ID;
    }

    public bool IsAlive
    {
        get => isAlive;
    }

    public int MovementRange => movementComponent.MovementRange;
    public bool IsMoving => movementComponent.IsMoving;

    public bool HasTakenActionThisTurn { get; set; }
    public Faction UnitFaction { get => faction; }

    private Dictionary<AbilityBaseSO, int> usedAbilitiesAmountPerTurn = new Dictionary<AbilityBaseSO, int>();
    private int movedPerTurn = 0;
    private RewindableID id;

    private bool isAlive = true;

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

        id = gameObject.AddComponent<RewindableID>();
        RegisterSelf();
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

        if (RewindManager.Instance != null)
        {
            RewindManager.Instance.UnregisterRewindable(this);
        }

        if (healthComponent != null)
        {
            healthComponent.OnDeath -= HandleDeath; 
        }
    }

    public void RegisterSelf()
    {
        RewindManager.Instance.RegisterRewindable(this);
    }
    
    public object CaptureState()
    {
        Dictionary<AbilityBaseSO, int> abilitiesCopy = new();

        foreach (var kvp in usedAbilitiesAmountPerTurn)
        {
            abilitiesCopy[kvp.Key] = kvp.Value;
        }

        UnitSnapshotState state = new UnitSnapshotState
        {
            gridPosition = this.gridPosition,
            hasTakenActionThisTurn = this.HasTakenActionThisTurn,
            usedAbilitiesAmountPerTurn = abilitiesCopy,
            movedPerTurn = this.movedPerTurn,

            currentHealth = healthComponent.CurrentHealth,
            maxHealth = healthComponent.MaxHealth,

            isMoving = movementComponent.IsMoving,

            abilities = this.Abilities,

            isAlive = this.IsAlive
        };

        Debug.Log($"Capture State has been called.\nGridPosition: {gridPosition}");
        return state;
    }

    public object CaptureDeactivatedState()
    {
        Dictionary<AbilityBaseSO, int> abilitiesCopy = new();

        foreach (var kvp in usedAbilitiesAmountPerTurn)
        {
            abilitiesCopy[kvp.Key] = kvp.Value;
        }

        UnitSnapshotState state = new UnitSnapshotState
        {
            gridPosition = this.gridPosition,
            hasTakenActionThisTurn = this.HasTakenActionThisTurn,
            usedAbilitiesAmountPerTurn = abilitiesCopy,
            movedPerTurn = this.movedPerTurn,

            currentHealth = 0,
            maxHealth = 0,

            isMoving = false,

            abilities = this.Abilities,

            isAlive = false
        };

        return state;
    }

    public void RestoreState(object state)
    {
        StopAllCoroutines();
        movementComponent.StopAllCoroutines();
        var s = (UnitSnapshotState)state;

        if (!s.isAlive && isAlive)
        {
            SetUnitDead(false);
            return;
        }
        else if (s.isAlive && !isAlive)
        {
            RessurectUnit();
        }

        Debug.Log($"RESTORE STATE HAS BEEN CALLED IN UNIT. \nGridPositionToMove: {s.gridPosition}");

        if (gridPosition != s.gridPosition)
        {
            Vector3 targetWorldPosition = GridManager.Instance.GridToWorld(s.gridPosition);
            transform.DOMove(targetWorldPosition, 1f);

            IGridObject obj = GridObjectRegistry.Instance.GetObjectAt(s.gridPosition);
            GridObjectRegistry.Instance.UnregisterObject(obj, s.gridPosition);
            GridObjectRegistry.Instance.MoveObject(this, gridPosition, s.gridPosition);
        }

        gridPosition = s.gridPosition;
        characterClassSO.abilities = s.abilities;
        HasTakenActionThisTurn = s.hasTakenActionThisTurn;

        healthComponent.SetHealth(s.currentHealth, s.maxHealth);
        movementComponent.SetMovingState(s.isMoving);

        usedAbilitiesAmountPerTurn.Clear();
        foreach (var kvp in s.usedAbilitiesAmountPerTurn)
        {
            usedAbilitiesAmountPerTurn[kvp.Key] = kvp.Value;    
        }

        movedPerTurn = s.movedPerTurn;

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
        OnUnitMadeAction?.Invoke();
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
        SetUnitDead(true);
    }

    private void SetUnitDead(bool triggerEvent = true)
    {
        isAlive = false;
        gameObject.SetActive(false);
        GridObjectRegistry.Instance.UnregisterObject(this, gridPosition);

        if (triggerEvent)
        {
            OnUnitDied?.Invoke(this);
        }
    }

    private void RessurectUnit()
    {
        isAlive = true;
        gameObject.SetActive(true);
        GridObjectRegistry.Instance.RegisterObject(this);

        Debug.Log($"{gameObject.name} has been resurrected");
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
        OnUnitMadeAction?.Invoke();
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
