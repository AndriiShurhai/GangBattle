using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Showcase/Wait")]
public class WaitCommandSO : ShowcaseCommandSO
{
    public float duration = 1f;
    public override IEnumerator Execute(MainMenuShowcaseContext ctx)
    {
        yield return new WaitForSeconds(duration);
    }
}