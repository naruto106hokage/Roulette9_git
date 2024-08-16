using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

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
        // Retrieve bet value from the TextMeshProUGUI component of the button
        TextMeshProUGUI betValueText = betValueButton.GetComponentInChildren<TextMeshProUGUI>();
        Debug.Log("betValueText: "+ betValueText);
        if (betValueText != null && int.TryParse(betValueText.text, out int value))
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
            Destroy(chip);
            imagesToActivate.Remove(chip);
            positionOfBetPlaced.Remove(chip.name); // Remove the chip's name from the dictionary

            string buttonName = chip.transform.parent.name;
            if (betsPlaced.ContainsKey(buttonName))
            {
                betsPlaced[buttonName] -= selectedBetValue;
                if (betsPlaced[buttonName] <= 0)
                {
                    betsPlaced.Remove(buttonName);
                }
                UpdateTotalBetValue();
            }
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

            UpdateBets(chip.transform.parent.name, selectedBetValue);
            UpdateTotalBetValue();
        }
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
                if (button != null)
                {
                    button.interactable = false;
                }
            }
        }
    }

    public void EnableButtons(bool enable)
    {
        if (buttons != null)
        {
            foreach (Button button in buttons)
            {
                if (button != null)
                {
                    button.interactable = enable;
                }
            }
        }
    }

    public void DestroyAllImages()
    {
        foreach (GameObject image in imagesToActivate)
        {
            Destroy(image);
        }
        imagesToActivate.Clear();
        betsPlaced.Clear();
        positionOfBetPlaced.Clear(); // Clear the dictionary when all images are destroyed
        UpdateTotalBetValue();
    }

    private void DoubleAllBets()
    {
        List<GameObject> currentImages = new List<GameObject>(imagesToActivate);
        foreach (GameObject image in currentImages)
        {
            OnChipClick(null, image); // Double the bet on each chip
        }
        UpdateTotalBetValue();
    }

    private void UpdateBets(string buttonName, int betValue)
    {
        if (betsPlaced.ContainsKey(buttonName))
        {
            betsPlaced[buttonName] += betValue;
        }
        else
        {
            betsPlaced[buttonName] = betValue;
        }
        UpdateTotalBetValue();
    }

    private void ToggleDeleteMode()
    {
        isDeleteModeActive = !isDeleteModeActive;
        string mode = isDeleteModeActive ? "Delete Mode Activated" : "Delete Mode Deactivated";
        Debug.Log(mode);
    }

    private void UpdateTotalBetValue()
    {
        if (totalBetValueText != null)
        {
            int totalBetValue = 0;
            foreach (var bet in betsPlaced.Values)
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

    public Dictionary<string, int> displayBetPositions()
    {
        return positionOfBetPlaced;
    }
}
