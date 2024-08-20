using UnityEngine;
using TMPro;
using System;
using System.Collections;

public class MoveUIElementAfterTimer : MonoBehaviour
{
    [SerializeField] private RectTransform uiElementToMove;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float startingXPosition = 0f;
    [SerializeField] private float endingXPosition = 550f;
    [SerializeField] private float timeToWait = 20f;
    [SerializeField] private float moveDuration = 1f;
    [SerializeField] private Color startColor = Color.green;
    [SerializeField] private Color endColor = Color.red;
    [SerializeField] private TMP_FontAsset timerFont;
    [SerializeField] private int timerFontSize = 40;
    [SerializeField] private float delayBeforeMovingBack = 10f;

    private float elapsedTime = 0f;
    private bool isMoving = false;
    private bool hasMoved = false;
    private float moveStartTime;
    private bool movingBack = false;

    // Event for when the UI element reaches the final position
    public event Action OnMoveComplete;

    // Event for when the UI element has moved back to the start position
    public event Action OnMoveBackComplete;

    // Property to check if the UI element has moved
    public bool HasMoved => hasMoved;

    private void Awake()
    {
        SetStartingPosition();
        SetupTimerText();
    }

    private void Update()
    {
        if (isMoving)
        {
            MoveUIElement();
        }
        else if (movingBack)
        {
            MoveBackToStartPosition();
        }
    }

    public void StartTimer(float duration)
    {
        timeToWait = duration; // Set the timer duration
        elapsedTime = 0f;
        hasMoved = false;
        isMoving = false;
        movingBack = false;

        StartCoroutine(UpdateTimer());
    }

    private IEnumerator UpdateTimer()
    {
        while (elapsedTime < timeToWait && !hasMoved)
        {
            elapsedTime += Time.deltaTime;
            float remainingTime = Mathf.Max(0, timeToWait - elapsedTime);

            UpdateTimerText(remainingTime);
            yield return null;
        }

        if (!hasMoved)
        {
            StartMovement();
        }
    }

    public void SetStartingPosition()
    {
        if (uiElementToMove != null)
        {
            uiElementToMove.anchoredPosition = new Vector2(startingXPosition, uiElementToMove.anchoredPosition.y);
        }
    }

    public void SetupTimerText()
    {
        if (timerText != null)
        {
            timerText.font = timerFont;
            timerText.fontSize = timerFontSize;
            timerText.color = startColor;
        }
    }

    private void StartMovement()
    {
        if (!hasMoved)
        {
            isMoving = true;
            moveStartTime = Time.time;
        }
    }

    private void MoveUIElement()
    {
        if (uiElementToMove != null)
        {
            float timeSinceMoveStarted = Time.time - moveStartTime;
            float t = Mathf.Clamp01(timeSinceMoveStarted / moveDuration);

            float newXPosition = Mathf.Lerp(startingXPosition, endingXPosition, t);
            uiElementToMove.anchoredPosition = new Vector2(newXPosition, uiElementToMove.anchoredPosition.y);

            if (t >= 1.0f)
            {
                isMoving = false;
                hasMoved = true;

                // Trigger the move complete event
                OnMoveComplete?.Invoke();

                StartCoroutine(DelayedMoveBack());
            }
        }
    }

    private IEnumerator DelayedMoveBack()
    {
        yield return new WaitForSeconds(delayBeforeMovingBack);
        StartMovingBack();
    }

    public void StartMovingBack()
    {
        if (hasMoved && !movingBack)
        {
            movingBack = true;
            moveStartTime = Time.time;
        }
    }

    private void MoveBackToStartPosition()
    {
        if (uiElementToMove != null)
        {
            float timeSinceMoveStarted = Time.time - moveStartTime;
            float t = Mathf.Clamp01(timeSinceMoveStarted / moveDuration);

            float newXPosition = Mathf.Lerp(endingXPosition, startingXPosition, t);
            uiElementToMove.anchoredPosition = new Vector2(newXPosition, uiElementToMove.anchoredPosition.y);

            if (t >= 1.0f)
            {
                movingBack = false;
                hasMoved = false;

                // Trigger the move back complete event
                OnMoveBackComplete?.Invoke();
            }
        }
    }

    private void UpdateTimerText(float remainingTime)
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";

            timerText.color = Color.Lerp(endColor, startColor, remainingTime / timeToWait);

            if (remainingTime <= 10f)
            {
                float pulseScale = 1.2f;
                timerText.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * pulseScale, Mathf.PingPong(Time.time, 0.5f));
            }
        }
    }
}
