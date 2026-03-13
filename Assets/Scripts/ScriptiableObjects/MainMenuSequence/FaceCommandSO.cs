using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Showcase/Face")]
public class FaceCommandSO : ShowcaseCommandSO
{
    [FormerlySerializedAs("actor")]
    [SerializeField] private string _actorId;

    [FormerlySerializedAs("targetPosition")]
    [SerializeField] private Vector3 _targetPosition; // face toward another actor

    public override IEnumerator Execute(MainMenuShowcaseContext ctx)
    {
        ctx.Actors[_actorId].Face(_targetPosition);
        yield break;
    }
}