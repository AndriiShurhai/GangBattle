using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Showcase/JumpAttack")]
public class JumpAttackCommandSO : ShowcaseCommandSO
{
    [FormerlySerializedAs("AttackerID")]
    [SerializeField] private string _attackerId;

    [FormerlySerializedAs("TargetID")]
    [SerializeField] private string _targetId;

    [FormerlySerializedAs("JumpHeight")]
    [SerializeField] private float _jumpHeight;

    [FormerlySerializedAs("Duration")]
    [SerializeField] private float _duration;

    public override IEnumerator Execute(MainMenuShowcaseContext ctx)
    {
        var attacker = ctx.Actors[_attackerId];
        var target = ctx.Actors[_targetId];

        Vector3 originalPos = attacker.transform.position;
        Vector3 dir = target.transform.position - attacker.transform.position;
        Vector3 offset = new Vector3(dir.x > 0 ? -1.5f : 1.5f, 0, 0);

        var seq = DOTween.Sequence();

        seq.Append(attacker.transform.DOJump(target.transform.position + offset, _jumpHeight, 1, _duration));

        float attackMoment = _duration * 0.5f;
        float takeDamageMoment = _duration * 0.7f;   

        seq.InsertCallback(attackMoment, () => attacker.Play(PlayerState.ATTACK));
        seq.InsertCallback(takeDamageMoment, () => target.Play(PlayerState.DAMAGED));

        seq.AppendInterval(0.35f);
        seq.Append(attacker.transform.DOJump(originalPos, _jumpHeight, 1, _duration));

        yield return seq.WaitForCompletion();
    }
}
