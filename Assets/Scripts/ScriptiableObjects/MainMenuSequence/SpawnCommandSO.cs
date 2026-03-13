using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Showcase/Spawn")]
public class SpawnCommandSO : ShowcaseCommandSO
{
    [FormerlySerializedAs("ActorID")]
    [SerializeField] private string _actorId;

    [FormerlySerializedAs("prefab")]
    [SerializeField] private MenuActor _prefab;

    [FormerlySerializedAs("position")]
    [SerializeField] private Vector3 _position;

    public override IEnumerator Execute(MainMenuShowcaseContext ctx)
    {
        var actor = Instantiate(_prefab, _position, Quaternion.identity);
        ctx.Actors.Add(_actorId, actor);
        Debug.Log($"New Actor has been spawned: {_actorId}");
        yield break;
    }
}
