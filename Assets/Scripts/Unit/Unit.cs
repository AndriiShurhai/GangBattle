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
    public static event Action<Unit, Vector3> OnAnyUnitStartMoving;
    public static event Action<Unit> OnAnyUnitFinishedMoving;
    public static event Action<Unit, AbilityBaseSO> OnAnyUnitUsedAbility;
    public static event Action<Unit, EffectStatusType> OnAnyUnitGainedStatusEffect;
    public static event Action<Unit, EffectStatusType> OnAnyUnitLostStatusEffect;

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
    [SerializeField] private UnitVisualBridge unitVisualBridge;

    private Vector3Int gridPosition;
    public List<AbilityBaseSO> Abilities {  get { return characterClassSO.abilities; } }
    public int CurrentHealth { get { return healthComponent.CurrentHealth; } }
    public int MaxHealth { get { return healthComponent.MaxHealth; } }
    public HealthComponent Health { get { return healthComponent; } }
    public Vector3Int? ForcedUnitGridPosition
    {
        get
        {
            if (forcedTarget != null) return forcedTarget.GridPosition;
            else return null;
        }
    }
    public int Strength { get; private set; }
    public int Intelligence { get; private set; }
    public int Agility { get; private set; }
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

    public int MoveAllowedPerTurn => characterClassSO.movementAmountPerTurn;    

    private List<StatusEffect> activeEffects = new List<StatusEffect>();
    private Dictionary<AbilityBaseSO, int> usedAbilitiesAmountPerTurn = new Dictionary<AbilityBaseSO, int>();
    private int movedPerTurn = 0;
    private RewindableID id;
    private Unit forcedTarget = null;

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

        this.Strength = characterClassSO.strength;
        this.Intelligence = characterClassSO.intelligence;
        this.Agility = characterClassSO.agility;
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

    public void ProvokeUnit(Unit forcedTarget)
    {
        this.forcedTarget = forcedTarget;
    }

    public void BoostUnit(int strengthBonus = 0, int intelligenceBonus = 0, int agilityBonus = 0)
    {
        this.Strength += strengthBonus;
        this.Intelligence += intelligenceBonus;
        this.Agility += agilityBonus;
    }

    public void UnboostUnit(bool strength = true, bool intelligence = true, bool agility = true)
    {
        if (strength) this.Strength = characterClassSO.strength;
        if (intelligence) this.Intelligence = characterClassSO.intelligence;
        if (agility) this.Agility = characterClassSO.agility;
    }
    public void ApplyEffect(EffectStatusType effectType, int duration, Action tickAction = null, GameObject visualEffectPrefab = null)
    {
        var existing = activeEffects.Find(e => e.type == effectType);

        if (existing != null)
        {
            existing.duration = Mathf.Max(existing.duration, duration);
        }
        else
        {
            StatusEffect newEffect = new StatusEffect(effectType, duration, tickAction, visualEffectPrefab);
            activeEffects.Add(newEffect);
            Debug.Log($"{name} is {effectType} for {duration} moves");

            if ((effectType == EffectStatusType.Stunned || effectType == EffectStatusType.Rooted) && IsMoving)
            {
                movementComponent.StopAllCoroutines();
                SetMovingState(false);
                movementComponent.MoveTo(GridManager.Instance.WorldToGrid(transform.position));
                transform.position = GridManager.Instance.GridToWorld(GridPosition);
            }

            OnAnyUnitGainedStatusEffect?.Invoke(this, effectType);
            newEffect.InstantiateVisualEffect(this);
        }
    }

    public bool HasStatus(EffectStatusType effectType)
    {
        return activeEffects.Exists(e => e.type == effectType);
    }

    public void UpdateEffectsStatus()
    {
        for (int i = activeEffects.Count-1; i >= 0; i--)
        {
            StatusEffect currentEffect = activeEffects[i];
            activeEffects[i].Tick(this);
            if (activeEffects[i].duration < 0)
            {
                if (activeEffects[i].type == EffectStatusType.Boosted)
                {
                    UnboostUnit();
                }
                activeEffects[i].RemoveVisualEffect(this);
                activeEffects.RemoveAt(i);
                OnAnyUnitLostStatusEffect?.Invoke(this, currentEffect.type);
            } 
        }

    }
    public bool CanMoveTo(Vector3Int position)
    {
        if (HasStatus(EffectStatusType.Stunned) || HasStatus(EffectStatusType.Rooted))
        {
            return false;
        }

        return !movementComponent.IsMoving && movedPerTurn < characterClassSO.movementAmountPerTurn && movementComponent.CanMoveTo(position);
    }

    public void MoveTo(Vector3Int targetPosition, Action onComplete = null)
    {
        Vector3 worldTargetPosition = GridManager.Instance.GridToWorld(targetPosition);

        OnAnyUnitStartMoving.Invoke(this, worldTargetPosition);

        onComplete += () =>
        {
            OnAnyUnitFinishedMoving.Invoke(this);
        };

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
            OnAnyUnitDied?.Invoke(this);
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
        if (HasStatus(EffectStatusType.Stunned))
        {
            Debug.Log("ABILITY CAN'T BE USED BECAUSE OF STUN");
            return false;
        }

        if (!characterClassSO.abilities.Contains(abilitySO))
        {
            Debug.Log("ABILITY IS NOT IN CHARACTER CLASS ABILITIES");
            return false;
        }

        if (abilitySO.howMuchCanBeUsed <= usedAbilitiesAmountPerTurn[abilitySO])
        {
            Debug.Log("ABILITY CAN'T BE USED NO MORE THIS ROUND");
            return false;
        }

        return abilitySO.CanUse(this);
    }

    public void UseAbility(AbilityBaseSO abilitySO, Vector3Int targetPosition)
    {
        if (!CanUseAbility(abilitySO))
        {
            Debug.Log("Unit can't use ability");
            return;
        }

        if (!abilitySO.IsValidTarget(gridPosition, targetPosition, this))
        {
            Debug.Log("Target is invlaid");
            return;
        }

        if (HasStatus(EffectStatusType.Provoked) && forcedTarget != null && forcedTarget.GridPosition != targetPosition)
        {
            Debug.Log($"Unit is provoked to another target. TargetPosition {targetPosition}; ForcedUnitPosition {forcedTarget.GridPosition}");
            return;
        }

        void InvokeAbilityUsage()
        {
            OnAnyUnitUsedAbility.Invoke(this, abilitySO);
        }

        abilitySO.Execute(this, targetPosition, InvokeAbilityUsage);
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

    public UnitVisualBridge GetUnitVisualBridge()
    {
        return unitVisualBridge;
    }
}
