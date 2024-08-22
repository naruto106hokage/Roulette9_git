using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public void changeToRecharge()
    {
        SceneManager.LoadScene("Recharge");
    }
    public void backToMain()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
