using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BetManager : MonoBehaviour
{
    [SerializeField] private GameObject imagePrefab;
    [SerializeField] private Button[] buttons;
    [SerializeField] private Button[] betValueButtons;
    [SerializeField] private Sprite[] levelSprites;
    [SerializeField] private int[] thresholds;
    [SerializeField] private TextMeshProUGUI totalBetValueText;
    [SerializeField] private Image[] neighbourBetImages;
    [SerializeField] private Button clearBet;
    [SerializeField] private Button doubleBet;
    [SerializeField] private Button removeBetButton; // Button to activate delete mode
    [SerializeField] private GameObject notice; // Button to activate delete mode

    private int selectedBetValue = 0;
    private bool isDeleteModeActive = false;

    private List<GameObject> imagesToActivate = new List<GameObject>();
    private Dictionary<string, int> betsPlaced = new Dictionary<string, int>();
    private Dictionary<string, int> positionOfBetPlaced = new Dictionary<string, int>(); // Dictionary to store image names and bet amounts

    void Start()
    {
        // Add null checks before adding listeners or performing operations
        if (buttons != null)
        {
            foreach (Button button in buttons)
            {
                if (button != null)
                {
                    button.onClick.AddListener(() => OnButtonClick(button));
                }
                else
                {
                    Debug.LogWarning("One of the buttons is null.");
                }
            }
        }

        if (betValueButtons != null)
        {
            foreach (Button betValueButton in betValueButtons)
            {
                if (betValueButton != null)
                {
                    betValueButton.onClick.AddListener(() => SetBetValue(betValueButton));
                }
                else
                {
                    Debug.LogWarning("One of the bet value buttons is null.");
                }
            }
        }

        if (clearBet != null)
        {
            clearBet.onClick.AddListener(DestroyAllImages);
        }
        else
        {
            Debug.LogWarning("Clear Bet button is null.");
        }

        if (doubleBet != null)
        {
            doubleBet.onClick.AddListener(DoubleAllBets);
        }
        else
        {
            Debug.LogWarning("Double Bet button is null.");
        }

        if (removeBetButton != null)
        {
            removeBetButton.onClick.AddListener(ToggleDeleteMode);
        }
        else
        {
            Debug.LogWarning("Remove Bet button is null.");
        }

        EnableButtons(true);
    }

    private void SetBetValue(Button betValueButton)
    {
        selectedBetValue = 0;
        if (betValueButton != null && int.TryParse(betValueButton.name, out int value))
        {
            selectedBetValue = value;
            Debug.Log($"Bet value set to: {selectedBetValue}");
        }
        else
        {
            Debug.LogWarning("Bet value button does not have a valid integer text.");
        }
    }

    private void OnButtonClick(Button clickedButton)
    {
        Debug.Log($"Button pressed: {clickedButton?.name}");

        if (selectedBetValue == 0)
        {
            activateNotice();
            Debug.LogWarning("No bet value selected.");
            return;
        }

        string buttonName = clickedButton.name;
        if (selectedBetValue < 10 && !IsValidSingleNumberButton(buttonName))
        {
            Debug.LogWarning("Small bets can only be placed on single numbers.");
            return;
        }

        GameObject newImage = Instantiate(imagePrefab, clickedButton.transform.parent);
        newImage.name = buttonName; // Set the name of the new image to the button's name
        RectTransform buttonRectTransform = clickedButton.GetComponent<RectTransform>();
        RectTransform newImageRectTransform = newImage.GetComponent<RectTransform>();

        // Copy position and size from the button
        if (buttonRectTransform != null && newImageRectTransform != null)
        {
            newImageRectTransform.anchoredPosition = buttonRectTransform.anchoredPosition;
            newImage.transform.localRotation = Quaternion.identity;

            if (neighbourBetImages != null)
            {
                foreach (var neighbourBetImage in neighbourBetImages)
                {
                    if (neighbourBetImage != null && string.Equals(neighbourBetImage.name, buttonName, System.StringComparison.OrdinalIgnoreCase))
                    {
                        RectTransform neighbourRectTransform = neighbourBetImage.GetComponent<RectTransform>();
                        if (neighbourRectTransform != null)
                        {
                            newImageRectTransform.sizeDelta = neighbourRectTransform.sizeDelta;
                            newImageRectTransform.anchoredPosition = neighbourRectTransform.anchoredPosition;
                            newImageRectTransform.localScale = neighbourRectTransform.localScale;
                            newImageRectTransform.pivot = neighbourRectTransform.pivot;
                            newImageRectTransform.anchorMin = neighbourRectTransform.anchorMin;
                            newImageRectTransform.anchorMax = neighbourRectTransform.anchorMax;
                            newImageRectTransform.rotation = neighbourRectTransform.rotation;
                        }
                        break;
                    }
                }
            }
        }

        imagesToActivate.Add(newImage);
        positionOfBetPlaced[newImage.name] = selectedBetValue; // Add the name of the new image and the bet amount to the dictionary

        TextMeshProUGUI tmpComponent = newImage.GetComponentInChildren<TextMeshProUGUI>();
        if (tmpComponent == null)
        {
            GameObject tmpObj = new GameObject("TMPText");
            tmpObj.transform.SetParent(newImage.transform);

            tmpComponent = tmpObj.AddComponent<TextMeshProUGUI>();
            tmpComponent.text = "0";
            tmpComponent.alignment = TextAlignmentOptions.Center;
            tmpComponent.color = UnityEngine.Color.black;
            tmpComponent.fontSize = 20;

            RectTransform rectTransform = tmpComponent.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(40, 40);
            rectTransform.anchoredPosition = Vector2.zero;
        }

        if (tmpComponent != null && int.TryParse(tmpComponent.text, out int currentValue))
        {
            int newValue = currentValue + selectedBetValue;
            tmpComponent.text = newValue.ToString();

            Sprite appropriateSprite = GetSpriteForValue(newValue);
            if (appropriateSprite != null)
            {
                Image imgComponent = newImage.GetComponent<Image>();
                if (imgComponent != null)
                {
                    imgComponent.sprite = appropriateSprite;
                }
            }
        }
        else
        {
            Debug.LogWarning("TextMeshPro component does not contain a valid integer.");
        }

        AddClickEventToChip(newImage);

        UpdateBets(buttonName, selectedBetValue);
        UpdateTotalBetValue();
    }
    private void activateNotice()
    {
        notice.SetActive(true);
        Invoke("deactivateNotice", 1.5f);
    }
    private void deactivateNotice()
    {
        notice.SetActive(false);
    }
    private bool IsValidSingleNumberButton(string buttonName)
    {
        if (int.TryParse(buttonName, out int number))
        {
            return number >= 0 && number <= 36;
        }
        return false;
    }

    private void AddClickEventToChip(GameObject chip)
    {
        EventTrigger trigger = chip.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => { OnChipClick((PointerEventData)data, chip); });
        trigger.triggers.Add(entry);
    }

    private void OnChipClick(PointerEventData data, GameObject chip)
    {
        if (isDeleteModeActive)
        {
            // Remove the chip and update the total bet value
            string chipName = chip.name;
            if (positionOfBetPlaced.ContainsKey(chipName))
            {
                int chipBetValue = positionOfBetPlaced[chipName];
                positionOfBetPlaced.Remove(chipName);
                betsPlaced[chipName] -= chipBetValue;

                if (betsPlaced[chipName] <= 0)
                {
                    betsPlaced.Remove(chipName);
                }

                UpdateTotalBetValue();
            }
            Destroy(chip);
            imagesToActivate.Remove(chip);
            return;
        }

        TextMeshProUGUI tmpComponent = chip.GetComponentInChildren<TextMeshProUGUI>();
        if (tmpComponent != null && int.TryParse(tmpComponent.text, out int currentValue))
        {
            int newValue = currentValue + selectedBetValue;
            tmpComponent.text = newValue.ToString();

            Sprite appropriateSprite = GetSpriteForValue(newValue);
            if (appropriateSprite != null)
            {
                Image imgComponent = chip.GetComponent<Image>();
                if (imgComponent != null)
                {
                    imgComponent.sprite = appropriateSprite;
                }
            }

            positionOfBetPlaced[chip.name] = newValue;
            UpdateBets(chip.name, selectedBetValue);
        }
        else
        {
            Debug.LogWarning("TextMeshPro component does not contain a valid integer.");
        }

        UpdateTotalBetValue();
    }

    private Sprite GetSpriteForValue(int value)
    {
        for (int i = thresholds.Length - 1; i >= 0; i--)
        {
            if (value >= thresholds[i])
            {
                return levelSprites[i];
            }
        }
        return null;
    }

    public void InvokeDisableButtons(float time)
    {
        Invoke("DisableButtons", time);
    }

    public void DisableButtons()
    {
        if (buttons != null)
        {
            foreach (Button button in buttons)
            {
                button.interactable = false;
            }
        }
    }

    public void EnableButtons(bool enable)
    {
        if (buttons != null)
        {
            foreach (Button button in buttons)
            {
                button.interactable = enable;
            }
        }
    }

    private void UpdateBets(string position, int value)
    {
        if (betsPlaced.ContainsKey(position))
        {
            betsPlaced[position] += value;
        }
        else
        {
            betsPlaced[position] = value;
        }
    }

    public void DestroyAllImages()
    {
        foreach (GameObject img in imagesToActivate)
        {
            Destroy(img);
        }

        imagesToActivate.Clear();
        positionOfBetPlaced.Clear();
        betsPlaced.Clear();
        UpdateTotalBetValue();
    }

    private void DoubleAllBets()
    {
        Dictionary<string, int> newBetsPlaced = new Dictionary<string, int>(betsPlaced);

        foreach (KeyValuePair<string, int> bet in betsPlaced)
        {
            string position = bet.Key;
            int value = bet.Value;

            newBetsPlaced[position] = value * 2;
            foreach (GameObject img in imagesToActivate)
            {
                if (img.name == position)
                {
                    TextMeshProUGUI tmpComponent = img.GetComponentInChildren<TextMeshProUGUI>();
                    if (tmpComponent != null)
                    {
                        int newValue = value * 2;
                        tmpComponent.text = newValue.ToString();

                        Sprite appropriateSprite = GetSpriteForValue(newValue);
                        if (appropriateSprite != null)
                        {
                            Image imgComponent = img.GetComponent<Image>();
                            if (imgComponent != null)
                            {
                                imgComponent.sprite = appropriateSprite;
                            }
                        }
                    }
                }
            }
        }

        betsPlaced = newBetsPlaced;
        UpdateTotalBetValue();
    }

    private void UpdateTotalBetValue()
    {
        if (totalBetValueText != null)
        {
            int totalBetValue = 0;
            foreach (var bet in positionOfBetPlaced.Values)
            {
                totalBetValue += bet;
            }
            totalBetValueText.text = "Total Bet: " + totalBetValue;

            // Activate or deactivate buttons based on total bet value
            bool buttonsActive = totalBetValue > 0;
            if (clearBet != null) clearBet.gameObject.SetActive(buttonsActive);
            if (doubleBet != null) doubleBet.gameObject.SetActive(buttonsActive);
            if (removeBetButton != null) removeBetButton.gameObject.SetActive(buttonsActive);
        }
        else
        {
            Debug.LogWarning("Total Bet Value Text is null.");
        }
    }

    private void ToggleDeleteMode()
    {
        isDeleteModeActive = !isDeleteModeActive;
        Debug.Log("Delete mode: " + (isDeleteModeActive ? "Active" : "Inactive"));
    }

    public void SendBetsToServer()
    {
        // Dictionary to store the last bet value for each position
        Dictionary<string, int> lastBetValues = new Dictionary<string, int>();

        foreach (GameObject img in imagesToActivate)
        {
            string position = img.name;
            TextMeshProUGUI tmpComponent = img.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpComponent != null)
            {
                int betValue = int.Parse(tmpComponent.text); // Get the value on the chip

                // Only keep the last value for each position
                lastBetValues[position] = betValue;
            }
        }

        // Example: Sending data using a hypothetical network manager
        // NetworkManager.SendBetData(lastBetValues);

        // Debug log to simulate sending the data
        foreach (var bet in lastBetValues)
        {
            Debug.Log($"Sending bet: Position - {bet.Key}, Value - {bet.Value}");
        }
    }
    public void DeactivateAllImages()
    {
        foreach (GameObject img in imagesToActivate)
        {
            if (img != null)
            {
                img.SetActive(false);
            }
        }

        // Optionally clear the dictionaries if deactivation means the bets are also cleared
        positionOfBetPlaced.Clear();
        betsPlaced.Clear();

        // Update the UI after deactivation
        //UpdateTotalBetValue();
    }

}
