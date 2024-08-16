using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultWheelHandler : MonoBehaviour
{
    [SerializeField] private GameObject[] winWheelPos;
    [SerializeField] private float countdownTime = 10f;
    [SerializeField] private float rotationSpeed = 150f;
    [SerializeField] private GameObject wheel;
    private Transform wheelTransform;
    [SerializeField] private GameObject wheelPrefab;

    public void disableAll()
    {
        wheelPrefab.SetActive(true);
        foreach (GameObject obj in winWheelPos)
        {
            obj.SetActive(false);
        }
    }

    public IEnumerator rotateTheWheel()
    {
        wheelPrefab.SetActive(true);
        wheel.SetActive(true);
        wheelTransform = wheel.GetComponent<Transform>();
        float timeElapsed = 0f;

        while (timeElapsed < countdownTime)
        {
            wheelTransform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
            timeElapsed += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        //wheel.SetActive(false);
    }

    public void invokeResultWheel(float time)
    {
        Invoke("disableResultWheel", time);
    }

    public void disableResultWheel()
    {
        wheelPrefab.SetActive(false);
    }

    public void displayResult(int result)
    {
        foreach (GameObject obj in winWheelPos)
        {
            if (obj.name == result.ToString())
            {
                obj.SetActive(true);
            }
        }
    }
}
