using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private MoveUIElementAfterTimer uiElementMover;

    private void Start()
    {
        // Start the timer when the game starts
        uiElementMover.StartTimer();
    }

    public void ResetUIElement()
    {
        // Reset the UI element and timer
        uiElementMover.ResetElement();
    }

    public void TriggerMovementImmediately()
    {
        // Trigger the movement immediately without waiting for the timer
        uiElementMover.StartMovement();
    }
}
