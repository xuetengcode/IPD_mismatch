using UnityEngine;
using UnityEngine.XR;
using System.IO;
using System.Collections.Generic;


public class StartStaircase : MonoBehaviour
{
    public GameObject triPrism; 
    public float triPrismX; 
    public float triPrismY;

    public float triPrismZ; 

    public int seed;
    
    public float triPrismRotateCCW;

    public Material[] triPrismMaterials;
    public Vector2 triPrismScale = new Vector2(1.0f, 3.0f);

    public Vector3 cornerBottomLeft; // Bottom-left corner
    public Vector3 cornerTopRight; // Top-right corner

    public float wallSparseness; 

    public int n_rows;
    //public int n_cols;

    public Material shelfMaterial;

    public float distanceToTriPrism = 0.5f;

    public float otherObjectsZInset = -0.3f;

    public Material[] bookMaterials; 

    public GameObject[] bookMeshes;

    private float initialTriPrismScaleZ;

    private float finalTriPrismScaleZ; 

    private string filePath; // For recording response data
    private string headTrackingDataFilePath; // For recording head-tracking data 
    // For method of adjustment

    private Vector3 triPrismScaleChange = new Vector3(0.0f, 0.0f, 0.115f);

    public XRNode controllerNode;

    private List<GameObject> instantiatedBooks = new List<GameObject>();
    private List<GameObject> instantiatedShelves = new List<GameObject>();

    private bool isButtonPressed = false ; 

    private int seedRecorded; 

    private float triangleHeight;

    private bool? S1IsScalingUp;
    private bool? S2IsScalingUp;
    private bool S1Active = true; // If false, S2 is Active
    private float TriPrismScaleZ ; 
    private float S1TriPrismScaleZ;
    private float S2TriPrismScaleZ;
    private int staircaseNum;
    private string S1Response;
    private int S1Reversals = 0;

    private string S2Response;
    private int S2Reversals = 0;

    private float trialStartTime ; 
    private float trialResponseTime ; 

    private string response;
    private List<float> reversalHeightList;

    private string didReversalHappen;

    private string csvReversal;

    private int maxReversals = 12;


    void Awake()
    {
        
        InstantiateBooksPrism();
    }

