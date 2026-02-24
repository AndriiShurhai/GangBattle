using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Showcase/PlayAnimation")]
public class PlayAnimationCommandSO : ShowcaseCommandSO
{
    public string actor;
    public PlayerState state;
    public override IEnumerator Execute(MainMenuShowcaseContext ctx)
    {
        ctx.Actors[actor].Play(state);
        yield break;
    }
}