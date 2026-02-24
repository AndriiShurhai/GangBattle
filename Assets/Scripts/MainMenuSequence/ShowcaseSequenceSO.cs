using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Showcase/Sequence")]
public class ShowcaseSequenceSO : ScriptableObject
{
    public List<ShowcaseCommandSO> commands;
}