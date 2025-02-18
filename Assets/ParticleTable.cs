using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NaughtyAttributes;
using System.Collections;

public class ParticleTable : MonoBehaviour
{
    public GameObject cellPrefab; // Prefab containing TextMeshProUGUI inside an Image
    public Transform gridParent; // Parent GridLayoutGroup
    private float[,] attractionMatrix; // The matrix storing attraction/repulsion values

    private ParticleJobManager particleJobManager;
    private int typeParticles;

    void Start()
    {
        StartCoroutine(FillArray());
    }

    void GenerateRandomMatrix()
    {
        // Get reference to ParticleJobManager
        particleJobManager = FindFirstObjectByType<ParticleJobManager>();
        if (particleJobManager == null)
        {
            Debug.LogError("ParticleJobManager not found in the scene.");
            return;
        }

        // Get the particle count from ParticleJobManager
        typeParticles = ParticleManager.Instance.NumberOfTypes; ;

        // Initialize the attractionMatrix
        attractionMatrix = new float[typeParticles, typeParticles];

        // Populate the attractionMatrix with values from forces
        GetForcesMatrix();

        // Generate the table UI
        GenerateTableUI();
    }

    IEnumerator FillArray()
    {
        Debug.Log("FillArray called");
        // Get reference to ParticleJobManager
        particleJobManager = FindFirstObjectByType<ParticleJobManager>();
        if (particleJobManager == null)
        {
            Debug.LogError("ParticleJobManager not found in the scene.");
            yield break;
        }

        // Wait until the forces array is initialized
        yield return new WaitUntil(() => particleJobManager.forces != null && particleJobManager.forces.Length > 0);

        // Proceed with initialization
        typeParticles = ParticleManager.Instance.NumberOfTypes;

        attractionMatrix = new float[typeParticles, typeParticles];

        GetForcesMatrix();

        GenerateTableUI();
    }

    void GetForcesMatrix()
    {
        // Assuming forces is a 1D array representing a 2D matrix (row-major order)
        float[] forces = particleJobManager.forces;
        Debug.Log($"typeParticles: {typeParticles}, forces.Length: {forces.Length}");

        if (forces == null || forces.Length != typeParticles * typeParticles)
        {
            Debug.LogError("Forces array size does not match expected size. ");
            return;
        }

        for (int i = 0; i < typeParticles; i++)
        {
            for (int j = 0; j < typeParticles; j++)
            {
                int index = i * typeParticles + j;
                attractionMatrix[i, j] = forces[index];
            }
        }
    }

    void GenerateTableUI()
    {
        GridLayoutGroup grid = gridParent.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(500f / typeParticles, 500f / typeParticles);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = typeParticles;

        for (int i = 0; i < typeParticles; i++)
        {
            for (int j = 0; j < typeParticles; j++)
            {
                GameObject cell = Instantiate(cellPrefab, gridParent);
                TMP_Text text = cell.GetComponentInChildren<TMP_Text>();
                Image bg = cell.GetComponentInChildren<Image>();

                float value = attractionMatrix[i, j];
                text.text = value.ToString("0.##");

                // Color coding based on value
                // Color coding based on value
                if (value > 0)
                {
                    // Interpolate between white and green
                    bg.color = Color.Lerp(Color.white, Color.green, value);
                }
                else if (value < 0)
                {
                    // Interpolate between white and red
                    bg.color = Color.Lerp(Color.white, Color.red, -value);
                }
                else
                {
                    bg.color = Color.white;
                }
            }
        }
    }
}
