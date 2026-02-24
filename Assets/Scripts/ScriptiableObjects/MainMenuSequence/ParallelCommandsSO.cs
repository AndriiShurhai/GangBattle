using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Showcase/Parallel")]
public class ParallelCommandSO : ShowcaseCommandSO
{
    public List<ShowcaseCommandSO> commands;

    public override IEnumerator Execute(MainMenuShowcaseContext ctx)
    {
        var coroutines = commands.Select(c => ctx.Runner.StartCoroutine(c.Execute(ctx))).ToList();
        foreach (var co in coroutines)
            yield return co;
    }
}