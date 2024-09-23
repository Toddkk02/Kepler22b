using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using static MyGame.Tiles.TileClass;

public class TerrainGeneration : MonoBehaviour
{   
    public GameObject TileDrop;
    public Texture2D worldTilesMap;
    public Material lightShader;
    public float lightThreshold;
    public float lightRadius = 7f;
    public float torchLightRadius = 7f;
    public float torchLightIntensity = 1f;
    public bool background;
    List<Vector2Int> unlitBlock = new List<Vector2Int>();
    List<Vector2Int> toRelight = new List<Vector2Int>();
    
    [Header("Item Sprite")]
    public Sprite HotShell;
    public Sprite leather;
    public Sprite Wood;
    public Sprite WoodBackground;
    public Sprite Door;
    public Sprite Furnace;
    public Sprite Cobblestone;
    public Sprite Torch;
    [Header("Tile Sprites")]
    public int dirtLayerHeight = 5;
    public Sprite grass;
    public Sprite stone;
    public Sprite dirt;
    public Sprite snow;
    public Sprite sand;
    public Sprite log;
    public Sprite leaf;
    public Sprite cactus;

    [Header("Ore Sprites")]
    public Sprite coal;
    public Sprite iron;
    public Sprite gold;
    public Sprite diamond;

    [Header("Background Sprites")]
    public Sprite backgroundStone;
    public Sprite backgroundDirt;
    public Sprite backgroundSand;

    [Header("Tree Generation")]
    public int DistanceBetweenTreeMin = 8;
    public int DistanceBetweenTreeMax = 12;
    public int minTreeHeight = 4;
    public int maxTreeHeight = 6;

    [Header("Generation Settings")]
    public float SurfaceValue = 0.25f;

    public int ChunkSize = 16;
    public bool generateCaves = true;
    public int worldSize = 100;
    public float heightMultiplier = 4f;
    public int heightAddition = 25;
    public float caveFreq = 0.02f;
    public float terrainFreq = 0.1f;
    float seed;

    [Header("Noise Settings")]
    public Texture2D noiseTexture;
    public Texture2D coalSpread;
    public Texture2D ironSpread;
    public Texture2D goldSpread;
    public Texture2D diamondSpread;

    [Header("Ore Settings")]
    public float coalSize = 0.8f;
    public float ironSize = 0.8f;
    public float goldSize = 0.6f;
    public float diamondSize = 0.95f;
    public float coalRarity = 0.05f;
    public float ironRarity = 0.04f;
    public float goldRarity = 0.02f;
    public float diamondRarity = 0.01f;

    private GameObject[] worldChunks;
    private TileType[,] tiles;
    private List<Vector2> worldTiles = new List<Vector2>();
    private List<Vector2> backgroundTiles = new List<Vector2>();
    private List<GameObject> worldTileObjects = new List<GameObject>();
    private List<GameObject> backgroundTileObjects = new List<GameObject>();
    private List<Vector2> treePositions = new List<Vector2>();
    public List<GameObject> worldTileObject = new List<GameObject>();
    private Dictionary<Vector2Int, ItemClass> blockTypes = new Dictionary<Vector2Int, ItemClass>();
    private Dictionary<string, ItemClass> blockDictionary = new Dictionary<string, ItemClass>();
    public CraftingSystem craftingSystem;
    public PlayerController player;
    public Inventory inventory;
     public BackgroundChange backgroundChange;
     public GameObject torchPrefab; // Prefab della torcia

     public ToolClass currentTool;
     private bool[,] blockGrid;

    private void OnValidate()
    {
        DrawTexture();
    }

