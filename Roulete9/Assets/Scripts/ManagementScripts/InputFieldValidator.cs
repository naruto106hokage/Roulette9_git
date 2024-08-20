using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InputFieldValidator : MonoBehaviour
{
    public TMP_InputField inputField; // Reference to the TMP InputField for validation
    public Button otpButton; // Reference to the OTP Button
    public int inputLimit = 4; // Character limit for the input field
    public bool manageOtpButton = true; // Boolean to manage if OTP button functionality is used

    void Start()
    {
        // Ensure the input field only accepts numbers and has a character limit
        inputField.characterLimit = inputLimit;
        inputField.contentType = TMP_InputField.ContentType.IntegerNumber;

        // Add listener for value change to validate input
        inputField.onValueChanged.AddListener(delegate { ValidateInput(); });

        if (manageOtpButton && otpButton != null)
        {
            // Initially update the OTP button state
            UpdateOtpButtonState();
            // Add listener for value change to update OTP button state
            inputField.onValueChanged.AddListener(delegate { UpdateOtpButtonState(); });
        }
    }

    void ValidateInput()
    {
        // Remove any non-numeric characters
        string validText = "";
        foreach (char c in inputField.text)
        {
            if (char.IsDigit(c))
            {
                validText += c;
            }
        }

        // Update the input field text with the valid characters only
        if (inputField.text != validText)
        {
            inputField.text = validText;
        }
    }

    void UpdateOtpButtonState()
    {
        if (otpButton != null)
        {
            otpButton.interactable = inputField.text.Length == inputLimit;
        }
    }
}
