using UnityEngine;

public class Door : MonoBehaviour
{
    private bool isOpen = false;
    public GameObject openDoorSprite;
    public GameObject closedDoorSprite;
    private BoxCollider2D triggerCollider;
    private BoxCollider2D solidCollider;
    private GameObject player;

    void Start()
    {
        // Aggiungiamo due collider: uno trigger e uno solido
        triggerCollider = gameObject.AddComponent<BoxCollider2D>();
        triggerCollider.isTrigger = true;

        solidCollider = gameObject.AddComponent<BoxCollider2D>();
        solidCollider.isTrigger = false;

        isOpen = false;
        UpdateDoorState();

        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player non trovato nella scena. Assicurati che abbia il tag 'Player'.");
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            OpenDoor();
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            CloseDoor();
        }
    }

    void OpenDoor()
    {
        isOpen = true;
        UpdateDoorState();
        Debug.Log("La porta si è aperta");
    }

    void CloseDoor()
    {
        isOpen = false;
        UpdateDoorState();
        Debug.Log("La porta si è chiusa");
    }

    void UpdateDoorState()
    {
        openDoorSprite.SetActive(isOpen);
        closedDoorSprite.SetActive(!isOpen);
        
        // Attiviamo/disattiviamo il collider solido in base allo stato della porta
        solidCollider.enabled = !isOpen;
        
        Debug.Log("Stato della porta: " + (isOpen ? "Aperta" : "Chiusa"));
    }
}