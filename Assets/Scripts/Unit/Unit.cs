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

    public static event Action<Unit> OnAnyUnitSpawned;
    public static event Action<Unit> OnAnyUnitDied;
    public static event Action<Unit, int, int> OnAnyUnitTookDamage;
    public static event Action<Unit, int, int> OnAnyUnitHealed;

    [Header("Components")]
    [SerializeField] private HealthComponent healthComponent;
    [SerializeField] private MovementComponent movementComponent;
    private UnitRewindComponent rewindComponent = new UnitRewindComponent();

    [Header("Unit Settings")]
    [SerializeField] private Faction faction;

    [Header("Class Defenition")]
    [SerializeField] private CharacterClassSO characterClassSO;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform healthBarAttachPoint;

    private Vector3Int gridPosition;
    public List<AbilityBaseSO> Abilities {  get { return characterClassSO.abilities; } }
    public int CurrentHealth { get { return healthComponent.CurrentHealth; } }
    public int MaxHealth { get { return healthComponent.MaxHealth; } }
    public HealthComponent Health { get { return healthComponent; } }
    
    public int Damage { get; private set; }
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

    public Dictionary<AbilityBaseSO, int> UsedAbilitiesAmountPerTurn
        => new Dictionary<AbilityBaseSO, int>(usedAbilitiesAmountPerTurn);

    public int MovedPerTurn
    {
        get => movedPerTurn;
        set => movedPerTurn = value;
    }

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
        rewindComponent.Initialize(this);
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

        this.Damage = characterClassSO.damage;
    }

    private void Start()
    {
        OnAnyUnitSpawned?.Invoke(this);
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

        OnAnyUnitDied?.Invoke(this);
    }

    public void RegisterSelf()
    {
        RewindManager.Instance.RegisterRewindable(this);
    }
    
    public object CaptureState()
    {
        Debug.Log($"Capture State has been called.\nGridPosition: {gridPosition}");
        return rewindComponent.CaptureState();
    }

    public object CaptureDeactivatedState()
    {
        return rewindComponent.CaptureDeactivatedState();
    }

    public void RestoreState(object state)
    {
        StopAllCoroutines();
        movementComponent.StopAllCoroutines();
        rewindComponent.RestoreState(state);
    }

    public bool CanMoveTo(Vector3Int position)
    {
        return !movementComponent.IsMoving && movedPerTurn < characterClassSO.movementAmountPerTurn && movementComponent.CanMoveTo(position);
    }

    public void MoveTo(Vector3Int targetPosition, Action onComplete = null)
    {
        movementComponent.MoveTo(targetPosition, onComplete);
        movedPerTurn++;
    }

    public void SetMovingState(bool isMoving)
    {
        movementComponent.SetMovingState(isMoving);
    }
    public void OnGridPositionChanged(Vector3Int newGridPosition)
    {
        OnUnitMadeAction?.Invoke();
        Debug.Log($"Unit moved to a new position: {newGridPosition}");
    }

    public void TakeDamage(int damage, Unit attacker)
    {
        healthComponent.TakeDamage(damage);
        OnAnyUnitTookDamage?.Invoke(this, damage, CurrentHealth);
    }

    public void Heal(int amount)
    {
        healthComponent.Heal(amount);
        OnAnyUnitHealed?.Invoke(this, amount, CurrentHealth);
    }
    public void SetHealth(int currentHealth, int maxHealth)
    {
        healthComponent.SetHealth(currentHealth, maxHealth);
    }
    private void HandleDeath()
    {
        SetUnitDead(true);
    }

    public void SetUnitDead(bool triggerEvent = true)
    {
        isAlive = false;
        gameObject.SetActive(false);
        GridObjectRegistry.Instance.UnregisterObject(this, gridPosition);

        if (triggerEvent)
        {
            OnUnitDied?.Invoke(this);
        }
    }

    public void RessurectUnit()
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

    public void CopyUsedAbilities(Dictionary<AbilityBaseSO, int> abilities)
    {
        usedAbilitiesAmountPerTurn.Clear();
        foreach (var kvp in abilities)
        {
            usedAbilitiesAmountPerTurn[kvp.Key] = kvp.Value;
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

    public Transform GetHealthBarAttachPoint()
    {
        return healthBarAttachPoint;
    }

    public SpriteRenderer GetSpriteRenderer()
    {
        return spriteRenderer;
    }
}
