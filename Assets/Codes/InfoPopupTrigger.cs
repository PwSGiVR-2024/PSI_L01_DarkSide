using UnityEngine;

public class InfoPopupTrigger : MonoBehaviour
{
    public GameObject popupUI; 
    private bool playerInside = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            popupUI.SetActive(true);
            playerInside = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            popupUI.SetActive(false);
            playerInside = false;
        }
    }
}
