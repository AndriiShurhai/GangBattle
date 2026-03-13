using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Showcase/Wait")]
public class WaitCommandSO : ShowcaseCommandSO
{
    [FormerlySerializedAs("duration")]
    [SerializeField] private float _duration = 1f;

    public override IEnumerator Execute(MainMenuShowcaseContext ctx)
    {
        yield return new WaitForSeconds(_duration);
    }
}