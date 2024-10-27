using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
//using System.Runtime.Remoting.Contexts;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Random = UnityEngine.Random;

enum Stage
{
    First,
    Second
}

enum Location
{
    near,
    mid,
    far

}

struct Trial
{
    public float Degrees { get; }
    public Location Location { get; }
    public Vector3 AdjustmentAngle { get; set; }



    public Trial(float degrees, Location location, Vector3 adjustmentAngle = default)
    {
        this.Degrees = degrees;
        this.Location = location;
        this.AdjustmentAngle = Vector3.zero;
    }



    public override string ToString()
    {
        return $"{Degrees}, {Location.ToString()}, {AdjustmentAngle.z}";
    }
}


public class ExperimentScript : MonoBehaviour
{
    [SerializeField] GameObject panel_top;
    [SerializeField] GameObject panel_bottom;


    public GameObject adjustment;
    public GameObject fixation;
    public GameObject fixation2;
    public GameObject aperture;
    public GameObject aperture_sec;
    public GameObject aperture_horizontal;
    public GameObject left_panel;
    public GameObject right_panel;
    public GameObject left_panel_tex;
    public GameObject right_panel_tex;
    public float stereoSeparation;

    public float d;

    public Vector2 Adjust;
    public float ipd;

    public float X2;
    public float probe_depth2;

    public float X3;
    public float probe_depth3;

    public float refz_offset;
    public float comz_offset;

    // ---------------------------
    private const int Step = 1, MaxAngle = 1, MinAngle = 1
         , Repetitions = 20; // Repetition = 20

    private List<Trial> angles;
    private int currentTrial = 0;

    private bool firstinterval = false;
    private bool secondinterval = false;

    private System.IO.StreamWriter file;
    private Stage stage = Stage.First;

    private bool hasStarted = false;

    private bool stop_flash = true;

    [SerializeField]
    private XRNode xrNode = XRNode.RightHand;

    private List<InputDevice> devices = new List<InputDevice>();
    private InputDevice device;

    bool LastStateA = false;
    bool LastStateB = false;

    bool primaryButton = false;
    bool secondaryButton = false;

    private float scrollSpeed = 0.05f;
    float scaleFactor = 0.5f;
    float scaleFactor_debug;
    float angleInDegrees;
    float angleInRadians;
    // Start is called before the first frame update

    void Start()
    {
        adjustment.SetActive(false);

        aperture.SetActive(false);
        fixation.SetActive(false);
        fixation2.SetActive(false);


        //left_panel.SetActive(false);
        //right_panel.SetActive(false);

        this.angles = new List<Trial>();


        for (int i = 0; i < Repetitions; i++)
       {
            for (int j = MinAngle; j <= MaxAngle; j += Step)
            {
                angles.AddRange(Enumerable.Range(0, 3).Select(x => new Trial(j, (Location)x)));
            }
        }

        //Camera cam = GetComponent<Camera>();
        //stereoSeparation = cam.stereoSeparation;

        file = new StreamWriter(Application.persistentDataPath + "/ipd_results.txt", true);

        Shuffle(angles);

        panel_top.transform.localPosition = new Vector3(0f, 2f, 2.1f);
        panel_bottom.transform.localPosition = new Vector3(0f, 2f, 2.1f);
    }

    // Update is called once per frame
    void Update()
    {
        // texture rolling
        float offset = Time.time * scrollSpeed;
        left_panel_tex.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(offset, 0);
        //panel_top.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0, -offset);
        //panel_bottom.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0, offset);
        /*
        // Scale the panel's width
        right_panel_tex.transform.localScale = new Vector3(scaleFactor, right_panel_tex.transform.localScale.y, right_panel_tex.transform.localScale.z);

        // Set the texture offset for scrolling
        right_panel_tex.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(offset, 0);

        // Adjust the texture tiling to keep it from stretching on the scaled panel
        right_panel_tex.GetComponent<Renderer>().material.mainTextureScale = new Vector2(1 / scaleFactor / 4, 1);
        */
        
        //scale_panel(panel_top, -offset);
        //scale_panel(panel_bottom, offset, scaleFactor);

        if (!device.isValid)
        {
            GetDevice();
        }
        
        device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 Adjust);

