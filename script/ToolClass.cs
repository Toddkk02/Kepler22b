using UnityEngine;

[System.Serializable]
public class ToolClass
{
    public string name;
    public Sprite sprite;
    public ItemClass.ToolType toolType;
    public float damage;
    public float attackSpeed;

    // Costruttore che accetta tutti i parametri
    public ToolClass(string _name, Sprite _sprite, ItemClass.ToolType _toolType, float _damage, float _attackSpeed)
    {
        name = _name;
        sprite = _sprite;
        toolType = _toolType;
        damage = _damage;
        attackSpeed = _attackSpeed;

        Debug.Log("Strumento creato: " + name + " | Danno: " + damage + " | Velocità: " + attackSpeed);
    }
    public ItemClass.ToolType type;

    public ToolClass(ItemClass.ToolType type)
    {
        this.type = type;
    }
}

