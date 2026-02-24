using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class MenuActor : MonoBehaviour
{
    [SerializeField] private UnitVisualBridge unitVisualBridge;
    
    public IEnumerator MoveTo(Vector3 targetPosition, float duration)
    {
        yield return transform.DOMove(targetPosition, duration).WaitForCompletion();
    }

    public void Play(PlayerState playerState)
    {
        unitVisualBridge.PlayAnimation(playerState);
    }

    public void Face(Vector3 targetPosition)
    {
        if (targetPosition.x < transform.position.x)
        {
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
        else
        {
            transform.localScale = new Vector3(-(Mathf.Abs(transform.localScale.x)), transform.localScale.y, transform.localScale.z);
        }
    }
}   
