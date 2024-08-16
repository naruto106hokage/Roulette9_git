using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RandomNumberGenerator : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI resultText;
    public Transform rotatingImage;
    [SerializeField] private GameObject timer;
    [SerializeField] private Image timerImage;
    [SerializeField] private Color defaultColor = Color.green; // Default color of the timer image
    [SerializeField] private Color warningColor = Color.red;   // Color when time is low
    [SerializeField] private float warningThreshold = 0.2f;    // Threshold as a percentage of the initial time

    public IEnumerator StartCountdown(float repeatInterval)
    {
        timer.SetActive(true);
        timerText.gameObject.SetActive(true);

        float initialTime = repeatInterval;
        timerImage.color = defaultColor;

        while (repeatInterval > 0)
        {
            timerText.text = "Time Left: " + repeatInterval.ToString("F0"); 

            float timeRatio = repeatInterval / initialTime;
            timerImage.fillAmount = timeRatio; 

          
            if (timeRatio <= warningThreshold)
            {
                timerImage.color = warningColor;
            }

            yield return new WaitForSeconds(1f);
            repeatInterval--;
        }

        timerText.gameObject.SetActive(false);
        timerImage.fillAmount = 0f; 
    }

    public void disableResultNumber()
    {
        resultText.gameObject.SetActive(false);
    }
    public void display(int number)
    {
        resultText.gameObject.SetActive(true);
        resultText.text = number.ToString();
    }
}
