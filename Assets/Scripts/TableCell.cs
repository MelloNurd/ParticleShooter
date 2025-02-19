using UnityEngine;
using TMPro;
using static ParticleTable;

public class Cell : MonoBehaviour
{
    public TMP_InputField inputField;
    private int row;
    private int col;

    public void Initialize(int row, int col, float initialValue)
    {
        this.row = row;
        this.col = col;
        inputField.text = initialValue.ToString("0.00");
        inputField.onEndEdit.AddListener(OnTextChanged);
    }

    private void OnTextChanged(string newText)
    {
        if (float.TryParse(newText, out float newValue))
        {
            inputField.text = newValue.ToString("0.00"); // Ensure the text is formatted to two decimal places
            CellTextChangedEvent.Raise(row, col, newValue);
        }
        else
        {
            Debug.LogError("Invalid input. Please enter a valid number.");
        }
    }
}