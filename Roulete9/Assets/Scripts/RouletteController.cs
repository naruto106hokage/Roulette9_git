using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RouletteController : MonoBehaviour
{
    public Rigidbody2D wheel; // Reference to the roulette wheel's Rigidbody2D
    public Transform ball; // Reference to the ball
    public List<Transform> positions; // List of empty game objects' positions
    public float initialWheelImpulse = 1000f; // Initial impulse to the wheel
    public float initialBallSpeed = 5f; // Initial speed of the ball
    public float deceleration = 0.9f; // Deceleration factor

    private int targetPositionIndex;
    private float currentBallSpeed;

    void Start()
    {
        currentBallSpeed = initialBallSpeed;
        targetPositionIndex = Random.Range(0, positions.Count); // Set the predefined position
        wheel.AddTorque(initialWheelImpulse); // Add initial impulse to the wheel
        StartCoroutine(TraverseBall());
    }

    IEnumerator TraverseBall()
    {
        int ballRounds = 3 * positions.Count; // Ball should move 3 times across all positions

        while (ballRounds > 0)
        {
            for (int i = 0; i < positions.Count; i++)
            {
                ball.position = Vector3.MoveTowards(ball.position, positions[i].position, currentBallSpeed * Time.deltaTime);
                yield return new WaitForSeconds(0.1f); // Adjust the wait time as needed
            }

            currentBallSpeed *= deceleration;
            ballRounds--;
        }

        // Snap ball to the predefined position
        ball.position = positions[targetPositionIndex].position;
    }
}
