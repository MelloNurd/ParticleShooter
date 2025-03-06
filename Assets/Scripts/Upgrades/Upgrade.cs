using NaughtyAttributes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Upgrade", menuName = "Scriptable Objects/Upgrade")]
public class Upgrade : ScriptableObject
{
    public UpgradeType UpgradeType;
    [Dropdown("adjustType")] public string AdjustType;
    public float AdjustValue;

    [Space(5)]
    public bool UseTypeAsName = true;
    [HideIf("UseTypeAsName")] public string UpgradeName;
    [TextArea(3, 20)] public string UpgradeDescription;
    public Rarity Rarity;

    private string[] adjustType = new string[] { "Additive", "Multiplicative" }; // This is used for the dropdown

    public void ApplyUpgrade()
    {
        if (StatsManager.Instance == null || StatsManager.CheckForNulls()) return;

        void ApplyAdjustment(ref float stat)
        {
            switch (AdjustType)
            {
                case "Additive":
                    stat += AdjustValue;
                    break;
                case "Multiplicative":
                    stat *= AdjustValue;
                    break;
            }
        }

        switch (UpgradeType)
        {
            case UpgradeType.MaxHealth:
                ApplyAdjustment(ref StatsManager.Player.maxHealth);
                break;
            case UpgradeType.HealthRegen:
                ApplyAdjustment(ref StatsManager.Player.healthRegenRate);
                break;
            case UpgradeType.MovementSpeed:
                ApplyAdjustment(ref StatsManager.Player.movementSpeed);
                break;
            case UpgradeType.TurnSpeed:
                ApplyAdjustment(ref StatsManager.Player.rotationSpeed);
                break;
            case UpgradeType.BoostSpeed:
                ApplyAdjustment(ref StatsManager.Player.boostMultiplier);
                break;
            case UpgradeType.MaxBoost:
                ApplyAdjustment(ref StatsManager.Player.maxBoost);
                break;
            case UpgradeType.BoostUsageRate:
                ApplyAdjustment(ref StatsManager.Player.boostUsageRate);
                break;
            case UpgradeType.BoostRechargeRate:
                ApplyAdjustment(ref StatsManager.Player.boostRechargeRate);
                break;
            case UpgradeType.AttackRange:
                foreach (var beam in StatsManager.BeamList)
                {
                    ApplyAdjustment(ref beam.fireRange);
                }
                break;
            case UpgradeType.AttackPierce:
                foreach (var beam in StatsManager.BeamList)
                {
                    ApplyAdjustment(ref beam.pierce);
                }
                break;
            case UpgradeType.AttackDamage:
                foreach (var beam in StatsManager.BeamList)
                {
                    ApplyAdjustment(ref beam.damage);
                }
                break;
            case UpgradeType.ExpGain:
                ApplyAdjustment(ref PlayerExp.Instance.ExpMultiplier);
                break;
        }
    }
}

public enum UpgradeType
{
    MaxHealth,
    HealthRegen,
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
