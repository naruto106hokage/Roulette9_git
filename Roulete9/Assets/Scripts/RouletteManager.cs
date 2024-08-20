using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RouletteManager : MonoBehaviour
{
    public Image wheelImage; // The wheel image to spin
    public Image ballImage; // The ball image to move along the path
    public List<Transform> pathPoints; // List of empty game objects defining the path
    public float rotationSpeed = 100f; // Speed of the wheel's rotation in degrees per second
    public float spinDuration = 5f; // Time in seconds for how long the wheel will spin
    public float ballMoveSpeed = 5f; // Speed at which the ball moves along the path

    private Transform targetPoint; // The point where the ball will stop
    private Vector3 initialBallPosition; // Initial ball position
    private int totalSpins = 3; // Number of spins

    void Awake()
    {
        // Set the ball's position to zero on Awake
        if (ballImage != null)
        {
            initialBallPosition = ballImage.rectTransform.position;
            ballImage.rectTransform.position = Vector3.zero;
        }
    }

    public void spinTheWheel(int drawnNumber)
    {
        if (wheelImage != null && ballImage != null && pathPoints.Count > 0)
        {
            // Find the target point based on the drawn number
            targetPoint = pathPoints[drawnNumber % pathPoints.Count]; // Ensure index is within bounds

            // Start the spinning and ball movement coroutines
            StartCoroutine(SpinWheel());
            StartCoroutine(MoveBallAlongPath());
        }
    }

    private IEnumerator SpinWheel()
    {
        float elapsedTime = 0f;
        float totalRotation = 360f * totalSpins; // Total rotation angle

        while (elapsedTime < spinDuration)
        {
            // Rotate the wheel
            float rotationAmount = rotationSpeed * Time.deltaTime;
            wheelImage.rectTransform.Rotate(Vector3.forward, rotationAmount);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the wheel stops rotating exactly at the end of the spin duration
        wheelImage.rectTransform.eulerAngles = new Vector3(0, 0, wheelImage.rectTransform.eulerAngles.z + totalRotation);

        // Optional: Reset rotationSpeed to 0 if needed
        rotationSpeed = 0f;
    }

    private IEnumerator MoveBallAlongPath()
    {
        for (int spin = 0; spin < totalSpins; spin++)
        {
            float currentSpeed = ballMoveSpeed * (1f - (0.3f * spin)); // Decrease speed with each spin
            int currentPoint = 0;

            while (currentPoint < pathPoints.Count)
            {
                Transform currentPathPoint = pathPoints[currentPoint];
                float elapsedTime = 0f;
                float duration = spinDuration / pathPoints.Count / currentSpeed;

                while (elapsedTime < duration)
                {
                    // Move the ball towards the current point in a linear motion
                    ballImage.rectTransform.position = Vector3.Lerp(initialBallPosition, currentPathPoint.position, elapsedTime / duration);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                // Snap the ball to the current point at the end of the move
                ballImage.rectTransform.position = currentPathPoint.position;
                initialBallPosition = currentPathPoint.position;

                // On the last spin, check if the current point's name matches the target point's name
                if (spin == totalSpins - 1 && currentPathPoint == targetPoint)
                {
                    // Attach the ball to the target point
                    ballImage.transform.SetParent(currentPathPoint);
                    break; // Stop the movement as the ball has reached the target
                }

                currentPoint++;
            }
        }
    }
}
