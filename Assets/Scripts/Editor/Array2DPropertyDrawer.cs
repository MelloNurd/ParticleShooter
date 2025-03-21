using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(Array2D<>), true)]
public class Array2DPropertyDrawer : PropertyDrawer
{
    // Foldout states tracked by property path
    private static Dictionary<string, bool> foldoutStates = new Dictionary<string, bool>();

    private const float VerticalSpacing = 2f;
    private const float HorizontalSpacing = 5f;
    private const float HeaderWidth = 40f; // fixed width for row header

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Check if we are folded out
        bool show = IsFoldedOut(property);
        if (!show)
            return EditorGUIUtility.singleLineHeight + VerticalSpacing;

        SerializedProperty rowsProp = property.FindPropertyRelative("Array");
        if (rowsProp == null || !rowsProp.isArray)
            return EditorGUIUtility.singleLineHeight + VerticalSpacing;

        // Height for:
        // - foldout line
        // - column header row
        // - each row in the grid
        float totalHeight = EditorGUIUtility.singleLineHeight + VerticalSpacing; // foldout
        totalHeight += EditorGUIUtility.singleLineHeight + VerticalSpacing; // column headers

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

        // Move below foldout
        position.y += EditorGUIUtility.singleLineHeight + VerticalSpacing;
        if (!newShow)
            return;

        EditorGUI.indentLevel++;

        SerializedProperty rowsProp = property.FindPropertyRelative("Array");
        if (rowsProp != null && rowsProp.isArray && rowsProp.arraySize > 0)
        {
            // Use the row count as the number of types
            int numTypes = rowsProp.arraySize;

            // --- Draw Column Headers ---
            // We get the number of columns from the first row.
            SerializedProperty firstRowProp = rowsProp.GetArrayElementAtIndex(0);
            SerializedProperty firstRowArrayProp = firstRowProp.FindPropertyRelative("row");
            int columns = firstRowArrayProp != null && firstRowArrayProp.isArray ? firstRowArrayProp.arraySize : 0;

            // Calculate available width for the cells (excluding row header)
            float remainingWidth = position.width - HeaderWidth;
            float columnWidth = (columns > 0) ? (remainingWidth - HorizontalSpacing * (columns - 1)) / columns : 0f;

            for (int colIndex = 0; colIndex < columns; colIndex++)
            {
                // X offset includes header width plus spacing for each prior column
                float xOffset = position.x + HeaderWidth + colIndex * (columnWidth + HorizontalSpacing);
                Rect headerRect = new Rect(xOffset, position.y, columnWidth, EditorGUIUtility.singleLineHeight);
                Color headerColor = GetColorForType(colIndex);
                EditorGUI.DrawRect(headerRect, headerColor);

                // Centered label showing the type index (or you could use "Type " + colIndex)
                GUIStyle centeredStyle = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleCenter
                };
                EditorGUI.LabelField(headerRect, colIndex.ToString(), centeredStyle);
            }

            // Move down past the column header row
            position.y += EditorGUIUtility.singleLineHeight + VerticalSpacing;

            // --- Draw Each Row (with row header and cells) ---
            for (int rowIndex = 0; rowIndex < numTypes; rowIndex++)
            {
                SerializedProperty rowProp = rowsProp.GetArrayElementAtIndex(rowIndex);
                if (rowProp == null)
                    continue;

                SerializedProperty rowArrayProp = rowProp.FindPropertyRelative("row");
                if (rowArrayProp == null || !rowArrayProp.isArray)
                    continue;

                // Draw the row header (first cell in the row)
                Rect rowHeaderRect = new Rect(position.x, position.y, HeaderWidth, EditorGUIUtility.singleLineHeight);
                Color rowHeaderColor = GetColorForType(rowIndex);
                EditorGUI.DrawRect(rowHeaderRect, rowHeaderColor);
                GUIStyle centeredStyle = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleCenter
                };
                EditorGUI.LabelField(rowHeaderRect, rowIndex.ToString(), centeredStyle);

                // Now draw the rest of the cells in the row
                int currentColumns = rowArrayProp.arraySize;
                float totalSpacing = HorizontalSpacing * (currentColumns - 1);
                float cellWidth = (position.width - HeaderWidth - totalSpacing) / Mathf.Max(currentColumns, 1);

                // Temporarily remove indent so each cell lines up properly
                int oldIndent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                for (int colIndex = 0; colIndex < currentColumns; colIndex++)
                {
                    SerializedProperty elementProp = rowArrayProp.GetArrayElementAtIndex(colIndex);
                    float xOffset = position.x + HeaderWidth + colIndex * (cellWidth + HorizontalSpacing);
                    Rect cellRect = new Rect(xOffset, position.y, cellWidth, EditorGUIUtility.singleLineHeight);

                    // If the element is a float, use a draggable float field; otherwise use default drawing.
                    if (elementProp.propertyType == SerializedPropertyType.Float)
                    {
                        float oldLabelWidth = EditorGUIUtility.labelWidth;
                        EditorGUIUtility.labelWidth = 12f;
                        float newVal = EditorGUI.FloatField(cellRect, " ", elementProp.floatValue);
                        elementProp.floatValue = newVal;
                        EditorGUIUtility.labelWidth = oldLabelWidth;
                    }
                    else
                    {
                        EditorGUI.PropertyField(cellRect, elementProp, GUIContent.none);
                    }
                }

                EditorGUI.indentLevel = oldIndent;
                position.y += EditorGUIUtility.singleLineHeight + VerticalSpacing;
            }
        }

        EditorGUI.indentLevel--;
    }

    private bool IsFoldedOut(SerializedProperty property)
    {
        bool show = false;
        foldoutStates.TryGetValue(property.propertyPath, out show);
        return show;
    }

    private void SetFoldout(SerializedProperty property, bool show)
    {
        foldoutStates[property.propertyPath] = show;
    }

    // This method calculates a color based on type index and total types.
    private Color GetColorForType(int type)
    {
        return (ParticleType)type switch
        {
            ParticleType.Fire => new Color(1f, 0.5f, 0f),
            ParticleType.Ice => Color.blue,
            ParticleType.Electric => Color.yellow,
            ParticleType.Attack => Color.red,
            ParticleType.Defense => Color.green,
            ParticleType.Speed => Color.magenta,
            ParticleType.Neutral => Color.white, // Neutral
            _ => Color.gray, // Default
        };
    }
}
