using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Upgrade : MonoBehaviour
{
    [SerializeField] UpgradeType upgradeType;
    [SerializeField] int totalBars;
    [SerializeField] int startCost;
    [SerializeField] float costMultiplier;

    List<Image> OutsideBars = new();
    List<Image> InsideBars = new();
    GameObject purchaseButton;

    static Color UIColor = new Color(0f, 0.7672955f, 0.2378616f, 1f);

    private int price;
    private TMP_Text priceName;

    private int numPurchased;

    ScreenShake screenShaker;
    void Start()
    {
        // Initialize the variables
        numPurchased = PlayerPrefs.GetInt(upgradeType.ToString() + "_Upgrade", 0);

        GameObject upgradeTextObject = transform.Find("UpgradeName").gameObject;
        TMP_Text upgradeName = upgradeTextObject.GetComponent<TMP_Text>();
        upgradeName.text = upgradeType.ToString().Replace("_", " ");
        

        purchaseButton = transform.Find("BuyButton").gameObject;
        GameObject priceTextObject = transform.Find("Price").gameObject;
        priceName = priceTextObject.GetComponent<TMP_Text>();

        Transform temp = transform.Find("Bars");
        for(int i = 0; i < temp.childCount; i++ )
        {
            OutsideBars.Add(temp.GetChild(i).GetComponent<Image>());
            InsideBars.Add(OutsideBars[i].transform.GetChild(0).GetComponent<Image>());
        }

        // Initialize the upgrade bars UI outlines
        for (int i = totalBars; i < OutsideBars.Count; i++)
        {
            OutsideBars[i].enabled = false;
        }

        screenShaker = GameObject.Find("Shaker").GetComponent<ScreenShake>();

        // Function that updates the UI to reflect the current state of the upgrade
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Updates the price text and purchase button visibility
        price = startCost;
        if (numPurchased >= totalBars)
        {
            purchaseButton.GetComponent<Button>().interactable = false;
            purchaseButton.GetComponentInChildren<TMP_Text>().enabled = false;
            priceName.text = "MAX";
        }
        else if(numPurchased > 0)
        {
            price *= Mathf.RoundToInt(numPurchased * costMultiplier);
        }
        if (numPurchased < totalBars)
        {
            priceName.text = "$" + price.ToString();
        }

        // Updates the upgrade bar UI
        for (int i = 0; i < numPurchased; i++)
        {
            InsideBars[i].color = UIColor;
        }
    }

    // OnClick method for the purchase button
    public void TryPurchaseUpgrade()
    {
        // Resets button state
        EventSystem.current.SetSelectedGameObject(null);
        //Checks if player has enough money to purchase upgrade
        int playerMoney = PlayerPrefs.GetInt("StoredMoney", 0);
        if (playerMoney >= price)
        {
            // Deducts player's money and updates the money UI
            playerMoney -= price;
            PlayerPrefs.SetInt("StoredMoney", playerMoney);
            TMP_Text crystals = GameObject.Find("Money").GetComponent<TMP_Text>();
            crystals.text = playerMoney.ToString("000000000");
            // Updates the upgrade UI and saves the number of upgrades purchased
            numPurchased++;
            UpdateUI();
            PlayerPrefs.SetInt(upgradeType.ToString() + "_Upgrade", numPurchased);
        }
        else
        {
            screenShaker.shake = true;
        }
    }
}
