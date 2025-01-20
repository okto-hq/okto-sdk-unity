#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class CredentialsEditor : EditorWindow
{
    private Credentials credentials;

    [MenuItem("SDK/Credentials Manager")]
    public static void ShowWindow()
    {
        GetWindow<CredentialsEditor>("Credentials Manager");
    }

    private void OnGUI()
    {
        GUILayout.Label("SDK Credentials", EditorStyles.boldLabel);

        if (credentials == null)
        {
            credentials = Resources.Load<Credentials>("Credentials");
        }

        if (credentials == null)
        {
            if (GUILayout.Button("Create New Credentials"))
            {
                credentials = CreateCredentialsAsset();
            }
        }
        else
        {
            credentials.apiKey = EditorGUILayout.TextField("API Key", credentials.apiKey);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(credentials);
                AssetDatabase.SaveAssets();
            }
        }
    }

    private Credentials CreateCredentialsAsset()
    {
        // Ensure the Resources folder exists
        string resourcesFolderPath = "Assets/Resources";
        if (!AssetDatabase.IsValidFolder(resourcesFolderPath))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        // Create the Credentials asset
        var asset = ScriptableObject.CreateInstance<Credentials>();
        AssetDatabase.CreateAsset(asset, $"{resourcesFolderPath}/Credentials.asset");
        AssetDatabase.SaveAssets();
        return asset;
    }
}
#endif