    private void Start()
    {
        seed = Random.Range(0f, 1000f);
          if (inventory == null)
        {
            inventory = FindObjectOfType<Inventory>();
        }
        worldTilesMap = new Texture2D(worldSize, worldSize);
        worldTilesMap.filterMode = FilterMode.Bilinear;
        lightShader.SetTexture("_ShadowTex", worldTilesMap);
        blockGrid = new bool[worldSize, worldSize];
        InitializeBlockGrid();
        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                worldTilesMap.SetPixel(x, y, Color.white);
            }
        }
        worldTilesMap.Apply();

        if (noiseTexture == null)
        {
            DrawTexture();
        }
        if (backgroundChange != null)
        {
            UpdateBackgroundChange();
        }
        CreateItem();
        CreateChunks();
        GenerateTerrain();
        player.Spawn();
        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                if(worldTilesMap.GetPixel(x,y) == Color.white)
                {
                    LightBlock(x, y, 1f, 0);
                }
            }
        }
        
    }
    public void CreateItem()
{
blockDictionary["Grass"] = new ItemClass("Grass", grass, true ,true, ItemClass.ItemType.Block, 1);
blockDictionary["Dirt"] = new ItemClass("Dirt", dirt, true, true, ItemClass.ItemType.Block, 1);
blockDictionary["Sand"] = new ItemClass("Sand", sand, true, true, ItemClass.ItemType.Block, 1);
blockDictionary["Stone"] = new ItemClass("Stone", stone,true, true, ItemClass.ItemType.Block, 1);
blockDictionary["Snow"] = new ItemClass("Snow", snow,true, true, ItemClass.ItemType.Block, 1);
blockDictionary["Wood"] = new ItemClass("Wood", log, true,true, ItemClass.ItemType.Block, 1);
blockDictionary["Leaf"] = new ItemClass("Leaf", leaf, true, true, ItemClass.ItemType.Block, 1);
blockDictionary["Coal"] = new ItemClass("Coal", coal, true, true, ItemClass.ItemType.Block, 1);
blockDictionary["Iron"] = new ItemClass("Iron", iron, true, true, ItemClass.ItemType.Block, 1);
blockDictionary["Gold"] = new ItemClass("Gold", gold, true, true, ItemClass.ItemType.Block, 1);
blockDictionary["Diamond"] = new ItemClass("Diamond", diamond,true, true, ItemClass.ItemType.Block, 1);
blockDictionary["BackgroundDirt"] = new ItemClass("BackgroundDirt", backgroundDirt,true, true, ItemClass.ItemType.Block, 1);
blockDictionary["BackgroundSand"] = new ItemClass("BackgroundSand", backgroundSand,true, true, ItemClass.ItemType.Block, 1);
blockDictionary["BackgroundStone"] = new ItemClass("BackgroundStone", backgroundStone, true,true, ItemClass.ItemType.Block, 1);
blockDictionary["Cactus"] = new ItemClass("Cactus", cactus,true, true, ItemClass.ItemType.Block, 1);
blockDictionary["Leathers"] = new ItemClass("Leathers",leather, true, true, ItemClass.ItemType.Block, 1);
blockDictionary["HotShell"] = new ItemClass("Hot Shell", HotShell,true, true, ItemClass.ItemType.Block, 1);
blockDictionary["Furnace"] = new ItemClass("Furnace", Furnace, true, true, ItemClass.ItemType.Block, 1);
blockDictionary["Door"] = new ItemClass("Door", Door, true, true, ItemClass.ItemType.Block, 1);
blockDictionary["WoodBackground"] = new ItemClass("WoodBackground", WoodBackground, true, true, ItemClass.ItemType.Block, 1);
blockDictionary["Torch"] = new ItemClass("Torch", Torch, true, true, ItemClass.ItemType.Block, 1);
blockDictionary["Cobblestone"] = new ItemClass("Cobblestone", Cobblestone, true, true, ItemClass.ItemType.Block, 1);
}
    private void InitializeBlockGrid()
    {
        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                blockGrid[x, y] = false;
            }
        }
         tiles = new TileType[worldSize, worldSize];

    // Logica di generazione del terreno
    for (int x = 0; x < worldSize; x++)
    {
        for (int y = 0; y < worldSize; y++)
        {
            // Esempio di popolamento del mondo
            tiles[x, y] = TileType.Empty; // Imposta un valore predefinito
        }
    }

    }
    public bool IsBlockAt(int x, int y)
    {
        if (x >= 0 && x < worldSize && y >= 0 && y < worldSize)
        {
            return blockGrid[x, y];
        }
        return false;
    }

    private void Update()
    {
        RefreshChunks();
        UpdateBackgroundChange(); // Assicurati che il background sia aggiornato ad ogni frame
    }
 private void UpdateBackgroundChange()
    {
        if (backgroundChange == null) return;

        // Determina il biomeOffset basato sulla posizione del giocatore
        int biomeOffset = Mathf.FloorToInt((player.transform.position.x / 50) % 3);
        backgroundChange.biomeOffset = biomeOffset;

        // Cambia lo sfondo in base al biomeOffset
        backgroundChange.ChangeBackground();
    }
    private bool IsValidPosition(int x, int y)
{
    return x >= 0 && x < worldSize && y >= 0 && y < worldSize;
}
private ItemClass GetBlockTypeFromSprite(Sprite sprite)
{
    if (sprite == null)
    {
        Debug.LogWarning("GetBlockTypeFromSprite: Sprite passato è null.");
        return null;
    }

    if (blockDictionary == null || blockDictionary.Count == 0)
    {
        Debug.LogError("GetBlockTypeFromSprite: blockDictionary non è inizializzato o è vuoto.");
        return null;
    }

    var matchingItem = blockDictionary.FirstOrDefault(kvp => kvp.Value.sprite == sprite);

    if (matchingItem.Value != null)
    {
        return matchingItem.Value;
    }
    else
    {
        Debug.LogWarning($"GetBlockTypeFromSprite: Nessun ItemClass trovato per lo sprite {sprite.name}.");
        return null;
    }
}
     public bool RemoveBlockWithTool(int x, int y, ToolClass tool)
    {
        if (!IsValidPosition(x, y))
            return false;

        Vector2Int position = new Vector2Int(x, y);
        if (!blockTypes.ContainsKey(position))
            return false;

        ItemClass blockType = blockTypes[position];

        if (IsCorrectToolForBlock(tool, blockType))
        {
            if (blockType.name == "Log")
            {
                RemoveEntireTree(x, y);
            }
            else
            {
                RemoveSingleBlock(x, y);
            }
            return true;
        }

        return false;
    }

    public bool IsCorrectToolForBlock(ToolClass tool, ItemClass block)
    {
        if (tool == null)
            return false;

        switch (block.name)
        {
            case "Wood":
            case "Leaf":
            case "Door":
                return tool.toolType == ItemClass.ToolType.Axe;
            case "Stone":
            case "Coal":
            case "Iron":
            case "Gold":
            case "Diamond":
            case "Dirt":
            case "Grass":
            case "Sand":
            case "Snow":
            case "torch":
                return tool.toolType == ItemClass.ToolType.Pickaxe;
            case "woodBackground":
            case "BackgroundDirt":
            case "BackgroundSand":
            case "BackgroundStone":
                return tool.toolType == ItemClass.ToolType.Hammer;
            default:
                return false;
        }
    }

    private void RemoveEntireTree(int x, int y)
    {
        Stack<Vector2Int> toRemove = new Stack<Vector2Int>();
        toRemove.Push(new Vector2Int(x, y));

        while (toRemove.Count > 0)
        {
            Vector2Int current = toRemove.Pop();
            RemoveSingleBlock(current.x, current.y);

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    Vector2Int neighbor = new Vector2Int(current.x + dx, current.y + dy);
                    if (blockTypes.ContainsKey(neighbor) && (blockTypes[neighbor].name == "Log" || blockTypes[neighbor].name == "Leaf"))
                    {
                        toRemove.Push(neighbor);
                    }
                }
            }
        }
    }

    private void RemoveSingleBlock(int x, int y)
    {
        Vector2Int position = new Vector2Int(x, y);
        if (!blockTypes.ContainsKey(position))
            return;

        ItemClass blockType = blockTypes[position];
        blockTypes.Remove(position);

        if (blockType.name != "Leaf") // Leaves don't drop items
        {
            DropItem(x, y, blockType);
        }

        // Remove the block from the world
        RemoveTile(x, y, currentTool);
    }

    private void DropItem(int x, int y, ItemClass blockType)
    {
        if (TileDrop != null)
        {
            GameObject dropObject = Instantiate(TileDrop, new Vector3(x, y, 0), Quaternion.identity);
            TileDropController dropController = dropObject.GetComponent<TileDropController>();

            if (dropController != null)
            {
                ItemClass droppedItem = new ItemClass(blockType);
                droppedItem.quantity = 1;
                dropController.SetItem(droppedItem);
            }
        }
    }

    

     void RefreshChunks()
    {
        Vector2 playerPos = player.transform.position;
        float viewDistance = Camera.main.orthographicSize * 2f * Camera.main.aspect;
        int playerChunkIndex = Mathf.FloorToInt(playerPos.x / ChunkSize);

        int visibleChunksHalf = Mathf.CeilToInt(viewDistance / ChunkSize) + 1;

        for (int i = 0; i < worldChunks.Length; i++)
        {
            bool isVisible = Mathf.Abs(i - playerChunkIndex) <= visibleChunksHalf;
            worldChunks[i].SetActive(isVisible);
        }
    }

    public void DrawTexture()
    {
        noiseTexture = new Texture2D(worldSize, worldSize);
        coalSpread = new Texture2D(worldSize, worldSize);
        ironSpread = new Texture2D(worldSize, worldSize);
        goldSpread = new Texture2D(worldSize, worldSize);
        diamondSpread = new Texture2D(worldSize, worldSize);

        GenerateNoiseTexture(caveFreq, SurfaceValue, noiseTexture);
        GenerateNoiseTexture(coalRarity, coalSize, coalSpread);
        GenerateNoiseTexture(ironRarity, ironSize, ironSpread);
        GenerateNoiseTexture(goldRarity, goldSize, goldSpread);
        GenerateNoiseTexture(diamondRarity, diamondSize, diamondSpread);
    }

    private void GenerateNoiseTexture(float frequency, float limit, Texture2D noiseTexture)
    {
        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                float v = Mathf.PerlinNoise((x + seed) * frequency, (y + seed) * frequency);
                noiseTexture.SetPixel(x, y, new Color(v, v, v));
            }
        }
        noiseTexture.Apply();
    }

     public void CreateChunks()
    {
        int NumChunks = worldSize / ChunkSize;
        worldChunks = new GameObject[NumChunks];
        for (int i = 0; i < NumChunks; i++)
        {
            GameObject newChunk = new GameObject();
            newChunk.name = "Chunk_" + i.ToString();
            newChunk.transform.parent = this.transform;
            worldChunks[i] = newChunk;
        }
    }


     

    
     public void UpdateLightingAroundPosition(int x, int y)
    {
        for (int dx = -Mathf.CeilToInt(torchLightRadius); dx <= Mathf.CeilToInt(torchLightRadius); dx++)
        {
            for (int dy = -Mathf.CeilToInt(torchLightRadius); dy <= Mathf.CeilToInt(torchLightRadius); dy++)
            {
                int nx = x + dx;
                int ny = y + dy;
                if (nx >= 0 && nx < worldSize && ny >= 0 && ny < worldSize)
                {
                    UpdateLightingAtPosition(nx, ny);
                }
            }
        }
        worldTilesMap.Apply();
        
    }

    public void UpdateLightingAtPosition(int x, int y)
    {
        float maxIntensity = 0f;
        foreach (GameObject tileObj in worldTileObject)
        {
            TorchLight torch = tileObj.GetComponent<TorchLight>();
            if (torch != null)
            {
                float distance = Vector2.Distance(new Vector2(x, y), tileObj.transform.position);
                if (distance <= torch.lightRadius)
                {
                    float intensity = torch.lightIntensity * (1 - distance / torch.lightRadius);
                    maxIntensity = Mathf.Max(maxIntensity, intensity);
                }
            }
        }

        Color currentColor = worldTilesMap.GetPixel(x, y);
        float newIntensity = Mathf.Max(currentColor.r, maxIntensity);
        worldTilesMap.SetPixel(x, y, new Color(newIntensity, newIntensity, newIntensity));
    }

     public void GenerateTerrain()
{
    int biomeWidth = 50;
    float lastTreeX = -DistanceBetweenTreeMax;

    for (int x = 0; x < worldSize; x++)
    {
        float height = Mathf.PerlinNoise((x + seed) * terrainFreq, seed * terrainFreq) * heightMultiplier + heightAddition;
        if (x == worldSize / 2)
            PlayerController.SpawnPosition = new Vector2(x, height + 1);

        int biomeOffset = (x / biomeWidth) % 3;

        // Check bioma change and create collider
        if (x % biomeWidth == 0)
        {
            CreateBiomeCollider(x, biomeWidth);
        }
        

        Sprite tileSprite = stone;
        Sprite backgroundSprite = backgroundStone;

        for (int y = 0; y < height; y++)
        {
            switch (biomeOffset)
            {
                case 0: // Snow biome
                    tileSprite = (y >= height - dirtLayerHeight) ? ((y < height - 1) ? dirt : snow) : stone;
                    backgroundSprite = (y >= height - dirtLayerHeight) ? backgroundDirt : backgroundStone;
                    break;
                case 1: // Desert biome
                    tileSprite = (y >= height - dirtLayerHeight) ? sand : stone;
                    backgroundSprite = (y >= height - dirtLayerHeight) ? backgroundSand : backgroundStone;
                    break;
                case 2: // Grass biome
                    tileSprite = (y >= height - dirtLayerHeight) ? ((y < height - 1) ? dirt : grass) : stone;
                    backgroundSprite = (y >= height - dirtLayerHeight) ? backgroundDirt : backgroundStone;
                    break;
            }

            if (generateCaves)
            {
                if (noiseTexture.GetPixel(x, y).r > 0)
                {
                    float caveValue = Mathf.PerlinNoise((x + seed) * caveFreq, (y + seed) * caveFreq);
                    if (caveValue < 0.4f)
                    {
                        tileSprite = null;
                    }
                }
            }

            if (tileSprite == stone || tileSprite == dirt || tileSprite == grass || tileSprite == snow || tileSprite == sand)
            {
                if (y >= 15)
                {
                    if (diamondSpread.GetPixel(x, y).r > 0.5f && Random.value < diamondRarity && y < 20)
                    {
                        tileSprite = diamond;
                    }
                    else if (goldSpread.GetPixel(x, y).r > 0.5f && Random.value < goldRarity && y < 25)
                    {
                        tileSprite = gold;
                    }
                    else if (ironSpread.GetPixel(x, y).r > 0.5f && Random.value < ironRarity && y < 35)
                    {
                        tileSprite = iron;
                    }
                    else if (coalSpread.GetPixel(x, y).r > 0.5f && Random.value < coalRarity && y <= 50)
                    {
                        tileSprite = coal;
                    }
                }
            }

            if (tileSprite != null)
            {
                CheckTile(tileSprite, x, y, true);
            }
            
            // Posiziona il tile di sfondo
            PlaceTile(backgroundSprite, x, y, true);
            
            // Colora di nero i tile di sfondo (background)
            if (backgroundSprite != null)
            {
                worldTilesMap.SetPixel(x, y, Color.black);
            }
            else if(tileSprite != null)
            {
                worldTilesMap.SetPixel(x, y, Color.black);

            }
        }

        if (biomeOffset != 1 && x - lastTreeX >= Random.Range(DistanceBetweenTreeMin, DistanceBetweenTreeMax) && Random.value < 0.1f)
        {
            GenerateTree(x, Mathf.RoundToInt(height) + 1);
            lastTreeX = x;
        }

        if (biomeOffset == 1 && Random.value < 0.02f)
        {
            GenerateCactus(x, Mathf.RoundToInt(height) + 1);
        }
    }
    worldTilesMap.Apply();
}

