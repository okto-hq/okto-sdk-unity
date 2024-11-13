using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using OktoProvider;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

public class UIManager : MonoBehaviour
{
    //Buttons 
    [Header("Buttons")]
    [SerializeField] private Button authenticateButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private Button getPortfolio;
    [SerializeField] private Button getUserDetail;
    [SerializeField] private Button getSupportedTokens;
    [SerializeField] private Button getSupportedNetworks;
    [SerializeField] private Button orderHistory;
    [SerializeField] private Button getWallet;
    [SerializeField] private Button getNFTOrder;
    [SerializeField] private Button createWallet;
    [SerializeField] private Button showModel;

    //TokenTransfer
    [Header("Token Transfer UI")]
    [SerializeField] private TMP_InputField tokenAddress;
    [SerializeField] private TMP_InputField receipent_address;
    [SerializeField] private TMP_InputField amount;
    [SerializeField] private TMP_Dropdown dropDown;
    [SerializeField] private Button transferTokenButton;

    [Header("NFT Transfer UI")]
    [SerializeField] private TMP_InputField collectionAddress;
    [SerializeField] private TMP_InputField recipientAddress;
    [SerializeField] private TMP_InputField collectionName;
    [SerializeField] private TMP_InputField nftQuantity;
    [SerializeField] private TMP_InputField nftAddress;
    [SerializeField] private TMP_Dropdown networkDropdown;
    [SerializeField] private Button transferNFTButton;

    [Header("Raw Transaction UI SOL")]
    [SerializeField] private TMP_InputField instructions;
    [SerializeField] private TMP_InputField signer_address;
    [SerializeField] private TMP_InputField network_name_sol;
    [SerializeField] private Button solTransactionButton;

    //Raw Transaction UI EVM
    [Header("Raw Transaction UI EVM")]
    [SerializeField] private TMP_InputField tokenAddress_from;
    [SerializeField] private TMP_InputField tokenAddress_to;
    [SerializeField] private TMP_InputField evm_data;
    [SerializeField] private TMP_InputField evm_value;
    [SerializeField] private TMP_InputField network_name_evm;
    [SerializeField] private Button evmTransactionButton;

    [Header("Raw Transaction UI APTOS")]
    [SerializeField] private TMP_InputField aptosFunction;
    [SerializeField] private TMP_InputField aptosTypeArguments;
    [SerializeField] private TMP_InputField aptosFunctionArguments;
    [SerializeField] private TMP_InputField network_name_aptos;
    [SerializeField] private Button aptosTransactionButton;

    //DisplayUI
    [Header("Display UI Elements")]
    [SerializeField] private GameObject displayPanel;
    [SerializeField] private GameObject scrollObject;
    [SerializeField] private TMP_Text displayText;
    [SerializeField] private TMP_Text loginText;
    [SerializeField] private DisplayObject displayObject;
    [SerializeField] private Transform objectHolder;


    private string apiKey;
    private List<DisplayObject> displayObjects = new List<DisplayObject>();
    private OktoProviderSDK loginManager;
    private AuthDetails authenticationData;
    private Credentials credentials;


    private void Start()
    {
        credentials = Resources.Load<Credentials>("Credentials");
        apiKey = credentials.apiKey;
    }

    private void OnEnable()
    {

        logoutButton.onClick.AddListener(logoutButtonPressed);
        getPortfolio.onClick.AddListener(OnGetPortfolioClicked);
        getUserDetail.onClick.AddListener(OnGetUserDetailClicked);
        getSupportedTokens.onClick.AddListener(OnGetSupportedTokensClicked);
        orderHistory.onClick.AddListener(OnOrderHistoryClicked);
        getWallet.onClick.AddListener(OnGetWalletClicked);
        getNFTOrder.onClick.AddListener(OnGetNFTOrderClicked);
        createWallet.onClick.AddListener(OnCreateWalletClicked);
        showModel.onClick.AddListener(OnShowModelClicked);
        getSupportedNetworks.onClick.AddListener(OnSupportedNetworksClicked);
        transferTokenButton.onClick.AddListener(OnTransferTokenClicked);
        solTransactionButton.onClick.AddListener(OnSOLRawTransactionClicked);
        evmTransactionButton.onClick.AddListener(OnEVMRawTransactionClicked);
        aptosTransactionButton.onClick.AddListener(OnAptosRawTransactionClicked);
        transferNFTButton.onClick.AddListener(OnTransferNFTClicked);
    }

