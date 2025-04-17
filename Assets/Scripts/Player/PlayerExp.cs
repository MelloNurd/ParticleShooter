using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerExp : MonoBehaviour
{
    public static PlayerExp Instance { get; private set; }

    public int Level = 1;
    public float ExpMultiplier = 1f;

    private float currentExp = 0;
    public float levelExp = 100;
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

    public void AddExp(float amount)
    {
        currentExp += amount * ExpMultiplier;
        if (currentExp >= levelExp)
        {
            // Wrap around remaining exp (don't set to zero because they might have some still left over)
            currentExp -= levelExp;
            if (Level < 5) levelExp *= 1.2f; // Increase exp needed for next level (amount based on current level)
            else levelExp *= 1.4f;

            // Increment level
            Level++;
        }
        UpdateSlider();
    }

    private void UpdateSlider()
    {
        expSlider.value = currentExp / levelExp;
    }
}
