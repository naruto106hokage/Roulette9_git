using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement;

public class SendVerificationCode : MonoBehaviour
{
    public TMP_InputField otpInputField; // Reference to the OTP input field
    public TMP_Text numberField; // Reference to the phone number display field

    private void Start()
    {
        numberField.text = PlayerPrefs.GetString("PhoneNumber", string.Empty);
    }

    public void OnSendCodeButtonClicked()
    {
        string phoneNumber = PlayerPrefs.GetString("PhoneNumber", string.Empty); // Get phone number from PlayerPrefs
        string otp = otpInputField.text; // Get OTP from input field

        if (!string.IsNullOrEmpty(phoneNumber) && !string.IsNullOrEmpty(otp))
        {
            StartCoroutine(SendVerificationCodeRequest(phoneNumber, otp));
        }
        else
        {
            Debug.LogWarning("Phone number or OTP is empty.");
        }
    }

    private IEnumerator SendVerificationCodeRequest(string phoneNumber, string otp)
    {
        string url = "https://utlsolutions.com/color-prediction/api/login"; // Updated URL

        // Create the JSON body using a defined class
        var requestBody = new VerificationRequest
        {
            phoneNumber = phoneNumber,
            otp = otp
        };
        string jsonString = JsonUtility.ToJson(requestBody);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonString);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

#if UNITY_2019_1_OR_NEWER
            if (request.isNetworkError || request.isHttpError)
#else
            if (request.isError)
#endif
            {
                Debug.LogError("Error: " + request.error);
                Debug.LogError("Response Code: " + request.responseCode);
                Debug.LogError("Response Text: " + request.downloadHandler.text);
            }
            else
            {
                Debug.Log("Verification code sent successfully.");
                string jsonResponse = request.downloadHandler.text;
                VerificationResponse verificationResponse = JsonUtility.FromJson<VerificationResponse>(jsonResponse);

                // Save the auth token and user ID in PlayerPrefs
                PlayerPrefs.SetString("AuthToken", verificationResponse.data.token);
                PlayerPrefs.SetInt("UserId", verificationResponse.data.user.id);
                PlayerPrefs.Save();

                Debug.Log("Auth Token and User ID saved.");

                // Load the next scene after successful login
                SceneManager.LoadScene("MainGame");
            }
        }
    }
}

[System.Serializable]
public class VerificationRequest
{
    public string phoneNumber;
    public string otp;
}

[System.Serializable]
public class VerificationResponse
{
    public bool success;
    public VerificationData data;
    public string message;
}

[System.Serializable]
public class VerificationData
{
    public string token;
    public User user;
}

[System.Serializable]
public class User
{
    public int id;
    public string name;
    public string email;
    public string mobile_number;
    public string email_verified_at;
    public string created_at;
    public string updated_at;
}
