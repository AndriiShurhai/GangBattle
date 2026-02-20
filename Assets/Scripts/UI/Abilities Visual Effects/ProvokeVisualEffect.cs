using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ProvokeVisualEffect : MonoBehaviour
{
    [SerializeField] private Light2D light2D;

    private float outerLightTarget;
    private float innerLightTarget;

    public void Execute(float range)
    {
        outerLightTarget = range * 1.5f;
        innerLightTarget = range * 1f;

        StartCoroutine(AnimateLight(light2D, outerLightTarget, innerLightTarget));
    }
   
    private IEnumerator AnimateLight(Light2D light, float outerTarget, float innerTarget)
    {
        float duration = 0.3f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float lerp = t / duration;

            light.pointLightOuterRadius = Mathf.SmoothStep(0, outerTarget, lerp);
            light.pointLightInnerRadius = Mathf.SmoothStep(0, innerTarget, lerp);

            yield return null;
        }

        light.pointLightOuterRadius = outerTarget;
        light.pointLightInnerRadius = innerTarget;

        t = 0f;
        duration = 0.5f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float lerp = t / duration;

            Color lightColor = light.color;
            light.color = new Color(lightColor.r, lightColor.g, lightColor.b, Mathf.SmoothStep(1f, 0f, lerp));
            yield return null;
        }

        Destroy(gameObject);
    }
}
