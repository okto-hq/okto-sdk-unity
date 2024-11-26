using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OnboardingManager : MonoBehaviour
{
    [Header("WebView Configuration")]
    public Button openOnboardingButton; 
    public Button closeWebViewButton;

    [SerializeField] private UIManager uiManager;
    public enum AuthType { Email, Phone, GAuth }
    public AuthType authType = AuthType.Email;

    public float initialHeightPercentage = 0.97f; 

    [Header("Dynamic Colors")]
    public string textPrimaryColor = "0xFFFFFFFF";
    public string textSecondaryColor = "0xFFFFFFFF";
    public string textTertiaryColor = "0xFFFFFFFF";
    public string accent1Color = "0xFF905BF5";
    public string accent2Color = "0x80905BF5";
    public string strokeBorderColor = "0xFFACACAB";
    public string strokeDividerColor = "0x4DA8A8A8";
    public string surfaceColor = "0xFF1F1F1F";
    public string backgroundColor = "0xFF000000";

    private WebViewObject webViewObject;

    void Start()
    {
        if (openOnboardingButton != null)
            openOnboardingButton.onClick.AddListener(OpenOnboarding);
    }

    private string GetBuildUrl()
    {
        switch (DataManager.Instance.buildStage)
        {
            case "SANDBOX":
                return "https://okto-sandbox.firebaseapp.com/#/login_screen";
            case "STAGING":
                return "https://3p.oktostage.com/#/login_screen";
            case "PRODUCTION":
                return "https://3p.okto.tech/login_screen/#/login_screen";
            default:
                return "";
        }
    }

    private string GetInjectedJs()
    {
        return $@"
        window.localStorage.setItem('ENVIRONMENT', '{DataManager.Instance.buildStage}');
        window.localStorage.setItem('API_KEY', '{DataManager.Instance.apiKey}');
        window.localStorage.setItem('textPrimaryColor', '{textPrimaryColor}');
        window.localStorage.setItem('textSecondaryColor', '{textSecondaryColor}');
        window.localStorage.setItem('textTertiaryColor', '{textTertiaryColor}');
        window.localStorage.setItem('accent1Color', '{accent1Color}');
        window.localStorage.setItem('accent2Color', '{accent2Color}');
        window.localStorage.setItem('strokeBorderColor', '{strokeBorderColor}');
        window.localStorage.setItem('strokeDividerColor', '{strokeDividerColor}');
        window.localStorage.setItem('surfaceColor', '{surfaceColor}');
        window.localStorage.setItem('backgroundColor', '{backgroundColor}');
        window.localStorage.setItem('primaryAuthType', '{authType}');
        window.localStorage.setItem('brandTitle', 'OktoWalletTest');
        window.localStorage.setItem('brandSubtitle', 'Test it out');
        window.localStorage.setItem('brandIconUrl', '');
    ";
    }

    private void OpenOnboarding()
    {
        if (webViewObject != null) return;
        webViewObject = gameObject.AddComponent<WebViewObject>();

        float screenHeight = Screen.height;
        float webViewHeight = screenHeight * initialHeightPercentage;
        webViewObject.Init(
            cb: (msg) => OnMessageReceived(msg),
            err: (msg) => Debug.LogError("WebView Error: " + msg),
            started: (url) => Debug.Log("Page Started: " + url),
            ld: (url) => OnPageLoaded(url)
        );

        int margin = (int)(screenHeight - webViewHeight);
        webViewObject.SetMargins(0, 80, 0, 80);
        webViewObject.SetVisibility(true);

        string url = GetBuildUrl();
        webViewObject.LoadURL(url);

        webViewObject.EvaluateJS(GetInjectedJs());

        closeWebViewButton.gameObject.SetActive(true);
        closeWebViewButton.onClick.AddListener(CloseWebView);
    }

    private void OnPageLoaded(string url)
    {
        Debug.Log("Page Loaded: " + url);
    }

    private void OnMessageReceived(string message)
    {
        Debug.Log("Message Received: " + message);
        if (message.Contains("auth_success"))
        {
            string authToken = ExtractTokenFromMessage(message, "authToken");
            Debug.Log($"Auth Success: {authToken}");
            OnLoginSuccess(authToken);
        }
        else if (message.Contains("g_auth"))
        {
            HandleGoogleAuth();
        }
    }

    private string ExtractTokenFromMessage(string message, string key)
    {
        string[] parts = message.Split('&');
        foreach (string part in parts)
        {
            if (part.StartsWith(key + "="))
                return part.Substring(key.Length + 1);
        }
        return "";
    }

    private void OnLoginSuccess(string authToken)
    {
        uiManager.authenticationCompleted(authToken);
        Debug.Log("Login Success Callback Invoked");

        CloseWebView();
    }

    private void HandleGoogleAuth()
    {
        Debug.Log("Handling Google Authentication");
    }

    private void CloseWebView()
    {
        closeWebViewButton.onClick.RemoveListener(CloseWebView);
        closeWebViewButton.gameObject.SetActive(false);
        webViewObject.SetVisibility(false);
    }
}