public GameObject GetTorchAt(int x, int y)
{
    // Assuming you have a list or dictionary storing the objects in your world
    // Example: List of all world objects that includes torches
    foreach (GameObject worldObject in worldTileObjects)
    {
        // Check if the world object is a torch and at the given position
        if (worldObject.CompareTag("Torch")) // Assuming your torch objects have a tag "Torch"
        {
            Vector2 torchPosition = worldObject.transform.position;
            
            // Check if the torch's position matches the given x, y coordinates
            if (Mathf.RoundToInt(torchPosition.x) == x && Mathf.RoundToInt(torchPosition.y) == y)
            {
                return worldObject; // Return the torch GameObject if found
            }
        }
    }

    return null; // No torch found at the given position
}
public bool PlaceTorch(int x, int y)
{
    if (x < 0 || x >= worldSize || y < 0 || y >= worldSize || blockGrid[x, y])
    {
        return false;
    }

    GameObject torch = Instantiate(torchPrefab, new Vector3(x, y, 0), Quaternion.identity);
    torch.transform.parent = worldChunks[Mathf.FloorToInt((float)x / ChunkSize)].transform;

    SpriteRenderer spriteRenderer = torch.GetComponent<SpriteRenderer>();
    spriteRenderer.sprite = Torch; // Assicurati che 'Torch' sia definito come Sprite pubblica
    spriteRenderer.sortingLayerName = "WorldTile";
    spriteRenderer.sortingOrder = 1; // Sopra gli altri tile

    blockGrid[x, y] = true;
    worldTiles.Add(new Vector2(x, y));
    worldTileObject.Add(torch);

    // Aggiungi un componente TorchLight
    TorchLight torchLight = torch.AddComponent<TorchLight>();
    torchLight.lightRadius = torchLightRadius;
    torchLight.lightIntensity = torchLightIntensity;

    // Aggiorna l'illuminazione
    UpdateLightingAroundPosition(x, y);

    return true;
}

