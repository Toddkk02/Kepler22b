// PlayerController.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static MyGame.Tiles.TileClass;

public class PlayerController : MonoBehaviour
{
    Light torchLight;
    public int selectedSlotIndex = 0;
    public GameObject HotbarSelector;
    public Inventory inventory;
    public bool InventoryShowing = false;
    public int PlayerReach = 5;
    public Vector2Int mousePos;
    public float MoveSpeed;
    public float JumpForce;
    public float cooldownJump = 3f;
    private float lastUsedTime;
    public bool onGround;
    public bool hit;
    public float attackAngle = 120f;
    public float attackRadius = 3.5f;
    public GameObject equippedItemDisplay;
    private SpriteRenderer equippedItemRenderer;
    public float initialSwordDamage = 5f;
    public float swordCooldown = 0.5f;
    private bool canSwingSword = true;
    public bool place;
    public int maxHealth = 100;
    public float health = 100f;
    public float knockbackForce = 5f;
    private Rigidbody2D rb;
    public static Vector2 SpawnPosition;
    public TerrainGeneration terrainGenerator;
    public ItemClass selectedblock;
    public ToolClass selectedTool;
    public GameObject DeathScreen;
    public float timeToAutoHealth = 0;
    public float timeToRespawn = 5f;
    public float Waiting = 0f;
    public float MaxtimeToAutoHealth = 10f;
    public GameObject invisibleBlockPrefab;
    private ItemClass equippedItem;
    private float blockBreakTimer = 0f;
    private float blockBreakDuration = 1f;
    private Vector2Int currentBreakingBlock;
    private bool isBreakingBlock = false;

    public float torchLuminosity = 1f;
    public float maxTorchLuminosity = 1f;
    public float torchDecayRate = 0.1f;
    public float torchRefuelAmount = 0.5f;
    public List<Light> activeTorches = new List<Light>();
    public float torchPlacementCooldown = 0.5f;
    private float lastTorchPlacementTime;

    public GameObject doorPrefab;

    private PrefabPlacer prefabPlacer;

    private void Start()
    {
        equippedItemRenderer = equippedItemDisplay.GetComponent<SpriteRenderer>();
        DeathScreen.gameObject.SetActive(false);
        rb = GetComponent<Rigidbody2D>();
        inventory = GetComponent<Inventory>();
        inventory.inventoryUI.SetActive(false);
        InventoryShowing = false;

        inventory = GetComponent<Inventory>();
        prefabPlacer = FindObjectOfType<PrefabPlacer>();
    }

    public void Spawn()
    {
        transform.position = SpawnPosition;
    }

    private void Update()
    {
        HandleMovement();
        HandleInventorySelection();
        UpdateEquippedItem();
        HandleBlockInteraction();
        HandleInventoryToggle();
        AutoHealth();
        UpdateTorchLuminosity();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        Vector2 movement = new Vector2(horizontal * MoveSpeed, rb.velocity.y);

        if (Input.GetButtonDown("Jump") && onGround)
        {
            movement.y = JumpForce;
            onGround = false;
        }

        rb.velocity = movement;

        if (horizontal < 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (horizontal > 0)
            transform.localScale = new Vector3(-1, 1, 1);

        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.x = Mathf.RoundToInt(mouseWorldPosition.x);
        mousePos.y = Mathf.RoundToInt(mouseWorldPosition.y);
    }

    private void HandleInventorySelection()
    {
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            if (selectedSlotIndex < inventory.inventoryWidth - 1)
                selectedSlotIndex++;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (selectedSlotIndex > 0)
                selectedSlotIndex--;
        }

        HotbarSelector.transform.position = inventory.hotbarUISlot[selectedSlotIndex].transform.position;
    }

    private void UpdateEquippedItem()
    {
        if (inventory.inventorySlots[selectedSlotIndex, 0] != null)
        {
            equippedItem = inventory.inventorySlots[selectedSlotIndex, 0].item;
            if (equippedItem != null && equippedItem != selectedblock)
            {
                if (equippedItem.sprite != null)
                {
                    equippedItemRenderer.sprite = equippedItem.sprite;
                    equippedItemDisplay.SetActive(true);
                }
                else
                {
                    equippedItemDisplay.SetActive(false);
                }
            }
        }
        else
        {
            equippedItem = null;
            equippedItemRenderer.sprite = null;
            equippedItemDisplay.SetActive(false);
        }
        selectedblock = equippedItem;
        selectedTool = (equippedItem?.itemType == ItemClass.ItemType.Tool) ? equippedItem.tool : null;
    }

    private void HandleBlockInteraction()
    {
        if (!InventoryShowing && Vector2.Distance(transform.position, mousePos) <= PlayerReach)
        {
            if (Input.GetMouseButton(0))
            {
                if (selectedTool != null && selectedTool.toolType != ItemClass.ToolType.Sword)
                {
                    BreakBlock();
                }
                else if (selectedTool != null && selectedTool.toolType == ItemClass.ToolType.Sword)
                {
                    OnSwingSword();
                }
            }
            else if (Input.GetMouseButtonDown(1))
            {
                PlaceBlock();
            }
            else
            {
                ResetBlockBreaking();
            }
        }
    }

