using UnityEngine;

public class TileDropController : MonoBehaviour
{
     private ItemClass item;
    public SpriteRenderer spriteRenderer;
    private bool isCollected = false;  // Flag per prevenire raccolta multipla

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!isCollected && col.CompareTag("Player"))  // Controlla che non sia già stato raccolto
        {
            Inventory playerInventory = col.GetComponent<Inventory>();
            if (playerInventory != null && item != null)
            {
                if (playerInventory.Add(item))  // Aggiungi l'oggetto all'inventario del giocatore
                {
                    isCollected = true;  // Imposta il flag per prevenire raccolte multiple
                    Debug.Log("TileDrop raccolto: " + item.name);
                    Destroy(gameObject);  // Distruggi il gameObject dopo la raccolta
                }
            }
        }
    }

   public void SetItem(ItemClass newItem)
    {
        item = newItem;
        if (spriteRenderer != null && item != null)
        {
            spriteRenderer.sprite = item.sprite;
        }
    }

    public ItemClass GetItem()
    {
        return item;
}
}