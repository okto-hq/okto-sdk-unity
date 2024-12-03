using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class GoogleLoginManager : MonoBehaviour
{

    [SerializeField] private UIManager UIManager;

    private void Start()
    {
        InitializePlayGamesLogin();
    }

    void InitializePlayGamesLogin()
    {
        var config = new PlayGamesClientConfiguration.Builder()
            .RequestIdToken()
            .RequestEmail()
            .Build();

        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        Debug.Log(config);
    }
    public void LoginGoogle()
    {
        Social.localUser.Authenticate(success =>
        {
            if (success)
            {
                string idToken = ((PlayGamesLocalUser)Social.localUser).GetIdToken();
                Debug.Log("Login successful. IdToken: " + idToken);
                DataManager.Instance.IdToken = idToken;
                UIManager.onAuthenticateClicked();
                // Use the idToken to authenticate with your backend or other services
                //SignInWithGoogleAsync(idToken);
            }
            else
            {
                Debug.Log("Login failed.");
            }
        });
    }


    [Serializable]
    private class TokenResponse
    {
        public string id_token;
    }
}
