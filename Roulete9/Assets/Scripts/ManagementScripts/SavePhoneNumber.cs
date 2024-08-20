using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SavePhoneNumber : MonoBehaviour
{
    public TMP_InputField phoneNumberInputField;
    public Toggle TMConditionsToggle;
    public Toggle LMSPCOToggle;
    public string sceneName; // The name of the scene to load

    // Call this method when you want to save the phone number
    public void SavePhoneNumberAndChangeScene()
    {
        string phoneNumber = phoneNumberInputField.text;
        if (!string.IsNullOrEmpty(phoneNumber) && TMConditionsToggle.isOn && LMSPCOToggle.isOn)
        {
            PlayerPrefs.SetString("PhoneNumber", phoneNumber);
            PlayerPrefs.Save();
            Debug.Log("Phone number saved: " + phoneNumber);
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("Phone number input field is empty.");
        }
    }
}
