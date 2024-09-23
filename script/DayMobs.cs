using System.Collections.Generic;
using UnityEngine;

public class DayMobs : MonoBehaviour
{
    public GameObject dayMobPrefab;
    public PlayerController player;
    public Camera mainCamera;
    public float spawnRadius = 10f;
    public float spawnProbability = 0.1f;
    public int mobDamage = 10;
    public float movementSpeed = 2f;
    public int maxMobCount = 6;
    public int coin;
    public GameObject dropItemPrefab;
    public List<GameObject> activeMobs = new List<GameObject>();
    public TerrainGeneration terrainGenerator;
    public LightingSystem lightingSystem;

    void Start()
    {
        mainCamera = Camera.main;
        terrainGenerator = FindObjectOfType<TerrainGeneration>();
        lightingSystem = FindObjectOfType<LightingSystem>();
        if (lightingSystem != null)
        {
            lightingSystem.OnDayNightChange += HandleDayNightChange;
        }
        else
        {
            Debug.LogError("LightingSystem not found in the scene!");
        }
    }

    void Update()
    {
        activeMobs.RemoveAll(mob => mob == null);
        if (lightingSystem.isDay && activeMobs.Count < maxMobCount && ShouldSpawnMob())
        {
            Vector2 spawnPosition = GetSpawnPosition();
            SpawnMob(spawnPosition);
        }
    }

    bool ShouldSpawnMob()
    {
        return Random.value < spawnProbability * Time.deltaTime;
    }

    Vector2 GetSpawnPosition()
    {
        Vector3 cameraPosition = mainCamera.transform.position;
        float cameraHeight = 2f * mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;
        float horizontalSpawnRange = cameraWidth / 2 + spawnRadius;
        float spawnX = cameraPosition.x + (Random.value > 0.5f ? -horizontalSpawnRange : horizontalSpawnRange);
        int terrainHeight = FindHighestTerrainPoint(Mathf.RoundToInt(spawnX));
        Vector2 spawnPosition = new Vector2(spawnX, terrainHeight + 10);
        return spawnPosition;
    }

    int FindHighestTerrainPoint(int x)
    {
        for (int y = terrainGenerator.worldSize - 1; y >= 0; y--)
        {
            if (terrainGenerator.IsBlockAt(x, y))
            {
                return y + 1;
            }
        }
        return 0;
    }

    public void DropCoin()
    {
        
        int coinDropped = Random.Range(2, 16);
        coin += coinDropped;

    }

    void SpawnMob(Vector2 spawnPosition)
    {
        GameObject mob = Instantiate(dayMobPrefab, spawnPosition, Quaternion.identity);
        MobAI mobAI = mob.GetComponent<MobAI>();
        mobAI.Initialize(player, movementSpeed);
        mobAI.dropitemPrefab = dropItemPrefab;
        activeMobs.Add(mob);
        Rigidbody2D rb = mob.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = mob.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 1;
    }

   void HandleDayNightChange(bool isDay)
    {
        if (!isDay)
        {
            // Despawn all day mobs when night begins
            foreach (GameObject mob in activeMobs)
            {
                if (mob != null)
                {
                    Destroy(mob);
                }
            }
            activeMobs.Clear();
        }
    }

    void OnDestroy()
    {
        if (lightingSystem != null)
        {
            lightingSystem.OnDayNightChange -= HandleDayNightChange;
        }
    }
}