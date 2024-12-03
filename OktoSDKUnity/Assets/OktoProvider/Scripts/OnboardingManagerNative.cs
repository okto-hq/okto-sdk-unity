using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OnboardingManagerNative : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button Google;
    [SerializeField] private Button Google1;
    [SerializeField] private Button Mail;
    [SerializeField] private Button Mail1;
    [SerializeField] private Button Phone;
    [SerializeField] private Button Phone1;
    [SerializeField] private Button sendOTP;
    [SerializeField] private Button sendCode;
    [SerializeField] private Button googleAccount;
    [SerializeField] private Button paste;
    [SerializeField] private Button resendOTP;
    [SerializeField] private Button backButtonMain;
    [SerializeField] private Button backButtonConfirm;

    [Header("Input Fields")]
    [SerializeField] private TMP_InputField phoneNumber;
    [SerializeField] private TMP_InputField emailAddress;
    [SerializeField] private TMP_InputField code1;
    [SerializeField] private TMP_InputField code2;
    [SerializeField] private TMP_InputField code3;
    [SerializeField] private TMP_InputField code4;
    [SerializeField] private TMP_InputField code5;
    [SerializeField] private TMP_InputField code6;

    [Header("GameObjects")]
    [SerializeField] private GameObject defaultObjects;
    [SerializeField] private GameObject phoneScreen;
    [SerializeField] private GameObject emailScreen;
    [SerializeField] private GameObject googleScreen;
    [SerializeField] private GameObject confirmationScreen;
    [SerializeField] private GameObject onboardingScreen;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI timeCounter;
    [SerializeField] private TextMeshProUGUI verificationText;

    [Header("Sprites")]
    [SerializeField] private Sprite codeTyping;
    [SerializeField] private Sprite codeSuccessful;
    [SerializeField] private Sprite codeIncorrect;


    [SerializeField] private UIManager uiManager;
    private string emailToken;
    private string phoneToken;

    private bool isEmailAuth = false;

    private float resendCooldown = 30f;
    private float timer = 0f;

    private void Start()
    {
        Google.onClick.AddListener(OpenGoogleScreen);
        Phone.onClick.AddListener(OpenPhoneScreen);
        Mail.onClick.AddListener(OpenMailScreen);        
        Google1.onClick.AddListener(OpenGoogleScreen);
        Phone1.onClick.AddListener(OpenPhoneScreen);
        Mail1.onClick.AddListener(OpenMailScreen);
        paste.onClick.AddListener(PasteCode);
        resendOTP.onClick.AddListener(ResendOTP);

        sendCode.onClick.AddListener(SendEmailOtp);
        sendOTP.onClick.AddListener(SendEmailOtp);
        googleAccount.onClick.AddListener(SendEmailOtp);
        backButtonConfirm.onClick.AddListener(backToMain);
        backButtonMain.onClick.AddListener(backToHome);

        InitializeCodeInputListeners();
    }

    private void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            timeCounter.text = "(" + Mathf.Ceil(timer).ToString() + "sec)";
            resendOTP.interactable = timer <= 0;
        }
    }

    private void backToHome()
    {
        CloseAllScreens();
        phoneScreen.SetActive(true);
        onboardingScreen.SetActive(false);
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }
    private void backToMain()
    {
        CloseAllScreens();
        phoneScreen.SetActive(true);
        defaultObjects.SetActive(true);
    }

    private void OpenGoogleScreen()
    {
        CloseAllScreens();
        googleScreen.SetActive(true);
    }

    private void OpenPhoneScreen()
    {
        CloseAllScreens();
        phoneScreen.SetActive(true);
        isEmailAuth = false;
    }

    private void OpenMailScreen()
    {
        CloseAllScreens();
        emailScreen.SetActive(true);
        isEmailAuth = true;
    }

    private void CloseAllScreens()
    {
        phoneScreen.SetActive(false);
        emailScreen.SetActive(false);
        googleScreen.SetActive(false);
        confirmationScreen.SetActive(false);
    }

    private void PasteCode()
    {
        string clipboard = GUIUtility.systemCopyBuffer;
        if (clipboard.Length == 6)
        {
            code1.text = clipboard[0].ToString();
            code2.text = clipboard[1].ToString();
            code3.text = clipboard[2].ToString();
            code4.text = clipboard[3].ToString();
            code5.text = clipboard[4].ToString();
            code6.text = clipboard[5].ToString();
            CheckCodeAsync();
        }
    }

    private void InitializeCodeInputListeners()
    {
        TMP_InputField[] codeFields = { code1, code2, code3, code4, code5, code6 };
        for (int i = 0; i < codeFields.Length; i++)
        {
            int index = i;
            codeFields[i].onValueChanged.AddListener(delegate { HandleCodeInput(codeFields, index); });
            codeFields[i].onSelect.AddListener(delegate { HighlightSelectedField(codeFields[index]); });
            codeFields[i].onDeselect.AddListener(delegate { ResetFieldHighlight(codeFields[index]); });
        }
    }

    private void HighlightSelectedField(TMP_InputField field)
    {
        Image fieldImage = field.transform.GetComponent<Image>(); 
        if (fieldImage != null)
        {
            fieldImage.sprite = codeTyping; 
        }
    }

    private void ResetFieldHighlight(TMP_InputField field)
    {
        Image fieldImage = field.transform.GetComponent<Image>();
        if (fieldImage != null)
        {
            fieldImage.sprite = null; 
        }
    }

    private void HandleCodeInput(TMP_InputField[] codeFields, int index)
    {
        if (codeFields[index].text.Length == 1 && index < codeFields.Length - 1)
        {
            codeFields[index + 1].Select();
        }
        if (AllFieldsFilled(codeFields))
        {
            CheckCodeAsync();
        }
    }

    private bool AllFieldsFilled(TMP_InputField[] codeFields)
    {
        foreach (var field in codeFields)
        {
            if (string.IsNullOrEmpty(field.text))
            {
                return false;
            }
        }
        return true;
    }
    private async Task CheckCodeAsync()
    {
        string enteredCode = code1.text + code2.text + code3.text + code4.text + code5.text + code6.text;
        TMP_InputField[] codeFields = { code1, code2, code3, code4, code5, code6 };

        bool verified;
        if (isEmailAuth)
        {
            verified = await VerifyEmailOtp(enteredCode);
            Sprite resultSprite = verified ? codeSuccessful : codeIncorrect;

            foreach (TMP_InputField field in codeFields)
            {
                Image fieldImage = field.transform.GetComponent<Image>();
                if (fieldImage != null)
                {
                    fieldImage.sprite = resultSprite;
                }
            }
        }
        else
        {
            verified = await VerifyPhoneOtp(enteredCode);
            Sprite resultSprite = verified ? codeSuccessful : codeIncorrect;

            foreach (TMP_InputField field in codeFields)
            {
                Image fieldImage = field.transform.GetComponent<Image>();
                if (fieldImage != null)
                {
                    fieldImage.sprite = resultSprite;
                }
            }
        }

        
    }


    private void ResendOTP()
    {
        if (timer <= 0)
        {
            timer = resendCooldown;
            if (isEmailAuth)
            {
                SendEmailOtp();
            }
            else
            {
                SendPhoneOtp();
            }
            // Send OTP logic here
        }
    }
    private async void SendEmailOtp()
    {
        string email = emailAddress.text;
        if (string.IsNullOrEmpty(email))
        {
            uiManager.displayOutput("Email cannot be empty.");
            return;
        }

        var (success, token, error) = await uiManager.loginManager.SendEmailOtpAsync(email);
        if (success)
        {
            timer = 30;
            emailToken = token;
            CloseAllScreens();
            defaultObjects.SetActive(false);
            verificationText.text = email;
            confirmationScreen.SetActive(true);
        }
        else
        {
            uiManager.displayOutput($"Error: {error?.Message ?? "Failed to send email OTP"}");
        }
    }

    public async Task<bool> VerifyEmailOtp(string otp)
    {
        string email = emailAddress.text;

        var (success, authToken, error) = await uiManager.loginManager.VerifyEmailOtpAsync(email, otp, emailToken);
        if (success)
        {
            //displayOutput("Email OTP verified successfully!");
            uiManager.authenticationCompleted(authToken);
            onboardingScreen.SetActive(false);
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            return true;
        }
        else
        {
            return false;
        }
    }

    private async void SendPhoneOtp()
    {
        string number = phoneNumber.text;
        string countryCode = "IN";

        var (success, token, error) = await uiManager.loginManager.SendPhoneOtpAsync(number, countryCode);
        if (success)
        {
            timer = 30;
            phoneToken = token;
            CloseAllScreens();
            defaultObjects.SetActive(false);
            verificationText.text = number;
            confirmationScreen.SetActive(true);
        }
        else
        {

        }
    }

    public async Task<bool> VerifyPhoneOtp(string otp)
    {
        string number = phoneNumber.text;
        string countryCode = "IN";

        var (success, authToken, error) = await uiManager.loginManager.VerifyPhoneOtpAsync(number, countryCode, otp, phoneToken);
        if (success)
        {
            uiManager.authenticationCompleted(authToken);
            onboardingScreen.SetActive(false);
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            return true;
        }
        else
        {
            return false;
        }
    }
}
