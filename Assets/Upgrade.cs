using TMPro;
using UnityEngine;

public class Upgrade : MonoBehaviour
{
    [SerializeField] UpgradeType upgradeType;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject upgradeTextObject = transform.Find("UpgradeName").gameObject;
        TMP_Text upgradeName = upgradeTextObject.GetComponent<TMP_Text>();
        upgradeName.text = upgradeType.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
