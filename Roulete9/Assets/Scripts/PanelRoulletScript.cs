using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Networking;  // Required for UnityWebRequest
using System.Collections;

public class PanelRoulletScript : MonoBehaviour
{
    public TMP_Dropdown firstDropdown;  // Dropdown for values 0-9
    public TMP_Dropdown secondDropdown; // Dropdown for "SP" or "DP"
    public TMP_Dropdown thirdDropdown;  // Dropdown for numbers based on table
    public TMP_InputField betAmountInput;  // Input field for bet amount
    public Button submitButton;  // Button to send the response to the server

    public TMP_FontAsset customFont;  // Assign a custom font asset in the Inspector
    public Color normalTextColor = Color.white;  // Normal text color
    public Color highlightedTextColor = new Color(1, 0.8f, 0);  // Highlighted text color (e.g., gold)
    public Color dropdownBackgroundColor = new Color(0, 0, 0, 0.5f);  // Semi-transparent background
    public Vector2 dropdownPadding = new Vector2(10, 10);  // Padding for dropdown items

    private Dictionary<int, Dictionary<string, List<string>>> panelData = new Dictionary<int, Dictionary<string, List<string>>>()
    {
        { 0, new Dictionary<string, List<string>>() {
            { "DP", new List<string>() { "118", "226", "244", "299", "334", "488", "550", "668", "677" } },
            { "SP", new List<string>() { "127", "136", "145", "190", "235", "280", "370", "389", "460", "479", "569", "578" } }
        }},
        // Add the rest of your data here...
    };

    private void Start()
    {
        ApplyCustomStyle(firstDropdown);
        ApplyCustomStyle(secondDropdown);
        ApplyCustomStyle(thirdDropdown);

        PopulateFirstDropdown();
        firstDropdown.onValueChanged.AddListener(delegate { OnFirstDropdownValueChanged(); });
        secondDropdown.onValueChanged.AddListener(delegate { OnSecondDropdownValueChanged(); });

        // Add listener for the submit button
        submitButton.onClick.AddListener(OnSubmitButtonClicked);
    }

    private void PopulateFirstDropdown()
    {
        firstDropdown.ClearOptions();
        List<string> options = new List<string>();

        for (int i = 0; i <= 9; i++)
        {
            options.Add(i.ToString());
        }

        firstDropdown.AddOptions(options);
    }

    private void OnFirstDropdownValueChanged()
    {
        PopulateSecondDropdown();
        thirdDropdown.ClearOptions(); // Clear third dropdown when first dropdown changes
    }

    private void PopulateSecondDropdown()
    {
        secondDropdown.ClearOptions();
        List<string> options = new List<string>() { "SP", "DP" };
        secondDropdown.AddOptions(options);
    }

    private void OnSecondDropdownValueChanged()
    {
        PopulateThirdDropdown();
    }

    private void PopulateThirdDropdown()
    {
        thirdDropdown.ClearOptions();

        int selectedValue = firstDropdown.value;
        string panelType = secondDropdown.options[secondDropdown.value].text;

        if (panelData.ContainsKey(selectedValue) && panelData[selectedValue].ContainsKey(panelType))
        {
            thirdDropdown.AddOptions(panelData[selectedValue][panelType]);
        }
    }

    private void ApplyCustomStyle(TMP_Dropdown dropdown)
    {
        dropdown.captionText.font = customFont;
        dropdown.itemText.font = customFont;

        dropdown.captionText.color = normalTextColor;
        dropdown.itemText.color = normalTextColor;

        Image dropdownImage = dropdown.GetComponent<Image>();
        if (dropdownImage != null)
        {
            dropdownImage.color = dropdownBackgroundColor;
        }

        Transform template = dropdown.transform.Find("Template");
        if (template != null)
        {
            Image templateImage = template.GetComponent<Image>();
            if (templateImage != null)
            {
                templateImage.color = dropdownBackgroundColor;
            }

            Transform item = template.Find("Viewport/Content/Item");
            if (item != null)
            {
                Image itemBackground = item.GetComponent<Image>();
                if (itemBackground != null)
                {
                    itemBackground.color = dropdownBackgroundColor;
                }

                TMP_Text itemText = item.Find("Item Label").GetComponent<TMP_Text>();
                if (itemText != null)
                {
                    itemText.color = normalTextColor;
                }

                Toggle toggle = item.GetComponent<Toggle>();
                if (toggle != null)
                {
                    ColorBlock colorBlock = toggle.colors;
                    colorBlock.normalColor = dropdownBackgroundColor;
                    colorBlock.highlightedColor = highlightedTextColor;
                    toggle.colors = colorBlock;
                }
            }
        }

        dropdown.captionText.margin = new Vector4(dropdownPadding.x, dropdownPadding.y, dropdownPadding.x, dropdownPadding.y);
    }

    private void OnSubmitButtonClicked()
    {
        string betAmount = betAmountInput.text;
        string selectedFirstDropdown = firstDropdown.options[firstDropdown.value].text;
        string selectedSecondDropdown = secondDropdown.options[secondDropdown.value].text;
        string selectedThirdDropdown = thirdDropdown.options.Count > 0 ? thirdDropdown.options[thirdDropdown.value].text : "None";

        Debug.Log($"Bet Amount: {betAmount}, First Dropdown: {selectedFirstDropdown}, Second Dropdown: {selectedSecondDropdown}, Third Dropdown: {selectedThirdDropdown}");

        // Send the data to the server
        StartCoroutine(SendBetDataToServer(betAmount, selectedFirstDropdown, selectedSecondDropdown, selectedThirdDropdown));
    }

    private IEnumerator SendBetDataToServer(string betAmount, string first, string second, string third)
    {
        string url = "http://yourserver.com/api/bet";  // Replace with your actual server URL
        WWWForm form = new WWWForm();
        form.AddField("betAmount", betAmount);
        form.AddField("firstDropdown", first);
        form.AddField("secondDropdown", second);
        form.AddField("thirdDropdown", third);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error sending data: " + www.error);
            }
            else
            {
                Debug.Log("Response from server: " + www.downloadHandler.text);
                HandleServerResponse(www.downloadHandler.text);
            }
        }
    }

    private void HandleServerResponse(string response)
    {
        // Handle the server response here (e.g., parse JSON, update UI, etc.)
        Debug.Log("Server response handled: " + response);
    }
}