// Modifica il metodo UpdateLightingAtPosition per gestire le torce



private void CreateBiomeCollider(int startX, int width)
{
    GameObject biomeCollider = new GameObject("BiomeCollider");
    biomeCollider.transform.parent = this.transform;

    BoxCollider2D boxCollider = biomeCollider.AddComponent<BoxCollider2D>();
    boxCollider.isTrigger = true;

    // Imposta la dimensione del collider per coprire il bioma
    boxCollider.size = new Vector2(width, worldSize);
    boxCollider.offset = new Vector2(startX + width / 2, worldSize / 2);
    
    // Aggiungi il componente per cambiare sfondo
    BackgroundChange switcher = biomeCollider.AddComponent<BackgroundChange>();
    switcher.biomeOffset = (startX / width) % 3;
}
    public void GenerateTree(int x, int y)
{
    int treeHeight = Random.Range(minTreeHeight, maxTreeHeight);
    
    for (int i = 0; i < treeHeight; i++)
    {
        PlaceTreeTile(log, x, y + i);
    }
    
    for (int leafX = -2; leafX <= 2; leafX++)
    {
        for (int leafY = -2; leafY <= 2; leafY++)
        {
            if (Mathf.Abs(leafX) == 2 && Mathf.Abs(leafY) == 2)
                continue;
            PlaceTreeTile(leaf, x + leafX, y + treeHeight + leafY);
        }
    }
}

