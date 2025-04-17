using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Upgrade : MonoBehaviour
{
    [SerializeField] UpgradeType upgradeType;
    [SerializeField] int totalBars;
    [SerializeField] int startCost;
    [SerializeField] float costMultiplier;
    List<Image> OutsideBars = new();
    List<Image> InsideBars = new();
    static Color UIColor = new Color(0f, 0.7672955f, 0.2378616f, 1f);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int numPurchased = PlayerPrefs.GetInt(upgradeType.ToString() + "_UpgradeType", 0);
        GameObject upgradeTextObject = transform.Find("UpgradeName").gameObject;
        TMP_Text upgradeName = upgradeTextObject.GetComponent<TMP_Text>();
        upgradeName.text = upgradeType.ToString();

        int price = startCost;
        if(numPurchased > 0)
        {
            price *= Mathf.RoundToInt(numPurchased * costMultiplier);
        }

        GameObject priceTextObject = transform.Find("Price").gameObject;
        TMP_Text priceName = priceTextObject.GetComponent<TMP_Text>();
        priceName.text = "$" + price.ToString();

        Transform temp = transform.Find("Bars");
        for(int i = 0; i < temp.childCount; i++ )
        {
            OutsideBars.Add(temp.GetChild(i).GetComponent<Image>());
            InsideBars.Add(OutsideBars[i].transform.GetChild(0).GetComponent<Image>());
        }
        for(int i = totalBars; i < OutsideBars.Count; i++)
        {
            OutsideBars[i].enabled = false;
        }
        for(int i = 0; i < numPurchased; i++)
        {
            InsideBars[i].color = UIColor;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
