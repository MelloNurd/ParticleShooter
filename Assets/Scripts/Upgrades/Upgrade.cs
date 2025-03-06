using NaughtyAttributes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Upgrade", menuName = "Scriptable Objects/Upgrade")]
public class Upgrade : ScriptableObject
{
    public UpgradeType UpgradeType;
    public bool UseTypeAsName = true;
    [HideIf("UseTypeAsName")] public string UpgradeName;
    [TextArea(5, 20)] public string UpgradeDescription;
    public Rarity Rarity;

    public void ApplyUpgrade()
    {
        if (StatsManager.Instance == null || StatsManager.CheckForNulls()) return;

        Debug.Log("Upgrade type: " + UpgradeType);

        // Might be a more efficient way of doing this, but this works fine for what the game is
        switch (UpgradeType)
        {
            case UpgradeType.MovementSpeed:
                StatsManager.Player.movementSpeed += 1;
                break;
            case UpgradeType.TurnSpeed:
                StatsManager.Player.rotationSpeed *= 1.1f;
                break;
            case UpgradeType.BoostSpeed:
                StatsManager.Player.boostMultiplier += 1;
                break;
            case UpgradeType.MaxBoost:
                StatsManager.Player.maxBoost += 50;
                break;
            case UpgradeType.BoostUsageRate:
                StatsManager.Player.boostUsageRate -= 5;
                break;
            case UpgradeType.BoostRechargeRate:
                Debug.Log("Upgrading recharge rate.");
                StatsManager.Player.boostRechargeRate += 1;
                break;
            case UpgradeType.AttackRange:
                foreach (var beam in StatsManager.BeamList)
                {
                    beam.fireRange += 1;
                }
                break;
            case UpgradeType.AttackPierce:
                foreach (var beam in StatsManager.BeamList)
                {
                    beam.pierce += 1;
                }
                break;
            case UpgradeType.AttackDamage:
                foreach (var beam in StatsManager.BeamList)
                {
                    beam.damage += 1;
                }
                break;
            case UpgradeType.ExpGain:
                PlayerExp.Instance.ExpMultiplier += 0.1f;
                break;
        }
    }
}

public enum UpgradeType
{
    MovementSpeed,
    TurnSpeed,
    BoostSpeed,
    MaxBoost,
    BoostUsageRate,
    BoostRechargeRate,
    AttackRange,
    AttackPierce,
    AttackDamage,
    ExpGain
}

public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}