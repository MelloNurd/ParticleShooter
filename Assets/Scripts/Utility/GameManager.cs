using NaughtyAttributes;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [Scene] public string sceneName;
    [SerializeField] private GameObject Medkit;
    [SerializeField] private GameObject Expkit;

    private float lastItemSpawnTime;
    private float lastClusterSpawnTime;
    private float lastAggressionIncrease;
    public float clusterSpawnRate = 30f;
    public float aggressionIncreaseRate = 30f;

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
        lastItemSpawnTime = 1f; // Initialize to ensure the first spawn happens immediately
    }

    // Update is called once per frame
    void Update()
    {

        if (Timer.Instance.elapsedTime - lastItemSpawnTime >= 10f)
        {
            lastItemSpawnTime = Timer.Instance.elapsedTime;

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

        if (!ParticleManager.Instance.RunGame) return;

        if(Timer.Instance.elapsedTime - lastClusterSpawnTime >= clusterSpawnRate && ParticleManager.Instance.Clusters.Count < 40)
        {
            lastClusterSpawnTime= Timer.Instance.elapsedTime;
            ParticleManager.Instance.CreateCluster();
        }
        if(Timer.Instance.elapsedTime - lastAggressionIncrease >= aggressionIncreaseRate && ParticleManager.Instance.PlayerAttractionStrength < 6f)
        {
            lastAggressionIncrease = Timer.Instance.elapsedTime;
            ParticleManager.Instance.PlayerAttractionStrength += .5f;
            ParticleManager.Instance.ParticleDamage += 1;
        }
    }

    public void Restart()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
