using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RewindManager : MonoBehaviour
{
    public static RewindManager Instance { get; private set; }

    [SerializeField] private int maxHistorySize = 100;

    private readonly Dictionary<int, TurnSnapshot> history = new();
    private readonly List<IRewindable> rewindables = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SaveTurn(int turnIndex)
    {
        var snapshot = new TurnSnapshot { turnIndex = turnIndex };

        foreach (var r in rewindables)
        {
            if (r == null) continue;
            snapshot.objectStates[r.RewindID] = r.CaptureState();
        }

        // Overwrite any existing snapshot for this turn — always keep the latest version.
        history[turnIndex] = snapshot;

        if (history.Count > maxHistorySize)
        {
            int oldest = history.Keys.Min();
            history.Remove(oldest);
            Debug.Log($"Removed oldest snapshot (turn {oldest}). History size: {history.Count}");
        }

        Debug.Log($"Saved turn {turnIndex}. Total snapshots: {history.Count}");
    }

    public void RewindTo(int turnIndex)
    {
        if (!history.TryGetValue(turnIndex, out var snapshot))
        {
            Debug.LogError($"No snapshot found for turn {turnIndex}");
            return;
        }

        Debug.Log($"Rewinding to turn {turnIndex}");

        rewindables.RemoveAll(r => r == null);

        foreach (var r in new List<IRewindable>(rewindables))
        {
            if (snapshot.objectStates.TryGetValue(r.RewindID, out var state))
            {
                r.RestoreState(state);
            }
            else
            {
                // This rewindable didn't exist at this turn (e.g. a trap placed mid-turn).
                // Restore it to its declared deactivated state and drop it from tracking.
                Debug.LogWarning($"No state for {r.RewindID} at turn {turnIndex} — restoring as deactivated.");
                r.RestoreState(r.CaptureDeactivatedState());
                rewindables.Remove(r);
            }
        }
    }

    public void RegisterRewindable(IRewindable rewindable)
    {
        if (rewindable == null)
        {
            Debug.LogError("Attempted to register null rewindable.");
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
        if (rewindable == null) return;
        rewindables.Remove(rewindable);
        Debug.Log($"Unregistered rewindable: {rewindable.RewindID}");
    }

    public List<int> GetAvailableTurns() => history.Keys.ToList();
    public int GetHistoryCount() => history.Count;
    public void ClearHistory() { history.Clear(); Debug.Log("History cleared."); }
}