using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Homebase : MonoBehaviour
{
    public static Homebase Instance { get; private set; }

    public Vector3 HomePos => transform.position;

    private LineRenderer _playerBaseLine;

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

        _playerBaseLine = transform.Find("Trail").GetComponent<LineRenderer>();
        _playerBaseLine.SetPosition(0, transform.position);
    }

    private void Update()
    {
        if (_playerBaseLine != null && Player.Instance != null)
        {
            _playerBaseLine.SetPosition(1, Player.Instance.transform.position);
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
