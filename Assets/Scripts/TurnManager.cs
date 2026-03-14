using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Linq;

public class TurnManagerSnapshotState
{
    public int currentTurn;
    public TurnManager.TurnState currentState;
    public bool isGameOver;

    public List<string> alivePlayerUnitIds;
    public List<string> aliveEnemyUnitIds;
}

public class TurnManager : MonoBehaviour, IRewindable
{
    public static TurnManager Instance { get; private set; }

    [SerializeField] private List<Transform> playersPositions;
    [SerializeField] private List<Transform> enemiesPositions;

    [SerializeField] private List<Unit> playerUnits;
    [SerializeField] private List<Unit> enemyUnits;

    [SerializeField] private CharacterSelectionController characterSelectionController;
    [SerializeField] private Button endPlayerTurnButton;
    [SerializeField] private float enemyPreTurnDelay = 0.3f;
    [SerializeField] private float enemyPostTurnDelay = 0.5f;

    public string RewindID => id.ID;

    public enum TurnState { PlayerTurn, EnemyTurn }

    private TurnState currentState;
    private bool isGameOver;
    private int currentTurn = -1;
    private RewindableID id;

    private List<Unit> allPlayerUnits = new();
    private List<Unit> allEnemyUnits = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        endPlayerTurnButton.onClick.AddListener(EndTurn);
        id = GetComponent<RewindableID>();
    }

    private void Start()
    {
        RegisterSelf();

        for (int i = 0; i < enemyUnits.Count; i++)
        {
            Unit enemyScript = Instantiate(enemyUnits[i].gameObject).GetComponent<Unit>();
            enemyScript.Initialize();
            enemyScript.PlaceUnit(enemiesPositions[i].position);

            enemyUnits[i] = enemyScript;
            allEnemyUnits.Add(enemyScript);
        }

        for (int i = 0; i < playerUnits.Count; i++)
        {
            Unit playerScript = Instantiate(playerUnits[i].gameObject).GetComponent<Unit>();
            playerScript.Initialize();
            playerScript.PlaceUnit(playersPositions[i].position);

            playerUnits[i] = playerScript;
            allPlayerUnits.Add(playerScript);
        }

        foreach (var unit in allPlayerUnits) unit.OnUnitDied += Unit_OnUnitDied;
        foreach (var unit in allEnemyUnits) unit.OnUnitDied += Unit_OnUnitDied;

        StartPlayerTurn();
    }

    // ── Rewind ────────────────────────────────────────────────────────────────

    public void RegisterSelf() => RewindManager.Instance.RegisterRewindable(this);

    public object CaptureState()
    {
        return new TurnManagerSnapshotState
        {
            currentState = this.currentState,
            currentTurn = this.currentTurn,
            isGameOver = this.isGameOver,
            alivePlayerUnitIds = playerUnits.Select(u => u.RewindID).ToList(),
            aliveEnemyUnitIds = enemyUnits.Select(u => u.RewindID).ToList()
        };
    }

    public object CaptureDeactivatedState() => CaptureState();

    public void RestoreState(object state)
    {
        var s = (TurnManagerSnapshotState)state;

        this.currentState = s.currentState;
        this.currentTurn = s.currentTurn;
        this.isGameOver = s.isGameOver;

        playerUnits.Clear();
        foreach (string unitId in s.alivePlayerUnitIds)
        {
            Unit unit = allPlayerUnits.Find(u => u.RewindID == unitId);
            if (unit != null) playerUnits.Add(unit);
        }

        enemyUnits.Clear();
        foreach (string unitId in s.aliveEnemyUnitIds)
        {
            Unit unit = allEnemyUnits.Find(u => u.RewindID == unitId);
            if (unit != null) enemyUnits.Add(unit);
        }

        bool isPlayerTurn = currentState == TurnState.PlayerTurn;
        endPlayerTurnButton.gameObject.SetActive(isPlayerTurn);
        if (characterSelectionController != null)
            characterSelectionController.gameObject.SetActive(isPlayerTurn);

        Debug.Log($"TurnManager state restored to turn {s.currentTurn}.");
    }

    private void SaveTurn()
    {
        currentTurn++;
        RewindManager.Instance.SaveTurn(currentTurn);
    }

    public void RewindOneStep()
    {
        CharacterSelectionController.Instance.ClearSelection();
        if (currentTurn < 0)
        {
            Debug.Log("Already at the beginning, cannot rewind.");
            return;
        }

        var availableTurns = RewindManager.Instance.GetAvailableTurns();

        if (availableTurns.Count == 0)
        {
            Debug.Log("No snapshots available to rewind.");
            return;
        }

        // Dictionary keys have no guaranteed order — sort descending to find the closest previous turn.
        availableTurns.Sort((a, b) => b.CompareTo(a));

        int targetTurn = -1;
        foreach (int turn in availableTurns)
        {
            if (turn < currentTurn)
            {
                targetTurn = turn;
                break;
            }
        }

        if (targetTurn == -1)
        {
            Debug.Log("No previous snapshot found.");
            return;
        }

        Debug.Log($"Rewinding from turn {currentTurn} to turn {targetTurn}.");

        StopAllCoroutines();
        StopAllEnemyCoroutines();
        RewindManager.Instance.RewindTo(targetTurn);
    }

    public void RewindToCurrentTurn()
    {
        Debug.Log($"Resetting current turn {currentTurn}.");
        CharacterSelectionController.Instance.ClearSelection();

        StopAllCoroutines();
        StopAllEnemyCoroutines();
        RewindManager.Instance.RewindTo(currentTurn);
    }

    private void StopAllEnemyCoroutines()
    {
        foreach (var unit in allEnemyUnits)
        {
            if (unit == null) continue;
            unit.GetComponent<AIBrain>()?.StopAllCoroutines();
            unit.StopAllCoroutines();
        }
    }
    // ── Turn Flow ─────────────────────────────────────────────────────────────

    public void StartPlayerTurn()
    {
        CharacterSelectionController.Instance.ClearSelection();
        currentState = TurnState.PlayerTurn;
        Debug.Log("--- PLAYER TURN START ---");

        if (characterSelectionController != null)
            characterSelectionController.gameObject.SetActive(true);

        foreach (Unit unit in GetAlivePlayerUnits())
        {
            unit.ResetUsedAbilities();
            unit.HasTakenActionThisTurn = false;
            unit.ResetUsedMovement();
            unit.UpdateEffectsStatus();
        }

        SaveTurn();
    }

    public void StartEnemyTurn()
    {
        CharacterSelectionController.Instance.ClearSelection();
        currentState = TurnState.EnemyTurn;
        Debug.Log("--- ENEMY TURN START ---");

        if (characterSelectionController != null)
            characterSelectionController.gameObject.SetActive(false);

        foreach (Unit unit in enemyUnits)
        {
            unit.ResetUsedAbilities();
            unit.HasTakenActionThisTurn = false;
            unit.ResetUsedMovement();
        }

        StartCoroutine(EnemyTurnRoutine());
    }

    public void EndTurn()
    {
        List<Trap> traps = new List<Trap>(TrapRegistry.Instance.GetTraps());
        foreach (Trap trap in traps)
            trap.DecreaseDuration();

        if (currentState == TurnState.PlayerTurn)
        {
            endPlayerTurnButton.gameObject.SetActive(false);
            StartEnemyTurn();
        }
        else if (currentState == TurnState.EnemyTurn)
        {
            endPlayerTurnButton.gameObject.SetActive(true);
            StartPlayerTurn();
        }
    }

    private IEnumerator EnemyTurnRoutine()
    {
        foreach (Unit unit in GetAliveEnemyUnits())
            unit.UpdateEffectsStatus();

        yield return new WaitForSeconds(enemyPreTurnDelay);

        List<Unit> enemiesToTakeTurn = new List<Unit>(enemyUnits);

        foreach (Unit enemyUnit in enemiesToTakeTurn)
        {
            if (!enemyUnits.Contains(enemyUnit)) continue;

            bool isTurnComplete = false;
            enemyUnit.GetComponent<AIBrain>()?.TakeTurn(() => isTurnComplete = true);

            yield return new WaitUntil(() => isTurnComplete);
            yield return new WaitForSeconds(enemyPostTurnDelay);

            if (enemyUnit != null) enemyUnit.HasTakenActionThisTurn = true;
        }

        Debug.Log("--- ENEMY TURN END ---");
        EndTurn();
    }

    private void EndGame(bool playerWon)
    {
        StopAllCoroutines();
        characterSelectionController.ClearSelection();
        characterSelectionController.gameObject.SetActive(false);

        if (playerWon)
        {
            GameOverScreenUI.Instance.ShowGameOverScreenCompletedLevel(
                enemiesDestroyed: allEnemyUnits.Count - enemyUnits.Count,
                unitsAlive: playerUnits.Count,
                timeTaken: currentTurn 
            );
            Debug.Log("Player wins the level!");
        }
        else
        {
            GameOverScreenUI.Instance.ShowGameOverScreenFailedLevel();
            Debug.Log("Enemies win the level!");
        }
    }

    // ── Unit Events ───────────────────────────────────────────────────────────

    private void Unit_OnUnitDied(Unit unit)
    {
        if (isGameOver) return;

        if (playerUnits.Contains(unit))
        {
            playerUnits.Remove(unit);
            if (playerUnits.Count == 0)
            {
                Debug.Log("Enemies win.");
                isGameOver = true;
                EndGame(false);
            }
        }
        else if (enemyUnits.Contains(unit))
        {
            enemyUnits.Remove(unit);
            if (enemyUnits.Count == 0)
            {
                Debug.Log("Player wins.");
                isGameOver = true;
                EndGame(true);
            }
        }
    }

    // ── Accessors ─────────────────────────────────────────────────────────────

    public List<Unit> GetPlayerUnits() => playerUnits;
    public List<Unit> GetEnemyUnits() => enemyUnits;

    public List<Unit> GetAllEnemyUnits() => allEnemyUnits;

    public List<Unit> GetAllPlayerUnits() => allPlayerUnits;

    public List<Unit> GetAllUnits()
    {
        var all = new List<Unit>(playerUnits);
        all.AddRange(enemyUnits);
        return all;
    }

    public List<Unit> GetAlivePlayerUnits() => playerUnits.Where(u => u.IsAlive).ToList();
    public List<Unit> GetAliveEnemyUnits() => enemyUnits.Where(u => u.IsAlive).ToList();

    internal int GetBestTimeForLevel()
    {
        return 100;
    }
}