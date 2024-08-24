using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Networking;  // Required for UnityWebRequest
using System.Collections;
using System.Linq;

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

    [SerializeField] private TMP_Text panelNumber;

    public GameObject panelRoulette;
    public Button enableButton;
    private Dictionary<int, Dictionary<string, List<string>>> panelData = new Dictionary<int, Dictionary<string, List<string>>>()
    {
        { 0, new Dictionary<string, List<string>>() {
            { "DP", new List<string>() { "118", "226", "244", "299", "334", "488", "550", "668", "677" } },
            { "SP", new List<string>() { "127", "136", "145", "190", "235", "280", "370", "389", "460", "479", "569", "578" } }
        }},
        { 1, new Dictionary<string, List<string>>() {
            { "DP", new List<string>() { "100", "119", "155", "227", "335", "344", "399", "588", "669" } },
            { "SP", new List<string>() { "128", "137", "146", "236", "245", "290", "380", "470", "489", "560", "579", "678" } }
        }},
        { 2, new Dictionary<string, List<string>>() {
            { "DP", new List<string>() { "110", "200", "228", "255", "336", "499", "660", "688", "778" } },
            { "SP", new List<string>() { "129", "138", "147", "156", "237", "246", "345", "390", "480", "570", "589", "679" } }
        }},
        { 3, new Dictionary<string, List<string>>() {
            { "DP", new List<string>() { "166", "229", "300", "337", "355", "445", "599", "779", "788" } },
            { "SP", new List<string>() { "120", "139", "148", "157", "238", "247", "256", "346", "490", "580", "670", "689" } }
        }},
        { 4, new Dictionary<string, List<string>>() {
            { "DP", new List<string>() { "112", "220", "266", "338", "400", "446", "455", "699", "770" } },
            { "SP", new List<string>() { "130", "149", "158", "167", "239", "248", "257", "347", "356", "590", "680", "789" } }
        }},
        { 5, new Dictionary<string, List<string>>() {
            { "DP", new List<string>() { "113", "122", "177", "339", "366", "447", "500", "799", "889", "555" } },
            { "SP", new List<string>() { "140", "159", "168", "230", "249", "258", "267", "348", "357", "456", "690", "780" } }
        }},
        { 6, new Dictionary<string, List<string>>() {
            { "DP", new List<string>() { "600", "114", "277", "330", "448", "466", "556", "880", "899" } },
            { "SP", new List<string>() { "123", "150", "169", "178", "240", "259", "268", "349", "358", "367", "457", "790" } }
        }},
        { 7, new Dictionary<string, List<string>>() {
            { "DP", new List<string>() { "115", "133", "188", "223", "377", "449", "557", "566", "700" } },
            { "SP", new List<string>() { "124", "160", "278", "179", "250", "269", "340", "359", "368", "458", "467", "890" } }
        }},
        { 8, new Dictionary<string, List<string>>() {
            { "DP", new List<string>() { "116", "224", "233", "288", "440", "477", "558", "800", "990" } },
            { "SP", new List<string>() { "125", "134", "170", "189", "260", "279", "350", "369", "468", "378", "459", "567" } }
        }},
        { 9, new Dictionary<string, List<string>>() {
            { "DP", new List<string>() { "117", "144", "199", "225", "388", "559", "577", "667", "900" } },
            { "SP", new List<string>() { "126", "135", "180", "234", "270", "289", "360", "379", "450", "469", "478", "568" } }
        }},
    };

    private void Start()
    {
        ApplyCustomStyle(firstDropdown);
        ApplyCustomStyle(secondDropdown);
        ApplyCustomStyle(thirdDropdown);

        PopulateFirstDropdown();

        // Initially, only the first dropdown is active
        firstDropdown.gameObject.SetActive(true);
        secondDropdown.gameObject.SetActive(false);
        thirdDropdown.gameObject.SetActive(false);

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
        if (firstDropdown.value > 0)  // Ensure a valid selection
        {
            secondDropdown.gameObject.SetActive(true);
            PopulateSecondDropdown();
        }

        thirdDropdown.gameObject.SetActive(false); // Hide third dropdown when first dropdown changes
    }

    private void PopulateSecondDropdown()
    {
        secondDropdown.ClearOptions();
        List<string> options = new List<string>() { "SP", "DP" };
        secondDropdown.AddOptions(options);
    }

    private void OnSecondDropdownValueChanged()
    {
        if (secondDropdown.value > 0)  // Ensure a valid selection
        {
            thirdDropdown.gameObject.SetActive(true);
            PopulateThirdDropdown();
        }
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

    public void SelectRandomValueBasedOnNumber(int number)
    {
        // Ensure the number is within the valid range and exists in panelData
        if (panelData.ContainsKey(number))
        {
            // Randomly select "DP" or "SP"
            string[] dpOrSpOptions = new string[] { "DP", "SP" };
            string selectedOption = dpOrSpOptions[Random.Range(0, dpOrSpOptions.Length)];

            // Get the list of values for the selected option and number
            List<string> valuesList = panelData[number][selectedOption];

            if (valuesList.Count > 0)
            {
                // Randomly select a value from the list
                string selectedValue = valuesList[Random.Range(0, valuesList.Count)];

                // Output the selected DP/SP and value
                Debug.Log($"Selected: {selectedOption}, Value: {selectedValue}");
                panelNumber.text = selectedValue;
            }
            else
            {
                Debug.LogWarning($"No values found for number {number} and option {selectedOption}");
            }
        }
        else
        {
            Debug.LogError($"Invalid number provided: {number}. Must be between 0 and 9 and exist in the panelData.");
        }
    }

    public void acitvateOrDeactivatePanelText(bool activate)
    {
        panelNumber.gameObject.SetActive(activate);
    }

    private void HandleServerResponse(string response)
    {
        // Handle the server response here (e.g., parse JSON, update UI, etc.)
        Debug.Log("Server response handled: " + response);
    }
    public void DisablePanelAndButton()
    {
        if (panelRoulette != null)
        {
            panelRoulette.SetActive(false);
        }

        if (enableButton != null)
        {
            enableButton.interactable = false;  // Disable the button
        }
    }
    public void enablePanelButton()
    {
        if (enableButton != null)
        {
            enableButton.interactable = true;  // Enable the button
        }
    }

}
