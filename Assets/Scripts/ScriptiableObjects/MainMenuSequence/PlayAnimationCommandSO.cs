using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Showcase/PlayAnimation")]
public class PlayAnimationCommandSO : ShowcaseCommandSO
{
    [FormerlySerializedAs("actor")]
    [SerializeField] private string _actorId;

    [FormerlySerializedAs("state")]
    [SerializeField] private PlayerState _state;

    public override IEnumerator Execute(MainMenuShowcaseContext ctx)
    {
        ctx.Actors[_actorId].Play(_state);
        yield break;
    }
}