using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void AdjustmentButton()
    {
        SceneManager.LoadScene("RevisedScene");
    }

    public void StaircaseButton()
    {
        SceneManager.LoadScene("StaircaseScene");
    }
}
