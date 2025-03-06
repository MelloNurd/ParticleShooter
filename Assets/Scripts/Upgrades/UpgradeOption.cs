using UnityEngine;
using NaughtyAttributes;
using TMPro;
using UnityEngine.UI;

public class UpgradeOption : MonoBehaviour
{

    [OnValueChanged("InitializePanel")] public Upgrade upgrade;

    private TMP_Text _upgradeTitle;
    private TMP_Text _upgradeDescription;
    public Button UpgradeButton;

    public void Initialize()
    {
        _upgradeTitle = transform.Find("Frame").Find("Title").GetComponentInChildren<TMP_Text>();
        _upgradeDescription = transform.Find("Frame").Find("Description").GetComponentInChildren<TMP_Text>();
        UpgradeButton = transform.Find("Purchase Button").GetComponent<Button>();

        SetVisuals();
    }

    private void SetVisuals()
    {
        if (upgrade == null || !Application.isPlaying) return;

        _upgradeTitle.text = upgrade.UseTypeAsName ? upgrade.UpgradeType.ToString().AsSentence() : upgrade.UpgradeName;
        _upgradeDescription.text = upgrade.UpgradeDescription;

        UpgradeButton.GetComponentInChildren<TMP_Text>().color = upgrade.Rarity.GetColor();
    }
}
