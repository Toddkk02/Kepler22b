using UnityEngine;
using UnityEngine.UI;  // Necessario per gestire l'UI

public class PlayerHealthUI : MonoBehaviour
{
    public PlayerController player;  // Riferimento al player
    public Image[] healthSegments;  // Array dei segmenti della barra della vita

    // Update is called once per frame
    void Update()
    {
        UpdateHealthBar();
    }

    // Metodo per aggiornare la barra della vita
    void UpdateHealthBar()
    {
        int healthPerSegment = player.maxHealth / healthSegments.Length;  // Quanta vita rappresenta ciascun segmento

        for (int i = 0; i < healthSegments.Length; i++)
        {
            if (player.health > i * healthPerSegment)
            {
                healthSegments[i].enabled = true;  // Mostra il segmento
            }
            else
            {
                healthSegments[i].enabled = false;  // Nascondi il segmento se la vita è troppo bassa
            }
        }
    }
}

