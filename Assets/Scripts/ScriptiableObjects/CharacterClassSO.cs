using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Character Class", menuName = "Classes/Character Class")]
public class CharacterClassSO : ScriptableObject
{
    [Header("Class Information")]
    public string className;
    [TextArea] public string description;
    public Sprite classIcon;

    [Header("Base Stats")]
    public int maxHealth = 100;
    public int movementRange = 2;

    [Header("Abilites")]
    public List<AbilityBaseSO> abilities;
}
