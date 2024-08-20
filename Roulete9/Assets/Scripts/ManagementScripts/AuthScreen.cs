using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.SceneManagement;
using System.Text;

public class AuthScreen : MonoBehaviour
{
    public TMP_InputField phoneNumberSignUpInputField;
    public TMP_InputField emailSignUpInputField;
    public TMP_InputField username;
    public TMP_InputField password;

    public TMP_InputField loginPhoneNumber;
    public TMP_InputField loginEmail;

    public TMP_Text OtpPhoneNumber;
    public TMP_InputField otpInputField;

    public GameObject loginToDisable;
    public GameObject otpScreenPrefab;
    public GameObject signupScreenPrefab;

    public TMP_Text phoneNumberTextField;

    public Button sendOtpButton;
    public Button verifyOtpButton;
    public Button verifyLoginButton;

    public Button setActiveLoginTrue;
    public Button setActiveSignUpTrue;

    public GameObject errorPrefab;
    public TMP_Text errorText;

    private string baseURL = "http://127.0.0.1:8000";
    private void Awake()
    {
        Screen.orientation = ScreenOrientation.Portrait;
    }
    private void Start()
    {
        StartCoroutine(AutoLogin());
        loginToDisable.SetActive(true);
        otpScreenPrefab.SetActive(false);
        signupScreenPrefab.SetActive(false);

        phoneNumberSignUpInputField.onEndEdit.AddListener(delegate { ValidatePhoneNumber(phoneNumberSignUpInputField); });
        emailSignUpInputField.onEndEdit.AddListener(delegate { ValidateEmail(emailSignUpInputField); });
        otpInputField.onEndEdit.AddListener(delegate { ValidateOTP(); });

        phoneNumberSignUpInputField.onValueChanged.AddListener(delegate { CheckInputs(); });
        emailSignUpInputField.onValueChanged.AddListener(delegate { CheckInputs(); });
        otpInputField.onValueChanged.AddListener(delegate { CheckInputs(); });

        loginPhoneNumber.onEndEdit.AddListener(delegate { ValidatePhoneNumber(loginPhoneNumber); });
        loginEmail.onEndEdit.AddListener(delegate { ValidateEmail(loginEmail); });

        loginPhoneNumber.onValueChanged.AddListener(delegate { CheckInputs(); });
        loginEmail.onValueChanged.AddListener(delegate { CheckInputs(); });

        sendOtpButton.onClick.AddListener(OnSendOtpButtonClick);
        verifyOtpButton.onClick.AddListener(OnVerifyOtpButtonClick);
        verifyLoginButton.onClick.AddListener(OnVerifyLoginButtonClick);

        setActiveLoginTrue.onClick.AddListener(ActivateLogin);
        setActiveSignUpTrue.onClick.AddListener(ActivateSignUp);

        sendOtpButton.interactable = false;
        verifyOtpButton.interactable = false;
        verifyLoginButton.interactable = false;
    }

    IEnumerator AutoLogin()
    {
        yield return new WaitForSeconds(1f);
        if (PlayerPrefs.GetString("login", "NO") == "YES")
        {
            PlayerPrefs.SetString("login", "YES");
            SceneManager.LoadScene(1);
        }
    }

    public void OnSendOtpButtonClick()
    {
        if (ValidatePhoneNumber(phoneNumberSignUpInputField) && ValidateEmail(emailSignUpInputField))
        {
            loginToDisable.SetActive(false);
            StartCoroutine(doSignUp());
            if (PlayerPrefs.GetString("login", "NO") == "YES")
            {
                loginPhoneNumber.text = PlayerPrefs.GetString("playerName");
                loginEmail.text = PlayerPrefs.GetString("playerEmail");
            }
            phoneNumberTextField.text = phoneNumberSignUpInputField.text;
        }
    }

    public void OnVerifyOtpButtonClick()
    {
        if (ValidateOTP())
        {
            StartCoroutine(doVerifyOtp());
        }
        else
        {
            Debug.Log("Invalid OTP. Please try again.");
        }
    }

    public void OnVerifyLoginButtonClick()
    {
        if (ValidatePhoneNumber(loginPhoneNumber) && ValidateEmail(loginEmail))
        {
            StartCoroutine(doLoginVerify());
        }
    }