    private void OnDisable()
    {
        logoutButton.onClick.RemoveListener(logoutButtonPressed);
        getPortfolio.onClick.RemoveListener(OnGetPortfolioClicked);
        getUserDetail.onClick.RemoveListener(OnGetUserDetailClicked);
        getSupportedTokens.onClick.RemoveListener(OnGetSupportedTokensClicked);
        orderHistory.onClick.RemoveListener(OnOrderHistoryClicked);
        getWallet.onClick.RemoveListener(OnGetWalletClicked);
        getNFTOrder.onClick.RemoveListener(OnGetNFTOrderClicked);
        createWallet.onClick.RemoveListener(OnCreateWalletClicked);
        showModel.onClick.RemoveListener(OnShowModelClicked);
        getSupportedNetworks.onClick.RemoveListener(OnTransferTokenClicked);
        transferTokenButton.onClick.RemoveListener(OnTransferTokenClicked);
        solTransactionButton.onClick.RemoveListener(OnSOLRawTransactionClicked);
        evmTransactionButton.onClick.RemoveListener(OnEVMRawTransactionClicked);
        aptosTransactionButton.onClick.RemoveListener(OnAptosRawTransactionClicked);
        transferNFTButton.onClick.RemoveListener(OnTransferNFTClicked);
    }


    private void logoutButtonPressed()
    {
        loginManager.Logout();
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
            loginManager = new OktoProviderSDK(apiKey, "");
            Debug.Log("login" + loginManager);
            Exception error = null;
            (authenticationData, error) = await loginManager.AuthenticateAsync(id);
            Debug.Log("loginDone");
            Debug.Log("loginDone" + authenticationData);
            displayOutput("AuthTokens" + authenticationData.authToken.ToString());

            if (authenticationData != null)
            {
                Debug.Log("Login successful.");
                loginText.text = "Logged In";
                try
                {
                    var wallet = await loginManager.GetWallets();
                    if (wallet.wallets.Count > 0)
                    {
                        createWallet.gameObject.SetActive(false);
                    }
                }
                catch(Exception e)
                {
                    Debug.Log("Please create wallet.");
                }

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
            foreach (Portfolio token in portfolio.tokens)
            {
                DisplayObject dObject = Instantiate(displayObject, objectHolder);
                displayObjects.Add(dObject);
                dObject.setup(token.token_name, token.token_address, token.quantity);

            }
            displayTokens();
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
            foreach (TokenNetwork token in networks.network)
            {
                DisplayObject dObject = Instantiate(displayObject, objectHolder);
                displayObjects.Add(dObject);
                dObject.setup(token.network_name, token.chain_id, token.logo);

            }
            displayTokens();
            Debug.Log("Portfolio retrieved: " + networks);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to retrieve portfolio: " + e.Message);
        }
    }

    private async void OnTransferTokenClicked()
    {
        TransferTokens tokenData = new TransferTokens();
        int selectedIndex = dropDown.value;
        string selectedText = dropDown.options[selectedIndex].text;
        tokenData.network_name = selectedText;
        tokenData.quantity = amount.text;
        tokenData.recipient_address = receipent_address.text;
        tokenData.token_address = tokenAddress.text;
        try
        {
            var tokenTransferData = await loginManager.TransferTokens_(tokenData);
            Debug.Log("Order Id: " + tokenTransferData.orderId);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to get user detail: " + e.Message);
        }
    }
    private async void OnTransferNFTClicked()
    {
        TransferNft nftData = new TransferNft();

        int selectedIndex = networkDropdown.value;
        string selectedNetwork = networkDropdown.options[selectedIndex].text;
        nftData.network_name = selectedNetwork; 
        nftData.opteration_type = "transfer";  
        nftData.collection_address = collectionAddress.text;
        nftData.collection_name = collectionName.text;  
        nftData.quantity = nftQuantity.text;
        nftData.recipient_address = recipientAddress.text;
        nftData.nft_address = nftAddress.text;

        try
        {
            var nftTransferData = await loginManager.transferNft(nftData);
            Debug.Log("Order Id: " + nftTransferData.order_id);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to transfer NFT: " + e.Message);
        }
    }


