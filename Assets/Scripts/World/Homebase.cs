using System.ComponentModel;
using System.Globalization;
using TMPro;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Homebase : MonoBehaviour
{
    public static Homebase Instance { get; private set; }

    public Vector3 HomePos => transform.position;

    private TMP_Text _distanceText;
    private GameObject _outlinePivot;
    private Image _distanceOutline;

    public static UnityEvent EnterHomebase = new();
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

        _distanceText = transform.Find("UI").GetComponentInChildren<TMP_Text>();
        _outlinePivot = _distanceText.transform.GetChild(0).gameObject;
        _distanceOutline = _outlinePivot.transform.GetChild(0).GetComponent<Image>();

        GetComponentInChildren<Canvas>().worldCamera = Camera.main;
    }

    private void Update()
    {
        if (Player.Instance != null)
        {
            Vector3 playerPos = Player.Instance.transform.position;

            Vector3 direction = (transform.position - playerPos).normalized;
            Vector3 point = Utilities.GetScreenEdgePosition(playerPos, direction);

            float alpha = Mathf.Lerp(-0.2f, 0.2f, Vector3.Distance(playerPos, transform.position) * 0.02f);

            _distanceText.text = $"{Vector3.Distance(playerPos, transform.position):F0}m";
            _distanceText.alpha = alpha * 2f;

            _outlinePivot.transform.up = direction;

            _distanceOutline.color = new Color(1, 1, 1, alpha);

            _distanceText.transform.position = point - direction * 1.5f;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            SceneManager.LoadScene("Homebase");
            EnterHomebase?.Invoke();
        }
    }
}