    void Update()
    {
        //Vector3 position = new Vector3(triPrismX, triPrismY, triPrismZ);
        //triPrism.transform.position = position;
        //triPrism.transform.localScale = triPrismScale; 

        if (!Application.isEditor)
        { 
            InputDevice device = InputDevices.GetDeviceAtXRNode(controllerNode);

            if (device.TryGetFeatureValue(CommonUsages.secondaryButton, out bool secondaryButtonValue) && secondaryButtonValue && !isButtonPressed)
            {
                response = "H";

                if (S1Active)
                {
                    S1IsScalingUp = false;
                    S1ReversalTracker();
                    SaveTriPrismScale();

                    if (S2Reversals < maxReversals)
                    {
                        float staircaseRandom = Random.Range(0f, 1f);
                        if (staircaseRandom <= 0.5f || S1Reversals == maxReversals)
                        {
                            S1Active = false; 
                        }
                        else
                        {
                            Debug.Log("S2 Randomly not activated.");
                            S1Active = true;
                        }
                    }
                    else
                    {
                        Debug.Log("Maximum Reversals reached for Staircase 2. Will finish Staircase 1.");
                        CheckS1MaxReversal();
                        
                    }
                    
                    
                }

                else
                {
                    S2IsScalingUp = false;   
                    S2ReversalTracker();
                    SaveTriPrismScale();  
                    
                    if (S1Reversals < maxReversals)
                    {
                        float staircaseRandom = Random.Range(0f, 1f);
                        if (staircaseRandom <= 0.5f || S2Reversals == maxReversals)
                        {
                            S1Active = true;
                        }
                        else
                        {
                            Debug.Log("S1 Randomly not activated.");
                            S1Active = false;
                        }
                        
                    }
                    else
                    {
                        Debug.Log("Maximum Reversals reached for Staircase 1. Will finish Staircase 2.");
                        CheckS2MaxReversal();
                        
                    }      
                         
                }

                InstantiateBooksPrism();
                isButtonPressed = true ;
                Invoke("ResetButtonState", 1f);
                
            }

            if (device.TryGetFeatureValue(CommonUsages.primaryButton, out bool buttonValue) && buttonValue && !isButtonPressed)
            {
                response="B";

                if (S1Active)
                {
                    S1IsScalingUp = true;
                    S1ReversalTracker();
                    SaveTriPrismScale();

                    if (S2Reversals < maxReversals)
                    {
                        float staircaseRandom = Random.Range(0f, 1f);
                        if (staircaseRandom <= 0.5f || S1Reversals == maxReversals)
                        {
                            S1Active = false; 
                        }
                        else
                        {
                            Debug.Log("S2 Randomly not activated.");
                            S1Active = true;
                        }
                    }
                    else
                    {
                        Debug.Log("Maximum Reversals reached for Staircase 2. Will finish Staircase 1.");
                        CheckS1MaxReversal();
                        
                    }

                    
                }

                else
                {
                    S2IsScalingUp = true;
                    S2ReversalTracker();     
                    SaveTriPrismScale();  

                    if (S1Reversals < maxReversals)
                    {
                        float staircaseRandom = Random.Range(0f, 1f);
                        if (staircaseRandom <= 0.5f || S2Reversals == maxReversals)
                        {
                            S1Active = true;
                        }
                        else
                        {
                            Debug.Log("S1 Randomly not activated.");
                            S1Active = false;

                        }
                        
                    }
                    else
                    {
                        Debug.Log("Maximum Reversals reached for Staircase 1. Will finish Staircase 2.");
                        CheckS2MaxReversal();
                        
                    }     
                            
                }
                isButtonPressed = true ;
                InstantiateBooksPrism();
                
                Invoke("ResetButtonState", 1f);

            }

            // }   

            if (device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position) && device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation))
            {
                if (!File.Exists(headTrackingDataFilePath))
                {
                    File.WriteAllText(headTrackingDataFilePath, "Timestamp, PositionX, PositionY, PositionZ, RotationX, RotationY, RotationZ, RotationW\n");
                }

                string headTrackingCsvEntry = $"{System.DateTime.Now},{position.x},{position.y},{position.z},{rotation.x},{rotation.y},{rotation.z},{rotation.w}\n";
                File.AppendAllText(headTrackingDataFilePath, headTrackingCsvEntry);
            }


        }
        
        // When not using VR (for testing)
        else
        {

            if (Input.GetKeyDown(KeyCode.H) )
            {
                response = "H";

                if (S1Active)
                {
                    S1IsScalingUp = false;
                    S1ReversalTracker();
                    SaveTriPrismScale();

                    if (S2Reversals < maxReversals)
                    {
                        float staircaseRandom = Random.Range(0f, 1f);
                        if (staircaseRandom <= 0.5f)
                        {
                            S1Active = false; 
                        }
                        else
                        {
                            Debug.Log("S2 Randomly not activated.");
                            S1Active = true;
                        }
                    }
                    else
                    {
                        Debug.Log("Maximum Reversals reached for Staircase 2. Will finish Staircase 1.");
                        if (S1Reversals < maxReversals)
                        {
                            S1Active = true;
                        }
                        else
                        {
                            Application.Quit();
                        }
                    }
                    
                    
                }

                else
                {
                    S2IsScalingUp = false;   
                    S2ReversalTracker();
                    SaveTriPrismScale();  
                    
                    if (S1Reversals < maxReversals)
                    {
                        float staircaseRandom = Random.Range(0f, 1f);
                        if (staircaseRandom <= 0.5f)
                        {
                            S1Active = true;
                        }
                        else
                        {
                            Debug.Log("S1 Randomly not activated.");
                            S1Active = false;
                        }
                        
                    }
                    else
                    {
                        Debug.Log("Maximum Reversals reached for Staircase 1. Will finish Staircase 2.");
                        if (S2Reversals < maxReversals)
                        {
                            S1Active = false;
                        }
                        else
                        {
                            Application.Quit();
                        }
                    }      
                         
                }

                
                InstantiateBooksPrism();
                
        
                
            }

            if (Input.GetKeyDown(KeyCode.B) )
            {
                response="B";

                if (S1Active)
                {
                    S1IsScalingUp = true;
                    S1ReversalTracker();
                    SaveTriPrismScale();
                    
                    if (S2Reversals < maxReversals)
                    {
                        float staircaseRandom = Random.Range(0f, 1f);
                        if (staircaseRandom <= 0.5f)
                        {
                            S1Active = false; 
                        }
                        else
                        {
                            Debug.Log("S2 Randomly not activated.");
                            S1Active = true;
                        }
                    }
                    else
                    {
                        Debug.Log("Maximum Reversals reached for Staircase 2. Will finish Staircase 1.");
                        if (S1Reversals < maxReversals)
                        {
                            S1Active = true;
                        }
                        else
                        {
                            Application.Quit();
                        }
                    }

                    
                }

                else
                {
                    S2IsScalingUp = true;
                    S2ReversalTracker();     
                    SaveTriPrismScale();  

                    if (S1Reversals < maxReversals)
                    {
                        float staircaseRandom = Random.Range(0f, 1f);
                        if (staircaseRandom <= 0.5f)
                        {
                            S1Active = true;
                        }
                        else
                        {
                            Debug.Log("S1 Randomly not activated.");
                            S1Active = false;
                        }
                        
                    }
                    else
                    {
                        Debug.Log("Maximum Reversals reached for Staircase 1. Will finish Staircase 2.");
                        if (S2Reversals < maxReversals)
                        {
                            S1Active = false;
                        }
                        else
                        {
                            Application.Quit();
                        }
                    }     
                            
                }

                
                InstantiateBooksPrism();
                
            }
        }

    }

    private void CheckS1MaxReversal()
    {
        if (S1Reversals < maxReversals)
        {
            S1Active = true;
        }
        else
        {
            Application.Quit();
        }    
    }

    private void CheckS2MaxReversal()
        {
        if (S2Reversals < maxReversals)
        {
            S1Active = false;
        }
        else
        {
            Application.Quit();
        }    
    }

    private void S1ReversalTracker()
    {
        if (S1Response != null)
        {
            if (S1Response != response)
            {
                S1Reversals ++;
                //instantiatedShelves.Add(shelfLedge);
                //reversalHeightList.Add(triangleHeight);
                didReversalHappen = "Yes";
            }
            else
            {
                didReversalHappen = "No";
            }
        }

        Debug.Log($"S1 Number of Reversals: {S1Reversals}");
        S1Response = response;

        // if (S1Reversals >= maxReversals && S2Reversals >= maxReversals)
        // {
        //     Debug.Log("Maximum Reversals reached for both Staircase");

        //     if (Application.isEditor)
        //     {
                
        //         // UnityEditor.EditorApplication.isPlaying = false; 
        //     }
        //     else
        //     {
        //         Application.Quit();
        //     }
        // }

        if (S1Reversals >= 1 && S1Reversals < 3)
        {
            triPrismScaleChange = new Vector3(0f, 0f, 0.0575f);
        }
        else if (S1Reversals >= 3)
        {
            triPrismScaleChange = new Vector3(0f, 0f, 0.02875f);
        }



        
    }

    private void S2ReversalTracker()
    {
        if (S2Response != null)
        {
            if (S2Response != response)
            {
                S2Reversals ++;
                //instantiatedShelves.Add(shelfLedge);
                //reversalHeightList.Add(triangleHeight);
                didReversalHappen = "Yes";
            }
            else
            {
                didReversalHappen = "No";
            }
        }

        S2Response = response;
        Debug.Log($"S2 Number of Reversals: {S2Reversals}");

        // if (S1Reversals >= maxReversals && S2Reversals >= maxReversals)
        // {
        //     Debug.Log("Maximum Reversals reached for both Staircase");
        //     if (Application.isEditor)
        //     {
                
        //         // UnityEditor.EditorApplication.isPlaying = false; 
        //     }
        //     else
        //     {
        //         Application.Quit();
        //     }
        // }

        if (S2Reversals >= 1 && S2Reversals < 3)
        {
            triPrismScaleChange = new Vector3(0f, 0f, 0.125f);
        }
        else if (S2Reversals >= 3)
        {
            triPrismScaleChange = new Vector3(0f, 0f, 0.0625f);
        }



    }

    private void ResetButtonState()
    {
        isButtonPressed = false ; 
    }
    
    private void InstantiateBooksPrism()
    {
        
        int ipdValueFp = SharedData.ipdValue;
        int participantValueFp = SharedData.participantValue;
        trialStartTime = Time.time ; 

        if (seed != 0)
        {
            Random.InitState(seed);
            seedRecorded = seed;
        }

        else
        {
            int dynamicSeed = (int)(System.DateTime.Now.Ticks % 10000);
            Random.InitState(dynamicSeed);
            Debug.Log($"Seed: {dynamicSeed}");
            seedRecorded = dynamicSeed; 
        }

        if (!Application.isEditor && IsHeadsetConnected())
        {
            string participantFileName = $"IPD-Staircase-fromQuest-P{participantValueFp}-IPD{ipdValueFp}.csv";
            filePath = Path.Combine("/sdcard/Download", participantFileName);
            string headTrackingFileName = $"HeadTrackingData-fromQuest-P{participantValueFp}-IPD{ipdValueFp}.csv";
            headTrackingDataFilePath = Path.Combine("/sdcard/Download", headTrackingFileName);
        }
        
        else
        {
            filePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "IPD-Staircase.csv");
        }

        // Generating the prism 
        if (triPrism != null)
        {
            Destroy(triPrism);
        }

        float jitterTriPrismY = Random.Range(-0.01f, 0.01f);
        float jitterTriPrismZ = Random.Range(-0.01f, 0.01f);
        Vector3 position = new Vector3(triPrismX, triPrismY + jitterTriPrismY, triPrismZ);
        triPrism = Instantiate(triPrism, position, Quaternion.Euler(0, 0, triPrismRotateCCW));



        Renderer objectRenderer = triPrism.GetComponent<Renderer>();
        // if (objectRenderer != null && triPrismMaterial != null)
        // {
        //     objectRenderer.material = triPrismMaterial;
        // }

        if (objectRenderer != null && triPrismMaterials.Length > 0)
        {
            Material triPrismMaterial = triPrismMaterials[Random.Range(0, triPrismMaterials.Length)];
            objectRenderer.material = triPrismMaterial;
        }

        if (S1IsScalingUp == null)
        {

            float startUpDownRandom = Random.Range(0f, 1f);
            Debug.Log($"StartUpDown: {startUpDownRandom}");
            
            if (startUpDownRandom <= 0.5)
            {
                S1TriPrismScaleZ = Random.Range(0.17f, 0.285f); // 16-20cm
                S2TriPrismScaleZ = Random.Range(0.8525f, 0.9675f); // 52-56cm
            } 
            else
            {
                S1TriPrismScaleZ = Random.Range(0.8525f, 0.9675f);
                S2TriPrismScaleZ = Random.Range(0.17f, 0.285f); 
            }

            TriPrismScaleZ = S1TriPrismScaleZ; 

        }

        if (S2IsScalingUp == null && S1Active == false)
        {
            TriPrismScaleZ = S2TriPrismScaleZ; 
        }

        if (S1Active == true && S1IsScalingUp == true)
        {
            if (S1TriPrismScaleZ + triPrismScaleChange.z <= 1.1375)
            {
                S1TriPrismScaleZ += triPrismScaleChange.z;
            }
            TriPrismScaleZ = S1TriPrismScaleZ;
        }
        else if (S1Active == true && S1IsScalingUp == false)
        {
            if (S1TriPrismScaleZ - triPrismScaleChange.z >= 0.17075)
            {
                S1TriPrismScaleZ -= triPrismScaleChange.z;
            }
            
            TriPrismScaleZ = S1TriPrismScaleZ;
        }
        else if (S1Active == false && S2IsScalingUp == true)
        {
            if (S2TriPrismScaleZ + triPrismScaleChange.z <= 1.1375)
            {
                S2TriPrismScaleZ += triPrismScaleChange.z;
            }

            TriPrismScaleZ = S2TriPrismScaleZ;
        }
        else if (S1Active == false && S2IsScalingUp == false)
        {
            if (S2TriPrismScaleZ - triPrismScaleChange.z >= 0.17075)
            {
                S2TriPrismScaleZ -= triPrismScaleChange.z;
            }

            TriPrismScaleZ = S2TriPrismScaleZ;
        }
       
        triPrism.transform.localScale = new Vector3(triPrismScale.x, triPrismScale.y, 1.0f); 
        float basicTriPrismScale = triPrism.GetComponent<Renderer>().bounds.size.z; 
        // This just fixes base at a certain location it's not necessary
        triPrism.transform.localScale = new Vector3(triPrismScale.x, triPrismScale.y, TriPrismScaleZ); 
        float triPrismZScaleOffset = (triPrism.GetComponent<Renderer>().bounds.size.z - basicTriPrismScale) / 2;
        triPrism.transform.position += new Vector3(0.0f, 0.0f, -triPrismZScaleOffset);

        triangleHeight = triPrism.GetComponent<Renderer>().bounds.size.z ; 

        // triangleBase = triPrism.GetComponent<Renderer>().bounds.size.y ; 



        // Generating the other objects
        float width = cornerTopRight.x - cornerBottomLeft.x;
        float height = cornerTopRight.y - cornerBottomLeft.y;

        //float proximityThreshold = Mathf.Max(triPrismScale.x, triPrismScale.y, triPrismScale.z) ;
        float rowHeight = height / n_rows;

        float shelfLedgeY = cornerTopRight.y;

        if (instantiatedShelves.Count > 0)
        {
            foreach (GameObject obj in instantiatedShelves)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
        }


        for (int row = 0; row < n_rows - 1; row++)
        {
            float shelfLedgeX = 0f;
            shelfLedgeY -= rowHeight ;
            Vector3 shelfLedgePosition = new Vector3(shelfLedgeX, shelfLedgeY, -0.3f);

            GameObject shelfLedge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            instantiatedShelves.Add(shelfLedge);
            shelfLedge.transform.position = shelfLedgePosition;

            Renderer shelfRenderer = shelfLedge.GetComponent<Renderer>();
                if (shelfRenderer != null && shelfMaterial != null)
                {
                    shelfRenderer.material = shelfMaterial;
                }

            shelfLedge.transform.localScale = new Vector3(5f, 0.025f, 0.5f);
        }

        // Let's add books and objects
        float rowY = cornerTopRight.y;

        // Vector3 triPrismPosition = new Vector3(triPrismX, triPrismY, 0f);
        // float triPrismWidth = triPrism.GetComponent<Renderer>().bounds.size.x * triPrismScale.x;
        // float triPrismHeight = triPrism.GetComponent<Renderer>().bounds.size.y * triPrismScale.y;
        // float triPrismXMin = triPrismPosition.x - triPrismWidth / 2 - Mathf.Abs(Mathf.Sin(triPrismRotateCCW * Mathf.Deg2Rad) * triPrismHeight / 2)  ;
        // float triPrismXMax = triPrismPosition.x + triPrismWidth / 2 + Mathf.Abs(Mathf.Sin(triPrismRotateCCW * Mathf.Deg2Rad) * triPrismHeight / 2)  ;

        if (instantiatedBooks.Count > 0)
            {
                //instantiatedBooks.Clear();
                foreach (GameObject obj in instantiatedBooks)
                    {
                        if (obj != null) 
                        {
                            Destroy(obj);
                        }
                    }
                                        

            }



        for (int row = 0; row < n_rows; row++) 
        {
            rowY -= rowHeight;
            float currentXPosition = cornerBottomLeft.x + 0.3f;
            float rowCenterY = rowY + 0.5f *rowHeight + 0.0125f ; // 0.0125 is half the shelf thickness



            while (currentXPosition < cornerTopRight.x - 0.5f) 
            {
                //float currentYPostion = rowY + (rowHeight/2);
                if (Random.value >= wallSparseness)
                {

                    if (bookMeshes.Length > 0)
                    {
                        

                        // if (rotationAngle > -90f)
                        // {
                        //     currentXPosition += (book.GetComponent<Renderer>().bounds.size.x) * 0.5f / Mathf.Abs(Mathf.Cos((rotationAngle + 90f ) * Mathf.Deg2Rad)) ;
                        // }
                        // more buffer to reduce book overlap but it adds too much space



                        if (currentXPosition < triPrismX - triPrism.GetComponent<Renderer>().bounds.size.x * 1.0f  || currentXPosition > triPrismX + triPrism.GetComponent<Renderer>().bounds.size.x * 1.0f || rowCenterY >  triPrismY + triPrism.GetComponent<Renderer>().bounds.size.y * 0.75f || rowCenterY < triPrismY - triPrism.GetComponent<Renderer>().bounds.size.y * 0.75f ) 
                        {
                            
                            GameObject book = Instantiate(bookMeshes[Random.Range(0, bookMeshes.Length)]);
                            instantiatedBooks.Add(book);

                            float rotationAngle = -90f;
                            if (Random.value < 0.5f)
                            {
                                rotationAngle = Random.Range(-135f, -45f);
                            }

                            // book.transform.rotation = Quaternion.Euler(0, 0, rotationAngle);

                            float bookHeight = book.GetComponent<Renderer>().bounds.size.y ;

                            float bookScaleZ = Random.Range(0.7f * rowHeight / bookHeight , (rowHeight - 0.1f) / bookHeight )  ;
                            float bookScaleY = Mathf.Min( bookScaleZ + Random.Range( 0 , 0.7f * (rowHeight - 0.1f) / bookHeight), (rowHeight - 0.1f) / bookHeight ) ; // Maximum height is shelf height minus the ledge thickness
                            float bookScaleX = bookScaleZ ;

                            book.transform.localScale = new Vector3(bookScaleX, bookScaleY, bookScaleZ);

                            if (rotationAngle != -90f)
                            {
                                currentXPosition += Mathf.Abs(Mathf.Sin((rotationAngle + 90f ) * Mathf.Deg2Rad)) * (book.GetComponent<Renderer>().bounds.size.z   ) / 2  ;
                            }
                            float currentYPosition = rowY + book.GetComponent<Renderer>().bounds.size.y  / 2 ;

                            float currentZPosition = Random.Range(-0.2f, -0.3f) - book.GetComponent<Renderer>().bounds.size.z  / 2;

                            book.transform.position = new Vector3(currentXPosition, 
                                                                currentYPosition,
                                                                currentZPosition); // Z needs to be based on scale (different depth)

                            book.transform.rotation = Quaternion.Euler(rotationAngle, 90f, 0f);

                            Renderer bookRenderer = book.GetComponent<Renderer>();

                            currentXPosition += (book.GetComponent<Renderer>().bounds.size.x )  ;


                        }
                        else
                        { 
                            currentXPosition += 0.2f; 
                        }

                        
                        

                    }

                    else
                    {
                        GameObject book = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    
                        float rotationAngle = 0f;
                        if (Random.value < 0.5f)
                        {
                            rotationAngle = Random.Range(-45f, 45f);
                        }

                        book.transform.rotation = Quaternion.Euler(0, 0, rotationAngle);

                        float bookScaleX = Random.Range(0.02f, 0.1f);
                        float bookScaleY = Random.Range(1.0f , 2.5f) / n_rows;
                        float bookScaleZ = Random.Range(0.1f, 0.4f);

                        book.transform.localScale = new Vector3(bookScaleX, bookScaleY, bookScaleZ);

                        if (rotationAngle != 0)
                        {
                            currentXPosition += Mathf.Sin(rotationAngle * Mathf.Deg2Rad) * (book.GetComponent<Renderer>().bounds.size.x  * bookScaleX ) + 0.1f ;
                        }

                        float currentYPosition = rowY + book.GetComponent<Renderer>().bounds.size.y * bookScaleY / 2 + 0.1f ;

                        float currentZPosition = -0.1f - book.GetComponent<Renderer>().bounds.size.z  * bookScaleZ / 2;

                        book.transform.position = new Vector3(currentXPosition, 
                                                            currentYPosition,
                                                            currentZPosition); // Z needs to be based on scale (different depth)

                        Renderer bookRenderer = book.GetComponent<Renderer>();
                        if (bookRenderer != null && bookMaterials.Length > 0)
                        {
                            Material randomMaterial = bookMaterials[Random.Range(0, bookMaterials.Length)];
                            bookRenderer.material = randomMaterial;
                        }

                        currentXPosition += (book.GetComponent<Renderer>().bounds.size.x  * bookScaleX) * 2 + 0.1f;
                    }
                    

                }

                // Leave some space in the row 
                else
                {
                    currentXPosition += 0.2f;
                } 

            }
        } 


    }
    
    private bool IsHeadsetConnected()
    {
        var devices = new List<InputDevice>();
        InputDevices.GetDevices(devices);
        return devices.Count > 0;
    }
    

    private void SaveTriPrismScale()
    {
        trialResponseTime = Time.time - trialStartTime ; 
        finalTriPrismScaleZ = triPrism.transform.localScale.z;
        

        if (S1Active)
        {
            staircaseNum = 1;
        }
        else
        {
            staircaseNum = 2;
        }

        if (didReversalHappen == "Yes")
        {
            csvReversal = "Reversal";
        }
        else
        {
            csvReversal = "";
        }

        string csvEntry = $"{System.DateTime.Now}, {staircaseNum}, {response}, {csvReversal}, {finalTriPrismScaleZ}, {triPrism.GetComponent<Renderer>().bounds.size.z}, {triPrism.GetComponent<Renderer>().bounds.size.y}, {triPrism.GetComponent<Renderer>().bounds.size.z / triPrism.GetComponent<Renderer>().bounds.size.y}, {trialResponseTime}, {seedRecorded}\n";

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "Timestamp, Staircase Num, Response, Reversal?, Final TriPrism Scale Z, Height, Base, Height:Base Ratio, Response Time, Random Seed\n");
        }

        File.AppendAllText(filePath, csvEntry);
        Debug.Log($"Data Saved: {triPrism.GetComponent<Renderer>().bounds.size.z / triPrism.GetComponent<Renderer>().bounds.size.y} to {filePath}");
        

    }

    private void ScaleUp()
    {
        float currentTriPrismZ = triPrism.GetComponent<Renderer>().bounds.size.z;
        triPrism.transform.localScale += triPrismScaleChange;
        float changeInScale = (triPrism.GetComponent<Renderer>().bounds.size.z - currentTriPrismZ) / 2;
        triPrism.transform.position += new Vector3(0.0f, 0.0f, -changeInScale);
        Debug.Log($"TriPrism Height is: {triPrism.GetComponent<Renderer>().bounds.size.z}");
        Debug.Log($"TriPrism Base is: {triPrism.GetComponent<Renderer>().bounds.size.y}");
    }

    private void ScaleDown()
    {
        float currentTriPrismZ = triPrism.GetComponent<Renderer>().bounds.size.z;
        triPrism.transform.localScale -= triPrismScaleChange;
        float changeInScale = (triPrism.GetComponent<Renderer>().bounds.size.z - currentTriPrismZ) / 2;
        triPrism.transform.position += new Vector3(0.0f, 0.0f, -changeInScale);
        Debug.Log($"TriPrism Height is: {triPrism.GetComponent<Renderer>().bounds.size.z}");
        Debug.Log($"TriPrism Base is: {triPrism.GetComponent<Renderer>().bounds.size.y}");
    }

}
