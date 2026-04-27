using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using System.Collections;

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

    public List<StatusEffect> activeEffects;
}

public enum Faction { Player, Enemy }

public class Unit : MonoBehaviour, IMoveable, IRewindable
{
    // ── Static / global events ──────────────────────────────────────────────
    public static event Action<Unit, Vector3Int> OnUnitEnteredTile;
    public static event Action<Unit> OnAnyUnitSpawned;
    public static event Action<Unit> OnAnyUnitDied;
    public static event Action<Unit, int, int> OnAnyUnitTookDamage;
    public static event Action<Unit, int, int> OnAnyUnitHealed;
    public static event Action<Unit, Vector3> OnAnyUnitStartMoving;
    public static event Action<Unit> OnAnyUnitFinishedMoving;
    public static event Action<Unit, AbilityBaseSO> OnAnyUnitCastingAbility;
    public static event Action<Unit, AbilityBaseSO> OnAnyUnitUsedAbility;
    public static event Action<Unit, EffectStatusType> OnAnyUnitGainedStatusEffect;
    public static event Action<Unit, EffectStatusType> OnAnyUnitLostStatusEffect;

    public string unitName;
    // ── Instance events ──────────────────────────────────────────────────────
    public event Action<Unit> OnUnitDied;
    public event Action OnUnitMadeAction;

    // ── Inspector ────────────────────────────────────────────────────────────
    [Header("Components")]
    [SerializeField] private HealthComponent healthComponent;
    [SerializeField] private MovementComponent movementComponent;
    [SerializeField] private UnitStatusEffectComponent statusEffectComponent;

    [Header("Unit Settings")]
    [SerializeField] private Faction faction;

    [Header("Class Definition")]
    [SerializeField] private CharacterClassSO characterClassSO;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform healthBarAttachPoint;
    [SerializeField] private UnitVisualBridge unitVisualBridge;

    // ── Private state ────────────────────────────────────────────────────────
    private Vector3Int gridPosition;
    private bool isAlive = true;
    private Unit forcedTarget;
    private int movedPerTurn;
    private RewindableID id;
    private readonly UnitRewindComponent rewindComponent = new();
    private readonly Dictionary<AbilityBaseSO, int> usedAbilitiesAmountPerTurn = new();
    private Light2D highlightLight;

    // ── Public accessors ─────────────────────────────────────────────────────
    public string UnitName => unitName;
    public GameObject ClassIcon => characterClassSO.classIconPrefab;
    public List<AbilityBaseSO> Abilities => characterClassSO.abilities;
    public int CurrentHealth => healthComponent.CurrentHealth;
    public int MaxHealth => healthComponent.MaxHealth;
    public HealthComponent Health => healthComponent;
    public UnitStatusEffectComponent StatusEffects => statusEffectComponent;
    public int Strength { get; private set; }
    public int Intelligence { get; private set; }
    public int Agility { get; private set; }
    public bool HasTakenActionThisTurn { get; set; }
    public Faction UnitFaction => faction;
    public int MovementRange => movementComponent.MovementRange;
    public bool IsMoving => movementComponent.IsMoving;
    public bool IsAlive => isAlive;
    public string RewindID => id.ID;
    public bool BlocksMovement => true;
    public int MoveAllowedPerTurn => characterClassSO.movementAmountPerTurn;

    public int MovedPerTurn
    {
        get => movedPerTurn;
        set => movedPerTurn = value;
    }

    public Vector3Int GridPosition
    {
        get => gridPosition;
        set => gridPosition = value;
    }

    /// <summary>Returns a snapshot copy — safe to pass around, not live state.</summary>
    public Dictionary<AbilityBaseSO, int> UsedAbilitiesAmountPerTurn
        => new(usedAbilitiesAmountPerTurn);

    public Vector3Int? ForcedUnitGridPosition => forcedTarget?.GridPosition;

