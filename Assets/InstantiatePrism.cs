using UnityEngine;

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

    public GameObject[] rockMeshes;


    void Awake()
    {

        // Generating the prism 
        Vector3 position = new Vector3(triPrismX, triPrismY, triPrismZ);
        triPrism = Instantiate(triPrism, position, Quaternion.Euler(0, 0, triPrismRotateCCW));

        Renderer objectRenderer = triPrism.GetComponent<Renderer>();
        if (objectRenderer != null && triPrismMaterial != null)
        {
            objectRenderer.material = triPrismMaterial;
        }

        triPrism.transform.localScale = triPrismScale; 

        // Generating the other objects
        float width = cornerTopRight.x - cornerBottomLeft.x;
        float height = cornerTopRight.y - cornerBottomLeft.y;

        //float proximityThreshold = Mathf.Max(triPrismScale.x, triPrismScale.y, triPrismScale.z) ;
        float rowHeight = height / n_rows;

        float shelfLedgeY = cornerTopRight.y;
        for (int row = 0; row < n_rows - 1; row++)
        {
            float shelfLedgeX = 0f;
            shelfLedgeY -= rowHeight ;
            Vector3 shelfLedgePosition = new Vector3(shelfLedgeX, shelfLedgeY, -0.2f);

            GameObject shelfLedge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shelfLedge.transform.position = shelfLedgePosition;

            Renderer shelfRenderer = shelfLedge.GetComponent<Renderer>();
                if (shelfRenderer != null && shelfMaterial != null)
                {
                    shelfRenderer.material = shelfMaterial;
                }

            shelfLedge.transform.localScale = new Vector3(5f, 0.05f, 0.5f);
        }

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
            float currentXPosition = cornerBottomLeft.x + 0.2f;
            while (currentXPosition < cornerTopRight.x - 0.3f) 
            {
                //float currentYPostion = rowY + (rowHeight/2);
                if (Random.value >= wallSparseness)
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

                    // if (currentYPosition < triPrismY + triPrismHeight / 2 && currentYPosition > triPrismY - triPrismHeight/ 2 )
                    // {
                    //     if (currentXPosition + (book.GetComponent<Renderer>().bounds.size.x  * bookScaleX) / 2 > triPrismXMin && currentXPosition + (book.GetComponent<Renderer>().bounds.size.x  * bookScaleX) / 2 < triPrismXMax)
                    //     {

                    //         currentXPosition += (triPrismWidth + Mathf.Abs(Mathf.Sin( triPrismRotateCCW * Mathf.Deg2Rad) * (triPrism.GetComponent<Renderer>().bounds.size.y  * triPrismScale.y ))) / 2    ;//(book.GetComponent<Renderer>().bounds.size.x  * bookScaleX);
                    //     }
                    // }

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

                // Leave some space in the row 
                else
                {
                    currentXPosition += 0.2f;
                }

                

            }
        } 
    }

    void Update()
    {
        Vector3 position = new Vector3(triPrismX, triPrismY, triPrismZ);
        triPrism.transform.position = position;
        triPrism.transform.localScale = triPrismScale; 
    }



}
