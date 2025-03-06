using NaughtyAttributes;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [Scene] public string sceneName;

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

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Restart()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
