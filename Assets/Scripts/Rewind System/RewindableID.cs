using System;
using UnityEngine;

public class RewindableID : MonoBehaviour
{
    [SerializeField] private string id;

    public string ID => id;

    private void Awake()
    {
        if (string.IsNullOrEmpty(id))
        {
            id = Guid.NewGuid().ToString();
        }
    }
}
