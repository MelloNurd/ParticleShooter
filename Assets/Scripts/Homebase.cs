using System.ComponentModel;
using System.Globalization;
using TMPro;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Homebase : MonoBehaviour
{
    public static Homebase Instance { get; private set; }

    public Vector3 HomePos => transform.position;

    private LineRenderer _playerBaseLine;
    private Material _lineMaterial;
    private TMP_Text _distanceText;

    [SerializeField] private GameObject _test;

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

        _playerBaseLine = transform.Find("Trail").GetComponent<LineRenderer>();
        _distanceText = transform.Find("UI").GetComponentInChildren<TMP_Text>();
        _lineMaterial = _playerBaseLine.material;
        _playerBaseLine.SetPosition(0, transform.position);
    }

    private void Update()
    {
        if (_playerBaseLine != null && Player.Instance != null)
        {
            Vector3 playerPos = Player.Instance.transform.position;
            Vector3 direction = (transform.position - playerPos).normalized;
            Vector3 point = Utilities.GetScreenEdgePosition(playerPos, direction);

            float horizontalOffset = Mathf.Abs(point.x - playerPos.x)*0.125f;
            Vector3 targetPos = (Vector2.Distance(point - direction * 1.75f, playerPos) < Vector2.Distance(point - direction * horizontalOffset, playerPos))
                ? point - direction * 1.75f
                : point - direction * horizontalOffset;

            //_test1.transform.position = point - direction;
            //_test2.transform.position = point - direction * horizontalOffset;
            _playerBaseLine.SetPosition(0, targetPos); // Screen point
            _playerBaseLine.SetPosition(1, playerPos + direction); // Player point

            float alpha = Mathf.Lerp(-0.2f, 0.2f, Vector3.Distance(playerPos, transform.position) * 0.02f);

            _playerBaseLine.colorGradient = new Gradient
            {
                colorKeys = new[]
                {
                    new GradientColorKey(Color.white, 0)
                },
                alphaKeys = new[]
                {
                    new GradientAlphaKey(alpha, 0)
                }
            };
            _distanceText.text = $"{Vector3.Distance(playerPos, transform.position):F0}m";
            _distanceText.alpha = alpha * 2f;

            _distanceText.transform.position = point - direction;
        }
    }

    private void FixedUpdate()
    {
        if (_lineMaterial != null)
        {
            _lineMaterial.SetTextureOffset("_MainTex", new Vector2(Time.time * 0.5f, 0));
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
