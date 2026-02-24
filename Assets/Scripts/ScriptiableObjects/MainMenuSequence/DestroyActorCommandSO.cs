using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Showcase/Destroy")]
public class DestroyActorCommandSO : ShowcaseCommandSO
{
    public string actor;
    public override IEnumerator Execute(MainMenuShowcaseContext ctx)
    {
        Destroy(ctx.Actors[actor].gameObject);
        ctx.Actors.Remove(actor);
        yield break;
    }
}