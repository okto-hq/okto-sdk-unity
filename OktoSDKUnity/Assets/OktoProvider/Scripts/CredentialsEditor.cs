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

        // Load the credentials from the Resources folder (only in the Editor)
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
            // Allow editing of the credentials in the Editor
            credentials.apiKey = EditorGUILayout.TextField("API Key", credentials.apiKey);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(credentials); // Mark as dirty to save changes
                AssetDatabase.SaveAssets();
            }
        }
    }

    private Credentials CreateCredentialsAsset()
    {
        var asset = ScriptableObject.CreateInstance<Credentials>();
        AssetDatabase.CreateAsset(asset, "Assets/Resources/Credentials.asset");
        AssetDatabase.SaveAssets();
        return asset;
    }
}
#endif
