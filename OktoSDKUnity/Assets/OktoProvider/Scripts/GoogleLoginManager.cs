using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class GoogleLoginManager : MonoBehaviour
{

    private Credentials credentials;
    private string ClientId = "";
    private string ClientSecret = "";
    [SerializeField]private string RedirectUri = ""; 
    private const string AuthEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
    private const string TokenEndpoint = "https://oauth2.googleapis.com/token";
    private string authorizationCode;
    [SerializeField] private UIManager UIManager;

    private void Start()
    {
        credentials = Resources.Load<Credentials>("Credentials");
        ClientId = credentials.clientId;
        ClientSecret = credentials.clientSecret;
    }

    public async void SignInWithGoogle()
    {
        string authUrl = $"{AuthEndpoint}?client_id={ClientId}" +
                         $"&redirect_uri={RedirectUri}" +
                         "&response_type=code" +
                         "&scope=openid%20email%20profile";

        Application.OpenURL(authUrl); 
        await StartLocalServer();
    }

    private async Task StartLocalServer()
    {
        using (var listener = new HttpListener())
        {
            listener.Prefixes.Add($"{RedirectUri}/");
            listener.Start();
            Debug.Log("Local server started, waiting for Google redirect...");

            var context = await listener.GetContextAsync();
            var response = context.Response;
            var query = context.Request.QueryString;
            authorizationCode = query["code"];

            string responseString = "<html><body>Authentication Successful. You may close this window.</body></html>";
            Debug.Log("Response from Google Token Endpoint: " + response);

            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.OutputStream.Close();
            listener.Stop();
        }

        string idToken = await ExchangeAuthorizationCodeForIdToken(authorizationCode);
        Debug.Log("ID Token: " + idToken);
        DataManager.Instance.IdToken = idToken;
        UIManager.onAuthenticateClicked();
    }

    private async Task<string> ExchangeAuthorizationCodeForIdToken(string code)
    {
        Debug.Log("Exchanging Authorization Code for ID Token...");

        var requestData = new StringContent(
            $"client_id={ClientId}" +
            $"&client_secret={ClientSecret}" +
            $"&code={code}" +
            $"&redirect_uri={RedirectUri}" +
            "&grant_type=authorization_code",
            Encoding.UTF8, "application/x-www-form-urlencoded");

        var httpClient = new HttpClient();
        var response = await httpClient.PostAsync(TokenEndpoint, requestData);
        string responseBody = await response.Content.ReadAsStringAsync();

        Debug.Log("Response from Google Token Endpoint: " + responseBody);

        var tokenData = JsonUtility.FromJson<TokenResponse>(responseBody);
        if (tokenData != null && !string.IsNullOrEmpty(tokenData.id_token))
        {
            Debug.Log("ID Token successfully retrieved: " + tokenData.id_token);
            return tokenData.id_token;
        }
        else
        {
            Debug.LogWarning("Failed to retrieve ID Token.");
            return null;
        }
    }

    [Serializable]
    private class TokenResponse
    {
        public string id_token;
    }
}
