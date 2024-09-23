using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;
using System.Collections.Generic;

public class LightingSystem : MonoBehaviour
{
    public float dayLength = 240f; // Duration of the day in seconds (4 minutes)
    public float nightLength = 240f; // Duration of the night in seconds (4 minutes)
    public SpriteRenderer nightOverlay;
    public Light2D globalLight;
    public Color dayColor = Color.white;
    public Color nightColor = new Color(0.1f, 0.1f, 0.2f);
    public bool isDay { get; private set; }
    public float torchLightRadius = 10f;
    public float torchLightIntensity = 1f;

    public delegate void DayNightChangeHandler(bool isDay);
    public event DayNightChangeHandler OnDayNightChange;
    
    private float timeElapsed;
    private TerrainGeneration terrain;
    private List<TorchData> torches = new List<TorchData>();
    
    private const int updateBatchSize = 100;
    private const float updateInterval = 0.1f;
    
    private void Start()
    {
        terrain = FindObjectOfType<TerrainGeneration>();
        if (terrain == null)
        {
            Debug.LogError("TerrainGeneration not found in the scene!");
            return;
        }
        
        if (nightOverlay == null) CreateNightOverlay();
        if (globalLight == null) CreateGlobalLight();
        
        // Start the game with morning light
        timeElapsed = 0f;
        isDay = true;
        UpdateLighting(0f);
        
        StartCoroutine(UpdateLightingRoutine());
    }
    
    private void Update()
    {
        timeElapsed += Time.deltaTime;
        float totalCycleLength = dayLength + nightLength;
        float cycleCompletion = (timeElapsed % totalCycleLength) / totalCycleLength;
        
        bool wasDay = isDay;
        isDay = cycleCompletion < (dayLength / totalCycleLength);
        
        if (wasDay != isDay)
        {
            OnDayNightChange?.Invoke(isDay);
        }
        
        UpdateLighting(cycleCompletion);
    }
    
    private void UpdateLighting(float cycleCompletion)
    {
        float dayNightTransition = Mathf.Sin(cycleCompletion * 2 * Mathf.PI) * 0.5f + 0.5f;
        globalLight.color = Color.Lerp(nightColor, dayColor, dayNightTransition);
        globalLight.intensity = Mathf.Lerp(0.2f, 1f, dayNightTransition);
        
        Color overlayColor = nightOverlay.color;
        overlayColor.a = Mathf.Lerp(0.7f, 0f, dayNightTransition);
        nightOverlay.color = overlayColor;
    }
    
    public float GetDaylightFactor()
    {   
        float totalCycleLength = dayLength + nightLength;
        float cycleCompletion = (timeElapsed % totalCycleLength) / totalCycleLength;
        return Mathf.Sin(cycleCompletion * 2 * Mathf.PI) * 0.5f + 0.5f;
    }
    
    public void AddTorch(Vector2 position)
    {
        torches.Add(new TorchData(position));
    }
    
    public void RemoveTorch(Vector2 position)
    {
        torches.RemoveAll(t => Vector2.Distance(t.position, position) < 0.1f);
    }
    
    private IEnumerator UpdateLightingRoutine()
    {
        while (true)
        {
            yield return StartCoroutine(UpdateLightingEfficient());
            yield return new WaitForSeconds(updateInterval);
        }
    }
    
    private IEnumerator UpdateLightingEfficient()
    {
        float daylightFactor = GetDaylightFactor();
        float currentTorchLightRadius = torchLightRadius;
        float currentTorchLightIntensity = torchLightIntensity * (1f - daylightFactor * 0.5f);
        
        List<Vector2Int> positionsToUpdate = new List<Vector2Int>();
        
        foreach (var torch in torches)
        {
            Vector2Int torchPosition = Vector2Int.RoundToInt(torch.position);
            for (int dx = -Mathf.CeilToInt(currentTorchLightRadius); dx <= Mathf.CeilToInt(currentTorchLightRadius); dx++)
            {
                for (int dy = -Mathf.CeilToInt(currentTorchLightRadius); dy <= Mathf.CeilToInt(currentTorchLightRadius); dy++)
                {
                    int x = torchPosition.x + dx;
                    int y = torchPosition.y + dy;
                    if (x >= 0 && x < terrain.worldSize && y >= 0 && y < terrain.worldSize)
                    {
                        positionsToUpdate.Add(new Vector2Int(x, y));
                    }
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
                UpdateLightingAtPosition(pos.x, pos.y, currentTorchLightRadius, currentTorchLightIntensity);
            }
            
            batchesProcessed++;
            if (batchesProcessed % 5 == 0)
            {
                terrain.worldTilesMap.Apply();
                yield return null;
            }
        }
        
        terrain.worldTilesMap.Apply();
    }
    
    private void UpdateLightingAtPosition(int x, int y, float radius, float intensity)
    {
        Vector2 tilePos = new Vector2(x, y);
        float maxIntensity = 0f;
        
        foreach (var torch in torches)
        {
            float distance = Vector2.Distance(tilePos, torch.position);
            if (distance <= radius)
            {
                float torchIntensity = Mathf.Lerp(intensity, 0f, distance / radius);
                maxIntensity = Mathf.Max(maxIntensity, torchIntensity);
            }
        }
        
        Color currentColor = terrain.worldTilesMap.GetPixel(x, y);
        float newIntensity = Mathf.Max(currentColor.r, maxIntensity);
        terrain.worldTilesMap.SetPixel(x, y, new Color(newIntensity, newIntensity, newIntensity, 1f));
    }
    
    private void CreateNightOverlay()
    {
        GameObject overlayObj = new GameObject("NightOverlay");
        nightOverlay = overlayObj.AddComponent<SpriteRenderer>();
        nightOverlay.sprite = CreateOverlaySprite();
        nightOverlay.color = new Color(0, 0, 0, 0);
        nightOverlay.sortingOrder = 1000;
        overlayObj.transform.localScale = new Vector3(100, 100, 1);
    }
    
    private Sprite CreateOverlaySprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
    }
    
    private void CreateGlobalLight()
    {
        GameObject lightObj = new GameObject("GlobalLight");
        globalLight = lightObj.AddComponent<Light2D>();
        globalLight.lightType = Light2D.LightType.Global;
        globalLight.color = dayColor;
        globalLight.intensity = 1f;
    }
}

public class TorchData
{
    public Vector2 position;
    
    public TorchData(Vector2 pos)
    {
        position = pos;
    }
}