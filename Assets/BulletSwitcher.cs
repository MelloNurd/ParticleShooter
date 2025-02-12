using UnityEngine;

public class BulletSwitcher : MonoBehaviour
{
    [SerializeField] int blasterType;  // 0 = standard, 1 = fire
    // Called when another collider enters this trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))  // Assuming your ship has the tag "Player"
        {
            // Call a method on the ship to modify it
            other.GetComponent<PlayerMovement>().swapBlaster(blasterType);  // Assuming 'Ship' is the script on the ship
        }
    }
}
