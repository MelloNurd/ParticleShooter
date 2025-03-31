using NaughtyAttributes;
using TMPro;
using UnityEngine;

public class Money : MonoBehaviour
{
    public static Money Instance;
    public TextMeshProUGUI moneyText;
    public int currentMoney = 0;

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
        moneyText = gameObject.GetComponent<TextMeshProUGUI>();
        UpdateMoneyUI();
    }

    void UpdateMoneyUI()
    {
        moneyText.text = currentMoney.ToString("N0");
    }
    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateMoneyUI();
    }
}
