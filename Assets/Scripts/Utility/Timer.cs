using NaughtyAttributes;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public static Timer Instance;
    public TextMeshProUGUI timerText;
    private float startTime;
    private bool isRunning = false;
    public float elapsedTime = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartTimer();
        timerText = gameObject.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isRunning)
        {
            elapsedTime = Time.time - startTime;

            int hours = ((int)elapsedTime / 3600);
            int minutes = ((int)elapsedTime / 60) % 60;
            int seconds = (int)elapsedTime % 60;
            int centiseconds = (int)((elapsedTime - (int)elapsedTime) * 100);

            timerText.text = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", hours, minutes, seconds, centiseconds);
        }
    }

    public void StartTimer()
    {
        startTime = Time.time;
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResetTimer()
    {
        isRunning = false;
        timerText.text = "00:00:00.00";
    }
}
