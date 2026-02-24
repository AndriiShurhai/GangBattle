using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Showcase/Face")]
public class FaceCommandSO : ShowcaseCommandSO
{
    public string actor;
    public Vector3 targetPosition; // face toward another actor
    public override IEnumerator Execute(MainMenuShowcaseContext ctx)
    {
        ctx.Actors[actor].Face(targetPosition);
        yield break;
    }
}