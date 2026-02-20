using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

[CreateAssetMenu(fileName = "FireballAttackAbilitySO", menuName = "Abilities/Fireball Attack Ability")]
public class FireballAttackAbilitySO : AbilityBaseSO
{
    public GameObject projectile;
    public int explosionRadius = 1;
    public float coefficent = 1f;

    public override void Execute(Unit caster, Vector3Int targetPosition, Action onAbilityInvoke = null)
    {
        Vector3 startPosition = caster.transform.position;
        Vector3 targetWorldPosition = GridManager.Instance.GridToWorld(targetPosition);

        List<Unit> unitsInRange = new List<Unit>();

        List<Vector3Int> area = RangeFinder.GetSquareRange(targetPosition, explosionRadius);
        foreach (Vector3Int position in area)
        {
            if (!IsValidTarget(caster.GridPosition, position, caster)) continue;
            IGridObject obj = GridObjectRegistry.Instance.GetObjectAt(position);
            if (obj is Unit unit) unitsInRange.Add(unit);
        }

        int damage = GetPower(caster);

        Sequence attackSequence = DOTween.Sequence();

        GameObject projectileGameObject = Instantiate(projectile, caster.transform.position, Quaternion.identity);

        attackSequence.Append(projectileGameObject.transform.DOJump(targetWorldPosition, 0.5f, 1, 0.3f));
        attackSequence.AppendCallback(() =>
        {
            onAbilityInvoke?.Invoke();

            foreach(Unit unit in unitsInRange)
            {
                unit.TakeDamage(damage, caster);
            }
            Destroy(projectileGameObject);

            Camera.main.transform.DOShakePosition(0.5f, 0.5f);
            // Spawn effect if available
            if (abilityEffectPrefab != null)
            {
                Vector3 worldPos = GridManager.Instance.GridToWorld(targetPosition);
                GameObject effect = Instantiate(abilityEffectPrefab, worldPos, Quaternion.identity);
                effect.GetComponentInChildren<Animator>().SetTrigger("collide");

                effect.transform.DOScale(1.8f, 0.5f).SetEase(Ease.OutBack);

                effect.GetComponentInChildren<SpriteRenderer>().DOFade(0, 1.5f).SetDelay(0.5f).OnComplete(() =>
                {
                    Destroy(effect);
                });
            }
        });
    }

    public override List<Vector3Int> GetAbilityRadiusTiles(Vector3Int targetPosition)
    {
        return RangeFinder.GetSquareRange(targetPosition, explosionRadius);
    }
  
}
