using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

// Matches generic types that inherit/implement Array2D<>
[CustomPropertyDrawer(typeof(Array2D<>), true)]
public class Array2DPropertyDrawer : PropertyDrawer
{
    // Foldout states tracked by property path
    private static Dictionary<string, bool> foldoutStates = new Dictionary<string, bool>();

    private const float VerticalSpacing = 2f;
    private const float HorizontalSpacing = 5f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Check if we are folded out
        bool show = IsFoldedOut(property);

        // If not showing, just one line for the foldout
        if (!show)
            return EditorGUIUtility.singleLineHeight + VerticalSpacing;

        // Otherwise, compute height for all rows
        SerializedProperty rowsProp = property.FindPropertyRelative("Array");
        if (rowsProp == null || !rowsProp.isArray)
            return EditorGUIUtility.singleLineHeight + VerticalSpacing;

        // One line for the foldout label itself
        float totalHeight = EditorGUIUtility.singleLineHeight + VerticalSpacing;

        // Then one line per row
        for (int i = 0; i < rowsProp.arraySize; i++)
        {
            totalHeight += EditorGUIUtility.singleLineHeight + VerticalSpacing;
        }

        return totalHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Draw the foldout
        Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        bool show = IsFoldedOut(property);
        bool newShow = EditorGUI.Foldout(foldoutRect, show, label, true);
        SetFoldout(property, newShow);

        // Move below the foldout
        position.y += EditorGUIUtility.singleLineHeight + VerticalSpacing;

        // If not expanded, skip drawing the rest
        if (!newShow)
            return;

        EditorGUI.indentLevel++;

        // Get the rows property
        SerializedProperty rowsProp = property.FindPropertyRelative("Array");
        if (rowsProp != null && rowsProp.isArray)
        {
            // For each row
            for (int rowIndex = 0; rowIndex < rowsProp.arraySize; rowIndex++)
            {
                SerializedProperty rowProp = rowsProp.GetArrayElementAtIndex(rowIndex);
                if (rowProp == null) continue;

                // The "row" field in each Row, which is an array of T
                SerializedProperty rowArrayProp = rowProp.FindPropertyRelative("row");
                if (rowArrayProp == null || !rowArrayProp.isArray) continue;

                // Prepare a rect for the row
                Rect rowRect = new Rect(
                    position.x,
                    position.y,
                    position.width,
                    EditorGUIUtility.singleLineHeight
                );

                int columns = rowArrayProp.arraySize;

                // Calculate column width to fit them evenly with some spacing
                float totalSpacing = HorizontalSpacing * (columns - 1);
                float columnWidth = (position.width - totalSpacing) / Mathf.Max(columns, 1);

                // Temporarily remove indent so each cell lines up
                int oldIndent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                // Draw each column side by side
                for (int colIndex = 0; colIndex < columns; colIndex++)
                {
                    SerializedProperty elementProp = rowArrayProp.GetArrayElementAtIndex(colIndex);

                    // X offset includes column width + spacing for each prior column
                    float xOffset = rowRect.x + colIndex * (columnWidth + HorizontalSpacing);

                    Rect cellRect = new Rect(
                        xOffset,
                        rowRect.y,
                        columnWidth,
                        EditorGUIUtility.singleLineHeight
                    );

                    // If it's a float, draw a draggable float field
                    if (elementProp.propertyType == SerializedPropertyType.Float)
                    {
                        float oldLabelWidth = EditorGUIUtility.labelWidth;
                        EditorGUIUtility.labelWidth = 12f; // small label area to drag on

                        float newVal = EditorGUI.FloatField(cellRect, " ", elementProp.floatValue);
                        elementProp.floatValue = newVal;

                        EditorGUIUtility.labelWidth = oldLabelWidth;
                    }
                    else
                    {
                        // Fallback: draw normally for other types
                        EditorGUI.PropertyField(cellRect, elementProp, GUIContent.none);
                    }
                }

                // Restore indent
                EditorGUI.indentLevel = oldIndent;

                // Move down for next row
                position.y += EditorGUIUtility.singleLineHeight + VerticalSpacing;
            }
        }

        EditorGUI.indentLevel--;
    }

    /// <summary>
    /// Checks if the given property is currently folded out (expanded).
    /// </summary>
    private bool IsFoldedOut(SerializedProperty property)
    {
        bool show = false;
        foldoutStates.TryGetValue(property.propertyPath, out show);
        return show;
    }

    /// <summary>
    /// Sets the foldout state for the given property.
    /// </summary>
    private void SetFoldout(SerializedProperty property, bool show)
    {
        foldoutStates[property.propertyPath] = show;
    }
}
