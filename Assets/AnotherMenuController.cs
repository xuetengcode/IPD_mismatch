using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AnotherMenuController : MonoBehaviour
{
    public void StartBlockBtn()
    {
        SceneManager.LoadScene("Staircase_Big");
    }

}
