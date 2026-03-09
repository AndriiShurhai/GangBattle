using UnityEngine;
using System.Collections;

namespace Assets.Scripts.UI.Abilities_Visual_Effects
{
	public class FireBurnVisualEffect: MonoBehaviour
	{
        [SerializeField] private float duration = 0.5f;
        public void ExtinctFire()
        {
            StartCoroutine(ExtintBurnVisual());
        }
        private IEnumerator ExtintBurnVisual()
        {
            float t = 0f;

            yield return new WaitForSeconds(duration);

            while (t < duration)
            {                
                t += Time.deltaTime;

                GetComponentInChildren<SpriteRenderer>().color = new Color(1f, 1f, 1f, Mathf.SmoothStep(1f, 0f, t / duration));

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}