    // ── Lifecycle ────────────────────────────────────────────────────────────
    public void Initialize()
    {
        if (healthComponent == null) healthComponent = GetComponent<HealthComponent>();
        if (movementComponent == null) movementComponent = GetComponent<MovementComponent>();
        if (statusEffectComponent == null) statusEffectComponent = GetComponent<UnitStatusEffectComponent>();

        healthComponent.OnDeath += HandleDeath;

        statusEffectComponent.Initialize(this);
        statusEffectComponent.OnEffectGained += type => OnAnyUnitGainedStatusEffect?.Invoke(this, type);
        statusEffectComponent.OnEffectLost += type => OnAnyUnitLostStatusEffect?.Invoke(this, type);

        InitializeFromClass();

        foreach (var ability in Abilities)
            usedAbilitiesAmountPerTurn[ability] = 0;

        id = gameObject.AddComponent<RewindableID>();
        rewindComponent.Initialize(this);
        RegisterSelf();

        if (GetComponentInChildren<Light2D>() != null)
            highlightLight = GetComponentInChildren<Light2D>();
        else
        {
            GameObject go = Instantiate(new GameObject("HighlightLight"), transform);
            go.AddComponent<Light2D>();

            highlightLight = go.GetComponent<Light2D>();
            highlightLight.intensity = 12f;
            highlightLight.gameObject.SetActive(false);
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

        Strength = characterClassSO.strength;
        Intelligence = characterClassSO.intelligence;
        Agility = characterClassSO.agility;
    }

    private void Start()
    {
        OnAnyUnitSpawned?.Invoke(this);
    }

    private void OnDestroy()
    {
        if (GridObjectRegistry.Instance != null)
            GridObjectRegistry.Instance.UnregisterObject(this, gridPosition);

        if (RewindManager.Instance != null)
            RewindManager.Instance.UnregisterRewindable(this);

        if (healthComponent != null)
            healthComponent.OnDeath -= HandleDeath;
    }

    public IEnumerator HighlightUnit(bool show)
    {
        float fadeDuration = 0.3f;
        if (show)
        {
            highlightLight.gameObject.SetActive(true);
            float t = 0;
            Color targetColor = highlightLight.color;
            targetColor.a = 0f;

            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                targetColor.a = Mathf.Lerp(targetColor.a, 1f, t / fadeDuration);
                highlightLight.color = targetColor;
                yield return null;
            }
        }
        else
        {
            float t = 0;
            Color targetColor = highlightLight.color;
            targetColor.a = 1f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                targetColor.a = Mathf.Lerp(targetColor.a, 0f, t / fadeDuration);
                highlightLight.color = targetColor;
                yield return null;
            }
            highlightLight.gameObject.SetActive(false);
        }
    }

    public void ForceHighlightUnit(bool show)
    {
        if (show){
            highlightLight.gameObject.SetActive(true);
        }
        else
        {
            highlightLight.gameObject.SetActive(false);
        }
    }

    public void RegisterSelf() => RewindManager.Instance.RegisterRewindable(this);

    public object CaptureState()
    {
        Debug.Log($"CaptureState called. GridPosition: {gridPosition}");
        return rewindComponent.CaptureState();
    }

    public object CaptureDeactivatedState() => rewindComponent.CaptureDeactivatedState();

    public void RestoreState(object state)
    {
        StopAllCoroutines();
        movementComponent.StopAllCoroutines();
        rewindComponent.RestoreState(state);

        if (!statusEffectComponent.Has(EffectStatusType.Provoked)) UnprovokeUnit();
    }

    // ── Stats / Buffs ─────────────────────────────────────────────────────────
    public void BoostUnit(int strengthBonus = 0, int intelligenceBonus = 0, int agilityBonus = 0)
    {
        if (HasStatus(EffectStatusType.Boosted)) { return; }
        Strength += strengthBonus;
        Intelligence += intelligenceBonus;
        Agility += agilityBonus;
    }

    public void UnboostUnit(bool strength = true, bool intelligence = true, bool agility = true)
    {
        if (strength) Strength = characterClassSO.strength;
        if (intelligence) Intelligence = characterClassSO.intelligence;
        if (agility) Agility = characterClassSO.agility;
    }

    // ── Status Effects ────────────────────────────────────────────────────────
    public void ApplyEffect(EffectStatusType effectType, int duration, Action tickAction = null, GameObject visualEffectPrefab = null)
        => statusEffectComponent.Apply(effectType, duration, tickAction, visualEffectPrefab);

    public bool HasStatus(EffectStatusType effectType) => statusEffectComponent.Has(effectType);

    public void UpdateEffectsStatus() => statusEffectComponent.UpdateAll();

    public void ProvokeUnit(Unit target) => forcedTarget = target;
    public void UnprovokeUnit() => forcedTarget = null;

    /// <summary>Called by UnitStatusEffectComponent when hard-CC is applied mid-move.</summary>
    public void InterruptMovement()
    {
        movementComponent.Interrupt();
        SetMovingState(false);
        movementComponent.MoveTo(GridManager.Instance.WorldToGrid(transform.position));
        transform.position = GridManager.Instance.GridToWorld(gridPosition);
    }

    // ── Movement ──────────────────────────────────────────────────────────────
    public bool CanMoveTo(Vector3Int position)
    {
        if (HasStatus(EffectStatusType.Stunned) || HasStatus(EffectStatusType.Rooted))
            return false;

        return !movementComponent.IsMoving
            && movedPerTurn < characterClassSO.movementAmountPerTurn
            && movementComponent.CanMoveTo(position);
    }

    public void MoveTo(Vector3Int targetPosition, Action onComplete = null)
    {
        if (!CanMoveTo(targetPosition))
        {
            onComplete?.Invoke();
            return;
        }

        Vector3 worldTargetPosition = GridManager.Instance.GridToWorld(targetPosition);
        OnAnyUnitStartMoving?.Invoke(this, worldTargetPosition);

        movementComponent.MoveTo(
            targetPosition,
            onComplete: () =>
            {
                movedPerTurn++;                          // Only increments on successful completion
                OnAnyUnitFinishedMoving?.Invoke(this);
                onComplete?.Invoke();
            },
            onFailed: () =>
            {
                onComplete?.Invoke();                    // Caller still gets notified on failure
            });
    }

    public void SetMovingState(bool isMoving) => movementComponent.SetMovingState(isMoving);

    public void OnGridPositionChanged(Vector3Int newGridPosition)
    {
        OnUnitMadeAction?.Invoke();
        Debug.Log($"Unit moved to: {newGridPosition}");
    }

    // ── Health ────────────────────────────────────────────────────────────────
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
        => healthComponent.SetHealth(currentHealth, maxHealth);

    private void HandleDeath() => SetUnitDead(true);

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
        Debug.Log($"{gameObject.name} has been resurrected.");
    }

    // ── Abilities ─────────────────────────────────────────────────────────────
    public bool CanUseAbility(AbilityBaseSO abilitySO)
    {
        if (IsMoving)
        {
            Debug.Log("Unit is currently moving");
            return false;
        }
        if (HasStatus(EffectStatusType.Stunned))
        {
            Debug.Log($"Ability blocked: unit is Stunned. [{abilitySO.name}]");
            return false;
        }

        if (!characterClassSO.abilities.Contains(abilitySO))
        {
            Debug.Log($"Ability not found in class abilities. [{abilitySO.name}]");
            return false;
        }

        if (abilitySO.MaxUses <= usedAbilitiesAmountPerTurn[abilitySO])
        {
            Debug.Log($"Ability usage limit reached this round. $[{abilitySO.name}]");
            return false;
        }

        return abilitySO.CanUse(this);
    }

    public void UseAbility(AbilityBaseSO abilitySO, Vector3Int targetPosition)
    {
        if (!CanUseAbility(abilitySO))
        {
            Debug.Log("Unit can't use ability.");
            return;
        }

        if (!abilitySO.IsValidTarget(gridPosition, targetPosition, this))
        {
            Debug.Log("Target is invalid.");
            return;
        }

        if (HasStatus(EffectStatusType.Provoked) && forcedTarget != null
            && forcedTarget.IsAlive && forcedTarget.GridPosition != targetPosition)
        {
            Debug.Log($"Unit is provoked. ForcedTarget: {forcedTarget.GridPosition}, Attempted: {targetPosition}");
            return;
        }

        OnAnyUnitCastingAbility?.Invoke(this, abilitySO);
        abilitySO.Execute(this, targetPosition, () => OnAnyUnitUsedAbility?.Invoke(this, abilitySO));
        usedAbilitiesAmountPerTurn[abilitySO]++;
        OnUnitMadeAction?.Invoke();
        CharacterSelectionController.Instance.ChangeState(CharacterSelectionController.Instance.noSelectionState);
    }

    public void ResetUsedAbilities()
    {
        foreach (var ability in usedAbilitiesAmountPerTurn.Keys.ToList())
            usedAbilitiesAmountPerTurn[ability] = 0;
    }

    public void CopyUsedAbilities(Dictionary<AbilityBaseSO, int> abilities)
    {
        usedAbilitiesAmountPerTurn.Clear();
        foreach (var kvp in abilities)
            usedAbilitiesAmountPerTurn[kvp.Key] = kvp.Value;
    }

    public void ResetUsedMovement() => movedPerTurn = 0;

    // ── Helpers / Pass-throughs ───────────────────────────────────────────────
    public static void InvokeUnitEnteredTile(Unit unit, Vector3Int tilePosition)
        => OnUnitEnteredTile?.Invoke(unit, tilePosition);

    /// <summary>
    /// Fires OnAnyUnitStartMoving without going through the gameplay movement path.
    /// Used by cinematic systems (e.g. entry animation) that move units visually
    /// without consuming movement points or touching the grid registry.
    /// </summary>
    public static void InvokeUnitStartMoving(Unit unit, Vector3 destination)
        => OnAnyUnitStartMoving?.Invoke(unit, destination);

    /// <summary>
    /// Fires OnAnyUnitFinishedMoving without going through the gameplay movement path.
    /// Pair with InvokeUnitStartMoving for cinematic unit movement.
    /// </summary>
    public static void InvokeUnitFinishedMoving(Unit unit)
        => OnAnyUnitFinishedMoving?.Invoke(unit);

    public Transform GetHealthBarAttachPoint() => healthBarAttachPoint;
    public SpriteRenderer GetSpriteRenderer() => spriteRenderer;
    public UnitVisualBridge GetUnitVisualBridge() => unitVisualBridge;
}