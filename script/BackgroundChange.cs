using UnityEngine;

public class BackgroundChange : MonoBehaviour
{
    public int biomeOffset; // Identificatore del bioma (0 = Snow, 1 = Desert, 2 = Grass)
    public SpriteRenderer backgroundRenderer; // SpriteRenderer per lo sfondo
    public Texture2D BackgroundTextureForest;
    public Texture2D BackgroundTextureSnow;
    public Texture2D BackgroundTextureDesert;
    public Camera mainCamera; // Riferimento alla camera principale
    public Vector3 cameraOffset; // Offset per regolare la posizione dello sfondo rispetto alla camera

    private void Start()
    {
    }

    private void Update()
    {
        if (mainCamera != null)
        {
            FollowCamera();
        }
    }

    public void ChangeBackground()
    {
        if (backgroundRenderer == null) return;

        switch (biomeOffset)
        {
            case 0:
                backgroundRenderer.sprite = TextureToSprite(BackgroundTextureSnow);
                break;
            case 1:
                backgroundRenderer.sprite = TextureToSprite(BackgroundTextureDesert);
                break;
            case 2:
                backgroundRenderer.sprite = TextureToSprite(BackgroundTextureForest);
                break;
        }
    }

    private Sprite TextureToSprite(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    private void FollowCamera()
    {
        if (mainCamera == null) return;

        // Imposta la posizione dello sfondo in base alla posizione della camera
        Vector3 newPosition = mainCamera.transform.position + cameraOffset;
        newPosition.z = transform.position.z; // Assicurati che la posizione z rimanga costante
        transform.position = newPosition;
    }
}