    private void BreakBlock()
    {
        if (!isBreakingBlock || currentBreakingBlock != mousePos)
        {
            ResetBlockBreaking();
            currentBreakingBlock = mousePos;
            isBreakingBlock = true;
        }

        blockBreakTimer += Time.deltaTime;
        if (blockBreakTimer >= blockBreakDuration)
        {
            terrainGenerator.RemoveBlockWithTool(currentBreakingBlock.x, currentBreakingBlock.y, selectedTool);
            ResetBlockBreaking();

            torchLight = prefabPlacer.GetTorchLight(currentBreakingBlock.x, currentBreakingBlock.y);
            if (torchLight != null)
            {
                activeTorches.Remove(torchLight);
                prefabPlacer.RemoveObject(currentBreakingBlock.x, currentBreakingBlock.y);
            }
        }
    }

    private void ResetBlockBreaking()
    {
        blockBreakTimer = 0f;
        isBreakingBlock = false;
    }

    private void PlaceBlock()
    {
        if (selectedblock != null && selectedblock.isPlaceable)
        {
            bool placed = false;

            switch (selectedblock.name)
            {
                case "InvisibleBlock":
                    placed = prefabPlacer.PlaceInvisibleBlock(mousePos.x, mousePos.y);
                    break;
                case "Door":
                    placed = prefabPlacer.PlaceDoor(mousePos.x, mousePos.y);
                    break;
                case "Torch":
                    placed = PlaceTorch(mousePos.x, mousePos.y);
                    break;
                case "WoodBackground":
                    placed = prefabPlacer.PlaceWoodBackground(mousePos.x, mousePos.y);
                    break;
                default:
                    terrainGenerator.PlaceTile(selectedblock.sprite, mousePos.x, mousePos.y, false);
                    placed = true;
                    break;
            }

            if (placed)
            {
                inventory.RemoveSingle(selectedblock);
            }
        }
    }

    private bool PlaceTorch(int x, int y)
    {
        if (terrainGenerator.IsTileBackground(x, y) || terrainGenerator.GetTileType(x, y) == TileType.WoodBackground)
        {
            if (terrainGenerator.IsSolidBlock(x - 1, y))
            {
                prefabPlacer.PlaceTorch(x, y, Quaternion.Euler(0, 0, 45));
            }
            else if (terrainGenerator.IsSolidBlock(x + 1, y))
            {
                prefabPlacer.PlaceTorch(x, y, Quaternion.Euler(0, 0, -45));
            }
            else
            {
                return prefabPlacer.PlaceTorch(x, y, Quaternion.identity);
            }
            return true;
        }

        return false;
    }

    private void HandleInventoryToggle()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            InventoryShowing = !InventoryShowing;
            inventory.inventoryUI.SetActive(InventoryShowing);
        }
    }

    public void AutoHealth()
    {
        timeToAutoHealth += Time.deltaTime;
        if (timeToAutoHealth >= MaxtimeToAutoHealth)
        {
            health += 10;
            timeToAutoHealth = 0f;
        }
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection, float knockbackForce)
    {
        health -= damage;
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        if (health <= 0)
        {
            PlayerDeath();
        }
    }

    private void PlayerDeath()
    {
        DeathScreen.SetActive(true);
        this.enabled = false;
        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(timeToRespawn);
        DeathScreen.SetActive(false);
        transform.position = SpawnPosition;
        health = maxHealth;
        timeToAutoHealth = 0f;
        this.enabled = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            onGround = true;
        }
    }

    public void OnSwingSword()
    {
        if (canSwingSword && equippedItem != null && equippedItem.itemType == ItemClass.ItemType.Tool && equippedItem.toolType == ItemClass.ToolType.Sword)
        {
            float swordDamage = initialSwordDamage;
            InflictDamageToEnemies(swordDamage);
            StartCoroutine(SwordCooldown());
        }
    }

    private IEnumerator SwordCooldown()
    {
        canSwingSword = false;
        yield return new WaitForSeconds(swordCooldown);
        canSwingSword = true;
    }

    void InflictDamageToEnemies(float swordDamage)
    {
        Vector2 attackOrigin = (Vector2)transform.position + (Vector2)transform.right * 0.5f;
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackOrigin, attackRadius);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                Vector2 directionToEnemy = (enemy.transform.position - transform.position).normalized;
                float angle = Vector2.Angle(transform.right, directionToEnemy);

                if (angle < attackAngle / 2f)
                {
                    MobAI mobAI = enemy.GetComponent<MobAI>();
                    if (mobAI != null)
                    {
                        mobAI.TakeDamage(swordDamage);
                    }
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 center = transform.position + transform.right * 0.5f;
        Gizmos.DrawWireSphere(center, attackRadius);

        Vector3 leftBoundary = center + Quaternion.Euler(0, 0, -attackAngle / 2f) * transform.right * attackRadius;
        Vector3 rightBoundary = center + Quaternion.Euler(0, 0, attackAngle / 2f) * transform.right * attackRadius;

        Gizmos.DrawLine(center, leftBoundary);
        Gizmos.DrawLine(center, rightBoundary);
        Gizmos.DrawLine(leftBoundary, rightBoundary);
    }

    private void UpdateTorchLuminosity()
    {
        torchLuminosity -= torchDecayRate * Time.deltaTime;
        torchLuminosity = Mathf.Clamp(torchLuminosity, 0f, maxTorchLuminosity);

        foreach (Light torchLight in activeTorches)
        {
            if (torchLight != null)
            {
                torchLight.intensity = torchLuminosity;
            }
        }
        activeTorches.RemoveAll(light => light == null);
    }
}