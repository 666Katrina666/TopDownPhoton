using UnityEngine;
using UnityEditor;

public class SceneSelectorAttribute : PropertyAttribute
{
}

[CustomPropertyDrawer(typeof(SceneSelectorAttribute))]
public class SceneSelectorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        if (property.propertyType == SerializedPropertyType.String)
        {

            string[] sceneNames = GetSceneNames();

            int selectedIndex = Mathf.Max(0, System.Array.IndexOf(sceneNames, property.stringValue));

            selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, sceneNames);

            property.stringValue = sceneNames[selectedIndex];
        }
        else
        {
            EditorGUI.LabelField(position, label.text, "ONLY STRINGS");
        }

        EditorGUI.EndProperty();
    }

    private string[] GetSceneNames()
    {
        int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
        string[] sceneNames = new string[sceneCount];

        for (int i = 0; i < sceneCount; i++)
        {
            string scenePath = EditorBuildSettings.scenes[i].path;
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            sceneNames[i] = sceneName;
        }

        return sceneNames;
    }
}
