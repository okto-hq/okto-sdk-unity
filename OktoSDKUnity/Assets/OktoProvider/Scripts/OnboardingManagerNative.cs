using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
    [SerializeField] private GameObject IncorrectCode;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI timeCounter;
    [SerializeField] private TextMeshProUGUI verificationText;

    [Header("Sprites")]
    [SerializeField] private Sprite codeDefault;
    [SerializeField] private Sprite codeTyping;
    [SerializeField] private Sprite codeSuccessful;
    [SerializeField] private Sprite codeIncorrect;


    [SerializeField] private UIManager uiManager;
    [SerializeField] private GoogleLoginManager loginManager;
    private string emailToken;
    private string phoneToken;

    private bool isEmailAuth = false;

    private float resendCooldown = 30f;
    private float timer = 0f;

    public string otpString = "";

    private Color defaultColor = new Color32(112, 112, 112, 255);
    private Color activeColor = new Color32(84, 102, 238, 255);
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
        sendOTP.onClick.AddListener(SendPhoneOtp);
        googleAccount.onClick.AddListener(GoogleLogin);
        backButtonConfirm.onClick.AddListener(backToMain);
        backButtonMain.onClick.AddListener(backToHome);

        InitializeCodeInputListeners();

        phoneNumber.onValueChanged.AddListener(ValidatePhoneNumber);
        emailAddress.onValueChanged.AddListener(ValidateEmailAddress);
    }

    private void ValidatePhoneNumber(string phone)
    {
        if (phone.Length > 10)
        {
            phoneNumber.text = phone.Substring(0, 10); 
        }

        sendOTP.interactable = phoneNumber.text.Length == 10;
    }

    private void ValidateEmailAddress(string email)
    {
        bool isValidEmail = !string.IsNullOrEmpty(email) && email.Contains("@") && email.Contains(".");
        sendCode.interactable = isValidEmail;
    }

    private void GoogleLogin()
    {
        loginManager.LoginGoogle();
        CloseAllScreens();
        phoneScreen.SetActive(true);
        defaultObjects.SetActive(true);
        clearCodeField();
        onboardingScreen.SetActive(false);
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }

    private void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            timeCounter.text = $"({Mathf.Ceil(timer)} sec)";
            resendOTP.interactable = false;
            resendOTP.transform.GetComponentInChildren<TextMeshProUGUI>().color = defaultColor;
        }
        else
        {
            timer = 0;
            timeCounter.text = "";
            resendOTP.interactable = true;
            resendOTP.transform.GetComponentInChildren<TextMeshProUGUI>().color = activeColor;
        }
    }
    private void backToHome()
    {
        CloseAllScreens();
        IncorrectCode.SetActive(false);
        phoneScreen.SetActive(true);
        onboardingScreen.SetActive(false);
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }
    private void backToMain()
    {
        clearCodeField();
        IncorrectCode.SetActive(false);
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

    /*private void InitializeCodeInputListeners()
    {
        TMP_InputField[] codeFields = { code1, code2, code3, code4, code5, code6 };
        for (int i = 0; i < codeFields.Length; i++)
        {
            int index = i;
            codeFields[i].onValueChanged.AddListener(delegate { HandleCodeInput(codeFields, index); });
            codeFields[i].onSelect.AddListener(delegate { HighlightSelectedField(codeFields[index]); });
            codeFields[i].onDeselect.AddListener(delegate { ResetFieldHighlight(codeFields[index]); });
        }
    }*/

    private void HighlightSelectedField(TMP_InputField field)
    {
        Image fieldImage = field.transform.GetComponent<Image>(); 
        if (fieldImage != null)
        {
            if(field == code1)
            {
                if(otpString.Length < 1)
                {
                    fieldImage.sprite = codeTyping;
                }
            }
            else
            {
                fieldImage.sprite = codeTyping;
            }
            
        }
    }

    private void ResetFieldHighlight(TMP_InputField field)
    {
        Image fieldImage = field.transform.GetComponent<Image>();
        if (fieldImage != null)
        {
            fieldImage.sprite = codeDefault; 
        }
    }
    private void InitializeCodeInputListeners()
    {
        TMP_InputField[] codeFields = {code2, code3, code4, code5, code6 };
        TMP_InputField[] codeFieldsSend = {code1, code2, code3, code4, code5, code6 };

        foreach (TMP_InputField field in codeFields)
        {
            field.interactable = false;
            field.onSelect.AddListener(delegate { HighlightSelectedField(field); SwitchToInputField(code1); });
        }

        code1.onValueChanged.AddListener(delegate { HandleInputChange(codeFieldsSend, 0); });
        code1.onSelect.AddListener(delegate { HighlightSelectedField(code1); });
        //codeFields[5].onSelect.AddListener(delegate { HighlightSelectedField(codeFields[5]); SwitchToInputField(code1); });
    }
    private void RemoveCodeInputListeners()
    {
        TMP_InputField[] codeFields = { code1, code2, code3, code4, code5, code6 };

        foreach (TMP_InputField field in codeFields)
        {
            field.onSelect.RemoveAllListeners();
            field.onValueChanged.RemoveAllListeners();        
        }

    }

    private void SwitchToInputField(TMP_InputField targetInputField)
    {
        Debug.Log($"Switching focus to {targetInputField.name}");

        StartCoroutine(SafeSwitchToInputField(targetInputField));

    }
    private IEnumerator SafeSwitchToInputField(TMP_InputField targetInputField)
    {
        yield return new WaitForEndOfFrame();

        targetInputField.Select();
        targetInputField.interactable = true;
        if(otpString.Length > 1)
        {
            if (IncorrectCode.activeInHierarchy)
            {
                targetInputField.transform.GetComponent<Image>().sprite = codeIncorrect;
            }
            else
            {
                targetInputField.transform.GetComponent<Image>().sprite = codeDefault;
            }
        }
        else if (otpString.Length == 1)
        {
            targetInputField.transform.GetComponent<Image>().sprite = codeTyping;
        }



    }

    private void ClearInputFields(TMP_InputField[] codeFields)
    {
        // Clear all input fields when the last field is selected (back button press)
        foreach (var field in codeFields)
        {
            field.text = "";
            Image fieldImage = field.transform.GetComponent<Image>();
            if (fieldImage != null)
            {
                fieldImage.sprite = codeDefault;  // Reset field highlight
            }
        }
    }

    private void HandleInputChange(TMP_InputField[] codeFields, int index)
    {
        if(index == 0)
        {
            string val = codeFields[0].text;
            if ((otpString.Length == 0) || val.Length == 2 && otpString.Length < 6 && val.Length > 0)
            {
                Debug.Log("herio" + val);
                otpString += val[val.Length - 1];
                codeFields[otpString.Length - 1].transform.GetComponent<Image>().sprite = codeDefault;
                if(otpString.Length > 1)
                {
                    codeFields[otpString.Length - 1].interactable = false;
                }
                
                deselectCodeField();
                if (otpString.Length < 6)
                {
                    codeFields[otpString.Length].transform.GetComponent<Image>().sprite = codeTyping;
                    codeFields[otpString.Length].interactable = true;
                }
                if (index == 0)
                {
                    codeFields[0].text = otpString[0].ToString();
                }
                else
                {
                    codeFields[index].text = "";
                }
                codeFields[otpString.Length - 1].text = otpString[otpString.Length - 1].ToString();


                if (otpString.Length == 6)
                {
                    codeFields[5].interactable = true;
                    EventSystem.current.SetSelectedGameObject(null);
                    CheckCodeAsync();
                }
            }
            else if (val.Length == 0)
            {
                if (otpString.Length > 0)
                {
                    codeFields[otpString.Length - 1].text = "";
                    codeFields[otpString.Length - 1].interactable = false;
                    otpString = otpString.Substring(0, otpString.Length - 1);
                    deselectCodeField();
                    if (otpString.Length > 0)
                    {
                        codeFields[0].text = otpString[0].ToString();
                        codeFields[otpString.Length - 1].transform.GetComponent<Image>().sprite = codeTyping;
                        codeFields[otpString.Length - 1].interactable = true;
                    }
                    else
                    {
                        codeFields[0].interactable = true;
                        codeFields[0].transform.GetComponent<Image>().sprite = codeTyping;
                    }
                    SwitchToInputField(code1);
                    Debug.Log(otpString);
                }
            }
        }
        else
        {
            if(code6.text == "")
            {
                otpString = otpString.Substring(0, otpString.Length - 1);
                code1.Select();
                code1.interactable = true;
                code1.GetComponent<Image>().sprite = codeIncorrect;
            }

        }

    }

    private void HandleCodeInput(TMP_InputField[] codeFields, int index)
    {
        IncorrectCode.SetActive(true);
        TMP_InputField field = codeFields[index];
        if (field.text.Length > 1)
        {
            field.text = field.text[field.text.Length - 1].ToString();
        }

        if (field.text.Length == 1 && index < codeFields.Length - 1)
        {
            codeFields[index + 1].Select();
            EventSystem.current.SetSelectedGameObject(codeFields[index + 1].gameObject);
        }
        if (field.text.Length == 0 && index > 0)
        {
            codeFields[index - 1].Select();
            EventSystem.current.SetSelectedGameObject(codeFields[index-1].gameObject);
        }
        if (AllFieldsFilled(codeFields))
        {
            EventSystem.current.SetSelectedGameObject(null);
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

    private void clearCodeField()
    {
        TMP_InputField[] codeFields = { code1, code2, code3, code4, code5, code6 };
        foreach(TMP_InputField field in codeFields)
        {
            field.text = "";
            Image fieldImage = field.transform.GetComponent<Image>();
            if (fieldImage != null)
            {
                fieldImage.sprite = codeDefault;
            }
        }
    }
    private void deselectCodeField()
    {
        TMP_InputField[] codeFields = { code1, code2, code3, code4, code5, code6 };
        foreach (TMP_InputField field in codeFields)
        {
            Image fieldImage = field.transform.GetComponent<Image>();
            if (fieldImage != null)
            {
                if (IncorrectCode.activeInHierarchy)
                {
                    fieldImage.sprite = codeIncorrect;
                }
                else
                {
                    fieldImage.sprite = codeDefault;
                }
                
            }
        }
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
            IncorrectCode.SetActive(!verified);

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
            IncorrectCode.SetActive(!verified);

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
        RemoveCodeInputListeners();
        otpString = "";
        clearCodeField();
        InitializeCodeInputListeners();

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
            verificationText.text = MaskEmail(email);
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
            CloseAllScreens();
            clearCodeField();
            phoneScreen.SetActive(true);
            defaultObjects.SetActive(true);
            clearCodeField();
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
            clearCodeField();
            defaultObjects.SetActive(false);
            verificationText.text = MaskPhoneNumber(number);
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
            CloseAllScreens();
            phoneScreen.SetActive(true);
            defaultObjects.SetActive(true);
            clearCodeField();
            onboardingScreen.SetActive(false);
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            return true;
        }
        else
        {
            return false;
        }
    }
    private string MaskPhoneNumber(string phoneNumber)
    {
        if (phoneNumber.Length >= 10)
        {
            return $"{phoneNumber.Substring(0, 2)}****{phoneNumber.Substring(8)}";
        }
        return phoneNumber;
    }

    private string MaskEmail(string email)
    {
        if (string.IsNullOrEmpty(email) || !email.Contains("@"))
            return email;

        string[] parts = email.Split('@');
        string localPart = parts[0];
        string domain = parts[1];

        if (localPart.Length > 2)
        {
            localPart = $"{localPart[0]}****{localPart[^1]}"; 
        }

        return $"{localPart}@{domain}";
    }

}
