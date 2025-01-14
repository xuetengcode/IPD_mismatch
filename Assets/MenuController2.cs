using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuController2 : MonoBehaviour
{
    public Slider participantSlider;
    public Slider ipdSlider;

    public void Awake()
    {
        SharedData.participantValue = Mathf.RoundToInt(participantSlider.value);
        SharedData.ipdValue = Mathf.RoundToInt(ipdSlider.value);
    }

    public void Update()
    {
        SharedData.participantValue = Mathf.RoundToInt(participantSlider.value);
        SharedData.ipdValue = Mathf.RoundToInt(ipdSlider.value);        
    }

    public void StaircaseButton()
    {
        SceneManager.LoadScene("FixationSceneStaircase");
    }

    public void AdjustmentButton()
    {
        SceneManager.LoadScene("MethodOfAdjustment");
    }

    public void AdjustBigPrism()
    {
        SceneManager.LoadScene("MoA_Big");        
    }

    public void AdjustSmallPrism()
    {
        SceneManager.LoadScene("MoA_Small");              
    }

}
