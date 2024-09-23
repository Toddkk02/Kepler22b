using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingSystem : MonoBehaviour
{
    public GameObject craftingSlotPrefab;
    public GameObject craftingUI;
    public Inventory inventory;
    public GameObject torchPrefab;  // Prefab della torcia

    public List<Recipe> recipes = new List<Recipe>();
    private List<GameObject> craftingSlots = new List<GameObject>();
    private Dictionary<string, GameObject> itemPrefabDictionary;
    public LayerMask placementLayerMask;
    private int currentSlotIndex = 0;

    public Sprite torchSprite, woodSprite, hotShellSprite, woodBackgroundSprite, doorSprite, furnaceSprite, craftingStationSprite, stoneSprite;

    void Start()
    {
        itemPrefabDictionary = new Dictionary<string, GameObject>();
        if (inventory == null)
        {
            Debug.LogWarning("Manca il riferimento all'inventario. Provo a trovarlo...");
            inventory = FindObjectOfType<Inventory>();

            if (inventory == null)
            {
                Debug.LogError("Impossibile trovare l'inventario nella scena.");
                return;
            }
        }
        itemPrefabDictionary.Add("Torch", torchPrefab);

        AddCraftingRecipes();
        CreateCraftingSlots();
        ShowCurrentSlot();
    }

    void AddCraftingRecipes()
    {
        recipes.Add(new Recipe(
            torchSprite,
            4,
            new ItemClass[] { 
                new ItemClass("Wood", woodSprite, true, true, ItemClass.ItemType.Block, 1), 
                new ItemClass("Hot Shell", hotShellSprite, false, true, ItemClass.ItemType.Tool, 1) 
            },
            new ItemClass("Torch", torchSprite, true, true, ItemClass.ItemType.Block, 4)
        ));

        recipes.Add(new Recipe(
            woodBackgroundSprite,
            8,
            new ItemClass[] { 
                new ItemClass("Wood", woodSprite, true, true, ItemClass.ItemType.Block, 4) 
            },
            new ItemClass("WoodBackground", woodBackgroundSprite, true, true, ItemClass.ItemType.Block, 8)
        ));

        recipes.Add(new Recipe(
            doorSprite,
            1,
            new ItemClass[] { 
                new ItemClass("Wood", woodSprite, true, true, ItemClass.ItemType.Block, 8) 
            },
            new ItemClass("Door", doorSprite, true, true, ItemClass.ItemType.Block, 1)
        ));

        recipes.Add(new Recipe(
            furnaceSprite,
            1,
            new ItemClass[] { 
                new ItemClass("Stone", stoneSprite, true, true, ItemClass.ItemType.Block, 8) 
            },
            new ItemClass("Furnace", furnaceSprite, true, true, ItemClass.ItemType.Block, 1)
        ));

        recipes.Add(new Recipe(
            craftingStationSprite,
            1,
            new ItemClass[] { 
                new ItemClass("Wood", woodSprite, true, true, ItemClass.ItemType.Block, 10) 
            },
            new ItemClass("Crafting Station", craftingStationSprite, true, true, ItemClass.ItemType.Block, 1)
        ));
    }

    void CreateCraftingSlots()
    {
        foreach (var slot in craftingSlots)
        {
            Destroy(slot);
        }
        craftingSlots.Clear();

        for (int i = 0; i < recipes.Count; i++)
        {
            GameObject slotObject = Instantiate(craftingSlotPrefab, craftingUI.transform);
            RectTransform slotRect = slotObject.GetComponent<RectTransform>();
            slotRect.anchoredPosition = new Vector2(0, 0);

            Image itemImage = slotObject.transform.Find("ItemImage").GetComponent<Image>();
            Text itemNameText = slotObject.transform.Find("ItemName").GetComponent<Text>();
            Button craftButton = slotObject.transform.Find("CraftButton").GetComponent<Button>();
            Button nextButton = slotObject.transform.Find("NextButton").GetComponent<Button>();
            Button previousButton = slotObject.transform.Find("PreviousButton").GetComponent<Button>();

            if (itemImage != null) itemImage.sprite = recipes[i].CraftedItem.sprite;
            if (itemNameText != null) itemNameText.text = recipes[i].CraftedItem.name;

            if (craftButton != null)
            {
                int index = i;
                craftButton.onClick.AddListener(() => CraftItem(index));
            }

            if (nextButton != null) nextButton.onClick.AddListener(ShowNextSlot);
            if (previousButton != null) previousButton.onClick.AddListener(ShowPreviousSlot);

            craftingSlots.Add(slotObject);
            slotObject.SetActive(false);
        }

        ShowCurrentSlot();
    }

    void ShowCurrentSlot()
    {
        if (craftingSlots.Count == 0) return;

        foreach (var slot in craftingSlots)
        {
            slot.SetActive(false);
        }

        currentSlotIndex = Mathf.Clamp(currentSlotIndex, 0, craftingSlots.Count - 1);
        craftingSlots[currentSlotIndex].SetActive(true);
    }

    public void ShowNextSlot()
    {
        currentSlotIndex = (currentSlotIndex + 1) % craftingSlots.Count;
        ShowCurrentSlot();
    }

    public void ShowPreviousSlot()
    {
        currentSlotIndex = (currentSlotIndex - 1 + craftingSlots.Count) % craftingSlots.Count;
        ShowCurrentSlot();
    }

    public void CraftItem(int recipeIndex)
    {
        if (recipeIndex < 0 || recipeIndex >= recipes.Count)
        {
            Debug.LogError("Indice della ricetta non valido");
            return;
        }

        Recipe recipe = recipes[recipeIndex];

        if (CanCraftItem(recipe))
        {
            foreach (var ingredient in recipe.RequiredItems)
            {
                RemoveItemFromInventory(ingredient.name, ingredient.quantity);
            }

            AddItemToInventory(new ItemClass(recipe.CraftedItem));

            if (itemPrefabDictionary.ContainsKey(recipe.CraftedItem.name))
            {
                GameObject craftedPrefab = itemPrefabDictionary[recipe.CraftedItem.name];
                Instantiate(craftedPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                Debug.Log($"Craftato {recipe.CraftedItem.quantity}x {recipe.CraftedItem.name} con prefab associato.");
            }
            else
            {
                Debug.LogWarning($"Non è stato trovato nessun prefab per {recipe.CraftedItem.name}");
            }
        }
        else
        {
            Debug.Log($"Materiali insufficienti per craftare {recipe.CraftedItem.name}");
        }
    }

    bool CanCraftItem(Recipe recipe)
    {
        foreach (var ingredient in recipe.RequiredItems)
        {
            if (!HasEnoughItems(ingredient.name, ingredient.quantity))
            {
                return false;
            }
        }
        return true;
    }

    public bool HasEnoughItems(string itemName, int requiredQuantity)
    {
        int totalQuantity = 0;

        for (int y = 0; y < inventory.inventoryHeight; y++)
        {
            for (int x = 0; x < inventory.inventoryWidth; x++)
            {
                InventorySlot slot = inventory.inventorySlots[x, y];
                if (slot != null && slot.item != null && slot.item.name == itemName)
                {
                    totalQuantity += slot.quantity;
                    if (totalQuantity >= requiredQuantity)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    void RemoveItemFromInventory(string itemName, int quantity)
    {
        for (int x = 0; x < inventory.inventoryWidth; x++)
        {
            for (int y = 0; y < inventory.inventoryHeight; y++)
            {
                InventorySlot slot = inventory.inventorySlots[x, y];
                if (slot != null && slot.item != null && slot.item.name == itemName)
                {
                    if (slot.quantity > quantity)
                    {
                        slot.quantity -= quantity;
                        inventory.UpdateInventoryUI();
                        return;
                    }
                    else if (slot.quantity == quantity)
                    {
                        inventory.inventorySlots[x, y] = null;
                        inventory.UpdateInventoryUI();
                        return;
                    }
                    else
                    {
                        quantity -= slot.quantity;
                        inventory.inventorySlots[x, y] = null;
                        inventory.UpdateInventoryUI();
                    }
                }
            }
        }
    }

    void AddItemToInventory(ItemClass item)
    {
        for (int x = 0; x < inventory.inventoryWidth; x++)
        {
            for (int y = 0; y < inventory.inventoryHeight; y++)
            {
                InventorySlot slot = inventory.inventorySlots[x, y];

                if (slot == null || slot.item == null)
                {
                    inventory.inventorySlots[x, y] = new InventorySlot
                    {
                        item = new ItemClass(item),
                        quantity = item.quantity
                    };
                    inventory.UpdateInventoryUI();
                    Debug.Log($"Aggiunto {item.quantity}x {item.name} allo slot {x}, {y}");
                    return;
                }
                else if (slot.item.name == item.name && slot.item.isStackable)
                {
                    slot.quantity += item.quantity;
                    inventory.UpdateInventoryUI();
                    Debug.Log($"Aggiunto {item.quantity}x {item.name} allo slot {x}, {y} (Totale: {slot.quantity})");
                    return;
                }
            }
        }
        Debug.LogWarning("Inventario pieno. Impossibile aggiungere l'oggetto.");
    }
}
