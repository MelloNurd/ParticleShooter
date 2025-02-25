using TMPro;
using UnityEngine;

public class Points : MonoBehaviour
{
    public static Points Instance { get; private set; }
    public TextMeshProUGUI pointsText;
    private int points;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        pointsText = gameObject.GetComponent<TextMeshProUGUI>();

        points = 0;
        UpdatePointsText();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AddPoints(int amount)
    {
        points += amount;
        UpdatePointsText();
    }

    private void UpdatePointsText()
    {
        pointsText.text = points + " Pts";
    }
}
