using System;
using UnityEngine;


[System.Serializable]
public class InventorySlot
{
    public ItemClass item;
    public Vector2Int position;
    public int quantity;
       public Vector2Int SlotPosition;

    public void SetPosition(int x, int y)
    {
        SlotPosition = new Vector2Int(x, y);
    }

    public static implicit operator InventorySlot(ItemClass v)
    {
        throw new NotImplementedException();
    }
}
