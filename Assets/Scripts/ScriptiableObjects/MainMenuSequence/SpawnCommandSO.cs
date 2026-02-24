using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName = "Showcase/Spawn")]
public class SpawnCommandSO : ShowcaseCommandSO
{
    public string ActorID;
    public MenuActor prefab;
    public Vector3 position;
    public override IEnumerator Execute(MainMenuShowcaseContext ctx)
    {
        var actor = Instantiate(prefab, position, Quaternion.identity);
        ctx.Actors.Add(ActorID, actor);
        Debug.Log($"New Actor has been spawned: {ActorID}");
        yield break;
    }
}   
