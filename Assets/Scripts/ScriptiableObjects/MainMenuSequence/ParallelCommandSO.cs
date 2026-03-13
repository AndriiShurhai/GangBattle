using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Showcase/Parallel")]
public class ParallelCommandSO : ShowcaseCommandSO
{
    [SerializeField] private List<ShowcaseCommandSO> _commands = new();

    public IReadOnlyList<ShowcaseCommandSO> Commands => _commands;

    public override IEnumerator Execute(MainMenuShowcaseContext ctx)
    {
        if (_commands == null || _commands.Count == 0)
        {
            yield break;
        }

        // 1. Kick off all valid commands in parallel
        var coroutines = _commands
            .Where(c => c != null)
            .Select(c => ctx.Runner.StartCoroutine(c.Execute(ctx)))
            .ToList();

        // 2. Wait for all of them to finish sequentially
        foreach (var co in coroutines)
        {
            yield return co;
        }
    }
}