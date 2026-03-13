using DG.Tweening;
using TMPro;
using UnityEngine;

public class HpPopupText : MonoBehaviour
{
    [SerializeField] private TMP_Text textMesh;
    [SerializeField] private float moveYOffset = 1f;
    [SerializeField] private float moveYDuration = 1.5f;
    [SerializeField] private float fadeDuration = 2f;

    public void Setup(int amount, Color color)
    {
        textMesh.text = amount.ToString();
        textMesh.color = color;
        transform.DOMoveY(transform.position.y + moveYOffset, moveYDuration);
        textMesh.DOFade(0, fadeDuration).OnComplete(() => Destroy(gameObject));

    }
}
