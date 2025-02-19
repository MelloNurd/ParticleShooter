using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using NaughtyAttributes;
using System;

public class ParticleTable : MonoBehaviour
{
    public GameObject cellPrefab; // Prefab containing TextMeshProUGUI inside an Image
    public Transform gridParent;  // Parent GridLayoutGroup
    private float[,] attractionMatrix; // The matrix storing attraction/repulsion values

    private int typeParticles;

    IEnumerator Start()
    {
        // Wait until ParticleManager is initialized
        yield return new WaitUntil(() => ParticleManager.Instance != null && ParticleManager.IsFinishedSpawning);

        // Get the particle count from ParticleManager
        typeParticles = ParticleManager.Instance.NumberOfTypes;

        // Initialize the attractionMatrix
        attractionMatrix = new float[typeParticles, typeParticles];

        // Populate the attractionMatrix with values from forces
        GetForcesMatrix();

        // Generate the table UI
        GenerateTableUI();

        gameObject.SetActive(false);
    }

    void GetForcesMatrix()
    {
        float[,] forces = ParticleManager.Forces;

        if (forces == null || forces.GetLength(0) != typeParticles || forces.GetLength(1) != typeParticles)
        {
            Debug.LogError("Forces array size does not match expected size.");
            return;
        }

        for (int i = 0; i < typeParticles; i++)
        {
            for (int j = 0; j < typeParticles; j++)
            {
                attractionMatrix[i, j] = forces[i, j];
            }
        }
    }

    void GenerateTableUI()
    {
        int gridSize = typeParticles + 1; // Including headers
        GridLayoutGroup grid = gridParent.GetComponent<GridLayoutGroup>();
        RectTransform rectTransform = gridParent.GetComponent<RectTransform>();
        float cellWidth = rectTransform.rect.width / gridSize;
        float cellHeight = rectTransform.rect.height / gridSize;
        grid.cellSize = new Vector2(cellWidth, cellHeight);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = gridSize;

        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                GameObject cell = Instantiate(cellPrefab, gridParent);
                TMP_InputField inputField = cell.GetComponentInChildren<TMP_InputField>();
                Image bg = cell.GetComponentInChildren<Image>();

                if (i == 0 && j == 0)
                {
                    // Top-left corner cell
                    inputField.text = "";
                    bg.color = new Color(1, 1, 1, 0);
                    inputField.interactable = false;
                }
                else if (i == 0)
                {
                    // Top row - column headers
                    int particleType = j - 1;
                    inputField.text = ""; // Optionally display type number
                    bg.color = GetParticleColor(particleType);
                    inputField.interactable = false;
                }
                else if (j == 0)
                {
                    // First column - row headers
                    int particleType = i - 1;
                    inputField.text = ""; // Optionally display type number
                    bg.color = GetParticleColor(particleType);
                    inputField.interactable = false;
                }
                else
                {
                    // Data cell
                    int rowType = i - 1;
                    int colType = j - 1;
                    float value = attractionMatrix[rowType, colType];
                    Cell cellScript = cell.GetComponent<Cell>();
                    cellScript.Initialize(rowType, colType, value);
                    inputField.text = value.ToString("0.00");

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

    private Color GetParticleColor(int typeIndex)
    {
        return Color.HSVToRGB((float)typeIndex / typeParticles, 1, 1);
    }

    private void OnEnable()
    {
        CellTextChangedEvent.OnCellTextChanged += UpdateParticleManager;
    }

    private void OnDisable()
    {
        CellTextChangedEvent.OnCellTextChanged -= UpdateParticleManager;
    }

    private void UpdateParticleManager(int row, int col, float newValue)
    {
        attractionMatrix[row, col] = newValue;
        ParticleManager.Forces[row, col] = newValue;
    }

    public static class CellTextChangedEvent
    {
        public static event Action<int, int, float> OnCellTextChanged;

        public static void Raise(int row, int col, float newValue)
        {
            OnCellTextChanged?.Invoke(row, col, newValue);
        }
    }
}