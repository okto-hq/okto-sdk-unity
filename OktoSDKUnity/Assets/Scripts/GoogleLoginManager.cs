using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class GoogleLoginManager : MonoBehaviour
{
    private const string ClientId = "93847741863-vsrpkaejcdaqfusjfhemnp2o06b2p0ll.apps.googleusercontent.com";
    private const string ClientSecret = "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";
    private const string RedirectUri = "http://localhost:3000"; // Set your redirect URI here
    private const string AuthEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
    private const string TokenEndpoint = "https://oauth2.googleapis.com/token";
    private string authorizationCode;
    [SerializeField] private TMP_Text loginText;

    public async void SignInWithGoogle()
    {
        string authUrl = $"{AuthEndpoint}?client_id={ClientId}" +
                         $"&redirect_uri={RedirectUri}" +
                         "&response_type=code" +
                         "&scope=openid%20email%20profile";

        Application.OpenURL(authUrl); // Open Google Sign-In in a browser

        // Start a local HTTP server to capture the redirect with the authorization code
        await StartLocalServer();
    }

    private async Task StartLocalServer()
    {
        using (var listener = new HttpListener())
        {
            listener.Prefixes.Add($"{RedirectUri}/");
            listener.Start();
            Debug.Log("Local server started, waiting for Google redirect...");

            // Wait for an incoming request (this will capture the authorization code)
            var context = await listener.GetContextAsync();
            var response = context.Response;
            var query = context.Request.QueryString;
            authorizationCode = query["code"];

            // Send a response to the browser
            string responseString = "<html><body>Authentication Successful. You may close this window.</body></html>";
            Debug.Log("Response from Google Token Endpoint: " + response);

            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.OutputStream.Close();
            listener.Stop();
        }

        // Exchange the authorization code for an ID token
        string idToken = await ExchangeAuthorizationCodeForIdToken(authorizationCode);
        Debug.Log("ID Token: " + idToken);
        loginText.text = "Logged In";
        DataManager.Instance.IdToken = idToken;
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

        // Parse the ID token from response if successful
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
