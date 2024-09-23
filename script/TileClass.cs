using UnityEngine;

[System.Serializable]
public class TileClass
{
    public enum TileType
    {
        Grass,
        Stone,
        Dirt,
        Snow,
        Sand,
        Log,
        Leaf,
        Cactus,
        Coal,
        Iron,
        Gold,
        Diamond,
        BackgroundStone,
        BackgroundDirt,
        BackgroundSand,
        WoodBackground
    }

    public TileType type;
    public Sprite sprite;
    public bool isBackgroundTile;
    internal string name;
    internal bool isStackable;
    internal Sprite tileDrop;
    internal bool isPlaceable;

    public TileClass(TileType type, Sprite sprite, bool isBackgroundTile)
    {
        this.type = type;
        this.sprite = sprite;
        this.isBackgroundTile = isBackgroundTile;
    }
}
