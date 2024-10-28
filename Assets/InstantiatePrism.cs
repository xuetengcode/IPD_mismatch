using UnityEngine;
using UnityEngine.XR;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using static UnityEditorInternal.VersionControl.ListControl;

public class StartStimulus : MonoBehaviour
{
    public GameObject triPrism; 
    public float triPrismX; 
    public float triPrismY;

    public float triPrismZ; 
    
    public float triPrismRotateCCW;

    public Material triPrismMaterial;
    public Vector3 triPrismScale = new Vector3(1.0f, 1.0f, 1.0f);

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

    [SerializeField]
    public XRNode controllerNode = XRNode.RightHand;

    float rowHeight;
    private InputDevice device;
    bool primaryButton = false;
    bool secondaryButton = false;
    bool LastStateA = false;
    bool LastStateB = false;
    private bool firstinterval = false;
    private bool secondinterval = false;
    private List<InputDevice> devices = new List<InputDevice>();
    void GetDevice()
    {
        InputDevices.GetDevicesAtXRNode(controllerNode, devices);
        device = devices.FirstOrDefault();
    }
    void OnEnable()
    {
        if (!device.isValid)
        {
            GetDevice();
        }
    }

    void Awake()
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
        Vector3 position = new Vector3(triPrismX, triPrismY, triPrismZ);
        triPrism = Instantiate(triPrism, position, Quaternion.Euler(0, 0, triPrismRotateCCW));

        Renderer objectRenderer = triPrism.GetComponent<Renderer>();
        if (objectRenderer != null && triPrismMaterial != null)
        {
            objectRenderer.material = triPrismMaterial;
        }

        triPrism.transform.localScale = triPrismScale; 

        initialTriPrismScaleZ = triPrismScale.z;

        // Generating the other objects
        float width = cornerTopRight.x - cornerBottomLeft.x;
        float height = cornerTopRight.y - cornerBottomLeft.y;

        //float proximityThreshold = Mathf.Max(triPrismScale.x, triPrismScale.y, triPrismScale.z) ;
        rowHeight = height / n_rows;

