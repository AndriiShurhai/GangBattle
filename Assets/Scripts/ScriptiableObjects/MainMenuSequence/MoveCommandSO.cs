using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Showcase/Move")]
public class MoveCommandSO : ShowcaseCommandSO
{
    [FormerlySerializedAs("actor")]
    [SerializeField] private string _actorId;

    [FormerlySerializedAs("position")]
    [SerializeField] private Vector3 _position;

    [FormerlySerializedAs("duration")]
    [SerializeField] private float _duration = 0.6f;

    public override IEnumerator Execute(MainMenuShowcaseContext ctx)
    {
        yield return ctx.Actors[_actorId].MoveTo(_position, _duration);
    }
}