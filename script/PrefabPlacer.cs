using UnityEngine;
using System.Collections.Generic;

public class PrefabPlacer : MonoBehaviour
{
    public GameObject torchPrefab;
    public GameObject doorPrefab;
    public GameObject invisibleBlockPrefab;
    public GameObject[] enemyPrefabs;
    public GameObject woodBackgroundPrefab;
    private TerrainGeneration terrainGenerator;
    public ToolClass toolUsed;
    private PlayerController playerController;
    private Inventory inventory;
    [SerializeField] private int worldWidth = 100;
    [SerializeField] private int worldHeight = 100;
    private GameObject[,] placedObjects;

    private void Start()
    {
        terrainGenerator = FindObjectOfType<TerrainGeneration>();
        playerController = FindObjectOfType<PlayerController>();
        inventory = FindObjectOfType<Inventory>();
        placedObjects = new GameObject[worldWidth, worldHeight];
    }

    public bool PlaceTorch(int x, int y, Quaternion rotation)
    {
        if (IsValidPosition(x, y) && !IsObjectAt(x, y) && inventory.RemoveItemFromInventory("Torch", 1))
        {
            Vector3 position = new Vector3(x, y, 0);
            GameObject torch = Instantiate(torchPrefab, position, rotation);
            placedObjects[x, y] = torch;
            Light torchLight = torch.GetComponent<Light>();
            if (torchLight != null)
            {
                playerController.activeTorches.Add(torchLight);
            }
            return true;
        }
        return false;
    }

    public int GetItemQuantity(string itemName)
    {
        int totalQuantity = 0;

        if (inventory == null || inventory.inventorySlots == null)
        {
            return 0;
        }

        for (int y = 0; y < inventory.inventoryHeight; y++)
        {
            for (int x = 0; x < inventory.inventoryWidth; x++)
            {
                InventorySlot slot = inventory.inventorySlots[x, y];
                if (slot != null && slot.item != null && slot.item.name == itemName)
                {
                    totalQuantity += slot.quantity;
                }
            }
        }

        return totalQuantity;
    }

    public bool PlaceDoor(int x, int y)
    {
        if (IsValidPosition(x, y) && !IsObjectAt(x, y) && inventory.RemoveItemFromInventory("Door", 1))
        {
            Vector3 position = new Vector3(x, y, 0);
            GameObject door = Instantiate(doorPrefab, position, Quaternion.identity);
            placedObjects[x, y] = door;
            return true;
        }
        return false;
    }

    public bool PlaceInvisibleBlock(int x, int y)
    {
        if (IsValidPosition(x, y) && !IsObjectAt(x, y) && inventory.RemoveItemFromInventory("InvisibleBlock", 1))
        {
            Vector3 position = new Vector3(x, y, 0);
            GameObject invisibleBlock = Instantiate(invisibleBlockPrefab, position, Quaternion.identity);
            placedObjects[x, y] = invisibleBlock;

            Collider2D collider = invisibleBlock.GetComponent<Collider2D>();
            if (collider != null)
            {
                Destroy(collider);
            }

            SpriteRenderer spriteRenderer = invisibleBlock.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = null;
            }

            return true;
        }
        return false;
    }

    public bool PlaceWoodBackground(int x, int y)
    {
        if (IsValidPosition(x, y) && !IsObjectAt(x, y) && inventory.RemoveItemFromInventory("WoodBackground", 1))
        {
            Vector3 position = new Vector3(x, y, 0);
            GameObject woodBackground = Instantiate(woodBackgroundPrefab, position, Quaternion.identity);
            woodBackground.tag = "WoodBackground";
            placedObjects[x, y] = woodBackground;

            // Update the terrain
            terrainGenerator.PlaceTile(terrainGenerator.WoodBackground, x, y, true);

            return true;
        }
        return false;
    }

    public void PlaceWoodBackgrounds(int count)
    {
        int placed = 0;
        int attempts = 0;
        int maxAttempts = count * 100;

        while (placed < count && attempts < maxAttempts)
        {
            int x = Random.Range(0, worldWidth);
            int y = Random.Range(0, worldHeight);

            if (PlaceWoodBackground(x, y))
            {
                placed++;
            }

            attempts++;
        }

        Debug.Log($"Placed {placed} WoodBackground blocks out of {count} requested after {attempts} attempts.");
    }

    public void SpawnEnemy(int x, int y)
    {
        if (IsValidPosition(x, y) && !IsObjectAt(x, y))
        {
            Vector3 position = new Vector3(x, y, 0);
            int randomIndex = Random.Range(0, enemyPrefabs.Length);
            Instantiate(enemyPrefabs[randomIndex], position, Quaternion.identity);
        }
    }

    public bool RemoveObject(int x, int y)
    {
        if (IsValidPosition(x, y) && IsObjectAt(x, y))
        {
            GameObject obj = placedObjects[x, y];
            Light torchLight = obj.GetComponent<Light>();
            if (torchLight != null)
            {
                playerController.activeTorches.Remove(torchLight);
            }
            Destroy(obj);
            placedObjects[x, y] = null;
            return true;
        }
        return false;
    }

    public GameObject GetObjectAt(int x, int y)
    {
        if (IsValidPosition(x, y))
        {
            return placedObjects[x, y];
        }
        return null;
    }

    public Light GetTorchLight(int x, int y)
    {
        if (IsValidPosition(x, y) && IsObjectAt(x, y))
        {
            return placedObjects[x, y].GetComponent<Light>();
        }
        return null;
    }

    private bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < worldWidth && y >= 0 && y < worldHeight;
    }

    public bool IsObjectAt(int x, int y)
    {
        return placedObjects[x, y] != null;
    }
    
    public bool RemoveWoodBackground(int x, int y)
    {
        if (IsValidPosition(x, y) && IsObjectAt(x, y))
        {
            GameObject woodBackground = placedObjects[x, y];
            if (woodBackground.CompareTag("WoodBackground"))
            {
                placedObjects[x, y] = null;
                terrainGenerator.RemoveTile(x, y, toolUsed);
                Destroy(woodBackground);
                return true;
            }
        }
        return false;
    }
}