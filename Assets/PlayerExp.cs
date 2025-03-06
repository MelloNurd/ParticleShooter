using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerExp : MonoBehaviour
{
    public static PlayerExp Instance { get; private set; }
    private float currentExp = 0;
    private float levelExp = 100;
    private Slider expSlider;

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
        expSlider = GameObject.Find("PlayerExp").GetComponent<Slider>();
        UpdateSlider();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AddExp(float amount)
    {
        currentExp += amount;
        if (currentExp >= levelExp)
        {
            currentExp -= levelExp;
            // LEVEL UP & INCREASE SLIDER VALUE
        }
        UpdateSlider();
    }

    private void UpdateSlider()
    {
        expSlider.value = currentExp / levelExp;
    }
}
