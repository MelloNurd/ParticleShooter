using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Mines : MonoBehaviour
{
    public GameObject[] LargeMines;
    public GameObject[] SmallMines;
    public int asteroidEnergy = 38;

    // Energy required to enable/deactivate each mine type
    public int largeMineEnergyCost = 10;
    public int smallMineEnergyCost = 1;

    // Accumulators to track energy removed
    private int smallEnergyTracker = 0;
    private int largeEnergyTracker = 0;

    // Crystal prefab attached to the mine
    public GameObject crystalPrefab;
    // Force with which crystals are launched
    public float crystalLaunchForce = 2f;

    // Time-based cooldown for crystal pop (in seconds)
    public float crystalReleaseCooldown = 1f;
    private float _lastCrystalPopTime = -Mathf.Infinity;

    public int crystalEnergyCost = 1;

    void Start()
    {
        InitializeMines();
        ActivateMines();
    }

    void InitializeMines()
    {
        // Deactivate all large mines
        foreach (GameObject largeMine in LargeMines)
        {
            largeMine.SetActive(false);
        }
        // Deactivate all small mines
        foreach (GameObject smallMine in SmallMines)
        {
            smallMine.SetActive(false);
        }
    }

    public void ActivateMines()
    {
        // Calculate number of large mines to activate
        int numLarge = Mathf.Min(asteroidEnergy / largeMineEnergyCost, LargeMines.Length);
        // Create a list of large mines for random selection
        List<GameObject> availableLargeMines = new List<GameObject>(LargeMines);
        for (int i = 0; i < numLarge && availableLargeMines.Count > 0; i++)
        {
            int index = Random.Range(0, availableLargeMines.Count);
            availableLargeMines[index].SetActive(true);
            availableLargeMines.RemoveAt(index);
        }

        // Remaining energy after activating large mines
        int leftover = (asteroidEnergy - (numLarge * largeMineEnergyCost)) / smallMineEnergyCost;
        // Create a list of small mines for random selection
        List<GameObject> availableSmallMines = new List<GameObject>(SmallMines);
        for (int i = 0; i < leftover && availableSmallMines.Count > 0; i++)
        {
            int index = Random.Range(0, availableSmallMines.Count);
            availableSmallMines[index].SetActive(true);
            availableSmallMines.RemoveAt(index);
        }
    }

    void DeactivateRandomMine(GameObject[] mineArray)
    {
        List<GameObject> activeMines = new List<GameObject>();
        foreach (GameObject mine in mineArray)
        {
            if (mine.activeSelf)
                activeMines.Add(mine);
        }

        if (activeMines.Count == 0)
            return;

        int randIndex = Random.Range(0, activeMines.Count);
        activeMines[randIndex].SetActive(false);
    }

    bool AnyMineActive(GameObject[] mineArray)
    {
        foreach (GameObject mine in mineArray)
        {
            if (mine.activeSelf)
                return true;
        }
        return false;
    }

    public void PopCrystal(Vector2 hitPoint, Vector2 beamDirection)
    {
        if (crystalPrefab != null && asteroidEnergy > 0 && Time.time >= _lastCrystalPopTime + crystalReleaseCooldown)
        {
            _lastCrystalPopTime = Time.time;

            asteroidEnergy -= crystalEnergyCost;
            smallEnergyTracker += crystalEnergyCost;
            largeEnergyTracker += crystalEnergyCost;

            // Deactivate a small mine if enough energy has been removed and one is active.
            if (smallEnergyTracker >= smallMineEnergyCost && AnyMineActive(SmallMines))
            {
                DeactivateRandomMine(SmallMines);
                smallEnergyTracker -= smallMineEnergyCost;
                largeEnergyTracker -= smallMineEnergyCost;
            }
            // If no small mines are active, check large mines.
            else if (largeEnergyTracker >= largeMineEnergyCost && AnyMineActive(LargeMines))
            {
                DeactivateRandomMine(LargeMines);
                largeEnergyTracker -= largeMineEnergyCost;
            }

            // Determine a spawn position slightly outside the hit point in the opposite direction of the beam.
            float offsetDistance = 0.5f;
            Vector2 spawnPos = hitPoint - beamDirection.normalized * offsetDistance + Random.insideUnitCircle * 0.1f;

            // First try: move further away from the mines.
            int safetyTries = 10;
            Vector2 safeSpawnPos = spawnPos;
            bool foundSafe = false;
            while (safetyTries > 0)
            {
                if (Physics2D.OverlapCircle(safeSpawnPos, 0.2f) == null)
                {
                    foundSafe = true;
                    break;
                }
                safeSpawnPos -= beamDirection.normalized * 0.2f;
                safetyTries--;
            }

            // Second try: try lateral moves if moving away didn't work.
            if (!foundSafe)
            {
                // Get a perpendicular direction.
                Vector2 perp = new Vector2(-beamDirection.y, beamDirection.x);
                safetyTries = 10;
                for (int i = 1; i <= safetyTries; i++)
                {
                    // Try shifting to one side.
                    Vector2 lateralPos = spawnPos + perp * (0.2f * i);
                    if (Physics2D.OverlapCircle(lateralPos, 0.2f) == null)
                    {
                        safeSpawnPos = lateralPos;
                        foundSafe = true;
                        break;
                    }
                    // Try shifting to the other side.
                    lateralPos = spawnPos - perp * (0.2f * i);
                    if (Physics2D.OverlapCircle(lateralPos, 0.2f) == null)
                    {
                        safeSpawnPos = lateralPos;
                        foundSafe = true;
                        break;
                    }
                }
            }

            // In all cases, spawn the crystal at the best available position.
            GameObject crystal = Instantiate(crystalPrefab, safeSpawnPos, Quaternion.Euler(0, 0, 90));

            Rigidbody2D rb = crystal.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Calculate the base launch direction as the reverse of the beam direction,
                // so the crystal is launched toward where the beam came from.
                Vector2 baseDirection = (-beamDirection).normalized;
                Vector2 randomDeviation = Random.insideUnitCircle.normalized;
                float deviationFactor = 0.6f; // Adjust this value for more or less deviation.
                Vector2 launchDirection = Vector2.Lerp(baseDirection, randomDeviation, deviationFactor).normalized;
                rb.AddForce(launchDirection * crystalLaunchForce, ForceMode2D.Impulse);
                rb.AddTorque(Random.Range(-1f, 1f), ForceMode2D.Impulse);
            }
        }
    }


}

