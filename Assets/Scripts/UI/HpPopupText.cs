using DG.Tweening;
using TMPro;
using UnityEngine;

public class HpPopupText : MonoBehaviour
{
    [SerializeField] private TMP_Text textMesh;

    public void Setup(int amount, Color color)
    {
        textMesh.text = amount.ToString();
        textMesh.color = color;
        transform.DOMoveY(transform.position.y + 1f, 1f);
        textMesh.DOFade(0, 1f).OnComplete(() => Destroy(gameObject));

    }
}
