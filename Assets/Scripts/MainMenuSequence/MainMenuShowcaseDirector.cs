using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuShowcaseDirector : MonoBehaviour
{
    [SerializeField] private List<ShowcaseSequenceSO> sequences;

    private void Start()
    {
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        while (true)
        {
            foreach (var seq in sequences)
                yield return PlaySequence(seq);
        }
    }

    private IEnumerator PlaySequence(ShowcaseSequenceSO seq)
    {
        var ctx = new MainMenuShowcaseContext();
        ctx.Actors = new Dictionary<string, MenuActor>();
        ctx.Runner = this;

        foreach (var cmd in seq.commands)
            yield return cmd.Execute(ctx);
    }
}