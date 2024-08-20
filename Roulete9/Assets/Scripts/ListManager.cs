using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ListManager : MonoBehaviour
{
    [SerializeField] private TMP_Text[] drawnNumberList;
    [SerializeField] private TMP_Text winningNumber;

    public void setNumberToList(List<int> randomNumbers)
    {
        for (int i = 0; i < drawnNumberList.Length; i++)
        {
            if (i < randomNumbers.Count)
            {
                int number = randomNumbers[i];
                drawnNumberList[i].text = number.ToString();

                if (number == 0)
                {
                    drawnNumberList[i].alignment = TextAlignmentOptions.Top;
                    drawnNumberList[i].color = Color.green;
                }
                else if(number == 1 || number == 3 || number == 5 || number == 7 || number == 9 || number == 12 || number == 14 || number == 16 || number == 18 || number == 19 || number == 21 || number == 23 || number == 25 || number == 27 || number == 30 || number == 32 || number == 34 || number == 36)
                {
                    drawnNumberList[i].alignment = TextAlignmentOptions.Right;
                    drawnNumberList[i].color = Color.red; 
                }
                else
                {
                    drawnNumberList[i].alignment = TextAlignmentOptions.Left; 
                    drawnNumberList[i].color = Color.yellow;
                }
            }
            else
            {
                drawnNumberList[i].text = ""; // Clear any extra UI elements
            }
        }
    }
    public void disableOrUnable(bool state)
    {
        winningNumber.gameObject.SetActive(state);
    }

    public void displayWinningNumber(int number)
    {
        winningNumber.text= number.ToString();

    }
}
