using UnityEngine;
using System.Collections;

public class RouletteWheel : MonoBehaviour
{
    public Transform wheel; // The wheel to rotate
    public Transform ball;  // The ball to move
    public float wheelRotationSpeed = 100f; // Rotation speed of the wheel
    public float ballRotationSpeed = 150f;  // Rotation speed of the ball
    public float stopAfterSeconds = 5f;     // Time to stop rotation
    public int targetPosition = 0;          // Target position index (0-36)
    public Transform[] ballPositions;       // Array of Transforms for ball positions

    private bool isRotating = true;
    private float elapsedTime = 0f;

    void Update()
    {
        if (isRotating)
        {
            // Rotate the wheel clockwise
            wheel.Rotate(Vector3.back * wheelRotationSpeed * Time.deltaTime);

            // Rotate the ball counter-clockwise
            ball.Rotate(Vector3.forward * ballRotationSpeed * Time.deltaTime);

            // Increment elapsed time
            elapsedTime += Time.deltaTime;

            // Stop rotation after the specified time
            if (elapsedTime >= stopAfterSeconds)
            {
                isRotating = false;
                StartCoroutine(MoveBallToPosition());
            }
        }
    }

    IEnumerator MoveBallToPosition()
    {
        yield return new WaitForSeconds(1f); // A short delay before placing the ball

        // Move the ball to the target position
        ball.position = ballPositions[targetPosition].position;

        Debug.Log("Ball placed at position: " + targetPosition);
    }
}
