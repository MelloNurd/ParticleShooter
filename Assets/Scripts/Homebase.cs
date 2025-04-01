using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Homebase : MonoBehaviour
{
    public static Homebase Instance { get; private set; }

    public Vector3 HomePos => transform.position;

    private void Awake()
    {
        // Singleton Implementation
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            SceneManager.LoadScene("Homebase");
        }
    }
}
