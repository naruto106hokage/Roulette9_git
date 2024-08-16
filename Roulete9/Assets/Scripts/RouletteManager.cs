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

    void Start()
    {
        if (wheelImage != null && ballImage != null && pathPoints.Count > 0)
        {
            // Start the spinning and ball movement coroutines
            StartCoroutine(SpinWheel());
            StartCoroutine(MoveBallAlongPath());
        }
    }

    IEnumerator SpinWheel()
    {
        float elapsedTime = 0f;

        while (elapsedTime < spinDuration)
        {
            // Rotate the wheel
            Vector3 rotation = Vector3.forward * rotationSpeed * Time.deltaTime;
            wheelImage.rectTransform.eulerAngles += rotation;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Stop the wheel rotation (optional)
        rotationSpeed = 0f;
    }

    IEnumerator MoveBallAlongPath()
    {
        // Calculate the duration to move from one point to the next
        float duration = spinDuration / pathPoints.Count;
        int currentPoint = 0;

        while (currentPoint < pathPoints.Count)
        {
            Transform targetPoint = pathPoints[currentPoint];
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                // Move the ball towards the target point
                ballImage.rectTransform.position = Vector3.Lerp(ballImage.rectTransform.position, targetPoint.position, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Snap the ball to the target point at the end of the move
            ballImage.rectTransform.position = targetPoint.position;
            currentPoint++;
        }
    }
}
