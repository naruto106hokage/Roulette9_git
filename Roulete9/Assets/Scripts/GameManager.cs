using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private MoveUIElementAfterTimer uiElementMover;
    [SerializeField] private ListManager listManager;
    [SerializeField] private RouletteManager rouletteManager;
    [SerializeField] private BetManager betManager;
    private List<int> randomNumbers = new List<int>();
    private const int MaxRandomNumbers = 9;
    private float playAgain = 8f;

    private const string PlayerPrefsKey = "RandomNumbersList";

    private void Awake()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        LoadRandomNumbers();

        // Subscribe to the events
        uiElementMover.OnMoveComplete += HandleMoveComplete;
        uiElementMover.OnMoveBackComplete += HandleMoveBackComplete;
    }

    private void Start()
    {
        listManager.setNumberToList(randomNumbers);
        StartCoroutine(RepeatedStartGame());
    }

    private IEnumerator RepeatedStartGame()
    {
        while (true)
        {
           
            yield return StartCoroutine(StartGame());
            yield return new WaitForSeconds(playAgain);
        }
    }

    private IEnumerator StartGame()
    {
        //betManager.EnableButtons(true);
        listManager.disableOrUnable(false);
        uiElementMover.SetStartingPosition();
        uiElementMover.SetupTimerText();

        // Set the timer duration and start it
        float timerDuration = 10f; // Example duration; adjust as needed
        uiElementMover.StartTimer(timerDuration);

        // Wait for the UI element to move to the end position
        while (!uiElementMover.HasMoved)
        {
            yield return null;
        }

        // Generate and store a random number
        int randomNumber = Random.Range(0, rouletteManager.pathPoints.Count);
        Debug.Log("Random Number: " + randomNumber);
        AddRandomNumber(randomNumber);

        //betManager.DisableButtons();
        // Start the roulette wheel spin and ball movement
        rouletteManager.spinTheWheel(randomNumber);

        // Wait for the wheel spin to complete
        yield return new WaitForSeconds(rouletteManager.spinDuration);
        listManager.disableOrUnable(true);
        listManager.displayWinningNumber(randomNumber);
        listManager.setNumberToList(randomNumbers); // Update the list manager

        // Move the UI element back to the starting position
        uiElementMover.StartMovingBack();

        // Wait for the UI element to move back to the start position
        while (uiElementMover.HasMoved)
        {
            yield return null;
        }

        // Restart the timer
        uiElementMover.StartTimer(timerDuration);
    }

    private void AddRandomNumber(int number)
    {
        if (randomNumbers.Count >= MaxRandomNumbers)
        {
            randomNumbers.RemoveAt(0);
        }
        randomNumbers.Add(number);
        SaveRandomNumbers();
    }

    private void SaveRandomNumbers()
    {
        string json = JsonUtility.ToJson(new Serialization<int>(randomNumbers));
        PlayerPrefs.SetString(PlayerPrefsKey, json);
        PlayerPrefs.Save();
    }

    private void LoadRandomNumbers()
    {
        if (PlayerPrefs.HasKey(PlayerPrefsKey))
        {
            string json = PlayerPrefs.GetString(PlayerPrefsKey);
            randomNumbers = JsonUtility.FromJson<Serialization<int>>(json).ToList();
        }
    }

    private void HandleMoveComplete()
    {
        Debug.Log("UI Element reached the final position.");
    }

    private void HandleMoveBackComplete()
    {
        Debug.Log("UI Element moved back to the starting position.");
    }

    [System.Serializable]
    public class Serialization<T>
    {
        public List<T> items;

        public Serialization(List<T> items)
        {
            this.items = items;
        }

        public List<T> ToList()
        {
            return items;
        }
    }
}
