using UnityEngine;

[CreateAssetMenu(fileName = "Upgrade", menuName = "Scriptable Objects/Upgrade")]
public class Upgrade : ScriptableObject
{
    public UpgradeType upgradeType;
    public string upgradeName;
    [TextArea(5, 20)] public string upgradeDescription;

    public float upgradeBaseCost;
}

public enum UpgradeType
{
    MovementSpeed,
    BoostSpeed,
    BoostTime,
    BoostCooldown
}