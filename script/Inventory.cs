using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Inventory : MonoBehaviour
{
    public int stackLimit = 999;
    public Vector2 hotbarOffset;
    public Vector2 Inventoryoffset;
    public Vector2 multiplier;
    public GameObject inventoryUI;
    public GameObject HotBarUI;
    public GameObject inventorySlotPrefab;
    public int inventoryWidth;
    public int inventoryHeight;
    public InventorySlot[,] inventorySlots;
    public GameObject[,] uiSlots;
    public GameObject[] hotbarUISlot;
    private InventorySlot draggedItem;
    
    private Vector2Int draggedItemOriginalPosition;
    public Sprite SwordSprite;
    public Sprite PickAxeSprite;
    public Sprite AxeSprite;
    public Sprite HammerSprite;

    void Start()
    {   
        
        InitializeInventory();
        inventorySlots = new InventorySlot[inventoryWidth, inventoryHeight];
        uiSlots = new GameObject[inventoryWidth, inventoryHeight];
        hotbarUISlot = new GameObject[inventoryWidth];
        SetupUI();
        UpdateInventoryUI();
        
        // Creazione degli oggetti
        ToolClass Sword = new ToolClass("Wooden Sword", SwordSprite, ItemClass.ToolType.Sword, 5f, 1f);
        ItemClass WoodenSword = new ItemClass(Sword);
        ToolClass PickAxe = new ToolClass("Wooden Pickaxe", PickAxeSprite, ItemClass.ToolType.Pickaxe,1f, 0.6f);
        ItemClass WoodenPickaxe = new ItemClass(PickAxe);
        ToolClass Axe = new ToolClass("Wooden Axe", AxeSprite, ItemClass.ToolType.Axe, 6f, 0.2f);
        ItemClass WoodenAxe = new ItemClass(Axe);
        ToolClass Hammer = new ToolClass("Wooden Hammer", HammerSprite, ItemClass.ToolType.Hammer, 7f, 0.3f);
        ItemClass WoodenHammer = new ItemClass(Hammer);
        Add(WoodenSword);
        Add(WoodenPickaxe);
        Add(WoodenAxe);
        Add(WoodenHammer);
    }
    
    private void InitializeInventory()
    {
        inventorySlots = new InventorySlot[inventoryWidth, inventoryHeight];
        for (int y = 0; y < inventoryHeight; y++)
        {
            for (int x = 0; x < inventoryWidth; x++)
            {
                inventorySlots[x, y] = new InventorySlot();
            }
        }
        Debug.Log($"Inventario inizializzato con dimensioni: {inventoryWidth}x{inventoryHeight}");
    }
    public void RemoveSingle(ItemClass item)
{
    Vector2Int itemPos = Contains(item);
    if (itemPos != Vector2Int.one * -1)
    {
        InventorySlot slot = inventorySlots[itemPos.x, itemPos.y];
        slot.quantity--;

        if (slot.quantity <= 0)
        {
            inventorySlots[itemPos.x, itemPos.y] = null;
        }

        UpdateInventoryUI();
        Debug.Log("Single item removed from inventory: " + item.name);
    }
    else
    {
        Debug.LogWarning("Unable to remove item. Not found in inventory.");
    }
}


    void SetupUI()
    {
        for (int y = 0; y < inventoryHeight; y++)
        {
            for (int x = 0; x < inventoryWidth; x++)
            {
                GameObject inventorySlot = Instantiate(inventorySlotPrefab, inventoryUI.transform.GetChild(0).transform);
                
                float posX = (x * multiplier.x) + Inventoryoffset.x;
                float posY = -((y * multiplier.y) + Inventoryoffset.y);

                inventorySlot.GetComponent<RectTransform>().localPosition = new Vector3(posX, posY);
                uiSlots[x, y] = inventorySlot;

                // Aggiungi il button allo slot
                Button button = inventorySlot.AddComponent<Button>();
                int slotX = x;
                int slotY = y;
                button.onClick.AddListener(() => OnSlotClicked(slotX, slotY, Input.GetMouseButton(1))); // Aggiungi anche il controllo del tasto
            }
        }

        // Setup hotbar UI (condivisa con la prima riga dell'inventario)
        for (int x = 0; x < inventoryWidth; x++)
        {
            GameObject hotbarSlotObject = Instantiate(inventorySlotPrefab, HotBarUI.transform);
            hotbarSlotObject.GetComponent<RectTransform>().localPosition = new Vector3((x * multiplier.x) + hotbarOffset.x, hotbarOffset.y);
            hotbarUISlot[x] = hotbarSlotObject;

            // Aggiungi button anche alla hotbar
            Button hotbarButton = hotbarSlotObject.AddComponent<Button>();
            int slotX = x;
            hotbarButton.onClick.AddListener(() => OnSlotClicked(slotX, 0, Input.GetMouseButton(1))); // Click su hotbar
        }
    }

    public void OnSlotClicked(int x, int y, bool isRightClick)
    {
        if (draggedItem == null)
        {
            // Inizio del drag
            InventorySlot slot = inventorySlots[x, y];
            if (slot != null && slot.item != null)
            {
                if (isRightClick)
                {
                    // Se si clicca con il tasto destro, prendi solo un item
                    draggedItem = new InventorySlot
                    {
                        item = new ItemClass(slot.item), // Copia dell'item
                        quantity = 1
                    };
                    slot.quantity--; // Riduci la quantità nello slot di 1
                    if (slot.quantity <= 0)
                    {
                        inventorySlots[x, y] = null; // Rimuovi lo slot se non ci sono più item
                    }
                }
                else
                {
                    // Se si clicca con il tasto sinistro, prendi l'intero stack
                    draggedItem = slot;
                    draggedItemOriginalPosition = new Vector2Int(x, y);
                    inventorySlots[x, y] = null;
                }
            }
        }
        else
        {
            // Fine del drag: inserisci l'item nello slot di destinazione
            InventorySlot targetSlot = inventorySlots[x, y];

            if (targetSlot == null || targetSlot.item == null)
            {
                // Se lo slot di destinazione è vuoto, sposta l'item
                inventorySlots[x, y] = draggedItem;
                draggedItem = null;
            }
            else if (targetSlot.item.name == draggedItem.item.name && targetSlot.item.isStackable)
            {
                // Se gli item sono dello stesso tipo e impilabili, unisci gli stack
                if (isRightClick && targetSlot.quantity < stackLimit)
                {
                    targetSlot.quantity++; // Aggiungi solo un item con click destro
                    draggedItem.quantity--;

                    if (draggedItem.quantity <= 0)
                    {
                        draggedItem = null; // Se non ci sono più item nel drag, resettalo
                    }
                }
                else if (!isRightClick && targetSlot.quantity + draggedItem.quantity <= stackLimit)
                {
                    // Unisci completamente lo stack se si clicca con il sinistro
                    targetSlot.quantity += draggedItem.quantity;
                    draggedItem = null;
                }
                else if (!isRightClick && targetSlot.quantity + draggedItem.quantity > stackLimit)
                {
                    // Aggiungi la quantità massima possibile senza superare il limite di stack
                    int spaceLeft = stackLimit - targetSlot.quantity;
                    targetSlot.quantity += spaceLeft;
                    draggedItem.quantity -= spaceLeft;

                    if (draggedItem.quantity <= 0)
                    {
                        draggedItem = null;
                    }
                }
            }
            else
            {
                // Se gli item sono diversi, scambiali
                inventorySlots[x, y] = draggedItem;
                draggedItem = targetSlot;
            }
        }

        UpdateInventoryUI(); // Aggiorna l'interfaccia grafica
    }

    public void UpdateInventoryUI()
    {
        for (int y = 0; y < inventoryHeight; y++)
        {
            for (int x = 0; x < inventoryWidth; x++)
            {
                UpdateSlotUI(uiSlots[x, y], inventorySlots[x, y]);

                // Update hotbar UI (prima riga dell'inventario)
                if (y == 0)
                {
                    UpdateSlotUI(hotbarUISlot[x], inventorySlots[x, y]);
                }
            }
        }
    }

    private void UpdateSlotUI(GameObject slotObject, InventorySlot slot)
    {
        Image imageComponent = slotObject.transform.GetChild(0).GetComponent<Image>();
        TextMeshProUGUI textComponent = slotObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        if (slot == null || slot.item == null)
        {
            imageComponent.enabled = false;
            imageComponent.sprite = null;
            textComponent.text = "0";
            textComponent.enabled = false;
        }
        else
        {
            // Mostra lo sprite dell'oggetto
            imageComponent.enabled = true;
            imageComponent.sprite = slot.item.sprite;
            textComponent.text = slot.quantity.ToString();
            textComponent.enabled = true;
        }
    }

     public bool Add(ItemClass item, int quantity = 1)
    {
        // Cerca se l'oggetto esiste già nell'inventario
        Vector2Int itemPos = Contains(item);
        bool added = false;

        if (itemPos != Vector2Int.one * -1)
        {
            // L'oggetto esiste già, aggiungi alla quantità esistente
            InventorySlot slot = inventorySlots[itemPos.x, itemPos.y];
            int spaceLeft = stackLimit - slot.quantity;
            int amountToAdd = Mathf.Min(quantity, spaceLeft);
            
            if (amountToAdd > 0)
            {
                slot.quantity += amountToAdd;
                quantity -= amountToAdd;
                added = true;
            }
        }

        // Se c'è ancora quantità da aggiungere o l'oggetto non esisteva
        while (quantity > 0 && !added)
        {
            for (int y = 0; y < inventoryHeight; y++)
            {
                for (int x = 0; x < inventoryWidth; x++)
                {
                    if (inventorySlots[x, y] == null)
                    {
                        int amountToAdd = Mathf.Min(quantity, stackLimit);
                        inventorySlots[x, y] = new InventorySlot { item = item, quantity = amountToAdd };
                        quantity -= amountToAdd;
                        added = true;
                        break;
                    }
                }
                if (added) break;
            }
            if (!added) break; // Se non è stato possibile aggiungere, esci dal ciclo
        }

        UpdateInventoryUI();
        return added;
    }

    public Vector2Int Contains(ItemClass item)
    {
        for (int y = 0; y < inventoryHeight; y++)
        {
            for (int x = 0; x < inventoryWidth; x++)
            {
                if (inventorySlots[x, y] != null && inventorySlots[x, y].item.name == item.name)
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return Vector2Int.one * -1;
    }
        public void Remove(ItemClass item)
    {
        Vector2Int itemPos = Contains(item);
        if (itemPos != Vector2Int.one * -1)
        {
            InventorySlot slot = inventorySlots[itemPos.x, itemPos.y];
            slot.quantity--;

            if (slot.quantity <= 0)
            {
                inventorySlots[itemPos.x, itemPos.y] = null;
            }

            UpdateInventoryUI();  // Aggiorna l'interfaccia grafica dell'inventario
            Debug.Log("Oggetto rimosso dall'inventario: " + item.name);
        }
        else
        {
            Debug.LogWarning("Impossibile rimuovere l'oggetto. Non trovato nell'inventario.");
        }
    }
      public bool RemoveItemFromInventory(string itemName, int quantity)
    {
        for (int y = 0; y < inventoryHeight; y++)
        {
            for (int x = 0; x < inventoryWidth; x++)
            {
                InventorySlot slot = inventorySlots[x, y];
                if (slot != null && slot.item != null && slot.item.name == itemName)
                {
                    if (slot.quantity >= quantity)
                    {
                        slot.quantity -= quantity;
                        if (slot.quantity == 0)
                        {
                            inventorySlots[x, y] = null;
                        }
                        UpdateInventoryUI();
                        return true;
                    }
                    else
                    {
                        quantity -= slot.quantity;
                        inventorySlots[x, y] = null;
                        UpdateInventoryUI();
                    }
                }
            }
        }
        return false;
    }

}