private void PlaceTreeTile(Sprite tileSprite, int x, int y)
{
    GameObject treeTile = new GameObject("TreeTile");
    treeTile.transform.parent = worldChunks[Mathf.FloorToInt((float)x / ChunkSize)].transform;
    treeTile.tag = "Ground";

    SpriteRenderer spriteRenderer = treeTile.AddComponent<SpriteRenderer>();
    spriteRenderer.sprite = tileSprite;
    spriteRenderer.sortingLayerName = "WorldTile";
    spriteRenderer.sortingOrder = 0;

    treeTile.transform.position = new Vector2(x, y);

    // Add to world tiles list but don't add collision
    worldTiles.Add(new Vector2(x, y));
    worldTileObject.Add(treeTile);

    // Update blockTypes
    ItemClass blockType = GetBlockTypeFromSprite(tileSprite);
    if (blockType != null)
    {
        blockTypes[new Vector2Int(x, y)] = blockType;
    }

    // Update the world pixel map
    worldTilesMap.SetPixel(x, y, Color.white);
    worldTilesMap.Apply();
}

    public bool IsTileBackground(int x, int y)
    {
        // Implementazione esempio: controlla se la tile è di tipo background
        TileType tileType = GetTileType(x, y);
        return tileType == TileType.Background || tileType == TileType.WoodBackground;
    }

   public TileType GetTileType(int x, int y)
{
    // Controlla se 'tiles' è nullo
    if (tiles == null)
    {
        Debug.LogError("tiles array is not initialized!");
        return TileType.Empty;
    }

    // Controllo dei limiti della mappa
    if (x < 0 || x >= worldSize || y < 0 || y >= worldSize)
    {
        return TileType.Empty;
    }

    return tiles[x, y];
}

    public bool IsSolidBlock(int x, int y)
    {
        // Implementazione esempio: controlla se il blocco è solido
        TileType tileType = GetTileType(x, y);
        return tileType == TileType.Dirt || tileType == TileType.Stone || tileType == TileType.Wood;
    }

    

    public void GenerateCactus(int x, int y)
    {
        int cactusHeight = Random.Range(minTreeHeight, maxTreeHeight);

        for (int i = 0; i < cactusHeight; i++)
        {
            PlaceTile(cactus, x, y + i, false);
        }
    }

