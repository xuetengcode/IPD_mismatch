using UnityEngine;
using UnityEngine.XR;
using System.IO;
using System.Collections.Generic;

public class StartStimulus : MonoBehaviour
{
    public GameObject triPrism; 
    public float triPrismX; 
    public float triPrismY;

    public float triPrismZ; 
    
    public float triPrismRotateCCW;

    public Material triPrismMaterial;
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

    private string filePath;

    // For method of adjustment

    private Vector3 triPrismScaleChange = new Vector3(0.0f, 0.0f, 0.1f);

    public XRNode controllerNode;

    private List<GameObject> instantiatedBooks = new List<GameObject>();
    private List<GameObject> instantiatedShelves = new List<GameObject>();

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

            if (device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 axisValue))
            {
                float joystickInput = axisValue.y;

                if (joystickInput > 0)
                {
                    if (triPrism.transform.position.z - triPrism.GetComponent<Renderer>().bounds.size.z/2 > -2)
                    {
                        ScaleUp();
                    }
                }

                else if (joystickInput < 0)
                {
                    if (triPrism.transform.localScale.z - triPrismScaleChange.z > 0)
                    {
                        ScaleDown();
                    }
                }

                if (device.TryGetFeatureValue(CommonUsages.primaryButton, out bool buttonValue) && buttonValue)
                {
                    SaveTriPrismScale();
                    InstantiateBooksPrism();
                }
            }   
        }
        
        

        
        
        // When not using VR (for testing)
        else
        {
            if (Input.GetKeyDown(KeyCode.UpArrow)) 
            {
                if (triPrism.transform.position.z - triPrism.GetComponent<Renderer>().bounds.size.z/2 > -2)
                {
                    ScaleUp();
                }
                
            }
            
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (triPrism.transform.localScale.z - triPrismScaleChange.z > 0)
                {
                    ScaleDown();
                }
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                SaveTriPrismScale();
                InstantiateBooksPrism();
            }
        }

    }

    private void InstantiateBooksPrism()
    {
        

        if (!Application.isEditor && IsHeadsetConnected())
        {
            filePath = Path.Combine("/sdcard/Download", "IPD-RescaleData.csv");
        }
        
        else
        {
            filePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "IPD-RescaleData.csv");
        }

        // Generating the prism 
        if (triPrism != null)
        {
            Destroy(triPrism);
        }

        float jitterTriPrismY = Random.Range(-0.03f, 0.03f);
        Vector3 position = new Vector3(triPrismX, triPrismY + jitterTriPrismY, triPrismZ);
        triPrism = Instantiate(triPrism, position, Quaternion.Euler(0, 0, triPrismRotateCCW));



        Renderer objectRenderer = triPrism.GetComponent<Renderer>();
        if (objectRenderer != null && triPrismMaterial != null)
        {
            objectRenderer.material = triPrismMaterial;
        }

        float jitteredTriPrismScaleZ = Random.Range(0.5f, 1.5f);
        triPrism.transform.localScale = new Vector3(triPrismScale.x, triPrismScale.y, 1.0f); 
        float basicTriPrismScale = triPrism.GetComponent<Renderer>().bounds.size.z;
        triPrism.transform.localScale = new Vector3(triPrismScale.x, triPrismScale.y, jitteredTriPrismScaleZ); 
        float triPrismZScaleOffset = (triPrism.GetComponent<Renderer>().bounds.size.z - basicTriPrismScale) / 2;
        triPrism.transform.position += new Vector3(0.0f, 0.0f, -triPrismZScaleOffset);

        
  
        
        // Jitter triangle height every trial
        
        initialTriPrismScaleZ = triPrism.transform.localScale.z;

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
            Vector3 shelfLedgePosition = new Vector3(shelfLedgeX, shelfLedgeY, -0.45f);

            GameObject shelfLedge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            instantiatedShelves.Add(shelfLedge);
            shelfLedge.transform.position = shelfLedgePosition;

            Renderer shelfRenderer = shelfLedge.GetComponent<Renderer>();
                if (shelfRenderer != null && shelfMaterial != null)
                {
                    shelfRenderer.material = shelfMaterial;
                }

            shelfLedge.transform.localScale = new Vector3(5f, 0.05f, 1.0f);
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
            float rowCenterY = rowY + 0.5f *rowHeight ; 



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
        finalTriPrismScaleZ = triPrism.transform.localScale.z;
        string csvEntry = $"{System.DateTime.Now}, {finalTriPrismScaleZ}, {triPrism.GetComponent<Renderer>().bounds.size.z}, {triPrism.GetComponent<Renderer>().bounds.size.y}, {triPrism.GetComponent<Renderer>().bounds.size.z / triPrism.GetComponent<Renderer>().bounds.size.y}\n";

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "Timestamp, Final TriPrism Scale Z, Height, Base, Height:Base Ratio\n");
        }

        File.AppendAllText(filePath, csvEntry);
        Debug.Log($"Data Saved: {finalTriPrismScaleZ} to {filePath}");
        

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
