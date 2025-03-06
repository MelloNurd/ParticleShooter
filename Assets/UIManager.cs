using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private Button restartButton;
    private TextMeshProUGUI timer;
    private TextMeshProUGUI score;
    private TextMeshProUGUI finalTimer;
    private TextMeshProUGUI finalScore;
    private Player player;
    private GameObject endMenu;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timer = GameObject.Find("Timer").GetComponent<TextMeshProUGUI>();
        score = GameObject.Find("Points").GetComponent<TextMeshProUGUI>();
        endMenu = transform.Find("GameOver").gameObject;
        finalScore = endMenu.transform.Find("FinalPoints").GetComponent<TextMeshProUGUI>();
        finalTimer = endMenu.transform.Find("FinalTimer").GetComponent<TextMeshProUGUI>();
        player = FindFirstObjectByType<Player>();
        player.onDeath.AddListener(GameOver);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void GameOver()
    {
        endMenu.SetActive(true);
        finalTimer.text = timer.text;
        finalScore.text = score.text;
        timer.gameObject.SetActive(false);
        score.gameObject.SetActive(false);
    }

}
