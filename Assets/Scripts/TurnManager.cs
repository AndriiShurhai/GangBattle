using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.UI;
using System;
using System.Linq;

public class  TurnManagerSnaphsotState
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

    public List<Transform> playersPositions;
    public List<Transform> enemiesPositions;

    public List<Unit> playerUnits;
    public List<Unit> enemyUnits;

    public string RewindID => id.ID;

    [SerializeField] private CharacterSelectionController characterSelectionController;
    [SerializeField] private Button endPlayerTurnButton;
    public enum TurnState { PlayerTurn, EnemyTurn }
    private TurnState currentState;

    private bool isGameOver;
    private int currentTurn = -1;

    private RewindableID id;

    private List<Unit> allPlayerUnits = new();
    private List<Unit> allEnemyUnits = new();
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        endPlayerTurnButton.onClick.AddListener(() =>
        {
            EndTurn();
        });

        id = GetComponent<RewindableID>();
    }

    private void Unit_OnUnitMadeAction()
    {
    }

    private void Start()
    {
        RegisterSelf();

        for (int i = 0; i < enemyUnits.Count; i++)
        {
            GameObject enemy = Instantiate(enemyUnits[i].gameObject);
            Unit enemyScript = enemy.GetComponent<Unit>();
            enemyScript.Initialize();
            enemyScript.PlaceUnit(enemiesPositions[i].position);
            enemyScript.OnUnitMadeAction += Unit_OnUnitMadeAction;

            enemyUnits[i] = enemyScript;
            allEnemyUnits.Add(enemyScript);
        }

        for (int i = 0; i < playerUnits.Count; i++)
        {
            GameObject player = Instantiate(playerUnits[i].gameObject);
            Unit playerScript = player.GetComponent<Unit>();
            playerScript.Initialize();
            playerScript.PlaceUnit(playersPositions[i].position);
            playerScript.OnUnitMadeAction += Unit_OnUnitMadeAction;

            playerUnits[i] = playerScript;
            allPlayerUnits.Add(playerScript);
        }

        foreach (var playerUnit in allPlayerUnits)
        {
            playerUnit.OnUnitDied += Unit_OnUnitDied;
        }

        foreach (var enemyUnit in allEnemyUnits)
        {
            enemyUnit.OnUnitDied += Unit_OnUnitDied;
        }

        StartPlayerTurn();
    }

    public object CaptureState()
    {
        TurnManagerSnaphsotState currentState = new TurnManagerSnaphsotState
        {
            currentState = this.currentState,
            currentTurn = this.currentTurn,
            isGameOver = this.isGameOver,

            alivePlayerUnitIds = playerUnits.Select(u => u.RewindID).ToList(),
            aliveEnemyUnitIds = enemyUnits.Select(u => u.RewindID).ToList()
        };

        return currentState;
    }

    public void RestoreState(object state)
    {
        var s = (TurnManagerSnaphsotState)state;

        this.currentState = s.currentState;
        this.currentTurn = s.currentTurn;
        this.isGameOver = s.isGameOver;

        playerUnits.Clear();
        foreach (string unitId in s.alivePlayerUnitIds)
        {
            Unit unit = allPlayerUnits.Find(u => u.RewindID == unitId);
            if (unit != null)
            {
                playerUnits.Add(unit);
            }
        }

        enemyUnits.Clear();
        foreach (string unitId in s.aliveEnemyUnitIds)
        {
            Unit unit = allEnemyUnits.Find(u => u.RewindID == unitId);
            if (unit != null)
            {
                enemyUnits.Add(unit);
            }
        }

        if (currentState == TurnState.PlayerTurn)
        {
            endPlayerTurnButton.gameObject.SetActive(true);
            if (characterSelectionController != null)
                characterSelectionController.gameObject.SetActive(true);
        }
        else
        {
            endPlayerTurnButton.gameObject.SetActive(false);
            if (characterSelectionController != null)
                characterSelectionController.gameObject.SetActive(false);
        }

        Debug.Log("Restore in the Manager has been called");
    }

    public void RegisterSelf()
    {
        RewindManager.Instance.RegisterRewindable(this);
    }
    private void SaveTurn()
    {
        currentTurn++;
        RewindManager.Instance.SaveTurn(currentTurn);
    }

    private void RewindTo(int turnIndex)
    {
        RewindManager.Instance.RewindTo(turnIndex);
    }

    public void RewindOneStep()
    {
        if (currentTurn < 0)
        {
            Debug.Log("Already at the beginning, cannot rewind");
            return;
        }

        var availableTurns = RewindManager.Instance.GetAvailableTurns();

        if (availableTurns.Count == 0)
        {
            Debug.Log("No snapshots available to rewind");
            return;
        }

        int targetTurn = -1;
        for (int i = currentTurn - 1; i >= 0; i--)
        {
            if (availableTurns.Contains(i))
            {
                targetTurn = i;
                break;
            }
        }

        if (targetTurn == -1)
        {
            Debug.Log("No previous snapshot found");
            RewindManager.Instance.RewindTo(currentTurn);
            return;
        }

        Debug.Log($"Rewinding from turn {currentTurn} to turn {targetTurn}");
        RewindManager.Instance.RewindTo(targetTurn);
    }

    public void RewindToCurrentTurn()
    {
        Debug.Log($"Reset current turn has been called. Current turn: {currentTurn}");
        RewindManager.Instance.RewindTo(currentTurn);
    }
    private void Unit_OnUnitDied(Unit unit)
    {
        if (isGameOver) return;

        if (playerUnits.Contains(unit))
        {
            playerUnits.Remove(unit);  
            
            if (playerUnits.Count == 0)
            {
                Debug.Log("enemies win");
                isGameOver = true;
                EndGame();
            }
        }

        else if(enemyUnits.Contains(unit))
        {
            enemyUnits.Remove(unit);

            if (enemyUnits.Count == 0)
            {
                Debug.Log("player wins");
                isGameOver = true;
                EndGame();
            }
        }
    }

    public void StartPlayerTurn()
    {
        currentState = TurnState.PlayerTurn;
        Debug.Log("--- PLAYER TURN START ---");

        if (characterSelectionController != null)
        {
            Debug.Log("enabling character selection controller");
            characterSelectionController.gameObject.SetActive(true);
        }

        foreach (Unit unit in playerUnits)
        {
            unit.ResetUsedAbilities();
            unit.HasTakenActionThisTurn = false;
            unit.ResetUsedMovement();
        }
        SaveTurn();
    }

    public void StartEnemyTurn()
    {
        currentState = TurnState.EnemyTurn;
        Debug.Log("--- ENEMY TURN START ---");

        if (characterSelectionController != null)
        {
            Debug.Log("Disabling character selection controller");
            characterSelectionController.gameObject.SetActive(false);
        }

        foreach (Unit unit in enemyUnits)
        {
            unit.HasTakenActionThisTurn = false;
        }

        StartCoroutine(EnemyTurnRoutine());
    }
    public void EndTurn()
    {
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


        List<Trap> traps = new List<Trap> (TrapRegistry.Instance.GetTraps());
        foreach (Trap trap in traps)
        {
            trap.DecreaseDuration();
        }
    }

    private IEnumerator EnemyTurnRoutine()
    {
        List<Unit> enemiesToTakeTurn = new List<Unit>(enemyUnits);

        foreach (Unit enemyUnit in enemiesToTakeTurn)
        {
            if (enemyUnits.Contains(enemyUnit))
            {
                bool isTurnComplete = false;

                enemyUnit.GetComponent<AIBrain>()?.TakeTurn(() =>
                {
                    isTurnComplete = true;
                });

                yield return new WaitUntil(() => isTurnComplete);

                if (enemyUnit != null) enemyUnit.HasTakenActionThisTurn = true;
            }
        }

        Debug.Log("--- ENEMY TURN END ---");

        EndTurn();
    }

    private void EndGame()
    {
        StopAllCoroutines();

        characterSelectionController.ClearSelection();

        characterSelectionController.gameObject.SetActive(false);

        // show a victory or defeat screen or something
    }

    public List<Unit> GetPlayerUnits()
    {
        return playerUnits;
    }

    public List<Unit> GetEnemyUnits()
    {
        return enemyUnits;
    }

    public List<Unit> GetAllUnits()
    {
        List<Unit> allUnits = new List<Unit>();
        allUnits.AddRange(playerUnits);
        allUnits.AddRange(enemyUnits);

        return allUnits;
    }

    public List<Unit> GetAlivePlayerUnits()
    {
        return playerUnits.Where(unit => unit.IsAlive).ToList();
    }

    public List<Unit> GetAliveEnemyUnits()
    {
        return enemyUnits.Where(unit => unit.IsAlive).ToList(); 
    }
}
