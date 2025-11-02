using UnityEngine;
using System.Collections.Generic;
public class TrapRegistry : MonoBehaviour
{
    public static TrapRegistry Instance { get; private set; }

    [SerializeField] private List<Trap> registeredTraps;

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

    public void RegisterTrap(Trap trap)
    {
        if (registeredTraps.Contains(trap)) return;

        registeredTraps.Add(trap);
    }

    public void UnregisterTrap(Trap trap)
    {
        if (!registeredTraps.Contains(trap)) return;

        registeredTraps.Remove(trap);
    }

    public List<Trap> GetTraps()
    {
        return registeredTraps;
    }
}
