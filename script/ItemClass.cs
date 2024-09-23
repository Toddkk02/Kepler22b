using System;
using UnityEngine;

[System.Serializable]
public class ItemClass
{
    public enum ItemType { Tool, Block, Resource }
    public enum ToolType { Pickaxe, Axe, Hammer, Sword }

    public ItemType itemType;
    public ToolType toolType;
    public string name;
    public Sprite sprite;
    public bool isStackable;
    public bool isPlaceable;  // Aggiungi questa proprietà
    public int quantity;
    public TileClass tile;
    public ToolClass tool;

    // Constructor for blocks
    public ItemClass(string _name, Sprite _sprite, bool _isPlaceable, bool _isStackable, ItemType _itemType, int _quantity)
    {
        name = _name;
        sprite = _sprite;
        isStackable = _isStackable;
        isPlaceable = _isPlaceable;  // Aggiungi questo assegnamento
        itemType = _itemType;
        quantity = _quantity;

        Debug.Log("Blocco creato: " + name + " | Tipo: " + itemType);
    }

    // Constructor for tools
    public ItemClass(ToolClass _tool)
    {
        itemType = ItemType.Tool;
        tool = _tool;
        name = _tool.name;
        sprite = _tool.sprite;
        isStackable = false;
        toolType = _tool.toolType;
        isPlaceable = false;  // Gli strumenti non sono piazzabili per default

        Debug.Log("Strumento creato: " + name + " | Tipo: " + toolType);
    }

    // Constructor for blocks with TileClass
    public ItemClass(TileClass _tile)
    {
        name = _tile.name;
        sprite = _tile.tileDrop;
        isStackable = _tile.isStackable;
        isPlaceable = _tile.isPlaceable;  // Usa la proprietà del TileClass per verificare se è piazzabile
        itemType = ItemType.Block;
        tile = _tile;
        quantity = 1;

        Debug.Log("Blocco creato: " + name + " | Tipo: " + itemType);
    }

    // Copy constructor
    public ItemClass(ItemClass other)
    {
        itemType = other.itemType;
        toolType = other.toolType;
        name = other.name;
        sprite = other.sprite;
        isStackable = other.isStackable;
        isPlaceable = other.isPlaceable;  // Copia il valore di isPlaceable
        quantity = other.quantity;
        tile = other.tile;
        tool = other.tool;

        Debug.Log("Item copiato: " + name + " | Tipo: " + itemType);
    }

    // Metodo per ottenere il danno dello strumento, se presente
    public float GetToolDamage()
    {
        if (tool != null)
        {
            return tool.damage;
        }
        return 0f; 
    }

    public static implicit operator ItemClass(ToolClass v)
    {
        throw new NotImplementedException();
    }
}


