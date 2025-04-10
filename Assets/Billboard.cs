using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Billboard : MonoBehaviour
{
    private TMP_Text signText;
    private int storedMoney = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created  
    void Start()
    {
        signText = gameObject.GetComponent<TMP_Text>();
        storedMoney = PlayerPrefs.GetInt("StoredMoney", 0);
        signText.text = storedMoney.ToString("000000000");
    }

    // Update is called once per frame  
    void Update()
    {

    }
}
