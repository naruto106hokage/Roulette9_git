using UnityEngine;
using TMPro;
using System.Collections;

public class MoveUIElementAfterTimer : MonoBehaviour
{
    [SerializeField] private RectTransform uiElementToMove;
    [SerializeField] private TextMeshProUGUI timerText; // Use TextMeshProUGUI for the timer text
    [SerializeField] private float startingXPosition = 0f;
    [SerializeField] private float endingXPosition = 550f;
    [SerializeField] private float timeToWait = 20f; // Time in seconds
    [SerializeField] private float moveDuration = 1f; // Duration of the move
    [SerializeField] private Color startColor = Color.green;
    [SerializeField] private Color endColor = Color.red;
    [SerializeField] private TMP_FontAsset timerFont; // TextMeshPro font asset
    [SerializeField] private int timerFontSize = 40;
    [SerializeField] private float delayBeforeMovingBack = 10f; // Delay before moving back in seconds

    private float elapsedTime = 0f; // Track the elapsed time
    private bool isMoving = false;
    private bool hasMoved = false; // Track if the element has already moved
    private float moveStartTime;
    private bool movingBack = false; // Track if the element is moving back to the start position

    private void Start()
    {
        // Set the UI element to the starting X position at the beginning
        SetStartingPosition();

        // Set up the timer text font and size
        SetupTimerText();

        // Automatically start the timer when the script starts
        StartTimer();
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

    private void StartTimer()
    {
        elapsedTime = 0f;
        hasMoved = false;
        isMoving = false;
        movingBack = false;

        // Update the timer every frame
        StartCoroutine(UpdateTimer());
    }

    private IEnumerator UpdateTimer()
    {
        while (elapsedTime < timeToWait && !hasMoved)
        {
            elapsedTime += Time.deltaTime;
            float remainingTime = Mathf.Max(0, timeToWait - elapsedTime);

            // Update the timer text and color transition
            UpdateTimerText(remainingTime);

            yield return null;
        }

        // After the timer ends, start the movement
        if (!hasMoved)
        {
            StartMovement();
        }
    }

    private void SetStartingPosition()
    {
        if (uiElementToMove != null)
        {
            Vector2 anchoredPosition = uiElementToMove.anchoredPosition;
            uiElementToMove.anchoredPosition = new Vector2(startingXPosition, anchoredPosition.y);
        }
    }

    private void SetupTimerText()
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
            float t = Mathf.Clamp01(timeSinceMoveStarted / moveDuration); // Calculate how far along the movement is

            // Interpolate the position between starting and ending positions based on 't'
            float newXPosition = Mathf.Lerp(startingXPosition, endingXPosition, t);
            Vector2 anchoredPosition = uiElementToMove.anchoredPosition;
            uiElementToMove.anchoredPosition = new Vector2(newXPosition, anchoredPosition.y);

            // Stop moving once the target is reached
            if (t >= 1.0f)
            {
                isMoving = false;
                hasMoved = true; // Mark that the movement has been completed

                // Start moving back after a delay
                StartCoroutine(DelayedMoveBack());
            }
        }
    }

    private IEnumerator DelayedMoveBack()
    {
        // Wait for the specified delay before moving back
        yield return new WaitForSeconds(delayBeforeMovingBack);
        StartMovingBack();
    }

    private void StartMovingBack()
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
            float t = Mathf.Clamp01(timeSinceMoveStarted / moveDuration); // Calculate how far along the movement is

            // Interpolate the position between ending and starting positions based on 't'
            float newXPosition = Mathf.Lerp(endingXPosition, startingXPosition, t);
            Vector2 anchoredPosition = uiElementToMove.anchoredPosition;
            uiElementToMove.anchoredPosition = new Vector2(newXPosition, anchoredPosition.y);

            // Stop moving once the target is reached
            if (t >= 1.0f)
            {
                movingBack = false;
                hasMoved = false; // Reset the movement state
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

            // Update the text color based on the time left
            timerText.color = Color.Lerp(endColor, startColor, remainingTime / timeToWait);

            // Pulse the text as it approaches the end
            if (remainingTime <= 10f)
            {
                float pulseScale = 1.2f;
                timerText.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * pulseScale, Mathf.PingPong(Time.time, 0.5f));
            }
        }
    }

    public void ResetElement()
    {
        // Reset the UI element to the starting position
        SetStartingPosition();

        // Reset the movement and timer variables
        elapsedTime = 0f;
        isMoving = false;
        hasMoved = false;
        movingBack = false;

        // Reset the timer text
        SetupTimerText();

        // Restart the timer
        StartTimer();
    }
}
 