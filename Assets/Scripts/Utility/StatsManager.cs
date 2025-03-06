using NaughtyAttributes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance;

    // References to any needed scripts for upgrading
    public static Player Player => Instance.player;
    private Player player;

    public static ParticleManager ParticleManager => Instance.particleManager;
    private ParticleManager particleManager;

    public static List<LazerBeam> BeamList => Instance.beamList;
    private List<LazerBeam> beamList = new();

    void Awake()
    {
        // Singleton pattern
        if (Instance == null) Instance = this; 
        else Destroy(gameObject); 
    }
    private void Start()
    {
        player = FindFirstObjectByType<Player>();
        particleManager = FindFirstObjectByType<ParticleManager>();
        beamList = Player.GetComponentsInChildren<LazerBeam>().ToList();
    }

    public void StopAttacking()
    {
        // This is so when the menu opens, the player is forced to stop attacking
        foreach (var beam in BeamList)
        {
            beam.IsAttacking = false;
        }
    }

    // Used this to make sure no references are null when applying upgrades
    public static bool CheckForNulls()
    {
        if (Player == null)
        {
            Debug.Log("Player is null");
            return true;
        }
        if (ParticleManager == null)
        {
            Debug.Log("ParticleManager is null");
            return true;
        }
        if (BeamList == null)
        {
            Debug.Log("BeamList is null");
            return true;
        }
        return false;
    }
}