        float tmp = 0.5f * Adjust.x;
        Debug.Log("==> " + tmp);

        //probe2.transform.Translate(0.0005f * Adjust.x, 0f, 0f);
        left_panel.transform.Rotate(0f, 0f, 0.5f*Adjust.x);
        right_panel.transform.Rotate(0f, 0f, 0.5f*-Adjust.x);

        panel_top.transform.Rotate(0.5f * -Adjust.x, 0f, 0f);
        panel_bottom.transform.Rotate(0.5f * Adjust.x, 0f, 0f);

        angleInDegrees = panel_top.transform.eulerAngles.x;

        // Convert the angle to radians
        angleInRadians = angleInDegrees * Mathf.Deg2Rad;

        //float angleInRadians = Adjust.x; //angleInDegrees * Mathf.Deg2Rad;
        scaleFactor_debug = 1 / Mathf.Cos(angleInRadians);
        scale_panel(panel_top, -offset, scaleFactor_debug);

        scale_panel(panel_bottom, offset, scaleFactor_debug);
        //-----------------------------------------------------------------------------------
        primaryButton = false;
        secondaryButton = false;

        device.TryGetFeatureValue(CommonUsages.primaryButton, out primaryButton);

        if (primaryButton != LastStateA)
        {
            if (primaryButton == true)
            {
                //button was pressed this frame
            }
            else if (primaryButton == false)
            {
                //ButtonPressed();
                firstinterval = true;
                secondinterval = false;
            }
            //Set last known state of button
            LastStateA = primaryButton;
        }

        device.TryGetFeatureValue(CommonUsages.secondaryButton, out secondaryButton);

        if (secondaryButton != LastStateB)
        {
            if (secondaryButton == true)
            {
                //button was pressed this frame
            }
            else if (secondaryButton == false)
            {
                //ButtonPressed();
                firstinterval = false;
                secondinterval = true;
            }
            //Set last known state of button
            LastStateB = secondaryButton;
        }
    }

    void scale_panel(GameObject panel, float offset, float scaleFactor0)
    {
        // Scale the panel's width
        panel.transform.localScale = new Vector3(panel.transform.localScale.x, panel.transform.localScale.y, scaleFactor0);
        //debug_panel.transform.localScale = new Vector3(debug_panel.transform.localScale.x, debug_panel.transform.localScale.y, debug_panel.transform.localScale.z * scaleFactor);

        // Adjust the texture tiling to keep it from stretching on the scaled panel
        panel.GetComponent<Renderer>().material.mainTextureScale = new Vector2(1, scaleFactor0);

        // Set the texture offset for scrolling, with an initial offset to center it
        //panel.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0, 0.5f - offset);
    }

    void GetDevice()
    {
        InputDevices.GetDevicesAtXRNode(xrNode, devices);
        device = devices.FirstOrDefault();
    }

    void OnEnable()
    {
        if (!device.isValid)
        {
            GetDevice();
        }
    }

    public static void Shuffle<T>(IList<T> list)
    {
        var rng = new System.Random();
        var n = list.Count;
        while (n > 1)
        {
            n--;
            var k = rng.Next(n + 1);
            var value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    void ButtonPressed()
    {

        switch (stage)
        {
            case Stage.First:
                //FirstStage();
                //stage = Stage.Second;
                break;

            case Stage.Second:
                //SecondStage();
                stage = Stage.First;
                break;


            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    
    IEnumerator waitTime()
    {

        yield return new WaitForSeconds(1f);


    }

  

    IEnumerator present_panels()
    {

        fixation.SetActive(true);
        fixation2.SetActive(true);

        yield return new WaitForSeconds(1f);



        fixation.SetActive(false);
        fixation2.SetActive(false);
        left_panel.SetActive(true);
        right_panel.SetActive(true);

    }


    

}
