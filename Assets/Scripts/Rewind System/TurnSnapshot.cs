using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TurnSnapshot
{
    public int turnIndex;
    public Dictionary<string, object> objectStates = new();
}
