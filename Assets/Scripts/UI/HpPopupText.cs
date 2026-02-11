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
        transform.DOMoveY(transform.position.y + 1f, 1.5f);
        textMesh.DOFade(0, 2f).OnComplete(() => Destroy(gameObject));

    }
}
