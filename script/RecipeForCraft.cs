using UnityEngine;

[System.Serializable]
public class Recipe
{
    public Sprite RecipeSprite { get; private set; }
    public int CraftedQuantity { get; private set; }
    public ItemClass[] RequiredItems { get; private set; }
    public ItemClass CraftedItem { get; private set; }

    public Recipe(Sprite recipeSprite, int craftedQuantity, ItemClass[] requiredItems, ItemClass craftedItem)
    {
        RecipeSprite = recipeSprite;
        CraftedQuantity = craftedQuantity;
        RequiredItems = requiredItems;
        CraftedItem = craftedItem;
    }
}