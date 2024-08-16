using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [SerializeField] private GameObject[] boardMarker;

    public void disableAllMarker()
    {
        foreach (GameObject obj in boardMarker)
        {
            obj.SetActive(false);
        }
    }
    public void setBoardMarker(int number)
    {
        foreach (GameObject obj in boardMarker)
        {
            if (obj.name == number.ToString())
            {
                obj.SetActive(true);
            }
        }
    }
}
