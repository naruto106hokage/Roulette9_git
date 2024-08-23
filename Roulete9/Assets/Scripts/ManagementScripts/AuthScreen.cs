using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json.Linq;
using UnityEngine.SceneManagement;

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
        if (ValidatePhoneNumber(loginPhoneNumber))
        {
            StartCoroutine(doLoginVerify());
        }
    }

    private IEnumerator doSignUp()
    {
        string url = "https://utlnews.com/roulette/api/player/signup";

        WWWForm form = new WWWForm();
        form.AddField("name", username.text);
        form.AddField("phone_number", phoneNumberSignUpInputField.text);

        using (UnityWebRequest request = UnityWebRequest.Post(url, form))
        {
            request.SetRequestHeader("Accept", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
                errorPrefab.SetActive(true);
                errorText.text = "Sign up failed. Please try again.";
            }
            else
            {
                Debug.Log(request.downloadHandler.text);
                JObject jsonResponse = JObject.Parse(request.downloadHandler.text);
                string authKey = jsonResponse["authKey"].ToString();
                PlayerPrefs.SetString("authKey", authKey);
                PlayerPrefs.SetString("login", "YES");
                SceneManager.LoadScene(1);
            }
        }
    }

    private IEnumerator doLoginVerify()
    {
        string url = "https://utlnews.com/roulette/api/player/login";

        WWWForm form = new WWWForm();
        form.AddField("phone_number", loginPhoneNumber.text);

        using (UnityWebRequest request = UnityWebRequest.Post(url, form))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
                errorPrefab.SetActive(true);
                errorText.text = "Login failed. Please try again.";
            }
            else
            {
                Debug.Log(request.downloadHandler.text);
                ShowOtpScreen();
                PlayerPrefs.SetString("PlayerNumber", loginPhoneNumber.text);
            }
        }
    }

    private IEnumerator doVerifyOtp()
    {
        string url = "https://utlnews.com/roulette/api/player/verifyOtp";

        WWWForm form = new WWWForm();
        form.AddField("phone_number", PlayerPrefs.GetString("PlayerNumber"));
        form.AddField("verify_otp", otpInputField.text);

        using (UnityWebRequest request = UnityWebRequest.Post(url, form))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
                errorPrefab.SetActive(true);
                errorText.text = "OTP verification failed. Please try again.";
            }
            else
            {
                Debug.Log(request.downloadHandler.text);
                JObject jsonResponse = JObject.Parse(request.downloadHandler.text);
                string authKey = jsonResponse["authKey"].ToString();
                PlayerPrefs.SetString("authKey", authKey);
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
        verifyLoginButton.interactable = ValidatePhoneNumber(loginPhoneNumber);
    }
}