public bool PlaceTile(Sprite tileSprite, int x, int y, bool background)
{
    // Verifica se le coordinate sono all'interno dei limiti del mondo
    if (x < 0 || x >= worldSize || y < 0 || y >= worldSize)
    {
        Debug.Log($"Impossibile piazzare il blocco in {x}, {y}: fuori dai limiti del mondo");
        return false;
    }

    int chunkIndex = Mathf.FloorToInt((float)x / ChunkSize);
    if (chunkIndex < 0 || chunkIndex >= worldChunks.Length)
    {
        Debug.Log($"Impossibile piazzare il blocco in {x}, {y}: chunk non valido");
        return false;
    }


    // Se c'è già un blocco solido (non sfondo) e stiamo piazzando un blocco solido, non possiamo piazzare
    if (blockGrid[x, y] && !background)
    {
        Debug.Log($"Impossibile piazzare il blocco in {x}, {y}: posizione già occupata");
        return false;
    }

    GameObject newTile = new GameObject();
    newTile.transform.parent = worldChunks[chunkIndex].transform;

    // Usa il dizionario blockDictionary per ottenere l'ItemClass corrispondente allo sprite
    ItemClass tileItem = blockDictionary.FirstOrDefault(kvp => kvp.Value.sprite == tileSprite).Value;
    if (tileItem != null)
    {
        newTile.name = tileItem.name;
        newTile.tag = "Ground";
    }
    else
    {
        newTile.name = tileSprite.name + " " + x.ToString() + " " + y.ToString();
    }

    SpriteRenderer spriteRenderer = newTile.AddComponent<SpriteRenderer>();
    spriteRenderer.sprite = tileSprite;

    if (background)
    {   newTile.tag = "BackgroundTile";
        spriteRenderer.sortingLayerName = "BackgroundTile";
        spriteRenderer.sortingOrder = -1;
        backgroundTiles.Add(new Vector2(x, y));
        backgroundTileObjects.Add(newTile);
    }
    else
    {
        spriteRenderer.sortingLayerName = "WorldTile";
        spriteRenderer.sortingOrder = 0;
        blockGrid[x, y] = true;
        worldTiles.Add(new Vector2(x, y));
        worldTileObject.Add(newTile);
        
        BoxCollider2D collider = newTile.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(1, 1);

        // Aggiorna blockTypes
        ItemClass blockType = GetBlockTypeFromSprite(tileSprite);
        if (blockType != null)
        {
            blockTypes[new Vector2Int(x, y)] = blockType;
        }

        // Aggiorna la mappa dei pixel del mondo
        worldTilesMap.SetPixel(x, y, Color.black);
        worldTilesMap.Apply();
    }

    newTile.transform.position = new Vector2(x, y);

    Debug.Log($"Blocco {tileItem?.name ?? tileSprite.name} piazzato con successo in {x}, {y}");
    return true;
}


