using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using OktoWallet;
using System.Threading.Tasks;
using static OktoWallet.OktoWalletSDK;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Button loginButton;
    [SerializeField] private Button authenticateButton;
    [SerializeField] private Button authenticateJWT;
    [SerializeField] private Button getPortfolio;
    [SerializeField] private Button getUserDetail;
    [SerializeField] private Button getSupportedTokens;
    [SerializeField] private Button getSupportedNetworks;
    [SerializeField] private Button orderHistory;
    [SerializeField] private Button getWallet;
    [SerializeField] private Button getNFTOrder;
    [SerializeField] private Button createWallet;
    [SerializeField] private Button showModel;

    [SerializeField] private GameObject displayPanel;
    [SerializeField] private TMP_Text displayText;
    private OktoWalletSDK loginManager;

    [SerializeField] private TMP_InputField userIdText;
    [SerializeField] private TMP_InputField jwtToken;

    [SerializeField] private Transform loginPanel, userPanel;

    [SerializeField] private LoginController loginController;
    private OktoWalletSDK.AuthDetails authenticationData;

    private PlayerProfile playerProfile;

    public GoogleLoginManager googleLogin;

    public string apiKey;

    private void OnEnable()
    {
        loginButton.onClick.AddListener(LoginButtonPressed);
        authenticateButton.onClick.AddListener(onAuthenticateClicked);
        authenticateJWT.onClick.AddListener(OnAuthenticateJWTClicked);
        getPortfolio.onClick.AddListener(OnGetPortfolioClicked);
        getUserDetail.onClick.AddListener(OnGetUserDetailClicked);
        getSupportedTokens.onClick.AddListener(OnGetSupportedTokensClicked);
        orderHistory.onClick.AddListener(OnOrderHistoryClicked);
        getWallet.onClick.AddListener(OnGetWalletClicked);
        getNFTOrder.onClick.AddListener(OnGetNFTOrderClicked);
        createWallet.onClick.AddListener(OnCreateWalletClicked);
        showModel.onClick.AddListener(OnShowModelClicked);
        getSupportedNetworks.onClick.AddListener(OnSupportedNetworksClicked);
    }

    private void OnDisable()
    {
        loginButton.onClick.RemoveListener(LoginButtonPressed);
        authenticateButton.onClick.RemoveListener(onAuthenticateClicked);
        authenticateJWT.onClick.RemoveListener(OnAuthenticateJWTClicked);
        getPortfolio.onClick.RemoveListener(OnGetPortfolioClicked);
        getUserDetail.onClick.RemoveListener(OnGetUserDetailClicked);
        getSupportedTokens.onClick.RemoveListener(OnGetSupportedTokensClicked);
        orderHistory.onClick.RemoveListener(OnOrderHistoryClicked);
        getWallet.onClick.RemoveListener(OnGetWalletClicked);
        getNFTOrder.onClick.RemoveListener(OnGetNFTOrderClicked);
        createWallet.onClick.RemoveListener(OnCreateWalletClicked);
        showModel.onClick.RemoveListener(OnShowModelClicked);
        getSupportedNetworks.onClick.RemoveListener(OnSupportedNetworksClicked);
    }


    private void LoginButtonPressed()
    {
        googleLogin.SignInWithGoogle();
    }

    public void onAuthenticateClicked()
    {
        if (DataManager.Instance.IdToken != null)
        {
            Authenticate(DataManager.Instance.IdToken);
        }
        else
        {
            Debug.LogError("Please sign in first.");
        }
    }

    private async void Authenticate(string id)
    {
        try
        {
            loginManager = new OktoWalletSDK(apiKey, "");
            Exception error = null;
            (authenticationData, error) = await loginManager.AuthenticateAsync(id);
            displayOutput(authenticationData.AuthToken.ToString());

            if (authenticationData != null)
            {
                Debug.Log("Login successful.");
            }
            else
            {
                Debug.LogError("Login failed: Invalid API Key.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Login failed: " + e.Message);
        }
    }

    private async void OnAuthenticateJWTClicked()
    {
        try
        {
            loginManager = new OktoWalletSDK(apiKey, "");
            Exception error = null;
            (authenticationData, error) = await loginManager.AuthenticateWithUserIdAsync(userIdText.text,jwtToken.text);
            displayOutput(authenticationData.ToString());

            if (authenticationData != null)
            {
                Debug.Log("Login successful.");
            }
            else
            {
                Debug.LogError("Login failed: Invalid API Key.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Login failed: " + e.Message);
        }
    }

    private async void OnGetPortfolioClicked()
    {
        try
        {
            var portfolio = await loginManager.GetPortfolio();
            displayOutput(portfolio.ToString());
            Debug.Log("Portfolio retrieved: " + portfolio);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to retrieve portfolio: " + e.Message);
        }
    }

    private async void OnSupportedNetworksClicked()
    {
        try
        {
            var networks = await loginManager.GetSupportedNetworks();
            displayOutput(networks.ToString());
            Debug.Log("Portfolio retrieved: " + networks);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to retrieve portfolio: " + e.Message);
        }
    }

    private async void OnGetUserDetailClicked()
    {
        try
        {
            var userDetail = await loginManager.GetUserDetails();
            displayOutput(userDetail.ToString());
            Debug.Log("User Detail: " + userDetail);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to get user detail: " + e.Message);
        }
    }

    private async void OnGetSupportedTokensClicked()
    {
        try
        {
            var tokens = await loginManager.GetSupportedTokens();
            displayOutput(tokens.ToString());
            Debug.Log("Supported Tokens: " + tokens);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to get supported tokens: " + e.Message);
        }
    }

    private async void OnOrderHistoryClicked()
    {
        try
        {
            OrderQuery query = new OrderQuery();
            var orders = await loginManager.OrderHistory(query);
            displayOutput(orders.ToString());
            Debug.Log("Order History: " + orders);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to get order history: " + e.Message);
        }
    }

    private async void OnGetWalletClicked()
    {
        try
        {
            var wallet = await loginManager.GetWallets();
            displayOutput(wallet.ToString());
            Debug.Log("Wallet: " + wallet);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to get wallet: " + e.Message);
        }
    }

    private async void OnGetNFTOrderClicked()
    {
        try
        {
            NftOrderDetailsQuery query = new NftOrderDetailsQuery();
            var nftOrder = await loginManager.GetNftOrderDetails(query);
            displayOutput(nftOrder.ToString());
            Debug.Log("NFT Order: " + nftOrder);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to get NFT order: " + e.Message);
        }
    }

    private async void OnCreateWalletClicked()
    {
        try
        {
            var walletCreationResult = await loginManager.CreateWallet();
            displayOutput(walletCreationResult.ToString());
            Debug.Log("Wallet created: " + walletCreationResult);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to create wallet: " + e.Message);
        }
    }

    private void OnShowModelClicked()
    {
        // Show model code here if any additional functionality is needed
        Debug.Log("Show model clicked");
    }

    public void displayOutput(string text)
    {
        displayText.text = text;
        displayPanel.gameObject.SetActive(true);
    }
}
