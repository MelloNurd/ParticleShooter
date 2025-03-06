using NaughtyAttributes;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [Scene] public string sceneName;
    [SerializeField] private GameObject Medkit;
    [SerializeField] private GameObject Expkit;

    private float lastSpawnTime;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lastSpawnTime = -2f; // Initialize to ensure the first spawn happens immediately
    }

    // Update is called once per frame
    void Update()
    {
        if (Timer.Instance.elapsedTime - lastSpawnTime >= 10f)
        {
            lastSpawnTime = Timer.Instance.elapsedTime;

            int random = Random.Range(1, 11);
            if (random < 6)
            {
                Instantiate(Medkit, ParticleManager.Instance.GetRandomPointOnScreen(), Quaternion.identity);
            }
            if (random < 3)
            {
                Instantiate(Expkit, ParticleManager.Instance.GetRandomPointOnScreen(), Quaternion.identity);
            }
        }
    }

    public void Restart()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
