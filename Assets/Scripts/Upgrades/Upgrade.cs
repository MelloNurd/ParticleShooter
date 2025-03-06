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

    // References to any needed scripts for upgrading
    private Player _player;
    private ParticleManager _particleManager;
    private List<LazerBeam> _beamList = new();

    private void Awake()
    {
        _player = FindFirstObjectByType<Player>();
        _particleManager = FindFirstObjectByType<ParticleManager>();
        _beamList = _player.GetComponentsInChildren<LazerBeam>().ToList();
    }

    public void ApplyUpgrade()
    {
        if(_player == null || _particleManager == null) return;

        // Might be a more efficient way of doing this, but this works fine for what the game is
        switch (UpgradeType)
        {
            case UpgradeType.MovementSpeed:
                _player.movementSpeed += 1;
                break;
            case UpgradeType.BoostSpeed:
                _player.boostMultiplier += 1;
                break;
            case UpgradeType.MaxBoost:
                _player.maxBoost += 50;
                break;
            case UpgradeType.BoostUsageRate:
                _player.boostUsageRate -= 5;
                break;
            case UpgradeType.BoostRechargeRate:
                _player.boostRechargeRate += 1;
                break;
            case UpgradeType.AttackRange:
                foreach (var beam in _beamList)
                {
                    beam.fireRange += 1;
                }
                break;
            case UpgradeType.AttackPierce:
                foreach (var beam in _beamList)
                {
                    beam.pierce += 1;
                }
                break;
            case UpgradeType.AttackDamage:
                foreach (var beam in _beamList)
                {
                    beam.damage += 1;
                }
                break;
        }
    }
}

public enum UpgradeType
{
    MovementSpeed,
    BoostSpeed,
    MaxBoost,
    BoostUsageRate,
    BoostRechargeRate,
    AttackRange,
    AttackPierce,
    AttackDamage
}

public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}