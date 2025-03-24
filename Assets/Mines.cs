using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Mines : MonoBehaviour
{
    public GameObject[] LargeMines;
    public GameObject[] SmallMines;
    public int mineEnergy = 38;

    // Energy required to enable/deactivate each mine type
    public int LargeMineEnergy = 10;
    public int SmallMineEnergy = 1;

    // Accumulators to track energy removed
    private int smallEnergyAccumulator = 0;
    private int largeEnergyAccumulator = 0;

    void Start()
    {
        InitializeMines();
        ActivateMines();
    }

    void Update()
    {
        // For demonstration we use the M key to lower mineEnergy.
        if (Input.GetKeyDown(KeyCode.M))
        {
            mineEnergy -= 1;
            smallEnergyAccumulator += 1;
            largeEnergyAccumulator += 1;

            // Deactivate a small mine if enough energy has been removed and one is active.
            if (smallEnergyAccumulator >= SmallMineEnergy && AnyMineActive(SmallMines))
            {
                DeactivateRandomMine(SmallMines);
                smallEnergyAccumulator -= SmallMineEnergy;
                largeEnergyAccumulator -= SmallMineEnergy;
            }
            // If no small mines are active, check large mines.
            else if (largeEnergyAccumulator >= LargeMineEnergy && AnyMineActive(LargeMines))
            {
                DeactivateRandomMine(LargeMines);
                largeEnergyAccumulator -= LargeMineEnergy;
            }
        }
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
        int numLarge = Mathf.Min(mineEnergy / LargeMineEnergy, LargeMines.Length);
        // Creates a list of large mines to avoid duplicate selection
        List<GameObject> availableLargeMines = new List<GameObject>(LargeMines);
        for (int i = 0; i < numLarge && availableLargeMines.Count > 0; i++)
        {
            int index = Random.Range(0, availableLargeMines.Count);
            availableLargeMines[index].SetActive(true);
            availableLargeMines.RemoveAt(index);
        }

        // Remaining energy after activating large mines
        int leftover = (mineEnergy - (numLarge * LargeMineEnergy)) / SmallMineEnergy;
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
}
