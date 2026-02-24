using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public abstract class ShowcaseCommandSO : ScriptableObject
{
    public abstract IEnumerator Execute(MainMenuShowcaseContext ctx);
}