// Helper method to get the quantity of an item in the inventory




public void LightPropagation(int x, int y)
{
    // Inizia la propagazione della luce dalla posizione in cui è stato rimosso il blocco
    for (int nx = x - 1; nx <= x + 1; nx++)
    {
        for (int ny = y - 1; ny <= y + 1; ny++)
        {
            if (nx != x || ny != y)
            {
                // Controlla se c'è un blocco in quella posizione
                if (worldTilesMap.GetPixel(nx, ny) == Color.white) // Se è bianco, è uno spazio vuoto
                {
                    LightBlock(nx, ny, 1f, 0); // Propaga la luce in tutte le direzioni
                }
            }
        }
    }
}

public void BlockLight(int x, int y)
{
    // Blocca la luce nella posizione del blocco appena piazzato
    worldTilesMap.SetPixel(x, y, Color.black); // Il blocco blocca la luce

    // Ripercorri i blocchi adiacenti per riaggiornare la luce
    for (int nx = x - 1; nx <= x + 1; nx++)
    {
        for (int ny = y - 1; ny <= y + 1; ny++)
        {
            if (nx != x || ny != y)
            {
                if (worldTilesMap.GetPixel(nx, ny) == Color.white)
                {
                    // Riaggiorna la propagazione della luce nei blocchi adiacenti
                    LightBlock(nx, ny, 1f, 0);
                }
            }
        }
    }
}


public bool RemoveTile(int x, int y, ToolClass toolUsed)
{
    // Trova l'indice del tile alle coordinate specificate
    int index = worldTiles.FindIndex(tile => tile.x == x && tile.y == y);
    int backgroundIndex = backgroundTiles.FindIndex(tile => tile.x == x && tile.y == y);
    
    if (index == -1 && backgroundIndex == -1)
    {
        // Se non c'è un tile in quella posizione, ritorna false
        return false;
    }

    GameObject tile = null;
    bool isBackgroundTile = false;

    if (index != -1)
    {
        tile = worldTileObject[index];
    }
    else if (backgroundIndex != -1)
    {
        tile = backgroundTileObjects[backgroundIndex];
        isBackgroundTile = true;
    }
    
    if (tile != null)
    {
        // Ottieni lo SpriteRenderer e lo sprite del tile
        SpriteRenderer spriteRenderer = tile.GetComponent<SpriteRenderer>();
        Sprite tileSprite = spriteRenderer != null ? spriteRenderer.sprite : null;

        // Controlla se il tile è un "TreeTile" (rimovibile con un'ascia)
        bool isTreeTile = tile.CompareTag("TreeTile");

        // Controlla se lo strumento usato è quello giusto per rimuovere il tile
        if ((toolUsed.toolType == ItemClass.ToolType.Hammer && isBackgroundTile) || 
            (toolUsed.toolType == ItemClass.ToolType.Axe && isTreeTile) ||
            (toolUsed.toolType == ItemClass.ToolType.Pickaxe && !isTreeTile && !isBackgroundTile))
        {
            // Rimuovi il tile dalla griglia dei blocchi e aggiorna la mappa del mondo
            if (x >= 0 && x < worldSize && y >= 0 && y < worldSize)
            {
                blockGrid[x, y] = false;
                worldTilesMap.SetPixel(x, y, Color.white);
                if (isBackgroundTile)
                {
                    backgroundTiles.RemoveAt(backgroundIndex);
                    backgroundTileObjects.RemoveAt(backgroundIndex);
                }
                else
                {
                    worldTiles.RemoveAt(index);
                    worldTileObject.RemoveAt(index);
                }
                blockTypes.Remove(new Vector2Int(x, y));
            }

            // Distruggi il GameObject associato
            Destroy(tile);

            worldTilesMap.Apply();

            // Propagazione della luce dopo la rimozione del tile
            LightPropagation(x, y);

            return true; // Tile rimosso con successo
        }
    }
    
    // Se arriviamo a questo punto, il tile non può essere rimosso
    return false;
}

    private int CountBlocksBelow(int x, int y)
{
    int count = 0;
    for (int i = y - 1; i >= 0; i--) // Start from the tile below and move downwards
    {
        int index = worldTiles.FindIndex(tile => tile.x == x && tile.y == i);
        if (index == -1)
        {
            break; // Stop if we find an empty tile
        }
        count++;
    }
    return count;
}
 void LightBlock(int x, int y, float intensity, int iteration)
{
    if (iteration >= lightRadius) return;

    worldTilesMap.SetPixel(x, y, Color.white * intensity);

    for (int nx = x - 1; nx < x + 2; nx++)
    {
        for (int ny = y - 1; ny < y + 2; ny++)
        {
            if (nx != x || ny != y)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(nx, ny));
                float targetIntensity = Mathf.Pow(0.7f, dist) * intensity;

                if (worldTilesMap.GetPixel(nx, ny) != null && worldTilesMap.GetPixel(nx, ny).r < targetIntensity)
                {
                    LightBlock(nx, ny, targetIntensity, iteration + 1);
                }
            }
        }
    }
}

