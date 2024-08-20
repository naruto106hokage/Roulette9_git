using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement;

public class RegisterUser : MonoBehaviour
{
    public TMP_InputField nameInputField;
    public TMP_InputField emailInputField;
    public TMP_InputField mobileNumberInputField;
    public TMP_InputField passwordInputField;
    public TMP_InputField confirmPasswordInputField;

    public void OnRegisterButtonClicked()
    {
        string name = nameInputField.text;
        string email = emailInputField.text;
        string mobileNumber = mobileNumberInputField.text;
        string password = passwordInputField.text;
        string confirmPassword = confirmPasswordInputField.text;

        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email) &&
            !string.IsNullOrEmpty(mobileNumber) && !string.IsNullOrEmpty(password) &&
            !string.IsNullOrEmpty(confirmPassword))
        {
            if (password == confirmPassword)
            {
                StartCoroutine(SendRegisterRequest(name, email, mobileNumber, password, confirmPassword));
            }
            else
            {
                Debug.LogWarning("Password and Confirm Password do not match.");
            }
        }
        else
        {
            Debug.LogWarning("All fields must be filled.");
        }
    }

    private IEnumerator SendRegisterRequest(string name, string email, string mobileNumber, string password, string confirmPassword)
    {
        string url = "https://utlsolutions.com/color-prediction/api/register";

        var requestBody = new RegisterRequest
        {
            name = name,
            email = email,
            mobile_number = mobileNumber,
            password = password,
            c_password = confirmPassword
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
                Debug.Log("Registration successful.");

                // Assuming the response contains a JSON object with "token" and "user_id"
                RegisterResponse response = JsonUtility.FromJson<RegisterResponse>(request.downloadHandler.text);

                // Save token and user_id in PlayerPrefs
                PlayerPrefs.SetString("token", response.token);
                PlayerPrefs.SetString("user_id", response.user_id);
                PlayerPrefs.Save();

                // Load the MainGame scene
                SceneManager.LoadScene("MainGame");
            }
        }
    }
}

[System.Serializable]
public class RegisterRequest
{
    public string name;
    public string email;
    public string mobile_number;
    public string password;
    public string c_password;
}

[System.Serializable]
public class RegisterResponse
{
    public string token;
    public string user_id;
}
