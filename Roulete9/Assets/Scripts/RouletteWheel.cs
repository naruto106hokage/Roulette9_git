using UnityEngine;
using System.Collections;

public class RouletteWheel : MonoBehaviour
{
    public Transform wheel; // The wheel to rotate
    public Transform ball;  // The ball to move
    public float wheelRotationSpeed = 100f; // Rotation speed of the wheel
    public float ballSpeed = 5f;            // Speed of the ball movement
    public float stopAfterSeconds = 5f;     // Time to stop rotation
    public int targetPosition = 0;          // Target position index (0-36)
    public Transform[] ballPositions;       // Array of Transforms for ball positions

    private bool isRotating = true;
    private float elapsedTime = 0f;
    private bool isMovingBall = false;
    private Vector3 startPosition;
    private Vector3 endPosition;
    private float moveStartTime;

    void Update()
    {
        if (isRotating)
        {
            // Rotate the wheel clockwise
            wheel.Rotate(Vector3.back * wheelRotationSpeed * Time.deltaTime);

            // Increment elapsed time
            elapsedTime += Time.deltaTime;

            // Stop rotation after the specified time
            if (elapsedTime >= stopAfterSeconds)
            {
                isRotating = false;
                isMovingBall = true;

                // Set the start and end positions for the ball movement
                startPosition = ball.position;
                endPosition = ballPositions[targetPosition].position;
                moveStartTime = Time.time;

                Debug.Log("Start moving ball to position: " + targetPosition);
            }
        }

        if (isMovingBall)
        {
            MoveBall();
        }
    }

    void MoveBall()
    {
        // Calculate how far along the ball is in its movement
        float elapsedMoveTime = Time.time - moveStartTime;
        float journeyLength = Vector3.Distance(startPosition, endPosition);
        float fractionOfJourney = elapsedMoveTime * ballSpeed / journeyLength;

        // Move the ball
        ball.position = Vector3.Lerp(startPosition, endPosition, fractionOfJourney);

        // Check if the ball has reached the end position
        if (fractionOfJourney >= 1f)
        {
            isMovingBall = false;
            Debug.Log("Ball reached position: " + targetPosition);
        }
    }
}
