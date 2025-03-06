using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    private float startTime;
    private bool isRunning = false;

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
            float t = Time.time - startTime;

            int hours = ((int)t / 3600);
            int minutes = ((int)t / 60) % 60;
            int seconds = (int)t % 60;
            int centiseconds = (int)((t - (int)t) * 100);

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
