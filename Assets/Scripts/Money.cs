using NaughtyAttributes;
using TMPro;
using UnityEngine;

public class Money : MonoBehaviour
{
    public static Money Instance;
    public TextMeshProUGUI moneyText;
    public int storedMoney = 0;
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
        Homebase.EnterHomebase.AddListener(storeMoney);
        storedMoney = PlayerPrefs.GetInt("StoredMoney", 0);
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
    
    private void storeMoney()
    {
        storedMoney += currentMoney;
        currentMoney = 0;
        UpdateMoneyUI();
        PlayerPrefs.SetInt("StoredMoney", storedMoney);
    }
}
