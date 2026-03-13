using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Showcase/Destroy")]
public class DestroyActorCommandSO : ShowcaseCommandSO
{
    [FormerlySerializedAs("actor")]
    [SerializeField] private string _actorId;

    public override IEnumerator Execute(MainMenuShowcaseContext ctx)
    {
        Destroy(ctx.Actors[_actorId].gameObject);
        ctx.Actors.Remove(_actorId);
        yield break;
    }
}