    private async void OnSOLRawTransactionClicked()
    {
        ExecuteRawTransaction tokenData = new ExecuteRawTransaction();
        tokenData.network_name = network_name_sol.text;
        string instructionsJson = instructions.text;
        var instructionsVal = JsonConvert.DeserializeObject<List<Instruction>>(instructionsJson);
        SOLTransaction sol = new SOLTransaction();
        sol.instructions = instructionsVal;
        sol.signer = signer_address.text;
        tokenData.transaction = sol;
        try
        {
            var transactionData = await loginManager.executeRawTransaction(tokenData);
            Debug.Log("Job Id: " + transactionData.jobId);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to get user detail: " + e.Message);
        }
    }

    private async void OnEVMRawTransactionClicked()
    {
        ExecuteRawTransaction tokenData = new ExecuteRawTransaction();
        tokenData.network_name = network_name_evm.text;
        EVMTransaction evm = new EVMTransaction();
        evm.from = tokenAddress_from.text;
        evm.to = tokenAddress_to.text;
        evm.data = evm_data.text;
        evm.value = evm_value.text;
        tokenData.transaction = evm;
        try
        {
            var transactionData = await loginManager.executeRawTransaction(tokenData);
            Debug.Log("Job Id: " + transactionData.jobId);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to get user detail: " + e.Message);
        }
    }

    private async void OnAptosRawTransactionClicked()
    {
        ExecuteRawTransaction tokenData = new ExecuteRawTransaction();
        tokenData.network_name = "APTOS"; 

        AptosTransaction aptos = new AptosTransaction();
        aptos.transactions = new List<AptosTransactionItem>
        {
            new AptosTransactionItem
            {
                function = aptosFunction.text, 
                typeArguments = aptosTypeArguments.text.Split(','), 
                functionArguments = aptosFunctionArguments.text.Split(',') 
            }
        };

        tokenData.transaction = aptos;

        try
        {
            var transactionData = await loginManager.executeRawTransaction(tokenData);
            Debug.Log("Job Id: " + transactionData.jobId);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to execute Aptos transaction: " + e.Message);
        }
    }


    private async void OnGetUserDetailClicked()
    {
        try
        {
            var userDetail = await loginManager.GetUserDetails();
            displayOutput("User Id: " + userDetail.user_id + " Email" + userDetail.email);
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
            foreach (Token token in tokens.tokens)
            {
                DisplayObject dObject = Instantiate(displayObject,objectHolder);
                displayObjects.Add(dObject);
                dObject.setup(token.token_address, token.token_name, token.network_name);
                
            }
            displayTokens();
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
            if(wallet.wallets.Count == 0)
            {
                displayOutput("You need to create wallet first.");
                return;
            }
            foreach (Wallet token in wallet.wallets)
            {
                DisplayObject dObject = Instantiate(displayObject, objectHolder);
                displayObjects.Add(dObject);
                dObject.setup(token.address, token.network_name, "");

            }
            displayTokens();
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
            displayOutput("Wallet created");
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
        scrollObject.gameObject.SetActive(false);
        displayPanel.gameObject.SetActive(true);
        displayText.gameObject.SetActive(true);
        displayText.text = text;
    }

    public void clearDisplayObjects()
    {
        foreach(DisplayObject dObject in displayObjects)
        {
            Destroy(dObject.gameObject);
        }
        displayObjects.Clear();
    }

    public void displayTokens()
    {
        displayText.gameObject.SetActive(false);
        displayPanel.gameObject.SetActive(true);
        scrollObject.gameObject.SetActive(true);
    }
}