void RemoveLightSource(int x, int y)
{
    unlitBlock.Clear();
    UnLightBlock(x, y, x, y);

    List<Vector2Int> toRelight = new List<Vector2Int>();
    foreach (Vector2Int block in unlitBlock)
    {
        for (int nx = block.x - 1; nx < block.x + 2; nx++)
        {
            for (int ny = block.y - 1; ny < block.y + 2; ny++)
            {
                if (worldTilesMap.GetPixel(nx, ny) != null)
                {
                    if (worldTilesMap.GetPixel(nx, ny).r > worldTilesMap.GetPixel(block.x, block.y).r)
                    {
                        if (!toRelight.Contains(new Vector2Int(nx, ny)))
                        {
                            toRelight.Add(new Vector2Int(nx, ny));
                        }
                    }
                }
            }
        }
    }
}

public bool CheckTile(Sprite tileSprite, int x, int y, bool isNaturallyPlaced)
{
    bool tileChanged = false;

    if (x >= 0 && x < worldSize && y >= 0 && y < worldSize)
    {
        if (!worldTiles.Contains(new Vector2(x, y)))
        {
            RemoveLightSource(x, y);
            PlaceTile(tileSprite, x, y, false);
            tileChanged = true;
        }
        else
        {
            int index = worldTiles.FindIndex(tile => tile.x == x && tile.y == y);
            if (index != -1 && worldTileObject[index].GetComponent<SpriteRenderer>().sprite != tileSprite)
            {
                RemoveLightSource(x, y);
                PlaceTile(tileSprite, x, y, false);
                tileChanged = true;
            }
        }
    }

    // Relight sources in the toRelight list
    foreach (Vector2Int source in toRelight)
    {
        LightBlock(source.x, source.y, worldTilesMap.GetPixel(source.x, source.y).r, 0);
    }

    worldTilesMap.Apply(); // Apply changes to the texture

    return tileChanged;
}




void UnLightBlock(int x, int y, int ix, int iy)
{
    if (Mathf.Abs(x - ix) >= lightRadius || Mathf.Abs(y - iy) >= lightRadius || unlitBlock.Contains(new Vector2Int(x, y)))
        return;

    for (int nx = x - 1; nx < x + 2; nx++)
    {
        for (int ny = y - 1; ny < y + 2; ny++)
        {
            if (nx != x || ny != y)
            {
                if (worldTilesMap.GetPixel(nx, ny) != null)
                {
                    if (worldTilesMap.GetPixel(nx, ny).r < worldTilesMap.GetPixel(x, y).r)
                    {
                        UnLightBlock(nx, ny, ix, iy);
                    }
                }
            }
        }
    }


    // Spegni la luce sul blocco attuale
    worldTilesMap.SetPixel(x, y, Color.black); // Imposta il pixel come bianco (nessuna luce)
    unlitBlock.Add(new Vector2Int(x, y)); // Aggiungi il blocco alla lista di blocchi spenti


}

}
