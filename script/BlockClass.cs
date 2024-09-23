using UnityEngine;

[System.Serializable]
public class Block
{
    public Sprite blockSprite;
    public string blockName;
    public int blockId;

    public Block(Sprite sprite, string name, int id)
    {
        blockSprite = sprite;
        blockName = name;
        blockId = id;
    }
}
