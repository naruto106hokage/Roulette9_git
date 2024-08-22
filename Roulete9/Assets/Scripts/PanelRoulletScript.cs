using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class PanelRoulletScript : MonoBehaviour
{
    public TMP_Dropdown firstDropdown;  // Dropdown for values 0-9
    public TMP_Dropdown secondDropdown; // Dropdown for "SP" or "DP"
    public TMP_Dropdown thirdDropdown;  // Dropdown for numbers based on table

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
        firstDropdown.onValueChanged.AddListener(delegate { OnFirstDropdownValueChanged(); });
        secondDropdown.onValueChanged.AddListener(delegate { OnSecondDropdownValueChanged(); });
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
        // Set the font asset
        dropdown.captionText.font = customFont;
        dropdown.itemText.font = customFont;

        // Set text colors
        dropdown.captionText.color = normalTextColor;
        dropdown.itemText.color = normalTextColor;

        // Set background color for the dropdown
        Image dropdownImage = dropdown.GetComponent<Image>();
        if (dropdownImage != null)
        {
            dropdownImage.color = dropdownBackgroundColor;
        }

        // Set the background color for each dropdown item
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
                    itemBackground.color = dropdownBackgroundColor; // Set item background color
                }

                TMP_Text itemText = item.Find("Item Label").GetComponent<TMP_Text>();
                if (itemText != null)
                {
                    itemText.color = normalTextColor; // Set normal text color
                }

                // Change highlighted text color
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

        // Adjust padding for better spacing
        dropdown.captionText.margin = new Vector4(dropdownPadding.x, dropdownPadding.y, dropdownPadding.x, dropdownPadding.y);
    }

}