        float shelfLedgeY = cornerTopRight.y;
        for (int row = 0; row < n_rows - 1; row++)
        {
            float shelfLedgeX = 0f;
            shelfLedgeY -= rowHeight ;
            Vector3 shelfLedgePosition = new Vector3(shelfLedgeX, shelfLedgeY, -0.45f);

            GameObject shelfLedge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shelfLedge.transform.position = shelfLedgePosition;

            Renderer shelfRenderer = shelfLedge.GetComponent<Renderer>();
                if (shelfRenderer != null && shelfMaterial != null)
                {
                    shelfRenderer.material = shelfMaterial;
                }

            shelfLedge.transform.localScale = new Vector3(5f, 0.05f, 1.0f);
        }
        AddBooks();
    }
    
    void Update()
    {
        //Vector3 position = new Vector3(triPrismX, triPrismY, triPrismZ);
        //triPrism.transform.position = position;
        //triPrism.transform.localScale = triPrismScale; 

        //AddBooks();

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
                AddBooks();
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
                AddBooks();
                firstinterval = false;
                secondinterval = true;
            }
            //Set last known state of button
            LastStateB = secondaryButton;
        }
        

    }

    void DestroyBooks()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        // Loop through all GameObjects
        foreach (GameObject obj in allObjects)
        {
            // Check if the GameObject's name starts with "rotated-book"
            if (obj.name.StartsWith("rotated-book"))
            {
                // Destroy the GameObject
                Destroy(obj);
            }
        }
    }
    void AddBooks()
    {
        Debug.Log($"==>: here \n");
        DestroyBooks();
        // Let's add books and objects
        float rowY = cornerTopRight.y;

        // Vector3 triPrismPosition = new Vector3(triPrismX, triPrismY, 0f);
        // float triPrismWidth = triPrism.GetComponent<Renderer>().bounds.size.x * triPrismScale.x;
        // float triPrismHeight = triPrism.GetComponent<Renderer>().bounds.size.y * triPrismScale.y;
        // float triPrismXMin = triPrismPosition.x - triPrismWidth / 2 - Mathf.Abs(Mathf.Sin(triPrismRotateCCW * Mathf.Deg2Rad) * triPrismHeight / 2)  ;
        // float triPrismXMax = triPrismPosition.x + triPrismWidth / 2 + Mathf.Abs(Mathf.Sin(triPrismRotateCCW * Mathf.Deg2Rad) * triPrismHeight / 2)  ;

        for (int row = 0; row < n_rows; row++)
        {
            rowY -= rowHeight;
            float currentXPosition = cornerBottomLeft.x + 0.3f;
            float rowCenterY = rowY + 0.5f * rowHeight;
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

                        if (currentXPosition < triPrismX - triPrism.GetComponent<Renderer>().bounds.size.x * 1.2f ||
                            currentXPosition > triPrismX + triPrism.GetComponent<Renderer>().bounds.size.x * 1.2f ||
                            rowCenterY > triPrismY + triPrism.GetComponent<Renderer>().bounds.size.y * 0.75f ||
                            rowCenterY < triPrismY - triPrism.GetComponent<Renderer>().bounds.size.y * 0.75f)
                        {

                            GameObject book = Instantiate(bookMeshes[Random.Range(0, bookMeshes.Length)]);

                            float rotationAngle = -90f;
                            if (Random.value < 0.5f)
                            {
                                rotationAngle = Random.Range(-135f, -45f);
                            }

                            // book.transform.rotation = Quaternion.Euler(0, 0, rotationAngle);

                            float bookHeight = book.GetComponent<Renderer>().bounds.size.y;

                            float bookScaleZ = Random.Range(0.7f * rowHeight / bookHeight, (rowHeight - 0.1f) / bookHeight);
                            float bookScaleY = Mathf.Min(bookScaleZ + Random.Range(0, 0.7f * (rowHeight - 0.1f) / bookHeight), (rowHeight - 0.1f) / bookHeight); // Maximum height is shelf height minus the ledge thickness
                            float bookScaleX = bookScaleZ;

                            book.transform.localScale = new Vector3(bookScaleX, bookScaleY, bookScaleZ);

                            if (rotationAngle != -90f)
                            {
                                currentXPosition += Mathf.Abs(Mathf.Sin((rotationAngle + 90f) * Mathf.Deg2Rad)) * (book.GetComponent<Renderer>().bounds.size.z) / 2;
                            }
                            float currentYPosition = rowY + book.GetComponent<Renderer>().bounds.size.y / 2;

                            float currentZPosition = Random.Range(-0.2f, -0.3f) - book.GetComponent<Renderer>().bounds.size.z / 2;

                            book.transform.position = new Vector3(currentXPosition,
                                                                currentYPosition,
                                                                currentZPosition); // Z needs to be based on scale (different depth)

                            book.transform.rotation = Quaternion.Euler(rotationAngle, 90f, 0f);

                            Renderer bookRenderer = book.GetComponent<Renderer>();

                            currentXPosition += (book.GetComponent<Renderer>().bounds.size.x);
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
                        float bookScaleY = Random.Range(1.0f, 2.5f) / n_rows;
                        float bookScaleZ = Random.Range(0.1f, 0.4f);

                        book.transform.localScale = new Vector3(bookScaleX, bookScaleY, bookScaleZ);

                        if (rotationAngle != 0)
                        {
                            currentXPosition += Mathf.Sin(rotationAngle * Mathf.Deg2Rad) * (book.GetComponent<Renderer>().bounds.size.x * bookScaleX) + 0.1f;
                        }

                        float currentYPosition = rowY + book.GetComponent<Renderer>().bounds.size.y * bookScaleY / 2 + 0.1f;

                        float currentZPosition = -0.1f - book.GetComponent<Renderer>().bounds.size.z * bookScaleZ / 2;

                        book.transform.position = new Vector3(currentXPosition,
                                                            currentYPosition,
                                                            currentZPosition); // Z needs to be based on scale (different depth)

                        Renderer bookRenderer = book.GetComponent<Renderer>();
                        if (bookRenderer != null && bookMaterials.Length > 0)
                        {
                            Material randomMaterial = bookMaterials[Random.Range(0, bookMaterials.Length)];
                            bookRenderer.material = randomMaterial;
                        }

                        currentXPosition += (book.GetComponent<Renderer>().bounds.size.x * bookScaleX) * 2 + 0.1f;
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
        string csvEntry = $"{System.DateTime.Now}, {finalTriPrismScaleZ}\n";

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "Timestamp, Final TriPrism Scale Z\n");
        }

        File.AppendAllText(filePath, csvEntry);
        Debug.Log($"Final TriPrism Z Scale Saved: {finalTriPrismScaleZ} to {filePath}\n");

    }

    private void ScaleUp()
    {
        float currentTriPrismZ = triPrism.GetComponent<Renderer>().bounds.size.z;
        triPrism.transform.localScale += triPrismScaleChange;
        float changeInScale = (triPrism.GetComponent<Renderer>().bounds.size.z - currentTriPrismZ) / 2;
        triPrism.transform.position += new Vector3(0.0f, 0.0f, -changeInScale);
    }

    private void ScaleDown()
    {
        float currentTriPrismZ = triPrism.GetComponent<Renderer>().bounds.size.z;
        triPrism.transform.localScale -= triPrismScaleChange;
        float changeInScale = (triPrism.GetComponent<Renderer>().bounds.size.z - currentTriPrismZ) / 2;
        triPrism.transform.position += new Vector3(0.0f, 0.0f, -changeInScale);
    }

}