    private IEnumerator doSignUp()
    {
        string url = baseURL + "api/signup";

        // Create a payload object
        var payload = new SignUpPayload
        {
            email = emailSignUpInputField.text,
            PhoneNumber = phoneNumberSignUpInputField.text,
            username = username.text,
            Password = password.text,
            deviceID = SystemInfo.deviceUniqueIdentifier
        };

        string jsonPayload = JsonConvert.SerializeObject(payload);

        using (UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");

            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                Debug.Log(request.error);
                errorPrefab.SetActive(true);
                errorText.text = "User already exists, please enter a different number and email";
            }
            else
            {
                JObject jsonNode = JObject.Parse(request.downloadHandler.text);
                if (jsonNode["notice"] != null && jsonNode["notice"].ToString() == "User Successfully Created !")
                {
                    Debug.Log("User Successfully Created");
                    PlayerPrefs.SetString("PID", jsonNode["playerid"].ToString());
                    PlayerPrefs.SetString("PlayerNumber", phoneNumberSignUpInputField.text);
                    PlayerPrefs.SetString("playerName", username.text);
                    PlayerPrefs.SetString("playerEmail", emailSignUpInputField.text);
                    PlayerPrefs.SetString("userToken", jsonNode["token"].ToString());
                    PlayerPrefs.SetString("login", "YES");
                    SceneManager.LoadScene(1);
                }
                else
                {
                    Debug.Log(jsonNode["notice"].ToString());
                }
            }
        }
    }

    private IEnumerator doLoginVerify()
    {
        string url = baseURL + "api/signin-player";

        // Create a payload object
        var payload = new LoginPayload
        {
            email = loginEmail.text,
            phone = loginPhoneNumber.text
        };

        string jsonPayload = JsonConvert.SerializeObject(payload);

        using (UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");

            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                Debug.Log(request.error);
            }
            else
            {
                JObject jsonNode = JObject.Parse(request.downloadHandler.text);
                if (jsonNode["success"] != null && (bool)jsonNode["success"])
                {
                    Debug.Log(jsonNode["message"].ToString());
                    ShowOtpScreen();
                    PlayerPrefs.SetString("PlayerNumber", loginPhoneNumber.text);
                }
                else
                {
                    Debug.Log(jsonNode["message"].ToString());
                    errorPrefab.SetActive(true);
                    errorText.text = jsonNode["message"].ToString();
                }
            }
        }
    }

    private IEnumerator doVerifyOtp()
    {
        string url = baseURL + "api/otp-verify";

        // Create a payload object
        var payload = new OtpPayload
        {
            otp = otpInputField.text,
            phone = PlayerPrefs.GetString("PlayerNumber")
        };

        string jsonPayload = JsonConvert.SerializeObject(payload);

        using (UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");

            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                Debug.Log(request.error);
            }
            else
            {
                JObject jsonNode = JObject.Parse(request.downloadHandler.text);
                if (jsonNode["success"] != null && (bool)jsonNode["success"])
                {
                    Debug.Log(jsonNode["message"].ToString());
                    Debug.Log("User Successfully Verified");
                    PlayerPrefs.SetString("PID", jsonNode["playerid"].ToString());
                    PlayerPrefs.SetString("playerName", jsonNode["username"].ToString());
                    PlayerPrefs.SetString("playerEmail", jsonNode["email"].ToString());
                    PlayerPrefs.SetString("userToken", jsonNode["token"].ToString());
                    PlayerPrefs.SetString("login", "YES");
                    SceneManager.LoadScene(1);
                }
                else
                {
                    Debug.Log(jsonNode["message"].ToString());
                    errorPrefab.SetActive(true);
                    errorText.text = jsonNode["message"].ToString();
                }
            }
        }
    }

    internal int signalForAppVersion = 0;
    private int recursionCheck = 0;

    public void Lobby()
    {
        print("yesaa");
        if (signalForAppVersion == 0)
        {
            recursionCheck++;
            Invoke("Lobby", 0.5f);
        }
        else
        {
            if (signalForAppVersion == 1)
            {
                PlayerPrefs.SetString("login", "YES");
                SceneManager.LoadScene(1);
            }
        }
    }

    private void ShowOtpScreen()
    {
        loginToDisable.SetActive(false);
        otpScreenPrefab.SetActive(true);
        signupScreenPrefab.SetActive(false);
    }

    public void ActivateLogin()
    {
        loginToDisable.SetActive(true);
        otpScreenPrefab.SetActive(false);
        signupScreenPrefab.SetActive(false);
    }

    public void ActivateSignUp()
    {
        signupScreenPrefab.SetActive(true);
        loginToDisable.SetActive(false);
        otpScreenPrefab.SetActive(false);
    }

    private bool ValidatePhoneNumber(TMP_InputField phoneNumberField)
    {
        string phoneNumber = phoneNumberField.text;
        string pattern = @"^[0-9]{10}$";
        return Regex.IsMatch(phoneNumber, pattern);
    }

    private bool ValidateEmail(TMP_InputField emailField)
    {
        string email = emailField.text;
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }

    private bool ValidateOTP()
    {
        string otp = otpInputField.text;
        string pattern = @"^\d{6}$";
        return Regex.IsMatch(otp, pattern);
    }

    private void CheckInputs()
    {
        sendOtpButton.interactable = ValidatePhoneNumber(phoneNumberSignUpInputField) && ValidateEmail(emailSignUpInputField);
        verifyOtpButton.interactable = ValidateOTP();
        verifyLoginButton.interactable = ValidatePhoneNumber(loginPhoneNumber) && ValidateEmail(loginEmail);
    }
}

public class SignUpPayload
{
    public string email { get; set; }
    public string PhoneNumber { get; set; }
    public string username { get; set; }
    public string Password { get; set; }
    public string deviceID { get; set; }
}

public class LoginPayload
{
    public string email { get; set; }
    public string phone { get; set; }
}

public class OtpPayload
{
    public string otp { get; set; }
    public string phone { get; set; }
}
