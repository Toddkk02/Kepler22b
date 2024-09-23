using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MobAI : MonoBehaviour
{
    private PlayerController player;
    public GameObject dropitemPrefab;

    private float speed;
    private Rigidbody2D rb;
    public float jumpForce = 9f;
    public bool onGround;

    public float obstacleCheckDistance = 1f;
    public float groundCheckDistance = 0.1f;

    private float timeOutOfView = 0f;
    public float maxTimeOutOfView = 20f;
    private Camera mainCamera;

    public float knockbackForce = 4f;
    public float maxHealth = 20f;
    public float mobDamage = 10f;
    public bool IsSolidEnemy;
    private float currentHealth;
    private bool isJumping = false;
    private float jumpCooldown = 0.5f;
    private float jumpTimer = 0f;
    private bool canBeHit = true;
    private float hitCooldown = 0.5f;

    // Variables for the UI-based HealthBar
    public GameObject SecondaryRareDropItem;
    public float possibilityToDropWeapon;
    public Sprite SecondaryItemSprite;

    private Canvas healthBarCanvas;
    private RectTransform healthBarRect;
    private Image healthBarImage;
    private Gradient healthBarGradient;
    public float healthBarYOffset = 1f; // Distance above the mob
    public Vector2 healthBarSize = new Vector2(1f, 0.1f); // Width and height of the health bar

    // Variables for jump behavior
    private float stationaryTimer = 0f;
    private Vector2 lastPosition;

    public void Initialize(PlayerController playerController, float movementSpeed)
    {
        player = playerController;
        speed = movementSpeed;
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        currentHealth = maxHealth;
        lastPosition = transform.position;

        InitializeHealthBar();
    }

    void InitializeHealthBar()
    {
        // Create a new Canvas for the HealthBar
        GameObject canvasObj = new GameObject("HealthBarCanvas");
        canvasObj.transform.SetParent(transform); // Parent to the mob
        healthBarCanvas = canvasObj.AddComponent<Canvas>();
        healthBarCanvas.renderMode = RenderMode.WorldSpace;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Create the HealthBar image
        GameObject healthBarObj = new GameObject("HealthBar");
        healthBarObj.transform.SetParent(canvasObj.transform, false);
        healthBarImage = healthBarObj.AddComponent<Image>();
        healthBarRect = healthBarImage.rectTransform;

        // Set the size and position of the HealthBar
        healthBarRect.sizeDelta = healthBarSize;
        healthBarRect.anchorMin = new Vector2(0.5f, 0.5f);
        healthBarRect.anchorMax = new Vector2(0.5f, 0.5f);
        healthBarRect.pivot = new Vector2(0.5f, 0.5f);

        // Set up the gradient for the health bar color
        healthBarGradient = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[3];
        colorKeys[0] = new GradientColorKey(Color.red, 0.0f);
        colorKeys[1] = new GradientColorKey(Color.yellow, 0.5f);
        colorKeys[2] = new GradientColorKey(Color.green, 1.0f);
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0] = new GradientAlphaKey(1.0f, 0.0f);
        alphaKeys[1] = new GradientAlphaKey(1.0f, 1.0f);
        healthBarGradient.SetKeys(colorKeys, alphaKeys);

        UpdateHealthBar();
        UpdateHealthBarPosition();
    }

    void Update()
    {
        FollowPlayer();
        HandleMovement();
        CheckOutOfView();
        UpdateStationaryTimer();
        HandleJumping();
        UpdateHealthBarPosition();
    }

    void FollowPlayer()
    {
        Vector2 playerPosition = player.transform.position;
        Vector2 mobPosition = transform.position;

        Vector2 direction = playerPosition - mobPosition;
        float distance = direction.magnitude;

        if (distance > 0.1f)
        {
            direction = direction.normalized;
        }
    }

    void HandleMovement()
    {
        Vector2 playerPosition = player.transform.position;
        Vector2 mobPosition = transform.position;
        Vector2 direction = (playerPosition - mobPosition).normalized;

        float mobDirection = Mathf.Sign(direction.x);
        transform.localScale = new Vector3(mobDirection, 1, 1);

        RaycastHit2D obstacleHit = Physics2D.Raycast(transform.position, Vector2.right * mobDirection, obstacleCheckDistance, LayerMask.GetMask("Ground"));
        Debug.DrawRay(transform.position, Vector2.right * mobDirection * obstacleCheckDistance, Color.red);

        if (obstacleHit.collider == null)
        {
            transform.position = Vector2.MoveTowards(mobPosition, playerPosition, speed * Time.deltaTime);
        }
    }

    void UpdateStationaryTimer()
    {
        if (Vector2.Distance(transform.position, lastPosition) < 0.01f)
        {
            stationaryTimer += Time.deltaTime;
        }
        else
        {
            stationaryTimer = 0f;
        }

        lastPosition = transform.position;
    }

    void HandleJumping()
    {
        if (isJumping)
        {
            jumpTimer += Time.deltaTime;
            if (jumpTimer >= jumpCooldown)
            {
                isJumping = false;
                jumpTimer = 0f;
            }
        }
        else if (onGround && stationaryTimer >= 0.5f)
        {
            Jump();
            isJumping = true;
            jumpTimer = 0f;
            stationaryTimer = 0f;
        }
    }

    void Jump()
    {
        if (rb != null)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    void CheckOutOfView()
    {
        Vector3 screenPoint = mainCamera.WorldToViewportPoint(transform.position);
        bool onScreen = screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

        if (!onScreen)
        {
            timeOutOfView += Time.deltaTime;
            if (timeOutOfView >= maxTimeOutOfView)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            timeOutOfView = 0;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            onGround = true;
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            player.TakeDamage((int)mobDamage, Vector2.one, knockbackForce);
        }
    }

    public bool TakeDamage(float damage)
    {
        if (!canBeHit) return false;

        currentHealth -= damage;
        Debug.Log($"{gameObject.name} ha subito {damage} danni. Vita rimanente: {currentHealth}");

        UpdateHealthBar();

        canBeHit = false;
        StartCoroutine(ResetHitCooldown());

        if (currentHealth <= 0)
        {
            HandleDeath();
        }

        return true;
    }

    void UpdateHealthBar()
    {
        if (healthBarImage != null)
        {
            float healthPercentage = currentHealth / maxHealth;
            healthBarImage.fillAmount = healthPercentage;
            healthBarImage.color = healthBarGradient.Evaluate(healthPercentage);
        }
    }

    void UpdateHealthBarPosition()
    {
        if (healthBarCanvas != null && healthBarRect != null)
        {
            Vector3 mobPosition = transform.position;
            healthBarCanvas.transform.position = new Vector3(mobPosition.x, mobPosition.y + healthBarYOffset, mobPosition.z);
            healthBarCanvas.transform.rotation = Quaternion.identity; // Ensure the health bar is always facing the camera
        }
    }

    private IEnumerator ResetHitCooldown()
    {
        yield return new WaitForSeconds(hitCooldown);
        canBeHit = true;
    }

    private void HandleDeath()
    {
        Debug.Log($"Il mob {gameObject.name} è morto.");
        DropItem(); // Droppa gli oggetti prima di distruggere il gameObject
        Destroy(gameObject); // Distrugge il mob
    }

    private void DropItem()
{
    float probability = Random.Range(0f, 100f);
    if (dropitemPrefab != null)
    {
        Debug.Log($"Dropping item: {dropitemPrefab.name}");

        GameObject droppedItem = Instantiate(dropitemPrefab, transform.position, Quaternion.identity);

        TileDropController tileDrop = droppedItem.GetComponent<TileDropController>();
        if (tileDrop != null)
        {
            ItemClass droppedItemData;

            if (dropitemPrefab.name == "HotShell")
            {
                droppedItemData = new ItemClass("Hot Shell", dropitemPrefab.GetComponent<SpriteRenderer>().sprite, false, true, ItemClass.ItemType.Resource, 1);
            }
            else if (dropitemPrefab.name == "Leather")
            {
                droppedItemData = new ItemClass("Leather", dropitemPrefab.GetComponent<SpriteRenderer>().sprite, false, true, ItemClass.ItemType.Resource, 1);
            }
            else if (dropitemPrefab.name == "Fiber")
            {
                droppedItemData = new ItemClass("Fiber", dropitemPrefab.GetComponent<SpriteRenderer>().sprite, false, true, ItemClass.ItemType.Resource, 1);
            }
            else if (dropitemPrefab.name == "RustyKnife" && probability <= 5f)
            {
                droppedItemData = new ToolClass("Rusty Knife", dropitemPrefab.GetComponent<SpriteRenderer>().sprite, ItemClass.ToolType.Sword, 9f, 1f);
             
            }
            else if(dropitemPrefab.name == "MemoryFragment" && probability <= 35f)
            {
                droppedItemData = new ItemClass("MemoryFragment", dropitemPrefab.GetComponent<SpriteRenderer>().sprite, false, true, ItemClass.ItemType.Resource, 1);
            }
            else if (dropitemPrefab.name == "EtherResidue")
            {
                droppedItemData = new ItemClass("EtherResidue", dropitemPrefab.GetComponent<SpriteRenderer>().sprite, false, true, ItemClass.ItemType.Resource, 1);
            }
            else
            {
                // Handle other cases or throw an exception
                return;
            }

            tileDrop.SetItem(droppedItemData);
        }
        else
        {
            Debug.LogError("Il prefab droppato non ha il componente TileDropController!");
        }
    }
    else
    {
        Debug.LogWarning("dropitemPrefab non impostato. Nessun item droppato.");
    }
}
}