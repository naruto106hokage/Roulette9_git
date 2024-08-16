using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResultCalculator : MonoBehaviour
{
    [SerializeField] private TMP_Text amountWonText;
    private int amountWon = 0;
    [SerializeField] private Dictionary<int, int> winMultiplier = new Dictionary<int, int>();

    public void calculateResult(int numberDrawn, Dictionary<string, int> betPlacedPositions)
    {
        foreach (var betPlacedPosition in betPlacedPositions)
        {
            string[] positions = betPlacedPosition.Key.Split('-');
            List<int> betNumbers = new List<int>();
            foreach (var pos in positions)
            {
                if (int.TryParse(pos, out int betNumber))
                {
                    betNumbers.Add(betNumber);
                }
            }

            if (betNumbers.Contains(numberDrawn))
            {
                Debug.Log("You won");

                // Calculate multiplier based on the number of bet positions
                int multiplier = 36 / betNumbers.Count;
                amountWon += betPlacedPosition.Value * multiplier;
            }
        }

        amountWonText.text = "You won: " + amountWon;
    }

    public void setWinMultiplier()
    {
        winMultiplier.Clear();
        // Populate winMultiplier if necessary, currently not used
    }
}
