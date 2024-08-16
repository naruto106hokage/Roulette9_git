using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private MoveUIElementAfterTimer uiElementMover; 
    [SerializeField] private ListManager listManager; 
    private List<int> randomNumbers = new List<int>();
    private const int MaxRandomNumbers = 9;
    private float playAgain = 5f;

    private const string PlayerPrefsKey = "RandomNumbersList";

    private void Awake()
    {
        LoadRandomNumbers();
    }
    private void Start()
    {
        //uiElementMover.StartTimer();
        // Start the timer when the game starts
        listManager.setNumberToList(randomNumbers);
        StartCoroutine("RepeatedStartGame");
    }
    IEnumerator RepeatedStartGame()
    {
        while (true)
        {
            yield return StartCoroutine(startGame());
            yield return new WaitForSeconds(playAgain);
        }
    }

    private IEnumerator startGame()
    {
        int randomNumber = Random.Range(0, 10);
        Debug.Log("Random Number: " + randomNumber);
        AddRandomNumber(randomNumber);
        //uiElementMover.StartTimer();
        yield return null;
    }

    public void ResetUIElement()
    {
        // Reset the UI element and timer
        uiElementMover.ResetElement();
    }

    public void TriggerMovementImmediately()
    {
        // Trigger the movement immediately without waiting for the timer
        //uiElementMover.StartMovement();
    }
    void AddRandomNumber(int number)
    {
        if (randomNumbers.Count >= MaxRandomNumbers)
        {
            randomNumbers.RemoveAt(0); // Remove the oldest number
        }
        randomNumbers.Add(number);
        SaveRandomNumbers();
    }

    void SaveRandomNumbers()
    {
        string json = JsonUtility.ToJson(new Serialization<int>(randomNumbers));
        PlayerPrefs.SetString(PlayerPrefsKey, json);
        PlayerPrefs.Save();
    }
    void LoadRandomNumbers()
    {
        if (PlayerPrefs.HasKey(PlayerPrefsKey))
        {
            string json = PlayerPrefs.GetString(PlayerPrefsKey);
            randomNumbers = JsonUtility.FromJson<Serialization<int>>(json).ToList();
        }
    }
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

