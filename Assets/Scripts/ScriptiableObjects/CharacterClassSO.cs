using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Character Class", menuName = "Classes/Character Class")]
public class CharacterClassSO : ScriptableObject
{
    [Header("Class Information")]
    public string className;
    [TextArea] public string description;
    public GameObject classIconPrefab;

    [Header("Base Stats")]
    public int maxHealth = 100;
    public int movementRange = 2;
    public int movementAmountPerTurn = 2;

    [Header("Combat stats")]
    public int strength = 10;
    public int intelligence = 10;
    public int agility = 10;

    [Header("Abilites")]
    public List<AbilityBaseSO> abilities;
}
