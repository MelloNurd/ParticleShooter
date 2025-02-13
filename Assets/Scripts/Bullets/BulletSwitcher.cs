using UnityEngine;

public class BulletSwitcher : MonoBehaviour
{
    [SerializeField] int blasterType;  // 0 = standard, 1 = fire
    // Called when another collider enters this trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Collision with player
        if (other.CompareTag("Player"))
        {
            // Basic Ship
            if(other.GetComponent<Player>())
            {
                other.GetComponent<Player>().swapBlaster(blasterType);
            }
            // ShipM2
            else if (other.GetComponent<PlayerM2>())
            {
                other.GetComponent<PlayerM2>().swapBlaster(blasterType);
            }
        }
    }
}
