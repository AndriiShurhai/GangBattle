using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RewindManager : MonoBehaviour
{
    public static RewindManager Instance;

    [SerializeField] private int maxHistorySize = 100;

    private List<TurnSnapshot> history = new();
    private List<IRewindable> rewindables = new();
    private int currentTurn = 0;

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
    }

    public void SaveTurn(int currentTurn)
    {
        var snapshot = new TurnSnapshot
        {
            turnIndex = currentTurn,
        };

        foreach (var r in rewindables)
        {
            if (r == null) continue;

            if (!snapshot.objectStates.ContainsKey(r.RewindID))
            {
                snapshot.objectStates.Add(r.RewindID, r.CaptureState());
            }
            else
            {
                snapshot.objectStates[r.RewindID] = r.CaptureState();
            }
        }

        var previousSnapshotOnCurrentTurn = history.FirstOrDefault(r => r.turnIndex == currentTurn);
        if (previousSnapshotOnCurrentTurn != null)
        {
            history.Remove(previousSnapshotOnCurrentTurn);
        }
        history.Add(snapshot);

        if (history.Count > maxHistorySize)
        {
            history.RemoveAt(0);
            Debug.Log($"Removed oldest snapshot. History size: {history.Count}");
        }

        this.currentTurn = currentTurn;
        Debug.Log($"Saved turn {currentTurn}. Total snapshots: {history.Count}");
    }

    public void RewindTo(int turnIndex)
    {
        var snapshot = history.FirstOrDefault(r => r.turnIndex == turnIndex);

        if (snapshot == null)
        {
            Debug.LogError($"No snapshot found for turn {turnIndex}");
            return;
        }

        Debug.Log($"Rewinding to turn {turnIndex}");

        rewindables.RemoveAll(r => r == null);

        var rewindablesCopy= new List<IRewindable>(rewindables);

        foreach (var r in rewindablesCopy)
        {
            if (snapshot.objectStates.TryGetValue(r.RewindID, out var state))
            {
                r.RestoreState(state);
            }
            else
            {
                Debug.LogWarning($"No state found for {r.RewindID} in turn {turnIndex}");
                if (r is Trap trap)
                {
                    var deactivatedState = new TrapSnapshotState
                    {
                        gridPosition = trap.GridPosition,
                        remainingDuration = 0,
                        isActive = false,
                        roundSinceRegister = 0
                    };
                    trap.RestoreState(deactivatedState);

                    rewindables.Remove(trap);
                }
            }
        }

        this.currentTurn = turnIndex;
    }

    public void RegisterRewindable(IRewindable rewindable)
    {
        if (rewindable == null)
        {
            Debug.LogError("Attempted to register null rewindable");
            return;
        }

        if (!rewindables.Contains(rewindable))
        {
            rewindables.Add(rewindable);
            Debug.Log($"Registered rewindable: {rewindable.RewindID}");
        }
    }

    public void UnregisterRewindable(IRewindable rewindable)
    {
        if (rewindable != null)
        {
            rewindables.Remove(rewindable);
            Debug.Log($"Unregistered rewindable: {rewindable.RewindID}");
        }
    }

    public int GetHistoryCount()
    {
        return history.Count;
    }

    public List<int> GetAvailableTurns()
    {
        return history.Select(h => h.turnIndex).ToList();
    }

    public void ClearHistory()
    {
        history.Clear();
        Debug.Log("History cleared");
    }
}