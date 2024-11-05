using UnityEditor;
using UnityEngine;
using System;
using OktoProvider;
using System.Threading.Tasks;

public class EditorAuthentication : EditorWindow
{
    private string apiKey = "";
    private string buildStage = "";
    private bool isLoggedIn = false;

    private GUIStyle headerStyle;
    private GUIStyle linkStyle;
    private int selectedIndex = 0;

    private string[] buildStageOptions = new string[] { "Staging", "Sandbox", "Production" };

    [MenuItem("Tools/Authentication")]
    public static void ShowWindow()
    {
        GetWindow<EditorAuthentication>("Login Window");
    }

    private void OnEnable()
    {
        headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 16,
            normal = { textColor = Color.cyan }
        };

        linkStyle = new GUIStyle(EditorStyles.label)
        {
            normal = { textColor = Color.blue },
            fontStyle = FontStyle.Italic
        };

        apiKey = EditorPrefs.GetString("OktoAPIKey", "");
        buildStage = EditorPrefs.GetString("BuildStage", "");

        // Check if there's saved data to avoid login
        isLoggedIn = !string.IsNullOrEmpty(apiKey);
    }

    private void OnGUI()
    {
        if (headerStyle == null || linkStyle == null)
        {
            OnEnable(); // Re-initialize styles if they are null
        }

        GUILayout.Space(10);

        if (!isLoggedIn)
        {
            DrawLoginPanel();
        }
        else
        {
            DrawTournamentDataPanel();
        }

        GUILayout.Space(10);
    }

    private void DrawLoginPanel()
    {
        GUILayout.Label("Login with API Key", headerStyle);

        apiKey = EditorGUILayout.TextField("API Key", apiKey);

        selectedIndex = EditorGUILayout.Popup("Select Option", selectedIndex, buildStageOptions);

    }

    private void DrawTournamentDataPanel()
    {

        if (GUILayout.Button("Logout"))
        {
            Logout();
        }
    }

    private void Logout()
    {
        isLoggedIn = false;
        apiKey = "";

        // Clear saved data
        EditorPrefs.DeleteKey("OktoAPIKey");
        EditorPrefs.DeleteKey("BuildStage");

        Debug.Log("Logged out.");
    }
}

