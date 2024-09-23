using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TorchLight : MonoBehaviour
{
    public float lightRadius = 10f;
    public float lightIntensity = 1f;
    private float currentLightRadius;
    private float currentLightIntensity;
    private const int updateBatchSize = 100;
    private const float updateInterval = 0.1f;
    private TerrainGeneration terrain;
    private LightingSystem dayNightCycle;
    private bool isUpdating = false;

    private void Start()
    {
        terrain = FindObjectOfType<TerrainGeneration>();
        dayNightCycle = FindObjectOfType<LightingSystem>();
        if (terrain == null)
        {
            Debug.LogError("TerrainGeneration non trovato nella scena!");
            return;
        }
        if (dayNightCycle == null)
        {
            Debug.LogError("DayNightCycle non trovato nella scena!");
            return;
        }
        StartCoroutine(UpdateLightingRoutine());
    }

    private IEnumerator UpdateLightingRoutine()
    {
        while (true)
        {
            if (!isUpdating)
            {
                isUpdating = true;
                StartCoroutine(UpdateLightingEfficient());
            }
            yield return new WaitForSeconds(updateInterval);
        }
    }

    private IEnumerator UpdateLightingEfficient()
    {
        UpdateLightParameters();
        Vector2Int position = Vector2Int.RoundToInt(transform.position);
        List<Vector2Int> positionsToUpdate = new List<Vector2Int>();

        for (int dx = -Mathf.CeilToInt(currentLightRadius); dx <= Mathf.CeilToInt(currentLightRadius); dx++)
        {
            for (int dy = -Mathf.CeilToInt(currentLightRadius); dy <= Mathf.CeilToInt(currentLightRadius); dy++)
            {
                int x = position.x + dx;
                int y = position.y + dy;
                if (x >= 0 && x < terrain.worldSize && y >= 0 && y < terrain.worldSize)
                {
                    positionsToUpdate.Add(new Vector2Int(x, y));
                }
            }
        }

        int totalPositions = positionsToUpdate.Count;
        int batchesProcessed = 0;

        while (batchesProcessed * updateBatchSize < totalPositions)
        {
            int startIndex = batchesProcessed * updateBatchSize;
            int endIndex = Mathf.Min(startIndex + updateBatchSize, totalPositions);

            for (int i = startIndex; i < endIndex; i++)
            {
                Vector2Int pos = positionsToUpdate[i];
                UpdateLightingAtPosition(pos.x, pos.y);
            }

            batchesProcessed++;
            if (batchesProcessed % 5 == 0)
            {
                terrain.worldTilesMap.Apply();
                yield return null;
            }
        }

        terrain.worldTilesMap.Apply();
        isUpdating = false;
    }

    private void UpdateLightParameters()
    {
        float nightFactor = 1f - dayNightCycle.GetDaylightFactor();
        currentLightRadius = lightRadius * nightFactor;
        currentLightIntensity = lightIntensity * nightFactor;
    }

    private void UpdateLightingAtPosition(int x, int y)
    {
        Vector2 tilePos = new Vector2(x, y);
        float distance = Vector2.Distance(tilePos, transform.position);
        if (distance <= currentLightRadius)
        {
            float intensity = Mathf.Lerp(currentLightIntensity, 0f, distance / currentLightRadius);
            Color currentColor = terrain.worldTilesMap.GetPixel(x, y);
            float newIntensity = Mathf.Max(currentColor.r, intensity);
            terrain.worldTilesMap.SetPixel(x, y, new Color(newIntensity, newIntensity, newIntensity, 1f));
        }
    }
}