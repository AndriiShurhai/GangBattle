using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class LightDefaultIntensity : MonoBehaviour
{
    public float defaultIntensity;

    void Awake()
    {
        defaultIntensity = GetComponent<Light2D>().intensity;
    }
}