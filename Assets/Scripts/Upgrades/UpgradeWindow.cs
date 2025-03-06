using System.Collections.Generic;
using System.Linq;
using Unity.Burst.Intrinsics;
using UnityEngine;

public class UpgradeWindow : MonoBehaviour
{
    public GameObject upgradeOptionPrefab;
    public List<Upgrade> upgrades = new();

    private GameObject _window;

    private void Start()
    {
        _window = transform.GetChild(0).gameObject;
        _window.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            OfferUpgrades(3);
        }
    }

    public void OfferUpgrades(int upgradesAmount)
    {
        _window.SetActive(true);
        Time.timeScale = 0f;

        Transform upgradeOptionTransform = _window.transform.Find("Panel");
        foreach (Transform child in upgradeOptionTransform)
        {
            Destroy(child.gameObject); // Just incase there are leftover upgrades
        }

        List<Upgrade> selectedUpgrades = new();
        List<Upgrade> availableUpgrades = new(upgrades);

        for (int i = 0; i < upgradesAmount; i++)
        {
            if (availableUpgrades.Count == 0) break;

            Upgrade selectedUpgrade = GetRandomUpgradeByRarity(availableUpgrades);
            selectedUpgrades.Add(selectedUpgrade);
            availableUpgrades.Remove(selectedUpgrade);
        }

        foreach (var upgrade in selectedUpgrades)
        {
            UpgradeOption upgradeOption = Instantiate(upgradeOptionPrefab, upgradeOptionTransform).GetComponent<UpgradeOption>();
            upgradeOption.upgrade = upgrade;
            upgradeOption.Initialize();
            upgradeOption.UpgradeButton.onClick.AddListener(() => ChooseUpgrade(upgrade));
        }
    }

    public void ChooseUpgrade(Upgrade selectedUpgrade)
    {
        Debug.Log("Chose upgrade: " + selectedUpgrade.name);
        selectedUpgrade.ApplyUpgrade();

        Transform upgradeOptionTransform = _window.transform.Find("Panel");
        foreach (Transform child in upgradeOptionTransform)
        {
            Destroy(child.gameObject); // Just incase there are leftover upgrades
        }

        Time.timeScale = 1f;
        _window.SetActive(false);
    }

    private Upgrade GetRandomUpgradeByRarity(List<Upgrade> availableUpgrades)
    {
        float totalWeight = availableUpgrades.Sum(u => GetRarityWeight(u.Rarity));
        float randomValue = Random.Range(0, totalWeight);

        foreach (var upgrade in availableUpgrades)
        {
            float weight = GetRarityWeight(upgrade.Rarity);
            if (randomValue < weight)
            {
                return upgrade;
            }
            randomValue -= weight;
        }

        return availableUpgrades[0]; // Fallback
    }

    private float GetRarityWeight(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => 1f,
            Rarity.Uncommon => 0.6f,
            Rarity.Rare => 0.3f,
            Rarity.Epic => 0.2f,
            Rarity.Legendary => 0.1f,
            _ => 1f, // default
        };
    }
}
