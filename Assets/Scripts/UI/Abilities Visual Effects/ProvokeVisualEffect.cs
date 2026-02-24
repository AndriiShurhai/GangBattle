using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ProvokeVisualEffect : MonoBehaviour
{
    [SerializeField] private GameObject abilityHighlightTilePrefab;

    [Header("Wave")]
    [SerializeField] private float waveSpeed = 12f; // tiles per second

    [Header("Timing")]
    [SerializeField] private float fadeIn = 0.18f;
    [SerializeField] private float hold = 0.25f;
    [SerializeField] private float fadeOut = 0.35f;

    [Header("Light")]
    [SerializeField] private float maxIntensity = 20f;
    [SerializeField] private Vector2 startScale = new Vector2(0f, 0f);
    [SerializeField] private Vector2 endScale = new Vector2(2.3f, 1.2f);

    private int runningTiles = 0;

    public void Execute(Vector3Int casterPos, List<Vector3Int> reachableTiles)
    {
        runningTiles = reachableTiles.Count;
        foreach (Vector3Int pos in reachableTiles)
        {
            if (pos == casterPos) continue;
            GameObject tile = Instantiate(
                abilityHighlightTilePrefab,
                GridManager.Instance.GridToWorld(pos),
                Quaternion.identity
            );

            float distance = Mathf.Abs(pos.x - casterPos.x) + Mathf.Abs(pos.y - casterPos.y);
            float delay = distance / waveSpeed;

            StartCoroutine(DoWaveTile(tile, delay));
        }
    }

    private IEnumerator DoWaveTile(GameObject tile, float delay)
    {
        Light2D light = tile.GetComponentInChildren<Light2D>();
        Transform tr = tile.transform;

        if (light == null)
        {
            Destroy(tile);
            yield break;
        }

        tr.localScale = startScale;
        light.intensity = 0;

        // --- wave propagation delay ---
        yield return new WaitForSeconds(delay);

        // --- fade in ---
        float t = 0f;
        while (t < fadeIn)
        {
            t += Time.deltaTime;
            float k = t / fadeIn;

            light.intensity = Mathf.Lerp(0, maxIntensity, k);

            Vector2 scaleX = Vector3.one * Mathf.Lerp(startScale.x, endScale.x, k);
            Vector3 scaleY = Vector3.one * Mathf.Lerp(startScale.y, endScale.y, k);
            tr.localScale = new Vector3(scaleX.x, scaleY.y, 1);

            yield return null;
        }

        yield return new WaitForSeconds(hold);

        // --- fade out ---
        t = 0f;
        while (t < fadeOut)
        {
            t += Time.deltaTime;
            float k = t / fadeOut;

            light.intensity = Mathf.Lerp(maxIntensity, 0, k);
            Vector2 scaleX = Vector3.one * Mathf.Lerp(endScale.x, startScale.x, k);
            Vector3 scaleY = Vector3.one * Mathf.Lerp(endScale.y, startScale.y, k);
            tr.localScale = new Vector3(scaleX.x, scaleY.y, 1); 

            yield return null;
        }

        FinishTile(tile);
    }

    private void FinishTile(GameObject tile)
    {
        if (tile) Destroy(tile);

        runningTiles--;

        if (runningTiles <= 0)
            Destroy(gameObject); 
    }
}