using UnityEngine;
using NaughtyAttributes;
using TMPro;
using UnityEngine.UI;

public class UpgradeOption : MonoBehaviour
{

    [OnValueChanged("InitializePanel")] public Upgrade upgrade;

    private TMP_Text _upgradeTitle;
    private TMP_Text _upgradeDescription;
    private Button _upgradeButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _upgradeTitle = transform.Find("Frame").Find("Title").GetComponentInChildren<TMP_Text>();
        _upgradeDescription = transform.Find("Frame").Find("Description").GetComponentInChildren<TMP_Text>();
        _upgradeButton = transform.Find("Purchase Button").GetComponent<Button>();

        InitializePanel();
    }

    private void InitializePanel()
    {
        if (upgrade == null || !Application.isPlaying) return;

        _upgradeTitle.text = upgrade.UseTypeAsName ? upgrade.UpgradeType.ToString() : upgrade.UpgradeName;
        _upgradeDescription.text = upgrade.UpgradeDescription;

        _upgradeButton.GetComponentInChildren<TMP_Text>().color = upgrade.Rarity.GetColor();
    }
}
