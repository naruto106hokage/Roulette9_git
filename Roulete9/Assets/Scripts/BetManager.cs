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
    [SerializeField] private Sprite[] levelSprites;
    [SerializeField] private int[] thresholds;
    [SerializeField] private Button[] betValueButtons;
    [SerializeField] private CanvasGroup bannerCanvasGroup;
    [SerializeField] private CanvasGroup smallBetBannerCanvasGroup; // New CanvasGroup for small bets
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
        foreach (Button button in buttons)
        {
            button.onClick.AddListener(() => OnButtonClick(button));
        }

        foreach (Button betButton in betValueButtons)
        {
            betButton.onClick.AddListener(() => OnBetValueButtonClick(betButton));
        }

        clearBet.onClick.AddListener(DestroyAllImages);
        doubleBet.onClick.AddListener(DoubleAllBets);
        removeBetButton.onClick.AddListener(ToggleDeleteMode);

        EnableButtons(true);
    }

    private void OnBetValueButtonClick(Button betButton)
    {
        TextMeshProUGUI tmpText = betButton.GetComponentInChildren<TextMeshProUGUI>();
        if (tmpText != null && int.TryParse(tmpText.text, out int value))
        {
            selectedBetValue = value;
            Debug.Log($"Bet value button pressed: {betButton.name}, Value: {selectedBetValue}");
        }
        else
        {
            Debug.LogError("Invalid bet value format or TextMeshPro component missing.");
        }
    }

    private void OnButtonClick(Button clickedButton)
    {
        Debug.Log($"Button pressed: {clickedButton.name}");

        if (selectedBetValue == 0)
        {
            ShowBanner("Select a bet value first.");
            return;
        }

        string buttonName = clickedButton.name;
        if (selectedBetValue < 10)
        {
            if (!IsValidSingleNumberButton(buttonName))
            {
                ShowBanner("Small bets can only be placed on single numbers.");
                return;
            }
        }

        GameObject newImage = Instantiate(imagePrefab, clickedButton.transform.parent);
        newImage.name = buttonName; // Set the name of the new image to the button's name
        RectTransform buttonRectTransform = clickedButton.GetComponent<RectTransform>();
        RectTransform newImageRectTransform = newImage.GetComponent<RectTransform>();

        // Copy position and size from the button
        newImageRectTransform.anchoredPosition = buttonRectTransform.anchoredPosition;
        newImage.transform.localRotation = Quaternion.identity;

        foreach (var neighbourBetImage in neighbourBetImages)
        {
            if (string.Equals(neighbourBetImage.name, buttonName, System.StringComparison.OrdinalIgnoreCase))
            {
                newImageRectTransform.sizeDelta = neighbourBetImage.GetComponent<RectTransform>().sizeDelta;
                newImageRectTransform.anchoredPosition = neighbourBetImage.GetComponent<RectTransform>().anchoredPosition;
                newImageRectTransform.localScale = neighbourBetImage.GetComponent<RectTransform>().localScale;
                newImageRectTransform.pivot = neighbourBetImage.GetComponent<RectTransform>().pivot;
                newImageRectTransform.anchorMin = neighbourBetImage.GetComponent<RectTransform>().anchorMin;
                newImageRectTransform.anchorMax = neighbourBetImage.GetComponent<RectTransform>().anchorMax;
                newImageRectTransform.rotation = neighbourBetImage.GetComponent<RectTransform>().rotation;
                break;
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

        if (int.TryParse(tmpComponent.text, out int currentValue))
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
        foreach (Button button in buttons)
        {
            button.interactable = false;
        }
    }

    public void EnableButtons(bool enable)
    {
        foreach (Button button in buttons)
        {
            button.interactable = enable;
        }
    }

    private void ShowBanner(string message)
    {
        if (selectedBetValue == 0)
        {
            // Display the general banner when no bet value is selected
            totalBetValueText.text = message;
            bannerCanvasGroup.alpha = 1;
            bannerCanvasGroup.gameObject.SetActive(true);
            Invoke("FadeOutBanner", 1f);
        }
        else if (selectedBetValue < 10)
        {
            // Display the small bet banner if the selected bet value is less than 10
            smallBetBannerCanvasGroup.alpha = 1;
            smallBetBannerCanvasGroup.gameObject.SetActive(true);
            Invoke("FadeOutSmallBetBanner", 1f);
        }
        else
        {
            // Display the general banner for other cases
            totalBetValueText.text = message;
            bannerCanvasGroup.alpha = 1;
            bannerCanvasGroup.gameObject.SetActive(true);
            Invoke("FadeOutBanner", 1f);
        }
    }

    private void FadeOutSmallBetBanner()
    {
        StartCoroutine(FadeCanvasGroup(smallBetBannerCanvasGroup, 1f, 0f, 3f));
    }

    private void FadeOutBanner()
    {
        StartCoroutine(FadeCanvasGroup(bannerCanvasGroup, 1f, 0f, 3f));
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            cg.alpha = Mathf.Lerp(start, end, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        cg.alpha = end;
        cg.gameObject.SetActive(end > 0);
    }

    private void UpdateBets(string position, int amount)
    {
        if (betsPlaced.ContainsKey(position))
        {
            betsPlaced[position] += amount;
        }
        else
        {
            betsPlaced[position] = amount;
        }
    }

    public void DestroyAllImages()
    {
        foreach (var image in imagesToActivate)
        {
            Destroy(image);
        }
        imagesToActivate.Clear();
        betsPlaced.Clear();
        UpdateTotalBetValue();
    }

    public void DeactivateAllImages()
    {
        foreach (var image in imagesToActivate)
        {
            image.SetActive(false);
        }
    }

    private void DoubleAllBets()
    {
        foreach (var key in betsPlaced.Keys.ToList())
        {
            betsPlaced[key] *= 2;
        }
        UpdateTotalBetValue();
    }

    private void ToggleDeleteMode()
    {
        isDeleteModeActive = !isDeleteModeActive;
        string mode = isDeleteModeActive ? "Delete Mode Activated" : "Delete Mode Deactivated";
        ShowBanner(mode);
    }

    private void UpdateTotalBetValue()
    {
        int totalBetValue = 0;
        foreach (var bet in betsPlaced.Values)
        {
            totalBetValue += bet;
        }
        totalBetValueText.text = "Total Bet: " + totalBetValue;

        // Activate or deactivate buttons based on total bet value
        bool buttonsActive = totalBetValue > 0;
        clearBet.gameObject.SetActive(buttonsActive);
        doubleBet.gameObject.SetActive(buttonsActive);
        removeBetButton.gameObject.SetActive(buttonsActive);
    }

    public Dictionary<string, int> displayBetPositions()
    {
        return positionOfBetPlaced;
    }
}
