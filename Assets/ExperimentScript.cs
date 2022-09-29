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
    private const int Step = 1, MaxAngle = 1, MinAngle = 1
         , Repetitions = 20; // Repetition = 20

    private List<Trial> angles;
    private int currentTrial = 0;

   
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

    // Start is called before the first frame update

    

    void Start()
    {

        
        adjustment.SetActive(false);

        aperture.SetActive(false);
        fixation.SetActive(false);
        fixation2.SetActive(false);


        left_panel.SetActive(false);
        right_panel.SetActive(false);

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
        

    }

    // Update is called once per frame
    void Update()
    {

        

        if (!device.isValid)
        {
            GetDevice();
        }

        bool primaryButton = false;

        bool secondaryButton = false;


        device.TryGetFeatureValue(CommonUsages.primaryButton, out primaryButton);

        if (primaryButton != LastStateA)
        {
            if (primaryButton == true)
            {
                //button was pressed this frame
            }
            else if (primaryButton == false)
            {
                ButtonPressed();
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
                ButtonPressed();
                firstinterval = false;
                secondinterval = true;
            }
            //Set last known state of button
            LastStateB = secondaryButton;
        }

        device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 Adjust);
        //probe2.transform.Translate(0.0005f * Adjust.x, 0f, 0f);
        left_panel.transform.Rotate(0f, 0f, 0.5f*Adjust.x);
        right_panel.transform.Rotate(0f, 0f, 0.5f*-Adjust.x);


    }

    void ButtonPressed()
    {

        switch (stage)
        {
            case Stage.First:
                FirstStage();
                //stage = Stage.Second;
                break;

            case Stage.Second:
                SecondStage();
                stage = Stage.First;
                break;


            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void FirstStage()
    {

        //show fixation dot, all other discs invisible.
        adjustment.SetActive(false);
        fixation.SetActive(false);
        fixation2.SetActive(false);
        aperture.SetActive(false);
        aperture_sec.SetActive(false);


        left_panel.SetActive(false);
        right_panel.SetActive(false);

        //random depth offset of surfaces
        var ref_z_offset = Random.Range(20f, 70f);
        var com_z_offset = Random.Range(20f, 70f);

        //var scale = Random.Range(0.5f, 1f);


        refz_offset = ref_z_offset; //ref_z_offset
        comz_offset = com_z_offset; //ref_z_offset


        var ref_offset = ref_z_offset.ToString(); //ref_z_offset.ToString()
        var com_offset = left_panel.transform.localEulerAngles.y.ToString(); //com_z_offset.ToString()



        if (hasStarted)
        {
            var angle = this.angles[currentTrial++];

            angle.AdjustmentAngle = left_panel.transform.localPosition; 

            this.file.WriteLine(angle + ", " + com_offset + ", " + ref_offset + "\n"); // this.file.WriteLine(angle + "\n");
        }



        if (currentTrial >= angles.Count)
        {
            file.WriteLine("\n");
            file.Close();
            Application.Quit();
        }
        //randomize initial angle
        left_panel.transform.localEulerAngles = new Vector3(90f, ref_z_offset, 0f);
        right_panel.transform.localEulerAngles = new Vector3(90f, -ref_z_offset, 0f);

        SecondStage();


    }



    void SecondStage()
    {
        firstinterval = false;
        secondinterval = false;

        var angle = angles[currentTrial];


        //randomize texture offset


        left_panel_tex.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(Random.Range(0f, 0.1f), Random.Range(0f, 0.1f));
        right_panel_tex.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(Random.Range(0f, 0.1f), Random.Range(0f, 0.1f));




        switch (angle.Location)
        {

            //case Location.near:

            //    StartCoroutine(present_near());
            //    break;

            case Location.mid:
                fixation.transform.localPosition = new Vector3(0f, 0f, 2f);
                fixation.transform.localScale = new Vector3(0.06f, 0.011f, 0.001f);
                fixation2.transform.localPosition = new Vector3(0f, 0f, 2f);
                fixation2.transform.localScale = new Vector3(0.011f, 0.06f, 0.001f);
                left_panel.transform.localPosition = new Vector3(0f, 0f, 2f);
                right_panel.transform.localPosition = new Vector3(0f, 0f, 2f);

                //left_panel.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                //right_panel.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

                var scale = 0.5f; /*Random.Range(0.5f, 0.8f);*/

                left_panel.transform.localScale = new Vector3(scale, 0.5f, scale); //2f 
                right_panel.transform.localScale = new Vector3(scale, 0.5f, scale); //2f

               // left_panel_tex.GetComponent<Renderer>().material.mainTextureScale = new Vector2(1f, 1f);
               //right_panel_tex.GetComponent<Renderer>().material.mainTextureScale = new Vector2(1f, 1f);

                StartCoroutine(present_panels());
                break;

            case Location.near:
                fixation.transform.localPosition = new Vector3(0f, 0f, 1.5f);
                fixation.transform.localScale = new Vector3(0.06f * 0.75f, 0.011f * 0.75f, 0.001f);
                fixation2.transform.localPosition = new Vector3(0f, 0f, 1.5f);
                fixation2.transform.localScale = new Vector3(0.011f * 0.75f, 0.06f * 0.75f, 0.001f);
                left_panel.transform.localPosition = new Vector3(0f, 0f, 1.5f);
                right_panel.transform.localPosition = new Vector3(0f, 0f, 1.5f);

                //left_panel.transform.localScale = new Vector3(0.5f*0.5f, 0.5f*0.5f, 0.5f);
                //right_panel.transform.localScale = new Vector3(0.5f*0.5f, 0.5f*0.5f, 0.5f);

                var scale2 = 0.5f; /*Random.Range(0.55f, 0.8f);*/

                left_panel.transform.localScale = new Vector3(scale2 * 0.75f, 0.5f, scale2 * 0.75f); //2f *0.5
                right_panel.transform.localScale = new Vector3(scale2* 0.75f, 0.5f, scale2 * 0.75f); //2f *0.5
                //left_panel_tex.GetComponent<Renderer>().material.mainTextureScale = new Vector2(1f, 1f);
                //right_panel_tex.GetComponent<Renderer>().material.mainTextureScale = new Vector2(1f, 1f);

                StartCoroutine(present_panels());
                break;

            case Location.far:
                fixation.transform.localPosition = new Vector3(0f, 0f, 2.5f);
                fixation.transform.localScale = new Vector3(0.06f*1.25f, 0.011f * 1.25f, 0.001f);
                fixation2.transform.localPosition = new Vector3(0f, 0f, 2.5f);
                fixation2.transform.localScale = new Vector3(0.011f * 1.25f, 0.06f * 1.25f, 0.001f);
                left_panel.transform.localPosition = new Vector3(0f, 0f, 2.5f);
                right_panel.transform.localPosition = new Vector3(0f, 0f, 2.5f);
                //StartCoroutine(present_mid());

                //left_panel.transform.localScale = new Vector3(0.5f * 1.5f, 0.5f * 1.5f, 0.5f);
                //right_panel.transform.localScale = new Vector3(0.5f * 1.5f, 0.5f * 1.5f, 0.5f);

                var scale3 = 0.5f; /*Random.Range(0.5f, 0.8f);*/

                left_panel.transform.localScale = new Vector3(scale3 * 1.25f, 0.5f, scale3 * 1.25f);
                right_panel.transform.localScale = new Vector3(scale3 * 1.25f, 0.5f, scale3 * 1.25f);
                //left_panel_tex.GetComponent<Renderer>().material.mainTextureScale = new Vector2(1f, 1f);
                //right_panel_tex.GetComponent<Renderer>().material.mainTextureScale = new Vector2(1f, 1f);


                StartCoroutine(present_panels());
                break;



        }




        adjustment.SetActive(false);

        hasStarted = true;


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
