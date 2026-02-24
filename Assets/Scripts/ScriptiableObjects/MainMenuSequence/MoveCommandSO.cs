using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Showcase/Move")]
public class MoveCommandSO : ShowcaseCommandSO
{
    public string actor;
    public Vector3 position;
    public float duration = 0.6f;

    public override IEnumerator Execute(MainMenuShowcaseContext ctx)
    {
        yield return ctx.Actors[actor].MoveTo(position, duration);
    }
}