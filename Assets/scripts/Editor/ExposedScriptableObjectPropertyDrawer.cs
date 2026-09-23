using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ExposedScriptableObjectAttribute))]
public class ExposedScriptableObjectAttributeDrawer : PropertyDrawer
{
    private Editor _editor = null;
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //draw the label
        EditorGUI.PropertyField(position, property, label, true);

        //if the object is null, exit early
        if (property.objectReferenceValue == null)
            return;

        //draw foldout
        if (property.objectReferenceValue != null)
        {
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
        }

        //draw the foldout properties
        if (!property.isExpanded) 
            return;
        
        //indent
        EditorGUI.indentLevel++;

        //draw properties 
        if (!_editor) 
            Editor.CreateCachedEditor(property.objectReferenceValue, null, ref _editor);
        _editor.OnInspectorGUI();

        //end indent
        EditorGUI.indentLevel--;
    }
}
