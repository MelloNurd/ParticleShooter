using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

[CustomPropertyDrawer(typeof(SerializedDictionary<,>), true)]
public class SerializedDictionaryDrawer : PropertyDrawer
{
    private bool foldout;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!foldout)
        {
            // Only the foldout line if collapsed
            return EditorGUIUtility.singleLineHeight;
        }

        // Foldout line + header line + one line per dictionary entry
        SerializedProperty keysProp = property.FindPropertyRelative("m_Keys");
        int numKeys = keysProp != null ? keysProp.arraySize : 0;

        // total lines = 1 (foldout) + 1 (header) + numKeys
        return EditorGUIUtility.singleLineHeight * (numKeys + 2);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Foldout
        Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        foldout = EditorGUI.Foldout(foldoutRect, foldout, label, true);

        if (!foldout)
            return;

        EditorGUI.indentLevel++;

        // Grab the serialized arrays
        SerializedProperty keysProp = property.FindPropertyRelative("m_Keys");
        SerializedProperty valuesProp = property.FindPropertyRelative("m_Values");

        if (keysProp == null || valuesProp == null)
        {
            EditorGUI.LabelField(
                new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight),
                "Could not find 'm_Keys' or 'm_Values'!"
            );
            EditorGUI.indentLevel--;
            return;
        }

        // Header row (Key / Value)
        float lineHeight = EditorGUIUtility.singleLineHeight;
        Rect headerRect = new Rect(position.x, position.y + lineHeight, position.width, lineHeight);

        float halfWidth = headerRect.width * 0.5f;
        Rect keyHeaderRect = new Rect(headerRect.x, headerRect.y, halfWidth - 5f, headerRect.height);
        Rect valueHeaderRect = new Rect(headerRect.x + halfWidth, headerRect.y, halfWidth, headerRect.height);

        EditorGUI.LabelField(keyHeaderRect, "Key", EditorStyles.boldLabel);
        EditorGUI.LabelField(valueHeaderRect, "Value", EditorStyles.boldLabel);

        // Draw each key/value pair
        int arraySize = keysProp.arraySize;
        for (int i = 0; i < arraySize; i++)
        {
            // Shift down by 2 lines: one for the foldout, one for the header
            Rect lineRect = new Rect(
                position.x,
                position.y + lineHeight * (i + 2),
                position.width,
                lineHeight
            );

            halfWidth = lineRect.width * 0.5f;
            Rect keyRect = new Rect(lineRect.x, lineRect.y, halfWidth - 5f, lineRect.height);
            Rect valueRect = new Rect(lineRect.x + halfWidth, lineRect.y, halfWidth, lineRect.height);

            EditorGUI.PropertyField(keyRect, keysProp.GetArrayElementAtIndex(i), GUIContent.none);
            EditorGUI.PropertyField(valueRect, valuesProp.GetArrayElementAtIndex(i), GUIContent.none);
        }

        EditorGUI.indentLevel--;
    }
}
