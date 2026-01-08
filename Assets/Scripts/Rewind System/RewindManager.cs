using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RewindManager : MonoBehaviour
{
    public static RewindManager Instance;
    private List<TurnSnapshot> history = new();
    private List<IRewindable> rewindables = new();

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
            if (!snapshot.objectStates.ContainsKey(r.RewindID))
            {
                snapshot.objectStates.Add(r.RewindID, r.CaptureState());
            }
            else
            {
                snapshot.objectStates[r.RewindID] = r.CaptureState();
            }
        }

        history.Add(snapshot);
    }

    public void RewindTo(int turnIndex)
    {
        var snapshot = history.First(r => r.turnIndex == turnIndex);

        foreach (var r in rewindables)
        {
            if (snapshot.objectStates.TryGetValue(r.RewindID, out var state))
            {
                r.RestoreState(state);
            }
        }
    }

    public void RegisterRewindable(IRewindable rewindable)
    {
        rewindables.Add(rewindable);
    }

    public void UnregisterRewindable(IRewindable rewindable)
    {
        rewindables.Remove(rewindable);
    }
}
