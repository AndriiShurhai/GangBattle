using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName = "Showcase/JumpAttack")]
public class JumpAttackCommandSO : ShowcaseCommandSO
{
    public string AttackerID;
    public string TargetID;
    public float JumpHeight;
    public float Duration;
    public override IEnumerator Execute(MainMenuShowcaseContext ctx)
    {
        var attacker = ctx.Actors[AttackerID];
        var target = ctx.Actors[TargetID];

        Vector3 originalPos = attacker.transform.position;
        Vector3 dir = target.transform.position - attacker.transform.position;
        Vector3 offset = new Vector3(dir.x > 0 ? -1.5f : 1.5f, 0, 0);

        var seq = DOTween.Sequence();

        seq.Append(attacker.transform.DOJump(target.transform.position + offset, JumpHeight, 1, Duration));

        float attackMoment = Duration * 0.5f;
        float takeDamageMoment = Duration * 0.7f;   

        seq.InsertCallback(attackMoment, () => attacker.Play(PlayerState.ATTACK));
        seq.InsertCallback(takeDamageMoment, () => target.Play(PlayerState.DAMAGED));

        seq.AppendInterval(0.35f);
        seq.Append(attacker.transform.DOJump(originalPos, JumpHeight, 1, Duration));

        yield return seq.WaitForCompletion();
    }
}
