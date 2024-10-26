using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoldTranslation : MonoBehaviour
{
    Mesh mesh;
    MeshFilter Mf;
    Vector3[] verts;
    public Vector3 OriginOffset;

    // Start is called before the first frame update
    void Start()
    {
        Mf = GetComponent<MeshFilter>();
        mesh = Mf.mesh;
        verts = mesh.vertices;
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] += OriginOffset;
        }

        OriginOffset = Vector3.zero;
        mesh.vertices = verts;
    }
}
